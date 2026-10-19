"""Generates the tintable book covers and spines (test set of six series) as high-resolution textures.

Run (system Python with Pillow and numpy):  python tools/covers/make_covers.py <output folder> [preview folder]

For each of six series (topics from the game design: the reading-room lamps, the catalogue, the founder's letters, the map room, conservation and the
vaults) it writes
  T_Cover_<key>.png   2048 x 1600  the back board (left half) and the front board (right half), each a 1024 x 1600 cell
  T_Spine_<key>.png   3072 x 1024  eight volume cells of 384 x 1024: the series icon, the title along the spine, and the big volume number
The textures are MULTIPLY textures (a light cloth with dark ink): the game tints them per set, so only the ink and the cloth weave are in them.
Texel density: a spine cell is 0.13 m x 0.356 m (about 2,900 px per metre), a board cell 0.254 m x 0.356 m.

Fonts are system fonts (Georgia Bold, Garamond Bold) and are for TESTING ONLY: the rendered text is baked into pictures, but replace them with an open-licence
face (for example EB Garamond or IM Fell, SIL OFL) before release. See Docs/unity-practices/26-ai-assisted-book-models-and-materials.md.
"""
import math
import os
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFont

OUT = sys.argv[1] if len(sys.argv) > 1 else "."
PREVIEW = sys.argv[2] if len(sys.argv) > 2 else None
SS = 3                                  # supersampling for the vector drawing
FONT_TITLE = "C:/Windows/Fonts/georgiab.ttf"
FONT_SUB = "C:/Windows/Fonts/GARABD.TTF"
INK = 0.84                              # how much of the cloth brightness the ink removes (0.84 leaves 16 percent)
rng = np.random.default_rng(20261020)

SERIES = [
    dict(key="lamps", spine="LAMPS", title=("THE LAMPLIGHTER'S", "HANDBOOK"), sub="for the reading-room lamps", motif="lantern"),
    dict(key="catalogue", spine="CATALOGUE", title=("THE LAMPLIT", "CATALOGUE"), sub="entries and corrections", motif="drawer"),
    dict(key="letters", spine="LETTERS", title=("LETTERS OF", "THE FOUNDER"), sub="the mail-room collection", motif="envelope"),
    dict(key="atlas", spine="ATLAS", title=("ATLAS OF THE", "MARSH COAST"), sub="the map room", motif="compass"),
    dict(key="mending", spine="MENDING", title=("THE ART OF", "MENDING"), sub="a conservator's manual", motif="spool"),
    dict(key="vault", spine="VAULT", title=("SHELFMARKS OF", "THE VAULT"), sub="two systems, one library", motif="key"),
]


# ------------------------------------------------------------------ drawing helpers (all coordinates are in final pixels; SS is applied inside)
class Ink:
    """A mask the size of a cell, drawn at SS times the resolution and downsampled with anti-aliasing."""

    def __init__(self, w, h):
        self.w, self.h = w, h
        self.img = Image.new("L", (w * SS, h * SS), 0)
        self.d = ImageDraw.Draw(self.img)

    def p(self, pts):
        return [(x * SS, y * SS) for x, y in pts]

    def poly(self, pts, fill=255):
        self.d.polygon(self.p(pts), fill=fill)

    def line(self, pts, width, fill=255):
        self.d.line(self.p(pts), fill=fill, width=max(1, int(width * SS)), joint="curve")

    def rect(self, x0, y0, x1, y1, fill=255):
        self.d.rectangle([x0 * SS, y0 * SS, x1 * SS, y1 * SS], fill=fill)

    def frame(self, x0, y0, x1, y1, width):
        self.d.rectangle([x0 * SS, y0 * SS, x1 * SS, y1 * SS], outline=255, width=max(1, int(width * SS)))

    def ellipse(self, cx, cy, rx, ry, fill=255):
        self.d.ellipse([(cx - rx) * SS, (cy - ry) * SS, (cx + rx) * SS, (cy + ry) * SS], fill=fill)

    def ring(self, cx, cy, r, width):
        self.d.ellipse([(cx - r) * SS, (cy - r) * SS, (cx + r) * SS, (cy + r) * SS], outline=255, width=max(1, int(width * SS)))

    def text(self, xy, s, font_path, size, anchor="mm", spacing=0.0, stroke=0.0):
        """Draws s centred (anchor 'mm') at xy; spacing adds extra pixels between letters."""
        font = ImageFont.truetype(font_path, int(size * SS))
        if spacing == 0.0:
            self.d.text((xy[0] * SS, xy[1] * SS), s, fill=255, font=font, anchor=anchor, stroke_width=int(stroke * SS), stroke_fill=255)
            return
        total = sum(font.getlength(c) for c in s) + spacing * SS * (len(s) - 1)
        x = xy[0] * SS - total / 2
        for c in s:
            self.d.text((x, xy[1] * SS), c, fill=255, font=font, anchor="lm", stroke_width=int(stroke * SS), stroke_fill=255)
            x += font.getlength(c) + spacing * SS

    def mask(self):
        small = self.img.resize((self.w, self.h), Image.LANCZOS)
        return np.asarray(small, dtype=np.float32) / 255.0


