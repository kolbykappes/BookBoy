// GENERATED from build_library.src.cs by tools/qualify.js - edit the .src.cs, not this file.
// Re-runnable. Edit Mode only. Leaves the scene dirty: save it afterwards and verify the file on disk.
if (UnityEditor.EditorApplication.isPlaying) return "REFUSED: stop Play Mode first";

var log = new System.Text.StringBuilder();
var cats = new[] { BookCategory.Alchemy, BookCategory.Astronomy, BookCategory.Beasts, BookCategory.History };
var colorWord = new System.Collections.Generic.Dictionary<BookCategory, string> { { BookCategory.Alchemy, "Amber" }, { BookCategory.Astronomy, "Indigo" }, { BookCategory.Beasts, "Green" }, { BookCategory.History, "Crimson" } };
var seriesWord = new[] { "Tome", "Codex" };
const int seriesCount = 2;
const int volumes = 4;

// ---- 0. remove everything this script (or the previous layout) generated ----
foreach (var name in new[] { "ShelfBack", "ShelfBoard_1", "ShelfBoard_2", "ShelfBoard_3", "ShelfBoard_4" })
{
    var old = UnityEngine.GameObject.Find(name);
    if (old != null) UnityEngine.Object.DestroyImmediate(old);
}
foreach (var sh in UnityEngine.Object.FindObjectsByType<Shelf>(UnityEngine.FindObjectsInactive.Include))
    if (sh != null && sh.transform.parent == null) UnityEngine.Object.DestroyImmediate(sh.gameObject);
foreach (var old in UnityEngine.Object.FindObjectsByType<ShelfSlot>(UnityEngine.FindObjectsInactive.Include))
    if (old != null && old.transform.parent == null) UnityEngine.Object.DestroyImmediate(old.gameObject);
foreach (var bc in UnityEngine.Object.FindObjectsByType<Bookcase>(UnityEngine.FindObjectsInactive.Include))
    if (bc != null) UnityEngine.Object.DestroyImmediate(bc.gameObject);

// ---- 1. Player capsule must not cast a shadow ----
var cap = UnityEngine.GameObject.Find("Player/Capsule");
if (cap != null)
{
    cap.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    log.Append("capsule shadow off; ");
}

// ---- materials (plain colors only) ----
UnityEngine.Material EnsureMat(string path, string shaderName, UnityEngine.Color color)
{
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null)
    {
        m = new UnityEngine.Material(Shader.Find(shaderName));
        UnityEditor.AssetDatabase.CreateAsset(m, path);
    }
    m.SetColor("_BaseColor", color);
    UnityEditor.EditorUtility.SetDirty(m);
    return m;
}
var woodMat = EnsureMat("Assets/_Project/Art/Materials/Mat_Wood.mat", "Universal Render Pipeline/Lit", new UnityEngine.Color(0.42f, 0.25f, 0.12f));
woodMat.SetFloat("_Smoothness", 0.2f);
var highlightMat = EnsureMat("Assets/_Project/Art/Materials/Mat_SlotHighlight.mat", "Universal Render Pipeline/Unlit", new UnityEngine.Color(1f, 0.9f, 0.3f));
var digitMat = EnsureMat("Assets/_Project/Art/Materials/Mat_Digit.mat", "Universal Render Pipeline/Unlit", UnityEngine.Color.white);

