// Play Mode check of the score effects: the mote flying to the ITEMS counter, the counter pop, and the finished-shelf celebration. Run with Lamplit.unity playing.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var sb = new System.Text.StringBuilder(); int fails = 0;
void Check(string name, bool ok, string detail = "") { if (!ok) fails++; sb.Append((ok ? "PASS " : "FAIL ") + name + (ok ? "" : "  <-- " + detail) + "\n"); }
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var zone = cc.Room(1); cc.JumpTo(1);
var ui = UnityEngine.GameObject.Find("Game").GetComponent<GameUI>(); var fx = ui.GetComponent<ScoreFx>();
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>(); var carry = pi.Carry;
var fl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
Book B(char g, int n) { return UnityEngine.GameObject.Find("R1_" + g + n).GetComponent<Book>(); }
foreach (var b in zone.items) if (b.currentSlot != null) b.currentSlot.Release();
while (carry.Count > 0) { var c0 = carry.Current; UnityEngine.Vector3 sc0; carry.Release(c0, out sc0); }
cc.Recompute();
var rows8 = new System.Collections.Generic.List<Shelf>(); foreach (var sh in zone.shelves) if (sh.slots.Length == 8) rows8.Add(sh);
var row = rows8[0];
void Put(Book b, ShelfSlot s) { if (b.currentSlot != null) b.currentSlot.Release(); b.transform.position = new UnityEngine.Vector3(-6f, 0.1f, 3f); s.Accept(b, b.transform.localScale); }

Check("the score effects exist on the game object", fx != null);
// ---- a correct placement: a mote flies to the ITEMS counter and the counter pops when it arrives ----
int arrivals0 = fx.Arrivals; int items0 = cc.RoomCorrect(1);
var a1 = B('A', 1);
Put(a1, row.slots[0]);
Check("placing volume 1 in slot 1 grades green and the room counter is already up (an immediate recount)", a1.IsCorrectlyShelved && cc.RoomCorrect(1) == items0 + 1, cc.RoomCorrect(1) + " vs " + (items0 + 1));
Check("a mote is in flight to the counter", fx.MotesInFlight >= 1, fx.MotesInFlight.ToString());
UnityEngine.Vector2 target; bool haveTarget = ui.TryGetStatTarget(1, out target);
Check("the counter's target point is on screen at the top left", haveTarget && target.x > 100f && target.x < 600f && target.y > 800f, target.ToString());
// run the clock past the flight by hand (frames are frozen while the Editor is unfocused)
var upd = typeof(ScoreFx).GetMethod("Update", fl);
fx.Skip(1.5f); upd.Invoke(fx, null);
Check("the mote arrived and punched the counter", fx.Arrivals == arrivals0 + 1 && ui.StatPunch > 0.3f, fx.Arrivals + " arrivals, punch " + ui.StatPunch);
// a wrong placement sends nothing
int inFlight = fx.MotesInFlight; var c1 = B('C', 1); Put(c1, row.slots[6]);
Check("a placement that is not green sends no mote", fx.MotesInFlight == inFlight);
c1.currentSlot.Release();

// ---- finishing a row ----
for (int n = 2; n <= 7; n++) Put(B('A', n), row.slots[n - 1]);
Check("seven of eight in place: the row is not complete and nothing celebrated", !row.IsComplete && fx.ShelvesCelebrated == 0 && !row.celebrated);
var a8 = B('A', 8);
int flightBefore = fx.MotesInFlight;
Put(a8, row.slots[7]);
Check("the eighth volume completes the row: it celebrates once", row.IsComplete && row.celebrated && fx.ShelvesCelebrated == 1);
var flourish = ShelfFlourish.Active.ToArray();
Check("a ShelfFlourish is playing with a gold sheet over the row and a wave over all 8 books", flourish.Length == 1 && flourish[0].IsPlaying && flourish[0].Sheet != null && flourish[0].BookCount == 8);
if (flourish.Length == 0) { sb.Append("FAIL no celebration to inspect"); return sb.ToString(); }
var f = flourish[0];
var sheetBounds = f.Sheet.bounds;
Check("the sheet covers the whole row (about 8 books wide, a book tall)", sheetBounds.size.magnitude > 0.8f && sheetBounds.size.magnitude < 2.5f, sheetBounds.size.ToString());
Check("a bigger gem mote is on its way to the SHELVES counter, after the wave", fx.MotesInFlight >= flightBefore + 2, fx.MotesInFlight + " vs " + flightBefore);
// the wave: at 0.3 s the first books are up, the last are not yet
var rest = new System.Collections.Generic.List<float>(); foreach (var s in row.slots) rest.Add(s.occupant.transform.position.y);
f.Tick(0.3f);
float up0 = row.slots[0].occupant.transform.position.y - rest[0], up7 = row.slots[7].occupant.transform.position.y - rest[7];
Check("the books pop up in a wave: at 0.3 s book 1 is up, book 8 has not started", up0 > 0.01f && up7 < 0.001f, up0 + " / " + up7);
f.Tick(0.75f);
float up7b = row.slots[7].occupant.transform.position.y - rest[7];
Check("by 0.75 s the wave has reached book 8", up7b > 0.005f, up7b.ToString());
f.Tick(2.5f);
Check("when it ends every book is back exactly where it was (no drift)", System.Math.Abs(row.slots[0].occupant.transform.position.y - rest[0]) < 0.0005f && System.Math.Abs(row.slots[7].occupant.transform.position.y - rest[7]) < 0.0005f);
fx.Skip(2f); upd.Invoke(fx, null);
Check("the gem arrived and the stats panel popped", fx.Arrivals >= arrivals0 + 2 && fx.MotesInFlight == 0, fx.Arrivals + " arrivals");
foreach (var o in ShelfFlourish.Active.ToArray()) UnityEngine.Object.DestroyImmediate(o.gameObject);
// pick one out and put it back: a new celebration
a8.currentSlot.Release();
Check("taking a book out clears the celebrated flag", !row.celebrated);
row.slots[7].Accept(a8, a8.transform.localScale);
Check("putting it back celebrates again", fx.ShelvesCelebrated == 2);
foreach (var o in ShelfFlourish.Active.ToArray()) UnityEngine.Object.DestroyImmediate(o.gameObject);
fx.Skip(2f); upd.Invoke(fx, null);

// ---- admin tidy-up is quiet ----
int cel = fx.ShelvesCelebrated; int fl0 = fx.MotesInFlight;
zone.PlaceAllHome();
Check("tidying the whole room by admin command sends no motes and celebrates nothing", fx.ShelvesCelebrated == cel && fx.MotesInFlight == fl0, fx.ShelvesCelebrated + " / " + fx.MotesInFlight);
foreach (var o in ShelfFlourish.Active.ToArray()) UnityEngine.Object.DestroyImmediate(o.gameObject);
sb.Append(fails == 0 ? "ALL PASSED" : fails + " FAILED");
return sb.ToString();
