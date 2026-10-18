# 19. Unity Test Framework best practice and a headless/batch workflow next to an open Editor

Research date 2026-10-18. Unity 6000.6 (docs also cover 6000.0 - 6000.7), Test Framework 1.6.x. Web research only. Items without a URL are **UNSOURCED GUESS**.

## 1. Question

How should Edit Mode / Play Mode tests, asmdefs and `testables` be set up; how are tests run in batch mode; and since two Editors cannot open one project, what are the supported or at least well-evidenced ways to run headless verification while the developer keeps their Editor open (separate clone/worktree, Library copy, `-projectPath` to a copy, cloud build)? What do the Unity CLI/Pipeline docs say about running tests against a running Editor? What are the pros and cons of a second "agent" copy of the project?

## 2. Findings

### 2.1 Edit Mode vs Play Mode, asmdefs, testables

- Unity classifies a test as Edit Mode or Play Mode from its parent assembly's references. https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/test-framework-introduction/getting-started/edit-mode-vs-play-mode-tests
  - Edit Mode: runs only in the Editor, in the `EditorApplication.update` loop, no coroutines; assembly references `nunit.framework.dll` and has `"includePlatforms": ["Editor"]`.
  - Play Mode: runs in the Editor or a Player; `[UnityTest]` runs as coroutine; assembly has its own asmdef referencing NUnit and every assembly under test; **cannot reference `Assembly-CSharp.dll`** (move code under test into a custom assembly).
  - Recommendation: "Use NUnit's `[Test]` instead of `[UnityTest]`" unless you must yield to the Editor (Edit Mode) or skip frames/wait (Play Mode).
- Creating a test assembly: Assets > Create > Testing > Test Assembly Folder gives `Tests.asmdef` referencing `UnityEngine.TestRunner`, `UnityEditor.TestRunner` (Edit Mode only), NUnit, with `UNITY_INCLUDE_TESTS` define constraint. https://docs.unity.com/en-us/engine/6000.6/manual/programming-environment/test-framework-introduction/getting-started/workflow-create-test-assembly
- `testables`: other-package tests only run if the package name is listed in the project manifest's `testables`; embedded packages need no entry. https://docs.unity3d.com/Manual/cus-tests.html (result snippet; not relevant to BookBoy unless package tests are wanted).
- "No Engine References" asmdef option (`noEngineReferences`, default false) stops Unity adding UnityEngine/UnityEditor references, which is how an engine-free rules assembly is enforced by the compiler. https://docs.unity3d.com/Manual/class-AssemblyDefinitionImporter.html , https://docs.unity3d.com/6000.1/Documentation/Manual/assembly-definition-file-format.html
- Unity's own guide: use Edit Mode for Editor + game code, Play Mode for runtime behaviour; arrange-act-assert; keep test scenes minimal; reference game assemblies and packages (Input System) from the test asmdef. It does **not** recommend keeping logic out of MonoBehaviours (that is a community/architecture point, not in the guide). https://unity.com/how-to/automated-tests-unity-test-framework
- Custom asmdefs do not inherit auto-referenced packages (project rule 18) - consistent with Unity's asmdef docs; the test guide tells you to add Input System etc. explicitly.

### 2.2 Batch-mode / CI

- Official example: `Unity.exe -runTests -batchmode -projectPath <path> -testResults <xml> -testPlatform <EditMode|PlayMode|platform>`. Default platform EditMode; default results file in project root; NUnit XML. https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-command-line.html
- Other args (1.4 page): `-testFilter` (names/regex, `;` lists, `!` negation), `-testCategory`, `-assemblyNames`, `-orderedTestListFile`, `-runSynchronously` (EditMode only, single update, excludes multi-frame tests), `-randomOrderSeed`, `-retry`, `-repeat`, `-playerHeartbeatTimeout` (default 10 min), `-testSettingsFile`, `-forgetProjectPath`. "There is no standard exit code set across Unity components."
- Editor command-line page: `-batchmode` suppresses dialogs and exits 1 on exceptions; **"Only one Unity instance can run at a time, so you can't open a project in batch mode while the Editor has that project open"**; `-quit` with `-runTests` makes the Editor quit before in-progress tests finish (so do not pass `-quit`); `-nographics`: "Automated input only works when a window has focus", no GI baking; `-logFile -` sends the log to the console. https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html (the `Manual/editor-CommandLineArguments.html` path 404s; use this one)
- Unity CLI `unity test` wraps this: launches the built-in runner in batch mode, **omits `-quit` deliberately**, writes NUnit (default `test-results.xml`) or JUnit; exit 0 pass, 8 tests failed, 6 no verdict (compile error, license, crash, timeout); `--timeout`/`UNITY_TEST_TIMEOUT`: SIGTERM then SIGKILL after 2 s; `--mode`, `--filter`; `--affected` selects only reachable test assemblies via the `vcs affected` graph and falls back to the full suite when unsure. It "does not involve the Pipeline package or server". https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/references/build-run-test.md . The page is silent on what happens when the project is already open (so rely on the Editor doc above: it fails).
- Unity Build Automation (cloud) can run Edit Mode and/or Play Mode tests per build target and fail the build on test failure. https://docs.unity.com/en-us/build-automation/reference/unit-tests
- GameCI test-runner (community, widely used) runs in Docker, `testMode: All|EditMode|PlayMode`, and recommends caching the `Library` folder per project/platform ("could speed up your test runs by more than 50%"). https://game.ci/docs/github/test-runner/

