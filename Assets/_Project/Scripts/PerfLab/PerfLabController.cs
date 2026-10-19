using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// The controls and the overlay of the PerfLab test scene (see backlog T-018): switch how many books are in the room, the lighting set, whether the
/// books cast shadows, GPU instancing and how the books are coloured, and watch frame time, draw calls, SetPass calls and triangles live.
/// A developer tool: it lives only in PerfLab.unity, never in the game's scenes.
/// Keys: [ and ] book count, 1 to 5 lighting, Z book shadows, X GPU instancing, C colour mode (property block or shared materials),
/// V uncapped frame rate, ` (backquote) hide or show the overlay.
/// </summary>
public class PerfLabController : MonoBehaviour
{
    [Tooltip("One inactive template book per item definition. The books are spawned from these at start, so the scene file stays small.")]
    public Book[] templates;
    [Tooltip("For each book of the layout: which template it is, and where it lies. Ordered so that the first N books are spread evenly over the piles.")]
    public int[] templateOf;
    public Vector3[] positions;
    public Quaternion[] rotations;
    [Tooltip("All the spawned books, in layout order.")]
    public Book[] books;
    [Tooltip("The lamp lights (the room's magelights).")]
    public Light[] orbs;
    [Tooltip("Many small point lights, for the 'many lights' preset.")]
    public Light[] smallLights;
    public Light directional;
    public int[] bookCounts = { 100, 300, 1000, 3000 };
    public int countIndex = 2;
    public int lightingPreset = 2;
    public bool booksCastShadows = true;
    public bool gpuInstancing;
    public bool sharedColourMaterials;
    public bool uncappedFrameRate = true;

    public static readonly string[] PresetNames =
    {
        "1  Orbs, no shadows",
        "2  Orbs, one shadowed (the game's setting)",
        "3  Orbs, all shadowed",
        "4  Many small lights (24), no shadows",
        "5  Directional light and ambient only",
    };

    private ProfilerRecorder _drawCalls, _setPass, _tris, _verts, _batches, _shadowCasters;
    private readonly Queue<float> _frames = new Queue<float>();
    private float _frameSum;
    private bool _overlay = true;
    private Material[] _coverCopies;       // runtime copies, so toggling instancing never edits the project's material assets
    private Material _pagesCopy;
    private Material[] _perColour;         // shared cover materials, one per distinct set colour
    private readonly Dictionary<Color, int> _colourIndex = new Dictionary<Color, int>();
    private GUIStyle _style;

    public int ActiveBooks => bookCounts[Mathf.Clamp(countIndex, 0, bookCounts.Length - 1)];

    private void OnEnable()
    {
        _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        _setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        _tris = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        _verts = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
        _batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        _shadowCasters = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
    }

    private void OnDisable()
    {
        _drawCalls.Dispose(); _setPass.Dispose(); _tris.Dispose(); _verts.Dispose(); _batches.Dispose(); _shadowCasters.Dispose();
    }

    private void Start()
    {
        SpawnBooks();
        PrepareMaterials();
        ApplyAll();
    }

    private void SpawnBooks()
    {
        if (templates == null || templates.Length == 0 || positions == null)
        {
            return;
        }

        var root = new GameObject("SpawnedBooks").transform;
        root.SetParent(transform, false);
        books = new Book[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            Book b = Instantiate(templates[templateOf[i]], positions[i], rotations[i], root);
            b.name = "PerfBook_" + i;
            b.gameObject.SetActive(false);
            books[i] = b;
        }
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        _frames.Enqueue(dt);
        _frameSum += dt;
        while (_frames.Count > 120)
        {
            _frameSum -= _frames.Dequeue();
        }

        Keyboard k = Keyboard.current;
        if (k == null)
        {
            return;
        }

        bool changed = false;
        if (k.rightBracketKey.wasPressedThisFrame) { countIndex = Mathf.Min(countIndex + 1, bookCounts.Length - 1); changed = true; }
        if (k.leftBracketKey.wasPressedThisFrame) { countIndex = Mathf.Max(countIndex - 1, 0); changed = true; }
        if (k.digit1Key.wasPressedThisFrame) { lightingPreset = 1; changed = true; }
        if (k.digit2Key.wasPressedThisFrame) { lightingPreset = 2; changed = true; }
        if (k.digit3Key.wasPressedThisFrame) { lightingPreset = 3; changed = true; }
        if (k.digit4Key.wasPressedThisFrame) { lightingPreset = 4; changed = true; }
        if (k.digit5Key.wasPressedThisFrame) { lightingPreset = 5; changed = true; }
        if (k.zKey.wasPressedThisFrame) { booksCastShadows = !booksCastShadows; changed = true; }
        if (k.xKey.wasPressedThisFrame) { gpuInstancing = !gpuInstancing; changed = true; }
        if (k.cKey.wasPressedThisFrame) { sharedColourMaterials = !sharedColourMaterials; changed = true; }
        if (k.vKey.wasPressedThisFrame) { uncappedFrameRate = !uncappedFrameRate; changed = true; }
        if (k.backquoteKey.wasPressedThisFrame) { _overlay = !_overlay; }
        if (changed)
        {
            ApplyAll();
        }
    }

