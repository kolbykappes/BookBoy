# MVP Spec

## MVP scope

The first end-to-end playable MVP is **one room, thirty-two books (four colors x two series x four
volumes), four bookcases with two shelves each, and one complete sorting loop**. (Was twelve books and
exact-slot placement until 2026-10-17; see [04-build-status.md](04-build-status.md).)

## Player experience requirements

- First-person player can walk, look around, collide with geometry, and approach shelves.
- The room contains 32 scattered books.
- Books are visibly distinct at normal player distance.
- There are four colors, each with two series of four numbered books (volumes 1-4).
- Player can identify a book's intended section using color and/or a clear category marker.
- Player can pick up exactly one book at a time.
- Player can place a carried book on any shelf; the slot under the crosshair is used.
- Bookcases and shelves start unlabeled. The first book shelved claims the bookcase's color and the
  shelf's series (the earlier claim wins a conflict); all other books on them must match.
- Volume N goes in slot N, left to right. Slots are barely wider than a book.
- Feedback is a flash on the book: red = wrong bookcase (color), yellow = right bookcase but wrong shelf
  (series) or wrong slot, green = right bookcase, shelf and slot.
- A wrong placement is never refused and never penalized; the player can pick the book back up.
- A crosshair dot shows what is targeted and whether it is in reach.
- HUD reports `Books Restored: X / 32`.
- Completing 32 correct placements shows `The Arcane Library Is Restored` and a simple restart option.

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
