"""Generates the BookBoy tome mesh and exports it as FBX for Unity.

Run:  blender -b --factory-startup -P tools/blender/make_book.py -- <output.fbx>

The mesh is a hardcover book with a rounded spine, two boards, an inset page block and a small bevel. It is built at real size
(thickness 4 in, depth 10 in, nominal height 14 in) and then NORMALISED to a 1 x 1 x 1 box, because the game scales every book by
(thickness, height, depth) (the old cube did the same) and the slot code positions a book by that scale. Because thickness and depth
are the same for every book, the spine rounding stays exact; only the top and bottom edge bevels vary with the book height (about 15 percent).

Axes in the file (Blender, Z up): X = thickness, Z = height, Y = depth, spine at -Y. Unity should see x = thickness, y = height, spine toward -z
(checked in Unity after import: see tools/verify_book_mesh.cs).
Material slot 0 = cover (tinted per set in the game), slot 1 = pages (cream, never tinted).
UVs: cover u 0..0.30 back board, 0.30..0.70 spine, 0.70..1.0 front board, v up the height; inner faces and end caps sit on a plain strip.
Pages: u across the thickness (page lines), v up the height or along the depth.
"""
import bmesh
import bpy
import math
import sys

# ---- real-size parameters, metres ----
T = 0.1016        # 4 in thickness (the whole book, boards included)
H = 0.356         # 14 in nominal height
D = 0.254         # 10 in depth
BT = 0.006        # board thickness
ST = 0.014        # spine wall thickness (floor of the cavity)
R = 0.025         # radius of the two rounded spine corners
INS_Z = 0.005     # page block inset from top and bottom
INS_D = 0.006     # page block inset from the fore-edge
GAP = 0.001       # air between the page block and the boards
ARC = 6           # segments per rounded corner
BEVEL = 0.0015    # board edge bevel

argv = sys.argv
out_path = argv[argv.index("--") + 1] if "--" in argv else "Book_Tome.fbx"

# ---- cover profile in the (x, d) plane, counter-clockwise, d = 0 at the spine ----
pts = []   # (x, d)
lab = []   # label of the edge STARTING at this point
pts.append((-T / 2, D)); lab.append("back")
pts.append((-T / 2, R)); lab.append("spine")
for k in range(1, ARC + 1):                       # left corner: angle 180 -> 270
    a = math.radians(180 + 90 * k / ARC)
    pts.append((-T / 2 + R + R * math.cos(a), R + R * math.sin(a))); lab.append("spine")
# the last point of the left arc is (-T/2+R, 0); the flat to the right corner start is next
pts.append((T / 2 - R, 0.0)); lab.append("spine")
for k in range(1, ARC):                           # right corner: angle 270 -> 360 (the last point is added below)
    a = math.radians(270 + 90 * k / ARC)
    pts.append((T / 2 - R + R * math.cos(a), R + R * math.sin(a))); lab.append("spine")
pts.append((T / 2, R)); lab.append("front")
pts.append((T / 2, D)); lab.append("inner")
pts.append((T / 2 - BT, D)); lab.append("inner")
pts.append((T / 2 - BT, ST)); lab.append("inner")
pts.append((-T / 2 + BT, ST)); lab.append("inner")
pts.append((-T / 2 + BT, D)); lab.append("inner")   # edge back to point 0
n = len(pts)

# cumulative length along the spine edges, for u
spine_len = 0.0
spine_s = {}
for i in range(n):
    if lab[i] == "spine":
        x0, d0 = pts[i]; x1, d1 = pts[(i + 1) % n]
        spine_s[i] = (spine_len, spine_len + math.hypot(x1 - x0, d1 - d0))
        spine_len += math.hypot(x1 - x0, d1 - d0)


def u_of(i, t):
    """u at parameter t (0 at the edge's start point, 1 at its end) along edge i."""
    kind = lab[i]
    if kind == "back":
        return 0.30 * t                      # fore-edge (d = D) -> spine start (d = R)
    if kind == "spine":
        s0, s1 = spine_s[i]
        return 0.30 + 0.40 * (s0 + (s1 - s0) * t) / spine_len
    if kind == "front":
        return 0.70 + 0.30 * t               # spine -> fore-edge
    return 0.005                             # inner faces: plain strip


def u_vertex(i):
    return u_of(i, 0.0) if lab[i] != "inner" else 0.005


bm = bmesh.new()
uv = bm.loops.layers.uv.new("UVMap")
z0, z1 = -H / 2, H / 2
bot = [bm.verts.new((x, d - D / 2, z0)) for (x, d) in pts]
top = [bm.verts.new((x, d - D / 2, z1)) for (x, d) in pts]
bm.verts.ensure_lookup_table()

faces_cover = []
for i in range(n):
    j = (i + 1) % n
    f = bm.faces.new((bot[i], bot[j], top[j], top[i]))
    f.material_index = 0
    ua, ub = u_of(i, 0.0), u_of(i, 1.0)
    for loop in f.loops:
        v = loop.vert
        if v == bot[i]: loop[uv].uv = (ua, 0.0)
        elif v == bot[j]: loop[uv].uv = (ub, 0.0)
        elif v == top[j]: loop[uv].uv = (ub, 1.0)
        else: loop[uv].uv = (ua, 1.0)
    faces_cover.append(f)

