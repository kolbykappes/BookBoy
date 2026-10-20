// Play Mode check of the held-book stack (Librarian-style: flat books at the lower right, current on top). Run with Lamplit.unity playing.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
Book B(string n) { return UnityEngine.GameObject.Find("R1_" + n).GetComponent<Book>(); }
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var zone = cc.Room(1);
foreach (var b in zone.items) if (b.currentSlot != null) b.currentSlot.Release();
while (carry.Count > 0) { var cur = carry.Current; Vector3 sc; carry.Release(cur, out sc); }
pl.transform.position = V(-4.0f, 0f, 3.56f); pl.transform.rotation = UnityEngine.Quaternion.Euler(0f, 270f, 0f); UnityEngine.Physics.SyncTransforms();

Check("the carry point is the CarryStack child of the player root (turns with the player, not with the camera pitch)", carry.carryPoint != null && carry.carryPoint.name == "CarryStack" && carry.carryPoint.parent == pl.transform);
Check("books are carried at their shelf size", UnityEngine.Mathf.Approximately(carry.carryScale, 1f), carry.carryScale.ToString());
string TopToBottom() { var l = new System.Collections.Generic.List<Book>(carry.Items); l.Sort((x, y) => y.transform.position.y.CompareTo(x.transform.position.y)); var t = ""; foreach (var b in l) t += b.name.Substring(3) + " "; return t.Trim(); }
var names = new[] { "A1", "A2", "A3", "B1" };
var expectAfter = new[] { "A1", "A2 A1", "A3 A2 A1", "B1 A3 A2 A1" };
bool buildOk = true; string buildLog = "";
for (int pk = 0; pk < names.Length; pk++) { carry.TryPickUp(B(names[pk])); carry.SnapStack(); string got = TopToBottom(); buildLog += "[" + got + "] "; if (got != expectAfter[pk]) buildOk = false; }
Check("picking up four in a row builds the stack newest on top, then each earlier pick in order (B1 A3 A2 A1), the second book moving down each time", buildOk, buildLog);
Check("four books in hand", carry.Count == 4, carry.Count.ToString());
var top = carry.Current;
bool flat = true; string badFlat = "";
foreach (var b in carry.Items) { float up = UnityEngine.Vector3.Dot(b.transform.right, UnityEngine.Vector3.up); if (up < 0.999f) { flat = false; badFlat += b.name + ":" + up; } }
Check("every carried book lies flat with its front cover up", flat, badFlat);
float spineLeft = UnityEngine.Vector3.Dot(top.transform.forward, pl.transform.right);
Check("the spine of the top book points to the player's left (fore-edge to the right)", spineLeft > 0.99f, spineLeft.ToString());
bool descending = true; float lastY = float.MaxValue;
for (int o = 0; o < carry.Count; o++)
{
    var b = carry.Items[(carry.CurrentIndex - o + carry.Count) % carry.Count];
    float y = b.transform.position.y;
    if (y >= lastY) descending = false; lastY = y;
}
Check("the current book is on top and the others hang below it in order", descending);
var below = carry.Items[(carry.CurrentIndex - 1 + carry.Count) % carry.Count];
float gap = top.transform.position.y - below.transform.position.y;
Check("the books sit one thickness apart vertically (no overlap)", gap >= top.transform.localScale.x * 1.0f - 0.003f && gap <= top.transform.localScale.x * 1.06f, gap.ToString());
var fan = top.transform.position - below.transform.position;
Check("the stack is fanned: each lower book is also shifted to the player's left and toward the player, so its edge shows", UnityEngine.Vector3.Dot(fan, pl.transform.right) > 0.02f && UnityEngine.Vector3.Dot(fan, pl.transform.forward) > 0.03f, fan.ToString());
Check("the newest pick-up is the current book and on top", carry.Current == B("B1") && top == B("B1"));
Check("the stack's anchor is lower than the old 1.36 m (looking down at the floor it no longer covers the middle of the view)", carry.carryPoint.localPosition.y < 1.2f, carry.carryPoint.localPosition.y.ToString());

