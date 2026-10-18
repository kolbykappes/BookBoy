# Scale Reference

BookBoy uses Unity's standard convention: **1 unit = 1 meter.** Everything below is grounded in
real-world human/book/furniture proportions, corrected after playtest feedback found the original
Phase 2 spec (see the note in [05-implementation-plan.md](05-implementation-plan.md)) was roughly
2-3x oversized — a 2.38m player eye height and a 3.48m-tall shelf, both well beyond real-world
scale, which is what actually caused "the shelf is 10 feet tall" to be an accurate complaint, not
an exaggeration.

**Rule of thumb: if you're placing a new object and unsure of its size, ask "how big is this in
real life, in meters?" before picking a number.** Don't scale relative to other in-scene objects
that haven't been checked against this reference — errors compound.

## Player

| Measurement | Value | Reasoning |
|---|---:|---|
| Eye height above floor | **1.7m** | Standard estimate for a 6'0" (1.83m) person — eyes sit roughly 4-5 inches (~0.12m) below the top of the head. |
| `PlayerCameraRoot` local Y | `1.61` | The `Player` transform starts at Y `1.0` but the `CharacterController` (center `0.93`, height `2.0`) settles on the floor at Y `0.09`, so eye height = `0.09 + 1.61 = 1.70`. Verified live: camera Y reads `1.700` after settling. (It was `0.7`, which gave a real eye height of only `0.79` m - the earlier `0.7 + 1.0` arithmetic used the starting Y, not the resting Y. If the controller's center/height/skin change, recompute.) |
| `CharacterController` height/center | `2.0` / `(0, 0.93, 0)` | Left as-is (pre-existing rig) — not touched during the eye-height fix. Worth revisiting for full consistency (closer to `1.83`) if it ever matters for gameplay, but no reported issue with it specifically. |

## Books ("tomes")

Real-world reference: a standard hardback novel is about 20-24cm tall. BookBoy's books are
magical library "tomes" — larger and more dramatic than a paperback, but still a book, not a
prop the size of a filing cabinet.

| Dimension | Range | Real-world equivalent |
|---|---:|---|
| Height | 0.22m – 0.32m | ~8.5" – 12.5" — a big reference-book-sized hardback |
| Thickness (spine) | 0.03m – 0.06m | ~1.2" – 2.4" — normal book thickness |
| Width (page depth) | 0.15m – 0.19m | ~6" – 7.5" — normal page width |

**Orientation matters**: a book standing on a shelf shows its **spine** (the thin edge) to the
room, not its wide cover face. `Book` GameObjects use local scale `(thickness, height, width)` —
thickness on X (the horizontal, side-by-side shelf-run axis), height on Y, width on Z (the
front-to-back depth axis, going into the shelf). Getting X and Z swapped makes books look like fat
blocks instead of slim spines — this was an actual bug caught and fixed once already.

Current per-category dimensions (`Assets/_Project/Scripts/Books/` scene objects,
thickness/height/width):

| Category | Scale (X, Y, Z) |
|---|---|
| Alchemy | `(0.045, 0.24, 0.16)` |
| Astronomy | `(0.04, 0.28, 0.18)` |
| Beasts | `(0.05, 0.32, 0.15)` |
| History | `(0.055, 0.22, 0.19)` |

## Bookcases and shelves

Updated 2026-10-17 (the earlier wide boards at z=5.1 and the `ShelfBack` slab were removed).

| Measurement | Value | Reasoning |
|---|---:|---|
| Bookcase footprint | 0.44 x 0.36 m (W x D) | Narrow tower: one shelf holds exactly one packed 4-book series |
| Bookcase height | 1.79 m | A real tall bookcase, under 2 m; the top board is above eye height (1.7 m) |
| Base cabinet | 0.85 m tall, solid | Lower cabinet like the reference pictures; shelf 1 sits on it |
| Shelf tops | 0.85 m and 1.30 m | 0.45 m apart: clears the tallest book (0.32 m) with room for a hand |
| Bookcase spacing | 0.5 m centers, z = 5.67 | A wall of sixteen (x = -3.75 to 3.75), 0.06 m gaps, against the back wall (front face at z = 5.85) |
| Slot pitch | 0.072 m | Widest book (0.055 m) plus a little: "barely wider than a book" |
| Slots per shelf | 4, centered, at local z = -0.14 (the books' front line) | Volume N in slot N, left to right; the shelf's front edge is at z = -0.17 |

Each `Shelf` object's origin is on its board's top surface; it carries a trigger `BoxCollider`
(size `0.36 x 0.40 x 0.34`, center `(0, 0.2, 0)`) that the interact ray uses to find it, and 4 `ShelfSlot`
children. The aimed slot is chosen where the ray crosses the plane the books stand in (not at the front of the
volume), so off-axis aiming stays accurate with slots only 7 cm apart. A shelved book stands at
`slot.position + up * (bookHeight / 2) + forward * (bookDepth / 2)`: the slot origin is a front line 3 cm inside the board's
front edge, and every book's spine face is placed on it whatever its depth, so books sit flush near the front where the
numbers are readable (they used to be centered in the shelf depth, ~9 cm back). Keep the 0.40 m volume under the 0.45 m board spacing minus the
board thickness, or neighboring shelves overlap.

Series differ by shade and size so a player can tell them apart: series A is the base color at the sizes in
the table above, series B is a lighter shade and a different size (thickness 0.045-0.055, height 0.24-0.30,
depth 0.17-0.18). Volume numbers are 0.03 m pixel digits near the bottom of the spine.

## Lamplit uses a "storybook" scale (2026-10-18) - this supersedes the life-size rule above for the Lamplit scene

The life-size rule above made the books too small to read and the rooms too distant (Kolby's comparison with Witch Cabin Cleanup: books there fill
about twice as much of the screen, and a pile is a few chunky, readable books). Measured and decided:

- **Lens:** vertical field of view **75 degrees** (was 40, a telephoto look). Witch Cabin's default is 90. A wider lens alone makes everything look
  *smaller*, so it needs the next two changes with it.
- **Player:** eye height **1.15 m** (was 1.7), walk 3.2 and sprint 4.8 m/s, reach 2.8 m (aim 12 m), held books drawn at 0.55. A smaller player
  makes the whole world feel big and close.
- **Everything the player handles is 1.6 times life size** (`LamplitBuilder.WorldScale`): containers, items, finds and the desk props. A book is about
  0.38 x 0.26 x 0.07 m. A container is 1.0 m wide, 2.34 m tall, with a 0.96 m base cabinet, boards at 0.96 m and 1.68 m (0.72 m apart) and a slot pitch
  of 0.152 m. Book spines are 0.09 to 0.14 m thick (2026-10-18: boards lowered so the eye line at 1.15 m is inside the shelf band; then thicker spines, wider slots).
  **Rooms (2026-10-18):** ceiling 3.0 m (2.6 x the eye height; it was 4.2 m with no ceiling at all, which read as a giants' hall), doors 1.5 m x 2.2 m (were 2.0 x 3.0),
  magelights at 2.55 m, family signs at 2.65 m. Rooms, doors and the hub are sized to hold them: rooms 12 m wide, 11.4 to
  13.4 m deep, walls 4.2 m, doors 2.0 x 3.0 m, hub 4.8 x 24.8 m.
- **One number:** change `WorldScale` (and the player values in `ApplyPlayerScale`) and rebuild the hub and rooms with `LamplitBuilder`.
- The prototype scene (`LibraryPrototype.unity`) keeps the life-size values.
