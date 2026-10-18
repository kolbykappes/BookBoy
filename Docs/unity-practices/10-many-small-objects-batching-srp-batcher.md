# 10 - Rendering hundreds of small coloured objects in URP: SRP Batcher, batching, instancing, GPU Resident Drawer

Researched 2026-10-18 from Unity manual / script reference / Unity blog pages (versions 6000.0 to 6000.6) plus two developer write-ups for budget numbers. No Unity commands were run; no project settings were inspected.

## 1. Question

How should hundreds of similar small coloured objects (about 340 books + about 10 cube primitives per container, one Material per colour/group, shared cube mesh) be rendered in URP on a Windows PC? What do the SRP Batcher, static batching, dynamic batching, GPU instancing and the Unity 6 GPU Resident Drawer each need and do? MaterialPropertyBlock vs one material per colour? When do draw calls actually matter at this scale, how do we measure, and what are reasonable budgets?

## 2. Findings

### The methods and what Unity says about each in URP

- Unity's own decision page ("Choose a method for optimizing draw calls", 6000.3) gives a per-pipeline table. For **URP**: SRP Batcher = "Enable"; GPU Resident Drawer = "Enable"; BRG API = "use GPU Resident Drawer instead, except advanced cases"; **GPU Instancing material checkbox = "Disable, to avoid extra shader variants"**; **static batching ("Batching") = "Disable"** because "Static batching isn't compatible with the BRG API or GPU Resident Drawer"; dynamic batching "no longer recommended" (for Built-in). Source: https://docs.unity3d.com/6000.3/Documentation/Manual/optimizing-draw-calls-choose-method.html (also summarized from http://docs.unity3d.com/Manual/optimizing-draw-calls-choose-method.html).
- Same page, best practices: use the same materials across GameObjects as much as possible; "for the same mesh with different properties, use Material Variants instead of multiple materials"; **avoid the MaterialPropertyBlock API in URP and HDRP**. Same URL.
  - Caveat: Material Variants (Material.parent) are an Editor-time authoring feature; the API reference says the parent link exists only in the Editor and "in a built project all material hierarchies are flattened" (https://docs.unity.cn/ScriptReference/Material-parent.html, 2022.3 page; the Unity 6 class reference lists `isVariant`/`parent` too). I did not find a Unity statement that variants batch *better* than ordinary separate materials at runtime. UNSOURCED GUESS: the recommendation is about authoring/maintenance and shader-variant count rather than batching speed. See the SRP Batcher section: the SRP Batcher itself does not care how many materials share a shader.
- Priority when several methods apply: each GameObject uses one method; with all enabled, "Dynamic meshes with a compatible shader use the SRP Batcher with the GPU Resident Drawer or the BRG API" (the GRD takes eligible renderers; the rest stay on SRP Batcher). Same page. (A search summary described an explicit priority order SRP Batcher > instancing > dynamic batching on the 6000.0 version of this page; the 6000.3 page says there is no explicit ranking. I only verified the 6000.3 wording.)

### SRP Batcher

- What it does: "reduces the CPU time Unity requires to prepare and dispatch draw calls" for materials using the same shader variant. It does **not** reduce the number of draw calls; it reduces render-state changes between them by keeping material data persistent in GPU constant buffers. Source: https://docs.unity3d.com/6000.0/Documentation/Manual/SRPBatcher.html and https://unity.com/blog/engine-platform/srp-batcher-speed-up-your-rendering
- Material count: "You can still use as many different materials with the same shader as you want" - the thing to minimise is shader *variants*. Same docs page. The blog adds "the more Materials you have in a Scene, the more CPU will be required to setup GPU data", but any number of Materials is fine if they use the same shader.
- Requirements for a GameObject (Unity 6 manual): must contain a mesh or skinned mesh (not a particle); "mustn't use MaterialPropertyBlocks"; shader must declare engine properties in one `UnityPerDraw` constant buffer and material properties in one `UnityPerMaterial` constant buffer. All URP/HDRP Lit and Unlit shaders (and Shader Graph shaders) qualify. Source: https://docs.unity3d.com/Manual/SRPBatcher-Materials.html ; opt-out mechanics (adding a MaterialPropertyBlock, or a non-UnityPerMaterial property) at https://docs.unity.cn/Manual/SRPBatcher-Incompatible.html
- Benchmarks from Unity's blog: CPU render-loop speedups of about 1.2x-4x depending on scene (markers `RenderLoop.Draw`/`ShadowLoop.Draw`, not total frame rate). PC DX11 FPS Sample: 1.23x. Worst case (every object dynamic, unique material) about 4x on PS4. Book of the Dead: 1.47x with no frame-rate change because it was GPU-bound. Source: https://unity.com/blog/engine-platform/srp-batcher-speed-up-your-rendering (2018-era; HDRP/LWRP).
- Multithreaded only on DX12/Vulkan/consoles with Graphics Jobs (table on the choose-method page).

### MaterialPropertyBlock vs one material per colour

- MaterialPropertyBlock docs: "Note that this is not compatible with SRP Batcher" and "Using this in URP, HDRP or a custom SRP ... will likely result in a drop in performance." Source: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MaterialPropertyBlock.html
- GPU Resident Drawer's 6000.3 compatibility page lists "Not use the MaterialPropertyBlock API" as a requirement (the 6000.3 URL returned seven requirements, including this one); a search summary of the 6000.3 snippet claimed a shorter list without it. Read the page for your exact version. Source: https://docs.unity3d.com/Manual/urp/make-object-compatible-gpu-rendering.html
- A Unity roadmap card "Material Property Block / SRP Batcher compatibility" exists and is marked Under Consideration (no commitment): https://activation.unity3d.com/roadmap/75-material-property-block-srp-batcher-compatibility
- Conclusion supported by sources: **one shared material per colour (same shader) is the URP-blessed way**; MPB is the discouraged way. With ~20-40 colour materials sharing one shader, SRP Batcher handles them fine.
- `Renderer.material` (getter or setter use) clones the material per renderer; "It is your responsibility to destroy the materials when the game object is being destroyed" (https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Renderer-material.html). Unity's GPU optimisation page: use `Renderer.sharedMaterial` to avoid unintentionally instancing materials and breaking batches (https://unity.com/how-to/gpu-optimization). **Relevant to BookOutline / highlight code**: any runtime `renderer.material = ...` or `.material.color` on a highlighted book creates a clone.

### Static batching

- Combines meshes into one vertex+index buffer in world space; Mesh Renderers only; up to 64,000 vertices per buffer (multiple batches possible); manual combining means "can't cull meshes individually". Source: http://docs.unity3d.com/Manual/DrawCallBatching.html . Unity's GPU page says it "uses more memory" than dynamic batching: https://unity.com/how-to/gpu-optimization
- In URP the choose-method page says disable it (incompatible with GRD/BRG). Unity's GRD performance page also suggests disabling Static Batching in Player Settings for speed (https://docs.unity3d.com/Manual/urp/gpu-resident-drawer-performance.html). That is a ProjectSettings change; see Implication.
- Static batching does not fit objects that move or that are spawned/despawned (books get picked up and thrown; rooms toggle). Marking only the container furniture static is possible in principle; with SRP Batcher the benefit is small (see Implication).

### Dynamic batching

- Unity's GPU page: only for small meshes, "no more than 300 vertices each and 900 total vertex attributes", otherwise it wastes CPU. Choose-method page: "dynamic batching is no longer recommended". A cube primitive is 24 vertices (UNSOURCED GUESS from the standard Unity cube mesh, not from docs), so it would qualify, but URP does not use it for SRP-Batcher-compatible renderers anyway.

### GPU instancing (material checkbox)

- Unity's table says disable the "Enable GPU Instancing" material checkbox in URP "to avoid extra shader variants". GPU instancing and the SRP Batcher are not compatible on the same renderer: the SRP Batcher incompatible page says you can deliberately opt out of the SRP Batcher "to use GPU instancing" and tells you to confirm with the Profiler that instancing is actually better (https://docs.unity.cn/Manual/SRPBatcher-Incompatible.html). Instancing requires identical mesh **and material**, so 340 books across many colour materials would not collapse into one instanced draw anyway (UNSOURCED GUESS that follows from the "same mesh and material" statement on https://unity.com/how-to/gpu-optimization).

### GPU Resident Drawer (Unity 6)

- Uses the BatchRendererGroup API to draw with GPU instancing automatically; "Enable" is Unity's recommendation for URP. Source: https://docs.unity3d.com/6000.3/Documentation/Manual/urp/gpu-resident-drawer.html
- Requirements (6000.3 page): **Forward+ rendering path only**; graphics API with compute shaders (not OpenGL ES); GameObject needs a Mesh Renderer; setting BatchRendererGroup Variants = Keep All (Project Settings > Graphics > Shader Stripping) raises build time; SRP Batcher must be enabled in the URP asset; GPU Resident Drawer = Instanced Drawing in the URP asset. Compatibility page: Mesh Renderer, no Light Probe Proxy Volume, only static GI, DOTS-instancing-compatible shader, no moving between camera renders, no MaterialPropertyBlock, no per-instance callbacks like `OnRenderObject`. Source: https://docs.unity3d.com/Manual/urp/make-object-compatible-gpu-rendering.html
- A search summary said some versions of the page differ on the Forward+ requirement (a Deferred+ path is on a Unity roadmap card and not confirmed released). I could not read the 6000.6 enable page directly (404 on the .md URL; the landing page does not state the rendering-path requirement). Check the page for your installed version. Roadmap card: https://activation.unity3d.com/roadmap/2777-deferred-optimisations-gpu-resident-drawer
- When it is worth it (Unity): "most effective in large scenes where many GameObjects share a mesh" and a shader variant, with a high CPU draw-call load; gains are "usually smaller in the Scene and Game views than in Play mode or a built project". Cost: it "reduces CPU load but slightly increases GPU work"; if GPU-bound, frame time can increase; merged instanced draws can increase overdraw on desktop GPUs (use Depth Priming Mode Auto/Forced with Forward+). Source: https://docs.unity3d.com/Manual/urp/gpu-resident-drawer-performance.html
- Unity shows one Chinese-language deck number (URP 10.7 ms vs GRD 7.5 ms, no scene given) - anecdotal; I found no source quantifying GRD overhead in small scenes.
- Static batching must be off to get full GRD benefit (see above); objects with Rigidbody and moving transforms: GRD "updates when you change or create GameObjects at runtime" (same GRD page). I found no source on the cost of moving many GRD-managed objects.

### How to measure

- Rendering Statistics / Stats overlay (Game view, Play mode only): FPS, Global/GPU frametime, CPU Main vs Render thread share, Tris/Verts, **Set Pass Calls** ("how often Unity switches shader passes"), **Draw Calls (Batches)** with a breakdown: SRP Batcher, BRG/GRD, Non-SRP Compatible, Standard Instanced. Caveat: main thread time includes Editor overhead; Estimated Graphics Memory is an estimate. Source: https://docs.unity3d.com/Manual/RenderingStatistics.html
- Frame Debugger: step through rendering events; the Event Information panel has a **Batch cause** row ("reason why the SRP Batcher is unable to batch the selected rendering event with the previous rendering event"). GRD draws show as "Hybrid Batch Group". The Frame Debugger "updates every frame and you can't pause it" (page note), and does not provide GPU timing. Sources: https://docs.unity3d.com/Manual/frame-debugger-window-event-information.html , https://docs.unity3d.com/Manual/FrameDebugger.html , https://docs.unity3d.com/Manual/urp/gpu-resident-drawer-performance.html
- Profiler: CPU Usage timeline plus the Rendering module (batches and SetPass calls per frame). Render-thread-bound signs: main thread waits for the render thread; causes include poor batching and poor culling. Healthy: render thread idling in `Gfx.WaitForGfxCommandsFromMainThread`. GPU-bound: main thread waiting on GPU markers. "The most accurate profiling results always come from running and profiling builds." Source: https://unity.com/how-to/best-practices-for-profiling-game-performance
- Unity says CPU-bound check procedure for draw calls: Rendering Statistics (SetPass calls), Profiler Rendering section, Frame Debugger (choose-method page above).
- Also relevant to Unity docs' wording: "Updating the render state is described as the most CPU-intensive step" of a draw call (https://docs.unity3d.com/Manual/optimizing-draw-calls.html) - this is the cost the SRP Batcher attacks.

## 3. Numbers and rules of thumb

| Number | Source | Quality |
|---|---|---|
| Desktop: aim for under ~2000 draw calls (low/medium-spec PCs); mobile ~200. Main-thread rendering cost under ~5 ms (about a third of a 16.67 ms budget); render thread within total budget. | https://thegamedev.guru/unity-cpu-performance/draw-call-bound/ (author states it is personal opinion and adjustable) | Low-medium: single practitioner, not Unity |
| "PC and console hardware can push a lot of draw calls" but per-call overhead is still worth reducing; mobile is where it is vital. No numeric PC budget. | https://unity.com/how-to/gpu-optimization | Medium: official, but no number |
| SRP Batcher CPU render-loop speedup 1.2x-4x (PC DX11 FPS Sample 1.23x). | https://unity.com/blog/engine-platform/srp-batcher-speed-up-your-rendering | Medium: old, specific scenes |
| Dynamic batching limit: 300 vertices per mesh, 900 vertex attributes total. | https://unity.com/how-to/gpu-optimization | Medium |
| Static batch buffer limit 64,000 vertices. | http://docs.unity3d.com/Manual/DrawCallBatching.html | High |
| Mid-range PC 2000-5000, low-end integrated GPU 500-1000 draw calls before CPU-bound. | Appears in one developer article surfaced via search summary; I could not open it. | Low |
| Frame budget = 1/target FPS (16.67 ms at 60). | thegamedev.guru and Unity profiling page (33.33 ms at 30 fps) | High |

Scale check for this project (UNSOURCED GUESS, simple arithmetic): one room is about 110 items + 20-24 shelves + 10-12 containers x ~10 cubes = roughly 250 to 350 renderers beyond the shelves/walls; with hub and three rooms active that is maybe 1000-1500 renderers. Even at one draw call per renderer plus a shadow pass this is in the neighbourhood of the 2000 rule-of-thumb, not far above it, and the SRP Batcher cheapens each call. With rooms 2-3 inactive until opened, the active count is lower.

## 4. Confidence

- High: SRP Batcher rules and its incompatibility with MaterialPropertyBlock; Unity's explicit URP guidance (SRP Batcher on, instancing checkbox off, static batching off if using GRD); how to read the Stats/Frame Debugger.
- Medium: GRD requirements (version differences; I could not read the 6000.6 enable page); SRP Batcher gain magnitude (2018 data).
- Low: any specific PC draw-call budget (only a single-practitioner "2000" rule; Unity gives no number).
- Unknown: whether this project's URP asset has SRP Batcher on, which Rendering Path (Forward / Forward+ / Deferred) it uses, whether shadows are on and how many shadow casters, whether the book material shader is a standard URP Lit/Unlit (compatible) or a custom/Shader Graph one with extra properties, current Stats numbers, and whether the scene is CPU- or GPU-bound. None of this is measured.

## 5. Implication for BookBoy

Project setup (per brief): each item is a cube-mesh MeshRenderer with one Material per colour/group, a Collider trigger and MonoBehaviours; outline hulls/light/particles created only on highlight.

- **The current design (shared cube mesh, one shared material per colour/group, no MaterialPropertyBlock) is the pattern Unity recommends for URP.** Do not switch to MaterialPropertyBlock to "save materials"; the docs say that would drop performance in URP.
- **Do not bake/merge or statically batch the books**; they are dynamic (picked up, thrown, placed) and static batching is documented as incompatible with GPU Resident Drawer and "Disable" for URP.
- **Likely not a real rendering problem at this scale.** A few hundred small meshes sharing a handful of materials is within the range where the SRP Batcher already makes each draw cheap. Evidence is indirect (2000-draw-call rule of thumb from one practitioner; Unity's "PC can push a lot of draw calls"). Verify with one measurement before changing anything: open Stats in Play mode in the busiest room and look at Batches, Set Pass Calls, and the SRP Batcher vs "Non-SRP Compatible" breakdown; then check the Profiler for render-thread time in a **build**, not the Editor.
- **Smallest useful checks (no scene or settings change):**
  1. In the Stats breakdown, confirm books and furniture land under "SRP Batcher". If anything lands under "Non-SRP Compatible", use the Frame Debugger's Batch cause row. Likely culprits: a runtime MaterialPropertyBlock, a custom shader whose properties are outside `UnityPerMaterial`, or a particle/skinned renderer.
  2. Audit BookOutline and highlight code: if it uses `renderer.material` (clones) or a MaterialPropertyBlock, switch to `sharedMaterial`/dedicated outline material swapping so highlight does not create clones or break the SRP Batcher path. The highlight creates three hull renderers, a light and a particle system: all of that is a per-frame-cost only while highlighted (one item), so it is not a draw-call concern; but the *light* addition is worth a look under Forward rendering (UNSOURCED GUESS: additional lights add per-object cost; not researched here).
  3. Check the colliders and Rigidbody counts separately; they are physics cost, not rendering (see file 12).
- **If measurement shows CPU render-thread time is the bottleneck**, the next step, in order: (a) reduce active renderers (rooms off via SetActive already helps; see file 12); (b) turn on GPU Resident Drawer, which needs Forward+ and disabling Static Batching (both are `ProjectSettings`/URP-asset changes, which CLAUDE.md says need approval and a plan first); expect a benefit only if many renderers share a mesh *and* a material (here: cube + about 30 colours, so batches of ~10 books per colour per room); (c) merge container cubes into one mesh per container (an authoring change; loses per-cube culling but containers are small).
- **Where the current settings could be wrong:** (1) if the URP asset has "SRP Batcher" off (it is on by default; UNSOURCED GUESS that it is on in this project); (2) if the "Enable GPU Instancing" checkbox is on for the colour materials (Unity says to disable in URP; it only adds shader variants); (3) if the material shader is custom and not SRP-Batcher-compatible. All three are one-click checks in the Inspector/Frame Debugger.
- **Where current settings look right:** shared mesh; per-colour material sharing one shader; no MPB; no combining of dynamic items.
