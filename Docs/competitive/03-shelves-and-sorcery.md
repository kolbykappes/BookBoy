# Shelves and Sorcery: Tidy Up the Enchanted Shop

*Owned by Kolby.*

- **Steam:** [store.steampowered.com/app/3614130](https://store.steampowered.com/app/3614130/Shelves_and_Sorcery_Tidy_Up_the_Enchanted_Shop/) (App ID 3614130)
- **Released:** August 11, 2026
- **Price:** $5.99 (20% off seen at various points)
- **Reviews (2026-09-06):** Very Positive — 595 total, 93.6% positive (557 pos / 38 neg)

## Premise & scope

Restore order to three ransacked fantasy shops:

| Shop | Item count |
|---|---:|
| The Hearth & Barrel (village shop) | 1,048 |
| The Merchant's Rest (larger market) | 3,085 |
| The Grand Athenaeum (third shop) | not confirmed from public sources |

Weapons, potions, spellbooks, fruits, vegetables, and more. Global leaderboards for competitive
completion times.

## Mechanics

- Gather matching items, use magic to "reveal" objects you're struggling to find, or send items
  flying toward their correct shelves — a direct QoL/magic-ability layer on top of plain sorting.
- Four modes: Casual, Timed, Archmage, No Magic — the same content replayed under different
  constraint sets (a cheap way to add replay value without new content).

## Abilities & unlocks

Splits into two unlock categories rather than one flat list — active spells (Sorceries) and
passive movement upgrades (Enchantments), both earned permanently, presumably per-shop-progress
(exact thresholds not published).

| Type | Ability | Effect |
|---|---|---|
| Sorcery | Arcane Sight | Reveals/highlights objects you're struggling to locate — the ability that received the most post-launch tuning (see update history) |
| Sorcery | (item-fling spell) | Sends held or nearby items flying toward their correct shelves |
| Enchantment | Carry capacity | Hold more items at once |
| Enchantment | Reach | Grab from farther away |
| Enchantment | Speed | Move faster |
| Enchantment | Jump | Jump higher |
| Enchantment | Flight | Eventually fly |

The Sorcery/Enchantment split (active magic vs. passive movement stats) is a genuinely distinct
structure from every other title in this set, which mostly bundle everything into one skill list.

## Update history (selected, chronological)

| Date | Update | What changed |
|---|---|---|
| 2026-08-10–08-12 | Launch | Released "Tidy Up Three Shops in Four Different Modes" |
| 2026-08-13 | Two patches same day | Item-recovery tooling, controller-nav improvements, "revisit completed shops / find missing items" (a fix for the common sorting-game problem of a run stuck at 99% with no way to find the last item) |
| 2026-08-15–08-16 | Patches | Bigger carry-capacity packs, "Arcane Sight" (item-locating ability) improvements, repeat-actions toggle, clearer shelf placards |
| 2026-08-20 | Demo released | Free demo covering the first shop, Casual mode |
| 2026-08-24 | Patch | Further Arcane Sight tuning, DirectX 11 support added |
| 2026-08-26 | Patch | Physics feel improvements ("items should now feel a little more reactive") |

**Pattern:** almost every post-launch patch targets the same pain point — helping players locate
the last few remaining misplaced items in a large shop — which is the single most-repeated
player-feedback theme across this whole title.

## Common complaints (ALL 27 English-language negative reviews, read in full, 2026-09-07)

- **"Asset test game" / recycled-assets feel is the dominant theme, ahead of AI specifically**:
  "assets were taken from their shop-keeping game," inconsistent item scale across objects makes
  sorting visually harder, several reviewers directly call it a cheaper, lower-effort copy of
  Librarian ("cheap copy of Librarian... way less fun and satisfaction," "money grab after the
  popularity of Arcane Library"), and vegetables in a *magic shop* feel thematically incoherent
  to several reviewers ("Game called Shelves and Sorcery about a magic shop... more than half the
  stuff I'm picking up is... vegetables?").
- **Generative AI use is present but a minority complaint here (3 of 27, 11%)** — smaller than it
  looked from a partial sample. Where it does come up, it's blamed for hurting readability
  directly, not just objected to on principle: "The books... it's tedious that a bunch are
  identical, all are low res and the font is unreadable. The lighting messes with all the
  colours."
- Performance/optimization complaints (struggles even on a "mid to high end PC"), Steam Deck
  crashes and control drops, a few pile-physics bugs (items floating mid-air), and one comparison
  naming Librarian directly as the higher bar this game doesn't clear on readability.

## Relevance to BookBoy

- Closest thematic match to BookBoy (magic shop vs. magic library). Its "find the last few
  items" problem is worth pre-empting: at only 12 books, BookBoy's MVP scale should never hit
  this, but any post-MVP scale-up should keep a "locate remaining items" affordance in mind before
  it becomes a live issue.
- The mode split (Casual / Timed / Archmage / No Magic) is a lightweight way to add replayability
  BookBoy could borrow later without adding new content — relevant to the "soft-time goals later"
  language in [01-vision-and-design.md](../01-vision-and-design.md#design-principles).

## Personal impressions (Kolby, 2026-09-06)

**Suffered from a lot of the same "rushed out the door" problems as `Supermarket Chaos`** at
launch — many items were very hard to visually differentiate, and it was brutal to pick individual
items out of a pile. Played this one a decent amount, more than Supermarket Chaos.

The difference: **they've actually fixed it.** The Arcane Sight (locate) ability has been
significantly improved since launch, and the physics patch (2026-08-26, "items should now feel a
little more reactive") noticeably helped placement feel. Also liked:
- The different-sized shops (small village shop vs. much larger market) — nice pacing/scope
  variety within one game.
- More visually interesting items overall than Supermarket Chaos's cube-everything approach.

**Takeaway for BookBoy:** direct evidence that a shaky launch on the "can I actually see/pick this
item" problem is recoverable with focused patching — but better to just not ship that problem in
the first place, which is exactly why
[05-implementation-plan.md](../05-implementation-plan.md) Phase 7 asks "can the player understand
where a book belongs without reading lots of text?" and "are books visually large enough to read"
*before* adding more content. The varied-shop-size structure is also a good future-scope idea:
BookBoy's eventual multi-room library could vary room/shelf-wall size the same way, for pacing.
