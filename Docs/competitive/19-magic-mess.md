# Magic Mess: Restore Order to the Wizard's Tower!

*Not played. Research from the Steam store page, the dev's single announcement, and all 19 English reviews
(9 negative, 10 positive; the 17 non-English reviews are 13 positive), 2026-10-17.*

- **Steam:** [store.steampowered.com/app/5012990](https://store.steampowered.com/app/5012990/) (App ID 5012990)
- **Released:** August 24, 2026 (same day as `Shelve the Potions!`)
- **Price:** $4.99
- **Reviews (2026-10-17):** Mixed — 36 total, 63.9% positive (23 pos / 13 neg)
- **Developer/publisher:** TinyClaw Games / Quadfix Games. Windows only. 10 languages.

## Premise & scope

Return to your abandoned wizard's tower, with a **talking dragon companion**, to put back **3,040 items**
(books, potions, scrolls, crystals, mushrooms, artifacts) after a break-in. **One tower, one room**: the
space is a single open rectangle. A full run is about **2 to 3 hours** for the players who finished and got
the achievements, and several reviewers say there is no replay value.

## Mechanics & abilities

A level-gated skill tree, with **XP paid per item placed** (10 XP per item, 50 for completing a shelf):

| Skill | Effect | Reviewer verdict |
|---|---|---|
| Magical Sense | Identical items near you hover and spin; passive once unlocked, retargets when you switch items | **Praised as "the best implementation of an item sense mechanic I've seen"**; a second reviewer likes that it is passive rather than a cooldown |
| Mass Collection | Passive; picking up one item also grabs the identical ones within reach | Radius smaller than Magical Sense, so it highlights what you couldn't reach; criticized as odd |
| Instant Placement | Passive; places items automatically if you're already close enough to a shelf | Criticized as redundant: if you are that close you could click |
| Combo (XP bonus) | Bonus XP for quick placements | One reviewer says it never triggered in a full playthrough |
| Sprint, Fly | Movement | Flight called slower than sprinting and "pointless" on a flat map |

- **Carry capacity starts at one item and maxes at five.** This is the most repeated complaint. A full
  bookshelf is about 105 books and a crystal shelf about 60, so the run is "grab 5, run over, repeat."
- **Useful skills are gated by level, with a large jump between levels 8 and 14**; the "gather" skill
  doesn't exist until 14. Players say the first half of the game is therefore a slog and the late-game
  skills (all passive, no cooldown by level 16) arrive too late to enjoy.
- Every item has **one specific shelf**, not a section (liked), but **everything is a shelf** (scrolls,
  crystals, mushrooms), so items feel interchangeable apart from name and count.

## Update history

| Date | Update | What changed |
|---|---|---|
| 2026-08-24 | Launch | |
| 2026-08-25 | v1.1 | Fixed an unassigned-key visual bug; tuned the Fast Shelving and Remote Shelving skill speeds |

**Pattern:** one patch ever, then silence for eight weeks. Reviews from Sep 7 to Oct 8 still report the same
bugs (including the unfinishable shelf) and ask for more maps and a longer game; a launch-day request for
controller support was never answered in the news feed. Compare `Witch Cabin Cleanup` and
`Still Sorting`, which answered feedback within days.

## Common complaints (all 9 English negatives)

- **Short and thin**: one map, finished in under 3 hours, "no replayability."
- **Five-item carry limit** and the back-and-forth it forces; skills "don't have a large impact, making
  everything take more time than it earns."
- **The dragon**: talks constantly, **roughly 85% of his lines nag you about being slow** (per one detailed
  review), can't be turned off apart from muting the game, and isn't ridable even though the ending tease
  suggests it. Two positive reviewers, in contrast, enjoy him.
- **Timer keeps running while paused** (two reviewers) and a **bugged timed achievement**.
- **Counter and shelf bugs**: a count stuck at 2,870/3,040 with every shelf full; a shelf that wouldn't
  accept items; no way to highlight the last missing item (a tiny red gem hidden in the carpet).
- **Map layout**: a big empty rectangle with invisible walls and ceiling that block reachable areas.
- **AI mentions: none** in any review.

## Relevance to BookBoy

- **Two ideas validated by its reviewers, both close to our design.** Paying XP *per item placed* was praised
  by two reviewers (it makes early progress feel quick); and a **passive "see all matching items" glow**
  was called best in class. Our Sister Glow is an active, cooldown-gated pulse; a passive version, or a
  toggle, is worth testing.
- **A carry limit of one, growing to five, was the whole complaint.** We start at four, going to ten with
  Strong Arms I-VI, which is much kinder. Given how hard this hit Magic Mess, I'd keep the floor at four and avoid shrinking
  it to make the tree feel more meaningful.
- **Skills gated by level as well as points feel like arbitrary friction.** Prefer buying power directly with
  points.
- **A companion that nags is a design risk.** If we ever add a narrator, give it a mute and make its lines
  encouraging rather than about player speed (our score is the time to finish, so this matters for us).
- **Don't let timers run in menus.** Our menus pause the game; keep that true for the run timer too.
- **Provide a "what's left" aid for the last book.** It is the genre's most common endgame complaint; see
  [17-still-sorting.md](17-still-sorting.md).
- **Spread of item *types*, not just item counts.** "Everything goes on a shelf" read as same-y; a reviewer
  suggested multiple floors or a U-shaped layout to break up a flat space.
