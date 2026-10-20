# 31 - Score feedback: motes to the counter, the finished-shelf celebration, series and standalone books

Written 2026-10-19. This is DESIGN from the competitor screenshots (no new web research: the techniques are plain Unity UI and a small additive shader, nothing fenced). Sources are our own competitive notes: `Docs/competitive/16-witch-cabin-cleanup.md` (screenshots 33, 35, 42, 44 and 45) and `scale-and-camera-comparison.md` (Librarian).

## What the reference does

- Witch Cabin: when the last slot of a shelf fills, the items pulse and flash gold one after another, the whole case glows pale gold, faint "+1" labels float up, gold hexagon tokens and a purple gem arc out toward the top-right counters, sparkles hang in the air; a banner when a room is cleared. Per placement: a spin into the slot and a particle that flies to the counter as the item lands.

## What we built (Lamplit)

1. **Per correct placement:** a gold mote flies from the book, along a curved path with a fading trail, to the ITEMS counter at the top left; when it lands the stats panel pops and a gold bar lights behind that line (`ScoreFx`, `GameUI.PunchStat`). The counter is recounted at once when the book is placed (the 0.25 s timer would be late).
2. **Finished shelf:** the books hop in a wave left to right (0.075 s apart, 3.5 cm, each flashing gold through the existing outline), a gold sheet lies over the row with a sweeping bright band and a pulsing glow (shader `BookBoy/ShelfSweep`), gold motes rise, a "Shelf complete: <series title>" line shows, and a bigger gold hexagon flies to the SHELVES counter after the wave and pops it (`ShelfFlourish`). Played once per finished state: taking a book out and putting it back plays it again.
3. **Quiet when tidied by admin command** (`Book.QuietPlacement`): no motes, no celebrations.
4. No project setting, layer, renderer feature or input binding is touched. Everything is pooled; the world effect creates and destroys one small object.

## Series and standalone books

- A **series** book is numbered; volume N belongs in slot N (#1 leftmost): `BookDefinition.ordered`. A **standalone** book is any slot of the right shelf. Engine-free rule: `PlacementRules.ModeFor(containerOrdered, itemInSeries)` (unit-tested). The first book on a row still claims it for its set, and a set still belongs on a row of its own size.
- Demo books in Room 1: six printed series A to F (Lamplighter's Handbook, Lamplit Catalogue, Letters of the Founder, Atlas of the Marsh Coast, The Art of Mending, Shelfmarks of the Vault; 8 volumes, or 4 for the odd rows) and two standalone groups: **Gardening** (8 different titles on an 8-row) and **Cookery** (4 different titles on a 4-row), each book with its own spine and its own cover from a cover atlas.

## Unknowns

How the celebration feels in motion and how loud it is at the 20th shelf (tone down the sheet or the hop first); the sheet reads a little flat on very bright books; whether the HUD target (the middle of the number) is right at other aspect ratios (it is computed from the text layout, tested at 16:9 only).
