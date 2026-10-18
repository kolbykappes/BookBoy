var sb = new System.Text.StringBuilder();
var cam = UnityEngine.Camera.main;
sb.AppendLine("camera: " + (cam != null ? cam.transform.position.ToString("F2") + " fov " + cam.fieldOfView : "none"));
var lights = UnityEngine.Object.FindObjectsByType<UnityEngine.Light>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
sb.AppendLine("lights: " + lights.Length);
foreach (var l in lights)
{
    var t = l.transform;
    string path = t.name; var p = t.parent; int g = 0;
    for (; p != null && g < 6; g++) { path = p.name + "/" + path; p = p.parent; }
    sb.AppendLine(l.type + " " + path + " int=" + l.intensity.ToString("F1") + " range=" + l.range.ToString("F1") + " en=" + l.enabled + " pos=" + t.position.ToString("F1") + " sh=" + l.shadows);
}
sb.AppendLine("ambient: " + UnityEngine.RenderSettings.ambientMode + " " + UnityEngine.RenderSettings.ambientLight + " sky " + UnityEngine.RenderSettings.ambientIntensity);
var vols = UnityEngine.Object.FindObjectsByType<UnityEngine.Rendering.Volume>(UnityEngine.FindObjectsSortMode.None);
foreach (var v in vols)
{
    sb.AppendLine("volume " + v.name + " global=" + v.isGlobal + " prof=" + (v.sharedProfile != null ? v.sharedProfile.name : "null"));
    if (v.sharedProfile != null) foreach (var c in v.sharedProfile.components) sb.AppendLine("   " + c.GetType().Name);
}
return sb.ToString();
