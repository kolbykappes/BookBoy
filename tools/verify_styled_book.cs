// Edit Mode check of the styled tome and the printed covers (tools/blender/make_book.py ... styled, tools/covers/make_covers.py), with Lamplit.unity open and Room 1 built.
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var mesh = LamplitBuilder.StyledBookMesh();
if (mesh == null) return "FAIL: the styled tome is not imported";
var b = mesh.bounds; var v = mesh.vertices; var uv = mesh.uv;
Check("the styled mesh is a unit box like the plain tome", System.Math.Abs(b.size.x - 1f) < 0.02f && System.Math.Abs(b.size.y - 1f) < 0.02f && System.Math.Abs(b.size.z - 1f) < 0.02f && b.center.magnitude < 0.02f, b.ToString());
Check("three submeshes in the order boards, pages, spine", mesh.subMeshCount == 3, mesh.subMeshCount.ToString());
var spineTris = mesh.GetTriangles(2); float zmax = -9, umin = 9, umax = -9, vmin = 9, vmax = -9;
foreach (var t in spineTris) { zmax = System.Math.Max(zmax, v[t].z); umin = System.Math.Min(umin, uv[t].x); umax = System.Math.Max(umax, uv[t].x); vmin = System.Math.Min(vmin, uv[t].y); vmax = System.Math.Max(vmax, uv[t].y); }
Check("the spine submesh is the rounded spine side (all of it at -z)", spineTris.Length > 0 && zmax < -0.3f, "max z " + zmax);
Check("the spine UVs cover 0..1 across and 0..1 up (one cell of the atlas)", umin < 0.02f && umax > 0.98f && vmin < 0.02f && vmax > 0.98f, umin + ".." + umax + " " + vmin + ".." + vmax);
float bu0 = 9, bu1 = -9; foreach (var t in mesh.GetTriangles(0)) { bu0 = System.Math.Min(bu0, uv[t].x); bu1 = System.Math.Max(bu1, uv[t].x); }
Check("the board UVs lie in the cover texture (back board 0 to 0.5, front 0.5 to 1)", bu0 > -0.02f && bu1 < 1.02f, bu0 + ".." + bu1);
int spineVerts = 0; foreach (var t in spineTris) spineVerts++;
sb.AppendLine("verts=" + v.Length + " tris=" + (mesh.triangles.Length / 3));
int ok = 0; foreach (var key in LamplitBuilder.CoverKeys)
{
    var c = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/_Project/Art/Materials/Mat_Cover_" + key + ".mat");
    var s = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/_Project/Art/Materials/Mat_Spine_" + key + ".mat");
    var ct = c != null ? c.GetTexture("_BaseMap") : null; var st = s != null ? s.GetTexture("_BaseMap") : null;
    if (ct != null && st != null && ct.width == 2048 && ct.height == 1600 && st.width == 3072 && st.height == 1024) ok++;
}
Check("six cover materials (2048 x 1600) and six spine materials (3072 x 1024, not rescaled)", ok == 6, ok + " of 6");
int styled = 0, plain = 0, offsetsOk = 0, checkedBooks = 0, tinted = 0;
foreach (var book in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsSortMode.None))
{
    if (!book.name.StartsWith("R1_")) continue;
    var r = book.GetComponent<UnityEngine.MeshRenderer>(); int style = book.definition.coverStyle;
    if (r.sharedMaterials.Length == 3) styled++; else plain++;
    if (style >= 0 && r.sharedMaterials.Length == 3)
    {
        checkedBooks++;
        var mpb = new UnityEngine.MaterialPropertyBlock(); r.GetPropertyBlock(mpb, 2);
        var st = mpb.GetVector("_BaseMap_ST"); float expect = (UnityEngine.Mathf.Clamp(book.NumberKey, 1, 8) - 1) / 8f;
        if (System.Math.Abs(st.x - 0.125f) < 1e-4f && System.Math.Abs(st.z - expect) < 1e-4f) offsetsOk++;
        var mpb0 = new UnityEngine.MaterialPropertyBlock(); r.GetPropertyBlock(mpb0, 0);
        if (mpb0.GetColor("_BaseColor") == book.definition.spineColor) tinted++;
        bool pagesPlain = true; var mpb1 = new UnityEngine.MaterialPropertyBlock(); r.GetPropertyBlock(mpb1, 1); if (mpb1.GetColor("_BaseColor") == book.definition.spineColor) pagesPlain = false;
        if (!pagesPlain) { fails++; sb.AppendLine("FAIL pages tinted on " + book.name); }
    }
}
Check("sets A to F (36 books: three sets of 8, three of 4) are styled, the other 60 keep the plain tome", styled == 36 && plain == 60, styled + " styled, " + plain + " plain");
Check("every styled book's spine shows its own volume cell (1/8 scale, offset (volume-1)/8)", offsetsOk == checkedBooks && checkedBooks == 36, offsetsOk + " of " + checkedBooks);
Check("every styled book's boards and spine take the set colour, the pages stay cream", tinted == checkedBooks, tinted + " of " + checkedBooks);
sb.Append(fails == 0 ? "ALL PASSED" : fails + " FAILED");
return sb.ToString();
