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
