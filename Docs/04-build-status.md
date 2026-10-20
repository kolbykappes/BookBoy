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
6. **Quick Shelve** (F, `QuickShelve`, `ShelfPlanner` rules): plans every held book against the live claims (the bookcase of its
   theme and the shelf that already holds its series, else the first unclaimed shelf; volume N in slot N), reserves the slots, and flies each
   book along an arc with a spin and a glow trail, snapping it in with the normal green/yellow/red feedback.
7. **Sister Glow** (G, `SisterGlow`): pulses the outline on every unshelved book of the held book's series (not the held
   ones, not shelved ones). Needs the upgrade (locked at the start); own 60 s timer (see "Abilities, timers..." below).
8. **HUD** (`GameUI`): run time, books correct / total, shelves finished, upgrade points, what you carry, toasts, and a
   completion message with the final time (your score is the time to finish).
9. **Outline glow instead of whole-book glow** (`BookOutline`, `BookOutline.shader`): a thin outline plus a faint halo (back-face
   shells of the book mesh) and sparkles for focus, sister glow and placement feedback; the book's body color is never
   changed. Shells are flat in depth, otherwise they read as a slab behind the book.
10. **Tech tree and menus**: `UpgradeCatalog` (24 upgrades, exactly 32 points; see the abilities section below), the upgrade menu on Tab, an admin menu on F10 (+1/+5/+10 points, unlock all, reset), and
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

- **4 bookcases x 2 shelves x 4 slots** (the original slice; now 16 bookcases, see the themes section below). One
  bookcase per color, one series per shelf, volume N in slot N (left to right). Superseded: bookcases no longer
  take their color from the first book - each has a fixed theme with a wall sign. The **first book shelved on a
  shelf still claims** that shelf's series; if two claim it the earlier claim wins. Slots are only
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
- **Code:** `Bookcase` (fixed `theme`), `Shelf` (claims a series, aim volume, ray-plane slot picking),
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

## Themes and wall signs (2026-10-17)

The player asked for a key telling them which bookcase is which, by theme and not by color.

- **Sixteen magical themes**, one per color (`BookThemes`): Alchemy & Potions (amber), Astral Magic (indigo), Beast Lore
  (green), Forbidden Lore (crimson), Tidal Magic (teal), Illusion Magic (violet), Charm Magic (rose), Fire Magic (rust),
  Herbal Lore (olive), Storm Magic (azure), Earth Magic (brown), Rune Craft (slate), **Healing Magic (white)**, Shadow
  Magic (charcoal), Radiant Magic (lemon), Renewal Magic (mint). The color names appear only in this table; the game
  never shows them. Book titles use the theme word ("Healing Tome Vol. 1").
- **Fixed bookcase themes.** `Bookcase.theme` is set by the scene generator (a seeded shuffle, so the wall is not in
  color order). A book in a bookcase of another theme is red even when that bookcase is empty. The old "first book
  claims the bookcase" rule is gone; per-shelf series claiming is unchanged.
- **Wall signs.** A world-space plaque (`ThemeSign`, `Sign_1`..`Sign_16`) hangs above each bookcase and shows two
  lines such as HEALING / MAGIC. Generated by `tools/build_library.src.cs` (best-fit text, 46 cm wide plaques on the
  0.5 m bookcase pitch).
- **Sister Glow is much stronger** (thicker outline, halo, point light, sparkles, x-ray through occluders).
- **Verified:** the Play Mode suites (shelves 30 checks incl. themes/signs/no color words, carry 19, toss 4,
  progress 27 + 6) and 58 Edit Mode tests pass; the scene file was byte-identical after Play. Not verified by me: how
  the plaques read at game-window resolution while walking (checked in a 1920x900 render from the spawn point only).

## Abilities, timers, mouse controls, lighting and floor (2026-10-17)

- **Mouse:** left click = E (pick up / place), right click = Q (toss). The keys still work.
- **Readout:** after a correct (green) placement, aiming at that book shows the NEXT held book's name in the middle of the
  screen ("Next up - aim at an open spot to place"). A finished green book is not picked up while the hands are busy
  (with empty hands it still can be); yellow/red shelved books are still pick-up-able.
- **Four abilities, all locked at the start** (Sister Glow was already locked by default; a test now asserts it): G Sister
  Glow, F Quick Shelve, C Gather Family (`GatherFamily`), B Home Beacon (`HomeBeacon`).
- **Own timer per ability** (`AbilityTimers`, engine-free, Time.time based so menus pause it): ready when bought, 60 s after it
  does something (a use that does nothing - no sisters, hands full - costs nothing). Each ability has two recharge upgrades:
  40 s and 20 s (never below 5 s); buying one shortens a running timer at once. The HUD (bottom right) lists each unlocked
  ability with a fill bar and READY / countdown.
- **Gather Family:** pulls loose (unshelved) books of the held book's series, nearest first, along curved, spinning flights with
  a sparkle trail; 2 books, 3 with Bigger Pull, the whole series with Whole Family; never more than the free hand space
  (books already in flight count) or the loose sisters. A book that finds the hands full on arrival drops to the floor.
- **Home Beacon:** a pulsing gold 12-edge frame (LineRenderers, `Mat_Beacon`) around the outer bounds of the bookcase whose theme
  matches the held book, plus a soft light, for 8 s.
- **Tree:** 24 nodes costing exactly 32 points (Strong Arms I-VI 10, Fleet Feet I-III 5, per ability: unlock + recharge I + II,
  Lingering Glow, Bigger Pull, Whole Family). The menu rows are 32 px so all 24 fit.
- **Admin menu (F10):** two new buttons pile every unshelved book (hands included) - or every book, shelves emptied - in the middle
  of the room (`AdminTools`): the books are dropped in bursts and stay solid physics bodies until the whole pile has landed,
  then become normal pick-up-able books. Also "reset ability timers".
