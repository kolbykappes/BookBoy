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
- **L-021** Palette and contrast against the shelf (from the scale study, 2026-10-18): every group must stand out from the wood. Measured in Lamplit: ratio of
  book to wood luminance averaged 2.0 but ranged from 4.8 (yellow) down to 0.7 (purple, navy), i.e. some groups are darker than the shelf. Give `GroupColor` a
  lightness floor, avoid dark navy, purple and maroon, keep the wood dark and warm and the room lit enough, and re-measure (target: each group 2.0 or more, average
  2.5 or more; method and numbers in [competitive/scale-and-camera-comparison.md](competitive/scale-and-camera-comparison.md)). Keep letters, not colour alone, as the
  identifier. **P2, decided to do later**
- **L-022** Ghost of the held item in the aimed slot (Librarian and Witch Cabin do this): a translucent silhouette of the carried book in the exact slot, tinted by
  how it would grade (green, yellow, red), in place of today's thin highlight bar. Needs the slot to know the held item's size and the grade preview from the rules
  (engine-free, tested). **P2, decided to do later (before L-023)**
- **L-023** Make claims visible: when the first group is placed on a shelf, a nameplate or small label appears on the board edge with the group's letter and name
  (Shelves and Sorcery does this); cleared when the shelf empties. Today the first-book-claims rule is invisible. Try after L-022 and decide whether both are needed.
  **P3, decided to do later**
- **L-015** Placeholder cubes to real shapes and models (see D-005); ceilings and per-room lighting. **P3**
- **L-016 to L-020** Data-driven items, rooms loaded on demand, cheap shelved items and piles: see the next section. **P2, not started**

---

## Data-driven items and cheap rooms (Kolby, 2026-10-18; decided to do later, not yet)

**Why:** Lamplit currently has every item of every room as a real object baked into the scene file (about 345 books, each 3 GameObjects, plus
about 1,100 objects in the rooms). Measured 2026-10-18: 2,019 GameObjects, 950 renderers, 658 colliders in the scene (rooms 2 and 3 are now
switched off at the start, which cut the live numbers to roughly a third, but the objects still exist). That does not scale to the roughly 930
items of the campaign, and it is the wrong shape for save/load.

**Decided (Kolby, 2026-10-18):**
- **Items are dynamic data, not objects or asset files in the project.** Rooms (walls, containers, lights) are static scene content; the items that
  get put away are created from data. Item definitions live only in data files, not as one asset per item.
- **Rooms are not loaded until they are unlocked, and once loaded they stay loaded.** But a room's state is stored as data, so unloading a room
  can be switched on later without redesign.
- **Shelved items are inert**: no physics, no scripts.
- **Physics must still work on piles** (a pile that ignores a thrown book looks wrong), but it is not running all the time: at rest everything is
  frozen and cheap; physics is switched on locally, around where something happens (see L-020). No walking into piles to kick them apart.

### L-016 Item data model, data files and spawner
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-18
- Item definitions (family, group, number, symbol, colour, size, name) live **only in data files** (for example one JSON or text file per room under
  `Assets/_Project/Data/`, loaded at run time); the 128-plus per-item `BookDefinition` asset files and the per-item scene objects go away.
  `BookDefinition` can stay as a runtime object created from a data row. The builder (`LamplitBuilder`) writes and reads these files.
- The scene keeps only each room's shell, containers and finds' desk spots, not the items. A spawner creates an item from a data row on demand,
  and a **pool** reuses item objects (no create/destroy churn). Every item has a stable id, so where each one is can be saved and restored.
- The spine labels (letter, number) stop being separate objects: fold them into the item's own mesh or material, so one item is one object, one
  renderer, and can be instanced.
- Engine-free part (rules assembly): the data rows, ids, parsing and validation (never crash on a bad file), and a "room layout" description,
  with tests.
- **Acceptance:** Lamplit looks and plays the same; the saved scene holds no item objects and the project holds no per-item assets; a room's items
  can be rebuilt from data at any time; all Lamplit suites and the admin commands (give, complete, reset, pile) still work.

