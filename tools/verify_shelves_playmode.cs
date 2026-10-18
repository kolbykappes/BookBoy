// Play Mode integration check for bookcases/shelves/crosshair (run with: unity command eval_file tools/verify_shelves_playmode.cs while playing).
// Exercises claims + grading through ShelfSlot.Accept, cleanup counting, and the PlayerInteractor aim/crosshair logic via reflection.
// Everything it changes is scene state, which the Editor restores on leaving Play Mode - verify that afterwards.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";

var sb = new System.Text.StringBuilder();
int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }

var R = BookBoy.Rules.PlacementResult.Correct;
var cats = new[] { BookCategory.Alchemy, BookCategory.Astronomy, BookCategory.Beasts, BookCategory.History };
Book B(BookCategory c, int series, int vol) { return UnityEngine.GameObject.Find("Book_" + c + "_" + (series == 0 ? "A" : "B") + vol).GetComponent<Book>(); }
Bookcase Case(int i) { foreach (var bc in UnityEngine.Object.FindObjectsByType<Bookcase>(UnityEngine.FindObjectsInactive.Include)) if ((int)bc.theme == i) return bc; return null; }   // the bookcase whose fixed theme is category i
ShelfSlot Slot(int bc, int shelf, int k) { return Case(bc).shelves[shelf].slots[k]; }
BookBoy.Rules.PlacementResult Put(Book b, ShelfSlot s) { if (b.currentSlot != null) b.currentSlot.Release(); return s.Accept(b, b.transform.localScale); }
void ClearAll() { foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) if (b.currentSlot != null) b.currentSlot.Release(); }
string G(Book b) { return b.Grade().ToString(); }

// ---- fixed themes and wall signs ----
var allCases = UnityEngine.Object.FindObjectsByType<Bookcase>(UnityEngine.FindObjectsInactive.Include);
var allSigns = UnityEngine.Object.FindObjectsByType<ThemeSign>(UnityEngine.FindObjectsInactive.Include);
var themeSet = new System.Collections.Generic.HashSet<int>(); foreach (var bc in allCases) themeSet.Add((int)bc.theme);
Check("16 bookcases hold 16 distinct themes and there is one sign per bookcase", allCases.Length == 16 && themeSet.Count == 16 && allSigns.Length == 16, allCases.Length + " cases, " + themeSet.Count + " themes, " + allSigns.Length + " signs");
bool signsOk = true; string signBad = "";
foreach (var sg in allSigns) {
  var txt = sg.GetComponentInChildren<UnityEngine.UI.Text>().text; var lines = BookThemes.SignLines(sg.bookcase.theme);
  if (sg.theme != sg.bookcase.theme || txt != lines[0] + "\n" + lines[1]) { signsOk = false; signBad += " " + sg.name; }
  if (System.Math.Abs(sg.transform.position.x - sg.bookcase.transform.position.x) > 0.01f || sg.transform.position.y <= 2.0f) { signsOk = false; signBad += " " + sg.name + "(pos)"; }
}
Check("every sign is above its own bookcase and names that bookcase's theme", signsOk, signBad);
var colorWords = new[] { "amber", "indigo", "green", "crimson", "teal", "violet", "rose", "rust", "olive", "azure", "brown", "slate", "ivory", "charcoal", "lemon", "mint", "white", "red", "blue", "yellow", "orange", "purple", "pink", "grey", "gray", "black" };
bool noColor = true; string leaked = "";
foreach (var sg in allSigns) foreach (var cw in colorWords) if (sg.GetComponentInChildren<UnityEngine.UI.Text>().text.ToLowerInvariant().Contains(cw)) { noColor = false; leaked += " " + sg.name + ":" + cw; }
foreach (var bk in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) foreach (var cw in colorWords) if (bk.definition.displayName.ToLowerInvariant().Contains(cw)) { noColor = false; leaked += " " + bk.name + ":" + cw; }
Check("signs and book titles name the theme, never a color", noColor, leaked);
Check("the white books are 'Healing' (title) and their bookcase says HEALING MAGIC", B(BookCategory.Light, 0, 1).definition.displayName.StartsWith("Healing") && BookThemes.ThemeName(BookCategory.Light) == "Healing Magic" && Case((int)BookCategory.Light).theme == BookCategory.Light);

