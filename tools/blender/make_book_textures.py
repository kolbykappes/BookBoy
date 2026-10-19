"""Generates the sample book cover textures (procedural, no AI, no external assets) for the BookBoy tome mesh.

Run:  blender -b --factory-startup -P tools/blender/make_book_textures.py -- <output folder>

Writes (8-bit PNGs, written directly so no colour-space conversion can sneak in):
  T_BookCover_Albedo.png  2048 x 1024  a light, tintable cloth cover: weave, embossed border on both boards, ribs / bands / a label plate on the spine.
                                        It is a MULTIPLY texture: its values are 0.6 to 1.0 and the game tints it per set, so keep it bright and neutral.
  T_BookCover_Normal.png  2048 x 1024  tangent-space normal map (OpenGL convention, Y up) from the same height field
  T_BookPages_Albedo.png  128 x 64     cream page stack, thin vertical lines across the thickness; tiled 6x across the page block
UV layout (see make_book.py): u 0..0.30 back board, 0.30..0.70 spine, 0.70..1.0 front board, v 0..1 up the height.
"""
import math
import struct
import sys
import zlib

import numpy as np

argv = sys.argv
out_dir = argv[argv.index("--") + 1] if "--" in argv else "."

W, H = 2048, 1024
rng = np.random.default_rng(20261019)

# real-size numbers, to place features in metres
BOOK_H = 0.356
BOARD_LEN = 0.229        # a board face, spine to fore-edge
SPINE_LEN = 0.130        # the whole spine path including its two rounded corners


def png(path, rgb):
    """Write an (h, w, 3 or 4) uint8 array, row 0 = BOTTOM of the image, as a PNG (rows are stored top first)."""
    a = np.ascontiguousarray(rgb[::-1])
    h, w, c = a.shape
    color_type = 6 if c == 4 else 2
    raw = b"".join(b"\x00" + a[y].tobytes() for y in range(h))

    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

    with open(path, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, color_type, 0, 0, 0)))
        f.write(chunk(b"IDAT", zlib.compress(raw, 9)))
        f.write(chunk(b"IEND", b""))


def lowpass_noise(h, w, sigma):
    """Band-limited noise in [-1, 1]: white noise through a Gaussian low-pass in the frequency domain (wraps, so it tiles)."""
    n = rng.standard_normal((h, w))
    fy = np.fft.fftfreq(h)[:, None]
    fx = np.fft.fftfreq(w)[None, :]
    g = np.exp(-(fx * fx + fy * fy) * (2 * (math.pi * sigma) ** 2))
    out = np.real(np.fft.ifft2(np.fft.fft2(n) * g))
    out -= out.mean()
    return out / (np.abs(out).max() + 1e-9)


def smoothstep(a, b, x):
    t = np.clip((x - a) / (b - a), 0.0, 1.0)
    return t * t * (3 - 2 * t)


v = (np.arange(H) + 0.5)[:, None] / H           # 0 bottom .. 1 top
u = (np.arange(W) + 0.5)[None, :] / W
U = np.broadcast_to(u, (H, W))
V = np.broadcast_to(v, (H, W))

# ---- height field (0..1) ----
height = np.zeros((H, W))

# cloth weave: two thread directions, ~1.6 mm pitch on the board
px_per_m_u = W * 0.30 / BOARD_LEN
px_per_m_v = H / BOOK_H
pitch = 0.0016
weave = (np.sin(2 * math.pi * np.arange(W)[None, :] / (pitch * px_per_m_u)) *
         np.sin(2 * math.pi * np.arange(H)[:, None] / (pitch * px_per_m_v)))
height += 0.05 * weave
height += 0.05 * lowpass_noise(H, W, 6)           # mottling
height += 0.02 * lowpass_noise(H, W, 1.2)         # fibre

# regions
back = U < 0.30
spine = (U >= 0.30) & (U < 0.70)
front = U >= 0.70


def board_coords(u0):
    """For a board: metres from the spine (s) and from the bottom (z), in the board's own frame."""
    return None


# distance (metres) from the spine side and from the fore-edge for the two boards
s_back = (0.30 - U) / 0.30 * BOARD_LEN             # back board: 0 at the spine start, BOARD_LEN at the fore-edge
s_front = (U - 0.70) / 0.30 * BOARD_LEN
z_m = V * BOOK_H

# embossed border on each board: a groove inset 12 mm and a finer one at 18 mm
lines = np.zeros((H, W))


def groove(coord_s, inset, width):
    """A rectangle groove at 'inset' from the fore-edge, the top and the bottom (and 'inset' from the spine side)."""
    d_edge = BOARD_LEN - coord_s                    # metres to the fore-edge
    d_spine = coord_s
    d_bot = z_m
    d_top = BOOK_H - z_m
    dist = np.minimum.reduce([d_edge, d_spine - 0.0, d_bot, d_top])
    return np.exp(-((dist - inset) / width) ** 2)