def text_width(s, font_path, size, spacing=0.0):
    font = ImageFont.truetype(font_path, int(size))
    return sum(font.getlength(c) for c in s) + spacing * (len(s) - 1)


def fit_size(s, font_path, max_width, start, spacing_ratio=0.12):
    size = start
    while size > 8 and text_width(s, font_path, size, size * spacing_ratio) > max_width:
        size -= 2
    return size


# ------------------------------------------------------------------ motifs: each draws into an Ink around (cx, cy) with half-size r
def lantern(k, cx, cy, r):
    lw = max(3, r * 0.055)
    k.ring(cx, cy - r * 0.93, r * 0.16, lw)                                                   # the handle ring
    k.poly([(cx - r * 0.42, cy - r * 0.70), (cx + r * 0.42, cy - r * 0.70), (cx + r * 0.30, cy - r * 0.52), (cx - r * 0.30, cy - r * 0.52)])   # the cap
    k.line([(cx - r * 0.34, cy - r * 0.50), (cx - r * 0.46, cy + r * 0.52)], lw)               # the glass sides
    k.line([(cx + r * 0.34, cy - r * 0.50), (cx + r * 0.46, cy + r * 0.52)], lw)
    k.line([(cx - r * 0.34, cy - r * 0.50), (cx + r * 0.34, cy - r * 0.50)], lw)
    k.poly([(cx - r * 0.56, cy + r * 0.52), (cx + r * 0.56, cy + r * 0.52), (cx + r * 0.46, cy + r * 0.72), (cx - r * 0.46, cy + r * 0.72)])  # the base
    k.poly([(cx, cy - r * 0.34), (cx + r * 0.17, cy + r * 0.06), (cx + r * 0.10, cy + r * 0.32), (cx - r * 0.10, cy + r * 0.32), (cx - r * 0.17, cy + r * 0.06)])  # the flame
    for a in (-24, 24, 156, 204):                                                      # rays
        ang = math.radians(a)
        k.line([(cx + math.cos(ang) * r * 0.72, cy - r * 0.05 + math.sin(ang) * r * 0.72), (cx + math.cos(ang) * r * 0.95, cy - r * 0.05 + math.sin(ang) * r * 0.95)], lw * 0.8)


def drawer(k, cx, cy, r):
    lw = max(3, r * 0.05)
    k.frame(cx - r * 0.85, cy - r * 0.62, cx + r * 0.85, cy + r * 0.62, lw)                    # the drawer front
    k.frame(cx - r * 0.72, cy - r * 0.49, cx + r * 0.72, cy + r * 0.49, lw * 0.6)
    k.frame(cx - r * 0.30, cy - r * 0.34, cx + r * 0.30, cy - r * 0.12, lw * 0.7)                # the label holder
    k.rect(cx - r * 0.22, cy - r * 0.27, cx + r * 0.22, cy - r * 0.19)
    k.ellipse(cx, cy + r * 0.20, r * 0.10, r * 0.10)                                            # the knob
    for i in range(3):                                                                           # cards standing behind
        k.rect(cx - r * 0.60 + i * r * 0.2, cy - r * 0.86, cx - r * 0.50 + i * r * 0.2, cy - r * 0.66)


def envelope(k, cx, cy, r):
    lw = max(3, r * 0.05)
    k.frame(cx - r * 0.85, cy - r * 0.55, cx + r * 0.85, cy + r * 0.55, lw)
    k.line([(cx - r * 0.85, cy - r * 0.55), (cx, cy + r * 0.10), (cx + r * 0.85, cy - r * 0.55)], lw)    # the flap
    k.line([(cx - r * 0.85, cy + r * 0.55), (cx - r * 0.20, cy - r * 0.05)], lw * 0.7)
    k.line([(cx + r * 0.85, cy + r * 0.55), (cx + r * 0.20, cy - r * 0.05)], lw * 0.7)
    k.ellipse(cx, cy + r * 0.10, r * 0.22, r * 0.22)                                           # the wax seal
    for i in range(8):
        a = math.pi / 4 * i
        k.line([(cx + math.cos(a) * r * 0.30, cy + r * 0.10 + math.sin(a) * r * 0.30), (cx + math.cos(a) * r * 0.36, cy + r * 0.10 + math.sin(a) * r * 0.36)], lw * 0.7)


