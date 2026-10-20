"""Builds the two hero finds (the apron and the soft-soled shoes) and exports them as FBX for Unity.

Run:  blender -b --factory-startup --python-exit-code 3 -P tools/blender/make_finds.py -- <output folder> [preview folder]

Both objects are modelled at real size, LYING FLAT like a book, in the book's axes (Blender: X = thickness / up when lying, Y = width, Z = length;
after the FBX import Unity sees x = thickness, y = length, z = width, as for the tomes), then NORMALISED per axis to a 1 x 1 x 1 box: the game scales each item by its real
size (the same trick as make_book.py), so the shape comes out exactly right and the collider is the bounding box. The real sizes are written to find_dims.json
(Unity axes) next to this script; LamplitBuilder reads them.

Period (autumn 1947, docs/15-art-direction.md): heavy canvas twill apron, leather-bound patch pockets, brass buttons, linen tapes, a paper tag on string;
mended brown leather shoes with thick felt soles, cotton laces, a paper tag. No plastic, no zips.

Material slot order (the Unity builder relies on it, see tools/verify_find_meshes.cs):
  apron: 0 canvas, 1 pocket canvas, 2 leather, 3 brass, 4 tape, 5 paper, 6 patch
  shoes: 0 leather, 1 felt, 2 lining, 3 brass, 4 lace, 5 paper, 6 leather dark
UVs are planar from the position (u = (y + 0.7 x) * 3, v = (z + 0.5 x) * 3: about one tile per 0.33 m), so the textures only need to be fine grain.
"""
import bmesh
import bpy
import json
import math
import os
import sys
from mathutils import Vector

argv = sys.argv
args = argv[argv.index("--") + 1:] if "--" in argv else []
out_dir = args[0] if args else "."
preview_dir = args[1] if len(args) > 1 else None
here = os.path.dirname(os.path.abspath(__file__))

APRON_MATS = ["canvas", "pocket", "leather", "brass", "tape", "paper", "patch"]
SHOE_MATS = ["leather", "felt", "lining", "brass", "lace", "paper", "leather_dark"]
VIEW_COLORS = {   # only for the preview renders
    "canvas": (0.12, 0.17, 0.36), "pocket": (0.18, 0.24, 0.42), "leather": (0.34, 0.19, 0.09), "brass": (0.75, 0.55, 0.20), "tape": (0.80, 0.74, 0.58),
    "paper": (0.90, 0.84, 0.68), "patch": (0.62, 0.45, 0.20), "felt": (0.72, 0.66, 0.54), "lining": (0.14, 0.09, 0.06), "lace": (0.88, 0.82, 0.66),
    "leather_dark": (0.20, 0.11, 0.06),
}


