// Play Mode check for Lamplit Room 1 (the Reading Room) at adult scale: 96 books in 16 sets (8 sets of 8, 8 sets of 4) on a triple bookcase (4 + 8 + 4
// slots wide) and a solo 8-wide bookcase, four shelves each. A set belongs on a row of its own size; Group grading; claims across shelves; completion.
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
var rows8 = new System.Collections.Generic.List<Shelf>(); var rows4 = new System.Collections.Generic.List<Shelf>();
foreach (var s in zone.shelves) (s.slots.Length == 8 ? rows8 : rows4).Add(s);
BookBoy.Rules.PlacementResult Put(Book b, ShelfSlot s) { if (b.currentSlot != null) b.currentSlot.Release(); return s.Accept(b, b.transform.localScale); }
void ClearAll() { foreach (var b in zone.items) if (b.currentSlot != null) b.currentSlot.Release(); }
// letters A, C, E, ... are sets of 8; B, D, F, ... are sets of 4

// ---- the room as built ----
var cases = System.Linq.Enumerable.Distinct(System.Linq.Enumerable.Select(zone.shelves, sh => sh.bookcase));
Check("96 items in 16 sets, 16 shelves (8 of 8 slots, 8 of 4), 4 bookcases", zone.items.Length == 96 && zone.shelves.Length == 16 && rows8.Count == 8 && rows4.Count == 8 && System.Linq.Enumerable.Count(cases) == 4,
    zone.items.Length + " items, " + zone.shelves.Length + " shelves (" + rows8.Count + "/" + rows4.Count + "), " + System.Linq.Enumerable.Count(cases) + " cases");
Check("each bookcase has four shelves", System.Linq.Enumerable.All(cases, c => c.shelves.Length == 4));
var perGroup = new System.Collections.Generic.Dictionary<int, int>(); foreach (var b in zone.items) { int g = b.GroupKey; perGroup[g] = perGroup.ContainsKey(g) ? perGroup[g] + 1 : 1; }
Check("16 sets: 8 of 8 books and 8 of 4 books", perGroup.Count == 16 && System.Linq.Enumerable.Count(perGroup.Values, c => c == 8) == 8 && System.Linq.Enumerable.Count(perGroup.Values, c => c == 4) == 8);
Check("every item knows its set size and it matches its set", System.Linq.Enumerable.All(zone.items, b => b.definition.setSize == perGroup[b.GroupKey]));
var symbols = new System.Collections.Generic.HashSet<string>(); foreach (var b in zone.items) symbols.Add(b.definition.symbol);
Check("every set has its own letter (colour never decides alone)", symbols.Count == 16);
Check("all items start loose on the floor, inside the room", System.Linq.Enumerable.All(zone.items, b => b.currentSlot == null && zone.Contains(b.transform.position) && b.transform.position.y < 1.0f));
Check("each bookcase uses the Group rule and accepts any family", System.Linq.Enumerable.All(zone.shelves, s => s.bookcase.rules == ContainerRules.Group && s.bookcase.containerFamily == -1));
Check("the room's door and controller are wired (room 1 registered)", cc.RoomCount >= 1 && cc.Room(1) == zone);
Check("books are tomes: 12 to 15 in tall, 4 in thick, 10 in deep, and fit their shelf (spine under the slot pitch)", System.Linq.Enumerable.All(zone.items, b => b.transform.localScale.y >= 0.3047f && b.transform.localScale.y <= 0.3811f && System.Math.Abs(b.transform.localScale.x - 0.1016f) < 0.0005f && System.Math.Abs(b.transform.localScale.z - 0.254f) < 0.0005f && b.transform.localScale.x < LamplitBuilder.SlotPitch));

// the spine: icon, letter and number on every book; every set colour keeps its contrast with the wood (backlog T-010)
Check("every plain book has a set icon, a letter and a volume number on its spine (the pixel labels)", System.Linq.Enumerable.All(zone.items, bk => bk.definition.coverStyle >= 0 || (bk.transform.Find("Label_Icon") != null && bk.transform.Find("Label_Symbol") != null && bk.transform.Find("Label_Number") != null)));
Check("every printed book (sets A to F) has no pixel labels: its spine texture carries the icon, the title and the volume number", System.Linq.Enumerable.All(zone.items, bk => bk.definition.coverStyle < 0 || (bk.transform.Find("Label_Icon") == null && bk.GetComponent<UnityEngine.MeshRenderer>().sharedMaterials.Length == 3)));
double woodEff = 0.066, coverTex = 0.733; double minRatio = 1e9, sumRatio = 0; var seenSets = new System.Collections.Generic.HashSet<int>(); int nSets = 0;
foreach (var bk in zone.items) { if (!seenSets.Add(bk.GroupKey)) continue; var c = bk.definition.spineColor; double r = BookBoy.Rules.Palette.ContrastRatio(BookBoy.Rules.Palette.Luminance(c.r, c.g, c.b) * coverTex, woodEff); minRatio = System.Math.Min(minRatio, r); sumRatio += r; nSets++; }
Check("every set colour keeps a contrast of at least 2.2 against the wood, average above 2.5", minRatio >= 2.19 && sumRatio / nSets > 2.5, "min " + minRatio.ToString("F2") + " avg " + (sumRatio / nSets).ToString("F2"));
// headroom: the tallest book clears the board above it by at least 1 in, on every row including the top one
float board = LamplitBuilder.BoardThickness, inch = 0.0254f;
Check("every row has at least 1 in of headroom above the tallest (15 in) book", LamplitBuilder.ShelfSpacing - board - LamplitBuilder.BookMaxHeight >= inch - 0.0005f && LamplitBuilder.ContainerHeight - board - (LamplitBuilder.BaseHeight + 3f * LamplitBuilder.ShelfSpacing) - LamplitBuilder.BookMaxHeight >= inch - 0.0005f,
    (LamplitBuilder.ShelfSpacing - board - LamplitBuilder.BookMaxHeight).ToString("F3") + " m between rows, " + (LamplitBuilder.ContainerHeight - board - (LamplitBuilder.BaseHeight + 3f * LamplitBuilder.ShelfSpacing) - LamplitBuilder.BookMaxHeight).ToString("F3") + " m on top");

