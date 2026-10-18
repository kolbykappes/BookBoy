# 13 - Enter Play Mode options, domain/scene reload, and static state

Researched 2026-10-18 from Unity's own manual and script reference (6000.0, 6000.3, 6000.6 pages) plus a Unity staff post. Nothing here was verified against the live project; no Unity commands were run.

## 1. Question

What do "Reload Domain" and "Reload Scene" do on entering Play mode, what do we gain by turning them off, what breaks (statics, event handlers, ScriptableObject runtime state), what is Unity's own guidance and recommended reset pattern, how do we detect stale state, and is it wise to keep both reloads OFF in a project that is going to grow statics?

## 2. Findings

### What the options are

- Setting lives in Edit > Project Settings > Editor > Enter Play Mode Settings > "When entering Play Mode". In the 6000.6 manual the dropdown has four values: Reload Domain and Scene; Reload Scene only (marked "the default option"); Reload Domain only; Do not reload Domain or Scene.
  Source: https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/code-reload-serialization/code-reload/code-reloading-editor/configurable-enter-play-mode.md
- In 6000.0/6000.3 the same page described it as: by default both reloads happen; you can disable either. So **the default changed in Unity 6.6**.
  Source (6000.3): https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/code-reloading-editor/configurable-enter-play-mode.md
- Unity staff, June 2026: "default setting for entering playmode is now scene reload only for all new projects", and "code will not be reloaded, and state, such as static variables, is not reset when entering playmode". Stated reason: with the move to CoreCLR, "traditional domain reloads go away entirely in Unity 7.0"; defaulting to off gives developers "a clear runway to handle static state cleanup". Unity used Project Auditor to find un-reset static fields/delegates in popular Asset Store packages.
  Source: https://discussions.unity.com/t/coreclr-scripting-and-serialization-update-june-2026/1723299
  - Disagreement on timing: this Unity post says Unity 7.0; a secondary summary of the same period claims 6.8 is the CoreCLR cutover and 6.7 is the last Mono release. Treat the exact version as unconfirmed. Either way, the direction (domain reload disappears) is Unity's stated plan.
- The 6000.6 manual says verbatim-in-spirit: "It's recommended to leave domain reload off to improve development iteration speed and prepare for Unity's adoption of the CoreCLR runtime, which doesn't have the domain reload concept." (same configurable-enter-play-mode page as above).

### What each reload does

- Entering Play mode stages: backup modified scenes, domain reload, scene reload, update scene (twice). Source: https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/code-reload-serialization/code-reload/code-reloading-editor/configurable-enter-play-mode-details.md
- Domain reload (6000.3 detail): stops the C# domain, calls OnDisable on all ScriptableObjects/MonoBehaviours, serializes their state, destroys the child domain (GC, finalizers, threads, JIT info deleted), loads assemblies, recreates objects (constructors run, statics get default values), deserializes. Source: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/code-reloading-editor/configurable-enter-play-mode-details.md
- Scene reload on: Unity destroys all scene GameObjects and reloads the scene from disk. Scene reload off: "Unity only resets the scene's modified contents", and still calls OnEnable/OnDisable/OnDestroy "as if the scene were freshly loaded". Source: https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/code-reload-serialization/code-reload/code-reloading-editor/scene-reloading.md
- With scene reload off, "the time it takes to start your application in the Editor is no longer representative of the startup time in the built version", so profile startup with scene reload on. Same source.
- With scene reload off, `[ExecuteInEditMode]`/`[ExecuteAlways]` scripts do not get OnDestroy or Awake calls. Same source.

### Speed benefit

- The only number Unity has published: the original 2019.3 blog said internal testing showed "roughly 50-90%" savings of Enter-Play wait time depending on project, with no absolute times (and called the feature experimental then). Source: https://unity.com/blog/cn/technology/enter-play-mode-faster-in-unity-2019-3 (Chinese-language page of the Unity blog; the summary I read was machine-extracted, so treat the 50-90% figure as medium confidence).
- Unity's manual says the cost "increases as your scripts and scenes become more complex" but gives no timing. (configurable-enter-play-mode pages above.)
- Domain reload cost scales with the amount of script code/assemblies; scene reload cost scales with scene size. UNSOURCED GUESS: for a ~2000-transform scene plus a modest codebase, scene reload may be the larger share now; measure it.

### What breaks when domain reload is off