# ------------------------------------------------------------------ geometry helpers (all coordinates are (x, y, z) = (thickness, width, length), metres)
class Builder:
    def __init__(self, name, mats):
        self.name = name
        self.mats = mats
        self.bm = bmesh.new()
        self.jobs = []   # (mat index, function) run in material order, so Blender's slots keep the order above

    def add(self, mat, fn):
        self.jobs.append((self.mats.index(mat), fn))

    def run(self):
        for _, fn in sorted(self.jobs, key=lambda j: j[0]):
            fn(self)

    def face(self, verts, mat):
        try:
            f = self.bm.faces.new(verts)
        except ValueError:
            return None
        f.material_index = self.mats.index(mat) if isinstance(mat, str) else mat
        return f

    def vert(self, p):
        return self.bm.verts.new(Vector(p))

    def prism(self, outline, x0, x1, mat):
        """A flat shape: the (y, z) outline (counter-clockwise seen from +x) extruded from x0 to x1."""
        lo = [self.vert((x0, y, z)) for (y, z) in outline]
        hi = [self.vert((x1, y, z)) for (y, z) in outline]
        n = len(outline)
        self.face(list(reversed(lo)), mat)
        self.face(hi, mat)
        for i in range(n):
            j = (i + 1) % n
            self.face([lo[i], lo[j], hi[j], hi[i]], mat)

    def box(self, c, size, mat):
        cx, cy, cz = c
        sx, sy, sz = size[0] / 2, size[1] / 2, size[2] / 2
        v = [self.vert((cx + dx * sx, cy + dy * sy, cz + dz * sz)) for dx in (-1, 1) for dy in (-1, 1) for dz in (-1, 1)]
        idx = lambda dx, dy, dz: ((dx > 0) * 4 + (dy > 0) * 2 + (dz > 0))
        quads = [((-1, -1, -1), (-1, 1, -1), (-1, 1, 1), (-1, -1, 1)), ((1, -1, -1), (1, -1, 1), (1, 1, 1), (1, 1, -1)),
                 ((-1, -1, -1), (-1, -1, 1), (1, -1, 1), (1, -1, -1)), ((-1, 1, -1), (1, 1, -1), (1, 1, 1), (-1, 1, 1)),
                 ((-1, -1, -1), (1, -1, -1), (1, 1, -1), (-1, 1, -1)), ((-1, -1, 1), (-1, 1, 1), (1, 1, 1), (1, -1, 1))]
        for q in quads:
            self.face([v[idx(*p)] for p in q], mat)

    def cylinder_x(self, c, r, h, mat, seg=14):
        """A short cylinder with its axis along x (a button or an eyelet), base at c.x, top at c.x + h."""
        cx, cy, cz = c
        lo = [self.vert((cx, cy + r * math.cos(2 * math.pi * i / seg), cz + r * math.sin(2 * math.pi * i / seg))) for i in range(seg)]
        hi = [self.vert((cx + h, cy + r * math.cos(2 * math.pi * i / seg), cz + r * math.sin(2 * math.pi * i / seg))) for i in range(seg)]
        self.face(list(reversed(lo)), mat)
        self.face(hi, mat)
        for i in range(seg):
            j = (i + 1) % seg
            self.face([lo[i], lo[j], hi[j], hi[i]], mat)

    def ribbon(self, pts, normals, width, thick, mat, closed=False):
        """A flat strip along the 3D polyline pts; its width runs across the path and along the surface (normal given per point), thick along the normal."""
        n = len(pts)
        P = [Vector(p) for p in pts]
        N = [Vector(v).normalized() for v in normals]
        rows = []
        for i in range(n):
            a = P[(i - 1) % n] if closed else P[max(i - 1, 0)]
            b = P[(i + 1) % n] if closed else P[min(i + 1, n - 1)]
            t = (b - a).normalized()
            side = t.cross(N[i]).normalized() * (width / 2)
            rows.append([self.bm.verts.new(P[i] - side), self.bm.verts.new(P[i] + side),
                         self.bm.verts.new(P[i] - side + N[i] * thick), self.bm.verts.new(P[i] + side + N[i] * thick)])
        last = n if closed else n - 1
        for i in range(last):
            j = (i + 1) % n
            a, b = rows[i], rows[j]
            self.face([a[0], b[0], b[1], a[1]], mat)      # bottom
            self.face([a[2], a[3], b[3], b[2]], mat)      # top
            self.face([a[0], a[2], b[2], b[0]], mat)
            self.face([a[1], b[1], b[3], a[3]], mat)
        if not closed:
            r = rows[0]; self.face([r[0], r[1], r[3], r[2]], mat)
            r = rows[-1]; self.face([r[0], r[2], r[3], r[1]], mat)

    def loft(self, rings, mat, cap_first=True, cap_last=True):
        """Bridges equal-sized rings of 3D points (each ring a closed loop)."""
        R = [[self.vert(p) for p in ring] for ring in rings]
        m = len(R[0])
        for k in range(len(R) - 1):
            for i in range(m):
                j = (i + 1) % m
                self.face([R[k][i], R[k + 1][i], R[k + 1][j], R[k][j]], mat)
        if cap_first:
            self.face(list(reversed(R[0])), mat)
        if cap_last:
            self.face(R[-1], mat)


