using System.Collections.Generic;
using BookBoy.Rules;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Builds PerfLab.unity, the performance test room (backlog T-018): one big room with the real tome books lying in many baked piles, six lamp
/// orbs, 24 small lights, and a PerfLabController with an overlay and hotkeys. The scene is a copy of Lamplit's (player rig, volume, lighting settings)
/// with the hub, the rooms and the campaign removed. Books are NOT stored in the scene: it holds one inactive template per item definition and
/// the layout (positions and rotations); the controller spawns the books at start. Run in Edit Mode with Lamplit.unity open and saved:
/// `unity command eval "return LamplitBuilder.BuildPerfLab(3000);"`. It leaves Lamplit.unity open again at the end.
/// </summary>
public static partial class LamplitBuilder
{
    public const string PerfScenePath = "Assets/Scenes/PerfLab.unity";
    private const float PerfHalf = 13f;   // the room is 26 x 26 m

    public static string BuildPerfLab(int bookCount = 3000, int seed = 4242)
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode first.");
        var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (active.isDirty) throw new System.InvalidOperationException("Save the open scene first.");
        if (active.path == ScenePath && AssetDatabase.LoadAssetAtPath<SceneAsset>(PerfScenePath) == null)
        {
            AssetDatabase.CopyAsset(ScenePath, PerfScenePath);
        }

        if (active.path != PerfScenePath)
        {
            EditorSceneManager.OpenScene(PerfScenePath, OpenSceneMode.Single);
        }

        SetupBookAssets();
        var log = new System.Text.StringBuilder();

