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

    /// <summary>Real size (1 unit = 1 m). The storybook build used 1.6 here to make books chunky; that made the rooms feel like a giants' hall
    /// next to a child-height eye, so (2026-10-18, backlog T-002) everything is back to adult scale and readability comes from the FOV.</summary>
    public const float WorldScale = 1.0f;

    /// <summary>The storybook layout numbers (room sizes, door positions, light and table offsets) divided by 1.6, so they can be written
    /// in the old units and converted: LayoutK * 11.4 m = the 7.1 m depth of Room 1.</summary>
    public const float LayoutK = 0.625f;

    // Adult scale (see Docs/unity-practices/27-adult-scale-cozy-rooms-and-readable-items.md and backlog T-002 / T-017).
    public const float WallHeight = 2.74f;      // ceiling 9'0"
    public const float WallThickness = 0.2f;
    public const float DoorWidth = 0.91f;       // 3'0"
    public const float DoorHeight = 2.13f;      // 7'0"
    public const float OrbHeight = 2.1f;        // magelights float above head height, under the ceiling
    public const float SignHeight = 2.5f;      // family signs: above the cases (2.31 m with the crown), below the ceiling
    public const float HubHalf = 1.5f;          // hub wall centres are at x = +-HubHalf (a 10 ft hall)
    public const float HubLength = 24.8f * LayoutK;

    // Tomes (Kolby, 2026-10-18): books are 12 to 15 in tall, 10 in deep and 4 in thick (a spine of 0.1016 m). A bookcase is sized around the
    // tallest (15 in = 0.381 m): slots 4.1 in apart (packed: the spine is 4 in), boards and sides 2.4 in thick, 13 in deep, four shelves 0.47 m (18.5 in)
    // apart on a 15 in (0.38 m) plinth, so the top shelf is at 5'10.5" and the case is about 7'7" tall with its crown. Headroom: 0.47 - 0.06 (the board) - 0.381
    // = 0.029 m, just over 1 in above the tallest book, on every row including the top one.
    public const float BookSpine = 0.1016f;     // 4 in
    public const float BookDepth = 0.254f;      // 10 in
    public const float BookMinHeight = 0.3048f; // 12 in
    public const float BookMaxHeight = 0.381f;  // 15 in
    public const float SlotPitch = 0.104f;      // 4.1 in
    public const float BoardThickness = 0.06f;  // shelf boards, top and base cap (2.4 in: a solid, library-weight board)
    public const float SideThickness = 0.06f;
    public const float CaseMargin = 2f * SideThickness + 0.01f;
    public const float CaseDepth = 0.33f;       // 13 in
    public const int ShelfRows = 4;
    public const float BaseHeight = 0.38f;      // 15 in plinth under the first shelf
    public const float ShelfSpacing = 0.47f;    // 18.5 in: a 15 in book, the 2.4 in board and 1 in of headroom
    public const float ContainerHeight = 2.26f; // top shelf at 1.79 m plus the 0.381 m book, 1 in of headroom and the 0.06 m top board
    public const float FrontLine = -0.15f;
    private static readonly float[] ShelfTops = { BaseHeight, BaseHeight + ShelfSpacing, BaseHeight + 2f * ShelfSpacing, BaseHeight + 3f * ShelfSpacing };

    public static float CaseWidth(int slots) { return slots * SlotPitch + CaseMargin; }

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

    /// <summary>A flat ceiling slab whose underside is at y = WallHeight (the rooms were open to a black sky, which made them feel enormous).</summary>
    public static void Ceiling(Transform parent, string name, float x0, float x1, float z0, float z1)
    {
        var mat = EnsureMat("Mat_Ceiling", new Color(0.34f, 0.25f, 0.18f), 0.1f);
        Cube(name, parent, new Vector3((x0 + x1) / 2f, WallHeight + 0.1f, (z0 + z1) / 2f), new Vector3(x1 - x0, 0.2f, z1 - z0), mat, true, false);
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

    /// <summary>One bookcase: a wooden unit with ShelfRows shelves of <paramref name="slots"/> slots (8 for a main case, 4 for a wing). yaw rotates it
    /// about Y (0 = front faces -Z).</summary>
    /// <summary>How a bookcase looks: plain wooden cubes (the old look and the colliders), colliders only (the visual is a separate mesh), or the
    /// Blender-built mesh named by visualMesh (Case_Solo, Case_Wing) with the cubes kept as invisible colliders.</summary>
    public enum CaseLook { Cubes, ColliderOnly, Mesh }

    public static Bookcase Container(Transform parent, string name, Vector3 pos, float yaw, ContainerRules rules, int family, RoomZone room, Material wood, int slots = 5,
        CaseLook look = CaseLook.Cubes, string visualMesh = null)
    {
        float w = CaseWidth(slots), d = CaseDepth, h = ContainerHeight;
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = pos;
        root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        var bookcase = root.AddComponent<Bookcase>();
        bookcase.rules = rules;
        bookcase.containerFamily = family;
        bookcase.room = room;

        Cube("Base", root.transform, new Vector3(0f, BaseHeight / 2f, 0f), new Vector3(w, BaseHeight, d), wood);
        float upper = h - BaseHeight;
        Cube("BaseCap", root.transform, new Vector3(0f, BaseHeight - 0.025f, -0.015f), new Vector3(w + 0.03f, 0.05f, d + 0.03f), wood);   // a lip on the plinth
        Cube("SideL", root.transform, new Vector3(-(w / 2f - SideThickness / 2f), BaseHeight + upper / 2f, 0f), new Vector3(SideThickness, upper, d), wood);
        Cube("SideR", root.transform, new Vector3(w / 2f - SideThickness / 2f, BaseHeight + upper / 2f, 0f), new Vector3(SideThickness, upper, d), wood);
        Cube("Back", root.transform, new Vector3(0f, BaseHeight + upper / 2f, d / 2f - 0.01f), new Vector3(w - 2f * SideThickness, upper, 0.02f), wood);
        for (int b = 1; b < ShelfRows; b++)
        {
            Cube("ShelfBoard_" + b, root.transform, new Vector3(0f, ShelfTops[b] - BoardThickness / 2f, 0f), new Vector3(w - 2f * SideThickness, BoardThickness, d - 0.02f), wood);
        }

        Cube("Top", root.transform, new Vector3(0f, h - BoardThickness / 2f, 0f), new Vector3(w, BoardThickness, d), wood);
        Cube("Crown", root.transform, new Vector3(0f, h + 0.025f, -0.01f), new Vector3(w + 0.04f, 0.05f, d + 0.04f), wood);   // a cornice on top
        if (look != CaseLook.Cubes)
        {
            foreach (Transform c in root.transform)   // keep only the colliders: the visual is the Blender mesh
            {
                var mr = c.GetComponent<MeshRenderer>();
                if (mr != null) { Object.DestroyImmediate(mr); Object.DestroyImmediate(c.GetComponent<MeshFilter>()); }
            }
        }

        if (look == CaseLook.Mesh)
        {
            AddCaseVisual(root.transform, visualMesh, Vector3.zero);
        }

        var highlightMat = LoadMat("Mat_SlotHighlight");
        var shelfList = new List<Shelf>();
        for (int s = 0; s < ShelfRows; s++)
        {
            var shelfGo = new GameObject("Shelf_" + (s + 1));
            shelfGo.transform.SetParent(root.transform, false);
            shelfGo.transform.localPosition = new Vector3(0f, ShelfTops[s], 0f);
            var shelf = shelfGo.AddComponent<Shelf>();
            shelf.bookcase = bookcase;
            var box = shelfGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, (ShelfSpacing - 0.01f) / 2f, 0f);
            box.size = new Vector3(w - 2f * SideThickness, ShelfSpacing - 0.01f, d - 0.02f);   // the aim volume of one row, a little under the shelf spacing

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
                bar.transform.localPosition = new Vector3(0f, 0.002f, BookDepth / 2f);
                bar.transform.localScale = new Vector3(SlotPitch - 0.008f, 0.004f, BookDepth);
                var mr = bar.GetComponent<MeshRenderer>();
                mr.sharedMaterial = highlightMat;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.enabled = false;
                slot.highlight = mr;
                slotList[k] = slot;
            }

            shelf.slots = slotList;
            shelfList.Add(shelf);
            if (look != CaseLook.Cubes)
            {
                AddClaimPlate(shelfGo.transform, shelf, s);
            }
        }

        bookcase.shelves = shelfList.ToArray();
        bookcase.all = new[] { bookcase };
        root.transform.localScale = Vector3.one * WorldScale;   // the whole container (boards, shelves, slots, aim volumes) at world scale
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
        go.transform.localPosition = new Vector3(0f, localY, -0.5f - 0.006f);   // just in front of the spine's flat (the tome's spine apex is at -0.5)
        var ps = item.lossyScale;
        go.transform.localScale = new Vector3(1f / ps.x, 1f / ps.y, 1f / ps.z) * height;
        var pd = go.AddComponent<PixelDigit>();
        pd.value = glyph;
        pd.color = color;
        pd.material = LoadMat("Mat_Digit");
        pd.Rebuild();
    }

    private const string ModelDir = "Assets/_Project/Art/Models";
    private const string WoodTexDir = "Assets/_Project/Art/Textures/Wood";

    /// <summary>The Blender bookcase mesh (Case_Solo, Case_Triple or Case_Wing), or null if it has not been imported.</summary>
    public static Mesh CaseMesh(string meshName)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ModelDir + "/" + meshName + ".fbx"))
        {
            if (o is Mesh m) return m;
        }

        return null;
    }

    /// <summary>Adds the visual of a bookcase (the carved wood mesh with its three materials) as a child at the given local position.</summary>
    public static GameObject AddCaseVisual(Transform parent, string meshName, Vector3 localPos)
    {
        var mesh = CaseMesh(meshName);
        if (mesh == null) throw new System.InvalidOperationException("Missing mesh " + meshName + " (run tools/blender/make_cases.py and LamplitBuilder.SetupCaseAssets())");
        var go = new GameObject("Visual_" + meshName, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        go.GetComponent<MeshRenderer>().sharedMaterials = new[] { LoadMat("Mat_CaseWood"), LoadMat("Mat_CaseBack"), LoadMat("Mat_CaseBrass") };
        return go;
    }

    /// <summary>Imports the three bookcase meshes and the wood textures with the right settings and (re)creates the wood, back-panel and brass
    /// materials. Safe to run repeatedly. Sources: tools/blender/make_cases.py and make_wood_textures.py.</summary>
    public static string SetupCaseAssets()
    {
        foreach (string n in new[] { "Case_Solo", "Case_Triple", "Case_Wing" })
        {
            var model = AssetImporter.GetAtPath(ModelDir + "/" + n + ".fbx") as ModelImporter;
            if (model == null) throw new System.InvalidOperationException("Missing " + n + ".fbx (run tools/blender/make_cases.py)");
            model.globalScale = 1f;
            model.useFileScale = true;   // Blender writes centimetres
            model.materialImportMode = ModelImporterMaterialImportMode.None;
            model.importNormals = ModelImporterNormals.Import;
            model.importTangents = ModelImporterTangents.CalculateMikk;
            model.isReadable = false;
            model.addCollider = false;
            model.meshCompression = ModelImporterMeshCompression.Off;
            model.importBlendShapes = false;
            model.importCameras = false;
            model.importLights = false;
            model.animationType = ModelImporterAnimationType.None;
            model.SaveAndReimport();
        }

        string albedo = WoodTexDir + "/T_Wood_Albedo.png", normal = WoodTexDir + "/T_Wood_Normal.png";
        foreach (var path in new[] { albedo, normal })
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) throw new System.InvalidOperationException("Missing " + path + " (run tools/blender/make_wood_textures.py)");
            ti.textureType = path == normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            ti.wrapMode = TextureWrapMode.Repeat;
            ti.mipmapEnabled = true;
            ti.anisoLevel = 8;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            ti.SaveAndReimport();
        }

        var wood = EnsureMat("Mat_CaseWood", new Color(0.62f, 0.30f, 0.14f), 0.60f);
        wood.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(albedo));
        wood.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normal));
        wood.SetFloat("_BumpScale", 1.0f);
        wood.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(wood);
        var back = EnsureMat("Mat_CaseBack", new Color(0.30f, 0.15f, 0.07f), 0.10f);
        back.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(albedo));
        back.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normal));
        back.SetFloat("_BumpScale", 0.6f);
        back.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(back);
        var brass = EnsureMat("Mat_CaseBrass", new Color(1.0f, 0.78f, 0.34f), 0.45f, 0.12f);
        EditorUtility.SetDirty(brass);
        AssetDatabase.SaveAssets();
        return "case assets set up: " + (CaseMesh("Case_Solo") != null) + " " + (CaseMesh("Case_Triple") != null) + " " + (CaseMesh("Case_Wing") != null);
    }

    /// <summary>Writes the case numbers (from the constants above) to tools/blender/case_params.json for make_cases.py.</summary>
    public static string WriteCaseParams()
    {
        string json = "{\n  \"slot_pitch\": " + SlotPitch.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
            + ",\n  \"side_thickness\": " + SideThickness.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
            + ",\n  \"board_thickness\": " + BoardThickness.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
            + ",\n  \"case_depth\": " + CaseDepth.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
            + ",\n  \"case_margin\": " + CaseMargin.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
            + ",\n  \"base_height\": " + BaseHeight.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
            + ",\n  \"shelf_spacing\": " + ShelfSpacing.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
            + ",\n  \"shelf_rows\": " + ShelfRows
            + ",\n  \"container_height\": " + ContainerHeight.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "\n}\n";
        string path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../tools/blender/case_params.json"));
        System.IO.File.WriteAllText(path, json);
        return "wrote " + path;
    }

    /// <summary>The transparent unlit material the placement ghost is tinted from (Mat_SlotGhost). Created or updated in place.</summary>
    public static Material EnsureGhostMaterial()
    {
        const string path = MatDir + "/Mat_SlotGhost.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(m, path);
        }

        m.SetFloat("_Surface", 1f);                                  // transparent
        m.SetFloat("_Blend", 0f);                                    // alpha blending
        m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetFloat("_ZWrite", 0f);
        m.SetFloat("_Cull", 2f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetOverrideTag("RenderType", "Transparent");
        m.renderQueue = 3000;
        m.SetColor("_BaseColor", new Color(0.25f, 1f, 0.35f, 0.4f));
        EditorUtility.SetDirty(m);
        return m;
    }

    /// <summary>Makes sure the player has a SlotGhost component wired to the ghost material.</summary>
    public static void EnsureSlotGhost(GameObject player)
    {
        var ghost = player.GetComponent<SlotGhost>();
        if (ghost == null) ghost = player.AddComponent<SlotGhost>();
        ghost.template = EnsureGhostMaterial();
        EditorUtility.SetDirty(ghost);
    }

    /// <summary>Puts the claim labels on the brass plate of a shelf (the plate is part of the case mesh): the set's icon and letter appear on it once a set claims the shelf.
    /// Row 0 has its plate on the plinth panel, the other rows on the board nosing. Local positions are in the case's own space.</summary>
    private static void AddClaimPlate(Transform shelfGo, Shelf shelf, int row)
    {
        float plateZ = row == 0 ? 0.209f : ShelfTops[row] - 0.027f;
        float plateY = row == 0 ? -CaseDepth / 2f - 0.005f : -CaseDepth / 2f - 0.0195f;
        var digitMat = LoadMat("Mat_Digit");
        PixelDigit Make(string name, float x)
        {
            var go = new GameObject(name);
            go.transform.SetParent(shelfGo, false);
            go.transform.localPosition = new Vector3(x, plateZ - ShelfTops[row], plateY);
            go.transform.localScale = Vector3.one * 0.028f;
            var pd = go.AddComponent<PixelDigit>();
            pd.material = digitMat;
            pd.value = 10;
            pd.color = new Color(0.20f, 0.11f, 0.03f);
            go.SetActive(false);
            return pd;
        }

        var plate = shelfGo.gameObject.AddComponent<ClaimPlate>();
        plate.shelf = shelf;
        plate.letter = Make("ClaimLetter", 0.026f);
        plate.icon = Make("ClaimIcon", -0.028f);
    }

    public const string BookModelPath = "Assets/_Project/Art/Models/Book_Tome.fbx";
    private const string BookTexDir = "Assets/_Project/Art/Textures/Books";

    /// <summary>Imports the tome mesh and its textures with the right settings and (re)creates the cover and page materials. Safe to run repeatedly
    /// (assets are mutated in place). The mesh and textures come from tools/blender/make_book.py and make_book_textures.py.</summary>
    public static string SetupBookAssets()
    {
        var model = AssetImporter.GetAtPath(BookModelPath) as ModelImporter;
        if (model == null) throw new System.InvalidOperationException("Missing " + BookModelPath + " (run tools/blender/make_book.py)");
        model.globalScale = 1f;
        model.useFileScale = true;   // Blender writes centimetres; convert to metres
        model.materialImportMode = ModelImporterMaterialImportMode.None;
        model.importNormals = ModelImporterNormals.Import;
        model.importTangents = ModelImporterTangents.CalculateMikk;
        model.isReadable = false;
        model.addCollider = false;
        model.meshCompression = ModelImporterMeshCompression.Off;
        model.importBlendShapes = false;
        model.importCameras = false;
        model.importLights = false;
        model.animationType = ModelImporterAnimationType.None;
        model.SaveAndReimport();

        string albedo = BookTexDir + "/T_BookCover_Albedo.png", normal = BookTexDir + "/T_BookCover_Normal.png", pages = BookTexDir + "/T_BookPages_Albedo.png";
        foreach (var path in new[] { albedo, normal, pages })
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) throw new System.InvalidOperationException("Missing " + path + " (run tools/blender/make_book_textures.py)");
            ti.textureType = path == normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            ti.wrapMode = path == pages ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            ti.mipmapEnabled = true;
            ti.anisoLevel = 4;
            ti.textureCompression = TextureImporterCompression.CompressedHQ;
            ti.SaveAndReimport();
        }

        var cover = EnsureMat("Mat_BookCover", Color.white, 0.30f);
        cover.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(albedo));
        cover.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normal));
        cover.SetFloat("_BumpScale", 1.0f);
        cover.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(cover);
        var paper = EnsureMat("Mat_BookPages", Color.white, 0.05f);
        paper.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(pages));
        EditorUtility.SetDirty(paper);
        AssetDatabase.SaveAssets();
        string styled = SetupStyledBookAssets();
        return "book assets set up: mesh " + (BookMesh() != null) + ", materials Mat_BookCover and Mat_BookPages; " + styled;
    }

    public const string StyledBookModelPath = "Assets/_Project/Art/Models/Book_TomeStyled.fbx";
    private const string CoverTexDir = "Assets/_Project/Art/Textures/Books/Covers";

    /// <summary>The six test series of printed covers and spines (tools/covers/make_covers.py). Index = BookDefinition.coverStyle.</summary>
    public static readonly string[] CoverKeys = { "lamps", "catalogue", "letters", "atlas", "mending", "vault" };

    /// <summary>Imports the styled tome (boards, pages, spine submeshes) and its six series of cover and spine textures, and makes Mat_Cover_* and Mat_Spine_*.
    /// Missing files are skipped (the styled books then fall back to the plain tome).</summary>
    public static string SetupStyledBookAssets()
    {
        var model = AssetImporter.GetAtPath(StyledBookModelPath) as ModelImporter;
        if (model == null) return "no styled tome (run tools/blender/make_book.py ... styled)";
        model.globalScale = 1f;
        model.useFileScale = true;
        model.materialImportMode = ModelImporterMaterialImportMode.None;
        model.importNormals = ModelImporterNormals.Import;
        model.importTangents = ModelImporterTangents.CalculateMikk;
        model.isReadable = false;
        model.addCollider = false;
        model.meshCompression = ModelImporterMeshCompression.Off;
        model.importBlendShapes = false;
        model.importCameras = false;
        model.importLights = false;
        model.animationType = ModelImporterAnimationType.None;
        model.SaveAndReimport();

        int made = 0;
        foreach (string key in CoverKeys)
        {
            foreach (string kind in new[] { "Cover", "Spine" })
            {
                string path = CoverTexDir + "/T_" + kind + "_" + key + ".png";
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;
                ti.textureType = TextureImporterType.Default;
                ti.sRGBTexture = true;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.mipmapEnabled = true;
                ti.anisoLevel = 8;
                ti.maxTextureSize = 4096;
                ti.npotScale = TextureImporterNPOTScale.None;   // the spine atlas is 3072 wide: keep it, the eight cells line up
                ti.textureCompression = TextureImporterCompression.CompressedHQ;
                ti.SaveAndReimport();
                var m = EnsureMat("Mat_" + kind + "_" + key, Color.white, 0.30f);
                m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                EditorUtility.SetDirty(m);
                made++;
            }
        }

        AssetDatabase.SaveAssets();
        return "styled tome " + (StyledBookMesh() != null) + ", " + made + " cover and spine materials";
    }

    /// <summary>The styled tome mesh (three submeshes: boards, pages, spine), or null if it has not been imported.</summary>
    public static Mesh StyledBookMesh()
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(StyledBookModelPath))
        {
            if (o is Mesh m) return m;
        }

        return null;
    }

    /// <summary>The tome mesh (a unit box scaled by each item's thickness, height and depth), or null if it has not been imported.</summary>
    public static Mesh BookMesh()
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(BookModelPath))
        {
            if (o is Mesh m) return m;
        }

        return null;
    }

    /// <summary>One sortable item: a Book (the tome mesh: a hardcover with a rounded spine and a page block) with a letter (the group's symbol) and,
    /// for numbered sets, a number on its spine. useBookMesh = false keeps a plain cube (the finds are not books).</summary>
    public static Book Item(Transform parent, string name, BookDefinition def, Vector3 size, bool showNumber, bool useBookMesh = true)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale = size;
        var mesh = useBookMesh ? BookMesh() : null;
        Mesh styledMesh = useBookMesh && def.coverStyle >= 0 && def.coverStyle < CoverKeys.Length ? StyledBookMesh() : null;
        string styleKey = styledMesh != null ? CoverKeys[def.coverStyle] : null;
        if (styledMesh != null && AssetDatabase.LoadAssetAtPath<Material>(MatDir + "/Mat_Cover_" + styleKey + ".mat") == null)
        {
            styledMesh = null;
        }

        if (styledMesh != null)
        {
            mesh = styledMesh;
            go.GetComponent<MeshFilter>().sharedMesh = styledMesh;
            go.GetComponent<MeshRenderer>().sharedMaterials = new[] { LoadMat("Mat_Cover_" + styleKey), LoadMat("Mat_BookPages"), LoadMat("Mat_Spine_" + styleKey) };
        }
        else if (mesh != null)
        {
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterials = new[] { LoadMat("Mat_BookCover"), LoadMat("Mat_BookPages") };
        }
        else
        {
            go.GetComponent<MeshRenderer>().sharedMaterial = LoadMat("Mat_BookSpine");
        }
        go.GetComponent<BoxCollider>().isTrigger = true;
        var book = go.AddComponent<Book>();
        book.definition = def;
        book.currentSlot = null;
        book.placedTime = 0;
        book.ApplyColor();

        // the spine has three plates (tools/blender/make_book_textures.py): icon on top, the set letter in the middle, the volume number at the bottom
        if (styledMesh != null)
        {
            return book;   // the printed spine carries the icon, the title and the volume number; no pixel labels
        }

        Color text = TextColorFor(def.spineColor);
        bool plates = mesh != null;
        if (!string.IsNullOrEmpty(def.symbol))
        {
            Label(go.transform, "Label_Symbol", PixelDigit.LetterValue(def.symbol[0]), plates ? 0.062f : 0.05f, 0.055f * WorldScale, text);
        }

        if (def.iconIndex >= 0 && plates)
        {
            Label(go.transform, "Label_Icon", PixelDigit.IconValue(def.iconIndex), 0.353f, 0.040f * WorldScale, text);
        }

        if (showNumber)
        {
            Label(go.transform, "Label_Number", Mathf.Clamp(def.number, 0, 9), plates ? -0.243f : -0.30f, 0.046f * WorldScale, text);
        }

        return book;
    }

    /// <summary>The 24 set colours (backlog T-010): solved by BookBoy.Rules.Palette.Solve (tools/solve_palette.cs) so that every one keeps a contrast of at least 2.2 against
    /// the wood once the cover texture's darkening is counted (average 3.5), and stays as far from the others as possible for normal vision and the three kinds of
    /// colour blindness (smallest pair distance 0.045 in Oklab). In greedy order: the first k are the best k. The letter, icon and number on the spine carry the identity,
    /// colour alone never does.</summary>
    private static readonly string[] GroupHex =
    {
        "#74FEFD", "#A09600", "#F0DA1C", "#4FAAFE", "#F067A6", "#A5D2A4", "#C9F5A8", "#DEB13A", "#29D5E4", "#30A46F", "#D87DD8", "#9EB47D",
        "#9BEBCC", "#C9D374", "#FB85B2", "#4490FE", "#FEB2FD", "#E9EE6F", "#BBA442", "#D47D73", "#C56FDF", "#DBC70D", "#FCA4D6", "#B2C16D",
    };

    /// <summary>The colour of set number g (the table repeats after 24).</summary>
    public static Color GroupColor(int g, int n)
    {
        ColorUtility.TryParseHtmlString(GroupHex[((g % GroupHex.Length) + GroupHex.Length) % GroupHex.Length], out Color c);
        return c;
    }

    public static void MarkDirty()
    {
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