- **Lighting:** the single directional light (soft, 50 m shadows) is now a dim, shadowless moon fill; four floating warm
  "magelights" (`MageLight`: bobbing orb + flickering point light; two cast soft shadows), trilight ambient, dark night sky
  behind the open room, warmer wall material. URP's per-object additional-light limit is 4, so there are exactly 4 orbs; changing
  the pipeline asset (e.g. Forward+) was not done (needs approval).
- **Floor:** generated 2x2 marble checker tiles with grout, veining and a normal map (`tools/setup_assets.cs` writes the PNGs and
  materials; `tools/setup_scene.cs` applies them and adds the new components; both re-runnable).
- **Verified:** 74 Edit Mode tests; Play Mode suites: shelves 30, carry 21 + toss 4, progress 29 + 6, abilities 20 + 8, pile 5 + 7 + 4,
  mouse 8 (driven through the Input System, not just by calling methods). Renders checked for the lighting, floor, signs, pile,
  HUD, beacon and the 24-row upgrade menu. Not verified by me: how the lighting reads on the user's monitor, and mouse feel.

### Follow-up: menu cursor, HUD position, carried-book light (2026-10-17)

- **Menus no longer release the Windows cursor.** Tab / F10 used to free it, which broke out of the game window. Now
  `CursorLock.Suspend` keeps it locked and hidden (and re-takes the lock if the OS drops it, e.g. alt-tab), and `GameUI` draws a
  software arrow pointer (`GameUI_Pointer`) moved by the mouse delta and clamped to the screen; hover and left click use it.
  `tools/verify_menu_pointer.cs` (13 checks) drives Tab / F10 / Esc / clicks through the Input System.
- **Ability timer panel moved** to the middle of the right-hand side (narrower: 330 px rows) so it no longer covers the books
  you carry (drawn in the lower right). Checked numerically: no overlap with the legend, carry panel or held-book corner.
- **Carried books in dim light:** a soft warm point light ("HandLight", range 2.6, no shadows) on the player camera lights the
  carried books and the shelf in front of you. Edit-mode renders show the held books clearly brighter with it.
- Not verified by me: how it looks live (the Editor was in the background, so the Game view only gave stale captures, and the
  frame loop was frozen; the pointer and cursor checks are synchronous and passed).

## Lamplit build: hub and Rooms 1 to 3 (2026-10-18, branch `lamplit/day-1`)

Built from [14-build-plan.md](14-build-plan.md): steps S0 to S9, all stops (A to F) reached. The prototype scene
(`LibraryPrototype.unity`) is unchanged and all its suites still pass; the new scene is `Assets/Scenes/Lamplit.unity`.

- **Scene:** a copy of the prototype with the library content stripped (`tools/lamplit_s0_scene.cs`), then built by
  `LamplitBuilder` (Editor-folder class, `Assets/_Project/Editor/`): `BuildHub()`, `BuildRoom1()`, `BuildRoom2()`, `BuildRoom3()`.
  Each is re-runnable (removes and rebuilds its own objects; definitions and materials are mutated in place).
  `LamplitBuilder.Shot(path, pos, lookAt)` renders a PNG from any point of view.
- **Hub:** a hallway 3 x 16 m. The librarian's desk is at the entrance end with six spots (apron hook, shoe rack, glasses stand,
  shelf-list board, clipboard, and a locked dumbwaiter hatch). Doors: Room 1 and Room 3 on the left, Room 2 on the right; a door
  slides up when its room is open and its sign says LOCKED otherwise. Room 1 is open from the start; each next door opens when
  the previous room is tidy.
- **Room 1, Reading Room:** 100 books, 20 groups of 5 (a colour and a letter per group), 10 containers with 20 five-slot shelves.
  **Group** rule: the first book on a shelf claims it; the same group in any slot is green, another group yellow. Apron and shoes
  are on the lost-and-found table.
- **Room 2, Mailroom:** 120 items: books, magazines, letters, parcels (distinct sizes), 6 groups of 5 each, 12 containers (3 per
  family, colour-coded with signs). **Group** rule plus a **family check**: the wrong kind of container grades red, even when empty.
  Glasses and shelf list are hidden here.
- **Room 3, Map Room:** 120 items: atlases, rolled maps, flat sheets, 8 numbered sets of 5 each, 12 containers (atlas shelves, tube
  racks, drawers). **Ordered** rule: number N in slot N (letter and number on the spine). The pull list is hidden here.
- **Finds and the desk:** a find is a carried item (a `Book` with a `findId`); aim at its desk spot and press E or click. Only the
  matching spot accepts it. Apron: carry +2 (6). Shoes: speed +15%. Glasses: Sister Glow. Shelf list: Home Beacon. Pull list: Gather
  Family. Quick Shelve stays locked (the dumbwaiter hatch says so). Tab shows the finds list; shelves earn no tech-tree points here.
- **Grading (engine-free, tested):** `PlacementRules.EvaluateCampaign` (red: wrong family or wrong room; yellow: another group owns
  the shelf, or the number is not in its slot; green otherwise). Group claims are compared across every shelf in the room, so a
  group cannot be split over two shelves. `Book` has `FamilyKey` / `GroupKey` / `NumberKey` that fall back to the library's
  category / series / volume, so the prototype is untouched.
- **Campaign state (engine-free, tested):** `CampaignState`, `CampaignCatalog` (finds, spots, grants), `UpgradeState.Grant`.
  In the scene: `CampaignController`, `RoomZone`, `RoomDoor`, `FindSpot`.
- **Abilities in Lamplit:** Sister Glow, Gather Family and Home Beacon match by family and group (`Book.SameGroupAs`); Home Beacon
  lights a container of the item's family that already holds its group, else one with a free shelf, nearest first.
- **Admin menu (F10) in Lamplit:** reset / scatter / complete this room, reset everything, jump to the hub or any room, give or
  install any find, unlock the three abilities, open every door or put doors back to the rule, and a counts line.
