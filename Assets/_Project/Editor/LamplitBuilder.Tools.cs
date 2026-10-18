using UnityEditor;
using UnityEngine;

public static partial class LamplitBuilder
{
    /// <summary>Renders the scene from a point of view into a PNG (Edit Mode or Play Mode). Used to eyeball what was built.</summary>
    public static string Shot(string path, Vector3 position, Vector3 lookAt, float fov = 75f, int width = 1600, int height = 800)
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
