var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder(); float fov = 58.7f;
var zone = UnityEngine.GameObject.Find("Room1").GetComponent<RoomZone>();
zone.PlaceAllHome();
// case front: container x = -9 + 0.1 + 0.165 = -8.735, front at about -8.57; triple centre z = 3.5625 - 1.5
sb.AppendLine(LamplitBuilder.Shot(d + "m_triple_4ft.png", V(-7.35f, 1.65f, 2.06f), V(-8.7f, 1.2f, 2.06f), fov, 1386, 719));
sb.AppendLine(LamplitBuilder.Shot(d + "m_row_close.png", V(-7.9f, 1.55f, 2.06f), V(-8.7f, 1.35f, 2.06f), fov, 1386, 719));
sb.AppendLine(LamplitBuilder.Shot(d + "m_wide.png", V(-5.0f, 1.65f, 3.56f), V(-9f, 1.2f, 3.56f), fov, 1386, 719));
// a single book from above and the side to inspect the mesh (spine view, then 3/4 view)
var b = zone.items[0].transform.position;
sb.AppendLine(LamplitBuilder.Shot(d + "m_solo_5ft.png", V(-7.0f, 1.65f, 5.46f), V(-8.7f, 1.0f, 5.46f), fov, 1386, 719));
return sb.ToString();
