# Build Status

> Keep this file current — update it as milestones from
> [05-implementation-plan.md](05-implementation-plan.md) land, so it always reflects what's
> actually in the repo, not what's planned.

**Last updated:** 2026-09-06 (Phases 1, 2, and 3 landed via Unity MCP).

## Completed

- **Unity MCP connected.** `Edit → Project Settings → AI → Unity MCP Server` bridge is running;
  Claude Code is registered as a client via a local relay
  (`C:\Users\<user>\.unity\relay\relay_win.exe`, configured through `~/.claude.json`, not checked
  into the repo). 7 of 54 available tools are currently enabled — asset generation, console logs,
  scene/camera capture, and `RunCommand` (arbitrary C# execution in the Editor). GameObject/
  Prefab/Material-specific tool categories are not yet enabled; `RunCommand` covers that ground
  for now. See [06-agent-workflow.md](06-agent-workflow.md) for how it's used.
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
- Tested basic first-person movement and mouse look successfully.
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

- Only 4 test book cubes exist (one per category), not the full 12 — Phase 2 says expand only
  after playtesting scale/readability, which hasn't happened yet (see above).
- No book meshes/models beyond primitive cubes, no book prefabs yet (still loose scene objects,
  not prefabbed — reasonable for a 4-book visual proof, but worth prefabbing before scaling to 12).
- No pickup interaction.
- No held-object/carry state.
- No shelf slots or correct-placement validation.
- No UI count or completion screen.
- No game audio or feedback effects.

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
  Player
    PlayerCameraRoot
    Capsule
  MainCamera                  # Active Starter Assets rendering camera
  PlayerFollowCamera          # Active Cinemachine camera, targets PlayerCameraRoot
  ScatteredBooks               # Added in Phase 2
    Book_Alchemy
    Book_Astronomy
    Book_Beasts
    Book_History
```

Current room is intentionally greyboxed. It should remain primitive until book scale, player
interaction distance, and shelf readability have been validated.
