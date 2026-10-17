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
| `PlayerCameraRoot` local Y | `0.7` | `Player` transform's own baseline Y offset is `1.0` (a pre-existing rig quirk, not touched) — `0.7 + 1.0 = 1.7` measured eye height. If `Player`'s baseline ever changes, recompute this. |
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
| Bookcase spacing | 0.5 m centers, z = 5.67 | A wall of four, 0.06 m gaps, against the back wall (front face at z = 5.85) |
| Slot pitch | 0.072 m | Widest book (0.055 m) plus a little: "barely wider than a book" |
| Slots per shelf | 4, centered, at local z = +0.02 | Volume N in slot N, left to right |

Each `Shelf` object's origin is on its board's top surface; it carries a trigger `BoxCollider`
(size `0.36 x 0.40 x 0.34`, center `(0, 0.2, 0)`) that the interact ray uses to find it, and 4 `ShelfSlot`
children. The aimed slot is chosen where the ray crosses the plane the books stand in (not at the front of the
volume), so off-axis aiming stays accurate with slots only 7 cm apart. A shelved book stands at
`slot.position + up * (bookHeight / 2)`. Keep the 0.40 m volume under the 0.45 m board spacing minus the
board thickness, or neighboring shelves overlap.

Series differ by shade and size so a player can tell them apart: series A is the base color at the sizes in
the table above, series B is a lighter shade and a different size (thickness 0.045-0.055, height 0.24-0.30,
depth 0.17-0.18). Volume numbers are 0.03 m pixel digits near the bottom of the spine.
