# Changelog

A dated log of what has been completed, newest first. Add an entry when a piece of work lands (same day, one short line per
change, with the commit if there is one). Planned work lives in [10-backlog.md](10-backlog.md); the current state of the game is
in [04-build-status.md](04-build-status.md). The full record is `git log`; this is the readable version.

Format: `### YYYY-MM-DD` then bullets. Entries before 2026-10-17 were backfilled from the git history on 2026-10-17, so they are
grouped by commit date, not by hour.

---

### 2026-10-18

*Lamplit build (branch `lamplit/day-1`, plan: [14-build-plan.md](14-build-plan.md))*
- S1: Group placement mode, the container family check and campaign grading rules, engine-free with tests (`16f6a5f`).
- S2: campaign state (finds, installs, rooms, doors) and `UpgradeState.Grant`, engine-free with tests (`efed941`).
- Campaign runtime in the game code: item fields (family, group, number, room, symbol, find), Lamplit grading on `Book` and
  `Bookcase`, `RoomZone`, `RoomDoor`, `FindSpot`, `CampaignController`, a finds list on Tab (`00b8ce2`).
- S0: new scene `Lamplit.unity`, a copy of the prototype with the library content removed; the prototype scene is untouched (`2d4fe4a`).
- S3: the hub - hallway, librarian's desk with six find spots, three doors (Stop A) (`660312a`).
- S4: Room 1, the Reading Room - 100 books in 20 groups of 5, Group grading, finds on a side table (Stop B) (`b6f3b6e`).
- S5: finds and installs - apron raises carry to 6, shoes raise speed 15%, wrong spots refuse (Stop C, the core loop) (`1787c4a`).
- S6: Room 2, the Mailroom - four item families, the family check, glasses and shelf list (Stop D) (`6dffd34`).
- S7: Room 3, the Map Room - ordered sets across three container types, the pull list (Stop E) (`4002833`).
- S8: admin menu for Lamplit (Stop F) (`33596aa`).
- S9: Play Mode check scripts for every step plus an end-to-end run of the whole test plan; docs updated.
- Piles: each room starts as several random piles tossed around the middle (6, 7 and 8 piles), baked into the scene with a physics
  settle in the Editor; a new "pile up the books" feature (`RoomZone.PileUp`, `CampaignController.PileRoom`, admin menu) tosses a room's
  books into fresh random piles with real physics; engine-free `PilePlanner` with tests.
- Rooms beyond a shut door are switched off until the door opens: live books 345 to 102, renderers 950 to 306, colliders 658 to 212, lights 14
  to 8 at the start of a run (`RoomZone.SetLive`, `CampaignController`).
