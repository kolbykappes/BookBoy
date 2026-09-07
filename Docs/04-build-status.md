# Build Status

> Keep this file current — update it as milestones from
> [05-implementation-plan.md](05-implementation-plan.md) land, so it always reflects what's
> actually in the repo, not what's planned.

**Last updated:** 2026-09-06 (Phases 1-5 landed via Unity MCP; expanded to 8 of 12 books/slots).

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
