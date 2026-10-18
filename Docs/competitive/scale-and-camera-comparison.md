# Scale, Camera and Legibility Comparison (running file)

Kolby is studying how the genre's games feel: camera height, book size, bookcase size, legibility. One section per game, plus a
side-by-side table that grows as screenshots arrive. First entry: **Librarian** (2026-10-18).

**Method and caveats.** Competitor numbers are **ratios read off screenshots**: there is no known-size object in frame, the
field of view is unknown, and the camera pitch is unknown, so absolute heights are not measurable. Ratios between things in the
same frame are fairly reliable (medium confidence); anything absolute is a guess (low confidence). Our own numbers were
**read live from the Unity Editor on 2026-10-18** (scene `Lamplit`, Edit Mode, read-only) and from
[../09-scale-reference.md](../09-scale-reference.md) and [../04-build-status.md](../04-build-status.md).

## Librarian: what the six screenshots show

Screenshots: [img/librarian/](img/librarian/) (six files, listed at the end).

**Camera and eye line**
- A standing view. Crosshair rests on the middle of the books on the aimed board, with the other boards a single board-spacing above
  and below. The eye line sits **inside the working band of the bookcase**, not above or below all of it.
- Floor and counter piles are read by **looking down at roughly 45 degrees**; covers are large enough to read from standing height.
- The lens looks wide with visible perspective stretch at the frame edges (guess: 75 to 90 degrees vertical; not measurable).

**Bookcase and books (ratios)**
- Book spine height is about **22 to 23% of screen height** in the spine-on shots (about 235 to 250 px of 1080).
- Book height is about **0.75 to 0.8 of the distance between boards**, so books nearly fill their compartments.
- A compartment holds **about 10 volumes** (about 640 px wide, with books about 62 px thick). Books are **packed**: gaps appear only
  where a volume is missing, each gap exactly one book wide. Series are ten volumes long, and a row is a whole series.
- Totals on the HUD: 400 rows and 3,072 books, about 7.7 books a row on average.

**Legibility**
- Spines carry a **large series icon** (cat head and paw prints) that reads from across the room, a **vertical title**, and a
  **tiny volume digit** near the bottom (about 15 px tall, under 7% of the spine height), which is the hardest thing on the book
  to read.
- Covers lying flat have big, varied titles and art and are easy to read when looking down at them.
- Two text aids make up for small spines: the **aimed book's full title** in a strip at the bottom center ("Monsterology: An
  Introduction to Forbidden Beast Vol.7"), and the **held book's title** under the counts at the top left.

**Placement feedback and HUD**
- Aiming a held book at a shelf shows a **translucent green ghost of the book in its exact slot** with "Press E To Action" at the
  center. (Witch Cabin does the same with a purple ghost.)
- Placed volumes keep fixed slot positions, so missing numbers show as gaps.
- Carried books are drawn as a **huge stack at the lower right**, about 30 to 35% of screen width. The carry count is at the very
  bottom right (cut off in these crops, reads 10/1x).
- Top left: "2 row(s) left to level up", a row counter (0/400) and a book counter (0/3072). Top right: a translucent parchment panel
  with a tutorial hint and ornate corners.

## Shelves and Sorcery: what the three screenshots show (2026-10-18)

Screenshots: [img/shelves-and-sorcery/](img/shelves-and-sorcery/) (three files, listed at the end). Kolby's verdict: **the books are too
dark, not visually distinct enough and too muddy; the UI is worse than Librarian's.** The numbers below back that up.

**Camera and eye line**
- The eye line sits **around the middle board of a three-board bay**: the top board shows only its front edge and the lower boards
  show their tops. That is the same "inside the working band" rule as Librarian.
- Bays are **tall recessed alcoves** inside a carved frame, each with three boards above a closed lower cabinet. A **round brass
  porthole in the cabinet door** shows books inside (storage, purpose unclear).
- Books are slim on the shelf: about **15% of screen height** at the distance of the second screenshot, and they stand **spaced out**,
  not packed (single books and pairs with open board between them).

**The books**
- Brown and dark-green leather with gold ornament; even the mix of items in the pile at the right is bright (stone, produce,
  potions) but **the books themselves are the dullest things in the room**.
- **No readable title on the spines or on the cover face as held**; the cover is ornament only. Titles come only from the HUD.
- The shelved books are nearly the same brightness as the wood behind them (measured below), so a bay of books reads as a
  brown-on-brown surface.