### 2.3 Running tests while the developer's Editor is open

Official statements:
- Batch mode cannot open a project that is open in the Editor (above).
- Unity's lock is a file `Temp/UnityLockfile` (community statement, no staff reply): errors come from a shared or restored `Temp` folder, two runs on one path, or a CI cache that restored the whole project instead of only `Library`. https://discussions.unity.com/t/multiple-unity-instances-cannot-open-the-same-project-with-different-projects-on-build-machine/944045
- The `Temp` folder "gets cleared every time you close Unity"; `Library` is a local cache that "Unity regenerates ... next time you open the project"; deleting forces recompile and reimport "which can take a significant amount of time". Cloud-synced storage of a project is "unsupported". https://docs.unity.com/en-us/engine/6000.5/manual/get-started/project-configuration/default-directories.md
- Official route to run commands in the *open* Editor: Pipeline `run_tests` via `unity command` (the project's own rule), with `list_tests`, `--mode`, `--filter`, `--async_tests true` + `test_status` + `cancel_tests`. Also `unity run --command` reuses an already-open Editor. https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-pipeline/SKILL.md , build-run-test.md. These run **inside** the owner's Editor (so they are subject to focus throttling; see file 20 `set_autotick`).
- Official mechanism for several Editors on *one set of source assets*: **Multiplayer Play Mode** (MPPM) - up to four players (main + three virtual), "using the same source assets on disk", Unity 6+, Windows/Mac. https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@1.3/manual/index.html . Release notes say clones depend on **symlinked folders** and a **Library redirect** (clones are separate Editor processes with their own Library). https://mp-docs.dl.it.unity3d.com/mppm/1.1.0/release-notes/ , https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@1.3/changelog/CHANGELOG.html . MPPM is a multiplayer tool; whether a clone can be driven headless for tests is **not documented** (UNSOURCED). The CLI's instance resolver treats a project's MPPM clones as part of its main Editor (use the clone's folder with `--project-path` to target it). integration-advanced.md above.
- A plain second copy of the project is the standard CI pattern (separate project path per job). The minimal files needed to open a project are `Assets`, `Packages`, `ProjectSettings` (forum answer, community) and everything else regenerates. https://discussions.unity.com/t/proper-way-of-moving-project-to-other-machine-system/736056 . A `git worktree` contains only tracked files, so it needs a first full import (Library is untracked) - from general git behaviour, plus Unity's "Library ... exclude from version control" guidance (default-directories page). Pre-seeding the copy's `Library` from the original is an accepted CI trick (GameCI caches Library) but the Unity docs say not to edit it by hand and a stale/partial copy can corrupt; **do not copy `Temp`**.
- Unity CLI `--project-path` / `UNITY_PROJECT_PATH`, `-projectPath` and `-forgetProjectPath` (keeps the copy out of Hub history) let a second project path coexist. `unity run` manages `-batchmode`, `-quit`, `-projectPath` itself and rejects them if passed after `--`. build-run-test.md above.

### 2.4 Pros and cons of a second "agent" copy (own clone or worktree; own Library; batch Editor)

Pros (sourced or direct consequences of sourced facts):
- Satisfies "one Editor per project" rule; the owner's Editor, focus and Play Mode are never touched.
- `unity test` / `-runTests -batchmode` is the documented, deterministic CI path with an NUnit XML and exit code 8 for failures (no focus issues because the batch Editor is not an interactive window; note `-nographics` says automated input needs focus).
- Compile/test of the committed state is isolated from the owner's half-finished edits and from modal dialogs in the owner's Editor.
- Mirrors GameCI/Build Automation practice; trivially extendable to Play Mode tests.

Cons / risks:
- First open = full import and compile (cost "significant"; magnitude for BookBoy: **UNSOURCED GUESS**, likely minutes, not hours, for a small prototype); disk use roughly equal to the original Library.
- The copy only sees what is on disk/committed. Uncommitted work in the owner's Editor is invisible unless synced; sync (robocopy `Assets`, `Packages`, `ProjectSettings`) can race with the owner's saves. **UNSOURCED GUESS:** two Editors writing the same `Assets` through symlinks/junctions can produce duplicate or conflicting `.meta` files; a copy avoids this, a junction/MPPM-style share needs care (MPPM exists for exactly this and handles it).
- Pipeline `run_tests` and visual checks (Game view) cannot be done in a batch Editor with `-nographics`; Play Mode tests that render need a GPU. Visual/scene verification still needs a windowed Editor (the copy would have to run non-batch, i.e. a second visible Editor window, which is what the owner dislikes), or use off-screen render requests (file 20).
- CLI note: "A headless Editor can serve commands but may not appear in `unity status`; confirm with `unity command`/`unity list` using `--project-path`." So a second Editor *could* be driven through Pipeline too if started headless with the package installed. integration-advanced.md above.
- Licence/activation for a second Editor process on one machine: not researched (**UNSOURCED**; one machine with Personal licence normally just works but verify).
- Anything the agent builds in the copy (scene builders, assets) must be committed and pulled back; two diverging working copies need a merge discipline.

## 3. Numbers and rules of thumb

- Default `-testPlatform` EditMode; `-playerHeartbeatTimeout` default 10 min; `-quitTimeout` default 300 s (Editor args page); `unity test` kill: SIGTERM then SIGKILL after 2 s; exit 8 = tests failed, 6 = no verdict.
- GameCI: caching Library "could speed up ... by more than 50%".
- Pipeline `run_tests`: use `--async_tests true` for long runs.

## 4. Confidence and unknowns

- High: Edit/Play mode asmdef rules, batch-mode single-instance rule, `unity test` semantics, why `-quit` is omitted.
- Medium: lock-file mechanics (community), CI-copy practices.
- Unknown: whether `unity test` refuses cleanly or errors when the Editor has the project open; headless Pipeline driving of a second Editor; import time for BookBoy; MPPM clones for non-multiplayer automation; licence terms.

## 5. Implication for BookBoy

1. **Current setup is right for rules code:** `BookBoy.Rules` asmdef + ~109 Edit Mode tests, run through `unity command run_tests` in the owner's Editor. That is the supported "Editor already open" path. Consider setting `"noEngineReferences": true` on `BookBoy.Rules` if not already (the compiler then enforces "engine-free"; Unity docs above). A Rules test asmdef must be `includePlatforms: ["Editor"]` for Edit Mode; Play Mode tests would need their own asmdef that references engine-side assemblies (not `Assembly-CSharp`).
2. **Run `set_autotick` first** so `run_tests`/`recompile` do not stall while the Editor is unfocused (vendor skill, see file 21). That removes the main reason to want a second Editor for Edit Mode tests.
3. **A second agent copy is worth it only if** the owner's Editor must stay completely untouched (no recompile, no Play Mode entries) or CI-style verification of *committed* state is wanted. Cheapest version: `git worktree add ../BookBoy-agent` (or a plain clone), open it once with `unity run -- ...`/batch to build its Library, then `unity test --project-path <copy> --mode editmode --output results.xml` (exit 8 = failures). Do not share `Library`/`Temp` between copies; do not use symlinks/junctions into the owner's `Assets`.
4. **Keep Play Mode / visual verification in the owner's Editor** but without focus: `set_autotick`, `capture_game_view --source camera` or `SubmitRenderRequest` (file 20). A batch copy with `-nographics` cannot do rendering checks.
5. Do not add `-quit` to any `-runTests` command; use `-logFile -` (or `unity run --log-file`) and read the NUnit XML (project rule 20 is right).
6. Not worth doing: MPPM clones or Build Automation for this prototype.
