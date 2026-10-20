using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static partial class LamplitBuilder
{
    /// <summary>Renders the scene from a point of view into a PNG (Edit Mode or Play Mode). Used to eyeball what was built.</summary>
    public static string Shot(string path, Vector3 position, Vector3 lookAt, float fov = 75f, int width = 1600, int height = 800, float near = 0.05f)
    {
        var go = new GameObject("TMP_ShotCam");
        try
        {
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = near;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.03f, 0.055f);
            cam.transform.position = position;
            cam.transform.LookAt(lookAt);
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;   // the game's tonemapping, bloom and vignette, as in the Game view
            var rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            return "wrote " + path;
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}

public static partial class LamplitBuilder
{
    /// <summary>Like <see cref="Shot"/> but returns the picture instead of writing it (the caller destroys it).</summary>
    public static Texture2D ShotTexture(Vector3 position, Vector3 lookAt, float fov, int width, int height)
    {
        var go = new GameObject("TMP_ShotCam");
        try
        {
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.05f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.03f, 0.055f);
            cam.transform.position = position;
            cam.transform.LookAt(lookAt);
            var rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            return tex;
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    /// <summary>The centre of the biggest cluster of loose items in a room (where the biggest pile is).</summary>
    public static Vector3 BiggestPileCentre(RoomZone zone)
    {
        int best = -1; Vector3 centre = Vector3.zero;
        foreach (var a in zone.items)
        {
            int n = 0; Vector3 sum = Vector3.zero;
            foreach (var b in zone.items)
            {
                if ((a.transform.position - b.transform.position).sqrMagnitude < 0.6f * 0.6f) { n++; sum += b.transform.position; }
            }

            if (n > best) { best = n; centre = sum / n; }
        }

        return centre;
    }
}

public static partial class LamplitBuilder
{
    /// <summary>Renders a view the way the game does (post-processing on) and measures it: luma (gamma space, Rec 709 weights) percentiles of the
    /// whole frame, the share of near-white and near-black pixels, the share of very bright pixels in the top third (ceiling hotspots), and the
    /// average luma of a 7 x 7 pixel patch at each probe point (world positions: a spine, the wood, a wall...). Edit Mode or Play Mode.</summary>
    public static string LightStats(string label, Vector3 position, Vector3 lookAt, float fov, Vector3[] probes, string pngPath = null)
    {
        const int w = 1386, h = 719;
        var go = new GameObject("TMP_StatsCam");
        try
        {
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.05f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.03f, 0.055f);
            cam.transform.position = position;
            cam.transform.LookAt(lookAt);
            cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            var px = tex.GetPixels32();
            if (pngPath != null) System.IO.File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            var luma = new float[px.Length];
            int clipped = 0, crushed = 0, topBright = 0, topCount = 0;
            for (int i = 0; i < px.Length; i++)
            {
                float l = (0.2126f * px[i].r + 0.7152f * px[i].g + 0.0722f * px[i].b) / 255f;
                luma[i] = l;
                if (l > 0.97f) clipped++;
                if (l < 0.03f) crushed++;
                if (i / w >= h * 2 / 3)   // texture rows run bottom to top, so the top third is the last third
                {
                    topCount++;
                    if (l > 0.90f) topBright++;
                }
            }

            var sorted = (float[])luma.Clone();
            System.Array.Sort(sorted);
            var sb = new System.Text.StringBuilder();
            sb.Append(label).Append(": luma p5 ").Append(sorted[sorted.Length / 20].ToString("F2")).Append(" median ").Append(sorted[sorted.Length / 2].ToString("F2"))
              .Append(" p95 ").Append(sorted[sorted.Length * 19 / 20].ToString("F2")).Append(" | near-white ").Append((100f * clipped / px.Length).ToString("F1"))
              .Append("% near-black ").Append((100f * crushed / px.Length).ToString("F1")).Append("% | top-third hotspot ").Append((100f * topBright / System.Math.Max(1, topCount)).ToString("F1")).Append("%");
            for (int p = 0; probes != null && p < probes.Length; p++)
            {
                Vector3 sp = cam.WorldToScreenPoint(probes[p]);
                if (sp.z <= 0f || sp.x < 4 || sp.y < 4 || sp.x > w - 5 || sp.y > h - 5) { sb.Append(" | probe").Append(p).Append(" off-screen"); continue; }
                float acc = 0f; int n = 0;
                for (int dy = -3; dy <= 3; dy++) for (int dx = -3; dx <= 3; dx++) { acc += luma[((int)sp.y + dy) * w + (int)sp.x + dx]; n++; }
                sb.Append(" | probe").Append(p).Append(' ').Append((acc / n).ToString("F2"));
            }

            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            return sb.ToString();
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}
