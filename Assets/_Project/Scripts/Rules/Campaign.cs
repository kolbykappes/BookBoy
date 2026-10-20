using System;
using System.Collections.Generic;

namespace BookBoy.Rules
{
    /// <summary>A find hidden in a room that is carried to the desk and installed in its own spot, switching an ability or boost on.</summary>
    public sealed class FindDef
    {
        public FindDef(string id, string name, string spotId, int room, string[] grants, string effect, string title = "", string story = "", string hint = "")
        {
            Title = string.IsNullOrEmpty(title) ? name : title;
            Story = story ?? "";
            Hint = hint ?? "";
            Id = id;
            Name = name;
            SpotId = spotId;
            Room = room;
            Grants = grants;
            Effect = effect;
        }

        public string Id { get; }
        public string Name { get; }

        /// <summary>The desk spot that accepts this find (and nothing else).</summary>
        public string SpotId { get; }

        /// <summary>The room (1-based) where it is hidden.</summary>
        public int Room { get; }

        /// <summary>Upgrade node ids switched on when it is installed (see <see cref="UpgradeCatalog"/>).</summary>
        public string[] Grants { get; }

        public string Effect { get; }

        /// <summary>The heading of the discovery dialog ("Uncle Archie's apron").</summary>
        public string Title { get; }

        /// <summary>A few lines in the player's voice: how the find helped Uncle Archie (warm and quiet, autumn 1947, no magic).</summary>
        public string Story { get; }

        /// <summary>What to do with it ("You tie it on. It will be waiting on its hook at the desk.").</summary>
        public string Hint { get; }
    }

    public enum InstallResult
    {
        Installed,
        WrongSpot,
        AlreadyInstalled,
        UnknownFind,
        LockedSpot
    }

    public static class CampaignCatalog
    {
        public const int RoomCount = 3;

        public static readonly string[] RoomNames = { "Reading Room", "Mailroom", "Map Room" };

        public static string RoomName(int room)
        {
            return room >= 1 && room <= RoomNames.Length ? RoomNames[room - 1] : "Room " + room;
        }

        /// <summary>The desk spot that stays locked until a later build (Quick Shelve).</summary>
        public const string DumbwaiterSpot = "dumbwaiter_hatch";

        public static readonly IReadOnlyList<FindDef> Finds = new List<FindDef>
        {
            new FindDef("apron", "Apron", "apron_hook", 1, new[] { "arms1", "arms2" }, "Carry 2 more books",
                "Uncle Archie's apron",
                "Heavy canvas, patched so many times there is more mending than apron. Thirty years of returns went through those pockets, and he never once made a second trip to the cart. He used to say a librarian who carries one book at a time is only walking.",
                "You tie it on. It will be waiting on its hook at the desk."),
            new FindDef("shoes", "Soft-soled shoes", "shoe_rack", 1, new[] { "feet1" }, "Move 15% faster",
                "Uncle Archie's felt-soled shoes",
                "He had them resoled every winter so the reading room never heard him coming, and the floorboards never once complained. A quiet step is a quick step when you know where everything lives. They are a size too big for me. They will do.",
                "You lace them up. They will be waiting on the shoe rack at the desk."),
            new FindDef("glasses", "Reading glasses", "glasses_stand", 2, new[] { "glow" }, "Sister Glow (G): matching items glow",
                "Uncle Archie's reading glasses",
                "Round wire spectacles with a little brass magnifier that folds down. He could pick one cloth spine out of a wall of them from across the room, and I never learned how.",
                "Stand them on the glasses stand at the desk."),
            new FindDef("shelflist", "Shelf list", "shelf_list_board", 2, new[] { "beacon" }, "Home Beacon (B): the right shelf glows",
                "Uncle Archie's shelf list",
                "A narrow oak tray of index cards in shelf order, held by a brass rod. Whatever you were holding, he could tell you where it lived without looking up.",
                "Hang it on the shelf-list board at the desk."),
            new FindDef("pulllist", "Pull list", "clipboard", 3, new[] { "gather" }, "Gather Family (C): pulls matching items to you",
                "Uncle Archie's pull list",
                "A long ruled slip on a thin board, with a pencil on a string. He wrote down what a reader needed and the whole family of books came off the shelves at once.",
                "Clip it to the clipboard at the desk."),
        };

        public static FindDef FindById(string id)
        {
            foreach (FindDef f in Finds)
            {
                if (f.Id == id)
                {
                    return f;
                }
            }

            return null;
        }

