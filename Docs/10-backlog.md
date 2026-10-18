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
