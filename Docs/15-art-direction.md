# Art Direction and AI Prompt Kit (Lamplit)

Written 2026-10-19, revised the same day for the 1947 setting. **Status: proposal for Kolby to edit.** Everything here that describes a person, a colour or a look is a starting point, not a decision; items that depend on a decision are marked **[confirm]**. The story it serves is in [13-concept-draft-inherited-library.md](13-concept-draft-inherited-library.md) ("Story decisions, v2"); the sizes and materials it must agree with are in [09-scale-reference.md](09-scale-reference.md), [04-build-status.md](04-build-status.md) and [unity-practices/26](unity-practices/26-ai-assisted-book-models-and-materials.md).

## 1. What this document is for

Kolby is generating **proxy art** with an image generator. Proxy means placeholder: good enough to judge mood, readability and fit now, and built to the same specs as the final art so a commissioned or hand-made piece can replace it without touching code. Rules from the backlog (D-005, 2026-10-18) and the Steam research (files 25 and 26):

- AI art is fine for **placeholders and mood**. Real art comes later.
- Every AI image gets a row in the **provenance log** ([asset-provenance.csv](asset-provenance.csv)): tool, model, prompt, date, what a human changed. Steam asks for disclosure of AI content that ships, and a "no AI" claim backfires if any visible asset looks generated (DinoBones, [competitive/15](competitive/15-refresh-2026-10-17.md)).
- **Text is never drawn by the image tool.** Titles, letters, numbers, the checklist and Rufus's notes are drawn by the game or set in a font, because generators garble small text and the set letter is a sorting key.
- No AI 3D meshes for books (file 26). 3D shapes come from Blender; AI helps with flat 2D pictures and neutral surface patterns only.
- US copyright: raw prompt-only output is probably not protectable (file 26). Edit, crop, paint over or composite by hand so the shipped version has a human author, or keep AI work out of anything you want to own exclusively (the logo, the capsule).
- Store page screenshots and key art must match the shipped game (Witch Cabin lost goodwill to mismatched trailer images).

## 2. The setting: autumn 1947, a 1905 building full of 1910 to 1930 things

**The story takes place in 1947** **[confirm: whether the year is ever printed; I suggest it is not, and the game just says "after the war"]**. The building is an Edwardian town library, built about 1905. The town has little money, so the library was fitted out in the 1910s and 1920s and has run on those fittings, mended again and again, ever since. **Nothing in the frame may be newer than about 1930.** Nothing from the 1950s, and no postwar tech or design.

Why 1947, and why old things: Rufus has been head librarian for over thirty years (since about 1915), so the library is the world he built before and through both wars. A town that kept the Lamp going through thrift, rationing and patience is the right place for a story about mending, patience and passing a craft on. The wars are never shown. They are felt only as thrift: string saved, wrapping reused, spines mended, things kept going.

**Timeline for consistency (derived from the story; adjust freely):** Rufus is born about 1875, becomes head librarian about 1915, and dies in 1947 aged about 72. The first portrait is a studio photograph from about 1936 to 1938, when he was in his early 60s. The player's childhood job (age nine to fourteen) was in the 1930s or early 1940s.

### 2.1 Period rules: what may and may not appear

| Allowed (1900 to 1930, worn) | Not allowed (anything later) |
|---|---|
| Gas wall brackets with mantles, hand-lit with a taper; early electric bulbs with green glass banker's lamps; brass oil lamps | Fluorescent tubes, modern light fittings, chrome |
| Oak and mahogany furniture, cast-iron radiators, pot-bellied or tile stoves, wavy window glass, leaded fanlights | Formica, laminate, vinyl, plastic, moulded chairs, aluminium, any 1950s "mid-century" shape |
| Encaustic floor tiles, oak parquet, worn linoleum, long runner rugs | Wall-to-wall carpet, vinyl tile |
| Upright office typewriter, candlestick telephone (or none), rubber date-stamps, ink pads, ink wells, dip pens and fountain pens, pencils | Ballpoint pens, electric typewriters, rotary-dial telephones, electric clocks, radios, televisions, adhesive tape |
| Card catalogue of oak drawers with brass pulls and rods, wooden shelf-list trays, date-due slips, library-card pockets | Plastic card cases, filing cabinets in grey steel, computers or screens |
| Brown paper, string, wax seals, telegram forms, penny-post style stamps and postmarks (no real stamps or rates) | Cellophane, plastic bags, zip-lock anything |
| Woollen, cotton, linen, canvas and leather clothes; waistcoats, braces, sleeve garters, watch chains, round wire spectacles, felt and leather shoes | Nylon, zips on workwear, crepe soles, trainers, jeans, T-shirts, sunglasses |
| Cloth bindings with gilt stamping, marbled endpapers, leather corners, library buckram, foxed paper | Glossy dust jackets, paperbacks with modern cover art, colour photographs in books |
| A bicycle, a horse-and-cart or a plain delivery handcart in the street | Modern cars; if a vehicle is needed, leave it out of frame |

