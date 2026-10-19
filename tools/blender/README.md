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