gb = np.where(back, groove(s_back, 0.012, 0.0009) + 0.6 * groove(s_back, 0.018, 0.0006), 0.0)
gf = np.where(front, groove(s_front, 0.012, 0.0009) + 0.6 * groove(s_front, 0.018, 0.0006), 0.0)
lines += gb + gf
# a small centred cartouche on each board: a thin rectangle 0.09 x 0.13 m
for s_coord, region in ((s_back, back), (s_front, front)):
    cs = BOARD_LEN / 2
    cz = BOOK_H / 2
    dx = np.abs(s_coord - cs) - 0.045
    dz = np.abs(z_m - cz) - 0.065
    box = np.maximum(dx, dz)
    lines += np.where(region, 0.8 * np.exp(-(box / 0.0008) ** 2), 0.0)
height -= 0.35 * lines

# ---- the spine ----
# position along the spine path in metres (0..SPINE_LEN) and up the book
sp = (U - 0.30) / 0.40 * SPINE_LEN
ribs = np.zeros((H, W))
for zc in (0.060, 0.115, 0.241, 0.296):            # four raised bands (m from the bottom)
    ribs += np.exp(-((z_m - zc) / 0.0055) ** 2)
for zc in (0.022, 0.334):                           # head and tail bands
    ribs += 0.8 * np.exp(-((z_m - zc) / 0.0045) ** 2)
for zc in (0.075, 0.100, 0.256, 0.281):             # fine rules between the bands
    ribs += 0.25 * np.exp(-((z_m - zc) / 0.0007) ** 2)
height += np.where(spine, 0.45 * ribs, 0.0)

# label plate: 62 mm wide centred on the spine, 0.14 to 0.235 up (matches the letter's position, local y = +0.05)
pw, pz0, pz1 = 0.062, 0.150, 0.250
in_plate = (np.abs(sp - SPINE_LEN / 2) < pw / 2) & (z_m > pz0) & (z_m < pz1) & spine
plate_edge = np.minimum(pw / 2 - np.abs(sp - SPINE_LEN / 2), np.minimum(z_m - pz0, pz1 - z_m))
height += np.where(in_plate, 0.12, 0.0)
height -= np.where(in_plate, 0.25 * np.exp(-((plate_edge - 0.003) / 0.0007) ** 2), 0.0)   # a rule inside the plate edge

# ---- albedo (a MULTIPLY texture: bright and neutral) ----
hn = (height - height.mean()) / (np.abs(height - height.mean()).max() + 1e-9)
gy, gx = np.gradient(height)
edge_shade = np.clip(np.sqrt(gx * gx + gy * gy) * 40.0, 0.0, 1.0)       # steep parts of the relief (rib sides, groove walls) read darker
albedo = 0.84 + 0.06 * hn
albedo += 0.015 * lowpass_noise(H, W, 3)
albedo -= 0.30 * np.clip(lines, 0, 1)                                    # grooves print darker
albedo -= 0.16 * edge_shade                                              # relief edges
albedo += np.where(spine, 0.07 * np.clip(ribs, 0, 1), 0.0)              # the tops of the raised bands catch the light
albedo += np.where(in_plate, 0.16, 0.0)                                  # the label plate is lighter so the letter reads
# edge wear on the board perimeters and the spine's head and tail
wear = np.zeros((H, W))
edge_b = np.minimum.reduce([BOARD_LEN - s_back, z_m, BOOK_H - z_m])
edge_f = np.minimum.reduce([BOARD_LEN - s_front, z_m, BOOK_H - z_m])
wear += np.where(back, np.exp(-edge_b / 0.004), 0.0) + np.where(front, np.exp(-edge_f / 0.004), 0.0)
albedo -= 0.05 * wear * (0.5 + 0.5 * (lowpass_noise(H, W, 2) > 0))
# plain top and bottom rows, and the plain strip the inner faces and end caps use (u near 0.005)
plain = (V < 0.012) | (V > 0.988) | (U < 0.012)
albedo = np.where(plain, 0.84, albedo)
height = np.where(plain, 0.0, height)
albedo = np.clip(albedo, 0.55, 1.0)

rgb = np.repeat(albedo[:, :, None], 3, axis=2)
rgb = (rgb * 255).astype(np.uint8)
png(out_dir + "/T_BookCover_Albedo.png", rgb)

# ---- normal map (OpenGL, Y up) from the height field ----
strength = 5.0
dh_dv, dh_du = np.gradient(height)                 # rows are v, columns are u
nx = -dh_du * strength * 8
ny = -dh_dv * strength * 8
nz = np.ones_like(nx)
ln = np.sqrt(nx * nx + ny * ny + nz * nz)
normal = np.stack([nx / ln, ny / ln, nz / ln], axis=2) * 0.5 + 0.5
png(out_dir + "/T_BookCover_Normal.png", (normal * 255).astype(np.uint8))

# ---- pages: thin lines across the thickness (u), cream ----
PW, PH = 128, 64
cols = np.arange(PW)[None, :]
page = np.ones((PH, PW, 3)) * np.array([0.93, 0.89, 0.77])
stripe = (cols % 4 == 0).astype(float)
page *= (1.0 - 0.13 * stripe)[:, :, None]
page *= (1.0 + 0.025 * rng.standard_normal((PH, PW))[:, :, None])
png(out_dir + "/T_BookPages_Albedo.png", (np.clip(page, 0, 1) * 255).astype(np.uint8))
print("TEXTURES written to", out_dir)
