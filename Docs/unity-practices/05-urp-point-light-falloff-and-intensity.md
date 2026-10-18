# 05 - URP point light falloff and intensity

Researched 2026-10-18 for Unity 6000.6 / URP. Web research only; nothing in Unity was touched. Project values quoted below were read from `Assets/Settings/*.asset` and `Assets/Scenes/Lamplit.unity` (read only).

## 1. Question

How do URP point lights attenuate, what unit is "Intensity" in URP, why does a close light blow out, what is standard practice for lights near the camera (headlamps) and near ceilings, and how should BookBoy's magelights (intensity 4.4-5.0, range 10 m, 2.55 m up, 3.0 m ceiling) and HandLight be set?

## 2. Findings

- **URP intensity is unitless in 6000.6.** The 6000.6 URP Light inspector reference lists Intensity under "Emission" as "Set the brightness of the light" with defaults 0.5 (Directional) and 1 (Point/Spot/Area). It has no Unit selector (no lumen/candela/lux/EV100). The only physical unit on the page is Kelvin for Temperature. https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/light-component.md
  - The scripting enum `LightUnit` (Lumen, Candela, Lux, Nits, Ev100) exists, but its own docs say "Only Scriptable Render Pipelines use this property" and the physical-unit workflow is documented for HDRP, not URP. https://docs.unity3d.com/ScriptReference/Light-lightUnit.html and https://docs.unity3d.com/Packages/hdrp/manual/Physical-Light-Units.html
  - So "default Light units" in BookBoy means a plain scalar multiplier on light colour. There is no "Light Appearance"-driven physical mode to switch on in URP. "Light Appearance" (Color vs Filter and Temperature) only changes how the light colour is specified. Same 6000.6 page.
- **Attenuation is inverse-square times a range window.** In the URP shader library `DistanceAttenuation` starts from `rcp(distanceSqr)` (1/d^2), multiplies by `smoothFactor = saturate(1 - (d^2/R^2)^2)` and squares that. The comments say it "matches Unity Vanilla HINT_NICE_QUALITY attenuation" and uses "the smoothing factor also used in the Unity lightmapper". https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl (master branch, not pinned to 6000.6; see confidence).
  - The C# side confirms the formula in a comment: `smoothFactor = (1.0 - saturate((distanceSqr * 1.0 / lightRangeSqr)^2))^2`, with attenuation `x = 1/lightRange^2`. https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/Runtime/UniversalRenderPipelineCore.cs
  - Unity's own custom-falloff tutorial replaces this function with a different one and does not print the stock code, so it cannot be used to confirm the stock formula. https://docs.unity3d.com/Manual/urp/lighting/custom-lighting-change-light-falloff.html
- **There is no near-field clamp.** The stock function has no epsilon or minimum distance: 1/d^2 grows without bound as d shrinks. (Read from the function above; no clamp appears.) Epic's UE4 model avoids this with a `+1` in the denominator and a different window, as quoted second-hand: `falloff = saturate(1-(d/r)^4)^2 / (d^2+1)`. This is a secondary source (forum quote), not the Karis paper text. https://gamedev.net/forums/topic/664177-quadratic-attenuation-for-radius/5200673/ and https://cgvr.informatik.uni-bremen.de/teaching/cg_literatur/Brian%20Karis%20-%20Real%20Shading%20in%20Unreal%20Engine%204,%202013%20Siggraph%20Course.pdf
- **Diffuse has no 1/pi.** `LightingPhysicallyBased` computes `radiance = lightColor * (lightAttenuation * NdotL)` and returns `brdf * radiance`; `InitializeBRDFData` sets `diffuse = albedo * oneMinusReflectivity` with no division by PI. https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl and https://raw.githubusercontent.com/Unity-Technologies/Graphics/master/Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl
  - Consequence (derived by me from those two files): a surface facing a point light of intensity I at distance d returns pixel energy `albedo * I / d^2` (times the range window). Intensity 1 at 1 m on albedo 1.0 gives 1.0.
- **Unity forum consensus on "URP light is way too bright up close":** URP's falloff is more physical than the old Built-in one; recommended responses were Linear colour space plus Tonemapping, lowering intensity/moving the light back, and custom falloff. Nobody reported a built-in switch. https://discussions.unity.com/t/point-light-becomes-way-too-bright-when-i-switch-to-urp/765728 (community, unofficial)
- **Light Layers (Rendering Layers).** Enable in the URP asset: Lighting > More (three dots) > Advanced Properties > Use Rendering Layers. Then Light > Rendering > Rendering Layers and Mesh Renderer > Additional Settings > Rendering Layer Mask pick which lights hit which objects. Custom Shadow Layers is separate and is only available for Mixed/Baked lights. https://docs.unity.com/en-us/engine/6000.6/manual/lighting-overview/lighting/rendering-layers/lights.md
- **Culling Mask** (Light > Rendering) "Use this to selectively exclude groups of objects from being affected by the light" using ordinary Unity layers. Same 6000.6 Light reference. (Note: project rules forbid editing layers/tags without approval; Rendering Layers are also project settings.)
- **Shadow Near Plane** is clamped to "0.1 units or 1% of Range, whichever is lower", default 0.2. Same 6000.6 Light reference. Relevant only to shadowed lights.

