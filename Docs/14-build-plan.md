# Build Plan v2: The Hub and Rooms 1 to 3

Written 2026-10-18 (replaces the longer first draft). Concept and story: [13-concept-draft-inherited-library.md](13-concept-draft-inherited-library.md).

**Goal:** get the core of the game working, small enough to build overnight and test the next day:

1. **A base**: a hallway with a desk at the entrance, with spots where finds go in and unlock abilities.
2. **Rooms 1 to 3**: shelving books, handling different item types, and putting things in order, plus finding things.
3. **An admin view** to reset and test.

**Not in this build:** save/load, story text, the door notice and corkboard, maintenance jobs, placement ghosts and
completion pulses, sound, Rooms 4 to 7, leaderboards, settings. Say so before adding any of them.

## What we reuse (why this is smaller than it looks)

The prototype's model already fits: **category = item family, series = group, volume = number in an ordered set**. A
bookcase has a fixed family, a shelf is claimed by the first group placed on it, and slot N holds volume N.

| Prototype piece | Use in this build |
|---|---|
| `Book`, `BookDefinition`, `BookOutline`, feedback colors | Every item is a `Book`-style item with a different mesh |
| `Bookcase` / `Shelf` / `ShelfSlot` | Containers: shelves, racks, pigeonholes, bins, drawers, tube racks, all as shelves with slots |
| `PlacementRules` | Add a **Group** mode (any slot is fine); **Ordered** mode already exists |
| `CarryInventory`, toss, `PlayerInteractor` | As is |
| Abilities and `UpgradeState` | Finds switch the existing abilities on (below) |
| F10 admin menu (`AdminTools`) | Extended |
| `tools/` scene scripts | Copied to build the new scene repeatably |

`GameProgress` (the timer, points and tech tree) stays in the prototype scene only.

## The base

A hallway about 3 m wide and 12 m long. The **desk is at the entrance end**, with three doors to Rooms 1, 2 and 3 along the
walls. Prototype-grade geometry and the existing warm lighting.

| Spot at the desk | You put in | Effect (existing code) |
|---|---|---|
| Apron hook | Apron | Carry capacity +2 |
| Shoe rack | Soft-soled shoes | Move speed +15% |
| Glasses stand | Reading glasses | Sister Glow (highlight matching items) |
| Shelf-list board | Shelf list | Home Beacon (the right bookcase glows) |
| Clipboard | Pull list | Gather Family (pulls matching items to you) |
| Dumbwaiter hatch | (locked until later) | Quick Shelve |

Spots accept only their own find. Installing is a short move-and-snap with a toast. A room's door opens when the previous
room is finished.

## The three rooms

| Room | Items (about) | Containers | Rule | Finds hidden here |
|---|---|---|---|---|
| **1 Reading Room** | 100 books: 20 groups of 5 | 20 five-slot shelves | **Group** (first book claims the shelf) | Apron, shoes |
| **2 Mailroom** | 120: books, magazines, letters, parcels, 6 groups of 5 each | Shelves, slanted racks, pigeonholes, bins (one family each) | **Group**, plus the container's family must match the item | Glasses, shelf list |
| **3 Map Room** | 120: atlases, rolled maps, flat sheets, 8 numbered sets of 5 each | Shelves, tube racks, flat-file drawers | **Ordered** (number N in slot N) | Pull list |

Rooms finish when every item is correct: a short "Room tidied" banner and the next door opens. About a dozen simple shapes in
total (book, atlas, magazine, letter, parcel, tube, sheet, plus the containers), each reused with color and symbol variants.
Every group carries a symbol or word as well as a color, so color alone never decides.

## Admin view (extends F10)

Reset this room, reset everything, scatter the current room again, complete this room, jump to the hub or any room, give or
install any find, unlock all abilities, lock or open doors, and show counts (items, shelves, finds).

## Steps in order, with stop lines

| Step | Work | Done when | Stop line |
|---|---|---|---|
| S0 | New branch `lamplit/day-1`; new scene `Assets/Scenes/Lamplit.unity` (the prototype scene is untouched) | Opens and runs | |
| S1 | Rules: **Group** mode and the container-family check (engine-free) with Edit Mode tests | Old and new tests pass (not 0/0) | |
| S2 | Campaign state: finds collected and installed, rooms finished, door state (engine-free) with tests | Tests pass | |
| S3 | The hub: hallway, desk, six spots, door stubs, player start | You can walk it | **Stop A**: walkable base |
| S4 | Room 1: 100 books, 20 shelves, group grading, completion banner, apron and shoes placed | Room 1 can be finished | **Stop B**: Room 1 playable |
| S5 | Finds and installs: pick up a find, put it in its spot, the effect applies | Apron raises carry; shoes raise speed | **Stop C**: the core loop works (hub, find, upgrade) |
| S6 | Room 2: four item families, containers, family check, glasses and shelf list placed | Room 2 can be finished | **Stop D** |
| S7 | Room 3: ordered sets across three container types, pull list placed | Room 3 can be finished | **Stop E** |
| S8 | Admin menu extension | Every listed command works | **Stop F**: everything |
| S9 | Play Mode check scripts in `tools/`, docs updated | Scripts pass on the live Editor | |

Each step is its own commit with tests. Honest forecast: scene building is the slowest and most fragile work, so **Stop C
is the realistic overnight target, with Stops D to F as stretch**. Every stop leaves the project working.

## Tomorrow's test script

1. Open `Lamplit.unity`, press Play, and start in the hub at the desk. *(S3)*
2. Walk to Room 1. Place the first book on an empty shelf; it claims it. A different group on that shelf grades wrong; a matching
   one in any slot grades green. *(S1, S4)*
3. Fill a shelf with five matching books: it counts as finished. *(S4)*
4. Find the apron and the shoes in Room 1, carry them to the hub, and install them. Carry reads 6 and you move faster. *(S5)*
5. Finish Room 1: the banner shows and Room 2's door opens. *(S4)*
6. Room 2: letters, parcels, magazines and books each refuse the wrong container; each fits its own. Install the glasses and
   shelf list; Sister Glow and Home Beacon work. *(S6)*
7. Room 3: numbered sets only grade green in order. Install the pull list; Gather Family works. *(S7)*
8. F10: reset a room, give a find, jump rooms, complete a room. *(S8)*

Please note: how long Room 1 takes (the target is about 20 minutes), whether five-item groups feel right, and whether colors and
symbols are easy to tell apart.

## Before bed

Open the project in Unity and leave the Editor **open in Edit Mode** (not Play Mode). An unfocused Editor defers compilation
(see [../CLAUDE.md](../CLAUDE.md), rule 2), so keep it focused and stop the PC sleeping; I will request keep-awake at the start.

## Risks

- **Scene building** is fragile. Mitigation: scripted in `tools/`, saved often, nothing edited on disk while the scene is open.
- **Editor focus and compile deferral** can stall the work overnight (see above).
- **New item shapes** are placeholders (cubes, cylinders, quads). That is intentional: this is a feel and flow test.
- **Group size equals container capacity (5)**, so a group never needs two containers. Revisit after Room 2.

## After this build

Rooms 4 to 7, save/load, the story layer (notice, corkboard, notes), the feel pass and sound, per
[13-concept-draft-inherited-library.md](13-concept-draft-inherited-library.md) and the backlog.
