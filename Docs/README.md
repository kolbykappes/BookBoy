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

## Quick orientation

- **Engine:** Unity 6.6.0f1 (`6000.6.0f1`), URP, Windows PC target.
- **Working scene:** `Assets/Scenes/LibraryPrototype.unity`.
- **Current state:** Greybox room + Starter Assets first-person controller/camera are playable.
  No project-specific scripts, book system, pickup, or shelf logic exist yet — see
  [04-build-status.md](04-build-status.md).
- **Next step:** Phase 1 of [05-implementation-plan.md](05-implementation-plan.md) — create the
  `Assets/_Project/` folder structure — then Phase 2 (visual book proof), per the handoff note
  at the end of that file.
