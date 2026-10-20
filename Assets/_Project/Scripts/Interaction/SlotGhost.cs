using BookBoy.Rules;
using UnityEngine;

/// <summary>
/// The placement ghost (backlog L-022; Librarian and Witch Cabin do the same): while the player holds a book and aims at an empty shelf slot, a
/// hologram of the book stands in that slot exactly where it would sit, coloured by how it would grade (green: right place, yellow: right case but
/// the wrong shelf or slot, red: wrong case): a see-through body with a bright rim and a slow rising light band (BookBoy/BookGhost) inside a crisp outline
/// (BookBoy/BookOutline on a smoothed-normal hull, docs/unity-practices/29). It replaces the thin highlight bar. The ghost has no collider, casts no shadow and
/// is never saved.
/// </summary>
public class SlotGhost : MonoBehaviour
{
    [Tooltip("The see-through body material (Mat_SlotGhost, shader BookBoy/BookGhost). Copies of it are tinted green, yellow and red.")]
    public Material template;

    [Tooltip("The crisp outline material (Mat_SlotGhostOutline, shader BookBoy/BookOutline). Copies of it are tinted green, yellow and red.")]
    public Material outlineTemplate;

    private static readonly Color Green = new Color(0.25f, 1f, 0.35f);
    private static readonly Color Yellow = new Color(1f, 0.9f, 0.15f);
    private static readonly Color Red = new Color(1f, 0.25f, 0.18f);

    private GameObject _go;
    private MeshFilter _filter;
    private MeshRenderer _renderer;
    private readonly Material[] _tint = new Material[3];
    private readonly Material[] _lineTint = new Material[3];
    private MeshFilter _lineFilter;
    private MeshRenderer _lineRenderer;
    private Mesh _mesh;
    private int _grade = -1;
    private bool _shown;

    public bool IsShown => _shown;

    /// <summary>0 green, 1 yellow, 2 red (for tests and tools); -1 before the first show.</summary>
    public int GradeIndex => _grade;

    /// <summary>The ghost transform while it exists (for tests and tools).</summary>
    public Transform GhostTransform => _go != null ? _go.transform : null;

    /// <summary>Shows the ghost of <paramref name="held"/> in <paramref name="slot"/> at the size the book will have on the shelf, tinted for <paramref name="grade"/>.</summary>
    public void Show(Book held, ShelfSlot slot, PlacementResult grade, Vector3 realScale)
    {
        MeshFilter source = held != null ? held.GetComponent<MeshFilter>() : null;
        if (template == null || slot == null || source == null || source.sharedMesh == null)
        {
            Hide();
            return;
        }

        Ensure();
        if (_mesh != source.sharedMesh)
        {
            _mesh = source.sharedMesh;
            _filter.sharedMesh = _mesh;
            _lineFilter.sharedMesh = BookOutline.HullMesh(_mesh);
            _grade = -1;
        }

        int g = grade == PlacementResult.Correct ? 0 : grade == PlacementResult.RightBookcaseWrongSpot ? 1 : 2;
        if (g != _grade)
        {
            _grade = g;
            Material m = TintMaterial(g);
            var all = new Material[Mathf.Max(1, _mesh.subMeshCount)];
            for (int i = 0; i < all.Length; i++)
            {
                all[i] = m;
            }

            _renderer.sharedMaterials = all;
            Material line = LineMaterial(g);
            var lines = new Material[all.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = line;
            }

            _lineRenderer.sharedMaterials = lines;
        }

        // the same pose ShelfSlot.Accept gives the book: standing on the board, its spine on the slot's front line
        Transform t = slot.transform;
        _go.transform.SetPositionAndRotation(t.position + Vector3.up * (realScale.y * 0.5f) + t.forward * (realScale.z * 0.5f), t.rotation);
        _go.transform.localScale = realScale;
        if (!_shown)
        {
            _go.SetActive(true);
            _shown = true;
        }

        ApplyTint();   // right away, so the first frame (and a render in Edit Mode) already has the right colour
    }

    public void Hide()
    {
        if (_shown && _go != null)
        {
            _go.SetActive(false);
        }

        _shown = false;
    }

    private void Ensure()
    {
        if (_go != null)
        {
            return;
        }

        _go = new GameObject("SlotGhost") { hideFlags = HideFlags.HideAndDontSave };
        _filter = _go.AddComponent<MeshFilter>();
        _renderer = _go.AddComponent<MeshRenderer>();
        _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _renderer.receiveShadows = false;
        var line = new GameObject("Outline") { hideFlags = HideFlags.HideAndDontSave };
        line.transform.SetParent(_go.transform, false);
        _lineFilter = line.AddComponent<MeshFilter>();
        _lineRenderer = line.AddComponent<MeshRenderer>();
        _lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _lineRenderer.receiveShadows = false;
        _lineRenderer.sortingOrder = 1;   // after the body
        _go.SetActive(false);
    }

    private Material LineMaterial(int g)
    {
        if (_lineTint[g] == null)
        {
            _lineTint[g] = new Material(outlineTemplate != null ? outlineTemplate : template) { hideFlags = HideFlags.HideAndDontSave };
        }

        return _lineTint[g];
    }

    private Material TintMaterial(int g)
    {
        if (_tint[g] == null)
        {
            _tint[g] = new Material(template) { hideFlags = HideFlags.HideAndDontSave };
        }

        return _tint[g];
    }

    private void LateUpdate()
    {
        ApplyTint();
    }

    private void ApplyTint()
    {
        if (!_shown || _grade < 0)
        {
            return;
        }

        Color c = _grade == 0 ? Green : _grade == 1 ? Yellow : Red;
        float breath = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3f);
        _tint[_grade].SetColor("_Color", new Color(c.r, c.g, c.b, 0.85f + 0.15f * breath));          // the body: bright rim, a faint fill, a rising band
        float hdr = 1.3f;                                                                           // over 1, so the bloom gives the line a soft glow
        Material line = _lineTint[_grade];
        if (line != null)
        {
            line.SetColor("_Color", new Color(c.r * hdr, c.g * hdr, c.b * hdr, 0.8f + 0.2f * breath));
            line.SetFloat("_WidthPx", 2f * Mathf.Max(1f, Screen.height / 1080f));
        }
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        if (_go != null)
        {
            Destroy(_go);
        }

        foreach (Material m in _tint)
        {
            if (m != null)
            {
                Destroy(m);
            }
        }

        foreach (Material m in _lineTint)
        {
            if (m != null)
            {
                Destroy(m);
            }
        }
    }
}
