# 12 - Room loading, activation and culling for a small game (3 to 10 rooms)

Researched 2026-10-18 from Unity manual / script reference pages (6000.0 to 6000.4), plus forum and book excerpts where marked. No Unity commands were run; project settings were not inspected.

## 1. Question

For a hub plus a few rooms connected by doors (about 2000 transforms, roughly 1000 renderers when everything is active): is `SetActive` on a room root, additive scene loading, or Addressables the right mechanism? How do we keep state when a room unloads? What does it cost? Is baked occlusion culling, GPU occlusion culling, or portal/room visibility worth it, or is frustum culling enough? What do we recommend for 3 to 10 rooms?

## 2. Findings

### SetActive on a room root

- `SetActive(false)` "disables each component, including attached renderers, colliders, rigidbodies, and scripts"; `Update` stops, coroutines stop; when `activeInHierarchy` changes Unity calls `OnEnable`/`OnDisable` on all attached MonoBehaviours; `activeSelf` can be true while `activeInHierarchy` is false if a parent is inactive. Source: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/GameObject.SetActive.html . The page does not say it costs anything proportional to children; a forum summary says activation fans out to every child component (OnEnable) and heavy `OnEnable` code is the usual spike source: https://discussions.unity.com/t/should-i-use-gameobject-setactive-or-instantiate-and-destroy/1540591 and https://bugnet.io/blog/how-to-fix-unity-object-pool-setactive-cost (the latter is a vendor blog; low confidence).
- An inactive room stays resident in memory (the objects, meshes, materials, textures are loaded; they just are not rendered/simulated). This follows from the SetActive description (components disabled, objects still exist) and from the unload page below (only unloading/destroying frees objects). Memory saving therefore needs unloading or destroying, not SetActive.
- No Unity source gives a ms cost for activating a ~300-object room. UNSOURCED GUESS: a single activation of a few hundred simple renderers with light `OnEnable` code is likely small (single-digit ms) but could spike if `OnEnable` creates things; measure with the Profiler.

### Additive scene loading / unloading

- `SceneManager.LoadSceneAsync` "Loads the Scene asynchronously in the background" and returns an `AsyncOperation`; `LoadSceneMode.Single` unloads all current scenes first and Unity then calls `Resources.UnloadUnusedAssets`. Source: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html (the page itself does not describe additive mode).
- With `allowSceneActivation = false`, "Unity stops progress at 0.9, and isDone remains false", and "While isDone is false, the AsyncOperation queue is stalled" (other async operations wait). Source: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AsyncOperation-allowSceneActivation.html . So pre-loading a room scene and holding activation blocks other async loads.
- `SceneManager.UnloadSceneAsync` "destroys all GameObjects in a Scene and removes that Scene from the SceneManager"; "Assets are currently not unloaded" - call `Resources.UnloadUnusedAssets` to free asset memory; you cannot unload the last remaining scene. Source: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneManagement.SceneManager.UnloadSceneAsync.html
- Multi-scene lighting: lightmap and Realtime GI data "load and unload separately for each scene ... you can safely unload a scene that has lightmaps"; Light Probe data baked together is shared and loads together; baked occlusion data for additively loaded scenes is stored in one `OcclusionCullingData.asset` named for the active scene, and additive scenes referencing the same data initialise their culling from it. Source: https://docs.unity3d.com/6000.4/Documentation/Manual/bakemultiplescenes.html
- Forum reports (older Unity, anecdotal): lightmaps of an additively loaded, separately baked scene can look wrong unless that scene is the active scene, with `SceneManager.MergeScenes` as a workaround; seams appear where separately baked GI overlaps. https://discussions.unity.com/t/lightmaps-with-additive-scenes/672482 , https://discussions.unity.com/t/about-loading-lightmaping-data-from-scene-with-additive-mode/783729
- Skipping Enter Play Mode scene reload interacts with scene loading profiling: Unity says to profile startup with scene reload on because otherwise start time is "no longer representative" (see file 13).
- Unity's own manual pages I checked (Creating Scenes, Multi-Scene Editing) say little about runtime guidance for rooms; I found no Unity statement of "use additive scenes for X-sized games".

### Addressables

