// Part 2 of the pile check, run ~15 s after verify_pile_playmode.cs. Verifies the loose-books pile, then starts "pile ALL".
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var game = UnityEngine.GameObject.Find("Game"); var ui = game.GetComponent<GameUI>(); var admin = game.GetComponent<AdminTools>();
if (System.AppDomain.CurrentDomain.GetData("pileStart") == null) return "run part 1 first";
var center = new UnityEngine.Vector3(admin.pileCenter.x, 0f, admin.pileCenter.y);
var all = UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include);
int loose = 0, shelved = 0, inPile = 0, solid = 0, withBody = 0; float maxY = 0f, maxR = 0f; string stray = "";
foreach (var b in all)
{
    if (b.currentSlot != null) { shelved++; continue; }
    loose++;
    var p = b.transform.position; float r = new UnityEngine.Vector2(p.x - center.x, p.z - center.z).magnitude;
    maxR = UnityEngine.Mathf.Max(maxR, r); maxY = UnityEngine.Mathf.Max(maxY, p.y);
    if (r < 2.2f && p.y < 1.2f && p.y > -0.05f) inPile++; else stray += " " + b.name + "(r=" + r.ToString("F1") + ",y=" + p.y.ToString("F2") + ")";
    if (!b.GetComponent<UnityEngine.Collider>().isTrigger) solid++;
    if (b.GetComponent<UnityEngine.Rigidbody>() != null) withBody++;
}
Check("the drop has finished", !admin.IsPiling);
Check("all 124 unshelved books are in one pile in the middle of the room (within 2.2 m, on the floor)", loose == 124 && inPile == 124, loose + " loose, " + inPile + " in pile, stray:" + stray);
Check("the 4 shelved books were left on their shelf", shelved == 4);
Check("the pile is a heap, not a flat spread: it reaches above a single book's thickness", maxY > 0.12f, "highest book y=" + maxY.ToString("F2") + ", widest r=" + maxR.ToString("F2"));
Check("every book settled and went back to being a normal pick-up-able book (trigger collider, no rigidbody left)", solid == 0 && withBody == 0, solid + " still solid, " + withBody + " with bodies");
sb.Append("pile: highest y=" + maxY.ToString("F2") + ", widest radius=" + maxR.ToString("F2") + "\n");

ui.OpenAdmin(); bool did = ui.Activate(6);
Check("'Pile ALL books' also empties the shelves", did && !ui.MenuOpen && admin.IsPiling);
Check("the shelves are emptied at once", System.Linq.Enumerable.All(all, b => b.currentSlot == null));
sb.Append("pile ALL started; check with verify_pile_part3 in ~15 s\n");
sb.Append(fails == 0 ? "ALL PASSED (part 2)" : (fails + " FAILED"));
return sb.ToString();
