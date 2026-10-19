# Printed book covers and spines (test set)

```
python tools/covers/make_covers.py C:/Code/BookBoy/Assets/_Project/Art/Textures/Books/Covers
blender -b --factory-startup --python-exit-code 3 -P tools/blender/make_book.py -- C:/Code/BookBoy/Assets/_Project/Art/Models/Book_TomeStyled.fbx styled
```

System Python with Pillow and numpy. After either step run `LamplitBuilder.SetupBookAssets()` (it also sets up the styled mesh and the twelve `Mat_Cover_*` / `Mat_Spine_*` materials), then `BuildRoom1()`.

- Six series from the game design: **LAMPS** (the reading-room lamps), **CATALOGUE**, **LETTERS** (the founder's), **ATLAS**, **MENDING**, **VAULT**. Index = `BookDefinition.coverStyle`; Room 1 sets A to F use them, sets G to P keep the old pixel-label tome.
- `T_Cover_<key>.png` is 2048 x 1600: the back board (left) and the front board (right), 1024 x 1600 each (0.229 x 0.356 m, so the texel density is the same both ways). `T_Spine_<key>.png` is 3072 x 1024: eight 384-px cells, one per volume (0.13 m wide, about 2,900 px per metre). The game picks the cell with `_BaseMap_ST` in a per-slot `MaterialPropertyBlock` (`Book.SetColor`).
- The textures are MULTIPLY textures: a light cloth (about 0.94) with dark ink (16 percent left). The set colour tints boards and spine; the pages stay cream. The ink therefore takes the tint's darkness, not a fixed colour.
- Styled mesh (`Book_TomeStyled.fbx`, same box and bevel as the plain tome): submesh 0 boards, 1 pages, 2 spine. Slot 1 stays the pages, so the ghost, glow hull and tint code work for both meshes.
- The spine design, bottom to top: volume plate, the title running down the spine, the series icon plate. The set letter of the old design is gone from the printed books (the claim plate on the shelf still shows icon and letter).
- Fonts are **Windows system fonts (Georgia Bold, Garamond Bold)**: for testing only. Replace them with an open-licence face (SIL OFL: EB Garamond, IM Fell, Cormorant) before release. The text is baked into the pictures, so nothing ships the fonts, but a licence-clean source is still better.
- Checks: `tools/verify_styled_book.cs` (Edit Mode), `tools/shots_covers.cs`, `tools/shots_covers_front.cs`. Previews in `preview/`.
