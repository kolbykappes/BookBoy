# 06 - Exposure, tonemapping, bloom and the cozy look in URP

Researched 2026-10-18 for Unity 6000.6 / URP. Web research only. Project values were read (read only) from `Assets/Settings/SampleSceneProfile.asset`, `PC_RPAsset.asset`, `ProjectSettings/ProjectSettings.asset` and `Assets/Scenes/Lamplit.unity`.

## 1. Question

Neutral or ACES for saturated flat colours? How do Bloom threshold/intensity/clamp interact with emissive orbs and bright hotspots? Does URP have auto exposure (and what replaces it)? How do we keep a dark interior readable without washing out coloured books, and what ambient level should we aim for?

## 2. Findings

### Tonemapping
- Unity's description, 6000.6: **Neutral** = "range-remapping with minimal impact on color hue & saturation. It is generally a good starting point for extensive color grading." **ACES** = "a close approximation of the reference ACES tonemapper, for a more cinematic look. It is more contrasted than Neutral, and has an effect on actual color hue & saturation." With ACES, "Unity does all the grading operations in the ACES color spaces". https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/effect-list/tonemapping.md
- Source code (Unity Graphics master, `Color.hlsl`): Neutral is a rational curve "based on Neutral tonemapping (Hable/Hejl/Frostbite)" with constants a=0.2, b=0.29, c=0.24, d=0.272, e=0.02, f=0.3, whiteLevel=5.3, whiteClip=1.0, applied to the float3 RGB (so **per channel**). The fitted ACES path does a glow module, a red modifier, an AP1 conversion, a global desaturation lerp (`RRT_SAT_FACTOR`), the RRT curve, then a dim-surround gamma, a second desaturation (`ODT_SAT_FACTOR`) and conversion to Rec.709. https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl (master, not pinned to 6000.6)
- Independent confirmation (Catlike Coding, custom SRP that mirrors Unity's model): ACES "adds a hue shift to very bright colors, pushing them toward white" and darkens darks, which "enhances contrast"; Neutral is a Hable-style S-curve. Tone mapping alone makes the image "less colorful except when it's very bright". https://catlikecoding.com/unity/tutorials/custom-srp/hdr/ and https://catlikecoding.com/unity/tutorials/custom-srp/color-grading/
- Third-party reports that ACES shifts very bright blues toward turquoise and desaturates bright colours deliberately (search-result snippets, not fetched pages; low confidence): see the results for "ACES hue shift" at https://modelviewer.dev/examples/tone-mapping and https://discourse.threejs.org/t/tone-mapping-overview/75204
- Caution on sources: Narkowicz's popular ACES *fit* is luminance-only and "over saturates brights" by its author's own words, which is **not** what Unity ships (Unity's is the RRT/ODT-style chain above). https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/
- Without tonemapping and with an LDR frame buffer, "color channels get cut off at 1... Extremely bright colors end up looking no different than those what are fully saturated" (Catlike HDR tutorial, URL above). The legacy post-processing manual says the same for HDR cameras: "always apply Tonemapping, or values above 1 are clamped". https://docs.unity3d.com/Packages/com.unity.postprocessing@3.2/manual/Color-Grading.html
- URP asset: **HDR** "the brightest part of the image can be greater than 1... useful if you want... bloom effects". **Grading Mode** HDR "applies color grading before tonemapping"; LDR "applies a limited range of color grading after tonemapping". https://docs.unity.com/en-us/engine/6000.6/manual/render-pipelines/universal-render-pipeline/urp-reference/universalrp-asset.md

### Bloom
- Threshold: "the gamma space brightness value at which URP applies Bloom... default value is 0.9. There is no maximum." **Intensity** 0 to 1, default 0 (off). Scatter 0-1 default 0.7. **Clamp**: "the maximum intensity that Unity uses to calculate Bloom. If pixels in your scene are more intense than this, URP renders them at their current intensity, but uses this intensity value for the purposes of Bloom calculations", default 65472. High Quality Filtering, Filter (Gaussian/Dual/Kawase), Downscale are perf levers. https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/effect-list/bloom.md
- Catlike: "Often the bloom threshold is set to 1 so only HDR colors contribute"; scattering bloom intensity should not exceed 1 ("that would add light"), typically low, example 0.5 with scatter 0.7. Without HDR bloom is weak because colours are clamped before pre-filtering. URL: https://catlikecoding.com/unity/tutorials/custom-srp/hdr/
- Emission: the Lit shader's Emission uses the HDR colour picker's Intensity for "HDR glow brighter than white" (https://docs.unity3d.com/Manual/urp/lit-shader.html), and the standard fix for "emission does not glow" is to turn on Bloom (https://docs.unity3d.com/Manual/ts-emissive-mats-not-rendering.html). The emission-GI setting only affects whether the emissive object lights others (Realtime/Baked/None); it does not light the scene by itself.

### Exposure
- **Post Exposure** (Color Adjustments) "Adjusts the overall exposure of the scene in EV (not EV100). URP applies this after the HDR effect and before tonemapping, which means that it does not affect previous effects in the chain" (so it does not change what Bloom sees). https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/effect-list/color-adjustments.md
- **Auto exposure / eye adaptation is not part of the URP Volume stack** I could find. The only auto-exposure docs found belong to the old Post Processing package (v2/v3). A search snippet claims Unity's public roadmap lists "Automatic Exposure" as Planned (April 2025) with no commitment; I could not re-open that page (404), so treat as unverified. https://docs.unity3d.com/Packages/com.unity.postprocessing@3.2/manual/Auto-Exposure.html (legacy) and https://activation.unity3d.com/roadmap/221-automatic-exposure (unverified)
  - Practical consequence: exposure in URP is fixed; brightness must be authored via light intensities, ambient and Post Exposure.

### Colour grading, vignette
- Color Adjustments: Post Exposure, Contrast, Color Filter (multiplies the render), Hue Shift, Saturation. Lift Gamma Gain: ASC CDL trackballs; Lift = dark tones, Gamma = mids, Gain = highlights. Vignette: Intensity, Smoothness 0.01-1 (default 0.2), Rounded, Center, Color. (Same docs folder: `.../urp/effect-list/lift-gamma-gain.md`, `.../vignette.md`.)
- LUT size 32 "provides a good balance of speed and quality" (URP asset page above); Catlike: 32 is usually enough, 64 if banding.
- Motion Blur in URP only blurs camera motion; Clamp default 0.05; "Camera Only" has better performance. https://docs.unity.com/en-us/engine/6000.6/manual/post-processing-and-full-screen-effects/urp/effect-list/motion-blur.md

## 3. Numbers and rules of thumb

- **Neutral curve output (derived by me from the constants above; linear in, linear out):**

| input | 0.05 | 0.18 | 0.3 | 0.5 | 0.8 | 1.0 | 1.5 | 2 | 3 | 5.3 | 10 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| output | 0.053 | 0.182 | 0.282 | 0.416 | 0.560 | 0.631 | 0.757 | 0.839 | 0.939 | 1.046 | 1.124 |
| sRGB-encoded | 0.26 | 0.46 | 0.57 | 0.68 | 0.77 | 0.82 | 0.88 | 0.93 | 0.97 | 1.0 | 1.0 |

  So Neutral is near-identity below about 0.3, compresses above, and only reaches full white at an input of about 5. Channels are compressed independently, so a saturated colour whose dominant channel is above about 1.5 desaturates (for example (1.2, 0.12, 0.12) becomes about (0.69, 0.12, 0.12), a smaller R:G ratio).
- Bloom threshold 1.0 in the project's setup means any *lit surface* that reaches pre-tonemap value 1.0 blooms, not just emissives (value 1.0 is reached within about 1.5-2 m of an orb on a mid-albedo surface; see doc 05).
- Ambient (UNSOURCED GUESS, no Unity guidance on a target level): for a stylised dark-but-readable interior a common art target is that the *least lit* readable surface lands around 0.05-0.10 linear (roughly sRGB 0.25-0.35) and the key surface (books being sorted) around 0.3-0.6 linear. I found no authoritative number; use Post Exposure and a pixel probe on the frame to decide.
- Current project ambient (read from Lamplit.unity): Trilight sky (0.30, 0.27, 0.34), equator (0.21, 0.18, 0.17), ground (0.08, 0.065, 0.05), intensity 1. If Linear colour space converts these as sRGB colours (believed, UNSOURCED, verify), the sky term is only about 0.07 linear and the equator term about 0.03, i.e. vertical book spines get about 0.015 from ambient at albedo 0.5, nearly black. The room is therefore lit almost entirely by the magelights and HandLight.

## 4. Confidence and unknowns

- Tonemapper behaviour: **high** for Unity's wording, **medium** for the per-channel/desaturation details (read from `master` source, summarised by a fetch model that could not show the whole file).
- Bloom parameter semantics: **high** (6000.6 page). How "gamma space" thresholds map to pre-tonemap linear values: **medium**. The unit is gamma-space brightness; for threshold 1.0 gamma and linear coincide (1.0 = 1.0), which is why threshold 1 equals "above display white". Lower thresholds differ.
- Auto exposure absence: **medium** (absence of evidence plus the roadmap snippet).
- Ambient level targets: **low** (guess).
- Unknown: whether ambient colour fields are gamma-converted in Linear; the exact albedo values on materials; whether the player ever looks at an orb directly (affects the Clamp choice).

## 5. Implication for BookBoy

Current: HDR on, Linear colour space, Grading Mode HDR, Tonemapping **Neutral** (mode 1), Bloom threshold **1**, intensity **0.25**, scatter 0.5, Clamp 65472 (default), Vignette intensity 0.2 / smoothness 0.2, Motion Blur on (intensity 0.6, mode 0), no Color Adjustments override.

- **Neutral is the right tonemapper. Keep it.** Per Unity's wording and the code it keeps hue/saturation better than ACES, which pushes bright colours to white and changes hue. Switching to ACES would work against colour-readable books. (Matches current settings: right.)
- **HDR + HDR grading mode: keep.** No performance reason to drop on a PC target; dropping HDR would clamp everything above 1.0 and hotspots would go flat white.
- **Bloom: threshold 1 is conventional; the weak point is the unbounded hotspot.** A ceiling spot pre-tonemap value of about 20 (doc 05) feeds bloom at full strength. Smallest change: set Bloom **Clamp** to about 3-6 (Unity says the pixel is still rendered at full intensity, only the bloom calculation is capped). Alternatively fix the cause (doc 05). Do not raise Intensity above about 0.3. Optionally raise threshold to 1.2-1.5 so lit walls near orbs do not bloom, and keep orb emission HDR intensity above that (for example 3+) so only orbs bloom. These values are UNSOURCED GUESSES; tune by eye.
- **Do not rely on auto exposure; there is none in URP.** If the room reads too dark after fixing the hotspot, add a **Color Adjustments** override and raise **Post Exposure** by +0.3 to +0.7 EV (guess) rather than raising lights (raising lights re-creates the near-field problem). Post Exposure does not feed Bloom, which is handy.
- **Ambient looks too dark for a readable interior** (see section 3); the cheapest fix is the Lighting window's ambient **Intensity Multiplier** (range 0-8, default 1) or lifting the equator colour. Do this before raising light intensities. Ambient is flat and shadowless, which is on-brand for a flat-colour cozy look.
- Optional cozy grade: small warm **Color Filter** or Lift Gamma Gain tint, a touch of Saturation. Not needed if the lights are already warm; skip until the exposure is right.
- Vignette 0.2: harmless; do not increase while readability is the goal.
- Motion Blur 0.6 on a book-reading first-person game is a readability risk (UNSOURCED opinion; only the Unity description that it blurs camera motion is sourced). Consider turning it off or lowering it.
