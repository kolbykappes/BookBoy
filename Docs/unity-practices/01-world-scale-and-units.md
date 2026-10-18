# 01 - World scale and units (scale the world, the player, or the camera?)

Researched 2026-10-18. Web research only; Unity was not opened. Project is Unity 6000.6, URP, Cinemachine 3, Starter Assets FirstPersonController.

Source-reliability tags used below: **[fetched]** = I opened the page and read the quoted text; **[search-only]** = I only saw a search-result summary, not the page body; **[derived]** = my own arithmetic, no outside source; **UNSOURCED GUESS** = could not source.

## 1. Question

BookBoy's owner wanted books to look big and readable. The previous assistant scaled the whole world (books, shelves, rooms, doors) by 1.6x (`LamplitBuilder.WorldScale`) while the player stays child-sized (eye 1.15 m). Was changing the world a sound choice, or should the player, camera or book proportions have been changed instead? What does it do to physics (gravity, solver, contact offset, CharacterController skin and step), to light falloff and to shadows?

## 2. Findings

### 2.1 Unity's unit convention

- Unity's physics assumes 1 world unit = 1 metre. Rigidbody scripting reference: "The default gravity settings assume that one world unit corresponds to one metre of distance." If a large scale is used for objects that are supposed to be small, they "will appear to fall very slowly" because physics treats them as very large objects; "be sure to keep your objects more or less at their scale in real life" (example: a car about 4 units). [fetched] https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody.html
- Transform manual: "By default, the physics engine assumes that one unit in world space corresponds to one meter." It says the mesh's size in Unity (and so the Transform scale) "is important, especially during physics simulation", and recommends not adjusting Scale on the GameObject for sizing (fix size at import/model). [fetched] https://docs.unity3d.com/6000.0/Documentation/Manual/class-Transform.html
- Older FBX importer manuals say the physics system expects 1 metre = 1 unit and that, for correct physics, the model should be correctly scaled in the modelling application; Scale Factor on import is the fallback. [search-only, 5.x/2017 manuals] https://docs.unity3d.com/462/Documentation/Manual/FBXImporter-Model.html
- Physics Settings: gravity is "defined in world units per seconds squared"; Default Contact Offset is 0.01 by default; if you raise gravity "you might need to also increase the Default Solver Iterations". The page states no scale guideline. [fetched] https://docs.unity3d.com/6000.0/Documentation/Manual/class-PhysicsManager.html
- Community: very small rigidbodies (0.05 scale vehicle blocks) jitter and fall through floors; scaling up fixes it; the 1 unit = 1 m assumption also underlies Doppler. Anecdotal. [search-only] https://discussions.unity.com/t/hololens-physics-of-small-objects/673759 and https://discussions.unity.com/t/does-scale-matter-as-long-as-i-keep-it-the-same/381947
- Related project research on small-object physics: see `14-physics-small-objects-and-settings.md` in this folder.

### 2.2 How real games choose scale

- The Level Design Book: "Build at the player's scale", "Don't follow real world scale", game scale is often inconsistent with reality (Doom player about 1 m wide, TF2 characters effectively very tall because "inches" are used loosely) and still feels right; judge scale in a blockout, "metrics are not magic". Camera height "strongly affects how fast and large the world feels, so finalize it early." [fetched] https://book.leveldesignbook.com/process/blockout/metrics
- Same page, engine defaults: Unity bounding box 1.0 x 1.8 m (or 2.0), eye 1.5 to 1.7 m; Unreal 60 x 176 cm, eye 152 cm; Quake/Source 32 x 72 in, eye 64 in; Unity door 1.25 x 2.5 m, minimum hall 2.0 m. [fetched] same URL.
- Source units: 1 foot = 16 units, player 72 high, eye 64, doors 48 x 108, hallway width 64, wall height 128 (fan-run tutorial site). [fetched] https://worldofleveldesign.com/categories/sourcesdk-authoringtools/hammer-source-player-scale-world-dimensions.php . Quake itself says "1 Quake unit = 1 inch, but not really. There is no consistent scale." [fetched] https://book.leveldesignbook.com/process/blockout/metrics/quake
- Takeaway from these: oversized or stylised scale is normal and fine in shipped games; what matters is that player, doors, props and physics agree with each other. No source says "never exceed life size in Unity".

