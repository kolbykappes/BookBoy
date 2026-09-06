# Build Status

> Keep this file current — update it as milestones from
> [05-implementation-plan.md](05-implementation-plan.md) land, so it always reflects what's
> actually in the repo, not what's planned.

**Last updated:** 2026-09-06 (digested from project brief; matches repo state as of commit
`c66cf16`).

## Completed

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

- No project-specific script folders or gameplay script architecture.
- No book materials, meshes, prefabs, or ScriptableObject data.
- No book category system.
- No pickup interaction.
- No held-object/carry state.
- No shelf slots or correct-placement validation.
- No UI count or completion screen.
- No game audio or feedback effects.
- No Unity MCP / coding-agent integration.

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
```

Current room is intentionally greyboxed. It should remain primitive until book scale, player
interaction distance, and shelf readability have been validated.
