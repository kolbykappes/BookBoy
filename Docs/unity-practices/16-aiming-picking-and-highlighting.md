# 16 - Aiming, picking items in clutter, and highlighting (URP)

Researched 2026-10-18 by web research only (no Unity Editor touched). Project: Unity 6000.6, URP, Windows PC.
Unmarked bullets carry a URL. Anything else is marked **UNSOURCED GUESS**. "Derived" means my own reasoning from sourced facts.

## 1. Question

How should a first-person camera pick one item out of overlapping, cluttered items (100+ books in piles), and how should the aimed item be highlighted?

- raycast vs SphereCast/BoxCast, aim assist or cone selection
- trigger colliders and `Physics.queriesHitTriggers`, layer masks
- URP highlight options: Render Objects with stencil, full-screen outline, inverted hull, emission pulse, rim light
- cost per highlighted object vs always-on outlines
- crosshair size and state practice
- pros and cons of the project's per-item runtime-created outline renderers and a point light

## 2. Findings

### 2.1 What the project does now (read-only inspection)

- `PlayerInteractor.UpdateFocus`: `Physics.RaycastAll(ray, maxAimRange, ~0, QueryTriggerInteraction.Collide)`, sorted by distance each frame, then walks hits to the first `Book` (skipping `Shelf` aim volumes). Interaction range is smaller than `maxAimRange` (the far case lights the crosshair as "Far").
- `BookOutline` (per book, built lazily on first glow): 3 hull `MeshRenderer`s (inner, halo, x-ray), one `Light` (point, range 1.8, no shadows) used for "strong" pulses, one `ParticleSystem`. Colour is set per frame through `MaterialPropertyBlock` on each hull. Everything is `HideAndDontSave`. The hulls are the book mesh scaled in X and Y only.
- `Crosshair`: a UI `Image` dot, 10 px idle, 18 px active/blocked, 12 px far, colour per state.
- URP: `PC_Renderer.asset` `m_RenderingMode: 2` (Forward+), additional lights per pixel, SRP Batcher on, MSAA off (`PC_RPAsset.asset`). I did not confirm which URP asset is active for Windows builds; I assumed the PC one.

### 2.2 Raycast vs SphereCast vs overlap

- Ray: "Raycasts will not detect Colliders for which the Raycast origin is inside the Collider." Closest hit via `out RaycastHit`; triggers obey `queryTriggerInteraction`, default `Physics.queriesHitTriggers`. https://docs.unity3d.com/ScriptReference/Physics.Raycast.html (ON in this project's DynamicsManager, and also the Unity default per https://docs.unity.com/en-us/engine/6000.6/manual/unity-editor/editor-settings-reference/comp-manager-group/class-physics-manager.md)
- `RaycastAll`: "order of the results is undefined" (sort yourself). `RaycastNonAlloc`: also unordered, and if the buffer fills "it is not guaranteed that the results are the closest hits". https://docs.unity3d.com/ScriptReference/Physics.RaycastAll.html , https://docs.unity3d.com/ScriptReference/Physics.RaycastNonAlloc.html
- `SphereCast`: "will not detect colliders for which the sphere overlaps the collider" at its start; `hit.normal` "does not always represent the surface normal"; zero radius is undefined. https://docs.unity3d.com/ScriptReference/Physics.SphereCast.html
- Overlap queries return everything in a volume, not an ordering (so they gather candidates, not a pick). https://discussions.unity.com/t/physics-overlapsphere-vs-raycast/866231 (community thread; the "use directional queries for picking" part is my inference from it)
- Cone / best-candidate selection exists in indie practice: a devlog collects interactables with `SphereCastAll`, filters by interface, removes occluded ones with a `LineCast` to each object's centre (newer version also checks bounding-box corners), then picks the one closest to screen centre (Manhattan distance in screen space). No performance numbers given. https://itch.io/devlog/628697/tech-behind-jen-part-1-interaction-system.amp
- Valve's Half-Life: Alyx Gravity Gloves: the pointed-at object glows, trigger press locks the selection so it "won't change to any other object", then a pulling gesture confirms. A Valve developer said the catch window is deliberately generous (laser-guided missile "wasn't that much fun"). https://www.roadtovr.com/these-details-make-half-life-alyx-unlike-any-other-vr-game-inside-xr-design/ and https://www.digipen.edu/showcase/news/with-half-life-alyx-digipen-graduate-kerry-davis-helps-redefine-vr-games (seen via search results; I did not find the targeting algorithm)
- Derived for piles: books are box meshes with box colliders of the same size, so the **nearest ray hit is exactly the surface the eye sees**. Fully hidden books cannot be hit (correct), and a visible sliver is hit only when the crosshair is exactly on it. A single ray through overlapping baked books therefore already satisfies "pick any visible item" as long as the collider is not inflated beyond the mesh. Inflating colliders to make aim easier would break that (a hit would land on a book that is visually behind another).
- If slivers feel too hard to hit, the sourced way to be forgiving is a SphereCast/candidate list with a ranking step (screen-centre distance), with a line-of-sight check so you do not select a book behind another. Radius values are **UNSOURCED GUESS** (about 0.02 to 0.04 m, roughly half the thinnest book).
- Hysteresis ("stick to the current target for a few frames"): the Alyx lock-on is the only sourced relative; a fixed hold time such as 0.1 to 0.15 s is an **UNSOURCED GUESS**.

