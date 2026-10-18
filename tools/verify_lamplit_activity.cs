// Play Mode check: rooms beyond a shut door are switched off (no objects, renderers, colliders or lights live) until the door opens.
// Run with Lamplit.unity open and playing, in a fresh Play session.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != "Assets/Scenes/Lamplit.unity") return "REFUSED: Lamplit.unity must be the active scene";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var cc = UnityEngine.Object.FindAnyObjectByType<CampaignController>();
if (cc.RoomCount != 3) return "FAIL: all three rooms must be built";
int LiveRenderers() { int n = 0; foreach (var r in UnityEngine.Object.FindObjectsByType<UnityEngine.Renderer>(UnityEngine.FindObjectsInactive.Exclude)) n++; return n; }
int LiveColliders() { return UnityEngine.Object.FindObjectsByType<UnityEngine.Collider>(UnityEngine.FindObjectsInactive.Exclude).Length; }
int LiveLights() { int n = 0; foreach (var l in UnityEngine.Object.FindObjectsByType<UnityEngine.Light>(UnityEngine.FindObjectsInactive.Exclude)) if (l.enabled) n++; return n; }
int LiveBooks() { return UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Exclude).Length; }

// ---- at the start: only the hub and Room 1 are alive ----
Check("Room 1 is on (its door is open); Rooms 2 and 3 are switched off", cc.IsRoomActive(1) && !cc.IsRoomActive(2) && !cc.IsRoomActive(3));
int books0 = LiveBooks(), rend0 = LiveRenderers(), col0 = LiveColliders(), light0 = LiveLights();
Check("only Room 1's 100 items (and its 2 finds) are live books: 102, not 345", books0 == 102, books0.ToString());
Check("Rooms 2 and 3 objects are not live: renderers, colliders and lights are well below the whole-scene totals", rend0 < 650 && col0 < 400 && light0 < 12, "renderers=" + rend0 + " colliders=" + col0 + " lights=" + light0);
sb.Append("live at the start: books=" + books0 + " renderers=" + rend0 + " colliders=" + col0 + " lights=" + light0 + "\n");
Check("a switched-off room's items are not found by the abilities (they search live books only)", System.Array.IndexOf(UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Exclude), cc.Room(2).items[0]) < 0);

// ---- finishing Room 1 opens Room 2's door and switches the room on ----
cc.CompleteRoom(1); cc.Recompute();
Check("tidying Room 1 opens Room 2's door AND switches Room 2 on; Room 3 stays off", cc.State.IsDoorOpen(2) && cc.IsRoomActive(2) && !cc.IsRoomActive(3));
Check("Room 2's items are live now (102 + 122)", LiveBooks() == 102 + 122, LiveBooks().ToString());
cc.CompleteRoom(2); cc.Recompute();
Check("tidying Room 2 switches Room 3 on", cc.State.IsDoorOpen(3) && cc.IsRoomActive(3));

// ---- admin: reset everything, then jump into a shut room ----
cc.ResetAll();
Check("after 'reset everything' the doors follow the rule again but rooms already opened stay on (their piles are dropping)", !cc.State.IsDoorOpen(2) && cc.IsRoomActive(2) && cc.IsRoomActive(3));
sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
