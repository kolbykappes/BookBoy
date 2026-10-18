# 20. Editor background Play Mode, focus, and verifying without the Editor in front

Research date 2026-10-18. Project: Unity 6000.6.6f1, URP, Windows, `com.unity.pipeline` 0.8.0-exp.1, Unity CLI 1.0.0-beta.13.
Method: web research only (Unity manual, scripting API, package docs, the `Unity-Technologies/skills` repo, forum/issue-tracker snippets). Nothing was run in Unity.
Anything not backed by a URL is marked **UNSOURCED GUESS**.

## 1. Question

Why does Play Mode freeze (and Game-view captures go stale) when the Editor window is not in front, which official switches exist (Run In Background, Interaction Mode, Input System background behaviour, Play Focused/Unfocused, Pipeline autotick), can a script advance frames deterministically without focus, and what is the best way to verify gameplay and visuals without bringing the Editor to the foreground?

## 2. Findings

### 2.1 What the engine docs say (they partly disagree with what we observe)

- `Application.runInBackground` defaults to `false`: the Player pauses in the background. The page says nothing about the Editor or Play Mode. https://docs.unity3d.com/ScriptReference/Application-runInBackground.html
- Player Settings, Windows (6000.6): "Run In Background ... When disabled, the application pauses when it loses focus." https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/windows/playersettings
- Input System docs: "In the Editor, [Run In Background] is treated as always enabled" and "In Play mode, the Editor keeps running the player loop regardless of focus, so background running is effectively always on." Focus in the Editor "follows the Game View". https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Settings.html and https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Devices.html
  - **Source conflict:** this says the player loop keeps running unfocused. Our observation (frames freeze, captures stale) and the Unity CLI skill (below) say the Editor throttles/stalls. The reconcilable reading: the *player loop* is allowed to run, but the *Editor tick that drives it* is throttled/stalled by the OS-level inactive window. Neither Unity page states this explicitly, so treat the mechanism as **UNSOURCED GUESS**; the symptom and the cure are sourced (2.2).
- `EditorApplication.update` runs at the Editor's tick rate, which "is not fixed"; "running in the background" is explicitly listed as something that can change it (default idle interval 4 ms via EditorPrefs `ApplicationIdleTime`). https://docs.unity3d.com/ScriptReference/EditorApplication-update.html
- Preferences > General > **Interaction Mode** (Default = idle up to 4 ms per tick; No Throttling; Monitor Refresh Rate; Custom with Frame Throttling 0-33 ms): "In Play mode, Unity ignores any throttling that you apply using this preference." The page has no setting for unfocused/background behaviour. So Interaction Mode is **not** the fix. https://docs.unity3d.com/Manual/preferences-general.html
- Game view "Play Focused / Play Unfocused / Play Maximized" only decide where focus goes when Play Mode starts (Play Unfocused = does not move focus to the Game view). They do not keep an unfocused Editor ticking. https://docs.unity3d.com/Manual/GameView.html
- `EditorApplication.focusChanged` / `isFocused` exist (OS-level focus of the Editor). https://docs.unity3d.com/ScriptReference/EditorApplication-focusChanged.html
- Forum history (not official): `EditorApplication.update` stops firing when the Editor is unfocused; `Application.runInBackground = true` was reported to help in 5.3.3. https://discussions.unity.com/t/editorapplication-update-in-background/53713 . A search snippet of the issue tracker says Unity throttles on deactivation "by design" with no Editor setting to reduce it (the issue pages themselves returned 404/redirect, so this is snippet-only, low confidence).
- Editor scripts that set `Application.runInBackground = true` on entering Play Mode are the common community pattern. https://discussions.unity.com/t/keep-editor-playing-while-in-background/168236

### 2.2 What the Unity CLI / Pipeline docs say (this is the officially sanctioned fix)

- The vendor's `unity-cli` skill: "An unfocused Editor can freeze at frame 1 while `unity status` still reports it as playing. A screenshot can show a frozen frame that looks like a live game." https://github.com/Unity-Technologies/skills/blob/main/skills/unity-cli/SKILL.md (raw: https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/SKILL.md)
- Its Play-mode procedure (https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/references/playmode-verification-loop.md):
  1. `unity command set_autotick` once per session, **before** entering Play Mode. Defaults: `enable=true`, `interval_ms=16` (about 60 Hz), `persist=true` (survives domain reloads). `-- --persist false` for a one-off.
  2. Check `unity command --query editor_play --format json` for a `run_in_background` parameter; if present it sets and restores `Application.runInBackground`; if not, read the value with eval, set it true, and restore after `editor_stop`.
  3. After `editor_play` the listener drops during domain reload: `unity status --until-ready --project-path <path> --format json`.
  4. Prove frames advance: `unity command wait_for -- --condition '{"member":"UnityEngine.Time.frameCount","op":"changed"}' --timeout_s 10`. `met:true` + `framesObserved>0` = running; `timedOut:true` with unchanged frame count = the frozen symptom. `timeout_s` default 30; `--async true` returns a `waitId` to poll with `wait_status`, cancel with `wait_cancel`.
  5. `unity status --format json` also shows `frameCount` and `playerLoopTicking` per Play-mode instance (added in CLI beta.12): https://docs.unity.com/en-us/unity-cli/release-notes
  6. `capture_game_view -- --source screen|camera --save_path Screenshots/x.png`. `screen` includes overlay UI; `camera` renders one camera off-screen and omits overlay canvases. Pass `source` explicitly because its default changed in pipeline 0.8.0-exp.1.
