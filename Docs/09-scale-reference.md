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

## Shelf

| Measurement | Value | Reasoning |
|---|---:|---|
| Board spacing | 0.45m | Fits books up to ~0.32m tall with clearance for a hand to reach in. |
| Board thickness | 0.05m | ~2" — a normal shelf board, not a slab. |
| Board Y positions | `0.35, 0.80, 1.25, 1.70` | Bottom shelf at comfortable reach-down height; top shelf at ~eye level (1.7m) rather than towering above it. |
| Total shelf unit height | ~1.75m | Under 2m — a real tall bookcase, not an industrial rack. |

`ShelfBack`, the walls, and the floor were **not** rescaled — only the shelf boards, books, and
player eye height. A tall-ceilinged room with a human-scale bookcase inside it is a normal, even
appealingly grand, library layout; the oversized *shelf* specifically was the actual problem.

## Shelf slots

Each `ShelfSlot`'s position is derived, not hand-placed: `slotY = boardTopY + (book height / 2)`,
so a slot always sits flush on its board regardless of that category's specific book height. The
visible marker panel (see [04-build-status.md](04-build-status.md)) is `1.6m` wide × `0.6m` deep,
and the slot's own hit-detection `BoxCollider` is sized to match it — the two must stay in sync,
or the panel look clickable in an area that isn't (an actual bug hit once: the panel was widened
without widening the collider underneath it, making most of the "shelf" area unresponsive).
