# 15 - Piles, thrown items and local physics "bubbles" without always-on physics

Researched 2026-10-18 by web research only (no Unity Editor touched). Project: Unity 6000.6, built-in PhysX 3D physics, Windows PC.
Unmarked bullets carry a URL. Anything else is marked **UNSOURCED GUESS**. "Derived" means my own reasoning from sourced facts.
Companion file: `14-physics-small-objects-and-settings.md` (numbers for iterations, CCD, depenetration, sleeping).

## 1. Question

How do games and Unity projects get believable piles and thrown items without keeping physics on for everything? Specifically:

- bake-then-freeze
- kinematic or sleeping bodies woken on demand
- waking neighbours in a radius with automatic re-sleep
- trigger vs non-trigger colliders (a trigger cannot collide)
- pulling one item out of a pile so the rest settle
- stopping the player from kicking piles (CharacterController pushing Rigidbodies)
- what real developers have written, and what could not be found

## 2. Findings

### 2.1 What the project does now (read-only inspection)

- Idle books: one `BoxCollider` with `isTrigger = true`, no Rigidbody (`Book.cs` lines 67-76, `EndSolid`, `SettleRoutine`).
- Throw or pile-up: `BeginSolid` flips the collider to non-trigger, adds a Rigidbody (mass 0.4, ContinuousDynamic, Interpolate), and `SettleRoutine` destroys the Rigidbody and restores the trigger once velocity stays under 0.05 m/s and 0.2 rad/s for 0.5 s.
- Room piles are baked in the Editor with proxy cubes in an `EditorSceneManager.NewPreviewScene()` physics scene, `Physics.Simulate(0.02)`, up to 3000 steps, positions copied onto the real books (`LamplitBuilder.Piles.cs`).
- Aim uses `Physics.RaycastAll(ray, maxAimRange, ~0, QueryTriggerInteraction.Collide)` sorted by distance (`PlayerInteractor.UpdateFocus`).
- Player is a `CharacterController`. `ProjectSettings/TagManager.asset` has **no custom layers** (only built-ins), so there is no layer to exclude.
- Derived consequence: while a book is a trigger, nothing can land on it. A thrown book, which is solid, falls straight through every idle pile book. So today "landing on a pile" is only ever correct for the pre-baked case, never for a live throw.

### 2.2 Collider types: what a trigger can and cannot do (Unity 6000.6 manual)

- "A trigger collider does not collide with other colliders; instead, other colliders pass through it." At least one side of a trigger event needs a Rigidbody. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/collision-section/collider-interactions/create-trigger.md
- "Trigger colliders don't generate collision detection messages." A static trigger vs a static collider generates nothing. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/collision-section/collider-interactions/collider-types-interaction.md
- Static colliders (no Rigidbody) "don't move in response to collisions". Moving them by Transform is possible but the engine "cannot immediately take Transform-led movement into consideration"; for a collider that moves at runtime use a kinematic Rigidbody. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/collision-section/collider-types-introduction.md and https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-optimization/cpu/static-colliders.md (transform changes are picked up "during the next physics step or when Physics.SyncTransforms is called")
- **Very relevant to pulling a book out of a pile**: "the physics system does not recalculate or awaken physics bodies in response to Static colliders moving or disappearing" (example: a sleeping car does not fall when its static floor moves). Same collider-types-introduction URL. So removing or hiding a supporting static object will not wake what rested on it; you must wake them yourself (`Rigidbody.WakeUp`). https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/rigidbody/rigidbodies-overview.md
- Kinematic Rigidbody colliders "can wake other colliders from a sleeping state when they make contact", can act as triggers, and apply friction. `Rigidbody.isKinematic` can be switched at runtime and Unity calls this "a useful way to manage performance on colliders that don't always need to be dynamic" (ragdoll example). Same collider-types-introduction URL.
- Query side: `Physics.Raycast` and overlap queries see triggers when "Queries Hit Triggers" is on (default, and ON in this project); per-call `QueryTriggerInteraction` can override. `Physics.OverlapSphereNonAlloc` allocates nothing, never resizes the buffer, and truncates silently. https://docs.unity.com/en-us/engine/6000.6/manual/unity-editor/editor-settings-reference/comp-manager-group/class-physics-manager.md and https://docs.unity3d.com/ScriptReference/Physics.OverlapSphereNonAlloc.html
- Derived: because queries hit non-trigger colliders too, a book does **not** need to be a trigger for the aim ray. The trigger flag exists only to stop it blocking the player and to stop physical interaction. Making that decision by layer or by per-pair ignore would let idle books keep solid colliders (see 2.6), but that needs approval to touch layers.

