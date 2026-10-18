# MVP Spec

## MVP scope

The first end-to-end playable MVP is **one room, 128 books (16 colors x two series x four volumes), 16
bookcases with two shelves each (32 shelves), and one complete sorting loop with light progression**. (Was
twelve books and exact-slot placement until 2026-10-17; see [04-build-status.md](04-build-status.md).)

> **Scope decisions made by request, 2026-10-17 (these override items that were on the non-goals list below):**
> a run timer whose result is your score (time to finish the room), upgrade points earned per finished shelf, a small
> tech tree to spend them on, and unlockable abilities. They were requested explicitly; the "hard timers / score
> attacks / economy" non-goals were written to keep the first loop simple and are relaxed for these features only.

> **Scope clarification, 2026-10-17 (Kolby):** this spec describes the *prototype*, a mechanics and Unity-learning
> test. The real game is larger (multiple zones, persistent progress, per-zone leaderboards, a story), and the
> non-goals below (save/load in particular) apply to the prototype only. See
> [13-concept-draft-inherited-library.md](13-concept-draft-inherited-library.md).

## Player experience requirements

- First-person player can walk, look around, collide with geometry, and approach shelves.
- The room contains 128 scattered books.
- Books are visibly distinct at normal player distance.
- There are 16 colors, each with two series of four numbered books (volumes 1-4).
- Player can identify a book's intended section using color and/or a clear category marker.
- The player carries several books at once (starts at 4; upgrades raise it up to 10), can cycle which one is current
  (R or mouse wheel), and can toss the current one (Q or right mouse button): it tumbles with real physics and settles
  where it lands.
- Player can place a carried book on any shelf; the slot under the crosshair is used. E or the left mouse button picks
  a book up / places the current one. Once a placement is correct (green) the readout in the middle of the screen moves
  on to the next book in the hands (and a finished green book is not picked up while the hands are busy).
- Every bookcase has a fixed magical theme (Healing Magic, Fire Magic, Forbidden Lore, ...) named on a plaque on the
  wall above it; the player is told the theme, never the color ("Healing" books are the white ones). Book titles use
  the theme word ("Healing Tome Vol. 2"). Shelves are unlabeled: the first book shelved on a shelf claims it for that
  book's series (the earlier claim wins a conflict); the other books on it must match.
- Volume N goes in slot N, left to right. Slots are barely wider than a book.
- Feedback is a flash on the book: red = wrong bookcase (color), yellow = right bookcase but wrong shelf
  (series) or wrong slot, green = right bookcase, shelf and slot.
- A wrong placement is never refused and never penalized; the player can pick the book back up.
- A crosshair dot shows what is targeted and whether it is in reach. A targeted book, a flash on placement, and the
  sister glow are drawn as an outline with sparkles; the book's own color never changes.
- Finishing a shelf (4 books of one series in order) earns 1 upgrade point, once per shelf. Tab opens the upgrade menu
  (capacity, move speed, four abilities and their refinements; the whole tree costs exactly the 32 points available).
  F10 opens an admin menu for testing (points, unlock all, reset, reset timers, pile every loose book / every book in
  the middle of the room).
- Four abilities, all locked at the start, each bought with points: **G Sister Glow** (the held book's unshelved sisters
  glow), **F Quick Shelve** (every held book flies to its right place), **C Gather Family** (pulls 2 loose books of the
  held book's series to you, upgrades make it 3 and then the whole series; limited by free hand space; sparkle trail),
  **B Home Beacon** (the held book's bookcase glows around its outer edges for 8 s). Each ability has its own timer: it is
  ready when bought, goes on a 60 s recharge when it actually does something, and recharge upgrades make it 40 s then 20 s.
- The HUD shows the run time, correct books out of all books, finished shelves, upgrade points, what is carried, a
  controls legend that marks locked abilities, and one recharge bar with a countdown per unlocked ability (on the middle of
  the right side, so it never covers the carried books in the lower right).
- Menus never free the Windows cursor: it stays locked and hidden, and the menus draw their own pointer that moves with
  the mouse (left click chooses a row; keyboard also works). Carried books get a soft light so they stay bright in the dim room.
- The room is lit by four warm floating "magelights" (soft point lights, two with soft shadows) and a dim shadowless
  moon fill instead of one directional light; the floor is a tiled marble texture.
- Completing all 128 correct placements shows `The Arcane Library Is Restored` and a simple restart option.

## Initial categories

Use four highly readable, visual-first magical disciplines:

| Category | Color family | Symbol / shelf marker | Initial role |
|---|---|---|---|
| Alchemy | Amber / gold | Flask | Easy visual sorting |
| Astronomy | Indigo / deep blue | Star | Easy visual sorting |
| Beast Lore | Forest green | Leaf or claw | Easy visual sorting |
| Forbidden History | Burgundy / crimson | Eye or key | Easy visual sorting |

For the earliest build, color is enough. Symbols are a useful second visual language. Do not make
tiny text the primary classification mechanic.

## Explicit non-goals for the MVP

Do not build these yet:

- Multiple rooms or a full library.
- Procedural generation.
- Save/load.
- Inventory grids.
- Physics-driven shelves or realistic book-toppling simulation.
- Combat, hazards, fail states, score attacks, or hard timers.
- NPCs, dialogue, quests, economy, shopkeeping, crafting, achievements, Steam integration, or
  co-op.
- Mobile support.
- Final art, final sound, complex animation, voice acting, or generated lore.
- Runtime LLM/NPC systems.
- Complex alphabetization, fake Dewey Decimal systems, or text-heavy catalog workflows.

**Guiding test:** if a feature does not make **pick up → identify → place → feel good** better,
it is not an MVP feature.

## Definition of success today

A successful session ends with a short playable loop that can be demonstrated without
explanation:

1. Launch Unity scene.
2. Walk into the library.
3. See books scattered in front of shelves.
4. Pick up an amber Alchemy book.
5. Carry it to the amber/flask shelf slot.
6. Place it and hear/see a satisfying confirmation.
7. Repeat for the remaining books.
8. Complete the room and see the restoration result.

If that works, the project has moved from infrastructure setup into actual game development. The
next decision is then based on evidence: improve interaction feel, deepen the book logic, improve
atmosphere, or broaden the content.
