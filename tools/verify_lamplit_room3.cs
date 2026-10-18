// Play Mode check for Lamplit Room 3 (the Map Room): numbered sets in order across three container types, family check,
// completion of all three rooms, and the pull list (Gather Family). Run with Lamplit.unity open and playing.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var R = BookBoy.Rules.PlacementResult.Correct; var Y = BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot; var Red = BookBoy.Rules.PlacementResult.WrongBookcase;
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var zone = cc.Room(3);
if (zone == null) return "FAIL: Room 3 is not built";
var game = UnityEngine.GameObject.Find("Game"); var gp = game.GetComponent<GameProgress>(); var gf = game.GetComponent<GatherFamily>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
Book B(char fam, char g, int n) { return UnityEngine.GameObject.Find("R3_" + fam + "_" + g + n).GetComponent<Book>(); }
ShelfSlot Slot(int container, int shelf, int k) { return zone.shelves[container * 2 + shelf].slots[k]; }
BookBoy.Rules.PlacementResult Put(Book b, ShelfSlot s) { if (b.currentSlot != null) b.currentSlot.Release(); return s.Accept(b, b.transform.localScale); }
void ClearAll() { foreach (var b in zone.items) if (b.currentSlot != null) b.currentSlot.Release(); }
cc.ResetAll();

// containers 0-3 atlases (shelves), 4-7 rolled maps (tube racks), 8-11 flat sheets (drawers)
Check("120 items, 24 shelves of 5, 12 containers with the Ordered rule", zone.items.Length == 120 && zone.shelves.Length == 24 && System.Linq.Enumerable.All(zone.shelves, s => s.slots.Length == 5 && s.bookcase.rules == ContainerRules.Ordered));
var perFamily = new int[3]; foreach (var b in zone.items) perFamily[b.FamilyKey]++;
Check("3 families of 40 (atlases, rolled maps, flat sheets), 8 numbered sets of 5 each", perFamily[0] == 40 && perFamily[1] == 40 && perFamily[2] == 40);
var sets = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>>(); foreach (var b in zone.items) { if (!sets.ContainsKey(b.GroupKey)) sets[b.GroupKey] = new System.Collections.Generic.HashSet<int>(); sets[b.GroupKey].Add(b.NumberKey); }
Check("24 sets, each numbered 1 to 5", sets.Count == 24 && System.Linq.Enumerable.All(sets.Values, h => h.Count == 5 && h.Contains(1) && h.Contains(5)));
bool famOk = true; for (int c = 0; c < 12; c++) if (zone.shelves[c * 2].bookcase.containerFamily != c / 4) famOk = false;
Check("four containers per family (atlas shelves, tube racks, drawers)", famOk);
Check("every item shows its set letter and its number on the spine", System.Linq.Enumerable.All(zone.items, b => b.transform.Find("Label_Symbol") != null && b.transform.Find("Label_Number") != null));

// ---- Ordered grading ----
ClearAll();
Check("atlas A-1 in slot 1 of an atlas shelf: green", Put(B('A', 'A', 1), Slot(0, 0, 0)) == R);
Check("atlas A-2 in slot 2: green", Put(B('A', 'A', 2), Slot(0, 0, 1)) == R);
Check("atlas A-3 in slot 5 (wrong slot of the right shelf): yellow", Put(B('A', 'A', 3), Slot(0, 0, 4)) == Y);
Check("... and in its own slot 3: green", Put(B('A', 'A', 3), Slot(0, 0, 2)) == R);
Check("atlas B-1 on that shelf (another set), even in its right slot: yellow", Put(B('A', 'B', 1), Slot(0, 0, 3)) == Y);
Check("a rolled map in an ATLAS container: red", Put(B('R', 'A', 1), Slot(1, 0, 0)) == Red);
Check("a flat sheet in a TUBE container: red", Put(B('S', 'A', 1), Slot(4, 0, 0)) == Red);
Check("a rolled map in a tube rack, its own slot: green", Put(B('R', 'A', 1), Slot(4, 0, 0)) == R);
Check("a flat sheet in a drawer, wrong slot: yellow; right slot: green", Put(B('S', 'A', 4), Slot(8, 0, 0)) == Y && Put(B('S', 'A', 4), Slot(8, 0, 3)) == R);
Check("a set cannot be split over two shelves: its 2nd shelf is yellow", Put(B('A', 'A', 4), Slot(1, 0, 3)) == Y);
Check("a Room 1 item in a Room 3 container: red (wrong room)", Put(UnityEngine.GameObject.Find("R1_A1").GetComponent<Book>(), Slot(2, 1, 0)) == Red);

// a set in the wrong order is not a finished shelf; in order it is
ClearAll();
for (int n = 1; n <= 5; n++) Put(B('A', 'C', n), Slot(2, 0, 5 - n));
Check("a whole set in reverse order: not finished (only the middle one is right)", zone.CountFinishedShelves() == 0 && zone.CountCorrect() == 1, zone.CountFinishedShelves() + " finished, " + zone.CountCorrect() + " correct");
ClearAll(); for (int n = 1; n <= 5; n++) Put(B('A', 'C', n), Slot(2, 0, n - 1));
Check("the same set in order: finished", zone.CountFinishedShelves() == 1 && zone.CountCorrect() == 5);

// ---- every room finished ----
ClearAll();
cc.CompleteRoom(1); cc.CompleteRoom(2); cc.CompleteRoom(3); cc.Recompute();
Check("room 3 complete: 120 items green, 24 shelves finished", zone.CountCorrect() == 120 && zone.CountFinishedShelves() == 24);
Check("all three rooms are tidy and every door is open", cc.State.IsRoomFinished(1) && cc.State.IsRoomFinished(2) && cc.State.IsRoomFinished(3) && cc.State.IsDoorOpen(2) && cc.State.IsDoorOpen(3));
cc.JumpTo(0); cc.Recompute();
Check("in the hub the HUD counts 3 of 3 rooms tidy", cc.StatsText().Contains("3 / 3"), cc.StatsText());
cc.PileRoom(1); cc.PileRoom(2); cc.PileRoom(3);

// ---- the pull list switches Gather Family on ----
Check("the pull list is hidden here", zone.finds.Length == 1 && zone.finds[0].definition.findId == "pulllist" && zone.Contains(zone.finds[0].transform.position));
Check("Gather Family is locked before the pull list is installed", gf.Activate() == 0 && !gp.Upgrades.HasGatherFamily);
cc.InstallFind("pulllist");
Check("installing the pull list unlocks Gather Family (and only it)", gp.Upgrades.HasGatherFamily && !gp.Upgrades.HasSisterGlow && !gp.Upgrades.HasHomeBeacon && !gp.Upgrades.HasQuickShelve);
cc.JumpTo(3);
var held = B('A', 'D', 3); pi.PickUp(held);
int pulled = gf.Activate();
var incoming = new System.Collections.Generic.List<string>(); for (int n = 1; n <= 5; n++) if (n != 3 && B('A', 'D', n).transform.Find("GatherTrail") != null) incoming.Add("A-D" + n);
Check("Gather Family pulls 2 of the held atlas's loose sisters (same set only) with the trail", pulled == 2 && gf.IncomingCount == 2 && incoming.Count >= 1, "pulled=" + pulled + " trails=" + string.Join(",", incoming));
Check("it starts its own 60 s timer", gp.CooldownRemaining(BookBoy.Rules.AbilityId.GatherFamily) > 59f);

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
