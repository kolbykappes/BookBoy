using System.Collections.Generic;
using BookBoy.Rules;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Builds the Lamplit scene (hub, rooms, items, finds) from code so it is repeatable. Run from the Editor in Edit Mode with Lamplit.unity
/// open, e.g. `unity command eval "LamplitBuilder.BuildHub();"`. Every Build* method removes what it built before and rebuilds it, so it is
/// safe to run again; asset files (definitions, materials) are mutated in place and never recreated (GUIDs stay).
/// </summary>
public static partial class LamplitBuilder
{
    public const string ScenePath = "Assets/Scenes/Lamplit.unity";
    private const string MatDir = "Assets/_Project/Art/Materials";
    private const string DefDir = "Assets/_Project/ScriptableObjects/Lamplit";

    public const float WallHeight = 3.2f;
    public const float WallThickness = 0.2f;
    public const float DoorWidth = 1.4f;
    public const float DoorHeight = 2.4f;
    public const float SlotPitch = 0.072f;
    public const float FrontLine = -0.14f;
    private static readonly float[] ShelfTops = { 0.85f, 1.30f };

    // ---------------------------------------------------------------- scene and materials

    public static void RequireLamplitScene()
    {
        if (EditorApplication.isPlaying)
        {
            throw new System.InvalidOperationException("Stop Play Mode first.");
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            throw new System.InvalidOperationException("Open " + ScenePath + " first (active scene is " + scene.path + ").");
        }
    }

