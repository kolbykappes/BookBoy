# BookBoy Docs

Project reference docs, split from the original project brief so they're easy to load
piecemeal (by a human or an agent) instead of one giant file.

**Source document:** `BookBoy_Project_Brief_and_MVP_Plan.md` (2026-09-06), digested into the
files below. If the two ever disagree, these files are the current source of truth — update
them as decisions change, don't re-import the original brief.

| File | Contents |
|---|---|
| [01-vision-and-design.md](01-vision-and-design.md) | Pitch, product intent, core loop, design principles, longer-term differentiation, competitive landscape |
| [02-mvp-spec.md](02-mvp-spec.md) | MVP scope, player experience requirements, categories, explicit non-goals |
| [03-technical-stack.md](03-technical-stack.md) | Engine/tooling choices, AI-assisted workflow, AI guardrails |
| [04-build-status.md](04-build-status.md) | What's done, what's not, current scene inventory — **update this as work lands** |
| [05-implementation-plan.md](05-implementation-plan.md) | Phase-by-phase build plan (project structure → books → interaction → shelves → feedback → playtest) |
| [06-agent-workflow.md](06-agent-workflow.md) | Suggested agent prompts, Git commit rhythm, Unity MCP setup notes |
| [competitive/](competitive/README.md) | Deep competitive analysis of the "tidy up" Steam sorting wave — one file per game (release date, patch history, reviews) plus a dated review tracker |
| [07-unity-mcp-tools.md](07-unity-mcp-tools.md) | **Superseded 2026-10-17** — historical record of the Unity MCP bridge setup; replaced by the Unity CLI workflow in the root `CLAUDE.md` |
| [../.claude/memory/](../.claude/memory/unity-editor-state.md) | Unity dev lessons (cold tier, read on demand; each file has a `retrieval_hook`): [editor state](../.claude/memory/unity-editor-state.md), [prefabs & assets](../.claude/memory/unity-prefabs-and-assets.md), [testing](../.claude/memory/unity-testing.md), [editor-driving tooling](../.claude/memory/unity-tooling-editor-drivers.md), [geometry/physics](../.claude/memory/unity-measuring-geometry-animation-physics.md). **These take precedence over other docs on Unity workflow.** |
| [10-backlog.md](10-backlog.md) | **The to-do list**: requested features (controller support, achievements, ...), ideas, chores and open decisions. Add to it; hand any item to an agent |
| [11-changelog.md](11-changelog.md) | **Dated log of what was completed**, newest first. Add an entry whenever work lands |
| [09-scale-reference.md](09-scale-reference.md) | Real-world-grounded sizes for player eye height, books, and shelving — check before placing/sizing any new object |

## Quick orientation

- **Engine:** Unity 6.6.6f1 (`6000.6.6f1`), URP, Windows PC target.
- **Working scene:** `Assets/Scenes/LibraryPrototype.unity`.
- **Current state:** a playable library: 128 books in 16 colors, 16 themed bookcases with wall signs, carrying, tossing,
  green/yellow/red placement feedback, a run timer, an upgrade tree and four timed abilities, menus, lighting and a tiled floor.
  See [04-build-status.md](04-build-status.md) for details and [11-changelog.md](11-changelog.md) for what landed when.
- **Next step:** pick from [10-backlog.md](10-backlog.md) (controller support and achievements are the requested ones).
