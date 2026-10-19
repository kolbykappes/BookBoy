// Edit Mode check of the imported bookcase meshes (tools/blender/make_cases.py) against LamplitBuilder's constants.
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
sb.AppendLine(LamplitBuilder.SetupCaseAssets());
float widthSolo = LamplitBuilder.CaseWidth(8), widthTriple = LamplitBuilder.CaseWidth(8) + 2f * LamplitBuilder.CaseWidth(4), widthWing = LamplitBuilder.CaseWidth(4);
foreach (var kv in new[] { ("Case_Solo", widthSolo), ("Case_Triple", widthTriple), ("Case_Wing", widthWing) })
{
    var m = LamplitBuilder.CaseMesh(kv.Item1);
    if (m == null) { Check(kv.Item1 + " is imported", false); continue; }
    var b = m.bounds; var v = m.vertices;
    // the cornice overhangs the case by 0.065 m on each side and 0.075 m at the front
    Check(kv.Item1 + ": width = the case width plus the cornice overhang (" + (kv.Item2 + 0.13f).ToString("F3") + ")", System.Math.Abs(b.size.x - (kv.Item2 + 0.13f)) < 0.01f, b.size.x.ToString("F3"));
    Check(kv.Item1 + ": the front is toward -z (min z about -(depth/2 + 0.075), max z about depth/2 + 0.01)", System.Math.Abs(b.min.z + (LamplitBuilder.CaseDepth / 2f + 0.075f)) < 0.01f && System.Math.Abs(b.max.z - (LamplitBuilder.CaseDepth / 2f + 0.01f)) < 0.01f, b.min.z.ToString("F3") + " .. " + b.max.z.ToString("F3"));
    Check(kv.Item1 + ": stands on the floor and clears the ceiling (0 to under 2.7 m)", b.min.y > -0.001f && b.max.y < 2.70f, b.min.y.ToString("F3") + " .. " + b.max.y.ToString("F3"));
    Check(kv.Item1 + ": three submeshes (wood, back, brass)", m.subMeshCount == 3, m.subMeshCount.ToString());
    if (m.subMeshCount == 3)
    {
        float backMin = 9f, brassMax = -9f;
        foreach (var t in m.GetTriangles(1)) backMin = System.Math.Min(backMin, v[t].z);
        foreach (var t in m.GetTriangles(2)) brassMax = System.Math.Max(brassMax, v[t].z);
        Check(kv.Item1 + ": submesh 1 is the back panel (all at the back, z > 0.1) and submesh 2 the brass (all at the front, z < -0.1)", backMin > 0.1f && brassMax < -0.1f, "back min z " + backMin.ToString("F3") + ", brass max z " + brassMax.ToString("F3"));
    }
    for (int i = 0; i < LamplitBuilder.ShelfRows; i++)
    {
        float top = LamplitBuilder.BaseHeight + i * LamplitBuilder.ShelfSpacing; int n = 0;
        foreach (var p in v) if (System.Math.Abs(p.y - top) < 0.002f) n++;
        Check(kv.Item1 + ": a board top at " + top.ToString("F2") + " m (shelf " + (i + 1) + ")", n >= 4, n + " vertices");
    }
    sb.AppendLine(kv.Item1 + " verts=" + v.Length + " tris=" + (m.triangles.Length / 3) + " bounds=" + b);
}
sb.Append(fails == 0 ? "ALL PASSED" : fails + " FAILED");
return sb.ToString();