def compass(k, cx, cy, r):
    lw = max(3, r * 0.045)
    k.ring(cx, cy, r * 0.88, lw)
    k.ring(cx, cy, r * 0.72, lw * 0.6)
    for ang, length, wid in ((-90, 0.95, 0.15), (0, 0.95, 0.15), (90, 0.95, 0.15), (180, 0.95, 0.15), (-45, 0.58, 0.10), (45, 0.58, 0.10), (135, 0.58, 0.10), (-135, 0.58, 0.10)):
        a = math.radians(ang)
        tip = (cx + math.cos(a) * r * length, cy + math.sin(a) * r * length)
        l = (cx + math.cos(a + math.pi / 2) * r * wid, cy + math.sin(a + math.pi / 2) * r * wid)
        rr = (cx + math.cos(a - math.pi / 2) * r * wid, cy + math.sin(a - math.pi / 2) * r * wid)
        k.poly([tip, l, (cx, cy), rr])
    k.ellipse(cx, cy, r * 0.06, r * 0.06, fill=0)


def spool(k, cx, cy, r):
    lw = max(3, r * 0.05)
    k.rect(cx - r * 0.45, cy - r * 0.78, cx + r * 0.45, cy - r * 0.64)                          # the flanges
    k.rect(cx - r * 0.45, cy + r * 0.64, cx + r * 0.45, cy + r * 0.78)
    k.frame(cx - r * 0.32, cy - r * 0.64, cx + r * 0.32, cy + r * 0.64, lw)                      # the wound thread
    for i in range(7):
        y = cy - r * 0.55 + i * r * 0.185
        k.line([(cx - r * 0.32, y), (cx + r * 0.32, y + r * 0.10)], lw * 0.7)
    k.line([(cx + r * 0.32, cy + r * 0.20), (cx + r * 0.78, cy + r * 0.45), (cx + r * 0.60, cy + r * 0.78), (cx + r * 0.9, cy + r * 0.92)], lw * 0.8)   # the loose thread
    k.line([(cx + r * 0.55, cy - r * 0.9), (cx + r * 0.85, cy - r * 0.1)], lw)                    # the needle
    k.ring(cx + r * 0.56, cy - r * 0.86, r * 0.045, lw * 0.6)


def key(k, cx, cy, r):
    lw = max(3, r * 0.06)
    k.ring(cx, cy - r * 0.50, r * 0.30, lw * 1.4)                                               # the bow
    k.ring(cx, cy - r * 0.50, r * 0.10, lw)
    k.rect(cx - r * 0.07, cy - r * 0.22, cx + r * 0.07, cy + r * 0.88)                          # the shaft
    k.rect(cx + r * 0.07, cy + r * 0.50, cx + r * 0.34, cy + r * 0.62)                          # the bits
    k.rect(cx + r * 0.07, cy + r * 0.70, cx + r * 0.26, cy + r * 0.82)
    k.rect(cx - r * 0.22, cy - r * 0.02, cx + r * 0.22, cy + r * 0.06)                          # the collar


MOTIFS = dict(lantern=lantern, drawer=drawer, envelope=envelope, compass=compass, spool=spool, key=key)


# ------------------------------------------------------------------ the cloth
def lowpass(h, w, sigma):
    n = rng.standard_normal((h, w)).astype(np.float32)
    fy = np.fft.fftfreq(h)[:, None]
    fx = np.fft.fftfreq(w)[None, :]
    g = np.exp(-((fx * sigma) ** 2 + (fy * sigma) ** 2) * (2 * math.pi ** 2))
    out = np.real(np.fft.ifft2(np.fft.fft2(n) * g))
    out -= out.mean()
    return out / (np.abs(out).max() + 1e-9)


def cloth(w, h, base=0.94):
    x = np.arange(w)[None, :]
    y = np.arange(h)[:, None]
    weave = np.sin(2 * math.pi * x / 8.0) * np.sin(2 * math.pi * y / 8.0)
    return base + 0.018 * weave + 0.016 * lowpass(h, w, 5) + 0.008 * lowpass(h, w, 1.4)


def save(path, arr):
    a = np.clip(arr, 0.0, 1.0)
    Image.fromarray((np.stack([a, a, a], axis=2) * 255 + 0.5).astype(np.uint8), "RGB").save(path, optimize=True)