### 2.3 Scale the world vs the player vs the camera (what actually differs)

- Rendering a perspective image depends only on ratios: object size over distance, and the field of view. Uniformly scaling world, eye position and everything else by the same factor gives an identical picture. [derived; basic projective geometry] Therefore "world x1.6 with the same player" is the same picture as "world x1 with a player 1/1.6 the size (eye 0.72 m, reach 1.75 m, speed 2.0 m/s, capsule radius 0.31)". What differs between the two is only the absolute-unit machinery: gravity and fall times, contact offset and solver tolerances, light range and falloff, shadow map density per metre, CharacterController skin and step in metres, near clip in metres, audio Doppler. [derived, backed by 2.1 and 2.4 below]
- Changing only the camera (narrower FOV, or standing closer) changes on-screen size without changing any of those, but changes the perspective (see `02-first-person-fov.md`).
- Perspective is set by camera position, not the lens: "linear perspective changes are caused by distance, not by the lens per se". [fetched] https://en.wikipedia.org/wiki/Perspective_distortion . So the lens option and the distance option are not interchangeable in look, only in size on screen.
- For small player vs big world, the Unity-side cost of option B (shrink the player to 0.72 m eye, leave books at life size 0.24 m) is a tiny CharacterController radius and skin width, near clip near 0.03, and small props, all in the zone Unity forums report as troublesome; option A (scale props up 1.6x, player fixed) moves the props toward, not away from, the sizes Unity's physics defaults were tuned for. This is an inference from 2.1; no source compares the two directly.

### 2.4 Physics and lighting consequences of a 1.6x world

