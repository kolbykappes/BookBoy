# 03 - Eye height, CharacterController capsule, crouch, camera height, reach

Researched 2026-10-18. Web research plus read-only inspection of the repo's Starter Assets files. Unity was not opened; scene values are NOT verified live.

Tags: **[fetched]** = page body read; **[search-only]** = only a search-result summary seen; **[repo]** = read from files in this repo; **[derived]** = my arithmetic; **UNSOURCED GUESS** = could not source.

## 1. Question

What are typical eye heights and CharacterController capsule settings (height, radius, center, skin width, step offset, slope limit) in Unity first-person games and in the Starter Assets defaults? What should change for a short (child-sized) player? How do crouch, camera height inside the capsule, and interaction reach norms relate to eye height?

## 2. Findings

### 2.1 Unity's own CharacterController guidance (Unity 6000.6 manual)

- "You can modify the Height and Radius to fit your Character's mesh. It is recommended to always use around 2 meters for a human-like character." Step Offset "between 0.1 and 0.4 for a 2 meter sized human". Slope Limit "should not be too small. Often using a value of 90 degrees works best." [fetched] https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/character-control-section/class-character-controller.md
- Skin Width: "one of the most critical properties"; if the character gets stuck it is most likely too small; "good practice to keep your Skin Width at least greater than 0.01 and more than 10% of the Radius". Min Move Distance: keep at 0. Step Offset "should not be greater than the Character Controller's height or it will generate an error". Same URL.
- Height changes scale "the collider along the Y axis in both positive and negative directions" (so shrinking height moves the bottom and top about the center; fix `Center` to keep feet planted). Center offsets the capsule "and won't affect how the Character pivots". Same URL.
- Changing inspector properties recreates the controller and loses existing trigger contacts until it moves; queries (raycast) against the controller capsule "might shrink by a small factor". Same URL.
- Capsule dimensions are also multiplied by Transform scale for CapsuleCollider (radius and height "scaled by the transform's scale"); so do not scale the player root. [search-only] https://docs.unity.cn/kr/ScriptReference/CapsuleCollider-radius.html

### 2.2 Starter Assets FirstPersonController defaults [repo]

