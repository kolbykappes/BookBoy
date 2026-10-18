# 17 - UI scaling, world-space signs, and text legibility

Researched 2026-10-18 by web research only (no Unity Editor touched). Project: Unity 6000.6, URP, Windows PC.
Unmarked bullets carry a URL. Anything else is marked **UNSOURCED GUESS**. "Derived" means my own arithmetic or reasoning from sourced facts.

## 1. Question

How should BookBoy set up UI for a PC game?

- Canvas Scaler (Scale With Screen Size, reference resolution, match)
- legacy `UnityEngine.UI.Text` vs TextMeshPro (SDF crispness)
- world-space canvases for signs and labels (sizing, pixels per unit, cost of many)
- minimum readable text sizes in pixels at 1080p and 1440p
- legibility of tiny 3D spine labels (texture vs mesh text vs TMP 3D)
- whether the number of Canvas objects is a performance problem

## 2. Findings

### 2.1 What the project does now (read-only inspection)

- HUD: one overlay canvas (`HUD_Canvas`) with a `CanvasScaler`: Scale With Screen Size, reference 1920 x 1080, Screen Match Mode 0 (Match Width Or Height), Match = 0 (width), Reference PPU 100, Dynamic PPU 1 (Lamplit.unity, `m_UiScaleMode: 1`). Only one scaler in each scene.
- HUD text is legacy `UnityEngine.UI.Text` with the built-in `LegacyRuntime.ttf`, built at runtime in `GameUI.cs`: stats 34, carry 28, legend 20 (canvas units; the Text font size is in points).
- Signs: `LamplitBuilder.Sign` makes a world-space `Canvas` (no CanvasScaler, no GraphicRaycaster) at 1 px = 1 mm (`localScale = 1/1000`), frame and face `Image`s, and a legacy `Text` label with Best Fit (min 12, max 80), Bold, wrap.
- Lamplit.unity currently has 23 world-space canvases and 15 other canvases (render mode 0; includes the HUD and nested ones). Only 2 `GraphicRaycaster`s exist in the scene.
- Book numbers/letters on spines and shelf slots are `PixelDigit`: a mesh of quads from a 5 x 7 bitmap per glyph (one shared mesh per glyph), colour via `MaterialPropertyBlock`.
- Camera: vertical FOV 60. Anti-aliasing: camera `m_Antialiasing: 0`, URP MSAA off, quality `anisotropicTextures: 2` (Force Enabled) on the PC level (read from `QualitySettings.asset` and the scene; I did not confirm which quality level is active).

### 2.2 Canvas Scaler

- Constant Pixel Size keeps UI at the same pixel size; Scale With Screen Size designs against a Reference Resolution and scales; Constant Physical Size needs DPI. https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html
- Screen Match Mode: Match Width Or Height ("scale the canvas area with the width as reference, the height as reference, or something in between"), Expand ("never smaller than the reference"), Shrink ("never larger than the reference"). The page gives **no recommended value**. Same URL.
- Match value: 0 = scale by width, 1 = by height; in-between is a blend computed in logarithmic space (0.5 averages the two). https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.CanvasScaler.html (and the search-result text of older scripting references)
- `Canvas.scaleFactor` "only applies when Canvas.renderMode is set to Screen Space". https://docs.unity3d.com/ScriptReference/Canvas-scaleFactor.html
- World Space canvases: the Screen Space scale modes do not apply; only Dynamic Pixels Per Unit and Reference Pixels Per Unit do. Dynamic PPU is "the amount of pixels per unit to use for dynamically created bitmaps in the UI, such as Text". https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html , https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.CanvasScaler.html
- For dynamic fonts, `Text.pixelsPerUnit` "is equivalent to the scale factor of the canvas". https://docs.unity3d.com/Packages/com.unity.ugui@2.0/api/UnityEngine.UI.Text.html . So a world canvas with no CanvasScaler rasterises dynamic font text at scale factor 1 (derived), i.e. at its stated point size in pixels regardless of how small the canvas is scaled in the world.
- Forum reports: raising Dynamic PPU is the usual fix for blurry legacy Text on world canvases; one user needed values over 2000, another source suggests 2 to 4; scaling the canvas down by transform scale is the approach people complain about. https://discussions.unity.com/t/fix-blurry-ui-text/141667 and https://discussions.unity.com/t/ui-text-still-blurry/615698 (seen in search results; low authority; the numbers conflict)
- Derived effect of match = width on a landscape-only game. Reference 1920 x 1080:
  - 2560 x 1440 (16:9): scale 1.33, same layout as 1080p, no difference between width and height matching.
  - 3440 x 1440 (21:9): match width scale 1.79, canvas height only 804 units, UI occupies about 34% more of the screen height than the 1080p design; match height gives scale 1.33 and canvas width 2580 units (UI the same fraction of screen height, extra width is empty).
  - 1280 x 1024 (5:4): match width scale 0.667, canvas height 1536; match height scale 0.948, canvas width 1350, still wide enough for the corner panels (440 + 470 units at 1080p design).
  - Matching height keeps HUD proportions tied to vertical space, which is what a first-person game's crosshair-centred layout depends on. This is my reasoning; **no source recommends a value**.

