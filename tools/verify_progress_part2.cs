// Part 2: run ~4 s after verify_progress_playmode.cs, still playing. Checks the Quick Shelve flights landed and the timer ran.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var pick = System.AppDomain.CurrentDomain.GetData("qsPick") as string[]; if (pick == null) return "run part 1 first";
var gp = UnityEngine.GameObject.Find("Game").GetComponent<GameProgress>(); var qs = UnityEngine.GameObject.Find("Game").GetComponent<QuickShelve>();
int home = 0; string detail = "";
foreach (var n in pick) { var b = UnityEngine.GameObject.Find(n).GetComponent<Book>(); var ok = b.currentSlot != null && b.Grade() == BookBoy.Rules.PlacementResult.Correct; if (ok) home++; detail += " " + n + (ok ? "=home" : "=NOT(" + (b.currentSlot != null ? b.Grade().ToString() : "floating") + ")"); }
Check("all 4 books flew to a shelf and are graded Correct (right bookcase, right shelf, volume N in slot N)", home == 4, detail);
Check("the flights are over (nothing is still busy) and no slot is left reserved", !qs.IsBusy && System.Linq.Enumerable.Count(UnityEngine.Object.FindObjectsByType<ShelfSlot>(UnityEngine.FindObjectsInactive.Include), s => s.reserved) == 0);
var held = UnityEngine.GameObject.Find("Player").GetComponent<PlayerInteractor>().Carry;
Check("the hands are empty afterwards", held.IsEmpty);
double t0 = (double)System.AppDomain.CurrentDomain.GetData("t0"); double t1 = gp.ElapsedSeconds;
Check("the run timer advances while playing (and is shown as m:ss)", t1 > t0 + 1.0 && BookBoy.Rules.TimeFormat.Format(t1).Contains(":"), t0.ToString("F1") + " -> " + t1.ToString("F1"));

// ---- completion: arrange every book correctly -> level complete, time frozen ----
var cats = new[] { "Alchemy", "Astronomy", "Beasts", "History" };
foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) if (b.currentSlot != null) b.currentSlot.Release();
for (int c = 0; c < 4; c++) for (int s = 0; s < 2; s++) for (int v = 1; v <= 4; v++) {
  var b = UnityEngine.GameObject.Find("Book_" + cats[c] + "_" + (s == 0 ? "A" : "B") + v).GetComponent<Book>();
  UnityEngine.GameObject.Find("Bookcase_" + (c + 1)).GetComponent<Bookcase>().shelves[s].slots[v - 1].Accept(b, b.transform.localScale); }
gp.Recompute();
Check("all 32 correct: 8 shelves finished, level complete", gp.BooksCorrect == 32 && gp.ShelvesFinished == 8 && gp.LevelComplete, gp.BooksCorrect + " " + gp.ShelvesFinished);
var text = gp.completionText; Check("the completion message shows the final time", text != null && text.enabled && text.text.Contains("restored") && text.text.Contains(":"), text != null ? text.text : "null");
double frozen = gp.ElapsedSeconds; sb.Append("final time " + BookBoy.Rules.TimeFormat.Format(frozen) + ", points earned in total " + gp.Upgrades.TotalEarned + "\n");
sb.Append(fails == 0 ? "ALL PASSED (part 2)" : (fails + " FAILED"));
return sb.ToString();