// ---- claims and the three grades ----
var first = Put(B(BookCategory.Alchemy, 0, 1), Slot(0, 0, 0));
Check("first book (amber A1) in an empty case, slot 1: Correct", first == R, first.ToString());
var wrongSlot = Put(B(BookCategory.Alchemy, 0, 3), Slot(0, 0, 1));
Check("amber A3 in slot 2 of the same shelf: yellow (right shelf, wrong slot)", wrongSlot == BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot, wrongSlot.ToString());
var wrongSeries = Put(B(BookCategory.Alchemy, 1, 2), Slot(0, 0, 2));
Check("amber B2 on the shelf already holding series A: yellow (wrong shelf)", wrongSeries == BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot, wrongSeries.ToString());
var wrongColor = Put(B(BookCategory.Astronomy, 0, 1), Slot(0, 1, 0));
Check("indigo book in the amber bookcase: red", wrongColor == BookBoy.Rules.PlacementResult.WrongBookcase, wrongColor.ToString());
var otherShelf = Put(B(BookCategory.Alchemy, 1, 1), Slot(0, 1, 0));
Check("amber B1 on the case's empty second shelf claims it for series B: Correct", otherShelf == R, otherShelf.ToString());
var dupShelf = Put(B(BookCategory.Alchemy, 0, 4), Slot(0, 1, 3));
Check("amber A4 on the shelf claimed by series B: yellow", dupShelf == BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot, dupShelf.ToString());

var idigo = Put(B(BookCategory.Astronomy, 0, 1), Slot(1, 0, 0));
Check("indigo A1 in the (empty) Astral bookcase: Correct", idigo == R, idigo.ToString());
var dupCase = Put(B(BookCategory.Astronomy, 0, 2), Slot(2, 0, 1));
Check("an indigo book in the (empty) Beast bookcase: red - bookcase themes are fixed, no claiming", dupCase == BookBoy.Rules.PlacementResult.WrongBookcase, dupCase.ToString());
var otherBookInCase2 = Put(B(BookCategory.Astronomy, 0, 3), Slot(1, 0, 2));
Check("indigo A3 in slot 3 of the Astral bookcase: Correct", otherBookInCase2 == R, otherBookInCase2.ToString());

// regrade is live: removing the earliest book moves the claim
var a1 = B(BookCategory.Alchemy, 0, 1); var a3 = B(BookCategory.Alchemy, 0, 3);
a1.currentSlot.Release();
Check("removing the earliest book hands the shelf claim to the next one (A3 still wrong slot)", G(a3) == "RightBookcaseWrongSpot", G(a3));
Check("a released book is no longer shelved", !a1.isPlaced && a1.Grade() == BookBoy.Rules.PlacementResult.WrongBookcase);
Check("flash colors: red / yellow / green",
    Book.ColorFor(BookBoy.Rules.PlacementResult.WrongBookcase).g < 0.3f && Book.ColorFor(BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot).g > 0.8f && Book.ColorFor(BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot).r > 0.9f && Book.ColorFor(R).g > 0.9f && Book.ColorFor(R).r < 0.4f);

// ---- full solutions: the intended one, and a different one (any color on any case, any series on any shelf) ----
int Solve(System.Func<int, int> caseOfColor, System.Func<int, int> shelfOfSeries)
{
    ClearAll();
    int correct = 0;
    for (int ci = 0; ci < 4; ci++) for (int series = 0; series < 2; series++) for (int vol = 1; vol <= 4; vol++)
    {
        var bk = B(cats[ci], series, vol);
        Put(bk, Slot(caseOfColor(ci), shelfOfSeries(series), vol - 1));
    }
    foreach (var bk in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) if (bk.IsCorrectlyShelved) correct++;
    return correct;
}
int c1 = Solve(ci => ci, s => s);
Check("32 of 32 correct for the straightforward arrangement", c1 == 32, c1 + "/32");

