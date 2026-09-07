# Plushy Store Tidy Up

- **Steam:** [store.steampowered.com/app/1470810](https://store.steampowered.com/app/1470810/Plushy_Store_Tidy_Up/) (App ID 1470810)
- **Released:** September 1, 2026
- **Price:** $4.49 (10% intro discount from $4.99, through ~Sep 15)
- **Reviews (2026-09-06):** Mixed — 48 total, 58.3% positive (28 pos / 20 neg)
- **Developer:** SIV Games

## Premise & scope

Turn a messy plushy store into an organized one — 1,600+ plushies across 90+ shelves in one large
store. Store atmosphere shifts from day to evening with weather variety.

## Mechanics

- Two modes from launch: **Relaxed Mode** (no placement rules, organize however you like) and
  **Challenge Mode** (figure out correct placement, unlock abilities via "bulbs" earned per
  completed shelf).
- This Relaxed/Challenge split (also seen conceptually elsewhere in the genre) directly answers
  the "cozy vs. correctness" tension — letting players opt out of the puzzle layer entirely.

## Update history (selected, chronological)

| Date | Update | What changed |
|---|---|---|
| 2026-09-01 | Launch + Patch #1 | Released (9,000 wishlists pre-launch); same-day QoL patch |
| 2026-09-02 | Patch 2 | Full control rebinding (keyboard/mouse + controller), autosaves, Relaxed Mode improvements |
| 2026-09-03 | Patch 3 | More placement-help visual clues (especially for Challenge Mode), audio and "magnet" (snap-assist) improvements |
| 2026-09-04 | Patch 4 | New Accessibility screen, better onboarding, new Challenge-mode ending |
| 2026-09-06 | Patch 5 | Main menu settings, loading screen, clearer mistake feedback; default graphics quality raised for new players |

**Pattern:** fast, dense patch cadence (5 patches in the first 5 days) heavily focused on
onboarding and placement clarity — "more placement help," "clearer mistake feedback," "better
onboarding" are three separate patches inside one week. That's a strong signal the base game
shipped with a readability problem, which lines up with its current **Mixed** score — the softest
launch reception of any released title in this set so far.

## Common complaints (ALL 13 English-language negative reviews, read in full, 2026-09-07) — this is the whole story here

- **Total absence of placement feedback is, by a wide margin, the #1 complaint** — most of the
  full 13-review set leads with a version of this: "I placed dozens of plushies and never
  once knew if I got it right. No checkmark, no sparkle, nothing." / "Putting plushies on shelves
  offers absolutely zero feedback about proper positioning." / "no clue if you are placing them
  correctly, refunded fast." / "shelves need to have names so we know where things go... it is
  wrong everytime." This is not a minor polish gap — it's reported as the reason people bounced
  off entirely.
- Resolution/display bugs (can't see part of the screen, especially Steam Deck), separate,
  un-rebindable buttons for pickup/drop/shelve/switch-stacks criticized as needlessly complex,
  and a "feels empty" complaint (a big store with comparatively few plushies to place).
- Some limited AI-disclosure concern, smaller than most other titles here but present ("I feel
  like they weren't honest about the AI Disclosure... traces of AI are everywhere").

## Relevance to BookBoy

- **This is the single strongest piece of retroactive validation in this whole competitive set for
  a decision BookBoy already made.** The MVP spec's explicit requirement that placement "snaps
  reliably into a **visible** slot" and that correct placement create "immediate feedback: a snap,
  chime, glow" is precisely the thing this game shipped without, and it's the dominant reason for
  its weak score. BookBoy's own first pass at a shelf slot was *also* invisible (an oversight,
  caught during development, not shipped) — worth remembering this doc's origin story next time a
  feedback affordance seems skippable "for now."
- More generally: multiple post-launch patches here specifically chased "can the player tell where
  something goes" and "does the game make clear when you got it wrong" — precisely the two things
  [02-mvp-spec.md](../02-mvp-spec.md)'s player-experience requirements and
  [05-implementation-plan.md](../05-implementation-plan.md)'s Phase 7 playtest questions are
  designed to catch *before* shipping.