- **Verification:** 95 Edit Mode tests (21 new). Play Mode scripts in `tools/`: `verify_lamplit_hub.cs` (15), `_room1.cs` (24),
  `_finds.cs` (18, includes a real mouse click), `_room2.cs` (27), `_room3.cs` (26), `_admin.cs` (20) and `_campaign.cs` (17, the
  whole loop end to end). All prototype suites re-run and pass. Renders checked for the hub, each room and the HUD. Not verified by
  me: how it feels to play (pacing, whether groups of five are right, colour and symbol legibility) - see the playtest chore in
  [10-backlog.md](10-backlog.md).
- **Known limits:** item shapes are cubes (placeholders on purpose); finds have no glow; Quick Shelve (`ShelfPlanner`) still plans by
  the old colour/series rules, so it is locked in Lamplit; Home Beacon lights a whole container, not a shelf; no ceilings, sound or
  story text.

### Lamplit: piles of books (2026-10-18)

Every room now starts as **several random piles tossed around the middle of the room** instead of a grid of single books, and there
is a "pile up the books" feature to do it again.

- **Plan (engine-free, tested):** `PilePlanner.Plan` picks about one pile per 18 items (3 to 7), random centres inside the room's
  scatter rectangle (at least 1.3 m apart), uneven pile sizes, and which item goes in which pile; seeded, so the same seed gives the
  same layout.
- **In game:** `RoomZone.PileUp(seed)` picks every item off its shelf or the floor and tosses them down with real physics, one item
  per pile every 0.1 s, each with a random spin and a little sideways toss; they stay solid until the whole room has landed
  (`Book.BeginSolid` / `Toss`), then become normal pick-up-able items (`Book.EndSolid`). `CampaignController.PileRoom(n)` does it for a
  room; admin menu row 3 ("Pile this room's books up"), "Reset this room" and "Reset everything" all use it.
- **Starting state (baked):** `LamplitBuilder.BakePiles` runs the same plan in the Editor on proxy cubes in a throwaway preview
  scene (its own physics scene, just a floor) and copies where they land onto the real items, so the scene file already holds the
  piles; `RebakeAllPiles()` redoes all three rooms. Setting `Physics.simulationMode` was tried and rejected: it makes the Editor
  rewrite `ProjectSettings/DynamicsManager.asset` (a fenced file), and `Physics.Simulate` does nothing without it.
- **Result:** Room 1 starts as 6 piles, Room 2 as 7, Room 3 as 8 (uneven sizes, highest point under 0.3 m).
- **Verified:** 14 new Edit Mode tests (109 total). Play Mode: `tools/verify_lamplit_piles.cs` (14 checks: baked piles in all three rooms, then the admin
  pile-up of a tidy room starts) and `_piles_part2.cs` (6 checks: the live drop lands in several piles, nothing in the air or still solid, a book
  can be picked up from a pile) pass; part 2 needs running frames, so run it with the Editor window in front. All seven other Lamplit suites and
  the prototype suites (carry, toss, abilities, pile) still pass.
- **Bug found and fixed on the way:** `Book.StopPhysicsTumble` removed the rigidbody with a deferred `Destroy`. If a pile-up restarted while
  books were still in flight, the new toss reused that rigidbody in the same frame and lost it at the end of the frame, leaving the book frozen
  in mid-air with a solid collider. It now removes it immediately (`EndSolid` too).

### Lamplit: rooms are switched off until their door opens (2026-10-18)

Question raised: why are Rooms 2 and 3 full of live books and lights before the player can reach them? Measured before the change: 2,019
GameObjects, 950 renderers, 658 colliders, 345 books, 14 lights in the scene (about 1,100 objects in the three rooms; each item is 3
GameObjects: the cube and two spine-label glyphs). Unity frustum-culls drawing automatically, but it does not occlude things behind walls,
and it never culls colliders or scripts, so unreachable rooms were costing memory, physics and draw submissions for nothing.

- **Change:** `CampaignController` switches a room off (`RoomZone.SetLive(false)`: the room object plus every item and find, since items do not stay
  parented to the room) until its door is open. Room 1 starts on; tidying a room opens the next door and switches that room on. Admin commands that
  touch a room (jump, pile, complete, give or install a find) switch it on and keep it on; "reset everything" keeps already-open rooms on.
- **Result at the start of a run:** live books 102 (was 345), renderers 306 (was 950), colliders 212 (was 658), lights 8 (was 14).
- **Verified:** `tools/verify_lamplit_activity.cs` (8 checks) and all other Lamplit suites pass. The scene is still saved with every room on; the
  controller switches them off when the game starts.
- **Also fixed:** `RoomZone.IsPiling` now stays true until every item has settled back into a normal book (it used to clear about a second early).
- **Not done (backlog L-016):** the real fix for scale is to keep items as data and create them when a room opens (pooled), merge the spine
  labels into the item's own material, share materials for instancing, and bake occlusion culling. Frame time and draw calls were not measured.

### Lamplit: storybook scale (2026-10-18)

Kolby compared a Lamplit pile with Witch Cabin Cleanup: our books were too small and the view too distant. Diagnosis (measured): our camera had a **40
degree** field of view (a telephoto look), and the items followed the life-size rule, so a book filled about half as much of the screen. A wider lens
on its own made things *smaller*, which the renders showed. What fixed it was changing the item size relative to the player.

- **Camera and player:** FOV 75, eye height 1.15 m, speed 3.2 / 4.8, reach 2.8 m, aim 12 m, held books 0.55 (`LamplitBuilder.ApplyPlayerScale`).
- **World scale 1.6** (`LamplitBuilder.WorldScale`): containers (the whole container object is scaled, so slots, aim volumes and highlights follow), items, finds and
  desk props. The hub is 4.8 x 24.8 m, rooms 12 m wide and 11.4 to 13.4 m deep, walls 4.2 m, doors 2.0 x 3.0 m; signs, lights and the pile spacing
  (`RoomZone.pileSpread`) are scaled to match. Installed finds keep their real size on the scaled desk spots; the slot-aim depth follows the container's scale.
- **Builder fix:** items do not stay parented under their room, so rebuilding a room used to leave the previous build's items behind (duplicates). The room
  builders now call `RemoveOrphanItems()`.
