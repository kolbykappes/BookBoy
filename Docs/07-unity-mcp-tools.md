# Unity MCP: Setup & Enabled Tools

Unity MCP connects Claude Code directly to a running Unity Editor instance, so an agent can
create/inspect GameObjects, assets, and scenes live instead of hand-authoring YAML or waiting for
manual Editor steps. See [03-technical-stack.md](03-technical-stack.md) for the original
planned-setup steps; this file documents the actual working configuration and — importantly —
**what is and isn't saved in this repo**, since most of it lives outside version control.

## What's NOT in git (must be redone per machine)

None of the following survive a fresh clone or a different machine. If you set this up
somewhere new, you'll need to redo all of it:

- **The Unity Bridge itself** — `Edit → Project Settings → AI → Unity MCP Server` in the Editor.
  Must show `Unity Bridge: Running`. Requires the Unity Editor to actually be open with this
  project loaded (it is *not* running just because Unity Hub is open).
- **The Claude Code connection** — click **Configure** next to "Claude Code" in that same panel's
  Integrations section. This writes a `unity-mcp` entry into the *global*
  `~/.claude.json` (`mcpServers.unity-mcp`, a stdio command pointing at a local relay binary
  under `~/.unity/relay/`). It is user-scoped, not project-scoped, and not part of this repo.
- **Which of the 54 available tools are enabled** — the checkboxes in that same panel's "Tools"
  list. Confirmed by testing: toggling these produces zero diff anywhere in the repo (tracked or
  untracked), so this is stored in Editor preferences outside the project folder entirely. The
  current enabled set (see below) has to be manually recreated on any new machine/clone.
- A Claude Code session only picks up newly-connected/newly-enabled MCP tools after a real
  restart of the whole desktop app (not just a new chat/session) — see the troubleshooting note
  at the bottom.

## Currently enabled (13 of 54)

Enabled deliberately, in two passes — first the defaults Unity ships with, then a second batch
picked to cover Phase 4-6 work without touching anything on the guardrail list below.

| Tool | Category | Purpose |
|---|---|---|
| `Unity_RunCommand` | Core | Compile-and-execute arbitrary C# in the Editor. The workhorse — everything else is really a convenience layer on top of what this can already do. |
| `Unity_ManageGameObject` | Core | Create/read/update/delete GameObjects without hand-rolling it in a `RunCommand` script. |
| `Unity_ManageScene` | Core | Scene save/load/management — needed after learning the hard way that `AssetDatabase.SaveAssets()` does *not* save the open scene. |
| `Unity_ManageAsset` | Core | Generic asset Create/Modify/Delete/Duplicate/Move/Rename/Search/GetInfo/CreateFolder across any asset type (materials, folders, etc.) — the main tool for Phase 4/5 prefab and material work. |
| `Unity_FindProjectAssets` | Assets | Name + semantic/visual asset search. |
| `Unity_ListResources` | Core | Lists `unity://` resource URIs under a folder. |
| `Unity_ReadResource` | Core | Reads a resource by `unity://` URI, with slicing options. |
| `Unity_ValidateScript` | Core | Compile-check a script without executing it — a pre-flight safety net. |
| `Unity_GetProjectData` | Editor | Read-only project overview data. |
| `Unity_GetConsoleLogs` | Debug & Diagnostics | Read Console errors/warnings/logs. |
| `Unity_Camera_Capture` | Editor | Render an image from a specific in-scene camera. |
| `Unity_SceneView_Capture2DScene` | Editor | Orthographic top-down capture of a world-space rectangle. |
| `Unity_SceneView_CaptureMultiAngleSceneView` | Editor | 2x2 grid (Iso/Front/Top/Right) scene capture — the main tool used so far for visually checking book placement/color. |
| `Unity_AssetGeneration_GenerateAsset` / `Unity_AssetGeneration_GetModels` | Assets | AI asset generation (textures/models/sounds/etc.). Ships enabled by default; not used yet — MVP scope doesn't call for final art/sound. Requires a per-conversation consent step before first use regardless. |

## Deliberately left off, and why

Grouped by the reason, not by category — most of this maps directly onto guardrails already
written down elsewhere in `Docs/`.

**Matches the "don't touch casually" guardrail in [03-technical-stack.md](03-technical-stack.md)**
(ProjectSettings, packages, render pipeline, broad Editor state):
`Unity_ManageEditor`, `Unity_ManageMenuItem`, `Unity_PackageManager_ExecuteAction`.

**Matches the "no final art/sound/animation yet" non-goal in
[02-mvp-spec.md](02-mvp-spec.md)**: the AssetGeneration conversion/animation/audio sub-tools
(`ConvertToMaterial`, `CreateAnimation`-type tools, `EditAnimation`-type tools, etc.),
`Unity_ImportExternalModel`, `Unity_AudioClip_Edit`, `Unity_ManageShader`.

**Redundant with tools Claude Code already has directly** (kept off to keep script changes as
plain, git-diffable files rather than routed through the Unity bridge): `Unity_ApplyTextEdits`,
`Unity_CreateScript`, `Unity_DeleteScript`, `Unity_GetSha`, `Unity_FindInFile`,
`Unity_ManageScript`, `Unity_ManageScript_capabilities`, `Unity_ScriptApplyEdits`, `Unity_Grep`,
`Unity_ReadConsole` (duplicates `Unity_GetConsoleLogs`, already enabled).

**Not needed for this project's scope**: all 14 `Unity_Profiler_*` tools (performance profiling —
no perf work planned), `Unity_PackageManager_GetData`, `Unity_GetUserGuidelines`.

**Important nuance:** `Unity_RunCommand` already grants full C# execution — per its own tool
description it "can programmatically control virtually every aspect of the game, including
physics, input, graphics, gameplay logic, project setting and package management." So this
enable/disable list is **not a hard security boundary** — it's about which convenience tools
surface and reduce ad-hoc scripting, not about what's technically possible. The actual guardrail
is behavioral: never use `RunCommand` (or any tool) to touch `ProjectSettings`, package manifests,
input bindings, render-pipeline settings, tags, or layers without an explicit stated plan and
approval first, exactly as already written in [03-technical-stack.md](03-technical-stack.md).

## Troubleshooting notes (things that cost real time to diagnose)

- A newly-added/newly-enabled MCP tool does not appear via `ToolSearch` until the Claude desktop
  app is **fully quit and relaunched** — a new conversation/session in the Code tab alone is not
  enough, it just reuses the already-loaded config.
- `Unity_RunCommand` calls can transiently fail with `"Unity not detected (no fresh discovery
  files found)"` right after editing a `.cs` file — that's Unity mid-domain-reload from
  recompiling. Wait a few seconds and retry.
- The Unity Editor must actually be open (not just Unity Hub) with the Unity Bridge showing
  `Running` in that Project Settings panel — a system reboot or closing the Editor kills this
  silently, and tool calls will report a successful connection to the *relay* while returning
  zero actual tools, since the relay has nothing live to bridge to.
- `GameObject.GetInstanceID()` is obsolete in this Unity version (`GetEntityId()` replaces it,
  returning a `World:Entity`-style composite id like `35315:768`) — the older int-based
  `cameraInstanceID` parameter some tools expect doesn't accept this format directly, so
  camera-specific capture by ID doesn't currently work; scene-view capture (not tied to a specific
  camera) does.
- Always call `EditorSceneManager.SaveScene()` explicitly after creating scene GameObjects via
  `RunCommand` — `AssetDatabase.SaveAssets()` only saves asset files (materials, ScriptableObjects,
  etc.), not the open scene's contents.