- Addressables loads assets by address, supports local or remote content, reference-counts assets ("doesn't unload an asset until the reference count returns to zero"), and requires you to release anything you load explicitly; it needs a content build per platform, groups/labels/profiles to maintain. Source: https://docs.unity3d.com/Packages/com.unity.addressables@2.3/manual/AddressableAssetsOverview.html . The page gives no threshold for when it is worth adopting.
- UNSOURCED GUESS: for a small game that ships all content in the Player, Addressables adds build/maintenance cost without a need (no remote content, no DLC, memory not tight).

### Culling

- Frustum culling is on by default for every camera; it "does not check whether a Renderer is occluded". Occlusion culling is an additional bake step that works best in "small, well-defined areas separated by solid geometry, such as rooms connected by corridors". Runtime cost is CPU; Unity warns it "can offset the CPU time that it saves"; the baked data uses memory; dynamic GameObjects can be occluded but "cannot occlude other GameObjects". It is "most likely to result in performance improvements when a Project is GPU-bound due to overdraw". Source: https://docs.unity3d.com/Manual/OcclusionCulling.html
- Baked occlusion setup: flag big solid static meshes (walls) as Occluder Static, any static renderer as Occludee Static, enable Occlusion Culling on the camera, bake in Window > Rendering > Occlusion Culling. Source: https://docs.unity3d.com/6000.0/Documentation/Manual/occlusion-culling-getting-started.html
- **Occlusion Portal** (for doors): a component representing a solid object with open/closed states; closed it occludes, open it does not; must not itself be Occluder/Occludee Static; set `open` from script when the door opens; requires a baked occlusion data. Source: https://docs.unity3d.com/6000.0/Documentation/Manual/class-OcclusionPortal.html
- **Items spawned dynamically are not occludees**: "Occludee Static" requires the object not to move at runtime; Unity says projects that generate scene geometry at runtime are not suited to the baked system (OcclusionCulling page above). Books are dynamic. The same page does say dynamic GameObjects "can be occluded" (they are tested against the baked data), they just cannot act as occluders, so hidden books would still be culled when behind baked static walls.
- GPU occlusion culling (URP): needs Render Graph (Compatibility Mode off), GPU Resident Drawer enabled, and GPU Occlusion enabled on the Universal Renderer; works best with many objects sharing a mesh, lots of occlusion, small occluded objects; uses bounding spheres, conservative; "If occlusion culling doesn't have a big effect on your scene, rendering time might increase." Source: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/gpu-culling.html . It depends on the GPU Resident Drawer (Forward+ in 6000.3; see file 10).
- Unity's graphics-performance page lists per-layer `Camera.layerCullDistances`, lower far clip planes and occlusion culling as culling tools, and says CPU rendering cost is dominated by "the cost of sending rendering commands to the GPU". It gives no numeric thresholds. Source: https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html
- Frustum culling is jobified/multithreaded since 2017.1 (Unity Learn tutorial via search summary) and a Unity staffer on the forum advised manual disabling of off-screen objects only if profiling shows a lot of time in culling: https://discussions.unity.com/t/bug-culling-called-even-if-disabled-everywhere/765582 (forum; low-medium confidence). I found no data on how cull time scales with thousands of renderers.

### Portal / room-visibility approaches

- Unity's built-in solution is the Occlusion Portal (above). For an enclosed room behind a door, the cheapest custom method is simply to deactivate the room (or its renderers) when the door is closed and the player is not inside, which is what the project does for rooms 2 and 3. No Unity source describes a custom portal system; UNSOURCED GUESS that a hand-rolled "visible rooms = current room plus rooms adjacent through open doors" set is sufficient for a hub with a handful of doors.
- UNSOURCED GUESS: the hub-to-room doors being closed should hide the room completely; when a door is open the neighbour room must be active and in view.

### Does any of it matter for ~1000 renderers?

- Unity gives no number. See file 10: the only desktop figure found is a single practitioner's "under ~2000 draw calls" for low/medium PCs (https://thegamedev.guru/unity-cpu-performance/draw-call-bound/). Roughly 1000 active renderers plus shadow passes is near but not clearly over that mark; the SRP Batcher lowers per-call cost.
- Unity's occlusion docs say the culling has a CPU cost that can offset savings and helps most when GPU-bound by overdraw. For small rooms with few overlapping surfaces, baked occlusion is probably not a net win. UNSOURCED GUESS on net effect; measure.

## 3. Numbers and rules of thumb