### 2.3 Sleeping, waking and re-sleeping

- Unity: below the threshold a body sleeps and is left out of calculations; a collision or force wakes it; `Rigidbody.Sleep()` only works if the body is "not in contact with an awake rigidbody"; `IsSleeping()` reports; don't call `WakeUp` unnecessarily because "continuously waking objects negates the benefit". https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-optimization/cpu/rigidbody-sleeping.md , https://docs.unity3d.com/ScriptReference/Rigidbody.Sleep.html
- PhysX: wake triggers include touch with an awake actor, setting a non-zero velocity; "a zero wake counter does not mean that the actor has to be asleep". https://archive.docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/RigidBodyDynamics.html
- Sleeping works per island in Box2D (similar principle in PhysX, but the PhysX source for Unity's island handling is **UNSOURCED**): "Sleeping must be done per island, not per body"; waking "must propagate through all touching bodies immediately". Consequence: one awake body in a touching pile keeps the pile awake, and touching one body wakes the lot. https://box2d.org/posts/2023/10/simulation-islands/ (Box2D, not PhysX)
- Community: raising the sleep threshold from 0.005 to 0.05 made piles come to rest and sleep much sooner (Unity forum, one user). https://discussions.unity.com/t/optimizing-performance-of-many-rigidbodies/594677 (anecdote)
- Same thread, anecdotes: removing Rigidbodies at runtime raised physics cost 2-6 ms in a scene with about 2,300 bodies, removing them at start cut cost; adding Rigidbodies on demand was called "pointless"; enabling/disabling in the same frame causes spikes; "with about 100 bodies, optimizing likely does more harm than good. Test first." Same URL. **Scale caveat**: those were thousands of bodies; BookBoy's bubble is dozens.
- A distance-based pattern from a forum: start asleep, kinematic when far, remove the Rigidbody when very far. Anecdotal. https://discussions.unity.com/t/saving-space-in-physics-any-ideas/411252 (seen in search results; not opened)
- A developer write-up on exactly the "touching one wakes the whole pile" problem (Godot with Jolt, not Unity): touching one settled body woke the entire pile so the player sank in or shoved it aside. Their fix: when a body sleeps, zero its velocities and **freeze** it so "anything colliding with it will bounce off"; force-sleep parts that stay barely moving; unfreeze only on a big enough hit. Known limitation: "Frozen pieces can float if the surface beneath them is removed." This is the same trade-off as BookBoy's "remove the physics once at rest" and the same failure mode as pulling a book out from under others. https://bird-with-toes.itch.io/robot-shooter/devlog/1102591/physics-of-robot-shooter

### 2.4 Bake-then-freeze (what the project already does)

- Manual simulation of a physics scene: use a fixed step; `Physics.Simulate`/`PhysicsScene.Simulate`; "Using step values greater than 0.03 is likely to produce inaccurate results"; non-default physics scenes "need to be simulated manually". https://docs.unity3d.com/ScriptReference/Physics.Simulate.html , https://docs.unity3d.com/ScriptReference/PhysicsScene.Simulate.html , https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-optimization/cpu/manual-simulation.md
- Editor-time settle tools exist and use the same shape of idea: simulate in Edit Mode, stop on velocity threshold / time after impact with a max time, remove temporary components. https://github.com/unitycoder/TCS-PhysicsDropper (a pattern, not Unity-official). A forum answer says the same: disable auto simulation, step in an editor window until bodies sleep, restore auto simulation, copy transforms. https://discussions.unity.com/t/saving-the-rest-state-of-physics-simulation-to-a-new-scene/810547 (2D example; same idea in 3D)
- Rigid bodies "created in an initially overlapping state may explode"; limit with a max depenetration velocity (PhysX suggests starting at 3.0). https://archive.docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/BestPractices.html . This matters whenever a baked book (which touches or slightly overlaps its neighbours) is turned back into a solid body.
- No developer statement was found for how Unpacking, a tidy-up game, or a Goose-style game handles piles. Unpacking is described as grid-snap placement with placement sounds and rules like "misplaced items will glow red", i.e. not rigid-body piles, so it is a poor reference for physics piles. https://www.gamedeveloper.com/design/telling-story-someones-belongings-unpacking (the full interview text did not mention physics in the excerpt I could access) and https://limitedrungames.com/blogs/news/unpacking-unpacking-with-designer-tim-dawson . I found **no** GDC talk about Untitled Goose Game physics. Treat "how cozy games do it" as **unknown**, not as a pattern.
- Source Engine (Half-Life) physics props: sleep when settled, wake on outside force; levels can mark props "start asleep". The wiki pages could not be fetched (HTTP 403), so this rests on search-result text only. https://developer.valvesoftware.com/wiki/Physics_prop (low confidence)

### 2.5 Local physics: three ways to build a "bubble"

**Option A - convert the neighbours in place (Rigidbody/solid collider only for items inside the bubble).**
- Enumerate items near the landing spot with `Physics.OverlapSphereNonAlloc` (trigger-inclusive), turn them from static trigger to dynamic solid, optionally start them asleep, drop the thrown book, wait until all bubble bodies sleep (with a time cap), then restore. Everything outside the bubble stays a trigger, so it cannot be hit and cannot be chained. Pieces: sections 2.2 and 2.3.
- Boundary problem (derived): inside bubble bodies need something solid to rest on at the edge. Idle trigger books are not solid, so the outermost ring must also be made solid, either kinematic (documented to wake others and add friction) or static non-trigger, during the bubble only.
- Cost (anecdotal): add/remove Rigidbody and enable/disable colliders can spike (2.3). Alternative documented as good practice: keep a Rigidbody on every item permanently and use `isKinematic` as the switch; kinematic sleeping bodies with many child colliders still cost, but a book has one collider, so this concern is small (**derived**).

**Option B - proxy bodies in a separate PhysicsScene (the bake, at runtime).**
- A scene created with `LocalPhysicsMode.Physics3D` owns its own PhysX scene; queries and callbacks are per scene; you step it yourself. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-multi-scene.md , https://docs.unity3d.com/ScriptReference/SceneManagement.LocalPhysicsMode.html
- Design (derived from those pages): keep a small pool of proxy cubes in that scene, clone the nearby static geometry (floor, shelf) and the neighbours' poses as static or kinematic proxies, simulate only the bubble in fixed 0.02 steps, copy the final (or per-frame) poses back to the real books, then park the proxies. The player is not in that scene, so the player cannot kick anything and nothing outside the bubble can be touched. This is what `BakePiles` already does in the Editor.
- Costs: building the proxy world on every throw; visuals must follow the proxies each fixed step; domain reload is disabled in this project (CLAUDE.md rule 9), so PhysicsScene handles and pools must be re-created defensively; non-default scenes are not simulated automatically.
- No developer write-up found that uses this exact runtime pattern, so quality at runtime is unproven (**UNSOURCED GUESS** that it is practical for dozens of bodies).

**Option C - no rigid bodies: procedural settle.**
- When a book is pulled, move the books stacked above it by the lost thickness (a short tween), no engine physics. **UNSOURCED GUESS**; no source describes it, but it fits "neighbours settling" with zero chain risk and zero kicking. It cannot produce tumbling.

### 2.6 Preventing the player from kicking piles

- Unity manual (component reference, 6000.6): "The Controller does not react to forces on its own and it does not automatically push Rigidbodies away." You push only via `OnControllerColliderHit` scripting. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/character-control-section/class-character-controller.md
- **Sources disagree**: the intro page says a controller "can push rigidbody objects aside while moving". https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/character-control-section/character-controllers.md A forum report (Unity 5.6) says a controller cannot move a stationary Rigidbody but can push one that is already moving, erratically. https://discussions.unity.com/t/prevent-charactercontroller-from-pushing-rigidbodies/660569 (community). Treat "it does not push" as likely but verify in a test.
- Options that appear in the sources:
  - `CharacterController.detectCollisions = false`: "decides whether an incoming collider will be blocked by the controller's collider" and "does not affect collisions during the controller's own movement". The forum poster used it to stop Rigidbodies reacting to the controller. https://docs.unity3d.com/ScriptReference/CharacterController-detectCollisions.html , thread above
  - Layers: the controller and Rigidbody components have Include/Exclude Layers overrides, and the collision matrix is global and two-way. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/character-control-section/class-character-controller.md , https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/rigidbody/class-rigidbody.md . This needs a new layer, which the CLAUDE.md guardrails say needs approval.
  - `Physics.IgnoreCollision(a, b)` per pair: "not persistent"; Unity issue tracker entries report colliders with ignore pairs take much longer to destroy (grows with collider count; open), and an `IgnoreCollision` call spike of 400 to 600 ms fixed in 6000.0.38f1 / 6000.1.0b6 / 6000.2.0a3. https://issuetracker.unity3d.com/issues/colliders-with-ignorecollision-take-significantly-longer-to-be-destroyed , https://issuetracker.unity.com/issues/1742/bad-performance-when-executing-the-physicsignorecollisions-method-while-in-play-mode (both seen in search results only)
  - Proxy collider outside the Rigidbody hierarchy: reported to work for one user and fail for another (same forum thread).
  - Do the bubble where the player is not (Option B), or only while the player is farther than a few metres from the landing spot (**UNSOURCED GUESS**).
- Derived: the existing design (idle books are triggers, no Rigidbody) is already immune to kicking; the risk only exists for bodies that are solid, i.e. during a throw or bubble.

### 2.7 Pulling one book out of a pile so the others settle

- Not found in any source as a ready-made pattern. Composition of sourced facts (derived):
  1. Raycast picks book X. Before removing X, `OverlapBoxNonAlloc`/`OverlapSphereNonAlloc` around X's bounds (expanded a little upward and sideways) to find candidates that overlap or rest on X.
  2. Turn only those candidates (and a one-layer ring around them, kinematic) into a bubble (Option A) or hand them to the proxy scene (Option B). Because static colliders that vanish do not wake others, wake explicitly.
  3. Give the converted bodies a low `maxDepenetrationVelocity` (about 1 to 3, PhysX suggests 3.0 as a starting point) so the baked contact overlap doesn't explode.
  4. Let them settle; when all are asleep (or at the time cap), revert to triggers and call `Physics.SyncTransforms()` so the next aim ray sees the new poses.
- A comparable existing behaviour in the Godot write-up: frozen pieces float if the support is removed (2.3). Same issue; they chose "unfreeze on hit".

## 3. Numbers and rules of thumb

| Item | Value | Source |
|---|---|---|
| Fixed step for any manual sim | 0.02 s; never above 0.03 s | Physics.Simulate reference |
| Max depenetration velocity for bodies that start overlapping | start near 3.0 (project default is 10) | PhysX BestPractices |
| Sleep threshold anecdote | 0.005 to 0.05 made piles settle and sleep sooner | Unity forum, single user |
| Runtime Rigidbody removal cost anecdote | 2 to 6 ms in a ~2,300 body scene | Unity forum, single user |
| Optimization advice | "with about 100 bodies, optimizing likely does more harm than good. Test first." | Unity forum (hippocoder) |
| Bake cap used by project | 3000 steps (60 s), early exit when all `IsSleeping()` | project code |

## 4. Confidence and unknowns

- **High**: trigger vs collision semantics, static colliders not waking bodies, kinematic can wake others, manual stepping of a PhysicsScene, queries hit triggers by default.
- **Medium**: waking propagates through touching bodies (PhysX guide plus Box2D article), so a bubble boundary of non-solid items is a real isolation boundary. CharacterController does not push by default (manual itself disagrees).
- **Low / unknown**:
  - Any real developer write-up for cozy sorting / Unpacking-style / Goose-style pile handling (none found).
  - Measured cost of adding/removing Rigidbodies for dozens of bodies in Unity 6000.6.
  - Whether the PhysX island sleeping in Unity wakes a whole pile when one body is touched (the Box2D and Godot/Jolt accounts say yes for those engines).
  - Practical cost/benefit of Option B at runtime.
  - Whether `Rigidbody.Sleep()` right after adding a Rigidbody to an item inside a baked pile is accepted (docs: only if not in contact with an awake body, which should hold if all neighbours are also asleep).

## 5. Implication for BookBoy

What looks **right**: bake in an isolated preview physics scene; idle items as static triggers with no Rigidbody (cheap, cannot be kicked); freeze after rest.

What looks **wrong or incomplete**:
1. **A live throw cannot land on a pile**: pile books are triggers, so the thrown book passes through them (derived from the collider matrix). Nothing in the current code simulates the pile at throw time. If the owner wants real landing on piles, a bubble is required.
2. **Pulling a book out does nothing physical**: neighbours are static triggers; removal of a static collider wakes nothing, and they have no Rigidbody anyway. Settling must be done explicitly.
3. **Destroying the Rigidbody the instant the book is "still"** (SettleRoutine, 0.05 m/s for 0.5 s) can freeze a book in a slightly unstable pose, and it is the Godot developer's "floating frozen piece" trap. Using `IsSleeping()` would be closer to the engine's own judgement (the bake already does).

**Smallest change that gets the owner's list** (no ProjectSettings, no layers, no new tags):
- Implement a `PileBubble` helper (plain C# rules for "which items are in the bubble" can be unit-tested per CLAUDE.md rule 17) using Option A:
  - on throw landing (and on a pull): `OverlapSphereNonAlloc` with radius about 0.6 to 0.9 m (**UNSOURCED GUESS**, roughly 2 to 3 book lengths at 0.38 m) at the landing/removal point, trigger-inclusive;
  - items inside: Rigidbody + non-trigger collider, `maxDepenetrationVelocity` about 2, per-body `solverIterations` about 10 (see file 14), started asleep with `Sleep()` except the thrown book and anything resting directly on the pulled one;
  - one outer ring of items: kinematic + non-trigger (so inside bodies have something to rest on);
  - end when every bubble body `IsSleeping()` or at a time cap (about 6 s, matching the existing `limit`), then destroy/disable the Rigidbodies, restore triggers, `Physics.SyncTransforms()`.
- Kicking: keep `CharacterController.detectCollisions` as is, add no `OnControllerColliderHit` push code (so no push by default per the manual), and test once whether the controller moves a solid resting book. If it does, set `detectCollisions = false` while a bubble is active or use per-pair `Physics.IgnoreCollision` for bubble members only (bounded count, cleared at end; mind the destroy-cost report). A dedicated "Items" layer with the controller's Exclude Layers is the cleanest fix but needs approval.
- Chained physics: guaranteed by construction because only bubble items have solid colliders; everything else stays a trigger.
- If Option A proves flaky (explosions or edge-ring artefacts), fall back to Option B (proxy PhysicsScene) because the bake already proves the idea, or Option C for pulls.

Measure first: log the bubble item count, steps-to-sleep, and the time of the add/remove frame in the Profiler (Physics module), and only then decide between A and B.
