var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
string d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
foreach (var l in UnityEngine.Object.FindObjectsByType<UnityEngine.Light>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None)) if (l.transform.root.name == "Hub" && l.name == "OrbLight") { sb.Append(l.intensity + "/" + l.range + " "); }
sb.AppendLine();
sb.AppendLine(LamplitBuilder.LightStats("hub desk  ", V(0f, 1.65f, 2.2f), V(0f, 1.2f, 0.5f), 58.7f, null, d + "H_desk.png"));
sb.AppendLine(LamplitBuilder.LightStats("hub hall  ", V(0f, 1.65f, 2.2f), V(-1.5f, 1.4f, 7.75f), 58.7f, null, d + "H_hall.png"));
return sb.ToString();
