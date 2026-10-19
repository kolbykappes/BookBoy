using BookBoy.Rules;
using UnityEngine;

/// <summary>
/// The placement ghost (backlog L-022; Librarian and Witch Cabin do the same): while the player holds a book and aims at an empty shelf slot, a
/// translucent copy of the book stands in that slot exactly where it would sit, tinted by how it would grade (green: right place, yellow: right case but
/// the wrong shelf or slot, red: wrong case). It replaces the thin highlight bar. The ghost has no collider, casts no shadow and is never saved.
/// </summary>
public class SlotGhost : MonoBehaviour
{
    [Tooltip("A transparent unlit material (Mat_SlotGhost). Copies of it are tinted green, yellow and red.")]
    public Material template;

    private static readonly Color Green = new Color(0.25f, 1f, 0.35f);
    private static readonly Color Yellow = new Color(1f, 0.9f, 0.15f);
    private static readonly Color Red = new Color(1f, 0.25f, 0.18f);

    private GameObject _go;
    private MeshFilter _filter;
    private MeshRenderer _renderer;
    private readonly Material[] _tint = new Material[3];
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
        _go.SetActive(false);
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
        c.a = 0.30f + 0.08f * Mathf.Sin(Time.unscaledTime * 5f);   // translucent, with a slow pulse so it reads as a preview and not as a book
        _tint[_grade].SetColor("_BaseColor", c);
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
    }
}