- Backlog: data-driven items, rooms loaded when unlocked, inert shelved items, frozen piles and physics only on special occasions written up as L-016 to
  L-020 (Kolby's direction), with open decisions listed; not started.
- Storybook scale for Lamplit: FOV 75, eye 1.15 m, and a world 1.6x life size (containers, items, finds, desk; hub and rooms resized to hold them), after a
  comparison with Witch Cabin Cleanup showed our books were too small and the view too distant; builder now removes orphaned items on rebuild
  (`LamplitBuilder.WorldScale`, `ApplyPlayerScale`).
- Six printed cover and spine designs (LAMPS, CATALOGUE, LETTERS, ATLAS, MENDING, VAULT; tintable, high resolution, eight volume cells per spine) on a styled tome mesh with a separate spine submesh; Room 1 sets A to F use them. The held books are now a Librarian-style flat stack at the lower right that rises into view when looking down and pulls back near bookcases (T-004, T-010).
- Placement ghost (a translucent copy of the held book in the aimed slot, tinted by how it would grade), brass claim plates showing each shelf's set (icon and letter), and a pooled glow whose hull covers the page block (L-022, L-023, T-013).
- Lighting pass (scene-level): brighter orbs (5.0, range 9), measured before and after with post-processing on; pipeline-setting changes proposed, not applied (T-003).
- Palette: 24 colours solved for contrast against the wood and for colour-blind separation (BookBoy.Rules.Palette, tested); spines now carry a set icon, a letter and a volume number on three plates (T-010).
- Lamplit bookcases: carved Blender meshes (stepped plinth, reeded pilasters, dentil cornice, arched crest, brass nameplate plates) with a procedural mahogany wood material, one shared mesh for the triple (Mat_CaseWood, Mat_CaseBack, Mat_CaseBrass; tools/blender/make_cases.py).
- PerfLab: a performance test scene with up to 3000 tome books in baked piles, an overlay of frame time and draw calls and hotkeys for book count, lighting presets, book shadows, GPU instancing and colour mode (T-018 stage A); the pile planner can make many piles.
- Lamplit: books are now a Blender-built tome mesh (rounded spine, boards, page block) with a sample cloth cover material (Mat_BookCover and Mat_BookPages); bookcases have 2.4 in boards, packed slots, 1 in of headroom, a plinth lip and a crown; held books at full size (T-004, T-017).
- Fixed: nothing could be picked up at the new eye height (the aim ray hit the player's own collider first); the interactor now ignores the player's colliders; new check tools/verify_lamplit_pickup.cs.
- Lamplit tomes: books 12 to 15 in tall, 10 in deep, 4 in thick; bookcases resized (17 in shelf spacing, 15 in plinth, 7 ft tall, solo 37.6 in, triple 6'6.7" wide), top shelf at 5'5.7".
- Lamplit adult scale: hub and Room 1 rebuilt at real size (eye 5'5", ceiling 9 ft, FOV 90 horizontal) with a solo 8-wide and a triple (4+8+4) four-shelf bookcase, 96 books in sets of 8 and 4 that belong on a row of their own size; Rooms 2 and 3 not yet converted (T-002, T-017).
- Lamplit: the head light that washed out close views is dimmer and set back from the lens; rooms get ceilings and are scaled to the player (ceiling 3.0 m, doors 1.5 x 2.2 m);
  book spines about 1.45 times thicker with wider slots.
- Lamplit shelves: boards lowered so the eye line is inside the shelf band (lower board 0.96 m, upper 1.68 m), slots packed (pitch 0.10 m), containers closer together;
  palette contrast, a held-item ghost and visible claims added to the backlog (L-021 to L-023).
- Fixed: an unbounded `while` in three verify scripts hung the Editor (needed a kill and relaunch); loops are bounded now and the
  lesson is in `.claude/memory/unity-testing.md` (`dfb230f`).

### 2026-10-17

*Docs and process*
- Added this changelog and the [backlog](10-backlog.md); seeded the backlog with controller support, achievements and save/load.
- Refreshed the competitive research (new deep dives, screenshots index) (`611df0b`, `8aea8c2`).
- Witch Cabin Cleanup hands-on write-up from Kolby's 51 screenshots: settings, HUD, economy, the ledger, the tarot collection,
  spells in play, a 21-row "what to borrow" table, and a screenshot index
  ([competitive/16-witch-cabin-cleanup.md](competitive/16-witch-cabin-cleanup.md)).
- Narrative reference research: how successful games unlock story through a simple mechanic (Unpacking, Organized Inside, Obra
  Dinn, Outer Wilds, and more) ([12-narrative-reference-games.md](12-narrative-reference-games.md)).
- First concept draft for the real game: an inherited library, a missing relative, no magic, seven days (wings) with a
  mechanical ladder and a time and item budget ([13-concept-draft-inherited-library.md](13-concept-draft-inherited-library.md)).
- Build plan for Lamplit v2: the hub (a hallway with a desk) and Rooms 1 to 3, steps S0 to S9 with stop lines, a reuse map and
  a test script ([14-build-plan.md](14-build-plan.md)); backlog items L-000 to L-009.
- Story settled (Docs/13, "Story decisions v2"): Lamplit, the Lamp, Uncle Rufus has died and left the library in disarray
  as a test and training for you.
- Scope clarification recorded in [../CLAUDE.md](../CLAUDE.md) and [02-mvp-spec.md](02-mvp-spec.md): the non-goals describe the
  prototype; the real game must save. Backlog updated (new F-010 and D-001 to D-005, settings research folded into F-004, F-003
  approved in principle).

*Menus, HUD and lighting follow-up*
- Menus no longer free the Windows cursor: it stays locked and hidden and the menus draw their own pointer (`dc720bd`).
- Ability timer panel moved to the middle of the right side so it stops covering the carried books (`dc720bd`).
- Soft "hand light" on the camera so carried books stay bright in the dim room (`dc720bd`).

*Abilities, mouse, lighting, floor*
- Four abilities, each locked at the start and on its own 60 s timer with recharge upgrades (40 s, 20 s): Sister Glow, Quick Shelve, and
  the new Gather Family (pulls 2 / 3 / the whole series of loose books to you, with a sparkle trail) and Home Beacon (glowing frame
  around the held book's bookcase). The tech tree is now 24 upgrades costing exactly 32 points (`a9903fd`, `970ad6c`).
- Left click picks up / places, right click tosses; after a correct placement the readout shows the next held book (`970ad6c`).
- Admin menu: pile every loose book (or every book) in the middle of the room; reset ability timers (`970ad6c`).
- New lighting (four floating magelights, dim moon fill, night sky) and a tiled marble floor (`21596d8`).
- Docs updated (`37d4037`).

*Themes and wall signs*
- Every bookcase has a fixed magical theme (Healing Magic, Fire Magic, ...) named on a wall plaque above it; book titles use the
  theme word; colors are never shown to the player (`4df9bc2`, `27aef56`, `908a722`).
- Sister Glow made much more noticeable: thicker outline, halo, light, sparkles, x-ray through occluders (`997d391`).

*Scale-up and progression*
- Run timer (score is the time), points per finished shelf, tech tree, F10 admin menu, Tab upgrade menu (`9ecf278`, `f1c8e33`).
- Library expanded to 16 bookcases, 32 shelves and 128 books in 16 colors (`000e862`); non-goals relaxed for progression (`3eb44cc`, `c1c6d2d`).
- Carry several books, cycle them, toss with real physics (`7aa87c3`); engine-free rules for carrying, upgrades and quick shelve (`bffea2f`).
- Outline glow with sparkles replaces whole-book glow (`d556b02`).

*Feel and fixes*
- Player eye height raised to a true 1.7 m (`b1ef133`, `c1d6b6a`); camera shake off; frame rate capped to the display refresh when VSync is off,
  which fixed the mouse drag (`a17f84c`, `55dd300`, `5355752`).
- Books and slots moved to the front line of the shelf so books sit flush (`34baac2`, `55dd300`).
- Cursor hidden and locked during play (`6a575dc`).

*Shelving rework*
- Bookcases claim a color, shelves claim a series, volume N goes in slot N; crosshair added (`1cf972e`); 4 bookcases and 32 books (`f1e665b`);
  docs updated (`7c404a1`).
- Placement rules extracted into an engine-free assembly with the first Edit Mode tests (`6596672`).

*Tooling*
- Upgraded to Unity 6000.6.6f1 and added the Unity CLI Pipeline package (`7ba3b95`); adopted the Unity operating rules, CLI
  workflow and the `unity-editor-ops` skill (`d9b4dea`); Play Mode options and eval notes recorded (`f7413f5`).

### 2026-09-07

- Competitive research: negative Steam review analysis with exact numbers, and the abilities/unlocks of all 8 released competitors
  (`163d302`, `1fb469c`, `b290fd0`).

### 2026-09-06

*Project start*
- Arcane library greybox with a first-person controller (`c66cf16`); project docs and `CLAUDE.md` from the brief (`738ecec`).
- Competitive analysis docs for the 2026 Steam tidy-up wave, plus Kolby's hands-on impressions (`b499282`, `803aaf2`).

*First playable slice*
- Project folder structure and the book data layer (`727d80a`); visual book proof (`8a26e72`); pickup and single-slot placement (`849c9d7`).
- Expanded to 8 books and 8 shelf targets with a cleanup indicator (`3a30471`, `9ec739a`); Unity MCP tool setup documented (`17c99ba`,
  later superseded by the Unity CLI).
- Fixes: mouse look, shelf slot marker, books that block walking, drop on Q, movement drift, placement feel, Astronomy placement,
  human-scale proportions, sluggish horizontal look (`f2c38a4`, `ba341ca`, `10f5593`, `4c40329`, `c322131`, `d2e106f`).