- Gravity and fall: Unity says objects scaled up look like they fall slowly (2.1). A body 1.6x taller than life falls its own height in sqrt(1.6) = 1.26x the time at the same g. [derived] Starter Assets already uses a player gravity of -15 instead of -9.81 ("The character uses its own gravity value") in `FirstPersonController.cs` (read from the repo, Assets/Starter Assets/Runtime/FirstPersonController/Scripts/FirstPersonController.cs). Whether to also raise `Physics.gravity` for thrown or dropped books is a feel decision; no source gives a number. UNSOURCED GUESS: -9.81 x 1.6 = -15.7 would make 1.6x books fall like life-size books.
- Contact offset: default 0.01 per collider (2 cm contact distance) [fetched, class-PhysicsManager link above]. Relative to a 0.07 to 0.14 m thick spine this is a slightly smaller fraction than on a 0.045 m life-size spine, so scaling up helps, not hurts. [derived]
- CharacterController: Unity's rules are expressed in absolute metres for a 2 m human: step offset 0.1 to 0.4, skin width above 0.01 and above 10% of radius (default capsule 2 m). [fetched] https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/character-control-section/class-character-controller.md . None of this changes when only props are scaled. If the capsule itself were scaled with the transform, radius and height scale with it (CapsuleCollider docs) and skin width would need re-tuning. [search-only] https://docs.unity.cn/kr/ScriptReference/CapsuleCollider-radius.html
- Light falloff: URP point and spot lights use a distance attenuation tied to the light Range in world units (the URP custom-falloff guide's code normalises distance by range and applies a quadratic falloff and smoothstep). The default formula's exact form is not given on that page. [fetched] https://docs.unity3d.com/Manual/urp/lighting/custom-lighting-change-light-falloff.html . Shared Core RP code uses a physically based inverse-square term times a range window. [search-only, third-party mirrors of CommonLighting.hlsl]. Consequence [derived]: if lights stay at the same position-to-surface distances in metres, scaling the room changes nothing; if the lights and surfaces are moved apart by 1.6x without raising Range, the same intensity lights a surface 1/1.6^2 = 0.39x as strongly (inverse square) and Range must grow by 1.6.
- Shadows: "Unity spreads the shadow map over the area defined by the Max Distance property. The higher the value, the lower the pixel density of the shadow map." [fetched] https://docs.unity3d.com/Manual/urp/shadow-resolution-urp.html . Bigger rooms therefore need either more shadow resolution or a shorter Max Distance for the same sharpness. Cinemachine's Lens tooltip adds that raising the camera near plane "will also increase your shadow resolution". [fetched] https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/api/Unity.Cinemachine.LensSettings.html
- Bias values are chosen by eye, not computed; "there may be no one set of bias settings that avoids both shadow acne and peter panning". [search-only] https://docs.unity3d.com/Manual/urp/shadows-troubleshooting-urp.html
- Lightmap resolution is expressed in texels per unit, so a 1.6x larger surface needs 1.6x the lightmap area for the same density. [derived; Lighting Settings page only defines the unit] https://docs.unity3d.com/6000.0/Documentation/Manual/class-LightingSettings.html

## 3. Numbers and rules of thumb

| Number | Source |
|---|---|
| 1 unit = 1 m; keep objects near real-life size | Rigidbody and Transform manuals (links in 2.1) |
| Unity default capsule about 1.8 to 2.0 m, eye 1.5 to 1.7 m | Level Design Book (2.2) |
| Unity manual: use about 2 m for a human-like CharacterController; step 0.1 to 0.4; skin width above 0.01 and above 10% of radius | class-character-controller.md (2.4) |
| Default Contact Offset 0.01 | class-PhysicsManager (2.1) |
| Fall time of an object scaled by s, same g, same body-length drop: x sqrt(s) (1.6 -> 1.26) | derived |
| Illuminance at a fixed light intensity falls as 1/d^2 (1.6x distance -> 0.39x) | derived (inverse-square; URP details in 2.4) |
| Real doors: Unity 1.25 x 2.5 m, Unreal 1.1 x 2.2 m, Source 48 x 108 in; hall minimum "at least double the player width" | Level Design Book, WoLD |

## 4. Confidence and unknowns

- Confidence: **high** that Unity physics assumes metres and that scale of objects, not the camera, is what physics cares about; **high** that the picture depends only on ratios (basic geometry); **medium** on the lighting and shadow consequences (documented in principle, not measured for this project); **low** on any "best" scale choice, because no source compares world-scaling to player-scaling for a Unity first-person game.
- Not found: any Unity page giving a minimum or maximum object size for PhysX; any shipped-game post-mortem about scaling the world 1.6x for readability; the precise default URP attenuation formula in the Unity 6000.6 docs.
- Version notes: the 6000.6 manual moved to https://docs.unity.com/en-us/engine/6000.6/manual/...md URLs; several older docs.unity3d.com pages 404 or redirect. Everything physical above is unchanged across versions I could see (5.x to 6.x) but the CharacterController text for 6000.6 was read directly.

## 5. Implication for BookBoy

- **Scaling props 1.6x instead of shrinking the player was a defensible, small-risk choice.** It keeps the player capsule, step offset, skin width and near clip in sensible absolute metres and moves book sizes toward, not away from, Unity's tuned range. Do not undo it.
- **What it did not do by itself:** make the world "feel closer". It only changes ratios; the previous finding that a wider lens alone shrinks everything on screen is correct (see file 02).
- **Smallest follow-ups, in order:**
  1. Re-check lights in the rooms: ranges and intensities should grow with the 1.6x room sizes (Range x1.6, intensity roughly x2.5 if lights moved with the walls). Eyeball it; no source gives a formula for matching look.
  2. Check shadow Max Distance and cascade count against the bigger rooms; shorter Max Distance gives sharper shadows near the player.
  3. Decide gravity for thrown and dropped books by feel (candidate -15.7); log the decision with the `14-physics-small-objects-and-settings.md` findings.
  4. Leave `Physics.defaultContactOffset` at 0.01; the books are now thicker relative to it.
- **Where the current setup looks wrong:** the world is scaled but the CharacterController capsule seems still to be the Starter Assets adult capsule (radius 0.5, height 2.0, center y 0.93, per the prefab); see file 03. The player is child-sized visually but not physically.
- **Where it looks right:** the "one number" `WorldScale` pattern in `LamplitBuilder`, and keeping items at roughly human-plus-60% size rather than 3x.