// the view: a 16:9 camera at the eye, level and looking down
var camGo = new UnityEngine.GameObject("TmpCam"); var cam = camGo.AddComponent<UnityEngine.Camera>(); cam.fieldOfView = 58.7f; cam.aspect = 16f / 9f; cam.nearClipPlane = 0.05f;
bool InView(Book b) { var r = b.GetComponent<UnityEngine.Renderer>().bounds; int c = 0; for (int i = 0; i < 8; i++) { var p = r.center + V((i & 1) == 0 ? -r.extents.x : r.extents.x, (i & 2) == 0 ? -r.extents.y : r.extents.y, (i & 4) == 0 ? -r.extents.z : r.extents.z); var v = cam.WorldToViewportPoint(p); if (v.z > 0 && v.x >= 0 && v.x <= 1 && v.y >= 0 && v.y <= 1) c++; } return c > 0; }
Vector3 Center(Book b) { var v = cam.WorldToViewportPoint(b.GetComponent<UnityEngine.Renderer>().bounds.center); return v; }
var eye = pl.transform.position + V(0f, 1.65f, 0f);
camGo.transform.position = eye; camGo.transform.rotation = UnityEngine.Quaternion.Euler(0f, 270f, 0f);
Check("at a level gaze the stack stays out of the way (below the view)", !InView(top));
camGo.transform.rotation = UnityEngine.Quaternion.Euler(28f, 270f, 0f);
Check("tilted down 28 degrees the top book peeks in at the lower right", InView(top) && Center(top).x > 0.55f && Center(top).y < 0.35f, Center(top).ToString());
camGo.transform.rotation = UnityEngine.Quaternion.Euler(40f, 270f, 0f);
var cvp = Center(top);
Check("looking down 40 degrees the stack is in the right half of the view", InView(top) && cvp.x > 0.55f && cvp.y > 0.1f && cvp.y < 0.9f, cvp.ToString());
camGo.transform.rotation = UnityEngine.Quaternion.Euler(40f, 270f, 0f);
float sep = 9f; for (int k = 1; k < carry.Count; k++) { var bk = carry.Items[(carry.CurrentIndex - k + carry.Count) % carry.Count]; var prev = carry.Items[(carry.CurrentIndex - k + 1 + carry.Count) % carry.Count]; sep = UnityEngine.Mathf.Min(sep, UnityEngine.Mathf.Abs(Center(bk).y - Center(prev).y) + UnityEngine.Mathf.Abs(Center(bk).x - Center(prev).x)); }
Check("looking down 40 degrees every book in the stack sits visibly apart from the one above it", sep > 0.03f, sep.ToString());
camGo.transform.rotation = UnityEngine.Quaternion.Euler(0f, 90f, 0f);
Check("looking away (turned round) the stack is behind the camera", !InView(top));
UnityEngine.Object.DestroyImmediate(camGo);

// cycling: the top book goes to the bottom and the next one comes up (and the other way round)
string orderBefore = TopToBottom();   // B1 A3 A2 A1
carry.Cycle(-1); carry.SnapStack();
Check("one step (R or wheel down) sends the top book to the bottom and brings the next one up", orderBefore == "B1 A3 A2 A1" && TopToBottom() == "A3 A2 A1 B1", orderBefore + " -> " + TopToBottom());
carry.Cycle(+1); carry.SnapStack();
Check("one step the other way (wheel up) brings the bottom book back to the top", TopToBottom() == orderBefore, TopToBottom());
carry.Cycle(+1); carry.SnapStack();
Check("wheel up again brings the bottom book (A1) to the top with the old top second", TopToBottom() == "A1 B1 A3 A2", TopToBottom());
carry.Cycle(-1); carry.SnapStack();
float yTop = carry.Current.transform.position.y; bool allBelow = true;
foreach (var b in carry.Items) if (b != carry.Current && b.transform.position.y >= yTop) allBelow = false;
Check("after cycling the new current book is the highest", allBelow);

var cur2 = carry.Current; carry.Cycle(-1); carry.SnapStack(); var back = carry.Current; carry.Cycle(+1); carry.SnapStack();
Check("the wheel's two directions walk the held books both ways and come back", back != cur2 && carry.Current == cur2);
// hands full: the reticle, the nudge and the notice

