var d = "C:/Users/angry/AppData/Local/Temp/claude/C--Code-BookBoy/f093f5b7-069b-4f91-9e47-626a41770065/scratchpad/";
var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main.transform;
var hand = cam.Find("HandLight");
var hl = hand.GetComponent<UnityEngine.Light>();
var p0 = cam.position; var r0 = cam.rotation;
var c = UnityEngine.GameObject.Find("Container_5").transform.position;
var eye = new UnityEngine.Vector3(c.x + 0.9f, 1.15f, c.z);
var look = new UnityEngine.Vector3(c.x, 1.25f, c.z);
cam.SetPositionAndRotation(eye, UnityEngine.Quaternion.LookRotation(look - eye));
var lp = hand.localPosition; float li = hl.intensity, lr = hl.range;
// old settings
hand.localPosition = new UnityEngine.Vector3(0.1f, 0.05f, 0.2f); hl.intensity = 2f; hl.range = 2.6f;
sb.AppendLine(LamplitBuilder.Shot(d + "h_old.png", eye, look, 75f, 1386, 719));
// new settings
hand.localPosition = lp; hl.intensity = li; hl.range = lr;
sb.AppendLine(LamplitBuilder.Shot(d + "h_new.png", eye, look, 75f, 1386, 719));
cam.SetPositionAndRotation(p0, r0);
sb.AppendLine("hand light now " + hl.intensity + " " + hl.range + " " + hand.localPosition.ToString("F2"));
return sb.ToString();
