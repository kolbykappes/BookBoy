using System.Collections.Generic;
using BookBoy.Rules;
using UnityEditor;
using UnityEngine;

public static partial class LamplitBuilder
{
    // Rooms sit beside the hub. Left of the hub: Room 1 (z 0..11.4) and Room 3 (z 11.4..24.8). Right of the hub: Room 2 (z 5.7..19.1).
    // Each is RoomWidth wide with the containers (at world scale) along the far wall and the piles of loose items in the middle.
    private static float ContainerPitch => CaseWidth(5) + 0.08f;   // legacy 5-slot rows (Rooms 2 and 3 are not converted yet)
    private const float RoomWidth = 12f * LayoutK;   // 7.5 m (24'7")
    private const float OrbIntensity = 5.0f;   // provisional: the lamps are lower (2.1 m) and the rooms smaller, so the storybook 4.4 to 5 is too bright; retune in the lighting pass (T-003)
    private const float OrbRange = 9f;

    public struct RoomSpec
    {
        public int Number;
        public bool Left;
        public float Z0, Z1;
        public bool BuildLowWall, BuildHighWall;
        public Color Wall;

        public float DoorZ => (Z0 + Z1) / 2f;
        public float InnerX => Left ? -(HubHalf + 0.1f) : HubHalf + 0.1f;   // the hub-side edge of the room
        public float FarWallX => Left ? -(HubHalf + RoomWidth) : HubHalf + RoomWidth;
        public float ContainerX => FarWallX + (Left ? 1f : -1f) * (0.1f + CaseDepth * WorldScale / 2f);   // container centre: back against the far wall
        public float ContainerYaw => Left ? -90f : 90f;                     // front faces into the room
        public Vector3 Inward => Left ? Vector3.left * -1f : Vector3.left;  // from the far wall towards the hub (+X for Left rooms)
    }

    public static RoomSpec Spec(int room)
    {
        switch (room)
        {
            case 1: return new RoomSpec { Number = 1, Left = true, Z0 = 0f, Z1 = 11.4f * LayoutK, BuildLowWall = true, BuildHighWall = true, Wall = new Color(0.40f, 0.48f, 0.38f) };
            case 2: return new RoomSpec { Number = 2, Left = false, Z0 = 5.7f * LayoutK, Z1 = 19.1f * LayoutK, BuildLowWall = true, BuildHighWall = true, Wall = new Color(0.34f, 0.46f, 0.50f) };
            default: return new RoomSpec { Number = 3, Left = true, Z0 = 11.4f * LayoutK, Z1 = 24.8f * LayoutK, BuildLowWall = false, BuildHighWall = true, Wall = new Color(0.36f, 0.40f, 0.55f) };
        }
    }

