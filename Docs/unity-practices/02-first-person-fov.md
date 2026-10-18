# 02 - First-person field of view (vertical vs horizontal, typical values, size and distortion)

Researched 2026-10-18. Web research only; Unity was not opened. Project: Unity 6000.6, URP, Cinemachine 3 (vertical FOV 75 in `PlayerFollowCamera`), Starter Assets FirstPersonController.

Tags: **[fetched]** = page body read; **[search-only]** = only a search-result summary seen; **[derived]** = my arithmetic (formulas given, reproducible); **UNSOURCED GUESS** = could not source.

## 1. Question

Unity's field of view is vertical. What is 75 degrees vertical in horizontal terms at 16:9 and 21:9? What FOV do shipped first-person games use, and why? How does FOV relate to perceived object size, closeness and distortion? How do cozy, object-focused games choose it, and what should an FOV slider look like?

## 2. Findings

### 2.1 Axis and conversion

- Unity: "This is the vertical field of view; horizontal field of view depends on the viewport's aspect ratio." [fetched] https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera-fieldOfView.html . The Camera component also has an FOV Axis drop-down (Horizontal or Vertical). [fetched] https://docs.unity3d.com/6000.0/Documentation/Manual/class-Camera.html
- Cinemachine 3: `LensSettings.FieldOfView` is "the camera vertical field of view in degrees"; internally always vertical, the inspector shows horizontal only if the camera's FOV axis is set so. [fetched] https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/api/Unity.Cinemachine.LensSettings.html
- Unity has a built-in converter: `Camera.VerticalToHorizontalFieldOfView(vFov, aspect)`. [fetched] https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.VerticalToHorizontalFieldOfView.html
- Formula: H = 2*atan(tan(V/2) * w/h). [fetched, Wikipedia] https://en.wikipedia.org/wiki/Field_of_view_in_video_games . The shortcut "H = V * aspect" is wrong for wide angles (it would give about 133 degrees for V=75 at 16:9 instead of 107.5). [search-only, gamedev.net threads] https://www.gamedev.net/topic/431111-perspective-math-calculating-horisontal-fov-from-vertical/
- Because Unity fixes the vertical angle, wider screens automatically show more horizontally ("Hor+"), which Wikipedia calls "the most common method today". Vert- (horizontal fixed, vertical shrinks) is the alternative. [fetched] Wikipedia link above.

### 2.2 Computed conversions [derived with the formula above, checked in node]

| Aspect | 75 deg vertical -> horizontal | 90 deg horizontal -> vertical | 100 deg h -> v | 70 deg h -> v |
|---|---:|---:|---:|---:|
| 4:3 | 91.3 | 73.7 | 83.6 | 55.4 |
| 16:10 | 101.7 | 64.0 | 73.4 | 47.3 |
| **16:9** | **107.5** | **58.7** | 67.7 | 43.0 |
| **21:9 (7/3)** | **121.6** | **46.4** | 54.1 | 33.4 |
| 64:27 (real 2560x1080) | 122.4 | 45.7 | 53.4 | 32.9 |
| 32:9 | 139.7 | 31.4 | 37.1 | 22.3 |

Other verticals: 40 v = 65.8 h at 16:9 (Starter Assets default, see 2.3); 50 v = 79.3 h; 55 v = 85.6 h; 60 v = 91.5 h.

### 2.3 What shipped games use (values and how they were chosen)

