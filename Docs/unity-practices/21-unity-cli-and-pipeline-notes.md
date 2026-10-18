# 21. Unity CLI and `com.unity.pipeline` notes

Research date 2026-10-18. Project versions: Unity CLI 1.0.0-beta.13, `com.unity.pipeline` 0.8.0-exp.1, Editor 6000.6.6f1.
Web research only; no `unity` command was run. Items without a URL are **UNSOURCED GUESS**.

## 1. Question

What do the official Unity CLI docs, the Pipeline package docs/changelog and the Unity-Technologies/skills `unity-cli` and `unity-pipeline` skills say about: the command surface, `eval`/`eval_file` limits and timeouts, safe patterns for long operations, avoiding Editor hangs, and focus handling?

## 2. Findings

### 2.1 Where the real docs are (and what is thin)

- Landing/announcement: https://unity.com/blog/meet-the-unity-cli (published 2026-07-20; says experimental, Unity 6.0 LTS and newer). Walkthrough: https://unity.com/resources/unity-pipeline-cli-technical-walkthrough
- CLI docs: https://docs.unity.com/en-us/unity-cli/unity-cli , usage https://docs.unity.com/en-us/unity-cli/use-unity-cli , reference https://docs.unity.com/en-us/unity-cli/unity-cli-reference , release notes https://docs.unity.com/en-us/unity-cli/release-notes , MCP migration https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli
- The official docs pages themselves say almost nothing about `eval` timeouts, focus or background; the useful detail lives in (a) the skills repo, (b) the Pipeline package manual/changelog, (c) release notes. The CLI reference says `unity --help` is authoritative for flags on the installed version.
- Skills repo (vendor-maintained): https://github.com/Unity-Technologies/skills/tree/main/skills/unity-cli (SKILL.md plus `references/`: `build-run-test.md`, `integration-advanced.md`, `playmode-verification-loop.md`, `diagnostics-maintenance.md`, ...) and https://github.com/Unity-Technologies/skills/tree/main/skills/unity-pipeline (SKILL.md, SECURITY.md). Raw files: `https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/<skill>/SKILL.md` and `.../references/<file>.md`.
- Pipeline package manual: https://docs.unity3d.com/Packages/com.unity.pipeline@0.8/manual/index.html (index only was readable; sub-pages 404 under guessed names). Readable older pages: https://docs.unity3d.com/Packages/com.unity.pipeline@0.6/manual/commands/editor-lifecycle-and-observability.html , https://docs.unity3d.com/Packages/com.unity.pipeline@0.6/manual/commands/scripts.html , https://docs.unity3d.com/Packages/com.unity.pipeline@0.4/manual/creating-commands.html . Changelog: https://docs.unity3d.com/Packages/com.unity.pipeline@0.8/changelog/CHANGELOG.html
- The Unity CLI "skill" in the repo says: command names and parameters come from the connected Editor's package version, so confirm with `unity command` / `unity list`; `unity commands --format json` gives a machine-readable manifest (beta.10).

### 2.2 Command surface (as documented)

- Discovery: `unity command` (no args) lists command *tags* (beta.12); `unity command --tag <tag>`; `unity command --query <text>`; `unity commands --grep <pattern>`; `unity list` shows tools with parameter schemas. https://docs.unity.com/en-us/unity-cli/release-notes
- Editor-control commands named in vendor docs: `editor_status`, `editor_play`, `editor_stop`, `editor_focus`, `set_autotick`, `recompile` / `recompile_status`, `list_tests`, `run_tests` / `test_status` / `cancel_tests`, `wait_for` / `wait_status` / `wait_cancel`, `capture_game_view`, `capture_scene_view`, `console` (older name `get_console_logs`), `console_status`, `eval`, `eval_file`, `run_script`, `create_script`, `attach_script`, `audit` / `audit_status`, `reload_file` (method hot reload), runtime-only `log`, `set_timescale`, `runtime_status`. Sources: https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-pipeline/SKILL.md , https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/references/playmode-verification-loop.md
- Pipeline 0.8 manual index lists command areas: assets, scenes, GameObjects/components, prefabs, scripts, animation, materials/shaders, baking, navigation/selection, screenshots, builds/compilation/tests, project settings, Package Manager, Editor lifecycle/observability, runtime. https://docs.unity3d.com/Packages/com.unity.pipeline@0.8/manual/index.html
- Safety conventions in the manual: state-changing commands generally require `confirm`; `dry_run` gating; undo scopes; path sandbox. (Same index page.)
- Instance selection order: `--runtime`/`--runtime-path`, then `--project-path`, then the running Editor whose project contains the cwd (deepest match). Ambiguity fails with `AMBIGUOUS_EDITOR` (exit 6) listing candidates. beta.13 fixed `--project-path` matching sibling projects with a shared prefix (`/x/MyGame` vs `/x/MyGame-lighting`). https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/references/integration-advanced.md , release notes above.
- Env var `UNITY_PROJECT_PATH` is the default for `--project-path` (CLI reference).