- `set_autotick` reference text: "Keeps the Editor ticking while unfocused by forcing `EditorApplication.SignalTick` at a throttled rate"; `interval_ms` 16 default, `0` = every update which "pegs a CPU core"; `persist` stores to SessionState. https://docs.unity3d.com/Packages/com.unity.pipeline@0.6/manual/commands/editor-lifecycle-and-observability.html (0.6 page; the 0.8 manual's page was not reachable).
- The unity-pipeline skill: "Run `set_autotick` first. Recompiles and tests can hang while the Editor is unfocused." https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-pipeline/SKILL.md
- Pipeline changelog: 0.3.0 server keeps working when minimised ("RunInBackground"); 0.5.0 `set_autotick` persists across domain reloads; 0.6.0 the server no longer permanently enables Player Settings "Run In Background" - the driver saves and restores the original; 0.8.0 `capture_game_view` defaults to `source=screen` and works in Edit Mode (`camera` restores old behaviour). https://docs.unity3d.com/Packages/com.unity.pipeline@0.8/changelog/CHANGELOG.html
- `editor_focus` ("Brings the Editor window to the foreground") exists in 0.6 docs. This is the command that would annoy the owner. Do not use it.
- Installing the Pipeline package into an already-open Editor does not load until the window regains focus (CLI beta.12 notes; skills doc). That is a one-time cost, not a runtime requirement.
- CLI beta.12: `unity mcp` screenshots fall back to a desktop screenshot when the Editor does not respond in time. That fallback would capture whatever is on the owner's screen, so avoid the `unity mcp` capture path.

### 2.3 Input System and focus

- `InputSettings.backgroundBehavior` (default `ResetAndDisableNonBackgroundDevices`; others `ResetAndDisableAllDevices`, `IgnoreFocus`). Applies when run-in-background is on; in the Editor it is treated as always on. On focus loss devices that cannot run in the background are soft-reset and disabled; their bound actions are cancelled. https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Settings.html
- Separate Editor-only setting `editorInputBehaviorInPlayMode` ("Play Mode Input Behavior"): `Pointers And Keyboards Respect Game View Focus` (default; gamepads and other devices always reach the app), `All Devices Respect Game View Focus`, `All Device Input Always Goes To Game View` (Editor input disabled; EditorWindows cannot see device input). https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/api/UnityEngine.InputSystem.InputSettings.html
- `InputSystem.settings` can be changed at runtime and takes effect immediately (`onSettingsChange`). Whether a runtime change persists to the settings asset on disk is **UNSOURCED GUESS**; assume it can dirty the asset and that CLAUDE.md's guardrail on input settings applies.
- Simulating input: virtual devices via `InputSystem.AddDevice`; `QueueStateEvent`; `InputState.Change` applies state synchronously and fires change monitors; a simulated device can report `canRunInBackground` and sync support via `ExecuteCommand` so it is not reset/disabled on focus loss. https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Devices.html
- **Open conflict with CLAUDE.md rule 2:** the docs say gamepads *always reach the app* under the default Play Mode Input Behavior, yet CLAUDE.md says gamepad devices were disabled when unfocused. The matrix that would settle this is an image the fetch tool could not read. Needs a one-off empirical check.

### 2.4 Deterministic stepping and off-screen rendering

- `EditorApplication.Step()` = one frame step, same as the toolbar Step button; `isPaused` is true afterwards. https://docs.unity3d.com/ScriptReference/EditorApplication.Step.html , https://docs.unity3d.com/ScriptReference/EditorApplication-isPaused.html . Whether `Step()` advances synchronously when called from inside an eval, and whether it works while the Editor tick is throttled: **UNSOURCED GUESS** (the page has no details). Pair it with autotick if used.
- `Physics.Simulate(step)` / `PhysicsScene.Simulate(step)`: manual physics step, fixed step recommended for determinism; works outside Play Mode on a physics scene (contacts generated, no script callbacks). https://docs.unity3d.com/ScriptReference/Physics.Simulate.html , https://docs.unity3d.com/ScriptReference/PhysicsScene.Simulate.html . Note `Physics.Simulate` needs `simulationMode = Script` (changing that is a ProjectSettings/Physics change); `PhysicsScene.Simulate` on a separate physics scene does not (the project already uses this for the baked preview scene).
- `Time.captureFramerate` fixes delta time (reciprocal of `captureDeltaTime`). The page does not discuss stepping. https://docs.unity3d.com/ScriptReference/Time-captureFramerate.html
- URP off-screen render: `UniversalRenderPipeline.SingleCameraRequest` + `RenderPipeline.SupportsRenderRequest` + `RenderPipeline.SubmitRenderRequest(camera, request)` with `request.destination = renderTexture`. The official example's comment: "Render the camera output to the RenderTexture synchronously ... At this point, the RenderTexture contains the scene rendered from the point of view of the Camera". Base Cameras only. https://docs.unity.com/en-us/engine/6000.7/manual/cameras/urp/multiple/user-render-requests.md , https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Camera.SubmitRenderRequest.html
- `Camera.Render()` also renders on demand but cannot be called from a camera that is currently rendering; the manual example reads pixels via `RenderTexture.active` + `ReadPixels`. https://docs.unity3d.com/ScriptReference/Camera.Render.html . A forum thread reports `RenderSingleCamera` is obsolete in favour of SubmitRenderRequest and that SRP throws "Recursive rendering is not supported" if called inside render callbacks (community report, not official). https://discussions.unity.com/t/rendersinglecamera-is-obsolete-but-the-suggested-solution-has-error/898627
- Whether a SingleCameraRequest applies the Volume post-processing (tonemapping, vignette) so the image matches the Game view: **UNSOURCED GUESS** (docs silent). Verify once by comparing a render to a `capture_game_view source=screen` shot taken while focused.
- `ScreenCapture.CaptureScreenshot` captures "the final frame presented to the user" and the page says nothing about focus; it depends on presented frames, so it is the wrong tool for unfocused verification. https://docs.unity3d.com/ScriptReference/ScreenCapture.CaptureScreenshot.html

