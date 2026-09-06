# Sort Them Ducks

*Owned by Kolby.*

- **Steam:** [store.steampowered.com/app/4992070](https://store.steampowered.com/app/4992070/Sort_Them_Ducks/) (App ID 4992070)
- **Released:** August 13, 2026
- **Price:** $5.99
- **Reviews (2026-09-06):** Very Positive — 1,422 total, 92.3% positive (1,312 pos / 110 neg)

## Premise & scope

Sort more than 4,000 unique rubber ducks (pirate ducks, wizard ducks, astronauts, knights, chefs,
dinosaurs, etc.) onto correct shelves in a messy duck store. Correctly sorted ducks earn money to
spend on ability upgrades. Steam controller input supported.

## Mechanics

- Pure visual/collectible sort — the entire hook is the sheer variety and novelty of duck designs,
  not a puzzle layer.
- Currency-for-abilities loop (sort → earn → upgrade) is the systemic layer, same shape as most
  of this genre.

## Update history (selected, chronological)

| Date | Update | What changed |
|---|---|---|
| 2026-08-14 | Patch 1 + same-day save-corruption incident | Day-one patch, immediately followed by a report that the patch corrupted save files (upgrades/abilities); fixed same day |
| 2026-08-14 | FPS/jitter/black-screen fixes | Additional stability patch same day as the save issue |
| 2026-08-15 | Patch 2 | Fixed camera jitter/motion sickness |
| 2026-08-17 | "Become a Tester" call | Developers openly recruited players to stress-test ahead of Patch 3, specifically targeting a lingering savefile issue |
| 2026-08-18 | Patch 3 | Savefile issue fixed, motion sickness further addressed |
| 2026-08-23–08-24 | Patch 4 | Save backups, input rebinding, Steam Cloud fix |

**Pattern:** the roughest launch week of any game in this set — a save-corrupting patch on day
one, followed by a five-day scramble (jitter, motion sickness, corrupted saves again, then a
public tester call) before things stabilized around Patch 4. Reviews stayed Very Positive through
this, suggesting the core hook (duck variety) was strong enough to survive a bad first week — but
also that they got lucky.

## Relevance to BookBoy

- Direct cautionary tale: don't ship a save/progress system without hardening it first — exactly
  why "save/load" is an explicit MVP non-goal in [02-mvp-spec.md](../02-mvp-spec.md).
- Evidence that a strong, simple visual hook (novel object designs) can carry a rough technical
  launch — worth remembering that BookBoy's visual identity (book covers, spine colors, magical
  symbols) is doing real work, not just decoration.

## Personal impressions (Kolby, 2026-09-06)

**Clever idea, fun visually, and much easier to sort out of a pile than Supermarket Chaos or
early Shelves and Sorcery** — the duck designs are distinct enough to actually pick out by eye.
The overall vibe is vibrant and ducks are just inherently fun to interact with.

Problems:
- A grindy early phase.
- Noticeable camera jitter (matches the patch notes — camera jitter/motion sickness fixes were a
  recurring theme in the first week, see update history above).
- **Piles of ducks could physically block access to the shelves** — a concrete case of clutter
  becoming a movement/interaction obstruction, not just a visual-noise problem.
- A lottery/risk-your-currency system that seemed like a promising idea on paper but felt poorly
  implemented in practice.

Liked:
- Named supporter ducks with custom designs in a special area — a nice, cheap touch of
  personality/community recognition.

**Takeaway for BookBoy:** the "piles blocking shelf access" problem is a concrete physical-layout
failure mode to design around from the start — scattered books or bins near shelves need to leave
a clear approach path to the shelf itself, not just be readable. The strong visual-hook + shaky
systems combo (fun ducks, weak lottery mechanic) also reinforces: get the core identify-carry-place
loop excellent first, and be cautious about bolting on secondary economy systems (risk/reward,
currency sinks) before they're really needed — echoes the MVP non-goals list in
[02-mvp-spec.md](../02-mvp-spec.md) (no economy/currency systems yet).
