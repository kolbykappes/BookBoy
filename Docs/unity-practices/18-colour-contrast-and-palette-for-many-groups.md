# 18. Colour, contrast and palette design for 20-24 item groups on a dark wood shelf

Research date 2026-10-18. Project: Unity 6000.6, URP, Windows. Sources are web pages read in this session; computed numbers come from a throw-away Python script (Oklab/WCAG formulas below, Machado CVD matrices) run outside the project - they are not Unity measurements. Anything without a URL is **UNSOURCED GUESS**.

## 1. Question

How should ~20-24 box groups be coloured so each is separable from the dark brown shelf (target: every group >= 2.0 contrast ratio vs wood, average >= 2.5) and from each other, for normal and colour-blind players? Covers: WCAG luminance/contrast definitions and why they are only a proxy for graphics; perceptual colour spaces; how many hues are distinguishable at a glance; colour-blind safety and tools; Game Accessibility Guidelines; redundancy (letter/shape); value vs hue; how Unity's Linear/Gamma colour space and tonemapping change what is seen; and a concrete generation algorithm with a guaranteed lightness floor.

## 2. Findings

### 2.1 Definitions: WCAG relative luminance and contrast ratio

- Relative luminance Y = 0.2126 R + 0.7152 G + 0.0722 B on linearised sRGB channels (c/12.92 if c <= 0.04045 else ((c+0.055)/1.055)^2.4). The W3C notes the printed WCAG threshold 0.03928 is an erratum; 0.04045 is correct and the difference is not significant for 8-bit. https://www.w3.org/WAI/GL/wiki/Relative_luminance
- Contrast ratio = (L1 + 0.05) / (L2 + 0.05), L1 the lighter. Quoted in WCAG 2.2: https://www.w3.org/TR/WCAG22/ (1.4.3: 4.5:1 text, 3:1 large text; 1.4.6 AAA 7:1 / 4.5:1; 1.4.11 3:1).
- WCAG 1.4.11 Non-text Contrast: "graphical objects" needed to understand content and UI-component states need **3:1 against adjacent colour(s)**; threshold is not rounded (2.999 fails); text inside graphics falls under 1.4.3; hue-only change fails 1.4.1 (Use of Color) rather than 1.4.11. The page does not mention games. https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast.html
- **Why it is only a proxy:** WCAG 2 was designed for web text/UI on a bright screen; its `+0.05` flare term dominates when both colours are dark. Critics (APCA authors, not independent studies) say WCAG 2 over-states contrast for dark pairs, e.g. 8:1 reported for #a0a0a0 on #000. https://github.com/Myndex/apca-introduction , https://www.w3.org/WAI/GL/task-forces/silver/wiki/Visual_Contrast_of_Text_Subgroup/APCA_model . APCA is a WCAG 3 candidate, not adopted. Evidence quality: advocacy/blog, no independent psychophysics found.
  - Consequence for BookBoy: the project's measured ratios (2.0 avg, 0.7-4.8 range) use the WCAG formula on a *dark scene*, so the absolute numbers are a heuristic. Using the same formula consistently for relative comparison is fine; the 3:1 (non-text) bar is the official reference, and the owner's own target (2.0 / 2.5) is below it. That target is a **project choice**, not a standard.

### 2.2 Perceptual colour spaces for generating palettes