### 2.3 Legacy Text vs TextMeshPro

- Legacy Text is on the "UI (Canvas)/Legacy/Text" menu path; Unity staff (Nov 2022): "the legacy Text components to now to be properly deprecated", "Most likely the TMP text object will replace the legacy one", TMP integration is "hosted in the UGUI Package", and the TMP package "will no longer be added to new projects"; TMP itself: "Not for the moment [being deprecated], we're just simplifying the ecosystem". https://discussions.unity.com/t/future-of-text-at-unity/898971 (2022; plans may have changed)
- TMP is now part of the built-in uGUI package (version 2.0.0, follows the Editor); essential resources may need importing from Window > TextMeshPro > Import TMP Essential Resources. https://discussions.unity.com/t/textmesh-pro-and-ugui-merged-clarification-on-where-to-get-the-latest-tmp-for-urp/1603892 , https://discussions.unity.com/t/textmesh-pro-in-unity-6/1580163
- Unity 6.5: the Advanced Text Generator became the default for **UI Toolkit**; a Unity staff post says "uGUI is here to stay", ATG may arrive as an opt-in for uGUI in 6.8, and the rendering stack did not change. A staff benchmark claims about 30x speed-up vs TextMesh Pro on 100 large auto-size text objects (UI Toolkit, single test). https://discussions.unity.com/t/advanced-text-generator-atg-now-default-in-6-5/1715871 . Unity 6000.6 static font assets must be migrated for ATG: https://docs.unity3d.com/6000.6/Documentation/Manual/ui-systems/migrate-static-font-assets.html
- Unity's UI-system comparison: uGUI is the recommended default for runtime UI with easy MonoBehaviour referencing and animation; UI Toolkit favoured for "screen overlay UI that runs on a wide variety of screen resolutions", large numbers of interfaces, and "World space UI and VR". https://docs.unity3d.com/Manual/UI-system-compare.html (fetch summary; I could not re-read the exact wording)
- SDF/crispness: TMP font assets use SDF render modes; the Learn tutorial says to "use SDF modes unless you're rendering a small font on a 1:1 ratio", sampling size 50 to 70, padding about one tenth of sampling size, atlas at or under 2048 x 2048 for mobile; the 3.2 manual says 512 x 512 is fine for ASCII-only and "a higher resolution produces finer gradients". https://learn.unity.com/tutorial/textmesh-pro-font-asset-creation-1 , https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/FontAssetsCreator.html . Neither page states the mechanism "SDF stays sharp at any size" in so many words; that mechanism is general knowledge (**UNSOURCED** in these pages).
- Legacy Text note: Best Fit "ignore[s] the size properties"; the manual says nothing about its cost. Dynamic fonts "incur a higher performance overhead than static font assets" in the new text system (that sentence is about the TextCore dynamic font assets, not necessarily legacy Text). https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Text.html , https://docs.unity3d.com/6000.6/Documentation/Manual/ui-systems/migrate-static-font-assets.html . The claim "Best Fit is expensive" is **unsourced**.