**War imagery: none.** No uniforms, flags, weapons, ruins or propaganda posters. If you want one quiet trace, use a faded strip of tape or a pasted-over strip at the edge of one window (blackout leftovers), and nothing more. **[confirm]**

**Prompt guard.** Add this to every prompt's negative list: `1950s, mid-century modern, plastic, chrome, formica, vinyl, laminate, ballpoint pen, fluorescent light, modern clothing, zipper, sunglasses, television, radio, crepe sole, cellophane, modern car, atomic age`.

## 3. The look in one paragraph

**Lamplit looks like a well-kept, badly funded town library at the end of an autumn day in 1947.** Dark carved oak and mahogany, brass, cream paper gone yellow, cloth-bound tomes with gilt stamping. Pools of dim amber gaslight and green-shaded lamplight sit between deep, cool, never-black shadows. Everything is worn into itself: rubbed leather corners, foxed paper, a repaired spine, a tin of saved string, hand-lettered signs, a card catalogue whose brass pulls have gone dark with hands. The mood is hushed, grounded and quietly tender, with a trace of ache, because this was someone's working life and his things are still here. It is being put back in order by hand.

**Keywords:** hushed, grounded, worn, mended, patient, lamplit, handmade, thrifty, sepia-warm, quiet grief, quietly hopeful.

**Not:** magic, runes, glowing sigils, wizard hats, floating books, neon, grimdark, hyper-real photograph, glossy, clean, nostalgic-cute, anything after 1930 in the frame.

## 4. Palette and light

The in-game numbers come from the measured work in 04-build-status (wood, palette solve, lighting pass). Art should sit inside them, not fight them. Values below are starting points to be sampled in the engine **[guess]**.

