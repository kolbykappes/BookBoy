# Unity practices: researched, sourced notes (2026-10-18)

Why this exists: Kolby pointed out that Unity craft decisions (scale, FOV, lighting) had been made from memory and some were wrong. Per the rule in the root
`CLAUDE.md`, craft decisions are researched and documented here **before** the scene is changed. Twenty-seven topics were researched by parallel web-research
agents (no Unity involved). Every file has: question, sourced findings, numbers, confidence, and "Implication for BookBoy".

**Status of this material:** written by research agents, not yet independently verified by a person. Claims are tagged in each file (fetched, search-only,
derived, UNSOURCED GUESS). Shader-level URP facts come from the GitHub master branch, not the exact 6000.6 package. Nothing here has been applied to the project yet,
and several items need approval under the CLAUDE.md guardrails (`ProjectSettings/`, render-pipeline settings, layers).

## Files

| # | File | One-line answer |
|---|---|---|
| 1 | [01-world-scale-and-units.md](01-world-scale-and-units.md) | Rendering depends only on ratios, so the 1.6x world scale was defensible and can stay; light ranges, shadows and physics need retuning to it |
| 2 | [02-first-person-fov.md](02-first-person-fov.md) | Unity FOV is vertical: 75 vertical = 107.5 horizontal at 16:9. A wider FOV makes objects smaller, not "closer" |
| 3 | [03-eye-height-and-character-controller.md](03-eye-height-and-character-controller.md) | The player is a child (1.15 m eye) but probably still has the adult 2.0 m / 0.5 m radius capsule |
| 4 | [04-held-item-viewmodel-and-near-clip.md](04-held-item-viewmodel-and-near-clip.md) | Held-book shrink to 0.55 is fine; near clip 0.2 is fine; if clipping appears, pull back near surfaces first |
| 5 | [05-urp-point-light-falloff-and-intensity.md](05-urp-point-light-falloff-and-intensity.md) | URP lights are unitless, 1/d2 with a range window and no near-field clamp; the ceiling hotspot is near-field diffuse |
| 6 | [06-exposure-tonemapping-bloom-cozy-look.md](06-exposure-tonemapping-bloom-cozy-look.md) | Keep Neutral over ACES; no auto exposure in URP; Bloom Clamp is at its 65472 default |
| 7 | [07-ambient-gi-lightmaps-probes-light-count.md](07-ambient-gi-lightmaps-probes-light-count.md) | Realtime lights are right for flickering lamps; 5 to 9 lights under Forward+ is far under every cap |
| 8 | [08-shadows-in-urp-interiors.md](08-shadows-in-urp-interiors.md) | Soft shadows High, 4 cascades to 50 m, and 18 shadow maps in a 16-slot atlas are mis-sized for 12 m rooms |
| 9 | [09-stylised-materials-smoothness-specular-emission.md](09-stylised-materials-smoothness-specular-emission.md) | Low smoothness is right; hotspots come from near-field diffuse more than specular |
| 10 | [10-many-small-objects-batching-srp-batcher.md](10-many-small-objects-batching-srp-batcher.md) | One shared material per colour on a shared mesh is correct; ~340 books is probably not a draw-call problem; MaterialPropertyBlock breaks the SRP Batcher |
| 11 | [11-data-driven-items-and-spawning.md](11-data-driven-items-and-spawning.md) | ScriptableObjects as read-only definitions; plain C# state with string IDs for saves and room unload; Instantiate before pooling |
| 12 | [12-room-loading-activation-culling.md](12-room-loading-activation-culling.md) | SetActive is enough for 3 to 10 rooms; skip occlusion culling for now |
| 13 | [13-enter-play-mode-options-and-statics.md](13-enter-play-mode-options-and-statics.md) | Unity 6.6 defaults domain reload off and recommends it; scene reload off is the non-default half; 6.6 adds static-reset attributes |
| 14 | [14-physics-small-objects-and-settings.md](14-physics-small-objects-and-settings.md) | ContinuousDynamic everywhere is a last resort; set solver and depenetration per body; start overlaps can "explode" |
| 15 | [15-piles-throw-and-local-physics-bubbles.md](15-piles-throw-and-local-physics-bubbles.md) | A thrown solid book falls through idle trigger books; a bubble needs neighbours made solid plus a kinematic outer ring, or a proxy PhysicsScene |
| 16 | [16-aiming-picking-and-highlighting.md](16-aiming-picking-and-highlighting.md) | Picking by nearest ray hit is right; pool one shared highlighter instead of per-book hulls, light and particles |
| 17 | [17-ui-scaling-and-world-space-text.md](17-ui-scaling-and-world-space-text.md) | CanvasScaler Match 0 drifts on odd aspect ratios; legend font 20 is at the minimum; world-space legacy Text goes soft close up; use TMP |
| 18 | [18-colour-contrast-and-palette-for-many-groups.md](18-colour-contrast-and-palette-for-many-groups.md) | HSV value is not luminance: 10 of 20 groups are below 2.0 against the wood; a per-hue Oklab lightness solve fixes it; letters stay essential |
| 19 | [19-unity-test-framework-and-batch-mode-workflow.md](19-unity-test-framework-and-batch-mode-workflow.md) | Batch mode cannot open a project the Editor has open; use `run_tests` via Pipeline, or a separate clone with its own Library |
| 20 | [20-editor-background-play-and-focus.md](20-editor-background-play-and-focus.md) | Unfocused Editor freezes Play at frame 1; documented fix `set_autotick`; `capture_game_view --source camera` renders off-screen |
| 21 | [21-unity-cli-and-pipeline-notes.md](21-unity-cli-and-pipeline-notes.md) | eval times out at 5 s and the docs never say it aborts; use `eval_file` with polling; avoid `editor_focus` |
| 22 | [22-save-and-load-design.md](22-save-and-load-design.md) | persistentDataPath/saves, JsonUtility DTOs, schema + migrations, temp-then-replace with .bak, save room-local poses (not seeds), derive doors, Steam Auto-Cloud on LocalLow |
| 23 | [23-aspect-ratio-fov-and-motion-blur.md](23-aspect-ratio-fov-and-motion-blur.md) | Hor+ so any window works; horizontal FOV slider 70-110 (default 90h = 58.7v) converted with HorizontalToVerticalFieldOfView; motion blur off by default and toggleable; no head bob |
| 24 | [24-player-build-and-performance-testing.md](24-player-build-and-performance-testing.md) | Per-call BuildPlayerOptions (no ProjectSettings change), Mono only; self-driving -perf harness writes CSV/JSON/screenshots; Editor numbers mislead |
| 25 | [25-steam-release-path-for-a-small-unity-game.md](25-steam-release-path-for-a-small-unity-game.md) | $100/app, 30-day wait, Coming Soon >= 14 days, June Next Fest, AI disclosure, Runtime Fee cancelled, Steamworks.NET behind an interface, genre price $4.99-$5.99 |
| 26 | [26-ai-assisted-book-models-and-materials.md](26-ai-assisted-book-models-and-materials.md) | Generate the mesh in C#, not with AI; CC0/procedural covers first; keep spine letters game-drawn; Steam disclosure; books already use MaterialPropertyBlock (outside SRP Batcher); conflicts with backlog D-005 |
| 27 | [27-adult-scale-cozy-rooms-and-readable-items.md](27-adult-scale-cozy-rooms-and-readable-items.md) | 1.6x + 1.15 m eye made ceiling 2.6x eye height; readability = screen fraction (16-23%), not world size; proposed 1.0x numbers (eye 1.65, ceiling 2.6, FOV 65v, door 1.1x2.2) |
| 29 | [29-hover-outline-and-placement-ghost.md](29-hover-outline-and-placement-ghost.md) | Inverted hull with smoothed normals and clip-space pixel width (no renderer feature); depth pull so neighbours do not hide it; Librarian look: crisp white line for hover, edge-lit hologram ghost |
| 30 | [30-sprint-fov-kick.md](30-sprint-fov-kick.md) | Sprint already works (Shift, 4 to 6 m/s); a smoothed FOV kick that follows the real speed (+6 vertical, a guess), with a strength setting; Cinemachine has no built-in smoothing |
| 31 | [31-score-feedback-motes-and-shelf-celebration.md](31-score-feedback-motes-and-shelf-celebration.md) | Motes fly to the ITEMS and SHELVES counters, a gold sweep and book wave on a finished shelf (after Witch Cabin), series vs standalone books |