- Wikipedia: console games typically around 60 degrees, PC games usually 90 to 100 degrees, because monitors sit closer. FOV is often stated as one angle without saying which axis. Narrowing it can improve performance via culling. [fetched] https://en.wikipedia.org/wiki/Field_of_view_in_video_games
- Gearbox (Borderlands 2): per secondary coverage of Kyle Pittman's post, Borderlands 1 shipped at 70 degrees horizontal (about 43 vertical at 16:9); the PC sequel used a horizontal slider with a 90 default; a value that suits a TV at couch distance may cause tunnel vision or motion sickness at desk distance. [search-only; the PC Gamer and Gearbox pages themselves returned only boilerplate or 403] https://pcgamer.com/gearbox-blog-fov and https://www.gearboxsoftware.com/?p=10284
- Metro: Last Light kept FOV fixed because hand animations, 3D HUD elements and scene detail were built around one FOV. [search-only, same PC Gamer article]
- Firewatch (first-person, relaxed, object-focused): "Our default FOV is 55." [fetched] https://help.panic.com/firewatch/fov/ . PCGamingWiki lists it as vertical with no in-game slider [search-only] https://www.pcgamingwiki.com/wiki/Firewatch (page itself 403). If 55 is vertical, that is about 85.6 horizontal at 16:9 [derived].
- Gone Home: PCGamingWiki describes a slider with presets from 60 to 90 in steps of 5; the default was not stated. [search-only] https://www.pcgamingwiki.com/wiki/Gone_Home
- Source engine: default `fov` 90 and the Orange Box slider ran 75 to 90 ("recommended ... 110" for 16:9). [fetched] https://wiki.sourceruns.org/wiki/Default_fov . Valve's wiki reportedly says Source FOV is Horizontal+ (GoldSrc: Vertical-). [search-only, wiki page 403] https://developer.valvesoftware.com/wiki/FOV . If 90 is defined at 4:3 it equals 73.7 vertical and so 106.3 horizontal at 16:9 [derived], which is about the same view as BookBoy's 75 vertical (107.5).
- Starter Assets FirstPerson `PlayerFollowCamera.prefab` ships with vertical FOV 40 and near clip 0.2, far 500 (read from the repo, Assets/Starter Assets/Runtime/FirstPersonController/Prefabs/PlayerFollowCamera.prefab). 40 v is a narrow, telephoto view (65.8 h at 16:9).
- Witch Cabin Cleanup (reference game): settings screen shows FOV 90 with a reset button; the axis is not stated (project note, Docs/competitive/16-witch-cabin-cleanup.md line 316). If it is horizontal, 90 h = 58.7 v at 16:9; if it were vertical it would be 121 h, which is implausibly wide for a product like this. UNSOURCED GUESS: it is horizontal (Unity-style "hFOV slider" convention is common for PC slider UIs). Verify by measuring the game.
- Level Design Book: horizontal FOV is the usual reference for games; higher FOV raises apparent edge speed and awareness; accessibility sliders are roughly 60 to 90; "A narrower FOV feels more intimate." [fetched] https://book.leveldesignbook.com/process/blockout/metrics

### 2.4 FOV and apparent size, closeness, distortion

- On-screen size of an object of height s at distance d is s / (2 d tan(V/2)) of the screen height. [derived; pinhole geometry] At fixed distance a wider FOV makes every object smaller on screen; it does not make things feel closer in the sense of larger. Table for a 0.38 m book [derived]:

| Distance | 40 v | 50 v | 60 v | **75 v** | 90 v |
|---|---:|---:|---:|---:|---:|
| 0.75 m | 70% | 54% | 44% | 33% | 25% |
| 1.0 m | 52% | 41% | 33% | 25% | 19% |
| 1.5 m | 35% | 27% | 22% | 17% | 13% |
| 2.0 m | 26% | 20% | 16% | 12% | 10% |
| 2.8 m | 19% | 15% | 12% | 9% | 7% |

  To keep a book the same screen size when widening from 75 v to 90 v you must stand 1.30x closer; going to 60 v lets you stand 0.75x as close, to 50 v 0.61x, to 40 v 0.47x. [derived]
- Size and perspective are separate dials. Perspective (how much nearer things dwarf farther ones) is set by camera position: "Objects close to the lens appear abnormally large relative to more distant objects" at short camera-to-subject distance, and "linear perspective changes are caused by distance, not by the lens per se". [fetched] https://en.wikipedia.org/wiki/Perspective_distortion . A dolly-zoom keeps the subject the same size while the background grows or shrinks, showing that FOV and distance trade off in how background reads. [fetched] https://en.wikipedia.org/wiki/Dolly_zoom
- Wide FOV on a flat screen stretches the edges: for a rectilinear projection a small sphere at angle theta off axis is stretched about 1/cos(theta) tangentially and 1/cos^2(theta) radially. [derived, standard result, no outside citation] At the left and right screen edge of 107.5 h (theta 53.75) that is 1.69x and 2.86x; at the 16:9 corner of 75 v (57.4 degrees) the radial stretch is about 3.4x. At 90 h edges it is 1.41x and 2.0x. The Valve wiki is reported to say that wider FOVs introduce more distortion and that it is less noticeable in motion than in stills. [search-only] https://developer.valvesoftware.com/wiki/FOV
- Physically correct FOV depends on screen size and eye distance (2*atan(s / 2d)), so no single value is right; games choose a compromise. A sim-rig example: the same monitor needs 62 degrees at 50 cm but 46 at 70 cm. [search-only] https://community.khronos.org/t/draw-a-shpere-but-look-more-like-an-egg-whats-matter/73333 and https://simracingcockpit.gg/fov/
- VR/HMD research: rendering with a geometric FOV different from the display's FOV (mini- or magnification) shifts judged distance; the cleaner effect is on distance, with weaker evidence for size. I found no study showing wide FOV directly "shrinks" perceived size. [search-only] https://sreal.ucf.edu/wp-content/uploads/2017/02/BPS12.pdf (PDF not machine-readable) and https://pages.mtu.edu/%7Ekuhl/research/Zhang-2012-MAV.pdf . These are HMD studies, not desktop monitors.
- FOV is also a feel tool: reducing it conveys focus; widening it raises sense of speed. [fetched] Wikipedia link in 2.3.
- Held items: games commonly render the viewmodel at its own FOV because the world FOV "is too wide for the objects held in hand" (see `04-held-item-viewmodel-and-near-clip.md`).

