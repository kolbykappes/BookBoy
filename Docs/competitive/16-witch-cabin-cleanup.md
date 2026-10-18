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
Apothecary (next, behind a gem-locked portal door; **1,911 items / 192 shelves**, about 14 times the
Bedroom, per screenshot 47), plus a library, a greenhouse/garden and an enchanting hall
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
| Bigger Satchel | 30, then 60 | 5 | Carry capacity +1 per level: 4 → 5 after the first purchase, then "5 items → 6 items" for 60 (screenshot 30). Probably 9 at level 5 |
| Fleet Foot | 100 | 1 | Hold Shift to sprint, +125% |
| Ordered Inventory | 250 | 1 | Held items group themselves by type |
| Neat Stacks | 350 | 1 | Items stacked with Stackweaver group by type. **Needs Stackweaver** |
| Tether | 500 | 1 | Moving near a glimmered item puts it in your inventory (200 cm). **Needs Glimmer** |

### Spells (cast with a number key, gold)

| Spell | Price | Levels | Key | Effect at level 1 |
|---|---|---|---|---|
| Beckon | 50, then 100 | 5 | 1 | Fill your inventory with copies of the item you hold. Cooldown 80 s; level 2 is 72 s |
| Glimmer | 50 | 5 | 2 | Same-type items glimmer and float, easier to see. Cooldown 45 s, glow 10 s |
| Homeward | 90, then 160 | 5 | 3 | Sends items in your inventory to their shelf. Cooldown 90 s, 5 items per cast; level 2 is 80 s and 6 items |
| Stackweaver | 50 | 5 | 6 | Stacks loose items into piles. Cooldown 60 s, radius 300 cm |
| Amplify Familiar | 100 | 5 | — | Your familiar tidies items, one every 1 s. Cooldown 180 s |

The ladder covers the whole loop: **find** (Glimmer) → **collect** (Tether, Beckon) → **place** (Homeward) →
**have it done for you** (Familiar). Some entries have prerequisites, shown in red on the page (Tether needs
Glimmer, Neat Stacks needs Stackweaver). Cooldowns at level 1 are long (45 to 180 s). **Level 2 trims only about
10%** (Beckon 80 s → 72 s, Homeward 90 s → 80 s and 5 → 6 items per cast), and **level 2 costs about double**
(Beckon 50 → 100, Homeward 90 → 160), so the long cooldowns stay long for a good while.

### Other tools and fiction

| Feature | What it does | Notes |
|---|---|---|
| Cat familiar | Summoned; flies around and picks up and shelves items | Reviewers love the swimming-in-air animation and petting it |
| Broom | Unlocked by completing the broom closet; lets you fly the map | Earned, not bought |
| Tarot cards | **Hidden golden tarot packs** found in the rooms ("Open the golden tarot pack"); each opens a Major Arcana card with a **permanent perk** | Hands-on: "The Fool", Major Arcana 0, gave **+1 inventory slot** (screenshots 36-37); "The Empress", Major Arcana III, gave a **5% chance that a shelved item pays double gold** (screenshot 46). So perks range from capacity to economy modifiers. Whether a pack's card is random is not known |
| Summoning bell | A wall bell: "Ring the bell and summon this room's loose items: **10 gold**" | Hands-on: a 10-gold sink. Kolby's read (after ringing it): it **gathers "lost" items, such as ones that clipped through geometry**, which this genre is infamous for. The prompt text says "this room's loose items," and tutorial goal #10 says it summons "random items from the floor to your inventory" for "the last few items." Reviewers praise it for stuck or hidden items |
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
- **Validity is shown before you place, and a wrong item gets no ghost (Kolby, screenshot 28).** On a BOOKS
  shelf (open-book plaque) holding a light-purple book on one board and a teal one on another, a held
  **dark-purple** book shows **no ghost**, and the **crosshair becomes an ×** instead of the usual dot or
  diamond. Kolby's reading: because the board already holds a light-purple book, a dark-purple one is the
  wrong type, and the missing ghost tells you so. So the ghost doubles as a "this fits here" cue, and
  **books sort by a finer type than the shelf label** (a BOOKS shelf, with each board taking one book type).
  Screenshot 29 (same shelf, an **empty** board lower down) shows the ghost appearing and the crosshair
  becoming a **+**, with "Place on shelf" in the key hints. So an empty board accepts the book, which
  suggests **the first book placed claims the board** (not confirmed; the boards could be pre-assigned and
  simply open when empty). **Crosshair states seen so far:** small diamond (idle), circle (aiming at an item
  or the bell), **+ (can place)**, **× (cannot place)**.
- **The ghost is a plain, slab-shaped silhouette** in the shelf color, not a textured copy of the book. It
  sits flush in the first free slot at the left of the board, so you can see the footprint and where the
  book will land.
