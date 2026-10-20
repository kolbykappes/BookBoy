using System.Collections.Generic;
using BookBoy.Rules;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The HUD half of the score feedback (docs/unity-practices/31, after Witch Cabin and Librarian): when a book is placed correctly a glowing mote flies from the book to the
/// ITEMS counter at the top left and the counter pops when it arrives; when a shelf is finished the world celebration (<see cref="ShelfFlourish"/>) plays, a bigger gold
/// gem flies to the SHELVES counter after it, and a "shelf complete" line comes up. Listens to <see cref="Book.Placed"/>. Built in code on the HUD canvas, drawn above the
/// HUD panels. Everything is pooled; nothing allocates per frame.
/// </summary>
public class ScoreFx : MonoBehaviour
{
    private sealed class Mote
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Start, Control, End;
        public float T0, Duration, Size, NextTrail;
        public int Line;
        public Color Color;
        public bool Big;
    }

    private sealed class Trail
    {
        public RectTransform Rect;
        public Image Image;
        public float T0, Life, Size;
        public Color Color;
    }

    private const int ItemsLine = 1;
    private const int ShelvesLine = 2;

    private float _skip;

    private float Now => Time.unscaledTime + _skip;

    /// <summary>Moves this effect's clock forward (tests: frames are frozen while the Editor is unfocused).</summary>
    public void Skip(float seconds)
    {
        _skip += seconds;
    }

    private GameUI _ui;
    private RectTransform _layer;
    private Sprite _glow;
    private Sprite _gem;
    private readonly List<Mote> _motes = new List<Mote>();
    private readonly List<Trail> _trails = new List<Trail>();
    private readonly Stack<Image> _pool = new Stack<Image>();

    /// <summary>How many motes are in flight (tests).</summary>
    public int MotesInFlight => _motes.Count;

    /// <summary>Motes that arrived and punched a counter since the session began (tests).</summary>
    public int Arrivals { get; private set; }

    public int ShelvesCelebrated { get; private set; }

    private void OnEnable()
    {
        Book.Placed += OnPlaced;
    }

    private void OnDisable()
    {
        Book.Placed -= OnPlaced;
    }

    private void Start()
    {
        _ui = GetComponent<GameUI>();
    }

    private void OnDestroy()
    {
        if (_layer != null)
        {
            Destroy(_layer.gameObject);
        }

        if (_glow != null)
        {
            Destroy(_glow.texture);
            Destroy(_glow);
        }

        if (_gem != null)
        {
            Destroy(_gem.texture);
            Destroy(_gem);
        }
    }

    private void OnPlaced(Book book, PlacementResult result)
    {
        if (result != PlacementResult.Correct || book == null || !Application.isPlaying)
        {
            return;
        }

        FlyFrom(book.transform.position, ItemsLine, ShelfFlourish.Gold, 1f, false, 0f);

        Shelf shelf = book.currentSlot != null ? book.currentSlot.shelf : null;
        if (shelf != null && !shelf.celebrated && shelf.IsComplete)
        {
            shelf.celebrated = true;
            ShelvesCelebrated++;
            ShelfFlourish.Play(shelf);
            // the gem leaves from the middle of the row when the wave has run along it
            Vector3 middle = book.transform.position;
            if (shelf.slots != null && shelf.slots.Length > 0)
            {
                middle = (shelf.slots[0].transform.position + shelf.slots[shelf.slots.Length - 1].transform.position) / 2f + Vector3.up * 0.2f;
            }

            FlyFrom(middle, ShelvesLine, new Color(1f, 0.86f, 0.45f), 2.4f, true, 0.45f + shelf.slots.Length * 0.03f);
            string title = book.definition != null && !string.IsNullOrEmpty(book.definition.title) ? book.definition.title : "";
            _ui?.Toast(title.Length > 0 ? "Shelf complete:  " + title : "Shelf complete!", 2.4f);
        }
    }

    /// <summary>Sends a glowing mote from a world position to a stats line (0 header, 1 items, 2 shelves). <paramref name="delay"/> waits before it leaves.</summary>
    public void FlyFrom(Vector3 worldPosition, int statLine, Color color, float size, bool big, float delay)
    {
        if (_ui == null)
        {
            _ui = GetComponent<GameUI>();
        }

        if (_ui == null || !_ui.TryGetStatTarget(statLine, out Vector2 end))
        {
            return;
        }

        EnsureLayer();
        Camera cam = Camera.main;
        float scale = Mathf.Max(0.01f, _ui.CanvasScale);
        Vector2 start;
        if (cam != null)
        {
            Vector3 sp = cam.WorldToScreenPoint(worldPosition);
            if (sp.z < 0f)
            {
                sp = new Vector3(Screen.width * 0.5f, Screen.height * 0.25f, 0f);
            }

            start = new Vector2(sp.x, sp.y) / scale;
        }
        else
        {
            start = new Vector2(Screen.width, Screen.height) * 0.5f / scale;
        }

        var m = new Mote
        {
            Start = start,
            End = end,
            Control = new Vector2((start.x + end.x) * 0.5f + Random.Range(-120f, 120f), Mathf.Max(start.y, end.y) + Random.Range(80f, 220f)),
            T0 = Now + delay,
            Duration = Random.Range(0.62f, 0.8f) * (big ? 1.2f : 1f),
            Size = (big ? 92f : 52f) * size / (big ? 2.4f : 1f),
            Line = statLine,
            Color = color,
            Big = big
        };
        m.Image = Rent(big ? _gem : _glow);
        m.Rect = m.Image.rectTransform;
        m.Image.color = new Color(color.r, color.g, color.b, 0f);
        m.Rect.sizeDelta = Vector2.one * m.Size;
        m.Rect.anchoredPosition = start;
        _motes.Add(m);
    }

    private void Update()
    {
        if (_layer == null)
        {
            return;
        }

        float now = Now;
        for (int i = _motes.Count - 1; i >= 0; i--)
        {
            Mote m = _motes[i];
            float u = (now - m.T0) / m.Duration;
            if (u < 0f)
            {
                m.Image.color = new Color(m.Color.r, m.Color.g, m.Color.b, 0f);
                continue;
            }

            if (u >= 1f)
            {
                Arrive(m);
                Return(m.Image);
                _motes.RemoveAt(i);
                continue;
            }

            float e = u * u * (3f - 2f * u) * 0.35f + u * 0.65f;   // a little ease, mostly steady
            Vector2 p = Bezier(m.Start, m.Control, m.End, e);
            m.Rect.anchoredPosition = p;
            float pulse = 1f + 0.18f * Mathf.Sin(now * 30f + i);
            m.Rect.sizeDelta = Vector2.one * m.Size * Mathf.Lerp(1f, 0.7f, u) * pulse;
            m.Image.color = new Color(m.Color.r, m.Color.g, m.Color.b, Mathf.Clamp01(u * 8f));
            if (m.Big)
            {
                m.Rect.localRotation = Quaternion.Euler(0f, 0f, u * 360f);
            }

            if (now >= m.NextTrail)
            {
                m.NextTrail = now + (m.Big ? 0.022f : 0.03f);
                SpawnTrail(p, m.Size * 0.62f, m.Color, now);
            }
        }

        for (int i = _trails.Count - 1; i >= 0; i--)
        {
            Trail t = _trails[i];
            float u = (now - t.T0) / t.Life;
            if (u >= 1f)
            {
                Return(t.Image);
                _trails.RemoveAt(i);
                continue;
            }

            t.Image.color = new Color(t.Color.r, t.Color.g, t.Color.b, (1f - u) * 0.55f);
            t.Rect.sizeDelta = Vector2.one * t.Size * (1f - 0.6f * u);
        }
    }

    private void Arrive(Mote m)
    {
        Arrivals++;
        _ui?.PunchStat(m.Line, m.Big ? new Color(1f, 0.82f, 0.35f) : new Color(1f, 0.9f, 0.55f), m.Big ? 1.6f : 1f);
        // a small burst of sparks at the counter
        if (_ui != null && _ui.TryGetStatTarget(m.Line, out Vector2 end))
        {
            int n = m.Big ? 9 : 4;
            for (int k = 0; k < n; k++)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                SpawnTrail(end + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * Random.Range(8f, m.Big ? 46f : 26f), (m.Big ? 22f : 14f) * Random.Range(0.6f, 1f), m.Color, Now);
            }
        }
    }

    private void SpawnTrail(Vector2 pos, float size, Color color, float now)
    {
        Image img = Rent(_glow);
        var t = new Trail { Image = img, Rect = img.rectTransform, T0 = now, Life = 0.28f, Size = size, Color = color };
        t.Rect.anchoredPosition = pos;
        t.Rect.sizeDelta = Vector2.one * size;
        t.Rect.localRotation = Quaternion.identity;
        img.color = new Color(color.r, color.g, color.b, 0.55f);
        _trails.Add(t);
    }

    private static Vector2 Bezier(Vector2 a, Vector2 c, Vector2 b, float t)
    {
        float o = 1f - t;
        return o * o * a + 2f * o * t * c + t * t * b;
    }

    // ---------------------------------------------------------------- pooling and sprites

    private void EnsureLayer()
    {
        if (_layer != null)
        {
            return;
        }

        GameObject canvas = GameObject.Find("HUD_Canvas");
        var go = new GameObject("GameUI_ScoreFx", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        _layer = (RectTransform)go.transform;
        _layer.anchorMin = _layer.anchorMax = Vector2.zero;
        _layer.pivot = Vector2.zero;
        _layer.sizeDelta = Vector2.zero;
        _layer.SetAsLastSibling();
        _glow = MakeGlowSprite();
        _gem = MakeGemSprite();
    }

    private Image Rent(Sprite sprite)
    {
        Image img;
        if (_pool.Count > 0)
        {
            img = _pool.Pop();
            img.gameObject.SetActive(true);
        }
        else
        {
            var go = new GameObject("Mote", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_layer, false);
            img = go.GetComponent<Image>();
            img.raycastTarget = false;
            RectTransform rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        img.sprite = sprite;
        img.rectTransform.localRotation = Quaternion.identity;
        _layer.SetAsLastSibling();
        return img;
    }

    private void Return(Image img)
    {
        if (img == null)
        {
            return;
        }

        img.gameObject.SetActive(false);
        _pool.Push(img);
    }

    /// <summary>A soft round glow: white in the middle, fading to nothing.</summary>
    private static Sprite MakeGlowSprite()
    {
        const int n = 64;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                float r = Mathf.Sqrt(u * u + v * v);
                float a = Mathf.Clamp01(1f - r);
                a = Mathf.Clamp01(a * a * 1.5f + 0.55f * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.32f, 0f, r)));   // a soft halo with a bright core
                px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>A gold hexagon with a light face, a bright rim and a glow behind it (Witch Cabin's token, in our own drawing).</summary>
    private static Sprite MakeGemSprite()
    {
        const int n = 96;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                float r = Mathf.Sqrt(u * u + v * v);
                // regular hexagon of radius 0.62 (flat top)
                float hex = Mathf.Max(Mathf.Abs(v) * 1.1547f, Mathf.Abs(u) + Mathf.Abs(v) * 0.5774f) / 0.62f;
                float glow = Mathf.Clamp01(1f - r) * 0.45f;
                float body = 1f - Mathf.SmoothStep(0.94f, 1.0f, hex);
                float rim = Mathf.SmoothStep(0.70f, 0.90f, hex) * body;
                float face = Mathf.Clamp01(0.82f + 0.18f * (-u * 0.5f + v * 0.8f));
                float lum = Mathf.Lerp(face, 1.15f, rim);
                float alpha = Mathf.Max(body, glow * (1f - body));
                Color c = Color.Lerp(new Color(1f, 0.9f, 0.55f), new Color(1f, 0.78f, 0.25f), 1f - lum * 0.7f) * Mathf.Clamp(lum, 0.6f, 1.15f);
                c = Color.Lerp(new Color(1f, 0.8f, 0.3f), c, body);
                px[y * n + x] = new Color32((byte)(Mathf.Clamp01(c.r) * 255), (byte)(Mathf.Clamp01(c.g) * 255), (byte)(Mathf.Clamp01(c.b) * 255), (byte)(Mathf.Clamp01(alpha) * 255));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
    }
}