    /// <summary>Floor, walls, lights, door sign, entry point and the (empty) RoomZone. Returns the room's root; containers and items go under it.</summary>
    public static Transform BuildShell(RoomSpec s)
    {
        string name = "Room" + s.Number;
        var root = FreshRoot(name);
        var shell = FreshChild(root, "Shell");
        var wallMat = EnsureMat("Mat_Wall_R" + s.Number, s.Wall, 0.1f);
        float xHub = s.InnerX, xFar = s.FarWallX + (s.Left ? -0.1f : 0.1f);
        float x0 = Mathf.Min(xHub, xFar), x1 = Mathf.Max(xHub, xFar);

        Floor(shell, "Floor", x0, x1, s.Z0, s.Z1, FloorMat("R" + s.Number, x1 - x0, s.Z1 - s.Z0));
        Ceiling(shell, "Ceiling", x0, x1, s.Z0, s.Z1);
        Wall(shell, "FarWall", true, s.FarWallX, s.Z0, s.Z1, null, wallMat);
        if (s.BuildLowWall) Wall(shell, "WallLow", false, s.Z0, x0, x1, null, wallMat);
        if (s.BuildHighWall) Wall(shell, "WallHigh", false, s.Z1, x0, x1, null, wallMat);

        // the room-side sign over the doorway and the entry point just inside it
        float sx = s.Left ? -(HubHalf + 0.12f) : HubHalf + 0.12f;
        Sign(shell, "DoorSignInside", new Vector3(sx, (DoorHeight + WallHeight) / 2f, s.DoorZ), s.Left ? Vector3.left : Vector3.right, new Vector2(1.2f, 0.42f),
            CampaignCatalog.RoomName(s.Number).ToUpperInvariant(), new Color(0.84f, 0.70f, 0.40f), 80);

        var entry = new GameObject("Entry").transform;
        entry.SetParent(root, false);
        entry.SetPositionAndRotation(new Vector3(s.Left ? -(HubHalf + 0.8f) : HubHalf + 0.8f, 1f, s.DoorZ), Quaternion.Euler(0f, s.Left ? 270f : 90f, 0f));

        float cx = (x0 + x1) / 2f, cz = (s.Z0 + s.Z1) / 2f;
        var zone = root.gameObject.AddComponent<RoomZone>();
        zone.roomNumber = s.Number;
        zone.displayName = CampaignCatalog.RoomName(s.Number);
        zone.entryPoint = entry;
        zone.boundsCenter = new Vector3(cx, 1.4f, cz);
        zone.boundsSize = new Vector3(x1 - x0 - 0.2f, 3f, s.Z1 - s.Z0 - 0.2f);
        float sgn = s.Left ? 1f : -1f;   // from the far wall toward the hub
        zone.scatterMin = new Vector2(Mathf.Min(s.FarWallX + sgn * 3.2f * LayoutK, s.FarWallX + sgn * 9.4f * LayoutK), s.Z0 + 1.4f * LayoutK);
        zone.scatterMax = new Vector2(Mathf.Max(s.FarWallX + sgn * 3.2f * LayoutK, s.FarWallX + sgn * 9.4f * LayoutK), s.Z1 - 1.4f * LayoutK);
        zone.pileSpread = WorldScale;

        // lights: two near the middle, one near the containers casting soft shadows
        Orb(root, "MageLight_R" + s.Number + "_1", new Vector3(cx + (s.Left ? 2.5f : -2.5f) * LayoutK, OrbHeight, s.Z0 + 2.8f * LayoutK), OrbIntensity, false, OrbRange);
        Orb(root, "MageLight_R" + s.Number + "_2", new Vector3(cx + (s.Left ? 2.5f : -2.5f) * LayoutK, OrbHeight, s.Z1 - 2.8f * LayoutK), OrbIntensity, false, OrbRange);
        Orb(root, "MageLight_R" + s.Number + "_3", new Vector3(s.FarWallX + sgn * 4.0f * LayoutK, OrbHeight, cz), OrbIntensity * 1.1f, true, OrbRange);
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
        float x = s.FarWallX + (s.Left ? 1f : -1f) * 2.4f * LayoutK;
        float z = s.Z1 - 1.3f * LayoutK;
        float len = 0.55f * count * WorldScale + 0.5f;
        Cube("Top", table, new Vector3(x, 0.9f, z), new Vector3(len, 0.06f, 1.0f), wood);
        Cube("Legs", table, new Vector3(x, 0.44f, z), new Vector3(len - 0.15f, 0.88f, 0.85f), wood);
        Sign(table, "Label", new Vector3(x, 1.7f, z + 0.5f), Vector3.back, new Vector2(0.8f, 0.3f), "LOST &\nFOUND", new Color(0.84f, 0.70f, 0.40f), 70);

        var spots = new List<Transform>();
        for (int i = 0; i < count; i++)
        {
            var t = new GameObject("FindSpot_" + (i + 1)).transform;
            t.SetParent(table, false);
            t.position = new Vector3(x + (i - (count - 1) / 2f) * 0.55f * WorldScale, 0.93f, z);
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
        size *= WorldScale;
        var book = Item(parent, "Find_" + findId, def, size, false, false);
        book.transform.SetPositionAndRotation(hideSpot.position + Vector3.up * (size.y * 0.5f), Quaternion.Euler(0f, 180f, 0f));
        return book;
    }

    // ---------------------------------------------------------------- Room 1: the Reading Room

    // thickness, height, depth in metres: tomes, 4 in thick, 10 in deep, 12 to 15 in tall (the 15 in limit is what the bookcases are sized for)
    private static readonly Vector3[] ReadingScales =
    {
        new Vector3(BookSpine, BookMinHeight, BookDepth), new Vector3(BookSpine, 0.330f, BookDepth), new Vector3(BookSpine, 0.356f, BookDepth), new Vector3(BookSpine, BookMaxHeight, BookDepth)
    };

    /// <summary>Room 1: 96 books in 16 sets (even letters A, C, ... are sets of 8, odd letters are sets of 4) on two pieces of furniture along the
    /// far wall: a triple (a 4-wide wing, an 8-wide case, a 4-wide wing) and an 8-wide solo case, each with four shelves. A set belongs on a row of
    /// its own size. Rule: Group (the first set on a row claims it). Backlog T-017.</summary>
    public static string BuildRoom1()
    {
        RequireLamplitScene();
        SetupBookAssets();
        SetupCaseAssets();
        var s = Spec(1);
        var root = BuildShell(s);
        var zone = root.GetComponent<RoomZone>();
        var wood = LoadMat("Mat_Wood");

        // the furniture: triple 1.5 m before the middle of the wall, solo 1.9 m after it (the group is centred on the wall)
        var holder = FreshChild(root, "Containers");
        float zc = (s.Z0 + s.Z1) / 2f;
        float tripleZ = zc - 1.5f, soloZ = zc + 1.9f;
        float edge = (CaseWidth(8) + CaseWidth(4)) / 2f + 0.005f;
        var containers = new List<Bookcase>
        {
            Container(holder, "Triple_WingA", new Vector3(s.ContainerX, 0f, tripleZ - edge), s.ContainerYaw, ContainerRules.Group, PlacementRules.AnyFamily, zone, wood, 4, CaseLook.ColliderOnly),
            Container(holder, "Triple_Main", new Vector3(s.ContainerX, 0f, tripleZ), s.ContainerYaw, ContainerRules.Group, PlacementRules.AnyFamily, zone, wood, 8, CaseLook.ColliderOnly),
            Container(holder, "Triple_WingB", new Vector3(s.ContainerX, 0f, tripleZ + edge), s.ContainerYaw, ContainerRules.Group, PlacementRules.AnyFamily, zone, wood, 4, CaseLook.ColliderOnly),
            Container(holder, "Solo", new Vector3(s.ContainerX, 0f, soloZ), s.ContainerYaw, ContainerRules.Group, PlacementRules.AnyFamily, zone, wood, 8, CaseLook.Mesh, "Case_Solo"),
        };
        // one shared carved visual for the triple (the three cases above only carry the colliders, shelves and slots)
        var tripleVisual = new GameObject("Triple_Visual").transform;
        tripleVisual.SetParent(holder, false);
        tripleVisual.SetPositionAndRotation(new Vector3(s.ContainerX, 0f, tripleZ), Quaternion.Euler(0f, s.ContainerYaw, 0f));
        AddCaseVisual(tripleVisual, "Case_Triple", Vector3.zero);
        var allShelves = new List<Shelf>();
        foreach (var c in containers) allShelves.AddRange(c.shelves);
        zone.shelves = allShelves.ToArray();

        // rows by size, in shelf order
        var rows8 = new List<Shelf>();
        var rows4 = new List<Shelf>();
        foreach (var sh in zone.shelves) (sh.slots.Length == 8 ? rows8 : rows4).Add(sh);

        // 16 sets: a colour and a letter per set
        var itemsRoot = FreshChild(root, "Items");
        var items = new List<Book>();
        var homes = new List<ShelfSlot>();
        const int groups = 16;
        int next8 = 0, next4 = 0;
        for (int g = 0; g < groups; g++)
        {
            char letter = (char)('A' + g);
            Color color = GroupColor(g, groups);
            int setSize = g % 2 == 0 ? 8 : 4;
            var shelf = setSize == 8 ? rows8[next8++] : rows4[next4++];
            for (int n = 1; n <= setSize; n++)
            {
                var def = Definition("Item_R1_" + letter + n);
                def.setSize = setSize;
                def.iconIndex = g % PixelDigit.IconCount;
                def.coverStyle = g < CoverKeys.Length ? g : -1;   // sets A to F carry the printed test covers; the rest keep the pixel-label tome
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
                var book = Item(itemsRoot, "R1_" + letter + n, def, ReadingScales[g % ReadingScales.Length] * WorldScale, true);
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

        BakePiles(zone, 1001);
        RemoveOrphanItems();
        RegisterCampaign();
        MarkDirty();
        return "room 1 built: " + containers.Count + " cases, " + zone.shelves.Length + " shelves (" + rows8.Count + " of 8, " + rows4.Count + " of 4), " + items.Count + " items in " + groups + " sets, " + zone.finds.Length + " finds";
    }
}

public static partial class LamplitBuilder
{
    // ---------------------------------------------------------------- Room 2: the Mailroom

    private static readonly string[] MailFamilies = { "Book", "Magazine", "Letter", "Parcel" };
    private static readonly string[] MailPlural = { "BOOKS", "MAGAZINES", "LETTERS", "PARCELS" };
    private static readonly Vector3[] MailSizes =
    {
        new Vector3(0.065f, 0.26f, 0.17f),   // book
        new Vector3(0.058f, 0.30f, 0.22f),   // magazine: tall and wide, thin
        new Vector3(0.055f, 0.16f, 0.12f),   // letter: small
        new Vector3(0.085f, 0.14f, 0.14f),   // parcel: squat
    };

    /// <summary>Room 2: 120 items (books, magazines, letters, parcels; 6 groups of 5 each) on 24 shelves in 12 containers, three per family.
    /// Rule: Group, and the container's family must match the item's.</summary>
    public static string BuildRoom2()
    {
        RequireLamplitScene();
        var s = Spec(2);
        var root = BuildShell(s);
        var zone = root.GetComponent<RoomZone>();

        var woods = new[]
        {
            LoadMat("Mat_Wood"),
            EnsureMat("Mat_Wood_Magazine", new Color(0.28f, 0.40f, 0.44f), 0.2f),
            EnsureMat("Mat_Wood_Letter", new Color(0.55f, 0.45f, 0.30f), 0.2f),
            EnsureMat("Mat_Wood_Parcel", new Color(0.50f, 0.30f, 0.22f), 0.2f),
        };
        var containers = PlaceContainers(root, s, zone, 12, ContainerRules.Group, i => i / 3, i => woods[i / 3]);

        // a sign over each family's three containers
        float zc = (s.Z0 + s.Z1) / 2f;
        for (int f = 0; f < 4; f++)
        {
            float z = zc + ((f * 3 + 1) - 5.5f) * ContainerPitch;
            Sign(root, "FamilySign_" + MailFamilies[f], new Vector3(s.FarWallX - 0.12f * (s.Left ? -1f : 1f), SignHeight, z), s.Left ? Vector3.right : Vector3.left, new Vector2(2.5f, 0.6f), MailPlural[f], new Color(0.84f, 0.70f, 0.40f), 80);
        }

        var itemsRoot = FreshChild(root, "Items");
        var items = new List<Book>();
        var homes = new List<ShelfSlot>();
        for (int f = 0; f < 4; f++)
        {
            for (int gi = 0; gi < 6; gi++)
            {
                char letter = (char)('A' + gi);
                int group = f * 6 + gi;
                Color color = GroupColor(group, 24);
                var shelf = zone.shelves[f * 6 + gi];
                for (int n = 1; n <= 5; n++)
                {
                    var def = Definition("Item_R2_" + MailFamilies[f][0] + letter + n);
                    def.bookId = "r2_" + char.ToLowerInvariant(MailFamilies[f][0]) + char.ToLowerInvariant(letter) + n;
                    def.displayName = MailFamilies[f] + " " + letter;
                    def.family = f;
                    def.group = group;
                    def.number = n;
                    def.room = 2;
                    def.symbol = letter.ToString();
                    def.spineColor = color;
                    def.category = BookCategory.Alchemy;
                    def.series = 0;
                    def.volume = n;
                    EditorUtility.SetDirty(def);
                    items.Add(Item(itemsRoot, "R2_" + MailFamilies[f][0] + "_" + letter + n, def, MailSizes[f] * WorldScale, false));
                    homes.Add(shelf.slots[n - 1]);
                }
            }
        }

        zone.items = items.ToArray();
        zone.homeSlots = homes.ToArray();

        var findsRoot = FreshChild(root, "Finds");
        var hide = BuildFindTable(root, s, 2);
        var glasses = MakeFind(findsRoot, "glasses", new Color(0.75f, 0.85f, 0.95f), new Vector3(0.18f, 0.07f, 0.06f), hide[0]);
        var shelfList = MakeFind(findsRoot, "shelflist", new Color(0.95f, 0.88f, 0.50f), new Vector3(0.12f, 0.20f, 0.02f), hide[1]);
        zone.finds = new[] { glasses, shelfList };
        zone.findHideSpots = hide;

        BakePiles(zone, 1002);
        RemoveOrphanItems();
        RegisterCampaign();
        MarkDirty();
        return "room 2 built: " + containers.Count + " containers, " + zone.shelves.Length + " shelves, " + items.Count + " items, " + zone.finds.Length + " finds";
    }
}

public static partial class LamplitBuilder
{
    // ---------------------------------------------------------------- Room 3: the Map Room

    private static readonly string[] MapFamilies = { "Atlas", "Rolled map", "Flat sheet" };
    private static readonly string[] MapPlural = { "ATLASES", "ROLLED MAPS", "FLAT SHEETS" };
    private static readonly string[] MapInitial = { "A", "R", "S" };
    private static readonly Vector3[] MapSizes =
    {
        new Vector3(0.085f, 0.34f, 0.26f),   // atlas: big and thick
        new Vector3(0.070f, 0.30f, 0.070f),  // rolled map: a tube
        new Vector3(0.058f, 0.30f, 0.22f),   // flat sheet: wide and thin
    };

    /// <summary>Room 3: 120 items (atlases, rolled maps, flat sheets; 8 numbered sets of 5 each) on 24 shelves in 12 containers, four per family.
    /// Rule: Ordered (number N in slot N), and the container's family must match.</summary>
    public static string BuildRoom3()
    {
        RequireLamplitScene();
        var s = Spec(3);
        var root = BuildShell(s);
        var zone = root.GetComponent<RoomZone>();

        var woods = new[]
        {
            LoadMat("Mat_Wood"),
            EnsureMat("Mat_Wood_Tube", new Color(0.30f, 0.34f, 0.50f), 0.25f),
            EnsureMat("Mat_Wood_Drawer", new Color(0.45f, 0.48f, 0.50f), 0.35f, 0.3f),
        };
        var containers = PlaceContainers(root, s, zone, 12, ContainerRules.Ordered, i => i / 4, i => woods[i / 4]);

        float zc = (s.Z0 + s.Z1) / 2f;
        for (int f = 0; f < 3; f++)
        {
            float z = zc + ((f * 4 + 1.5f) - 5.5f) * ContainerPitch;
            Sign(root, "FamilySign_" + MapFamilies[f], new Vector3(s.FarWallX + 0.12f, SignHeight, z), Vector3.right, new Vector2(3.2f, 0.6f), MapPlural[f], new Color(0.84f, 0.70f, 0.40f), 80);
        }

        var itemsRoot = FreshChild(root, "Items");
        var items = new List<Book>();
        var homes = new List<ShelfSlot>();
        for (int f = 0; f < 3; f++)
        {
            for (int gi = 0; gi < 8; gi++)
            {
                char letter = (char)('A' + gi);
                int group = f * 8 + gi;
                Color color = GroupColor(group, 24);
                var shelf = zone.shelves[f * 8 + gi];
                for (int n = 1; n <= 5; n++)
                {
                    var def = Definition("Item_R3_" + MapInitial[f] + letter + n);
                    def.bookId = "r3_" + MapInitial[f].ToLowerInvariant() + char.ToLowerInvariant(letter) + n;
                    def.displayName = MapFamilies[f] + " " + letter + "-" + n;
                    def.family = f;
                    def.group = group;
                    def.number = n;
                    def.room = 3;
                    def.symbol = letter.ToString();
                    def.spineColor = color;
                    def.category = BookCategory.Alchemy;
                    def.series = 0;
                    def.volume = n;
                    EditorUtility.SetDirty(def);
                    items.Add(Item(itemsRoot, "R3_" + MapInitial[f] + "_" + letter + n, def, MapSizes[f] * WorldScale, true));
                    homes.Add(shelf.slots[n - 1]);
                }
            }
        }

        zone.items = items.ToArray();
        zone.homeSlots = homes.ToArray();

        var findsRoot = FreshChild(root, "Finds");
        var hide = BuildFindTable(root, s, 1);
        var pullList = MakeFind(findsRoot, "pulllist", new Color(0.95f, 0.55f, 0.40f), new Vector3(0.12f, 0.22f, 0.02f), hide[0]);
        zone.finds = new[] { pullList };
        zone.findHideSpots = hide;

        BakePiles(zone, 1003);
        RemoveOrphanItems();
        RegisterCampaign();
        MarkDirty();
        return "room 3 built: " + containers.Count + " containers, " + zone.shelves.Length + " shelves, " + items.Count + " items, " + zone.finds.Length + " finds";
    }
}
