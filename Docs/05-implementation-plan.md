# Implementation Plan

Do not overbuild. The first job is to make one book feel good to pick up and return to one
correct home. Work phase by phase; don't start a phase before playtesting the previous one where
a playtest step is called for.

## Phase 1: Project organization

Before creating scripts, establish a clean project-specific content structure:

```text
Assets/
  _Project/
    Art/
      Materials/
      Models/
      Textures/
    Audio/
    Prefabs/
      Books/
      Interactables/
      Shelves/
    Scenes/
    Scripts/
      Books/
      Core/
      Interaction/
      UI/
    ScriptableObjects/
    UI/
```

Move only project-created content into `_Project`. Leave imported `Starter Assets` untouched.

## Phase 2: Visual book proof

Build four test books first, one in each category:

- Use Unity cubes, scaled into visually chunky book shapes.
- Make simple URP materials for amber, indigo, forest green, and burgundy book spines.
- Place the books in a loose, visible scatter area several meters in front of the shelf.
- Play-test whether books are readable and appealing from first-person view.
- Only after the size and visual language feel right, expand to 12 total books.

Initial book physical profile (original spec — **superseded, see below**):

```text
Approximate scale:
X = 0.20 to 0.40
Y = 0.55 to 0.95
Z = 0.12 to 0.20
```

Books should vary in width and height, but not so much that visual clarity gets worse.

> **Corrected 2026-09-06.** The numbers above are roughly 2-3x real-world book size (a 0.55-0.95m
> tall book is 55-95cm — a comically oversized prop, not a "tome"), and combined with an
> unrealistic player eye height (2.38m, measured), made the whole room feel gigantic — "the shelf
> is 10 feet tall, I can't see the lip of it" was direct playtest feedback. Corrected against real
> human/book/furniture proportions: see [09-scale-reference.md](09-scale-reference.md) for the
> numbers now actually in use (player eye height, book dimensions, shelf spacing) and the
> reasoning behind them. Use that file, not the numbers above, for anything scale-related.

## Phase 3: Book data

Create a narrow, data-driven `BookDefinition` ScriptableObject.

Minimum fields:

```csharp
string bookId;
string displayName;
BookCategory category;
Color spineColor;
```

Initial category enum:

```csharp
public enum BookCategory
{
    Alchemy,
    Astronomy,
    Beasts,
    History
}
```

The first build can use a book-specific ID for exact placement. Later, category/order
relationships can become a puzzle layer.

## Phase 4: First-person interaction

Implement only enough generic interaction for books:

- Raycast outward from center of active player camera.
- Detect book within short interaction range.
- Show a simple prompt: `E Pick Up [Book Name]`.
- Press `E` to pick up one book.
- Disable the held book's physics and collisions while held.
- Parent it to a carry transform in front of the camera.
- Do not allow carrying more than one book.
- Do not build throwing, drag/drop, inventory slots, generic item frameworks, or broad physics
  systems.

## Phase 5: Explicit shelf slots

Place 12 explicit shelf-slot transforms in the scene.

Each `ShelfSlot` should have:

```text
acceptedBookId
snapTransform
occupied state
optional category marker
```

Rules:

- A carried book can only occupy its intended slot.
- Correct slot displays `E Place [Book Name]`.
- Correct placement snaps the book into a stable final transform.
- Placed books can no longer be picked up in the earliest MVP.
- Incorrect slots show a gentle refusal.
- Full slots reject any further placement.

## Phase 6: Feedback and completion

Add bare-minimum game feel:

- Screen-space UI: `Books Restored: X / 12`.
- Correct placement chime.
- Correct slot visual pulse/glow.
- Gentle wrong-placement response.
- Completion panel: `The Arcane Library Is Restored`.
- Single `Restart Room` button.

## Phase 7: Playtest and evaluate

Do not add content until testing answers these questions:

1. Is picking up a book reliable every time?
2. Can the player understand where a book belongs without reading lots of text?
3. Does correct placement create a satisfying enough response?
4. Is walking to/from shelves enjoyable or merely friction?
5. Are books visually large enough to read but small enough to create a satisfying dense shelf?
6. Does the room visibly transform as books return to their places?
7. Would a player want to sort 50 or 100 more books after doing 12?

If any answer is "no," improve the interaction, presentation, visual language, feedback, or
player movement before adding more rooms or more categories.

## Handoff note

The project is no longer blocked by Unity setup. The playable foundation works. The correct next
action is to create the `_Project` folder structure (Phase 1) and make the first four visibly
distinct cube books (Phase 2), then stop to play-test their scale and readability before coding
interaction (Phase 3+).
