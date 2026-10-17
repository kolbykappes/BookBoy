# BookBoy

Unity 6.6.6f1, URP, Windows PC target. First-person cozy book-sorting prototype set in an arcane
library. Working scene: `Assets/Scenes/LibraryPrototype.unity`.

**Read [Docs/README.md](Docs/README.md) first** — it indexes the full project brief (vision,
MVP spec, tech stack, build status, implementation plan, agent workflow). Keep
[Docs/04-build-status.md](Docs/04-build-status.md) up to date as milestones land.

## Guardrails

- Do not modify `ProjectSettings/`, `Packages/manifest.json`, input bindings, tags, layers, or
  render-pipeline settings without explicit approval and a stated plan first.
- Leave `Assets/Starter Assets/` (imported package content) untouched; put project-specific
  content under `Assets/_Project/`.
- Work in small, single-purpose vertical slices (data → interaction → shelves → feedback, per
  [Docs/05-implementation-plan.md](Docs/05-implementation-plan.md)) — not one large generated
  system.
- No inventory grids, save/load, procedural generation, combat/fail-states, or other items on the
  non-goals list in [Docs/02-mvp-spec.md](Docs/02-mvp-spec.md) until the MVP loop is proven fun.

## Unity operating rules (hot tier)

Read every session. This file is the ~24-rule summary; the *why* behind each
line lives in `.claude/memory/unity-*.md` (cold tier), and the procedures that
enforce these rules live in `.claude/skills/unity-editor-ops/` (warm tier).
Consolidated 2026-10-17 from one engineer's six-month CastleAssault operating
notes (two documents, since merged and de-duplicated). Source project was
single-player Unity 6 — no FishNet/MPPM content carried over here.

### 0. How we drive the Editor — CORRECTED 2026-10-17

The source notes this file is built from assumed a community MCP bridge
(command names like `set_serialized_field`, `list_open_scenes`, `run_tests`,
`recompile --focus`). **That path is superseded.** Unity now ships an
official, non-deprecated path:

- **Unity CLI** (beta) + the **`com.unity.pipeline`** package, installed into
  the project. Drive it as `unity command <verb>` and
  `unity command eval --code '<C#>'` / `unity command eval_file <path>` from
  the shell. This is the supported path — Unity's own docs say it's faster
  and uses fewer tokens than going through MCP.
- The CLI *has* an MCP mode, but that's for harnesses that can't run shell
  commands. We can, so we drive the CLI directly instead.
- The old in-Editor MCP server — what the agent prompts in
  [Docs/06-agent-workflow.md](Docs/06-agent-workflow.md) used to reference
  (`Use Unity MCP to inspect...`) — is the deprecated path. Those prompts and
  [Docs/07-unity-mcp-tools.md](Docs/07-unity-mcp-tools.md) were updated/marked
  superseded 2026-10-17.
- **BookBoy status (2026-10-17):** Unity CLI 1.0.0-beta.13 (`unity` on PATH)
  and `com.unity.pipeline` 0.8.0-exp.1 (installed via `unity pipeline install`,
  not Package Manager) are in place. Smoke test passed against Editor
  6000.6.6f1. Always pass `--project-path C:/Code/BookBoy`. After a fresh
  Editor start or package change, wait with
  `unity status --until-ready --project-path C:/Code/BookBoy`.
- Command names below describe *capabilities*, not guaranteed exact syntax —
  Pipeline is beta and its surface shifts between versions. Run
  `unity command` (no args) against the live project to list the actual
  surface before trusting a specific verb. Fetch `Unity-Technologies/skills`
  (the `unity-cli` skill) for the vendor's own authoritative interface
  description — it supersedes this file on *interface* questions. This file
  stays authoritative on *traps*.
- First smoke test of any new session:
  `unity command eval "return Application.unityVersion;"`

### 1. The one rule that explains the rest

**A successful-looking return is not evidence.** Writes report success and
change nothing. Status flags say "playing" with zero frames advanced. Test
runners print PASSED while discarding real failures. Before trusting any
success return, number, or flag: ask what it would look like if it were
lying, and pay the one extra call that tells you.

### 2. Editor state

1. Never trust a descriptive flag — read `Application.isPlaying` AND
   `Time.frameCount` together from inside the process, not a wrapper's own
   "is playing" status. A stopped Editor can still report a non-zero frame
   count, so neither alone is proof.
2. Unfocused Editor window = deferred compilation AND disabled keyboard/
   gamepad input (`BackgroundBehavior.ResetAndDisableNonBackgroundDevices`),
   but `InputSystem.devices` still reports every device `enabled=true`. First
   question on "the key does nothing": was the window focused?