### L-017 Rooms load when unlocked, with state kept as data
**Status:** Todo · **Priority:** P2 · **Depends on:** L-016 · **Added:** 2026-10-18
- A room's shell (walls, floor, containers, lights) and items are created or loaded when its door opens (or when an admin jumps there), not at
  game start. Candidate approaches: additive scenes per room (one `.unity` per room, loaded and unloaded), or one scene with rooms spawned
  from prefabs plus the data from L-016. Pick one with a short plan.
- **Once opened a room stays loaded** (decided). But all of its state lives in the campaign data, not in scene objects: where each item is (id,
  position, rotation, on which slot), which finds are installed, whether it is tidy. That is the same data the save file needs (F-003), so design
  them together.
- Unloading is an easy switch to turn on later: a `RoomZone.Unload()` that writes the state to data and removes the room, and a matching load that
  restores it. Build it and test the round trip (unload then reload gives back exactly the same room), even though the game does not call it yet.
- **Acceptance:** at the start of a run only the hub and Room 1 exist; opening a door brings the next room in; an unload and reload restores every
  item's place; measure scene object counts and memory before and after.

### L-018 Shelved items are inert
**Status:** Todo · **Priority:** P2 · **Depends on:** L-016 · **Added:** 2026-10-18
- An item on a shelf has no rigidbody, no `Update`, no coroutines, no outline objects and no collider of its own if it can be avoided: the
  shelf's aim volume plus the slot lookup already find the item for pick-up, and the grade is computed on events (placed, removed), not
  polled every 0.25 s for every item (replace `GameProgress.Recompute` / `CampaignController.Recompute` polling with change notifications).
- **Acceptance:** a shelved item costs one mesh renderer and nothing else; per-frame script cost does not grow with the number of shelved items.

### L-019 Piles at rest are frozen and cheap; picking from a pile
**Status:** Todo · **Priority:** P2 · **Depends on:** L-016 · **Added:** 2026-10-18 · **Decided 2026-10-18:** any item you can see is pickable; pulling one out makes the neighbours settle
- A pile at rest is a **frozen arrangement**: positions and rotations stored as data, no rigidbodies, no running physics. It never jitters and
  costs nothing while you walk past; the player's walking never pushes it (decided: no kicking piles).
