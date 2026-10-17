# Witch Cabin Cleanup

*Owned by Kolby; played for roughly the first eight minutes (first room, "Bedroom"). Research from the Steam
store page, dev announcements, all 134 English reviews (4 negative, read in full; the 103 English positives
mined for specifics), and **27 screenshots Kolby captured on 2026-10-17**, read by Claude. The screenshots
are not committed to the repo. Anything seen only in the first room is marked; ability effects beyond level 1
were not seen.*

- **Steam:** [store.steampowered.com/app/4991240](https://store.steampowered.com/app/4991240/) (App ID 4991240)
- **Released:** October 12, 2026 (slipped five days from Oct 7)
- **Price:** $5.99 ($5.39 launch discount)
- **Reviews (2026-10-17):** Very Positive — 196 total, 98.0% positive (192 pos / 4 neg); 134 English
- **Developer/publisher:** Cleanup Crew! (also made `Konbini Cleanup`, 229 reviews, 93%; announcements are
  written in the first person, so likely a very small team)
- **Platform:** Windows only. Single-player. 20 languages. Full controller support.

## Premise & scope

A witch's cabin wrecked by "the magic made it worse." The store page says **5,313 curios across 6 rooms**;
the in-game Records page says **6,920 items across 942 shelves ("boards")**. The two numbers disagree and
the cause is unknown. Rooms named so far: Bedroom (the tutorial room, **140 items / 38 shelves**),
Apothecary (next, behind a locked portal door), plus a library, a greenhouse/garden and an enchanting hall
from reviews; the Records page lists the Bedroom and five locked "???" rooms. Rooms unlock through sealed
portal doors, and a library-to-enchanting-room portal gets a specific shout-out. No timers, no fail state.
Length opinions are mixed ("plenty of gaming hours" vs "wish it was a little longer"); the median English
positive reviewer had about 6 hours in at review time. At the point of Kolby's last screenshot: 7 min 49 s
played, 20 items shelved, 3 boards filled.

## Mechanics & abilities (hands-on)

The standout is that **the genre's usual ability list was re-themed and tied into the fiction** rather than
being a generic cooldown tree. Everything in the ledger is bought with **gold**, with **no level gating**
(compare `Magic Mess`, where reviewers hated level-gated skills).

### Charms (passive, gold)

| Charm | Price | Levels | Effect (first level) |
|---|---|---|---|
| Swiftness | 30 | 5 | Base walk speed 0% → +15% |
| Reach | 30 | 5 | Pick-up and place reach 300 cm → 350 cm |
| Bigger Satchel | 30 | 5 | Carry capacity (detail page not seen) |
| Fleet Foot | 100 | 1 | Hold Shift to sprint, +125% |
| Ordered Inventory | 250 | 1 | Held items group themselves by type |
| Neat Stacks | 350 | 1 | Items stacked with Stackweaver group by type. **Needs Stackweaver** |
| Tether | 500 | 1 | Moving near a glimmered item puts it in your inventory (200 cm). **Needs Glimmer** |

### Spells (cast with a number key, gold)

| Spell | Price | Levels | Key | Effect at level 1 |
|---|---|---|---|---|
| Beckon | 50 | 5 | 1 | Fill your inventory with copies of the item you hold. Cooldown 80 s |
| Glimmer | 50 | 5 | 2 | Same-type items glimmer and float, easier to see. Cooldown 45 s, glow 10 s |
| Homeward | 90 | 5 | — | Sends items in your inventory to their shelf. Cooldown 90 s, 5 items per cast |
| Stackweaver | 50 | 5 | 6 | Stacks loose items into piles. Cooldown 60 s, radius 300 cm |
| Amplify Familiar | 100 | 5 | — | Your familiar tidies items, one every 1 s. Cooldown 180 s |

The ladder covers the whole loop: **find** (Glimmer) → **collect** (Tether, Beckon) → **place** (Homeward) →
**have it done for you** (Familiar). Some entries have prerequisites, shown in red on the page (Tether needs
Glimmer, Neat Stacks needs Stackweaver). Cooldowns at level 1 are long (45 to 180 s); how upgrades shorten
them was not seen.

### Other tools and fiction

| Feature | What it does | Notes |
|---|---|---|
| Cat familiar | Summoned; flies around and picks up and shelves items | Reviewers love the swimming-in-air animation and petting it |
| Broom | Unlocked by completing the broom closet; lets you fly the map | Earned, not bought |
| Tarot cards | Opened for random perks | A per-run variable layer |
| Summoning bell | A wall bell: "Ring the bell and summon this room's loose items: **10 gold**" | Hands-on: a gold sink that gathers every loose item in the room; reviewers praise it for stuck or hidden items |
| "No spells" achievement | Playable without any magic | Several reviewers did a pure run |

- **Sorting freedom inside a shelf.** Players place items "however you like, so long as they're on the right
  shelf," and each shelf has four colors, which one reviewer calls "very OCD friendly." A library reviewer
  says they enjoy arranging each section so the books "flow" for a hypothetical reader.
- **Sound and motion per item type**: each item class has its own pickup sound and animation, plus
  per-shelf and per-case completion effects, and reflections on a glossy floor.
- **Settings are a selling point.** One reviewer thanks the dev "10 times over" for a proper settings menu
  after a run of games with the bare minimum (see the settings inventory below).

## Hands-on: interface and feel (Bedroom, first minutes)

### HUD and feedback

- **Top left:** room name with room-scoped counts, "Items 0/140" and "Shelves 0/38". **Top right:** gold and
  gem counters, then a **Tutorial Goal** panel with a checkbox (goals #1 pick up, #3 put away, #4 throw with Q,
  #5 carry several and switch with the mouse wheel). The goals **pay out currency** when completed (Kolby),
  and the panel can be turned off in Game settings.
- **Bottom right:** a held-item card and a big **carry count, "0 / 4"**: the starting capacity is 4, the same
  as BookBoy's. The held item is drawn small at the corner of the screen, so it doesn't cover the center view.
- **Carry list:** one line per carried item with the current one highlighted. A tag under the list shows the
  **category of the selected item** (WAND BOX, TRINKET BOX, BROOM), so it changes as you cycle. Options exist
  for the full list or just the selected name, and for list size.
- **Contextual key hints, bottom left:** E Pick up, mouse Place on shelf, Q Toss, R Stack on floor, mouse wheel
  Next item. The set changes with what you're aiming at and carrying. Pick-up and place are **separate
  buttons** (E and a mouse button); a reviewer disliked this.
- **Focus highlight:** an aimed item gets a white outline and its whole top face tinted purple, with the name
  under the crosshair. The same purple is used on shelves; it is likely the default of the user-selectable
  "Glimmer Colour" setting (not confirmed).
- **Shelf identification, two layers.** A carved **wall plaque with an icon** on each fixture (wand, potion,
  chest), plus a **round UI badge with icon and name** that appears when you aim ("WITCH HAT", "WAND BOX").
  Cubbies are outlined in purple. With a held item aimed at a shelf, a **solid purple ghost of the item's
  shape** shows the exact slot it will take.
- **Shelf progress at the crosshair:** hovering a shelved item shows a section count such as "Ember Wand 9/9"
  (the Oct 16 patch). One cabinet can count as **two shelves** (two 3×3 halves, 9 boxes each).
- **Placement reward (Kolby):** the item **spins into the cubby** and a **particle flies to the gold counter
  as it lands**. Completing a shelf pays extra gold and a purple gem.
- **The mess is semi-organized.** Same-type boxes sit in neat stacks, with ivy and grass overgrowth, warm
  light and floating dust. The pile reads as "something happened here." Item names are readable on the floor
  (for example a book titled "An Encyclopedia of Ghosts & Ghouls"). It is not known whether book titles are
  the jokes reviewers mention.

### Economy and progression (inferred from counters; the formula was not confirmed)

- **Gold** is the steady currency: about 1 per item placed (20 → 38 across 9 placements and a completed
  shelf, so the completion adds roughly as much again), plus tutorial rewards. It buys charms, spells and the
  bell.
- **Gems** are the rare currency from finished shelves, and **unlock rooms**: "Unlock the Apothecary for **20
  gems**." The player held 2 gems after 3 boards filled, so a shelf is not always exactly one gem. 20 gems
  against 38 shelves in the Bedroom suggests you can move on after finishing a bit over half the room (the
  option to return to the map after finishing was added Oct 16).
- The tally "Coins earned 48" against a held balance of 68 suggests tutorial rewards are not counted as
  earned (a guess).
- **Prices** run 30 for entry-level five-level charms up to 500 for Tether. The level-1 prices of every charm
  and spell add up to about 1,630 gold (later levels were not seen), against roughly 7,000 items at about
  1 gold each plus completion bonuses; one reviewer says a no-spell player could afford everything halfway
  through room two.

### The Enchanted Ledger (the upgrade UI)

- A glowing book in a pentagram on a lectern; **E opens it**, Q and E turn the pages, Esc closes. Three tabs:
  **Charms**, **Spells**, **Records**.
- **Left page:** a list with icon, name, level pips and price; unaffordable prices are in red. **Right
  page:** a large icon, one flavor line, a "Level N of M" box with the **before → after change** ("0% →
  +15%", "300 cm → 350 cm", "Not learned yet → 80 s"), an Acquire button with price, and either "Need N"
  (gold short) or "Needs <prerequisite>" under it when blocked. Spells show their hotkey ("Cast with 1").
- **Records page:** a per-room completion bar (Bedroom checked, five locked "???" rooms with two empty badge
  slots each), tallies (time played, items shelved, coins earned, gems earned, boards filled 3 / 942, spells
  cast) and **22 achievements** (2/22 at the time). Examples: shelve your first item; Materia I to IV at 100,
  1,000, 2,000 and 5,000 items; complete a board; learn a charm; master every charm; master every spell.

### Settings menu (five tabs, switched with Q / E keycaps)

| Tab | Options seen |
|---|---|
| Display | Resolution (greyed in borderless), window mode, monitor picker, VSync, FPS limit, quality, graphics API |
| Audio | Master, music, sound FX and UI volume; Cat Meows toggle |
| Image | Motion blur, bloom, dust and potion-room grass toggles (all on, probably defaults), brightness, exposure (per map), saturation, contrast, field of view 90° with a reset button |
| Input | Mouse and controller sensitivity, invert Y, input prompts and controller type (both marked beta), full key rebinding |
| Game | Language, tutorial goals toggle, toggle crouch, toggle sprint, **Low Feedback Mode**, full carry list, carry list size, game recommendations, **Glimmer Colour** (purple default, resettable) |

Both the Image and Input lists scroll, so options below the fold were not seen. **Head-bob and camera height
may be there; not confirmed.** Motion blur appears to default on, which is what motion-sickness reviewers
complained about.

## Update history

| Date | Update | What changed |
|---|---|---|
| 2026-10-02 | Delay announced | Pushed five days to avoid "cutting corners" |
| 2026-10-12 | Launch | Went live roughly three hours after the "just a little longer" post |
| 2026-10-15 | "Day One(ish)" patch | Optimisation: the ship build had a 150k-triangle tree model placed about 50 times; portal resolution halved as a stopgap. Fixed bedroom potions spawning in the apothecary, bell failing to find stuck items, hotkey display after rebinding. Added accessibility/visual toggles. |
| 2026-10-16 | Performance and QoL | Steam Deck "stable for the most part." Collision fixes (a library tree, stairs); moved a missable broom; **inventory UI size slider and a "show only the selected item's name" option**; **a count of items on the shelf board when hovering a shelved item**; vsync default on; resolution dropdown fixed; option to return to the map after finishing. |

**Pattern:** the dev replied within 72 hours with specific, honest fixes (including the tree blunder), which
reviewers cite. The fixes line up with the first-week negatives almost one to one.

## Common complaints (all 4 negatives, plus caveats inside positives)

- **Performance** is the dominant complaint (2 of the 4 negatives, plus a Steam Deck report of stutter that
  triggered motion sickness). A third negative says the game looks nothing like the trailer.
- **Held-item list covers the hands**: a solid-blue inventory panel in the bottom right blocks the view of what
  you are holding. Patched with a slider and a selected-item-only option within four days.
- **Separate pick up and place buttons**: one reviewer found it unrelaxing; binding both to one button broke
  multi-place. Hands-on confirms E to pick up and a mouse button to place.
- **Store art mismatch → AI suspicion.** A 21-hour positive reviewer says the trailer images show rooms that
  don't exist in the game and wonders whether they were AI imagery; they later edit to say the images were
  taken down. Another notes the fireplace in the screenshots has no fire. A good example of how cheaply a
  trust issue is created by store assets that don't match the build.
- **Smaller items inside positives**: book titles blur until you look at them and take time to load; green and
  blue potions look alike in some light (and book colors across categories), a colorblind-accessibility risk;
  music is "elevator music" for one reviewer; the library has minor frame drops; 12 surplus Crimson Roundflask
  Tonics left a shelf unfinishable for two accounts; **in-game price balance** (a no-spell player could afford
  everything halfway through room two), the same weakness a reviewer attributes to Konbini Cleanup; the
  spells "max out too quick for the amount of space."
- **AI mentions: effectively none.** No review accuses the item art or book text of being generated.

## Personal impressions (Kolby, 2026-10-17)

Bought it on release week and described it as **one of the nicest-looking tidy-ups seen so far**. Specific
things called out: the spin-into-place animation with a particle that flies to the gold counter as the item
lands; tutorial goals that pay currency as you complete them; and the ledger, which Kolby said is "really
clean" as a presentation of the upgrades.

## Relevance to BookBoy: what to borrow

Kolby's stated intent is to borrow heavily from this game. Ordered roughly by value for effort. "Today" is
from [04-build-status.md](../04-build-status.md): carry starts at 4 (Strong Arms I–VI to 10), 13 upgrades for
24 to 32 points, points paid once per finished shelf, Sister Glow, Quick Shelve, a Tab upgrade menu, a
controls legend, a run timer, and a HUD with books correct and shelves finished.

| # | Idea from Witch Cabin | BookBoy today | Suggested move | Effort |
|---|---|---|---|---|
| 1 | **Ledger UI**: before → after numbers, level pips, "Need N", visible prerequisites | Tab menu lists upgrades; locked abilities grey out in the legend | Show the change in numbers (capacity 4 → 5), the price, and the reason it is blocked | Small |
| 2 | **Placement reward**: a spin into the slot and a particle that flies to the counter on landing | Outline plus sparkles on a placement; points only at shelf completion | Add a visible "progress" pop on each correct placement, even with no gold | Small |
| 3 | **Two-speed economy**: small per-item gold plus a gem per shelf | Single point per finished shelf | Test a hybrid (partial progress per book, the point on completion); `Megastore` and `Magic Mess` reviewers praised per-placement XP | Medium (design) |
| 4 | **Shelf identity**: carved wall plaque plus an aim-time badge with icon and name; section count on hover | Themed signs with no color words | Compare for colorblind players; add a "2/4" count at the crosshair | Small |
| 5 | **Slot ghost** showing exactly where an item will go | Green/yellow/red feedback after placing | A ghost preview of the slot for the held book | Small |
| 6 | **Carry card** with the selected item's category, and **contextual key hints** | HUD lists what you carry; controls legend | Tag the selected book's series, and show only the keys that apply right now | Small |
| 7 | **Records page** with tallies and milestone achievements | Run time and a completion message | A per-run Records screen (best time, books, series, upgrades used); no persistence needed | Small to medium |
| 8 | **Settings inventory** (FOV, blur, bloom, dust toggles, brightness, contrast, toggle crouch/sprint, Low Feedback Mode, carry-list size, **highlight color**) | Not audited | A checklist to compare against; a recolorable glow is cheap and helps colorblind players | Medium |
| 9 | **Prerequisite chains** (Tether needs Glimmer) | 13 upgrades; unclear which already depend on others | Check that Lingering Glow and Quick Recharge require Sister Glow, and show the "Needs X" reason in the menu | Small |
| 10 | **Paid recall bell** that gathers loose items | No stuck-item recovery | A "call the books" tool, so no book is ever unreachable | Medium |
| 11 | **Tutorial goals** that pay out, toggleable | Not audited | Short goals with a visible checkbox, optional | Small |
| 12 | **Room gates unlocked by the rare currency** | One room | Future wings opened by finished shelves; ties to the "library rebuilds itself" idea | Large |
| 13 | **Familiar** that tidies for you | Quick Shelve flies books along an arc | Give Quick Shelve a character (a cat or spirit) | Medium |
| 14 | **Environmental story** (overgrowth, plaques, props) | Not audited | Dress the room so the mess reads as an event, not noise | Medium |

Cautions from the research:

- **Guardrails.** A currency economy, room unlocking and persistent records brush the non-goals in
  [CLAUDE.md](../../CLAUDE.md) (save/load, progression beyond what Kolby listed). Rebinding keys would touch
  input bindings, which needs explicit approval and a stated plan first.
- **Pricing.** One reviewer says a pure-run player can afford everything by mid-game here. Keep our 13
  upgrades meaningful across the whole run.
- **Cooldowns.** Level-1 cooldowns of 45 to 180 s are long; our Sister Glow is 6 s (3 s with Quick Recharge),
  which is far friendlier. Don't copy the numbers.
- **Store-page honesty.** Screenshots and trailers must match the shipped game, or players read the gap as AI.
- **Held items must not block the view**, as both this game and `Sort of Magic` had to patch.
- Performance: a single mis-imported high-poly asset was most of this launch's frame-rate complaints.
- **This is the nearest thing to a direct competitor for the library fantasy**, and the library is the part
  reviewers rave about ("my absolute favorite was sorting the library"; "I want to read each one"). The hook
  is **funny book titles and publisher names**, which is cheap for us to do well.

## Screenshot index

Kolby's screenshots, 2026-10-17, saved in [img/witch-cabin/](img/witch-cabin/) (27 files, about 7 MB). They
show the first room only.

| # | File | Shows |
|---|---|---|
| 1 | [01-settings-display](img/witch-cabin/01-settings-display.png) | Settings, Display tab |
| 2 | [02-settings-audio](img/witch-cabin/02-settings-audio.png) | Settings, Audio tab (Cat Meows toggle) |
| 3 | [03-settings-image](img/witch-cabin/03-settings-image.png) | Settings, Image tab (blur, bloom, FOV 90) |
| 4 | [04-settings-input](img/witch-cabin/04-settings-input.webp) | Settings, Input tab (rebinding) |
| 5 | [05-settings-game](img/witch-cabin/05-settings-game.webp) | Settings, Game tab (Low Feedback Mode, Glimmer Colour) |
| 6 | [06-first-frame-bedroom-mess](img/witch-cabin/06-first-frame-bedroom-mess.webp) | First frame after the intro: HUD, tutorial goal, the pile, carry 0/4 |
| 7 | [07-witch-hat-shelf-badge-holding-broom](img/witch-cabin/07-witch-hat-shelf-badge-holding-broom.webp) | Shelf badge with icon and name; carry card; key hints |
| 8 | [08-ember-wand-focus-highlight-tutorial-4](img/witch-cabin/08-ember-wand-focus-highlight-tutorial-4.webp) | Focus highlight (outline plus purple tint), potion wall plaque, tutorial goal #4 |
| 9 | [09-wand-box-cabinet-carrying-4of4](img/witch-cabin/09-wand-box-cabinet-carrying-4of4.webp) | Carry list of four, 4 / 4, cabinet with cubbies and plaques |
| 10 | [10-wand-boxes-placed-5of140](img/witch-cabin/10-wand-boxes-placed-5of140.webp) | Four boxes placed, 5/140, gold 20 |
| 11 | [11-shelf-9of9-hover-count-gems](img/witch-cabin/11-shelf-9of9-hover-count-gems.webp) | Full 3x3 half, hover count "9/9", first gem |
| 12 | [12-slot-ghost-trinket-box-carry-card](img/witch-cabin/12-slot-ghost-trinket-box-carry-card.webp) | Purple slot ghost, TRINKET BOX tag, bell price tag |
| 13 | [13-summoning-bell-10-gold](img/witch-cabin/13-summoning-bell-10-gold.webp) | "Ring the bell and summon this room's loose items: 10 gold" |
| 14 | [14-locked-door-unlock-apothecary-20-gems](img/witch-cabin/14-locked-door-unlock-apothecary-20-gems.webp) | Locked portal door, "Unlock the Apothecary for 20 gems" |
| 15 | [15-enchanted-ledger-lectern](img/witch-cabin/15-enchanted-ledger-lectern.webp) | The glowing ledger on its lectern, "Open Enchanted Ledger" |
| 16 | [16-ledger-charms-swiftness](img/witch-cabin/16-ledger-charms-swiftness.webp) | Charms page, Swiftness |
| 17 | [17-ledger-spells-beckon](img/witch-cabin/17-ledger-spells-beckon.webp) | Spells page, Beckon |
| 18 | [18-ledger-charms-reach](img/witch-cabin/18-ledger-charms-reach.webp) | Charms page, Reach (300 cm → 350 cm) |
| 19 | [19-ledger-records-achievements](img/witch-cabin/19-ledger-records-achievements.webp) | Records tab: per-room bars, tallies, achievements |
| 20 | [20-ledger-charms-fleet-foot-need-32](img/witch-cabin/20-ledger-charms-fleet-foot-need-32.webp) | Fleet Foot, greyed Acquire with "Need 32" |
| 21 | [21-ledger-charms-ordered-inventory](img/witch-cabin/21-ledger-charms-ordered-inventory.webp) | Ordered Inventory |
| 22 | [22-ledger-charms-neat-stacks-needs-stackweaver](img/witch-cabin/22-ledger-charms-neat-stacks-needs-stackweaver.webp) | Neat Stacks, "Needs Stackweaver" |
| 23 | [23-ledger-charms-tether-needs-glimmer](img/witch-cabin/23-ledger-charms-tether-needs-glimmer.webp) | Tether, "Needs Glimmer" |
| 24 | [24-ledger-spells-glimmer](img/witch-cabin/24-ledger-spells-glimmer.webp) | Spells page, Glimmer |
| 25 | [25-ledger-spells-homeward](img/witch-cabin/25-ledger-spells-homeward.webp) | Spells page, Homeward |
| 26 | [26-ledger-spells-stackweaver](img/witch-cabin/26-ledger-spells-stackweaver.webp) | Spells page, Stackweaver |
| 27 | [27-ledger-spells-amplify-familiar](img/witch-cabin/27-ledger-spells-amplify-familiar.webp) | Spells page, Amplify Familiar |

## Open questions (not yet seen)

- How spell levels reduce the 45 to 180 s cooldowns, and what Bigger Satchel's levels give.
- Whether every room gate costs 20 gems, and how fast the player reached it.
- What a wrong placement looks like, and whether placing is blocked while the spin plays (`Paver` reviewers
  complained that animations blocked input).
- Whether head-bob and camera height exist below the settings scroll line.
- Why the in-game total (6,920) differs from the store page (5,313).