- **Verified:** 109 unit tests; all eight Lamplit suites, the live pile check and the prototype shelves, carry and toss suites pass; the checks were updated for
  the new geometry (spawn, doors, eye height, "in the air" thresholds). Real Game-view captures show chunky, readable books and a pile with weight, and held
  books are bright with the group letter legible. Not judged: how it feels to walk around (pace, room size).
- **Next if it still feels off:** a different multiplier is a one-number change; richer item art (cover colour, a real title, lying face up), smaller
  piles, and depth of field are the next levers.

### Lamplit: lower boards and packed slots (2026-10-18)

From the scale study ([competitive/scale-and-camera-comparison.md](competitive/scale-and-camera-comparison.md)): every board was above the player's eyes (eye 1.15 m, lower
board top 1.36 m, upper 2.08 m), where Librarian, Shelves and Sorcery and Witch Cabin keep the eye line inside the shelf band, and slots left visible gaps.

- **Boards:** base cabinet 0.6 m (local; 0.96 m in the world), boards 0.45 m apart (0.72 m), container height 1.54 m (2.4 m); family signs moved to 3.0 m
  (`LamplitBuilder.BaseHeight`, `ShelfSpacing`, `ContainerHeight`). The lower board top is now at 0.96 m and the upper at 1.68 m, so at standing distance both rows of
  books are in the frame.
- **Slots packed:** pitch 0.0625 m local (0.10 m in the world) instead of 0.072 (0.115), just clearing the thickest item (0.06 local); container pitch 0.52
  local so containers stand closer together.
- **Verified:** all eight Lamplit suites pass on the rebuilt rooms, 109 unit tests pass, the pile check part 1 passes. Renders show a wall of 20 shelves with both
  rows visible and packed books. **Not re-run:** the live pile drop (`verify_lamplit_piles_part2.cs`) needs running frames and Unity was in the background; it passed
  earlier the same day and this change does not touch the pile code.
- **Deferred (backlog L-021 to L-023):** palette and contrast (purple and navy groups are darker than the wood), a ghost of the held item in the aimed slot, and
  visible claims (a nameplate on the board).

### Lamplit: head light, room scale and chunkier books (2026-10-18)

Kolby's playtest of the lowered-board build: the books do not look wider, the game washes out close to a shelf ("a light on my forehead"), and the rooms look sized for giants.

- **Head light (found by listing every Light in the scene):** `MainCamera/HandLight` (point, intensity 2, range 2.6, 0.2 m ahead of the lens) was added so carried books stay readable in
  the dim rooms. At the new scale the shelves are about 0.9 m from the eyes, where inverse-square falloff makes it 4 to 8 times brighter than the room lights, so the
  aimed books bleached to white. Now intensity `0.6`, range 2.8, set back 0.25 m behind and 0.25 m above the lens (`LamplitBuilder.HandLight*`, applied by `ApplyPlayerScale`, which
  now finds the light through `Camera.main`, because the brain camera is not a child of the player). Old and new settings rendered side by side from 0.9 m: the old one reproduces the washed-out
  screenshot, the new one keeps the colours and the spine letters.
- **Rooms scaled to the player:** ceiling slabs added (the rooms were open to a black sky), wall height 4.2 to 3.0 m, doors 2.0 x 3.0 to 1.5 x 2.2 m, magelights 3.5 to 2.55 m, desk and
  family signs lowered, containers 2.46 to 2.34 m (`OrbHeight`, `SignHeight`, `ContainerHeight`).
- **Chunkier books:** spine thickness up about 1.45 times (Room 1 0.065 to 0.078 local, atlases 0.085), slot pitch 0.0625 to 0.095 local (0.152 m), container pitch 0.68 local; ten containers still
  fit Room 1 (10.8 m of 11.4) and twelve fit Rooms 2 and 3 (13.0 of 13.4).
