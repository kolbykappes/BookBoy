// SOURCE for tools/build_library.cs (the eval environment has no `using` directives or LINQ, so a
// qualify step expands short type names). Regenerate with: node tools/qualify.js
// Re-runnable. Edit Mode only. Leaves the scene dirty: save it afterwards and verify the file on disk.
// Builds the library: 16 colors x 2 series x 4 volumes = 128 books, 16 bookcases x 2 shelves x 4 slots = 32 shelves.
// NOTE: this can take longer than the Pipeline's 5 s main-thread limit, so the call may report a timeout while the script
// still finishes: check the scene state (counts) instead of re-running blind.
if (EditorApplication.isPlaying) return "REFUSED: stop Play Mode first";

var log = new System.Text.StringBuilder();
var cats = (BookCategory[])System.Enum.GetValues(typeof(BookCategory));
const int seriesCount = 2;
const int volumes = 4;

var colorWords = new[] { "Amber", "Indigo", "Green", "Crimson", "Teal", "Violet", "Rose", "Rust", "Olive", "Azure", "Brown", "Slate", "Ivory", "Charcoal", "Lemon", "Mint" };
var newColors = new[] {   // colors for categories 4..15 (0..3 keep the colors already in their definition assets)
    new Color(0.05f, 0.50f, 0.52f), new Color(0.50f, 0.22f, 0.68f), new Color(0.82f, 0.36f, 0.56f), new Color(0.78f, 0.34f, 0.10f),
    new Color(0.55f, 0.65f, 0.12f), new Color(0.25f, 0.55f, 0.88f), new Color(0.38f, 0.22f, 0.10f), new Color(0.42f, 0.45f, 0.52f),
    new Color(0.92f, 0.90f, 0.82f), new Color(0.12f, 0.12f, 0.15f), new Color(0.97f, 0.92f, 0.28f), new Color(0.42f, 0.86f, 0.68f) };
var seriesWord = new[] { "Tome", "Codex" };

// ---- 0. remove everything this script (or the previous layouts) generated ----
foreach (var name in new[] { "ShelfBack", "ShelfBoard_1", "ShelfBoard_2", "ShelfBoard_3", "ShelfBoard_4" })
{
    var old = GameObject.Find(name);
    if (old != null) Object.DestroyImmediate(old);
}
foreach (var sh in Object.FindObjectsByType<Shelf>(FindObjectsInactive.Include))
    if (sh != null && sh.transform.parent == null) Object.DestroyImmediate(sh.gameObject);
foreach (var old in Object.FindObjectsByType<ShelfSlot>(FindObjectsInactive.Include))
    if (old != null && old.transform.parent == null) Object.DestroyImmediate(old.gameObject);
foreach (var bc in Object.FindObjectsByType<Bookcase>(FindObjectsInactive.Include))
    if (bc != null) Object.DestroyImmediate(bc.gameObject);

// ---- 1. Player capsule must not cast a shadow ----
var cap = GameObject.Find("Player/Capsule");
if (cap != null)
{
    cap.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
    log.Append("capsule shadow off; ");
}

// ---- materials (plain colors only) ----
Material EnsureMat(string path, string shaderName, Color color)
{
    var m = AssetDatabase.LoadAssetAtPath<Material>(path);
    if (m == null)
    {
        m = new Material(Shader.Find(shaderName));
        AssetDatabase.CreateAsset(m, path);
    }
    m.SetColor("_BaseColor", color);
    EditorUtility.SetDirty(m);
    return m;
}
var woodMat = EnsureMat("Assets/_Project/Art/Materials/Mat_Wood.mat", "Universal Render Pipeline/Lit", new Color(0.42f, 0.25f, 0.12f));
woodMat.SetFloat("_Smoothness", 0.2f);
var highlightMat = EnsureMat("Assets/_Project/Art/Materials/Mat_SlotHighlight.mat", "Universal Render Pipeline/Unlit", new Color(1f, 0.9f, 0.3f));
var digitMat = EnsureMat("Assets/_Project/Art/Materials/Mat_Digit.mat", "Universal Render Pipeline/Unlit", Color.white);

