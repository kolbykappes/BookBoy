# Backlog

The list of features and chores still to do. Kolby adds to it (just say "add X to the backlog"); an AI agent can be pointed at
any single item ("do F-001") and should have everything it needs here. Finished items move to
[11-changelog.md](11-changelog.md) (with the date) and are deleted from this file.

**Rules for working from this list**

- One item = one small vertical slice, per the guardrails in [../CLAUDE.md](../CLAUDE.md). Split an item if it is too big.
- Items marked **Needs approval** touch something CLAUDE.md fences off (input bindings, `ProjectSettings/`, `manifest.json`,
  render-pipeline settings) or something on the MVP non-goals list ([02-mvp-spec.md](02-mvp-spec.md)). Do not start them
  without a stated plan and an explicit yes from Kolby.
- Status values: `Todo`, `In progress`, `Blocked`. Priority: `P1` next up, `P2` soon, `P3` someday.
- Every item should end with tests where the rule is engine-free (Edit Mode) and a Play Mode check script in `tools/` where it is
  not, plus a line in the changelog and an update to [04-build-status.md](04-build-status.md).

---

## Requested by Kolby

### F-001 Controller (gamepad) support
**Status:** Todo · **Priority:** P1 · **Needs approval** (input bindings are fenced off; see the plan note) · **Added:** 2026-10-17

**Goal:** the whole game, including every menu, is playable and completable with a gamepad alone.

**Details**
- Move, look, jump, sprint (the Starter Assets controller already has gamepad actions for these; verify dead zones and
  look speed feel right).
- Pick up / place = a face button or right trigger; toss = another; cycle held book = bumpers; the four abilities
  (Sister Glow, Quick Shelve, Gather Family, Home Beacon) on the remaining face buttons / d-pad.
- Upgrade menu and admin menu: open with Start / View, move the selection with the d-pad or left stick, buy with A, close with B.
  Optional: the menu pointer (`GameUI` software pointer) driven by the right stick.
- On-screen prompts and the controls legend show the right button names for the device last used (Xbox style is enough to start).
- Aiming at a slot is by the crosshair, which already works with any look input.

**Plan note (approval needed):** today game code reads `Keyboard.current` / `Mouse.current` directly so no input bindings were
changed. The same approach (reading `Gamepad.current` directly) needs no binding changes and is the suggested route; adding
actions to an Input Actions asset would be a binding change and needs a stated plan first.

**Acceptance:** a scripted test drives the Input System with queued `Gamepad` states through pick up, place, toss, cycle, all four
abilities and both menus; legend shows controller glyph names when a gamepad was last used.

### F-002 Achievements
**Status:** Todo · **Priority:** P1 · **Needs approval** (persisting them needs save/load, which is on the non-goals list; see F-003) · **Added:** 2026-10-17

**Goal:** a set of achievements that unlock during play, show a toast when earned, and can be viewed in a list screen.

**Details**
- Engine-free rules in `BookBoy.Rules` (like `UpgradeCatalog` / `UpgradeState`): an `AchievementCatalog` (id, name, description,
  secret yes/no) and an `AchievementState` that is fed events and says what newly unlocked. Edit Mode tests for each rule.
- Events come from existing hooks: `GameProgress` (shelf finished, points earned, level complete, time), ability use, tosses,
  the pile tool (should not count!), carry capacity.
- A toast (reuse `GameUI.Toast` or a dedicated banner) and an achievements screen in the menus (a key, e.g. `J`, and a controller
  button once F-001 lands).
- Steam achievements are a separate later item (needs the Steam SDK and a store page).

**Starter list (proposals, trim or change freely)**
- *First Shelf* - finish one shelf. *Shelf Master* - finish all 32 shelves.
- *Half Restored / Library Restored* - 64 / 128 books correct.
- *Healing Hands* - finish the Healing Magic bookcase (one per theme is possible: 16).
- *Speed Reader* - finish under a time target (e.g. 10 minutes; time targets are also part of F-006).
- *Packrat* - carry 10 books at once. *Pitching Arm* - toss 25 books. *Family Reunion* - gather a whole series in one go.
- *No Shortcuts* - finish without Quick Shelve. *Blindfolded* - finish without Sister Glow or Home Beacon.
- *Fully Upgraded* - buy every upgrade. *Perfect Pairs* - place 20 books in a row without a yellow or red flash.

**Acceptance:** unit tests for every achievement rule; a Play Mode script triggers a few and checks the toast and the list screen;
admin-menu pile-up and "unlock all" do not award achievements.

### F-003 Save progress, best times and achievements
**Status:** Todo · **Priority:** P2 (needed by F-002 to be worth anything) · **Approved in principle** (Kolby, 2026-10-17: "the game has to save"; the non-goals list describes the prototype). Still present the file-format plan before building. · **Added:** 2026-10-17

