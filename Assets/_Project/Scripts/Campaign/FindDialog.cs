using BookBoy.Rules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// The discovery dialog: the first time the player picks up a find that has a real model, the game pauses and a card comes up with a turning 3D view of the item,
/// a few lines in the player's voice about how it helped Uncle Archie, and what it gives (carry 2 more books, move 15% faster). Closed with E, Enter, Space, Esc or a
/// click. Built in code on the HUD canvas like the other menus (legacy UI Text, so no extra package). The fonts are Windows system fonts for now (Georgia): replace
/// them with an open-licence face before release (see Docs/15-art-direction.md, handwriting).
/// </summary>
public class FindDialog : MonoBehaviour
{
    private static readonly Color Ink = new Color(0.17f, 0.11f, 0.07f);
    private static readonly Color Accent = new Color(0.50f, 0.33f, 0.14f);
    private static readonly Color Brass = new Color(0.72f, 0.54f, 0.24f);
    private static readonly Color Leather = new Color(0.16f, 0.095f, 0.055f);

    [Tooltip("Open the dialog by itself when a find with a model is picked up for the first time (tests turn it off).")]
    public bool autoShow = true;

    private CampaignController _campaign;
    private GameUI _ui;
    private CarryInventory _carry;
    private FindPreview _preview;
    private Font _serif;
    private RectTransform _root;
    private RectTransform _card;
    private CanvasGroup _group;
    private RectTransform _column;
    private RectTransform _rule;
    private Text _kicker;
    private Text _title;
    private Text _story;
    private Text _benefit;
    private Text _hint;
    private RawImage _view;
    private Sprite _paper;
    private Sprite _vignette;
    private float _openedAt;
    private bool _open;

    public bool IsOpen => _open;
    public string TitleText => _title != null ? _title.text : "";
    public string StoryText => _story != null ? _story.text : "";
    public string BenefitText => _benefit != null ? _benefit.text : "";
    public string HintText => _hint != null ? _hint.text : "";
    public string KickerText => _kicker != null ? _kicker.text : "";
    public FindPreview Preview => _preview;

    private void Start()
    {
        _campaign = FindFirstObjectByType<CampaignController>();
        _ui = FindFirstObjectByType<GameUI>();
        _carry = FindFirstObjectByType<CarryInventory>();
        if (_campaign != null)
        {
            _campaign.FindDiscovered += OnFindDiscovered;
        }
    }

    private void OnDestroy()
    {
        if (_campaign != null)
        {
            _campaign.FindDiscovered -= OnFindDiscovered;
        }

        if (_open && _ui != null)
        {
            _ui.EndModal();
        }

        if (_root != null)
        {
            Destroy(_root.gameObject);
        }

        if (_paper != null)
        {
            Destroy(_paper.texture);
            Destroy(_paper);
        }

        if (_vignette != null)
        {
            Destroy(_vignette.texture);
            Destroy(_vignette);
        }
    }

    private void OnFindDiscovered(FindDef find, Book book)
    {
        if (autoShow && find != null && book != null && book.definition != null && book.definition.discoveryDialog)
        {
            Show(find, book);
        }
    }

    /// <summary>Opens the dialog for a find that is in the player's hands.</summary>
    public void Show(FindDef find, Book book)
    {
        if (find == null || book == null)
        {
            return;
        }

        if (_root == null)
        {
            Build();
        }

        if (_preview == null)
        {
            _preview = gameObject.AddComponent<FindPreview>();
        }

        _kicker.text = "A FIND IN THE " + CampaignCatalog.RoomName(find.Room).ToUpperInvariant();
        _title.text = find.Title;
        // a one-line heading lets the rule and the story move up
        float shift = find.Title.Length <= 22 ? 64f : 0f;
        _rule.anchoredPosition = new Vector2(0f, -180f + shift);
        _story.rectTransform.anchoredPosition = new Vector2(0f, -204f + shift);
        _story.text = find.Story;
        _benefit.text = find.Effect;
        _hint.text = find.Hint;

        Vector3 real = _carry != null ? _carry.RealScale(book) : book.transform.localScale;
        _preview.Show(book, real, book.definition.hangEuler);
        _view.texture = _preview.Texture;

        _root.gameObject.SetActive(true);
        _root.SetAsLastSibling();
        _group.alpha = 0f;
        _card.localScale = Vector3.one * 0.94f;
        _openedAt = Time.unscaledTime;
        _open = true;
        if (_ui != null)
        {
            _ui.BeginModal();
        }
    }

