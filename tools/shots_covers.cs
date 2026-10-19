var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var zone = UnityEngine.GameObject.Find("Room1").GetComponent<RoomZone>();
zone.PlaceAllHome(); UnityEngine.Physics.SyncTransforms();
var sb = new System.Text.StringBuilder();
sb.AppendLine(LamplitBuilder.LightStats("close row ", V(-7.9f, 1.55f, 2.06f), V(-8.7f, 1.3f, 2.06f), 58.7f, null, d + "C_close.png"));
sb.AppendLine(LamplitBuilder.LightStats("4 ft      ", V(-7.2f, 1.65f, 2.06f), V(-8.7f, 1.3f, 2.06f), 58.7f, null, d + "C_4ft.png"));
sb.AppendLine(LamplitBuilder.LightStats("door      ", V(-3.2f, 1.65f, 3.56f), V(-9f, 1.4f, 3.56f), 58.7f, null, d + "C_door.png"));
return sb.ToString();
