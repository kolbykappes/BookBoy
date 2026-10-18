# 14 - PhysX in Unity for small, light objects, and project-wide settings

Researched 2026-10-18 by web research only (no Unity Editor touched). Project: Unity 6000.6, built-in 3D physics (NVIDIA PhysX integration), Windows PC.
Every claim has a URL. Anything without a source is marked **UNSOURCED GUESS**. Statements labelled "derived" are my own arithmetic from sourced numbers, not a source's claim.

Doc-site note: the Unity 6000.6 manual lives at `docs.unity.com/en-us/engine/6000.6/manual/...` (append `.md` for raw text). Several older `docs.unity3d.com/Manual/...` URLs 404 now.

## 1. Question

What does PhysX in Unity do with small, light, thin objects (BookBoy books: boxes about 0.38 x 0.26 x 0.07 to 0.14 m, mass 0.4 kg in code, world 1.6x life size)? Specifically:

- effect of object scale and mass
- default contact offset and the smallest sensible object size
- solver and velocity iterations for stable stacks and piles
- sleep thresholds
- Rigidbody interpolation
- collision detection modes for fast-thrown thin books
- box collider vs mesh collider
- friction and bounce values for books on a floor
- `Time.fixedDeltaTime`
- whether to change project-wide physics settings or per-body settings

## 2. Findings

### 2.1 What the project currently has (read from ProjectSettings, read-only)

- `DynamicsManager.asset`: gravity -9.81, bounce threshold 2, default max depenetration velocity 10, sleep threshold 0.005, default contact offset 0.01, default solver iterations 6, default solver velocity iterations 1, queries hit triggers ON, queries hit backfaces OFF, adaptive force OFF, friction type 0 (Patch), solver type 0 (Projected Gauss Seidel). `TimeManager.asset`: fixed timestep 0.02, max allowed timestep 0.3333. All of these are stock defaults.
- `Book.BeginSolid` (Assets/_Project/Scripts/Books/Book.cs): mass 0.4, linear damping 0.05, angular damping 0.6, `CollisionDetectionMode.ContinuousDynamic`, `Interpolate`, a PhysicsMaterial (bounciness 0.05, dynamic friction 0.6, static friction 0.7, friction combine Maximum, bounce combine Minimum). Settle routine: destroys the Rigidbody after the book is under 0.05 m/s linear and 0.2 rad/s angular for 0.5 s.
- `LamplitBuilder.BakePiles`: proxy cubes with the same settings, `Physics.Simulate(0.02)` in an Editor preview physics scene, up to 3000 steps, stops early when every proxy `IsSleeping()`.

### 2.2 Defaults and what each setting means (Unity 6000.6 manual)

