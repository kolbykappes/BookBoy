---
title: Unity geometry/animation/physics measurement traps (cold tier)
retrieval_hook: >
  Read this when: sizing or positioning anything from a prefab/mesh/rig; the
  player CharacterController's grounded state seems wrong; using a
  SphereCast/raycast for the book pickup interaction and getting weird
  contact points; iterating GameObject hierarchies in editor tooling; if
  BookBoy later adds animated characters, revisit the animation-specific
  entries below, currently NOT YET APPLICABLE.
source: consolidated from unity-agent-bootstrap.md §3.C and
  unity-notes-for-a-new-unity-dev.md Part 3 (CastleAssault, single-player
  sections; [MP] items dropped).
---

## Triage against BookBoy specifically — 2026-10-17

CastleAssault is a character-animation-heavy project (skinned meshes,
humanoid rigs, root motion, ragdolls). BookBoy's MVP has **no animated
character content** — books are static meshes, and the player is first-person
with no visible body mesh. Most of the animation/skinned-mesh entries below
are therefore **NOT YET APPLICABLE** — kept here because BookBoy's
longer-term roadmap (brief §6: "quality-of-life abilities," later content)
could eventually introduce rigged assets, at which point re-triage these as
APPLIES. The physics-cast and hierarchy-iteration entries **APPLY NOW** —
the first-person pickup raycast (brief Phase 4) and any editor tooling that
walks the scene hierarchy both touch exactly these traps.

## [APPLIES — Phase 4 pickup raycast] Sweeps/casts that begin already overlapping a collider

**When** writing or debugging the raycast-based pickup interaction (brief
Phase 4: "Raycast outward from center of active player camera"), **do**
never trust `hit.point`/`hit.normal` unguarded if you ever switch from
`Raycast` to any `SphereCast`/shape-cast variant, **because** a
`Physics.SphereCast*` that begins already overlapping a collider returns
`hit.distance == 0`, `hit.point == Vector3.zero`, and `hit.normal` equal to
exactly the negated cast direction. The collider itself is correctly
detected — the point and normal are garbage. (Plain `Raycast` behaves
differently: a ray starting inside a collider typically reports no hit at
all, so this specific trap is shape-cast-only.)

Concrete cost on the source project: a damage-number spawn offset was
computed from `point + normal*0.25 + up*0.25`, which evaluated to the world
origin under this condition — which happened to coincide with where a
player was standing, so the bug read as "effect spawned on the wrong
entity" rather than "cast returned garbage." It only reproduced at
point-blank range, because at any distance the cast shape starts clear of
the target.

**Two tells that identify this specific trap if you hit something like it:**
it requires proximity (point-blank only), and the wrong position is the
**same value every time**, regardless of what was actually hit.

**If BookBoy's pickup ever moves from a plain raycast to a sphere/capsule
cast** (e.g. for a more forgiving aim-assist), repair the degenerate case
with `Collider.ClosestPoint(castOrigin)` and derive the normal as
`(castOrigin - point).normalized`, falling back to `-direction` — note
`ClosestPoint` returns its input unchanged when the point is inside the
collider, which is actually the correct behavior to build on.

## [APPLIES — any editor tooling] `GetComponentsInChildren(bool, List<T>)` clears before filling

**When** writing any editor tool or runtime code that walks multiple root
GameObjects collecting components via the `List<T>`-output overload, **do**
use the array-returning overload plus `AddRange` instead if you need to
accumulate across more than one root, **because** the List-output overload
**clears the list before filling it** — it does not append. Measured
directly: handing the same list to the same root twice reads the same count
both times, never double.

The common pattern that silently breaks:
```csharp
var found = new List<RoomVolume>();
foreach (var root in scene.GetRootGameObjects())
    root.GetComponentsInChildren(true, found);   // keeps ONLY the last root's results
```
On a scene holding two of whatever you're counting, this reports "0 found"
(or whatever the last root happened to contain) — which reads exactly like
a genuinely broken/empty scene rather than a collection-API gotcha.

**Fix:** `GetComponentsInChildren<T>(true)` (array-returning) + manual
`AddRange` into your accumulator, since the array overload is a clean
per-call result rather than an in-place mutation of something you passed in.

## [NOT YET APPLICABLE — no skinned meshes in BookBoy MVP] Skinned renderer bounds are bind-pose, not deformed

