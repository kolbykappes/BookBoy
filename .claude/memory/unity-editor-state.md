---
title: Unity Editor state — focus, Play Mode, modals, console (cold tier)
retrieval_hook: >
  Read this when: a command times out or hangs for no obvious reason; a
  script/scene/settings write reported success but the change doesn't seem
  to have happened; input isn't reaching the game; a console check is
  confusing; a branch switch on Windows seems to have triggered a recompile.
source: consolidated from Jim's unity-agent-bootstrap.md §3.A and
  unity-notes-for-a-new-unity-dev.md Part 1, both compiled 2026-10-16 from
  six months on CastleAssault (Unity 6, single-player sections only).
---

## When a status flag and the real state disagree

**When** a runtime "is playing" status reads `true` but nothing seems to be
happening, **do** read `Application.isPlaying` AND `Time.frameCount`
together, from inside the Editor process, **because** a wrapper's own status
flag was measured reporting `true` on processes whose game loop had not
advanced a single frame — downstream, that made a dependent component read
as `null` and a diagnostic install fail, while every status read insisted
"playing." `Time.frameCount` alone isn't a safe substitute either — a
stopped Editor can still report a non-zero frame count (seen: `frameCount 1`
on a dead process). Read both, cross-check them.

General version: any boolean that *describes* state can drift from the state
itself. When you have access to the state directly, read that instead of the
description of it.

## When the Editor window loses focus

**When** commands are timing out and the Editor process looks idle (near-zero
CPU over several seconds), **do** check whether the Editor window has focus
before assuming a hang, **because** script compilation and asset work are
deferred while the window is unfocused — measured at ~0.06s CPU over 4
seconds of elapsed time, with every command timing out at 30s, while clicking
into the window flushed the backlog instantly. This is not a deadlock and
restarting the Editor does not help.

**Separately:** an unfocused Editor receives **no keyboard or gamepad
input** — Unity's Input System defaults to
`BackgroundBehavior.ResetAndDisableNonBackgroundDevices`. The dangerous part:
`InputSystem.devices` still lists every device as `enabled=True`, so a "check
whether input is working" probe reports fine while the game is actually deaf.
**When** told "I press the key and nothing happens," **do** confirm window
focus before debugging anything downstream of input.

**Do NOT** set `backgroundBehavior = IgnoreFocus` to fix this. It was tried
on the source project and made things worse: the Editor then kept processing
input while unfocused and began interpreting chat-window typing as game
input. A typed letter is a ~50ms tap; against a 0.8s hold interaction, that
produced "hold began" with no "hold completed" logged — indistinguishable
from a genuinely broken hold mechanic. Keep the default setting.

Also: `InputSystem.QueueStateEvent` does not deliver synthetic input to an
Editor game view — two independent attempts at automated input injection
produced no device edge. To exercise an input-gated path without human
hands, call the method the input action itself calls, and say plainly that
the keyboard→action hop specifically was exercised by a human, not by you.

## When a write reports success but the change isn't showing up

**When** about to trust that a scene, prefab, or ProjectSettings write
"worked" because the API returned success, **do** verify the file on disk
rather than the in-memory property read-back, **because** this is the
highest-cost trap recorded: with the Editor in Play Mode, a write to a scene
component returned `Success: true`, reading the field back **confirmed the
new value**, and the scene file on disk still held the old value the entire
time. Stopping Play Mode reverted the change completely. Every signal short
of reading the file said it worked; the fix silently never shipped and the
eventual commit contained no scene change at all.

ProjectSettings are worse in a different way: they are **not written at all**
during Play Mode. Every API tried — explicit save calls, menu-equivalent
commands, `AssetDatabase.SaveAssets()`, `SaveAssetIfDirty`,
`ExecuteMenuItem("File/Save Project")` — reported success and left the files
byte-identical, with month-old mtimes. The cause was simply
`EditorApplication.isPlaying == true`.

**The one exception:** `AssetDatabase.SaveAssets()` on an asset genuinely
*does* persist during Play Mode. It's scene state and ProjectSettings
specifically that get silently restored on exit.

**Rule:** stop Play Mode before any scene/asset/settings write. After
saving, check the file on disk — never trust the property read-back alone.

## When the Editor goes completely silent

