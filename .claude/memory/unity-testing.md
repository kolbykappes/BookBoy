---
title: Unity testing traps — asmdef, test runner, result parsing (cold tier)
retrieval_hook: >
  Read this when: setting up BookBoy's first test assembly for
  BookCategory/placement-validation logic; a test run's results seem
  inconsistent with what you just edited; writing any script that parses
  test-runner output; a test count looks like "0/0 passed."
source: consolidated from unity-agent-bootstrap.md §3.D and
  unity-notes-for-a-new-unity-dev.md Part 4 (CastleAssault, single-player
  sections; [MP] item dropped as not applicable).
---

## Keep game rules/math out of MonoBehaviours — the highest-leverage call available

**When** implementing BookBoy's book-category-matching and
placement-validation logic (the actual rules of "does this book belong in
this slot"), **do** put that logic in plain C# classes with zero scene or
MonoBehaviour dependency, covered by fast Edit Mode tests, **because** this
is flagged independently in both source documents as the single
highest-leverage structural decision available — it's what makes the core
game rules testable without a running Editor at all, and Edit Mode tests run
in seconds rather than requiring Play Mode's domain-reload overhead. Reserve
Play Mode tests specifically for what needs the engine actually running:
physics, coroutines, component lifecycle (first-person pickup detection,
shelf-slot collision, feedback timing).

## Custom assembly definitions don't inherit auto-referenced packages

**When** BookBoy's scripts get their own `.asmdef` (which they will, per
the rule above, the moment you want a separate test assembly), **do**
explicitly list every package your scripts reference, **because**
`autoReferenced` only applies to the predefined `Assembly-CSharp` assembly.
The failure mode when you forget this is a plain
`The type or namespace name 'X' does not exist` compile error — an ordinary-
looking compile error with **nothing in the message pointing at the asmdef**
as the actual cause, so it reads like a typo or missing using-statement
rather than an assembly-reference problem.

**Related and relevant here specifically:** a test assembly cannot reference
the predefined `Assembly-CSharp` assembly either. If you want tests against
BookBoy's actual game code (the category-matching logic above), that code
needs its own `.asmdef` from the start — which is the actual mechanical
reason for splitting it out, not just the testability argument.

## The test runner does not recompile your test sources first

**When** editing a test file and immediately running the suite, **do** force
a recompile and wait for it to settle before trusting the result, **because**
without that, the run executes against the **stale** compiled assembly — new
tests are simply missing from the run entirely, or an edit to an existing
test has zero effect and the exact same failure reproduces *after* you've
already fixed the underlying code. Nothing signals that this happened; the
reported counts look completely plausible either way.

**Rule, matching `.claude/skills/unity-editor-ops` Procedure 4:** force a
compile, poll until it settles, and **abort the test run** (don't report a
result) if the compile doesn't settle cleanly — rather than silently
reporting a result that isn't actually about the change you just made. Keep
a skip-compile flag as an explicit, deliberately-invoked escape hatch; never
make it the default path.

## The test runner's console output is not where the actual failure detail lives

**When** a test run reports anything other than all-green, **do** go
straight to the results XML rather than trying to diagnose from the console
summary, **because** the runner's own summary output (e.g. "3/4 passed")
gives **no indication of which test failed and no assertion text anywhere
in the console** — this was measured as the single biggest time sink in the
entire source-project toolchain, and the natural fallback (re-running tests
one at a time to bisect which one failed) is slow and was done twice on the
source project for a bug the XML stated outright on first read.

The runner announces its results file path in its own output; it lives
**outside the project directory**, under the OS's local-app-data folder for
the product. Standard NUnit3 XML — the useful query shape is:
```
//test-case[@result='Failed'] → fullname, .//failure/message, .//failure/stack-trace
```

**Guard specifically on the run's start time** when reading this file — it
gets overwritten in place on every run, so a stale file from a previous run
is structurally indistinguishable from this run's actual result unless you
check its timestamp against when you kicked the run off.

## Play Mode tests cannot run synchronously

Entering Play Mode triggers a domain reload that drops a synchronous
request outright. Run Play Mode tests asynchronously and poll a status
endpoint instead; a synchronous attempt should surface an explanatory error
rather than simply hanging with no indication of why.

**`0/0 passed` means the test filter matched nothing** — it is not a broken
filter report and it is absolutely not a pass. Treat it as its own distinct
outcome in any wrapper script, not folded into either the pass or fail
branch.

## Result parsers must fail closed, and must be validated against the real thing

**When** writing anything that parses test-runner output programmatically
(per `.claude/skills/unity-editor-ops` Procedure 4), **do** make unknown
result shapes return failure, never null and never a silent pass,
**because** the source project had two parsers fail in opposite, equally
costly directions: one matched the wrong JSON shape, fell through to an
"unrecognised" branch, and printed **TESTS PASSED** while discarding 64 real
test results unread; a second parser, built against a *different* valid
result shape, reported **TESTS FAILED against an actually-green 71/71 run**.

The root cause of the first one specifically: a leaked diagnostic string
ended up in a variable that became the function's return value, which
silently changed a null-comparison's outcome from "unknown, bail" to
"looks truthy, proceed as if passed." **Rule:** emit any diagnostic dumps
straight to console/log output — never into a variable that could become
or influence a return value.

**Validate any parser fix against the actual command's real output**, never
against a sample payload you constructed by hand to match your fix — the
source project's earlier "fix" to this exact bug survived code review
specifically because it was tested only against a hand-made sample that
happened to match the fix, not against what the real test runner actually
produces.

## BookBoy observations, 2026-10-17 (Unity 6000.6.6f1, Pipeline 0.8.0-exp.1)

- **Use `unity command run_tests` with the Editor open; `unity test` spawns a
  second batch-mode Editor** (violates the "no second instance" rule).
- `run_tests` returns `success: true` **even when tests fail**; the only
  truthful fields are `Summary.Total/Failed`. Per-test detail is in
  `Results[]` (`Status`, `Message`, `StackTrace`), so no XML hunt is needed
  for EditMode. A zero-test project also reports `success: true` (0/0).
- Right after a real recompile the domain reload makes the next command fail
  with a generic "Network error" - that is "not ready yet", not a test
  problem. Wait for `editor_status` = ready, then run.

## An unbounded loop in an eval script hangs the Editor for good (2026-10-18)

`unity command eval_file` runs on the Editor's main thread. A `while (cond) step();` whose condition never becomes true (here:
`while (carry.Current != book) carry.Cycle(1)` after a pickup silently failed because a Gather Family flight had disabled that
book's collider) spins the main thread at 100% CPU. The Pipeline just reports "Main thread operation timed out" for every call
after that, the window shows "Not Responding", and the only way out is to kill the Unity process and relaunch it
(`Unity.exe -projectPath C:\Code\BookBoy`). Play Mode state is lost, which is harmless; unsaved edits are not.
**Rule: never write `while` in a `tools/` script. Use `for (int guard = 0; guard < N && cond; guard++)`.** Also: a flight or
timer started by one verify script is still running when the next one starts in the same Play session; use a fresh session
(stop, focus, play) per suite.
