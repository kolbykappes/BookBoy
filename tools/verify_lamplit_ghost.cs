// Play Mode check of the placement ghost (L-022), the claim plates (L-023) and the pooled, two-submesh glow (T-013). Run with Lamplit.unity playing.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var zone = cc.Room(1);
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry; var cam = pi.interactionCamera.transform;
Book B(char g, int n) { return UnityEngine.GameObject.Find("R1_" + g + n).GetComponent<Book>(); }
var rows8 = new System.Collections.Generic.List<Shelf>(); var rows4 = new System.Collections.Generic.List<Shelf>();
foreach (var s in zone.shelves) (s.slots.Length == 8 ? rows8 : rows4).Add(s);
void ClearAll() { foreach (var b in zone.items) if (b.currentSlot != null) b.currentSlot.Release(); }
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var piT = typeof(PlayerInteractor);
void Aim(ShelfSlot slot)
{
    var p = slot.transform.position; var from = p - slot.transform.forward * 1.3f; from.y = 1.65f;
    cam.position = from; cam.rotation = UnityEngine.Quaternion.LookRotation((p + UnityEngine.Vector3.up * 0.15f - from).normalized);
    UnityEngine.Physics.SyncTransforms();
    piT.GetMethod("UpdateFocus", flags).Invoke(pi, null);
    piT.GetMethod("UpdateFocusVisuals", flags).Invoke(pi, null);
}

// ---- the preview grade equals the real grade after placing ----
ClearAll();
var cases = new (Book, ShelfSlot)[] {
    (B('A', 1), rows8[0].slots[2]), (B('A', 2), rows8[0].slots[5]), (B('C', 1), rows8[0].slots[1]), (B('C', 1), rows8[1].slots[3]),
    (B('B', 1), rows8[2].slots[0]), (B('B', 1), rows4[0].slots[0]), (B('A', 3), rows8[2].slots[1]), (B('D', 1), rows4[0].slots[2]),
    (B('E', 1), rows8[3].slots[0]), (B('E', 2), rows8[3].slots[7]), (B('G', 1), rows8[3].slots[4]) };
int agree = 0; string bad = "";
foreach (var (bk, sl) in cases)
{
    if (bk.currentSlot != null) bk.currentSlot.Release();
    var pre = bk.PreviewGrade(sl); sl.Accept(bk, bk.transform.localScale); var act = bk.Grade();
    if (pre == act) agree++; else bad += " " + bk.name + "@" + sl.shelf.name + ":" + pre + "!=" + act;
}
Check("the preview grade equals the grade after really placing, in " + cases.Length + " mixed placements (empty row, own set, other set, wrong size, claimed elsewhere)", agree == cases.Length, bad);

// ---- the ghost ----
ClearAll();
var ghost = pl.GetComponent<SlotGhost>();
Check("the player has a SlotGhost with the ghost material", ghost != null && ghost.template != null);
var held = B('A', 1);
Check("pick a book up", pi.PickUp(held) && carry.Current == held);
var slot1 = rows8[3].slots[3]; Aim(slot1);
Check("aiming at an empty slot with a book in hand shows the ghost", ghost.IsShown);
var scale = carry.RealScale(held);
var expected = slot1.transform.position + UnityEngine.Vector3.up * (scale.y * 0.5f) + slot1.transform.forward * (scale.z * 0.5f);
Check("the ghost stands exactly where the book would be placed", (ghost.GhostTransform.position - expected).magnitude < 0.002f && (ghost.GhostTransform.localScale - scale).magnitude < 0.002f, ghost.GhostTransform.position + " vs " + expected);
Check("an empty 8-row and an 8-set: the ghost is green", ghost.GradeIndex == 0, ghost.GradeIndex.ToString());
Check("the thin highlight bar is off while the ghost shows", slot1.highlight != null && !slot1.highlight.enabled);
var slotWrongSize = rows4[2].slots[1]; Aim(slotWrongSize);
Check("a set of 8 aimed at a 4-slot row: the ghost is yellow", ghost.IsShown && ghost.GradeIndex == 1, ghost.GradeIndex.ToString());
Check("the ghost holds every submesh of the book (A1 is a printed book: boards, pages and spine)", ghost.GhostTransform.GetComponent<UnityEngine.MeshRenderer>().sharedMaterials.Length == held.GetComponent<UnityEngine.MeshRenderer>().sharedMaterials.Length && held.GetComponent<UnityEngine.MeshRenderer>().sharedMaterials.Length == 3);
Aim(slot1); pi.PlaceCurrent(slot1);
var other = B('C', 1); pi.PickUp(other); var slotSameShelf = rows8[3].slots[6]; Aim(slotSameShelf);
Check("after set A claims a shelf, a different set aimed at it: the ghost is yellow", ghost.IsShown && ghost.GradeIndex == 1, ghost.GradeIndex.ToString());
cam.rotation = UnityEngine.Quaternion.Euler(-80f, 0f, 0f); UnityEngine.Physics.SyncTransforms(); piT.GetMethod("UpdateFocus", flags).Invoke(pi, null); piT.GetMethod("UpdateFocusVisuals", flags).Invoke(pi, null);
Check("aiming away from every slot hides the ghost", !ghost.IsShown);
pi.PlaceCurrent(slotSameShelf);   // put it down somewhere so the hands are empty