ftop = bm.faces.new(top)                           # counter-clockwise seen from +Z: normal up
fbot = bm.faces.new(list(reversed(bot)))           # reversed: normal down
for f, vv in ((ftop, 1.0), (fbot, 0.0)):
    f.material_index = 0
    for loop in f.loops:
        idx = (top if f is ftop else bot).index(loop.vert)
        loop[uv].uv = (u_vertex(idx), vv)

# ---- page block: a box inside the C ----
px0, px1 = -(T / 2 - BT - GAP), (T / 2 - BT - GAP)
pd0, pd1 = ST + GAP, D - INS_D
pz0, pz1 = z0 + INS_Z, z1 - INS_Z
corners = {}
for xi, x in enumerate((px0, px1)):
    for di, d in enumerate((pd0, pd1)):
        for zi, z in enumerate((pz0, pz1)):
            corners[(xi, di, zi)] = bm.verts.new((x, d - D / 2, z))
c = corners
quads = [  # each ordered so the normal points outward
    (c[0, 0, 0], c[0, 1, 0], c[0, 1, 1], c[0, 0, 1]),   # -x face
    (c[1, 0, 0], c[1, 0, 1], c[1, 1, 1], c[1, 1, 0]),   # +x face
    (c[0, 0, 0], c[0, 0, 1], c[1, 0, 1], c[1, 0, 0]),   # -y (toward the spine)
    (c[0, 1, 0], c[1, 1, 0], c[1, 1, 1], c[0, 1, 1]),   # +y (fore-edge)
    (c[0, 0, 0], c[1, 0, 0], c[1, 1, 0], c[0, 1, 0]),   # -z (bottom)
    (c[0, 0, 1], c[0, 1, 1], c[1, 1, 1], c[1, 0, 1]),   # +z (top)
]
for q in quads:
    f = bm.faces.new(q)
    f.material_index = 1
    nrm = f.normal if f.normal.length > 0 else None
    for loop in f.loops:
        p = loop.vert.co
        u = (p.x - px0) / (px1 - px0)
        v = (p.z - z0) / H if (abs(f.normal.z) < 0.5) else (p.y + D / 2) / D
        loop[uv].uv = (u * 6.0, v)                      # six repeats of the 128 px page-line stripe across the block

bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
bmesh.ops.triangulate(bm, faces=[ftop, fbot], quad_method='BEAUTY', ngon_method='EAR_CLIP')

me = bpy.data.meshes.new("Book_Tome")
bm.to_mesh(me)
bm.free()
obj = bpy.data.objects.new("Book_Tome", me)
bpy.context.scene.collection.objects.link(obj)
for name in ("BookCover", "BookPages"):
    m = bpy.data.materials.new(name)
    me.materials.append(m)

# ---- bevel the hard edges (UVs are interpolated by the modifier) ----
mod = obj.modifiers.new("Bevel", 'BEVEL')
mod.width = BEVEL
mod.segments = 2
mod.limit_method = 'ANGLE'
mod.angle_limit = math.radians(35)
dg = bpy.context.evaluated_depsgraph_get()
eval_obj = obj.evaluated_get(dg)
me2 = bpy.data.meshes.new_from_object(eval_obj)
bpy.data.objects.remove(obj, do_unlink=True)
obj = bpy.data.objects.new("Book_Tome", me2)
bpy.context.scene.collection.objects.link(obj)
for name in ("BookCover", "BookPages"):
    obj.data.materials.append(bpy.data.materials[name]) if len(obj.data.materials) < 2 else None

# ---- smooth shading where the angle is gentle (the rounded spine), flat where it is sharp ----
bpy.context.view_layer.objects.active = obj
obj.select_set(True)
try:
    with bpy.context.temp_override(object=obj, active_object=obj, selected_objects=[obj], selected_editable_objects=[obj]):
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(40))
except Exception as e:   # older or newer API: fall back to plain smooth shading
    print("shade_smooth_by_angle failed:", e)
    for p in me2.polygons:
        p.use_smooth = True

# ---- normalise to a 1 x 1 x 1 box and report ----
sx, sy, sz = 1.0 / T, 1.0 / D, 1.0 / H     # x thickness, y depth, z height
for v in me2.vertices:
    v.co.x *= sx
    v.co.y *= sy
    v.co.z *= sz
me2.update()
xs = [v.co.x for v in me2.vertices]; ys = [v.co.y for v in me2.vertices]; zs = [v.co.z for v in me2.vertices]
print("BOOKMESH verts=%d tris=%d bounds x[%.4f,%.4f] y[%.4f,%.4f] z[%.4f,%.4f]" % (
    len(me2.vertices), sum(len(p.vertices) - 2 for p in me2.polygons), min(xs), max(xs), min(ys), max(ys), min(zs), max(zs)))

bpy.ops.object.select_all(action='DESELECT')
obj.select_set(True)
bpy.context.view_layer.objects.active = obj
bpy.ops.export_scene.fbx(
    filepath=out_path,
    use_selection=True,
    apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_NONE',
    bake_space_transform=True,
    axis_forward='Z',
    axis_up='Y',
    object_types={'MESH'},
    mesh_smooth_type='OFF',
    use_mesh_modifiers=True,
    add_leaf_bones=False,
)
print("EXPORTED", out_path)