- **Oklab/OKLCH** (Bjorn Ottosson): designed to predict lightness, chroma and hue well, simple to compute, D65. Measured against CIEDE2000 test data: lightness RMS error Oklab 0.20 vs CIELAB 1.70, CIELUV 1.72, HSV 11.59; chroma 0.81 vs 1.84/2.32/3.38; hue: JzAzBz slightly better (0.43 vs 0.49), CIELAB 0.69, HSV 1.10. CIELAB/CIELUV/HSV shift blue blends toward purple. Full linear-sRGB <-> Oklab matrices on the page. https://bottosson.github.io/posts/oklab/
- **HSLuv**: human-friendly HSL built on CIELUV where saturation is "all available chroma as a percentage"; the page states no lightness guarantee or limitations. https://www.hsluv.org/
- **HSV (Unity `Color.HSVToRGB`)** is not perceptual: "Value" says nothing about brightness across hues. Unity's page does not say which colour space the output is in. https://docs.unity3d.com/ScriptReference/Color.HSVToRGB.html . Evidence: Oklab page's HSV lightness error of 11.59 vs 0.20 for Oklab; and our computed example below (2.5 ratio needs HSV V from 0.60 to 0.98-1.00 for blue/purple but only 0.61 for green).
- **Glasbey method** (categorical palettes): greedily add the colour most different (CIELAB distance) from those already chosen; hand-picked sets are usually < 10, nearly always < 25 colours; `glasbey_light` removes colours with lightness below 30 (CIECAM02-JCh) for **dark backgrounds**; "display candidate palettes on the actual background". https://colorcet.holoviz.org/user_guide/Categorical.html , paper https://strathprints.strath.ac.uk/30312/
- **Polychrome** (J Stat Softw 2019): builds palettes of "at least 24 colors that can be distinguished by most people with normal color vision"; RColorBrewer qualitative palettes are limited to 12. (Abstract only was readable.) https://www.jstatsoft.org/article/view/v090c01
- **Kelly's 22 colours of maximum contrast** (1965, Color Engineering 3(6)): first 9 (white, black, yellow, purple, orange, light blue, red, buff, grey) are stated to stay discriminable for colour-deficient viewers; later ones are not; ordered so the top n are best for n < 22; hex values vary by source. https://rdocumentation.org/packages/BacArena/versions/1.8.2/topics/colpal3 , https://gist.github.com/ollieglass/f6ddd781eeae1d24e391265432297538 (secondary).

### 2.3 How many hues at a glance

