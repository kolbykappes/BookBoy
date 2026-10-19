// Measures Room 1's lighting from three views with the game's post-processing. Puts every book home first (the caller rebuilds Room 1 afterwards).
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var zone = UnityEngine.GameObject.Find("Room1").GetComponent<RoomZone>();
zone.PlaceAllHome();
UnityEngine.Physics.SyncTransforms();
UnityEngine.Vector3 P(string n) { return UnityEngine.GameObject.Find(n).transform.position; }
float cx = -8.585f; // the case front plane (board edges)
var probes = new[] {
    P("R1_E4"), P("R1_C4"), P("R1_A4"), P("R1_G4"),                       // spines of four sets in the triple's middle bay
    V(cx, 0.85f - 0.03f, 2.06f), V(cx, 1.32f - 0.03f, 2.06f),             // the board fronts under two rows
    V(-8.89f, 1.5f, 3.9f),                                               // the far wall between the cases
    V(-6.0f, 0.02f, 2.8f)                                                // the floor under a lamp
};
var sb = new System.Text.StringBuilder();
string d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
string tag = System.IO.File.Exists("C:/Users/angry/AppData/Local/Temp/light_tag.txt") ? System.IO.File.ReadAllText("C:/Users/angry/AppData/Local/Temp/light_tag.txt").Trim() : "x";
sb.AppendLine(LamplitBuilder.LightStats("door view   ", V(-3.2f, 1.65f, 3.56f), V(-9f, 1.4f, 3.56f), 58.7f, probes, d + "L_" + tag + "_door.png"));
sb.AppendLine(LamplitBuilder.LightStats("4 ft triple ", V(-7.2f, 1.65f, 2.06f), V(-8.7f, 1.3f, 2.06f), 58.7f, probes, d + "L_" + tag + "_triple.png"));
sb.AppendLine(LamplitBuilder.LightStats("solo corner ", V(-6.9f, 1.65f, 4.0f), V(-8.7f, 1.2f, 5.46f), 58.7f, probes, d + "L_" + tag + "_solo.png"));
return sb.ToString();
