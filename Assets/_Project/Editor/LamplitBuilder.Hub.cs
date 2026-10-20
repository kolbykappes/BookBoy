using System.Collections.Generic;
using BookBoy.Rules;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class LamplitBuilder
{
    // Hub: a hallway 3 m (10 ft) wide and 15.5 m long (x -HubHalf..HubHalf, z 0..HubLength). The desk is at the z = 0 end. Doors: Room 1 and
    // Room 3 on the left wall, Room 2 on the right wall, each at the middle of its room.
    public static readonly float[] DoorZ = { 5.7f * LayoutK, 12.4f * LayoutK, 18.1f * LayoutK };

    /// <summary>A copy of the floor tile material whose tiling fits a floor of w x d metres (0.8 m tiles).</summary>
    public static Material FloorMat(string name, float w, float d)
    {
        string path = MatDir + "/Mat_Floor_" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(LoadMat("Mat_FloorTile"));
            AssetDatabase.CreateAsset(m, path);
        }

        m.shader = LoadMat("Mat_FloorTile").shader;
        m.CopyPropertiesFromMaterial(LoadMat("Mat_FloorTile"));
        m.mainTextureScale = new Vector2(w / 1.6f, d / 1.6f);
        EditorUtility.SetDirty(m);
        return m;
    }

    public static string BuildHub()
    {
        RequireLamplitScene();
        var hub = FreshRoot("Hub");
        var wallMat = EnsureMat("Mat_Wall_Hub", new Color(0.50f, 0.40f, 0.30f), 0.1f);
        var wood = LoadMat("Mat_Wood");
        var brass = EnsureMat("Mat_Brass", new Color(0.75f, 0.58f, 0.25f), 0.45f, 0.6f);
        var doorMat = EnsureMat("Mat_Door", new Color(0.30f, 0.18f, 0.09f), 0.2f);

        float edge = HubHalf + 0.1f;
        Floor(hub, "Floor", -edge, edge, 0f, HubLength, FloorMat("Hub", edge * 2f, HubLength));
        Ceiling(hub, "Ceiling", -edge, edge, 0f, HubLength);
        Wall(hub, "WallLeft", true, -HubHalf, 0f, HubLength, new[] { DoorZ[0], DoorZ[2] }, wallMat);
        Wall(hub, "WallRight", true, HubHalf, 0f, HubLength, new[] { DoorZ[1] }, wallMat);
        Wall(hub, "WallEnd0", false, 0f, -edge, edge, null, wallMat);
        Wall(hub, "WallEndFar", false, HubLength, -edge, edge, null, wallMat);

        BuildDesk(hub, wood, brass);

        // doors, each with a sign on the hub side
        var doors = new List<RoomDoor>();
        for (int i = 0; i < 3; i++)
        {
            int room = i + 1;
            bool left = room != 2;
            float x = left ? -HubHalf : HubHalf;
            var root = new GameObject("Door_" + room);
            root.transform.SetParent(hub, false);
            var door = root.AddComponent<RoomDoor>();
            door.roomNumber = room;
            var leaf = Cube("Leaf", root.transform, new Vector3(x, DoorHeight / 2f, DoorZ[i]), new Vector3(0.12f, DoorHeight, DoorWidth), doorMat);
            door.leaf = leaf.transform;
            door.blocker = leaf.GetComponent<Collider>();
            door.openHeight = DoorHeight + 0.1f;
            float signX = left ? -(HubHalf - 0.12f) : HubHalf - 0.12f;
            var text = Sign(root.transform, "DoorSign", new Vector3(signX, (DoorHeight + WallHeight) / 2f, DoorZ[i]), left ? Vector3.right : Vector3.left, new Vector2(1.2f, 0.42f),
                CampaignCatalog.RoomName(room).ToUpperInvariant(), new Color(0.84f, 0.70f, 0.40f), 80);
            door.label = text;
            doors.Add(door);
        }

        // lights: warm orbs down the hallway
        Orb(hub, "MageLight_Hub_1", new Vector3(0f, OrbHeight, HubLength * 0.17f), OrbIntensity, false, OrbRange);
        Orb(hub, "MageLight_Hub_2", new Vector3(0f, OrbHeight, HubLength * 0.5f), OrbIntensity, false, OrbRange);
        Orb(hub, "MageLight_Hub_3", new Vector3(0f, OrbHeight, HubLength * 0.83f), OrbIntensity, false, OrbRange);

        // player start: at the desk, looking at it
        var spawn = new GameObject("HubSpawn").transform;
        spawn.SetParent(hub, false);
        spawn.SetPositionAndRotation(new Vector3(0f, 1f, 3.6f * LayoutK), Quaternion.Euler(0f, 180f, 0f));
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            if (cc != null) cc.enabled = true;
        }

        RegisterCampaign();
        ApplyPlayerScale();
        MarkDirty();
        return "hub built: desk with 6 spots, 3 doors, 3 orbs";
    }

    private static void BuildDesk(Transform hub, Material wood, Material brass)
    {
        var desk = FreshChild(hub, "Desk");
        float deskWidth = HubHalf * 2f - 0.1f;
        Cube("Top", desk, new Vector3(0f, 0.875f, 0.6f), new Vector3(deskWidth, 0.05f, 1.0f), wood);
        Cube("Body", desk, new Vector3(0f, 0.425f, 0.6f), new Vector3(deskWidth - 0.1f, 0.85f, 0.9f), wood);
        Sign(desk, "DeskSign", new Vector3(0f, 2.4f, 0.14f), Vector3.forward, new Vector2(2.2f, 0.5f), "LAMPLIT\nthe librarian's desk", new Color(0.84f, 0.70f, 0.40f), 80);

        // six spots in a row on the desk top; each accepts one find. The last is locked for now.
        var spotRoot = FreshChild(desk, "Spots");
        string[] ids = { "apron_hook", "shoe_rack", "glasses_stand", "shelf_list_board", "clipboard", CampaignCatalog.DumbwaiterSpot };
        string[] labels = { "Apron hook", "Shoe rack", "Glasses stand", "Shelf-list board", "Clipboard", "Dumbwaiter hatch" };
        string[] signs = { "APRON\nHOOK", "SHOE\nRACK", "GLASSES\nSTAND", "SHELF-LIST\nBOARD", "CLIP-\nBOARD", "DUMB-\nWAITER" };
        float[] xs = { 1.2f, 0.72f, 0.24f, -0.24f, -0.72f, -1.2f };   // the player looks toward -Z, so +X is on the left
        const float top = 0.9f;
        for (int i = 0; i < ids.Length; i++)
        {
            var go = new GameObject("Spot_" + ids[i]);
            go.transform.SetParent(spotRoot, false);
            go.transform.position = new Vector3(xs[i], top, 0.75f);
            go.transform.localScale = Vector3.one * WorldScale;   // desk props at world scale (so are the finds that hang on them)
            var spot = go.AddComponent<FindSpot>();
            spot.spotId = ids[i];
            spot.label = labels[i];
            spot.locked = ids[i] == CampaignCatalog.DumbwaiterSpot;
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 0.25f, 0f);
            box.size = new Vector3(0.38f, 0.5f, 0.42f);

            Cube("Base", go.transform, new Vector3(0f, 0.03f, 0f), new Vector3(0.34f, 0.06f, 0.34f), brass, false);
            var anchor = new GameObject("Anchor").transform;
            anchor.SetParent(go.transform, false);
            anchor.localPosition = new Vector3(0f, 0.22f, 0f);
            spot.anchor = anchor;

            switch (ids[i])
            {
                case "apron_hook":
                    Cube("Post", go.transform, new Vector3(0f, 0.3f, 0.12f), new Vector3(0.03f, 0.5f, 0.03f), brass, false);
                    Cube("Hook", go.transform, new Vector3(0f, 0.52f, 0.11f), new Vector3(0.03f, 0.03f, 0.22f), brass, false);   // a peg long enough for the apron's neck loop to hang from
                    break;
                case "shoe_rack":
                    Cube("Rack", go.transform, new Vector3(0f, 0.1f, 0f), new Vector3(0.3f, 0.04f, 0.3f), brass, false);
                    break;
                case "glasses_stand":
                    Cube("Stand", go.transform, new Vector3(0f, 0.12f, 0.08f), new Vector3(0.03f, 0.2f, 0.03f), brass, false);
                    break;
                case "shelf_list_board":
                    Cube("Board", go.transform, new Vector3(0f, 0.3f, 0.14f), new Vector3(0.3f, 0.4f, 0.02f), wood, false);
                    break;
                case "clipboard":
                    Cube("Board", go.transform, new Vector3(0f, 0.28f, 0.14f), new Vector3(0.24f, 0.36f, 0.02f), wood, false);
                    Cube("Clip", go.transform, new Vector3(0f, 0.46f, 0.125f), new Vector3(0.1f, 0.04f, 0.02f), brass, false);
                    break;
                default:
                    Cube("Hatch", go.transform, new Vector3(0f, 0.22f, 0.1f), new Vector3(0.32f, 0.36f, 0.1f), wood, false);
                    Cube("Padlock", go.transform, new Vector3(0f, 0.22f, 0.04f), new Vector3(0.06f, 0.08f, 0.03f), brass, false);
                    break;
            }

            // where the installed find sits (the apron hangs from the peg, the shoes stand on the rack; see LamplitBuilder.Common.FindModelTable)
            if (ids[i] == "apron_hook") anchor.localPosition = new Vector3(0f, 0.28f, 0.19f);
            else if (ids[i] == "shoe_rack") anchor.localPosition = new Vector3(0f, 0.17f, 0f);

            Sign(spotRoot, "Label_" + ids[i], new Vector3(xs[i], 1.75f, 0.85f), Vector3.forward, new Vector2(0.44f, 0.28f), signs[i], new Color(0.84f, 0.70f, 0.40f), 60);
        }
    }

    /// <summary>Finds the doors, rooms and desk spots in the scene and (re)creates the CampaignController that ties them together.</summary>
    public static CampaignController RegisterCampaign()
    {
        var doors = new List<RoomDoor>(Object.FindObjectsByType<RoomDoor>(FindObjectsInactive.Include));
        doors.Sort((a, b) => a.roomNumber.CompareTo(b.roomNumber));
        var rooms = new List<RoomZone>(Object.FindObjectsByType<RoomZone>(FindObjectsInactive.Include));
        rooms.Sort((a, b) => a.roomNumber.CompareTo(b.roomNumber));
        var spots = new List<FindSpot>(Object.FindObjectsByType<FindSpot>(FindObjectsInactive.Include));
        spots.Sort((a, b) => string.CompareOrdinal(a.spotId, b.spotId));

        var go = FreshRoot("Campaign");
        var controller = go.gameObject.AddComponent<CampaignController>();
        controller.rooms = rooms.ToArray();
        controller.doors = doors.ToArray();
        controller.spots = spots.ToArray();
        var spawn = GameObject.Find("HubSpawn");
        controller.hubSpawn = spawn != null ? spawn.transform : null;
        EditorUtility.SetDirty(controller);
        return controller;
    }
}