- Capsule (`PlayerCapsule.prefab`): Height 2.0, Radius 0.5, Center (0, 0.93, 0), Slope Limit 45, Step Offset 0.25, Skin Width 0.02, Min Move Distance 0. Camera root `PlayerCameraRoot` local Y 1.375. `MoveSpeed` 4, `SprintSpeed` 6, `JumpHeight` 1.2, `Gravity` -15, `GroundedRadius` 0.5 ("Should match the radius of the CharacterController"), `GroundedOffset` -0.14. Files: Assets/Starter Assets/Runtime/FirstPersonController/Prefabs/PlayerCapsule.prefab and Scripts/FirstPersonController.cs. Camera prefab: near clip 0.2, FOV 40 (PlayerFollowCamera.prefab).
- So the stock rig is a 2 m tall, 1 m wide capsule (width/height 0.5) with the camera about 1.4 to 1.5 m above the floor, and about 0.55 m below the top of the capsule. [derived from the numbers above; the repo's own scale note says the controller settles 0.09 m above the floor, Docs/09-scale-reference.md]
- The controller script has **no crouch** (only `InputSystem_Actions.inputactions` mentions crouch). [repo, Grep]

### 2.3 Typical dimensions in other engines/games

- Level Design Book table: Unity bounding box 1.0 x 1.8 m (or 2.0), eye 1.5 to 1.7 m; Unreal 60 x 176 cm, eye 152 cm; Quake/Source 32 x 72 in, eye 64 in; real US adult 20 x 69 in, eye 66 in. "Collision boxes are usually larger than the character model, for stable movement." "In-game eye height is often lower than real eyes. Many FPS games place the camera near the neck or chest." "Camera height strongly affects how fast and large the world feels, so finalize it early." [fetched] https://book.leveldesignbook.com/process/blockout/metrics
- Source (fan tutorial site, 16 units = 1 ft): standing 72 high, crouching 36, eye standing 64, eye crouching 32, width 32, max step 18, slope 45 degrees, door 48 x 108, hallway width 64. [fetched] https://worldofleveldesign.com/categories/sourcesdk-authoringtools/hammer-source-player-scale-world-dimensions.php
- Quake: hull 32 x 32 x 56, view height 46, max step 18, no crouch; "1 Quake unit = 1 inch, but not really". [fetched] https://book.leveldesignbook.com/process/blockout/metrics/quake
- Derived ratios [derived from the above]: eye / capsule height = 0.89 (Source), 0.86 (Unreal), 0.83 to 0.94 (Unity table), 0.72 (Starter Assets: 1.445 / 2.0 using a 0.07 to 0.09 floor offset); width / height = 0.44 (Source), 0.34 (Unreal), 0.5 (Starter Assets); step height / capsule height = 0.25 (Source/Quake 18/72), 0.125 (Starter Assets 0.25/2.0).
- Unreal reference: character 180 cm, doors 110 to 140 x 210 to 230, steps 15 high x 30 deep; the page gives no eye height, crouch height or reach. [fetched] https://worldofleveldesign.com/categories/ue5/guide-to-scale-dimensions-proportions.php
- Hallway minimum "at least double the player width, and even then may feel tight." [fetched, Level Design Book metrics page]

### 2.4 Child-sized players

- Stature data: 6 to 7 y mean 122.7 cm, 7 to 8 y 128.6 cm, 8 to 9 y 132.5 cm; a sample of 3 to 6 year olds with mean age 5.03 had mean height 112.5 cm. Population not stated. [search-only] https://pmc.ncbi.nlm.nih.gov/articles/PMC6530363/table/T5 and https://www.frontiersin.org/articles/10.3389/fped.2025.1655087/full
- I found no table of standing eye height by age. UNSOURCED GUESS: eye height is about 0.10 to 0.12 m below stature for a child (the repo's scale note uses 0.12 m below the head top for an adult, unsourced). That puts the project's 1.15 m eye at a child of about 1.25 to 1.27 m, roughly 7 to 8 years old (derived from the two sources above plus the guess).
- No source states how a game should size a capsule for a short player. Unity's only rule is absolute (about 2 m human, skin width above 10% radius, step 0.1 to 0.4 for 2 m). For a shorter player the usual approach is to scale all three together (height, radius, step) so ratios stay near the engine defaults. UNSOURCED GUESS for the specific numbers.

### 2.5 Crouch

- The Source numbers above (standing eye 64, crouching 32 in a 72/36 hull) show a classic crouch halves both hull and eye height. [fetched, WoLD link]
- Unity's CharacterController has no crouch API; you change `height` and `center` (and raise/lower the camera) yourself, remembering height scales about the center (2.1). Raising back up needs a headroom check; I found no Unity doc for it. UNSOURCED GUESS: sphere/capsule cast upward before standing.
- Witch Cabin Cleanup (reference game) exposes "toggle crouch/sprint" in settings (project note, Docs/competitive/16-witch-cabin-cleanup.md line 389). Not a physics source.

### 2.6 Interaction reach

- Witch Cabin Cleanup: pick-up and place reach is **300 cm, upgradable to 350 cm** (project note citing the game's ledger, Docs/competitive/16-witch-cabin-cleanup.md line 41, not re-verified on the web). That is the only reach figure I found for a reference game.
- Several tidy-up games sell "Longer Reach" as an upgrade (project notes: Docs/competitive/04, 07, 08), which implies a short default is normal and extension is a progression lever.
- I could not source typical first-person interaction distances (m) relative to eye height from any engine doc or talk; search results were only implementation threads (ray length is a parameter; aim-based raycast is the first-person norm, range-radius the third-person norm per an asset manual). [search-only] https://s2powered.com/copiasito/Manual_2023/MANUAL/picking.htm . UNSOURCED GUESS: first-person "use" ranges of about 1.5 to 3 m, i.e. 1x to 2x eye height for adults.

## 3. Numbers and rules of thumb

| Rule | Value | Source |
|---|---|---|
| Human-like capsule height | about 2 m (Unity manual), 1.8 m (Level Design Book table) | 2.1, 2.3 |
| Unity eye height range | 1.5 to 1.7 m | Level Design Book |
| Eye height as share of capsule height | 0.83 to 0.94 in the tables; Starter Assets 0.72 | derived |
| Capsule width | 0.34 to 0.5 of height | derived |
| Skin width | above 0.01 and above 10% of radius | Unity manual |
| Step offset | 0.1 to 0.4 m for a 2 m human; below capsule height | Unity manual |
| Slope limit | "often 90 degrees works best" (Starter Assets 45) | Unity manual |
| Hall width | at least 2x player width | Level Design Book |
| Crouch | height and eye about 50% (Source) | WoLD |
| Reach in a reference game | 3.0 m, 3.5 m upgraded | project note, Witch Cabin ledger |

## 4. Confidence and unknowns

- **High**: Unity CharacterController rules and the Starter Assets numbers (read from the manual and the repo). **Medium**: ratios across engines (one book page and one fan tutorial each; Source "units" are loose). **Low**: anything about child-sized capsules, crouch headroom and reach norms (no authoritative source).
- Unknown: the live values on the Lamplit player. `LamplitBuilder.ApplyPlayerScale` changes camera root, speed, reach and FOV but not the capsule, and I did not open the scene; the evidence is that `Assets/Scenes/Lamplit.unity` has no property overrides for capsule height, radius, center, step or skin (grep), and the only `CharacterController` use in `Assets/_Project` is the teleport in `LamplitBuilder.Hub.cs` (disable, move, enable), so prefab defaults likely still apply. Verify in the Editor before acting.
- Not found: a Unity talk on tuning CharacterController for non-adult proportions; eye height by age.

## 5. Implication for BookBoy

- **Where the current setup looks wrong (probably):** the player is visually a 1.15 m-eye child but physically a Starter Assets adult-sized capsule (2.0 m tall, 1.0 m wide). Consequences: the camera sits about 0.87 m below the top of the capsule [derived: top is about 2.02 m, eye 1.15 m]; the player cannot get closer than about 0.5 m plus skin (0.52 m) to any wall or shelf (a book is 17% of screen height at 1.5 m but 50% at 0.5 m, so this radius is silently the closest viewing distance; see file 02); the 1.0 m wide body must pass 1.5 m doors, leaving about 0.25 m each side; and low gaps under 2 m will block a player whose eyes are at 1.15 m. The eye-to-door ratio is fine; it is the body that is off.
- **Where it looks right:** camera pitch pivots at the camera root (eye), step 0.25 and skin 0.02 are within Unity's stated ranges, move speed 3.2 and sprint 4.8 scaled with the smaller player.
- **Smallest change if the above is confirmed:** set the capsule to a child body once, in the Player prefab override or the builder: Height about 1.3 (eye 1.15 plus about 0.12 to 0.15 below the top), Radius 0.3 to 0.35, Center Y = Height/2 (about 0.65), Skin Width at least 0.033 (10% of radius), Step Offset about 0.2 (keep below Height; 0.1 to 0.4 stays valid), and set `GroundedRadius` to the same radius (script tooltip says it should match). Then lower the camera root so eye remains 1.15 m above the floor. Check doors, aisles between containers and the closest viewing distance by eye. These exact numbers are UNSOURCED GUESSES derived from the engine ratios in 2.3; the ratios are sourced, the child values are not.
- **If you do not want to change the capsule:** at least raise the near clip from the default only if clipping appears; with radius 0.5 the camera never gets within 0.2 m of a wall except when leaning.
- **Do not add crouch yet.** Starter Assets does not include it, the game is a sorting game with shelf boards at 0.96 m and 1.68 m and an eye line of 1.15 m already inside the band; add it later via `height` and `center` changes with an upward headroom check.
- **Reach 2.8 m** is in line with the only reference figure (3.0 m in Witch Cabin Cleanup) and is 2.4x the eye height; keep it, and treat "reach" as an upgrade lever like the reference games.
