// Play Mode renders of the hover outline and the placement ghost (docs/unity-practices/29). Frames freeze when the Editor is unfocused, so the glow's LateUpdate is called by hand.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var late = typeof(BookOutline).GetMethod("LateUpdate", flags);
void Focus(Book b, bool on) { b.SetFocused(on); late.Invoke(b.Outline, null); }
var zone = UnityEngine.GameObject.Find("Room1").GetComponent<RoomZone>();
// 1: a book on the cream floor and one on a dark floor tile
var loose = new System.Collections.Generic.List<Book>(); foreach (var b in zone.items) if (b.currentSlot == null) loose.Add(b);
Book floorBook = loose[0]; float best = -1f;
foreach (var c in loose) { float m = 99f; foreach (var o in loose) if (o != c) m = System.Math.Min(m, (o.transform.position - c.transform.position).magnitude); if (m > best && zone.Contains(c.transform.position)) { best = m; floorBook = c; } }
var p = floorBook.transform.position;
Focus(floorBook, true);
sb.AppendLine("floor book at " + p + " " + floorBook.name);
sb.AppendLine(LamplitBuilder.Shot(d + "O_floor.png", p + V(0.0f, 0.9f, -0.9f), p, 58.7f, 1920, 1080));
Focus(floorBook, false);
// 2: a shelved book between neighbours
zone.PlaceAllHome(); UnityEngine.Physics.SyncTransforms(); foreach (var b in zone.items) { if (b.Outline != null) b.Outline.StopAll(); }
var e3 = UnityEngine.GameObject.Find("R1_E3").GetComponent<Book>(); Focus(e3, true);
var q = e3.transform.position;
sb.AppendLine(LamplitBuilder.Shot(d + "O_shelf.png", V(-7.2f, 1.65f, 2.06f), V(-8.7f, 1.3f, 2.06f), 58.7f, 1920, 1080));
sb.AppendLine(LamplitBuilder.Shot(d + "O_shelf_close.png", q + V(1.0f, 0.1f, 0f), q, 40f, 1920, 1080));
Focus(e3, false);
return sb.ToString();