**When** every command times out and the Editor process is unresponsive,
**do** ask whether there's a modal dialog on screen before assuming a
deadlock, **because** a modal ("Save changes?", "unsaved changes will be
lost," "modified externally") blocks the Editor's main thread, and a blocked
main thread makes literally every command time out — including the command
that would have cancelled whatever raised the dialog. From outside, this is
indistinguishable from a genuine hang. The fix is dismissing the dialog, not
restarting the Editor. You cannot see the screen, so this has to be a
question you ask a human.

Discriminator table (same symptom class, different cause):

| Symptom | Cause | Fix |
|---|---|---|
| Near-zero CPU, clears the instant the window gets focus | deferred compile while unfocused | focus the window |
| Near-zero CPU, does NOT clear on focus | modal dialog | dismiss the dialog (ask the human) |

This only covers the *save-prompt* case. A "no signer for this asset import"
dialog or a "modified externally" prompt needs a human regardless.

**Prevention, not just diagnosis:** anything that's about to stop Play Mode,
switch scenes, or trigger a recompile should save dirty scenes first,
deliberately — enumerate open scenes, check each dirty flag, save any that
are dirty. That's what prevents the save-prompt modal from appearing in the
first place.

## Never edit scene/prefab/asset files on disk while the Editor has them open

**When** you need to change a `.unity`, `.prefab`, or `.asset` file and the
Editor currently has the project open, **do** go through the Editor's own
API (set the field → save the scene → recompile) rather than editing the
file directly, **because** the Editor detects external modification and
raises the "modified externally" modal — which is the main-thread-blocking
trap above, and kills every subsequent command. Worse: a save-dirty-scenes
step run afterward can write the Editor's **older in-memory copy back over
your disk edit**, silently discarding the change you just made.

**One safe exception, worth using deliberately:** a low-judgment
boolean/number field flip on a prefab or asset that has **no instance
currently in the open scene** is fine as a direct text edit — the Editor
holds no in-memory copy of that asset's contents to fight you with. Confirm
the "no instance in scene" half is actually true before relying on this.

## Console: persistence and deduplication

**When** a "show me the current errors" check comes back with anything,
**do** sample a cursor position before triggering whatever you're checking,
and read only entries after that cursor, **because** the console buffer
persists across sessions — a naive full-buffer read reports stale failures
from work done an hour (or a session) ago.

**Separately:** Unity deduplicates console entries. **When** a check reports
FAILED but lists no errors underneath it, **do** re-run the check exactly
once before concluding anything, **because** an error you have already fixed
can keep reading as "current" since the entry was deduped and never
re-logged — the stale entry disappears on a second run; a genuine ongoing
failure re-logs and reappears. A plain tail dump of the console carries no
sequence context either, so a text search over it can hand you a message
from well before the thing you actually just triggered.

## Don't run a second Unity instance in batch mode against an open project

They contend for the same project files. Just don't — no deeper trap here,
just a flat rule.

## Statics surviving across Play sessions

**When** static state looks wrong in a **fresh** Play session (not one you'd
expect carryover in), **do** check whether "Enter Play Mode Options" with
"skip domain reload" is enabled before suspecting the code, **because** with
that setting on, static fields are NOT reset when Play Mode stops and
restarts — they persist for as long as the Editor process itself lives, and
only an actual script recompile clears them. This is a fast-iteration
convenience setting and its effect is invisible until something static
accumulates.

Concrete defect this caused on the source project: a static registry holding
scene-object references kept **destroyed** entries across sessions, one new
stale entry per session. An index-based spread offset derived from registry
count drifted roughly 1.5m per accumulated stale entry — after four
sessions, five accumulated stale entries put a spawn 7.5m off target, with
no error anywhere in the chain.

**Rule:** anything static that caches scene object references must prune
itself on write. `OnDestroy`/teardown callbacks can fire *after* the object
is already destroyed, at which point the entry can no longer be matched
against anything live and removed — so pruning needs to happen proactively,
not reactively on a destroy callback alone.

## Windows: branch switches silently rewrite every file's mtime

**When** switching git branches on Windows in a folder the Editor has open,
**do** confirm no scene is dirty and Play Mode is off first, **because** with
`core.autocrlf=true`, `git checkout <branch>` rewrites every tracked text
file on disk even when the content is byte-identical — git re-applies
line-ending conversion on the way out. `git diff` comes back empty
afterward, so the switch *looks* like a no-op, but the file **mtimes**
change, and Unity watches mtimes, not content. Measured: a single branch
switch rewrote 86 files in one second, including the scene that was open in
the Editor at the time, which triggered an immediate recompile — and if that
scene happened to be dirty, that recompile raises exactly the blocking modal
described above.

**Do not describe a branch switch as "harmless to the Editor"** without
actually checking first — this was asserted once on the source project and
disproved by file mtimes within the same session.
