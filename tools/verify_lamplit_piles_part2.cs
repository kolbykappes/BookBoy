// Part 2 of the pile check, >= 10 s after verify_lamplit_piles.cs: the dropped items have landed in several piles and are normal items again.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var z = cc.Room(1);
int[] Clusters(Book[] items) {
  int n = items.Length; var comp = new int[n]; for (int i = 0; i < n; i++) comp[i] = -1; int count = 0;
  for (int i = 0; i < n; i++) { if (comp[i] >= 0) continue; var stack = new System.Collections.Generic.Stack<int>(); stack.Push(i); comp[i] = count;
    for (int guard = 0; guard < 100000 && stack.Count > 0; guard++) { int a = stack.Pop(); for (int j = 0; j < n; j++) if (comp[j] < 0 && (items[a].transform.position - items[j].transform.position).sqrMagnitude < 0.55f * 0.55f) { comp[j] = count; stack.Push(j); } }
    count++; }
  var sizes = new int[count]; foreach (var c in comp) sizes[c]++; System.Array.Sort(sizes); System.Array.Reverse(sizes); return sizes; }

Check("the pile-up has finished", !z.IsPiling);
var sizes = Clusters(z.items);
int big = 0; foreach (var s in sizes) if (s >= 5) big++;
Check("the 100 books landed in several piles (3 or more with 5+ books each), not one heap and not a grid", big >= 3 && sizes[0] < 70, string.Join(",", sizes));
int air = 0, bodies = 0, solid = 0, shelved = 0, outside = 0; float maxY = 0;
foreach (var b in z.items) { maxY = System.Math.Max(maxY, b.transform.position.y); if (b.transform.position.y > 0.7f) air++; if (b.GetComponent<UnityEngine.Rigidbody>() != null) bodies++; if (!b.GetComponent<UnityEngine.Collider>().isTrigger) solid++; if (b.currentSlot != null) shelved++; if (!z.Contains(b.transform.position)) outside++; }
Check("everything came to rest on the floor inside the room (highest " + maxY.ToString("F2") + " m)", air == 0 && outside == 0, air + " in the air, " + outside + " outside");
Check("every item settled back into a normal pick-up-able book (no rigidbody, trigger collider)", bodies == 0 && solid == 0, bodies + " bodies, " + solid + " solid");
Check("none of them is on a shelf, and the room scores 0 of 100", shelved == 0 && z.CountCorrect() == 0);
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>();
var pickMe = z.items[37]; bool picked = pi.PickUp(pickMe);
Check("a book from a pile can be picked up", picked && pi.Carry.Contains(pickMe));
sb.Append(fails == 0 ? "ALL PASSED (part 2)" : (fails + " FAILED"));
return sb.ToString();