- Healey's data-visualisation study cites a military rule of thumb of "no more than five to seven colours" at a time (unexplained), and models colour distance, linear separation and colour category as the factors for rapid search. https://www.csc2.ncsu.edu/faculty/healey/download/viz.96.pdf (snippet; PDF not parsed). Design-system guidance: beyond ~6 hard to tell apart (low authority). 
- 12 is a *convention* (ColorBrewer), not a perceptual ceiling (Polychrome abstract). 24 are separable *side by side, normal vision*; **at-a-glance identification across a room of 24 is beyond what colour alone supports** - this is why a second channel (the spine letter) matters. The Game Accessibility Guidelines reach the same conclusion for any colour-coded information (2.5).
- Computed here (Oklab, wood #6B401F, ratio >= 2.5, lightness 0.62-0.90): the best 24-colour set found by greedy farthest-point has minimum pairwise Oklab distance ~0.05 (normal) and ~0.045 under dichromat simulation. For reference, the current 20-colour palette has min pair 0.048 (normal) but **0.014 / 0.016 / 0.025 under deuteranopia** (#7F1F99 vs #325099, #613299 vs #325099, #457A99 vs #99458F). Oklab JND is roughly 0.02 (**UNSOURCED GUESS**), so ~0.05 means "distinct when adjacent", not "identifiable at a glance".

### 2.4 Colour-blind safety

- Prevalence: roughly 5-8% of men and 0.5% of women; protan + deutan are 99% of that (exhibit page; the NEI page itself returned 404 so the NEI attribution is unverified). https://webexhibits.org/causesofcolor/2C.html
- Game Accessibility Guidelines, Basic tier: "Ensure no essential information is conveyed by a fixed colour alone": use colour as a back-up for text, symbol, pattern or shape; red can look darker to colour-deficient players so check contrast with a simulator; design for 100% severity; test with simulators such as Colour Oracle; whole-palette filters are blunt, explore other solutions first; offer presets (deuteranopia, protanopia, tritanopia) if colour is essential. Also Basic: "Provide high contrast between text/UI and background". https://gameaccessibilityguidelines.com/ensure-no-essential-information-is-conveyed-by-a-fixed-colour-alone/ , https://gameaccessibilityguidelines.com/full-list/
- Colour Universal Design (Okabe & Ito): choose colours differing in **brightness or saturation, not just hue**; combine colour with shape/line type/pattern; label directly instead of via a key; avoid colours of equal brightness differing only in hue; yellow-green region looks similar to yellow/orange; simulations are approximations, have colour-blind people review. http://jfly.uni-koeln.de/color/ (Okabe-Ito 8-colour palette: #E69F00 #56B4E9 #009E73 #F0E442 #0072B2 #D55E00 #CC79A7 #000000, from secondary pages: https://www.audioeye.com/post/colorblind-friendly-palettes/ )
- Simulators: Colour Oracle (cited by GAG), Vischeck, Photoshop CUD proof (Jfly page). Matrix simulation: Machado et al. 2009, severity-1.0 matrices (3x3 on RGB; page does not state linear vs gamma, the usual practice is linear RGB - **UNSOURCED GUESS**): protan [0.152286 1.052583 -0.204868; 0.114503 0.786281 0.099216; -0.003882 -0.048116 1.051998], deutan [0.367322 0.860646 -0.227968; 0.280085 0.672501 0.047413; -0.011820 0.042940 0.968881], tritan [1.255528 -0.076749 -0.178779; -0.078411 0.930809 0.147602; 0.004733 0.691367 0.303900]. https://www.inf.ufrgs.br/~oliveira/pubs_files/CVD_Simulation/CVD_Simulation.html

### 2.5 Redundant channels

- GAG and Jfly agree: pair colour with text/symbol/pattern/shape; label objects directly. BookBoy already puts a big letter on each spine - that is the guideline-compliant channel. Letter legibility falls under WCAG 1.4.3 (>= 4.5:1, or 3:1 if "large"). Computed: choosing black or white per spine, the 24-colour set below gives >= 5.9:1; the current palette >= 4.6:1.
- Further redundancy options (shape/height/edge-band/pattern per group) are design choices; nothing in the sources quantifies how many are needed. **UNSOURCED GUESS:** a second channel only on the confusable pairs is enough.

### 2.6 Value separation vs hue

- Jfly: "pick colours that differ in brightness or saturation, not just hue"; avoid "colours that share brightness and differ only in hue". GAG: adding a contrast difference (light blue vs dark orange) helps colour-blind edge cases. Both support using lightness as a *guaranteed* channel versus the wood (and, in tiers, between neighbours).
- Why HSV V alternation (0.85/0.60) fails: it alternates a *nominal* brightness, but luminance depends strongly on hue (blue/purple/red low). Computed below.

### 2.7 Unity: Linear vs Gamma, tonemapping

- Unity supports Gamma and Linear; "Textures tend to be saved in gamma color space, while Shaders expect linear color space"; linear uses sRGB sampling; set under Player > Other Settings > Rendering > Color Space. https://docs.unity3d.com/6000.0/Documentation/Manual/set-project-color-space.html
- Gamma-mode lighting (5.5 manual): shader maths treats gamma values as linear, "brightness rises nonlinearly as light intensity increases", scenes can look washed out, falloff less accurate, blending over-saturated. https://docs.unity3d.com/550/Documentation/Manual/LinearRendering-LinearOrGammaWorkflow.html
- `Color.linear` applies "an inverse sRGB gamma curve" to convert; colours "are often written in sRGB". https://docs.unity3d.com/ScriptReference/Color-linear.html . So a material `_BaseColor` of (0.42, 0.25, 0.12) is in sRGB-encoded terms and the WCAG sRGB formula applies directly in Linear mode (our calc assumes this; the project's colour-space setting was not read).
- URP tonemapping: None / Neutral ("minimal impact on hue and saturation", good start for grading) / ACES ("more contrasted than Neutral and does affect hue and saturation"). https://docs.unity3d.com/6000.0/Documentation/Manual/urp/post-processing-tonemapping.html . Khronos says filmic curves like ACES can make base colours look washed out; its Neutral tone mapper aims for a 1:1 base-colour match up to a maximum value (vendor claim). https://khronos.org/news/press/khronos-pbr-neutral-tone-mapper-released-for-true-to-life-color-rendering-of-3d-products . Unity's Neutral and Khronos PBR Neutral are different implementations; **whether Unity's Neutral preserves albedo luminance ratios in dim light is UNSOURCED**. A tonemapper is nonlinear, so albedo ratios (flat) and on-screen ratios (lit, tonemapped, vignetted) differ - measure on a screenshot like the project already does.
- The measured ratios (avg 2.0) came from lit renders; my flat-albedo calc of the current palette gives avg 2.63, min 1.02, 10/20 below 2.0 - same shape (purple/navy ~1.0, yellow/cyan/lime ~5), lower on screen because lit.

## 3. Numbers and rules of thumb (computed unless a source is given)

All with wood = `Mat_Wood` _BaseColor (0.42, 0.25, 0.12) = #6B401F: Y = 0.0687, Oklab L = 0.415.

| Quantity | Value |
|---|---|
| Minimum Y for ratio R vs wood | Y >= R(0.0687+0.05) - 0.05: R=2.0 -> 0.187; 2.5 -> 0.247; 3.0 -> 0.306 |
| Smallest Oklab L such that *every* in-gamut colour with C <= 0.30 passes (lighter than wood) | R=2.0: L 0.62; R=2.5: L 0.67; R=3.0: L 0.72 (blue is the binding hue) |
| Same with a darker wood #4C2B14 (Y=0.034) | R=2.0: 0.53; R=2.5: 0.59; R=3.0: 0.63 |
| Current palette vs wood #6B401F | avg 2.63, min 1.02, 10/20 < 2.0, 14/20 < 2.5 |
| Current palette vs #4C2B14 / #402412 | n<2.0: 5 / 4; n<2.5: 7 / 6 (darkening wood alone cannot fix it) |
| HSV V needed for 2.5 (current hue/sat) | #325099 (g1) 0.60 -> 0.98; #7F1F99 (g11) -> 1.00; #613299 (g19) -> 1.00; green/teal groups only +0.01 |
| Best 24-colour set (Oklab L 0.62-0.90, C 0.07-0.22, ratio >= 2.5) | min Oklab distance 0.052 normal, 0.045-0.047 deut/prot/trit; ratio min 2.54, avg 4.24; letter contrast >= 6.0 |
| Simple golden-angle hue + 2 lightness tiers (24 colours, L 0.66/0.80) | ratio floor holds (min 2.57-2.68) but min dichromat distance 0.002 -> collapses; hue spacing alone is not enough |
| WCAG non-text bar | 3:1 (1.4.11); text 4.5:1 (1.4.3) |

## 4. Concrete algorithm (24 colours, guaranteed lightness floor over the wood)

Inputs: wood sRGB `W`, target ratio `R` (use 2.5 so the average target of 2.5 holds with margin), `n` = 24. Design in **Oklab/OKLCH**, validate with the **WCAG formula**.

1. Compute `Yw = Y(W)`. Required `Ymin = R*(Yw+0.05) - 0.05`.
2. Candidate set: sample OKLCH on a grid (L from 0.60 to 0.90 step 0.02; hue every 3 degrees; C from 0.07 to 0.22 step 0.01 - the lower C bound avoids beige/brown that merges with wood, the upper bound avoids neon). Convert to linear sRGB with the Oklab inverse matrices (Bottosson page) and **discard out-of-gamut** points (no silent clipping). Keep only points with `Y >= Ymin` (exact, not an L estimate) and Oklab distance from wood >= 0.12.
   - The L floor is therefore a *derived* guarantee, not a guess: for the current wood and R = 2.5 every candidate has ratio >= 2.5 by construction (blue is the binding hue at L ~0.67; a per-hue floor would be lower for yellow/green, which I did not compute, so one common floor is enforced).
3. Distance for selection: for each candidate precompute Oklab of itself and of its deutan, protan and tritan simulations (Machado matrices above, applied in linear RGB). Pair distance = `min(d_normal, d_deut, d_prot, d_trit)`.
4. Greedy farthest-point (Glasbey style): start from the candidate farthest from wood; repeatedly add the candidate maximising the minimum pair distance to the set; stop at 24. Greedy order is useful: the first k colours are the best k-subset, so use early entries for groups that sit on the same shelf bay.
5. Assign letters/colours so groups that remain nearest in colour space (the few pairs at ~0.045) are **not adjacent** and carry clearly different letters.
6. Bake the result as a fixed table (24 hex values) rather than generating at build time; re-run only if the wood colour changes. Re-validate: compute `ratio >= R` for every entry, report min/avg, report min pair distance per vision type.
7. Validate in the real scene: screenshot, sample the lit spine and wood luminance, compare with the flat ratio. This accounts for lighting + tonemapping (2.7).

Example output of that script for wood #6B401F, R=2.5 (illustrative, computed outside Unity; neon entries like #8EFF61 should be reviewed by eye and the chroma cap lowered to ~0.16 if the look is too loud): `#8EFF61, #D056DC, #27A902, #FEB2FD, #A2C390, #F24394, #EB84EC, #C9E1B0, #CBBA2F, #E87A69, #73D1C9, #2DB1C1, #7783FE, #98E36B, #8E84B3, #CD9396, #3BFEE7, #FDA9B6, #6BA362, #E1CD20, #13DCF6, #B2B04F, #CE8633, #3EB7F7`. Closest pairs (distance ~0.045): #3EB7F7/#2DB1C1, #CE8633/#27A902 (under CVD), #3EB7F7/#73D1C9, #CE8633/#E87A69, #B2B04F/#CBBA2F.

A cheaper variant that keeps the existing generator: keep `GroupColor`'s golden-angle hue stepping, but replace "HSV value" with a per-hue lightness solve (binary-search the HSV V or Oklab L so that `ratio(color, wood) >= R`), then run step 4 only as a check. The table above shows hue stepping alone leaves CVD collisions, so a fixed table with the pair-assignment rule (step 5) is the smaller, safer change.

## 5. Confidence and unknowns

- High: formulas (WCAG, Oklab), 3:1 non-text bar, GAG "not colour alone" text, Oklab metrics, the computed flat-albedo numbers (formulas are deterministic).
- Medium: how many colours are separable (sources give 5-7 rapid, 12 convention, 24 side-by-side); Machado matrices (read from the author's page, linear/gamma not stated).
- Low: effect of Unity tonemapping/lighting on the book/wood ratio; WCAG-vs-perception for dark scenes (APCA claims only); Oklab JND ~0.02; thresholds for "at a glance".
- Unknown: the project's Colour Space setting and Volume tonemapping mode (not read); in-scene luminance of the lit shelf; whether spines are emissive or lit.

## 6. Implication for BookBoy

1. **Current settings look wrong:** `LamplitBuilder.GroupColor` uses HSV with V alternating 0.85/0.60 and S 0.55-0.80. HSV V is not luminance: of 20 groups, 10 are below ratio 2.0 against `Mat_Wood` even in flat albedo (#613299 = 1.02, #7F1F99 = 1.08, #2B3AD9 = 1.14, #325099 = 1.15), matching the 0.7 seen in-scene. The 0.60 tier is the problem; lifting V only helps green/teal/yellow but blue/purple would need V ~1.0, so the fix is per-hue lightness solved in Oklab/Y, not a uniform V bump.
2. **Smallest change that meets the target:** stop alternating V; generate (or hard-code) 24 colours with the algorithm in section 4 (Oklab L >= ~0.67 for all hues at R=2.5 for the current wood, or ~0.59 if the wood is darkened to ~#4C2B14). Result: min ratio 2.54, avg ~4.2 (flat), which leaves ~1.5x headroom for lighting/tonemapping loss, and letter contrast >= 6:1.
3. **Wood:** darkening the wood is a second lever but not sufficient alone (5 groups still < 2.0 even at #4C2B14). If the wood is darkened, the floor drops to Oklab L ~0.59; do it only if the room still reads warm.
4. **Keep the letters.** They are the guideline-conforming redundancy (GAG Basic, Jfly rule 2). With 24 colours even the best set has pairs ~0.045 apart, and the current palette has deutan pairs at 0.014; the letter, not hue, is the real identifier. Make letter contrast >= 4.5:1 (black or white per spine; trivially met).
5. **Do not rely on tonemapping staying neutral.** Validate ratios from a rendered screenshot with the same tonemapping mode; prefer Neutral over ACES if hue/saturation fidelity matters (Unity docs), and re-measure after any lighting change.
6. **Do not add a colour-blind filter**; GAG says whole-palette filters are blunt. No presets are needed if letters carry identity; optional later: a per-colour-vision palette swap (not in scope for the prototype).
7. Target wording: the project's 2.0/2.5 target is below WCAG's 3:1 non-text bar; note this as a deliberate choice, or raise the floor to R=3.0 (Oklab L ~0.72 with the current wood) if the book edge against the shelf still looks weak in the lit scene.
