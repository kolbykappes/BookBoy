using System.Collections.Generic;
using BookBoy.Rules;
using UnityEditor;
using UnityEngine;

public static partial class LamplitBuilder
{
    // Rooms sit beside the hub. Left of the hub: Room 1 (z 0..8) and Room 3 (z 8..16). Right of the hub: Room 2 (z 4..12).
    // Each is 10 m wide (x 1.6..11.6 from the hub wall) with the containers along the far wall and loose items scattered in the middle.
    private const float ContainerPitch = 0.62f;
    private const float RoomWidth = 10f;

    public struct RoomSpec
    {
        public int Number;
        public bool Left;
        public float Z0, Z1;
        public bool BuildLowWall, BuildHighWall;
        public Color Wall;

        public float DoorZ => (Z0 + Z1) / 2f;
        public float InnerX => Left ? -1.6f : 1.6f;                         // the hub-side edge of the room
        public float FarWallX => Left ? -11.5f : 11.5f;
        public float ContainerX => Left ? -11.22f : 11.22f;                 // container centre: back against the far wall
        public float ContainerYaw => Left ? -90f : 90f;                     // front faces into the room
        public Vector3 Inward => Left ? Vector3.left * -1f : Vector3.left;  // from the far wall towards the hub (+X for Left rooms)
    }

    public static RoomSpec Spec(int room)
    {
        switch (room)
        {
            case 1: return new RoomSpec { Number = 1, Left = true, Z0 = 0f, Z1 = 8f, BuildLowWall = true, BuildHighWall = true, Wall = new Color(0.40f, 0.48f, 0.38f) };
            case 2: return new RoomSpec { Number = 2, Left = false, Z0 = 4f, Z1 = 12f, BuildLowWall = true, BuildHighWall = true, Wall = new Color(0.34f, 0.46f, 0.50f) };
            default: return new RoomSpec { Number = 3, Left = true, Z0 = 8f, Z1 = 16f, BuildLowWall = false, BuildHighWall = true, Wall = new Color(0.36f, 0.40f, 0.55f) };
        }
    }

    /// <summary>Floor, walls, lights, door sign, entry point and the (empty) RoomZone. Returns the room's root; containers and items go under it.</summary>
    public static Transform BuildShell(RoomSpec s)
    {
        string name = "Room" + s.Number;
        var root = FreshRoot(name);
        var shell = FreshChild(root, "Shell");
        var wallMat = EnsureMat("Mat_Wall_R" + s.Number, s.Wall, 0.1f);
        float xHub = s.Left ? -1.6f : 1.6f, xFar = s.Left ? -11.6f : 11.6f;
        float x0 = Mathf.Min(xHub, xFar), x1 = Mathf.Max(xHub, xFar);

        Floor(shell, "Floor", x0, x1, s.Z0, s.Z1, FloorMat("R" + s.Number, x1 - x0, s.Z1 - s.Z0));
        Wall(shell, "FarWall", true, s.FarWallX, s.Z0, s.Z1, null, wallMat);
        if (s.BuildLowWall) Wall(shell, "WallLow", false, s.Z0, x0, x1, null, wallMat);
        if (s.BuildHighWall) Wall(shell, "WallHigh", false, s.Z1, x0, x1, null, wallMat);

        // the room-side sign over the doorway and the entry point just inside it
        float sx = s.Left ? -1.62f : 1.62f;
        Sign(shell, "DoorSignInside", new Vector3(sx, 2.8f, s.DoorZ), s.Left ? Vector3.left : Vector3.right, new Vector2(1.3f, 0.5f),
            CampaignCatalog.RoomName(s.Number).ToUpperInvariant(), new Color(0.84f, 0.70f, 0.40f), 70);

        var entry = new GameObject("Entry").transform;
        entry.SetParent(root, false);
        entry.SetPositionAndRotation(new Vector3(s.Left ? -2.6f : 2.6f, 1f, s.DoorZ), Quaternion.Euler(0f, s.Left ? 270f : 90f, 0f));

        float cx = (x0 + x1) / 2f, cz = (s.Z0 + s.Z1) / 2f;
        var zone = root.gameObject.AddComponent<RoomZone>();
        zone.roomNumber = s.Number;
        zone.displayName = CampaignCatalog.RoomName(s.Number);
        zone.entryPoint = entry;
        zone.boundsCenter = new Vector3(cx, 1.6f, cz);
        zone.boundsSize = new Vector3(x1 - x0 - 0.2f, 4f, s.Z1 - s.Z0 - 0.2f);
        float sgn = s.Left ? 1f : -1f;   // from the far wall toward the hub
        zone.scatterMin = new Vector2(Mathf.Min(s.FarWallX + sgn * 2.4f, s.FarWallX + sgn * 8.4f), s.Z0 + 1.1f);
        zone.scatterMax = new Vector2(Mathf.Max(s.FarWallX + sgn * 2.4f, s.FarWallX + sgn * 8.4f), s.Z1 - 1.1f);

        // lights: two near the middle, one near the containers casting soft shadows
        Orb(root, "MageLight_R" + s.Number + "_1", new Vector3(cx + (s.Left ? 2.5f : -2.5f), 2.9f, s.Z0 + 2.0f), 3.2f, false, 8f);
        Orb(root, "MageLight_R" + s.Number + "_2", new Vector3(cx + (s.Left ? 2.5f : -2.5f), 2.9f, s.Z1 - 2.0f), 3.2f, false, 8f);
        Orb(root, "MageLight_R" + s.Number + "_3", new Vector3(s.FarWallX + sgn * 3.2f, 2.9f, cz), 3.6f, true, 8f);
        return root;
    }