# ------------------------------------------------------------------ the apron
def build_apron():
    b = Builder("Find_Apron", APRON_MATS)
    X0 = 0.0
    T = 0.010                                   # the cloth
    half = [(0.062, 0.270), (0.066, 0.225), (0.074, 0.180), (0.092, 0.130), (0.108, 0.090), (0.118, 0.062), (0.130, 0.030), (0.150, -0.020),
            (0.168, -0.090), (0.180, -0.170), (0.186, -0.235), (0.176, -0.262), (0.150, -0.272)]
    ring = [(w, z) for (w, z) in reversed(half)]      # right edge, bottom to top
    ring2 = [(-w, z) for (w, z) in half]              # left edge, top to bottom
    cloth = ring + ring2                              # bottom-right -> top-right -> top-left -> bottom-left: counter-clockwise from +x

    b.add("canvas", lambda s: s.prism(cloth, X0, X0 + T, "canvas"))

    # a waistband (doubled cloth) and the lower pockets are 'pocket' canvas, slightly lighter and raised
    b.add("pocket", lambda s: s.prism([(-0.123, 0.040), (0.123, 0.040), (0.121, 0.078), (-0.121, 0.078)], X0 + T, X0 + T + 0.004, "pocket"))
    for (yc, w) in ((-0.066, 0.108), (0.066, 0.108)):
        b.add("pocket", lambda s, yc=yc, w=w: s.prism([(yc - w / 2, -0.165), (yc + w / 2, -0.165), (yc + w / 2, -0.045), (yc - w / 2, -0.045)], X0 + T, X0 + T + 0.007, "pocket"))
    b.add("pocket", lambda s: s.prism([(-0.034, 0.108), (0.034, 0.108), (0.034, 0.158), (-0.034, 0.158)], X0 + T, X0 + T + 0.007, "pocket"))   # chest pocket

    # leather binding on the top edge of each pocket
    for (yc, w, z, th) in ((-0.066, 0.108, -0.045, 0.014), (0.066, 0.108, -0.045, 0.014), (0.0, 0.068, 0.158, 0.011)):
        b.add("leather", lambda s, yc=yc, w=w, z=z, th=th: s.prism([(yc - w / 2 - 0.002, z - th), (yc + w / 2 + 0.002, z - th), (yc + w / 2 + 0.002, z), (yc - w / 2 - 0.002, z)], X0 + T + 0.003, X0 + T + 0.010, "leather"))
    # leather corner binding at the hem and a leather strap tab at the neck
    b.add("leather", lambda s: s.prism([(-0.044, 0.272), (0.044, 0.272), (0.044, 0.286), (-0.044, 0.286)], X0, X0 + T + 0.002, "leather"))

    # mended patches (ochre and a grey one), in front of the pockets and on the skirt
    b.add("patch", lambda s: s.prism([(0.036, -0.150), (0.090, -0.150), (0.090, -0.100), (0.036, -0.100)], X0 + T + 0.007, X0 + T + 0.0095, "patch"))
    b.add("patch", lambda s: s.prism([(-0.150, -0.230), (-0.108, -0.226), (-0.106, -0.190), (-0.148, -0.192)], X0 + T, X0 + T + 0.003, "patch"))
    b.add("patch", lambda s: s.prism([(0.090, 0.085), (0.112, 0.085), (0.112, 0.110), (0.090, 0.110)], X0 + T, X0 + T + 0.003, "patch"))

    # linen tapes: the neck strap (a loop up from the bib corners) and the waist ties that lie along the skirt
    nrm = (1, 0, 0)

    def neck(s):
        path = [(X0 + 0.004, -0.060, 0.268), (X0 + 0.006, -0.066, 0.300), (X0 + 0.006, -0.040, 0.322), (X0 + 0.006, 0.0, 0.332), (X0 + 0.006, 0.040, 0.322),
                (X0 + 0.006, 0.066, 0.300), (X0 + 0.004, 0.060, 0.268)]
        s.ribbon(path, [nrm] * len(path), 0.016, 0.005, "tape")
    b.add("tape", neck)

    def tie(sign):
        def f(s):
            path = [(X0 + T, sign * 0.118, 0.058), (X0 + T + 0.002, sign * 0.150, 0.040), (X0 + T + 0.003, sign * 0.188, 0.0), (X0 + T + 0.003, sign * 0.206, -0.055),
                    (X0 + T + 0.003, sign * 0.202, -0.120), (X0 + T + 0.002, sign * 0.190, -0.170)]
            s.ribbon(path, [nrm] * len(path), 0.020, 0.004, "tape")
        return f
    b.add("tape", tie(1))
    b.add("tape", tie(-1))

    # brass buttons where the neck strap meets the bib, and one on the chest pocket
    for (y, z) in ((-0.060, 0.255), (0.060, 0.255), (0.0, 0.150)):
        b.add("brass", lambda s, y=y, z=z: s.cylinder_x((X0 + T, y, z), 0.0085, 0.006, "brass"))

    # the paper tag, tied on with string (a short loop to the right waist tie)
    def tag(s):
        c = (X0 + T + 0.008, 0.150, -0.062)
        ang = math.radians(-16)
        ca, sa = math.cos(ang), math.sin(ang)
        pts = [(-0.021, -0.034), (0.021, -0.034), (0.021, 0.034), (-0.021, 0.034)]
        outline = [(c[1] + y * ca - z * sa, c[2] + y * sa + z * ca) for (y, z) in pts]
        s.prism(outline, c[0], c[0] + 0.0025, "paper")
        string = [(X0 + T + 0.006, 0.150 + 0.004, -0.062 + 0.030), (X0 + T + 0.012, 0.172, -0.030), (X0 + T + 0.010, 0.196, -0.056)]
        s.ribbon(string, [nrm] * len(string), 0.0025, 0.0025, "paper")
    b.add("paper", tag)
    return b