## Where the research contradicts what the project does or what was said earlier

1. **"A wider FOV feels closer" was wrong.** At 75 vertical a 0.38 m book at 1.5 m is 17% of screen height; 22% at 60, 27% at 50. (02)
2. **The 1.6x world scale was not the mistake it was suspected to be**, but it was never followed by retuning light range, shadow settings and physics. (01)
3. **Player capsule** is probably the adult Starter Assets one (2.0 m, radius 0.5), which keeps the camera at least ~0.5 m from any shelf. Not yet checked in the Editor. (03)
4. **Lighting:** lamps at 2.55 m under a 3.0 m ceiling give the ceiling ~32x the floor's light; shadow atlas over-subscribed; bloom clamp at default; ambient very dark; motion blur 0.6. (05, 06, 08)
5. **Live throws cannot land on piles** (idle books are triggers). (15)
6. **Palette:** `GroupColor` fails the 2.0 target for half the groups. (18)
7. **UI:** CanvasScaler Match 0, legend font 20, legacy Text on world-space signs. (17)
8. **CLAUDE.md rule 2** says gamepads are disabled when the Editor is unfocused; the Input System docs say gamepads reach the app by default. Needs an empirical check. (20)
9. **CLAUDE.md does not mention** `set_autotick`, `wait_for` / `status` frame counts, or `capture_game_view --source camera`. (20, 21)
10. **Enter Play Mode:** scene reload off is the non-default half; add the 6.6 static-reset attributes before adding statics. (13)

