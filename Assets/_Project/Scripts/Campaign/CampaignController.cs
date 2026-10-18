using System.Text;
using BookBoy.Rules;
using UnityEngine;

/// <summary>
/// The Lamplit campaign in a scene: three rooms and their doors, the desk spots where finds are installed, and the campaign state.
/// Checks the rooms a few times a second (a room whose items are all correct becomes tidy and opens the next door), installs finds
/// into their spots (switching abilities on through <see cref="GameProgress"/>), and offers the commands the admin menu uses.
/// </summary>
public class CampaignController : MonoBehaviour
{
    private const float CheckInterval = 0.25f;

    public RoomZone[] rooms;
    public RoomDoor[] doors;
    public FindSpot[] spots;

    [Tooltip("Where the player starts, at the desk.")]
    public Transform hubSpawn;

    public readonly CampaignState State = new CampaignState();

    private GameProgress _progress;
    private GameUI _ui;
    private PlayerInteractor _interactor;
    private float _nextCheck;
    private int[] _correct;
    private int[] _finishedShelves;
    private int[] _total;

    // Rooms beyond a shut door are switched off entirely (no renderers, colliders, lights or scripts) until the door opens. An admin
    // command that touches a room (jump, pile, complete...) switches it on and keeps it on.
    private bool[] _adminActive;

    public int RoomCount => rooms != null ? rooms.Length : 0;

    private void Start()
    {
        _progress = FindFirstObjectByType<GameProgress>();
        _ui = FindFirstObjectByType<GameUI>();
        _interactor = FindFirstObjectByType<PlayerInteractor>();
        State.Reset();
        _adminActive = new bool[RoomCount + 2];
        _correct = new int[RoomCount + 1];
        _finishedShelves = new int[RoomCount + 1];
        _total = new int[RoomCount + 1];
        for (int i = 0; i < RoomCount; i++)
        {
            _total[i + 1] = rooms[i].items != null ? rooms[i].items.Length : 0;
        }

        foreach (FindSpot spot in spots)
        {
            spot.installed = null;
        }

        ApplyDoors(true);
        Recompute();
    }

    private void Update()
    {
        if (Time.unscaledTime >= _nextCheck)
        {
            _nextCheck = Time.unscaledTime + CheckInterval;
            Recompute();
        }
    }

    /// <summary>Shows a short message on screen.</summary>
    public void Say(string message)
    {
        _ui?.Toast(message);
    }

    // ---------------------------------------------------------------- rooms

    public RoomZone Room(int number)
    {
        return number >= 1 && number <= RoomCount ? rooms[number - 1] : null;
    }

    public int RoomItems(int number) { return number >= 1 && number <= RoomCount ? _total[number] : 0; }
    public int RoomCorrect(int number) { return number >= 1 && number <= RoomCount ? _correct[number] : 0; }
    public int RoomShelvesFinished(int number) { return number >= 1 && number <= RoomCount ? _finishedShelves[number] : 0; }

    /// <summary>The room the player is standing in, or 0 for the hub.</summary>
    public int CurrentRoom
    {
        get
        {
            if (_interactor == null)
            {
                return 0;
            }

            Vector3 p = _interactor.transform.position;
            for (int i = 0; i < RoomCount; i++)
            {
                if (rooms[i].Contains(p))
                {
                    return i + 1;
                }
            }

            return 0;
        }
    }

    /// <summary>Recounts every room and finishes the ones whose items are all correct.</summary>
    public void Recompute()
    {
        if (rooms == null)
        {
            return;
        }

        for (int i = 0; i < RoomCount; i++)
        {
            int n = i + 1;
            _total[n] = rooms[i].items != null ? rooms[i].items.Length : 0;
            _correct[n] = rooms[i].CountCorrect();
            _finishedShelves[n] = rooms[i].CountFinishedShelves();
            if (!State.IsRoomFinished(n) && PlacementRules.IsCleanupComplete(_total[n], _correct[n]) && State.FinishRoom(n))
            {
                OnRoomFinished(n);
            }
        }
    }

    private void OnRoomFinished(int room)
    {
        ApplyDoors(false);
        string name = CampaignCatalog.RoomName(room);
        string next = room < CampaignCatalog.RoomCount ? " The " + CampaignCatalog.RoomName(room + 1) + " door is open." : " The whole library is tidy!";
        _ui?.Toast("Room tidied: " + name + "." + next, 5f);
        _progress?.RaiseChanged();
    }

