# Concept Draft: The Inherited Library

Status: **brainstorm draft, updated 2026-10-17.** This describes the intended *real game*, not the current
prototype (the prototype in [04-build-status.md](04-build-status.md) is a mechanics test; its 128-book room
and non-goals do not limit this concept; save/load is in scope here). Nothing below is committed design until
Kolby confirms. Research behind it: [12-narrative-reference-games.md](12-narrative-reference-games.md) and
[competitive/](competitive/README.md).

## Decided so far (Kolby)

- **Audience and tone:** cozy players who want to relax. No stress, no failing, no clock.
- **Premise:** you arrive at a library you have **inherited from a family member who is missing**. The
  mystery is where they went and why; the arc is finding them. Intrigue is welcome.
- **No magic.** Decided 2026-10-17, partly because magic libraries are the most crowded corner of the genre.
  The player simply **gets better at being a librarian**: abilities are tools and techniques, learned by
  practice and from the relative's notes.
- **Structure:** **7 days**, each a wing and each teaching one lesson that makes the next day easier. A day is
  one sitting; **no clock**. Target **about 4 to 5 hours** total, with growing intrigue about rescuing the
  relative.
- **Narrative stays small:** patrons appear as **notes**, or at best **2D pictures commissioned from an
  artist**. **No generative AI** for art or text; budget is tight, so books and paper are the main objects.
- **Progression and unlocks:** cleaning and repair verbs, cosmetics and collectibles for completionists, a
  place that visibly restores itself. **No crafting by conversion** (turning items into resources).
- **Leaderboards per zone and mode** (not a single daily board). Save/load is required.

## Story decisions, v2 (Kolby, 2026-10-18) - these supersede the older story sections below

The story was reworked over a long brainstorm. Where the sections below mention Great-Aunt Ines, an island
archive, a collector, a missing-then-returning relative, or themed wings, treat them as **superseded**. The
mechanics sections (the librarian-craft tools, the mechanical ladder, the desk as a hub, the time budget) still
stand.

**Settled**
- **Title: Lamplit.** The library is known locally as **"the Lamp"**.
- **Setting (Kolby, 2026-10-19): autumn 1947.** An Edwardian town library built about 1905, run on worn 1910 to 1930 fittings
  (gaslight and early electric, oak and mahogany, typewriters and card catalogues, brown paper and string). Nothing from the
  1950s or later; the wars are never shown, only felt as thrift and patience. Full period rules and the art direction are in
  [15-art-direction.md](15-art-direction.md).
- You worked there **from age nine to fourteen**, helping light the reading-room lamps, shelving the returns, and so on.
- **Uncle Wilfred** (renamed from Rufus, 2026-10-19), known to you as **Ned**, head librarian for over thirty years, wanted to retire, but nobody was trained to take over
  and nobody knew his systems. He didn't think you would be interested. He wrote you a letter (it arrives about
  seven days before Lamplight Night; the exact timing is open to change).
- **Naming [confirm]:** "Ned" is the player's name for him and how he signs notes to you; "Wilfred" is what the town, the
  door notice and the brass plaque call him.
- **Wilfred has passed away** by the time you arrive. The tone should be warm and quiet (a Stardew grandfather), with
  the grief carried by objects and notes, not scenes.
- The town's leaders will **sell or condemn** the run-down library after Lamplight Night. The only outside
  pressure is **a notice on the door with a checklist** (for example: lamp lit, shelves in order, building safe, a
  librarian named). No inspector, no villain.
- The library is in disarray **on purpose**: Wilfred left it as a **test and training**. Putting it back in order
  teaches you his systems. His personal belongings are gone (cleared out).
- **The test was meant for you.** Wilfred knew he was sick, and this was his last wish: that you learn his systems
  and take over. His illness is part of the story, not hidden.