    /// <summary>Lays n containers in a row along the far wall, centred on the room, and returns them in order.</summary>
    public static List<Bookcase> PlaceContainers(Transform room, RoomSpec s, RoomZone zone, int n, ContainerRules rules, System.Func<int, int> familyOf, System.Func<int, Material> woodOf)
    {
        var holder = FreshChild(room, "Containers");
        var list = new List<Bookcase>();
        float zc = (s.Z0 + s.Z1) / 2f;
        for (int i = 0; i < n; i++)
        {
            float z = zc + (i - (n - 1) / 2f) * ContainerPitch;
            list.Add(Container(holder, "Container_" + (i + 1), new Vector3(s.ContainerX, 0f, z), s.ContainerYaw, rules, familyOf(i), zone, woodOf(i)));
        }

        var shelves = new List<Shelf>();
        foreach (var c in list) shelves.AddRange(c.shelves);
        zone.shelves = shelves.ToArray();
        return list;
    }

    /// <summary>A small side table in a corner of the room holding the finds. Returns the hide spots (one per find, on the table top).</summary>
    public static Transform[] BuildFindTable(Transform room, RoomSpec s, int count)
    {
        var table = FreshChild(room, "FindTable");
        var wood = LoadMat("Mat_Wood");
        float x = s.Left ? -10.2f : 10.2f;
        float z = s.Z1 - 0.7f;
        Cube("Top", table, new Vector3(x, 0.72f, z), new Vector3(0.9f, 0.05f, 0.55f), wood);
        Cube("Legs", table, new Vector3(x, 0.35f, z), new Vector3(0.8f, 0.7f, 0.45f), wood);
        Sign(table, "Label", new Vector3(x, 1.5f, z + 0.27f), Vector3.back, new Vector2(0.55f, 0.2f), "LOST &\nFOUND", new Color(0.84f, 0.70f, 0.40f), 60);

        var spots = new List<Transform>();
        for (int i = 0; i < count; i++)
        {
            var t = new GameObject("FindSpot_" + (i + 1)).transform;
            t.SetParent(table, false);
            t.position = new Vector3(x + (i - (count - 1) / 2f) * 0.3f, 0.745f, z);
            spots.Add(t);
        }

        return spots.ToArray();
    }

