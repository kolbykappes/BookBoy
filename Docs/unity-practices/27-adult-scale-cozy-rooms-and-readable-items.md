# 27 - Adult-scale cozy rooms and readable small items (how to keep books legible without a giants' hall)

Researched 2026-10-18. Web research plus read-only inspection of repo docs, screenshots and `LamplitBuilder` constants. Unity was not opened; nothing under Assets/ or ProjectSettings/ was changed. Builds on `01`, `02`, `03`, `04`, `05`, `17`, `18` in this folder, `Docs/09-scale-reference.md` and `Docs/competitive/scale-and-camera-comparison.md`; those findings are not repeated.

Tags: **[fetched]** = I opened the page and read the text; **[search-only]** = only a search-result summary seen (page blocked, truncated, or not opened); **[repo]** = read from this repo (docs, screenshots in Docs/competitive/img, builder constants); **[derived]** = my arithmetic or reading off a screenshot (shown so it can be redone); **UNSOURCED GUESS** = I could not source it. Where sources disagree I say so.

## 1. Question

The project started life-size with an adult (eye 1.7 m); books looked small, so the whole handled world was scaled 1.6x (`LamplitBuilder.WorldScale`) and the player's eye dropped to 1.15 m. The owner now says the rooms feel like a giants' hall and wants an adult-sized actor in a cozy room. How do successful first-person cozy tidy/sorting games (and shipped first-person games generally) keep small items readable and intimate without making the world gigantic? Specifically: (a) normal adult first-person numbers; (b) the standard readability tricks and the factors real games use; (c) what makes a room cozy; (d) what proportions the reference games imply for a book read from 1 to 1.5 m, for an adult at eye 1.65 m and FOV 60 to 75 vertical; (e) a concrete set of numbers for BookBoy at 1.0x world scale, and which exaggerations to keep.

## 2. Findings

### 2.1 Typical adult first-person numbers (part a)

