var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
var zone = UnityEngine.GameObject.Find("Room1").GetComponent<RoomZone>();
zone.PlaceAllHome(); UnityEngine.Physics.SyncTransforms();
sb.AppendLine(LamplitBuilder.Shot(d + "SA_shelves.png", V(-7.2f, 1.65f, 2.06f), V(-8.7f, 1.2f, 2.06f), 58.7f, 1920, 1080, 0.1f));
// a standalone book held up front-cover first
var g = UnityEngine.GameObject.Find("R1_G3").GetComponent<Book>(); if (g.currentSlot != null) g.currentSlot.Release();
g.transform.position = V(-6.0f, 1.45f, 3.56f); g.transform.rotation = UnityEngine.Quaternion.identity; UnityEngine.Physics.SyncTransforms();
sb.AppendLine(LamplitBuilder.Shot(d + "SA_front.png", V(-5.2f, 1.45f, 3.56f), V(-6.0f, 1.45f, 3.56f), 40f, 1280, 900, 0.1f));
return sb.ToString();
