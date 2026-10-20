# Review Tracker

Dated snapshots of Steam review counts and scores for every game in
[the competitive set](README.md). Append a new dated block each time you re-check — don't
overwrite old ones — so trends are visible over time.

**Source:** `store.steampowered.com/appreviews/<appid>?json=1&num_per_page=0&language=all&purchase_type=all`
(all languages, all time). Percent positive = `total_positive / total_reviews`.

## 2026-09-06 (baseline)

| Game | Score bucket | Total reviews | % positive |
|---|---|---:|---:|
| Librarian: Tidy Up the Arcane Library! | Very Positive | 25,108 | 94.5% |
| Supermarket Chaos | Very Positive | 3,271 | 92.9% |
| Shelves and Sorcery: Tidy Up the Enchanted Shop | Very Positive | 595 | 93.6% |
| Sort Them Ducks | Very Positive | 1,422 | 92.3% |
| Mall: Tidy Up Together | Mostly Positive | 228 | 70.6% |
| Shelve the Potions! | Very Positive | 362 | 85.9% |
| Plushy Store Tidy Up | Mixed | 48 | 58.3% |
| Megastore: Tidy Up Together | Very Positive | 150 | 88.0% |
| Spellbound & Shelved | No reviews (unreleased, launches Sep 7) | 0 | — |
| Black Friday Janitor: Clean the Whole Mall | No reviews (unreleased, ~Sep 9) | 0 | — |
| Tidy Up History | No reviews (unreleased, ~Sep 16) | 0 | — |
| Paver: Tidy Up Together | No reviews (unreleased, ~Sep 18) | 0 | — |
| Wand & Order: Tidy up the magic shop! | No reviews (unreleased, ~Sep 18) | 0 | — |
| DinoBones: Tidy Up the Museum | No reviews (unreleased, ~Sep 22) | 0 | — |

## 2026-10-17

| Game | Score bucket | Total reviews | % positive | Δ reviews since last | Δ % since last |
|---|---|---:|---:|---:|---:|
| Librarian: Tidy Up the Arcane Library! | Very Positive | 27,089 | 94.4% | +1,981 | -0.1 |
| Supermarket Chaos | Very Positive | 3,817 | 92.8% | +546 | -0.1 |
| Shelves and Sorcery: Tidy Up the Enchanted Shop | Very Positive | 816 | 94.5% | +221 | +0.9 |
| Sort Them Ducks | Very Positive | 2,098 | 92.6% | +676 | +0.3 |
| Mall: Tidy Up Together | Mostly Positive | 380 | 70.0% | +152 | -0.6 |
| Shelve the Potions! | Very Positive | 621 | 87.9% | +259 | +2.0 |
| Plushy Store Tidy Up | Mixed | 72 | 61.1% | +24 | +2.8 |
| Megastore: Tidy Up Together | Very Positive | 626 | 92.5% | +476 | +4.5 |
| Spellbound & Shelved | Very Positive | 425 | 88.0% | new (released Sep 7) | — |
| Black Friday Janitor: Clean the Whole Mall | Overwhelmingly Positive | 757 | 97.1% | new (released Sep 10) | — |
| Tidy Up History | Very Positive | 483 | 92.8% | new (released Sep 16) | — |
| Paver: Tidy Up Together | Very Positive | 489 | 85.5% | new (released Sep 18) | — |
| Wand & Order: Tidy up the magic shop! | Very Positive | 584 | 89.7% | new (released Sep 23) | — |
| DinoBones: Tidy Up the Museum | Overwhelmingly Positive | 1,416 | 95.6% | new (released Sep 25) | — |
| Witch Cabin Cleanup | Very Positive | 196 | 98.0% | new, first tracked (released Oct 12) | — |
| Still Sorting | Positive | 22 | 100% | new, first tracked (released Sep 23) | — |
| Sort of Magic | Mixed | 70 | 45.7% | new, first tracked (released Oct 8) | — |
| Magic Mess: Restore Order to the Wizard's Tower! | Mixed | 36 | 63.9% | new, first tracked (released Aug 24) | — |

See [15-refresh-2026-10-17.md](15-refresh-2026-10-17.md) for patch notes, review pushback and newly found games.

## 2026-10-19

| Game | Score bucket | Total reviews | % positive | Δ reviews since last | Δ % since last |
|---|---|---:|---:|---:|---:|
| Whisper of the House | Overwhelmingly Positive | 5,894 | 96.9% | new, first tracked (released Aug 27, 2025) | — |
| Witch Cabin Cleanup | Very Positive | 273 | 98% | +77 | 0 |
| Brick by Brick: Tidy Up the Small Town! | Very Positive | 237 | 89% | new, first tracked (released Oct 15) | — |
| Folded Fables: Tidy Up The Dragon Hoard | Very Positive | 126 | 98% | +64 | 0 |
| Gallery Organizer | Positive | 19 | 95% | new, first tracked (released Oct 14) | — |
| SCROLLS: Tidy up the Library of Olympus! | No reviews (unreleased, Nov 1) | 0 | — | new, first tracked | — |
| The Cozy Board Game Shop: Tidy Up | No reviews (unreleased, Nov 12) | 0 | — | new, first tracked | — |
| ARCHIVIST: Tidy Up & Sort | No reviews (unreleased, Dec 17) | 0 | — | new, first tracked | — |
| Tidy Up Her Toys | No reviews (unreleased, no date) | 0 | — | new, first tracked | — |

Whisper of the House is a story-driven tidy game, not a shelf-sorter; see [20-whisper-of-the-house.md](20-whisper-of-the-house.md).
(Counts from the Steam `appreviews` API, all languages.)

<!--
Template for the next check-in — copy this block, fill in the date, append below:

## YYYY-MM-DD

| Game | Score bucket | Total reviews | % positive | Δ reviews since last | Δ % since last |
|---|---|---:|---:|---:|---:|
| ... |

-->
