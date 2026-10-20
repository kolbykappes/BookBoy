# 30 - Sprint FOV kick (first person, Cinemachine 3)

Researched 2026-10-19 (web search only; no source gives a standard angle, so the numbers below are **GUESSES** to be tuned by feel). Related: file 23 (FOV: 90 horizontal = 58.7 vertical default, slider 70 to 110 horizontal).

## 1. Question

Kolby: "some games play with FOV to make your perspective shift while sprinting. Try that." How, and what are the comfort and accessibility risks?

## 2. Findings

- Sprint is already in the game: the Starter Assets controller reads a Sprint action (Left Shift on keyboard, left trigger on pad) and moves at `SprintSpeed` (6 m/s against 4 walking; the apron and shoes upgrades scale both). Source: the project's own `StarterAssets.inputactions` and `FirstPersonController.Move`.
- Games treat the sprint FOV change as a comfort-sensitive option: Avatar: Frontiers of Pandora has a separate sprint-FOV toggle (on by default, no slider) and one guide notes players often feel a widened sprint FOV makes movement seem slower; Minecraft mods let players scale sprint, speed, aiming and other FOV changes from 0 to 100 percent; a motion-sickness mod switches FOV effects off and sets FOV to 90; a Silent Hill 2 first-person mod removed the sprint FOV switch. https://digistatement.com/avatar-frontiers-of-pandora-how-to-change-fov/ , https://modrinth.com/mod/custom-fov-illusive , https://modrinth.com/project/Xm81g9Ce , https://www.nexusmods.com/silenthill2/mods/383 (search-result summaries; I did not open the pages)
- Cinemachine 3 has no built-in smooth FOV change; a forum thread suggests doing it yourself (SmoothDamp on a float written to the lens each frame), or blending between cameras. The lens is `CinemachineCamera.Lens` (a struct: read it, change `FieldOfView`, write it back, as the builder already does). https://discussions.unity.com/t/how-to-smoothly-change-the-value-of-a-single-virtualcamera/1551006 , https://docs.unity.cn/Packages/com.unity.cinemachine@3.0/api/Unity.Cinemachine.LensSettings.html
- No source gives a kick angle or ramp time. **GUESS:** about +6 degrees vertical (58.7 to 64.7, about +7 horizontal at 16:9), eased in over about 0.2 s and out over about 0.3 s.

## 3. Decision

1. The kick follows the real horizontal speed, not the key: 0 at walking speed, full at sprint speed (`BookBoy.Rules.SprintFovRules.Kick`, engine-free and unit-tested), so holding Shift while standing still or while blocked by a wall does nothing, and the ramp matches the controller's own acceleration.
2. A `SprintFov` component on the player writes `base FOV + kick` into the scene's Cinemachine lens every frame, smoothed with `Mathf.SmoothDamp` (0.16 s). The base FOV is whatever the lens had at start (or what a settings menu sets through `SetBaseFov`), so the FOV slider (file 23) stays the single source of the base value.
3. Comfort: it has a strength from 0 to 1 (`strength`, default 1) so the settings menu (backlog T-008) can offer an off switch and a slider; nothing else (no blur, no head bob, no vignette).
4. No project setting, layer or renderer is touched.

## 4. Unknowns

How +6 degrees feels (tune `maxKickDegrees`); whether the held stack should counter the kick (it is in view space, so it appears to shrink slightly at full kick; check by eye).
