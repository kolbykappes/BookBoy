# 08 - Shadows in URP interiors

Researched 2026-10-18 for Unity 6000.6 / URP. Web research only. Project values read (read only) from `Assets/Settings/PC_RPAsset.asset` (the active asset: `ProjectSettings/QualitySettings.asset` `m_CurrentQuality: 1` = PC) and `Assets/Settings/PC_Renderer.asset`.

## 1. Question

Which URP shadow settings matter for a small interior (shadow distance, cascades, bias, soft shadow quality, point-light cost and the shadow atlas), how do we avoid acne/peter-panning with tiny objects such as books on shelves, what can be turned off, and what are cheap contact-shadow/AO tricks for shelves?

## 2. Findings

### Cost
- Point light shadow: "To create a shadow map for a point light, Unity captures the scene in six directions. The performance impact is similar to rendering shadows from six spot lights, and increases the number of draw calls significantly." Spot light shadows are "significantly faster". https://docs.unity.com/en-us/engine/6000.5/manual/lighting-overview/shadows/shadow-realtime/shadow-mapping.md and https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/shadows-in-urp/shadows-optimization.md
- Factors that set shadow render time (Unity's list): visible shadow casters (depends on Max Distance), visible receivers, number of shadow-casting lights, Cascade Count and splits, shadow resolution of main and additional lights, Soft Shadows. Same optimization page.
- The 6000.6 "Adjust settings to improve performance in URP" table: Additional Lights > Cast Shadows = Disable (if you can), Shadow Atlas Resolution / Shadow Resolution = lowest acceptable, Cascade Count = lowest acceptable, Conservative Enclosing Sphere = Enable, Max Distance = Reduce, Soft Shadows = Disable or Low, Main Light > Cast Shadows = Disable. https://docs.unity.com/en-us/engine/6000.6/manual/analysis/graphics-performance-profiling/in-urp/optimize-for-better-performance.md
- Soft shadow quality in the URP asset: Low = 4 PCF taps (mobile), **Medium = 5x5 tent filter (desktop default)**, High = 7x7 tent filter ("best quality, higher performance impact"). The performance impact is high on tile-based GPUs (mobile/XR); on desktop Unity says soft shadows "only affect the GPU". https://docs.unity.com/en-us/engine/6000.6/manual/render-pipelines/universal-render-pipeline/urp-reference/universalrp-asset.md and https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/shadows/shadow-performance.md
- Per-light controls (Light inspector, URP): Strength 0-1; Bias = Use Pipeline Settings or Custom (Depth, Normal); Near Plane 0.1-10 default 0.2 clamped to 0.1 or 1% of Range; Soft Shadows Quality per light. https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/light-component.md

### Atlas
- URP uses **one shadow atlas for all spot and point lights** and another for directional. "An atlas of size 1024 x 1024 can fit four shadow maps of 512 x 512, or sixteen of 256 x 256." A point light needs **six** maps: Unity's example needs ten maps for four spot lights plus one point light, and a 512 atlas "would not be enough" for 256-pixel maps; they need 1024. The atlas size is the maximum resolution of additional light shadows. https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/shadows-in-urp/shadow-resolution-urp.md
- Shadow Resolution Tiers: "Resolutions must have a value of 128 or greater, and are rounded to the next power of two." (URP asset reference above.)
- Main light: "Unity spreads the shadow map over the area defined by Max Distance. The higher the value, the lower the pixel density." Unity's example: Max Distance 40 with 2048 vs Max Distance 10 with 1024 (the latter looks sharper near the camera). Cascade settings "only affect the main light". Same resolution page and URP asset page.

### Bias, acne, peter-panning
- Unity says the shadow bias can remove "shadow acne, shadow detachment (also known as peter-panning), light leaking, and self-shadowing". In URP the defaults are in the URP asset Shadows section and can be overridden per light (Bias = Custom: Depth, Normal). High values "might result in light leaking through meshes... a visible gap between the shadow and its caster". https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/shadows-in-urp/shadows-troubleshooting-urp.md
- Too-high **Depth Bias** makes shadows look "disconnected" (object appears to float); too-high **Normal Bias** makes the shadow "too narrow for the GameObject" and can cause light bleeding; a possible fix for bleeding is the Mesh Renderer's Cast Shadows = **Two Sided** (more expensive). Unity: "It is generally easier to gauge the right value by eye rather than attempting to calculate it." https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/shadows/shadow-performance.md (page is partly written for the Built-in pipeline)
- Shadow acne also arises from shadow filtering or a low-resolution map (same page). That means **soft-shadow filter width plus low tier resolution** worsen acne on small thin objects.
- No Unity page gives numeric bias ranges for small objects. The project's current values: Depth Bias 0.1, Normal Bias 0.5 (PC asset; Unity's template default is 1/1; I did not find the template default sourced, only observed it in the Mobile asset).

### Flicker from the light cap
- Under the per-camera cap Unity "disables some lights. It might disable a real-time shadow-casting light, which causes shadow flickering." Only relevant beyond hundreds of lights on desktop. (troubleshooting page above)

