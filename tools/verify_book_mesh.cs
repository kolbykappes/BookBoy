// Edit Mode check of the imported tome mesh (tools/blender/make_book.py): a unit box, two submeshes, spine toward -z, sane normals and UVs.
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
sb.AppendLine(LamplitBuilder.SetupBookAssets());
var mesh = LamplitBuilder.BookMesh();
if (mesh == null) return "FAIL: no mesh imported";
var b = mesh.bounds;
Check("the mesh is a unit box (about 1 x 1 x 1) so the item scale is its size in metres", System.Math.Abs(b.size.x - 1f) < 0.02f && System.Math.Abs(b.size.y - 1f) < 0.02f && System.Math.Abs(b.size.z - 1f) < 0.02f && b.center.magnitude < 0.02f, b.ToString());
Check("two submeshes: cover and pages", mesh.subMeshCount == 2, mesh.subMeshCount.ToString());
var v = mesh.vertices; var n = mesh.normals;
// the rounded spine side has many more vertices (the two arcs) than the fore-edge side
int minus = 0, plus = 0; foreach (var p in v) { if (p.z < -0.35f) minus++; else if (p.z > 0.35f) plus++; }
Check("the rounded spine is at -z (the slot front line): more vertices near -z than +z", minus > plus + 10, minus + " near -z, " + plus + " near +z");
// the spine apex (z about -0.5, x about 0) normal points -z
int apex = -1; float best = 1e9f; for (int i = 0; i < v.Length; i++) { float d = v[i].z + 0.5f; if (v[i].y > 0.3f && System.Math.Abs(v[i].x) < 0.3f && d < best) { best = d; apex = i; } }
Check("the spine normal points out of the spine (-z)", apex >= 0 && n[apex].z < -0.5f, apex >= 0 ? v[apex] + " n=" + n[apex] : "none");
// pages submesh is inside: its vertices are all within the cover's x extent and not at the outer spine
var pagesTris = mesh.GetTriangles(1); float pxMin = 9, pxMax = -9; foreach (var t in pagesTris) { pxMin = System.Math.Min(pxMin, v[t].x); pxMax = System.Math.Max(pxMax, v[t].x); }
Check("the page block sits inside the covers (x within about +-0.45 of the 4 in thickness)", pxMax < 0.46f && pxMin > -0.46f, pxMin + ".." + pxMax);
var uv = mesh.uv; float umin = 9, umax = -9, vmin = 9, vmax = -9;
foreach (var t in mesh.GetTriangles(0)) { var q = uv[t]; umin = System.Math.Min(umin, q.x); umax = System.Math.Max(umax, q.x); vmin = System.Math.Min(vmin, q.y); vmax = System.Math.Max(vmax, q.y); }
sb.AppendLine("cover uv range u " + umin + ".." + umax + " v " + vmin + ".." + vmax);
Check("UVs exist for every vertex and the cover's lie in about 0..1 (the page block repeats its stripe 6 times)", uv.Length == v.Length && umin > -0.08f && umax < 1.08f && vmin > -0.01f && vmax < 1.01f);
Check("tangents were calculated (normal map)", mesh.tangents != null && mesh.tangents.Length == v.Length);
sb.AppendLine("verts=" + v.Length + " tris=" + (mesh.triangles.Length / 3) + " bounds=" + b);
sb.Append(fails == 0 ? "ALL PASSED" : fails + " FAILED");
return sb.ToString();
