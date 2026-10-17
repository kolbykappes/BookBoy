using System.Collections.Generic;
using System.Text;
using BookBoy.Rules;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// The HUD and menus, built in code on the existing HUD canvas: timer / books placed / shelves / upgrade points, what you
/// are carrying, a controls legend that shows which abilities are unlocked, toasts, the upgrade menu (Tab) and an admin menu
/// (F10) for testing. Menus pause the game and free the cursor; they are driven by direct mouse and keyboard handling, so no
/// EventSystem is needed. Keys (read directly, no input-binding changes): Tab upgrades, F10 admin, Esc closes, Up/Down or
/// W/S move, Enter/Space/E choose, 1-9 buy directly, mouse hover and click.
/// </summary>
public class GameUI : MonoBehaviour
{
    private enum Menu
    {
        None,
        Upgrades,
        Admin
    }

    private sealed class Row
    {
        public Image Background;
        public Text Label;
    }

    private const int RowPool = 16;
    private static readonly Color PanelColor = new Color(0.05f, 0.05f, 0.08f, 0.72f);
    private static readonly Color RowColor = new Color(0.14f, 0.14f, 0.19f, 0.9f);
    private static readonly Color RowSelected = new Color(0.25f, 0.38f, 0.62f, 0.95f);
    private static readonly string[] AdminRows =
    {
        "+1 upgrade point", "+5 upgrade points", "+10 upgrade points", "Unlock every upgrade (free)", "Reset points and upgrades", "Close"
    };

    private GameProgress _progress;
    private CarryInventory _carry;
    private PlayerInteractor _interactor;
    private CursorLock _cursor;
    private Font _font;
    private Text _stats;
    private Text _carryText;
    private RectTransform _carryPanel;
    private readonly List<GameObject> _hudPanels = new List<GameObject>();
    private Text _legend;
    private Text _toast;
    private Text _menuTitle;
    private Text _menuHelp;
    private RectTransform _menuPanel;
    private readonly List<Row> _rows = new List<Row>();
    private Menu _menu;
    private int _selected;
    private float _toastUntil;
    private string _lastStats = "";
    private SisterGlow _glow;
    private QuickShelve _shelve;

    private FirstPersonController _fpc;
    private CameraLookBridge _lookBridge;
    private StarterAssetsInputs _inputs;

    public bool MenuOpen => _menu != Menu.None;
    public bool UpgradesOpen => _menu == Menu.Upgrades;
    public bool AdminOpen => _menu == Menu.Admin;

    private void Start()
    {
        _menu = Menu.None;
        Time.timeScale = 1f;
        _progress = FindFirstObjectByType<GameProgress>();
        _carry = FindFirstObjectByType<CarryInventory>();
        _interactor = FindFirstObjectByType<PlayerInteractor>();
        _cursor = FindFirstObjectByType<CursorLock>();
        _fpc = FindFirstObjectByType<FirstPersonController>();
        _lookBridge = FindFirstObjectByType<CameraLookBridge>();
        _inputs = FindFirstObjectByType<StarterAssetsInputs>();
        _glow = FindFirstObjectByType<SisterGlow>();
        _shelve = FindFirstObjectByType<QuickShelve>();
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Transform canvas = GameObject.Find("HUD_Canvas").transform;
        DestroyOld(canvas);
        BuildHud(canvas);
        BuildMenu(canvas);

        if (_progress != null)
        {
            _progress.Changed += OnProgressChanged;
            _progress.PointsEarned += OnPointsEarned;
        }

        if (_carry != null)
        {
            _carry.Changed += RefreshCarry;
        }

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (_progress != null)
        {
            _progress.Changed -= OnProgressChanged;
            _progress.PointsEarned -= OnPointsEarned;
        }

        if (_carry != null)
        {
            _carry.Changed -= RefreshCarry;
        }

        Time.timeScale = 1f;
    }

    private static void DestroyOld(Transform canvas)
    {
        foreach (string n in new[] { "GameUI_Stats", "GameUI_Carry", "GameUI_Legend", "GameUI_Toast", "GameUI_Menu" })
        {
            Transform old = canvas.Find(n);
            if (old != null)
            {
                Destroy(old.gameObject);
            }
        }
    }