# ------------------------------------------------------------------ the shoes
def superellipse_ring(yc, xb, xt, hw, z, n=14, p=2.6):
    """A closed ring in the (y, x) plane at length z: flat bottom at x = xb, top at xt, half width hw."""
    xc, hh = (xb + xt) / 2.0, (xt - xb) / 2.0
    ring = []
    for i in range(n):
        a = 2 * math.pi * i / n
        c, s = math.cos(a), math.sin(a)
        ring.append((xc + hh * math.copysign(abs(s) ** (2 / p), s), yc + hw * math.copysign(abs(c) ** (2 / p), c), z))
    return ring


UPPER = [   # z, half width, top x   (the bottom is the welt at x = 0.021)
    (-0.142, 0.026, 0.078), (-0.128, 0.033, 0.088), (-0.100, 0.037, 0.096), (-0.060, 0.038, 0.098), (-0.020, 0.040, 0.088), (0.020, 0.044, 0.070),
    (0.062, 0.048, 0.056), (0.100, 0.048, 0.046), (0.128, 0.040, 0.037), (0.143, 0.024, 0.030),
]


def upper_top(z):
    """The height of the top of the upper at length z (interpolated), used to lay the tongue and laces on it."""
    for (z0, _, t0), (z1, _, t1) in zip(UPPER, UPPER[1:]):
        if z0 <= z <= z1:
            return t0 + (t1 - t0) * (z - z0) / (z1 - z0)
    return UPPER[-1][2]


