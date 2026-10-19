var sb = new System.Text.StringBuilder();
var zone = UnityEngine.GameObject.Find("Room1").GetComponent<RoomZone>();
var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
void Measure(string label, UnityEngine.Vector3 pos, UnityEngine.Vector3 look)
{
    LamplitBuilder.Shot(d + "stats_tmp.png", pos, look, 58.7f, 1386, 719);   // renders through a camera
    LamplitBuilder.Shot(d + "stats_tmp.png", pos, look, 58.7f, 1386, 719);
    sb.AppendLine(label + ": drawCalls=" + UnityEditor.UnityStats.drawCalls + " setPass=" + UnityEditor.UnityStats.setPassCalls + " tris=" + UnityEditor.UnityStats.triangles + " verts=" + UnityEditor.UnityStats.vertices + " shadowCasters=" + UnityEditor.UnityStats.shadowCasters);
}
int tris = 0, rends = 0; foreach (var b in zone.items) { var mf = b.GetComponent<UnityEngine.MeshFilter>(); tris += mf.sharedMesh.triangles.Length / 3; rends++; }
sb.AppendLine("Room 1 items: " + rends + " books, " + tris + " triangles total (" + (tris / rends) + " each), submeshes per book " + zone.items[0].GetComponent<UnityEngine.MeshFilter>().sharedMesh.subMeshCount);
Measure("piled, standing at the door", new UnityEngine.Vector3(-2.2f, 1.65f, 3.56f), new UnityEngine.Vector3(-9f, 1.0f, 3.56f));
zone.PlaceAllHome();
Measure("all shelved, wall view", new UnityEngine.Vector3(-5.0f, 1.65f, 3.56f), new UnityEngine.Vector3(-9f, 1.2f, 3.56f));
Measure("all shelved, 4 ft from the triple", new UnityEngine.Vector3(-7.35f, 1.65f, 2.06f), new UnityEngine.Vector3(-8.7f, 1.2f, 2.06f));
return sb.ToString();