### 2.4 Minimum readable text size

- Xbox Accessibility Guideline 101 (body height from lowest descender to highest ascender, **not** point size): console at least 26 px at 1080p and 52 px at 4K; **PC/VR at least 18 px at 1080p and 36 px at 4K**; players should be able to scale up to 200%. https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101
- Game Accessibility Guidelines: "28px as a minimum" at 1080p, from Amazon TV 10-foot guidelines, "a minimum rather than a target". https://gameaccessibilityguidelines.com/?p=3285
- IGDA GA SIG: "ideally adhering to a minimum of 32px @ 1080p (46px for text that appears for a limited time, such as subtitles/captions)". https://igda-gasig.org/?p=1387
- These disagree (18 vs 28 vs 32 px at 1080p); the difference is mainly viewing distance assumed (desk PC vs sofa TV) and measurement (body height vs pixel size). For a desk PC game the Xbox PC row is the closest match (**derived** judgement).
- 1440p: no source gives a 1440p number. Scaling by resolution (derived): 18 px at 1080p corresponds to 24 px at 1440p and 36 px at 2160p, which matches the XAG 4K figure. Fixed pixel sizes shrink in relative terms as resolution rises; an example seen in a search-result summary of a UX article (not opened, source URL not captured) says a 16 px label at 1080p is the same physical size as an 8 px label at 4K; that is plain arithmetic (4K has twice the pixels per axis), so I trust it, but the article itself is **UNSOURCED** here.
- Visual-angle sources (secondhand, low confidence): ISO 9241 ergonomics cite roughly 16 arcmin minimum, 20 to 22 arcmin preferred for character height at 600 mm viewing distance. https://lists.w3.org/Archives/Public/public-css-archive/2020Jun/0612.html (a mailing-list summary, not the standard). Another whitepaper says 16 arcmin minimum, 20 optimal. https://activu.com/uploads/Optimal-Font-Size.pdf (search result only)
- Derived check on the project: legacy Text fontSize 20 for the legend. Ascender-plus-descender height of a typical sans font is roughly 0.9 to 1.0 em (**UNSOURCED GUESS**, font metrics), so fontSize 20 gives about 18 to 20 px body height at 1080p, right at the XAG PC minimum. Stats (34) and carry (28) are comfortably above it. If the legend carries needed info, 20 is the weakest.

### 2.5 World-space canvases for signs and labels

- World Space canvas: "behaves like any other scene object", "size can be set manually using its Rect Transform", draw order follows hierarchy. https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UICanvas.html
- Canvas isolation: "Each Canvas is an island"; changing one element makes the whole canvas get re-analysed ("a CPU spike costing multiple milliseconds" at the scale of "thousands of elements"); split static from dynamic; "disabling the Canvas component stops its draw calls without discarding its vertex data". Graphic Raycaster: remove from non-interactive canvases. https://unity.com/how-to/unity-ui-optimization-tips
- No official number for hundreds of world canvases. A community summary (low authority): batching depends on shared materials and hierarchy order, not merely canvas count; world canvases with raycasters and no Event Camera cost repeated `Camera.main` lookups per raycaster. https://www.wayline.io/blog/unity-ui-best-practices-for-performance and https://discussions.unity.com/t/too-many-draw-calls-with-textmesh-pro/767796 (search results only)
- UI Toolkit also offers world-space panels: Panel Settings render mode World Space, Pixels Per Unit default 100, intended for "health bars or labels" and wall UI. Known issues in the tracker; sorting of transparent elements as the camera moves. https://docs.unity3d.com/Manual/ui-systems/create-world-space-ui.html (6.4 manual page; I did not find a 6.6-specific change)
- Derived for the project: 1 px = 1 mm gives 1000 canvas px per metre. On screen at 1080p, a 60 degree vertical FOV gives about 935 / d screen px per metre at distance d metres (d in m), so the sign texture density matches the screen at about 1 m. Legacy Text rasterised at the stated point size is therefore adequate farther than about 1 m and soft closer than about 1 m unless Dynamic PPU is raised through a CanvasScaler. At 1440p the match point moves to about 1.33 m.

