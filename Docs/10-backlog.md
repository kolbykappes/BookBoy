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
**Status:** Todo · **Priority:** P2 (needed by F-002 to be worth anything) · **Needs approval** (save/load is on the non-goals list) · **Added:** 2026-10-17

**Goal:** remember unlocked achievements, best finish time and settings between sessions.

**Details:** a small JSON file in `Application.persistentDataPath`; versioned; never crash on a missing or corrupt file; no mid-run
save (a run is short). Engine-free serialization model in `BookBoy.Rules` with round-trip tests.

---

## Ideas (not requested yet - promote or delete)

### F-004 Settings menu
**Status:** Todo · **Priority:** P2 · **Added:** 2026-10-17
Mouse sensitivity, field of view, master volume, controller look speed / dead zone / invert Y. Needs F-003 to remember them.

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

---

## Chores and housekeeping

- **H-001** Push the branch and refresh PR #1's description (it still describes an early state; many commits are local only).
- **H-002** Decide what to do with the untracked competitive-research files in `Docs/competitive/` (not part of the game work).
- **H-003** Playtest pass by Kolby on the new lighting, floor, hand light and menu pointer; list anything that feels off here.
- **H-004** Look at the Editor-generated `BookBoy.slnx` change and decide whether to commit it.

---

## Decisions waiting on Kolby

1. F-001: OK to read `Gamepad.current` directly like the keyboard and mouse (no binding changes)? Which buttons for the four abilities?
2. F-002 / F-003: OK to lift the "no save/load" non-goal for a tiny settings/achievements file only?
3. Which of the starter achievements do you actually want, and should there be one per bookcase theme?
