using System.Collections.Generic;
using BookBoy.Rules;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class LamplitBuilder
{
    // Hub: a hallway about 4.8 m wide and 24.8 m long (x -HubHalf..HubHalf, z 0..HubLength). The desk is at the z = 0 end. Doors: Room 1 and
    // Room 3 on the left wall, Room 2 on the right wall, each at the middle of its room.
    public static readonly float[] DoorZ = { 5.7f, 12.4f, 18.1f };

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
            var text = Sign(root.transform, "DoorSign", new Vector3(signX, (DoorHeight + WallHeight) / 2f, DoorZ[i]), left ? Vector3.right : Vector3.left, new Vector2(1.8f, 0.7f),
                CampaignCatalog.RoomName(room).ToUpperInvariant(), new Color(0.84f, 0.70f, 0.40f), 80);
            door.label = text;
            doors.Add(door);
        }

        // lights: warm orbs down the hallway
        Orb(hub, "MageLight_Hub_1", new Vector3(0f, 3.5f, HubLength * 0.17f), 4.6f, false, 10f);
        Orb(hub, "MageLight_Hub_2", new Vector3(0f, 3.5f, HubLength * 0.5f), 4.4f, false, 10f);
        Orb(hub, "MageLight_Hub_3", new Vector3(0f, 3.5f, HubLength * 0.83f), 4.4f, false, 10f);

        // player start: at the desk, looking at it
        var spawn = new GameObject("HubSpawn").transform;
        spawn.SetParent(hub, false);
        spawn.SetPositionAndRotation(new Vector3(0f, 1f, 3.6f), Quaternion.Euler(0f, 180f, 0f));
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
        Sign(desk, "DeskSign", new Vector3(0f, 3.1f, 0.14f), Vector3.forward, new Vector2(2.8f, 0.8f), "LAMPLIT\nthe librarian's desk", new Color(0.84f, 0.70f, 0.40f), 80);

        // six spots in a row on the desk top; each accepts one find. The last is locked for now.
        var spotRoot = FreshChild(desk, "Spots");
        string[] ids = { "apron_hook", "shoe_rack", "glasses_stand", "shelf_list_board", "clipboard", CampaignCatalog.DumbwaiterSpot };
        string[] labels = { "Apron hook", "Shoe rack", "Glasses stand", "Shelf-list board", "Clipboard", "Dumbwaiter hatch" };
        string[] signs = { "APRON\nHOOK", "SHOE\nRACK", "GLASSES\nSTAND", "SHELF-LIST\nBOARD", "CLIP-\nBOARD", "DUMB-\nWAITER" };
        float[] xs = { 1.75f, 1.05f, 0.35f, -0.35f, -1.05f, -1.75f };   // the player looks toward -Z, so +X is on the left
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
                    Cube("Hook", go.transform, new Vector3(0f, 0.52f, 0.06f), new Vector3(0.03f, 0.03f, 0.14f), brass, false);
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

            Sign(spotRoot, "Label_" + ids[i], new Vector3(xs[i], 2.15f, 0.85f), Vector3.forward, new Vector2(0.62f, 0.36f), signs[i], new Color(0.84f, 0.70f, 0.40f), 60);
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
    public const float FieldOfView = 75f;
    public const float EyeHeight = 1.15f;
    public const float MoveSpeed = 3.2f;
    public const float SprintSpeed = 4.8f;
    public const float InteractionRange = 2.8f;
    public const float MaxAimRange = 12f;
    public const float CarryScale = 0.55f;   // items are 1.6x life size now, so held ones are drawn smaller than the 0.9 used with life-size books

    /// <summary>Applies the camera field of view, eye height, walking speed, reach and held-book size to the player rig of the open scene.</summary>
    public static string ApplyPlayerScale()
    {
        RequireLamplitScene();
        var player = GameObject.Find("Player");
        var vcam = GameObject.Find("PlayerFollowCamera").GetComponent<Unity.Cinemachine.CinemachineCamera>();
        var lens = vcam.Lens;
        lens.FieldOfView = FieldOfView;
        vcam.Lens = lens;
        EditorUtility.SetDirty(vcam);

        var root = player.transform.Find("PlayerCameraRoot");
        root.localPosition = new Vector3(0f, EyeHeight - 0.09f, 0f);   // the capsule settles 0.09 m above the floor
        EditorUtility.SetDirty(root);

        var fpc = player.GetComponent<StarterAssets.FirstPersonController>();
        fpc.MoveSpeed = MoveSpeed;
        fpc.SprintSpeed = SprintSpeed;
        EditorUtility.SetDirty(fpc);

        var interactor = player.GetComponent<PlayerInteractor>();
        interactor.interactionRange = InteractionRange;
        interactor.maxAimRange = MaxAimRange;
        EditorUtility.SetDirty(interactor);

        var carry = player.GetComponent<CarryInventory>();
        carry.carryScale = CarryScale;
        EditorUtility.SetDirty(carry);

        MarkDirty();
        return "fov " + FieldOfView + ", eye " + EyeHeight + " m, speed " + MoveSpeed + "/" + SprintSpeed + ", reach " + InteractionRange + " m, held books x" + CarryScale;
    }
}