- **Ending:** pass the checklist, light the lamp, and accept the keys. (The earlier "the town sees itself in what
  the library holds" beat was dropped as too much for a small game.)
- **Left out for now:** neighbors, an executor, a child helper, an appraiser or collector, and magic.

**Still open**
- Whether each system gets a small human reason (for example, the gardening books shelved by season because the town
  plants by the Lamplight calendar) and an optional answer drawer as a hint tier.
- The exact timing of the letter and the countdown (seven days is the working number).
- The checklist items on the door and how they map to the days.
- The wording of Wilfred's last note.

## The concept in brief (proposal)

You inherit an old library that has been reorganized by every generation, so each **wing** has its own look and
its own ordering system (an Edwardian reading room, a 1910s periodicals annex, a 1920s map room, and so on, all inside one
1947 building and nothing newer than 1930; see the Setting decision above). Your
missing relative left notes, and a recurring **mark** (a bookplate) appears in every wing. Restoring a wing
means learning its system, which opens the next one and reveals more of what happened. The finale is
**reopening day**, and the final catalog reveals where your relative went.

Tone reference: a **cozy mystery** set in a village library, with small secrets and no violence.

## Proposed building blocks

| Piece | What it is | Why |
|---|---|---|
| **Wings = episodes** | One wing per episode, each with a distinct palette, furniture kit and ordering rule | Variety and "ages" without time travel; real libraries keep legacy systems side by side |
| **Bundles as shelves** | A wing reopens when its collections are complete (the Stardew community-centre pattern) | Gives completion a purpose and gates content |
| **Day rhythm** | A "day" is a visit: read the mail and a patron's note, sort a section, see a small reveal, close up | Natural save point, no clock, about one sitting per day |
| **Patron notes** | Requests and thanks as letters or slips; a few commissioned 2D portraits in a bookplate or engraving style | Cheap, coherent with the library theme |
| **Librarian craft** | Abilities are real tools and techniques (below), levelled by use and unlocked from the relative's handbook | "Getting better at the job" |
| **The mark and the notes** | One bookplate in every wing; the relative's notes between days | The connective thread across episodes |
| **Archival detective work** | Clues from watermarks, bindings, handwriting, marginalia, shelfmarks | Intrigue with no magic; one satisfying action per clue |
| **Cosmetics and collection** | Decorate wings, hidden volumes, a Records page, bookplates | Completionist layer that never touches leaderboards |
| **Leaderboards** | One board per wing and mode on a fixed layout; a pure (no-tools) ruleset kept separate | Fair comparisons |

## Librarian craft (replaces the magic list)

| Ability idea | Librarian version | Art cost |
|---|---|---|
| Highlight matches | **Cataloguer's eye**: a learned focus; matching items catch the light softly. Also an accessibility option | Shader |
| Carry more, shelve in bulk | **Book truck**: load and wheel a cart | One prop |
| Reach | **Rolling ladder** | One prop |
| Repair and uncover | **Conservator's kit**: soft brush, bone folder, tissue repair; a **UV lamp** reveals faded ink | A few props, shaders |
| Look into the past | **Accession register**: provenance and history lookups | UI |
| A helper | A **page or volunteer** who helps in set ways, as notes | Notes |

Progression: **skills level through use** (shelving, cataloguing, conservation, reference); **handbook pages**
found in each wing teach one technique each; **titles** mark milestones (page, assistant, librarian, head
librarian). No currency to grind. (Details about real archival practice come from general knowledge and still
need proper research before being written into the game.)

## The seven days: a mechanical ladder (strawman, to test)

Each day is one wing and **one new layer of mechanics that adds to the earlier ones**. The lesson at the end of
a day is the tool or technique that helps with the next layer. The archival techniques are general-knowledge
placeholders and need proper research before they are built on.

**Honest note on Days 1 to 3:** group, then different types, then order is the same layering the originator
(Librarian: "color, then title, then volume") already does. Treat them as the tutorial trio, keep them short,
and let Days 4 to 7 (restore, decipher, reconcile, compose) carry most of the differentiation.

| Day | Wing and look | New layer (what you decide) | Lesson and tool | Mystery beat | Items | Sort min |
|---|---|---|---|---|---|---|
| 1 | Reading Room: warm oak, green lamps | **Group**: put like with like | Shelf-reading: cataloguer's eye | The relative has been missing three weeks; a letter; the bookplate in the first book | ~100 | ~20 |
| 2 | Mailroom / Periodicals Annex: bottle-green paint, oak pigeonholes, a brass parcel scale | **Different types, different homes**: books on shelves, magazines in boxes, loose papers in trays | Dating: read dates at a glance; the book truck | Postmarks show the relative kept writing after vanishing | ~110 | ~22 |
| 3 | Map Room: cool blues, flat files | **Order**: volumes, dates and scales in sequence | Finding aids: an index and the rolling ladder | A second correspondent appears; a region is marked on a map | ~120 | ~25 |
| 4 | Conservation Room: stone, candlelight | **Restore**: clean and mend before shelving | The kit and a UV lamp | UV reveals faded ink: "the catalog is wrong on purpose"; the bookplate is the founder's | ~100 | ~30 |
| 5 | Herbarium: greenhouse light | **Identify and decipher**: unlabeled specimens and labels in an old script, using a key card you build up | Provenance: the accession register | Records show the relative was collecting pages of one lost volume | ~130 | ~32 |
| 6 | The Vaults: low lamps, cabinets | **Reconcile**: items carry two numbers from two systems; place by the right one and add a cross-reference slip | Cross-referencing: convert between systems | Vault shelfmarks don't match the catalog: they point somewhere | ~150 | ~34 |
| 7 | The Founder's Study: palettes blend | **Compose**: rebuild the catalog from index cards using every rule | none (head librarian) | The catalog resolves to a place; the relative returns for reopening day | ~220 | ~36 |

Why this feels different each day: tactile matching, then spatial fit, then sequence logic, then slow craft, then
decoding, then reconciling two systems, then synthesis. Each day keeps the earlier rules active.

### Time and size budget (estimates to validate with a prototype)

Items are **generic simple 3D props and books**, like the rest of the genre (Kolby's correction: Organized
Inside is a hand-built puzzle with custom items, so its per-level counts do not apply). **Sort tiers:** about
100 items is a short sort, 300 to 500 a medium sort, 700 and up a long sort. The campaign stays in the short to
short-medium range because each day is mechanically richer; the long tiers live in replay (below).

Rate sanity check from single-reviewer data points in the genre research: Sort Them Ducks about 4,000 items in
about 10 hours (about 7 per minute), Mall about 3,682 in about 8 hours solo (about 8), Magic Mess 3,040 in
about 150 minutes with all abilities (about 20). So roughly 5 to 8 items per minute without strong tools.

Campaign total: about **930 items**, about **199 minutes of sorting** plus about 77 minutes of arrival, lesson
and closing, about **276 minutes (4.6 h)**. The effective rate stays near **4.5 to 6 items per minute** all the
way through: each lesson cancels out the extra thinking the next layer adds, so the tools do not need a dramatic
speed-up. **Prototype and measure this first.**

**Replay: the Grand Sort.** Each wing also offers an optional long sort (about 300 to 700 items) with every
rule from that point unlocked, for leaderboards and completionists. This is where the genre's bigger counts
live, without bloating the story. Each day can also carry two or three optional objectives (for example "clean
ten items", "mend five pages") for a crown rating, as in Organized Inside.

### Anatomy of a day

| Beat | Minutes | What happens |
|---|---|---|
| Arrival | about 3 | Lamp on, mail, a patron's note |
| Briefing | about 2 | The wing and one request |
| Work | 65 to 75% of the day | Sorting the wing |
| Lesson | about 5 | Read a handbook page, then a tiny three-item practice (skippable on replay) |
| Closing | about 2 | Lamp out, save, a hook line in a note |

Extras: three small patron requests per day; one hidden volume per wing on a collection screen; boards per wing
on a fixed layout with tools-allowed and pure rulesets; an optional polite collector who writes letters
wanting to buy the library, with an "open house" on day eight (narrative stakes only).

## Where the relative is (to decide)

1. **Travelling:** they left to find something, out of contact. The finished catalog reveals where (the
   shelfmarks encode a location), and they return for **reopening day**. (My pick: hopeful, no peril, and the
   climactic sort still delivers the answer.)
2. **Sealed in a hidden vault** after an accident. Has peril, so likely wrong for cozy.
3. **A trail of letters** across the wings, ending in a reunion somewhere in or near the library.

## Risks to keep in view

- **Cultural depiction in historical wings:** use real artifacts and research; caricatured designs drew
  serious backlash in Sort Them Ducks.
- **Episodic disconnection:** Neat! (eight stories) reviewed worse. The mark, the notes and the relative are
  the glue.
- **Each wing's rule must be fun on its own;** prototype each cheaply before committing.
- **"Real techniques" can feel like a lecture.** Keep each to one satisfying action, and research accuracy first.
- **With no fantasy hook, the mystery and warmth carry the game,** so the writing is the product.
- **Scope creep from Stardew-like systems.** Hold three pillars: restore (sorting), patrons (notes), decorate.

## Art and writing budget notes

- Objects are mostly books, scrolls and paper; use shaders for fog, dust and colour return.
- Variety between wings through palette, lighting and a few modular shelf pieces, not new props everywhere. The era is fixed
  (1947, nothing newer than 1930), so wings differ by what they were built for and when they were fitted out, not by decade.
- Free sources worth checking per item: public-domain museum images (CC0) and CC0 3D packs (Kenney,
  Quaternius, Poly Haven). Pick one style first to avoid an asset-flip look.
- Human-written text and commissioned portraits only.

## Open questions

- Which of the three relative locations above?
- Whether each mechanical layer is fun on its own. The riskiest are Day 4 (restore: must stay brief and
  satisfying) and Day 5 (decipher: needs hint tiers and an accessible key), and whether Day 7 should be two
  sittings. Prototype Days 4 and 5 early, since Days 1 to 3 copy what the genre already does.
- The real item rate: measure with the prototype (budget assumes about 4.5 to 6 items per minute throughout,
  with each lesson cancelling the extra thinking the next layer adds).
- Is Day 2's "different types, different homes" a real mechanic or just cosmetic fit? If it feels thin, replace
  it with a second sorting key (sort by subject, then by size).
- How many patron portraits to commission, and in what style?
- What is a "day" in minutes of play, and where does the game save?
- Does the pure (no-tools) ruleset need its own boards from the start?
- Research needed: real archival and conservation techniques, and the history of library ordering.