- **Verified:** 109 unit tests, the eight Lamplit suites and both pile suites pass (the live pile drop ran with advancing frames this time, answering last entry's open question).
  `tools/verify_lamplit_piles.cs` no longer requires at most 10 clusters (flat sheets slide into singles); it requires at least 3 real piles and at most 12% strays.
  Renders: hub, Room 1 wall and close-up with all books shelved.
- **Not verified:** how it feels live in the Game view (no new Game-view capture was possible from here; the renders use the same lighting and camera settings).

### Lamplit: adult scale, four-shelf bookcases, Room 1 rebuilt (2026-10-18)

Kolby decided the storybook rescale was a wrong turn and asked for a normal adult in a cozy room, with Room 1 rebuilt first to test (backlog T-002, T-017).

- **Scale back to 1.0x** (`WorldScale`, `LayoutK` = 1/1.6 converts the old layout numbers): eye 5'5" (1.65 m), capsule 5'11" x 1'0" radius, FOV 90 horizontal (58.7 vertical), ceiling 9 ft, door 3 x 7 ft, hub 10 ft wide, Room 1 about 25 x 23 ft. Full table in [09-scale-reference.md](09-scale-reference.md).
- **Two bookcase types:** a solo 8-wide case and a triple (4-wide wing, 8-wide centre, 4-wide wing), each with four shelves; the builder's `Container(..., slots)` takes the slot count.
- **Sets of 4 or 8:** a set belongs on a row of its own size (engine-free `PlacementRules.SetFitsShelf`, `BookDefinition.setSize`; wrong size grades yellow, never overriding red).
- **Room 1:** 96 books in 16 sets (8 of 8, 8 of 4) on a triple and a solo; hub desk and doors resized. The orbs are provisionally dimmer (2.0 at 6 m) for the lower ceiling; the lighting pass (T-003) retunes them.
- **Verified:** 112 Edit Mode tests; hub, Room 1, finds, Rooms 2 and 3 (old layout), admin, campaign, activity and pile part 1 suites pass (the suites for the hub, Room 1, admin, campaign, activity and piles were updated for the new geometry; Room 1's is rewritten). Offscreen renders of the hub, Room 1 and the cases.
  Not run: the live pile drop (part 2 needs running frames, which need the Editor in front).
- **Not done:** Rooms 2 and 3 are still the old layout and are not at the new scale (they do not line up with the narrower hub, so do not open them yet); decor or blocked rows; lighting retune; Game-view playtest.

### Lamplit: tomes (2026-10-19)

Kolby: the books look like normal books and should be tomes. Books are now **12 to 15 in tall, 10 in deep and 4 in thick** (all spines 4 in; heights 12, 13, 14 and 15 in by set). The bookcases were resized around a 15 in book: slot pitch 4.25 in,
shelves 17 in apart, a taller 15 in plinth so the top shelf sits at 5'5.7" (it was 4'2"), case 13 in deep and 7'0" tall; solo case about 37.6 in wide, triple about 6'6.7". Spine letters are bigger (2.2 in). Room 1 rebuilt; the suites were
re-run: 112 unit tests and the hub, Room 1, finds, Rooms 2 and 3 (old layout), admin, campaign, activity and pile part 1 suites pass. Table in [09-scale-reference.md](09-scale-reference.md). Not run: the live pile drop (needs the Editor in front).

### Fix: nothing could be picked up after the adult-scale rebuild (2026-10-19)

Kolby could not pick anything up. Cause (reproduced): the aim ray starts at the camera, which sits inside the player's own colliders (the character controller and the capsule mesh); from the new eye height (1.65 m, in the capsule's top hemisphere) every downward ray reported the player's own collider at about 1 cm as the first hit, and the interactor treats a first hit that is not a book, shelf or desk spot as "nothing here". Why it worked at the old eye height is not established (probably the old 1.15 m eye sat in the straight part of the capsule). Fix: `PlayerInteractor.UpdateFocus` skips colliders that belong to the player. Regression check: `tools/verify_lamplit_pickup.cs` (aims from the real eye position at books placed around the player; it failed 0 of 5 before the fix and passes 5 of 5 after). All other suites still pass.
Lesson: the earlier suites aimed from points outside the capsule, so they could not see this; the new check aims from the true eye position.

### Lamplit: tome mesh, sample cover material, heavier bookcases (2026-10-19)

Following the Librarian comparison (Kolby: book size is close enough; do the rest):
- **Blender pipeline:** Blender 5.2 is now installed; `tools/blender/make_book.py` builds the tome mesh headless and exports FBX, `make_book_textures.py` generates the textures (see `tools/blender/README.md`). The mesh is a unit box so the existing item scale, slot code and box collider are unchanged. Verified in Unity by `tools/verify_book_mesh.cs`.
- **Sample material:** `Mat_BookCover` (URP Lit: tintable cloth-weave albedo with embossed borders and a cartouche on the boards, raised bands, rules and a label plate on the spine, plus a normal map; smoothness 0.30) and `Mat_BookPages` (cream with page lines). Each book's set colour tints only the cover (material slot 0 through the property block), so pages stay cream.
- **Bookcases:** slots packed (4.1 in pitch for a 4 in spine), boards and sides 2.4 in thick (they were 0.8 in and 1.6 in), a lip on the plinth and a crown, 1 in of headroom above the 15 in books on every row (shelf spacing 18.5 in; top shelf at 5'10.5", case 7'5" plus the crown).
- **Held books** at full size (carry scale 1.0, was 0.6).
- Verified: 112 unit tests; all suites (hub, Room 1, finds, Rooms 2 and 3 old layout, admin, campaign, activity, pickup, pile part 1) pass. Not run: the live pile drop and a Game-view playtest.
- Not done: spine icons and numbers (with the palette work, T-010), lighting retune, Rooms 2 and 3 conversion, glow outline of the page block (the glow hull covers the cover submesh only).

### PerfLab: the performance test room (2026-10-19, backlog T-018 stage A)

A separate scene, `Assets/Scenes/PerfLab.unity` (a copy of Lamplit's player rig and lighting settings without the hub, rooms, campaign or game UI), for seeing and measuring large numbers of books. 26 x 26 m room; up to 3000 tome books in 60 baked piles (the physics bake runs in a preview scene, as for Room 1) stored as a layout of positions and rotations plus 96 inactive templates, spawned at start by `PerfLabController`; six lamp orbs (3.6 intensity, range 14) and 24 small lights.
In Play Mode: an overlay shows fps, frame ms (average and worst of the last 120 frames), draw calls, batches, SetPass calls, triangles and shadow casters (Profiler recorders, also available in a development Player). Keys: `[` `]` book count (100, 300, 1000, 3000), `1` to `5` lighting (orbs without shadows; one shadowed orb, the game's setting; all orbs shadowed; 24 small lights; directional light only), `Z` book shadows, `X` GPU instancing (on runtime copies of the materials, nothing is written to the assets), `C` colour by property block or shared per-colour materials, `V` uncapped or vsync, backquote hides the overlay.
Checked: the scene builds, spawns 3000 books with 1000 active, no console errors in a short Play; offscreen renders at 1000 and 3000 books and the lighting presets. Not yet measured: any frame time (Editor numbers are not trustworthy; the Player-build harness is next) and the Game-view feel.
Opening it: File, Open Scene, PerfLab (it is the open scene right now); `LamplitBuilder.OpenLamplit()` goes back. The other builders and the suites expect Lamplit.unity to be the open scene.

### Lamplit: carved bookcases and wood (2026-10-19)

Bookcases are now Blender meshes (`tools/blender/make_cases.py`) with a procedural wood material, designed after the Librarian reference: stepped plinth and cap, reeded pilasters with base blocks and capitals, thick shelf boards with a nosing, a planked back, a stepped cornice with a dentil course, an arched crest with a brass rosette on the 8-wide bays and brass nameplate plates on the shelf fronts (room for the claims labels, L-023). The solo is one mesh (8,000 triangles) and the triple one shared mesh (16,000 triangles) over the three shelf-carrying cases, which keep invisible cubes as colliders; shelf tops, board and side thicknesses and depth come from the same constants (`tools/blender/case_params.json`), checked by `tools/verify_case_mesh.cs`.
Materials: `Mat_CaseWood` (grain albedo and normal map tinted warm mahogany, smoothness 0.60), `Mat_CaseBack` (darker, matte) and `Mat_CaseBrass`. The cases are about 8 ft tall with the crest (2.65 m of the 2.74 m ceiling). Verified: 117 unit tests and all suites pass; the mesh check passes; renders at 4 ft and from the door show the grain, reeding, cornice and crest. Not done: Game-view look under the final lighting, wood variants per room, the same treatment for Rooms 2 and 3, per-shelf brass plates showing claims (L-023).

### Lighting pass, palette, spine icons and volume numbers (2026-10-19; backlog T-003 safe parts, T-010)

**Lighting (scene-level changes only; no ProjectSettings or pipeline asset touched).** `LamplitBuilder.LightStats` renders a view with the game's post-processing (tonemapping, bloom, vignette) and reports luma percentiles, near-white and near-black shares, a top-third hotspot share and the luma at probe points (spines, board fronts, a wall, the floor); `tools/measure_light.cs` runs it from three views of Room 1 with every book home.
Before (orbs 2.0 / 2.2, range 6): median luma 0.19 (door view), 0.18 (4 ft from the triple), 0.22 (solo corner); spine probes 0.17 to 0.29 against board fronts 0.12 to 0.17; hotspot share 0%.
After (orbs 5.0, the shadowed one 5.5, range 9; the 0.45 bluish directional fill kept because raising it to 0.8 changed nothing measurable): median 0.29 / 0.34 / 0.33. Nothing is near-white or near-black (0.0%) in any view; no clipped ceiling hotspot (it is a soft warm glow). Hub lamps use the same constants (hub median was 0.23 to 0.24 before; not re-measured).
Not applied, needs Kolby's approval (render-pipeline settings): soft shadows High to Medium, 4 cascades and 50 m to fewer and shorter, shadow atlas big enough for the shadowed lights, Bloom Clamp about 3 to 6, motion blur off or low. Not tuned: ambient (Trilight), because the measured medians are in range.

**Palette (T-010).** New engine-free `BookBoy.Rules.Palette` (WCAG luminance and contrast, Oklab, Machado colour-blind simulation, a deterministic greedy farthest-point solver; 9 Edit Mode tests). `tools/solve_palette.cs` solved 24 colours for the current wood and cover texture: every set keeps a contrast of at least 2.2 against the wood after the cover texture's darkening is counted (average 3.5), and the smallest pair distance for any vision type is 0.045 in Oklab. Baked into `LamplitBuilder.GroupHex`. The old HSV generator (half its groups below 2.0) is gone. The wood is a little darker (`Mat_CaseWood` base 0.62 / 0.30 / 0.14, effective luminance 0.066), and the cover texture is brighter (mean linear 0.73, was 0.46, which had been darkening every colour by more than half). The result is a lighter, clearer palette (pinks, yellows, sky blues, greens); whether it is too pastel is Kolby's call.
Checked: the Room 1 suite verifies every set colour's contrast (min and average) from the build's own numbers. Not done: the same check measured on rendered frames per set (the luminance probes are averages), and a Colour Oracle review by eye.

**Spine icons and volume numbers.** The cover texture has three label plates (icon on top, the set letter, the volume number at the bottom, each with a rule inside its edge) and the spine bands moved to sit between them. Eight icon glyphs (disc, triangle, diamond, square, star, heart, moon, cross) were added to the pixel font (`PixelDigit.IconValue`), `BookDefinition.iconIndex` stores the icon (set number mod 8), and Room 1 books show icon, letter and number. The case mesh materials were also fixed: Blender orders material slots by first use, which had swapped the back panel and the brass (the planks were built before the plates now; `tools/verify_case_mesh.cs` checks which submesh is which).

### Placement ghost, claim plates and the glow (2026-10-19; backlog L-022, L-023, T-013)

- **Placement ghost (`SlotGhost`).** With a book in hand and the crosshair on an empty slot, a translucent copy of the book stands in that slot exactly where it would sit (the same pose `ShelfSlot.Accept` uses, at the book's real size), tinted green, yellow or red by how it would grade, with a slow pulse. It replaces the thin highlight bar (the bar returns only if there is no ghost). The grade comes from the new `Book.PreviewGrade(slot)`, which grades the book as if it were already placed: an unclaimed shelf counts as claimed by this book from now on; both the campaign and the prototype grading were refactored to share one path with a preview flag. Finds never get a ghost. Recomputed when the aimed slot or the held book changes and every 0.2 s (claims change only when books move).
- **Claim plates (`ClaimPlate`).** Every shelf of a mesh bookcase has a brass plate on its front (the plate is part of the case mesh, enlarged to 0.16 m wide; the first shelf's plate sits on the plinth panel because the cornice of the plinth cap hides anything under it). Once a set claims a shelf, the plate shows that set's icon and letter, engraved; an empty shelf has a blank plate; emptying the shelf blanks it again. The first-book-claims rule is no longer invisible. `Shelf.ClaimingBook` names the claimant.
- **Glow (`BookOutline`).** The hull renderers now cover every submesh, so the page block glows with the cover. The point light and the sparkle particle system are no longer built per book: they are rented from a small `GlowPool` while a book actually glows and returned when it stops (8 pulsing books use 8 pooled lights; none exist before a book glows). The pool's objects are destroyed at the start of the next Play session (they survive leaving Play otherwise; the project skips domain reload). Not done: the single shared mesh hull is still one renderer set per glowing book, and an outline renderer feature (needs approval).
- **Checked:** `tools/verify_lamplit_ghost.cs` (16 checks: the preview grade equals the real grade in 11 mixed placements; the ghost's pose, size, colour, hiding and the bar; the claim plates blank, filled and blank again; the pooled lights and the two-submesh hull) passes, as do the 126 unit tests and every other suite. Offscreen renders show the green and yellow ghost in a gap and the plates on every row.
- **Not verified live:** how the ghost feels in the Game view (the pulse, the colours under the final lighting), and the plates' legibility at standing distance (about 15 px tall at 4 ft).

### Printed covers and spines, and the held stack (2026-10-19; backlog T-004, T-010 follow-up)

- **Why.** The pixel-font spine labels were too coarse to judge legibility. Six series of real covers and spines were designed from the game's own topics (LAMPS, CATALOGUE, LETTERS, ATLAS, MENDING, VAULT) as high-resolution tintable textures (`tools/covers/`, see its README): 2048 x 1600 boards (back and front), 3072 x 1024 spine atlases with eight volume cells (title along the spine, a series icon plate and a big volume number). Room 1 sets A to F (36 books) use them; G to P keep the old tome for comparison.
- **Mesh and materials.** `Book_TomeStyled.fbx` (made by `make_book.py ... styled`): boards, pages and spine as three submeshes, 440 triangles like the plain tome. Twelve materials (`Mat_Cover_*`, `Mat_Spine_*`). `Book.SetColor` tints the boards and spine and sets the spine's volume cell through `_BaseMap_ST` in a per-slot property block; the pages stay cream. `BookDefinition.coverStyle` (-1 = plain tome).
- **Legibility (rendered at 1920 x 1080, 58.7 degrees).** At 4 ft the titles, icons and volume numbers read clearly; at the closest row distance everything is crisp. The printed spines are cleaner than the pixel labels but carry the title instead of the set letter, so the shelf's claim plate is now the only place the letter shows for those sets.
- **Held books, after Librarian (`CarryInventory`).** The carried books are a stack of flat books at the lower right, the current one on top with its cover up and its spine toward the player's left, the others hanging below it one thickness apart with a slight stagger. The stack's anchor (`CarryStack`, built by `ApplyPlayerScale`) is a child of the player root: it turns with the player but not with the camera pitch, so at a level gaze only the top corner peeks into the lower right and it rises into the right third of the view when the player looks down. Books are carried at their shelf size (`CarryScale` 1.0, no shrink). A ray from the body pulls the stack back when a bookcase or wall is close so it does not poke through. The count and the titles are the existing HUD list.
- **Checked:** `tools/verify_lamplit_stack.cs` (flat, spine left, order, gap, in view at level and when looking down, pulled back against a case, placing from the stack), `tools/verify_styled_book.cs` (mesh, UVs, materials, per-volume offsets, tint), the updated room 1 and ghost suites, every other suite and the 126 unit tests pass.
- **Caveats.** The fonts are Windows system fonts for testing only (replace with an OFL face before release; text is baked into the pictures). The stack's look and feel in motion (the glide, the pull-back near cases) is not verified live in the Game view. At level the peeking corner is small; if it should show more, raise `StackOffset` height or lower the anchor forward distance.

### Hover outline and hologram placement ghost (2026-10-19; backlog T-013, research file 29)

- **Why.** The yellow glow on the aimed book was not liked; Librarian's hover is a thin white line, and its slot ghost is an edge-lit hologram. Research is in `Docs/unity-practices/29-hover-outline-and-placement-ghost.md`.
- **Hover (`BookOutline`, shader `BookBoy/BookOutline`).** An inverted hull with a width in screen pixels (clip-space extrusion along smoothed normals, scaled with the screen height over 1080): a 3 px warm-white line (HDR, so the project's bloom gives it a soft glow) over an 8 px dark underlay so it reads on the cream floor tiles as well as on dark wood, breathing slowly. No sparkles, no light. The smoothed hull mesh (`BookOutline.HullMesh`: vertex normals averaged by position, cached) fixes the gaps the box edges leave with the book's own split normals; the two book meshes are now readable for this (about 400 vertices each). `Book.FocusColor` is neutral warm white. Sister-glow pulses and the placement flash keep their colours, sparks and light, now with the same crisp hull (3.5 px line and 9 px halo; 2.2x for the strong pulse).
- **Ghost (`SlotGhost`, shaders `BookBoy/BookGhost` and `BookBoy/BookOutline`).** A see-through body (almost clear in the middle, a bright fresnel rim, a slow light band rising through the book) inside a crisp outline hull, in the grade colour (green, yellow, red). The body writes depth so the hull shows only around the silhouette. Materials `Mat_SlotGhost` (body) and `Mat_SlotGhostOutline`; both are copied per grade at runtime.
- **Tried and rejected:** a depth pull on the hull (to show the line over touching neighbours on a shelf) paints the book over, because the hull's back faces come in front of the book's front faces.
- **Known limit.** A hull cannot show where something nearer covers it, so a book on a shelf between touching neighbours only shows the line on its top edge (Librarian shows no outline on shelved books either and relies on the title text). If that is not enough, the next options are a Render Objects outline feature (renderer asset: needs approval) or lifting the aimed book a few millimetres.
- **Checked:** `tools/verify_lamplit_ghost.cs` (now with the hologram and hull checks, the neutral hover colour, no light and no sparks on hover, welded hull normals) and every other suite pass; 126 unit tests pass. Renders: `tools/outline_preview/`.
- **Not verified live:** how the breathing, the bloom glow and the rising band look in motion in the Game view.

### Held stack: lower, fanned, and a "hands full" reaction (2026-10-19, after Kolby's play test)

- **Too high, and the stack did not seem to build.** The anchor was at 1.36 m, which sits above the middle of the view when looking down at the floor, and the books were stacked exactly on top of each other, so from above the lower ones were hidden under the top cover. Now: anchor at (0.40, 1.10, 0.52) from the feet (it is out of view at a level gaze and comes in from about 28 degrees of pitch), and the stack is FANNED: each lower book is also shifted to the player's left and toward the player (`CarryInventory.StackPlace`), so a strip of its cover and its spine shows under the one above. The newest pick-up is the current book and sits on top; the offsets shrink when many books are held. Renders at 35 degrees pitch with 1 to 6 books show the stack growing.
- **Scrolling.** The wheel (and R) already cycled the held books; the swap was invisible because the top book just changed under the same pose. The books now glide between fanned places as the wheel turns, so the new top comes up and the old top goes under.
- **Hands full.** The crosshair turns grey (16 px) when aiming at a book with full hands, and so does the book's hover outline. Clicking or pressing E on it now punches and shakes the reticle (`Crosshair.Reject`), shakes the stack (`CarryInventory.Nudge`) and keeps "Hands full (n/n)  -  put a book down first" in the prompt for 1.8 s (`PlayerInteractor.NotifyHandsFull`). Not yet: a sound, and a text toast near the reticle (the prompt text is the notice).
- **Checked:** `tools/verify_lamplit_stack.cs` (fan, newest on top, lower anchor, level gaze out of view, peek at 28 degrees, every book visibly apart at 40 degrees, wheel both ways, nudge, reject, grey reticle and outline, pull-back, placing) and all other suites; 126 unit tests. Renders: `tools/shots_stack2.cs`.
- **Not verified live:** the wheel itself (the Input System scroll value cannot be injected while the Editor is unfocused), the feel of the glide and the shake.
- **Stack order bug fixed (2026-10-19, found by Kolby).** The stack place was computed as `(index - current) mod n`, which put the OLDEST pick-up second and the newest-but-one last (after A, B, C the stack read C A B). It is now `(current - index) mod n`: newest on top, then each earlier pick in order (C B A). One wheel step down (or R) sends the top book to the bottom and brings the next one up; one step up brings the bottom book to the top. The first suite only checked that heights descend by place, so it could not see this; `verify_lamplit_stack.cs` now sorts the held books by their real height after each of four pick-ups and after each wheel step and compares the names.

### Held stack follows the view (2026-10-19, after Kolby's play test)

- **Problem.** At the middle and top shelves the held stack was not visible (it was anchored to the body below the view, and near a case the pull-back pushed it out of the view too). Research: `Docs/unity-practices/04` section 6 (Librarian's stack moves with the view and always shows its corner in the lower right).
- **Now.** The stack anchor follows the camera pitch by 0.7 (looking down: it rises) and 0.95 (looking up: it stays in the corner), at (0.40, -0.27, 0.52) from the eye. Near a bookcase or wall it shrinks toward the eye down to 0.4 (a box cast of the stack's footprint), so it never pokes through a shelf and never leaves the view; the scene's Cinemachine near plane is 0.1 (was 0.2). The values are set by `LamplitBuilder.ApplyPlayerScale` (serialized values do not follow a changed default).
- **Checked:** `tools/verify_lamplit_stack.cs` (in frame at level, looking up 35, looking down 40; no overlap with the case at its closest approach for pitches -35, -15 and 0; full size in the open) and every other suite; 126 unit tests. Renders: `tools/shots_stack3.cs`.
- **Not verified live:** the feel in motion. At 0.4 m from a shelf the stack is a small corner; if it should be larger there, lower `minStackScale` is not an option (the near plane); raise the offset or accept a little clipping.

### The apron and the shoes: real models, taken (not carried), and a discovery dialog (2026-10-19)

- **Models** (`tools/blender/make_finds.py`, textures from `make_find_textures.py`): the apron (heavy indigo canvas twill, leather-bound patch pockets, mended patches, brass buttons, linen tapes, a paper tag on string; 1,500 triangles) and a pair of mended brown leather shoes with felt soles, cotton laces and a tag (5,600 triangles), 7 materials each. They are built lying flat like a book and normalised to a unit box, so the game scales them by their real sizes (the apron 52 x 37 cm, the shoes 29 x 20 cm); the collider is the bounding box. Table pose (apron flat, shoes standing on their soles) and hang pose (apron on the hook, shoes on the rack, front toward the player) are fields on the find's `BookDefinition`. The desk's hook peg is longer and the anchors moved so the apron hangs by its neck loop and the shoes stand on the rack. The other three finds are still cube proxies.
- **Taken, not carried (Kolby: "carrying the apron looks stupid").** Aiming at the apron or the shoes shows "Press (E) or click to take it" (also with full hands); taking one never puts it in the hands: it is marked found and installed at its desk spot at once, so the boost works immediately (`CampaignController.CollectFind`, `PlayerInteractor.PickUp`), and it hangs there when the player gets back to the desk. The admin menu's Give does the same (silently). The glasses, shelf list and pull list still go through the old carry-and-install flow until they get models.
- **Discovery dialog** (`FindDialog`, `FindPreview`): the first time a find with a model is taken, the game pauses and a parchment card comes up: a turning 3D view of the item (a small photo studio 2 km above the world, its own camera and short-range lights, rendered into a 1024 px texture; nothing in it reaches the game and no layer or setting is touched), a heading ("Uncle Archie's apron"), a few lines in the player's voice about how it helped him, "What it gives you" ("Carry 2 more books"), what happens next, and an E key hint. Closed with E, Enter, Space, Esc or a click. Texts live in `CampaignCatalog` (`FindDef.Title`, `Story`, `Hint`), written for all five finds; the three not yet modelled have no dialog. Previews: `tools/finds_preview/`.
- **Checked:** `tools/verify_find_meshes.cs` (unit boxes, 7 submeshes in material order, felt sole under the upper, both resting on the table), the rewritten `verify_lamplit_finds.cs` and `verify_lamplit_admin.cs` (taking installs and opens the dialog with the right text and pauses the game; closing resumes; the glasses still use the carry flow), one more unit test (every find has a title, story and hint; no anachronisms), all other suites; 127 unit tests.
- **Caveats.** The dialog's fonts are Windows system fonts (Georgia): replace with an open-licence face before release (the story text is meant to become Archie's handwriting, Docs/15 section 8.7). The paper is generated in code. The Input System ignores clicks while the Editor is unfocused, so the finds suite installs the glasses directly in that case (it says so). Not verified live: how the dialog feels (the 0.3 s fade-in, the turn speed) and the benefit line when abilities are tweaked later.
