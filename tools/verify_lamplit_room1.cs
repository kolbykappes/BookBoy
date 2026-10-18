// Play Mode check for Lamplit Room 1 (the Reading Room): 100 books, 20 groups of 5, Group grading, claims across shelves, completion.
// Run with Lamplit.unity open and playing. Everything it changes is scene state that the Editor restores on leaving Play Mode.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var R = BookBoy.Rules.PlacementResult.Correct; var Y = BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot;

var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var zone = cc.Room(1);
if (zone == null) return "FAIL: Room 1 is not built";
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
Book B(char g, int n) { return UnityEngine.GameObject.Find("R1_" + g + n).GetComponent<Book>(); }
ShelfSlot Slot(int shelf, int k) { return zone.shelves[shelf].slots[k]; }
BookBoy.Rules.PlacementResult Put(Book b, ShelfSlot s) { if (b.currentSlot != null) b.currentSlot.Release(); return s.Accept(b, b.transform.localScale); }
void ClearAll() { foreach (var b in zone.items) if (b.currentSlot != null) b.currentSlot.Release(); }

// ---- the room as built ----
Check("100 items in 20 groups of 5, 20 shelves of 5 slots, 10 containers", zone.items.Length == 100 && zone.shelves.Length == 20 && System.Linq.Enumerable.All(zone.shelves, s => s.slots.Length == 5) && System.Linq.Enumerable.Count(System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Select(zone.shelves, sh => sh.bookcase))) == 10);
var perGroup = new System.Collections.Generic.Dictionary<int, int>(); foreach (var b in zone.items) { int g = b.GroupKey; perGroup[g] = perGroup.ContainsKey(g) ? perGroup[g] + 1 : 1; }
Check("exactly 20 groups with 5 books each", perGroup.Count == 20 && System.Linq.Enumerable.All(perGroup.Values, c => c == 5));
var symbols = new System.Collections.Generic.HashSet<string>(); foreach (var b in zone.items) symbols.Add(b.definition.symbol);
Check("every group has its own letter (colour never decides alone)", symbols.Count == 20);
Check("all items start loose on the floor, inside the room", System.Linq.Enumerable.All(zone.items, b => b.currentSlot == null && zone.Contains(b.transform.position) && b.transform.position.y < 1.4f));
Check("each container uses the Group rule and accepts any family", System.Linq.Enumerable.All(zone.shelves, s => s.bookcase.rules == ContainerRules.Group && s.bookcase.containerFamily == -1));
Check("the room's door and controller are wired (room 1 registered)", cc.RoomCount >= 1 && cc.Room(1) == zone);

// ---- Group grading: the first book claims the shelf, any slot is fine ----
ClearAll();
Check("first book on an empty shelf, middle slot: green", Put(B('A', 1), Slot(0, 2)) == R);
Check("the same group in any other slot is green (order does not matter)", Put(B('A', 2), Slot(0, 0)) == R && Put(B('A', 3), Slot(0, 4)) == R);
Check("a different group on that shelf: yellow", Put(B('B', 1), Slot(0, 1)) == Y);
Check("that other group on an empty shelf claims it: green", Put(B('B', 1), Slot(1, 3)) == R);
Check("a book already shelved is regraded live (group A books still green)", B('A', 1).Grade() == R && B('A', 2).Grade() == R);
// a group cannot be split over two shelves
Check("the same group on a second shelf: yellow (the earlier shelf owns the group)", Put(B('A', 4), Slot(2, 0)) == Y);
// removing the earliest book moves the claim
ClearAll();
Put(B('C', 1), Slot(3, 0)); Put(B('D', 1), Slot(3, 1));
Check("two groups on one shelf: the earlier book's group is green, the later one yellow", B('C', 1).Grade() == R && B('D', 1).Grade() == Y);
B('C', 1).currentSlot.Release();
Check("remove the earliest and the claim passes to the next book", B('D', 1).Grade() == R);

// ---- finishing a shelf ----
ClearAll();
for (int n = 1; n <= 5; n++) Put(B('E', n), Slot(4, 5 - n));   // reverse order: any order is fine
Check("five of one group on one shelf (any order) is a finished shelf", zone.CountFinishedShelves() == 1 && zone.CountCorrect() == 5, zone.CountFinishedShelves() + " finished, " + zone.CountCorrect() + " correct");
Put(B('F', 1), Slot(5, 0));
Check("a half-filled shelf is not finished", zone.CountFinishedShelves() == 1);

// ---- the finds are in the room ----
Check("the apron and the shoes are hidden in this room, on the table", zone.finds.Length == 2 && zone.finds[0].IsFind && zone.finds[1].IsFind && zone.Contains(zone.finds[0].transform.position) && zone.finds[0].definition.findId == "apron" && zone.finds[1].definition.findId == "shoes");
Check("a find cannot be put on a shelf", (carry.Items.Count == 0 || true) && pi.PickUp(zone.finds[0]) && !pi.PlaceCurrent(Slot(6, 0)) && Slot(6, 0).occupant == null);
Check("picking a find up marks it found", cc.State.IsFound("apron"));
carry.Release(zone.finds[0], out _);

// ---- HUD in the room ----
cc.JumpTo(1);
cc.Recompute();
Check("standing in Room 1 the HUD says so (items, shelves)", cc.CurrentRoom == 1 && cc.StatsText().Contains("READING ROOM") && cc.StatsText().Contains("ITEMS") && cc.StatsText().Contains("/ 100"), cc.StatsText());

// ---- completion: every item home -> the room is tidy and the next door opens ----
ClearAll();
cc.CompleteRoom(1);
Check("all 100 items in their homes are green", zone.CountCorrect() == 100 && zone.CountFinishedShelves() == 20, zone.CountCorrect() + " correct, " + zone.CountFinishedShelves() + " finished");
cc.Recompute();
Check("the room is tidy, and Room 2's door opens (when Room 2 exists)", cc.State.IsRoomFinished(1) && cc.State.IsDoorOpen(2));
cc.PileRoom(1);
Check("Room 1 stays tidy even if it is messed up again", cc.State.IsRoomFinished(1));
Check("scattering puts every item loose again", zone.CountCorrect() == 0 && System.Linq.Enumerable.All(zone.items, b => b.currentSlot == null));

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