    /// <summary>A find: a carried item that is installed at the desk. Placed on its hide spot.</summary>
    public static Book MakeFind(Transform parent, string findId, Color color, Vector3 size, Transform hideSpot)
    {
        var f = CampaignCatalog.FindById(findId);
        var def = Definition("Find_" + findId);
        def.bookId = "find_" + findId;
        def.displayName = f.Name;
        def.findId = findId;
        def.family = 99;
        def.group = 99;
        def.number = 1;
        def.room = 0;
        def.symbol = "";
        def.spineColor = color;
        EditorUtility.SetDirty(def);
        var book = Item(parent, "Find_" + findId, def, size, false);
        book.transform.SetPositionAndRotation(hideSpot.position + Vector3.up * (size.y * 0.5f), Quaternion.Euler(0f, 180f, 0f));
        return book;
    }

    // ---------------------------------------------------------------- Room 1: the Reading Room

    private static readonly Vector3[] ReadingScales =
    {
        new Vector3(0.045f, 0.24f, 0.16f), new Vector3(0.040f, 0.28f, 0.18f), new Vector3(0.050f, 0.32f, 0.15f), new Vector3(0.055f, 0.22f, 0.19f)
    };

    /// <summary>Room 1: 100 books in 20 groups of 5 on 20 five-slot shelves (10 containers). Rule: Group (the first book on a shelf claims it).</summary>
    public static string BuildRoom1()
    {
        RequireLamplitScene();
        var s = Spec(1);
        var root = BuildShell(s);
        var zone = root.GetComponent<RoomZone>();
        var wood = LoadMat("Mat_Wood");

        var containers = PlaceContainers(root, s, zone, 10, ContainerRules.Group, i => PlacementRules.AnyFamily, i => wood);

        // 20 groups of 5 books: a colour and a letter per group
        var itemsRoot = FreshChild(root, "Items");
        var items = new List<Book>();
        var homes = new List<ShelfSlot>();
        const int groups = 20;
        for (int g = 0; g < groups; g++)
        {
            char letter = (char)('A' + g);
            Color color = GroupColor(g, groups);
            var shelf = zone.shelves[g];
            for (int n = 1; n <= 5; n++)
            {
                var def = Definition("Item_R1_" + letter + n);
                def.bookId = "r1_" + char.ToLowerInvariant(letter) + n;
                def.displayName = "Book " + letter;
                def.family = 0;
                def.group = g;
                def.number = n;
                def.room = 1;
                def.symbol = letter.ToString();
                def.spineColor = color;
                def.category = BookCategory.Alchemy;
                def.series = 0;
                def.volume = n;
                EditorUtility.SetDirty(def);
                var book = Item(itemsRoot, "R1_" + letter + n, def, ReadingScales[g % ReadingScales.Length], false);
                items.Add(book);
                homes.Add(shelf.slots[n - 1]);
            }
        }

        zone.items = items.ToArray();
        zone.homeSlots = homes.ToArray();

        // the finds: apron and shoes
        var findsRoot = FreshChild(root, "Finds");
        var hide = BuildFindTable(root, s, 2);
        var apron = MakeFind(findsRoot, "apron", new Color(0.93f, 0.90f, 0.78f), new Vector3(0.22f, 0.30f, 0.06f), hide[0]);
        var shoes = MakeFind(findsRoot, "shoes", new Color(0.18f, 0.25f, 0.55f), new Vector3(0.16f, 0.12f, 0.26f), hide[1]);
        zone.finds = new[] { apron, shoes };
        zone.findHideSpots = hide;

        zone.Scatter(1001);
        RegisterCampaign();
        MarkDirty();
        return "room 1 built: " + containers.Count + " containers, " + zone.shelves.Length + " shelves, " + items.Count + " items, " + zone.finds.Length + " finds";
    }
}
