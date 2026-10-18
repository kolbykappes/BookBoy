// Play Mode check for multi-carry, cycling, capacity, pickup-while-carrying, and the start of a toss (run while playing).
// Part 2 (tools/verify_toss_settle.cs) is run a few seconds later to confirm the tossed book settled.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }

Book B(string n) { return UnityEngine.GameObject.Find(n).GetComponent<Book>(); }
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
var fl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance; var tPi = typeof(PlayerInteractor);
var fBook = tPi.GetField("_focusedBook", fl); var mFocus = tPi.GetMethod("UpdateFocus", fl); var mVis = tPi.GetMethod("UpdateFocusVisuals", fl); var mPrompt = tPi.GetMethod("UpdatePrompt", fl);
var cam = pi.interactionCamera.transform;
void AimAt(UnityEngine.Vector3 target) { cam.position = new UnityEngine.Vector3(target.x, 1.55f, 3.2f); cam.rotation = UnityEngine.Quaternion.LookRotation((target - cam.position).normalized); UnityEngine.Physics.SyncTransforms(); mFocus.Invoke(pi, null); mVis.Invoke(pi, null); mPrompt.Invoke(pi, null); }

// reset: nothing held, everything off the shelves
foreach (var b in UnityEngine.Object.FindObjectsByType<Book>(UnityEngine.FindObjectsInactive.Include)) { if (b.currentSlot != null) b.currentSlot.Release(); if (carry.Items.Contains(b)) carry.Release(b, out _); }
carry.SetCapacity(4);
Check("starts with capacity 4 and empty hands", carry.Capacity == 4 && carry.IsEmpty);

var names = new[] { "Book_Alchemy_A1", "Book_Alchemy_A2", "Book_Astronomy_A1", "Book_Beasts_A1", "Book_History_A1" };
var scales = new UnityEngine.Vector3[5]; for (int i = 0; i < 5; i++) scales[i] = B(names[i]).transform.localScale;
int got = 0; for (int i = 0; i < 4; i++) if (pi.PickUp(B(names[i]))) got++;
Check("can pick up 4 books", got == 4 && carry.Count == 4 && carry.IsFull, "got=" + got);
Check("a 5th pickup is refused when full", !pi.PickUp(B(names[4])) && carry.Count == 4);
Check("the most recently picked up book is current", carry.Current == B(names[3]));
carry.Cycle(1); Check("cycle forward wraps to the first book", carry.Current == B(names[0]));
carry.Cycle(-1); Check("cycle back returns to the last book", carry.Current == B(names[3]));
carry.Cycle(-1); Check("cycle back again", carry.Current == B(names[2]));
Check("held books are parented to the carry point and their colliders are off", B(names[0]).transform.parent == carry.carryPoint && !B(names[0]).GetComponent<UnityEngine.Collider>().enabled);

carry.SetCapacity(5); Check("capacity can grow and a 5th book then fits", pi.PickUp(B(names[4])) && carry.Count == 5, "count=" + carry.Count);
carry.SetCapacity(4);   // 5 carried, capacity 4: nothing is dropped, but nothing more can be picked up

Bookcase CaseOfTheme(BookCategory t) { foreach (var bc in UnityEngine.Object.FindObjectsByType<Bookcase>(UnityEngine.FindObjectsInactive.Include)) if (bc.theme == t) return bc; return null; }
// place the current book on a shelf slot
var slot = CaseOfTheme(BookCategory.Alchemy).shelves[0].slots[0];
while (carry.Current != B("Book_Alchemy_A1")) carry.Cycle(1);
bool placed = pi.PlaceCurrent(slot);
Check("placing the current book puts it on the slot and out of the hands", placed && slot.occupant == B("Book_Alchemy_A1") && carry.Count == 4 && !carry.Items.Contains(B("Book_Alchemy_A1")));
Check("a placed book gets its real size back (not the carried size)", (B("Book_Alchemy_A1").transform.localScale - scales[0]).magnitude < 0.0005f, B("Book_Alchemy_A1").transform.localScale + " vs " + scales[0]);
Check("a placed book is graded (Alchemy vol 1 in the Alchemy bookcase, slot 1 = Correct)", B("Book_Alchemy_A1").Grade() == BookBoy.Rules.PlacementResult.Correct);
Check("cannot place into a taken slot", !pi.PlaceCurrent(slot));

