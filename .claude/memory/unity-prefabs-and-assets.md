---
title: Unity prefab and asset silent-corruption traps (cold tier)
retrieval_hook: >
  Read this when: working on BookBoy's book prefabs, shelf slot prefabs, or
  the BookDefinition ScriptableObject; after any prefab apply/push; before
  rebuilding or regenerating any referenced asset; when a scene reference
  reads null in a way that seems inconsistent between Editor sessions; when
  a grep/text search says an asset isn't referenced anywhere.
source: consolidated from unity-agent-bootstrap.md §3.B and
  unity-notes-for-a-new-unity-dev.md Part 2 (CastleAssault, single-player
  sections only — [MP] FishNet-specific items dropped as not applicable).
---

Prefab surgery produces more "looks correct, is wrong" outcomes than any
other area of Unity work. Treat everything in this file as higher-suspicion
territory than ordinary scripting.

## Prefab bounds are not prefab-local

**When** measuring a loaded prefab's bounds or a renderer's extent for
grounding/sizing purposes, **do** zero the root's position before measuring,
**because** `PrefabUtility.LoadPrefabContents(path)` returns the prefab with
its root's **serialized position still intact**. If that root was baked at a
non-zero offset — the source project's example was `(0.634, 0.444, 0.634)` —
every `Renderer.bounds` you read afterward is shifted by exactly that
offset.

Concrete cost: a grounding check "verified" a body at `bodyMinY = 0.000`
while the root actually sat at `y = 0.444`. The body landed 0.444 *below*
the root's own origin, so every object placed at `y = 0` using that
measurement sank through the floor. This was caught visually, not
numerically — the numbers all looked clean.

Two wrong fixes preceded the right one on the source project, both from
reasoning about the symptom ("floating" or "sinking") rather than actually
measuring the reference frame — one of the wrong fixes made the gap *worse*
(0.419 → 0.569m). The original human bug report was accurate the whole
time; it was the agent's explanation attached to it that was wrong twice in
a row.

**General rule this generalizes to:** when a spatial measurement is off,
suspect the reference frame (prefab-local vs. world, baked offset, bind pose
vs. animated) before suspecting the geometry itself — and measure directly
rather than reasoning from the symptom.

## `arraySize` can be correct while every element is null

**When** verifying an array/reference field after a prefab apply or
save/push round-trip, **do** count non-null elements specifically — never
trust `arraySize` alone, **because** after pushing 32 added components from
a scene instance up to its prefab, the scene instance's array field read
`arraySize = 11` with **all 11 object references null**. A naive size check
passes cleanly while the feature backed by that array silently never runs —
no error, no warning, just an array of nothing pretending to be an array of
11 things.

For a reference specifically, also check `ref.transform.IsChildOf(root)` —
a non-null reference that points outside the prefab's own hierarchy is its
own, quieter version of the same failure shape.

## Applying to a prefab can leave the instance holding duplicates

**When** you push changes from a scene instance to its prefab asset, **do**
check the added-component count on the instance afterward (should be zero),
**because** the instance can be left holding its own **second copy** of
whatever was just pushed. Concrete numbers from the source project: an
instance ended up with `joints=20, colliders=23` where the prefab itself
had only `10/12`.

This is expensive specifically because the symptom misdirects completely.
On the source project it manifested as a ragdoll that wouldn't collapse —
every physics-sleep signal pointed at sleep state (`IsSleeping()` true,
`maxVel=0`, `WakeUp()` not sticking) when the real cause was an entirely
separate, duplicate set of joint constraints pinning the assembly rigid.
Deleting all joints on the instance made the hips fall immediately, which is
what actually identified the real cause.

**Diagnostic sequence worth reusing whenever a symptom "should" be caused by
X but X-targeted fixes don't move it:**
1. Build a fresh, minimal version of the suspect subsystem elsewhere in the
   same scene (here: a fresh rigidbody). Does it behave correctly in
   isolation? → if yes, the subsystem itself is fine globally.
