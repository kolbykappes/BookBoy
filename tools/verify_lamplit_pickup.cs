// Regression check (2026-10-19): the player's own colliders must never hide what the camera is aimed at. Run with Lamplit.unity playing.
if (!UnityEditor.EditorApplication.isPlaying) return "enter Play first";
var sb = new System.Text.StringBuilder();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>();
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var zone = cc.Room(1);
var cam = pi.interactionCamera.transform;
var focusField = typeof(PlayerInteractor).GetField("_focusedBook", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var upd = typeof(PlayerInteractor).GetMethod("UpdateFocus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
var eye = new UnityEngine.Vector3(pl.transform.position.x, pl.transform.position.y + 1.56f, pl.transform.position.z);
sb.AppendLine("player " + pl.transform.position + " eye " + eye + " camera is at " + cam.position);
var offs = new[] { new UnityEngine.Vector3(-1.0f, 0.05f, 0f), new UnityEngine.Vector3(-0.6f, 0.05f, 0.5f), new UnityEngine.Vector3(0f, 0.05f, 1.2f), new UnityEngine.Vector3(-0.5f, 0.05f, -0.5f), new UnityEngine.Vector3(-1.2f, 1.0f, 0f) };
int ok = 0;
for (int i = 0; i < offs.Length; i++)
{
    var b = zone.items[i];
    b.transform.position = pl.transform.position + offs[i]; b.transform.rotation = UnityEngine.Quaternion.identity;
    cam.position = eye; cam.rotation = UnityEngine.Quaternion.LookRotation((b.transform.position - eye).normalized);
    UnityEngine.Physics.SyncTransforms();
    upd.Invoke(pi, null);
    var f = focusField.GetValue(pi) as Book;
    var ray = new UnityEngine.Ray(cam.position, cam.forward);
    var hits = UnityEngine.Physics.RaycastAll(ray, 12f, ~0, UnityEngine.QueryTriggerInteraction.Collide); System.Array.Sort(hits, (a, c) => a.distance.CompareTo(c.distance));
    string first = ""; for (int k = 0; k < hits.Length && k < 3; k++) first += hits[k].collider.name + "@" + hits[k].distance.ToString("F2") + " ";
    if (f == b) ok++;
    sb.AppendLine(b.name + " offset " + offs[i] + " focused=" + (f != null ? f.name : "null") + " hits: " + first);
}
sb.AppendLine((ok == offs.Length ? "PASS" : "FAIL") + " the aim ray from the real eye position (inside the player colliders) focuses a book on the floor in every direction: " + ok + " of " + offs.Length);
sb.Append(ok == offs.Length ? "ALL PASSED" : "1 FAILED");
return sb.ToString();
