// Play Mode check for Lamplit Room 2 (the Mailroom): four item families, the container family check, group claims across the room,
// completion, and the glasses (Sister Glow) and shelf list (Home Beacon) finds. Run with Lamplit.unity open and playing.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var R = BookBoy.Rules.PlacementResult.Correct; var Y = BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot; var Red = BookBoy.Rules.PlacementResult.WrongBookcase;
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var zone = cc.Room(2); var zone1 = cc.Room(1);
if (zone == null || zone1 == null) return "FAIL: Rooms 1 and 2 must be built";
var game = UnityEngine.GameObject.Find("Game"); var gp = game.GetComponent<GameProgress>(); var sg = game.GetComponent<SisterGlow>(); var hb = game.GetComponent<HomeBeacon>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
Book B(char fam, char g, int n) { return UnityEngine.GameObject.Find("R2_" + fam + "_" + g + n).GetComponent<Book>(); }
int Fam(int f) { return f; }
ShelfSlot Slot(int container, int shelf, int k) { return zone.shelves[container * 2 + shelf].slots[k]; }
BookBoy.Rules.PlacementResult Put(Book b, ShelfSlot s) { if (b.currentSlot != null) b.currentSlot.Release(); return s.Accept(b, b.transform.localScale); }
void ClearAll() { foreach (var b in zone.items) if (b.currentSlot != null) b.currentSlot.Release(); foreach (var b in zone1.items) if (b.currentSlot != null) b.currentSlot.Release(); }
cc.ResetAll();

// containers: 0-2 books, 3-5 magazines, 6-8 letters, 9-11 parcels
// ---- the room as built ----
Check("120 items, 24 shelves of 5, 12 containers", zone.items.Length == 120 && zone.shelves.Length == 24 && System.Linq.Enumerable.All(zone.shelves, s => s.slots.Length == 5));
var perFamily = new int[4]; foreach (var b in zone.items) perFamily[b.FamilyKey]++;
Check("4 families of 30 items each (books, magazines, letters, parcels)", perFamily[0] == 30 && perFamily[1] == 30 && perFamily[2] == 30 && perFamily[3] == 30, string.Join(",", perFamily));
var groups = new System.Collections.Generic.Dictionary<int, int>(); foreach (var b in zone.items) { groups[b.GroupKey] = groups.ContainsKey(b.GroupKey) ? groups[b.GroupKey] + 1 : 1; }
Check("24 groups of 5 (6 per family)", groups.Count == 24 && System.Linq.Enumerable.All(groups.Values, c => c == 5));
bool famOk = true; for (int c = 0; c < 12; c++) { var bc = zone.shelves[c * 2].bookcase; if (bc.containerFamily != c / 3 || bc.rules != ContainerRules.Group) famOk = false; }
Check("containers 0-2 hold books, 3-5 magazines, 6-8 letters, 9-11 parcels (Group rule)", famOk);
var shapes = new System.Collections.Generic.HashSet<string>(); foreach (var b in zone.items) shapes.Add(b.FamilyKey + ":" + b.transform.localScale.ToString());
Check("each family has its own shape (4 distinct sizes)", shapes.Count == 4, shapes.Count.ToString());

// ---- the family check: the wrong kind of container is red, even when empty ----
ClearAll();
Check("a letter in a BOOK container: red", Put(B('L', 'A', 1), Slot(0, 0, 0)) == Red);
Check("a parcel in a MAGAZINE container: red", Put(B('P', 'A', 1), Slot(3, 0, 0)) == Red);
Check("a magazine in a LETTER container: red", Put(B('M', 'A', 1), Slot(6, 0, 0)) == Red);
Check("a book in a PARCEL container: red", Put(B('B', 'A', 1), Slot(9, 0, 0)) == Red);
Check("each item in its own kind of container is green (empty shelf, any slot)", Put(B('B', 'A', 1), Slot(1, 0, 3)) == R && Put(B('M', 'A', 1), Slot(4, 0, 2)) == R && Put(B('L', 'A', 1), Slot(7, 0, 4)) == R && Put(B('P', 'A', 1), Slot(10, 0, 0)) == R);
// groups within a family container
Check("the same group on the same shelf, any slot: green", Put(B('B', 'A', 2), Slot(1, 0, 0)) == R && Put(B('B', 'A', 3), Slot(1, 0, 1)) == R);
Check("another group of the same family on that shelf: yellow", Put(B('B', 'B', 1), Slot(1, 0, 2)) == Y);
Check("...and on the container's other, empty shelf: green", Put(B('B', 'B', 1), Slot(1, 1, 0)) == R);
Check("the first group on a SECOND container's shelf: yellow (the group already lives in the first)", Put(B('B', 'A', 4), Slot(2, 0, 0)) == Y);
// the room check: a Room 1 book in a Room 2 container, and a Room 2 book in a Room 1 container
var r1book = UnityEngine.GameObject.Find("R1_A1").GetComponent<Book>();
Check("a Room 1 book in a Room 2 book container: red (wrong room)", Put(r1book, Slot(2, 1, 0)) == Red);
Check("a Room 2 book in a Room 1 container: red (wrong room)", Put(B('B', 'C', 1), zone1.shelves[0].slots[0]) == Red);