**What the UI does**
- **Held book:** drawn flat and huge at the right (about 21% of screen width and over half the screen height), cover facing you.
- **Held-stack titles:** the carried books' titles are listed at the bottom right as a column, **the current one large, the others
  smaller and fading above it**, with the carry count under it ("5 / 5"). This is the only place the books' names appear, which is
  why a player can use it as a sorting aid.
- **Self-labelling shelves:** the first book placed on a board makes a **paper nameplate** appear on the board's front edge with that
  book's title ("Spider's Web - Don't Touch the Web"). A second group gets its own nameplate. This is the same "first book claims the
  shelf" rule our prototype uses, shown on the shelf itself.
- **HUD top left:** a coin count, a spiral-currency count and a large **0 / 1,048** item counter; a **"Toggle Repeat Actions" (R)**
  checkbox; two **spell slots**, "Book of Sorcery" (Q) and "Book of Enchantment" (E), each a ringed icon with a key label.
- **Crosshair:** a small arrow-and-mouse glyph that tells you what a click will do.
- **No ghost** of the held book is shown in the slot in these frames (in the first frame the aimed slot is simply empty).

### How strongly the books stand out from the shelf behind them (measured)

Method: from each frame I took the mean luminance (WCAG relative luminance on the decoded pixels) of a hand-picked rectangle
covering the shelved books and of a rectangle on the bare wood behind them, then the usual (lighter + 0.05) / (darker + 0.05)
ratio. **This is a rough proxy**: one frame each, hand-picked boxes, different lighting and screenshot compression, and averages
that ignore edges and color. Use it to rank, not as a precise score.

| Game and frame | Books (luminance) | Wood behind (luminance) | Ratio |
|---|---:|---:|---:|
| Shelves and Sorcery, two shelved books | 0.027 | 0.017 | **1.14** |
| Shelves and Sorcery, one shelved book | 0.037 | 0.016 | **1.32** |
| Librarian, three adjacent spines (frame 04) | 0.104 | 0.021 | **2.16** |
| Librarian, three adjacent spines (frame 03) | 0.106 | 0.015 | **2.40** |
| Witch Cabin Cleanup, shelved books (its frame 28) | 0.432 | 0.091 | **3.41** |

Reading it: Shelves and Sorcery's books are **barely separable from the shelf by brightness alone**; Librarian is about twice as
separable; Witch Cabin is three to four times (its books are saturated purple and teal). Librarian also relies on a light icon and
text printed on a dark spine, which adds edge contrast the average hides. Lamplit has **not been measured yet** (next step below).

## Side by side

| Measure | Librarian (from screenshots) | Lamplit (measured live) | Prototype (docs) |
|---|---|---|---|
| Eye height | A standing eye line, absolute unknown | **1.15 m** (camera root 1.06 plus 0.09 floor offset) | 1.7 m |
| Vertical FOV | Looks wide; unknown | **75 degrees**, near clip 0.2 | 40 degrees |
| Bookcase | Several boards, eye line inside the band | 0.58 m wide, **2.86 m tall**, **2 boards** (tops at **1.36 m and 2.08 m**, 0.72 m apart) | 0.44 x 1.79 m, 2 boards at 0.85 m and 1.30 m |
| Eye line vs boards | **Inside the board range** | **Below both boards** (looking up at everything) | **Above both boards** (looking down) |
| Book size (H x D x T) | Unknown | Sample item **0.48 x 0.35 x 0.06 m** | 0.22 to 0.32 x 0.15 to 0.19 x 0.03 to 0.06 m |
| Book height / board spacing | **0.75 to 0.8** | **0.67** (0.48 / 0.72) | 0.5 to 0.7 |
| Book thickness / slot pitch | **About 1.0** (packed) | **0.52** (0.06 / 0.115) | 0.6 to 0.8 |
| Slots per row | About 10 | 5 | 4 |
| Book height on screen | **22 to 23%** at the distance in the shots | **21% at 1.5 m**, 31% at 1 m, 16% at 2 m (computed from FOV 75) | about 26% at 1.5 m (FOV 40) |
| Look angle to a book (from 1.5 m away) | Roughly level | **Up 17 degrees to the lower board, 38 degrees to the upper** (computed from book centers) | Down about 25 and 10 degrees |
| Carried books on screen | Huge stack, 30 to 35% of width | Held books drawn at 0.55 scale | not measured |
| Spine digit size | Under 7% of spine height | not measured here | 0.03 m digits on 0.22 to 0.32 m books, about 10 to 14% |

Notes: "Look angle" assumes standing 1.5 m in front of the container; the top of our vertical view is 37.5 degrees, so **the
upper board at 1.5 m sits right at the top edge of the screen** and needs you to look up. Item sizes are from one sample (all 345
current items reported the same family), so other item types may differ.