    public static Material EnsureMat(string name, Color color, float smoothness = 0.15f, float metallic = 0f)
    {
        string path = MatDir + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, path);
        }

        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", smoothness);
        m.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(m);
        return m;
    }

    public static Material LoadMat(string name)
    {
        var m = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/" + name + ".mat");
        if (m == null)
        {
            throw new System.InvalidOperationException("Missing material " + name);
        }

        return m;
    }

    /// <summary>Removes any root object with this name and returns a fresh empty one at the origin.</summary>
    public static Transform FreshRoot(string name)
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == name)
            {
                Object.DestroyImmediate(go);
            }
        }

        return new GameObject(name).transform;
    }

    public static Transform FreshChild(Transform parent, string name)
    {
        var old = parent.Find(name);
        if (old != null)
        {
            Object.DestroyImmediate(old.gameObject);
        }

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    // ---------------------------------------------------------------- primitives

    public static GameObject Cube(string name, Transform parent, Vector3 localPos, Vector3 size, Material mat, bool collider = true, bool castShadow = true)
    {
        var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
        p.name = name;
        p.transform.SetParent(parent, false);
        p.transform.localPosition = localPos;
        p.transform.localScale = size;
        var mr = p.GetComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        if (!castShadow)
        {
            mr.shadowCastingMode = ShadowCastingMode.Off;
        }

        if (!collider)
        {
            Object.DestroyImmediate(p.GetComponent<Collider>());
        }

        return p;
    }

    /// <summary>A flat floor slab whose top face is at y = 0.</summary>
    public static void Floor(Transform parent, string name, float x0, float x1, float z0, float z1, Material mat)
    {
        Cube(name, parent, new Vector3((x0 + x1) / 2f, -0.1f, (z0 + z1) / 2f), new Vector3(x1 - x0, 0.2f, z1 - z0), mat, true, false);
    }

    /// <summary>A straight wall with doorways. For alongZ the wall runs along Z at x = fixedCoord, otherwise along X at z = fixedCoord.
    /// gaps = centre positions along the wall of doorways (DoorWidth wide, DoorHeight tall; a lintel closes the top).</summary>
    public static void Wall(Transform parent, string name, bool alongZ, float fixedCoord, float from, float to, float[] gaps, Material mat)
    {
        var segments = new List<Vector2>();
        float cursor = from;
        var sorted = new List<float>(gaps ?? new float[0]);
        sorted.Sort();
        foreach (float g in sorted)
        {
            float a = g - DoorWidth / 2f, b = g + DoorWidth / 2f;
            if (a > cursor)
            {
                segments.Add(new Vector2(cursor, a));
            }

            cursor = b;
        }

        if (to > cursor)
        {
            segments.Add(new Vector2(cursor, to));
        }

        int i = 0;
        foreach (var seg in segments)
        {
            Slab(parent, name + "_" + (++i), alongZ, fixedCoord, seg.x, seg.y, 0f, WallHeight, mat);
        }

        foreach (float g in sorted)
        {
            Slab(parent, name + "_Lintel", alongZ, fixedCoord, g - DoorWidth / 2f, g + DoorWidth / 2f, DoorHeight, WallHeight, mat);
        }
    }

    private static void Slab(Transform parent, string name, bool alongZ, float fixedCoord, float a, float b, float y0, float y1, Material mat)
    {
        float len = b - a, mid = (a + b) / 2f, h = y1 - y0;
        var pos = alongZ ? new Vector3(fixedCoord, y0 + h / 2f, mid) : new Vector3(mid, y0 + h / 2f, fixedCoord);
        var size = alongZ ? new Vector3(WallThickness, h, len) : new Vector3(len, h, WallThickness);
        Cube(name, parent, pos, size, mat, true, true);
    }

    // ---------------------------------------------------------------- lights

    public static void Orb(Transform parent, string name, Vector3 pos, float intensity, bool shadows, float range = 9f)
    {
        var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = name;
        orb.transform.SetParent(parent, false);
        orb.transform.position = pos;
        orb.transform.localScale = Vector3.one * 0.24f;
        Object.DestroyImmediate(orb.GetComponent<Collider>());
        var mr = orb.GetComponent<MeshRenderer>();
        mr.sharedMaterial = LoadMat("Mat_Orb");
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;

        var lightGo = new GameObject("OrbLight");
        lightGo.transform.SetParent(orb.transform, false);
        lightGo.transform.localScale = Vector3.one / 0.24f;
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.82f, 0.58f);
        light.intensity = intensity;
        light.range = range;
        light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        if (shadows)
        {
            light.shadowStrength = 0.72f;
            light.shadowBias = 0.03f;
            light.shadowNormalBias = 0.5f;
        }

        orb.AddComponent<MageLight>();
    }

    // ---------------------------------------------------------------- signs

    /// <summary>A world-space plaque with text. facing is the direction the text faces (the plaque's -Z points the other way, so we look along -facing).</summary>
    public static Text Sign(Transform parent, string name, Vector3 worldPos, Vector3 facing, Vector2 sizeMeters, string text, Color face, int maxFont = 80)
    {
        var sign = new GameObject(name, typeof(RectTransform), typeof(Canvas));
        sign.transform.SetParent(parent, false);
        sign.transform.position = worldPos;
        // a UI canvas shows its content toward -Z; rotate so -Z points along 'facing'
        sign.transform.rotation = Quaternion.LookRotation(-facing, Vector3.up);
        sign.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var rt = (RectTransform)sign.transform;
        float px = 1000f;   // 1 px = 1 mm
        rt.sizeDelta = new Vector2(sizeMeters.x * px, sizeMeters.y * px);
        rt.localScale = Vector3.one / px;

        var frame = new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        frame.transform.SetParent(sign.transform, false);
        Stretch((RectTransform)frame.transform, 0f);
        frame.GetComponent<Image>().color = new Color(0.30f, 0.18f, 0.07f);
        var faceGo = new GameObject("Face", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        faceGo.transform.SetParent(sign.transform, false);
        Stretch((RectTransform)faceGo.transform, 10f);
        faceGo.GetComponent<Image>().color = face;

        var label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        label.transform.SetParent(sign.transform, false);
        Stretch((RectTransform)label.transform, 14f);
        var t = label.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = maxFont;
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = 12;
        t.resizeTextMaxSize = maxFont;
        t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.16f, 0.09f, 0.03f);
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.text = text;
        return t;
    }

    private static void Stretch(RectTransform r, float inset)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(inset, inset);
        r.offsetMax = new Vector2(-inset, -inset);
    }

    // ---------------------------------------------------------------- containers

    /// <summary>One container: a wooden unit with two shelves of five slots. yaw rotates it about Y (0 = front faces -Z).</summary>
    public static Bookcase Container(Transform parent, string name, Vector3 pos, float yaw, ContainerRules rules, int family, RoomZone room, Material wood, int slots = 5)
    {
        float w = slots * SlotPitch + 0.152f, d = 0.36f, h = 1.79f;
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        var bookcase = root.AddComponent<Bookcase>();
        bookcase.rules = rules;
        bookcase.containerFamily = family;
        bookcase.room = room;

        Cube("Base", root.transform, new Vector3(0f, 0.425f, 0f), new Vector3(w, 0.85f, d), wood);
        float upper = h - 0.85f;
        Cube("SideL", root.transform, new Vector3(-(w / 2f - 0.02f), 0.85f + upper / 2f, 0f), new Vector3(0.04f, upper, d), wood);
        Cube("SideR", root.transform, new Vector3(w / 2f - 0.02f, 0.85f + upper / 2f, 0f), new Vector3(0.04f, upper, d), wood);
        Cube("Back", root.transform, new Vector3(0f, 0.85f + upper / 2f, d / 2f - 0.01f), new Vector3(w - 0.08f, upper, 0.02f), wood);
        Cube("ShelfBoard", root.transform, new Vector3(0f, ShelfTops[1] - 0.025f, 0f), new Vector3(w - 0.08f, 0.05f, d - 0.02f), wood);
        Cube("Top", root.transform, new Vector3(0f, h - 0.02f, 0f), new Vector3(w, 0.04f, d), wood);

        var highlightMat = LoadMat("Mat_SlotHighlight");
        var shelfList = new List<Shelf>();
        for (int s = 0; s < 2; s++)
        {
            var shelfGo = new GameObject("Shelf_" + (s + 1));
            shelfGo.transform.SetParent(root.transform, false);
            shelfGo.transform.localPosition = new Vector3(0f, ShelfTops[s], 0f);
            var shelf = shelfGo.AddComponent<Shelf>();
            shelf.bookcase = bookcase;
            var box = shelfGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 0.2f, 0f);
            box.size = new Vector3(w - 0.08f, 0.40f, d - 0.02f);

            var slotList = new ShelfSlot[slots];
            for (int k = 0; k < slots; k++)
            {
                var slotGo = new GameObject("Slot_" + (k + 1));
                slotGo.transform.SetParent(shelfGo.transform, false);
                slotGo.transform.localPosition = new Vector3((k - (slots - 1) / 2f) * SlotPitch, 0f, FrontLine);
                var slot = slotGo.AddComponent<ShelfSlot>();
                slot.shelf = shelf;
                slot.slotIndex = k;

                var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = "Highlight";
                Object.DestroyImmediate(bar.GetComponent<Collider>());
                bar.transform.SetParent(slotGo.transform, false);
                bar.transform.localPosition = new Vector3(0f, 0.002f, 0.1f);
                bar.transform.localScale = new Vector3(SlotPitch - 0.008f, 0.004f, 0.2f);
                var mr = bar.GetComponent<MeshRenderer>();
                mr.sharedMaterial = highlightMat;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.enabled = false;
                slot.highlight = mr;
                slotList[k] = slot;
            }

            shelf.slots = slotList;
            shelfList.Add(shelf);
        }

        bookcase.shelves = shelfList.ToArray();
        bookcase.all = new[] { bookcase };
        return bookcase;
    }

    // ---------------------------------------------------------------- items

    public static BookDefinition Definition(string fileName)
    {
        if (!AssetDatabase.IsValidFolder(DefDir))
        {
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Lamplit");
        }

        string path = DefDir + "/" + fileName + ".asset";
        var d = AssetDatabase.LoadAssetAtPath<BookDefinition>(path);
        if (d == null)
        {
            d = ScriptableObject.CreateInstance<BookDefinition>();
            AssetDatabase.CreateAsset(d, path);
        }

        return d;
    }

    private static Color TextColorFor(Color bg)
    {
        return (0.2126f * bg.r + 0.7152f * bg.g + 0.0722f * bg.b) > 0.4f ? new Color(0.1f, 0.07f, 0.04f) : Color.white;
    }

    private static void Label(Transform item, string name, int glyph, float localY, float height, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(item, false);
        go.transform.localPosition = new Vector3(0f, localY, -0.5f - 0.02f);
        var ps = item.lossyScale;
        go.transform.localScale = new Vector3(1f / ps.x, 1f / ps.y, 1f / ps.z) * height;
        var pd = go.AddComponent<PixelDigit>();
        pd.value = glyph;
        pd.color = color;
        pd.material = LoadMat("Mat_Digit");
        pd.Rebuild();
    }

    /// <summary>One sortable item: a cube-shaped Book with a letter (the group's symbol) and, for numbered sets, a number on its spine.</summary>
    public static Book Item(Transform parent, string name, BookDefinition def, Vector3 size, bool showNumber)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().sharedMaterial = LoadMat("Mat_BookSpine");
        go.GetComponent<BoxCollider>().isTrigger = true;
        var book = go.AddComponent<Book>();
        book.definition = def;
        book.currentSlot = null;
        book.placedTime = 0;
        book.ApplyColor();

        Color text = TextColorFor(def.spineColor);
        if (!string.IsNullOrEmpty(def.symbol))
        {
            Label(go.transform, "Label_Symbol", PixelDigit.LetterValue(def.symbol[0]), showNumber ? 0.30f : 0.05f, 0.032f, text);
        }

        if (showNumber)
        {
            Label(go.transform, "Label_Number", Mathf.Clamp(def.number, 0, 9), -0.30f, 0.030f, text);
        }

        return book;
    }

    /// <summary>A distinct colour for group number g of n (hue steps, alternating lightness).</summary>
    public static Color GroupColor(int g, int n)
    {
        float hue = (g * 0.618034f) % 1f;   // golden-ratio steps keep neighbouring groups apart
        float sat = 0.55f + 0.25f * ((g % 3) / 2f);
        float val = g % 2 == 0 ? 0.85f : 0.60f;
        return Color.HSVToRGB(hue, sat, val);
    }

    public static void MarkDirty()
    {
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
