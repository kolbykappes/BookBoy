// Edit Mode eval script (run: unity command eval_file tools/setup_assets.cs --project-path C:/Code/BookBoy).
// Generates the floor tile textures (albedo + normal) and the materials the lighting / beacon / floor changes use.
// Re-runnable: textures are rewritten in place and existing materials are mutated, never recreated (their GUIDs stay).
if (UnityEditor.EditorApplication.isPlaying) return "REFUSED: stop Play Mode first";

var log = new System.Text.StringBuilder();
const string texDir = "Assets/_Project/Art/Textures";
const string matDir = "Assets/_Project/Art/Materials";

// ---------------------------------------------------------------- noise (periodic, so the texture tiles seamlessly)
float Hash(int x, int y, int seed)
{
    unchecked
    {
        uint h = (uint)(x * 374761393 + y * 668265263 + seed * 362437);
        h = (h ^ (h >> 13)) * 1274126177u;
        h ^= h >> 16;
        return (h & 0xFFFFFF) / (float)0x1000000;
    }
}
float VNoise(float x, float y, int period, int seed)
{
    int x0 = (int)UnityEngine.Mathf.Floor(x), y0 = (int)UnityEngine.Mathf.Floor(y);
    float fx = x - x0, fy = y - y0;
    fx = fx * fx * (3f - 2f * fx); fy = fy * fy * (3f - 2f * fy);
    int xa = ((x0 % period) + period) % period, ya = ((y0 % period) + period) % period;
    int xb = (xa + 1) % period, yb = (ya + 1) % period;
    float a = UnityEngine.Mathf.Lerp(Hash(xa, ya, seed), Hash(xb, ya, seed), fx);
    float b = UnityEngine.Mathf.Lerp(Hash(xa, yb, seed), Hash(xb, yb, seed), fx);
    return UnityEngine.Mathf.Lerp(a, b, fy);
}
float Fbm(float u, float v, int seed)   // u, v in 0..1 across the whole texture
{
    float sum = 0f, amp = 0.5f; int period = 4;
    for (int o = 0; o < 4; o++) { sum += amp * VNoise(u * period, v * period, period, seed + o); amp *= 0.5f; period *= 2; }
    return sum;
}

// ---------------------------------------------------------------- the floor texture: 2 x 2 checkered marble tiles with grout
const int S = 512; const int T = 256; const float G = 5f;   // texture size, tile size, grout half-width in pixels
var albedo = new UnityEngine.Color32[S * S];
var height = new float[S * S];
for (int py = 0; py < S; py++)
{
    for (int px = 0; px < S; px++)
    {
        float u = px / (float)S, v = py / (float)S;
        int tx = px / T, ty = py / T;
        bool dark = ((tx + ty) & 1) == 1;
        int lx = px % T, ly = py % T;
        float edge = UnityEngine.Mathf.Min(UnityEngine.Mathf.Min(lx, T - 1 - lx), UnityEngine.Mathf.Min(ly, T - 1 - ly));
        float grout = 1f - UnityEngine.Mathf.SmoothStep(0f, 1f, UnityEngine.Mathf.Clamp01((edge - (G - 1.5f)) / 3f));

        float n = Fbm(u, v, dark ? 11 : 3);
        float warp = Fbm(u + 0.31f, v + 0.17f, 29);
        float vein = UnityEngine.Mathf.Pow(1f - UnityEngine.Mathf.Abs(UnityEngine.Mathf.Sin(2f * UnityEngine.Mathf.PI * (3f * u + 2f * v + warp * 2.2f))), 14f);
        float tileShade = 0.94f + 0.12f * Hash(tx + (dark ? 7 : 0), ty, 5);

        UnityEngine.Color baseColor = dark ? new UnityEngine.Color(0.27f, 0.25f, 0.29f) : new UnityEngine.Color(0.80f, 0.74f, 0.64f);
        UnityEngine.Color c = baseColor * ((0.82f + 0.36f * n) * tileShade);
        c = UnityEngine.Color.Lerp(c, dark ? new UnityEngine.Color(0.55f, 0.52f, 0.58f) : new UnityEngine.Color(0.52f, 0.44f, 0.36f), vein * 0.28f);
        c = UnityEngine.Color.Lerp(c, new UnityEngine.Color(0.09f, 0.08f, 0.08f), grout);
        albedo[py * S + px] = new UnityEngine.Color32((byte)(UnityEngine.Mathf.Clamp01(c.r) * 255f), (byte)(UnityEngine.Mathf.Clamp01(c.g) * 255f), (byte)(UnityEngine.Mathf.Clamp01(c.b) * 255f), 255);
        height[py * S + px] = (1f - grout) + 0.05f * n;
    }
}