## What this suggests for Lamplit (to test by eye, not decided)

1. **The eye line is the biggest gap.** In Lamplit every board is above the player's eyes (looking up 17 to 38 degrees at 1.5 m),
   where Librarian keeps its boards around eye level. Options, cheapest first: lower the base cabinet so the lower board top sits
   near 0.9 to 1.0 m (the book's center then lands near the eye line); raise the eye height toward 1.4 m; or both. The
   storybook scale (a small player among large things) is a deliberate choice, so this is about where the shelves sit, not the scale.
2. **Fill the compartments.** Librarian's books occupy about three quarters of the board spacing and sit shoulder to shoulder, which
   makes shelves look rich and full. Ours fill about two thirds of the height and about half of each slot's width. Tightening the
   slot pitch (0.115 to roughly 0.075 to 0.085 m for 0.06 m books) would pack them more.
3. **Make the series symbol the main identifier** and the number big. Librarian's large icon works at a distance; its tiny digits
   do not. Aim for digits at 10% of spine height or more, and keep a large symbol.
4. **Show the aimed item's name** in a strip at the bottom center, and the held item's name at the top left, as a legibility
   fallback. Check whether we already do.
5. **Use a ghost silhouette in the exact slot,** not a thin bar. Librarian and Witch Cabin both do; ours is a bar on the board.
6. **Fixed slots with visible gaps** (Librarian's missing-volume gaps) are free progress feedback; Lamplit's ordered rooms already
   have fixed slots, so keep them.
7. **Show the held stack large at the screen edge** so the carry count reads at a glance.

## Screenshots (Librarian)

| # | File | Shows |
|---|---|---|
| 1 | [01-counter-pile-aimed-book-name-prompt](img/librarian/01-counter-pile-aimed-book-name-prompt.webp) | Looking down at a counter pile: an aimed book with a white outline, "Press E To Action", the title strip at the bottom, the tutorial panel |
| 2 | [02-green-slot-ghost-held-stack-capacity-10](img/librarian/02-green-slot-ghost-held-stack-capacity-10.webp) | The green ghost in the exact slot, the held stack at the right, carry count 10/1x, the held title at the top left |
| 3 | [03-spine-numbers-and-gaps-vol-1-3-5](img/librarian/03-spine-numbers-and-gaps-vol-1-3-5.webp) | Spine view with volumes 1, 3, 5 and gaps where 2 and 4 belong; the big paw icon and the tiny digits |
| 4 | [04-row-vol-1-3-5-7-8-9-aimed-vol-7](img/librarian/04-row-vol-1-3-5-7-8-9-aimed-vol-7.webp) | The same row with 7, 8 and 9 added; the held cat-face cover at the lower right |
| 5 | [05-tall-bookcase-from-standing-distance](img/librarian/05-tall-bookcase-from-standing-distance.webp) | A bookcase and a cluttered floor from a standing distance; an aimed book outlined |
| 6 | [06-floor-corner-covers-legible-looking-down](img/librarian/06-floor-corner-covers-legible-looking-down.webp) | A corner of the floor looking steeply down: covers readable, books at many angles |

## Screenshots (Shelves and Sorcery)

| # | File | Shows |
|---|---|---|
| 1 | [01-empty-bays-held-book-and-title-stack-5of5](img/shelves-and-sorcery/01-empty-bays-held-book-and-title-stack-5of5.webp) | Two empty recessed bays; the huge held book; the held-title column; the HUD and two spell slots; carry 5 / 5 |
| 2 | [02-first-book-placed-nameplate-on-board-edge](img/shelves-and-sorcery/02-first-book-placed-nameplate-on-board-edge.webp) | One book on a board and a paper nameplate with its title; carry 4 / 5; counter 1 / 1,048 |
| 3 | [03-two-groups-two-nameplates-mixed-pile-at-right](img/shelves-and-sorcery/03-two-groups-two-nameplates-mixed-pile-at-right.webp) | Two groups, two nameplates; a bright mixed pile (stone, produce, potions) at the right; carry 2 / 5 |

## Lessons so far (Librarian, Shelves and Sorcery, Witch Cabin)

1. **The eye line belongs inside the bookcase's working band** (all three do this; Lamplit's boards are all above the eye).
2. **Separate books from the shelf by brightness and color.** The three games rank in the order of how well they are liked for
   readability, and the ratios above rank the same way (1.1 to 1.3, then 2.2 to 2.4, then 3.4 or more). A working target for
   Lamplit to test: **average book-to-shelf luminance ratio of at least 2.5**, with the wood kept dark and warm.
3. **Never rely on dark brown or dark green for the main book colors.** Shelves and Sorcery proves that "leather and gold" looks
   rich in a screenshot and muddy in play.
4. **Put identifying marks where the eye already is.** Librarian: a big icon on the spine. Witch Cabin: a printed title and
   a symbol on the spine. Shelves and Sorcery: nothing on the spine, so everything leans on a HUD list.
5. **Self-labelling boards** (a nameplate or a printed label appearing when the first group is placed) make claims visible without
   a menu. Worth testing in Lamplit, since our first-book-claims rule is invisible today.
6. **Show the held book's title list in the HUD only if the spines cannot carry it.** If the spine has a readable title and
   symbol, the list is unnecessary clutter.
7. **A huge held book at the screen edge works** (Librarian, Shelves and Sorcery) but covers a fifth to a third of the screen.
   Keep the center clear, and make the held book's width an option.

## Lamplit measured the same way (2026-10-18, Claude)

Frame: a real Game-view capture (full URP pipeline, tonemapping on), eye height 1.15 m, standing 1.6 m in front of a Reading Room container with
every book placed, looking 12 degrees down. Same method as above (mean WCAG relative luminance of a hand-picked box on the books, and on the wood
behind them: the lit cabinet front and the compartment back; ratio = (lighter + 0.05) / (darker + 0.05)). One frame, so rank, do not score.

| Group (color) | Book luminance | Ratio to the wood |
|---|---:|---:|
| K (yellow) | 0.604 | **4.84** |
| M (green) | 0.285 | **2.48** |
| I (red) | 0.119 | **1.25** |
| G (purple) | 0.047 | **0.72** |
| O (blue) | 0.044 | **0.69** |
| **Average of the five** | | **2.00** |

Wood: lit front 0.127, compartment back 0.043 (average 0.085).

**Reading it.** On average Lamplit sits between Librarian (2.2 to 2.4) and Shelves and Sorcery (1.1 to 1.3), short of the working target of 2.5. The average
hides the real problem: **the spread is huge.** Bright groups (yellow, green) pop at 2.5 to 4.8, but **purple and navy are darker than the wood
(ratios under 1)** and red is barely above it. Our palette alternates a bright and a dark lightness (`GroupColor`: value 0.85 and 0.60), and the room is dim,
so the dark half of the groups fall into the muddy range that sank Shelves and Sorcery. The big spine letter helps (it is clearly readable in the frame), but a
book that is darker than its shelf is hard to find at a glance.

**Corrections to the table above (our own numbers).**
- The container is **0.82 m wide and 0.58 m deep** (five slots at 0.115 m, plus margins); 0.58 m is its depth, not its width.
- The sample item (0.48 x 0.35 x 0.06 m) is a Room 2 **magazine**. Room 1 books are 0.072 to 0.088 m thick (thickness / slot pitch 0.63 to 0.77); Room 3 atlases
  are about 0.096 m thick (0.84), rolled maps 0.08 m. So the "0.52" fill figure is the worst case, not typical.
- Everything else (eye 1.15 m, FOV 75, boards at 1.36 m and 2.08 m, looking up 17 and 38 degrees from 1.5 m) matches the live scene.

**What the capture shows that the numbers do not.** From standing distance the frame is mostly **underside of boards and cabinet front**: the books sit in a
band across the middle and the upper row is cut off at the top. This is the eye-line gap from the table, seen directly. The aimed-book name and the
"Press (E) or click to pick up" prompt do show at the bottom center, and the held list is at the bottom left, so the legibility fallback in suggestion 4 already exists.

**Suggested order (not applied yet).**
1. Lower the boards: base cabinet about 0.6 m (was 1.36 m) so the lower board top is near 0.95 m and the upper near 1.65 m; shorten the container to about 2.3 m.
2. Fix the palette: a lightness floor so no group is darker than the wood (target every group at a ratio of 2.0 or more, average 2.5 or more), avoid dark
   navy, purple and maroon, and keep the wood dark and warm.
3. Pack the slots: pitch from 0.115 m to about 0.10 m (thickest item 0.096 m).
4. Make claims visible: a nameplate on the board edge when the first group is placed (Shelves and Sorcery), and/or a ghost silhouette of the held item in the
   aimed slot (Librarian, Witch Cabin) in place of the thin bar.

## Next

Measure Lamplit the same way: take a Game view frame of a shelf at standing distance and run the same boxes, so the table
above gets a Lamplit row. Then send the next game's screenshots (ideally one square-on to a shelf, one looking down at a pile, one with the carried books
visible) and I'll add a section and a column.
