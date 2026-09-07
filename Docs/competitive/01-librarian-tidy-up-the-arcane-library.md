# Librarian: Tidy Up the Arcane Library!

- **Steam:** [store.steampowered.com/app/4197610](https://store.steampowered.com/app/4197610/Librarian_Tidy_Up_the_Arcane_Library/) (App ID 4197610)
- **Released:** April 30, 2026
- **Price:** $5.99 (frequently discounted, e.g. 20% "Midweek Deal" at $4.79)
- **Reviews (2026-09-06):** Very Positive — 25,108 total, 94.5% positive (23,719 pos / 1,389 neg)

## Premise & scope

Single-player. Return scattered books to their proper places in an arcane library — shelving
3,072 books total. This is **the direct ancestor of BookBoy's premise** and the apparent catalyst
for the entire sorting-genre wave that followed it.

## Mechanics

- Pure visual/color + category sorting, no puzzle-deduction layer.
- Completing shelf rows unlocks abilities: `Assemble` (summons other volumes of the same series
  to you), `Insight` (highlights series volumes), `Auto-Shelving` (instantly places books).
- The ability progression is the main systemic differentiator from a flat sort — it rewards
  completing sections with tools that speed up later sections.

## Update history (selected, chronological)

| Date | Update | What changed |
|---|---|---|
| 2026-03-12 | Dev update | Added Simplified Chinese and Cantonese localization (pre-launch) |
| 2026-04-16 | Release date announced | Launch set for Apr 30 |
| 2026-04-30 | Launch + emergency hotfix 1.0.1 | Released; same-day fix for an Auto-Shelve ability bug |
| 2026-04-30–05-06 | Patches 1.0.2–1.0.8 | Motion blur/V-Sync options, new book covers, head bobbing, several crash fixes (startup crash, hardware-optimization crash, early-init race) |
| 2026-05-05 | Patch 1.0.5 | Added "Recall Stone" — recovers books that visually disappear/glitch out of the world |
| 2026-05-08 | Rollback announcement | A prior update caused serious enough issues that a version was rolled back while fixed |
| 2026-05-21–07-13 | v1.0.9–1.0.12 | Keybinding options, save-data optimization, manual save/load with multiple slots, FOV/render-scale settings; one urgent rollback to 1.0.10 after a progression-breaking bug in 1.0.11 |
| 2026-07-15 | Hotfix | Fixed broken Steam Cloud sync after the save-system change |
| 2026-07-17 | Patch 1.0.13 | Added a Vignette option and a "stack all held books on the ground" convenience feature |

**Pattern:** steady post-launch support (13+ point releases in under 3 months), mostly QoL/settings
and stability, with at least two rollback incidents from regressions — a reminder that the save
system and any "instant complete" ability (Auto-Shelving, Recall Stone) are exactly the kind of
feature that's easy to destabilize.

## Common complaints (ALL 110 English-language negative reviews, read in full, 2026-09-07)

- **72 of 110 negative reviews (65%) mention generative AI — by far the largest complaint theme
  of any game in this set.** Many reviewers explicitly say they loved the game, played for hours,
  then discovered
  the AI disclosure (reportedly added to the store page only after launch, per several reviews)
  and either refunded or wrote a negative review out of principle — e.g. "I absolutely loved
  playing this game... I did not notice this game uses Gen AI... I'm really disappointed," "sad
  they used generative ai, genuinely liked the game and will no longer be playing it now that I
  know." This is a reputational issue layered entirely on top of gameplay satisfaction, not caused
  by it.
- **Progression trivializes itself.** Multiple reviewers say the ability unlocks (the same
  `Auto-Shelving`/`Assemble` system praised elsewhere) make the game "do everything for you after
  one bookcase," leaving nothing but repetitive motion for the remaining ~2/3 of the book count.
- Minor: performance/heat complaints on some systems; one Steam Deck launch-compatibility report.

## Relevance to BookBoy

- Confirms the core loop (find → identify → carry → shelve) sustains a large, sustained
  Very-Positive audience with zero puzzle layer — validates that BookBoy's plain color/category
  MVP is a reasonable bar to clear.
- The ability-unlock structure (`Assemble`, `Insight`, `Auto-Shelving`) is the natural next layer
  after BookBoy's MVP proves fun — see the "Potential future mechanics" list in
  [01-vision-and-design.md](../01-vision-and-design.md). But temper it against the complaint
  above: an ability that fully automates the core action removes the game, it doesn't enhance it.
  Any future automation/assist ability should speed up execution, not remove the
  identify-and-place decision entirely.
- Its rocky patch history around save/rollback is a caution for BookBoy's own eventual save/load
  system (explicitly a non-goal for the MVP per [02-mvp-spec.md](../02-mvp-spec.md), for good
  reason).
- **The AI-disclosure backlash is the single most important cross-cutting finding in this whole
  competitive set** (see [README.md](README.md)) — worth reading that synthesis regardless of
  whether BookBoy's own pipeline uses AI tools anywhere, since the market reaction is about
  transparency and trust, not the tooling itself.

## Personal impressions (Kolby, 2026-09-06)

**Very polished. The game just feels excellent — the strongest overall vibe of anything played so
far.** Nice lighting, a genuinely classy, chill, cozy atmosphere. Easy to pick items up. Clearly
the reason so many later entrants chased the same visual/tonal angle.

Visual differentiation is only *modestly* hard — but in a deliberately interesting way: color is
the primary, easy-to-read category signal for the first sorting pass, but within a color group
you then have to sort further by title, and then further still by volume number within a series.
So the difficulty is layered on purpose (color → title → volume), not accidental — it never feels
like the game is just being obtuse.

**Takeaway for BookBoy:** the layered-identification idea (coarse visual sort first, finer
distinction second) is worth stealing directly — it's a graceful way to add depth without making
the *first* pass harder to read. BookBoy's MVP category/color-only sort is the "first layer" here;
a natural next step post-MVP is a second layer (e.g., series/title) within an already-correct
shelf section, matching this game's actual proven structure rather than inventing one from
scratch. Also: atmosphere and lighting are pulling real weight in why this is the standout — not
just mechanics.