**Goal:** remember unlocked achievements, best finish time and settings between sessions.

**Details:** a small JSON file in `Application.persistentDataPath`; versioned; never crash on a missing or corrupt file; no mid-run
save (a run is short). Engine-free serialization model in `BookBoy.Rules` with round-trip tests.

---

## Ideas (not requested yet - promote or delete)

### F-004 Settings menu
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-17
Mouse sensitivity, field of view, master volume, controller look speed / dead zone / invert Y. Needs F-003 to remember them.
**From the competitor research (2026-10-17):** camera comfort was the most common new complaint across the genre (motion blur,
head bob, low camera), so also offer: a head-bob toggle, a motion-blur toggle, camera height, a recolorable highlight (also a
colorblind aid), held-item list size, toggle sprint, and a "low feedback" mode. See the settings table in
[competitive/16-witch-cabin-cleanup.md](competitive/16-witch-cabin-cleanup.md). Rebinding keys would touch input bindings (needs approval).

### F-005 Sound
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-17
Nothing makes a sound yet. Pick up, place (three distinct sounds for green / yellow / red), toss thud, ability whooshes, a quiet
ambient loop. Needs a short plan for where the audio files come from (generated, free packs, recorded).

### F-006 Results screen, restart and time targets
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-17
The spec asks for a simple restart option after the library is restored; the completion text exists but check that restart
works and add a results screen (final time, best time, books tossed, abilities used, stars for time targets).

### F-007 Better lighting without the 4-light limit
**Status:** Todo · **Priority:** P3 · **Needs approval** (render-pipeline settings are fenced off) · **Added:** 2026-10-17
URP's Forward renderer lets only 4 extra lights affect any one object, which is why the room has exactly four magelights. Switching
the renderer to Forward+ lifts that limit and allows a lit chandelier / wall sconces. Present the change and its cost first.

### F-008 First-time hints
**Status:** Todo · **Priority:** P3 · **Added:** 2026-10-17
Short on-screen hints the first time a mechanic matters (what the sign plaques are, what the flash colours mean, that finishing
a shelf earns a point). Dismissed forever once seen (needs F-003) or per run until then.

### F-009 Make the abilities feel better
**Status:** Todo · **Priority:** P3 · **Added:** 2026-10-17
Sound and screen feedback for Gather Family / Home Beacon / Quick Shelve, a visible recharge ring on the held book, a brief
slow-motion on the last book of a shelf. Needs playtest feedback first.

### F-010 Borrow list from the competitor research
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-17
The full ranked table (21 rows, with effort estimates) is in [competitive/16-witch-cabin-cleanup.md](competitive/16-witch-cabin-cleanup.md)
under "what to borrow". Split it into single items when picking one. Strongest candidates for this prototype, each small:
- Upgrade menu shows the change in numbers ("capacity 4 to 5") and a "Need N" line instead of just greying out (row 1).
- A visible progress pop on each correct placement, not only at shelf completion (rows 2 and 3).
- A shelf-completion sequence: a pulse along the finished series, a "+1" over each book, the point flying to the HUD (row 18).
- A hover count on shelved books ("2/4") and the placement ghost (rows 4 and 5).
- Check that a tossed book can never be lost or end up out of bounds, and consider a "call the books" recovery (row 10).
- A hotbar-style countdown ring for the four abilities (row 13 and the cooldown note).

---

## Lamplit follow-ups (the S0 to S9 build is done; see [04-build-status.md](04-build-status.md) and the changelog for 2026-10-18)

- **L-010** Playtest Lamplit with the test script at the end of [14-build-plan.md](14-build-plan.md) and note: how long Room 1 takes
  (target about 20 minutes), whether five-item groups feel right, whether colours and letters are easy to tell apart. **P1**
- **L-011** Quick Shelve for the dumbwaiter: `ShelfPlanner` still plans by colour, series and volume; give it Group and Ordered modes
  and the family check, then let a find unlock it. **P2**
- **L-012** Make finds easy to spot: a soft glow on a find until it is picked up, and a hint when a room's finds are still missing. **P2**
- **L-013** Home Beacon at shelf level (light the shelf, not the whole container) and an option to show which shelf already holds a group. **P3**
- **L-014** Rooms 4 to 7, the story layer, save/load (F-003), sound and the feel pass, per [13-concept-draft-inherited-library.md](13-concept-draft-inherited-library.md). **P2**
- **L-015** Placeholder cubes to real shapes and models (see D-005); ceilings and per-room lighting. **P3**
- **L-016 to L-020** Data-driven items, rooms loaded on demand, cheap shelved items and piles: see the next section. **P2, not started**

