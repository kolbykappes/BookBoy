# Build Status

> Keep this file current — update it as milestones from
> [05-implementation-plan.md](05-implementation-plan.md) land, so it always reflects what's
> actually in the repo, not what's planned.

**Last updated:** 2026-10-17 (shelf system reworked: 16 numbered books, free placement with color feedback).

## Progression, abilities, and the 128-book room (2026-10-17, overnight batch)

Ten features requested by Kolby, built in small slices, each committed with tests and a Play Mode check:

1. **Carry several books** (`CarryInventory`, `CarryList` rules): hands start at 4, shown as a small fan at the carry point; a
   loose or shelved book can be picked up while there is room ("Hands full (n/n)" otherwise).
2. **Cycle** the current book with R or the mouse wheel (E places the current one).
3. **Capacity is an upgrade**: starts at 4; Strong Arms I-VI take it to 10.
4. **Points for finished shelves** (`GameProgress`, `FinishedTracker`): a shelf with all 4 books of one series in order
   pays 1 upgrade point, once per shelf (re-finishing does not pay again).
5. **Toss** with Q (`Book.Toss`): real physics (continuous detection, low bounce, high friction); it settles after being still
   for 0.5 s, then returns to a normal trigger book. Eight tosses in eight directions all landed on the floor (one came to rest
   on top of a bookcase) and all settled.
6. **Quick Shelve** (F, `QuickShelve`, `ShelfPlanner` rules): plans every held book against the live claims (the bookcase and
   shelf that already hold its color/series, else the first unclaimed; volume N in slot N), reserves the slots, and flies each
   book along an arc with a spin and a glow trail, snapping it in with the normal green/yellow/red feedback.
7. **Sister Glow** (G, `SisterGlow`): pulses the outline on every unshelved book of the held book's series (not the held
   ones, not shelved ones); a 6 s cooldown (3 s with Quick Recharge). Needs the upgrade.
8. **HUD** (`GameUI`): run time, books correct / total, shelves finished, upgrade points, what you carry, toasts, and a
   completion message with the final time (your score is the time to finish).
9. **Outline glow instead of whole-book glow** (`BookOutline`, `BookOutline.shader`): a thin outline plus a faint halo (back-face
   shells of the book mesh) and sparkles for focus, sister glow and placement feedback; the book's body color is never
   changed. Shells are flat in depth, otherwise they read as a slab behind the book.
10. **Tech tree and menus**: `UpgradeCatalog` (13 upgrades, 24 points: Strong Arms I-VI, Fleet Feet I-III, Sister Glow, Lingering
    Glow, Quick Recharge, Quick Shelve), the upgrade menu on Tab, an admin menu on F10 (+1/+5/+10 points, unlock all, reset), and
    a controls legend that greys out locked abilities. Menus pause the game, free the cursor, and are driven by direct
    mouse/keyboard handling (no EventSystem). Keys are read directly: no input-binding changes.

Then the room grew to **16 bookcases / 32 shelves / 128 books in 16 colors** (so up to 32 points): `BookCategory` has 16 values
(append-only), the generator builds all 16 colors (light colors get a darker second series), the original 32 definitions were
updated in place (GUIDs unchanged), and the scatter keeps the spawn point clear.

**Verified:** 58 Edit Mode tests; Play Mode suites in `tools/`: `verify_shelves_playmode.cs` (26 checks), `verify_carry_playmode.cs` +
`verify_toss_settle.cs` (23), `verify_progress_playmode.cs` + `verify_progress_part2.cs` (33: points, upgrades, abilities, menus, the
Quick Shelve flights landing every book Correct, the timer, and completing all 128 books). Screenshots of the HUD, both menus,
the glow and the 16-bookcase wall were reviewed.

**Not verified by me / open:** how the glow, flights and tosses *feel* in motion with a real mouse and keyboard (input cannot be
injected here); no sound effects exist yet; the legend is always visible (no hide toggle); a book tossed onto a bookcase top
stays there (still pickable with the crosshair); the 16 colors are distinguishable in screenshots but the closest pairs
(amber/lemon, rust/brown) are worth a look in play; no save/load (still a non-goal), so upgrades and points reset each run.

## Bookcase rework (2026-10-17)

Design from Kolby, modeled on the layered color -> title -> volume sort in Librarian (see
[competitive/01-librarian-tidy-up-the-arcane-library.md](competitive/01-librarian-tidy-up-the-arcane-library.md)).
Supersedes the earlier "colored shelf sections" attempt and the "only the correct slot accepts a book"
rule in [02-mvp-spec.md](02-mvp-spec.md) (updated).

