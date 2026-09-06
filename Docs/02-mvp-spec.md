# MVP Spec

## MVP scope

The first end-to-end playable MVP is **one room, twelve books, four shelf categories, and one
complete sorting loop**.

## Player experience requirements

- First-person player can walk, look around, collide with geometry, and approach shelves.
- The room contains 12 scattered books.
- Books are visibly distinct at normal player distance.
- There are four categories with three books each.
- Player can identify a book's intended section using color and/or a clear category marker.
- Player can pick up exactly one book at a time.
- Player can only place a carried book in its explicitly correct shelf slot.
- Correct placement snaps reliably into a visible slot.
- Wrong placement gives a gentle "this belongs elsewhere" response and never loses the book.
- HUD reports `Books Restored: X / 12`.
- Completing 12 placements shows `The Arcane Library Is Restored` and a simple restart option.

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