// ---- the claim plates ----
ClearAll();
var shelfP = rows8[1]; var plate = shelfP.GetComponent<ClaimPlate>();
Check("every shelf of a mesh case has a claim plate", System.Linq.Enumerable.All(zone.shelves, sh => sh.GetComponent<ClaimPlate>() != null));
var upd = typeof(ClaimPlate).GetMethod("Update", flags); var nextF = typeof(ClaimPlate).GetField("_next", flags);
void Tick() { nextF.SetValue(plate, 0f); upd.Invoke(plate, null); }
Tick();
Check("an empty shelf has a blank plate", !plate.letter.gameObject.activeSelf && !plate.icon.gameObject.activeSelf);
var claimant = B('E', 3); shelfP.slots[2].Accept(claimant, claimant.transform.localScale); Tick();
Check("once a set claims the shelf its letter and icon appear on the plate", plate.letter.gameObject.activeSelf && plate.letter.value == PixelDigit.LetterValue('E') && plate.icon.gameObject.activeSelf && plate.icon.value == PixelDigit.IconValue(claimant.definition.iconIndex), plate.letter.value + " / " + plate.icon.value);
claimant.currentSlot.Release(); Tick();
Check("emptying the shelf blanks the plate again", !plate.letter.gameObject.activeSelf);

// ---- the glow: pooled light and sparks, covers both submeshes ----
ClearAll();
int Count(string n, bool onlyEnabled) { int c = 0; foreach (var l in UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Light>()) if (l.name == n && (!onlyEnabled || l.enabled)) c++; return c; }
int before = Count("GlowLight", false);
var group = new System.Collections.Generic.List<Book>(); for (int n = 1; n <= 8; n++) group.Add(B('K', n));   // a set nothing has touched (a book just placed is still flashing, which outranks a pulse)
var lateT = typeof(BookOutline).GetMethod("LateUpdate", flags);
foreach (var bk in group) bk.Outline.Pulse(UnityEngine.Color.yellow, 5f);
foreach (var bk in group) lateT.Invoke(bk.Outline, null);
int during = Count("GlowLight", true);
Check("no glow lights exist until a book glows, then 8 pulsing books use 8 pooled lights", before == 0 && during == 8, before + " before, " + during + " in use");
var hull = group[0].transform.Find("GlowInner");
Check("the glow hull covers both submeshes (the page block glows too)", hull != null && hull.GetComponent<UnityEngine.MeshRenderer>().sharedMaterials.Length == 2);
foreach (var bk in group) bk.Outline.StopAll();
Check("when the glows end every light goes back to the pool (disabled)", Count("GlowLight", true) == 0, Count("GlowLight", true).ToString());
sb.Append(fails == 0 ? "ALL PASSED" : fails + " FAILED");
return sb.ToString();