// ---- Group grading: the first set claims the row, any slot is fine ----
ClearAll();
Check("first book of an 8-set on an empty 8-row, middle slot: green", Put(B('A', 1), rows8[0].slots[2]) == R);
Check("the same set in any other slot is green (order does not matter)", Put(B('A', 2), rows8[0].slots[0]) == R && Put(B('A', 3), rows8[0].slots[7]) == R);
Check("a different set on that row: yellow", Put(B('C', 1), rows8[0].slots[1]) == Y);
Check("that other 8-set on an empty 8-row claims it: green", Put(B('C', 1), rows8[1].slots[3]) == R);
Check("a book already shelved is regraded live (set A books still green)", B('A', 1).Grade() == R && B('A', 2).Grade() == R);
Check("the same set on a second 8-row: yellow (the earlier row owns the set)", Put(B('A', 4), rows8[2].slots[0]) == Y);

// ---- set size must match the row ----
ClearAll();
Check("a set of 4 on an empty 8-row: yellow (this row is for sets of 8)", Put(B('B', 1), rows8[0].slots[0]) == Y);
Check("a set of 8 on an empty 4-row: yellow (this row is for sets of 4)", Put(B('A', 1), rows4[0].slots[0]) == Y);
Check("a set of 4 on an empty 4-row: green", Put(B('B', 1), rows4[1].slots[1]) == R);
Check("a set of 8 on an empty 8-row: green", Put(B('A', 1), rows8[0].slots[4]) == R);

// removing the earliest book moves the claim
ClearAll();
Put(B('C', 1), rows8[3].slots[0]); Put(B('E', 1), rows8[3].slots[1]);
Check("two sets on one row: the earlier book's set is green, the later one yellow", B('C', 1).Grade() == R && B('E', 1).Grade() == Y);
B('C', 1).currentSlot.Release();
Check("remove the earliest and the claim passes to the next book", B('E', 1).Grade() == R);

// ---- finishing a row ----
ClearAll();
for (int n = 1; n <= 8; n++) Put(B('G', n), rows8[4].slots[8 - n]);   // reverse order: any order is fine
Check("eight of one 8-set on an 8-row (any order) is a finished row", zone.CountFinishedShelves() == 1 && zone.CountCorrect() == 8, zone.CountFinishedShelves() + " finished, " + zone.CountCorrect() + " correct");
for (int n = 1; n <= 4; n++) Put(B('H', n), rows4[1].slots[4 - n]);
Check("four of one 4-set on a 4-row is a finished row", zone.CountFinishedShelves() == 2 && zone.CountCorrect() == 12, zone.CountFinishedShelves() + " finished, " + zone.CountCorrect() + " correct");
Put(B('I', 1), rows8[5].slots[0]);
Check("a half-filled row is not finished", zone.CountFinishedShelves() == 2);

// ---- the finds are in the room ----
Check("the apron and the shoes are hidden in this room, on the table", zone.finds.Length == 2 && zone.finds[0].IsFind && zone.finds[1].IsFind && zone.Contains(zone.finds[0].transform.position) && zone.finds[0].definition.findId == "apron" && zone.finds[1].definition.findId == "shoes");
Check("a find cannot be put on a shelf", (carry.Items.Count == 0 || true) && pi.PickUp(zone.finds[0]) && !pi.PlaceCurrent(zone.shelves[6].slots[0]) && zone.shelves[6].slots[0].occupant == null);
Check("picking a find up marks it found", cc.State.IsFound("apron"));
carry.Release(zone.finds[0], out _);

// ---- HUD in the room ----
cc.JumpTo(1);
cc.Recompute();
Check("standing in Room 1 the HUD says so (items, shelves)", cc.CurrentRoom == 1 && cc.StatsText().Contains("READING ROOM") && cc.StatsText().Contains("ITEMS") && cc.StatsText().Contains("/ 96"), cc.StatsText());

// ---- completion: every item home -> the room is tidy and the next door opens ----
ClearAll();
cc.CompleteRoom(1);
Check("all 96 items in their homes are green, all 16 rows finished", zone.CountCorrect() == 96 && zone.CountFinishedShelves() == 16, zone.CountCorrect() + " correct, " + zone.CountFinishedShelves() + " finished");
cc.Recompute();
Check("the room is tidy, and Room 2's door opens (when Room 2 exists)", cc.State.IsRoomFinished(1) && cc.State.IsDoorOpen(2));
cc.PileRoom(1);
Check("Room 1 stays tidy even if it is messed up again", cc.State.IsRoomFinished(1));
Check("scattering puts every item loose again", zone.CountCorrect() == 0 && System.Linq.Enumerable.All(zone.items, b => b.currentSlot == null));

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