    private void ApplyDoors(bool instant)
    {
        if (doors == null)
        {
            return;
        }

        foreach (RoomDoor door in doors)
        {
            bool open = State.IsDoorOpen(door.roomNumber);
            door.SetOpen(open, instant);
            string name = CampaignCatalog.RoomName(door.roomNumber);
            door.SetLabel(open ? name.ToUpperInvariant() : name.ToUpperInvariant() + "\nLOCKED");
        }

        ApplyRoomActivity();
    }

    /// <summary>Switches each room on if its door is open (or an admin command needs it), off otherwise.</summary>
    private void ApplyRoomActivity()
    {
        if (rooms == null || _adminActive == null)
        {
            return;
        }

        for (int i = 0; i < RoomCount; i++)
        {
            int n = i + 1;
            bool on = State.IsDoorOpen(n) || _adminActive[n];
            if (rooms[i].gameObject.activeSelf != on)
            {
                rooms[i].SetLive(on);
            }
        }
    }

    /// <summary>Admin / tests: switch a room on and keep it on even though its door is shut.</summary>
    public void ActivateRoom(int room)
    {
        if (room >= 1 && room <= RoomCount && _adminActive != null)
        {
            _adminActive[room] = true;
            ApplyRoomActivity();
        }
    }

    public void ActivateAllRooms()
    {
        for (int n = 1; n <= RoomCount; n++)
        {
            ActivateRoom(n);
        }
    }

    public bool IsRoomActive(int room)
    {
        return room >= 1 && room <= RoomCount && rooms[room - 1].gameObject.activeSelf;
    }

    // ---------------------------------------------------------------- finds and the desk

    public FindSpot SpotFor(string spotId)
    {
        foreach (FindSpot s in spots)
        {
            if (s.spotId == spotId)
            {
                return s;
            }
        }

        return null;
    }

    /// <summary>E at a desk spot with a find in hand: installs it if it is the right one. Returns the result (with a toast).</summary>
    public InstallResult InstallHeld(FindSpot spot, CarryInventory carry)
    {
        Book book = carry != null ? carry.Current : null;
        if (spot == null)
        {
            return InstallResult.UnknownFind;
        }

        if (spot.locked || spot.spotId == CampaignCatalog.DumbwaiterSpot)
        {
            _ui?.Toast("The " + spot.label + " is locked for now.");
            return InstallResult.LockedSpot;
        }

        if (book == null || !book.IsFind)
        {
            _ui?.Toast("The " + spot.label + " needs " + NeededFor(spot) + ". Look for it in the rooms.");
            return InstallResult.UnknownFind;
        }

        InstallResult result = State.TryInstall(book.definition.findId, spot.spotId);
        FindDef find = CampaignCatalog.FindById(book.definition.findId);
        switch (result)
        {
            case InstallResult.Installed:
                carry.Release(book, out _);
                Snap(book, spot);
                ApplyInstalled();
                _ui?.Toast(find.Name + " installed: " + find.Effect + ".", 4.5f);
                break;
            case InstallResult.WrongSpot:
                _ui?.Toast("The " + spot.label + " takes " + NeededFor(spot) + ", not the " + find.Name.ToLowerInvariant() + ".");
                break;
            case InstallResult.AlreadyInstalled:
                _ui?.Toast("The " + find.Name.ToLowerInvariant() + " is already installed.");
                break;
        }

        return result;
    }

    private static string NeededFor(FindSpot spot)
    {
        FindDef f = CampaignCatalog.FindForSpot(spot.spotId);
        return f != null ? "the " + f.Name.ToLowerInvariant() : "something else";
    }

