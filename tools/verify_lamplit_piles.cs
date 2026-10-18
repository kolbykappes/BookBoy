// Play Mode check for the room piles (part 1; part 2 = tools/verify_lamplit_piles_part2.cs, >= 10 s later, needs running frames).
// Part 1: every room STARTS as several random piles in the middle; then "pile up" is triggered from the admin menu on a tidy Room 1.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var game = UnityEngine.GameObject.Find("Game"); var ui = game.GetComponent<GameUI>();
if (cc.RoomCount != 3) return "FAIL: all three rooms must be built";

// returns the sizes of the clusters of items (items closer than 0.55 m belong together), largest first
int[] Clusters(Book[] items) {
  int n = items.Length; var comp = new int[n]; for (int i = 0; i < n; i++) comp[i] = -1; int count = 0;
  for (int i = 0; i < n; i++) { if (comp[i] >= 0) continue; var stack = new System.Collections.Generic.Stack<int>(); stack.Push(i); comp[i] = count;
    for (int guard = 0; guard < 100000 && stack.Count > 0; guard++) { int a = stack.Pop(); for (int j = 0; j < n; j++) if (comp[j] < 0 && (items[a].transform.position - items[j].transform.position).sqrMagnitude < 0.55f * 0.55f) { comp[j] = count; stack.Push(j); } }
    count++; }
  var sizes = new int[count]; foreach (var c in comp) sizes[c]++; System.Array.Sort(sizes); System.Array.Reverse(sizes); return sizes; }

// ---- the starting layout: piles, not a grid ----
foreach (var z in cc.rooms)
{
    var sizes = Clusters(z.items);
    int big = 0; foreach (var s in sizes) if (s >= 5) big++;
    Check("room " + z.roomNumber + " starts as 3-8 random piles in the middle (not a grid of single books)", big >= 3 && sizes.Length <= 10 && sizes[0] >= 15, string.Join(",", sizes));
    bool floor = true, solid = true, loose = true; float maxY = 0;
    foreach (var b in z.items) { maxY = System.Math.Max(maxY, b.transform.position.y); if (b.currentSlot != null) loose = false; if (b.GetComponent<UnityEngine.Rigidbody>() != null || !b.GetComponent<UnityEngine.Collider>().isTrigger) solid = false; if (!z.Contains(b.transform.position) || b.transform.position.y > 1.4f) floor = false; }
    Check("room " + z.roomNumber + ": every item is loose, on the floor inside the room, a normal pick-up-able trigger (highest " + maxY.ToString("F2") + " m)", floor && solid && loose);
    float cx = 0, cz = 0; foreach (var b in z.items) { cx += b.transform.position.x; cz += b.transform.position.z; } cx /= z.items.Length; cz /= z.items.Length;
    Check("room " + z.roomNumber + ": the piles are in the middle of the room, away from the shelves", cx > z.scatterMin.x - 1f && cx < z.scatterMax.x + 1f && cz > z.scatterMin.y - 1f && cz < z.scatterMax.y + 1f, cx.ToString("F1") + "," + cz.ToString("F1"));
}

// ---- pile up again from the admin menu, starting from a tidy room ----
cc.ResetAll();
cc.JumpTo(1); cc.CompleteRoom(1); cc.Recompute();
var z1 = cc.Room(1);
Check("setup: Room 1 is tidy, every item on a shelf", z1.CountCorrect() == 100 && cc.State.IsRoomFinished(1));
ui.OpenAdmin();
Check("the admin row says 'pile'", ui.RowText(2).Contains("Pile"), ui.RowText(2));
bool did = ui.Activate(2);
Check("'Pile this room's books up' starts the pile-up: every item is off its shelf at once", did && z1.IsPiling && System.Linq.Enumerable.All(z1.items, b => b.currentSlot == null) && z1.LastPlan != null && z1.LastPlan.PileCount >= 3, "piling=" + z1.IsPiling);
Check("the plan has 3-7 uneven piles that hold all 100 items", z1.LastPlan.PileCount >= 3 && z1.LastPlan.PileCount <= 7 && System.Linq.Enumerable.Sum(System.Linq.Enumerable.Range(0, z1.LastPlan.PileCount), p => z1.LastPlan.ItemsIn(p)) == 100);
Check("it stays tidy in the record (a room once finished stays finished)", cc.State.IsRoomFinished(1));
UnityEngine.Time.timeScale = 1f;
sb.Append("started; run verify_lamplit_piles_part2 in ~10 s\n");
sb.Append(fails == 0 ? "ALL PASSED (part 1)" : (fails + " FAILED"));
return sb.ToString();