// ---- 2. Book definitions: 2 series x 4 volumes per color (existing assets mutated in place, never recreated) ----
const string dir = "Assets/_Project/ScriptableObjects";
string DefPath(BookCategory c, int series, int vol)
{
    if (series == 0) return vol == 1 ? dir + "/Book_" + c + "_Sample.asset" : dir + "/Book_" + c + "_Sample_0" + vol + ".asset";
    return dir + "/Book_" + c + "_SeriesB_0" + vol + ".asset";
}
string Key(BookCategory c, int series, int vol) { return c + ":" + series + ":" + vol; }
var baseColor = new System.Collections.Generic.Dictionary<BookCategory, UnityEngine.Color>();
foreach (var c in cats) baseColor[c] = UnityEditor.AssetDatabase.LoadAssetAtPath<BookDefinition>(DefPath(c, 0, 1)).spineColor;
UnityEngine.Color SeriesColor(BookCategory c, int series)
{
    // spineColor of series 0 is the base; the second series is a clearly lighter shade of the same color
    var b = baseColor[c];
    return series == 0 ? b : UnityEngine.Color.Lerp(b, UnityEngine.Color.white, 0.4f);
}
var defs = new System.Collections.Generic.Dictionary<string, BookDefinition>();
foreach (var c in cats)
{
    for (int series = 0; series < seriesCount; series++)
    {
        for (int vol = 1; vol <= volumes; vol++)
        {
            var d = UnityEditor.AssetDatabase.LoadAssetAtPath<BookDefinition>(DefPath(c, series, vol));
            if (d == null)
            {
                d = UnityEngine.ScriptableObject.CreateInstance<BookDefinition>();
                UnityEditor.AssetDatabase.CreateAsset(d, DefPath(c, series, vol));
            }
            d.bookId = series == 0 ? "bk_" + c.ToString().ToLower() + "_0" + vol : "bk_" + c.ToString().ToLower() + "_b" + vol;
            d.displayName = colorWord[c] + " " + seriesWord[series] + " Vol. " + vol;
            d.category = c;
            d.series = series;
            d.volume = vol;
            d.spineColor = SeriesColor(c, series);
            UnityEditor.EditorUtility.SetDirty(d);
            defs[Key(c, series, vol)] = d;
        }
    }
}
UnityEditor.AssetDatabase.SaveAssets();
log.Append("defs=" + defs.Count + "; ");

