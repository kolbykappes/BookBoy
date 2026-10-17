# Sort of Magic

*Not played. Research from the Steam store page, all 8 dev announcements, and all 70 reviews (43 English,
27 other languages; every English review read, most of them only their first ~500 characters), 2026-10-17.*

- **Steam:** [store.steampowered.com/app/4398290](https://store.steampowered.com/app/4398290/) (App ID 4398290)
- **Released:** October 8, 2026 (slipped repeatedly waiting on Steam approval; had 20,000 wishlists)
- **Price:** $7.99 ($6.79 launch discount)
- **Reviews (2026-10-17):** Mixed — 70 total, 45.7% positive (32 pos / 38 neg)
- **Developer/publisher:** Riwon (Riwon Games); a **first game**, and the announcements say the
  technical development is handled by a single developer. Windows only, 11 languages.

## Premise & scope

You are a **tiny cat** who borrowed a wand and cast one very wrong spell, scattering **4,500+ books, potions,
brooms, magical ingredients and hidden wands** across a magic school. **Six rooms**, solo or **online
co-op for up to 4 players**, with cat/robe/hat customization. Same subject area as BookBoy (books, series,
an arcane school) and the most direct overlap of anything found.

## Mechanics & abilities

| Spell | Effect |
|---|---|
| Insight | Reveal matching items on the ground |
| Assemble | Summon matching items to you |
| Homing Sense | Reveal where an item belongs |
| Auto Shelving | Place everything you're carrying at once |

- **Books come in series** ("Rebuild book series"). Since patch 1.1 shelf labels name the volume
  ("Dark Arts", "Dark Arts II"...), which is nearly our own model (a series of four in order).
- Early on, **potion shelves accepted any order with no feedback**; 1.1 now accepts any potion but marks
  wrong placements.

## Why it's at 46%: a launch-quality failure, not a design failure

Both the dev and the reviews agree on this. The 1.1 patch post apologizes by name ("the state in which
Sort of Magic launched... the frustration and negative reviews caused by all the bugs"). Ranked by how
often and how hard each problem shows up in the 70 reviews:

1. **Saves that lose or corrupt progress.** Loading puts you in the wrong room or back at the start
   (the first room "the third time"), quitting autosave overwrote saves, a launch-day "bug fix" wiped
   a couple's progress, one load spawned players above the ceiling or outside the room permanently, and one
   Steam Deck player couldn't resume a game from the menu. Reviews at 5 to 20 hours report losing rooms of
   work, and one says they can no longer refund after those hours.
2. **Missing, duplicated or unfindable items.** Rooms had half their items missing, the "respawn missing
   items" button did nothing, shelf capacity was shown wrongly, books vanished when taken off a wrong
   shelf, and at least two reviewers (Oct 15-16) are unable to progress past room 3.
3. **Items you can't tell apart.** Books in the later rooms look near-identical; **colorblind players call
   it unplayable**, and a non-colorblind friend couldn't tell them apart either. Later rooms repeat the same
   book sets across three bookcases with slightly different color variation, and each copy has its own
   designated spot ("lazy," "rage bait"). Potion racks and silver gilding are similarly hard to read.
4. **Basic settings and polish.** Graphics settings reset every session, resolution changes itself, the image
   is blurry (render scale defaulted to 0), no key rebinding, repeating music and tutorial popups, and a
   held-item model that blocks half the screen. At least one pair **couldn't get co-op to connect at all** (refunded).
5. **Sharp edges in the mechanics.** You **couldn't pick up an item the game had already registered as
   correct**, which made most spells useless and prevented fixing an accidental placement. **Auto Shelving
   dropped all carried items on the ground if you were slightly out of range.** Potion placement gave no
   feedback.

**Not the cause:** the concept. Even the harshest reviewers call it cute; the 22 positive English reviews
praise the cat, customization, co-op ("good with a few friends") and calm mood. About half of the positive
reviews mention bugs themselves and recommend anyway, often citing a responsive dev (one streamer's chat got
live help from the dev). Only a single reviewer suspects AI ("a strong taste of AI"), unverified.

**Language skew:** English reviews are 51% positive (22/43), non-English 37% (10/27), with Russian worst
(4 pos / 8 neg in the sample). A 20k-wishlist launch brings lots of non-English buyers who hit the same
bugs, which pulls the all-language score down.

**Trend after the patch (small numbers):** Oct 8 to 13, before 1.1: 24 pos / 33 neg (42%). Oct 14 to 16:
8 pos / 5 neg (62%). Even so, two Oct 15-16 reviews still report a save reset and missing items that won't
respawn, so the fixes are not complete.

## Update history

| Date | Update | What changed |
|---|---|---|
| 2026-09-22 / 28 | 10k, then 20k wishlists | "Can't say whether it releases today" waiting on Steam approval |
| 2026-10-08 | Launch + same-day patch | Camera height slider, settings no longer reset, a game-breaking "spawn above the ceiling" load bug, render scale default |
| 2026-10-09 | Apology post | "Our first game release... we're truly sorry" |
| 2026-10-10 | Changelist | Pick up placed items, first-pass keybind remapping, bookcase shelf numbering, manual save button, shorter placement grace timer, book color sets updated |
| 2026-10-14 | **Update 1.1** | Save repairs and a stability pass; every book made visually distinct; volume labels on shelves; six distinct rainbow-rack colors; potion mis-placement marked; top-shelf reach; correct control prompts; Steam Deck glyphs |
| Planned | Update 1.2 | "A better potion-sorting system, better progression, more item variety, clearer colors and stronger visual differentiation" |

## Relevance to BookBoy

- **The cautionary tale: ship a stable, honest vertical slice.** Concept, cat, co-op and wishlists were
  enough for 20k wishlists and not enough to survive buggy saves. We have no save/load, which sidesteps the
  worst category, but "never lose or miss a book" (items stuck, tossed out of bounds, or off-shelf) is the
  equivalent for us. The Play Mode suites that land all 128 books are the right guard.
- **Distinguishability is the make-or-break variable.** The fix list and the 1.2 roadmap are mostly
  "make items readable." Our 16 colors, plus a darker second series for light colors, deserve a colorblind
  check before any release; both new book-sorter launches (this and `Witch Cabin Cleanup`) are hit on this.
- **Volume numbering on the shelf label is a good pattern.** It resolves "which copy goes where" for series.
  We already place volume N in slot N; showing the roman numeral on the shelf placard may be worth testing.
- **Let players pick back up what they've placed** (even if "correct"). Banning it caused real anger and
  disabled the abilities that depended on it.
- **Auto-shelve must be total or refuse.** Dropping the whole armful on the floor when out of range was a
  named, hated bug. Our Quick Shelve plans and reserves every slot before flying, which avoids this; worth
  keeping as a tested invariant.
- **Don't duplicate book sets across cases with only color tweaks.** Same structural complaint as
  `Supermarket Chaos` and `Shelves and Sorcery`.
