"""Generates the BookBoy bookcases (visual meshes) and exports them as FBX.

Run:  blender -b --factory-startup --python-exit-code 3 -P tools/blender/make_cases.py -- tools/blender/case_params.json <output folder>

Writes Case_Solo.fbx (one 8-slot bay), Case_Triple.fbx (4 + 8 + 4 slot bays with shared pilasters) and Case_Wing.fbx (one 4-slot bay).
The cases are VISUAL only: the game keeps its own colliders, shelves and slot objects (LamplitBuilder.Container), and the shelf tops, side
and board thicknesses and depth come from case_params.json, so the books sit exactly on the modelled boards.

Origin: the middle of the case footprint on the floor. Blender axes: X across, Y depth with the FRONT at -Y, Z up (exported with axis_forward='Z',
axis_up='Y', as the book mesh, so the front ends up toward Unity -z). Units are metres, written in centimetres (the Unity importer converts).
Material slots: 0 wood (tinted per case in Unity), 1 back panel wood, 2 brass.
Design (from the Librarian reference): a stepped plinth with a moulded cap, reeded pilasters on every side and seam with a base block and a stepped
capital, thick shelf boards with a front nosing, a planked back, a stepped cornice with a dentil course, an arched crest on every 8-wide bay with a
brass rosette, and a small brass nameplate plate on every shelf front (room for the 'claims' labels, backlog L-023).
"""
import json
import math
import random
import sys

import bmesh
import bpy

argv = sys.argv
args = argv[argv.index("--") + 1:] if "--" in argv else []
params_path = args[0] if len(args) > 0 else "case_params.json"
out_dir = args[1] if len(args) > 1 else "."
P = json.load(open(params_path))

PITCH = P["slot_pitch"]; SIDE = P["side_thickness"]; BOARD = P["board_thickness"]; D = P["case_depth"]
BH = P["base_height"]; SP = P["shelf_spacing"]; ROWS = P["shelf_rows"]; H = P["container_height"]
OPEN_EXTRA = P["case_margin"] - 2 * SIDE            # air in an opening beyond the slots (0.01)
HD = D / 2
TILE = 0.6                                           # metres of wood per texture tile

X_AXIS, Y_AXIS, Z_AXIS = 0, 1, 2


class Builder:
    def __init__(self):
        self.bm = bmesh.new()
        self.uv = self.bm.loops.layers.uv.new("UVMap")
        self.grain = self.bm.faces.layers.int.new("grain")
        self.offu = self.bm.faces.layers.float.new("offu")
        self.offv = self.bm.faces.layers.float.new("offv")
        self.rng = random.Random(7)

    def _finish(self, faces, mat, grain):
        ou, ov = self.rng.random(), self.rng.random()
        for f in faces:
            f.material_index = mat
            f[self.grain] = grain
            f[self.offu] = ou
            f[self.offv] = ov

    def box(self, x0, x1, y0, y1, z0, z1, mat=0, grain=Z_AXIS):
        bm = self.bm
        v = {}
        for i, x in enumerate((x0, x1)):
            for j, y in enumerate((y0, y1)):
                for k, z in enumerate((z0, z1)):
                    v[i, j, k] = bm.verts.new((x, y, z))
        quads = [
            (v[0, 0, 0], v[0, 1, 0], v[0, 1, 1], v[0, 0, 1]),
            (v[1, 0, 0], v[1, 0, 1], v[1, 1, 1], v[1, 1, 0]),
            (v[0, 0, 0], v[0, 0, 1], v[1, 0, 1], v[1, 0, 0]),
            (v[0, 1, 0], v[1, 1, 0], v[1, 1, 1], v[0, 1, 1]),
            (v[0, 0, 0], v[1, 0, 0], v[1, 1, 0], v[0, 1, 0]),
            (v[0, 0, 1], v[0, 1, 1], v[1, 1, 1], v[1, 0, 1]),
        ]
        self._finish([bm.faces.new(q) for q in quads], mat, grain)

    def prism_xz(self, pts, y0, y1, mat=0, grain=X_AXIS):
        """A flat shape given as (x, z) points (counter-clockwise seen from -Y), extruded from y0 to y1."""
        bm = self.bm
        front = [bm.verts.new((x, y0, z)) for x, z in pts]
        back = [bm.verts.new((x, y1, z)) for x, z in pts]
        faces = [bm.faces.new(front), bm.faces.new(list(reversed(back)))]
        n = len(pts)
        for i in range(n):
            j = (i + 1) % n
            faces.append(bm.faces.new((front[i], back[i], back[j], front[j])))
        self._finish(faces, mat, grain)

    def cylinder_y(self, cx, cz, r, y0, y1, mat=2, seg=14):
        bm = self.bm
        ring_f = [bm.verts.new((cx + r * math.cos(2 * math.pi * i / seg), y0, cz + r * math.sin(2 * math.pi * i / seg))) for i in range(seg)]
        ring_b = [bm.verts.new((cx + r * math.cos(2 * math.pi * i / seg), y1, cz + r * math.sin(2 * math.pi * i / seg))) for i in range(seg)]
        faces = [bm.faces.new(ring_f), bm.faces.new(list(reversed(ring_b)))]
        for i in range(seg):
            j = (i + 1) % seg
            faces.append(bm.faces.new((ring_f[i], ring_b[i], ring_b[j], ring_f[j])))
        self._finish(faces, mat, Z_AXIS)

    def assign_uvs(self):
        for f in self.bm.faces:
            n = f.normal
            a = max(range(3), key=lambda i: abs(n[i]))
            g = f[self.grain]
            others = [i for i in range(3) if i != a]
            if g in others:
                ua = g
                va = [i for i in others if i != g][0]
            else:                                    # end grain: use the two face axes
                ua, va = others
            ou, ov = f[self.offu], f[self.offv]
            for loop in f.loops:
                p = loop.vert.co
                loop[self.uv].uv = (p[ua] / TILE + ou, p[va] / TILE + ov)


