// Play Mode check for the Lamplit admin menu (F10): reset / scatter / complete a room, reset everything, jump, give and install finds,
// unlock abilities, doors, counts. Run with Lamplit.unity open and playing (all three rooms built).
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var game = UnityEngine.GameObject.Find("Game");
var gp = game.GetComponent<GameProgress>(); var ui = game.GetComponent<GameUI>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
if (cc.RoomCount != 3) return "FAIL: all three rooms must be built";
void Open() { if (!ui.AdminOpen) { if (ui.MenuOpen) ui.CloseMenu(); ui.OpenAdmin(); } }
bool Do(int row) { Open(); return ui.Activate(row); }
bool Loose(RoomZone z) { return System.Linq.Enumerable.All(z.items, b => b.currentSlot == null); }
cc.ResetAll();

Open();
Check("the admin menu is the Lamplit one: 22 rows, with the room commands first", ui.AdminOpen && ui.RowText(0).Contains("Reset this room") && ui.RowText(21) == "Close" && ui.RowText(22) == "", ui.RowText(0) + " | " + ui.RowText(21));
Check("commands that need a room refuse in the hub", !Do(0) && !Do(2) && !Do(3) && cc.CurrentRoom == 0);

// ---- jump ----
Check("jump to Room 1 closes the menu and puts the player inside it", Do(5) && !ui.MenuOpen && cc.CurrentRoom == 1, "room=" + cc.CurrentRoom);
Check("jump to Room 2", Do(6) && cc.CurrentRoom == 2, "room=" + cc.CurrentRoom);
Check("jump to Room 3", Do(7) && cc.CurrentRoom == 3, "room=" + cc.CurrentRoom);
Check("jump back to the hub", Do(4) && cc.CurrentRoom == 0 && System.Math.Abs(pl.transform.position.x) < 0.2f);

// ---- complete / scatter / reset this room ----
cc.JumpTo(1); cc.Recompute();
bool completed = Do(3); cc.Recompute();
Check("complete this room: all 96 items home and the room is tidy, Room 2's door opens", completed && cc.Room(1).CountCorrect() == 96 && cc.State.IsRoomFinished(1) && cc.State.IsDoorOpen(2));
Check("the counts line reflects it (R1 96/96 tidy)", ui.RowText(0) != null && cc.Counts().Contains("R1 96/96 tidy"), cc.Counts());
Check("scatter this room: items loose again, but it stays tidy", Do(2) && Loose(cc.Room(1)) && cc.State.IsRoomFinished(1));
Check("reset this room: items loose and the room is no longer tidy (door 2 shuts)", Do(0) && Loose(cc.Room(1)) && !cc.State.IsRoomFinished(1) && !cc.State.IsDoorOpen(2));

// ---- give / install finds ----
Check("give the apron: it goes straight to its hook (never in the hands), counted as found and installed", Do(8) && !carry.Contains(cc.FindBook("apron")) && cc.State.IsFound("apron") && cc.State.IsInstalled("apron") && carry.Capacity == 6);
Check("install the apron from the menu when it is already there: still capacity 6, hands empty", cc.State.IsInstalled("apron") && carry.Capacity == 6 && !carry.Contains(cc.FindBook("apron")));
Check("an installed find cannot be given again", !Do(8));
Check("install the shoes: speed up", Do(14) && cc.State.IsInstalled("shoes"));
Check("the install row says [installed] afterwards", ui.RowText(13).Contains("[installed]") && ui.RowText(14).Contains("[installed]") && !ui.RowText(15).Contains("[installed]"));
Check("unlock abilities: Sister Glow, Home Beacon and Gather Family on; Quick Shelve stays locked", Do(18) && gp.Upgrades.HasSisterGlow && gp.Upgrades.HasHomeBeacon && gp.Upgrades.HasGatherFamily && !gp.Upgrades.HasQuickShelve);

// ---- doors ----
Check("open every door", Do(19) && cc.doors[0].IsOpen && cc.doors[1].IsOpen && cc.doors[2].IsOpen && !cc.doors[2].label.text.Contains("LOCKED"));
Check("doors back to the rule (rooms not tidy: 2 and 3 shut again)", Do(20) && cc.doors[0].IsOpen && !cc.doors[1].IsOpen && !cc.doors[2].IsOpen);

// ---- reset everything ----
Check("reset everything: no finds, abilities locked, capacity 4, every room scattered, doors by rule", Do(1) && cc.State.InstalledCount == 0 && cc.State.FoundCount == 0 && !gp.Upgrades.HasSisterGlow && carry.Capacity == 4 && Loose(cc.Room(1)) && Loose(cc.Room(2)) && Loose(cc.Room(3)) && !cc.doors[1].IsOpen);
Check("the close row closes the menu and resumes play", Do(21) && !ui.MenuOpen && UnityEngine.Time.timeScale == 1f);

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