- Level Design Book, metrics table. Unity: bounding box 1.0 x 1.8 m (or 2.0), eye 1.5 to 1.7 m, interior wall 3.0 m, minimum hall 2.0 m, door 1.25 x 2.5 m, step 0.1 x 0.15 m. Unreal: 60 x 176 cm, eye 152 cm, wall 300 cm, hall 150 cm, door 110 x 220 cm. Quake/Source: 32 x 72 in, eye 64 in, wall 128 in, hall 64 in, door 56 x 112 in. US real life: 20 x 69 in, eye 66 in, wall 96 in, hall 48 in, door 36 x 80 in. "Minimum hallway width should be at least double the player width." "In-game eye height is often lower than real eyes"; camera height "strongly affects how fast and large the world feels." [fetched] https://book.leveldesignbook.com/process/blockout/metrics
- Unreal guide: character 180 uu, reference cube 50 x 180; walls 300 to 400 uu; doors 110 to 140 wide x 210 to 230 tall (plus 10 to 20 for a frame); steps 15 high x 30 deep. "Realistic real-world dimensions can still feel small in play, so test in-game and adjust." No eye height, corridor width or furniture heights on the page. [fetched] https://worldofleveldesign.com/categories/ue5/guide-to-scale-dimensions-proportions.php
- Unity manual: use about 2 m for a human-like CharacterController; step offset 0.1 to 0.4 for a 2 m human; skin width above 0.01 and above 10% of radius (read in file 03). [fetched via file 03] https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/character-control-section/class-character-controller.md
- Disagreements to know about. Source door: 56 x 112 in (Level Design Book) vs 48 x 108 in (WoLD, file 01). Unity capsule 1.8 m (table) vs "around 2 m" (manual). Unreal capsule 176 cm (table, half-height 88) vs forum reports of the first-person template at half-height 96 with camera offset 64 (192 collision, 160 eye). [fetched / search-only] https://book.leveldesignbook.com/process/blockout/metrics and https://forums.unrealengine.com/t/first-person-player-scale/411189 (the thread page itself held only the question; the template numbers are from search snippets of Unreal Answers).
- A GameGuru forum thread by a poster with a US architectural background lists real-world defaults (door 4 ft x 7'6", ceiling 8'4", player capsule "about 7'10" tall, 3 ft diameter", camera about 8 in above average height). It contradicts itself in unit conversions (a 7'10" capsule is 2.39 m, not the 2.16 m given), so treat as anecdote. Also says "real sized doors aren't used in fps games as they look small due to the field of view." [fetched] https://forum.game-guru.com/thread/211371
- Human standing eye height (anthropometric tables): US men 60.5 / 64.6 / 69.0 in (5th / 50th / 95th percentile), women 55.9 / 59.7 / 64.0 in, i.e. men 1.54 / 1.64 / 1.75 m, women 1.42 / 1.52 / 1.63 m [derived from inches]. A different table in mm gives US men 1595 / 1710 / 1825 and women 1420 / 1525 / 1630. The two tables disagree by about 5 to 7 cm for men (different survey populations). So 1.65 m is roughly an average man or a tall woman. [search-only] https://www.ergocenter.ncsu.edu/wp-content/uploads/sites/18/2017/09/Anthropometric-Summary-Data-Tables.pdf and https://personal.cityu.edu.hk/meachan/Online%20Anthropometry/Chapter2/Ch2-2.htm
- Interaction reach: Half-Life 2 maximum "use" distance 82 units (a switch or door handle), about 1.5 m at 16 units per foot [search-only; page is 403, figure from a snippet of the Polish copy of the Valve wiki] https://developer.valvesoftware.com/wiki/Dimensions_(Half-Life_2_and_Counter-Strike:_Source) . Witch Cabin Cleanup: pick-up and place reach 300 cm, 350 cm after the upgrade [repo, ledger screenshot 18, Docs/competitive/16-witch-cabin-cleanup.md line 41]. Unity forum examples use 2 m, 3 m and a Starter-Assets-based tutorial default of 4 m: tunable, no standard. [search-only] https://discussions.unity.com/t/how-can-i-tell-if-a-player-is-close-enough-to-an-object-to-pick-it-up/117360 and https://community.gamedev.tv/t/raycast-not-work-properly-on-y-axis/219503
- Library furniture in the real world: shelf pitch 13 in (0.33 m) for books, 15 in for bound journals; aisles 36 in minimum (ADA), 42 in preferred, some libraries aim for 3 ft in stacks and 4 ft on main aisles. [search-only] https://www.intraspace.com.au/2026/03/how-modern-shelving-systems-improve-library-accessibility-and-navigation and https://schooloutfitters.com/catalog/gttk.php/cPath/CAT6_CAT1311 (page bodies not opened).

### 2.2 Shipped first-person games already scale the world up against the player (part b: the exaggeration factor)

- Valve scales player models at about 12 units per foot and geometry/props at about 16 units per foot; "use these measurements as guidelines only"; sizes are "eyeballed" in practice; some items (doors, trash cans) are about a third larger than real. That is a 1.33x world-against-player factor. [search-only; page is 403, from snippets] https://developer.valvesoftware.com/wiki/Unit_scales
- A Garry's Mod/VRMod developer states: "players are not small but rather the maps and some props are made a bit oversized so they 'look better' in first person perspective on a typical computer monitor"; characters use 1 unit = 1 inch, maps 1 unit = 0.75 inch, so the player is about 180 cm. The same thread measures the player's eye at "just under 5 feet" against a real floor plan. [fetched] https://steamcommunity.com/workshop/filedetails/discussion/1678408548/1836811737988164764/
- Derived consequence [derived from the two bullets above and the Level Design Book table]: in map metres a Source player is 72 x 0.75 = 1.37 m tall with eyes at 64 x 0.75 = 1.22 m, in a 2.44 m (128 u) room with 2.13 m (112 u) doors. Source's "small eye, big world" is therefore a documented, shipped pattern, and it is the one BookBoy copied (eye 1.15 m). It works there because the rooms stay tight (hall 64 u, about 1.22 m in map metres); it is not what made BookBoy's rooms feel huge (see 2.4).
- Ratios across engines [derived from the fetched Level Design Book table]:

| Reference | Ceiling / eye | Door height / eye | Door height / player height | Ceiling / player height |
|---|---:|---:|---:|---:|
| Real US house | 1.45 | 1.21 | 1.16 | 1.39 |
| Unity (1.8, eye 1.6) | 1.88 | 1.56 | 1.39 | 1.67 |
| Unreal (176, eye 152) | 1.97 | 1.45 | 1.25 | 1.70 |
| Source (72, eye 64) | 2.00 | 1.75 | 1.56 | 1.78 |
| **Lamplit now (eye 1.15, ceiling 3.0, door 2.2)** | **2.61** | **1.91** | n/a | n/a |

  So engines already make doors 8% to 34% taller (against the player) and ceilings 20% to 28% taller than real; Lamplit's ceiling is 1.3x past even that. [derived]
- Practitioner reports: furniture in Max Payne "about 1/3 bigger" so it reads right on a third-person camera (a forum comment, no primary source); "you can't simply adopt a real life scale and expect the map to 'feel' right"; "typically you have to scale up to avoid claustrophobia"; "Doors have to be taller than they should be, wider than they should be"; but also "over-compensation, when the player looks too small within the environment." The last line describes BookBoy's current state. [search-only for the Polycount comment https://polycount.com/discussion/comment/1426594 (403)] and [fetched for the 2005 blog quotes] https://www.johnsto.co.uk/blog/that-d-word-again/
- Firewatch: "objects and places were scaled based on what felt right, not accurate"; no numbers. [fetched] https://80.lv/articles/creating-the-world-of-firewatch/
- Camera height changes perceived size: a 2023 Scientific Reports study reports that a lower simulated eye height makes objects look bigger (VR; posture-dependent). [search-only; page redirected to a login] https://www.nature.com/articles/s41598-023-47364-6 . Together with the Level Design Book's "camera height strongly affects how fast and large the world feels", this says the 1.15 m eye by itself makes everything feel bigger, which is the "giants" feeling in a room scaled up 1.6x on top.
- Unreal guidance thread (a question, no answers) frames the same choice: scale the level to the capsule, or shrink the capsule and build real dimensions. [fetched] https://forums.unrealengine.com/t/first-person-player-scale/411189

### 2.3 Readability tricks the reference games use (part b)

Rows marked [repo] come from the owner's screenshots in `Docs/competitive/img/` and notes in `Docs/competitive/`.

- **Chunky books (thick spines) rather than tall ones.** Reading Witch Cabin Cleanup frame 31 (1920 x 1080): three identical purple books occupy about 170 px of width (about 57 px each), their height from the board is about 175 px; the teal book is about 63 x 190 px; so thickness / height is about 0.30 to 0.33 and a book is about 16 to 18% of screen height. Board-to-board spacing about 258 px (24%), so book height is 0.68 to 0.74 of the spacing. [derived from a screenshot, +/- 10%] `Docs/competitive/img/witch-cabin/31-books-on-shelf-3of5-hover-pickup-outline.webp`. Librarian, from the owner's measurements: book 22 to 23% of screen height, spine about 62 px thick (thickness / height about 0.25), height 0.75 to 0.8 of board spacing, about 10 books per compartment. [repo] `Docs/competitive/scale-and-camera-comparison.md`. A real hardback is about 0.15 thick-to-height at 24 cm x 3.5 cm [derived, typical], so both games exaggerate thickness about 1.7 to 2x, more than height.
- **Eye line inside the shelf band.** Both Witch Cabin (frame 31: crosshair on the middle of the top-row books, other boards one spacing above and below) and Librarian (frame 05) frame the aimed row near the crosshair; Shelves and Sorcery sits at the middle board of a three-board bay. [repo, screenshots; see the existing comparison file]
- **Close but not too close: the player stands about 1.5 m from what they read.** If Witch Cabin's FOV 90 is horizontal (58.7 vertical at 16:9, file 02) and a book is 0.30 m, 16 to 18% of the screen puts the player 1.5 to 1.7 m away [derived; both assumptions unverified]. Witch Cabin's reach is 3.0 m, so it is not distance-limited.
- **Outline / highlight of the aimed item and a ghost of the held item in the slot.** Witch Cabin and Librarian both draw a white outline on the aimed book and a translucent ghost in the slot; Witch Cabin shows "Unlocking the Tarot 3/5" under the crosshair. [repo, screenshots 29 and 31, Librarian 01 and 02] Half-Life: Alyx lights key items subtly instead of using "a glaringly obvious yellow mesh", makes the pistol-magazine glow proportional to rounds, and glows the targeted object. [fetched] https://www.roadtovr.com/these-details-make-half-life-alyx-unlike-any-other-vr-game-inside-xr-design/ . Forum reports say an outline "can sometimes be missed because its color blends too much into the world color" and that small objects need a stronger cue (a recolorable highlight is a Witch Cabin setting, file 16). [search-only] https://discussions.unity.com/t/showing-that-an-item-is-interactable-can-be-picked-up/558864
- **Labels as a fallback, not the main reader.** Librarian prints a large series icon plus a vertical title and a tiny digit (under 7% of spine height, hardest to read) and shows the aimed book's full title at the bottom centre; Shelves and Sorcery relies on a HUD list and nameplates. [repo, comparison file]
- **Close-up inspect mode.** Gone Home lets the player pick up most small objects and turn them to any angle [search-only] https://giantbomb.com/gone-home/3030-38327/ ; Unity inspect-system assets exist for the same (pick up, rotate, zoom, title/description). [search-only] https://sometimesatnight.itch.io/object-inspection-system . None of the three book-sorting references uses an inspect mode in the screenshots; they use the held-book display at the screen edge (a third of the width in Librarian) and a title strip. [repo]
- **Narrower FOV for intimacy and size.** "A narrower FOV feels more intimate"; console about 60 and PC sliders 60 to 90 (axis not stated); Firewatch's default 55 (file 02). [fetched] https://book.leveldesignbook.com/process/blockout/metrics
- **Text-size thresholds for reading things at a distance.** Microsoft HoloLens research: minimum legible text 0.35 to 0.4 degrees at 2 m (12 to 14 mm) and 0.4 to 0.5 at 0.45 m; comfortable 0.6 to 0.75 degrees at 2 m (21 to 26 mm). Physical-display research, not a game-monitor rule. [fetched] https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/text-in-unity
- **Deeper shelves viewed at an angle; fewer-but-bigger items per shelf.** No reference found uses angled shelves; Witch Cabin has two-deep cabinets that "hide the back" [repo, file 16 line 180]; Witch Cabin boards hold 3 to 5 books in frame 31 and the Reading Room spec here holds 5 per board; Librarian holds about 10. UNSOURCED GUESS that fewer is better; the references show both.
- **Pick-up distance as progression.** Witch Cabin's "Reach" is a purchasable charm (300 to 350 cm), so a short default is normal. [repo, file 16 line 41]

### 2.4 What keeps a room cozy (part c)

- Ceiling height shapes thinking. Meyers-Levy and Zhu (2007): 10 ft rooms primed relational, abstract processing; 8 ft rooms primed item-specific, detail-focused processing; no effect "when the ceiling height goes unnoticed"; Baird et al. (1978): people preferred about 10 ft (3.0 m) ceilings; lighter ceilings seem taller (2010); high-ceiling rooms were rated more beautiful and approached more in an fMRI study (2015). Application noted: lower ceilings suit analytical, proofreading-like tasks. Caveats: few studies, no sample sizes on the page; priming interpretation not settled. [fetched] https://en.wikipedia.org/wiki/Cathedral_effect . This favours a lowish ceiling (about 8 to 8.5 ft, 2.4 to 2.6 m) for a sorting room, with one taller space (the hub) for relief.
- Low ceilings for intimacy, high for formality: Alexander's "Ceiling Height Variety" pattern, which says no room of homogeneous height serves a group well; no figure such as 7'6" confirmed. [search-only] https://kk.org/cooltools/?p=7591
- Human scale in massing: "Big monoliths won't feel human-sized"; break forms into parts "appropriately sized for people." [fetched] https://book.leveldesignbook.com/process/blockout/massing
- Narrower, tighter spaces and FOV read as intimate or claustrophobic; a PC Gamer article on claustrophobia and FOV exists but its body was truncated. [search-only] https://www.pcgamer.com/au/fov-isnt-everything-how-claustrophobia-makes-a-more-immersive-fps
- Lighting: several low sources beat one overhead fixture; layers (ambient, task, accent); "you can't have pools of light without the darker places in between"; blue-white feels cold. No Kelvin or lux numbers. [fetched, interior-design blog, qualitative only] https://www.us.pooky.com/blogs/inspiration/secret-cozy-lighting . A game-lighting article suggests warm key and fill with subtle cool shadows for safe hubs, modest ambient fill and "darkness allowed to exist". [search-only] https://gamineai.com/blog/game-lighting-design-creating-atmosphere-and-mood-in-your-games . I found no game source for 2700 K; treat it as a common warm-white reference, set by eye.
- Light distance matters more than light count here: from file 05 (derived), a magelight hung 0.45 m under the ceiling lights the ceiling about 32x harder than the floor; the lever is orb height relative to the ceiling, not just ceiling height.
- Clutter density and room-size-to-furniture ratios: I found no game-specific source. The references show: Witch Cabin's first room is a small bedroom with a floor pile in front of a wall of shelves (frame 06: far wall visible a few metres away, ceiling beams just above the tallest cabinet), and the apothecary is "a different scale" with 14x the items (file 16 line 193). [repo] UNSOURCED GUESS for any numeric ratio.
- Window of FOV: file 02 notes that 75 vertical gives 107.5 horizontal at 16:9 and 121.6 at 21:9; wide views make rooms read as larger and more open. [derived, file 02]

## 3. Numbers and rules of thumb

Sourced numbers (from section 2) and derived ones are kept apart.

**Sourced adult first-person numbers**

| Quantity | Value | Source |
|---|---|---|
| Capsule height | 1.8 m (Unity table, Unreal 1.76), about 2 m per Unity manual | LDB, Unity manual |
| Capsule width / radius | 0.6 to 1.0 m wide (radius 0.3 to 0.5); Starter Assets 0.5 | LDB, file 03 |
| Eye height | 1.5 to 1.7 m (Unity), 1.52 m (Unreal), real adults 1.42 to 1.75 m | LDB, anthropometric tables |
| Ceiling | 2.44 m real (96 in), 3.0 m engine defaults, 2.4 to 3.0 m range | LDB, WoLD |
| Door | 0.91 x 2.03 m real; 1.1 x 2.2 m (Unreal); 1.25 x 2.5 m (Unity) | LDB, WoLD |
| Hall | 1.22 m real, 1.5 m Unreal, 2.0 m Unity; at least 2x player width | LDB |
| Library aisle | 0.91 m minimum, 1.07 m preferred, 1.2 m main | search-only |
| Real library shelf pitch | 0.33 m (13 in) | search-only |
| Reach | 1.5 m (HL2 use), 3.0 to 3.5 m (Witch Cabin), 2 to 4 m (forum examples) | see 2.1 |
| Exaggeration of world vs player | 1.33x (Valve 12 vs 16 units per ft; GMod 1 in vs 0.75 in); doors +8% to +34%; furniture +33% (Max Payne, unverified) | see 2.2 |

**Derived rules of thumb**

- Reference-game book proportions [derived from screenshots]: book height 16 to 23% of screen height; spine thickness 5.3 to 5.7% of screen height (about 57 to 62 px at 1080p); thickness / height 0.25 to 0.33; book height 0.7 to 0.8 of board spacing; packed spines (pitch about equal to thickness).
- Screen fraction of a book = h / (d x 2 tan(V/2)). At eye 1.65 and V 60 to 75, standing 1.0 to 1.5 m (a 0.24 m life-size paperback-hardback gives 10 to 21%):

| Book height | Distance | V 60 | V 65 | V 70 | V 75 |
|---|---|---:|---:|---:|---:|
| 0.24 m (life-size) | 1.0 m | 20.8% | 18.8% | 17.1% | 15.6% |
| 0.24 m | 1.5 m | 13.9% | 12.6% | 11.4% | 10.4% |
| 0.30 m | 1.2 m | 21.7% | 19.6% | 17.9% | 16.3% |
| 0.30 m | 1.5 m | 17.3% | 15.7% | 14.3% | 13.0% |
| **0.32 m** | **1.2 m** | 23.1% | **20.9%** | 19.0% | 17.4% |
| **0.32 m** | **1.5 m** | 18.5% | **16.7%** | 15.2% | 13.9% |
| 0.38 m (Lamplit now) | 1.5 m | 21.9% | 19.9% | 18.1% | 16.5% |
| 0.38 m | 2.0 m | 16.5% | 14.9% | 13.6% | 12.4% |

  A life-size 0.24 m book is below 15% at any V of 65 or more from 1.5 m, which is why it looked small. A 0.32 m book at V 65 from 1.2 m is 20.9%, in the Librarian/Witch Cabin range, and at 1.5 m gives the same 16.7% as today's 0.38 m book at V 75 and 1.5 m (16.5%).
- Spine thickness at 1080p [derived]: 0.08 m at 1.2 m is 57 px at V 65 (62 at V 60, 47 at V 75), matching the references' 57 to 62 px; 0.085 m at 1.5 m is 48 px at V 65.
- Horizontal FOV at 16:9: V 60 = 91.5, V 65 = 97.0, V 70 = 102.4, V 75 = 107.5 [derived, formula in file 02].
- Look angles (eye 1.65, book-centre height c, standing 1.2 m away) [derived]: c = 0.96 m, 30 degrees down; 1.11, 24 down; 1.38, 13 down; 1.53, 6 down; 1.80, 7 up. At 1.5 m the same rows are 25, 20, 10, 5 down and 6 up. Natural gaze is commonly said to sit about 10 to 15 degrees below horizontal (UNSOURCED GUESS from general ergonomics), so rows with centres about 1.1 to 1.8 m are comfortable and the floor and bottom row need a downward tilt.
- Ceiling / eye ratio: real 1.45, engine defaults 1.9 to 2.0, Lamplit now 2.6. With eye 1.65 a 2.6 m ceiling is 1.58 and a 2.7 m ceiling 1.64 [derived].
- Shelf proportions: book height / board pitch 0.7 to 0.8 (Witch Cabin 0.68 to 0.74, Librarian 0.75 to 0.8, real library 0.76 for a 0.25 m book on a 0.33 m pitch [derived]).

## 4. Confidence and unknowns

- **High**: the geometry (screen-fraction formula and tables); Level Design Book and WoLD numbers (read directly); Valve's 12:16 convention and the GMod 1 in vs 0.75 in statement (the second was fetched, the first is search-only); that FOV, eye height and ceiling ratio, not absolute size, set the "giants" feeling.
- **Medium**: the screenshot-derived reference proportions (one frame per measurement, hand-read pixel positions, unknown FOV and distance; thickness / height 0.25 to 0.33 should be good to about 15%); the Cathedral effect (lab priming effects, small studies); anthropometric tables (two tables disagree).
- **Low**: Max Payne "a third larger" (forum comment); HL2 use range; library shelf and aisle numbers (search-only); the Nature eye-height study (VR, abstract not read); any claim about exactly which exaggeration factor a given cozy game uses.
- **Not found despite searching**: any measured or documented scale for Unpacking (isometric pixel art, no first-person scale), House Flipper (a Workshop post suggests a 1.6 m reference player but the page I fetched did not show the line; UNSOURCED), Cooking Simulator, PowerWash Simulator (interviews give no dimensions), Viscera Cleanup Detail, Lethal Company or Content Warning (a mod page implies the base player is the 1.0 scale unit and sets a 1.7 maximum "for the ship interior and doors", search-only https://thunderstore.io/c/lethal-company/p/Toybox/LittleCompany/ ), Dredge (not first person), Gone Home (GDC level-design session exists, content not available), Tiny Tina-style tidy games. No GDC talk or dev blog states a numeric "scale up key props by X" for cozy tidy games. The FOV and eye height of Witch Cabin and Librarian are still unmeasured.
- The 1.2x to 2x exaggeration range suggested in the brief is not supported by any source I found beyond 1.33x; anything above is the project's own choice (the 1.6x here).
- Not verified: live values of the Lamplit player capsule (file 03), actual container and book dimensions in the built scene (from builder constants and docs only).

## 5. Implication for BookBoy

**Verdict.** The owner is right that the 1.6x room and the 1.15 m eye are the cause of the giants' hall, and the evidence says it was avoidable. Books can stay chunky and readable at 1.0x rooms with an adult eye, because what makes books readable is (1) screen fraction of 15 to 25%, set by item size, standing distance and FOV; (2) thick, packed, high-contrast spines; (3) boards inside the eye-line band; (4) outline, ghost and a title strip. None of those need rooms, doors or ceilings above life size. Shipped engines already give a 1.1x to 1.35x door/ceiling-vs-player margin; Lamplit's ceiling is 2.6 x eye against an engine norm of about 2.0 and a real 1.45.

**Smallest sensible change (order matters; do it by eye in the Game view):**

1. Set the world back to 1.0x for rooms, doors, hub, containers, desk props (`WorldScale` 1.6 to 1.0), and give items only their own exaggeration (an `ItemScale` of about 1.33 on height and depth, about 1.8 to 2x on thickness). Do not scale the player.
2. Raise the eye to 1.65 m, capsule 1.8 m (file 03 numbers below).
3. Drop the default vertical FOV from 75 to 65 (97 horizontal at 16:9) and keep it adjustable 55 to 80 (file 02). This alone restores the book screen fraction lost by shrinking items.
4. Put the boards at the adult eye band (table below), pack the slots, and keep the outline, ghost and title strip already present.
5. Re-aim the magelights and check the ceiling hot-spot (file 05).
6. If books still read small after this, try eye 1.5 m (the low end of Unity's table) and 60 V before enlarging the world again.

**Proposed numbers at 1.0x** (everything here is a **derived proposal** unless the Source column says otherwise)

| Quantity | Proposal | Basis |
|---|---|---|
| Player capsule | height 1.8, radius 0.35, skin 0.04 (above 10% of radius), step 0.25 (keep Starter Assets), `GroundedRadius` equal to radius | Unity table and manual (sourced ranges); radius 0.35 derived (Unreal 0.30, Source 0.40, Starter 0.5) |
| Eye height | 1.65 m (camera root = 1.65 minus the settled floor offset; check live, file 03) | LDB Unity range 1.5 to 1.7; 0.92 of capsule height |
| Move speed | 4 walk, 6 sprint (Starter Assets) | repo |
| Reach | 2.8 m, keep (1.7x eye); Witch Cabin 3.0 | repo, sourced |
| Vertical FOV | 65 default, slider 55 to 80 | derived (table in section 3) |
| Ceiling | 2.6 m in rooms (1.58 x eye); 2.9 m in the hub for relief | derived; Cathedral effect, engine ratios |
| Door | 1.1 x 2.2 m (lintel to ceiling) | Unreal table (sourced) |
| Hub | 3.0 m wide x 15.5 m long (current 4.8 x 24.8 divided by 1.6); at least 2x the 0.7 m capsule | derived from current builder values and LDB hall rule |
| Rooms | start at 1/1.6 of today: about 7.5 m wide x 7.1 to 8.4 m deep; trim by feel, aisles at least 1.8 to 2.2 m between facing container fronts so the player can stand 1.0 to 1.3 m from a shelf | derived; no sourced ratio |
| Container | 0.55 to 0.6 m wide (5 slots at 0.09 plus margins), 0.36 m deep, about 1.85 m tall: solid base 0.95 m, two boards, top clear height 0.40 m | derived |
| Shelf boards (top surfaces) | 0.95 and 1.40 m (0.45 m pitch, the existing `ShelfSpacing`); book centres 1.11 and 1.53 m, which is 24 and 6 degrees down at 1.2 m. Optional 3-board bay: base 0.75, tops 0.75 / 1.17 / 1.59, height about 2.05 m | derived from eye 1.65, section 3 look angles; Librarian/Witch Cabin eye-in-band rule (repo) |
| Book | height 0.32 (range 0.28 to 0.34), depth 0.22, thickness 0.06 to 0.085 | derived from section 3 (about 21% at 1.2 m, V 65; spine 57 px) |
| Slot pitch | 0.09 (thickest book 0.085) | derived; packed like the references |
| Held book | draw at about 0.2 to 0.22 m high (`CarryScale` about 0.65 of a 0.32 m book) | derived; keep the shrink idea (file 04) |
| Magelight height | 2.0 m with a 2.6 m ceiling (0.6 m of ceiling gap, ratio about (2.0/0.6)^2 = 11); darken the ceiling albedo; hang over the aisle centre, not over a door | derived from file 05's inverse-square method |
| Signs | above containers: 2.1 to 2.4 m | derived |

**Currently exaggerated values to keep** (they match what the references do): thick spines (0.06 to 0.085 m, about 2x real thickness) and the packed pitch; item height of about 1.33x (the Valve factor); a door wider than real (1.1 vs 0.91 m); a long reach (2.8 m); the held-book shrink; the large series letter and number on spines (aim for digits at 10% of spine height or more, so about 3 cm on a 0.32 m book; file 17); the 0.45 m board pitch (a little roomier than a real 0.33 m library shelf so a hand fits). **Values to drop:** the 1.6x world for rooms, doors, containers and desk props; eye 1.15; ceiling 3.0 over a 1.15 m eye; 12 m wide rooms; and, as a default, V 75.

**Also worth doing, cheaply:** a single "aimed book" title strip and an outline at all distances (both already present); a recolorable highlight option (Witch Cabin); no inspect mode until playtest shows the title strip is not enough. Verify the new numbers with a Game-view capture at 1.2 m and 1.5 m before committing (rule: judge in a blockout, from the player's eye).

**Caveat.** The owner's own screenshots, not these numbers, are the real test: capture Lamplit again at 1.0x with the table above, compare the book height and spine thickness fractions against 16 to 23% and 5.3 to 5.7%, and tune FOV and `ItemScale` rather than touching the rooms.