    private void Snap(Book book, FindSpot spot)
    {
        Transform t = book.transform;
        Vector3 realSize = t.lossyScale;
        Transform parent = spot.anchor != null ? spot.anchor : spot.transform;
        t.SetParent(parent, false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        Vector3 ps = parent.lossyScale;   // the desk spots are built at world scale: keep the find its real size, not scaled again
        t.localScale = new Vector3(realSize.x / ps.x, realSize.y / ps.y, realSize.z / ps.z);
        Collider c = book.GetComponent<Collider>();
        if (c != null)
        {
            c.enabled = false;
        }

        book.Outline?.StopAll();
        spot.installed = book;
    }

    /// <summary>Applies every installed find's upgrades (carry, speed, abilities) to the player.</summary>
    public void ApplyInstalled()
    {
        if (_progress == null)
        {
            return;
        }

        State.ApplyTo(_progress.Upgrades);
        _progress.ApplyUpgrades();
        _progress.RaiseChanged();
    }

    /// <summary>Called when the player picks something up: a find is remembered as found.</summary>
    public void NotifyPickedUp(Book book)
    {
        if (book != null && book.IsFind && State.MarkFound(book.definition.findId))
        {
            _progress?.RaiseChanged();
        }
    }

    public Book FindBook(string findId)
    {
        foreach (RoomZone room in rooms)
        {
            if (room.finds == null)
            {
                continue;
            }

            foreach (Book b in room.finds)
            {
                if (b != null && b.definition != null && b.definition.findId == findId)
                {
                    return b;
                }
            }
        }

        return null;
    }

    // ---------------------------------------------------------------- HUD text

    /// <summary>The stats block for the HUD: the room you are in, its items and shelves, the finds installed.</summary>
    public string StatsText()
    {
        int room = CurrentRoom;
        var sb = new StringBuilder();
        if (room == 0)
        {
            int tidy = 0;
            for (int n = 1; n <= RoomCount; n++)
            {
                if (State.IsRoomFinished(n))
                {
                    tidy++;
                }
            }

            sb.Append("<b>THE LAMP</b>  (hub)\n<b>ROOMS TIDY</b>  ").Append(tidy).Append(" / ").Append(RoomCount);
        }
        else
        {
            sb.Append("<b>").Append(CampaignCatalog.RoomName(room).ToUpperInvariant()).Append("</b>")
              .Append(State.IsRoomFinished(room) ? "  (tidy!)" : "")
              .Append("\n<b>ITEMS</b>  ").Append(_correct[room]).Append(" / ").Append(_total[room])
              .Append("\n<b>SHELVES</b>  ").Append(_finishedShelves[room]).Append(" / ").Append(rooms[room - 1].shelves.Length);
        }

        sb.Append("\n<b>FINDS</b>  ").Append(State.InstalledCount).Append(" / ").Append(CampaignCatalog.Finds.Count).Append(" installed");
        return sb.ToString();
    }

    // ---------------------------------------------------------------- admin commands

    private void ReleaseCarriedItems(bool includeFinds)
    {
        if (_interactor == null)
        {
            return;
        }

        foreach (Book b in new System.Collections.Generic.List<Book>(_interactor.Carry.Items))
        {
            if (includeFinds || !b.IsFind)
            {
                _interactor.Carry.Release(b, out _);
            }
        }
    }

    /// <summary>Piles a room's items up again: picks every item off the shelves and floor and tosses them into several random piles.</summary>
    public void PileRoom(int room)
    {
        RoomZone zone = Room(room);
        if (zone == null)
        {
            return;
        }

        ActivateRoom(room);
        ReleaseCarriedItems(false);
        zone.PileUp(1000 + room + Mathf.FloorToInt(Time.realtimeSinceStartup));
        Recompute();
    }

    public void ResetRoom(int room)
    {
        RoomZone zone = Room(room);
        if (zone == null)
        {
            return;
        }

        State.ResetRoom(room);
        PileRoom(room);
        ApplyDoors(false);
    }

    public void CompleteRoom(int room)
    {
        RoomZone zone = Room(room);
        if (zone == null)
        {
            return;
        }

        ActivateRoom(room);
        ReleaseCarriedItems(false);
        zone.PlaceAllHome();
        Recompute();
    }

    private void HideFind(Book book, Transform spot)
    {
        if (book == null)
        {
            return;
        }

        if (_interactor != null && _interactor.Carry.Contains(book))
        {
            _interactor.Carry.Release(book, out _);
        }

        book.StopPhysicsTumble();
        book.transform.SetParent(null, true);
        if (spot != null)
        {
            book.transform.SetPositionAndRotation(spot.position, spot.rotation);
        }

        Collider c = book.GetComponent<Collider>();
        if (c != null)
        {
            c.enabled = true;
            c.isTrigger = true;
        }
    }

    public void ResetAll()
    {
        ReleaseCarriedItems(true);
        for (int n = 1; n <= RoomCount; n++)
        {
            _adminActive[n] = rooms[n - 1].gameObject.activeSelf;   // a room that is already on stays on (its piles are still dropping)
        }

        State.Reset();
        foreach (FindSpot s in spots)
        {
            s.installed = null;
        }

        foreach (RoomZone room in rooms)
        {
            if (room.finds != null)
            {
                for (int i = 0; i < room.finds.Length; i++)
                {
                    HideFind(room.finds[i], room.findHideSpots != null && i < room.findHideSpots.Length ? room.findHideSpots[i] : null);
                }
            }

            if (room.gameObject.activeSelf)
            {
                room.PileUp(1000 + room.roomNumber);   // rooms that were never opened are untouched and stay switched off
            }
        }

        _progress?.ResetUpgrades();
        ApplyDoors(true);
        Recompute();
        _progress?.RaiseChanged();
    }

    /// <summary>Admin: put the find in the player's hands (or at their feet if the hands are full).</summary>
    public bool GiveFind(string findId)
    {
        FindDef wanted = CampaignCatalog.FindById(findId);
        if (wanted != null)
        {
            ActivateRoom(wanted.Room);   // a find in a switched-off room would stay switched off in the player's hands
        }

        Book book = FindBook(findId);
        if (book == null || State.IsInstalled(findId) || _interactor == null)
        {
            return false;
        }

        if (_interactor.Carry.Contains(book))
        {
            return true;
        }

        book.StopPhysicsTumble();
        Collider c = book.GetComponent<Collider>();
        if (c != null)
        {
            c.enabled = true;
        }

        if (!_interactor.PickUp(book))
        {
            Vector3 p = _interactor.transform.position + _interactor.transform.forward * 1f;
            book.transform.SetParent(null, true);
            book.transform.position = new Vector3(p.x, book.transform.localScale.y * 0.5f, p.z);
        }

        State.MarkFound(findId);
        _progress?.RaiseChanged();
        return true;
    }

    /// <summary>Admin: install a find straight into its desk spot.</summary>
    public bool InstallFind(string findId)
    {
        FindDef def = CampaignCatalog.FindById(findId);
        if (def != null)
        {
            ActivateRoom(def.Room);
        }

        Book book = FindBook(findId);
        if (book == null || def == null || State.IsInstalled(findId))
        {
            return false;
        }

        FindSpot spot = SpotFor(def.SpotId);
        if (spot == null)
        {
            return false;
        }

        if (_interactor != null && _interactor.Carry.Contains(book))
        {
            _interactor.Carry.Release(book, out _);
        }

        book.StopPhysicsTumble();
        State.ForceInstall(findId);
        Snap(book, spot);
        ApplyInstalled();
        return true;
    }

    /// <summary>Admin: switch Sister Glow, Home Beacon and Gather Family on (the dumbwaiter stays locked).</summary>
    public void UnlockAbilities()
    {
        if (_progress == null)
        {
            return;
        }

        foreach (string id in new[] { "glow", "beacon", "gather" })
        {
            _progress.Upgrades.Grant(id);
        }

        _progress.ApplyUpgrades();
        _progress.RaiseChanged();
    }

    public void SetDoor(int room, bool open)
    {
        State.ForceDoor(room, open);
        ApplyDoors(false);
    }

    public void ClearDoorOverrides()
    {
        State.ClearForcedDoors();
        ApplyDoors(false);
    }

    /// <summary>Admin: teleport the player to the hub (0) or the entry of a room.</summary>
    public void JumpTo(int room)
    {
        if (room > 0)
        {
            ActivateRoom(room);
        }

        Transform target = room == 0 ? hubSpawn : (Room(room) != null ? Room(room).entryPoint : null);
        if (target == null || _interactor == null)
        {
            return;
        }

        var cc = _interactor.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        _interactor.transform.SetPositionAndRotation(target.position, target.rotation);
        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    /// <summary>A one-line summary for the admin menu.</summary>
    public string Counts()
    {
        var sb = new StringBuilder();
        for (int n = 1; n <= RoomCount; n++)
        {
            sb.Append("R").Append(n).Append(' ').Append(_correct[n]).Append('/').Append(_total[n])
              .Append(State.IsRoomFinished(n) ? " tidy" : "").Append(State.IsDoorOpen(n) ? "" : " (locked)").Append("   ");
        }

        sb.Append("finds ").Append(State.FoundCount).Append(" found, ").Append(State.InstalledCount).Append(" installed");
        return sb.ToString();
    }
}
