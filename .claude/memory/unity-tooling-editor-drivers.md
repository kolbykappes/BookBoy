---
title: Unity editor-driving tooling traps — eval semantics, targeting, teardown (cold tier)
retrieval_hook: >
  Read this when: writing any script/wrapper that calls the Unity CLI; C#
  state seems to vanish between calls that should share it; more than one
  Unity Editor instance might be reachable; writing a tool that instantiates
  anything into a scene; writing a forwarding wrapper around CLI flags;
  debugging a targeting/measurement probe that computes rather than fires.
source: consolidated from unity-agent-bootstrap.md §3.E and
  unity-notes-for-a-new-unity-dev.md Part 5 (CastleAssault, single-player
  sections; [MP]-tagged item dropped).
---

## Terminology note — verify against the live CLI before trusting command names below

Both source documents describe this distinction using a community bridge's
terminology: **`eval`** (a snippet, no domain reload, in-memory state
survives) vs. **`run_script`** (a whole file, reloads assets from disk every
call, fresh in-memory assembly each time). The *official* Unity CLI's
command surface uses `unity command eval --code '<C#>'` and
`unity command eval_file <path>`. **Whether `eval_file` has the same
domain-reload/fresh-assembly behavior as the community bridge's
`run_script` is not verified against the official CLI as of this writing —
confirm it live** (`unity command eval_file --help`, or test the specific
persistence behavior below directly) before assuming the mapping is exact.
The *underlying trap* — two code-execution entry points with different state
semantics — is worth assuming exists until disproven either way, because
it's a reasonable design for exactly the reasons below.

## The eval-vs-file distinction eats state if you pick wrong

**When** two separate editor-driving calls need to share state (a variable,
a cached reference, a delegate hookup), **do** share it through the scene
rather than through in-memory process state, **because** whichever entry
point reloads assets/recompiles into a fresh assembly on every call will not
preserve:
- an in-memory asset field patched by reflection (it snaps back to its
  originally-authored value on the next such call);
- delegate handlers installed by a previous call (they disappear);
- any static field (a fresh in-memory assembly means statics do not survive
  to the next call).

The symptom shape this produces is specifically nasty: a probe that *proves*
a hook fired correctly in one call, then reports the effect as never having
happened in a later call — because the hook itself got wiped between calls,
not because it failed to install.

**Concrete example of the right fix:** a cleanup routine that walked an
in-memory list a previous call had populated always found that list empty
on the next call. Scanning the scene directly instead — rather than relying
on a list surviving across calls — worked, because the scene is the one
thing genuinely shared between separate editor-driving invocations
regardless of which entry point reloads what.

**Practical rule:** use whichever entry point does NOT reload/recompile for
anything that must survive across calls; use the reloading entry point only
for genuinely self-contained one-shot work.

## `eval`-style calls typically need statement form, not expression form

A bare expression like `1+1` commonly fails to compile in a C# eval context;
`return 1+1;` works. Beyond being the fastest way to probe live Editor
state, this is also the fastest way to *reflect an API rather than guess
member names* — e.g. discovering a renamed importer property by querying
the live object's actual members rather than guessing at what it might be
called post-upgrade.

## `--args`-style parameters are often JSON arrays mapped 1:1 to method parameters

If a command-line wrapper takes a method's parameters via a `--args` flag
expecting a JSON array, a method parameter that is *itself* an array needs
an extra level of nesting: a `string[]` parameter needs
`--args '[["a","b"]]'`, not `--args '["a","b"]'` — the latter gets parsed as
two separate top-level arguments rather than one array argument, producing
an "expects 1 argument(s) but 2 were provided" error that doesn't obviously
point at the nesting issue. Verify the actual `unity command eval`/custom-
command argument-passing convention against the live CLI rather than
assuming this exact shape, but expect *some* version of this trap if a
method parameter is itself a collection.

## Keep throwaway editor tooling outside `Assets/`

**When** writing one-off editor scripts/wrappers for BookBoy tooling, **do**
put them in a `tools/` folder outside `Assets/` rather than inside the
compiled project, **because** scripts outside `Assets/` are not compiled
into the project and cost no domain reload — exactly what you want for
throwaway automation that doesn't need to ship with or be part of the game
itself.

## A tool that instantiates into a scene must tear itself down, structurally