| Role | Look | Starting value |
|---|---|---|
| Wood (cases, desk, floor) | Deep warm mahogany and dark oak, low gloss, rubbed pale at the edges and handles | `#6B401F` (the wood the palette was solved against) |
| Brass | Soft old gold, dull, dark in the crevices | about `#A9803A` |
| Paper and page blocks | Yellowed cream, foxed at the edges, never white | about `#E6D8B5` |
| Lamp light | Dim amber, like gas or a weak early bulb | about `#E8A64E` at the source (the game uses warm orbs, intensity 5, range 9) |
| Shadow and night | Cool blue-grey | about `#2B3340` (the scene has a dim bluish fill) |
| Paint | Chipped bottle-green and oxblood paint, mustard and nicotine-brown plaster, faded navy | muted, desaturated |
| Set colours (the game's 24) | Soft clear colours solved for contrast and colour-blind separation | **Assigned in code, not by the art.** Cover art must stay neutral and tintable (see 6.2) |

**Light recipe for any picture:** one or two dim warm pools from lamps or gas brackets, a faint cool light from a tall window or the door, soft falloff, a little dust, no hard black. The light says *someone is working here*, not *it is a cozy Instagram cabin*.

**Colour rule:** the picture is warm and mid-dark, and the **books are the brightest, clearest things** in the frame (the game measured every set at 2.2 or more contrast against the wood). If an image makes the books darker than the wood, it will fight the game. To make the books feel period, use cloth texture, gilt stamping, ornament and aging, **not** a change of hue.

## 5. Four kinds of art, four styles

Use a different medium per channel, but keep the same palette, light and period so they read as one world.

| Channel | Used for | Medium and style |
|---|---|---|
| **A. Game surfaces** (flat, neutral) | Book cover and spine textures, wood, plaster, floor tiles, paper, brass | Flat front-on swatches, tileable, even lighting, no perspective, no baked shadows, **light neutral tones that the game tints** (cover textures are MULTIPLY textures: light cloth about 0.94 with dark ink, per `tools/covers/README.md`) |
| **B1. Mood and scene art** | Key art, room mood boards, menu and loading backgrounds, the ending image | Muted illustrated realism in the manner of mid-century book illustration: gouache and ink on textured paper, visible grain, soft but grounded, limited warm palette, aged paper tone. Not storybook-soft and not photographic |
| **B2. Portraits** | Rufus, framed on the desk or wall | A period studio photograph: soft focus, sepia or silver-print tones, a painted backdrop, slight vignette, some cracking at the edge. **Riskier with AI** (faces show AI tells), so generate several and compare; the fallback is an ink-and-wash portrait with a sepia photo tone **[confirm]** |
| **B3. Emblems, bookplates, diagrams** | Series emblems, bookplates, handbook diagrams, the notice header | Copperplate engraving: fine cross-hatching, ink on cream paper. Engraving was still the standard for bookplates and trade ephemera, so it is period-correct |
| **C. UI and icons** | Find icons, title cards, handbook pages, door-notice paper | Same engraving and typewritten language, flat, one dark ink on cream, readable at 64 px |

3D props and furniture are modelled in Blender (see `tools/blender/`); the prompts in section 8 for those are **reference sheets** to model from, not files to import.

## 6. The master style block (paste this at the start of every prompt)

> **Style anchor.** Muted, grounded, hand-made illustration of a worn small-town library in autumn 1947, built in 1905 and furnished with 1910s to 1920s things. Dark carved oak and mahogany, dull brass, yellowed cream paper, cloth-bound books with gilt stamping. Dim amber gaslight and green-shaded lamp pools with cool blue-grey shadows and a little dust. Gouache and ink on textured paper, limited warm palette, visible grain, mended and lived-in, quietly hopeful. Nothing newer than 1930. No magic, no text.

**Negative prompt** (for tools that take one; otherwise end the prompt with "avoid:"):

> text, letters, words, captions, logos, watermark, signature, magic, glowing runes, sparkles, wizard hat, floating objects, neon, harsh black shadows, photorealistic, 3d render, glossy, oversaturated, cartoon, anime, extra fingers, deformed hands, blurry, low resolution, cluttered unreadable background, 1950s, mid-century modern, plastic, chrome, formica, vinyl, laminate, ballpoint pen, fluorescent light, modern clothing, zipper, sunglasses, television, radio, crepe sole, cellophane, modern car, atomic age

**Shared rules**

- Name no living artist, studio, game or film in a prompt (legal and trust reasons; the research files record how reviewers react).
- Name no real brand (typewriter, lamp or paper makers).
- Ask for **"no text"** every time and draw text in-game.
- Ask for **plain backgrounds** on anything that will be cut out.
- Generate **four variants**, keep the best, and log the *final* prompt used.
- First establish the look: run prompts 8.1 to 8.3, pick the best three, and **feed them back as style reference images** for everything else (most tools have a style-reference, image-prompt or "same style as" control). This does more for consistency than any wording.
- Aspect ratios: mood art 16:9 (the game is 16:9 only), portraits 4:5, items 1:1, textures 1:1.
- Resolution: at least 2048 px on the long side for mood art, 1024 for items and portraits, 1024 or 2048 for textures (the covers in the game are 2048 x 1600 boards).

## 7. What exists in the game, so the art matches

### 7.1 The world

- **Scale:** adult scale, eye 1.65 m, 9 ft ceiling, a hallway hub 10 ft wide with the librarian's desk at the entrance and doors to the rooms.
- **Bookcases:** carved mahogany with a stepped plinth, reeded pilasters, a dentil cornice, an arched crest on the wide bays, and a **brass nameplate on every shelf front** that engraves the claiming set's icon and letter. These are period-correct as built.
- **Books are tomes:** 12 to 15 in tall, 10 in deep, 4 in thick, rounded spine, cloth boards, cream page block.
- **Rooms built so far, and the period look for each:**
  - **The Hub:** the charging desk at the entrance, a long corridor, oak doors with brass numbers and glass transoms, a runner rug on encaustic tiles.
  - **The Reading Room:** warm oak, green-shaded banker's lamps on long tables, gas brackets on the walls, tall wavy-glass windows, a cast-iron radiator, a wall regulator clock.
  - **The Mailroom:** books, periodicals, letters and parcels; oak pigeonholes and sorting tables, a brass parcel scale, string on a hook, bottle-green painted walls. This **replaces the concept's "1960s annex, teal and steel"**, which breaks the period rule. Doc 13 still has the old wording **[confirm: update doc 13]**.
  - **The Map Room:** atlases, rolled maps in racks, flat sheets in wide oak drawers; cool blue-grey light from north windows.
  - The concept's later wings (a conservation room, a herbarium, the vaults, the founder's study) are **provisional** and tied to the older story **[confirm]**.