## 3. Numbers and rules of thumb

All numbers below are **derived** by me from the formulas above (source: RealtimeLights.hlsl, Lighting.hlsl, BRDF.hlsl, master). Verify with the Frame Debugger before relying on them.

- Window factor `W(d) = (1 - (d/R)^4)^2`: W = 0.88 at d = R/2, 0.35 at d = 0.8R, 0 at R. So the *useful* reach of a light is roughly 0.5-0.7 R, not R.
- Magelight (I = 4.4, R = 10), pixel energy before albedo, face-on:

| distance | 0.45 (ceiling above orb) | 0.75 | 1.0 | 1.5 | 2.0 | 2.55 (floor) | 3 | 5 | 8 | 10 |
|---|---|---|---|---|---|---|---|---|---|---|
| value | 21.7 | 7.8 | 4.4 | 1.95 | 1.10 | 0.67 | 0.48 | 0.155 | 0.024 | 0 |

- Ceiling-to-floor ratio for a lamp at 2.55 m under a 3.0 m ceiling: (2.55/0.45)^2 = about 32 to 1. This is the cause of the "hotspot above each orb": it is mostly **diffuse** near-field 1/d^2, not specular.
- A facing surface hits pixel value 1.0 at `d = sqrt(I * albedo)`: for I = 4.4-5.0 and albedo 0.3-0.8 that is 1.15-2.0 m. Anything facing an orb within about 2 m exceeds 1.0 before tonemapping.
- HandLight old (I = 2.0, R = 2.6) vs new (I = 0.6, R = 2.8), value at distance from the light:

| distance | 0.4 | 0.5 | 0.7 | 0.9 | 1.5 |
|---|---|---|---|---|---|
| old | 12.5 | 8.0 | 4.0 | 2.4 | 0.70 |
| new | 3.7 | 2.4 | 1.2 | 0.73 | 0.23 |

  With the lens 0.9 m from a book and the old light 0.2 m ahead of the lens, the light-to-book distance was about 0.7 m, so the old light delivered about 4.0 x albedo. With Neutral tonemapping (see doc 06) 3.2 maps to about 0.94 linear, which is near white: matches the observed bleaching. The new light gives about 1.2 x albedo at the same spot.
- The window means changing Range barely changes brightness at short distance (W is about 1 for d much less than R). Range mainly controls where the light stops and how many lights overlap.

## 4. Confidence and unknowns

- **Unit-less intensity in URP 6000.6: high** (6000.6 manual page).
- **Attenuation formula: medium-high.** Source files were read from GitHub `master`, not the exact 6000.6 package. The Unity 6.4 custom-falloff tutorial text is consistent with a quadratic-with-smoothstep replacement, and nothing suggests the stock formula changed, but I did not diff against the shipped package. To verify: open `Library/PackageCache/com.unity.render-pipelines.universal@*/ShaderLibrary/RealtimeLights.hlsl` (read only).
- The Karis "+1" falloff is second-hand (forum quote); the PDF text could not be extracted.
- Unknown: whether any Unity 6.6 Light settings page for "Light Appearance" adds more than colour/temperature (the page I read says only that).
- Unknown: the albedo values actually used by the book and ceiling materials (I assumed 0.3-0.8).

## 5. Implication for BookBoy

- **Current HandLight (0.6, range 2.8): right direction, keep.** The old 2.0 value was provably too hot at 0.7 m (about 4x pre-albedo). 0.6 gives about 1.2 at 0.7 m. If books still look pale at arm length, lower to 0.4-0.5 before touching anything else, and keep it at least 0.2 m in front of nothing: place it on or slightly behind the camera (a point light 0.2 m ahead of the lens makes the near field worse). UNSOURCED GUESS: moving it back about 0.2-0.3 m is a standard trick; only an Unreal forum post mentions it.
- **Magelights at 4.4-5.0 are only wrong because of where they hang.** At 2.55 m under a 3.0 m ceiling the ceiling gets about 32x the floor. Smallest changes, in order:
  1. Lower the orbs (for example to about 2.0 m) or raise the ceiling: at 2.0 m the ceiling distance is 1.0 m and the ratio becomes (2.0/1.0)^2 = 4 to 1 instead of 32 to 1. This is a transform change only.
  2. Make the ceiling material darker/lower albedo (or the ceiling excluded from magelights via Rendering Layers/Culling Mask, accepting that the ceiling then relies on ambient). That needs project-settings approval (layers).
  3. Lower orb light intensity only if the floor/wall result at 2-3 m is still too hot. At 2.55 m the floor gets only 0.67 x albedo, so reducing intensity will make the room dim before it removes the ceiling spot.
- **Range 10 m is fine but oversized.** Energy at 8 m is 0.024, so range could drop to 8 m with no visible change in the room and fewer overlapping lights through doors. Not urgent.
- Do not enable physical light units; there is nothing to enable in URP 6000.6.
- A custom attenuation with a +1 or epsilon (Unity's own tutorial route) means copying the URP package into `Packages/` and editing `RealtimeLights.hlsl`. That conflicts with the project guardrail (Packages/manifest.json) and with future upgrades; **not recommended** for the smallest-change goal.
