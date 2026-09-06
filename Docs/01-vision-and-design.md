# Vision & Design

## One-sentence pitch

**BookBoy is a first-person cozy sorting game in which the player restores a magical library by
identifying scattered books and returning each to its correct shelf slot.**

The emotional loop is simple and visual:

> Messy library → recognize a book → carry it → find its home → satisfying snap and feedback →
> library becomes orderly.

This is a small, practical vertical slice intended to test whether the physical act of sorting
books is satisfying enough to support a larger cozy organizing game.

## Product intent

The project is intentionally inspired by the emerging cozy "tidy up / organize thousands of
items" subgenre, but should become its own game rather than a literal clone.

The first playable version is an **arcane-library book restoration game**. Books are deliberately
chosen as the first item type because they offer more interesting design space than generic
supermarket products:

- Books can be sorted by school of magic, symbols, color families, eras, author lineages,
  magical materials, spell series, forbidden classifications, or catalog logic.
- Shelves can progress from simple visual categorization to light deduction puzzles.
- A magical library can support atmosphere, discovery, hidden collections, spells, strange
  artifacts, lore, and later special rooms without abandoning the core organizing loop.
- The setting supports strong visual identity while keeping the scope manageable.

The immediate goal is not to create a commercial-scale game. The immediate goal is to prove the
core interaction is tactile, readable, satisfying, and repeatable.

## Core gameplay loop

1. Player walks around a small library room in first person.
2. Player sees a scattered book.
3. The book has a visually readable identity, initially a color and magical-discipline category.
4. Player aims at the book and presses `E` to pick it up.
5. Player carries one book at a time.
6. Player looks at a shelf slot.
7. If the slot is correct, pressing `E` snaps the book neatly into position.
8. Correct placement creates immediate feedback: a snap, chime, glow, shelf progress, and an
   updated completion count.
9. Incorrect placement is gently refused. There is no punishment, hard failure, combat, health,
   countdown, or stressful score pressure.
10. When all books are returned, the room presents a simple completion screen.

```text
Find book
  → recognize its visual/category language
  → carry it
  → identify its destination
  → place it correctly
  → visual order increases
  → complete the room
```

The game should feel closer to deliberately putting things right than solving an abstract
spreadsheet puzzle.

## Design principles

**Cozy, no-fail play** — The game should not punish players for being slow or exploratory. The
desired structure is a relaxed task loop with optional completion ratings or soft-time goals
later, not a hard failure condition.

**Visual readability first** — A player should understand what to do from physical arrangement,
color families, symbols, shelf architecture, object silhouettes, and feedback. Text should
clarify, not carry the entire mechanic.

**Tactile reliability over physical realism** — Picking up and placing an object must be stable
and predictable. Do not sacrifice responsiveness for chaotic Rigidbody behavior. A book that
snaps cleanly into a slot is more satisfying than a book that realistically falls sideways every
third attempt.

**Visible transformation** — The payoff is not merely a counter. The player should watch a messy
space become ordered and beautiful. A completed shelf is a reward. A restored room is a reward.
Later, a finished exhibit or library wing can be an even larger reward.

**Small scope, complete loop** — Build a tiny complete game loop before building broad systems.
Every milestone should be playable and separately testable.

## Longer-term differentiation

The shelf-sorting market is rapidly filling with games whose pitch is roughly: "sort thousands of
themed objects onto shelves." The direct retail/store variants are already crowded.

BookBoy should avoid competing only on object count. The stronger differentiation is to turn
sorting into **classification, discovery, and curation**.

Potential evolution path:

```text
Simple color/category sorting
  → shelf ordering rules
  → symbols and magical catalog language
  → book series and relationships
  → condition/restoration
  → rare/forbidden books with special handling
  → room-level collections and displays
  → larger magical library / museum-backroom hybrid
```

Potential future mechanics:

- Identify books by magical symbols rather than only color.
- Arrange books in a deliberate sequence within a category.
- Use a staging/reading table to reveal a book's properties.
- Clean, repair, translate, authenticate, or bind damaged volumes.
- Unlock shelf tools, magical helpers, or quality-of-life abilities.
- Complete collections that transform a shelf, room, display, or story thread.
- Add rare books that lead to secret rooms or unusual puzzle rules.
- Expand toward the broader "Museum Backroom / Exhibit First" idea: not simply putting items
  away, but determining what they are, restoring them, and deciding where they belong.

The prototype should not attempt these yet. They are reasons the theme has strategic depth after
the basic sorting interaction works. See [02-mvp-spec.md](02-mvp-spec.md) for what's explicitly
out of scope right now.

## Competitive landscape

This project sits in a recent cozy organizing / high-volume sorting microgenre. The common
formula is a calming environment, thousands of objects, explicit destinations, visual cleanup,
light progression, and a small premium Steam price point.

| Game | Status / release timing | Core idea | What to learn |
|---|---:|---|---|
| `Librarian: Tidy Up the Arcane Library!` | Released April 30, 2026 | Return scattered books to proper places in an arcane library, with ability unlocks as shelf rows are completed | The closest direct reference and apparent catalyst for the current book/library sorting wave |
| `Supermarket Chaos` | Released June 29, 2026 | Sort 4,668 products in a messy supermarket | A literal retail version of the dense sorting loop |
| `Shelves and Sorcery: Tidy Up the Enchanted Shop` | Released August 11, 2026 | Organize thousands of items in fantasy shops, with abilities and leaderboard play | Magical theme plus light progression and repeated runs |
| `Sort Them Ducks` | Released August 13, 2026 | Sort more than 4,000 unique rubber ducks onto correct shelves | Extremely clear collectible premise and visual theme hook |
| `Konbini Cleanup` | Released August 14, 2026 | Sort 2,928 convenience-store products and restore order | Setting/atmosphere as a major differentiator |
| `Shelve the Potions!` | Released August 24, 2026 | Sort 2,000+ potions based on symbols, clues, and unique shelf ordering rules | Most relevant mechanical reference for using deduction instead of only visual grouping |
| `Too Many Toys!` | Released August 28, 2026 | Sort 5,517 toys in a toy shop | Evidence that raw object-count/shelf sorting is quickly becoming formulaic |
| `Plushy Store Tidy Up` | Released September 1, 2026 | Sort plushies on shelves in relaxed or challenge modes | Useful relaxed-mode versus correctness-challenge framing |
| `Megastore: Tidy Up Together` | Released September 2, 2026 | Solo/co-op organization across increasingly large stores | Scale and co-op as an attempt to broaden the baseline formula |
| `Let's Tidy Up!: Treasure` | Planned for October 2026 | Sort and stack 8,888 treasure objects in a dwarf treasure room | Mystery/theme wrapper beyond generic retail |
| `ARCHIVIST: Tidy Up & Sort` | Coming in 2026 | Organize archival files by category | Interesting adjacent territory: organization as archive/catalog work |

**Key market observation:** the genre is becoming visually repetitive — first-person hands/cursor,
a cluttered room or store, dense arrays of colorful objects, shelves with explicit destinations,
marketing centered on a large number of objects, "every item has a place" language.

BookBoy should not win by saying it has more books. It should win by making individual books and
shelf rules more meaningful, then showing more compelling room transformation and magical
discovery.
