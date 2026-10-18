# 07 - Ambient, GI, lightmaps, probes and light count

Researched 2026-10-18 for Unity 6000.6 / URP. Web research only. Project values read (read only) from `Assets/Settings/PC_Renderer.asset`, `PC_RPAsset.asset`, `ProjectSettings/QualitySettings.asset`, `ProjectSettings/ProjectSettings.asset`, `Assets/Scenes/Lamplit.unity`.

## 1. Question

For a small interior game (hallway plus three 12 x 11 m rooms, about 3 warm point lights per room that bob and flicker by script): baked vs realtime vs mixed lighting, light probes for dynamic objects, how to set ambient (Trilight/Gradient/Skybox), URP's per-object and per-camera light limits (Forward vs Forward+), and whether realtime point lights are an acceptable choice.

## 2. Findings

### Light modes
- **Realtime**: calculated at runtime every frame, shadows up to Shadow Distance; "Their runtime cost can be high, depending on the complexity of the scene." They suit moving objects and lights whose properties change at runtime, "for example, to flicker a light bulb". https://docs.unity3d.com/6000.4/Documentation/Manual/LightModes-introduction.html (6.4 page; the 6000.6 equivalent is `.../lighting-light-sources/components/configuring/light-modes.md`, not fetched)
- **Baked**: "no runtime cost to process these lights"; you "cannot change baked lights at runtime"; dynamic GameObjects "do not receive light or shadow from Baked Lights". Same page.
- **Mixed**: "same performance impact as real-time lights"; you can change properties at runtime "but only the real-time lighting updates, not the baked lighting". Same page.
- When **Baked GI is disabled** in the Lighting Settings, "Unity forces all Baked and Mixed lights in the Scene to act as though they were Realtime Lights". https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting-reference/class-lighting-settings.md (as summarised by a search snippet; page not re-read in full)
- **Lighting Mode (Mixed lights only)**: Baked Indirect = realtime direct + realtime shadows, baked indirect; Shadowmask = baked static shadows (max 4 lights per object/texel); Subtractive = "simple lighting and low-fidelity shadows... low-end hardware, simple scenes with few lights, or stylized art such as cel shading", real-time shadows only from the one brightest Directional Light. https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting-light-sources/components/configuring/lighting-mode/lighting-mode.md
- Forum evidence (community, unofficial): baked lightmaps suit lights that never change, realtime suits animated lamps; one interior developer found baked best for performance but non-static objects then cast no shadows onto the baked map. https://discussions.unity.com/t/best-way-to-do-interior-only-lighting/1611552 ; a Unity staff reply suggested realtime lights without bounces as a cheaper alternative, or Baked Indirect for bounces only. https://discussions.unity.com/t/unite-now-harnessing-light-with-urp-and-the-gpu-lightmapper/793084
- Unity 6 removed the Enlighten *baked* backend (Progressive Lightmapper replaces it); Enlighten *Realtime* GI was still available in 6.0 but is deprecated. https://docs.unity.cn/Manual/UpgradeGuideUnity6.html (search snippet only; status in 6000.6 unverified)

### Light probes / ambient probe
- Moving objects cannot use lightmaps; "moving objects do not receive bounced light from your static environment unless you use light probes". The probes store baked bounced information; realtime/mixed lights still light moving objects directly without probes. https://docs.unity3d.com/6000.4/Documentation/Manual/LightProbes-MovingObjects.html
- URP supports Light Probe Groups (legacy, per object) or Adaptive Probe Volumes (per pixel, auto-placed); selection is in the URP asset, Lighting section. https://docs.unity.com/en-us/engine/6000.6/manual/render-pipelines/universal-render-pipeline/urp-reference/universalrp-asset.md and the APV intro https://docs.unity.com/en-us/engine/6000.7/manual/lighting-overview/lighting/probevolumes/concept (6000.7 page)
- With nothing baked, the **ambient probe** is the fallback: "It is used as the last fallback if light probes or lightmaps are not present or enabled for an object." https://docs.unity3d.com/ScriptReference/RenderSettings-ambientProbe.html (6.3 page)

### Ambient
- Lighting window > Environment > Environment Lighting **Source**: Skybox (default), **Gradient** ("separate colors for ambient light from the sky, horizon and ground, and blend smoothly"), or Color (flat). **Intensity Multiplier** 0 to 8, default 1. https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting-reference/lighting-window.md
- The scripting enum `AmbientMode` has Skybox, **Trilight**, Flat, Custom; "Trilight ambient lighting" = the sky/equator/ground Gradient. https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/rendering/ambientmode.md (the Trilight = UI "Gradient" mapping is my inference from the two pages.)
- Unity's ambient page: ambient light "can be useful... bright, cartoon-style rendering where dark shadows may be undesirable... Ambient light can also be useful if you need to increase the overall brightness of a scene without adjusting individual lights." https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting-light-sources/lighting-ambient-light.md
- A game-lighting write-up recommends faint, hand-placed "invisible" fill lights with specular flattened when not baking GI (blog on a GDC 2018 talk, secondary). https://www.blog.radiator.debacle.us/2018/03/gdc-2018-how-to-light-level-slides-and.html