        public static FindDef FindForSpot(string spotId)
        {
            foreach (FindDef f in Finds)
            {
                if (f.SpotId == spotId)
                {
                    return f;
                }
            }

            return null;
        }
    }

    /// <summary>Where the Lamplit campaign stands: which finds were picked up and installed, which rooms are tidied, which doors are open.
    /// Engine-free. A room's door opens when the previous room is finished (room 1 is open from the start); an admin can force doors.</summary>
    public sealed class CampaignState
    {
        private readonly HashSet<string> _found = new HashSet<string>();
        private readonly HashSet<string> _installed = new HashSet<string>();
        private readonly bool[] _finished = new bool[CampaignCatalog.RoomCount + 1];
        private readonly int[] _forced = new int[CampaignCatalog.RoomCount + 1];   // 0 = by rule, 1 = forced open, -1 = forced shut

        public int FoundCount => _found.Count;
        public int InstalledCount => _installed.Count;

        public bool IsFound(string findId)
        {
            return findId != null && _found.Contains(findId);
        }

        public bool IsInstalled(string findId)
        {
            return findId != null && _installed.Contains(findId);
        }

        /// <summary>Records that the player has picked the find up. True the first time.</summary>
        public bool MarkFound(string findId)
        {
            return CampaignCatalog.FindById(findId) != null && _found.Add(findId);
        }

        /// <summary>Tries to put a find into a desk spot. Only the spot that matches the find accepts it.</summary>
        public InstallResult TryInstall(string findId, string spotId)
        {
            FindDef find = CampaignCatalog.FindById(findId);
            if (find == null)
            {
                return InstallResult.UnknownFind;
            }

            if (spotId == CampaignCatalog.DumbwaiterSpot)
            {
                return InstallResult.LockedSpot;
            }

            if (find.SpotId != spotId)
            {
                return InstallResult.WrongSpot;
            }

            if (_installed.Contains(findId))
            {
                return InstallResult.AlreadyInstalled;
            }

            _found.Add(findId);
            _installed.Add(findId);
            return InstallResult.Installed;
        }

        /// <summary>Admin: install without a spot.</summary>
        public bool ForceInstall(string findId)
        {
            return CampaignCatalog.FindById(findId) != null && _found.Add(findId) | _installed.Add(findId);
        }

        /// <summary>Applies every installed find's upgrades to <paramref name="upgrades"/> (free). Returns how many nodes were newly granted.</summary>
        public int ApplyTo(UpgradeState upgrades)
        {
            int granted = 0;
            foreach (string id in _installed)
            {
                foreach (string node in CampaignCatalog.FindById(id).Grants)
                {
                    if (upgrades.Grant(node))
                    {
                        granted++;
                    }
                }
            }

            return granted;
        }

        public bool IsRoomFinished(int room)
        {
            return room >= 1 && room <= CampaignCatalog.RoomCount && _finished[room];
        }

        /// <summary>Marks a room tidied. True the first time. A tidied room stays tidied even if it is messed up again.</summary>
        public bool FinishRoom(int room)
        {
            if (room < 1 || room > CampaignCatalog.RoomCount || _finished[room])
            {
                return false;
            }

            _finished[room] = true;
            return true;
        }

        public bool IsDoorOpen(int room)
        {
            if (room < 1 || room > CampaignCatalog.RoomCount)
            {
                return false;
            }

            if (_forced[room] != 0)
            {
                return _forced[room] > 0;
            }

            return room == 1 || _finished[room - 1];
        }

        /// <summary>Admin: force a door open or shut regardless of the rule.</summary>
        public void ForceDoor(int room, bool open)
        {
            if (room >= 1 && room <= CampaignCatalog.RoomCount)
            {
                _forced[room] = open ? 1 : -1;
            }
        }

        /// <summary>Admin: back to the rule for every door.</summary>
        public void ClearForcedDoors()
        {
            Array.Clear(_forced, 0, _forced.Length);
        }

        public void ResetRoom(int room)
        {
            if (room >= 1 && room <= CampaignCatalog.RoomCount)
            {
                _finished[room] = false;
            }
        }

        public void Reset()
        {
            _found.Clear();
            _installed.Clear();
            Array.Clear(_finished, 0, _finished.Length);
            Array.Clear(_forced, 0, _forced.Length);
        }
    }
}
