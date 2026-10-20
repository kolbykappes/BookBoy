// Play Mode renders of the hologram placement ghost (docs/unity-practices/29).
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
var zone = UnityEngine.GameObject.Find("Room1").GetComponent<RoomZone>();
zone.PlaceAllHome(); UnityEngine.Physics.SyncTransforms();
foreach (var b in zone.items) { if (b.Outline != null) b.Outline.StopAll(); }
var e3 = UnityEngine.GameObject.Find("R1_E3").GetComponent<Book>(); var slot = e3.currentSlot; slot.Release();
e3.transform.position = e3.transform.position + UnityEngine.Vector3.up * 5f;
var ghost = UnityEngine.GameObject.Find("Player").GetComponent<SlotGhost>();
var scale = new UnityEngine.Vector3(0.1016f, 0.34f, 0.254f);
ghost.Show(e3, slot, BookBoy.Rules.PlacementResult.Correct, scale);
var q = slot.transform.position + V(0f, 0.17f, 0f);
sb.AppendLine(LamplitBuilder.Shot(d + "G2_green_close.png", q + V(1.1f, 0.1f, 0f), q, 40f, 1920, 1080));
sb.AppendLine(LamplitBuilder.Shot(d + "G2_green_4ft.png", V(-7.2f, 1.65f, 2.06f), V(-8.7f, 1.3f, 2.06f), 58.7f, 1920, 1080));
ghost.Show(e3, slot, BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot, scale);
sb.AppendLine(LamplitBuilder.Shot(d + "G2_yellow_close.png", q + V(1.1f, 0.1f, 0f), q, 40f, 1920, 1080));
ghost.Show(e3, slot, BookBoy.Rules.PlacementResult.WrongBookcase, scale);
sb.AppendLine(LamplitBuilder.Shot(d + "G2_red_close.png", q + V(1.1f, 0.1f, 0f), q, 40f, 1920, 1080));
ghost.Hide();
return sb.ToString();
