// Play Mode check for the admin "pile up" buttons (run while playing; part 2 = tools/verify_pile_part2.cs, ~15 s later).
// Part 1 shelves 4 books correctly, holds 2 more, then presses "Pile every loose book" in the admin menu.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
Book B(string n) { return UnityEngine.GameObject.Find(n).GetComponent<Book>(); }
Bookcase CaseOfTheme(BookCategory t) { foreach (var bc in UnityEngine.Object.FindObjectsByType<Bookcase>(UnityEngine.FindObjectsInactive.Include)) if (bc.theme == t) return bc; return null; }
var game = UnityEngine.GameObject.Find("Game"); var gp = game.GetComponent<GameProgress>(); var ui = game.GetComponent<GameUI>(); var admin = game.GetComponent<AdminTools>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
if (admin == null) return "FAIL: AdminTools component missing from the Game object";

foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) { if (b.currentSlot != null) b.currentSlot.Release(); if (carry.Contains(b)) carry.Release(b, out _); b.Outline?.StopAll(); }
gp.ResetUpgrades(); carry.SetCapacity(4);
for (int v = 1; v <= 4; v++) { var bk = B("Book_Alchemy_A" + v); CaseOfTheme(BookCategory.Alchemy).shelves[0].slots[v - 1].Accept(bk, bk.transform.localScale); }
pi.PickUp(B("Book_Beasts_A1")); pi.PickUp(B("Book_Beasts_A2"));
int shelved = 0; foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) if (b.currentSlot != null) shelved++;
Check("setup: 4 books shelved, 2 in the hands", shelved == 4 && carry.Count == 2, shelved + " shelved, " + carry.Count + " carried");

ui.OpenAdmin();
Check("the admin menu lists the pile buttons", ui.AdminOpen && System.Linq.Enumerable.Any(new[] { 5, 6 }, i => ui.RowText(i).Contains("Pile")), ui.RowText(5) + " | " + ui.RowText(6));
bool did = ui.Activate(5);
Check("'Pile every loose book' closes the menu, resumes the game and starts dropping", did && !ui.MenuOpen && UnityEngine.Time.timeScale == 1f && admin.IsPiling);
Check("the hands are emptied into the pile and the shelved books are left alone", carry.IsEmpty && B("Book_Alchemy_A1").currentSlot != null && B("Book_Alchemy_A4").currentSlot != null);
Check("a second press while the pile is falling is ignored", admin.PileUp(false) == 0);
sb.Append("pile started at t=" + UnityEngine.Time.time.ToString("F1") + "; run verify_pile_part2 in ~15 s\n");
System.AppDomain.CurrentDomain.SetData("pileStart", (double)UnityEngine.Time.time);
sb.Append(fails == 0 ? "ALL PASSED (part 1)" : (fails + " FAILED"));
return sb.ToString();