### 7.2 The six printed book series

Each series has a cover and a spine design (`tools/covers/`). A new series must keep the same structure: tintable board art, a title running down the spine, a series-icon plate at the top, a volume-number plate at the bottom. **The text is drawn by the game.**

| Series | Subject | Motif |
|---|---|---|
| LAMPS | The reading-room lamps (the player's childhood job) | A gas lamp with a small flame |
| CATALOGUE | Index cards and drawers | A card-catalogue drawer |
| LETTERS | The founder's letters | A dip pen and sealed letter |
| ATLAS | Maps and travel | A compass rose |
| MENDING | Repair and conservation | A needle, thread and bone folder |
| VAULT | Rare and locked-away | A key and keyhole plate |

### 7.3 The six finds (Rufus's working tools, installed at the desk)

| Find | What it does | Where it goes | Period object |
|---|---|---|---|
| Apron | Carry capacity +2 | Apron hook | Heavy canvas twill apron, leather-bound pockets, linen tapes |
| Soft-soled shoes | Move speed +15% | Shoe rack | Mended leather shoes with thick felt soles, made to move silently on library floors |
| Reading glasses | Highlights matching items (Sister Glow) | Glasses stand | Spare round wire-rimmed spectacles on a cord, with a flip-down brass magnifier |
| Shelf list | Lights the right bookcase (Home Beacon) | Shelf-list board | A narrow oak tray of index cards in shelf order, held by a brass rod (a real shelf list is exactly that) |
| Pull list | Gathers matching items to you (Gather Family) | Clipboard | A long slip of ruled paper on a thin wooden board, pencil on a string |
| Dumbwaiter | Quick Shelve (locked until later) | Dumbwaiter hatch | A small hand-hauled lift hatch in the wall with a rope pulley |

The in-game ability names (Sister Glow, Home Beacon, Gather Family) are still prototype names; the concept wants librarian-craft names such as "cataloguer's eye" **[confirm]**.

## 8. Prompt library

Every prompt below starts with the **master style block** from section 6. Paste it first, then the subject text.

### 8.1 Key art: the library from outside (establishing)

> [style anchor] Exterior of a small-town Edwardian library at dusk in autumn, a tall narrow red-brick and sandstone building with an arched oak door and tall wavy-glass windows, a cast-iron gas lamp bracket above the door with its flame not yet lit, a typewritten paper notice pinned to the door (no readable text, just grey lines), fallen autumn leaves on worn stone steps, a bicycle leaning on the railing, cobbles, the town's chimneys and a darkening sky behind, a dim amber glow in the high windows. Wide 16:9, quiet and a little sad, with a sense of waiting.

### 8.2 Mood: the Reading Room, mid-restoration

> [style anchor] Interior of an old Edwardian reading room, dark oak panelling and tall mahogany bookcases with brass nameplates on the shelves, green-shaded brass banker's lamps on long oak tables, gas brackets on the walls with glowing mantles, tall wavy-glass windows, a cast-iron radiator, a wall regulator clock, a wheeled oak ladder, a worn leather armchair, half the shelves neatly full and half empty, a heap of unsorted books at one end of a table, dust in the lamplight. No people. 16:9, eye-level view, hushed and grounded.

### 8.3 Mood: the librarian's desk in the hub (the home base)

> [style anchor] The charging desk at the entrance of a long narrow library corridor, a big worn oak counter with a green-shaded banker's lamp, a wooden date-due stamp and ink pad, a brass bell, an upright typewriter under a cloth cover, a hook on the wall for an apron, a low rack for shoes, a stand for spectacles, a board of index cards, a wooden clipboard on a nail, a small brass-fronted hatch in the plaster wall, oak doors with brass numbers and glass transoms along the corridor, encaustic tile floor with a worn runner rug, dim amber light. No people. 16:9.

Run 8.1 to 8.3, choose the three best, then use them as style references below.

### 8.4 Rufus: the portrait you knew (studio photograph)

**Proposed design [confirm all]:** Rufus, early 60s in this portrait (taken about 1937), head librarian since 1915. Tall and long-limbed with a slight stoop from years leaning to low shelves. Silver-white hair, a bit untidy; thick white eyebrows; a neat white moustache, no beard. Round wire-rimmed spectacles, a pencil behind one ear. A grey wool waistcoat with a watch chain over a collarless shirt with sleeves rolled and held by sleeve garters, and a worn brown knit shawl-collar cardigan over the top; wool trousers held with braces. A ring of brass keys at his belt and a brass lamp-lighting taper tucked in his waistcoat pocket. His look is gentle, amused and a little shy, eyes crinkled, looking just off-camera as if listening. He is warm and quiet, not grand.

> Vintage studio portrait photograph from the late 1930s, soft focus, sepia-toned silver print, painted studio backdrop, slight vignette, small cracks and wear at the edges. Half-length portrait of an elderly librarian: tall, kind face, silver-white untidy hair, thick white eyebrows, neat white moustache, round wire-rimmed spectacles, a pencil behind one ear, a worn knit cardigan over a grey wool waistcoat with a watch chain and a collarless shirt with sleeve garters, a ring of brass keys at his belt. Gentle, amused expression, eyes crinkled, looking slightly off to the side. No text, no caption. 4:5.
>
> Fallback if the faces look wrong: *Pen-and-ink and wash portrait with a sepia photograph tone, same subject, cream paper, vignette frame.*

### 8.5 Rufus: the later portrait (he was ill)

> Vintage amateur snapshot photograph from the mid-1940s, slightly faded sepia print, a soft deckle edge. Same man as before: tall elderly librarian, now thinner and frailer, silver-white hair soft and thin, white moustache, round wire-rimmed spectacles, a scarf and the same worn cardigan loose on him, a pencil behind his ear, a thin tired smile. He sits in a worn leather armchair beside a small lamp, a book open on his knee. Tender and quiet, no hospital imagery, no dramatics. No text. 4:5.

(One reference portrait on the desk or in the staff room can be the first image; the second belongs to the last note near the end, if the story uses it **[confirm]**.)

### 8.6 Rufus: character reference sheet (for any later 2D or 3D work)

> Character reference sheet, three views (front, three-quarter, side) of the same elderly man, full body, neutral standing pose, plain cream background, muted gouache and ink illustration: tall, slight stoop, silver-white untidy hair, white moustache, round wire-rimmed spectacles, brown knit shawl-collar cardigan over a grey wool waistcoat with a watch chain, collarless shirt with sleeve garters, wool trousers with braces, worn leather shoes with thick soft soles, a ring of brass keys at the belt, a pencil behind one ear. No text, no labels.

Keep Rufus off screen in the first-person game; he is present in objects, handwriting, portraits and the one or two illustrations. This sheet is for consistency only.

### 8.7 Rufus's handwriting (not an image)

Rufus's notes and the paper tags on finds are **text set in a handwriting font**, not generated. Pick one handwriting face with an open licence (SIL OFL; verify the licence for the chosen face) in a **1920s schoolroom cursive or copperplate hand** (a dip-pen or fountain-pen look; no ballpoint-style marker handwriting) and use it for every note, tag and margin scribble. Ink: dark brown-black on yellowed cream, a little uneven.

### 8.8 The letter and the notice (props)

> Flat top-down view of an old cream envelope addressed in faded ink (no readable writing), a simple postmark smudge and a faded stamp-shaped square, a red wax seal and a thin string, on a worn wooden table, dim lamplight, muted gouache and ink, no text.

> Flat top-down view of a municipal paper notice pinned to a wooden door: yellowed cream card with a typewritten look, a purple rubber-stamp header band, a ruled checklist of four empty tick-boxes, slightly curled corners, a rusty pin, no readable text (just grey lines), dim light.

The game draws the checklist wording and the ticks.

### 8.9 Find: the apron

> Product illustration on a plain cream background, dim lamplight: a heavy, much-mended canvas twill librarian's apron in faded indigo, many patch pockets of different sizes with leather binding, brass buttons, linen tapes, worn at the hem, a small paper tag tied to a button with string, hanging from a cast-iron hook. Muted gouache and ink, no text on the tag.

### 8.10 Find: the soft-soled shoes

> Product illustration on a plain cream background: a pair of well-worn brown leather shoes with thick pale felt soles, freshly resoled, laces knotted, a paper tag tied to the laces with string, on a small oak shoe rack. Muted gouache and ink, no text.

### 8.11 Find: the reading glasses

> Product illustration on a plain cream background: a spare pair of round wire-rimmed reading glasses on a thin cord, a small flip-down brass magnifier clipped to one lens, resting on a closed cloth-bound book, a paper tag tied to the cord, a faint warm glint of lamplight on the glass. Muted gouache and ink, no magical glow, no text.

### 8.12 Find: the shelf list

> Product illustration on a plain cream background: a narrow oak card-catalogue tray with a brass retaining rod and a brass pull, filled with yellowed ruled index cards (no readable writing), one card standing slightly proud, a paper tag tied to the pull. Muted gouache and ink, no text.

### 8.13 Find: the pull list

> Product illustration on a plain cream background: a worn thin wooden board holding a long hand-ruled slip of paper under a spring clip, a pencil on a string, small tick marks down the margin (no readable writing), a paper tag tied to the clip. Muted gouache and ink, no text.

### 8.14 Find: the dumbwaiter hatch

> Illustration of a small wooden hatch built into a plaster wall: brass hinges, a brass keyhole plate, a short hemp rope pulley beside it, the little door closed and slightly warped with age, paint chipped bottle-green, dim lamplight. Muted gouache and ink, no text.

**What makes a find look like a find (applies to all six):** a small paper tag tied on with string (the game prints "For you" or a short note on it in Rufus's handwriting) and a faint warm glint of lamplight on brass or glass. That is the only visual signal. **No magical glow or sparkles**, because these are ordinary tools that Rufus left for you. The in-game "find easy to spot" highlight is backlog L-012.

### 8.15 Items in the library: cover and spine surfaces (Channel A)

Flat, neutral, tintable swatches. Ask for light cloth with dark ink, in the pattern only; **the title and letters are added by the game**.

> Flat front-on texture swatch, even lighting, no shadows, no perspective, seamless tileable: a fine woven bookcloth with a subtle linen weave and a faint worn sheen, light warm grey, very low contrast, no text, no objects, no border. 1:1.

> Flat front-on design for a book front board, even lighting, light cream bookcloth with a blind-stamped ornamental frame: a double-ruled border with decorative corner flourishes and an empty central rectangular cartouche, dark ink only, no text anywhere, no shadows, no perspective. 4:5.

> Flat front-on spine design, narrow vertical strip, light cream bookcloth with blind-stamped bands at the top and bottom, a blank oval plate at the top for an icon and a blank rectangular plate at the bottom for a number, a long blank rule in the middle for a title, dark ink only, no text, no shadows. 1:4.

Series emblems (small engraved icons for the plates at the top of each spine; ask for one at a time):

> Single small copperplate engraved emblem on a plain cream background: [a gas lamp with a small flame / a card-catalogue drawer with a pull handle / a dip pen and sealed letter / a compass rose / a needle and thread with a bone folder / a key and keyhole plate], bold simple silhouette, readable at 64 pixels, dark ink only, no text. 1:1.

### 8.16 Items: periodicals, letters, parcels (the Mailroom)

> Flat top-down illustration on a plain cream background, three pieces: a stack of old illustrated gazettes with plain engraved covers and a blank masthead band, a small bundle of cream letters tied with string, and a brown-paper parcel tied with twine and a red wax seal. Dim warm light, muted gouache and ink, no readable text.

### 8.17 Items: atlases, rolled maps, flat sheets (the Map Room)

> Flat top-down illustration on a plain cream background: a large cloth-bound atlas with leather corners, three rolled linen-backed maps tied with tape, and a flat sheet map with a faint grid and a compass rose (no readable place names). Cool blue-grey tones with a warm lamp glint, muted gouache and ink, no text.

### 8.18 Furniture reference: the bookcase (to brief Blender work)

> Orthographic front elevation of a tall carved mahogany library bookcase with a stepped plinth, reeded pilasters, thick shelf boards with a nosing, an arched crest with a small brass rosette, a stepped cornice with a row of small dentil blocks, and a small brass nameplate on the front of every shelf, plain cream background, ink and wash, no books, no text.

(The built meshes in `tools/blender/` already match this; the prompt is for variations or other rooms.)

### 8.19 Textures for rooms (Channel A)

> Seamless tileable texture, flat front-on, even lighting: [dark mahogany wood grain / yellowed plaster with faint hairline cracks and a chipped bottle-green painted dado / worn encaustic floor tiles in a small repeating black-ochre-cream pattern / oak herringbone parquet worn pale in the middle / worn dark-green linoleum with a faint pattern / dull old brass], low contrast, no text, no objects. 1:1.

Check every texture for visible seams by tiling it 3 x 3 before use (file 26).

### 8.20 The ending

> [style anchor] Close view of two hands, one holding a ring of old brass keys out toward the viewer, the other holding a long brass lamp-lighting taper with a small flame, a gas bracket on a library wall behind them just beginning to glow, a window beyond showing a few dim lights of the town at night, gentle and hopeful. No faces, no text.

### 8.21 Handbook pages and title cards (Channel C)

> Single page of an old handwritten librarian's handbook, yellowed ruled paper with a faint stain and a pinhole, a small copperplate engraved diagram at the top (hands holding a book spine up to a lamp), ruled lines for the rest of the page left blank, a pencil note in the margin as scribbled lines (no readable text). 4:5.

The game sets all page text in the handwriting font; the illustration is the only AI element.

## 9. Using the outputs

| Output | Where it goes | Must be done by hand |
|---|---|---|
| Mood and key art | Docs/art reference, store-page *concepts*, menu backgrounds | Crop and paint over; log it. Check it matches the game before it appears on a store page |
| Portraits | Framed on the desk or wall as flat quads; the letter | Add no text; frame in-engine; reject any face with AI tells |
| Find illustrations | Reference for 3D models and UI icons | Model in Blender; the in-engine find is a 3D prop with a paper tag |
| Cover and spine textures | `tools/covers/`, then `Assets/_Project/Art/Textures/Books/` | Run through the cover pipeline; the game draws the text; keep light and tintable |
| Surface textures | Room materials | Seam check; normal and smoothness maps made separately (file 26) |

**Keep proxy art separable:** put every AI file under a `Proxy` folder per channel (for example `Assets/_Project/Art/Proxy/Textures/`) and the images in `Docs/art/proxy/`, with the **same filenames, aspect ratios and pixel sizes as the final art will have**, so replacing a file replaces the art. (Folder names are a proposal; creating folders under `Assets/` is a normal change that needs no approval.)

## 10. Provenance and disclosure

Keep [asset-provenance.csv](asset-provenance.csv) up to date: one row per image kept. Fields (from file 26): file path, asset type, tool and model, date, plan or tier, full prompt and negative prompt, seed and settings, reference images used (with their own licence), what a human changed, and the Steam disclosure decision. Before shipping, re-read Valve's live AI-disclosure page (backlog T-009); at release, anything AI-made that ships needs to be on the store page.

## 11. Decisions for Kolby

1. **Rufus:** keep this design, or change anything (age, hair, moustache, cardigan and waistcoat, spectacles)? He is entirely invented; confirm before any portrait is used.
2. **Is the year (1947) ever printed in the game,** or does it just say "after the war"? Keeping it unprinted leaves room for a period detail to be slightly off.
3. **Portrait medium:** period photograph (more grounded, riskier with AI faces) or ink-and-wash with a sepia tone (safer).
4. **A war trace:** none, or one faded blackout strip on a window?
5. **Does the first-person player see their own hands and sleeves?** If so, which (the apron, a plain cuffed wool sleeve)?
6. **Do patrons get portraits,** or only notes? (Open since the concept draft: D-003.)
7. **Ability names:** rename Sister Glow, Home Beacon and Gather Family to librarian-craft names before UI art is drawn.
8. **Doc 13:** update the Mailroom's "1960s annex" and the other period wording in the concept to match.
9. **Later wings:** the concept's wings 4 to 7 predate the Rufus story; confirm names before generating their mood boards.