public static partial class LamplitBuilder
{
    // The "storybook" view: a wider lens and a smaller player, so books, shelves and rooms feel big and close (see Docs/09-scale-reference.md).
    // Eye height 1.7 m -> 1.15 m is a player about two thirds the size; speed and reach shrink with it.
    public const float FieldOfView = 58.7f;      // vertical: 90 degrees horizontal at 16:9 (backlog T-002, research files 2, 23, 27)
    public const float EyeHeight = 1.65f;       // 5'5"
    public const float MoveSpeed = 4.0f;
    public const float SprintSpeed = 6.0f;
    public const float InteractionRange = 3.0f;
    public const float MaxAimRange = 12f;
    public const float CapsuleHeight = 1.8f;    // 5'11"
    public const float CapsuleRadius = 0.3f;    // 1'0"
    public const float HandLightIntensity = 0.6f;
    public const float HandLightRange = 2.8f;
    public static readonly Vector3 HandLightPosition = new Vector3(0f, 0.25f, -0.25f);
    /// <summary>Where the middle of the top held book sits relative to the eye, in the view's space (right, up, forward). CarryInventory keeps the stack there, following the view.</summary>
    public static readonly Vector3 StackEyeOffset = new Vector3(0.40f, -0.27f, 0.52f);
    /// <summary>The stack anchor's starting place in the player root's space; CarryInventory moves it every frame to follow the view.</summary>
    public static readonly Vector3 StackOffset = new Vector3(0.40f, 1.38f, 0.52f);
    public const float CarryScale = 1.0f;   // held tomes at full size (Librarian's carried book fills 30 to 35 percent of the screen width)   // items are 1.6x life size now, so held ones are drawn smaller than the 0.9 used with life-size books

