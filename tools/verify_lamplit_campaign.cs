// Play Mode check of the whole Lamplit loop (the "tomorrow's test script" in Docs/14-build-plan.md), start to finish in one session:
// hub -> Room 1 (group rule, finds) -> install apron and shoes -> tidy Room 1 -> Room 2 door opens -> Room 2 (family rule, finds) ->
// glasses and shelf list -> tidy Room 2 -> Room 3 (ordered sets, find) -> pull list -> tidy Room 3 -> all done.
// Run with Lamplit.unity open and playing, in a fresh Play session. Needs no running frames.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var R = BookBoy.Rules.PlacementResult.Correct; var Y = BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot; var Red = BookBoy.Rules.PlacementResult.WrongBookcase;
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var game = UnityEngine.GameObject.Find("Game");
var gp = game.GetComponent<GameProgress>(); var ui = game.GetComponent<GameUI>(); var sg = game.GetComponent<SisterGlow>(); var hb = game.GetComponent<HomeBeacon>(); var gf = game.GetComponent<GatherFamily>(); var qs = game.GetComponent<QuickShelve>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry; var fpc = pl.GetComponent<StarterAssets.FirstPersonController>();
if (cc.RoomCount != 3) return "FAIL: all three rooms must be built";
cc.ResetAll(); float baseSpeed = fpc.MoveSpeed;
BookBoy.Rules.PlacementResult Put(Book b, ShelfSlot s) { if (b.currentSlot != null) b.currentSlot.Release(); return s.Accept(b, b.transform.localScale); }
Book Item(string name) { return UnityEngine.GameObject.Find(name).GetComponent<Book>(); }
void Bring(string findId, string spotId)   // pick the find up where it was hidden, walk to the hub, hang it in its spot
{
    var b = cc.FindBook(findId); cc.JumpTo(cc.State.IsRoomFinished(1) && findId != "apron" && findId != "shoes" ? 2 : 1); pi.PickUp(b); cc.JumpTo(0);
    cc.InstallHeld(cc.SpotFor(spotId), carry);
}

// 1. the hub
Check("1. start in the hub, at the desk, with 4 carry, normal speed and every ability locked", cc.CurrentRoom == 0 && carry.Capacity == 4 && !gp.Upgrades.HasSisterGlow && !gp.Upgrades.HasHomeBeacon && !gp.Upgrades.HasGatherFamily && !gp.Upgrades.HasQuickShelve);
Check("1. only Room 1's door is open", cc.State.IsDoorOpen(1) && !cc.State.IsDoorOpen(2) && !cc.State.IsDoorOpen(3) && !cc.doors[1].IsOpen);

// 2. Room 1: group rule
var z1 = cc.Room(1); cc.JumpTo(1);
var r8 = new System.Collections.Generic.List<Shelf>(); foreach (var sh in z1.shelves) if (sh.slots.Length == 8) r8.Add(sh);   // Room 1: sets of 8 (A, C, E, ...) belong on the 8-slot rows
Check("2. Room 1: the first book claims an empty row; another set grades yellow, a matching series volume in its own slot green", Put(Item("R1_A1"), r8[0].slots[0]) == R && Put(Item("R1_C1"), r8[0].slots[3]) == Y && Put(Item("R1_A2"), r8[0].slots[1]) == R);
for (int n = 1; n <= 8; n++) { if (Item("R1_A" + n).currentSlot != null) Item("R1_A" + n).currentSlot.Release(); }
if (Item("R1_C1").currentSlot != null) Item("R1_C1").currentSlot.Release();
for (int n = 1; n <= 8; n++) Put(Item("R1_E" + n), r8[3].slots[n - 1]);
cc.Recompute();
Check("3. eight volumes in order on an 8-slot row: it counts as finished", z1.CountFinishedShelves() == 1 && z1.CountCorrect() == 8);