### 2.5 Which of this is officially documented

| Item | Official? |
|---|---|
| `set_autotick`, `wait_for frameCount changed`, `status frameCount/playerLoopTicking`, `capture_game_view source=camera` | Yes (Unity-Technologies/skills + Pipeline package docs) |
| Interaction Mode does not apply in Play Mode and has no unfocused option | Yes (manual) |
| Play Unfocused only affects focus at Play start | Yes (manual) |
| Input System `backgroundBehavior`, `editorInputBehaviorInPlayMode`, virtual devices | Yes |
| `SubmitRenderRequest` synchronous off-screen render | Yes (manual example comment) |
| Why exactly the Editor stalls unfocused | No (forum/issue-tracker snippets only) |
| `EditorApplication.Step` from eval, post-processing in render requests | No |

## 3. Numbers and rules of thumb

- Autotick: 16 ms default (about 60 Hz); 0 ms "pegs a CPU core"; default persists across domain reloads (Pipeline docs above).
- `wait_for` timeout 30 s default; use 10 s for a frame-advance proof (vendor loop).
- eval default timeout 5 s (ms units); fence flags with `--` (see file 21).
- Editor default idle 4 ms per tick (Preferences / EditorApplication.update docs).
- Input System docs: Editor treats run-in-background as always on.

## 4. Confidence and unknowns

- Confidence: **high** that `set_autotick` + a frame-advance proof is the vendor's recommended fix and that `source=camera` / `SubmitRenderRequest` render without needing the Game view painted; **medium** on the exact stall mechanism; **low** on gamepad behaviour and on post-processing inside render requests.
- Unknown: whether the project has been running `set_autotick` at all (CLAUDE.md does not mention it); whether `Step()` works from eval; whether runtime InputSettings edits persist.

## 5. Implication for BookBoy

1. **Currently wrong or missing:** CLAUDE.md section 0/2 tells the assistant to read `Application.isPlaying` + `Time.frameCount`, which is right, but never mentions `unity command set_autotick`, `wait_for` on `Time.frameCount`, or `unity status --format json` (`frameCount`, `playerLoopTicking`). These are the vendor's own solution to the exact freezing problem. Smallest change: run `unity command set_autotick` at the start of every session (persist default means once is enough) and replace "read frameCount from eval" with `wait_for ... frameCount changed` as the Play-mode proof.
2. **Never call `editor_focus`** or the `unity mcp` capture path (desktop-screenshot fallback). Both defeat the "do not disturb the owner" goal.
3. **Stop depending on Game-view captures for visual checks.** Use `capture_game_view -- --source camera` (explicit) or a small eval/`run_script` that does `SubmitRenderRequest` into a RenderTexture, then reads pixels. Do this once also in Edit Mode (0.8 supports it), so many visual checks need no Play Mode at all. Validate once that post-processing/tonemapping matches the focused Game view, since the luminance measurements in file 18 depend on it.
4. **Drive logic synchronously, not through input devices.** Call the game's own public entry points (pickup, place, throw) from the check script; step physics with `PhysicsScene.Simulate` on the existing baked preview scene (no ProjectSettings change). This avoids all keyboard/gamepad focus rules and keeps `backgroundBehavior` untouched.
5. **Leave `InputSettings.backgroundBehavior` and `editorInputBehaviorInPlayMode` alone** (rule 3 in CLAUDE.md is consistent with the docs: IgnoreFocus makes the Editor treat other-window typing as game input). If ever needed, `All Device Input Always Goes To Game View` is the documented lever, but it is an Input Settings change and needs explicit approval.
6. **Re-check CLAUDE.md rule 2's gamepad claim** against the Input System docs (default says gamepads always reach the app).
7. Interaction Mode and Play Unfocused/Focused: no action; they do not solve this.