For asset packs sharing one mesh file across many characters,
`SkinnedMeshRenderer.bounds` reports the mesh's local AABB in **source-file
(bind-pose) space**, not its actual deformed/current extent — even after
`BakeMesh`, which writes vertices but does NOT refresh `Mesh.bounds`.
Measured cost on the source project: sizing a CharacterController from
unbaked bounds left characters floating ~0.19m; even after baking, a sole
height measurement was off by 0.107m against the actual baked vertices —
which was the entire float-vs-sink decision for that character.

**If this becomes relevant** (BookBoy adds a skinned/rigged character — a
librarian NPC, a visible player body): iterate `baked.vertices` through
`smr.transform.localToWorldMatrix` rather than reading `bounds` directly.
Works in edit mode.

## [NOT YET APPLICABLE] An asset's rig pose ≠ the pose the game renders

Sampling an instantiated rig in edit mode can read materially different
bone positions than the same rig in Play Mode, because a prefab stores its
raw source pose and the Animator only replaces it at runtime. Neither
`Animator.Rebind() + Update(0f)` nor a bare `Update(0f)` evaluates correctly
in edit mode.

**If relevant later:** resolve the default Animator state's clip and call
`AnimationClip.SampleAnimation(instance, 0f)`; for a BlendTree default
state, sample the lowest-threshold child (the standstill pose). Anything
sized off a rig directly — colliders, camera framing, spawn offsets — is
wrong if measured from the un-animated asset.

## [NOT YET APPLICABLE] Bones are not the silhouette

Sizing any collider from a bone position (rather than the actual mesh
silhouette) undersizes it — concrete example: a head bone sat 0.32m below
the actual mesh-top-including-hood, leaving the visible head outside its own
collider, which then surfaced as a UI bug (health bar anchored to
`collider.bounds.max.y` floated in the wrong place) rather than reading as a
collider-sizing bug. General lesson, worth keeping even without skinned
meshes: **anything anchored to a collider's bounds inherits every sizing
error upstream of that collider** — a UI symptom can have a physics-sizing
root cause.

## [NOT YET APPLICABLE] Don't size a character by sweeping every animation clip for the tallest pose

It picks the jump animation every time. A capsule sized that way is taller
than the character ever actually stands during normal play.

## [COULD APPLY — BookBoy's first-person CharacterController] Capsule grounding: account for skin width

**If** BookBoy's first-person controller (brief: "walk, look around, collide
with geometry") uses Unity's `CharacterController`, **do** remember the rule
the source project settled on after several wrong attempts: **capsule
bottom = baked sole position + `skinWidth`**. A `CharacterController` comes
to rest with its nominal capsule bottom sitting one `skinWidth` *above* the
floor by design, so lifting the capsule's conceptual bottom by that amount
is what actually puts visual feet (or, for BookBoy, the camera-height
reference) at the intended ground height.

## [NOT YET APPLICABLE] Root motion lives in muscle curves

Humanoid animation clips store translation/rotation in muscle curves
(`RootT.*`/`RootQ.*`), not per-bone transform curves. With
`applyRootMotion = false` (the normal setting for code-driven movement),
none of that curve data applies — so a clip authored with baked-in motion
(e.g. a death clip that lies the body down) will NOT move the in-engine
object to match; the pose animates correctly but the body doesn't translate,
producing a "hovering" artifact. No Unity 6 equivalent exists for Mixamo's
"Bake Into Pose" on the clip importer. Prefer genuinely in-place clips, or
drive positional offset in code.

## [COULD APPLY — if BookBoy's player uses CharacterController] `isGrounded` can lie on a genuinely grounded body

**If** any gameplay logic gates on `CharacterController.isGrounded` (not
currently part of BookBoy's MVP loop, but worth knowing before adding
anything like "can only place a book while standing still on the floor"),
**do** prefer state your own game logic maintains over the engine's own
grounded reading, **because** a character standing on flat ground and
moving at constant speed was measured reporting `isGrounded == false` in
**every sampled frame of a six-second run**, while height stayed constant
and a separate airborne flag correctly read false. A gameplay feature gated
on `isGrounded && otherCondition` silently never applied for the entire
session — no error, no warning, full animation and interaction playback,
indistinguishable from a feature that was never implemented at all.

## [NOT YET APPLICABLE] Animator stepping needs an explicit zero-delta update

When stepping a rig through clips frame-by-frame in editor tooling, call
`Animator.Update(0)` after each `Play` — without it, the first sample reads
the *previous* pose, inventing a peak/transition that doesn't actually exist
and corrupting any timing derived from it.