---

## Data-driven items and cheap rooms (Kolby, 2026-10-18; decided to do later, not yet)

**Why:** Lamplit currently has every item of every room as a real object baked into the scene file (about 345 books, each 3 GameObjects, plus
about 1,100 objects in the rooms). Measured 2026-10-18: 2,019 GameObjects, 950 renderers, 658 colliders in the scene (rooms 2 and 3 are now
switched off at the start, which cut the live numbers to roughly a third, but the objects still exist). That does not scale to the roughly 930
items of the campaign, and it is the wrong shape for save/load. Direction from Kolby:

- Items are **data**, not objects stored in the project: a room is a description (which items, which groups, where the containers are), and the
  items are created from it when needed.
- **Rooms are not loaded during the game until they are unlocked.** Whether a room should be unloaded again when the player leaves it is open
  (see decisions below); in a game this small it may not matter for memory, but it should not be designed out.
- **No physics and no scripts running on items while they sit on a shelf.**
- **No always-on physics on the floor either**: no kicking piles, no simulated piles. Physics only on special occasions (a thrown book in
  flight and what it hits). Interacting with items in a pile has to work without simulating them.

### L-016 Item data model and spawner
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-18
- One data asset (or file) per room that lists its items (family, group, number, symbol, colour, size) and its containers; the scene keeps only
  the room's shell and the container positions, not the items. \`BookDefinition\` stays, but the per-item scene objects and the 128-plus
  generated definition assets go away (definitions can be generated at load from compact data).
- A spawner creates an item from a data row on demand and a **pool** reuses item objects (no create/destroy churn). Items are identified by
  an id so state (where each one is) can be saved and restored.
- The spine labels (letter, number) stop being separate objects: fold them into the item's own mesh or material, so one item is one object, one
  renderer, and can be instanced.
- Engine-free part (rules assembly): the data rows, ids, and a "room layout" description, with tests.
- **Acceptance:** Lamplit looks and plays the same; the saved scene holds no item objects; a room's items can be rebuilt from data at any time;
  all Lamplit suites and the admin commands (give, complete, reset, pile) still work.

