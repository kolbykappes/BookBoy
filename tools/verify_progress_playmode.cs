// Play Mode check for GameProgress (timer, points, upgrades), the menus, Quick Shelve and Sister Glow (run while playing).
// Part 1 = this file. Part 2 = tools/verify_progress_part2.cs, run ~4 s later (flights need real frames).
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
Book B(string n) { return UnityEngine.GameObject.Find(n).GetComponent<Book>(); }
Bookcase Case(int i) { return UnityEngine.GameObject.Find("Bookcase_" + (i + 1)).GetComponent<Bookcase>(); }
var cats = new[] { "Alchemy", "Astronomy", "Beasts", "History" };
string BN(int c, int series, int vol) { return "Book_" + cats[c] + "_" + (series == 0 ? "A" : "B") + vol; }

var game = UnityEngine.GameObject.Find("Game"); var gp = game.GetComponent<GameProgress>(); var ui = game.GetComponent<GameUI>(); var qs = game.GetComponent<QuickShelve>(); var sg = game.GetComponent<SisterGlow>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry; var fpc = pl.GetComponent<StarterAssets.FirstPersonController>(); var cursor = pl.GetComponent<CursorLock>();
void Reset() { foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) { if (b.currentSlot != null) b.currentSlot.Release(); if (carry.Contains(b)) carry.Release(b, out _); b.Outline?.StopAll(); } }
void PlaceAt(Book b, ShelfSlot s) { if (b.currentSlot != null) b.currentSlot.Release(); s.Accept(b, b.transform.localScale); }

// ---- a fresh run ----
Reset(); gp.ResetUpgrades(); gp.Recompute();
Check("fresh run: 32 books, 8 shelves, 0 correct, 0 finished, 0 points, capacity 4", gp.BooksTotal == 32 && gp.ShelvesTotal == 8 && gp.BooksCorrect == 0 && gp.ShelvesFinished == 0 && gp.Upgrades.Points == 0 && carry.Capacity == 4, gp.BooksTotal + "/" + gp.ShelvesTotal + " cap=" + carry.Capacity);
double t0 = gp.ElapsedSeconds; System.AppDomain.CurrentDomain.SetData("t0", t0);

// ---- finishing a shelf pays one point, once ----
int earned = 0; System.Action<int> h = n => earned += n; gp.PointsEarned += h;
for (int v = 1; v <= 4; v++) PlaceAt(B(BN(0, 0, v)), Case(0).shelves[0].slots[v - 1]);
gp.Recompute();
Check("a finished shelf (4 books of one series, in order) is counted and pays 1 point", gp.ShelvesFinished == 1 && gp.BooksCorrect == 4 && gp.Upgrades.Points == 1 && earned == 1, "finished=" + gp.ShelvesFinished + " correct=" + gp.BooksCorrect + " points=" + gp.Upgrades.Points + " earned=" + earned);
// break it up and finish it again: no second payment
Case(0).shelves[0].slots[3].Release(); gp.Recompute();
Check("breaking the shelf drops it from the finished count but keeps the point", gp.ShelvesFinished == 0 && gp.Upgrades.Points == 1);
PlaceAt(B(BN(0, 0, 4)), Case(0).shelves[0].slots[3]); gp.Recompute();
Check("finishing the same shelf again does not pay twice", gp.ShelvesFinished == 1 && gp.Upgrades.Points == 1 && earned == 1, "points=" + gp.Upgrades.Points);
// a wrong-order shelf is not finished
for (int v = 1; v <= 4; v++) PlaceAt(B(BN(1, 0, v)), Case(1).shelves[0].slots[4 - v]);
gp.Recompute();
Check("four books in the wrong order do not finish a shelf", gp.ShelvesFinished == 1 && gp.Upgrades.Points == 1);
gp.PointsEarned -= h;

// ---- upgrades ----
Check("cannot buy a tier without its prerequisite", !gp.TryBuy("arms2"));
Check("buying Strong Arms I spends the point and raises the carry capacity to 5", gp.TryBuy("arms1") && gp.Upgrades.Points == 0 && carry.Capacity == 5);
Check("cannot buy with no points", !gp.TryBuy("feet1"));
gp.AddPoints(10); gp.TryBuy("feet1"); gp.TryBuy("feet2");
Check("speed upgrades raise the player's move speed (x1.3)", System.Math.Abs(fpc.MoveSpeed - 4f * 1.3f) < 0.01f && System.Math.Abs(fpc.SprintSpeed - 6f * 1.3f) < 0.01f, fpc.MoveSpeed + " / " + fpc.SprintSpeed);
gp.UnlockAll(); Check("unlock-all gives capacity 8 and both abilities", carry.Capacity == 8 && gp.Upgrades.HasQuickShelve && gp.Upgrades.HasSisterGlow);
gp.ResetUpgrades(); Check("reset returns to capacity 4, normal speed, no abilities", carry.Capacity == 4 && System.Math.Abs(fpc.MoveSpeed - 4f) < 0.01f && !gp.Upgrades.HasQuickShelve);