### URP light limits
- **Forward**: per object 1 Main Light + 8 Additional (per-pixel by default). Per camera: 256 additional on desktop/console, 32 mobile, 16 on OpenGL ES 3.0 and older. Beyond that, use Forward+ or Deferred. https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/light-limits-in-urp.md (numbers read from the 6.4/6.6 pages)
- **Forward+**: no per-object limit, still a per-camera cap; the screen is divided into tiles and each object uses only the lights in its tile. Forward+ ignores the URP asset's Additional Lights, Main Light and Per Object Limit settings. https://docs.unity.com/en-us/engine/6000.6/manual/render-pipelines/universal-render-pipeline/introduction/urp-concepts/rendering-paths/forward-rendering-paths.md
  - Per-camera cap on desktop is 256 and **includes the Main Light** under Forward+; it drives shader compile time and can be lowered by editing a copied config package (Unity calls it a design limitation to be mitigated "in one of the future Unity versions"). https://docs.unity3d.com/Manual/urp/rendering/forward-plus-rendering-path-limitations.html (6.4 page)
  - If visible lights exceed the cap, "Unity disables some lights", which can include a shadow-casting one and cause flicker. https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/shadows-in-urp/shadows-troubleshooting-urp.md
  - The Rendering Debugger "Lighting Complexity" view shows lights per tile.
- Meta's Quest guidance (VR, not desktop): Forward+ "start[s] to perform better at around 5 lights"; for "scenes made of very small meshes" or few dynamic lights, stay with Forward; test both. At 30 lights Forward about 12 ms vs Forward+ about 8.5 ms in their sample. https://developers.meta.com/horizon/documentation/unity/unity-forward-plus-rendering/
- I found **no** published per-light cost for desktop URP. A search turned up only that Meta page and academic Forward+ papers.

## 3. Numbers and rules of thumb

- Forward: at most 9 lights (1 main + 8 additional) per object; 256 per camera on desktop. Forward+: 256 per camera including the main light on desktop. (Unity docs above.)
- Point light shadow = 6 shadow maps; an atlas of 2048 x 2048 holds 16 maps of 512 x 512 (Unity URP shadow-resolution page, see doc 08).
- BookBoy's current counts (from the brief): per room 3 magelights + HandLight + directional main light = 5 in a room; adjacent rooms can overlap through a 10 m range, so maybe 5-9 visible lights in a doorway view. Well below any cap in either path.
- Realtime non-shadowed lights: UNSOURCED GUESS that a handful (under about 10) with no shadows is negligible on a desktop GPU; no benchmark found. Measure with the Frame Debugger/Profiler.

## 4. Confidence and unknowns

- Light mode behaviour and URP limits: **high** (Unity docs; 6.4 and 6.6 pages agree on numbers).
- Probes/ambient probe: **medium** (pages are 6.3/6.4, not 6000.6, and not URP-specific for the moving-object page).
- Realtime-light cost for this scene: **low** (no benchmark, no profiling done). Unknown: shader variant counts and build time effect of Forward+ in this project (Unity warns about compile time at 256).
- Unknown: whether Enlighten Realtime GI is enabled in the project (I did not check LightingSettings; the scene uses no baked data as far as the brief says).

## 5. Implication for BookBoy

- **Realtime point lights are an acceptable choice, and effectively the only one for these lamps.** The lamps bob and flicker by script; Unity states baked lights cannot change at runtime and Mixed lights only update their realtime part. Keep them Realtime. Counts are far under every cap. (Matches current setup: right.)
- **Do not bake the rooms for the prototype.** Baking would remove the bounce the brief does not have anyway, requires lightmap UVs on all level meshes, and moves flicker out of reach. Revisit only if a profiler shows lighting cost; a Baked Indirect Mixed setup would bake bounces only, but then the flickering lamps would need to be a dynamic-only effect. UNSOURCED GUESS: bounce probably is not worth the effort for flat-colour low-poly rooms.
- **Current renderer is already Forward+** (`PC_Renderer.m_RenderingMode: 2`), so the "Per Object Limit 4" in the PC URP asset is ignored. With 5-9 lights, Forward would also be fine; Forward+ is not required. Meta's note that Forward can win with very small meshes and few lights is a hint to A/B profile once, **not** a reason to change now. Smallest change: leave it.
- **Ambient is the main lighting lever you control cheaply.** Trilight (sky 0.30/0.27/0.34, equator 0.21/0.18/0.17, ground 0.08/0.065/0.05, intensity 1) is a sensible stylised setup. Unity's own ambient page endorses ambient for stylised looks where dark shadows are undesirable. It looks dark (see doc 06); raise via the Intensity Multiplier or equator colour if shelves read too dark, instead of raising light intensities.
- **Light probes: not needed** with fully realtime lights and no baked data, because dynamic objects (carried books, orbs) already receive direct realtime light and ambient (ambient probe is the documented fallback). Probes only add baked bounce, and there is none. Skip them.
- **Directional light 0.5 still in scene**: it is the Main Light. If it does not cast shadows it lights interior surfaces *through* the ceiling as a flat one-direction fill (UNSOURCED but follows from how unshadowed lights work: nothing occludes it); if it casts shadows it costs a main-light shadow pass (see doc 08) and the ceiling blocks it. Either way it is an invisible extra term in the lighting balance. If it is a leftover, turning it off (or lowering it) is a scene-only change; decide whether you want it as deliberate fill.
- Keep the per-camera visible-light cap in mind only if you later add many lights (256 incl. main under Forward+).
