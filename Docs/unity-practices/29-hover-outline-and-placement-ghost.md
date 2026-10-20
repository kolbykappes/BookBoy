# 29 - Hover outline and placement ghost (URP, inverted hull)

Researched 2026-10-19 (web, plus reading the project's own shaders and the competitor screenshots in `Docs/competitive/img`). Builds on file 16 (aiming, picking, highlighting), which compared the techniques and recommended pooling; this file settles the look and the cheapest way to build it. Unmarked bullets carry a URL; anything else is marked **GUESS** or **derived**.

## 1. What Kolby asked for

- The old hover (a wide yellow glow plus sparkles) is not liked. Wants a more nuanced, outline-style highlight.
- The "where it goes" preview should look more like Librarian's.

## 2. What the reference games do (from our screenshots)

- **Librarian, aimed book on the floor (`07-floor-pile-aimed-book-lectern-wainscot`):** a thin, crisp white line around the book's silhouette, no tint, no sparkles. The title appears at the bottom, a "Press E" prompt in the middle. On a shelf (`04-row...`) the aimed book shows no outline at all, only the text.
- **Librarian, slot ghost (`11-bookcase-1A-green-ghost...`, `02-green-slot-ghost...`):** a bright green edge-lit shape in the exact slot: strong lines on the silhouette and the cover's edges, a faint fill that is almost see-through. It looks like a hologram, not a coloured box.
- **Witch Cabin** (`Docs/competitive/16-witch-cabin-cleanup.md`): aimed items get a white outline; the slot ghost is a plain slab silhouette in the shelf colour, and a wrong item gets no ghost.

## 3. Technique findings

- Inverted hull = draw the mesh again, push it out along its normals, draw only back faces. https://github.com/Delt06/urp-toon-shader/wiki/Outline
- **Hard edges break it.** Normals are split at sharp edges, so extruding along them opens gaps at box corners. The fix is smoothed normals stored where the shader can read them (the Delt06 package writes them to UV2 in an asset post-processor so lighting is unchanged). https://github.com/Delt06/urp-toon-shader/wiki/Outline ; Ameye says the same and suggests vertex colours: https://ameye.dev/notes/rendering-outlines/
- **Constant width on screen:** extrude in clip space: offset divided by screen size, multiplied by clip w to cancel the perspective divide, times two, so "1 unit = 1 pixel". https://ameye.dev/notes/rendering-outlines/ ; Delt06: clip space "should generally be enabled to account for perspective distortion".
- Default depth setup: cull front faces, ZTest less-or-equal, so the hull shows only beyond the silhouette. https://ameye.dev/notes/rendering-outlines/ . A hull hides behind nearer geometry (neighbouring books); Delt06 lists "make objects always draw on top" as the workaround and notes the price (no depth write, the skybox overwrites it).
- No source gives a millisecond cost; Ameye's table calls vertex extrusion simple and cheap and the full-screen methods costlier. One extra draw per outlined book is the cost; only one book is steadily outlined at a time (derived).
- A hologram-style ghost: unlit, fresnel rim for the edges, a time-driven band for movement. General tutorials only (no authoritative source found for building-game previews): https://danielilett.itch.io/hologram-shaders-pro-unity (asset page), Shader Graph fresnel tutorials. Treat the exact look as design, not sourced practice.
- A Render Objects or Full Screen outline feature is the other route (selects by layer or rendering layer, one pass for all objects). It edits the renderer asset, which CLAUDE.md fences (render-pipeline settings): **not used here, needs approval if we ever want it.** https://docs.unity3d.com/Manual/urp/renderer-features/renderer-feature-render-objects.html

## 4. Decision (smallest change that satisfies the sources)

1. **Keep the per-book hull, but fix it.** New `BookOutline` shader: clip-space extrusion with a width in pixels (scaled by screen height / 1080), along smoothed normals. Smoothed normals are built once per shared mesh at runtime (average the normals of vertices at the same position) and cached; the book meshes become readable (about 400 vertices each). No renderer feature, no layers, no project settings.
2. **Depth pull: tried and rejected (2026-10-19).** Moving the hull's vertices toward the camera so a neighbouring book would not hide the line brought the hull's back faces in front of the book's own front faces and painted the whole book in the line colour (seen in a render). A hull can only show where nothing nearer covers it, so on a shelf the line is hidden between touching books and shows on the top and bottom edges and in gaps (Librarian shows no outline on shelved books either: screenshot 04). `_DepthPull` stays in the shader at 0.
3. **Hover look:** a crisp warm-white line (about 2 px at 1080p) with a dark thin underlay so it reads on the cream floor tiles as well as on dark wood, a slow breathing of the alpha, no sparkles and no light. The colour is HDR (over 1) so the project's existing bloom softens it into a glow (**GUESS**, check in a render). Sister-glow pulses and placement flashes keep their colours, sparks and light.
4. **Ghost look:** the same outline hull in the grade colour (green/yellow/red) around a see-through fill with a fresnel rim and a slow upward light band (new `BookGhost` shader, additive, unlit). Silhouette only: the cover's inner edge lines are not reproduced (Librarian has them; we get them for free later if the cover texture has an edge pass).
5. Colour feedback (green/yellow/red) stays on the ghost; the hover on a plain book is neutral white (hover is not a grade).

## 5. Unknowns

- How bloom treats the HDR line on every background (render and look).
- Whether the white line on the cream tiles reads without the dark underlay (render both).