// ---- PixelDigit helper (depth-tested, one-sided; legacy UnityEngine.TextMesh renders through geometry) ----
UnityEngine.Color TextColorFor(UnityEngine.Color bg) { return (0.2126f * bg.r + 0.7152f * bg.g + 0.0722f * bg.b) > 0.35f ? new UnityEngine.Color(0.1f, 0.07f, 0.04f) : UnityEngine.Color.white; }
// counterScale must cancel the parent's non-uniform scale in the child's axes.
UnityEngine.GameObject MakeLabel(UnityEngine.Transform parentT, string name, int digit, UnityEngine.Vector3 localPos, UnityEngine.Quaternion localRot, float worldHeight, UnityEngine.Color color, UnityEngine.Vector3 counterScale)
{
    var go = new UnityEngine.GameObject(name);
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
var scaleA = new System.Collections.Generic.Dictionary<BookCategory, UnityEngine.Vector3> {
    { BookCategory.Alchemy, new UnityEngine.Vector3(0.045f, 0.24f, 0.16f) }, { BookCategory.Astronomy, new UnityEngine.Vector3(0.04f, 0.28f, 0.18f) },
    { BookCategory.Beasts, new UnityEngine.Vector3(0.05f, 0.32f, 0.15f) }, { BookCategory.History, new UnityEngine.Vector3(0.055f, 0.22f, 0.19f) } };
var scaleB = new System.Collections.Generic.Dictionary<BookCategory, UnityEngine.Vector3> {
    { BookCategory.Alchemy, new UnityEngine.Vector3(0.055f, 0.29f, 0.17f) }, { BookCategory.Astronomy, new UnityEngine.Vector3(0.05f, 0.24f, 0.17f) },
    { BookCategory.Beasts, new UnityEngine.Vector3(0.045f, 0.27f, 0.17f) }, { BookCategory.History, new UnityEngine.Vector3(0.05f, 0.30f, 0.18f) } };

var booksRoot = UnityEngine.GameObject.Find("ScatteredBooks").transform;
var books = new System.Collections.Generic.List<Book>();
foreach (var c in cats)
{
    Book template = null;
    foreach (var b in booksRoot.GetComponentsInChildren<Book>(true))
        if (template == null && b.definition != null && b.definition.category == c && b.definition.series == 0) template = b;

    for (int series = 0; series < seriesCount; series++)
    {
        for (int vol = 1; vol <= volumes; vol++)
        {
            var def = defs[Key(c, series, vol)];
            Book book = null;
            foreach (var b in booksRoot.GetComponentsInChildren<Book>(true))
                if (book == null && b.definition == def) book = b;
            if (book == null)
            {
                var go = UnityEngine.Object.Instantiate(template.gameObject, booksRoot);
                book = go.GetComponent<Book>();
                book.definition = def;
            }
            book.name = "Book_" + c + "_" + (series == 0 ? "A" : "B") + vol;
            book.transform.localScale = series == 0 ? scaleA[c] : scaleB[c];
            book.currentSlot = null;
            book.placedTime = 0;
            book.ApplyColor();
            books.Add(book);

            for (int i = book.transform.childCount - 1; i >= 0; i--)
            {
                var ch = book.transform.GetChild(i);
                if (ch.name.StartsWith("Label_")) UnityEngine.Object.DestroyImmediate(ch.gameObject);
            }
            var ps = book.transform.lossyScale;
            var spineScale = new UnityEngine.Vector3(1f / ps.x, 1f / ps.y, 1f / ps.z);
            // small number near the bottom of the spine, like a real library series
            MakeLabel(book.transform, "Label_Spine", vol, new UnityEngine.Vector3(0f, -0.36f, -0.5f - 0.02f), UnityEngine.Quaternion.identity, 0.03f, TextColorFor(def.spineColor), spineScale);
        }
    }
}

var rng = new System.Random(7);
var xs = new[] { -3.5f, -2.5f, -1.5f, -0.5f, 0.5f, 1.5f, 2.5f, 3.5f };
var zs = new[] { 1.0f, 1.9f, 2.8f, 3.7f };
var cells = new System.Collections.Generic.List<UnityEngine.Vector2>();
foreach (var z in zs) foreach (var x in xs) cells.Add(new UnityEngine.Vector2(x, z));
for (int i = cells.Count - 1; i > 0; i--)
{
    int j = rng.Next(i + 1);
    var tmp = cells[i]; cells[i] = cells[j]; cells[j] = tmp;
}
for (int i = 0; i < books.Count; i++)
{
    var b = books[i];
    float jx = (float)(rng.NextDouble() - 0.5) * 0.5f, jz = (float)(rng.NextDouble() - 0.5) * 0.5f;
    b.transform.SetPositionAndRotation(new UnityEngine.Vector3(cells[i].x + jx, b.transform.localScale.y * 0.5f, cells[i].y + jz), UnityEngine.Quaternion.Euler(0f, rng.Next(0, 360), 0f));
}
log.Append("books=" + books.Count + "; ");

// ---- 4. Four bookcases, two shelves each, four packed slots per shelf ----
const float W = 0.44f, D = 0.36f, H = 1.79f;
const float slotPitch = 0.072f;           // widest book (0.055) + a little; slots are barely wider than a book
float[] shelfTops = { 0.85f, 1.30f };

UnityEngine.GameObject Part(string name, UnityEngine.Transform parentT, UnityEngine.Vector3 localPos, UnityEngine.Vector3 size)
{
    var p = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
    p.name = name;
    p.transform.SetParent(parentT, false);
    p.transform.localPosition = localPos;
    p.transform.localScale = size;
    p.GetComponent<UnityEngine.MeshRenderer>().sharedMaterial = woodMat;
    return p;
}

var cases = new System.Collections.Generic.List<Bookcase>();
for (int i = 0; i < 4; i++)
{
    var root = new UnityEngine.GameObject("Bookcase_" + (i + 1));
    root.transform.position = new UnityEngine.Vector3((i - 1.5f) * 0.5f, 0f, 5.67f);
    var bookcase = root.AddComponent<Bookcase>();
    cases.Add(bookcase);

    // plain wooden frame: base cabinet, two side panels, back, two shelf boards, top
    Part("Base", root.transform, new UnityEngine.Vector3(0f, 0.425f, 0f), new UnityEngine.Vector3(W, 0.85f, D));
    float upperH = H - 0.85f;
    Part("SideL", root.transform, new UnityEngine.Vector3(-(W / 2f - 0.02f), 0.85f + upperH / 2f, 0f), new UnityEngine.Vector3(0.04f, upperH, D));
    Part("SideR", root.transform, new UnityEngine.Vector3(W / 2f - 0.02f, 0.85f + upperH / 2f, 0f), new UnityEngine.Vector3(0.04f, upperH, D));
    Part("Back", root.transform, new UnityEngine.Vector3(0f, 0.85f + upperH / 2f, D / 2f - 0.01f), new UnityEngine.Vector3(W - 0.08f, upperH, 0.02f));
    Part("ShelfBoard", root.transform, new UnityEngine.Vector3(0f, shelfTops[1] - 0.025f, 0f), new UnityEngine.Vector3(W - 0.08f, 0.05f, D - 0.02f));
    Part("Top", root.transform, new UnityEngine.Vector3(0f, H - 0.02f, 0f), new UnityEngine.Vector3(W, 0.04f, D));

    var shelfList = new System.Collections.Generic.List<Shelf>();
    for (int s = 0; s < 2; s++)
    {
        var shelfGo = new UnityEngine.GameObject("Shelf_" + (s + 1));
        shelfGo.transform.SetParent(root.transform, false);
        shelfGo.transform.localPosition = new UnityEngine.Vector3(0f, shelfTops[s], 0f);
        var shelf = shelfGo.AddComponent<Shelf>();
        shelf.bookcase = bookcase;
        var box = shelfGo.AddComponent<UnityEngine.BoxCollider>();
        box.isTrigger = true;
        box.center = new UnityEngine.Vector3(0f, 0.2f, 0f);
        box.size = new UnityEngine.Vector3(W - 0.08f, 0.40f, D - 0.02f);

        var slotList = new ShelfSlot[volumes];
        for (int k = 0; k < volumes; k++)
        {
            var slotGo = new UnityEngine.GameObject("Slot_" + (k + 1));
            slotGo.transform.SetParent(shelfGo.transform, false);
            slotGo.transform.localPosition = new UnityEngine.Vector3((k - (volumes - 1) / 2f) * slotPitch, 0f, 0.02f);
            var slot = slotGo.AddComponent<ShelfSlot>();
            slot.shelf = shelf;
            slot.slotIndex = k;

            var bar = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
            bar.name = "Highlight";
            UnityEngine.Object.DestroyImmediate(bar.GetComponent<UnityEngine.Collider>());
            bar.transform.SetParent(slotGo.transform, false);
            bar.transform.localPosition = new UnityEngine.Vector3(0f, 0.002f, 0f);
            bar.transform.localScale = new UnityEngine.Vector3(slotPitch - 0.008f, 0.004f, 0.2f);
            var mr = bar.GetComponent<UnityEngine.MeshRenderer>();
            mr.sharedMaterial = highlightMat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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
var canvas = UnityEngine.GameObject.Find("HUD_Canvas");
var oldCross = canvas.transform.Find("Crosshair");
if (oldCross != null) UnityEngine.Object.DestroyImmediate(oldCross.gameObject);
var crossGo = new UnityEngine.GameObject("Crosshair", typeof(UnityEngine.RectTransform));
crossGo.transform.SetParent(canvas.transform, false);
var rt = crossGo.GetComponent<UnityEngine.RectTransform>();
rt.anchorMin = new UnityEngine.Vector2(0.5f, 0.5f);
rt.anchorMax = new UnityEngine.Vector2(0.5f, 0.5f);
rt.pivot = new UnityEngine.Vector2(0.5f, 0.5f);
rt.anchoredPosition = UnityEngine.Vector2.zero;
rt.sizeDelta = new UnityEngine.Vector2(10f, 10f);
var img = crossGo.AddComponent<UnityEngine.UI.Image>();
img.sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<UnityEngine.Sprite>("UI/Skin/Knob.psd");
img.raycastTarget = false;
img.color = new UnityEngine.Color(1f, 1f, 1f, 0.9f);
var outline = crossGo.AddComponent<UnityEngine.UI.Outline>();
outline.effectColor = new UnityEngine.Color(0f, 0f, 0f, 0.7f);
outline.effectDistance = new UnityEngine.Vector2(1.5f, -1.5f);
var crosshair = crossGo.AddComponent<Crosshair>();
crosshair.dot = img;
var pi = UnityEngine.Object.FindFirstObjectByType<PlayerInteractor>();
pi.crosshair = crosshair;
log.Append("crosshair wired=" + (pi.crosshair != null) + "; ");

UnityEditor.EditorUtility.SetDirty(woodMat);
UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return log.ToString();