def shoe(b, yc, zoff, yaw_deg, mirror=1):
    ca, sa = math.cos(math.radians(yaw_deg)), math.sin(math.radians(yaw_deg))

    def tr(p):   # place a shoe-local point (x, y, z) in the pair: yaw about x, then shift
        x, y, z = p
        y *= mirror
        return (x, yc + y * ca - z * sa, zoff + y * sa + z * ca)

    def trv(v):
        return tuple(v)

    sole = [(0.0, 0.030), (0.034, 0.0), (0.040, 0.0)]
    soleo = [(0.028, -0.146), (0.036, -0.124), (0.040, -0.070), (0.038, -0.020), (0.044, 0.030), (0.052, 0.074), (0.050, 0.112), (0.038, 0.140), (0.014, 0.153)]
    soleo = [(w, z) for (w, z) in soleo]
    outline = [(-(w), z) for (w, z) in reversed(soleo)] + [(w, z) for (w, z) in soleo]
    outline_t = [tr((0, y, z))[1:] for (y, z) in outline]
    if mirror < 0:
        outline_t = outline_t[::-1]

    b.add("felt", lambda s: s.prism(outline_t, 0.0, 0.016, "felt"))
    # welt and heel stack, darker leather
    inner = [((w - 0.003) if w > 0 else (w + 0.003), z) for (y, z) in outline for w in [y]]
    inner_t = [tr((0, y, z))[1:] for (y, z) in inner]
    if mirror < 0:
        inner_t = inner_t[::-1]
    b.add("leather_dark", lambda s: s.prism(inner_t, 0.016, 0.022, "leather_dark"))
    heel = [(-0.034, -0.142), (0.034, -0.142), (0.038, -0.078), (-0.038, -0.078)]
    heel_t = [tr((0, y, z))[1:] for (y, z) in heel]
    if mirror < 0:
        heel_t = heel_t[::-1]
    b.add("leather_dark", lambda s: s.prism(heel_t, 0.022, 0.030, "leather_dark"))

    def upper(s):
        rings = []
        for (z, hw, top) in UPPER:
            rings.append([tr(p) for p in superellipse_ring(0.0, 0.022, top, hw, z)])
        if mirror < 0:
            rings = [r[::-1] for r in rings]
        s.loft(rings, "leather", cap_first=True, cap_last=True)
    b.add("leather", upper)

    # the collar: a dark lining oval sunk in the top at the ankle, with a rolled leather rim around it
    def lining(s):
        # a dark funnel sunk into the top of the upper at the ankle: it reads as the opening
        z0, z1, hw = -0.126, -0.038, 0.024
        zc = (z0 + z1) / 2
        ring = []
        for i in range(16):
            a = 2 * math.pi * i / 16
            z = zc + (z1 - z0) / 2 * math.cos(a)
            ring.append(s.vert(tr((upper_top(z) + 0.0008, hw * math.sin(a), z))))
        centre = s.vert(tr((upper_top(zc) - 0.016, 0.0, zc)))
        for i in range(16):
            s.face([ring[i], ring[(i + 1) % 16], centre], "lining")
    b.add("lining", lining)

    def rim(s):
        z0, z1, hw = -0.132, -0.032, 0.030
        pts, nrm = [], []
        for i in range(18):
            a = 2 * math.pi * i / 18
            z = (z0 + z1) / 2 + (z1 - z0) / 2 * math.cos(a)
            pts.append(tr((upper_top(z) + 0.002, hw * math.sin(a), z)))
            nrm.append((1, 0, 0))
        s.ribbon(pts, nrm, 0.007, 0.004, "leather_dark", closed=True)
    b.add("leather_dark", rim)

    # the toe cap seam and the tongue
    def seam(s):
        z = 0.098
        pts, nrm = [], []
        hw, top = 0.0485, 0.0465
        for i in range(16):
            a = 2 * math.pi * i / 16
            cx, cy = math.cos(a), math.sin(a)
            x = 0.022 + (top - 0.022) / 2 + (top - 0.022) / 2 * math.copysign(abs(cy) ** 0.77, cy)
            y = hw * math.copysign(abs(cx) ** 0.77, cx)
            pts.append(tr((x, y, z)))
            nrm.append((0, 0, 1))
        s.ribbon(pts, nrm, 0.007, 0.0015, "leather_dark", closed=True)
    b.add("leather_dark", seam)

    def tongue(s):
        pts = [tr((upper_top(z) + 0.003, 0.0, z)) for z in (-0.034, -0.010, 0.020, 0.050, 0.075)]
        s.ribbon(pts, [(1, 0, 0)] * len(pts), 0.052, 0.003, "leather_dark")
    b.add("leather_dark", tongue)

    # eyelets and criss-cross laces
    stations = [-0.020, 0.002, 0.024, 0.046, 0.066]
    for z in stations:
        for side in (-1, 1):
            b.add("brass", lambda s, z=z, side=side: s.cylinder_x(tr((upper_top(z) - 0.002, side * 0.027, z)), 0.0042, 0.003, "brass", seg=6))

    def laces(s):
        for i in range(len(stations) - 1):
            za, zb = stations[i], stations[i + 1]
            for sa_, sb_ in ((-1, 1), (1, -1)):
                p0 = tr((upper_top(za) + 0.0055, sa_ * 0.027, za))
                p1 = tr((upper_top((za + zb) / 2) + 0.0075, 0.0, (za + zb) / 2))
                p2 = tr((upper_top(zb) + 0.0055, sb_ * 0.027, zb))
                s.ribbon([p0, p1, p2], [(1, 0, 0)] * 3, 0.0034, 0.0016, "lace")
        # the bow: two loops and two ends
        z = stations[0] - 0.012
        c = tr((upper_top(z) + 0.006, 0.0, z))
        for sg in (-1, 1):
            loop = [tr((upper_top(z) + 0.006, sg * 0.004, z + 0.002)), tr((upper_top(z - 0.010) + 0.0085, sg * 0.016, z - 0.010)),
                    tr((upper_top(z - 0.022) + 0.0085, sg * 0.018, z - 0.022)), tr((upper_top(z - 0.030) + 0.0075, sg * 0.006, z - 0.028))]
            s.ribbon(loop, [(1, 0, 0)] * 4, 0.0032, 0.0016, "lace")
    b.add("lace", laces)
    return tr