// ---- finishing a shelf in this room ----
ClearAll();
for (int n = 1; n <= 5; n++) Put(B('P', 'C', n), Slot(11, 1, 5 - n));
Check("five parcels of one group on a parcel shelf is a finished shelf", zone.CountFinishedShelves() == 1 && zone.CountCorrect() == 5);

// ---- completion ----
ClearAll(); cc.CompleteRoom(2); cc.Recompute();
Check("all 120 items home: every one is green, 24 shelves finished", zone.CountCorrect() == 120 && zone.CountFinishedShelves() == 24, zone.CountCorrect() + " / " + zone.CountFinishedShelves());
Check("Room 2 is tidy and opens Room 3's door", cc.State.IsRoomFinished(2) && cc.State.IsDoorOpen(3));
cc.PileRoom(2);
Check("scattering puts them back on the floor", zone.CountCorrect() == 0 && System.Linq.Enumerable.All(zone.items, b => b.currentSlot == null));

// ---- the glasses switch Sister Glow on; it lights only the held item's group ----
Check("two finds hidden here: glasses and shelf list", zone.finds.Length == 2 && zone.finds[0].definition.findId == "glasses" && zone.finds[1].definition.findId == "shelflist" && zone.Contains(zone.finds[0].transform.position));
Check("Sister Glow is locked before the glasses are installed", sg.Activate() == 0 && !gp.Upgrades.HasSisterGlow);
cc.InstallFind("glasses");
Check("installing the glasses unlocks Sister Glow (and only it)", gp.Upgrades.HasSisterGlow && !gp.Upgrades.HasHomeBeacon && !gp.Upgrades.HasGatherFamily && cc.State.IsInstalled("glasses"));
cc.JumpTo(2);
var held = B('B', 'D', 1); pi.PickUp(held);
int lit = sg.Activate();
bool sisters = true; for (int n = 2; n <= 5; n++) sisters &= B('B', 'D', n).Outline.IsPulsing;
bool noOthers = !B('L', 'D', 1).Outline.IsPulsing && !B('B', 'E', 1).Outline.IsPulsing && !B('M', 'D', 1).Outline.IsPulsing;
Check("Sister Glow lights the held book's 4 loose sisters (same family and group) and nothing else", lit == 4 && sisters && noOthers, "lit=" + lit);

// ---- the shelf list switches Home Beacon on; the right container glows ----
cc.InstallFind("shelflist");
Check("the shelf list unlocks Home Beacon", gp.Upgrades.HasHomeBeacon);
carry.Release(held, out _); var letter = B('L', 'B', 1); pi.PickUp(letter);
Check("Home Beacon lights a LETTER container for a letter (never a book or parcel container)", hb.Activate() && System.Linq.Enumerable.All(hb.LitBookcases, bc => bc.containerFamily == 2) && System.Linq.Enumerable.Count(hb.LitBookcases) == 1, "lit=" + System.Linq.Enumerable.Count(hb.LitBookcases));
gp.ResetCooldowns();
// a letter group already claimed on a shelf: the beacon points at that container
carry.Release(letter, out _);
var claimedSlot = Slot(8, 1, 1); Put(B('L', 'C', 1), claimedSlot);
var next = B('L', 'C', 2); pi.PickUp(next);
Check("with the group already claimed on a shelf, Home Beacon points at that very container", hb.Activate() && System.Linq.Enumerable.Last(hb.LitBookcases) == claimedSlot.shelf.bookcase);

sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
