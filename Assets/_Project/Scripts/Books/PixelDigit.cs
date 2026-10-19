using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A flat numeral (0-9) built from a 5x7 bitmap, one unit tall and centered on the origin, facing -Z.
/// Used for book volume numbers and shelf slot numbers. Single-sided and depth-tested, so unlike legacy
/// TextMesh (whose shader ignores depth) it never shows through the book it sits on or appears mirrored.
/// The mesh/renderer components are hidden and not saved; OnEnable rebuilds them.
/// </summary>
[ExecuteAlways]
public class PixelDigit : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private static readonly string[][] Glyphs =
    {
        new[] { "01110", "10001", "10011", "10101", "11001", "10001", "01110" },
        new[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" },
        new[] { "01110", "10001", "00001", "00010", "00100", "01000", "11111" },
        new[] { "11110", "00001", "00001", "01110", "00001", "00001", "11110" },
        new[] { "00010", "00110", "01010", "10010", "11111", "00010", "00010" },
        new[] { "11111", "10000", "11110", "00001", "00001", "10001", "01110" },
        new[] { "00110", "01000", "10000", "11110", "10001", "10001", "01110" },
        new[] { "11111", "00001", "00010", "00100", "01000", "01000", "01000" },
        new[] { "01110", "10001", "10001", "01110", "10001", "10001", "01110" },
        new[] { "01110", "10001", "10001", "01111", "00001", "00010", "01100" },
        new[] { "01110", "10001", "10001", "11111", "10001", "10001", "10001" },   // A
        new[] { "11110", "10001", "10001", "11110", "10001", "10001", "11110" },   // B
        new[] { "01110", "10001", "10000", "10000", "10000", "10001", "01110" },   // C
        new[] { "11110", "10001", "10001", "10001", "10001", "10001", "11110" },   // D
        new[] { "11111", "10000", "10000", "11110", "10000", "10000", "11111" },   // E
        new[] { "11111", "10000", "10000", "11110", "10000", "10000", "10000" },   // F
        new[] { "01110", "10001", "10000", "10111", "10001", "10001", "01111" },   // G
        new[] { "10001", "10001", "10001", "11111", "10001", "10001", "10001" },   // H
        new[] { "01110", "00100", "00100", "00100", "00100", "00100", "01110" },   // I
        new[] { "00111", "00010", "00010", "00010", "00010", "10010", "01100" },   // J
        new[] { "10001", "10010", "10100", "11000", "10100", "10010", "10001" },   // K
        new[] { "10000", "10000", "10000", "10000", "10000", "10000", "11111" },   // L
        new[] { "10001", "11011", "10101", "10101", "10001", "10001", "10001" },   // M
        new[] { "10001", "11001", "10101", "10011", "10001", "10001", "10001" },   // N
        new[] { "01110", "10001", "10001", "10001", "10001", "10001", "01110" },   // O
        new[] { "11110", "10001", "10001", "11110", "10000", "10000", "10000" },   // P
        new[] { "01110", "10001", "10001", "10001", "10101", "10010", "01101" },   // Q
        new[] { "11110", "10001", "10001", "11110", "10100", "10010", "10001" },   // R
        new[] { "01111", "10000", "10000", "01110", "00001", "00001", "11110" },   // S
        new[] { "11111", "00100", "00100", "00100", "00100", "00100", "00100" },   // T
        new[] { "10001", "10001", "10001", "10001", "10001", "10001", "01110" },   // U
        new[] { "10001", "10001", "10001", "10001", "10001", "01010", "00100" },   // V
        new[] { "10001", "10001", "10001", "10101", "10101", "11011", "10001" },   // W
        new[] { "10001", "10001", "01010", "00100", "01010", "10001", "10001" },   // X
        new[] { "10001", "10001", "01010", "00100", "00100", "00100", "00100" },   // Y
        new[] { "11111", "00001", "00010", "00100", "01000", "10000", "11111" },   // Z
        // set icons (36 to 43): solid shapes that cannot be mistaken for a letter or a digit, a second cue next to the colour and the letter
        new[] { "00000", "01110", "11111", "11111", "11111", "01110", "00000" },   // disc
        new[] { "00000", "00100", "01110", "01110", "11111", "11111", "00000" },   // triangle
        new[] { "00100", "01110", "11111", "11111", "01110", "00100", "00000" },   // diamond
        new[] { "00000", "11111", "11111", "11111", "11111", "11111", "00000" },   // square
        new[] { "00100", "10101", "01110", "11111", "01110", "10101", "00100" },   // star
        new[] { "01010", "11111", "11111", "11111", "01110", "00100", "00000" },   // heart
        new[] { "00110", "01100", "11000", "11000", "11000", "01100", "00110" },   // moon
        new[] { "00100", "00100", "11111", "11111", "00100", "00100", "00100" },   // cross
    };

    public const int IconCount = 8;

    /// <summary>The value to give a PixelDigit to show set icon number i (0 to 7, wraps): a disc, triangle, diamond, square, star, heart, moon or cross.</summary>
    public static int IconValue(int i)
    {
        return 36 + ((i % IconCount) + IconCount) % IconCount;
    }

    /// <summary>The value to give a PixelDigit to show the letter c (A-Z, case-insensitive): digits are 0-9, letters follow.</summary>
    public static int LetterValue(char c)
    {
        return 10 + (char.ToUpperInvariant(c) - 'A');
    }

    private static readonly Dictionary<int, Mesh> MeshCache = new Dictionary<int, Mesh>();

    [Tooltip("0-9 are digits; 10-35 are the letters A-Z (see LetterValue); 36-43 are the set icons (see IconValue).")]
    [Range(0, 43)] public int value = 1;
    public Color color = Color.white;
    public Material material;

    private MeshFilter _filter;
    private MeshRenderer _renderer;
    private MaterialPropertyBlock _block;

    private void OnEnable()
    {
        Rebuild();
    }

    /// <summary>Call after changing value, color or material from code.</summary>
    public void Rebuild()
    {
        _filter = GetOrAdd<MeshFilter>();
        _renderer = GetOrAdd<MeshRenderer>();
        _filter.sharedMesh = MeshFor(Mathf.Clamp(value, 0, Glyphs.Length - 1));
        _renderer.sharedMaterial = material;
        _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _renderer.receiveShadows = false;

        _block ??= new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_block);
        _block.SetColor(BaseColorId, color);
        _block.SetColor(ColorId, color);
        _renderer.SetPropertyBlock(_block);
    }

    private T GetOrAdd<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        component.hideFlags = HideFlags.HideInInspector | HideFlags.DontSave;
        return component;
    }

    private static Mesh MeshFor(int digit)
    {
        if (MeshCache.TryGetValue(digit, out Mesh cached) && cached != null)
        {
            return cached;
        }

        string[] rows = Glyphs[digit];
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();
        const float cell = 1f / 7f;
        float left = -2.5f * cell;
        float top = 3.5f * cell;

        for (int row = 0; row < 7; row++)
        {
            for (int column = 0; column < 5; column++)
            {
                if (rows[row][column] != '1')
                {
                    continue;
                }

                float x0 = left + column * cell;
                float y1 = top - row * cell;
                int first = vertices.Count;
                // Clockwise seen from -Z with +X to the right, so the front face looks toward -Z.
                vertices.Add(new Vector3(x0, y1 - cell, 0f));
                vertices.Add(new Vector3(x0, y1, 0f));
                vertices.Add(new Vector3(x0 + cell, y1, 0f));
                vertices.Add(new Vector3(x0 + cell, y1 - cell, 0f));
                for (int i = 0; i < 4; i++)
                {
                    normals.Add(Vector3.back);
                }

                triangles.AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });
            }
        }

        var mesh = new Mesh { name = "PixelDigit_" + digit, hideFlags = HideFlags.HideAndDontSave };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        MeshCache[digit] = mesh;
        return mesh;
    }
}
