// Edit Mode renders of the apron and the shoes on the find table and installed at the desk.
var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
var a = UnityEngine.GameObject.Find("Find_apron").transform.position; var s = UnityEngine.GameObject.Find("Find_shoes").transform.position;
var mid = (a + s) / 2f;
sb.AppendLine(LamplitBuilder.Shot(d + "FD_table.png", mid + V(1.3f, 0.7f, -0.5f), mid, 45f, 1600, 900, 0.1f));
sb.AppendLine(LamplitBuilder.Shot(d + "FD_table_top.png", mid + V(0.0f, 1.0f, -0.35f), mid, 45f, 1600, 900, 0.1f));
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>();
sb.AppendLine("install apron " + cc.InstallFind("apron") + " shoes " + cc.InstallFind("shoes"));
var desk = UnityEngine.GameObject.Find("Spot_apron_hook").transform.position;
sb.AppendLine(LamplitBuilder.Shot(d + "FD_desk.png", V(1.35f, 1.45f, 2.6f), V(0.95f, 1.05f, 0.8f), 42f, 1600, 900, 0.1f));
return sb.ToString();