var normals = new UnityEngine.Color32[S * S];
float Hh(int x, int y) { return height[((y + S) % S) * S + ((x + S) % S)]; }
for (int py = 0; py < S; py++)
{
    for (int px = 0; px < S; px++)
    {
        float dx = (Hh(px + 1, py) - Hh(px - 1, py)) * 0.5f, dy = (Hh(px, py + 1) - Hh(px, py - 1)) * 0.5f;
        var nrm = new UnityEngine.Vector3(-dx * 6f, -dy * 6f, 1f).normalized;
        normals[py * S + px] = new UnityEngine.Color32((byte)((nrm.x * 0.5f + 0.5f) * 255f), (byte)((nrm.y * 0.5f + 0.5f) * 255f), (byte)((nrm.z * 0.5f + 0.5f) * 255f), 255);
    }
}

void WritePng(string path, UnityEngine.Color32[] pixels)
{
    var tex = new UnityEngine.Texture2D(S, S, UnityEngine.TextureFormat.RGBA32, false);
    tex.SetPixels32(pixels); tex.Apply();
    System.IO.File.WriteAllBytes(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), path), tex.EncodeToPNG());
    UnityEngine.Object.DestroyImmediate(tex);
    UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
}
WritePng(texDir + "/Tile_Floor_Albedo.png", albedo);
WritePng(texDir + "/Tile_Floor_Normal.png", normals);

var albedoImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(texDir + "/Tile_Floor_Albedo.png");
albedoImporter.wrapMode = UnityEngine.TextureWrapMode.Repeat; albedoImporter.filterMode = UnityEngine.FilterMode.Trilinear; albedoImporter.anisoLevel = 8; albedoImporter.mipmapEnabled = true;
albedoImporter.SaveAndReimport();
var normalImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(texDir + "/Tile_Floor_Normal.png");
normalImporter.textureType = UnityEditor.TextureImporterType.NormalMap; normalImporter.wrapMode = UnityEngine.TextureWrapMode.Repeat; normalImporter.filterMode = UnityEngine.FilterMode.Trilinear; normalImporter.anisoLevel = 8;
normalImporter.SaveAndReimport();
log.Append("textures written; ");

// ---------------------------------------------------------------- materials (mutated in place if they already exist)
UnityEngine.Material GetMat(string path, string shaderName)
{
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null)
    {
        m = new UnityEngine.Material(UnityEngine.Shader.Find(shaderName));
        UnityEditor.AssetDatabase.CreateAsset(m, path);
    }
    return m;
}

var floor = GetMat(matDir + "/Mat_FloorTile.mat", "Universal Render Pipeline/Lit");
floor.SetTexture("_BaseMap", UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(texDir + "/Tile_Floor_Albedo.png"));
floor.SetTexture("_BumpMap", UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(texDir + "/Tile_Floor_Normal.png"));
floor.SetColor("_BaseColor", UnityEngine.Color.white);
floor.SetFloat("_BumpScale", 1f);
floor.EnableKeyword("_NORMALMAP");
floor.SetFloat("_Smoothness", 0.38f);
floor.SetFloat("_Metallic", 0f);
floor.SetFloat("_EnvironmentReflections", 0f); floor.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");   // only the room's own lights, not the sky, shine off the tiles
floor.mainTextureScale = new UnityEngine.Vector2(10f, 10f);   // the 16 m floor shows 10 x 10 repeats of the 2 x 2 tile texture = 0.8 m tiles
UnityEditor.EditorUtility.SetDirty(floor);

var wall = GetMat(matDir + "/Mat_Wall.mat", "Universal Render Pipeline/Lit");
wall.SetColor("_BaseColor", new UnityEngine.Color(0.43f, 0.35f, 0.31f));
wall.SetFloat("_Smoothness", 0.12f); wall.SetFloat("_Metallic", 0f);
UnityEditor.EditorUtility.SetDirty(wall);

var orb = GetMat(matDir + "/Mat_Orb.mat", "Universal Render Pipeline/Lit");
orb.SetColor("_BaseColor", new UnityEngine.Color(1f, 0.86f, 0.6f));
orb.EnableKeyword("_EMISSION"); orb.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.RealtimeEmissive;
orb.SetColor("_EmissionColor", new UnityEngine.Color(1f, 0.72f, 0.38f) * 3.2f);
UnityEditor.EditorUtility.SetDirty(orb);

var beacon = GetMat(matDir + "/Mat_Beacon.mat", "Sprites/Default");
beacon.SetColor("_Color", UnityEngine.Color.white);
UnityEditor.EditorUtility.SetDirty(beacon);

UnityEditor.AssetDatabase.SaveAssets();
log.Append("materials ready; floor shader=" + floor.shader.name + " bump=" + (floor.GetTexture("_BumpMap") != null) + " base=" + (floor.GetTexture("_BaseMap") != null));
return log.ToString();