def build_shoes():
    b = Builder("Find_Shoes", SHOE_MATS)
    shoe(b, -0.058, 0.000, 0.0, mirror=1)
    tr = shoe(b, 0.058, 0.012, 3.5, mirror=-1)

    def tag(s):
        base = tr((upper_top(0.020) + 0.008, 0.012, 0.010))
        c = (base[0] + 0.003, base[1] + 0.036, base[2] + 0.004)
        ang = math.radians(12)
        ca, sa = math.cos(ang), math.sin(ang)
        pts = [(-0.020, -0.032), (0.020, -0.032), (0.020, 0.032), (-0.020, 0.032)]
        outline = [(c[1] + y * ca - z * sa, c[2] + y * sa + z * ca) for (y, z) in pts]
        s.prism(outline, c[0], c[0] + 0.0025, "paper")
        string = [base, (base[0] + 0.004, base[1] + 0.014, base[2] + 0.016), (c[0] + 0.001, c[1] - 0.004, c[2] + 0.024)]
        s.ribbon(string, [(1, 0, 0)] * 3, 0.0025, 0.0025, "paper")
    b.add("paper", tag)
    return b


# ------------------------------------------------------------------ build, bevel, UV, normalise, export
def finish(b, out_path, dims, preview_name):
    b.run()
    me = bpy.data.meshes.new(b.name)
    b.bm.normal_update()
    b.bm.to_mesh(me)
    b.bm.free()
    obj = bpy.data.objects.new(b.name, me)
    bpy.context.scene.collection.objects.link(obj)
    for name in b.mats:
        m = bpy.data.materials.get("Find_" + name) or bpy.data.materials.new("Find_" + name)
        m.diffuse_color = VIEW_COLORS[name] + (1.0,)
        me.materials.append(m)

    mod = obj.modifiers.new("Bevel", 'BEVEL')
    mod.width = 0.0012
    mod.segments = 1
    mod.limit_method = 'ANGLE'
    mod.angle_limit = math.radians(40)
    dg = bpy.context.evaluated_depsgraph_get()
    me2 = bpy.data.meshes.new_from_object(obj.evaluated_get(dg))
    mats = list(me.materials)
    bpy.data.objects.remove(obj, do_unlink=True)
    obj = bpy.data.objects.new(b.name, me2)
    bpy.context.scene.collection.objects.link(obj)
    for m in mats:
        me2.materials.append(m)

    # recompute normals outward, smooth by angle
    bm2 = bmesh.new()
    bm2.from_mesh(me2)
    bmesh.ops.recalc_face_normals(bm2, faces=bm2.faces[:])
    bmesh.ops.triangulate(bm2, faces=bm2.faces[:], quad_method='BEAUTY', ngon_method='EAR_CLIP')
    bm2.faces.sort(key=lambda f: f.material_index)   # the importer builds submeshes in order of first use, and the bevel scrambles the face order: sort so slot k is submesh k
    bm2.faces.index_update()
    bm2.to_mesh(me2)
    bm2.free()
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    try:
        with bpy.context.temp_override(object=obj, active_object=obj, selected_objects=[obj], selected_editable_objects=[obj]):
            bpy.ops.object.shade_smooth_by_angle(angle=math.radians(35))
    except Exception as e:
        print("shade_smooth_by_angle failed:", e)
        for p in me2.polygons:
            p.use_smooth = True

    # planar UVs from the position (in metres, before normalising)
    uv = me2.uv_layers.new(name="UVMap")
    for poly in me2.polygons:
        for li in poly.loop_indices:
            v = me2.vertices[me2.loops[li].vertex_index].co
            uv.data[li].uv = ((v.y + 0.7 * v.x) * 3.0, (v.z + 0.5 * v.x) * 3.0)

    xs = [v.co.x for v in me2.vertices]; ys = [v.co.y for v in me2.vertices]; zs = [v.co.z for v in me2.vertices]
    mn = Vector((min(xs), min(ys), min(zs))); mx = Vector((max(xs), max(ys), max(zs)))
    size = mx - mn
    centre = (mn + mx) / 2
    dims[b.name] = [round(size.x, 4), round(size.z, 4), round(size.y, 4)]   # Unity axes: x thickness, y length, z width
    if preview_dir and preview_name:
        preview(obj, preview_dir, preview_name)
    for v in me2.vertices:
        v.co = Vector(((v.co.x - centre.x) / size.x, (v.co.y - centre.y) / size.y, (v.co.z - centre.z) / size.z))
    me2.update()
    tris = sum(len(p.vertices) - 2 for p in me2.polygons)
    counts = [sum(1 for p in me2.polygons if p.material_index == i) for i in range(len(b.mats))]
    print("FINDMESH %s verts=%d tris=%d size(x,y,z)=%.3f,%.3f,%.3f mats=%s tris_per_mat=%s" % (b.name, len(me2.vertices), tris, size.x, size.z, size.y, b.mats, counts))

    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(filepath=out_path, use_selection=True, apply_unit_scale=True, apply_scale_options='FBX_SCALE_NONE', bake_space_transform=True,
                             axis_forward='Z', axis_up='Y', object_types={'MESH'}, mesh_smooth_type='OFF', use_mesh_modifiers=True, add_leaf_bones=False)
    print("EXPORTED", out_path)
    bpy.data.objects.remove(obj, do_unlink=True)