Check("with the hands at capacity the carry list reports full", carry.IsFull, carry.Count + "/" + carry.Capacity);
var xhair = pi.crosshair; pi.NotifyHandsFull();
Check("a refused pickup nudges the stack, punches the reticle and raises the notice", carry.IsNudging && xhair != null && xhair.IsRejecting && pi.HandsFullNoticeShowing);
xhair.SetState(Crosshair.State.Idle); xhair.SetState(Crosshair.State.Blocked);
var dotCol = xhair.dot.color;
Check("the blocked reticle is grey (no colour) when the hands are full", System.Math.Abs(dotCol.r - dotCol.g) < 0.02f && System.Math.Abs(dotCol.g - dotCol.b) < 0.02f && dotCol.r < 0.8f, dotCol.ToString());
var hv0 = B("F1"); hv0.Outline.StopAll(); hv0.SetFocused(true, true); typeof(BookOutline).GetMethod("LateUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(hv0.Outline, null);
Check("the hover outline of a book that cannot be picked up is grey", System.Math.Abs(Book.UnavailableColor.r - Book.UnavailableColor.b) < 0.02f);
hv0.SetFocused(false);
// against a bookcase the stack is pulled back so it does not poke through
var shelfRow = zone.shelves[0]; var caseT = shelfRow.transform;
var front = caseT.position - caseT.forward * 0.0f;
pl.transform.position = V(-7.72f, 0f, 2.06f); pl.transform.rotation = UnityEngine.Quaternion.Euler(0f, 270f, 0f); UnityEngine.Physics.SyncTransforms(); carry.SnapStack();
var origin = pl.transform.TransformPoint(V(carry.carryPoint.localPosition.x, carry.carryPoint.localPosition.y, 0f));
float hitDist = float.MaxValue;
foreach (var h in UnityEngine.Physics.RaycastAll(origin, pl.transform.forward, 2f, ~0, UnityEngine.QueryTriggerInteraction.Ignore)) { if (h.collider.transform.IsChildOf(pl.transform)) continue; hitDist = UnityEngine.Mathf.Min(hitDist, h.distance); }
float farEdge = float.MinValue;
foreach (var b in carry.Items) { var r = b.GetComponent<UnityEngine.Renderer>().bounds; var f = pl.transform.forward; farEdge = UnityEngine.Mathf.Max(farEdge, UnityEngine.Vector3.Dot(r.center - origin, f) + UnityEngine.Mathf.Abs(f.x) * r.extents.x + UnityEngine.Mathf.Abs(f.y) * r.extents.y + UnityEngine.Mathf.Abs(f.z) * r.extents.z); }
Check("with a bookcase 0.3 m ahead the stack stops short of it", hitDist < 1.5f && farEdge <= hitDist, "hit " + hitDist + " farEdge " + farEdge);
pl.transform.position = V(-4.0f, 0f, 3.56f); UnityEngine.Physics.SyncTransforms(); carry.SnapStack();
float farOpen = float.MinValue; origin = pl.transform.TransformPoint(V(carry.carryPoint.localPosition.x, carry.carryPoint.localPosition.y, 0f));
foreach (var b in carry.Items) { var r = b.GetComponent<UnityEngine.Renderer>().bounds; var f = pl.transform.forward; farOpen = UnityEngine.Mathf.Max(farOpen, UnityEngine.Vector3.Dot(r.center - origin, f) + UnityEngine.Mathf.Abs(f.x) * r.extents.x + UnityEngine.Mathf.Abs(f.y) * r.extents.y + UnityEngine.Mathf.Abs(f.z) * r.extents.z); }
Check("in open floor the stack is out at full reach again", farOpen > 0.65f, farOpen.ToString());

// placing from the stack still works and the book returns to its real size
var placeBook = carry.Current; var slot = zone.shelves[0].slots[0];
var before = carry.RealScale(placeBook);
pi.PlaceCurrent(slot);
Check("placing the top book puts it in the slot at its real size", placeBook.currentSlot == slot && (placeBook.transform.localScale - before).magnitude < 0.001f && carry.Count == 3);
sb.Append(fails == 0 ? "ALL PASSED" : fails + " FAILED");
return sb.ToString();