- **Any item you can see can be picked up** (decided): if the crosshair is on it and it is in reach, E or click takes it, even when it lies under or between others. Options to try for finding it: (a) each item keeps a cheap static trigger collider and the aim ray picks the
  one it hits (today's behaviour); (b) one compound collider per pile and a per-pile list that returns the item under the crosshair; (c) a
  per-pile spatial grid with a look-at test.
- **Taking an item out of a pile makes the pile react**: the neighbours around the gap wake, settle (they may slump or slide into the gap) and
  freeze again, using the same local physics as L-020. The new resting positions are stored as data.
- **Acceptance:** zero active rigidbodies in a pile at rest; a pile of 40 items costs no per-frame physics or script time; every item in it can be
  picked up; removing one makes only the nearby items move.

### L-020 Local physics bubble (physics works, only where something happens)
**Status:** Todo · **Priority:** P2 · **Depends on:** L-018, L-019 · **Added:** 2026-10-18
- When a book is thrown, work out where it will land as soon as it leaves the hand: step its flight path forward (a few dozen short raycasts or
  sphere casts under gravity) to the first thing it hits. Around that spot wake every frozen item inside a sphere (a starting radius of about
  0.6 m and a cap of about 40 bodies, both tunable) so real collisions can happen: the thrown book plows into the pile, items shift, a few slide
  away. The bodies outside the sphere stay frozen and still act as solid walls for the ones that move.
- The thrown book is a real physics body while it flies and settles. When the bubble has been still for a moment (or after a time limit of a few
  seconds), everything in it is frozen again and the new positions are stored as data. If the book lands on empty floor nothing else wakes.
- The same bubble is used when an item is taken out of a pile (L-019) and by "pile up the books" (`RoomZone.PileUp`), which keeps its short real
  drop and then freezes the result.
- **Keep it small (decided):** one local bubble, no chained or cascading physics: bodies outside the sphere never wake because a body inside hit them, and the
  bubble freezes again as soon as it is calm. The aim is only that a throw or a pulled book feels a bit more natural than nothing moving at all.
- An engine-free part (rules assembly) holds the flight prediction math and the "which items are inside the sphere, and the cap" choice, with tests.
- **Acceptance:** with no book in flight and no bubble active there are no active rigidbodies in the scene; a thrown book into a pile moves only
  items near the landing spot and they all freeze again; a throw that misses every pile wakes nothing; the bubble never exceeds its cap.

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
~~No generative AI for art or text.~~ **Updated 2026-10-18 (Kolby):** AI is fine for placeholders; Kolby will add the real art later. Keep a provenance log; Steam requires disclosure of shipped AI assets (T-004). Choose one base 3D style (possibly CC0 packs plus a few hero props), the bookplate or engraving
style for commissioned patron portraits, and check the rights of any public-domain images per item.

---

## Tooling, scale and research follow-ups (Kolby, 2026-10-18)

Order Kolby set: **tooling first, then the player capsule and lighting.** The research behind all of this is in [unity-practices/](unity-practices/README.md) (27 files, each with sources and
"Implication for BookBoy"). Per the CLAUDE.md research rule, read the matching file before changing anything in the scene. "Approval" below means CLAUDE.md fences the change
(`ProjectSettings/`, render-pipeline settings, layers, input): present the plan and get an explicit yes first.

### Recommended order of work

1. **T-001** no-focus Editor workflow (stops the Editor stealing the window), then record it in CLAUDE.md and the unity-editor-ops skill.
2. **T-002** adult-sized player and cozy rooms (undo the storybook rescale; FOV per the decision below).
3. **T-003** lighting pass on the new scale (measured before and after; the settings items need approval).
4. **T-010** palette: per-hue Oklab lightness solve, rendered and measured (L-021).
5. **T-004 + T-012** procedural book mesh and cover materials, which also removes the per-book `MaterialPropertyBlock`.

Next after those: T-014 (pile throws), T-013 (one pooled highlighter), T-011 (UI), T-006 (Player-build check), T-015 (statics), T-008 (settings menu), T-009 (Steam lead times).

### Decisions made 2026-10-18

- **AI art:** fine for placeholders; Kolby will add the real art later. D-005 is updated. Keep a provenance log (tool, prompt, date) for every AI asset, because Steam requires disclosure of shipped AI assets (file 25, 26).
- **FOV (Claude to settle, derived from Kolby's Witch Cabin screenshot and research files 2, 23 and 27):** the player-facing FOV is **horizontal**, default **90 degrees at 16:9 (58.7 vertical in Unity)**, slider **70 to 110 horizontal (43.0 to 77.6 vertical)** with a reset button; convert with `Camera.HorizontalToVerticalFieldOfView(h, 16f/9f)`.
  Why: Witch Cabin, which Kolby likes, has its FOV slider at 90 (`Docs/competitive/img/witch-cabin/03-settings-image.png`); the knob sits about 43% along the track, which fits a horizontal range of about 60 to 130 and
  cannot be a vertical value (90 vertical would be about 121 horizontal). [derived, medium confidence: the game does not state the axis]. Narrower is more intimate (Level Design Book; Firewatch 55). At 58.7 vertical a 0.32 m book fills 23.7% of the screen at 1.2 m and
  19% at 1.5 m, inside the 16 to 23% band measured from the reference games; file 27's 65 vertical (97 horizontal) gives 20.9% and 16.7%, also inside. **Verify by measuring the rebuilt room;** fall back to about 97 horizontal if 90 feels tight.
- **Save design:** not now. Kolby will do it as a separate session later (T-007 is deferred; research file 22 is kept).
- **Sound, controller (F-001) and achievements (F-002):** later. **Aspect ratio:** 16:9 only. **Testing:** the Editor is fine for most checks.

### T-001 Test the no-focus Editor workflow  **P1, tested 2026-10-18: autotick does not work here; workaround adopted (see result)**
- **Result (2026-10-18, Kolby watching):** `set_autotick` (16 ms and 1 ms) did not keep Play frames advancing while the Editor was unfocused, minimised or covered; frames froze and `status` still said playing. `editor_play`, `editor_stop` and `set_autotick` do not steal focus; toggling `isPaused` does. `EditorApplication.Step()` advanced one frame while paused. `QueuePlayerLoopUpdate` and `SignalTick` did nothing. Recorded in CLAUDE.md rule 3a.
  **Workaround:** Edit Mode builds, synchronous suites and `LamplitBuilder.Shot` renders all work unfocused; frame-dependent checks (pile drop) wait for a moment Kolby allows the Editor in front. Still to try: `capture_game_view --source camera` while frozen, a Player build (T-006) or a separate project copy (file 19) for frame-dependent checks.
- Goal: stop the Editor window being forced to the front while Kolby works on the same PC.
- Try (research files 20 and 21): `unity command set_autotick` once per session before Play; prove frames advance with `wait_for Time.frameCount changed` or `status` frame count and `playerLoopTicking`;
  `capture_game_view --source camera` for off-screen captures; never call `editor_focus`. Compare one off-screen capture with a focused Game-view capture (do the Volume effects, tonemapping and vignette appear?).
  Check `EditorApplication.Step()` from an eval. Check empirically whether gamepad input reaches the Editor unfocused (CLAUDE.md rule 2 says no; the Input System docs say yes).
- Also record in CLAUDE.md and the skill: eval has a 5 s main-thread timeout and the docs never say a timed-out eval is aborted, so long work goes in `eval_file` with bounded loops and polling.
- Needs Unity open, so do it when Kolby says the Editor is open and a few minutes of Play is fine. Run the Lamplit suites in the background as the proof.
- Fallback if it does not work: a separate clone of the project with its own Library (file 19), or a Player build (T-006).
- Done when: the workflow is in CLAUDE.md and the unity-editor-ops skill, rule 2 is corrected, and a full suite run happens with Unity in the background.

### T-002 Back to an adult-sized player and cozy rooms  **P1, In progress: hub and Room 1 done 2026-10-18, Rooms 2 and 3 to do; Kolby to playtest**
- Kolby's call: the storybook rescale (world 1.6x, eye 1.15 m, FOV 75, held book 0.55) was a wrong turn. The rooms feel like a giants' hall. Want an **adult-sized actor** and rooms that feel cozy.
- Research: the ceiling is 2.6x the eye height (engine defaults about 1.9 to 2.0, a real house 1.45); no source supports exaggerating more than about 1.33x; readability comes from the **screen fraction** of a book
  (16 to 23% of screen height, spines 5.3 to 5.7% thick), not from world size (files 1, 3, 27). The player collider is still the adult prefab one (2.0 m, radius 0.5; confirmed from the prefab, no scene override), so it must be set explicitly.
- Starting numbers (derived in file 27, not sourced; settle by measuring): eye 1.65 m; capsule 1.8 m, radius 0.35; FOV per the decision above; ceiling 2.6 m; door 1.1 x 2.2 m; hub 3.0 m wide; book 0.32 m tall, 0.06 to 0.085 m thick; slot pitch 0.09 m;
  boards at 0.95 m and 1.40 m (0.45 m apart); reach 2.8 m; magelights at 2.0 m; world scale 1.0. Keep: thick spines, 1.33x item height, the wide door, the long reach, the held-book shrink.
- Steps: (1) plan and approve the numbers; (2) set them through the builder constants (`WorldScale, EyeHeight, MoveSpeed/SprintSpeed, InteractionRange, CarryScale, WallHeight, DoorWidth/DoorHeight, HubHalf, SlotPitch, BaseHeight, ShelfSpacing, ContainerHeight, OrbHeight, SignHeight, item size arrays`);
  (3) capsule height, radius and centre on the player (scene override, not the Starter Assets prefab); (4) rebuild the hub and rooms, re-run all suites; (5) render and measure the book's screen fraction at 1.2 and 1.5 m and the spine thickness fraction;
  (6) check held-book clipping against shelves (pull the held book back near surfaces first if it clips); (7) update `09-scale-reference.md` and `04-build-status.md`.

### T-017 Two bookcase types and sets of 4 or 8  **P1, In progress: rule and Room 1 done 2026-10-18; decor or blocked rows and Rooms 2 and 3 to do**
- Kolby: the prototype's narrow 5-slot cabinets are part of the problem. New designs: **(A) solo: 8 books wide**; **(B) triple: an 8-wide centre with a 4-wide wing on each side**. Book sets (series) are **4 or 8** books; no more sets of 5.
- **Update 2026-10-19 (Kolby): tomes.** Books are 12 to 15 in tall, 10 in deep, 4 in thick; cases resized for 15 in books (pitch 4.25 in, shelf spacing 17 in, plinth 15 in, depth 13 in, 7'0" tall; solo 37.6 in, wing 20.6 in, triple 6'6.7" wide). Values in [09-scale-reference.md](09-scale-reference.md); the older numbers below are superseded.
- Proposed dimensions (adult scale, T-002): slot pitch 2.75 in (0.07 m); frame about 3.5 in (0.09 m) in total; **solo about 25.5 in wide (0.65 m); wing about 14.5 in (0.37 m); triple about 54.5 in (1.38 m)**; depth 12 in (0.30 m). **Four shelves per case (Kolby, 2026-10-18; the references have 3 to 4 boards: Witch Cabin four, Shelves and Sorcery three above a base, Librarian several; the earlier two came from the prototype, not from references):** 8 in riser, shelf tops at 11 in, 2'0", 3'1" and 4'2" (0.28 / 0.61 / 0.94 / 1.27 m, 13 in pitch), books 9 to 12 in tall, case about 5'5" (1.65 m, top at eye level). Solo = 32 slots, triple = 64 slots. Supersedes the two boards at 0.95 / 1.40 m in T-002. Wings flush with the centre (an optional slight inward angle later).
- Build impact is small: the builder's container function already takes the slot count, so a solo is one 8-slot container and a triple is three containers (4, 8, 4) placed side by side; each row is still a Shelf.
- Rules to confirm (recommended default in brackets): a row has a capacity (8 or 4) [a set grades green only on a row of its own size; a 4-set on an 8-row, or the reverse, grades yellow with a "this shelf is for sets of 8" hint]; a row is finished when it holds a full set of its size; one set per row as today.
- **Item counts (Kolby, 2026-10-18):** keep about 100 items per room and use FEWER cases (Witch Cabin's bedroom has just two); spare rows are filled with decor (static, non-interactive props) or blocked off (a row that accepts nothing). Room 1 example: a triple plus a solo = 96 items (8 sets of 8 on the eight 8-rows, 8 sets of 4 on the eight wing rows) on about 6.6 ft of wall. Rows needing decor or a block flag are a builder option (Shelf blocked flag, decor props).
- **Done 2026-10-18 (step 1, engine-free):** `PlacementRules.SetFitsShelf`, an `EvaluateCampaign` overload with `setFitsShelf` (yellow, never overrides red), `BookDefinition.setSize`, `Book.GradeCampaign` wired to the shelf's slot count; 3 new Edit Mode tests (112 pass). Not yet used by the builder (setSize defaults to 0 = any shelf, so nothing changes in the current rooms).
- Touches: `CampaignCatalog` (set sizes and counts), `PlacementRules.EvaluateCampaign` (capacity check, engine-free, Edit Mode tests first), builder (containers, groups, item counts), signs, finished-shelf tracking, verify scripts and `CampaignRulesTests`.

### T-003 Lighting pass after T-002  **P2, Todo, Needs approval for the settings items**
- Scene and builder (no approval): lamps lower or ceiling hotspot reduced (the ceiling gets about 32x the floor's light with lamps 0.45 m below it, file 5), lamp range and intensity retuned for the new scale, remove the leftover 0.5 directional light if it only adds an invisible term, check ambient (Trilight is very dark).
- **Approval:** shadow settings sized for the rooms (soft shadows Medium instead of High; fewer than 4 cascades and a shorter max distance than 50 m; an atlas big enough for the shadowed lights, since 3 shadowed point lights at tier 512 need 18 shadow maps and a 2048 atlas holds 16);
  Bloom Clamp about 3 to 6 (default 65472); motion blur off or low; keep Neutral tonemapping and Post Exposure for brightness (files 6 to 8).
- Measure before and after on rendered frames (luminance of books and wood, hotspot size). Do not judge by eye alone.

### T-004 Book mesh and cover materials  **P2, first version done 2026-10-19 (Blender mesh and procedural sample material); spine icons and AI/CC0 cover art still to do**
- Per file 26: generate the book mesh in C# with an editor generator (about 60 to 300 triangles, authored at real size with a **bottom pivot**, which means changing `ShelfSlot.Accept`, which assumes a centre pivot); CC0 or procedural cover patterns first (ambientCG, Poly Haven);
  AI only for neutral tileable base patterns or placeholders. Spine letters and numbers stay drawn by the game. Avoid AI 3D meshes for books. 512 px covers (about 0.33 MB with BC7 and mips) are enough.
- First experiment: one procedural book, 3 cover variants, checked by rendered screenshot and the Stats window (acceptance criteria are at the end of file 26). Log tool, prompt and date per AI asset. Related: L-015. Do after T-002 (book sizes) and together with T-012.

### T-005 A reviewer for changes  **P2, Todo, discuss first**
- A separate agent that reviews a change before it is committed. Proposed design is in the chat of 2026-10-18: a fresh agent with read-only tools that gets only the diff and the matching research notes and returns block/advise findings
  (contradicts a sourced note, unsourced number, breaks a guardrail, tests do not exercise the change, diff bigger than the goal). Not built yet. See D-006.

### T-006 Player-build performance check  **P2, Todo**
- Use a real Windows Player (Development Build, Mono only; IL2CPP is not installed) to confirm performance facts; the Editor is fine for day-to-day work. Research file 24: pass the scene list and options per build call (`EditorBuildSettings` lists only LibraryPrototype, not Lamplit),
  set runInBackground and vSync in code, no ProjectSettings change. Build a small self-driving harness active only with `-perf` that walks the rooms, writes a CSV, a summary JSON and screenshots, and quits with an exit code; a script diffs the summary against a committed baseline.
- **First, take a baseline now** (Editor Stats and Frame Debugger: SetPass, batches, shadow casters) before T-003 and T-012 change anything. A Player is also a possible way to verify without the Editor's focus problem.
- Unknowns to test: whether a launched Player steals focus, whether `-batchmode` without `-nographics` renders, build time, whether the first build touches tracked ProjectSettings files.

### T-007 Save and load  **Deferred: Kolby will do the design in a separate session later**
- Research is kept in file 22 (location, JsonUtility DTOs, schema plus migrations, atomic write with .bak, room-local pile poses, derived doors, Steam Auto-Cloud). Do not start. Items to remember when it starts: persist which shelves already awarded points; set final Company and Product names (ProjectSettings, approval);
  verify `FindObjectsByType` finds objects under inactive rooms; design questions (does New Game keep per-zone best times; do resumed runs count for leaderboards).

### T-008 Settings menu: FOV, motion blur, windowed default  **P2, Todo, Needs approval (input and project settings)**
- File 23 and the FOV decision above: horizontal FOV slider 70 to 110, default 90, reset button; motion blur toggle (off by default; blur is velocity based and covers the whole frame while turning, and accessibility guidance wants a toggle); no head bob; resolution list (1080p, 1440p, 4K),
  fullscreen mode (windowed, borderless, fullscreen); the committed windowed default is 1024x768 (4:3), change it to 16:9. Do not force 16:9 in code. Feeds F-004.

### T-009 Steam release path  **P3, Todo; some steps are Kolby's**
- File 25. Lead times drive the order: Steamworks partner signup and tax/bank paperwork take 15 to 30 days, then a 30-day wait after the $100 app fee (Valve's pages say 21 on one, 30 on another; plan 30); the Coming Soon page must be live at least 14 days before release; store page and build review take 3 to 5 business days each;
  Next Fest editions are Feb 22 to Mar 1 and Jun 14 to 21, 2027 (a demo needs its own App ID; June looks realistic). Decide the AI policy and keep the provenance log. Steamworks.NET behind a plain-C# interface when it is time (domain reload is off). Genre pricing is $4.99 to $5.99.
  Re-read Valve's live AI-disclosure page before shipping.

### T-010 Palette: per-hue lightness solve  **P2, Todo, no approval needed (this is L-021)**
- File 18: HSV value is not luminance; 10 of 20 groups are below a 2.0 ratio against the wood (#6B401F) even in flat albedo (purple #613299 is 1.02, navy #325099 is 1.15). Replace `GroupColor` with a per-hue lightness solve in Oklab (floor L about 0.67 for a 2.5 ratio on this wood),
  check red/green pairs with a colour-blind simulation, keep the letters (deuteranopia collapses some pairs to a tiny distance). Re-measure on **rendered** frames (tonemapping changes lit ratios and is undocumented). WCAG's 3:1 non-text bar is higher than our 2.0 and 2.5 targets, and only a proxy in dark scenes.

### T-011 UI: world-space text and scaling  **P3, Todo, no approval needed**
- File 17: world-space signs from legacy Text go soft within about 1 m; use TextMeshPro (or Dynamic Pixels Per Unit). CanvasScaler Match is 0 (width), which drifts on odd aspect ratios; use 0.5. The legend font 20 is at the minimum (18 px at 1080p); raise it. Fewer Canvas objects.

### T-012 Book rendering: remove the per-book MaterialPropertyBlock  **P2, Todo, no approval needed**
- Found by file 26 and confirmed in the repo: all books share one material, `Mat_BookSpine`, and `Book.cs` sets each colour through a `MaterialPropertyBlock`. Unity says an MPB breaks the SRP Batcher in URP, so books are probably outside it (unconfirmed in the Frame Debugger).
  Bake the colour into vertex colours or use shared per-colour materials, and check the "Enable GPU Instancing" checkbox state. Measure with the T-006 baseline (batches, SetPass) before and after. About 340 items is probably not a real draw-call problem, so this is hygiene and a prerequisite for a pooled highlighter, not an emergency.

### T-013 One pooled highlighter  **P3, Todo; the outline-renderer-feature variant needs approval (renderer/layers)**
- File 16: per-book hulls, a light and a particle system are built at runtime and never pooled; `MaterialPropertyBlock` on the x-ray hull also breaks the SRP Batcher. Replace with one shared highlighter that follows the aimed or pulsing book. Picking itself is already right (nearest ray hit equals the visible surface); call `Physics.SyncTransforms()` after piles change.

### T-014 Pile throws and per-body physics  **P2, Todo, no approval if per body (project-wide changes need approval)**
- File 15: idle books are static triggers, so a thrown solid book falls **through** pile books today; pulling a book out wakes nothing. For L-020 choose between making neighbours solid in a bubble with a kinematic outer ring to rest on, or a runtime proxy PhysicsScene like the bake (both are reasoning from sources; prototype and measure).
- File 14: `ContinuousDynamic` on every tossed book and bake proxy is Unity's last resort and the most expensive; use Continuous Speculative first. Set solver iterations, max depenetration and sleep per body, not project-wide (starting overlaps may "explode"; PhysX default starting depenetration is 3.0, the project's is 10).
  Check that the player cannot kick piles (the manual disagrees with itself on whether CharacterController pushes Rigidbodies; test).

### T-015 Statics hygiene and Enter Play Mode  **P3, Todo; changing the setting needs approval**
- File 13: Unity 6.6 defaults domain reload off and recommends it; the project also skips scene reload (the non-default half). Audit statics now and add the 6.6 reset attributes (`[AutoStaticsCleanup]`, `[OnEnteringPlayMode]`, `[OnExitingPlayMode]`) or the older RuntimeInitializeOnLoadMethod(SubsystemRegistration) reset. Reconsider the scene-reload-off setting if room state leaks, and always when measuring load times.
  Update CLAUDE.md rule 9 with the concrete attributes.

### T-016 Item state as plain C# with string IDs  **P3, Todo, with the save design**
- File 11: ScriptableObjects as read-only definitions (in a Player they are read-only; runtime writes leak between Play sessions with domain reload off); a plain-C# ItemState with string IDs for state and room unload; plain Instantiate until profiling says to pool. Supports L-016 to L-019; do it when the save design starts.

### Approval queue (nothing here is applied; ask before doing any)

- T-003: shadow settings (soft shadows, cascades, max distance, atlas), Bloom Clamp, motion blur.
- T-008: windowed default 16:9, any input or render setting behind the settings menu.
- T-013: an outline renderer feature (renderer data / layers).
- T-014: any project-wide physics setting (Time.fixedDeltaTime, solver, contact offset).
- T-015: the scene-reload setting.
- T-007 (later): final Company and Product names.

### D-006 Overnight and interactive working modes  **Decision to settle**
- Kolby: approvals should work differently when he is interactive and when he leaves the assistant to work overnight. Not yet defined. Proposed split (in the chat of 2026-10-18): interactive = ask before fenced settings and before pushing; overnight = never touch fenced settings or push without a standing yes,
  queue those as written proposals with a measured before and after, keep working in the safe zone (builder, scripts, tests, docs), commit locally with the reviewer step on, stop and write up after two failed test runs. Open: may overnight work push to GitHub?

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
4. ~~L-017: unload a room when the player leaves it?~~ **Answered 2026-10-18:** keep it loaded once opened, but store its state as data so unloading is an easy switch later.
5. ~~L-019 / L-020: is it fine that a thrown book does not move a pile?~~ **Answered 2026-10-18:** no, physics should work on piles; wake the items in a sphere around the predicted landing spot, let them settle, freeze them again (L-020).
6. ~~L-019: picking from a pile, and what happens to the rest?~~ **Answered 2026-10-18:** you can pick up any item you can see (aim at), not just the top ones; when one is pulled out, the neighbours settle.
7. ~~L-016: item definition asset files?~~ **Answered 2026-10-18:** items live only in data files; rooms stay static scene content.
8. ~~L-020: bubble starting values (about 0.6 m, about 40 bodies)?~~ **Answered 2026-10-18:** good enough. The goal is only that it feels a bit natural: no chained or cascading physics, no always-on physics; most games in the genre move nothing but the active item, which feels odd, so a small local reaction is the target.
9. D-006: what should the assistant do differently overnight (unattended) compared with interactive work? A proposed split is in the chat of 2026-10-18.
10. ~~D-005 vs T-004: generative AI for art?~~ **Answered 2026-10-18:** fine for placeholders; real art comes later.
11. ~~FOV: 90 horizontal or 65 vertical?~~ **Settled by Claude 2026-10-18 from Kolby's data:** horizontal 90 default (58.7 vertical), slider 70 to 110; verify by measuring (see the decision in the tooling section).
12. ~~Save design now?~~ **Answered 2026-10-18:** no, a separate session later (T-007 deferred).