## Proposed changes (none applied; each needs Kolby's decision, grouped by how risky they are)

**A. Tooling, no gameplay risk (stops the Editor stealing focus):**
- Stop using `editor_focus`; run `set_autotick` once per session and verify frames with `wait_for Time.frameCount changed`; use `capture_game_view --source camera`.
- Correct CLAUDE.md rule 2 after an empirical gamepad check; add the focus workflow to the unity-editor-ops skill.

**B. Scene and builder only (no ProjectSettings):**
- Lower the magelights to ~2.0 m; shrink the player capsule to child size (test held-book clipping afterwards); lamp range and intensity retune for 1.6x.
- Replace `GroupColor` with the per-hue Oklab lightness solve (this is backlog L-021); re-measure rendered screenshots.
- World-space signs to TextMeshPro; CanvasScaler Match to 0.5.
- One pooled highlighter instead of per-book outline objects; stop using MaterialPropertyBlock on items.
- Solid-neighbour bubble or proxy PhysicsScene for throws onto piles (backlog L-020).

**C. Needs explicit approval (ProjectSettings / render pipeline):**
- Shadow settings (soft shadows Medium, fewer cascades, shorter max distance, atlas size), Bloom Clamp, motion blur lower, remove or reduce the 0.5 directional light, scene reload setting.
- Per-body physics settings rather than project-wide; CCD mode change from ContinuousDynamic to Continuous Speculative.

## Open questions the research could not settle

Child eye height by age and reach norms (only Witch Cabin's 3.0 m reach found); Witch Cabin's FOV axis; GPU Resident Drawer requirements in 6.6; whether Instantiate / SetActive timings matter here (unmeasured); how tonemapping changes lit contrast ratios (measure screenshots); no developer write-up on pile handling in Unpacking, Goose-style or tidy-up games was found; Unity's manual disagrees with itself on whether CharacterController pushes Rigidbodies; Unity's two CCD pages disagree on Speculative accuracy.

## Corrections to earlier files (found later)

- File 10 assumed one shared material per colour; the code uses ONE shared `Mat_BookSpine` with a per-book `MaterialPropertyBlock` colour (`Book.cs`), so books are probably outside the SRP Batcher (unconfirmed in the Frame Debugger). Fix by baking colour into vertex colours or per-colour shared materials.
- File 1 called the 1.6x scale defensible; file 27 shows it pushed the ceiling to 2.6x the eye height and the owner wants it undone (backlog T-002).
- **FOV decision (2026-10-18, derived):** files 23 (90 horizontal) and 27 (65 vertical) disagreed. Kolby's Witch Cabin settings screenshot shows an FOV slider at 90 whose track position fits a horizontal range of about 60 to 130, so the settled default is **90 horizontal (58.7 vertical), slider 70 to 110 horizontal (43.0 to 77.6 vertical)**. A 0.32 m book is then 23.7% of screen height at 1.2 m and 19% at 1.5 m (65 vertical: 20.9% and 16.7%), both inside the 16 to 23% reference band. Verify by measuring in the rebuilt room (backlog T-002).