// ---- 2. Book definitions (existing assets are mutated in place, never recreated) ----
const string dir = "Assets/_Project/ScriptableObjects";
string DefPath(int ci, int series, int vol)
{
    string c = cats[ci].ToString();
    if (ci < 4)
    {
        if (series == 0) return vol == 1 ? dir + "/Book_" + c + "_Sample.asset" : dir + "/Book_" + c + "_Sample_0" + vol + ".asset";
        return dir + "/Book_" + c + "_SeriesB_0" + vol + ".asset";
    }
    return dir + "/Book_" + c + (series == 0 ? "_SeriesA_0" : "_SeriesB_0") + vol + ".asset";
}
string Key(int ci, int series, int vol) { return ci + ":" + series + ":" + vol; }
var baseColor = new Color[cats.Length];
for (int ci = 0; ci < cats.Length; ci++)
    baseColor[ci] = ci < 4 ? AssetDatabase.LoadAssetAtPath<BookDefinition>(DefPath(ci, 0, 1)).spineColor : newColors[ci - 4];
Color SeriesColor(int ci, int series)
{
    var b = baseColor[ci];
    if (series == 0) return b;
    if (ci < 4) return Color.Lerp(b, Color.white, 0.4f);                       // the original four keep their lighter second series
    float lum = 0.2126f * b.r + 0.7152f * b.g + 0.0722f * b.b;
    return lum > 0.55f ? Color.Lerp(b, Color.black, 0.4f) : Color.Lerp(b, Color.white, 0.4f);   // light colors get a darker second series
}
var defs = new Dictionary<string, BookDefinition>();
for (int ci = 0; ci < cats.Length; ci++)
{
    for (int series = 0; series < seriesCount; series++)
    {
        for (int vol = 1; vol <= volumes; vol++)
        {
            var d = AssetDatabase.LoadAssetAtPath<BookDefinition>(DefPath(ci, series, vol));
            if (d == null)
            {
                d = ScriptableObject.CreateInstance<BookDefinition>();
                AssetDatabase.CreateAsset(d, DefPath(ci, series, vol));
            }
            string lower = cats[ci].ToString().ToLower();
            d.bookId = ci < 4 && series == 0 ? "bk_" + lower + "_0" + vol : "bk_" + lower + (series == 0 ? "_a" : "_b") + vol;
            d.displayName = colorWords[ci] + " " + seriesWord[series] + " Vol. " + vol;
            d.category = cats[ci];
            d.series = series;
            d.volume = vol;
            d.spineColor = SeriesColor(ci, series);
            EditorUtility.SetDirty(d);
            defs[Key(ci, series, vol)] = d;
        }
    }
}
AssetDatabase.SaveAssets();
log.Append("defs=" + defs.Count + "; ");

// ---- PixelDigit helper (depth-tested, one-sided; legacy TextMesh renders through geometry) ----
Color TextColorFor(Color bg) { return (0.2126f * bg.r + 0.7152f * bg.g + 0.0722f * bg.b) > 0.35f ? new Color(0.1f, 0.07f, 0.04f) : Color.white; }
GameObject MakeLabel(Transform parentT, string name, int digit, Vector3 localPos, Quaternion localRot, float worldHeight, Color color, Vector3 counterScale)
{
    var go = new GameObject(name);
    go.transform.SetParent(parentT, false);
    go.transform.localPosition = localPos;
    go.transform.localRotation = localRot;
    go.transform.localScale = counterScale * worldHeight;
    var pd = go.AddComponent<PixelDigit>();
    pd.value = digit;
    pd.color = color;
    pd.material = digitMat;
    pd.Rebuild();
    return go;
}

// ---- 3. Books: one per (color, series, volume), numbered on the spine, scattered in the room ----
// thickness, height, depth. Series B differs in size from series A; the four sets cycle over the 16 colors.
var scaleA = new[] { new Vector3(0.045f, 0.24f, 0.16f), new Vector3(0.04f, 0.28f, 0.18f), new Vector3(0.05f, 0.32f, 0.15f), new Vector3(0.055f, 0.22f, 0.19f) };
var scaleB = new[] { new Vector3(0.055f, 0.29f, 0.17f), new Vector3(0.05f, 0.24f, 0.17f), new Vector3(0.045f, 0.27f, 0.17f), new Vector3(0.05f, 0.30f, 0.18f) };

