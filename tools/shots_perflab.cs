var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
var c = UnityEngine.Object.FindFirstObjectByType<PerfLabController>();
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var T = typeof(PerfLabController);
T.GetMethod("SpawnBooks", flags).Invoke(c, null);
T.GetMethod("PrepareMaterials", flags).Invoke(c, null);
void Set(int idx, int preset) { c.countIndex = idx; c.lightingPreset = preset; T.GetMethod("ApplyBooks", flags).Invoke(c, null); T.GetMethod("ApplyLighting", flags).Invoke(c, null); }
Set(2, 2);
sb.AppendLine(LamplitBuilder.Shot(d + "pl_1000_from_start.png", V(0f, 1.65f, -11f), V(0f, 0.6f, 0f), 58.7f, 1386, 719));
sb.AppendLine(LamplitBuilder.Shot(d + "pl_1000_close.png", V(0f, 1.65f, -3.2f), V(0f, 0.3f, 0f), 58.7f, 1386, 719));
Set(3, 2);
sb.AppendLine(LamplitBuilder.Shot(d + "pl_3000_from_start.png", V(0f, 1.65f, -11f), V(0f, 0.6f, 0f), 58.7f, 1386, 719));
Set(2, 4);
sb.AppendLine(LamplitBuilder.Shot(d + "pl_1000_manysmall.png", V(0f, 1.65f, -11f), V(0f, 0.6f, 0f), 58.7f, 1386, 719));
Set(2, 5);
sb.AppendLine(LamplitBuilder.Shot(d + "pl_1000_directional.png", V(0f, 1.65f, -11f), V(0f, 0.6f, 0f), 58.7f, 1386, 719));
// clean up: remove the spawned books and put the lights back to the saved state
var sp = c.transform.Find("SpawnedBooks"); if (sp != null) UnityEngine.Object.DestroyImmediate(sp.gameObject);
c.books = null; c.countIndex = 2; c.lightingPreset = 2;
T.GetMethod("ApplyLighting", flags).Invoke(c, null);
return sb.ToString();
