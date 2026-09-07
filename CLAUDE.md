# BookBoy

Unity 6.6.0f1, URP, Windows PC target. First-person cozy book-sorting prototype set in an arcane
library. Working scene: `Assets/Scenes/LibraryPrototype.unity`.

**Read [Docs/README.md](Docs/README.md) first** — it indexes the full project brief (vision,
MVP spec, tech stack, build status, implementation plan, agent workflow). Keep
[Docs/04-build-status.md](Docs/04-build-status.md) up to date as milestones land.

## Guardrails

- Do not modify `ProjectSettings/`, `Packages/manifest.json`, input bindings, tags, layers, or
  render-pipeline settings without explicit approval and a stated plan first.
- Leave `Assets/Starter Assets/` (imported package content) untouched; put project-specific
  content under `Assets/_Project/`.
- Work in small, single-purpose vertical slices (data → interaction → shelves → feedback, per
  [Docs/05-implementation-plan.md](Docs/05-implementation-plan.md)) — not one large generated
  system.
- No inventory grids, save/load, procedural generation, combat/fail-states, or other items on the
  non-goals list in [Docs/02-mvp-spec.md](Docs/02-mvp-spec.md) until the MVP loop is proven fun.