- **Books, as seen:** each spine has a title, an ornament band and a small publisher or author line at the
  foot (the light-purple one reads like "Unlocking the Tarot" with a "Moth & Flame" publisher band, the teal
  one "Brewery Production" with an author line; the held book is "A Guide to Enchanting", carried as "BOOKS
  1 / 4"). The made-up publisher names are the kind of joke reviewers praised. Held books are drawn large, taking up roughly a
  quarter of the lower right of the screen, more than the small boxes seen earlier. The greenhouse is visible
  through a window beside the shelf.
- **Shelved books (screenshot 31).** Books stand **spine-out, edge to edge**, with several copies of the
  same title together: three "Unlocking the Tarot" on the top board, one teal "Brewery Production" on the
  second, three magenta "A Guide to Enchanting" on the bottom. Aiming at a shelved book draws a **white
  outline around that single book**, shows the title with a count (**"Unlocking the Tarot 3/5"**), and the
  hint reads **"E Pick up"**, so **a placed book can be picked back up**. The hover count is **placed / slots on that board**:
  "3/5" here, and screenshot 34 shows "Walnut Jewellery Box 2/3" on a board with two chests and one empty
  slot. Carry capacity is now 0 / 5
  after Kolby's Bigger Satchel purchase.
- **Shelf completion sequence (Kolby, screenshot 33 shows the finished state).** When the last slot of a shelf
  fills, **the items on it pulse in size and flash to a translucent gold light, one after another in
  sequence**; then **the shelf itself plays the same effect and shoots out currency**. Kolby called it "pretty
  satisfying." **Mid-animation frame (screenshot 35):** the **whole cabinet glows a pale, translucent gold**;
  faint **"+1" labels float over each chest**; about seven **hexagonal gold tokens** and one **purple gem** are
  in flight toward the top-right counters (a coin icon sits at the left edge of the counter panel as they
  arrive); gold sparkles hang in the air. The hover label still reads "Walnut Jewellery Box 3/3". The "+1" per
  chest fits a **completion bonus of 1 gold per item on the shelf** (it also matches 9 items paying about 9
  extra earlier). This event moved shelves 17 → 18 and gems 6 → 7, and the counters were still ticking up
  (gold 133 → 137 so far, items 69 → 71). The small yellow glowing object on the floor in screenshots 34 and 35
  is static, so it is not part of the effect; it is most likely a stray **yellow potion flask** (the potion
  shelf in screenshot 42 holds identical glowing yellow flasks). A finished cabinet is neat to look at: 8 hats in a 2 × 4 grid, each centered in its cubby, with
  a witch-hat plaque on top. (`Still Sorting` has a smaller version: books ripple in a wave when a slot
  finishes.)
- **A hidden collectible with a reveal screen (screenshots 36-37).** Kolby found a **golden tarot pack lying on
  the bed**: it is outlined when aimed at, the prompt reads "Open the golden tarot pack", and the corner hint
  reads "E Tarot Pack". Opening it shows a **full-screen reveal**: a purple radial burst with floating
  sparkles, a large card in a gold-toned engraving style, the line "Major Arcana · 0", the title **"The Fool"**
  in a gothic display face, and **"+1 inventory slot"**, with "Press any key to continue". Kolby's read: it
  "just gave me an additional unlock". Carry capacity was 0 / 5 before opening. This is the first permanent
  perk seen that is **found rather than bought**.
- **Spells in play (Kolby, screenshots 38-40).** Kolby bought Beckon (50) and Homeward (90) and said the first
  cast "had some nice animations and sound fx". After casting Beckon, the carry card lists **five copies of
  "Monsters Illustrated"** (tag BOOKS, **carry 5 / 6**, confirming capacity 4 + 1 from the satchel + 1 from The
  Fool). A **spell hotbar** sits at the bottom left: one circular icon per spell with its number key
  underneath; Beckon (key **1**) shows a dark **radial countdown with "74"** seconds left, and Homeward (key
  **3**) is lit and ready. Kolby calls it "a nice animated countdown and circular unlock timer."
- **Homeward mid-cast (Kolby, screenshot 41):** Kolby again praised the animation and sound. The carried books
  **leave the hand as translucent, motion-blurred pale-gold slabs** (the books lose their own color in
  flight), one trailing a **curved line of light**, and fly to their shelves. Items went 76 → 79 and shelves
  20 → 21 during the cast, with the carry count at 2 / 6 and the carry list still showing a faded line as it
  empties. The hotbar now shows **Beckon (key 1) lit and ready and Homeward (key 3) on a "90" radial
  countdown**, so a cast starts the countdown immediately. Level 1 is 5 items per cast, so the other
  carried books wait for the next cast.
- **Completion effect on a bookcase, and the autosave notice (Kolby, screenshot 42).** The same sequence on a
  four-board bookcase: the **whole case glows pale gold**, a stack of fading **"+1"** labels floats up over the
  top board, **gold hexagons and a purple gem arc out toward the top-right counters**, and gold sparkles hang
  in the air. The case holds **five books per board** (purple "Unlocking the Tarot", orange "Monsters
  Illustrated", magenta "A Guide to Enchanting", blue "Astrological Signs with Examples"). A small ornate
  **"Autosaving..." notice** is at the top center of the screen, so the game autosaves (and says so) without a
  menu. Counters moved 79 → 90 items, 21 → 26 shelves, 25 → 78 gold and **10 → 15 gems**: five shelves and
  five gems.
- **Potions shelve two deep (Kolby, screenshot 43).** Unlike the single-row book and box boards, a potion cubby
  holds **a front and a back row**: yellow and pink flasks sit in the front row with more visible behind them,
  and the purple flask in the top row has a yellow one behind it. The held item is a **"Golden Roundflask
  Tonic"** (tag **POTIONS**, carry 1 / 6); the naming pattern, **color + shape + noun** ("Golden / Crimson /
  Azure Roundflask Tonic"), gives many near-twins distinct names. The crosshair is a **+** (placeable) and
  the hints read Place on shelf, Toss, Stack on floor. Depth doubles a shelf's capacity but **hides the back
  row**, which is a readability cost. Hotbar: Beckon on 24, Homeward on 55 seconds.
- **Room cleared, door unlocked (Kolby, screenshots 44-45).** Finishing the last shelf shows a banner at the
  bottom center: **"BEDROOM — TIDIED"** with "**140 Items shelved · 38 Boards filled**" (counters 140/140 and
  38/38; gold 220, gems 27). The room is calm: a **gray cat asleep on the bed**, cat portraits on the wall,
  crystals neatly shelved, and the overgrown grass left in place. **Tutorial goal #9** then reads "Every
  shelf you fill earns a gem. Spend your gems on the sigil of the bedroom door to unlock it." Spending 20 gems
  (27 → 7, as the counter in screenshot 45 shows) **unlocks the door with a burst of white-cyan particles**
  filling the doorway, and the next room is visible beyond: a staircase, daisies, a huge heap of toys and
  items around a bubbling cauldron with purple smoke, and a portal arch. **Tutorial goal #10** follows
  at once: "Can't find the last few items? Ring the summoning bell to summon **random items from the floor to
  your inventory**. For a price." So the bell is pitched as the answer to the last-few-items hunt (and
  Kolby uses it for clipped items too).
- **The second room is a different scale (Kolby, screenshot 47).** The header reads **APOTHECARY, "Items
  6/1911, Shelves 0/192"**, against the Bedroom's 140 and 38, so the game jumps from a tutorial-sized room to
  about 14 times as many items, with roughly 10 items per shelf (the Bedroom had under 4). If the Records
  totals hold (6,920 items, 942 shelves), the other four rooms share the remaining ~4,870 items and ~710
  shelves. The room is tall and open, with long tables, rows of empty shelving, a large cauldron with purple
  smoke on a rug, a portal arch at the back, and enormous heaps of **neon-bright potions, lanterns, jars,
  funnels and scales**. Both spells are ready and the carry card reads 0 / 6.
- **Glimmer in use (Kolby, screenshots 48-49).** Spell 2 makes **every item of the held item's type float up
  and glow translucent purple, and you can see them through shelves**; when it ends they fall back to the
  floor. In the frames: a held "Silver Scales" turns matching scales into purple silhouettes (including
  glints hanging over the empty shelving far away), and a held "Cherrywood Ladle" lifts purple ladle shapes
  out of the heap so they hang above the room. The **hotbar now has three spells** (keys 1, 2, 3) with
  radial countdowns (Beckon 42, Glimmer 42-43, Homeward 29 in screenshot 49), and a **purple duration bar at
  the bottom center** shrinks as the glow runs out (about 64% full in screenshot 48, 77% in 49, one second
  earlier in the cooldown; the level-1 glow lasts 10 s).
- **Carry is now 7** (1 / 7 in both frames, so Bigger Satchel level 2 is bought), and the new category tag is
  **BREWING TOOLS**. Item names keep the **material + noun** pattern ("Silver Scales", "Cherrywood Ladle").
- **Gold with no shelf completions:** 92 → 104 for 12 more items (12/1911 → 24/1911). That is exactly
  **1 gold per item**, which confirms the base payout apart from completion bonuses.
- **Stackweaver in use (Kolby, screenshot 50).** Kolby found it **"weird"**: the spell **stacked random loose
  items vertically into a pile near the player**. The frame shows a single **tall mixed column** (an
  hourglass on top, then shears, a yellow lantern, a blue ornament, another lantern, more yellow and magenta
  pieces) rising from the floor in the middle of the room, with nothing in it grouped by type. That is by
  design: the **Neat Stacks charm (350 gold, needs Stackweaver) is what makes stacks group by type**. The
  hotbar now has **four spells on keys 1, 2, 3 and 6** (Stackweaver is the 6), with Beckon on a 54 s
  countdown. A **black cat** walks the floor near the tower. The cauldron on the right keeps smoking purple,
  and the room has several blue-sigil portal arches. Counters: Apothecary 62/1911, gold 63, gems 7, carry
  0 / 7.
- **The Tarot Collection screen (screenshot 51).** "**Tarot Collection, 2 of 22 found**": 22 slots in two rows
  of 11, laid out in Major Arcana order (the Fool in the first slot, the Empress in the fourth). Found
  cards show their art and name; the rest are a gold-on-black card back with a sun and moon and a "???"
  label. Selecting a card shows its number, name and perk. It means **there are 22 pack-sized collectibles to
  find across the game**, each with a different permanent effect, plus a completion screen to track them. It
  is the same hook as the Records page, aimed at exploration.
- **A second tarot card (screenshot 46).** "The Empress", Major Arcana III, reads "**5% chance a shelved item
  pays double gold**", shown on the same full-screen reveal. The Fool gave capacity, the Empress modifies the
  economy, so the 22 Major Arcana cards probably cover a range of perks.
- **Gems: the tutorial says every shelf earns one, the counter says otherwise.** At 38 of 38 shelves the
  player held 27 gems before spending any, so about 11 shelves paid nothing, or the rule is more subtle than
  the tutorial text. The 20-gem door price is confirmed.
- **The finished-library look.** The bookcases behind are full of neat same-title runs in distinct colors
  (white "An Encyclopedia of Ghosts & Ghouls", yellow "The First Coven" ×5, purple, magenta "A Guide to
  Enchanting", blue "Astrological Signs", teal), a good picture of what "done" looks like in a library.
- **Level-2 previews:** Beckon 80 s → 72 s for 100 (was 50 at level 1); Homeward 90 s → 80 s and 5 → 6 items
  for 160 (was 90). Gold afterwards: 20.
- **Held books are big.** The five held books, drawn large at the lower right, cover about a third of the
  screen's width in this frame, even with the carry card tucked in the corner.
- **The scatter on the rug** is bright and flat-colored: neon pink, cyan and yellow potion flasks, green and
  blue crystals, a yellow ring, and green, blue and purple books lying at angles. Saturated colors read well
  against the purple rug.
- **Room goal.** Tutorial goal #8 reads "Put every item in the bedroom back where it belongs. Tidy the
  bedroom", so the tutorial ends with a whole-room objective.
- **Stack on floor (R) (screenshot 32, Kolby).** The hint "R Stack on floor" stacks what you are holding into a
  pile. Examples: a **tall column of mixed books standing on end** (yellow "The First Coven", two blue
  "Astrological Signs with Examples", a white "An Encyclopedia of Ghosts & Ghouls", another yellow) and a
  **stack of two hats**. The stack is a tidy way to get loose items off the floor before shelving, and the
  Stackweaver spell and Neat Stacks charm build on it (Neat Stacks makes stacks group by type). The same
  frame shows a rack of upright brooms and an empty 2×3 cubby cabinet with a hat plaque.
- **Gem data points** (shelves finished → gems held): 2 → 1, 3 → 2, 7 → 3, 17 → 6, 18 → 7 (mid-animation),
  19 → 8, from screenshots 11, 14, 32, 34, 35 and 36. Gold at those points: 38, 68, 75, 133, 137, 151. The
  last step is informative: the chest cabinet's final two chests (items 69 → 71) took shelves 17 → 19 and
  gems 6 → 8, one gem per finished board, and gold 133 → 151 (+18, more than the +2 for items and the +1
  per item completion bonus would explain, so the rule is not fully understood). The earlier jump from 3 to 7
  shelves for only one more gem does not fit one gem per shelf, so **small shelves may not pay a gem**; that
  is a guess to test. Later data supports it: shelves 21 → 26 gave gems 10 → 15 (screenshots 41 and 42), one
  gem per finished board of 3 to 5 slots; 26 → 30 shelves with 15 → 19 gems (screenshot 43) repeats it.
- **Items distinguished by material, not just hue (screenshot 34, Kolby: "reasonably distinct looking items
  made of wood are nice").** The chest cabinet holds dark walnut jewellery boxes with brass clasps and pale
  wooden chests with leather straps, so types differ in **brightness, material and detail**, not only color.
  That is friendlier to colorblind players than hue alone. A small glowing yellow item sits on the floor at the
  right edge of the frame (most likely a yellow potion flask, per screenshot 42).
- **Counter check (confirmed by Kolby):** between the ledger screenshots and this one, items went 20 → 23
  and gold 68 → 61: **1 gold per item** (+3) with the 10-gold bell rung once (−10).
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
  shelf; the "+1 per item" popups in screenshot 35 suggest the completion adds one more gold per item on the
  shelf), plus tutorial rewards. It buys charms, spells and the
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
- **First purchase (Kolby, screenshot 30):** after buying Bigger Satchel level 1, the first level pip fills,
  the page now reads "Level 1 of 5, **5 items → 6 items**", and the **next level costs 60 (double the 30 for
  level 1)** with a greyed Acquire and "Need 9" (gold 51 against 60). A **Steam-style achievement toast**
  ("How Charming, Learn a charm") pops in the bottom-right corner right away, so the very first upgrade
  purchase is celebrated. Whether each level doubles again is not known.
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

**The caution (Kolby, after reaching the Apothecary):** the second room "feels too big", with "too much
duplication", which Kolby found annoying. "The production is nice", but Kolby asked whether the goal of the
game is really just **number go up on duplicate items**, and felt that **has a ceiling on fun**. Treat this as a
design constraint for BookBoy: borrow the production, not the scale. Evidence for and against is in
[the genre review](README.md); in short, duplication and padding are among the most common negative-review
themes, but many cozy players choose these games for exactly that repetitive, podcast-friendly loop.

## Relevance to BookBoy: what to borrow

Kolby's stated intent is to borrow heavily from this game. Ordered roughly by value for effort. "Today" is
from [04-build-status.md](../04-build-status.md) and [11-changelog.md](../11-changelog.md) (checked
2026-10-17): carry starts at 4 (Strong Arms I–VI to 10), a 24-upgrade tree costing exactly 32 points, points
paid once per finished shelf, four abilities (Sister Glow, Quick Shelve, Gather Family, Home Beacon) each on
its own 60 s timer (40 s and 20 s with recharge upgrades), a Tab upgrade menu, a controls legend, a run timer,
and a HUD with books correct and shelves finished. Note this describes the *prototype*; see
[13-concept-draft-inherited-library.md](../13-concept-draft-inherited-library.md) for the real game.

| # | Idea from Witch Cabin | BookBoy today | Suggested move | Effort |
|---|---|---|---|---|
| 1 | **Ledger UI**: before → after numbers, level pips, "Need N", visible prerequisites | Tab menu lists upgrades; locked abilities grey out in the legend | Show the change in numbers (capacity 4 → 5), the price, and the reason it is blocked | Small |
| 2 | **Placement reward**: a spin into the slot and a particle that flies to the counter on landing | Outline plus sparkles on a placement; points only at shelf completion | Add a visible "progress" pop on each correct placement, even with no gold | Small |
| 3 | **Two-speed economy**: small per-item gold plus a gem per shelf | Single point per finished shelf | Test a hybrid (partial progress per book, the point on completion); `Megastore` and `Magic Mess` reviewers praised per-placement XP | Medium (design) |
| 4 | **Shelf identity**: carved wall plaque plus an aim-time badge with icon and name; section count on hover | Themed signs with no color words | Compare for colorblind players; add a "2/4" count at the crosshair | Small |
| 5 | **Slot ghost** showing exactly where an item will go, only for valid slots, with an × crosshair when invalid | Green/yellow/red feedback after placing | A ghost preview of the slot for the held book. Consider making the "only valid slots ghost" cue a **hint level** (off / ghost on valid / ghost plus ×), since it nearly answers the puzzle; their Low Feedback Mode and DinoBones' hint toggles point the same way | Small to medium |
| 6 | **Carry card** with the selected item's category, and **contextual key hints** | HUD lists what you carry; controls legend | Tag the selected book's series, and show only the keys that apply right now | Small |
| 7 | **Records page** with tallies and milestone achievements | Run time and a completion message | A per-run Records screen (best time, books, series, upgrades used); no persistence needed | Small to medium |
| 8 | **Settings inventory** (FOV, blur, bloom, dust toggles, brightness, contrast, toggle crouch/sprint, Low Feedback Mode, carry-list size, **highlight color**) | Not audited | A checklist to compare against; a recolorable glow is cheap and helps colorblind players | Medium |
| 9 | **Prerequisite chains** (Tether needs Glimmer) | 24 upgrades; unclear which already depend on others | Check which upgrades require an ability (recharge upgrades should), and show the "Needs X" reason in the menu | Small |
| 10 | **Paid recall bell** that pulls random floor items into your inventory (pitched for "the last few items"; also rescues clipped ones) | No stuck-item recovery; our Toss uses real physics, so a book could end up out of reach or out of bounds | A "call the books" tool, so no book is ever unreachable; worth a test that a tossed book can never be lost | Medium |
| 11 | **Tutorial goals** that pay out, toggleable | Not audited | Short goals with a visible checkbox, optional | Small |
| 12 | **Room gates unlocked by the rare currency** | One room | Future wings opened by finished shelves; ties to the "library rebuilds itself" idea | Large |
| 13 | **Familiar** that tidies for you; **Homeward** sends carried items flying to their shelves | Quick Shelve already flies each book along an arc with a spin and a glow trail, and has no item cap per cast | We are close to parity on the effect: keep ours. Consider the uniform gold ghost-and-blur look for the flight, a hotbar countdown, and a character (a cat or spirit) for the caster | Medium |
| 14 | **Environmental story** (overgrowth, plaques, props) | Not audited | Dress the room so the mess reads as an event, not noise | Medium |
| 15 | **Stack on floor (R)**: stack what you hold into a tidy pile; spell and charm build on it | Toss leaves books where they land | A "pile what I'm holding" key would keep loose books from scattering and gives Toss a tidy counterpart. Kolby found the area spell Stackweaver "weird" (a tall mixed tower), and tidy by-type stacking costs an extra 350-gold charm, so if we copy it, group by series from the start | Small to medium |
| 16 | **Placed items can be picked back up**, with a per-book outline, a title and a count on hover | Shelved books can be picked up while there is room | Already matches; add the title-and-count line on hover | Small |
| 17 | **Whole-room tutorial goal** ("Tidy the bedroom") | Not audited | End any tutorial with the real objective, so the first goal is the game itself | Small |
| 18 | **Shelf completion sequence**: items pulse and flash gold in order, then the shelf bursts currency | A point is paid when a shelf finishes, with toasts and a HUD update (completion effect not audited) | A sequenced pulse along the finished series (volume 1 to 4, then the case), with a "+1" over each book and the point flying to the HUD. It lines up with the per-placement reward idea (row 2) | Small to medium |
| 19 | **Hidden collectible that grants a permanent perk**, with a full-screen reveal (golden tarot pack: "The Fool, +1 inventory slot"; "The Empress, 5% double gold") | None; every upgrade is bought | A "rare volume" hidden in the room that opens a reveal card and gives a perk. It matches the "forbidden book" idea in [01-vision-and-design.md](../01-vision-and-design.md) and rewards looking around | Medium |
| 20 | **Room-cleared banner** ("BEDROOM — TIDIED" with tallies), then a gem-gated door that unlocks with a particle burst and reveals the next room | A completion message with the final time | Show the tallies and a short celebration on the last book, then hand the player the next thing to do | Small |
| 21 | **Glimmer**: same-type items float up and glow translucent purple, **visible through shelves and from across the room**, with a duration bar and a cooldown ring on the hotbar | Sister Glow pulses an outline on every unshelved book of the held series, **already x-ray through occluders** (changelog 2026-10-17), on a 60 s timer (40 s or 20 s with recharge upgrades) | We are at parity on seeing through shelves. Remaining ideas: lift the glowing books slightly, and show the remaining glow time as a bar | Small |

Cautions from the research:

- **Guardrails.** A currency economy, room unlocking and persistent records brush the non-goals in
  [CLAUDE.md](../../CLAUDE.md) (save/load, progression beyond what Kolby listed). Rebinding keys would touch
  input bindings, which needs explicit approval and a stated plan first.
- **Pricing.** One reviewer says a pure-run player can afford everything by mid-game here. Keep our 24
  upgrades meaningful across the whole run.
- **Cooldowns.** Level-1 cooldowns of 45 to 180 s, and level 2 only shaves about 10% for about double the
  price. Our abilities already run on 60 s timers (40 s and 20 s with recharge upgrades), so we are in the same
  range, and our recharge upgrades are far stronger per step (about 33% and 67%). The **spell hotbar with a
  circular countdown** is worth copying either way.
- **Store-page honesty.** Screenshots and trailers must match the shipped game, or players read the gap as AI.
- **Held items must not block the view**, as both this game and `Sort of Magic` had to patch.
- Performance: a single mis-imported high-poly asset was most of this launch's frame-rate complaints.
- **This is the nearest thing to a direct competitor for the library fantasy**, and the library is the part
  reviewers rave about ("my absolute favorite was sorting the library"; "I want to read each one"). The hook
  is **funny book titles and publisher names**, which is cheap for us to do well.

## Screenshot index

Kolby's screenshots, 2026-10-17, saved in [img/witch-cabin/](img/witch-cabin/) (51 files, about 16 MB, still
growing). They show the first room only.

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
| 28 | [28-books-shelf-no-ghost-x-crosshair-wrong-type](img/witch-cabin/28-books-shelf-no-ghost-x-crosshair-wrong-type.webp) | **Books.** A BOOKS shelf with a light-purple and a teal book; the held dark-purple book gets no ghost and the crosshair turns to an ×, because it is the wrong type for that board |
| 29 | [29-books-empty-board-book-ghost-plus-crosshair](img/witch-cabin/29-books-empty-board-book-ghost-plus-crosshair.webp) | Same shelf, aimed at an empty board lower down: a solid purple book-shaped ghost appears in the first free slot, the crosshair is a +, and the prompt reads "Place on shelf" |
| 30 | [30-ledger-bigger-satchel-bought-level1-achievement-toast](img/witch-cabin/30-ledger-bigger-satchel-bought-level1-achievement-toast.webp) | Bigger Satchel after buying level 1: "5 items → 6 items" for 60, "Need 9", and the "How Charming" achievement toast |
| 31 | [31-books-on-shelf-3of5-hover-pickup-outline](img/witch-cabin/31-books-on-shelf-3of5-hover-pickup-outline.webp) | **Books shelved.** Three "Unlocking the Tarot" copies spine-out on one board, a teal "Brewery Production" on the next, three "A Guide to Enchanting" below; aiming at one shows a white outline, "Unlocking the Tarot 3/5" and "E Pick up"; tutorial goal #8, carry 0/5 |
| 32 | [32-stack-on-floor-book-tower-and-hats](img/witch-cabin/32-stack-on-floor-book-tower-and-hats.webp) | **Stack on floor (R).** A tall mixed column of books (yellow, blue, white) standing on end, a stack of two hats nearby, a broom rack, an empty 2×3 cubby cabinet; counters 37/140, 7/38, 75 gold, 3 gems |
| 33 | [33-finished-hat-cabinet-2x4](img/witch-cabin/33-finished-hat-cabinet-2x4.webp) | A **finished** hat cabinet (2 columns × 4 rows, 8 hats, witch-hat plaque); Kolby describes the completion effect that plays on it (cropped shot, no HUD) |
| 34 | [34-chest-cabinet-walnut-2of3-wood-types](img/witch-cabin/34-chest-cabinet-walnut-2of3-wood-types.webp) | Chest cabinet: dark walnut chests with brass clasps, pale wooden chests with leather straps; focused chest shows "Walnut Jewellery Box 2/3"; counters 69/140, 17/38, 133 gold, 6 gems |
| 35 | [35-shelf-completion-pulse-mid-animation](img/witch-cabin/35-shelf-completion-pulse-mid-animation.webp) | **Completion pulse mid-animation** on the chest cabinet: the whole fixture glows pale gold, "+1" pops over the chests, gold tokens and a purple gem fly toward the HUD; counters 71/140, 18/38, 137 gold, 7 gems |
| 36 | [36-golden-tarot-pack-on-bed-prompt](img/witch-cabin/36-golden-tarot-pack-on-bed-prompt.webp) | A **hidden golden tarot pack** lying on the bed, outlined, with the prompt "Open the golden tarot pack" and hint "E Tarot Pack"; counters 71/140, 19/38, 151 gold, 8 gems |
| 37 | [37-tarot-reveal-the-fool-plus-1-slot](img/witch-cabin/37-tarot-reveal-the-fool-plus-1-slot.webp) | Full-screen **card reveal**: purple radial burst, sparkles, an engraving-style card "The Fool" (Major Arcana · 0), "+1 inventory slot", "Press any key to continue" |
| 38 | [38-ledger-spells-homeward-level2-preview](img/witch-cabin/38-ledger-spells-homeward-level2-preview.webp) | Spells page after buying Beckon and Homeward: both show a filled first pip; Homeward level 2 = "Cooldown 90s, 5 items → 80s, 6 items" for 160 ("Need 140"); gold 20 |
| 39 | [39-ledger-spells-beckon-level2-preview](img/witch-cabin/39-ledger-spells-beckon-level2-preview.webp) | Beckon level 2 = "80s → 72s" for 100 ("Need 80") |
| 40 | [40-beckon-used-spell-hotbar-cooldown-carry-5of6](img/witch-cabin/40-beckon-used-spell-hotbar-cooldown-carry-5of6.webp) | **Spells in use.** After casting Beckon the hands hold five "Monsters Illustrated" (carry 5 / 6); a **spell hotbar** at the bottom left shows Beckon (key 1) with a circular **74 s countdown** and Homeward (key 3) ready; a rug strewn with bright potions, gems and books; counters 76/140, 20/38, 20 gold, 9 gems |
| 41 | [41-homeward-cast-mid-flight-books-fly-to-shelf](img/witch-cabin/41-homeward-cast-mid-flight-books-fly-to-shelf.webp) | **Homeward mid-cast:** carried books leave the hand as translucent, motion-blurred pale-gold slabs with a curved light trail, flying to their shelves; the hotbar now shows Beckon ready and Homeward on a "90" countdown; the room's finished bookcases are visible behind; counters 79/140, 21/38, 25 gold, 10 gems |
| 42 | [42-bookcase-completion-plus1-coins-gem-autosaving](img/witch-cabin/42-bookcase-completion-plus1-coins-gem-autosaving.webp) | **Completion effect on a bookcase:** the whole case glows pale gold, "+1" labels over the books, gold hexagons and a purple gem fly out toward the top-right counters; an **"Autosaving..." notice** at top center; four boards of five books each (purple, orange, magenta, blue); counters 90/140, 26/38, 78 gold, 15 gems |
| 43 | [43-potion-cabinet-two-deep-golden-roundflask-tonic](img/witch-cabin/43-potion-cabinet-two-deep-golden-roundflask-tonic.webp) | **Potions shelve two deep.** A potion cabinet with yellow, pink and purple flasks, front and back rows visible in each cubby; the held "Golden Roundflask Tonic" (tag POTIONS, 1 / 6); hotbar countdowns 24 and 55; counters 106/140, 30/38, 118 gold, 19 gems |
| 44 | [44-bedroom-tidied-banner-gem-door-goal-sleeping-cat](img/witch-cabin/44-bedroom-tidied-banner-gem-door-goal-sleeping-cat.webp) | **Room cleared:** the banner "BEDROOM — TIDIED, 140 Items shelved · 38 Boards filled"; counters 140/140, 38/38, 220 gold, 27 gems; tutorial goal #9 ("Every shelf you fill earns a gem. Spend your gems on the sigil of the bedroom door"); a gray cat asleep on the bed and cat portraits on the wall; crystals on a shelf |
| 45 | [45-door-unlocked-particle-burst-next-room-bell-goal](img/witch-cabin/45-door-unlocked-particle-burst-next-room-bell-goal.webp) | **Door unlocked:** a burst of white-cyan particles in the doorway, the next room visible beyond (stairs, daisies, a big mess around a bubbling cauldron, a portal arch); gems 27 → 7; tutorial goal #10 about the summoning bell |
| 46 | [46-tarot-reveal-the-empress-double-gold-chance](img/witch-cabin/46-tarot-reveal-the-empress-double-gold-chance.webp) | A second tarot reveal: "The Empress" (Major Arcana · III), "5% chance a shelved item pays double gold" |
| 47 | [47-apothecary-1911-items-192-shelves](img/witch-cabin/47-apothecary-1911-items-192-shelves.webp) | **The Apothecary:** "Items 6/1911, Shelves 0/192"; a huge heap of bright potions, lanterns, jars and bottles, long tables, rows of empty shelving, a large cauldron with purple smoke on a rug, a portal arch at the back; counters 226 gold, 7 gems; both spells ready, carry 0 / 6 |
| 48 | [48-glimmer-active-purple-scales-through-shelves](img/witch-cabin/48-glimmer-active-purple-scales-through-shelves.webp) | **Glimmer active** with a held "Silver Scales" (BREWING TOOLS, 1 / 7): matching items glow translucent purple, including small purple glints floating over the empty shelving; three-spell hotbar with Glimmer on 42 and a purple duration bar at the bottom center; Apothecary 12/1911, 92 gold |
| 49 | [49-glimmer-active-ladles-float-hotbar-three-spells](img/witch-cabin/49-glimmer-active-ladles-float-hotbar-three-spells.webp) | **Glimmer with a held "Cherrywood Ladle":** purple ladle shapes float out of the pile and hang above the room; all three spells on cooldown (42, 43, 29); the duration bar is longer than in 48; Apothecary 24/1911, 104 gold |
| 50 | [50-stackweaver-mixed-item-tower-four-spell-hotbar](img/witch-cabin/50-stackweaver-mixed-item-tower-four-spell-hotbar.webp) | **Stackweaver result:** one tall column of mixed loose items (an hourglass, shears, lanterns, a blue ornament) standing on the floor; the hotbar now has four spells (keys 1, 2, 3, 6; Beckon on 54); a black cat on the floor; Apothecary 62/1911, 63 gold, carry 0 / 7 |
| 51 | [51-tarot-collection-2-of-22](img/witch-cabin/51-tarot-collection-2-of-22.webp) | **Tarot Collection screen:** "2 OF 22 FOUND", two rows of 11 slots in numeric order; "The Fool" (slot 0) and "The Empress" (slot III) are revealed, the other 20 show a card back with a sun-and-moon design and "???"; the selected card's perk is shown below ("0 The Fool, +1 inventory slot"); arrow-key navigation and Esc to go back |

## Open questions (not yet seen)

- How far cooldowns fall by level 5 (level 2 trims about 10%), and whether prices keep doubling past level 2
  (30 → 60, 50 → 100, 90 → 160 so far).
- Whether every room gate costs 20 gems, and how fast the player reached it.
- Whether a wrong-type item can be placed at all (screenshot 28 suggests it is blocked, with an × crosshair),
  whether a board's type is really set by its first book (screenshot 29 suggests so), whether picking a
  placed book back up (possible, per screenshot 31) is how you reset a board's type, how
  gems are earned (7 shelves gave 3), and whether placing is blocked while the spin plays (`Paver`
  reviewers complained that animations blocked input).
- Whether head-bob and camera height exist below the settings scroll line.
- Why the in-game total (6,920) differs from the store page (5,313).
