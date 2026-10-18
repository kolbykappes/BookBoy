# 09 - Stylised flat-colour materials: smoothness, specular, emission

Researched 2026-10-18 for Unity 6000.6 / URP. Web research only. See also `10-many-small-objects-batching-srp-batcher.md` in this folder (written separately) for batching depth.

## 1. Question

How do we get a flat-colour, low-poly, matte look in URP Lit / Simple Lit / Unlit, why do bright specular-looking hotspots appear on matte surfaces under point lights, how do emissive materials and Bloom interact, what albedo ranges avoid clipping, and what is the trade-off between per-colour materials, vertex colours and textures for hundreds of coloured items?

## 2. Findings

### Lit shader controls
- URP Lit **Smoothness** is 0 to 1: "low values give a wide, rough highlight and high values give a small, sharp one". **Metallic** 0 to 1, 0 non-metallic. Advanced options: **Specular Highlights** ("Adds highlights from direct lights such as Directional, Point, and Spot lights. Disable it for faster shaders") and **Environment Reflections** ("Samples the nearest Reflection Probe or Light Probe set in the Lighting window... Disabling it speeds up shaders but removes reflections"). Both are shader variants ("affects batching"). https://docs.unity3d.com/Manual/urp/lit-shader.html (a 6.4 page; property names match 6000.6)
- A search snippet of the Lit docs puts the Shader Graph Lit default smoothness at 0.5 and describes 0.5 as plastic-like semi-gloss. https://docs.unity3d.com/6/Documentation/Manual/urp/shader-complex-lit.html (unverified: the docs I fetched did not list defaults; treat as a hint)
- **Simple Lit** exposes Specular Highlights, Smoothness (0-1, source Metallic Alpha or Albedo Alpha) and an optional Specular Map (Specular workflow only). The Unity page does not state its lighting model. https://docs.unity3d.com/Manual/urp/simple-lit-shader.html The URP shader library does contain a Blinn-Phong path (`CalculateBlinnPhong`, `LightingSpecular` using `NdotH ^ smoothness`), which is where Simple Lit's non-PBR look comes from (inference from Lighting.hlsl; I did not read Simple Lit's shader). https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl
- Unity says to "disable unused features" for performance and to "use as few shader variants as possible" for SRP Batcher efficiency (Simple Lit page above; SRP Batcher page https://docs.unity3d.com/Manual/SRPBatcher.html).
- **Unlit** materials get no lighting and "Unity can't calculate shadows for GameObjects that have materials with 'Unlit' type shaders" (the shadow-troubleshooting page, which is partly written for Built-in). https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/shadows/shadow-performance.md

### Where specular comes from (read from URP code)
- URP's non-metal specular colour is `kDielectricSpec` = 0.04 ("standard dielectric reflectivity coef at incident angle (= 4%)"); `diffuse = albedo * oneMinusReflectivity`; specular uses a GGX-like `roughness2 / (d*d * max(0.1, LoH^2) * normalizationTerm)` with `normalizationTerm = roughness*4 + 2`. No 1/pi anywhere. https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl and Lighting.hlsl above (master branch).
- Derived peak specular term (NoH = 1, LoH = 1) times the 0.04 specular colour, relative to the diffuse term (albedo times 1):

| smoothness | perceptual roughness | peak specular x 0.04 | note |
|---|---|---|---|
| 0.2 | 0.8 | 0.021 | about 2% of a mid albedo diffuse |
| 0.5 | 0.5 -> roughness 0.25 | 0.21 | comparable to a dark albedo; visible highlight |
| 0.8 | 0.2 -> roughness 0.04 | 11.6 | enormous, tiny, bright hotspot |

  (Derived by me. `roughness = (1 - smoothness)^2` and perceptual conversion assumed standard; the conversion function `PerceptualSmoothnessToPerceptualRoughness` was not shown in the files read, so this is **medium** confidence.)
- Combined with the 1/d^2 near-field (doc 05: 21.7 at 0.45 m for I = 4.4), the **diffuse** term of a mid-albedo ceiling is about 10.9 vs a specular contribution of about 0.5 at smoothness 0.2. The orb-top hotspot is therefore mainly diffuse brightness plus Bloom, not shininess.

### Emission and Bloom
- Lit Emission Map default black; the colour picker's Intensity gives "HDR glow brighter than white"; Global Illumination for emission is Realtime/Baked/None and only decides whether the emissive object lights neighbours. Unity's fix list for "emissive doesn't glow" is: turn on Bloom. https://docs.unity3d.com/Manual/urp/lit-shader.html and https://docs.unity3d.com/Manual/ts-emissive-mats-not-rendering.html
- Bloom threshold is gamma-space brightness, default 0.9; "Often the bloom threshold is set to 1 so only HDR colors contribute". An emissive colour with HDR intensity above the threshold blooms; raising Clamp lowers its bloom contribution. See doc 06. https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/effect-list/bloom.md and https://catlikecoding.com/unity/tutorials/custom-srp/hdr/
- Unity's emission page confirms an emissive mesh does **not** light other objects in real time unless it is a GI contributor with baked/realtime GI (see the "Global Illumination" notes above), so a visible glow on the ceiling from an emissive orb would need Bloom or the point light, not the emission.

### Albedo values
- Unity's Scene view **Material Validator** has a "Validate Albedo" mode (red = too dark, blue = too bright) with luminance presets; the docs I read do not print the default min/max values. https://docs.unity.com/en-us/engine/6000.6/manual/materials-and-shaders/materials/troubleshooting/material-validator
- Common practice for physically based albedo is roughly "avoid pure black and pure white", often quoted as about 30-240 in sRGB (0.012 to 0.9 linear). **UNSOURCED GUESS**: I could not find the Unity page that prints these numbers.

### Many coloured items
- SRP Batcher batches by **shader variant, not material**: "You can still use as many different materials with the same shader as you want." https://docs.unity3d.com/Manual/SRPBatcher.html
- MaterialPropertyBlocks "break SRP Batcher compatibility" (Unity docs), but allow per-object colour with GPU instancing; GPU instancing only batches objects sharing the same Mesh **and** Material. https://docs.unity3d.com/Manual/gpu-instancing-change-data.html . One self-reported benchmark (Unity 6000.2, URP, 100,000 cubes) went from about 103,000 batches to 157 with instanced MPB colour. https://dev.to/chris_majcher_de2b3fa94ff/series-unity-gpu-instancing-learning-out-loud-part-1-of-4-1dgf (single, unreviewed test)
- Vertex colour on URP Lit: no Unity doc found saying Lit reads vertex colour. The community workaround is a Shader Graph with a Vertex Color node wired into Base Color. https://community.gamedev.tv/t/changing-the-color-on-vertex/233521 (community post)

## 3. Numbers and rules of thumb

- Smoothness 0-0.2 keeps URP's specular term tiny (derived: about 0.02 relative to diffuse); smoothness around 0.5 is the plastic range; above 0.8 the term explodes at the highlight peak.
- Diffuse near-field (derived, doc 05): value = albedo x I / d^2. Hotspot = albedo x 21.7 for I = 4.4 at 0.45 m.
- Pre-tonemap pixel value above about 1 starts to desaturate saturated colours under Neutral (doc 06 table); above about 3 it is nearly white.
- Bloom threshold 1 and emission intensity 3+ is a commonly used pair for "only orbs glow" (UNSOURCED GUESS for the 3).

## 4. Confidence and unknowns

- Lit control semantics: **high**. Specular numbers: **medium** (code reading plus my arithmetic). Albedo ranges: **low** (unsourced). Vertex-colour on Lit: **low** (community only).
- Unknown: which shader the project's materials use (the brief says "URP Lit with low smoothness"), the actual smoothness value, whether Environment Reflections sample a default skybox that could add sheen to smooth surfaces (the Lighting window defaults Environment Reflections source to Skybox; UNSOURCED whether it shows indoors).

## 5. Implication for BookBoy

- **Low smoothness on Lit is right.** The derived GGX term at smoothness 0.2 is under 0.5 of the 21.7 hotspot; the ceiling spot is overwhelmingly diffuse. If the hotspot looks "specular", it is Bloom plus near-field (fix in doc 05: orb height, ceiling albedo/exclusion, Bloom Clamp in doc 06). Reducing smoothness further will not remove it.
- **Smallest cleanup for a flat look:** on the shared material set, set Smoothness 0, Metallic 0, and untick **Specular Highlights** and **Environment Reflections** (both are Unity-documented as speed-ups; every material shares the same variant so SRP Batcher stays happy). This removes any chance of a glint on books and the cost of probe sampling. Do it consistently on all materials; a mixed set increases variants.
- **Stay on URP Lit.** Simple Lit's page does not promise anything useful for this look, and Unlit loses lighting and shadows (the reason books must read as lit objects). Switch only if profiling shows Lit is slow, which is unlikely at this scale.
- **Orb emission:** keep an HDR emission colour with Intensity high enough to exceed the Bloom threshold (for example 3+), and do not rely on it to light anything. Keep the Bloom threshold above ordinary lit surfaces (doc 06) so books do not glow.
- **Albedo:** keep book colours away from 0 and 1: UNSOURCED GUESS of a floor of roughly 0.05 linear and a ceiling of roughly 0.8-0.9 linear so colours survive Neutral tonemapping, with letters on the spine in a contrasting value rather than a contrasting hue only. Use the Scene view Material Validator > Validate Albedo to check the actual set.
- **Many coloured books:** the SRP Batcher page explicitly allows many materials on one shader, so **one material per colour on the shared Lit shader** is the smallest change and keeps SRP batching. Avoid MaterialPropertyBlocks (breaks SRP Batcher) unless you also move to GPU instancing with same-mesh objects. A palette texture or Shader Graph with vertex colours is the lean alternative but adds custom shader work; not worth it for the prototype. See `10-many-small-objects-batching-srp-batcher.md`.