- Default Contact Offset is 0.01, "must be positive, and if set too close to zero, it can cause jitter"; colliders generate contacts when "their distance is less than the sum of their contact offset values". https://docs.unity.com/en-us/engine/6000.6/manual/unity-editor/editor-settings-reference/comp-manager-group/class-physics-manager.md
- Sleep Threshold: "If a Rigidbody's kinetic energy divided by its mass is below this threshold, it is a candidate for sleeping." The manual states no numeric default. Same URL. (0.005 is the value in this project's DynamicsManager; one third-party blog also calls it the default, https://bugnet.io/blog/fix-unity-rigidbody-sleeping-too-early, which cites no docs.)
- Default Solver Iterations: "advisable to change the property in case non-default `Time.fixedDeltaTime` is used, or the configuration is extra demanding." Default Solver Velocity Iterations: raises "accuracy of the resulting exit velocity after a Rigidbody bounce". Same URL. The scripting reference gives the global default as 6: https://docs.unity3d.com/ScriptReference/Physics-defaultSolverIterations.html (velocity default of 1 is from this project's asset; the docs pages I could read do not state it).
- Gravity note: "If you increase the gravity, you might need to also increase the Default Solver Iterations value to maintain stable contacts." Same URL.
- Bounce Threshold: below this relative velocity objects do not bounce; "also reduces jitter, so it is not recommended to set it to a very low value." Same URL.
- Default Max Depenetration Velocity: "the velocity that the solver can set to a body while trying to pull it out of overlap with the other bodies." Same URL. Per-body: https://docs.unity3d.com/ScriptReference/Rigidbody-maxDepenetrationVelocity.html
- Solver Type: Projected Gauss Seidel is the default; "Temporal Gauss Seidel ... offers a better convergence and a better handling of high-mass ratios ... It usually helps when you experience some erratic behavior during simulation with the default solver." Same URL. Also available: Enable Adaptive Force ("affects the way forces are transmitted through a pile or stack of objects, to give more realistic behaviour", off by default), Friction Type (Patch = "typically leads to the most stable results at low solver iteration counts"), Fast Motion Threshold. Same URL.
- Unity 6 still labels built-in 3D physics as "an integration of the Nvidia PhysX engine". The exact PhysX version (4.1 vs 5.x) is not stated in any page I could read. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-integrations.md and https://discussions.unity.com/t/physics-development-status-and-next-milestones-may-2025/1637988 (PhysX 5 is mentioned only as a possible future third-party backend). I used NVIDIA's older (3.4/4.x) docs as the nearest reference and flag PhysX 5.1 doc text separately.

### 2.3 Scale and mass

- "The default gravity settings assume that one world unit corresponds to one metre of distance." Objects scaled up "will seem to fall unnaturally slowly" and the docs advise keeping objects near real-life size. https://docs.unity3d.com/ScriptReference/Rigidbody.html
- Mass does not change fall speed ("mass does not affect how quickly an item falls under gravity"); mass defaults to 1 kg. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/rigidbody/class-rigidbody.md
- Unity rigidbodies do not auto-adjust mass or drag when you scale a prefab; you do it by hand (forum, community answer). https://discussions.unity.com/t/scaling-objects-with-rigidbodies/60517 (appeared in search results; not opened in full)
- Very small objects behave badly in practice: a developer with 2 cm dice got dice that never settle and about 10% failed rolls; tuning bounce threshold, sleep threshold and contact offset "improved realism but didn't produce consistent results"; the only suggested workaround was disabling gravity and applying a custom gravity from script. https://discussions.unity.com/t/problem-with-small-scale-objects-and-physics/660663 (community, 2017, no Unity staff reply)
- A HoloLens thread with 5 cm blocks: lowering Fixed Timestep helped. https://discussions.unity.com/t/hololens-physics-of-small-objects/673759 (community)
- Derived (my arithmetic, not a source): BookBoy books are 7 to 14 cm thick, far above the 2 cm dice, and the world is 1.6x life, so objects are "a bit large" by the 1 unit = 1 m convention, not small. The cost is that gravity feels about 1.6x too weak for the visual size (fall time scales with the square root of 1.6, about 1.26x longer). **UNSOURCED GUESS**: a designer might raise gravity toward -15 to restore the feel; no source says what Unity or PhysX prefer here.

### 2.4 Contact offset and thin objects

- PhysX guide: contacts are generated when shapes are closer than the sum of their contact offsets; default `contactOffset` is 0.02 at default tolerance scale (so 4 cm contact distance); "use the smallest gap that still avoids jitter at your time step", larger gaps cost performance. https://archive.docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/AdvancedCollisionDetection.html Unity's own default is half that (0.01 per collider, 2 cm distance), see 2.2.
- PhysX best practices: for jitter from contacts flickering on and off, "try to increase the contact offset". https://archive.docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/BestPractices.html
- Thin/tiny objects: "It is therefore recommended that paper-thin/tiny objects should be avoided if possible"; they can exceed their CCD threshold even at rest and make the simulation drop time (stutter) for themselves and their island. Same AdvancedCollisionDetection URL. Relevant if sweep-based CCD is on (see 2.6).
- Unity issue tracker (closed Won't Fix): a rigidbody passed through a box collider whose Y scale was under 0.07 because the box was "too thin compared to the contact offset"; a contact offset increase on the thin cube (0.1 was "more than enough") worked. Versions tested 5.6 to 2018.1 alpha, so possibly outdated. https://issuetracker-mig.prd.it.unity3d.com/issues/3d-gameobjects-with-rigidbody-goes-throw-box-collider-if-collider-transform-y-scale-is-lower-than-0-dot-07 (page returned 502 when I fetched directly; this summary came from a search-result summary of that page)
- Per-collider contact offset exists (`Collider.contactOffset`), same semantics. https://docs.unity3d.com/ScriptReference/Collider-contactOffset.html and `Physics.defaultContactOffset` applies to newly created colliders only. https://docs.unity3d.com/ScriptReference/Physics-defaultContactOffset.html
- No source gives a numeric minimum object size for Unity PhysX. The PhysX tolerance scale `length` (1 for human-scale metric) is the nearest thing, but Unity does not document exposing it. https://docs.nvidia.com/gameworks/content/gameworkslibrary/physx/apireference/files/classPxTolerancesScale.html (seen in search results only)

### 2.5 Solver iterations, stacks and piles

- Unity: raise per-body iterations for "preventing jittering in stacks"; "This targeted approach is more efficient than increasing the global solver iteration count". `Rigidbody.solverIterations` overrides the global for that body only. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-optimization/cpu/rigidbody-solver.md
- Experiment (Unity 2018.2, PhysX 3.3.3, default settings, 1 m cubes): about 12 stacked boxes at default settings; 10 to 12 wobbled for thousands of frames; 20 clearly unstable; solver iterations were "the one factor that matters"; compound colliders did not help and cost about 20% step time; spawning boxes one-by-one reduced bouncing but not stability. Conclusion: counts needed are "probably not practical in a real game". https://www.noio.nl/2018/11/box-stacking/ and https://www.noio.nl/2018/11/stacking-more-boxes/ (single developer, old engine version)
- Same author: a longer timestep or higher gravity act like fewer solver iterations. https://www.noio.nl/2018/11/box-stacking/
- PhysX guide (3.4-era): iterations default to 4 position + 1 velocity; "If you find a need to use a setting higher than 30, you may wish to reconsider the configuration of your simulation." https://archive.docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/RigidBodyDynamics.html
- PhysX best practices: "Rigid bodies created in an initially overlapping state may explode" - limit with `setMaxDepenetrationVelocity`, "a value such as 3.0 as a starting point" (Unity exposes `Rigidbody.maxDepenetrationVelocity`; project default is 10). For piles/stacks that will not sleep, PhysX suggests the stabilization flag, "safe to enable for objects like debris". https://archive.docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/BestPractices.html **I found no source that Unity exposes the PhysX stabilization flag** - treat as unavailable, **UNSOURCED GUESS** that it is not exposed.
- Temporal Gauss Seidel: documented as better for high mass ratios and erratic behaviour (see 2.2), but Unity's issue tracker has a report "Rigidbody collisions are erratic when using the Temporal Gauss Seidel Physics solver type" (marked Won't Fix); no authoritative comparison for piles. https://issuetracker.unity.com/issues/5282/rigidbody-collisions-are-erratic-when-using-the-temporal-gauss-seidel-physics-solver-type (seen in search results only)
- Devlog: raising default solver iterations and velocity iterations with Adaptive Force on fixed stacking at a performance cost. https://renzrivero.itch.io/rainy-day/devlog/431009/rainy-day-stacking-gameplay-overhaul-camera-adjustments-devlog-4 (anecdotal, seen in search results only)
- Piles carry a start-up cost: PhysX keeps nearly everything awake until the solver decides a stack is stable. https://discussions.unity.com/t/how-do-i-get-stacked-rigidbodies-to-settle-down/19918 (community)

### 2.6 Collision detection modes for fast thin books

- Unity's recommended order: Discrete first; if fast objects tunnel, Continuous Speculative ("often the best choice for CCD", cheaper than the sweep modes); then Continuous; Continuous Dynamic only "as a last resort" with highest CPU that "increases with the number of potential interactions". https://docs.unity3d.com/Manual/physics-optimization-cpu-rigidbody-collision-modes.html (6.4 manual, fetched; same page for 6000.6 returned the older URL pattern as 404)
- Rigidbody component reference (6000.6) says Continuous Speculative is for when "collision accuracy is not important", and sweep CCD "is more computationally intensive than Discrete or Continuous Speculative". https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/rigidbody/class-rigidbody.md **Sources disagree in tone**: the optimization page calls Speculative "often the best choice", the component reference suggests it is the least accurate. They agree on cost order.
- Continuous Dynamic only sweeps against other Rigidbodies set to Continuous/Continuous Dynamic, not against Discrete ones. Same collision-modes page; also https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/rigidbody/class-rigidbody.md ("all other colliders, except for those that are set to Discrete").
- Speculative CCD: fewer resources, handles rotation, but can produce "ghost collisions" and "missed collisions" when the solver adds energy; sweep CCD is linear only (cannot catch rotation) and "CCD overhead increases quickly" when many fast bodies are close together. CCD is supported for Box, Sphere, Capsule colliders and is "intended as a safety net". https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/collision-section/collision-detection/continuous/speculative-ccd.md , https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/collision-section/collision-detection/continuous/sweep-based-ccd.md , https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/collision-section/collision-detection/continuous.md
- Unity says CCD "does not always deliver physically accurate collision results, so you might still consider decreasing the physics timestep frequency to make the simulation more precise" (same continuous.md page; the wording "decreasing the frequency" is Unity's, and it reads like a slip for "decreasing the timestep").
- Derived arithmetic: a book thrown at 10 m/s moves 0.2 m per 0.02 s step, nearly 3x the 0.07 m thickness, so a thin collider (shelf board, door) can be skipped by Discrete detection. Floor and thick walls are not at risk. This is why some CCD on thrown books is justified; piles dropping from above at about 1 to 5 m/s are much less at risk (**derived**, speeds depend on `PilePlanner`).

### 2.7 Sleeping

- Sleeping: below the threshold the body sleeps and is excluded from simulation; it wakes on collision or force. `Rigidbody.Sleep()` works only "if it is not in contact with an awake rigidbody". Avoid unnecessary `WakeUp`. Use Physics Debug to find bodies that won't sleep. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-optimization/cpu/rigidbody-sleeping.md , https://docs.unity3d.com/ScriptReference/Rigidbody.Sleep.html
- A Rigidbody can fail to wake when a static collider (no Rigidbody) is moved by Transform; then call `WakeUp`. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/rigidbody/rigidbodies-overview.md
- PhysX: "An actor goes to sleep when its kinetic energy is below a given threshold for a certain time"; wake triggers include touching an awake actor. https://archive.docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/RigidBodyDynamics.html (3.4-era; the "certain time" is not given a number there)
- Third-party advice (not official): lower global threshold to 0.002 rather than 0; set `sleepThreshold = 0` only on bodies that must always respond. https://bugnet.io/blog/fix-unity-rigidbody-sleeping-too-early (cites no docs). Note it points the opposite direction from "raise the threshold so piles sleep sooner".
- Note the manual's sleep value is energy per unit mass (kinetic energy / mass), so it is mass-independent: a 0.4 kg book and a 4 kg book sleep at the same speeds. This is from the Physics settings page above.

### 2.8 Interpolation

- Use only if you see jitter; default None. It lags one physics step ("delays the Rigidbody's pose by one physics update"). Interpolate suits varying velocity; Extrapolate suits constant velocity. When on, the physics system controls the transform: "follow any direct (non-physics) change to the transform with a Physics.SyncTransforms call". https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/rigidbody/rigidbody-interpolation.md
- Jitter appears when the fixed timestep is slower than frame rate (50 Hz physics vs 60 to 144 Hz rendering) and is most visible when the camera tracks the body. Same page.

### 2.9 Colliders and surface materials

- Box collider is cheaper than convex mesh collider; non-convex mesh collider cannot be on a non-kinematic Rigidbody; compound colliders cost more than one primitive. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-optimization/cpu/collider-types.md
- Physics Material defaults: dynamic friction 0.6, static friction 0.6, bounciness 0, both combine modes Average. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/collision-section/collider-surfaces/class-physics-material.md
- No source gives book-on-floor friction numbers. Real-world numbers for paper/cardboard or wood are not in any source I fetched: **UNSOURCED GUESS** that 0.4 to 0.7 is plausible. The project's 0.6/0.7 with Maximum combine is within the stock range.
- Trigger colliders: "Trigger colliders don't generate collision detection messages" and are not physically solid (see the collider-types interaction matrix). https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/collision-section/collider-interactions/collider-types-interaction.md

### 2.10 Fixed timestep

- Default 0.02 s (50 Hz). Smaller = "more accurate and stable simulation, but increases CPU usage"; larger = tunneling and jitter. Maximum Allowed Timestep guards against the spiral of doom. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-optimization/cpu/frequency.md
- `Physics.Simulate`: "Using step values greater than 0.03 is likely to produce inaccurate results"; pass a fixed step. https://docs.unity3d.com/ScriptReference/Physics.Simulate.html
- PhysX alternative to sub-stepping: "N simulation calls with a time-step dt/N and 1 solver iteration". https://archive.docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/BestPractices.html

## 3. Numbers and rules of thumb

| Item | Value | Source |
|---|---|---|
| Unity default contact offset | 0.01 per collider (sum 0.02 m at contact) | Physics settings page, 2.2 |
| PhysX default contact offset | 0.02 per shape at metric scale (0.04 m distance) | PhysX AdvancedCollisionDetection, 2.4 |
| Default solver iterations | 6 position, 1 velocity (Unity); 4 + 1 (PhysX default) | Unity scripting ref; PhysX guide |
| PhysX iteration ceiling advice | over 30 means rethink the setup | PhysX guide |
| Default sleep threshold | 0.005 (project asset; blog agrees) | project; bugnet.io (low authority) |
| Default bounce threshold | 2 in this project | project asset; Unity does not state a default on the page |
| Default fixed timestep | 0.02 | Unity frequency page |
| Simulate step ceiling | 0.03 s | Physics.Simulate ref |
| Default stack height (1 m cubes) | about 12 | noio.nl (2018 experiment) |
| Max depenetration velocity | PhysX suggests 3.0 as a starting point; project default 10 | PhysX BestPractices |
| Friction defaults | 0.6 / 0.6, bounce 0, Average combine | Unity Physics Material page |
| Tunnel risk, derived | speed x 0.02 s compared with thickness 0.07 m: over 3.5 m/s moves one thickness per step | derived arithmetic |

## 4. Confidence and unknowns

- **High**: Unity setting meanings and defaults, CCD mode costs, sleeping mechanics, interpolation rules, collider cost ordering (Unity 6000.6 manual).
- **Medium**: stack stability needs more iterations; sweeps cost CPU in crowded piles (documented qualitatively, no numbers for 100+ small bodies).
- **Low / unknown**:
  - The PhysX version Unity 6000.6 ships (4.1 vs 5.x). NVIDIA text I used is from 3.4/4.x-era and 5.1 pages.
  - Whether the PhysX stabilization flag is reachable from Unity.
  - A numeric minimum object size.
  - Whether a 0.07 m thick box has any issue with a 0.01 contact offset (the only evidence is the old, closed issue about a thin static collider).
  - Real-world friction values for books.
  - How the "certain time" in PhysX sleeping converts to seconds in Unity.
  - Whether Temporal Gauss Seidel helps 100-book piles (no source tests it).

## 5. Implication for BookBoy

Smallest change first. Rule: prefer **per-body** settings to project-wide ones (Unity says per-body solver iterations are "more efficient than increasing the global" count; the project guardrails also forbid editing ProjectSettings without approval).

**Looks right**
- Box colliders (cheapest dynamic option). Keep.
- Physics material 0.6/0.7 friction, low bounce, Minimum bounce combine. Inside Unity's stock range. Keep.
- Interpolate on the thrown book: legitimate (fast, camera can watch it, 50 Hz vs higher frame rate). It is irrelevant in the Editor bake (`Physics.Simulate`). Keep.
- Mass 0.4 on all books: mass doesn't change fall speed and sleep uses energy per mass, so it only matters for ratios, and ratios are 1:1 among books. Fine.
- Default contact offset 0.01 relative to 0.07 m thickness (14%) is fine as far as any source says; do not raise it project-wide.
- Bake in a preview physics scene with `Physics.Simulate(0.02)` is within the documented step ceiling (0.03).

**Looks questionable (verify with a measured test before changing)**
1. `ContinuousDynamic` on every tossed book and every bake proxy. Unity says Continuous Dynamic is the most expensive and a "last resort"; with 100+ close bodies, "CCD overhead increases quickly". Suggested smallest change: switch tossed books and bake proxies to `ContinuousSpeculative` (or Discrete for pile drops that fall at low speed, keeping CCD only for the player's throw). Test: pile bake step time and any tunneling through shelf boards. Speculative has its own "ghost collision" risk, which Unity calls out.
2. No per-body `solverIterations`. If piles look jittery or take long to sleep, set `rb.solverIterations` to roughly 10 to 15 and `solverVelocityIterations` 2 on pile books only (and the same on the bake proxies) before touching project defaults. The value 10 to 15 is **UNSOURCED GUESS** within the PhysX "reconsider above 30" ceiling.
3. `maxDepenetrationVelocity` is 10 (project default). When a book that overlaps others (they are triggers that interpenetrate in a baked pile) is given a Rigidbody and a solid collider, PhysX says initially-overlapping bodies "may explode". Set `rb.maxDepenetrationVelocity` to about 1 to 3 on any book converted to solid inside a pile (PhysX suggests 3.0 as a starting point). This is directly relevant to "neighbours settling when one is pulled out" (see 15).
4. The runtime settle routine uses its own velocity test (0.05 m/s, 0.2 rad/s for 0.5 s) and then destroys the Rigidbody. The engine's own sleep (`rb.IsSleeping()`) is what the bake already uses. Using `IsSleeping()` (maybe plus a raised per-body `sleepThreshold`, e.g. 0.02 to 0.05) would be more consistent with the bake and with PhysX's wake-counter logic. Numeric values are **UNSOURCED GUESS**; the mechanism is sourced.
5. Gravity -9.81 with a 1.6x-scale world: sourced as "assumes 1 unit = 1 m". Falls will look about 1.26x slower than a real book of that visual size would (derived). This is a feel question, not a stability one. If changed, a larger gravity needs more solver iterations (Unity's own note), so change gravity and per-body iterations together, and do it on the bodies via `Physics.gravity` only with approval (it is a project setting). A per-body alternative is `useGravity = false` plus a constant extra force (the forum workaround), but that is a hack.

**Do not do**
- Do not lower `Default Contact Offset` or `Sleep Threshold` project-wide to "help small objects": Unity warns "too close to zero ... can cause jitter" and the bug-tracker evidence points the other way (raise contact offset for thin things). Books are not small by the 1 unit = 1 m measure.
- Do not change `Time.fixedDeltaTime` globally for piles. Sleeping piles cost little; a 0.01 step doubles the cost for everyone. Use per-body iterations instead.
- Do not switch the whole project to Temporal Gauss Seidel without a measured A/B on a baked pile, given the conflicting evidence (2.5). It is a project setting, so it needs your approval anyway.

**Suggested experiment (smallest, no ProjectSettings change)**: in a copy of `BakePiles`, run the same seed three times (current; Speculative; Speculative + per-body iterations 12/2 + maxDepenetrationVelocity 2), and log steps-to-sleep and total sim time. Choose by data.