// render the solved arrangement so the result can be eyeballed
UnityEngine.GameObject camGo = null; UnityEngine.RenderTexture rtex = null;
try {
    camGo = new UnityEngine.GameObject("TMP_SHOTCAM"); var cam = camGo.AddComponent<UnityEngine.Camera>(); cam.nearClipPlane = 0.05f; cam.fieldOfView = 55f;
    rtex = new UnityEngine.RenderTexture(1280, 720, 24); cam.targetTexture = rtex;
    camGo.transform.position = new UnityEngine.Vector3(0f, 1.55f, 3.6f); camGo.transform.LookAt(new UnityEngine.Vector3(0f, 1.0f, 5.6f)); cam.Render();
    UnityEngine.RenderTexture.active = rtex;
    var tex = new UnityEngine.Texture2D(1280, 720, UnityEngine.TextureFormat.RGB24, false); tex.ReadPixels(new UnityEngine.Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
    System.IO.File.WriteAllBytes(System.Environment.GetEnvironmentVariable("TEMP") + "/solved.png", tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex); UnityEngine.RenderTexture.active = null;
} finally { if (camGo) UnityEngine.Object.DestroyImmediate(camGo); if (rtex) { rtex.Release(); UnityEngine.Object.DestroyImmediate(rtex); } }

int c2 = Solve(ci => ci, s => 1 - s);
Check("32 of 32 correct with the two series swapped between the shelves (the first book on a shelf decides its series)", c2 == 32, c2 + "/32");

// one swap breaks it
var bkSwap1 = B(BookCategory.Alchemy, 0, 2); var bkSwap2 = B(BookCategory.Alchemy, 0, 3);
var slotA = bkSwap1.currentSlot; var slotB = bkSwap2.currentSlot;
slotA.Release(); slotB.Release(); slotA.Accept(bkSwap2, bkSwap2.transform.localScale); slotB.Accept(bkSwap1, bkSwap1.transform.localScale);
int c3 = 0; foreach (var bk in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) if (bk.IsCorrectlyShelved) c3++;
Check("swapping two volumes on a shelf leaves exactly 30 correct", c3 == 30, c3 + "/32");
ClearAll();

// ---- aim logic, crosshair, highlight (private members via reflection) ----
var pi = UnityEngine.Object.FindFirstObjectByType<PlayerInteractor>();
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance; var tPi = typeof(PlayerInteractor);
var fSlot = tPi.GetField("_focusedSlot", flags); var fBook = tPi.GetField("_focusedBook", flags);
var mFocus = tPi.GetMethod("UpdateFocus", flags); var mVisuals = tPi.GetMethod("UpdateFocusVisuals", flags); var mPrompt = tPi.GetMethod("UpdatePrompt", flags);
var cam2 = pi.interactionCamera.transform;
void Aim(UnityEngine.Vector3 from, UnityEngine.Vector3 target) {
    cam2.position = from; cam2.rotation = UnityEngine.Quaternion.LookRotation((target - from).normalized);
    UnityEngine.Physics.SyncTransforms(); mFocus.Invoke(pi, null); mVisuals.Invoke(pi, null);
}
float DotSize() { return pi.crosshair.dot.rectTransform.sizeDelta.x; }

var held = B(BookCategory.History, 1, 4);
pi.Carry.SetCapacity(6); foreach (var cb in new System.Collections.Generic.List<Book>(pi.Carry.Items)) pi.Carry.Release(cb, out _); pi.PickUp(held);
foreach (var side in new[] { 0f, 0.4f, -0.4f })
{
    int ok = 0, total = 0; string bad = "";
    for (int ci = 0; ci < 4; ci++) for (int sh = 0; sh < 2; sh++) for (int k = 0; k < 4; k++)
    {
        var slot = Slot(ci, sh, k); var p = slot.transform.position;
        Aim(new UnityEngine.Vector3(p.x + side, 1.55f, 3.2f), p + new UnityEngine.Vector3(0f, 0.05f, 0f));
        var f = (ShelfSlot)fSlot.GetValue(pi); total++;
        if (f == slot) ok++; else bad += " [" + ci + "," + sh + "," + k + "->" + (f ? f.shelf.bookcase.name + "/" + f.shelf.name + "/" + f.slotIndex : "none") + "]";
    }
    Check("aiming at each of 32 slots from " + (side == 0f ? "straight on" : "the side (" + side + "m)") + " picks the aimed slot", ok == total, ok + "/" + total + bad);
}

/* a bookcase no earlier step filled (released books stay where they were) */ var tgt = Slot(5, 1, 2); var tp = tgt.transform.position;
Aim(new UnityEngine.Vector3(tp.x, 1.55f, 3.2f), tp + new UnityEngine.Vector3(0f, 0.05f, 0f));
mPrompt.Invoke(pi, null);
Check("holding + aiming at an empty slot: place prompt, highlight bar on that slot only, active crosshair",
    pi.promptHintText.text.Contains("click to place") && tgt.highlight.enabled && DotSize() == 18f, pi.promptHintText.text + " hl=" + tgt.highlight.enabled + " dot=" + DotSize());
int litCount = 0; foreach (var s in UnityEngine.Object.FindObjectsByType<ShelfSlot>(UnityEngine.FindObjectsInactive.Include)) if (s.highlight.enabled) litCount++;
Check("exactly one slot highlighted", litCount == 1, litCount.ToString());

var other = B(BookCategory.History, 0, 1); Put(other, tgt);
Aim(new UnityEngine.Vector3(tp.x, 1.55f, 3.2f), tp + new UnityEngine.Vector3(0f, 0.05f, 0f));
mPrompt.Invoke(pi, null);
Check("carrying + aiming at a taken slot (with room in the hands): offers to pick the shelved book up, no placement highlight, active crosshair", pi.promptHintText.text.Contains("pick up") && !tgt.highlight.enabled && DotSize() == 18f && pi.crosshair.dot.color.g > 0.7f, pi.promptHintText.text + " dot=" + DotSize() + " " + pi.crosshair.dot.color);

foreach (var cb in new System.Collections.Generic.List<Book>(pi.Carry.Items)) pi.Carry.Release(cb, out _);
Aim(new UnityEngine.Vector3(tp.x, 1.55f, 3.2f), tp + new UnityEngine.Vector3(0f, other.transform.localScale.y * 0.5f, 0f));
Check("empty-handed aim at a shelved book focuses that book (pick it back up)", (Book)fBook.GetValue(pi) == other, ((Book)fBook.GetValue(pi)) ? ((Book)fBook.GetValue(pi)).name : "null");
ClearAll();

// crosshair: idle on a wall, active on a book in reach, far on a book out of reach
var loose = B(BookCategory.Beasts, 0, 1);
loose.transform.position = new UnityEngine.Vector3(-2f, loose.transform.localScale.y * 0.5f, 3f);   // a truly loose book on the open floor, clear of the player capsule and the cases
loose.transform.rotation = UnityEngine.Quaternion.identity; UnityEngine.Physics.SyncTransforms();
var lp = loose.transform.position;
Aim(new UnityEngine.Vector3(lp.x, 1.2f, lp.z - 1.5f), lp);
Check("crosshair on a loose book in reach is active and the book is focused", DotSize() == 18f && (Book)fBook.GetValue(pi) == loose, "dot=" + DotSize());
Aim(new UnityEngine.Vector3(lp.x, 1.2f, lp.z - 6f), lp);
Check("crosshair on a loose book out of reach (6m) shows 'far', not focused", DotSize() == 12f && (Book)fBook.GetValue(pi) == null, "dot=" + DotSize());
Aim(new UnityEngine.Vector3(0f, 1.5f, 0f), new UnityEngine.Vector3(7f, 3f, 0.2f));
Check("crosshair on empty space/wall is the small idle dot", DotSize() == 10f, "dot=" + DotSize());
loose.gameObject.SetActive(false); loose.gameObject.SetActive(true);   // clear any stale glow state
Aim(new UnityEngine.Vector3(lp.x, 1.2f, lp.z - 1.5f), lp);
bool glowOn = loose.Outline != null && loose.Outline.IsSteady;
UnityEngine.Color bodyWhileFocused; { var m = new UnityEngine.MaterialPropertyBlock(); loose.GetComponent<UnityEngine.Renderer>().GetPropertyBlock(m); bodyWhileFocused = m.GetColor("_BaseColor"); }
Aim(new UnityEngine.Vector3(0f, 1.5f, 0f), new UnityEngine.Vector3(7f, 3f, 0.2f));
bool glowOff = !loose.Outline.IsSteady;
Check("a focused book gets an outline glow (its body color untouched), and it clears when the crosshair leaves",
    glowOn && glowOff && UnityEngine.Mathf.Abs(bodyWhileFocused.g - loose.definition.spineColor.g) < 0.01f,
    "on=" + glowOn + " off=" + glowOff + " body=" + bodyWhileFocused + " spine=" + loose.definition.spineColor);

sb.Append("frame=" + UnityEngine.Time.frameCount + " isPlaying=" + UnityEditor.EditorApplication.isPlaying + "\n");
sb.Append(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
return sb.ToString();
