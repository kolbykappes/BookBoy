# Blender scripts (book mesh and cover textures)

Blender 5.2 is installed at `C:\Program Files\Blender Foundation\Blender 5.2\blender.exe`. Both scripts run headless and are safe to re-run;
they overwrite their outputs in place (Unity keeps the asset GUIDs because the files are replaced, not deleted).

```
blender -b --factory-startup --python-exit-code 3 -P tools/blender/make_book.py -- C:/Code/BookBoy/Assets/_Project/Art/Models/Book_Tome.fbx
blender -b --factory-startup --python-exit-code 3 -P tools/blender/make_book_textures.py -- C:/Code/BookBoy/Assets/_Project/Art/Textures/Books
```

- `make_book.py` builds the tome (rounded spine, two boards, inset page block, small bevel) at real size and normalises it to a 1 x 1 x 1 box;
  the game scales each item by (thickness, height, depth), exactly as it did with the old cube. Export axes: `axis_forward='Z'`, `axis_up='Y'`
  (with '-Z' the spine ended up at +z). Blender writes centimetres, so the Unity importer uses "Convert Units" (`useFileScale = true`).
- `make_book_textures.py` writes the tintable cover albedo (multiply texture, values 0.6 to 1.0), its normal map and the page-line texture.
- After running either script, in Unity run `LamplitBuilder.SetupBookAssets()` (it is also called by `BuildRoom1`), then rebuild the room.
- Check: `unity command eval_file tools/verify_book_mesh.cs` (unit box, two submeshes, spine toward -z, UVs, tangents).

## Bookcases and wood (2026-10-19)

```
blender -b --factory-startup --python-exit-code 3 -P tools/blender/make_wood_textures.py -- C:/Code/BookBoy/Assets/_Project/Art/Textures/Wood
blender -b --factory-startup --python-exit-code 3 -P tools/blender/make_cases.py -- C:/Code/BookBoy/tools/blender/case_params.json C:/Code/BookBoy/Assets/_Project/Art/Models
```

- `make_cases.py` writes `Case_Solo.fbx` (one 8-slot bay), `Case_Triple.fbx` (4 + 8 + 4 with shared pilasters) and `Case_Wing.fbx` (one 4-slot bay). They are VISUAL only: the game keeps cubes as invisible colliders and its own shelf and slot objects (`LamplitBuilder.Container` with `CaseLook.Mesh` or `ColliderOnly`).
  The numbers come from `tools/blender/case_params.json`, which mirrors the constants in `LamplitBuilder.Common.cs` (`LamplitBuilder.WriteCaseParams()` rewrites it); change a constant, rewrite the JSON, re-run the script, run `LamplitBuilder.SetupCaseAssets()`.
- Design (after the Librarian reference): stepped plinth with a moulded cap and raised panels, reeded pilasters at the sides and seams with base blocks and capitals, 2.4 in shelf boards with a front nosing, a planked back, a stepped cornice with a dentil course, an arched crest with a brass rosette on every 8-wide bay, and a brass nameplate plate on every shelf front (room for the 'claims' labels, backlog L-023). About 8,000 triangles for the solo and 16,000 for the triple.
- `make_wood_textures.py` writes a tileable procedural wood (rings, pores, fibres; 0.6 m per tile). Materials `Mat_CaseWood`, `Mat_CaseBack` and `Mat_CaseBrass` are created by `SetupCaseAssets()`.
- Check: `unity command eval_file tools/verify_case_mesh.cs` (width, front toward -z, height under the ceiling, board tops at the shelf heights, three submeshes).
- Axis note: as for the book, export `axis_forward='Z'`, `axis_up='Y'` and use Convert Units; the front (-Y in Blender) ends up toward Unity -z.

## The finds: apron and shoes (2026-10-19)

```
blender -b --factory-startup --python-exit-code 3 -P tools/blender/make_finds.py -- C:/Code/BookBoy/Assets/_Project/Art/Models C:/Code/BookBoy/tools/blender/finds_preview
python tools/blender/make_find_textures.py C:/Code/BookBoy/Assets/_Project/Art/Textures/Finds
```

- `make_finds.py` builds the apron (canvas twill, leather-bound patch pockets, mended patches, brass buttons, linen tapes, a paper tag on string) and the pair of mended leather shoes with felt soles (laces, eyelets, collar, tag) at real size, lying flat like a book (x thickness / up), normalises each to a unit box and writes `find_dims.json` (real sizes in Unity axes). 7 materials each, in slot order (apron: canvas, pocket, leather, brass, tape, paper, patch; shoes: leather, felt, lining, brass, lace, paper, dark leather). The script sorts the faces by material, because the importer builds submeshes in order of first use and the bevel scrambles the face order.
- `make_find_textures.py` writes five tileable surfaces (canvas twill, leather, felt, linen, paper); the `Mat_Find_*` materials tint them.
- After either script run `LamplitBuilder.SetupFindAssets()`, then `BuildHub()` and `BuildRoom1()`. Check: `tools/verify_find_meshes.cs`. Quick looks: `tools/blender/finds_preview/` (Workbench renders) and `tools/finds_preview/` (in-engine).
