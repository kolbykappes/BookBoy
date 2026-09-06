# Technical Stack

## Engine and rendering

| Component | Current choice | Why |
|---|---|---|
| Engine | Unity 6.6.0f1 (`6000.6.0f1`) | Current Unity 6 generation, suitable for rapid PC prototype work |
| Rendering | Universal Render Pipeline (URP) Empty Template | Correct default for a stylized, cozy, room-scale 3D game; avoids HDRP complexity |
| Target platform | Windows PC | Fastest test/build iteration; Steam-friendly eventual target |
| Language | C# | Unity-native gameplay scripting |
| Camera system | Cinemachine | Installed via Unity Starter Assets; currently used for the player camera chain |
| Input | Unity Input System | Installed and used by Starter Assets |
| Player base | Unity Starter Assets, First Person Controller | Avoids wasting prototype time rebuilding character movement/camera behavior |
| Version control | Git | Local repository initialized and committed |
| Remote repository | GitHub, `BookBoy` | Existing remote project used for backup and future agent/repository workflow |

## AI-assisted development stack

The intended model is not "ask AI to make a whole game." The intended model is an agentic,
incremental workflow:

```text
Specify one small feature
  → agent inspects relevant project context
  → agent states planned changes
  → agent changes a small set of files
  → Unity compiles
  → human runs the game
  → agent fixes exact failure
  → Git commit
```

Recommended AI roles:

| AI role | Appropriate jobs |
|---|---|
| Unity-aware assistant / Unity MCP | Inspect the open scene, console, GameObjects, components, prefabs, project setup, and Unity-specific errors |
| External coding agent, such as Claude Code or Cursor | Plan multi-file C# changes, create focused scripts, review diffs, explain Inspector wiring, help debug compile/runtime errors |
| Human developer/designer | Own game feel, feature scope, visual readability, architectural constraints, test gameplay, and approve every change |

## Unity AI/MCP status

Unity MCP is **not yet configured** in this project.

The expected eventual configuration:

1. Install Unity AI Assistant package if needed (already present in `Packages/manifest.json` as
   `com.unity.ai.assistant`).
2. Confirm Unity 6 project is connected to Unity Cloud as required by the current Unity AI/MCP
   setup.
3. Open `Edit → Project Settings → AI → Unity MCP`.
4. Confirm Unity Bridge runs.
5. Connect an MCP-capable client, ideally Claude Code or Cursor.
6. Start with a read-only project-inspection prompt.
7. Grant write actions only in small reviewed increments.

## AI guardrails

- Do not let an agent casually change `ProjectSettings`, `Packages/manifest.json`, input
  bindings, render pipelines, tags, layers, or broad package dependencies.
- Require a file-level change plan before implementation.
- Make changes in small vertical slices.
- Inspect Git diffs and run the Unity scene after each change.
- Never accept a giant catch-all `GameManager` or unreviewable generated framework.
- Commit working milestones frequently.

See [06-agent-workflow.md](06-agent-workflow.md) for the concrete prompt sequence and Git rhythm
that put these guardrails into practice.