**When** writing any tool that spawns a preview/content rig into a scene
(e.g. instantiating a book prefab to measure or screenshot it), **do**
structure teardown as unconditional (a `finally` block or equivalent, firing
on every return path including early exits and exceptions) rather than a
line of cleanup code you remember to add at the end, **because** a content
tool on the source project built a preview rig and never tore it down — the
next save-dirty-scenes step baked **two full copies of that rig directly
into the scene file**, producing a diff of **26,023 insertions against 783
deletions** for a session whose only *intended* scene change was deleting
one unrelated object. An expanded character prefab is thousands of lines on
its own, so the debris genuinely does not look like debris on casual diff
inspection.

Also specifically: use the immediate-destroy call, not a deferred one — a
deferred `Destroy()` inside a single-frame editor script means the rig is
**still alive when the frame ends**, which is exactly when a same-frame
scene-save step would capture it.

**Corollary, worth internalizing as its own rule:** when a diff from your
own tooling run is absurdly large relative to what you actually intended to
change, look for your own tool's debris as the first hypothesis — before
concluding the file got externally corrupted. The natural instinct (revert
the scene to "clean up") throws away any real, intended work mixed in with
the debris.

## Never name a wrapper parameter after a CLI flag it's forwarding

**When** writing a shell/PowerShell wrapper that forwards arguments to
`unity command`, **do** check any new wrapper parameter name against the
CLI's own flag list before adding it, **because** in PowerShell (and other
shells that merge single- and double-dash argument forms), a wrapper's own
parameter and the underlying CLI's flag can bind to the **same token**. A
wrapper `-Target` parameter on the source project silently swallowed the
real CLI's own `--target`/platform-selection flag — and the resulting
failure surfaced as *"matched 0 editors,"* a message about editor
*selection*, for a command that had nothing conceptually to do with editor
selection. The failure is displaced into a completely unrelated subsystem,
so nothing in the error message itself points back at the parameter-binding
collision as the actual cause.

## Pin the target Editor explicitly, every time, and log which one you used

**When** more than one Unity Editor instance might be reachable from the
CLI, **do** pin the target explicitly on every single command and log which
target was used, **because** the CLI registers one endpoint per reachable
Editor, and an unpinned command either errors outright or — worse —
**silently lands on the wrong instance**. A recompile request that lands on
the wrong Editor instance reports success while the actual project you care
about never changes at all, and a subsequent test run then reports results
against a stale, unrelated assembly. This is explicitly called out across
both source documents as a **silent wrong answer**, which both documents
rank as strictly worse than an outright failure — a failure at least tells
you something went wrong.

**Sub-detail worth knowing:** an exact-match target flag and a
substring-match target flag can behave differently when two Editor
instances' project paths nest inside one another (e.g. a project checked
out inside another project's folder) — exact-match disambiguates correctly;
substring-match can match both and leave you back at the silent-wrong-target
problem. Check which matching behavior the live CLI's targeting flag
actually uses.

## Make measurements fire the real path, not compute a predicted one

**When** writing any diagnostic/targeting probe, **do** have it trigger the
actual code path and observe the real result, rather than computing where
the result "ought to" land from known inputs, **because** two separate
probe implementations on the source project each computed a predicted
result and each read a fault that wasn't actually there:
- one measured against a target body's *center* while the actual game code
  aimed at the body's *surface* — a persistent, consistent-looking offset
  that was purely an artifact of the probe's own assumption, not a real bug;
- one swept from a position immediately after a teleport, before the
  physics engine's broadphase had actually registered the moved collider —
  so it swept an empty region of space and reported "missed" against a
  target that was, in engine, sitting dead ahead.

**When a symptom could plausibly have two separate causes** (the source
project's example: a targeting error that could be either a bearing problem
or a height problem), **split the measurement so the two possibilities
can't be confused with each other** — isolate and test for each
independently. That's specifically how the eventual real fix turned out to
be one distinct cause for the bearing component and a completely different
cause for the height component, and how a third, initially-suspected cause
was conclusively ruled out rather than left as an open question.

## Diagnostic output needs the identity of whatever produced it

**When** more than one source could plausibly be writing to the same log
stream (not currently a BookBoy concern with a single local Editor, but
worth keeping if tooling ever runs multiple parallel probes or processes
against the project), **do** put an explicit identity tag on every single
diagnostic line, **because** output from multiple interleaved sources reads
as one very confused single actor otherwise — a log line proves that
*something* ran, never conclusively *which* thing, and never that the
specific source you intended to target is actually the one that produced
that particular line. Match cross-source lines by an explicit sequence
marker if you need to correlate them, never by positional assumption about
ordering.