    /// <summary>Applies the camera field of view, eye height, walking speed, reach and held-book size to the player rig of the open scene.</summary>
    public static string ApplyPlayerScale()
    {
        RequireLamplitScene();
        var player = GameObject.Find("Player");
        var vcam = GameObject.Find("PlayerFollowCamera").GetComponent<Unity.Cinemachine.CinemachineCamera>();
        var lens = vcam.Lens;
        lens.FieldOfView = FieldOfView;
        // The held stack sits about half a metre in front of the eye and shrinks toward it near shelves, so the near plane (0.2 m from the Starter Assets prefab) would
        // cut it. 0.1 m is the first step docs/unity-practices/04 recommends; the scene's Cinemachine camera takes it, not the prefab or any project setting.
        lens.NearClipPlane = 0.1f;
        vcam.Lens = lens;
        EditorUtility.SetDirty(vcam);

        var root = player.transform.Find("PlayerCameraRoot");
        root.localPosition = new Vector3(0f, EyeHeight - 0.09f, 0f);   // the capsule settles 0.09 m above the floor
        EditorUtility.SetDirty(root);

        var cc = player.GetComponent<CharacterController>();
        cc.height = CapsuleHeight;
        cc.radius = CapsuleRadius;
        cc.center = new Vector3(0f, CapsuleHeight / 2f - 0.07f, 0f);   // the same 0.07 m below the player origin as the Starter Assets capsule, so the 0.09 m settle offset still holds
        EditorUtility.SetDirty(cc);

        var fpc = player.GetComponent<StarterAssets.FirstPersonController>();
        fpc.MoveSpeed = MoveSpeed;
        fpc.SprintSpeed = SprintSpeed;
        EditorUtility.SetDirty(fpc);

        var interactor = player.GetComponent<PlayerInteractor>();
        interactor.interactionRange = InteractionRange;
        interactor.maxAimRange = MaxAimRange;
        EditorUtility.SetDirty(interactor);

        // a soft light on the camera so carried books stay readable; weak and set back from the eyes, because a bright one near the
        // lens blows out whatever the player stands close to (shelves are half a metre away at this scale)
        var camRoot = Camera.main;   // the Cinemachine brain camera, not a child of the player
        var hand = camRoot != null ? camRoot.transform.Find("HandLight") : null;
        if (hand != null)
        {
            hand.localPosition = HandLightPosition;
            var hl = hand.GetComponent<Light>();
            hl.intensity = HandLightIntensity;
            hl.range = HandLightRange;
            EditorUtility.SetDirty(hl);
        }

        EnsureSlotGhost(player);

        var carry = player.GetComponent<CarryInventory>();
        carry.carryScale = CarryScale;
        // The held books are a Librarian-style stack of flat books at the lower right. Its anchor is a child of the player root (it turns with the player but not
        // with the camera pitch), so the stack sits just out of the bottom of the view at a level gaze and rises into it when the player looks down.
        var stack = player.transform.Find("CarryStack");
        if (stack == null)
        {
            stack = new GameObject("CarryStack").transform;
            stack.SetParent(player.transform, false);
        }

        stack.localPosition = StackOffset;
        stack.localRotation = Quaternion.identity;
        stack.localScale = Vector3.one;
        carry.carryPoint = stack;
        carry.eyeOffset = StackEyeOffset;   // serialized values do not follow a changed default, so the builder sets them
        carry.pitchFollow = 0.7f;
        carry.pitchFollowUp = 0.95f;
        carry.minStackScale = 0.4f;
        EditorUtility.SetDirty(carry);

        MarkDirty();
        return "fov " + FieldOfView + ", eye " + EyeHeight + " m, speed " + MoveSpeed + "/" + SprintSpeed + ", reach " + InteractionRange + " m, held books x" + CarryScale;
    }
}
