"""Generates a tileable procedural wood texture (albedo and normal map) for the bookcases. No AI, no external assets.

Run:  blender -b --factory-startup -P tools/blender/make_wood_textures.py -- <output folder>

Writes T_Wood_Albedo.png (1024 x 1024, a light neutral wood that the material tints) and T_Wood_Normal.png. The tile covers 0.6 m; the grain runs along u.
Rings, pores and fibres are anisotropic band-limited noise (long along the grain, thin across it); every period is an integer so it tiles.
"""
import math
import struct
import sys
import zlib

import numpy as np

argv = sys.argv
out_dir = argv[argv.index("--") + 1] if "--" in argv else "."
N = 1024
rng = np.random.default_rng(20261019)


def png(path, rgb):
    a = np.ascontiguousarray(rgb[::-1])
    h, w, c = a.shape
    raw = b"".join(b"\x00" + a[y].tobytes() for y in range(h))

    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)

    with open(path, "wb") as f:
        f.write(b"\x89PNG\r\n\x1a\n")
        f.write(chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 2 if c == 3 else 6, 0, 0, 0)))
        f.write(chunk(b"IDAT", zlib.compress(raw, 9)))
        f.write(chunk(b"IEND", b""))


def aniso(sx, sy):
    """Band-limited noise in [-1, 1]; feature size about sx pixels along u (the grain) and sy pixels along v. It wraps, so it tiles."""
    n = rng.standard_normal((N, N))
    fy = np.fft.fftfreq(N)[:, None]
    fx = np.fft.fftfreq(N)[None, :]
    g = np.exp(-((fx * sx) ** 2 + (fy * sy) ** 2) * (2 * math.pi ** 2))
    out = np.real(np.fft.ifft2(np.fft.fft2(n) * g))
    out -= out.mean()
    return out / (np.abs(out).max() + 1e-9)


v = (np.arange(N) + 0.5)[:, None] / N        # across the grain
RINGS = 22                                   # growth rings across the 0.6 m tile (an integer so the tile repeats)
warp = 0.9 * aniso(260, 70) + 0.25 * aniso(90, 22)
t = RINGS * v + warp
frac = t - np.floor(t)
ring = frac ** 2.0                            # a gradual early wood and a sharper late-wood edge
pores = np.clip(aniso(46, 1.4) - 0.55, 0, 1) * 2.2
fibre = aniso(140, 0.9)
mottle = aniso(520, 150)

albedo = 0.86 - 0.34 * ring - 0.20 * pores + 0.03 * fibre + 0.07 * mottle
albedo = np.clip(albedo, 0.30, 1.0)
# the dark late wood is also a little warmer (less blue), so the grain reads as colour as well as value
late = np.clip(ring, 0, 1)
rgb = np.stack([albedo, albedo * (1.0 - 0.05 * late), albedo * (1.0 - 0.12 * late)], axis=2)
png(out_dir + "/T_Wood_Albedo.png", (rgb * 255).astype(np.uint8))

height = 0.5 * ring - 0.9 * pores + 0.05 * fibre
dh_dv, dh_du = np.gradient(height)
nx = -dh_du * 14.0
ny = -dh_dv * 14.0
nz = np.ones_like(nx)
ln = np.sqrt(nx * nx + ny * ny + nz * nz)
normal = np.stack([nx / ln, ny / ln, nz / ln], axis=2) * 0.5 + 0.5
png(out_dir + "/T_Wood_Normal.png", (normal * 255).astype(np.uint8))
print("WOOD TEXTURES written to", out_dir)