### 2.6 Legibility of tiny 3D spine labels

- Derived pixel budget (FOV 60, 1080p; multiply by 1.33 for 1440p): screen px per metre = 935 / d. Examples: d = 1 m: 935 px/m; d = 1.5 m: 623; d = 2.8 m (reach limit): 334. A glyph 0.03 m tall is 28 px at 1 m, 19 px at 1.5 m, and 10 px at 2.8 m. A 5 x 7 pixel digit of 0.03 m has cells of 0.0043 m which are 4 px at 1 m, 2.7 px at 1.5 m, 1.4 px at 2.8 m.
- Unity: mipmaps give distant objects lower-resolution levels (softer) and cost 33% more memory; skip them when a texture is only ever seen at full resolution. https://docs.unity3d.com/Manual/texture-mipmaps-introduction.html . Anisotropic filtering keeps textures sharper at shallow angles; Aniso Level 0 disables it; Force Enabled clamps levels to 9 to 16 except textures set to 0; can be slower on the GPU. https://support.unity.com/hc/en-us/articles/210606003-How-does-the-Anisotropic-Textures-Quality-Setting-affect-each-texture-s-Aniso-Level (search result summary). Spines are seen at oblique angles from the side, so anisotropic filtering matters for any texture-based label (derived).
- Texture label (baked atlas): sharp at the baked resolution, mip-blurs with distance, benefits from anisotropy; no per-label geometry; one material can serve all. Source for the first points: the two Unity pages above. Whether a baked texture beats mesh digits for this game is **unknown** (no benchmark).
- Mesh text (PixelDigit style): vector-like geometry, never blurs, but is a few quads each so thin features alias when the cell is about 1 px (the project has MSAA off and no camera AA: derived); using the `MaterialPropertyBlock` per renderer means no SRP Batcher (Unity docs, see file 16).
- TMP 3D text (`TextMeshPro` component, MeshRenderer, world space): SDF stays smooth under oblique viewing; batching requires one shared font material, and per-object property changes break batching; static batching is blocked by TMP's tangent/UV2 attributes; reports that TMP under URP did not batch in one user's setup. https://discussions.unity.com/t/batch-textmeshpro-text-components-how/888150 , https://discussions.unity.com/t/textmeshpro-cannot-be-batched-in-urp/899601 (search results only). TMP 3D is "world space with a MeshRenderer, not a canvas". Cost for 100+ spine labels: **unknown, unmeasured**.
- Rule from usability guidance above: 18 px is the PC comfortable minimum for HUD text; for in-world spine text the target is "readable at the usual aiming distance of 1 to 1.5 m", where a 0.03 m glyph hits about 19 to 28 px. At the 2.8 m reach limit they are only recognisable shapes. That is a design choice, not a sourced rule.

### 2.7 Number of Canvas objects

- Each Canvas is an independent batching/rebuild unit; cost comes from rebuilds, which only happen when something on the canvas changes; static signs do not rebuild after the first build. Source: Unity UI optimization tips above.
- 23 static world canvases with two batches each (frame/face images share the default UI material, text uses the font material) is a small number (derived estimate: about 46 draws). No Unity page sets a threshold for "too many canvases".
- Unity's profiler has a UI/Canvas module for this. https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/ProfilerUI.html (listed in the uGUI manual TOC; page not fetched).

## 3. Numbers and rules of thumb

| Item | Value | Source |
|---|---|---|
| PC minimum default text | 18 px body height at 1080p; 36 px at 4K | Xbox XAG 101 |
| Console / TV | 26 px at 1080p (Xbox); 28 px (GAG, Amazon); 32 px (IGDA) | the three sources above |
| Allow user text scale | up to 200% | Xbox XAG 101 |
| TMP SDF font asset start | sampling 50 to 70, padding about 1/10 of sampling, 512 x 512 atlas for ASCII | Unity Learn / TMP manual |
| World canvas count cost | no official figure | none |
| Canvas Scaler recommended Match | none stated by Unity | CanvasScaler manual |
| Screen px per metre at 1080p, FOV 60 | 935 / distance in metres | derived |
| Visual angle for characters | 16 arcmin minimum, 20 to 22 preferred | secondhand ISO summary |