# ------------------------------------------------------------------ the front and back boards (1024 x 1024 cells)
def board(series, front):
    w, h = 1024, 1600                                                      # 0.229 m x 0.356 m: the same texel density both ways
    k = Ink(w, h)
    k.frame(40, 40, w - 40, h - 40, 9)                                     # the double rule
    k.frame(66, 66, w - 66, h - 66, 3)
    for (x, y) in ((86, 86), (w - 86, 86), (86, h - 86), (w - 86, h - 86)):   # corner ornaments
        k.rect(x - 14, y - 14, x + 14, y + 14)
    cx = w / 2
    if front:
        t1, t2 = series["title"]
        size = min(fit_size(t1, FONT_TITLE, 780, 84), fit_size(t2, FONT_TITLE, 780, 84))
        k.text((cx, 270), t1, FONT_TITLE, size, spacing=size * 0.10)
        k.text((cx, 270 + size * 1.25), t2, FONT_TITLE, size, spacing=size * 0.10)
        rule = 270 + size * 1.25 + size * 0.85
        k.line([(250, rule), (w - 250, rule)], 4)
        MOTIFS[series["motif"]](k, cx, 830, 310)
        k.line([(250, 1330), (w - 250, 1330)], 4)
        sub = series["sub"]
        k.text((cx, 1410), sub, FONT_SUB, fit_size(sub, FONT_SUB, 720, 58, 0.04), spacing=2)
    else:
        MOTIFS[series["motif"]](k, cx, h / 2 - 40, 200)
        k.text((cx, h - 200), "LAMPLIT", FONT_TITLE, 52, spacing=10)
    m = k.mask()
    yy, xx = np.mgrid[0:h, 0:w]
    edge = np.minimum.reduce([xx, yy, w - 1 - xx, h - 1 - yy]).astype(np.float32)
    wear = np.exp(-edge / 26.0) * 0.045                                     # a little darker toward the board edges
    return m, wear


def cover_texture(series):
    w, h = 2048, 1600
    base = cloth(w, h)
    back_ink, back_wear = board(series, False)
    front_ink, front_wear = board(series, True)
    ink = np.concatenate([back_ink, front_ink], axis=1)
    wear = np.concatenate([back_wear, front_wear], axis=1)
    return base * (1.0 - INK * ink) - wear * base


# ------------------------------------------------------------------ the spine cells (384 x 1024)
def spine_cell(series, volume):
    w, h = 384, 1024
    k = Ink(w, h)
    cx = w / 2
    k.rect(0, 0, w, 36)                                                    # head and tail bands
    k.rect(0, h - 36, w, h)
    k.rect(0, 54, w, 60)
    k.rect(0, h - 60, w, h - 54)
    # the icon plate
    k.frame(62, 84, w - 62, 262, 7)
    MOTIFS[series["motif"]](k, cx, 173, 80)
    # rules
    k.rect(40, 282, w - 40, 290)
    k.rect(40, 300, w - 40, 304)
    # the title, running down the spine (the letters' tops face right)
    title = series["spine"]
    zone = 470
    size = fit_size(title, FONT_TITLE, zone, 190, 0.08)
    tmp = Ink(900, 280)
    tmp.text((450, 140), title, FONT_TITLE, size, spacing=size * 0.08, stroke=2.0)
    tm = Image.fromarray((tmp.mask() * 255).astype(np.uint8)).rotate(-90, expand=True)     # now 220 wide and 900 tall
    tarr = np.asarray(tm, dtype=np.float32) / 255.0
    ty0 = 320 + (zone - 470) // 2
    th, tw = tarr.shape
    ink_t = np.zeros((h, w), dtype=np.float32)
    # centre the rotated text block (900 tall) on the title zone (320 to 790) and on the spine
    cy_zone = 320 + 470 / 2
    y0 = int(cy_zone - th / 2)
    x0 = int(cx - tw / 2)
    ys0, ys1 = max(0, y0), min(h, y0 + th)
    ink_t[ys0:ys1, x0:x0 + tw] = tarr[ys0 - y0:ys1 - y0, :]
    k.rect(40, 800, w - 40, 804)
    k.rect(40, 814, w - 40, 822)
    # the volume number plate
    k.frame(62, 834, w - 62, 950, 7)
    num = str(volume)
    k.text((cx, 894), num, FONT_SUB, 112, stroke=2.0)
    m = np.maximum(k.mask(), ink_t)
    return m


def spine_texture(series, volumes=8):
    base = cloth(384 * volumes, 1024)
    cells = [spine_cell(series, v) for v in range(1, volumes + 1)]
    ink = np.concatenate(cells, axis=1)
    return base * (1.0 - INK * ink)


# ------------------------------------------------------------------ run
os.makedirs(OUT, exist_ok=True)
if PREVIEW:
    os.makedirs(PREVIEW, exist_ok=True)
for s in SERIES:
    cov = cover_texture(s)
    spi = spine_texture(s)
    save(os.path.join(OUT, "T_Cover_" + s["key"] + ".png"), cov)
    save(os.path.join(OUT, "T_Spine_" + s["key"] + ".png"), spi)
    print("wrote", s["key"], cov.shape, spi.shape)
print("COVERS done")