// 4. the finds of Room 1
Bring("apron", "apron_hook"); Bring("shoes", "shoe_rack");
Check("4. the apron and shoes are installed: carry reads 6 and the player moves faster", carry.Capacity == 6 && System.Math.Abs(fpc.MoveSpeed - baseSpeed * 1.15f) < 0.01f, carry.Capacity + " / " + fpc.MoveSpeed);

// 5. finish Room 1
cc.JumpTo(1); cc.CompleteRoom(1); cc.Recompute();
Check("5. finishing Room 1: it is tidy and Room 2's door opens", cc.State.IsRoomFinished(1) && cc.State.IsDoorOpen(2) && cc.doors[1].IsOpen && !cc.State.IsDoorOpen(3));

// 6. Room 2
var z2 = cc.Room(2); cc.JumpTo(2);
Check("6. Room 2: a letter in a book container refuses (red), each family fits its own container (green)", Put(Item("R2_L_A1"), z2.shelves[0].slots[0]) == Red && Put(Item("R2_L_A1"), z2.shelves[12].slots[0]) == R && Put(Item("R2_P_A1"), z2.shelves[18].slots[1]) == R && Put(Item("R2_M_A1"), z2.shelves[7].slots[3]) == R && Put(Item("R2_B_A1"), z2.shelves[1].slots[4]) == R);
Bring("glasses", "glasses_stand"); Bring("shelflist", "shelf_list_board");
Check("6. glasses and shelf list installed: Sister Glow and Home Beacon are on", gp.Upgrades.HasSisterGlow && gp.Upgrades.HasHomeBeacon && !gp.Upgrades.HasGatherFamily);
foreach (var b in z2.items) if (b.currentSlot != null) b.currentSlot.Release();
cc.JumpTo(2); var held = Item("R2_B_C1"); pi.PickUp(held);
Check("6. Sister Glow lights the held book's group, Home Beacon shows a book container", sg.Activate() == 4 && hb.Activate() && System.Linq.Enumerable.All(hb.LitBookcases, bc => bc.containerFamily == 0));
carry.Release(held, out _);
cc.CompleteRoom(2); cc.Recompute();
Check("6. Room 2 tidy: Room 3's door opens", cc.State.IsRoomFinished(2) && cc.State.IsDoorOpen(3) && cc.doors[2].IsOpen);

// 7. Room 3
var z3 = cc.Room(3); cc.JumpTo(3);
Check("7. Room 3: numbered sets grade green only in order", Put(Item("R3_A_A1"), z3.shelves[0].slots[0]) == R && Put(Item("R3_A_A2"), z3.shelves[0].slots[3]) == Y && Put(Item("R3_A_A2"), z3.shelves[0].slots[1]) == R);
foreach (var b in z3.items) if (b.currentSlot != null) b.currentSlot.Release();
Bring("pulllist", "clipboard");
Check("7. the pull list is installed: Gather Family is on", gp.Upgrades.HasGatherFamily);
cc.JumpTo(3); var h3 = Item("R3_S_B2"); pi.PickUp(h3);
Check("7. Gather Family pulls two matching sheets to the player", gf.Activate() == 2 && gf.IncomingCount == 2);
carry.Release(h3, out _);
cc.CompleteRoom(3); cc.Recompute();

// the end
cc.JumpTo(0); cc.Recompute();
Check("all three rooms tidy and all five finds installed", cc.State.IsRoomFinished(1) && cc.State.IsRoomFinished(2) && cc.State.IsRoomFinished(3) && cc.State.InstalledCount == 5, cc.Counts());
Check("the HUD in the hub: 3 / 3 rooms tidy, 5 / 5 finds", cc.StatsText().Contains("3 / 3") && cc.StatsText().Contains("5 / 5"), cc.StatsText());
Check("Quick Shelve stays locked (the dumbwaiter is for later) and refuses politely", !gp.Upgrades.HasQuickShelve && qs.Run() == 0 && cc.InstallHeld(cc.SpotFor(BookBoy.Rules.CampaignCatalog.DumbwaiterSpot), carry) == BookBoy.Rules.InstallResult.LockedSpot);
Check("shelves earned no tech-tree points anywhere", gp.Upgrades.Points == 0);

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