2. Build the fresh minimal version in the *actual* hierarchy context (here:
   parented under the same bone). Still fine? → not a hierarchy problem.
3. Only the original object is wrong → the problem is specific to that
   object's own state, not the subsystem or the hierarchy. (Here: the
   duplicate constraint set, not the body.)

## Never rebuild a referenced asset by delete-then-copy

**When** you need to regenerate/rebuild an asset that other assets reference
(an AnimatorController, a material, anything with an inbound reference),
**do** mutate the existing asset in place rather than deleting and
re-copying, **because** `DeleteAsset(target); CopyAsset(source, target)`
**mints a brand-new GUID** for the target file. Every existing reference to
the old GUID silently loses its target — not an error, just a null
reference going forward.

Concrete cost: an AnimatorController rebuilt this way caused the character
prefab's Animator reference to go null (`runtimeAnimatorController == null`).
The character stood in its bind pose forever. **Nothing logged anywhere.**

**Rule:** copy once (only when genuinely missing), then mutate the existing
asset in place from then on. After any scripted asset write, diff the
`.meta` file specifically — a changed `guid:` line is the tell, and
critically, the asset's own diff will look perfectly clean and
unremarkable; the damage is entirely in the `.meta` file. After touching a
referenced asset, verify by reading the reference back through the
**runtime** API (e.g. `animator.runtimeAnimatorController != null`), never
by inspecting the asset's own serialized text.

## For BookBoy's BookDefinition ScriptableObject specifically: class/file name match

**When** creating `BookDefinition` (or any `ScriptableObject`/
`MonoBehaviour`), **do** confirm the C# class name matches the `.cs`
filename **exactly**, **because** when it doesn't, a scripted
`CreateAsset` call writes a null script reference (`m_Script: {fileID: 0}`)
to the resulting asset file.

The cruel part is the staging of the failure: the asset still *loads*
successfully at first via a class-identifier fallback, so everything looks
fine and `LoadAssetAtPath<T>` succeeds right after creation. It dies the
first time the asset is reimported — which an ordinary recompile triggers,
disconnected in time from whatever you were doing when you created it.

The most time-wasting symptom shape this produces: **a scene reference reads
null in one open Editor instance and fine in another**, because whichever
Editor instance happened to import the asset while the script reference was
still valid keeps its own cached good copy, while a fresh import in another
instance gets the null version. This reads exactly like Editor flakiness and
isn't.

Pinning the MonoScript reference manually afterward cannot rescue a
filename/classname mismatch — the fix is renaming one to match the other and
letting it reimport clean.

## Reading values: go through the engine, not the serialized text

**When** a serialized field's YAML looks wrong or corrupted, **do** verify
through the engine API before concluding anything's actually broken,
**because** this pattern burns time in two distinct shapes:

1. A byte-backed enum array serializes as compact hex with a trailing
   letter — e.g. `_weaponTypes: 05y`. This looks exactly like corruption on
   sight. It is Unity's own compact serialization form, not damage — it was
   hand-"fixed" twice on the source project before someone recognized the
   format and stopped touching it.
2. **A grep/text-search miss is not evidence an asset is missing or
   unreferenced.** Asset GUIDs live inside `.meta` files, and a plain content
   search does not see them. A GUID copied directly out of a `.controller`
   file's own YAML returned zero text-search hits for that string, while the
   Unity API resolved that exact same GUID to a real, present, correctly
   imported asset. This produced a confidently wrong claim made to a human
   ("that file was never imported") that was simply false.

**Rule:** for any question of the shape "does this asset exist / is it
referenced / is this value what I think it is," load it through the engine
and print it — the API resolves the reference *and* reports the actual
properties you care about, which a grep or a raw YAML read cannot do.
Sharpest version of this rule: where possible, read the **live gameplay
value** off a spawned/running instance rather than the serialized
definition — that proves what the *running game* actually believes, which
is the thing that matters. Serialized text is a storage format, not a
statement about game state.