- **4 bookcases x 2 shelves x 4 slots.** One bookcase per color, one series per shelf, volume N in slot N
  (left to right). Nothing is labeled or color-coded: the **first book shelved claims** the bookcase's
  color and the shelf's series; if two claim the same thing the earlier claim wins. Slots are only
  slightly wider than a book (0.072 m pitch) and invisible, apart from a thin bar on the board that
  lights up under the crosshair while holding a book.
- **32 books: 4 colors x 2 series x 4 volumes.** The second series of a color is a lighter shade and a
  different size. Volume numbers are small pixel digits near the bottom of the spine (`PixelDigit`,
  depth-tested; legacy `TextMesh` was rejected because its shader renders through geometry). Titles:
  "Amber Tome Vol. 3", "Amber Codex Vol. 1", etc.
- **Feedback flash on the book:** red = wrong bookcase (wrong color), yellow = right bookcase but wrong
  shelf (series) or wrong slot, green = all right. Wrong placements are never refused or penalized;
  shelved books can be picked back up. Completion ("All 32 books restored!") needs every book green;
  grades are recomputed from live state, so removing the first book of a shelf re-decides its claim.
- **Crosshair:** a dot at screen center. Small white = nothing; large gold = something in reach (a book to
  pick up, or a free slot to place in); large red = in reach but blocked (slot taken); faint = a target
  beyond reach (3 m; seen out to 12 m). A targeted book is brightened.
- **Code:** `Bookcase` (claims a color), `Shelf` (claims a series, aim volume, ray-plane slot picking),
  `ShelfSlot`, `Book` (`Grade()`, `placedTime` from wall-clock time so placements never tie),
  `PlayerInteractor`, `Crosshair`, `PixelDigit`. Engine-free rules in `BookBoy.Rules.PlacementRules`
  (`Evaluate`, `ClaimHolds`, `NearestSlot`, `IsCleanupComplete`) with 20 Edit Mode tests.
- **Player capsule no longer casts a shadow.**
- **Generated scene content:** `tools/build_library.src.cs` (then `node tools/qualify.js`, then
  `unity command eval_file tools/build_library.cs`, then save). Re-runnable; mutates the existing 16
  `BookDefinition` assets in place (GUIDs unchanged) and adds 16. The eval call can report a 5 s
  main-thread timeout while the script still completes - check the scene state instead of re-running blind.
- **Verified in Play Mode** (`tools/verify_shelves_playmode.cs`, 26 checks): every grade and claim rule,
  two different valid solutions (32/32 each), one swap leaving exactly 30, all 32 slots picked correctly
  from three viewing angles, highlight/prompt/crosshair states, pickup of shelved books, brightening of the
  focused book. The Editor was restored byte-for-byte afterwards.
- **Not verified by me:** how the flashes look in motion and the E-key/mouse feel (an unfocused Editor does
  not advance the game loop and input cannot be injected). Bookcases are narrow towers (0.44 m wide)
  because a shelf holds exactly one packed 4-book series.

## Books seat flush at the front of the shelf (2026-10-17)

Placed books were centered in the shelf depth (~9 cm back from the front edge) and hard to read. Slots now mark a front
line (`local z = -0.14`) and `ShelfSlot.Accept` places each book's spine face on it (`+ forward * depth/2`), so books of
different depth line up flush near the front. The aim plane in `Shelf.NearestSlot` moved to mid-book depth to match, and the
highlight bar sits under where the book stands. Re-verified: 26/26 Play Mode shelf checks; render from the real eye
height shows the spine numbers clearly.

## Mouse "dragging" fix: uncapped frame rate (2026-10-17)

Reported: sweeping the mouse across the shelves (worst while holding a book) felt laggy and draggy.

- **Cause: the game ran uncapped.** VSync is off and `Application.targetFrameRate` was -1, so the game rendered
  190-260 fps flat out in the Editor, saturating the GPU/CPU and queuing input behind rendering. Evidence from an
  in-game frame logger during a real hand sweep: slow frames took 23-80 ms of wall-clock while the main thread did
  only ~5-12 ms of work (so the time was spent waiting, not in scripts), they clustered while the mouse was moving
  (p99 11-19 ms vs 5.5-8 ms when still), and the per-frame aim detection never exceeded 0.34 ms. Capping the rate at
  runtime to 60 fps made it feel "much better" immediately.
