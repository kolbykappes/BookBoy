// Edit Mode check of the apron and shoe models (tools/blender/make_finds.py) and how they sit in the scene (Lamplit.unity open, Room 1 and the hub built).
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
foreach (var id in new[] { "apron", "shoes" })
{
    var model = LamplitBuilder.LoadFindModel(id);
    if (model == null) { Check(id + ": model imported", false, "LoadFindModel returned null (run make_finds.py and SetupFindAssets)"); continue; }
    var m = model.Mesh; var b = m.bounds;
    Check(id + ": a unit box like the tomes (the game scales it by its real size)", System.Math.Abs(b.size.x - 1f) < 0.02f && System.Math.Abs(b.size.y - 1f) < 0.02f && System.Math.Abs(b.size.z - 1f) < 0.02f && b.center.magnitude < 0.02f, b.ToString());
    Check(id + ": seven submeshes and seven materials in the same order", m.subMeshCount == 7 && model.Materials.Length == 7 && System.Array.TrueForAll(model.Materials, x => x != null), m.subMeshCount + " / " + model.Materials.Length);
    Check(id + ": readable (the hover outline builds a hull from it) and has UVs and normals", m.isReadable && m.uv.Length == m.vertexCount && m.normals.Length == m.vertexCount);
    var v = m.vertices;
    Bounds SubBounds(int i) { var bb = new Bounds(); bool first = true; foreach (var t in m.GetTriangles(i)) { if (first) { bb = new Bounds(v[t], UnityEngine.Vector3.zero); first = false; } else bb.Encapsulate(v[t]); } return bb; }
    if (id == "apron")
    {
        Check("apron: submesh 0 is the canvas (covers most of the length), submesh 5 the paper tag (small), submesh 3 the brass buttons (small)", SubBounds(0).size.y > 0.7f && SubBounds(5).size.y < 0.2f && SubBounds(3).size.y < 0.2f, SubBounds(0).size.y + " / " + SubBounds(5).size.y + " / " + SubBounds(3).size.y);
        Check("apron: lies flat (thin in x), face toward +x", model.Size.x < 0.05f && model.Size.y > 0.4f && model.Size.z > 0.3f, model.Size.ToString());
    }
    else
    {
        Check("shoes: submesh 1 is the felt sole at the bottom (x), submesh 0 the leather upper above it", SubBounds(1).max.x < SubBounds(0).max.x && SubBounds(1).center.x < SubBounds(0).center.x, SubBounds(1).center.x + " / " + SubBounds(0).center.x);
        Check("shoes: a pair about 31 by 22 cm and about 10 cm high", model.Size.y > 0.25f && model.Size.y < 0.35f && model.Size.z > 0.18f && model.Size.z < 0.26f && model.Size.x > 0.08f && model.Size.x < 0.13f, model.Size.ToString());
    }
}

// in the scene
var a = UnityEngine.GameObject.Find("Find_apron"); var s = UnityEngine.GameObject.Find("Find_shoes");
if (a == null || s == null) { sb.AppendLine("FAIL no finds in the scene"); fails++; }
else
{
    var ab = a.GetComponent<UnityEngine.Renderer>().bounds; var sbd = s.GetComponent<UnityEngine.Renderer>().bounds;
    Check("both finds use their model (seven materials), not the cube proxy, and open the discovery dialog", a.GetComponent<UnityEngine.MeshRenderer>().sharedMaterials.Length == 7 && s.GetComponent<UnityEngine.MeshRenderer>().sharedMaterials.Length == 7 && a.GetComponent<Book>().definition.discoveryDialog && s.GetComponent<Book>().definition.discoveryDialog);
    Check("the apron lies flat on the table (under 6 cm thick) and the shoes stand on their soles (about 10 cm)", ab.size.y < 0.06f && sbd.size.y > 0.07f && sbd.size.y < 0.13f, ab.size.y + " / " + sbd.size.y);
    var table = UnityEngine.GameObject.Find("FindTable"); float top = 0.93f;
    Check("both rest ON the table top (not sunk into it, not floating)", System.Math.Abs(ab.min.y - top) < 0.004f && System.Math.Abs(sbd.min.y - top) < 0.004f, ab.min.y + " / " + sbd.min.y + " vs " + top);
    var apronCol = a.GetComponent<UnityEngine.BoxCollider>(); var shoeCol = s.GetComponent<UnityEngine.BoxCollider>();
    Check("each has a box collider (a trigger) the size of its model", apronCol != null && apronCol.isTrigger && shoeCol != null && shoeCol.isTrigger);
}
sb.Append(fails == 0 ? "ALL PASSED" : fails + " FAILED");
return sb.ToString();
