# Agent Workflow

## Suggested agent prompt sequence

### First AI/MCP prompt: read only

Use this before allowing an agent to change anything:

```text
We are building a small Unity 6 URP prototype called BookBoy. It is a first-person cozy
book-sorting game in one greybox library room.

Do not modify files yet. Use Unity MCP to inspect the current project, active scene, Console,
installed packages, and relevant Player/Camera objects. Tell me:
1. Unity version and render pipeline.
2. Whether the Input System, Cinemachine, AI Assistant, and Unity MCP are installed and working.
3. Any red Console errors or material runtime warnings.
4. The active scene hierarchy relevant to player and camera.
5. The smallest safe plan for adding a project-specific BookDefinition ScriptableObject, 12
   primitive book prefabs, a first-person pickup interaction, and explicit shelf slots.

Do not modify ProjectSettings, package manifests, input bindings, tags, layers, or
render-pipeline settings. Wait for approval before editing.
```

### First implementation prompt: book data only

```text
Implement only the data-driven book foundation. Do not change player/camera code,
ProjectSettings, packages, input configuration, layers, or tags.

Create a BookCategory enum with Alchemy, Astronomy, Beasts, and History. Create a BookDefinition
ScriptableObject with bookId, displayName, category, and spineColor. Put game scripts in
Assets/_Project/Scripts/Books and assets in Assets/_Project/ScriptableObjects.

Create no interaction system yet. Before editing, list the files you will create. After editing,
summarize changed files, required Unity Inspector setup, and exact play/compile tests I should
run.
```

### Second implementation prompt: pickup only

```text
Implement only a minimal first-person book pickup system. Use the existing active player camera.
Raycast from the center of the screen to detect a book in range. When aiming at one, show a
minimal prompt, `E Pick Up [Book Name]`. On E, carry exactly one book at a time in front of the
camera. Disable its Rigidbody physics and collisions while held. Do not add inventory, throwing,
generic item systems, or shelf placement yet.

Do not modify ProjectSettings, packages, input bindings, tags, or layers. Before editing, state
intended files and plan. After editing, list Inspector wiring required and a short test plan.
```

### Third implementation prompt: shelf placement only

```text
Implement explicit shelf slots for the existing book system. Each ShelfSlot must have
acceptedBookId, a snap transform, occupied state, and a way to identify it as an intended
destination. When a player carrying the matching book aims at an empty matching slot, show
`E Place [Book Name]`. Pressing E snaps the book into the slot and disables further pickup.
Incompatible slots must gently refuse placement without penalty. Do not implement a generic
inventory, automatic shelf finding, or broad placement framework.

Before editing, list files and plan. After editing, list required Inspector assignments and a
manual test checklist.
```

## Git workflow

Use a simple commit-per-milestone rhythm:

```text
Create or modify one small feature
  → Unity compiles
  → run the scene
  → validate behavior
  → inspect diff
  → git add .
  → git commit -m "Clear specific milestone message"
  → git push
```

Suggested commits:

```text
git commit -m "Create project content structure"
git commit -m "Add primitive arcane book prefabs"
git commit -m "Add data driven book definitions"
git commit -m "Add first person book pickup"
git commit -m "Add explicit book shelf slots"
git commit -m "Add library completion feedback"
```

## Git hygiene rules

- Keep Unity-generated folders ignored (`Library`, `Temp`, `Logs`, `Obj`, `Build`, `Builds`,
  `UserSettings` — already handled in `.gitignore`).
- Keep `.gitignore` tracked.
- Do not let an agent mass-reformat or mass-delete unrelated files.
- If the agent makes a dubious change, revert before trying an alternative.

See [03-technical-stack.md](03-technical-stack.md) for the broader AI guardrails (don't touch
`ProjectSettings`, package manifests, input bindings, render pipeline, tags, or layers without
explicit review).