    // ---------------------------------------------------------------- building

    private RectTransform MakePanel(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return rt;
    }

    private Text MakeText(string name, Transform parent, int fontSize, TextAnchor align, Vector2 padding)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = padding;
        rt.offsetMax = -padding;
        var t = go.GetComponent<Text>();
        t.font = _font;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = true;
        t.raycastTarget = false;
        return t;
    }

    private void BuildHud(Transform canvas)
    {
        RectTransform stats = MakePanel("GameUI_Stats", canvas, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(440f, 176f), PanelColor);
        _stats = MakeText("Text", stats, 34, TextAnchor.UpperLeft, new Vector2(18f, 12f));
        _hudPanels.Add(stats.gameObject);

        _carryPanel = MakePanel("GameUI_Carry", canvas, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(520f, 120f), PanelColor);
        _carryText = MakeText("Text", _carryPanel, 28, TextAnchor.UpperLeft, new Vector2(18f, 10f));
        _hudPanels.Add(_carryPanel.gameObject);

        RectTransform legend = MakePanel("GameUI_Legend", canvas, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(500f, 292f), PanelColor);
        _legend = MakeText("Text", legend, 26, TextAnchor.UpperLeft, new Vector2(18f, 12f));
        _hudPanels.Add(legend.gameObject);

        RectTransform toast = MakePanel("GameUI_Toast", canvas, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1100f, 70f), new Color(0f, 0f, 0f, 0f));
        _toast = MakeText("Text", toast, 36, TextAnchor.MiddleCenter, Vector2.zero);
        _toast.color = new Color(1f, 0.93f, 0.55f, 0f);
    }

    private void BuildMenu(Transform canvas)
    {
        _menuPanel = MakePanel("GameUI_Menu", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240f, 1000f), new Color(0.03f, 0.03f, 0.06f, 0.94f));
        _menuTitle = MakeText("Title", _menuPanel, 44, TextAnchor.UpperCenter, new Vector2(20f, 18f));
        _menuHelp = MakeText("Help", _menuPanel, 24, TextAnchor.LowerCenter, new Vector2(20f, 14f));
        _menuHelp.color = new Color(0.75f, 0.78f, 0.9f);

        for (int i = 0; i < RowPool; i++)
        {
            RectTransform bg = MakePanel("Row" + i, _menuPanel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f - i * 56f), new Vector2(1180f, 52f), RowColor);
            Text label = MakeText("Label", bg, 25, TextAnchor.MiddleLeft, new Vector2(16f, 2f));
            _rows.Add(new Row { Background = bg.GetComponent<Image>(), Label = label });
        }

        _menuPanel.gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------- refresh

    private void OnProgressChanged()
    {
        RefreshAll();
    }

    private void OnPointsEarned(int amount)
    {
        Toast("+" + amount + " upgrade point" + (amount == 1 ? "" : "s") + "!  Press Tab to spend " + (amount == 1 ? "it" : "them") + ".", 3.5f);
    }

    private void RefreshAll()
    {
        RefreshStats();
        RefreshCarry();
        RefreshLegend();
        if (_menu != Menu.None)
        {
            RefreshMenu();
        }
    }

    private void RefreshStats()
    {
        if (_progress == null || _stats == null)
        {
            return;
        }

        string text = "<b>TIME</b>  " + TimeFormat.Format(_progress.ElapsedSeconds) + (_progress.LevelComplete ? "  (done!)" : "")
            + "\n<b>BOOKS</b>  " + _progress.BooksCorrect + " / " + _progress.BooksTotal
            + "\n<b>SHELVES</b>  " + _progress.ShelvesFinished + " / " + _progress.ShelvesTotal
            + "\n<b>UPGRADE POINTS</b>  <color=#FFE680>" + _progress.Upgrades.Points + "</color>";
        if (text != _lastStats)
        {
            _lastStats = text;
            _stats.text = text;
        }
    }

    private void RefreshCarry()
    {
        if (_carry == null || _carryText == null)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.Append("<b>CARRYING  ").Append(_carry.Count).Append(" / ").Append(_carry.Capacity).Append("</b>");
        if (_carry.IsEmpty)
        {
            sb.Append("\n<color=#9aa0b5>nothing yet</color>");
        }

        for (int i = _carry.Count - 1; i >= 0; i--)
        {
            Book b = _carry.Items[i];
            string name = b != null && b.definition != null ? b.definition.displayName : "?";
            bool current = i == _carry.CurrentIndex;
            sb.Append('\n').Append(current ? "<b><color=#FFE680>> " + name + "</color></b>" : "   " + name);
        }

        _carryText.text = sb.ToString();
        int lines = 1 + Mathf.Max(1, _carry.Count);
        _carryPanel.sizeDelta = new Vector2(520f, 24f + 36f * lines);
    }

    private void RefreshLegend()
    {
        if (_legend == null)
        {
            return;
        }

        UpgradeState u = _progress != null ? _progress.Upgrades : null;
        bool shelve = u != null && u.HasQuickShelve;
        bool glow = u != null && u.HasSisterGlow;
        string Line(string key, string what, bool on)
        {
            return on ? "<b>" + key + "</b>   " + what : "<color=#777b8c><b>" + key + "</b>   " + what + "  (locked)</color>";
        }

        _legend.text = "<b>CONTROLS</b>\n"
            + Line("E", "pick up / place", true) + "\n"
            + Line("Q", "toss held book", true) + "\n"
            + Line("R / Wheel", "next held book", true) + "\n"
            + Line("F", "quick shelve all held", shelve) + "\n"
            + Line("G", "sister glow", glow) + "\n"
            + Line("Tab", "upgrades menu", true) + "\n"
            + Line("F10", "admin (testing)", true);
    }

    public void Toast(string message, float seconds = 2.6f)
    {
        if (_toast == null)
        {
            return;
        }

        _toast.text = message;
        _toastUntil = Time.unscaledTime + seconds;
    }

    // ---------------------------------------------------------------- menus

    public void OpenUpgrades()
    {
        OpenMenu(Menu.Upgrades);
    }

    public void OpenAdmin()
    {
        OpenMenu(Menu.Admin);
    }

    private void OpenMenu(Menu menu)
    {
        bool wasOpen = MenuOpen;
        SetHudVisible(false);
        _menu = menu;
        _selected = 0;
        _menuPanel.gameObject.SetActive(true);
        if (!wasOpen)
        {
            SetGameplayActive(false);
        }

        RefreshMenu();
    }

    public void CloseMenu()
    {
        if (!MenuOpen)
        {
            return;
        }

        _menu = Menu.None;
        _menuPanel.gameObject.SetActive(false);
        SetHudVisible(true);
        SetGameplayActive(true);
    }

    private void SetHudVisible(bool visible)
    {
        foreach (GameObject panel in _hudPanels)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }
    }

    private void SetGameplayActive(bool active)
    {
        if (_fpc != null) _fpc.enabled = active;
        if (_interactor != null) _interactor.enabled = active;
        if (_lookBridge != null) _lookBridge.enabled = active;
        if (!active && _inputs != null)
        {
            _inputs.look = Vector2.zero;
            _inputs.move = Vector2.zero;
            _inputs.sprint = false;
            _inputs.jump = false;
        }

        if (_cursor != null)
        {
            if (active) _cursor.Resume(); else _cursor.Suspend();
        }

        Time.timeScale = active ? 1f : 0f;
    }

    private int RowCount => _menu == Menu.Upgrades ? UpgradeCatalog.All.Count : _menu == Menu.Admin ? AdminRows.Length : 0;

    private void RefreshMenu()
    {
        if (_menu == Menu.None)
        {
            return;
        }

        UpgradeState u = _progress.Upgrades;
        if (_menu == Menu.Upgrades)
        {
            _menuTitle.text = "UPGRADES        <color=#FFE680>" + u.Points + " point" + (u.Points == 1 ? "" : "s") + " to spend</color>";
            _menuHelp.text = "Finish a shelf to earn a point.   Up/Down + Enter, 1-9 (first nine), or click to buy.   Tab or Esc to close.";
        }
        else
        {
            _menuTitle.text = "ADMIN  (testing)        <color=#FFE680>" + u.Points + " points</color>";
            _menuHelp.text = "Up/Down + Enter, or click.   F10 or Esc to close.";
        }

        for (int i = 0; i < RowPool; i++)
        {
            bool used = i < RowCount;
            _rows[i].Background.gameObject.SetActive(used);
            if (!used)
            {
                continue;
            }

            _rows[i].Background.color = i == _selected ? RowSelected : RowColor;
            _rows[i].Label.text = RowText(i);
        }
    }

    /// <summary>The text of a menu row (also used by tests).</summary>
    public string RowText(int index)
    {
        if (_menu == Menu.Admin)
        {
            return index >= 0 && index < AdminRows.Length ? AdminRows[index] : "";
        }

        UpgradeState u = _progress.Upgrades;
        UpgradeNode n = UpgradeCatalog.All[index];
        string state;
        string color;
        if (u.IsUnlocked(n.Id))
        {
            state = "OWNED";
            color = "#7CE08A";
        }
        else if (n.Requires != null && !u.IsUnlocked(n.Requires))
        {
            state = "needs " + UpgradeCatalog.Find(n.Requires).Name;
            color = "#777b8c";
        }
        else if (u.Points < n.Cost)
        {
            state = "need " + (n.Cost - u.Points) + " more";
            color = "#a8a8b8";
        }
        else
        {
            state = "BUY";
            color = "#FFE680";
        }

        return "<color=" + color + "><b>" + (index + 1) + ".  " + n.Name + "</b>  (" + n.Cost + " pt)   " + n.Description + "   [" + state + "]</color>";
    }

    /// <summary>Chooses a menu row (Enter, a number key, or a click). Returns true if something happened.</summary>
    public bool Activate(int row)
    {
        if (_menu == Menu.None || row < 0 || row >= RowCount)
        {
            return false;
        }

        bool done = false;
        if (_menu == Menu.Upgrades)
        {
            UpgradeNode n = UpgradeCatalog.All[row];
            done = _progress.TryBuy(n.Id);
            Toast(done ? "Unlocked " + n.Name + "." : _progress.Upgrades.IsUnlocked(n.Id) ? n.Name + " is already yours." : "Can't buy " + n.Name + " yet.", 2f);
        }
        else
        {
            switch (row)
            {
                case 0: _progress.AddPoints(1); done = true; break;
                case 1: _progress.AddPoints(5); done = true; break;
                case 2: _progress.AddPoints(10); done = true; break;
                case 3: _progress.UnlockAll(); done = true; break;
                case 4: _progress.ResetUpgrades(); done = true; break;
                default: CloseMenu(); return true;
            }
        }

        RefreshAll();
        return done;
    }

    // ---------------------------------------------------------------- per frame

    private void Update()
    {
        RefreshStats();

        if (_toast != null)
        {
            float left = _toastUntil - Time.unscaledTime;
            Color c = _toast.color;
            c.a = Mathf.Clamp01(left / 0.5f);
            _toast.color = c;
        }

        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        if (kb.tabKey.wasPressedThisFrame)
        {
            if (UpgradesOpen) CloseMenu(); else OpenUpgrades();
        }
        else if (kb.f10Key.wasPressedThisFrame)
        {
            if (AdminOpen) CloseMenu(); else OpenAdmin();
        }
        else if (kb.escapeKey.wasPressedThisFrame && MenuOpen)
        {
            CloseMenu();
        }

        if (!MenuOpen)
        {
            return;
        }

        int count = RowCount;
        if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
        {
            _selected = (_selected + 1) % count;
            RefreshMenu();
        }
        else if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
        {
            _selected = (_selected - 1 + count) % count;
            RefreshMenu();
        }

        if (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
        {
            Activate(_selected);
        }

        for (int d = 0; d < 9 && d < count; d++)
        {
            if (kb[Key.Digit1 + d].wasPressedThisFrame)
            {
                _selected = d;
                Activate(d);
            }
        }

        HandleMouse(count);
    }

    private void HandleMouse(int count)
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        Vector2 pos = mouse.position.ReadValue();
        for (int i = 0; i < count; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(_rows[i].Background.rectTransform, pos, null))
            {
                if (_selected != i)
                {
                    _selected = i;
                    RefreshMenu();
                }

                if (mouse.leftButton.wasPressedThisFrame)
                {
                    Activate(i);
                }

                return;
            }
        }
    }
}