### Cheap contact shadow / AO
- **SSAO Renderer Feature** (URP): "darkens creases, holes, intersections and surfaces that are close to each other"; independent of Volumes; uses the DepthNormals pass. Properties and cost labels from the reference: Method (noise, insignificant), Intensity (insignificant), **Radius (High cost)**, Falloff Distance, **Direct Lighting Strength** ("how visible the effect is in areas receiving direct light"), Source Depth Normals, Normal Quality (Depth source only), **Downsample (very high saving: quarter pixel count)**, **After Opaque (medium; can over-darken areas with baked/real-time occlusion)**, **Blur Quality (very high cost; High=Bilateral 3 passes, Medium=Gaussian 2, Low=Kawase 1)**, **Samples** High 12 / Medium 8 / Low 4 ("from 4 to 8 doubles the GPU load"). https://docs.unity3d.com/Manual/urp/ssao-renderer-feature-reference.html and https://docs.unity3d.com/Manual/urp/post-processing-ssao.html (6.3 pages)
- **Decal Projector blob shadows**: Unity ships a URP package sample (`Decals/BlobShadow`) that fakes a soft shadow under an object with a Decal Projector and says this is a common technique where real-time shadows are too expensive. https://blog.unity.com/technology/explore-the-latest-package-samples-for-the-universal-render-pipeline (snippet-level; the sample needs the Decal Renderer Feature)
- **Occlusion Map** in URP Lit: documented as simulating shadows from ambient light and reflection (less light in crevices). Whether its strength also dims direct light in URP is **not** confirmed by what I found; Catlike's custom shader applies it to direct and indirect. https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.1/manual/lit-shader (older package page) and https://catlikecoding.com/unity/tutorials/rendering/part-10
- Using a simplified mesh or a "Shadows Only" proxy to cast the shadow, and excluding objects from casting (Cast Shadows Off), are Unity-documented optimisations (optimization page above). Using a simplified mesh needs the caster's original to have Cast Shadows Off.
- Using cookies instead of shadows in mostly-static areas is Unity-documented (optimization page).
- Baked AO into vertex colours or textures for the shelves: UNSOURCED GUESS (common practice, no Unity source read).

## 3. Numbers and rules of thumb

- A shadow-casting point light = 6 shadow maps. A 2048 atlas holds 16 maps of 512 or 64 of 256 (my arithmetic from Unity's 1024 example). Three simultaneously visible shadow-casting point lights at 512 need 18 maps = more than a 2048 atlas holds (16), so something must shrink or be skipped. Exact URP fallback behaviour on overflow is UNSOURCED (not found in docs).
- Unity default Soft Shadow quality for desktop = Medium (5x5 tent). The project is on **High** (7x7).
- Shadow near plane default 0.2 (clamped to min of 0.1 and 1% of Range = 0.1 for a 10 m light). For a lamp 0.45 m below a ceiling that is not an issue.

## 4. Confidence and unknowns

- Cost and atlas rules: **high** (Unity 6000.6 pages).
- Bias guidance: **medium**: sourced text is qualitative (by eye); the 6000.6 "shadow-performance" page is shared with Built-in.
- AO/blob shadow: **medium** for existence of the features, **low** for tuning numbers.
- Unknown: which magelights and which main light actually have shadows enabled in the built scene (the brief says one soft-shadowed point light per room and the directional at 0.5); whether books receive/cast shadows in practice; what the atlas does on overflow.

## 5. Implication for BookBoy

Current (PC asset): main-light shadows on, 2048; **4 cascades, Max Distance 50**; additional-light shadows on, atlas 2048, tiers 256/512/1024; Soft Shadows on, quality **High**; Depth Bias 0.1, Normal Bias 0.5; Conservative Enclosing Sphere on; SSAO feature on (Intensity 0.4, Radius 0.3, DirectLightingStrength 0.25, Depth Normals source, default samples/blur).

- **Wrong/oversized for a 12 x 11 x 3 m interior:** 4 cascades over 50 m is built for outdoor terrain. Cascade settings affect only the main light. If the directional light does not cast shadows (or is removed, doc 07), this costs nothing and can be ignored; if it does, drop to 1 cascade and Max Distance to about 15-20 (Unity's own example of 40 to 10 sharpens shadows); this is a URP asset change (not a ProjectSettings change), but get approval per CLAUDE.md guardrails since it touches render-pipeline settings. Recommendation: do nothing until you confirm the directional light is shadowed.
- **Keep the shadow count to what the eye needs.** One shadowed point light per room is a sensible budget (6 maps each). Make sure only 2 can be visible together, or raise the atlas to 4096 (memory 4096^2 depth is 64 MB at 32-bit; arithmetic, not Unity-sourced), or lower the lights' tier so 3 x 6 = 18 maps fit (at 256 tier a 2048 atlas holds 64 maps). Prefer lowering shadow tier on the room lights to Medium/Low over a bigger atlas.
- **Soft Shadows High is not necessary.** Unity's desktop default is Medium and its perf guidance is Disable or Low; High adds filter width that increases acne on small casters (Unity: filtering is one cause of acne). Smallest change: set quality to Medium in the URP asset (render-pipeline setting; needs approval). Not urgent for a desktop GPU.
- **Books on shelves:** do not give them their own shadow casters from the point lights unless a tester can see the difference; Unity's list says the number of casters is a main cost driver. Smallest option: shelf mesh and books Cast Shadows On only for the lit room lamp, small props Off. Try the existing bias first, adjust Normal Bias in 0.1 steps looking at a close-up of a book spine; the two failure modes to watch for are acne stripes (raise Depth Bias slightly) and floating books (lower it). Keep Normal Bias moderate; high values thin the shadow of thin objects such as book spines.
- **SSAO is already on and tuned small (Radius 0.3 matches shelf-scale crevices).** It gives the "contact shadow" feel for free. Costs: Radius and Blur Quality are the expensive properties; the project uses defaults, which is acceptable on desktop. If a profile shows SSAO as heavy, Downsample on first. Unity warns that After Opaque can over-darken areas with baked occlusion (not applicable).
- **Cheap extra contact shadow (optional, not recommended now):** decal-projector blob shadows under carried books while in hand; adds the Decal Renderer Feature. Skip unless a book "floating" problem is visible.
