var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var sb = new System.Text.StringBuilder();
var zone = UnityEngine.GameObject.Find("Room1").GetComponent<RoomZone>();
zone.PlaceAllHome(); UnityEngine.Physics.SyncTransforms();
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
foreach (var pl in UnityEngine.Object.FindObjectsByType<ClaimPlate>(UnityEngine.FindObjectsSortMode.None)) { typeof(ClaimPlate).GetField("_next", flags).SetValue(pl, 0f); typeof(ClaimPlate).GetMethod("Update", flags).Invoke(pl, null); }
// make a gap: take one book (set E) off its shelf and show it as a ghost in the slot it left, plus a yellow ghost for another set aimed at that shelf
var e3 = UnityEngine.GameObject.Find("R1_E3").GetComponent<Book>(); var slot = e3.currentSlot; slot.Release();
var ghostComp = UnityEngine.GameObject.Find("Player").GetComponent<SlotGhost>();
ghostComp.Show(e3, slot, BookBoy.Rules.PlacementResult.Correct, e3.transform.localScale);
e3.transform.position = e3.transform.position + UnityEngine.Vector3.up * 5f;
sb.AppendLine(LamplitBuilder.LightStats("ghost green ", V(-7.4f, 1.55f, 2.06f), V(-8.7f, 1.35f, 2.06f), 58.7f, null, d + "G_green.png"));
ghostComp.Show(e3, slot, BookBoy.Rules.PlacementResult.RightBookcaseWrongSpot, e3.transform.localScale);
sb.AppendLine(LamplitBuilder.LightStats("ghost yellow", V(-7.4f, 1.55f, 2.06f), V(-8.7f, 1.35f, 2.06f), 58.7f, null, d + "G_yellow.png"));
ghostComp.Hide();
sb.AppendLine(LamplitBuilder.LightStats("plates 4ft  ", V(-7.2f, 1.65f, 2.06f), V(-8.7f, 1.0f, 2.06f), 58.7f, null, d + "G_plates.png"));
sb.AppendLine(LamplitBuilder.LightStats("plinth row0 ", V(-7.4f, 1.4f, 2.06f), V(-8.7f, 0.55f, 2.06f), 58.7f, null, d + "G_row0.png"));
UnityEngine.Object.DestroyImmediate(UnityEngine.GameObject.Find("SlotGhost"));
return sb.ToString();
