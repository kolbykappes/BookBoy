// Part 3 of the pile check, ~15 s after verify_pile_part2.cs: all 128 books should be in the pile.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var game = UnityEngine.GameObject.Find("Game"); var admin = game.GetComponent<AdminTools>(); var gp = game.GetComponent<GameProgress>();
var center = new UnityEngine.Vector3(admin.pileCenter.x, 0f, admin.pileCenter.y);
int inPile = 0, shelved = 0, solid = 0; string stray = "";
foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include))
{
    if (b.currentSlot != null) shelved++;
    var p = b.transform.position; float r = new UnityEngine.Vector2(p.x - center.x, p.z - center.z).magnitude;
    if (r < 2.4f && p.y < 1.5f && p.y > -0.05f) inPile++; else stray += " " + b.name + "(r=" + r.ToString("F1") + ",y=" + p.y.ToString("F2") + ")";
    if (!b.GetComponent<UnityEngine.Collider>().isTrigger) solid++;
}
Check("the drop has finished", !admin.IsPiling);
Check("all 128 books are in the pile and none are shelved", inPile == 128 && shelved == 0, inPile + " in pile, " + shelved + " shelved, stray:" + stray);
Check("everything settled", solid == 0, solid + " still solid");
gp.Recompute();
Check("the scoreboard agrees: 0 of 128 books correct, 0 shelves finished", gp.BooksCorrect == 0 && gp.ShelvesFinished == 0);
sb.Append(fails == 0 ? "ALL PASSED (part 3)" : (fails + " FAILED"));
return sb.ToString();