def preview(obj, folder, name):
    """Workbench renders (material colours) from above and from the side, for a quick look at the shape."""
    os.makedirs(folder, exist_ok=True)
    scene = bpy.context.scene
    scene.render.engine = 'BLENDER_WORKBENCH'
    scene.display.shading.light = 'STUDIO'
    scene.display.shading.color_type = 'MATERIAL'
    scene.render.resolution_x = 900
    scene.render.resolution_y = 900
    scene.world = scene.world or bpy.data.worlds.new("W")
    scene.world.color = (0.55, 0.52, 0.48)
    cam_data = bpy.data.cameras.new("PreviewCam")
    cam = bpy.data.objects.new("PreviewCam", cam_data)
    scene.collection.objects.link(cam)
    scene.camera = cam
    cam_data.type = 'ORTHO'
    cam_data.ortho_scale = 0.78
    views = {"top": (0.9, 0.0, 0.0, True), "side": (0.0, -0.9, 0.0, True), "three": (0.55, -0.55, 0.45, False)}
    for key, (lx, ly, lz, ortho) in views.items():
        cam.location = (lx, ly, lz)
        cam_data.type = 'ORTHO' if ortho else 'PERSP'
        cam_data.lens = 70
        direction = Vector((0, 0, 0.0)) - Vector((lx, ly, lz))
        cam.rotation_euler = direction.to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = os.path.join(folder, "%s_%s.png" % (name, key))
        bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(cam, do_unlink=True)


dims = {}
os.makedirs(out_dir, exist_ok=True)
finish(build_apron(), os.path.join(out_dir, "Find_Apron.fbx"), dims, "apron")
finish(build_shoes(), os.path.join(out_dir, "Find_Shoes.fbx"), dims, "shoes")
with open(os.path.join(here, "find_dims.json"), "w") as f:
    json.dump(dims, f, indent=2)
print("FIND DIMS", dims)