### 2.3 `eval` / `eval_file` / `run_script`: limits and timeouts

- `eval "<C#>"` is for short ad-hoc code; it runs on the Editor main thread, compiled with Roslyn, no domain reload. `eval_file <path>` runs a `.cs` file kept **outside `Assets/`** or Unity compiles it as project source (suggested `AgentScripts/Scratch.cs`). The vendor skill says "put multi-line code in files. Reserve `eval` for one-liners." https://unity.com/blog/meet-the-unity-cli , https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-pipeline/SKILL.md
- **Default eval timeout is 5 s**, in **milliseconds**: `unity command eval -- --code '...' --timeout 15000`. The CLI's own `--timeout` is in **seconds** and is intercepted anywhere in the args, so Editor-side flags with the same name must come after a `--` fence. https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/references/playmode-verification-loop.md . The 0.4 manual shows `int timeout = 5000` on the eval handler and `MainThreadRequired` default true. https://docs.unity3d.com/Packages/com.unity.pipeline@0.4/manual/creating-commands.html
- Changelog: 0.5.0 eval timeout cap raised from 30 s to 24 h; a command's own timeout (not a fixed 60 s) governs how long `/api/exec` waits; 0.6.0 non-positive `timeout`/`timeout_ms` returns 400 instead of an opaque dispatcher timeout; eval/eval_file calls are logged to a local JSONL file (editor only, unless telemetry is off). CLI beta.4: long Editor commands no longer hard-fail at 60 s; `unity eval <expr> --detach` returns a job id. CLI beta.3: eval/eval_file compile errors now exit 6 with diagnostics (previously 0). Sources: Pipeline changelog and CLI release notes above.
- `unity command <name>` default CLI-side timeout is 30 s (`--timeout`); it retries briefly while the Editor is unavailable (e.g. right after `editor_play`) only when it can prove the command did not run; timeouts and dropped connections are never retried; persistent unavailability gives `EDITOR_NOT_READY` (exit 6). Typical warm command latency 200-600 ms. https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/references/integration-advanced.md
- `run_script` (preferred for bulk work): static builder class outside `Assets/`, `run_script --file <path> --entry Class.Method`, `timeout_ms` default 30000 ("main-thread wait budget end-to-end, like `eval`"), `--dry_run true` compile-only, `--args` JSON, `async Task`/`Task<T>` entry points are awaited, bounded by `timeout_ms`; on expiry it returns a `Timeout` error and "the task keeps running detached (its effects may still land)". Mono cannot unload the in-memory assembly, so prefer one builder entry over many tiny runs. https://docs.unity3d.com/Packages/com.unity.pipeline@0.6/manual/commands/scripts.html , unity-pipeline SKILL.md above.
- Gap: the docs do **not** say whether a timed-out *synchronous* eval is aborted. A Unity issue-tracker title ("a main-thread command that finishes after its timeout budget is reported as failed even though it succeeded", https://issuetracker.unity.com/issues/24771/comunitypipeline-a-main-thread-command-that-finishes-after-its-timeout-budget-is-reported-as-failed-even-though-it-succeeded , title only readable) implies the timeout is an *observer-side* budget, not preemption. **Inference, not stated:** a `while(true)` on the main thread cannot be killed by the timeout; the only escapes are a debugger trick or killing the Editor (Unity's own blog describes the debugger trick for 64-bit Windows Editor: https://unity.com/blog/engine-platform/how-to-stop-an-infinite-loop-in-a-unity-c-script ). This matches the project's earlier hang.

### 2.4 Long operations: async + poll

The vendor rule: "Never treat the trigger call's response as completion. Poll the matching status command."
- `recompile` -> poll `recompile_status` until `completed` or `up_to_date`; connection errors during domain reload are expected; if `failed=true` read `errors`. CLI `unity recompile` exits 0 ok / 6 compile errors / 7 no Editor responded (beta.11, beta.12).
- `run_tests --async_tests true` -> poll `test_status`, abort with `cancel_tests`. beta.13: no more misleading "0/0 passed" right after an async start. Known caveat: failing runs can return an opaque result; narrow `--filter` (`--filter_type testName|assembly|category`, `--mode all|editor|playmode`) or read Test Runner/logs.
- `wait_for` with `--async true` -> `wait_status` / `wait_cancel`.
- `audit` -> `audit_status` (terminal: completed, failed, unavailable, interrupted); only one scan at a time, no cancel.
- Generic: `unity command <name> --detach` returns a job id; `unity job status|wait|cancel`; `job wait --timeout` seconds (0 = forever), `--poll-interval` ms default 500. CLI-side `unity status --until-ready` waits up to 300 s by default; `--timeout` only valid with `--until-ready`; on timeout exits 6.
- Sources: unity-pipeline SKILL.md, https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/references/integration-advanced.md , release notes.

### 2.5 Avoiding Editor hangs

- Main-thread budget: handlers default to `MainThreadRequired = true`; only thread-safe, read-only, pollable handlers should run on a background thread "so they never block the main thread or deadlock against a busy editor." That is why `editor_status` and `console` still answer when the main thread is busy (0.5.0: "While the Editor is settling, background commands such as `console` and `editor_status` stay available"). Sources: 0.4 creating-commands page, changelog.
- Modal dialog: `editor_status` answers immediately even when blocked; `status: "blocked_by_dialog"` -> stop retrying and report `dialog.title/message/buttons`; the CLI cannot click it. CLI beta.13: `unity status` shows `STATUS_BLOCKED_BY_DIALOG` (with `blockedBy` in JSON). https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-pipeline/SKILL.md , release notes.
- Safe Mode (C# compile errors): the Pipeline package does not load, so the CLI cannot connect although the Editor is open. Confirm with `unity pipeline list`; look in `<project>/Logs/Editor.log` or `%USERPROFILE%\AppData\Local\Unity\Editor\Editor.log`; fix the C# on disk; restart. integration-advanced.md above.
- Sandboxed shells can hide a running Editor (restricted account cannot read the owner-only discovery file). Ask before assuming no Editor. Same file.
- Bounded-loop guidance: no vendor text addresses loops explicitly. The only documented defence is "keep eval to one-liners; put work in `run_script` with `timeout_ms`; use async entry points with polling for anything that spans frames". Writing loops with an iteration cap and a `Stopwatch` deadline is **UNSOURCED GUESS** best practice (but cheap).
- eval is "remote code execution" by design; gated by a per-project access key/security token; binds to localhost. https://unity.com/resources/unity-pipeline-cli-technical-walkthrough

### 2.6 Focus handling

- Pipeline `set_autotick` (see file 20) keeps the Editor ticking unfocused; run it first or "recompiles and tests can hang". Source: unity-pipeline SKILL.md; 0.6 manual page above.
- Installing the package into an open Editor: loads when the window next gains focus (beta.12). If `STATUS_PIPELINE_LOAD_PENDING` persists the skill says to ask the user to focus the Editor (only needed after package changes).
- `editor_focus` brings the window forward: avoid.
- `capture_game_view` default `source` changed to `screen` in 0.8.0-exp.1; screen capture needs a painted frame; `camera` source renders one camera off-screen.
- `unity run --command <cmd>` / `unity test` start a *batch-mode* Editor unless the project is already open, in which case `run --command` reuses the open Editor and leaves it running (`reusedRunningEditor` in JSON); `--log-file` applies only to an Editor the CLI started. https://raw.githubusercontent.com/Unity-Technologies/skills/main/skills/unity-cli/references/build-run-test.md . `unity test` itself "runs tests through the editor command line and does not involve the Pipeline package"; the page does not say what it does when the Editor already has the project open; Unity's Editor docs say batch mode cannot open a project the Editor has open (see file 19).

### 2.7 Exit codes and output

`--format json|ndjson` gives an envelope with `success`, `command`, `data`, `errors`, `warnings`; branch on `success`/exit code, not on `data`; failures still write a result to stdout; stderr is for human diagnostics. Exit: 0 ok, 1 general, 2 usage (never retry unchanged), 3 auth, 4 config, 6 primary operation failed / no Editor (not retryable unless "no instance"), 7 service unreachable (retry), 8 `unity test` ran and tests failed, 130/143 signals. https://docs.unity.com/en-us/unity-cli/unity-cli-reference , unity-cli SKILL.md.

## 3. Numbers and rules of thumb

| Value | Source |
|---|---|
| eval default timeout 5000 ms; cap 24 h since 0.5.0 | 0.4 manual, 0.5 changelog |
| `run_script` `timeout_ms` default 30000 | 0.6 scripts page |
| `unity command` default timeout 30 s (seconds) | integration-advanced.md |
| `status --until-ready` default 300 s | integration-advanced.md, release notes |
| `wait_for` default `timeout_s` 30 | playmode-verification-loop.md |
| warm command 200-600 ms | integration-advanced.md |
| autotick 16 ms, persists | 0.6 manual |
| Editor ports 7800-7849, runtime 7900-7949 | unity-pipeline SKILL.md |
| `unity test`/`run` timeout: SIGTERM then SIGKILL after 2 s, exit 6 | build-run-test.md |

## 4. Confidence and unknowns

- High: command names, defaults, async-poll rule, autotick, instance selection (vendor docs agree with each other).
- Medium: eval/loop behaviour (inference from changelog/issue title).
- Unknown: full 0.8 manual command pages (not retrievable); whether `editor_play` in 0.8 has `run_in_background` (vendor says check with `--query`); exact `eval` abort semantics; whether `wait_for` can express "condition on an arbitrary static member" beyond the shown JSON.
- Naming drift to expect: `get_console_logs` (0.6 manual) vs `console` (skill), MCP vs CLI names. Always list first.

## 5. Implication for BookBoy

1. The existing CLAUDE.md choices are **right**: drive the CLI directly, pin `--project-path`, never use `unity test` while the Editor is open, treat 0/0 test runs as no run, wait with `unity status --until-ready`. Keep them.
2. **Add** (smallest changes): (a) `set_autotick` at session start; (b) the 5 s eval default and the `-- --timeout <ms>` fence, so long-ish checks do not time out and a seconds/ms mix-up does not occur; (c) move anything beyond a one-liner (and all loops) into `eval_file`/`run_script` files outside `Assets/` with an explicit iteration cap and `Stopwatch` deadline; (d) for multi-frame checks, use `run_tests --async_tests true` + `test_status`, or `wait_for --async`, never a blocking loop in eval.
3. The unbounded `while` hang cannot be defended against by the Pipeline timeout (inference). Put a lint step in the ops helper: refuse to run an eval file containing `while (` / `for (;;)` without a deadline check.
4. Poll `editor_status` first whenever a command seems stuck; `blocked_by_dialog` explains most silent hangs and the fix is to tell the owner, not retry.
5. `capture_game_view` calls must pass `--source` explicitly (default changed in 0.8) and should prefer `camera` for no-focus checks (file 20).
6. Consider `unity run --command` only for one-shot headless commands when the Editor is closed; with the Editor open it reuses it (so it is not a second-Editor solution).
