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
- Enchantments unlock permanently: carry more at once, more reach, move faster, jump higher,
  eventually flight.
- Four modes: Casual, Timed, Archmage, No Magic — the same content replayed under different
  constraint sets (a cheap way to add replay value without new content).

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
