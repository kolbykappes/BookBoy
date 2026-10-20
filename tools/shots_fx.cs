// Play Mode: completes an 8-book series row and photographs the celebration (world) and the HUD motes mid-flight. Frames freeze while the Editor is unfocused, so the clocks are moved by hand.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var V = new System.Func<float, float, float, UnityEngine.Vector3>((x, y, z) => new UnityEngine.Vector3(x, y, z));
var cc = UnityEngine.Object.FindFirstObjectByType<CampaignController>(); var zone = cc.Room(1); cc.JumpTo(1);
var ui = UnityEngine.GameObject.Find("Game").GetComponent<GameUI>(); var fx = ui.GetComponent<ScoreFx>();
var fl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var sb = new System.Text.StringBuilder();
foreach (var b in zone.items) if (b.currentSlot != null) b.currentSlot.Release();
zone.PlaceAllHome();
var row = UnityEngine.GameObject.Find("R1_E1").GetComponent<Book>().currentSlot.shelf;   // set E, the pink MENDING series (index 4)
var e8 = UnityEngine.GameObject.Find("R1_E8").GetComponent<Book>(); var slot8 = e8.currentSlot; slot8.Release();
e8.transform.position = V(-6f, 0.1f, 3f);
cc.Recompute();
var pl = UnityEngine.GameObject.Find("Player"); var cam = pl.GetComponent<PlayerInteractor>().interactionCamera.transform;
var mid = row.slots[3].transform.position + V(0f, 0.17f, 0f);
slot8.Accept(e8, e8.transform.localScale);
var f = ShelfFlourish.Active.Count > 0 ? ShelfFlourish.Active[0] : null;
if (f == null) return "no celebration";
foreach (var t in new[] { 0.35f, 0.75f, 1.15f })
{
    f.Tick(t);
    LamplitBuilder.Shot(d + "FX_world_" + (int)(t * 100) + ".png", mid + V(1.25f, 0.1f, 0f), mid, 52f, 1600, 900, 0.1f);
}
var uiUpdate = typeof(GameUI).GetMethod("Update", fl); var fxUpdate = typeof(ScoreFx).GetMethod("Update", fl);
var canvasGo = UnityEngine.GameObject.Find("HUD_Canvas"); var canvas = canvasGo.GetComponent<UnityEngine.Canvas>();
var camGo = new UnityEngine.GameObject("UiShotCam"); var cam2 = camGo.AddComponent<UnityEngine.Camera>(); cam2.transform.position = mid + V(1.25f, 0.1f, 0f); cam2.transform.LookAt(mid); cam2.fieldOfView = 52f; cam2.nearClipPlane = 0.1f;
var rt = new UnityEngine.RenderTexture(1920, 1080, 24); cam2.targetTexture = rt;
var oldMode = canvas.renderMode; canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam2; canvas.planeDistance = 1f;
var tex = new UnityEngine.Texture2D(1920, 1080, UnityEngine.TextureFormat.RGB24, false);
void ShotUi(string name, float skip, float sheetT) { fx.Skip(skip); fxUpdate.Invoke(fx, null); uiUpdate.Invoke(ui, null); f.Tick(sheetT); UnityEngine.Canvas.ForceUpdateCanvases(); cam2.Render(); UnityEngine.RenderTexture.active = rt; tex.ReadPixels(new UnityEngine.Rect(0, 0, 1920, 1080), 0, 0); tex.Apply(); System.IO.File.WriteAllBytes(d + name, tex.EncodeToPNG()); }
ShotUi("FX_hud_1.png", 0.28f, 0.4f);
ShotUi("FX_hud_2.png", 0.75f, 0.9f);
ShotUi("FX_hud_3.png", 0.55f, 1.3f);
ShotUi("FX_hud_4.png", 0.12f, 1.5f);
UnityEngine.RenderTexture.active = null; canvas.renderMode = oldMode; cam2.targetTexture = null;
UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(camGo);
foreach (var o in ShelfFlourish.Active.ToArray()) UnityEngine.Object.DestroyImmediate(o.gameObject);
return "ok motes " + fx.MotesInFlight + " arrivals " + fx.Arrivals;