var booksRoot = GameObject.Find("ScatteredBooks").transform;
Book template = booksRoot.GetComponentsInChildren<Book>(true)[0];
var books = new List<Book>();
for (int ci = 0; ci < cats.Length; ci++)
{
    for (int series = 0; series < seriesCount; series++)
    {
        for (int vol = 1; vol <= volumes; vol++)
        {
            var def = defs[Key(ci, series, vol)];
            Book book = null;
            foreach (var b in booksRoot.GetComponentsInChildren<Book>(true))
                if (book == null && b.definition == def) book = b;
            if (book == null)
            {
                var go = Object.Instantiate(template.gameObject, booksRoot);
                book = go.GetComponent<Book>();
                book.definition = def;
            }
            book.name = "Book_" + cats[ci] + "_" + (series == 0 ? "A" : "B") + vol;
            book.transform.localScale = series == 0 ? scaleA[ci % 4] : scaleB[ci % 4];
            book.currentSlot = null;
            book.placedTime = 0;
            book.ApplyColor();
            books.Add(book);

            for (int i = book.transform.childCount - 1; i >= 0; i--)
            {
                var ch = book.transform.GetChild(i);
                if (ch.name.StartsWith("Label_")) Object.DestroyImmediate(ch.gameObject);
            }
            var ps = book.transform.lossyScale;
            var spineScale = new Vector3(1f / ps.x, 1f / ps.y, 1f / ps.z);
            // small number near the bottom of the spine, like a real library series
            MakeLabel(book.transform, "Label_Spine", vol, new Vector3(0f, -0.36f, -0.5f - 0.02f), Quaternion.identity, 0.03f, TextColorFor(def.spineColor), spineScale);
        }
    }
}

// scatter on a jittered grid across the room (clear of the spawn point at the origin), deterministic shuffle
var rng = new System.Random(7);
var cells = new List<Vector2>();
var zRows = new[] { -4f, -3f, -2f, -1f, 1f, 2f, 3f, 4f };
for (int zi = 0; zi < zRows.Length; zi++)
    for (int xi = 0; xi < 16; xi++) cells.Add(new Vector2(-6f + 0.8f * xi, zRows[zi]));
for (int i = cells.Count - 1; i > 0; i--)
{
    int j = rng.Next(i + 1);
    var tmp = cells[i]; cells[i] = cells[j]; cells[j] = tmp;
}
for (int i = 0; i < books.Count; i++)
{
    var b = books[i];
    float jx = (float)(rng.NextDouble() - 0.5) * 0.5f, jz = (float)(rng.NextDouble() - 0.5) * 0.5f;
    b.transform.SetPositionAndRotation(new Vector3(cells[i].x + jx, b.transform.localScale.y * 0.5f, cells[i].y + jz), Quaternion.Euler(0f, rng.Next(0, 360), 0f));
}
log.Append("books=" + books.Count + "; ");

// ---- 4. Sixteen bookcases, two shelves each, four packed slots per shelf ----
const float W = 0.44f, D = 0.36f, H = 1.79f;
const float slotPitch = 0.072f;           // widest book (0.055) + a little; slots are barely wider than a book
const float frontLine = -0.14f;           // local z of the books' front (spine) face; the shelf's front edge is at -0.17
float[] shelfTops = { 0.85f, 1.30f };

GameObject Part(string name, Transform parentT, Vector3 localPos, Vector3 size)
{
    var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
    p.name = name;
    p.transform.SetParent(parentT, false);
    p.transform.localPosition = localPos;
    p.transform.localScale = size;
    p.GetComponent<MeshRenderer>().sharedMaterial = woodMat;
    return p;
}