    public void Close()
    {
        if (!_open)
        {
            return;
        }

        _open = false;
        _root.gameObject.SetActive(false);
        _preview?.Hide();
        if (_ui != null)
        {
            _ui.EndModal();
        }
    }

    private void Update()
    {
        if (!_open)
        {
            return;
        }

        float t = Mathf.Clamp01((Time.unscaledTime - _openedAt) / 0.32f);
        float ease = 1f - (1f - t) * (1f - t);
        _group.alpha = ease;
        _card.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, ease);
        _preview.Tick(Time.unscaledDeltaTime);

        if (Time.unscaledTime - _openedAt < 0.45f)
        {
            return;   // the key that picked the find up must not close it again
        }

        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool close = (kb != null && (kb.eKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
                     || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        if (close)
        {
            Close();
        }
    }

    // ---------------------------------------------------------------- building

    private void Build()
    {
        _serif = Font.CreateDynamicFontFromOSFont(new[] { "Georgia", "Cambria", "Palatino Linotype", "Times New Roman" }, 32) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _paper = MakePaperSprite();
        _vignette = MakeVignetteSprite();

        Transform canvas = GameObject.Find("HUD_Canvas").transform;
        _root = Rect("GameUI_FindDialog", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        _root.offsetMin = Vector2.zero;
        _root.offsetMax = Vector2.zero;
        _group = _root.gameObject.AddComponent<CanvasGroup>();
        _group.blocksRaycasts = false;
        Image dim = _root.gameObject.AddComponent<Image>();
        dim.color = new Color(0.02f, 0.012f, 0.008f, 0.66f);
        dim.raycastTarget = false;

        _card = Rect("Card", _root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 780f));
        Fill(Rect("Shadow", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14f, -16f), new Vector2(1400f, 780f)), new Color(0f, 0f, 0f, 0.5f));
        Fill(Rect("Border", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 780f)), Leather);
        Fill(Rect("BrassLine", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1382f, 762f)), Brass);
        RectTransform paper = Rect("Paper", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1372f, 752f));
        Image paperImg = paper.gameObject.AddComponent<Image>();
        paperImg.sprite = _paper;
        paperImg.color = Color.white;
        paperImg.raycastTarget = false;

        // the picture: a brass frame, a dark well, the studio's render and a soft vignette over it
        Vector2 pic = new Vector2(-352f, 24f);
        Fill(Rect("FrameBrass", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pic, new Vector2(648f, 648f)), Brass);
        Fill(Rect("FrameWell", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pic, new Vector2(636f, 636f)), new Color(0.07f, 0.05f, 0.035f));
        RectTransform view = Rect("View", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pic, new Vector2(624f, 624f));
        _view = view.gameObject.AddComponent<RawImage>();
        _view.raycastTarget = false;
        RectTransform vig = Rect("Vignette", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pic, new Vector2(624f, 624f));
        Image vigImg = vig.gameObject.AddComponent<Image>();
        vigImg.sprite = _vignette;
        vigImg.raycastTarget = false;

        // the text column: heading at the top, the story under it, what it gives and what to do with it fixed to the bottom
        _column = Rect("Column", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(12f, 330f), new Vector2(650f, 680f), true);
        _kicker = Label("Kicker", _column, 0f, 30f, 21, FontStyle.Bold, Accent, TextAnchor.UpperLeft);
        _title = Label("Title", _column, -36f, 140f, 54, FontStyle.Bold, Ink, TextAnchor.UpperLeft);
        _rule = Rect("Rule", _column, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -180f), new Vector2(650f, 3f), true);
        Fill(_rule, new Color(Ink.r, Ink.g, Ink.b, 0.55f));
        _story = Label("Story", _column, -204f, 280f, 29, FontStyle.Italic, Ink, TextAnchor.UpperLeft);
        _story.lineSpacing = 1.12f;
        Label("GivesKicker", _column, -500f, 28f, 20, FontStyle.Bold, Accent, TextAnchor.UpperLeft).text = "WHAT IT GIVES YOU";
        RectTransform chip = Rect("Chip", _column, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -530f), new Vector2(650f, 70f), true);
        Fill(chip, new Color(0.20f, 0.125f, 0.07f));
        Fill(Rect("ChipBar", chip, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(9f, 70f)), Brass);
        _benefit = Label("Benefit", chip, 0f, 70f, 36, FontStyle.Bold, new Color(0.97f, 0.91f, 0.76f), TextAnchor.MiddleLeft);
        _benefit.rectTransform.anchoredPosition = new Vector2(28f, 0f);
        _benefit.rectTransform.sizeDelta = new Vector2(-28f, 70f);
        _hint = Label("Hint", _column, -614f, 62f, 24, FontStyle.Italic, Accent, TextAnchor.UpperLeft);

        // the key hint under the picture
        RectTransform cap = Rect("KeyCap", _card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-596f, -340f), new Vector2(50f, 50f));
        Fill(Rect("CapEdge", cap, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(50f, 50f)), Ink);
        Fill(Rect("CapFace", cap, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(45f, 45f)), new Color(0.93f, 0.87f, 0.72f));
        Text key = Label("Key", cap, 0f, 50f, 28, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        key.text = "E";
        key.rectTransform.anchorMin = Vector2.zero;
        key.rectTransform.anchorMax = Vector2.one;
        key.rectTransform.offsetMin = Vector2.zero;
        key.rectTransform.offsetMax = Vector2.zero;
        Text cont = Label("Continue", _card, 0f, 40f, 26, FontStyle.Italic, Accent, TextAnchor.MiddleLeft);
        cont.text = "Continue";
        cont.rectTransform.anchorMin = cont.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        cont.rectTransform.pivot = new Vector2(0f, 0.5f);
        cont.rectTransform.anchoredPosition = new Vector2(-560f, -340f);
        cont.rectTransform.sizeDelta = new Vector2(260f, 40f);

        _root.gameObject.SetActive(false);
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, bool topLeft = false)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = topLeft ? new Vector2(0f, 1f) : (anchorMin == anchorMax ? anchorMin : new Vector2(0.5f, 0.5f));
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    private static void Fill(RectTransform rt, Color color)
    {
        Image img = rt.gameObject.GetComponent<Image>() ?? rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private Text Label(string name, Transform parent, float y, float height, int size, FontStyle style, Color color, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(0f, height);
        var t = go.GetComponent<Text>();
        t.font = _serif;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = anchor;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        t.supportRichText = false;
        return t;
    }

    /// <summary>Aged cream paper: warm, a little mottled, darker toward the edges (computed once).</summary>
    private static Sprite MakePaperSprite()
    {
        const int w = 256, h = 160;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        var px = new Color32[w * h];
        var rng = new System.Random(1947);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w * 2f - 1f, v = (y + 0.5f) / h * 2f - 1f;
                float edge = Mathf.Clamp01(Mathf.Max(Mathf.Abs(u), Mathf.Abs(v)));
                float vignette = Mathf.Pow(edge, 4f) * 0.20f + (u * u + v * v) * 0.04f;
                float blotch = (Mathf.PerlinNoise(x * 0.045f + 3f, y * 0.045f + 9f) - 0.5f) * 0.10f + (Mathf.PerlinNoise(x * 0.25f, y * 0.25f) - 0.5f) * 0.035f;
                float grain = ((float)rng.NextDouble() - 0.5f) * 0.03f;
                float l = 0.93f - vignette + blotch + grain;
                px[y * w + x] = new Color32((byte)(Mathf.Clamp01(l * 0.97f) * 255), (byte)(Mathf.Clamp01(l * 0.90f) * 255), (byte)(Mathf.Clamp01(l * 0.74f) * 255), 255);
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>A soft dark ring around the picture (transparent in the middle).</summary>
    private static Sprite MakeVignetteSprite()
    {
        const int n = 128;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                float r = Mathf.Sqrt(u * u + v * v);
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.62f, 1.38f, r)) * 0.72f;
                px[y * n + x] = new Color32(10, 6, 3, (byte)(a * 255));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
    }
}