- **Fix:** `FrameRateCap` (`Assets/_Project/Scripts/Core/`) sets `Application.targetFrameRate` to the display's
  refresh rate (clamped 30-240) at startup when VSync is off and no target is set. Code only; `ProjectSettings` is
  untouched (turning on VSync in Quality settings, or the Game view's VSync toggle, would also work).
- **Ruled out by measurement:** the interaction/aim code (5-8 us per frame even with books in the ray's path, no GC),
  every `Update`, the yaw wrap at 0/360 deg, camera-vs-player latency (exactly 0 frames with the shake off), and
  rendering cost (~90 draw calls looking at the bookcases).
- **Also found and changed - camera shake disabled.** The follow camera's `CinemachineBasicMultiChannelPerlin`
  (`Handheld_normal_mild`, gain 0.5) made the camera wander on its own (~+/-0.1 deg yaw, ~0.45 deg pitch with no
  input), which hurts aiming at 7 cm slots. It is off via a prefab override on the scene's follow camera (component
  kept; re-enable or lower its amplitude for a gentler shake). It was not the cause of the drag.
- **Fixed - player eye height was 0.79 m, not 1.7 m.** The Player starts at y = 1.0 but gravity drops it to rest at
  y = 0.09 (CharacterController center 0.93, height 2), so the camera (local y 0.7) sat at 0.79 m; the 1.7 m in
  `09-scale-reference.md` was computed from the starting transform. `PlayerCameraRoot` local Y is now 1.61 (a one-line
  prefab override on the scene's Player), and the camera settles at exactly 1.700 m. Re-verified: 26/26 shelf checks;
  from 1.7 m you look down into both compartments of each bookcase.

## Dev workflow (2026-10-17)

- Adopted a Unity operating-rules set from a friend's six-month lessons-learned notes: ~24 hot
  rules merged into the root `CLAUDE.md`, detailed traps in `.claude/memory/unity-*.md`. They
  **supersede older workflow docs** (`07-unity-mcp-tools.md` is marked superseded; `06`/`03`
  updated to the Unity CLI).
- `com.unity.pipeline` `0.8.0-exp.1` installed (approved) and verified: `unity command eval`
  returns `6000.6.6f1` from the live Editor.
- Warm-tier skill built: `.claude/skills/unity-editor-ops/` (`SKILL.md` + `unity-ops.sh`),
  validated live. First test run: **0 tests** (no test assemblies exist yet) - correctly reported
  as "no tests ran", not a pass. No scene/asset changes resulted.
- **First Edit Mode tests landed (15, all passing):** `Assets/_Project/Scripts/Rules/`
  (`BookBoy.Rules.asmdef`, engine-free `PlacementRules`: exact-`bookId` slot matching and
  cleanup-complete check) and `Assets/_Project/Tests/EditMode/` (`BookBoy.Tests.EditMode.asmdef`,
  `PlacementRulesTests`). `ShelfSlot.Accepts` and `CleanupTracker` now call `PlacementRules`, so the
  tests cover the code the game runs. One deliberate behavior tightening: a slot or book with a
  null/empty id never matches (previously two empty ids would). Verified in the live scene: 8 books,
  8 slots, each book fits exactly one slot, no empty slot ids. Run with
  `bash .claude/skills/unity-editor-ops/unity-ops.sh test editor`. The helper's failure path was
  verified by breaking an assertion on purpose.
- **Open items:** not yet play-tested after the refactor (logic is covered by tests and the
  scene check, but pickup/place in Play Mode hasn't been run since); no PlayMode tests yet.
- Project upgraded from Unity `6000.6.0f1` to `6000.6.6f1` on 2026-10-17 (`ProjectVersion.txt`).

## Completed

- **Expanded to 8 books, 2 per category, each with its own shelf target** (12 is the full MVP
  count — 8 is a deliberate midpoint, not the final scope). Each of the 4 shelf boards now holds
  2 slots side by side, sized to that category's book height, with a visible marker matching the
  book's spine color. Same-category books/slots intentionally share the exact same color (no
  title/volume distinction yet — that's a post-MVP idea from the Librarian competitive-analysis
  notes), which incidentally exercises the exact-`bookId` matching logic: two visually identical
  slots, only one of which accepts a given book.
- **Added `CleanupTracker`**: watches every `ShelfSlot` in the scene and shows a "cleanup
  complete" message once all are occupied. Deliberately not the full Phase 6
  `Books Restored: X/12` HUD yet — just enough to confirm the system knows when the player is
  actually done.
- **Tuned `CameraLookBridge`** per feedback: sensitivity `1x → 3x` (reported sluggish), and
  inverted-Y is now the default (Kolby's preference; the previously-tested build was
  non-inverted).
- **Bug found and fixed: a `MaterialPropertyBlock` set once via script does not survive an
  Editor domain reload.** Caught via direct data verification, not just eyeballing a screenshot —
  `ShelfSlot_Alchemy`'s marker had gone fully transparent after an unrelated script recompiled.
  `ShelfSlot` now stores `markerColor` as a plain serialized field and re-applies it from
  `OnEnable`/`OnValidate`, the same self-healing pattern `Book.cs` already used. Worth remembering
  for any future per-instance-tinted object: property-block-only color state is fragile in the
  Editor (works fine at actual runtime; the risk is specifically Editor recompiles).
- **Fixed: mouse-look was completely non-functional in Play mode.** Confirmed via live
  diagnostics: `StarterAssetsInputs.look` correctly received mouse input, and
  `FirstPersonController` correctly rotated `PlayerCameraRoot` from it — but the actual rendered
  camera never rotated, because `PlayerFollowCamera`'s `CinemachinePanTilt` (the component
  actually driving what `MainCamera` shows via `CinemachineBrain`) had no input source wired up
  at all; `PanAxis`/`TiltAxis` sat frozen at 0 regardless of mouse movement. This was a
  pre-existing gap left over from adapting Starter Assets' third-person Cinemachine camera to
  first-person — **not** something introduced this session, and it means the "tested mouse look
  successfully" claim below was never actually validated end-to-end from the render camera.
  Fixed with `CameraLookBridge` (`Assets/_Project/Scripts/Core/`), which feeds
  `StarterAssetsInputs.look` into `CinemachinePanTilt` each frame.
- **Phases 4 & 5 — pickup and placement, scoped to exactly one book** (Alchemy only, per the
  "smallest complete loop first" principle): `PlayerInteractor`
  (`Assets/_Project/Scripts/Interaction/`) raycasts from the player camera, shows an on-screen
  `E Pick Up`/`E Place` prompt (minimal `HUD_Canvas` + legacy `Text`, no `EventSystem` needed for
  a passive label), carries one book at a time (collider disabled while held, parented to a
  `BookCarryPoint` under `MainCamera`), and places it into a matching `ShelfSlot`
  (`Assets/_Project/Scripts/Books/ShelfSlot.cs` — `acceptedBookId`, `snapTransform`, `occupied`,
  optional category marker, matching the spec exactly). One `ShelfSlot_Alchemy` exists on
  `ShelfBoard_1`, with a visible amber marker that hides once occupied (the MVP spec requires a
  **visible** slot — the first version of this was an invisible trigger collider with zero visual
  representation, which is a real gap, not a design choice; corrected). A mismatched slot shows
  "This belongs elsewhere" with no penalty. Verified end-to-end live in Play mode. The other 3
  scattered books have no destination yet — deliberate, to keep this one testable slice.
- **Unity MCP connected and in use** — see [07-unity-mcp-tools.md](07-unity-mcp-tools.md) for the
  full setup, the 13 currently-enabled tools and why, and what's not saved in the repo.
- **Phase 1 — project structure** ([05-implementation-plan.md](05-implementation-plan.md)):
  created the full `Assets/_Project/` folder tree (`Art/{Materials,Models,Textures}`, `Audio`,
  `Prefabs/{Books,Interactables,Shelves}`, `Scenes`, `Scripts/{Books,Core,Interaction,UI}`,
  `ScriptableObjects`, `UI`).
- **Phase 3 — book data layer**: added `BookCategory` enum (`Alchemy`, `Astronomy`, `Beasts`,
  `History`) and `BookDefinition` ScriptableObject (`bookId`, `displayName`, `category`,
  `spineColor`) at `Assets/_Project/Scripts/Books/`. Created 4 sample `BookDefinition` assets in
  `Assets/_Project/ScriptableObjects/` (one per category, colors matching the MVP spec table) so
  the data structure is inspectable in the Editor. No pickup/interaction system yet — that's
  Phase 4.
- **Phase 2 — visual book proof**: added a `Book` MonoBehaviour
  (`Assets/_Project/Scripts/Books/Book.cs`) that links a scene object to a `BookDefinition` and
  tints its renderer via a `MaterialPropertyBlock` (sets both `_BaseColor` and `_Color`, so it
  works with the shared URP material without instancing a per-object material or leaking memory
  in edit mode). Created one shared base material
  (`Assets/_Project/Art/Materials/Mat_BookSpine.mat`, URP/Lit) and 4 test book cubes —
  `Book_Alchemy`, `Book_Astronomy`, `Book_Beasts`, `Book_History` — under a `ScatteredBooks`
  parent, scaled to the spec's book proportions (X 0.20–0.40, Y 0.55–0.95, Z 0.12–0.20) and
  scattered on the floor a few meters in front of the shelf (Z ≈ 2.0–2.8, within the shelf's X
  footprint), each wired to its matching sample `BookDefinition`. Verified via scene captures:
  colors render correctly and are visually distinct; not yet confirmed live in Play mode from
  the player's actual first-person view (see Phase 7 playtest questions).
- Created a fresh Unity 6.6.0f1 project using the URP Empty Template.
- Renamed the default scene to `LibraryPrototype` (`Assets/Scenes/LibraryPrototype.unity`).
- Created a simple greybox room:
  - Floor.
  - Back wall.
  - Left wall.
  - Right wall.
  - Open front for easier editor navigation and prototyping.
- Created a basic shelf wall:
  - `ShelfBack`.
  - Four horizontal shelf boards: `ShelfBoard_1` through `ShelfBoard_4`.
- Installed the newer Unity `Starter Assets: Character Controllers | URP` package.
- Added the package's first-person `PlayerCapsule` prefab into the scene as `Player`.
- Disabled the original URP template `Main Camera` rather than deleting it.
- Added the Starter Assets modular `MainCamera` prefab.
- Added the modular `PlayerFollowCamera` Cinemachine prefab.
- Configured the camera system so it works in play mode:
  - The render camera has `Camera`, `Audio Listener`, and `Cinemachine Brain` components.
  - The player follow camera is live.
  - The first-person camera target is `PlayerCameraRoot`.
  - The third-person follow behavior was adapted to a first-person view.
- Tested basic first-person movement in Play mode. Mouse look was believed to work at the time
  but was not actually validated from the render camera's output — see the fix noted above.
- Initialized Git locally.
- Added a Unity-specific `.gitignore` after an initial attempt tried to include Unity's
  locked/generated `Temp` files.
- Created a clean Git commit for the playable greybox.
- Pushed the project to the existing GitHub repository named `BookBoy`.

## Known non-blocking issue

Unity displays a yellow compiler warning from a Starter Assets third-person editor helper
referencing an obsolete Unity API. It is in imported package/editor code, not the prototype
gameplay code, and it does not block compilation or play mode. Do not spend time fixing it during
the MVP.

## Not yet completed

- 8 of 12 books/shelf slots exist. The last 4 (one more per category) are deliberately deferred
  until the 8-book test is confirmed to feel right.
- No book meshes/models beyond primitive cubes, no book prefabs yet (still loose scene objects,
  not prefabbed — reasonable for this stage, but worth prefabbing before finishing the set of 12).
- No placement feedback (chime, glow/pulse on correct placement) — Phase 6.
- No `Books Restored: X/12` running counter or a proper completion screen/restart — `CleanupTracker`
  is a minimal stand-in, not Phase 6 itself.
- No game audio at all.
- `CameraLookBridge`'s sensitivity (`3x`) and invert-Y default were set from direct feedback after
  one round of testing — reasonable to expect another tuning pass once more of the loop exists.

## Current scene inventory

Scene hierarchy is approximately:

```text
LibraryPrototype
  Main Camera                 # Original URP template camera, disabled
  Directional Light
  Global Volume
  Floor
  BackWall
  LeftWall
  RightWall
  ShelfBack
  ShelfBoard_1
  ShelfBoard_2
  ShelfBoard_3
  ShelfBoard_4
  Player                      # + PlayerInteractor (Phase 4)
    PlayerCameraRoot
    Capsule
  MainCamera                  # Active Starter Assets rendering camera
    BookCarryPoint            # Added in Phase 4 — held-book parent
  PlayerFollowCamera          # + CameraLookBridge (mouse-look fix)
  ScatteredBooks              # Added in Phase 2, expanded to 8 books
    Book_Alchemy / Book_Alchemy_02
    Book_Astronomy / Book_Astronomy_02
    Book_Beasts / Book_Beasts_02
    Book_History / Book_History_02
  HUD_Canvas                  # Added in Phase 4 — E Pick Up / E Place prompt
    InteractionPrompt
    CompletionMessage         # "All 8 books restored!" once every slot is occupied
  ShelfSlot_Alchemy, ShelfSlot_Alchemy_02        # on ShelfBoard_1
  ShelfSlot_Astronomy_01, ShelfSlot_Astronomy_02 # on ShelfBoard_2
  ShelfSlot_Beasts_01, ShelfSlot_Beasts_02       # on ShelfBoard_3
  ShelfSlot_History_01, ShelfSlot_History_02     # on ShelfBoard_4
    # each has a Marker child — a visible placeholder matching its book's spine color, hides once occupied
  CleanupTracker               # watches all 8 ShelfSlots, drives CompletionMessage
```

Current room is intentionally greyboxed. It should remain primitive until book scale, player
interaction distance, and shelf readability have been validated.