All from https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/code-reload-serialization/code-reload/code-reloading-editor/domain-reloading.md :
- Non-serialized fields keep their Play-mode values when you return to Edit mode, for MonoBehaviours (including those on prefab assets), ScriptableObjects, and plain C# types.
- Static variables keep their values between Play sessions.
- Static events keep their subscribers between sessions (Unity's own example prints "Exiting Play mode!" several times after a few runs because the handler stacks up).
- Unity still does a domain reload on script change (asset database refresh) regardless of this setting. That means stale-state bugs can "come and go": a recompile silently clears them.
- Private fields: serialized by hot reload (script recompile) even without [SerializeField], but not in the build pipeline; so a private field may reset on exit in some situations and not others (scene-reload page, same note). Use [NonSerialized] to stop restore after recompile. Source: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/how-unity-uses.md
- Unity never restores statics after a hot reload; do not rely on them to survive recompiles. Same source.
- Unity converts null private/internal array and List fields to empty ones on domain reload (scene-reload page).
- ScriptableObject runtime mutation: in the Editor, modifying a ScriptableObject *asset* in Play mode modifies the project asset and is not reverted on exit (forum sources, not official docs; see below). With domain reload off, already-loaded SOs are not recreated, so OnEnable does not fire again and runtime values carry over. Sources (forum/blog, lower quality): https://discussions.unity.com/t/prevent-custom-scriptableobject-asset-persisting-changes-after-exiting-play-mode/545928 , https://bugnet.io/blog/fix-unity-scriptableobject-changes-not-persisting-after-domain-reload . The official page above does state non-serialized SO fields keep Play-mode values.

### Unity's recommended reset patterns

1. **Unity 6.6 (new, preferred):** `[AutoStaticsCleanup]` / `[NoAutoStaticsCleanup]` in `Unity.Scripting.LifecycleManagement`. Code generation resets static fields on each Play mode entry. The type must be `partial`. Resets: re-evaluates the field initializer (or C# default); `readonly` static collections keep their instance and get `Clear()`. Static constructors are NOT re-run. Guidance table: events/delegates, lists of user objects, and references to Unity objects (ScriptableObject, MonoBehaviour) -> `[AutoStaticsCleanup]`; ID counters and immutable cached UI resources -> `[NoAutoStaticsCleanup]`. Optional analyzer (off by default; warnings `UAL0010`-`UAL0014`) is enabled per-assembly through a `<asmdef name>.globalconfig` file next to the .asmdef with `build_property.UnityEnableAutoStaticsCleanupAnalysis = true`. Code generation is on by default. Sources: domain-reloading page above and https://docs.unity.com/en-us/engine/6000.6/script-reference/unity/scripting/lifecyclemanagement/autostaticscleanupattribute.md
2. **Manual, 6.6:** `[OnEnteringPlayMode]` / `[OnExitingPlayMode]` (namespace UnityEngine, static void no-arg methods, work in Player too, no `#if UNITY_EDITOR` needed). Unity says resetting on exit is "often most efficient", but if code also runs in Edit mode, reset on entry instead. `[OnExitingPlayMode]` fires on stop in Editor, before code reload in Play mode, and before application quit in Player (with the same platform limits as OnApplicationQuit). Sources: domain-reloading page; https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/onexitingplaymodeattribute.md ; lifecycle table: https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/code-reload-serialization/code-reload/code-lifecycle.md (which says the new lifecycle APIs are "guaranteed to work with any future scripting runtimes" and recommends adopting them in new code).
3. **Older, works in 6000.0-6000.6:** `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` static method that resets statics and unsubscribes handlers. SubsystemRegistration runs early ("Called before the first scene is loaded"); order inside one load type is not guaranteed. Sources: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/code-reloading-editor/domain-reloading.md ; https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/runtimeinitializeonloadmethodattribute.md
   - Doc bug to avoid: Unity's 6000.3 sample calls `EditorApplication.playModeStateChanged -= ...` inside the runtime Init method without `#if UNITY_EDITOR`; that will not compile in a Player build. Guard it.
4. **Editor-only code:** `[InitializeOnLoad]` + `EditorApplication.playModeStateChanged`, or `[InitializeOnEnterPlayMode]` (6000.3 page above).
- These attributes work on any class kind including ScriptableObject and plain C# classes (6000.3 domain-reloading page).
- Unity: "Best practice is to always unregister static event handlers on exiting Play mode. Waiting until the next Play mode session can cause object reference issues that lead to unregistering the wrong handler."

### Detecting stale state

- Unity's docs give no detection tool beyond the optional `AutoStaticsCleanup` analyzer (6.6), which warns about static members that lack an attribute (UAL0013). That is the sourced, built-in way to find statics.
- Run the analyzer's rule set once to enumerate statics: sourced (6.6 page).
- UNSOURCED GUESS: periodically do a full "Reload Domain and Scene" Play; if behaviour differs from the fast path, you have leaked state. Another trick: a script recompile forces a domain reload, so a bug that vanishes after editing a script is a stale-static signature (this follows from the sourced fact that script changes still trigger domain reload).
- UNSOURCED GUESS: grep the codebase for `static` (non-const, non-readonly-primitive) and `event` / `static Action` declarations as a cheap audit.

## 3. Numbers and rules of thumb

- Reload-off saves "roughly 50-90%" of Enter-Play wait (Unity 2019.3 blog, internal tests, project-dependent). https://unity.com/blog/cn/technology/enter-play-mode-faster-in-unity-2019-3
- Unity says scripts changes always cost a domain reload regardless. docs.unity.com 6000.6 domain-reloading page.
- Rule (Unity): every `static` field and every `static event` in runtime code needs an explicit reset when domain reload is off.
- Rule (Unity): handlers on static events must be unregistered on exit.
- Rule of thumb for what to attribute (Unity's table): events/delegates, collections of user objects, Unity object references -> reset each Play; ID counters, immutable caches -> keep.

## 4. Confidence

- High: what the options do, what persists, the reset attributes and their behaviour in 6.6 (read straight from the 6000.6 pages).
- Medium: 50-90% savings figure (old, machine-translated page), Unity 7.0 vs 6.8 CoreCLR timing.
- Low/unknown: how much time BookBoy actually saves (not measured; no source can say); whether third-party packages in the project (Input System, Starter Assets, Cinemachine etc.) leak statics with domain reload off - Unity says it audited popular Asset Store packages but I found no list.
- Unknown: whether the project's Unity 6000.6.6f1 build shows the default dropdown exactly as the 6000.6 manual describes (the manual is the only source).

## 5. Implication for BookBoy

Project facts: both Domain Reload and Scene Reload are currently disabled ("Do not reload Domain or Scene"). Unity 6000.6.

- **Domain reload OFF is right, not wrong.** In 6.6 it is Unity's own default and recommendation, and Unity is removing domain reload entirely. Keep it off. Do not turn it back on as a fix for stale-state bugs; fix the bugs.
- **Scene reload OFF is the non-default, riskier half.** Unity's default (6.6) is "Reload Scene only". Scene reload off means: the scene is only "reset" for modified contents, you are not testing true startup, and any scene-resident state (rooms 2 and 3 SetActive(false) until a door opens, baked pile positions, door/state flags on MonoBehaviours with non-serialized fields) can leak between runs if it is set in code and not restored by the scene reset. The CLAUDE.md rule 4 already notes Play-mode writes revert; this is the other direction (non-serialized runtime fields on scene objects survive in the object model). **Smallest change that is safe:** switch to "Reload Scene only" (Unity's default) if you see any "works the second time I press Play" or room-state-leak symptoms, and always use it when measuring load time or testing room loading. Cost: scene load time on every Play (scene is ~2000 transforms; probably a few seconds, unmeasured).
- **Statics policy for growth:** (a) No new `static` mutable state without an attribute. (b) Prefer `[AutoStaticsCleanup]` (needs `partial` classes and an asmdef; this project already uses asmdefs for tests) or `[OnExitingPlayMode]`/`[OnEnteringPlayMode]`; fall back to `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` with `#if UNITY_EDITOR` guards if code needs to compile on older Unity. (c) Turn on the autostatics analyzer for the game asmdef (`<name>.globalconfig`, `UnityEnableAutoStaticsCleanupAnalysis = true`) so new statics fail loudly. This does not touch ProjectSettings and is a small file addition; still, CLAUDE.md says to present a plan before build-system changes, so propose it first.
- **Event handlers:** any `static event Action` (for example item-picked-up, room-opened, score changed) must be `[AutoStaticsCleanup]` or cleared in an exit hook; instance handlers must unsubscribe in OnDisable.
- **ScriptableObject item definitions:** treat as read-only at runtime. Do not write counts, "found" flags or positions into BookDefinition assets: with domain reload off those values leak between Play sessions, and in the Editor they write through to the asset. Keep mutable state in plain C# runtime objects (see file 11). The existing CLAUDE.md rule 9 ("don't add static state without handling it") agrees with Unity's guidance; this doc makes it concrete.
- CLAUDE.md mentions both reloads are ON ("`DisableDomainReload` + `DisableSceneReload`" = reloads disabled). Wording: reloads are *disabled*. Not a contradiction with sources, just be careful which "ON" means.
