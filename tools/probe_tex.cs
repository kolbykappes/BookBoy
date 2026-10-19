var sb = new System.Text.StringBuilder();
System.Func<string, UnityEngine.Color> Mean = path => {
    var t = new UnityEngine.Texture2D(2, 2, UnityEngine.TextureFormat.RGBA32, false);
    UnityEngine.ImageConversion.LoadImage(t, System.IO.File.ReadAllBytes(path));
    var px = t.GetPixels(); double r = 0, g = 0, b = 0;
    foreach (var p in px) { r += UnityEngine.Mathf.GammaToLinearSpace(p.r); g += UnityEngine.Mathf.GammaToLinearSpace(p.g); b += UnityEngine.Mathf.GammaToLinearSpace(p.b); }
    UnityEngine.Object.DestroyImmediate(t);
    return new UnityEngine.Color((float)(r / px.Length), (float)(g / px.Length), (float)(b / px.Length));
};
var wood = Mean("C:/Code/BookBoy/Assets/_Project/Art/Textures/Wood/T_Wood_Albedo.png");
var cover = Mean("C:/Code/BookBoy/Assets/_Project/Art/Textures/Books/T_BookCover_Albedo.png");
sb.AppendLine("wood texture mean (linear) " + wood + "  cover texture mean (linear) " + cover);
System.Func<UnityEngine.Color, float> Y = c => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
foreach (var baseSrgb in new[] { new UnityEngine.Color(0.82f, 0.43f, 0.20f), new UnityEngine.Color(0.70f, 0.36f, 0.17f), new UnityEngine.Color(0.62f, 0.30f, 0.14f), new UnityEngine.Color(0.55f, 0.26f, 0.12f) })
{
    var lin = new UnityEngine.Color(UnityEngine.Mathf.GammaToLinearSpace(baseSrgb.r), UnityEngine.Mathf.GammaToLinearSpace(baseSrgb.g), UnityEngine.Mathf.GammaToLinearSpace(baseSrgb.b));
    var eff = new UnityEngine.Color(lin.r * wood.r, lin.g * wood.g, lin.b * wood.b);
    sb.AppendLine("wood base " + baseSrgb + " -> effective Y " + Y(eff).ToString("F3"));
}
return sb.ToString();