var cases = new List<Bookcase>();
for (int i = 0; i < cats.Length; i++)
{
    var root = new GameObject("Bookcase_" + (i + 1));
    root.transform.position = new Vector3((i - (cats.Length - 1) / 2f) * 0.5f, 0f, 5.67f);
    var bookcase = root.AddComponent<Bookcase>();
    cases.Add(bookcase);

    // plain wooden frame: base cabinet, two side panels, back, a shelf board, top
    Part("Base", root.transform, new Vector3(0f, 0.425f, 0f), new Vector3(W, 0.85f, D));
    float upperH = H - 0.85f;
    Part("SideL", root.transform, new Vector3(-(W / 2f - 0.02f), 0.85f + upperH / 2f, 0f), new Vector3(0.04f, upperH, D));
    Part("SideR", root.transform, new Vector3(W / 2f - 0.02f, 0.85f + upperH / 2f, 0f), new Vector3(0.04f, upperH, D));
    Part("Back", root.transform, new Vector3(0f, 0.85f + upperH / 2f, D / 2f - 0.01f), new Vector3(W - 0.08f, upperH, 0.02f));
    Part("ShelfBoard", root.transform, new Vector3(0f, shelfTops[1] - 0.025f, 0f), new Vector3(W - 0.08f, 0.05f, D - 0.02f));
    Part("Top", root.transform, new Vector3(0f, H - 0.02f, 0f), new Vector3(W, 0.04f, D));

    var shelfList = new List<Shelf>();
    for (int s = 0; s < 2; s++)
    {
        var shelfGo = new GameObject("Shelf_" + (s + 1));
        shelfGo.transform.SetParent(root.transform, false);
        shelfGo.transform.localPosition = new Vector3(0f, shelfTops[s], 0f);
        var shelf = shelfGo.AddComponent<Shelf>();
        shelf.bookcase = bookcase;
        var box = shelfGo.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, 0.2f, 0f);
        box.size = new Vector3(W - 0.08f, 0.40f, D - 0.02f);

        var slotList = new ShelfSlot[volumes];
        for (int k = 0; k < volumes; k++)
        {
            var slotGo = new GameObject("Slot_" + (k + 1));
            slotGo.transform.SetParent(shelfGo.transform, false);
            slotGo.transform.localPosition = new Vector3((k - (volumes - 1) / 2f) * slotPitch, 0f, frontLine);
            var slot = slotGo.AddComponent<ShelfSlot>();
            slot.shelf = shelf;
            slot.slotIndex = k;

            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "Highlight";
            Object.DestroyImmediate(bar.GetComponent<Collider>());
            bar.transform.SetParent(slotGo.transform, false);
            bar.transform.localPosition = new Vector3(0f, 0.002f, 0.1f);
            bar.transform.localScale = new Vector3(slotPitch - 0.008f, 0.004f, 0.2f);
            var mr = bar.GetComponent<MeshRenderer>();
            mr.sharedMaterial = highlightMat;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.enabled = false;
            slot.highlight = mr;
            slotList[k] = slot;
        }
        shelf.slots = slotList;
        shelfList.Add(shelf);
    }
    bookcase.shelves = shelfList.ToArray();
}
foreach (var bc in cases) bc.all = cases.ToArray();
log.Append("bookcases=" + cases.Count + "; ");

// ---- 5. Crosshair dot in the middle of the screen ----
var canvas = GameObject.Find("HUD_Canvas");
var oldCross = canvas.transform.Find("Crosshair");
if (oldCross != null) Object.DestroyImmediate(oldCross.gameObject);
var crossGo = new GameObject("Crosshair", typeof(RectTransform));
crossGo.transform.SetParent(canvas.transform, false);
var rt = crossGo.GetComponent<RectTransform>();
rt.anchorMin = new Vector2(0.5f, 0.5f);
rt.anchorMax = new Vector2(0.5f, 0.5f);
rt.pivot = new Vector2(0.5f, 0.5f);
rt.anchoredPosition = Vector2.zero;
rt.sizeDelta = new Vector2(10f, 10f);
var img = crossGo.AddComponent<UnityEngine.UI.Image>();
img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
img.raycastTarget = false;
img.color = new Color(1f, 1f, 1f, 0.9f);
var outline = crossGo.AddComponent<UnityEngine.UI.Outline>();
outline.effectColor = new Color(0f, 0f, 0f, 0.7f);
outline.effectDistance = new Vector2(1.5f, -1.5f);
var crosshair = crossGo.AddComponent<Crosshair>();
crosshair.dot = img;
var pi = Object.FindFirstObjectByType<PlayerInteractor>();
pi.crosshair = crosshair;
log.Append("crosshair wired=" + (pi.crosshair != null) + "; ");

EditorUtility.SetDirty(woodMat);
AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return log.ToString();
