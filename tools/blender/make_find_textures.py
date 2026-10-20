"""Procedural tileable textures for the finds (apron and shoes): canvas twill, leather grain, felt, linen, paper. 512 x 512 RGB.

Run (system Python with Pillow and numpy):  python tools/blender/make_find_textures.py <output folder>
The materials tint them (the pocket canvas is lighter, the dark leather darker, the tape and patches take a colour from the linen), so each file is a surface, not a colour.
"""
import math
import os
import sys

import numpy as np
from PIL import Image

OUT = sys.argv[1] if len(sys.argv) > 1 else "."
N = 512
rng = np.random.default_rng(1947)


def lowpass(sigma, n=N):
    """Periodic (tileable) smooth noise in -1..1."""
    noise = rng.standard_normal((n, n)).astype(np.float32)
    f = np.fft.fftfreq(n)
    g = np.exp(-((f[None, :] * sigma) ** 2 + (f[:, None] * sigma) ** 2) * (2 * math.pi ** 2))
    out = np.real(np.fft.ifft2(np.fft.fft2(noise) * g))
    out -= out.mean()
    return out / (np.abs(out).max() + 1e-9)


def save(name, rgb):
    a = np.clip(rgb, 0.0, 1.0)
    Image.fromarray((a * 255 + 0.5).astype(np.uint8), "RGB").save(os.path.join(OUT, name), optimize=True)


y, x = np.mgrid[0:N, 0:N].astype(np.float32)

# canvas twill: a diagonal weave (period 8 px) with thread-to-thread variation and slow fading, faded indigo
period = 8.0
diag = np.sin(2 * math.pi * (x + y) / period) * 0.5 + 0.5
thread = np.sin(2 * math.pi * x / (period / 2)) * np.sin(2 * math.pi * y / (period / 2))
weave = 0.55 * diag + 0.25 * thread
fade = lowpass(40) * 0.5 + lowpass(6) * 0.25
fibre = lowpass(1.2) * 0.35
lum = 0.62 + 0.20 * weave + 0.10 * fade + 0.05 * fibre
canvas = np.stack([lum * 0.50, lum * 0.62, lum * 1.0], axis=2) * 0.95
save("T_Find_Canvas.png", canvas)

# leather: a fine pebbled grain (cellular-looking noise), scuffs and darker creases, mid brown
grain = lowpass(2.2) * 0.5 + lowpass(1.0) * 0.5
cell = np.abs(lowpass(3.5))
scuff = np.clip(lowpass(18) * 1.4, 0, 1) * 0.25
lum = 0.74 + 0.08 * grain - 0.07 * (1 - cell) ** 3 + scuff * 0.15 + 0.05 * lowpass(60)
leather = np.stack([lum * 0.80, lum * 0.52, lum * 0.30], axis=2)
save("T_Find_Leather.png", leather)

# felt: a thick pressed wool, soft and uneven, warm pale grey
lum = 0.80 + 0.07 * lowpass(1.0) + 0.05 * lowpass(3.0) + 0.04 * lowpass(30)
felt = np.stack([lum * 0.95, lum * 0.90, lum * 0.80], axis=2)
save("T_Find_Felt.png", felt)

# linen: a plain weave with slubs, cream (used for the tapes, laces and patches, tinted by the materials)
warp = np.sin(2 * math.pi * x / 4.0) * 0.5 + 0.5
weft = np.sin(2 * math.pi * y / 4.0) * 0.5 + 0.5
checker = np.where(((x // 4 + y // 4) % 2) == 0, warp, weft)
slub_h = lowpass(1.0)[:, :1] * 0 + lowpass(0.8) * 0.5
lum = 0.74 + 0.14 * checker + 0.06 * slub_h + 0.05 * lowpass(25)
linen = np.stack([lum * 1.0, lum * 0.97, lum * 0.90], axis=2)
save("T_Find_Linen.png", linen)

# paper: yellowed cream with fibres and a few foxing spots
fibres = lowpass(0.9) * 0.5 + lowpass(2.0) * 0.5
fox = np.clip(lowpass(5) * 2.2 - 1.3, 0, 1) * 0.25
lum = 0.86 + 0.05 * fibres - fox + 0.03 * lowpass(40)
paper = np.stack([lum * 1.0, lum * 0.93, lum * 0.76], axis=2)
save("T_Find_Paper.png", paper)
print("find textures written to", OUT)