## 3. Numbers and rules of thumb

- Console about 60 (axis unstated); PC 90 to 100; sliders 60 to 90 (accessibility) or up to 100 to 120 (modern PC) — Wikipedia, Level Design Book, PC Gamer coverage.
- 75 v = 107.5 h at 16:9, 121.6 h at 21:9 — derived.
- 90 h = 58.7 v at 16:9, 46.4 v at 21:9 — derived.
- Book height on screen at 1.5 m: 17% at 75 v, 22% at 60 v, 27% at 50 v (0.38 m book) — derived.
- Firewatch default 55 (axis probably vertical = about 86 h) — Panic help page, PCGamingWiki.
- UNSOURCED GUESS: an object-focused cozy game wants the central 40 to 60 degrees of the vertical view dominated by the shelf and books, which argues for a vertical FOV in the 50 to 65 range with a player-adjustable slider. I found no developer talk that states this.

## 4. Confidence and unknowns

- **High**: Unity FOV is vertical; the conversion formula and all table values; the on-screen-size relation; that wider FOV shrinks things on screen at fixed distance.
- **Medium**: typical shipped values (Wikipedia plus secondary coverage; several first-party pages were 403 or truncated); Firewatch default 55 is read from the developer's own help page, axis from a search summary.
- **Low**: what FOV Witch Cabin Cleanup and Librarian really use (Witch Cabin's slider says 90, axis unknown; Librarian unknown); any claim about "feel" or intimacy beyond the Level Design Book sentence.
- Not found: a GDC or Unite talk on FOV choice for object-handling or cozy games; a controlled study of wide-FOV monitor games and perceived object size.

## 5. Implication for BookBoy

- **The claim "a wider FOV makes things feel closer" is wrong as stated.** At a fixed distance a wider FOV makes the book smaller on screen (17% of screen height at 1.5 m for 75 v vs 22% at 60 v). Wide FOV helps see more shelf and exaggerates depth; it does not enlarge. The previous measurement in the repo (a wider lens made things smaller) matches the geometry.
- **Where 75 vertical looks right**: it is not extreme. 107.5 h at 16:9 is about what Source's 90-at-4:3 gives on widescreen. It lets a 2.86 m container (top board looking up) fit at 1.5 m (the repo note says the upper board sits at the screen's top edge).
- **Where it looks risky**: 121.6 h at 21:9 and 139.7 h at 32:9 (Hor+ gives wider and wider views automatically) have 3x or worse edge stretch. Cap or offer a slider, and consider clamping vertical FOV to a narrower value on very wide aspects (Unity does not do this for you).
- **Smallest changes, cheapest first:**
  1. Add the FOV slider already in the backlog (F-003 area) as a range of about 55 to 90 vertical, and display it as horizontal at the current aspect for players (Level Design Book and Gearbox both argue players think in horizontal). Default can stay 75 until playtested.
  2. If books still read too small, first try standing closer (reach and shelf placement) or a vertical FOV of 60 to 65 before enlarging the world again; 65 v gives about 20% of screen height at 1.5 m and 97 h at 16:9 [derived]. Do it by eye in the Game view.
  3. Keep `Lens.FieldOfView` the single source of truth on the Cinemachine camera; Cinemachine "drives the lens settings" of the Brain's Camera (a forum reply, https://discussions.unity.com/t/cinemachine-camera-stacking/1597277 [fetched]), so change FOV there rather than on the Camera component, which Cinemachine will overwrite (UNSOURCED GUESS on the exact overwrite timing; test with a slider).
- **Starter Assets' default of 40 v is a telephoto prototype value**; the project already overrides it, which is right.