        // ---- strip what the perf room does not need (the campaign scene's content and its game UI, which scans all books) ----
        foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            string n = go.name;
            bool strip = n == "Hub" || n == "Room1" || n == "Room2" || n == "Room3" || n == "Campaign" || n == "Game" || n == "CleanupTracker" || n == "HUD_Canvas"
                || n == "PerfRoom" || (n.Length > 3 && n[0] == 'R' && (n[1] == '1' || n[1] == '2' || n[1] == '3') && n[2] == '_');
            if (strip) Object.DestroyImmediate(go);
        }

        // ---- the room ----
        var room = FreshRoot("PerfRoom");
        var wallMat = EnsureMat("Mat_Wall_Perf", new Color(0.42f, 0.40f, 0.34f), 0.1f);
        Floor(room, "Floor", -PerfHalf, PerfHalf, -PerfHalf, PerfHalf, FloorMat("Perf", PerfHalf * 2f, PerfHalf * 2f));
        Ceiling(room, "Ceiling", -PerfHalf, PerfHalf, -PerfHalf, PerfHalf);
        Wall(room, "WallN", false, PerfHalf, -PerfHalf, PerfHalf, null, wallMat);
        Wall(room, "WallS", false, -PerfHalf, -PerfHalf, PerfHalf, null, wallMat);
        Wall(room, "WallE", true, PerfHalf, -PerfHalf, PerfHalf, null, wallMat);
        Wall(room, "WallW", true, -PerfHalf, -PerfHalf, PerfHalf, null, wallMat);

        // ---- lights: six orbs (the last one near the far wall is the 'one shadowed' light), 24 small lights ----
        var orbLights = new List<Light>();
        float[] ox = { -6.5f, 6.5f }; float[] oz = { -8f, 0f, 8f };
        int k = 0;
        foreach (float z in oz)
        {
            foreach (float x in ox)
            {
                string n = "PerfOrb_" + (++k);
                Orb(room, n, new Vector3(x, OrbHeight, z), 3.6f, false, 14f);
                orbLights.Add(room.Find(n).GetComponentInChildren<Light>());
            }
        }

        var small = new List<Light>();
        var smallRoot = FreshChild(room, "SmallLights");
        for (int ix = 0; ix < 6; ix++)
        {
            for (int iz = 0; iz < 4; iz++)
            {
                var go = new GameObject("Small_" + ix + "_" + iz);
                go.transform.SetParent(smallRoot, false);
                go.transform.position = new Vector3(-10.4f + ix * 4.16f, 2.3f, -9.75f + iz * 6.5f);
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1f, 0.82f, 0.58f);
                l.intensity = 1.1f;
                l.range = 5f;
                l.shadows = LightShadows.None;
                l.enabled = false;
                small.Add(l);
            }
        }

        // ---- book templates, one per Room 1 item definition (the 96 letters and numbers) ----
        var templatesRoot = FreshChild(room, "BookTemplates");
        var templates = new List<Book>();
        for (int g = 0; g < 16; g++)
        {
            char letter = (char)('A' + g);
            int setSize = g % 2 == 0 ? 8 : 4;
            for (int n = 1; n <= setSize; n++)
            {
                var def = AssetDatabase.LoadAssetAtPath<BookDefinition>(DefDir + "/Item_R1_" + letter + n + ".asset");
                if (def == null) throw new System.InvalidOperationException("Missing " + letter + n + ": build Room 1 first (LamplitBuilder.BuildRoom1)");
                var t = Item(templatesRoot, "Template_" + letter + n, def, ReadingScales[g % ReadingScales.Length] * WorldScale, false);
                t.gameObject.SetActive(false);
                templates.Add(t);
            }
        }

        // ---- bake the piles with real, temporary books (so the physics proxies get the right sizes), then keep only the layout ----
        var temp = FreshChild(room, "TempBooks");
        var items = new List<Book>(bookCount);
        var tmplOf = new int[bookCount];
        for (int i = 0; i < bookCount; i++)
        {
            tmplOf[i] = i % templates.Count;
            var b = Object.Instantiate(templates[tmplOf[i]], temp);
            b.name = "T" + i;
            b.gameObject.SetActive(true);
            items.Add(b);
        }

        var zone = room.gameObject.AddComponent<RoomZone>();
        zone.roomNumber = 99;
        zone.displayName = "PerfLab";
        zone.items = items.ToArray();
        zone.shelves = new Shelf[0];
        zone.scatterMin = new Vector2(-PerfHalf + 2f, -PerfHalf + 2f);
        zone.scatterMax = new Vector2(PerfHalf - 2f, PerfHalf - 2f);
        zone.pileSpread = 1f;
        zone.maxPiles = 60;
        zone.itemsPerPile = Mathf.Max(18, bookCount / 60);
        string bake = BakePiles(zone, seed);
        log.Append(bake).Append("; ");

        // the layout, ordered by (round within pile, pile) so that the first N books spread evenly over all the piles
        var plan = zone.LastPlan;
        var roundOf = new int[bookCount];
        var counter = new int[plan.PileCount];
        for (int i = 0; i < bookCount; i++) roundOf[i] = counter[plan.PileOf[i]]++;
        var order = new int[bookCount];
        for (int i = 0; i < bookCount; i++) order[i] = i;
        System.Array.Sort(order, (a, c) => roundOf[a] != roundOf[c] ? roundOf[a].CompareTo(roundOf[c]) : plan.PileOf[a].CompareTo(plan.PileOf[c]));
        var pos = new Vector3[bookCount];
        var rot = new Quaternion[bookCount];
        var tOf = new int[bookCount];
        for (int j = 0; j < bookCount; j++)
        {
            var b = items[order[j]];
            pos[j] = b.transform.position;
            rot[j] = b.transform.rotation;
            tOf[j] = tmplOf[order[j]];
        }

        // BakePiles unparented the temporary books; remove them all (they carry no layout any more)
        foreach (var b in items) if (b != null) Object.DestroyImmediate(b.gameObject);
        Object.DestroyImmediate(temp.gameObject);
        Object.DestroyImmediate(zone);

        // ---- the controller and the player start ----
        Light directional = null;
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include)) if (l.type == LightType.Directional) directional = l;
        var ctrl = room.gameObject.AddComponent<PerfLabController>();
        ctrl.templates = templates.ToArray();
        ctrl.templateOf = tOf;
        ctrl.positions = pos;
        ctrl.rotations = rot;
        ctrl.orbs = orbLights.ToArray();
        ctrl.smallLights = small.ToArray();
        ctrl.directional = directional;
        ctrl.countIndex = 2;
        ctrl.lightingPreset = 2;

        var player = GameObject.Find("Player");
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.SetPositionAndRotation(new Vector3(0f, 1f, -PerfHalf + 2f), Quaternion.identity);
            if (cc != null) cc.enabled = true;
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        log.Append("PerfLab saved: " + bookCount + " books in " + plan.PileCount + " piles, " + templates.Count + " templates, " + orbLights.Count + " orbs, " + small.Count + " small lights");
        return log.ToString();
    }

    /// <summary>Opens Lamplit.unity again (the scene the other builders and checks expect).</summary>
    public static string OpenLamplit()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode first.");
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        return "Lamplit.unity is open";
    }
}
