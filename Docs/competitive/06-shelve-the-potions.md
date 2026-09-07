# Shelve the Potions!

*Owned by Kolby.*

- **Steam:** [store.steampowered.com/app/4928820](https://store.steampowered.com/app/4928820/Shelve_the_Potions/) (App ID 4928820)
- **Released:** August 24, 2026
- **Price:** $5.99 (10% launch discount)
- **Reviews (2026-09-06):** Very Positive — 362 total, 85.9% positive (311 pos / 51 neg)
- **Developer/publisher:** Knight Owl Games

## Premise & scope

Organize 2,000+ potions on shelves after the witch's cats knock them all over, using clues around
an enchanted cellar. Learn strange symbols, decipher cryptic notes, and uncover the correct shelf
order — **not** simply matching a picture to a labeled slot.

## Mechanics

- **The clearest deduction-based entrant in this whole set.** Symbols and clues have to be
  interpreted to figure out where a potion goes, rather than reading it off a color/icon key.
  This is the single closest mechanical reference for BookBoy's stated long-term direction
  ("shelf ordering rules," "symbols and magical catalog language" in
  [01-vision-and-design.md](../01-vision-and-design.md)).
- No AI-generated content, called out explicitly by the devs in their devlog — a positioning
  choice several titles in this set are making publicly (see also DinoBones).

## Abilities & unlocks

Deliberately just two abilities, not a tree — depth comes from the puzzle layer, not from ability
count. Both upgrade in place (cooldown reduction) rather than unlocking new abilities outright.

| Ability | Effect | Progression |
|---|---|---|
| Highlight | Marks items matching the currently-selected puzzle clue | Upgrades reduce cooldown; reached 40s cooldown by patch v1.0.11 |
| Assemble | Pulls matching potions into your hands (up to 9 at max level) | Final cooldown 10s at max upgrade |

An options toggle lets you disable both abilities entirely (added specifically for an achievement
that requires completing without magic) — the only title in this set with an explicit, first-class
"turn off all abilities" switch rather than a separate No-Magic *mode* bolted on alongside the
main one. Worth noting as a cheap way to serve both ability-loving and ability-skeptical players
without maintaining two parallel experiences.

## Update history (selected, chronological)

| Date | Update | What changed |
|---|---|---|
| 2026-07-22 | Limited public playtest | Early access to gather feedback pre-launch |
| 2026-08-05 | Public demo live (30k wishlists) | Fixes/QoL added since the playtest |
| 2026-08-24 | v1.0 launch + Day 1 patch | Released; day-one patch added an upgrade level to highlight/assemble abilities to lower their cooldown |
| 2026-08-27 | Patch v1.0.11 | Added an options toggle to disable magic abilities entirely (for an achievement requiring no-ability completion) |
| 2026-08-28 | Patch v1.0.12 | Added 360° controller stick movement |

**Pattern:** small, fast, low-drama patch cadence — mostly ability tuning and QoL, no rollback
incidents. Suggests the playtest-then-demo pre-launch process (playtest → demo → launch) paid off
in launch stability, unlike `Sort Them Ducks`.

## Common complaints (ALL 25 English-language negative reviews, read in full, 2026-09-07)

- **The clear outlier in this competitive set: only 1 of 25 negative reviews (4%) mentions AI at
  all, and that one is a compliment, not a complaint.** "A beautiful game, with zero AI
  influence." Matches the devs' public no-AI stance (Mechanics, above) actually landing with the
  audience — the one title here where that positioning shows up as a reviewer-volunteered positive
  rather than a non-issue.
- **The dominant complaint instead is content depth**: the puzzle-deduction layer (the game's
  whole differentiator) is reported to run out after roughly the first 2 of many "colors"/sections
  — "within the first 2 sections of potions, you've figured out all the puzzles... the rest of the
  game starts to feel like a chore," "all of the potions have the same solution." Several
  reviewers say they liked the idea enough to keep playing out of completionism alone, past the
  point the puzzle layer stopped delivering anything new.
- Minor: one keybind conflict (a controller button double-bound to both "confirm puzzle" and a
  magic ability) that silently breaks a specific achievement.

## Relevance to BookBoy

- This is the title to study most closely once BookBoy's MVP is proven: it demonstrates that
  clue/symbol-based deduction is viable and well-received (85.9% positive) at modest scale
  (2,000+ items, not tens of thousands) — a smaller, smarter game rather than a bigger, dumber one.
- Its pre-launch process (private playtest → public demo → launch) is a reasonable template if
  BookBoy ever moves toward a real release.
- The "puzzle solved after 2 sections, chore for the rest" complaint is directly relevant to
  BookBoy's own eventual symbol/deduction layer (see [01-vision-and-design.md](../01-vision-and-design.md)'s
  evolution path): a deduction mechanic needs enough *variety* of puzzle types to outlast the
  book count it's layered onto, not just one clever idea repeated at scale. Worth designing the
  puzzle variety budget deliberately rather than discovering the shortfall post-launch like this
  game did.

## Personal impressions (Kolby, 2026-09-06)

**A really different take — not 100% sold on it yet, but several hours in.** The potions are
low-poly but easy to visually differentiate, which matters a lot given how deduction-heavy this
game is (you need to be able to tell items apart to apply what you've solved).

The core loop: solve environmental puzzles scattered around the map to figure out the correct
shelf order, then the game shows you what you've already solved as it applies to *other* colors of
the same puzzle type. Mixed feelings on execution:
- Some puzzles are unintuitive/not fun.
- Some require carrying an item to a separate "decoder" station just to read numbers off it —
  feels like an odd extra step rather than a satisfying discovery.
- But some are genuinely visually interesting and effective — one favorite: a potion with a series
  of floating dots spinning around its perimeter, which made it noticeably easier to pick that
  specific one out of a pile just from the motion, not just the color/shape.
- Liked that shelf groups are smaller here (groups of 5 and 8) rather than the huge undifferentiated
  piles elsewhere — makes the puzzle-solving feel worth the effort instead of just a chore layered
  on top of a big sort.

**Takeaway for BookBoy:** the "decoder station" friction point is a caution — a puzzle step that
requires a special-purpose fixture just to read basic information can feel like an unnecessary
detour rather than a discovery. The **spinning-dots** detail is the standout idea worth borrowing:
subtle motion (not just static color/shape) as a differentiator makes an item easier to spot in a
cluttered pile *and* more satisfying to notice — worth considering for BookBoy's higher-value or
"forbidden" books later (a subtle glow pulse or particle effect, not just spine color) once the
MVP's plain static readability is proven. Smaller shelf groupings (5–8 items) rather than one huge
pile also supports BookBoy's MVP scale choice (4 books per category) — small, legible groups seem
to be where the puzzle layer actually lands well.