3. Never set `backgroundBehavior = IgnoreFocus` to fix #2 — tried, and it
   makes the Editor read chat-window typing as game input instead (a typed
   letter is a short tap; against a hold interaction it reads as "hold
   began, never completed").
4. **Nothing written during Play Mode persists except assets.** Scene edits
   and ProjectSettings changes report success, read back correctly, then
   revert the instant Play Mode stops. Stop Play Mode before any scene/
   settings write, and verify the **file on disk**, never the property
   read-back.
5. A modal dialog and a deadlock look identical from outside, and you can't
   see the screen. Save dirty scenes before anything that stops Play Mode,
   switches scenes, or recompiles — that's what prevents the modal that would
   otherwise wedge every subsequent command.
6. Never edit `.unity`/`.prefab`/`.asset` files on disk while the Editor has
   them open — it raises the "modified externally" modal (#5), and a
   save-dirty-scenes step can then write the Editor's older in-memory copy
   back over your edit. Go through the Editor API: set field → save scene →
   recompile. Exception: a low-judgment bool/number flip on an asset with no
   instance in the open scene is safe as a text edit.
7. Console persists across sessions and dedupes entries — a fixed error can
   keep reading as current because it was never re-logged. Sample a cursor
   before triggering a check; if a check says FAILED with no errors listed,
   re-run once before concluding it's stale.
8. Don't run a second Unity instance in batch mode against an open project.
9. If "skip domain reload" (Enter Play Mode Options) is enabled, statics
   survive across Play sessions and only a recompile clears them. Suspect
   this first when static state looks wrong in a *fresh* play session —
   anything static that caches scene objects must prune itself on write.
10. Windows + `core.autocrlf=true`: a branch switch rewrites every tracked
    file's mtime even when content is identical (`git diff` is empty, but
    Unity watches mtimes, not content). Confirm no dirty scene and Play Mode
    off before switching branches in an open project folder.

### 3. Prefabs and assets

11. A loaded prefab's bounds include the root's baked offset — they are not
    prefab-local. `LoadPrefabContents` keeps the root's serialized position.
    Zero the root before measuring.
12. `arraySize` can read correct with every element null after a prefab
    apply/push. Verify by counting non-null elements, never by size alone.
13. Applying to a prefab can leave the *instance* holding duplicate
    components (seen: instance ends up with ~2x the joints/colliders of the
    prefab). Check added-component count is zero on the instance after an
    apply. If something physics-related looks "asleep" and won't wake up,
    suspect duplicate constraints before suspecting sleep state.
14. Never rebuild a referenced asset by delete+copy — it mints a new GUID and
    silently nulls every reference, with nothing logged. Mutate in place;
    diff the `.meta` file for a changed `guid:` line after any scripted asset
    write. Verify by reading the reference back through the runtime API, not
    off the asset.
15. A `ScriptableObject`/`MonoBehaviour` class name must exactly match its
    `.cs` filename, or asset creation writes a null script reference that
    *looks* fine until the next reimport (`m_Script: {fileID: 0}`). Pinning
    the script afterward cannot rescue a mismatched filename.
16. For any question about whether an asset exists or is referenced, ask the
    engine (load it, print it) — a text/grep search misses GUIDs living in
    `.meta` files and will falsely report "never imported."

### 4. Testing

17. Keep game rules/math (book-category matching, placement validation) in
    plain C# with zero MonoBehaviour dependency, covered by Edit Mode tests.
    Highest-leverage structural decision available — reserve Play Mode tests
    for what genuinely needs the engine running.
18. A custom `.asmdef` does not inherit auto-referenced packages — list every
    package your scripts use explicitly, or get a plain "type/namespace does
    not exist" compile error with no pointer to the real cause (asmdef).
19. The test runner does not recompile stale test sources first. Force a
    recompile and wait for it to settle before every test run; abort rather
    than report a result that isn't about your actual change.
20. Test runners return counts only — the failure detail is in the results
    XML (standard NUnit3, path announced by the runner, lives outside the
    project). Read the XML; don't bisect by re-running tests one at a time.
    Guard on the run's start time — the file is overwritten in place.

### 5. Writing tools that drive the Editor

21. Pin the target Editor instance explicitly on every command and log which
    one you used. The CLI registers one endpoint per reachable Editor; an
    unpinned command can silently land on the wrong one and report success
    while the real project never changes.
22. Any tool that instantiates a preview/content rig into a scene must
    destroy it (immediate, not deferred) on every return path, including
    early exits. A deferred `Destroy` is not enough inside a single-frame
    script — the rig is still alive when the frame ends.
23. When a diff is absurdly large relative to what you intended, suspect your
    own tool's debris before believing the file got corrupted externally.
    Don't revert to "clean up" — you may be throwing away real work.
24. Result parsers fail closed: unknown shape → failure, never null/PASSED by
    default. Validate a parser against the real command output, not a
    hand-made sample.

### 6. Where things live

- Hot tier (this file): the ~24 rules above, read every session.
- Warm tier: `.claude/skills/unity-editor-ops/SKILL.md` — the verify loop,
  dirty-scene saver, test wrapper, and rig-teardown template that *enforce*
  the rules above. **Built 2026-10-17** (the source notes didn't include it),
  validated against the live Editor; helper: `unity-ops.sh preflight|save|compile|test`.
  It leans on native CLI commands (`recompile`, `run_tests`, `list_open_scenes`,
  `save_all`, `console_status`) rather than hand-rolling them; prefer the vendor's
  own solution. Cold-tier references to "Procedure 4" mean its compile+test procedure.
- **Tests with the Editor open: use `unity command run_tests`, never
  `unity test`** — the latter spawns a second batch-mode Editor on the same
  project (rule 8). `run_tests` returns `success:true` with 0 tests; treat
  0/0 as "no tests ran", not a pass (rule 19/24).
- Cold tier: `.claude/memory/unity-*.md` — the measured numbers, failed
  attempts, and full war stories behind each hot rule. Read on demand, when
  a rule above doesn't give you enough to act on.
- When you discover a new trap: fix it in a mechanism if you can (warm tier),
  write it down if you can't (cold tier), and only add a one-liner here if it
  must hold on literally every session. Annotate corrections in place with a
  date rather than silently deleting the old version — the wrong version is
  often the more instructive half.