### L-017 Rooms load when unlocked (and optionally unload)
**Status:** Todo · **Priority:** P2 · **Depends on:** L-016 · **Added:** 2026-10-18
- A room's shell (walls, floor, containers, lights) and items are created or loaded when its door opens (or when an admin jumps there), not at
  game start. Candidate approaches: additive scenes per room (one \`.unity\` per room, loaded and unloaded), or one scene with rooms spawned
  from prefabs plus the data from L-016. Pick one with a short plan.
- If a room is unloaded when the player leaves, its state (which item is where, found/installed finds, tidy or not) must live in the campaign
  state and be restored on re-entry; this is the same data the save file needs (F-003), so design them together.
- **Acceptance:** at the start of a run only the hub and Room 1 exist; opening a door brings the next room in; measure scene object counts
  and memory before and after.

### L-018 Shelved items are inert
**Status:** Todo · **Priority:** P2 · **Depends on:** L-016 · **Added:** 2026-10-18
- An item on a shelf has no rigidbody, no \`Update\`, no coroutines, no outline objects and no collider of its own if it can be avoided: the
  shelf's aim volume plus the slot lookup already find the item for pick-up, and the grade is computed on events (placed, removed), not
  polled every 0.25 s for every item (replace \`GameProgress.Recompute\` / \`CampaignController.Recompute\` polling with change notifications).
- **Acceptance:** a shelved item costs one mesh renderer and nothing else; per-frame script cost does not grow with the number of shelved items.

### L-019 Piles you can pick from without simulating them
**Status:** Todo · **Priority:** P2 · **Depends on:** L-016 · **Needs a decision (below)** · **Added:** 2026-10-18
- A pile is a **frozen arrangement** (positions and rotations baked or settled once by a short simulation, then stored as data) with no running
  physics: you cannot kick it, it never jitters, and it costs nothing while you walk past.
- Picking an item from a pile must still feel right. Options to try: (a) each item keeps a cheap static trigger collider and the aim ray picks the
  one it hits (today's behaviour); (b) one compound collider per pile and a per-pile list that returns the item under the crosshair; (c) a
  per-pile spatial grid with a look-at test. Items under others should be reachable (pick the top item first, or allow picking any item you can
  aim at).
- Removing an item from a pile leaves the others where they are (no collapse), or does one short, local settle of just the neighbours if that
  looks wrong; decide after trying.
- **Acceptance:** zero rigidbodies in a pile; a pile of 40 items costs no per-frame physics or script time; every item in it can be picked up.

### L-020 Physics only on special occasions
**Status:** Todo · **Priority:** P2 · **Depends on:** L-018, L-019 · **Added:** 2026-10-18
- A thrown book is a real physics body only while it flies and settles. When it lands on or hits other loose items, decide what happens: nothing
  moves (the thrown book just stops, simplest and cheapest), or only the items it actually touches are woken for a moment and then frozen again.
  No other item is ever simulated.
- The "pile up the books" feature (\`RoomZone.PileUp\`) keeps using a short real simulation for the drop and then freezes the result as data.
- **Acceptance:** with no book in flight there are no active rigidbodies in the scene; a thrown book never makes a whole pile slide or scatter.

### Measure first and last
Before starting, record frame time (CPU and GPU, ms), draw calls (SRP batcher and standard), GameObject, renderer and collider counts, and memory in
the hub, in Room 1 and with all rooms open, so the gains are real numbers and not guesses. Repeat after each item above.

---

## Design and research (the real game; see [13-concept-draft-inherited-library.md](13-concept-draft-inherited-library.md))

These are not prototype features. They are the next steps for designing the real game; the order is a suggestion.

### D-001 Prototype the least proven mechanical layers
**Status:** Todo · **Priority:** P1 · **Added:** 2026-10-17
Days 1 to 3 of the concept (group, different types, order) repeat what the genre does. The new ideas are Day 4 (restore: clean
and mend before shelving), Day 5 (identify and decipher with a key card) and Day 6 (reconcile two numbering systems). Build the
smallest slice of Day 4 and Day 5 and **measure items per minute and how fun each is**; the concept's time budget assumes about
4.5 to 6 items per minute throughout. Slice with the usual guardrails (engine-free rules plus tests).

### D-002 Research real archival and conservation techniques
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-17
The lessons in the concept (shelf-reading, dating by watermarks and postmarks, conservation, finding aids, provenance and
accession records, cross-referencing legacy systems) come from general knowledge. Research each properly before writing it into
the game, and the history of library ordering (a first pass confirmed the Pinakes, Dewey's dates and that catalogs were probably
introduced around 800 CE; chained libraries and card catalogs are unverified).

### D-003 Settle the open questions in the concept draft
**Status:** Todo · **Priority:** P1 · **Added:** 2026-10-17
Where the relative is, how many patron portraits to commission and in what style, how long a "day" is and where it saves,
whether the pure (no-tools) ruleset needs its own boards, and whether Day 2 is a real mechanic or a second sorting key.

### D-004 Look into why two narrative-through-mechanic games reviewed worse
**Status:** Todo · **Priority:** P3 · **Added:** 2026-10-17
`Telling Lies` (63%) and `Heaven's Vault` (84%) use the same kind of approach as the best-reviewed games in
[12-narrative-reference-games.md](12-narrative-reference-games.md) but reviewed noticeably worse. Find out why.

### D-005 Decide the visual style and commissioning plan
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-17
No generative AI for art or text. Choose one base 3D style (possibly CC0 packs plus a few hero props), the bookplate or engraving
style for commissioned patron portraits, and check the rights of any public-domain images per item.

---

## Chores and housekeeping

- **H-001** Push the branch and refresh PR #1's description (it still describes an early state; many commits are local only).
- **H-003** Playtest pass by Kolby on the prototype's lighting, floor, hand light and menu pointer; list anything that feels off here.
- **H-005** Merge or keep the branch: `lamplit/day-1` holds the whole Lamplit build (the PR #1 branch is `docs/project-and-competitive-analysis`).
- **H-004** Look at the Editor-generated `BookBoy.slnx` change and decide whether to commit it.

---

## Decisions waiting on Kolby

1. F-001: OK to read `Gamepad.current` directly like the keyboard and mouse (no binding changes)? Which buttons for the four abilities?
2. ~~F-002 / F-003: OK to lift the "no save/load" non-goal?~~ **Answered 2026-10-17:** yes, the real game has to save. Still
   present the file-format plan first (versioned, never crash on a corrupt file; per-zone checkpoints at the end of a "day").
3. Which of the starter achievements do you actually want, and should there be one per bookcase theme?
4. L-017: when the player leaves a room, should it be unloaded (state kept as data) or stay loaded once opened? Leaning: keep it loaded for now, but
   keep the room's state in data so unloading can be switched on later.
5. L-019 / L-020: when a thrown book lands on a pile, is it fine that nothing in the pile moves (the book just stops or rests on top)?
6. L-019: picking from a pile: any item you can aim at, or only the top ones? Items removed from a pile: leave the rest exactly where they are?
7. L-016: are generated asset files for each item definition still wanted in the project, or should item definitions live only in room data
   files (smaller project, easier to edit in bulk)?
