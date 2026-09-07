# Supermarket Chaos

- **Steam:** [store.steampowered.com/app/4800590](https://store.steampowered.com/app/4800590/Supermarket_Chaos/) (App ID 4800590)
- **Released:** June 29–30, 2026 (news post says "the supermarket is open" on Jun 30)
- **Price:** $4.99
- **Reviews (2026-09-06):** Very Positive — 3,271 total, 92.9% positive (3,040 pos / 231 neg)

## Premise & scope

Single-player or up to 4-player co-op. Arrange 4,668 products across 16 sections (fruit, tea,
frozen foods, books, wine, ramen, etc.) after the store's organizing robot "GPT-9000" decides all
product positions are temporary and dumps everything. No time limit, no game over.

## Mechanics

- Pure visual sort — pick up product, find section, match to price tag.
- Two major post-launch content additions (see below) rather than a puzzle layer: more maps, then
  multiplayer.

## Update history (selected, chronological)

| Date | Update | What changed |
|---|---|---|
| 2026-06-29–06-30 | Launch + 1.0.5/1.0.6 | Released; day-one fixes for a movement-speed bug and wide-resolution issues; added invert-axis options |
| 2026-07-01–02 | 1.0.7–1.0.8 | Sign-name/localization fixes, backup-file loading from settings |
| 2026-07-06 | 1.0.9 | Added Arabic localization |
| 2026-07-08 | **Major Update #1** (1.1.0) | New map: a second, larger supermarket layout |
| 2026-07-12 | 1.1.3 | Overhauled save/recovery logic, added a backup-selection list in the menu |
| 2026-07-27 | **Major Update #2** (1.1.4, Multiplayer Beta) | 2–4 player co-op added, with costumes and per-player saved progress |
| 2026-08-04 | 1.1.6 | New map: "Super Super Supermarket" — 9,999 items, 790 shelves, a large scope jump over the base 4,668/16 |
| 2026-08-12–08-16 | 1.2.0–1.2.3 | Achievement/multiplayer initialization fixes, 27 new achievements |
| 2026-09-05 | 1.2.4 | Keyboard remapping, physics fix for items sinking into the floor |

**Pattern:** the clearest example in this set of a title expanding almost entirely through scale
(more maps, more items) and social features (co-op, costumes) rather than mechanical depth — each
major update is "more of the same, bigger," which tracks with the "object count as differentiator"
trend flagged in [01-vision-and-design.md](../01-vision-and-design.md).

## Common complaints (ALL 58 English-language negative reviews, read in full, 2026-09-07)

- **"Lacks soul" / low-effort feel compared to Librarian is the more central theme here** —
  reviewers repeatedly frame this game as mechanically competent but atmospherically empty
  ("obviously lacks cozy atmosphere and WHIMSY... practically soulless," "the game struggles to
  show any sort of soul or passion"), often as a direct comparison against the genre's originator.
- **Generative AI use is a real but secondary complaint (16 of 58 negative reviews, 28% — smaller
  than it first looked from a partial sample)**, compounded here by AI-generated *content errors*
  more than the disclosure issue itself: "Pills clearly labeled as 'laxative' by texture are
  called 'A Vitamins'," a Pringles-style can labeled "Banana Thick-cut Chips," product labels not
  matching their shelf tags. One reviewer called the 9,999-item map specifically "AI-generated
  slop... full of obvious errors a human would never make."
- Items clipping through/spawning under the floor is the single most-repeated concrete bug across
  the full set (matches the personal-impressions finding above and the 1.2.4 patch notes), not
  enough shelf space for all items on the largest map, one late-game progression softlock (robot
  dialogue loop blocking a fresh playthrough), and several reviews calling the third/largest map
  specifically a much weaker experience than the first two.

## Relevance to BookBoy

- Its post-launch roadmap (bigger map → multiplayer) is the default, crowded path. BookBoy's
  brief explicitly steers away from following it for the MVP (no multiplayer, no procedural
  scale-up) — this game is evidence that path works commercially, but also that it's the least
  differentiated option.
- The physics bug (items sinking into the floor, fixed >2 months post-launch) is a good real-world
  argument for BookBoy's "tactile reliability over physical realism" principle — avoid
  Rigidbody-driven placement bugs by snapping cleanly instead.

## Personal impressions (Kolby, 2026-09-06)

**Felt really janky — the weakest hands-on experience so far, despite being the 2nd game out and
having decent review scores.** Played this one the least of any owned/tried title.

Specific problems:
- Extremely hard to pick individual items out of a pile — many items are small with heavy
  anti-aliasing artifacting, to the point where you effectively *can't* visually pick them out and
  have to rely on the "spell" (locate ability) instead of just looking. The core "find it
  yourself" interaction basically doesn't work without the assist tool bailing it out.
- Placement felt bad — items visibly sank into the floor (confirmed by the 1.2.4 patch notes
  fixing exactly this, more than 2 months after launch).
- Harsh lighting, no atmosphere.
- Most items are basically low-poly cubes (books, boxes of electronics, etc.) — visually
  uninteresting, "shovelware" vibes rather than a crafted world.
- Still got good reviews, and they've kept patching it — the review score doesn't fully reflect
  how it feels to actually play.

**Takeaway for BookBoy:** this is the clearest negative case in the whole set. A good review score
can coexist with a genuinely bad picking/placement feel if the object count is big enough that
players lean on assist abilities instead of noticing. Confirms two of BookBoy's stated principles
directly: **visual readability first** (an item you can't tell apart in a pile is a failure, no
matter how many categories exist) and **tactile reliability over physical realism** (don't let
Rigidbody physics cause objects to clip/sink — snap cleanly). Low-poly is fine (BookBoy's own
plan starts with cubes), but low-poly *and* visually indistinct together is the trap — Librarian
proves you can be simple and still read clearly.
