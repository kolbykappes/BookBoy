var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
var pl = UnityEngine.GameObject.Find("Player"); var carry = pl.GetComponent<CarryInventory>();
pl.transform.position = V(-4.0f, 0f, 3.56f); pl.transform.rotation = UnityEngine.Quaternion.Euler(0f, 270f, 0f);
UnityEngine.Physics.SyncTransforms();
foreach (var n in new[] { "A1", "A2", "A3", "B1", "B2", "C1" }) sb.Append(carry.TryPickUp(UnityEngine.GameObject.Find("R1_" + n).GetComponent<Book>()) ? "+" : "-");
carry.SnapStack();
var eye = pl.transform.position + V(0f, 1.65f, 0f); var fwd = pl.transform.forward;
sb.AppendLine(" count " + carry.Count);
sb.AppendLine(LamplitBuilder.LightStats("level     ", eye, eye + fwd * 3f, 58.7f, null, d + "H_level.png"));
sb.AppendLine(LamplitBuilder.LightStats("down 30   ", eye, eye + UnityEngine.Quaternion.AngleAxis(30f, pl.transform.right) * fwd * 3f, 58.7f, null, d + "H_down30.png"));
sb.AppendLine(LamplitBuilder.LightStats("down 55   ", eye, eye + UnityEngine.Quaternion.AngleAxis(55f, pl.transform.right) * fwd * 3f, 58.7f, null, d + "H_down55.png"));
// at the shelf: against the case, pulled back
pl.transform.position = V(-8.2f, 0f, 2.06f); UnityEngine.Physics.SyncTransforms(); carry.SnapStack();
eye = pl.transform.position + V(0f, 1.65f, 0f);
sb.AppendLine(LamplitBuilder.LightStats("at shelf  ", eye, eye + fwd * 3f + V(0f, -0.3f, 0f), 58.7f, null, d + "H_shelf.png"));
return sb.ToString();