// ---- locked abilities refuse ----
Reset(); pi.PickUp(B(BN(2, 0, 3)));
Check("Quick Shelve refuses while locked", qs.Run() == 0 && carry.Count == 1);
Check("Sister Glow refuses while locked", sg.Activate() == 0);

// ---- Sister Glow ----
gp.AddPoints(1); gp.TryBuy("glow");
Reset(); pi.PickUp(B(BN(2, 0, 3)));   // hold Beasts A3; its unshelved sisters are Beasts A1, A2, A4
PlaceAt(B(BN(2, 0, 2)), Case(2).shelves[0].slots[1]);   // Beasts A2 is shelved: must not glow
int lit = sg.Activate();
bool a1 = B(BN(2, 0, 1)).Outline.IsPulsing, a4 = B(BN(2, 0, 4)).Outline.IsPulsing, a2 = B(BN(2, 0, 2)).Outline.IsPulsing, held = B(BN(2, 0, 3)).Outline.IsPulsing;
bool otherSeries = B(BN(2, 1, 1)).Outline.IsPulsing, otherColor = B(BN(1, 0, 1)).Outline.IsPulsing;
Check("Sister Glow lights the held book's unshelved sisters only", lit == 2 && a1 && a4 && !a2 && !held && !otherSeries && !otherColor, "lit=" + lit + " A1=" + a1 + " A4=" + a4 + " A2(shelved)=" + a2 + " held=" + held + " B1=" + otherSeries + " other color=" + otherColor);
Check("Sister Glow then recharges (a second press is refused)", sg.Activate() == 0 && sg.SecondsUntilReady > 3f);

// ---- Quick Shelve (flights finish in part 2) ----
gp.AddPoints(2); gp.TryBuy("shelve");
Reset(); gp.Recompute();
var pick = new[] { BN(3, 0, 2), BN(0, 1, 1), BN(2, 0, 4), BN(1, 0, 3) };
foreach (var n in pick) pi.PickUp(B(n));
int sent = qs.Run();
Check("Quick Shelve sends all 4 held books flying and frees the hands as they leave", sent == 4);
System.AppDomain.CurrentDomain.SetData("qsPick", pick);
Check("while flying, the destination slots are reserved (nobody else can take them)", UnityEngine.Object.FindObjectsByType<ShelfSlot>(UnityEngine.FindObjectsInactive.Include).Length > 0 && System.Linq.Enumerable.Count(UnityEngine.Object.FindObjectsByType<ShelfSlot>(UnityEngine.FindObjectsInactive.Include), s => s.reserved) == 4);

// ---- menus ----
gp.ResetUpgrades(); gp.AddPoints(3);
ui.OpenUpgrades();
Check("the upgrade menu opens, pauses the game and frees the cursor", ui.MenuOpen && ui.UpgradesOpen && UnityEngine.Time.timeScale == 0f && !fpc.enabled && !pi.enabled && cursor.suspended && UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.None && UnityEngine.Cursor.visible);
Check("rows describe the upgrades with cost and state", ui.RowText(0).Contains("Strong Arms I") && ui.RowText(0).Contains("BUY") && ui.RowText(1).Contains("needs Strong Arms I") && ui.RowText(8).Contains("Quick Shelve"), ui.RowText(0) + " | " + ui.RowText(1));
Check("choosing a row buys it: Strong Arms I costs 1 of the 3 points", ui.Activate(0) && gp.Upgrades.IsUnlocked("arms1") && gp.Upgrades.Points == 2, "points=" + gp.Upgrades.Points);
Check("a row you cannot buy yet (Fleet Feet II needs Fleet Feet I) does nothing and costs nothing", !ui.Activate(5) && !gp.Upgrades.IsUnlocked("feet2") && gp.Upgrades.Points == 2);
Check("the 2-point Quick Shelve row buys with the remaining points", ui.Activate(8) && gp.Upgrades.IsUnlocked("shelve") && gp.Upgrades.Points == 0, "points=" + gp.Upgrades.Points);
ui.CloseMenu();
Check("closing restores play: time runs, player enabled, cursor locked", !ui.MenuOpen && UnityEngine.Time.timeScale == 1f && fpc.enabled && pi.enabled && !cursor.suspended && UnityEngine.Cursor.lockState == UnityEngine.CursorLockMode.Locked && !UnityEngine.Cursor.visible);
ui.OpenAdmin();
int before = gp.Upgrades.Points; ui.Activate(2);
Check("the admin menu opens and its buttons work (+10 points)", ui.AdminOpen && gp.Upgrades.Points == before + 10, "points=" + gp.Upgrades.Points);
ui.Activate(3); Check("admin 'unlock every upgrade' works", gp.Upgrades.HasQuickShelve && gp.Upgrades.HasSisterGlow && carry.Capacity == 8);
ui.Activate(4); Check("admin 'reset' works", gp.Upgrades.Points == 0 && carry.Capacity == 4);
ui.Activate(5); Check("admin 'close' closes the menu and restores play", !ui.MenuOpen && UnityEngine.Time.timeScale == 1f);

sb.Append(fails == 0 ? "ALL PASSED (part 1)" : (fails + " FAILED"));
return sb.ToString();
