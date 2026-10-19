if (!UnityEditor.EditorApplication.isPlaying) return "enter Play first";
var sb = new System.Text.StringBuilder();
var pl = UnityEngine.GameObject.Find("Player"); var ctrl = pl.GetComponent<UnityEngine.CharacterController>();
sb.AppendLine("skin=" + ctrl.skinWidth + " step=" + ctrl.stepOffset + " slope=" + ctrl.slopeLimit + " minMove=" + ctrl.minMoveDistance + " layer=" + pl.layer + " detectCollisions=" + ctrl.detectCollisions);
var o = new UnityEngine.Vector3(pl.transform.position.x, 1.65f, pl.transform.position.z);
string Test(string label) { UnityEngine.Physics.SyncTransforms(); var hits = UnityEngine.Physics.RaycastAll(new UnityEngine.Ray(o, UnityEngine.Vector3.left), 5f, ~0, UnityEngine.QueryTriggerInteraction.Collide); string s = label + ": "; foreach (var h in hits) s += h.collider.name + "@" + h.distance.ToString("F2") + " "; return s; }
sb.AppendLine(Test("current h=" + ctrl.height + " r=" + ctrl.radius + " c=" + ctrl.center.y));
float h0 = ctrl.height, r0 = ctrl.radius; var c0 = ctrl.center;
ctrl.height = 2f; ctrl.radius = 0.5f; ctrl.center = new UnityEngine.Vector3(0, 0.93f, 0); sb.AppendLine(Test("old 2.0 / 0.5 / 0.93"));
ctrl.height = 1.8f; ctrl.radius = 0.3f; ctrl.center = new UnityEngine.Vector3(0, 0.83f, 0); sb.AppendLine(Test("new 1.8 / 0.3 / 0.83"));
ctrl.height = 1.8f; ctrl.radius = 0.3f; ctrl.center = new UnityEngine.Vector3(0, 0.9f, 0); sb.AppendLine(Test("1.8 / 0.3 / 0.9"));
ctrl.height = 1.8f; ctrl.radius = 0.35f; ctrl.center = new UnityEngine.Vector3(0, 0.83f, 0); sb.AppendLine(Test("1.8 / 0.35 / 0.83"));
ctrl.height = 2.0f; ctrl.radius = 0.3f; ctrl.center = new UnityEngine.Vector3(0, 0.93f, 0); sb.AppendLine(Test("2.0 / 0.3 / 0.93"));
ctrl.height = h0; ctrl.radius = r0; ctrl.center = c0;
// does the camera root sit where I think
var root = pl.transform.Find("PlayerCameraRoot"); sb.AppendLine("cameraRoot world y=" + root.position.y.ToString("F2") + " local=" + root.localPosition.y.ToString("F2") + " bounds=" + ctrl.bounds);
return sb.ToString();
