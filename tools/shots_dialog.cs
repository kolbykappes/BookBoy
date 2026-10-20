// Play Mode: opens the discovery dialog for a find and photographs the HUD canvas (switching it to a camera canvas for the shot). Frames freeze when the Editor is unfocused,
// so the open animation is finished by hand.
if (!UnityEditor.EditorApplication.isPlaying) return "REFUSED: enter Play Mode first";
var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
string id = "shoes";   // edit to "shoes" for the other
var pl = UnityEngine.GameObject.Find("Player"); var pi = pl.GetComponent<PlayerInteractor>();
var dlg = UnityEngine.Object.FindFirstObjectByType<FindDialog>();
var book = UnityEngine.GameObject.Find("Find_" + id).GetComponent<Book>();
dlg.autoShow = true;
pi.PickUp(book);
if (!dlg.IsOpen) return "dialog did not open";
var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
var t = typeof(FindDialog);
((UnityEngine.CanvasGroup)t.GetField("_group", flags).GetValue(dlg)).alpha = 1f;
((UnityEngine.RectTransform)t.GetField("_card", flags).GetValue(dlg)).localScale = UnityEngine.Vector3.one;
dlg.Preview.Tick(1.1f);
var canvasGo = UnityEngine.GameObject.Find("HUD_Canvas"); var canvas = canvasGo.GetComponent<UnityEngine.Canvas>();
var camGo = new UnityEngine.GameObject("UiShotCam"); var cam = camGo.AddComponent<UnityEngine.Camera>(); cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor; cam.backgroundColor = new UnityEngine.Color(0.2f, 0.25f, 0.2f);
var rt = new UnityEngine.RenderTexture(1920, 1080, 24); cam.targetTexture = rt; cam.orthographic = false; cam.cullingMask = ~0;
var oldMode = canvas.renderMode;
canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 1f;
UnityEngine.Canvas.ForceUpdateCanvases();
cam.Render();
UnityEngine.RenderTexture.active = rt; var tex = new UnityEngine.Texture2D(1920, 1080, UnityEngine.TextureFormat.RGB24, false); tex.ReadPixels(new UnityEngine.Rect(0, 0, 1920, 1080), 0, 0); tex.Apply();
System.IO.File.WriteAllBytes(d + "DLG_" + id + ".png", tex.EncodeToPNG());
UnityEngine.RenderTexture.active = null; canvas.renderMode = oldMode; cam.targetTexture = null;
UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(camGo);
var sb = new System.Text.StringBuilder();
sb.AppendLine("open " + dlg.IsOpen + " timeScale " + UnityEngine.Time.timeScale + " title '" + dlg.TitleText + "' kicker '" + dlg.KickerText + "' benefit '" + dlg.BenefitText + "'");
dlg.Close();
sb.AppendLine("after close timeScale " + UnityEngine.Time.timeScale + " open " + dlg.IsOpen);
return sb.ToString();