    /// <summary>Runtime copies of the cover and page materials (and one cover copy per set colour) so nothing is written to the assets.</summary>
    private void PrepareMaterials()
    {
        if (books == null || books.Length == 0)
        {
            return;
        }

        var first = books[0].GetComponent<MeshRenderer>().sharedMaterials;
        if (first.Length < 2)
        {
            return;   // plain cubes: nothing to switch
        }

        _pagesCopy = new Material(first[1]) { name = first[1].name + " (perf copy)" };
        var covers = new List<Material>();
        foreach (Book b in books)
        {
            Color c = b.definition != null ? b.definition.spineColor : Color.white;
            if (!_colourIndex.ContainsKey(c))
            {
                _colourIndex[c] = covers.Count;
                var m = new Material(first[0]) { name = first[0].name + " " + ColorUtility.ToHtmlStringRGB(c) };
                m.SetColor("_BaseColor", c);
                covers.Add(m);
            }
        }

        _perColour = covers.ToArray();
        _coverCopies = new[] { new Material(first[0]) { name = first[0].name + " (perf copy)" } };
    }

    private void ApplyAll()
    {
        QualitySettings.vSyncCount = uncappedFrameRate ? 0 : 1;
        Application.targetFrameRate = uncappedFrameRate ? -1 : 0;
        ApplyBooks();
        ApplyLighting();
    }

    private void ApplyBooks()
    {
        if (books == null)
        {
            return;
        }

        int n = Mathf.Min(ActiveBooks, books.Length);
        if (_coverCopies != null)
        {
            _coverCopies[0].enableInstancing = gpuInstancing;
            _pagesCopy.enableInstancing = gpuInstancing;
            foreach (Material m in _perColour)
            {
                m.enableInstancing = gpuInstancing;
            }
        }

        for (int i = 0; i < books.Length; i++)
        {
            Book b = books[i];
            bool on = i < n;
            if (b.gameObject.activeSelf != on)
            {
                b.gameObject.SetActive(on);
            }

            if (!on)
            {
                continue;
            }

            var r = b.GetComponent<MeshRenderer>();
            r.shadowCastingMode = booksCastShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            if (_coverCopies != null)
            {
                if (sharedColourMaterials)
                {
                    Color c = b.definition != null ? b.definition.spineColor : Color.white;
                    r.sharedMaterials = new[] { _perColour[_colourIndex[c]], _pagesCopy };
                    r.SetPropertyBlock(null, 0);
                }
                else
                {
                    r.sharedMaterials = new[] { _coverCopies[0], _pagesCopy };
                    b.ApplyColor();
                }
            }
        }
    }

    private void ApplyLighting()
    {
        bool orbsOn = lightingPreset <= 3;
        bool smallOn = lightingPreset == 4;
        for (int i = 0; orbs != null && i < orbs.Length; i++)
        {
            orbs[i].enabled = orbsOn;
            LightShadows s = LightShadows.None;
            if (lightingPreset == 3 || (lightingPreset == 2 && i == orbs.Length - 1))
            {
                s = LightShadows.Soft;
            }

            orbs[i].shadows = s;
        }

        for (int i = 0; smallLights != null && i < smallLights.Length; i++)
        {
            smallLights[i].enabled = smallOn;
        }

        if (directional != null)
        {
            directional.enabled = lightingPreset == 5;
            directional.shadows = lightingPreset == 5 ? LightShadows.Soft : LightShadows.None;
            directional.intensity = 1.2f;
        }
    }

    private void OnGUI()
    {
        if (!_overlay)
        {
            return;
        }

        _style ??= new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true, normal = { textColor = Color.white } };
        float avg = _frames.Count > 0 ? _frameSum / _frames.Count : 0f;
        float worst = 0f;
        foreach (float f in _frames)
        {
            worst = Mathf.Max(worst, f);
        }

        var sb = new StringBuilder();
        sb.Append("<b>PerfLab</b>   ").Append((avg > 0f ? 1f / avg : 0f).ToString("F0")).Append(" fps   ").Append((avg * 1000f).ToString("F2")).Append(" ms avg   ")
          .Append((worst * 1000f).ToString("F1")).Append(" ms worst of the last ").Append(_frames.Count).Append(" frames\n");
        sb.Append("books ").Append(Mathf.Min(ActiveBooks, books != null ? books.Length : 0)).Append("   draw calls ").Append(_drawCalls.Valid ? _drawCalls.LastValue : -1)
          .Append("   batches ").Append(_batches.Valid ? _batches.LastValue : -1).Append("   SetPass ").Append(_setPass.Valid ? _setPass.LastValue : -1)
          .Append("   triangles ").Append(_tris.Valid ? _tris.LastValue / 1000 : -1).Append("k   shadow casters ").Append(_shadowCasters.Valid ? _shadowCasters.LastValue : -1).Append('\n');
        sb.Append("lighting ").Append(PresetNames[Mathf.Clamp(lightingPreset, 1, PresetNames.Length) - 1]).Append('\n');
        sb.Append("books cast shadows ").Append(booksCastShadows ? "ON" : "off").Append("   GPU instancing ").Append(gpuInstancing ? "ON" : "off")
          .Append("   colour ").Append(sharedColourMaterials ? "shared materials" : "property block").Append("   frame rate ").Append(uncappedFrameRate ? "uncapped" : "vsync").Append('\n');
        sb.Append("<size=12>[ ]  book count   1-5  lighting   Z  book shadows   X  instancing   C  colour mode   V  vsync   `  hide this</size>");
        GUI.Label(new Rect(12, 8, 1400, 130), sb.ToString(), _style);
    }
}