// a finished (green) book while carrying: the readout moves on to the NEXT book in the hands and the shelved one is left alone
AimAt(slot.transform.position + new UnityEngine.Vector3(0f, 0.1f, 0f));
Check("aiming at the book just placed correctly (green): the readout names the NEXT held book, offers no pick-up, crosshair idle",
    (Book)fBook.GetValue(pi) == null && pi.promptTitleText.enabled && pi.promptTitleText.text == carry.Current.definition.displayName && !pi.promptHintText.text.Contains("pick up") && pi.crosshair.dot.rectTransform.sizeDelta.x < 14f,
    pi.promptTitleText.text + " / " + pi.promptHintText.text + " dot=" + pi.crosshair.dot.rectTransform.sizeDelta.x);
// now make it a wrongly shelved (yellow) book: wrong slot of the right shelf
B("Book_Alchemy_A1").currentSlot.Release();
slot = CaseOfTheme(BookCategory.Alchemy).shelves[0].slots[1];
slot.Accept(B("Book_Alchemy_A1"), scales[0]);
Check("a book on the wrong spot is yellow, not finished", B("Book_Alchemy_A1").Grade() == BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot);

// pickup while carrying: full hands first, then with room
AimAt(slot.transform.position + new UnityEngine.Vector3(0f, 0.1f, 0f));
Check("full hands, aiming at a shelved book: it is focused, the prompt says 'Hands full (4/4)', the crosshair is the blocked red",
    (Book)fBook.GetValue(pi) == B("Book_Alchemy_A1") && pi.promptHintText.text.Contains("Hands full (4/4)") && pi.crosshair.dot.color.g < 0.5f, pi.promptHintText.text + " dot=" + pi.crosshair.dot.color);
Check("full hands: E does not pick it up", !pi.PickUp(B("Book_Alchemy_A1")) && slot.occupant == B("Book_Alchemy_A1"));
carry.SetCapacity(5); AimAt(slot.transform.position + new UnityEngine.Vector3(0f, 0.1f, 0f));
Check("with room, the same book says 'Press (E) or click to pick up' and the crosshair is active", pi.promptHintText.text.Contains("pick up") && pi.crosshair.dot.color.g > 0.7f, pi.promptHintText.text);
Check("picking a shelved book up while carrying frees its slot", pi.PickUp(B("Book_Alchemy_A1")) && !slot.occupied && carry.Count == 5);
carry.SetCapacity(4);

// aiming at an empty slot while carrying -> place target
var empty = CaseOfTheme(BookCategory.Astronomy).shelves[0].slots[2];
AimAt(empty.transform.position + new UnityEngine.Vector3(0f, 0.05f, 0f));
Check("carrying + aiming at an empty slot: 'Press (E) or click to place' for the CURRENT book", pi.promptHintText.text.Contains("click to place") && pi.promptTitleText.text == carry.Current.definition.displayName, pi.promptTitleText.text + " / " + pi.promptHintText.text);

// toss: start of the physics
var tossed = carry.Current; int before = carry.Count;
bool tossedOk = pi.TossCurrent();
var rb = tossed.GetComponent<UnityEngine.Rigidbody>();
Check("Q toss removes the current book from the hands and gives it real physics", tossedOk && carry.Count == before - 1 && rb != null && !rb.isKinematic && rb.linearVelocity.magnitude > 3f && !tossed.GetComponent<UnityEngine.Collider>().isTrigger, "rb=" + (rb != null));
System.AppDomain.CurrentDomain.SetData("tossedName", tossed.name);
sb.Append("tossed " + tossed.name + " from " + tossed.transform.position.ToString("F2") + "; run verify_toss_settle in ~6 s\n");
sb.Append(fails == 0 ? "ALL PASSED (part 1)" : (fails + " FAILED"));
return sb.ToString();