### 2.3 Triggers, layer masks, and what to watch for

- Queries see triggers only if `queriesHitTriggers` is on (it is) or the call passes `QueryTriggerInteraction.Collide`. The project already passes `Collide`. https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
- `Physics.SphereCast`/`OverlapSphereNonAlloc` use the same `QueryTriggerInteraction` argument, default `UseGlobal`. https://docs.unity3d.com/ScriptReference/Physics.OverlapSphereNonAlloc.html
- Layer masks: queries accept a `layerMask`; Unity recommends building it with `LayerMask.GetMask`. The project passes `~0` (all layers) and then filters by component, because there are no custom layers (TagManager shows only built-ins). A dedicated "Items" layer would let the ray ignore the player capsule, shelf volumes and walls up front, but needs approval (layers are guarded in CLAUDE.md). Same Raycast URL.
- Static trigger colliders that are moved by Transform are only seen by queries after the next physics step or `Physics.SyncTransforms()`. https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/physics-optimization/cpu/static-colliders.md . After a pile is rearranged, call `Physics.SyncTransforms()` or the first aim query may hit the old pose.
- Allocation: `RaycastAll` allocates an array every call; `RaycastNonAlloc` does not but is unsorted. Both are fine at one query per frame. (The allocation claim for RaycastAll is Unity's general guidance and the page I fetched did not discuss it, so treat as **UNSOURCED** for numbers.)

### 2.4 Highlight techniques in URP

- **Render Objects renderer feature**: draws only objects on a chosen **GameObject layer** (Layer Mask), at a chosen Event, with a Material or Shader override, depth overrides, and a Stencil block (Value, Compare Function, Pass, Fail, Z Fail). "Shader override is not SRPBatcher compatible and performs less well than Material override." https://docs.unity3d.com/Manual/urp/renderer-features/renderer-feature-render-objects.html . Unity's own worked example draws one layer in a different colour where it is hidden, using Depth Test = Greater, and does not mention stencil or outlines. https://docs.unity3d.com/Manual/urp/renderer-features/how-to-custom-effect-render-objects.html
- Consequence (derived): a stencil/Render Objects highlight needs the highlighted item to be on a distinct layer, which means a new layer (guarded) and, if physics layers matter, a physics-matrix thought.
- **Full Screen Pass renderer feature**: for post-process style effects; options include requirements for Depth/Normal/Color/Motion, each of which adds passes or textures; "Bind Depth-Stencil ... has an impact on performance". https://docs.unity3d.com/Manual/urp/renderer-features/renderer-feature-full-screen-pass.html
- **Ready-made Unity 6 outline feature** (unitycoder): selects objects by **rendering layer** (Outline_1..4), blur-based width shared by all four layers "for technical & performance reasons", runs after post-processing, no per-object width, does not support Render Graph compatibility mode, FXAA only. https://github.com/unitycoder/unity-urp-outline (third party, not Unity-official)
- **Technique comparison** (Alexander Ameye):
  - Rim effect: cheap but "only work well on spherical objects", breaks on cubes.
  - Vertex extrusion (inverted hull): simple, mesh drawn twice, gaps at sharp corners unless normals are fixed, backface culling is the usual masking, stencil can remove inner lines.
  - Blurred buffer: soft glow, heavier, downscale and a two-pass blur recommended.
  - Edge detection (Sobel/Roberts): one full-screen pass, weak per-object control.
  - Jump flood: good for wide outlines at moderate cost.
  https://ameye.dev/notes/rendering-outlines/ (no quantified costs)
- Derived: books are boxes, which is the case where rim lighting is weakest and where scaling a box mesh as a hull works (a scaled box has no corner gaps). So the project's hull-by-scaling is a good fit for boxes specifically.
- **Emission pulse**: for URP, `MaterialPropertyBlock` "will likely result in a drop in performance" and is "not compatible with SRP Batcher"; the manual says use "a different material for each property variation" and avoid `Renderer.material` (it creates a copy and breaks batching), read via `sharedMaterial`. https://docs.unity3d.com/ScriptReference/MaterialPropertyBlock.html , https://docs.unity3d.com/6000.0/Documentation/Manual/DrawCallBatching-Properties.html . Unity's public roadmap lists MPB + SRP Batcher compatibility as an open request. https://activation.unity3d.com/roadmap/75-material-property-block-srp-batcher-compatibility (seen in search results; not re-verified)
- Pulsing without per-object writes: drive the pulse from time in the shader (`_Time`) or with one shared material per colour; from search-result text, low authority. (**UNSOURCED GUESS** that this fits the project's shaders; I did not read them.)
- **Point light for a strong pulse**: URP Forward (non-plus) allows 1 main + 8 additional lights **per object**; Forward+ and Deferred remove that per-object limit and cap the **per camera** count (desktop 256). https://docs.unity3d.com/Manual/urp/lighting/light-limits-in-urp.html . Project's PC renderer is Forward+, so a few pulse lights do not hit a per-object cap. The per-light shading cost is not quantified in any page I could read; Forward+ is described as evaluating only lights that touch a screen tile. Light range is 1.8 m in `BookOutline`, so each lit area covers a few square metres of floor and nearby shelf (derived).

### 2.5 Cost per highlighted object vs always-on

- Inverted hull per item: 2 extra draws (3 with the x-ray hull) plus MPB state per hull, plus a light and a particle system for strong pulses. Sourced fact: the mesh is "drawn twice" (Ameye), MPB breaks SRP batching (Unity). Cost scales with the number of simultaneously highlighted items. For 1 steady focus it is negligible (derived); for 100+ always-on it is not.
- Screen-space approaches cost a roughly fixed number of full-screen passes irrespective of item count (Ameye's table), at the price of an extra setup and layer/rendering-layer assignment.
- No benchmark numbers for any of these exist in the sources I could read. Measure with the Frame Debugger (it reports whether a draw is an "SRP Batch") and the Profiler.

### 2.6 Crosshair practice

- Evidence is thin. One game-UX article: a crosshair too small "can disappear" in fast motion, too large "will likely block players' sight", colour change on aiming at a target "can mislead" unless the player has a high chance of getting the target, keep similar functions consistent. https://gamedeveloper.com/design/-ux-design-methodology-of-keeping-cross-hair-design-on-track (shooter context)
- Game Accessibility Guidelines: offer a choice of crosshair/cursor colours and designs because a default may be too small or faint, or fail for some colour-blind players. https://gameaccessibilityguidelines.com/provide-a-choice-of-cursor-crosshair-colours-designs/ (community guidelines)
- No sourced pixel size for a 1080p reticle. A fixed pixel-size crosshair covers 75% of the screen fraction at 1440p compared with 1080p, and 50% at 2160p (derived arithmetic), which is the argument for resolution-independent scaling (see file 17).
- Current 10/18/12 px values with colour changes match the "state via colour+size" practice above; whether 10 px is large enough at 1440p is **unknown** without a test.

## 3. Numbers and rules of thumb

| Item | Value | Source |
|---|---|---|
| Forward per-object lights | 1 main + 8 additional | URP light limits page |
| Forward+/Deferred per-camera lights (desktop) | 256 additional | URP light limits page |
| Selection ranking in a devlog | nearest to screen centre, after LineCast occlusion filter | itch.io Ječná Nightshift devlog |
| Hull draw cost | mesh drawn twice (no ms figure) | Ameye |
| SphereCast radius for slivers | 0.02 to 0.04 m | **UNSOURCED GUESS** |
| Hysteresis | 0.1 to 0.15 s | **UNSOURCED GUESS** |
| Crosshair size | no authoritative pixel value | none |

## 4. Confidence and unknowns

- **High**: query semantics (origin inside collider, SphereCast start-overlap, unordered RaycastAll), trigger handling, MPB vs SRP Batcher (Unity docs), Forward vs Forward+ light limits, Render Objects/Full Screen Pass options.
- **Medium**: comparison of outline techniques (one expert article), Alyx selection flow (press coverage, not Valve technical talk).
- **Low / unknown**:
  - Real costs (ms) of the current per-item hulls, light, and particle system in a 100+ book room.
  - Whether the active URP asset for Windows is the PC one (Forward+).
  - Whether the hull material shaders support SRP Batcher or GPU instancing.
  - Any authoritative reticle size.
  - How many items glow at once in practice (sister glow).

## 5. Implication for BookBoy

**Picking: looks right.** Nearest-hit raycast with trigger inclusion is the correct primitive for "pick any visible item in a pile" because box colliders equal the mesh. Smallest tweaks:
1. Keep colliders exactly the mesh size (do not inflate to help aim).
2. After the pile positions change, call `Physics.SyncTransforms()` before the first query.
3. If playtesting shows edge slivers are hard: after a miss (or a hit on a non-book), do a second query with `Physics.SphereCast` (radius about 0.03, starting about 0.1 m in front of the camera so it does not start inside colliders) and accept only a book with line of sight. No change to ProjectSettings.
4. Replace `RaycastAll` + `Sort` with `RaycastNonAlloc` into a reused buffer of about 16 plus a sort, only if the Profiler shows GC from this line (Unity's note about a full buffer applies).

**Highlighting: the current design works for a handful of glowing items but scales badly.**
- Looks right: inverted hull by scaling the box mesh (no corner gaps on boxes), light off unless strong, one steady glow at a time.
- Looks wrong/risky:
  - Each glowing book builds 3 renderers + a Light + a ParticleSystem and keeps them forever (lazy build, never pooled). That is per-item state for what is only ever one steady target; it grows with every book ever pulsed.
  - MPB on the hulls breaks SRP Batcher (Unity's docs say so), so every glowing hull is its own draw.
  - A point light per pulsing book means N lights; Forward+ removes the per-object cap but per-camera 256 remains, and shading cost is unmeasured.
- Smallest change: make the glow a **pooled singleton set** (one steady highlighter plus a small pool, e.g. 8, for pulses) that is positioned onto the target book and parented when active, with colour via a handful of shared materials (one per feedback colour) instead of MPB. That removes per-book renderers/lights/particle systems and keeps the current look. This does not touch layers, ProjectSettings or the URP asset.
- Later option, needs approval: a Render Objects stencil/outline feature or the unitycoder outline feature on a dedicated layer/rendering layer. Better for large numbers, but adds a layer, and Ameye's note says stencil-based approaches shine for overlapping silhouettes (relevant to piles).
- Do not make always-on outlines for every book with the hull method (draw count and unbatched hulls).

**Crosshair**: keep state-by-colour-and-size. Add (cheap, optional) a settings toggle for crosshair colour/size later (accessibility guideline). Verify readability at 1440p after the Canvas Scaler review in file 17.