def build(bays, name):
    b = Builder()
    opens = [n * PITCH + OPEN_EXTRA for n in bays]
    # panels: an outer side (SIDE), then for each bay the opening, then a seam (2 x SIDE) or the other outer side
    total = 2 * SIDE + sum(opens) + (len(bays) - 1) * 2 * SIDE
    x = -total / 2
    panels = []   # (x0, width, outer)
    bay_x = []    # (x0, x1) of each opening
    panels.append((x, SIDE, True)); x += SIDE
    for i, w in enumerate(opens):
        bay_x.append((x, x + w)); x += w
        if i < len(opens) - 1:
            panels.append((x, 2 * SIDE, False)); x += 2 * SIDE
    panels.append((x, SIDE, True))
    W = total
    tops = [BH + i * SP for i in range(ROWS)]

    # ---- plinth ----
    b.box(-W / 2, W / 2, -HD + 0.012, HD, 0.075, BH - 0.05, 0, X_AXIS)
    b.box(-W / 2 - 0.015, W / 2 + 0.015, -HD - 0.02, HD, 0.0, 0.045, 0, X_AXIS)                 # skirting
    b.box(-W / 2 - 0.008, W / 2 + 0.008, -HD - 0.008, HD, 0.045, 0.075, 0, X_AXIS)
    b.box(-W / 2 - 0.030, W / 2 + 0.030, -HD - 0.030, HD, BH - 0.050, BH - 0.035, 0, X_AXIS)    # stepped cap
    b.box(-W / 2 - 0.020, W / 2 + 0.020, -HD - 0.020, HD, BH - 0.035, BH - 0.020, 0, X_AXIS)
    b.box(-W / 2 - 0.010, W / 2 + 0.010, -HD - 0.010, HD, BH - 0.020, BH, 0, X_AXIS)
    for (x0, x1) in bay_x:                                                                       # raised panel on the plinth front
        b.box(x0 + 0.035, x1 - 0.035, -HD, -HD + 0.014, 0.115, BH - 0.095, 0, X_AXIS)

    # ---- side panels and reeded pilasters ----
    for (px, pw, outer) in panels:
        b.box(px, px + pw, -HD, HD, BH, H, 0, Z_AXIS)
        reeds = 3 if outer else 6
        gap = 0.004
        rw = (pw - gap * (reeds - 1)) / reeds
        for r in range(reeds):
            rx = px + r * (rw + gap)
            b.box(rx, rx + rw, -HD - 0.034, -HD, BH + 0.095, H - 0.115, 0, Z_AXIS)
        ex = 0.012
        b.box(px - ex, px + pw + ex, -HD - 0.050, -HD, BH, BH + 0.095, 0, Z_AXIS)               # base block
        b.box(px - ex, px + pw + ex, -HD - 0.044, -HD, H - 0.115, H - 0.045, 0, Z_AXIS)         # capital
        b.box(px - ex - 0.010, px + pw + ex + 0.010, -HD - 0.054, -HD, H - 0.045, H, 0, Z_AXIS)  # abacus

    # ---- the planked backs (built BEFORE the brass plates: Blender orders the material slots by first use, and Unity expects wood, back, brass) ----
    for (x0, x1) in bay_x:
        pw = 0.07
        nplank = max(1, int(round((x1 - x0) / pw)))
        w = (x1 - x0) / nplank
        for k in range(nplank):
            b.box(x0 + k * w + 0.0015, x0 + (k + 1) * w - 0.0015, HD - 0.02, HD, BH, H - BOARD, 1, Z_AXIS)

    # ---- shelf boards (the first shelf rests on the plinth cap), nosing, brass plates ----
    for bi, (x0, x1) in enumerate(bay_x):
        for r in range(1, ROWS):
            t = tops[r]
            b.box(x0, x1, -HD + 0.004, HD - 0.02, t - BOARD, t, 0, X_AXIS)
            b.box(x0 - 0.004, x1 + 0.004, -HD - 0.014, -HD + 0.004, t - 0.048, t - 0.006, 0, X_AXIS)   # front nosing
            cx = (x0 + x1) / 2
            b.box(cx - 0.055, cx + 0.055, -HD - 0.0185, -HD - 0.0145, t - 0.040, t - 0.014, 2, X_AXIS)  # brass nameplate

    # ---- top board and cornice ----
    b.box(-W / 2, W / 2, -HD, HD, H - BOARD, H, 0, X_AXIS)
    steps = [  # (extra x, front y beyond the case, z0, z1)
        (0.015, 0.015, H, H + 0.080),
        (0.035, 0.035, H + 0.080, H + 0.095),
        (0.050, 0.060, H + 0.125, H + 0.165),
        (0.065, 0.075, H + 0.165, H + 0.195),
        (0.055, 0.065, H + 0.195, H + 0.215),
    ]
    for ex, fy, z0, z1 in steps:
        b.box(-W / 2 - ex, W / 2 + ex, -HD - fy, HD + 0.01, z0, z1, 0, X_AXIS)
    # the dentil course under the cove
    nd = int((W + 0.06) / 0.045)
    for i in range(nd):
        dx = -W / 2 - 0.030 + (i + 0.5) * ((W + 0.06) / nd)
        b.box(dx - 0.011, dx + 0.011, -HD - 0.045, -HD - 0.012, H + 0.095, H + 0.125, 0, Z_AXIS)

    # ---- arched crest and rosette on every 8-wide bay ----
    for bi, (x0, x1) in enumerate(bay_x):
        if bays[bi] < 8:
            continue
        cx = (x0 + x1) / 2
        hw = (x1 - x0) / 2 + 0.05
        base_z = H + 0.215
        h = 0.17
        pts = [(cx - hw, base_z)]
        seg = 18
        for i in range(seg + 1):
            a = math.pi * (1 - i / seg)
            pts.append((cx + hw * math.cos(a), base_z + 0.03 + (h - 0.03) * math.sin(a)))
        pts.append((cx + hw, base_z))
        # counter-clockwise seen from the front (-Y): reverse the (x right, z up) order if needed
        pts = list(reversed(pts))
        b.prism_xz(pts, -HD - 0.01, -HD + 0.03, 0, X_AXIS)
        b.cylinder_y(cx, base_z + 0.115, 0.034, -HD - 0.024, -HD - 0.009, 2)

    b.assign_uvs()
    bm = b.bm
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    bmesh.ops.triangulate(bm, faces=[f for f in bm.faces if len(f.verts) > 4], quad_method='BEAUTY', ngon_method='EAR_CLIP')
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    for mname in ("CaseWood", "CaseBack", "CaseBrass"):
        mat = bpy.data.materials.get(mname) or bpy.data.materials.new(mname)
        me.materials.append(mat)
    obj = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(obj)

    # bevel the hard edges a little so the mouldings catch light
    mod = obj.modifiers.new("Bevel", 'BEVEL')
    mod.width = 0.0035
    mod.segments = 2
    mod.limit_method = 'ANGLE'
    mod.angle_limit = math.radians(35)
    dg = bpy.context.evaluated_depsgraph_get()
    me2 = bpy.data.meshes.new_from_object(obj.evaluated_get(dg))
    bpy.data.objects.remove(obj, do_unlink=True)
    obj = bpy.data.objects.new(name, me2)
    bpy.context.scene.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    try:
        with bpy.context.temp_override(object=obj, active_object=obj, selected_objects=[obj], selected_editable_objects=[obj]):
            bpy.ops.object.shade_smooth_by_angle(angle=math.radians(35))
    except Exception as e:
        print("shade_smooth_by_angle failed:", e)
        for p in me2.polygons:
            p.use_smooth = True
    xs = [v.co.x for v in me2.vertices]; ys = [v.co.y for v in me2.vertices]; zs = [v.co.z for v in me2.vertices]
    print("CASE %s verts=%d tris=%d x[%.3f,%.3f] y[%.3f,%.3f] z[%.3f,%.3f]" % (
        name, len(me2.vertices), sum(len(p.vertices) - 2 for p in me2.polygons), min(xs), max(xs), min(ys), max(ys), min(zs), max(zs)))
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    path = out_dir + "/" + name + ".fbx"
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_unit_scale=True, apply_scale_options='FBX_SCALE_NONE',
                             bake_space_transform=True, axis_forward='Z', axis_up='Y', object_types={'MESH'},
                             mesh_smooth_type='OFF', use_mesh_modifiers=True, add_leaf_bones=False)
    print("EXPORTED", path)
    bpy.data.objects.remove(obj, do_unlink=True)


build([8], "Case_Solo")
build([4, 8, 4], "Case_Triple")
build([4], "Case_Wing")