- `allowSceneActivation=false` stops progress at 0.9 (fact, Unity API page).
- ~2000 draw calls desktop rule of thumb; low confidence (see file 10).
- No sourced numeric costs for SetActive of a room, additive load time, or culling time. All timing questions should be answered with the Profiler (Hierarchy view, search for `SetActive`, `Awake`, `OnEnable`, `SceneManager.LoadSceneAsync`/`Integrate`).
- Unity's guidance order on CPU render cost: first profile, then reduce objects rendered (culling), then batch (Unity graphics performance page).

## 4. Confidence

- High: what SetActive and UnloadSceneAsync do; multi-scene lightmap behaviour (6000.4 manual); Occlusion Portal mechanics; occlusion culling's trade-offs as Unity states them.
- Medium: Addressables description (no adoption threshold in the source).
- Low: any claim about relative cost between SetActive and additive scene loads; forum claims on additive lightmaps in modern Unity 6; any ms figure.
- Unknown: whether the project uses baked lighting or only realtime lights (not inspected); whether rooms contain realtime shadow-casting lights; how big the activation spike is today; whether saved state reconciliation is already written.

## 5. Implication for BookBoy

Facts: one scene (Lamplit.unity) with hub plus rooms 1-3, rooms 2-3 inactive until their door opens; roughly 1000 renderers when all active; state must survive unload; skip-scene-reload ON.

- **Recommended approach for 3 to 10 rooms: stay in one scene, one root GameObject per room, toggle with SetActive for now, but make the room's state live in plain C# data (file 11), not in the scene objects.** This is the smallest change and is consistent with every source: SetActive stops rendering, physics and updates; additive scenes buy memory only if you actually unload; Addressables adds build-time complexity with no stated need.
- **When to graduate to unloading:** only if the Profiler/Memory Profiler shows the combined scene is too heavy to keep resident (not indicated by anything found; ~2000 transforms and ~1000 renderers is small by Unity project standards - UNSOURCED GUESS, no Unity number found). Then the cheapest next step is **destroy the room's item views and rebuild them from `ItemState` on entry** (item views are what dominate the object count), keeping the room's static shell (walls, shelves, containers) as inactive objects. A full additive scene per room is the step after that and needs per-scene lighting care (lightmaps are per-scene and unload cleanly per the 6000.4 manual; probes bake together).
- **State handling on unload/deactivate:** write each item's slot/transform/state into its `ItemState` before deactivating or destroying; never rely on the live GameObjects to retain it. This also solves the "no kicking piles" and "physics off for things on shelves" requirement, because shelved/pile items need no Rigidbody until interacted (file 11).
- **Culling: do nothing new for now.** Frustum culling is automatic. Baked occlusion/Occlusion Portals are designed for room-and-corridor layouts, so the *concept* fits the hub-and-doors design, but: (a) books are dynamic and cannot be occluders; (b) the room shells are small and already hidden by deactivation; (c) Unity says occlusion culling can cost more CPU than it saves, and helps mostly when GPU-bound. Revisit only if profiling shows high GPU fill/overdraw or visible-but-hidden rooms (for example, the hub seeing into an open room 1). If you add it: flag shells as Occluder/Occludee Static, add an Occlusion Portal to each door (not static, set `open` from the door script), bake once; the baked data is tied to the scene.
- **GPU occlusion culling: skip.** It requires GPU Resident Drawer (Forward+ in the 6000.3 docs) and Render Graph; these are render-pipeline settings that CLAUDE.md protects, and Unity says it can increase rendering time when it does not help.
- **Rooms 2 and 3 are SetActive(false):** current approach is consistent with Unity's docs. Make sure nothing in those rooms does heavy work in `OnEnable` (activation fan-out) and that room toggling does not rely on `Awake` firing (it fires only once per object; `OnEnable` fires on each activation).
- **Skip-scene-reload ON:** because Unity "only resets the scene's modified contents", an `activeSelf` flip made at runtime (door opens, room activated) is reverted at exit via the scene backup, but non-serialized fields on room managers can leak (file 13). Unity recommends enabling scene reload to profile real startup/load time; use that mode for any room-load measurements.
- **What to measure first (30 minutes, no code):** in a build, Profiler > CPU, activate room 2 and look at the single-frame spike; Rendering Statistics with all rooms active vs only the hub (batches, set-pass, SRP Batcher count); GPU vs CPU bound check. If the spike is under a frame or two and frame time is comfortably inside budget, no room-loading rework is needed.