## 4. Confidence and unknowns

- **High**: Canvas Scaler semantics; world-space Canvas scaler options; TMP is bundled in uGUI; XAG 101 PC numbers.
- **Medium**: legacy Text is on the way out but not removed (staff statements are from 2022 and 2026 forum posts, not a formal notice); disagreement among minimum-size recommendations.
- **Low / unknown**:
  - Whether TMP 3D labels batch in the project's URP setup (conflicting reports).
  - Cost of hundreds of world canvases or hundreds of TMP 3D objects.
  - Any 1440p-specific text-size figure.
  - Whether Best Fit on legacy Text is costly.
  - Which Quality level the player build uses.
  - Exact font metrics for `LegacyRuntime.ttf`.
  - What Unity 6000.6 changed in uGUI text specifically.

## 5. Implication for BookBoy

**Looks right**
- Scale With Screen Size at 1920 x 1080 is the standard choice.
- Signs as world-space canvases with 1 px = 1 mm and no GraphicRaycaster: cheap and static (23 of them, no rebuild after creation).
- `PixelDigit` for spine/slot numbers: geometry cannot blur, and a tiny 5 x 7 bitmap does not need a font asset.

**Looks questionable (smallest fixes first)**
1. **Match = 0 (width)** will make the HUD larger relative to screen height on 21:9 and smaller on 4:3 and 5:4 displays. For a landscape first-person game, the sourced facts (0 = width, 1 = height, in-between = log blend; Unity gives no recommendation) plus the derived table in 2.2 favour **Match = 1 (height)** or about 0.5, and **Expand** mode if you must guarantee nothing designed falls outside the screen. It is a one-field change on one CanvasScaler. It is not in ProjectSettings but it is in the scene, so confirm before editing.
2. **Legend text at font size 20** sits at the XAG PC minimum (18 px body at 1080p; my 0.9 to 1.0 em estimate is unsourced). If the legend is meant to be read, raise it to 24 (derived, equal to the 1440p-equivalent) or provide a text-size option up to 200%.
3. **Sign labels (legacy Text, Best Fit, dynamic font)**: the font is rasterised at its point size (scale factor 1, derived), so signs are soft within about 1 m and the Best Fit sizes cause multiple raster sizes. Smallest fix: add a `CanvasScaler` set to World Space on each sign with Dynamic Pixels Per Unit about 2 to 4 (sources disagree: 2 to 4 vs "over 2000"; **test visually**). Better fix: switch sign labels to `TextMeshProUGUI` (SDF, stays crisp at any distance; TMP lives in the uGUI package already); this needs the TMP Essential Resources imported (they add assets under `Assets/TextMesh Pro`, outside `_Project`; check the guardrail "Leave Assets/Starter Assets untouched" is not violated, since TMP is not Starter Assets but it adds files, so ask first).
4. **HUD in legacy Text** is fine for a prototype. A migration to TMP is optional; there is no performance reason in the sources, only crispness and future-proofing (legacy is expected to be deprecated; no removal date found).
5. **Spine labels**: keep `PixelDigit` (mesh glyphs) for numbers. If real titles/letters must appear on spines, a TMP 3D label would stay smooth at oblique angles but is unmeasured for 100+ objects and may not batch under URP; a baked texture atlas with anisotropic filtering is the other candidate. Decide with a Frame Debugger test of 100 labels, not from sources.
   Design rule (derived): make spine glyphs at least 0.025 to 0.03 m tall so they reach about 19 to 28 px at 1 to 1.5 m, and accept that the 2.8 m reach range shows shape only.
6. **No evidence the number of canvases is a problem at 23**; do not merge them. If it ever shows up in the Profiler, disable `Canvas.enabled` for far signs instead (Unity's documented trick keeps vertex data).

**Do not do**: add a canvas per book. If per-book labels are ever needed, use 3D text/mesh or a shared texture, not 100+ canvases.
