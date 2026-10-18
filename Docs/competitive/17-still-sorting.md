# Still Sorting

*Not played. Research from the Steam store page, dev announcements and all 16 English reviews (all
positive), 2026-10-17. Very small sample: 22 reviews total.*

- **Steam:** [store.steampowered.com/app/5157330](https://store.steampowered.com/app/5157330/) (App ID 5157330)
- **Released:** September 23, 2026
- **Price:** $4.99 (20% launch discount ended Sep 30)
- **Reviews (2026-10-17):** Positive — 22 total, 100% positive (22 pos / 0 neg)
- **Developer/publisher:** montsaint (announcements are signed "Rell"). Windows only; English and Korean.

## Premise & scope

A bookshop's moving truck flipped, so **4,344 books** end up in boxes and in a pile in front of the counter.
The sorting rules are in a hastily scribbled **manual and a chart**: **26 rows, A through Z, 182 slots**.
Open a box, fill a bag, walk to the row and shelve in order. A slot with room left over needs a
**bookend**. **The Z row is not for comics at all**: it holds ledgers and member files sorted by **date**
instead of volume. Closest in subject to BookBoy of anything found, and the only entrant whose sorting key
is genuinely alphabetical/ordinal rather than color or picture-match.

## What makes it different

- **2D, point-and-click, retro art**, not first-person 3D. Reviewers call it "unique" and a "great twist."
  One says the 2D view is why they enjoy it more than first-person tidy-ups; another has simulator
  sickness, can't play first-person games, and is thrilled to finish a session without headache or nausea.
- **No upgrades or abilities at all.** One reviewer says that is their preferred way to play, and that the
  game compensates by grouping books sensibly per box ("a majority of the books you pull from a specific
  box go within one set of shelves or two adjacent sets"), so you can memorize locations and "lean back."
- **A chart that doubles as the progress ledger.** Finished slots are crossed out, slots needing a
  bookend are marked, and with 20 or fewer slots left a red bar flags each unfinished one (the endgame
  "where is the last one" fix).
- **Colored progress tabs** show red when books in a slot are out of order and purple when a book from
  another slot is mixed in. Added after player feedback.
- **The floor pile is a workspace**: players restack it into their own sub-piles, and from 1.1.2 that
  layout is saved. One reviewer says moving books from the giant pile into boxes "reduced the overwhelming
  feeling immensely."
- Ambient rain; dark mode and colorblind mode (1.1.1). Reviewers cite the rain as why they stopped muting the
  game.

## Update history

| Date | Update | What changed |
|---|---|---|
| 2026-09-23 | Launch | |
| 2026-09-25 | Known-issues post, hotfix 1.0.8 | **Bookends held while saving vanished on load, blocking completion** (auto-restored for affected saves). Some GPUs got a black screen then a crash; a launch-option workaround (`--rendering-driver opengl3`) was posted. |
| 2026-09-25 | 1.1.0 QoL | Colored tab progress, drag-to-reorder bag, progress %, box-count estimates in steps of 50, windowed mode, "Saved!" confirmation, **quit now autosaves**, bug fix for books vanishing on large-pile saves (restored to the comic boxes on load) |
| 2026-09-25 | 1.1.1 | Dark mode, colorblind mode |
| 2026-09-29 | 1.1.2 | Saved floor-pile layout, easier stacking with a red outline preview, shelf completion ripple effect |
| 2026-10-01 | 1.1.3 | Chart improvements above; fix for late-game frame drops |

**Pattern:** five updates within eight days of launch, nearly all traceable to a suggestion thread the dev reads ("I read
every post"). Save-state corruption (the bookend) was the one launch-breaking bug, handled with a
self-healing fix rather than a "start over."

## Common complaints

There are no negative reviews. The mild criticisms inside positives:

- You **can't tell which books are in a box** until you pull them, so it becomes "random books in my
  pocket" and walking straight to the shelf; the dev partly answered with box-count estimates.
- **Book size** makes shelf management fiddly.
- A steep first-minutes learning curve ("it took me a few minutes to figure out the zoom and controls").

## Relevance to BookBoy

- **It validates that real ordering, not just category matching, is enjoyable**: alphabetical rows, a
  date-sorted exception row, and bookends as a visible capacity rule. This is the clearest evidence in
  the set for the "shelf ordering rules" direction in [01-vision-and-design.md](../01-vision-and-design.md).
- **A ledger that crosses off finished slots and flags the last few** directly answers the most common
  endgame complaint across the genre (hunting the last missing item). We show a books-correct count; a
  "what's left" list/bar could be cheap.
- **Some players prefer no upgrades.** Several tidy-ups now give an opt-out (see `Tidy Up History`,
  `Black Friday Janitor`); this is the strongest version of it. Our tech tree should stay optional if
  possible.
- **Self-healing saves.** Both the missing-bookend fix and the vanished-books fix repair existing saves on
  load instead of making players restart. We don't have save/load (non-goal), but the same principle
  applies if that ever changes.
- Sample is tiny (22 reviews, one dev) and it is 2D, so treat as an idea source rather than a benchmark.
