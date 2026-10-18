var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
sb.AppendLine(LamplitBuilder.Shot(d + "r3_top.png", V(-9.0f, 2.8f, 18.1f), V(-9.0f, 0f, 18.2f), 75f, 1386, 719));
sb.AppendLine(LamplitBuilder.Shot(d + "r3_wide.png", V(-3.6f, 1.15f, 18.1f), V(-12f, 0.3f, 18.1f), 75f, 1386, 719));
return sb.ToString();
