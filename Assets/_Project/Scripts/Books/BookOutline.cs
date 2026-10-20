using UnityEngine;

/// <summary>
/// The outline around a book: a crisp line plus a wider halo (two inverted hulls: the book's mesh drawn again, back faces only, pushed out along smoothed normals
/// in clip space, so the width is in screen pixels; see docs/unity-practices/29). The book's own color never changes. Three independent layers, strongest wins:
/// a steady highlight (what the crosshair is on: a thin warm-white line over a dark underlay, no sparkles, no light), a timed pulse (sister glow: coloured, with
/// sparkles and a light), and a short flash (placement feedback). Everything it creates at runtime is hidden and not saved.
/// </summary>
public class BookOutline : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int WidthId = Shader.PropertyToID("_WidthPx");
    private static readonly int PullId = Shader.PropertyToID("_DepthPull");

    // line widths in pixels at 1080p (the shader's unit; scaled with the screen height in ApplyHull)
    private const float SteadyCorePx = 3f;
    private const float SteadyUnderlayPx = 8f;
    private const float InnerPx = 3.5f;
    private const float HaloPx = 9f;

    /// <summary>The steady line is brighter than 1 so the project's bloom softens it into a glow.</summary>
    private const float SteadyHdr = 3f;

    private enum Layer
    {
        None,
        Steady,
        Pulse,
        Flash
    }

    private static GlowAssets _assets;

    private Renderer _bookRenderer;
    private MeshFilter _bookMesh;
    private MeshRenderer _inner;
    private MeshRenderer _halo;
    private MeshRenderer _xray;
    private Light _light;
    private Layer _lastLayer = Layer.None;
    private ParticleSystem _sparks;
    private MaterialPropertyBlock _block;

    private bool _steady;
    private Color _steadyColor;
    private float _pulseUntil;
    private float _pulseStart;
    private Color _pulseColor;
    private float _flashStart;
    private float _flashDuration;
    private Color _flashColor;
    private bool _built;

    public bool IsSteady => _steady;
    public bool IsPulsing => Time.time < _pulseUntil;
    public bool IsFlashing => Time.time < _flashStart + _flashDuration;
    public bool IsGlowing => _steady || IsPulsing || IsFlashing;

    private bool EnsureBuilt()
    {
        if (_built)
        {
            return true;
        }

        if (_assets == null)
        {
            _assets = FindFirstObjectByType<GlowAssets>();
        }

        _bookRenderer = GetComponent<Renderer>();
        _bookMesh = GetComponent<MeshFilter>();
        if (_assets == null || _assets.outlineMaterial == null || _bookMesh == null || _bookMesh.sharedMesh == null)
        {
            return false;
        }

        _inner = MakeHull("GlowInner");
        _halo = MakeHull("GlowHalo");
        _xray = MakeHull("GlowXray");
        if (_assets.xrayMaterial != null)
        {
            _xray.sharedMaterials = Repeat(_assets.xrayMaterial, _bookMesh.sharedMesh.subMeshCount);
        }

        // The point light and the sparkle particle system are NOT built per book: they are borrowed from a small shared pool while a book actually glows
        // (see GlowPool), because a room full of loose books would otherwise carry hundreds of idle lights and particle systems.
        _block = new MaterialPropertyBlock();
        _built = true;
        return true;
    }

    private static Material[] Repeat(Material m, int count)
    {
        var all = new Material[Mathf.Max(1, count)];
        for (int i = 0; i < all.Length; i++)
        {
            all[i] = m;
        }

        return all;
    }

    private Light AcquireLight()
    {
        if (_light == null)
        {
            _light = GlowPool.RentLight();
        }

        return _light;
    }

    private void ReleaseLight()
    {
        if (_light != null)
        {
            GlowPool.ReturnLight(_light);
            _light = null;
        }
    }

    private ParticleSystem AcquireSparks()
    {
        if (_sparks == null)
        {
            _sparks = GlowPool.RentSparks();
            ConfigureSparks(_sparks);
            _lastLayer = Layer.None;
        }

        return _sparks;
    }

    private void ReleaseSparks()
    {
        if (_sparks != null)
        {
            GlowPool.ReturnSparks(_sparks);
            _sparks = null;
        }
    }

    private MeshRenderer MakeHull(string hullName)
    {
        var go = new GameObject(hullName) { hideFlags = HideFlags.HideAndDontSave };
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = HullMesh(_bookMesh.sharedMesh);
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = Repeat(_assets.outlineMaterial, _bookMesh.sharedMesh.subMeshCount);   // one entry per submesh, so the page block glows too
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.enabled = false;
        return renderer;
    }

    private static readonly System.Collections.Generic.Dictionary<Mesh, Mesh> Hulls = new System.Collections.Generic.Dictionary<Mesh, Mesh>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetHulls()
    {
        foreach (Mesh m in Hulls.Values)
        {
            if (m != null)
            {
                Destroy(m);
            }
        }

        Hulls.Clear();
    }

    /// <summary>A copy of <paramref name="source"/> whose normals are smoothed across the hard edges (the normals of vertices at the same place are averaged), for the outline
    /// shader: the book's own normals are split at every box edge, and extruding along them would open gaps at the corners. Built once per mesh and cached; the mesh must
    /// be readable (the book models are), otherwise the source itself is returned.</summary>
    public static Mesh HullMesh(Mesh source)
    {
        if (source == null || !source.isReadable)
        {
            return source;
        }

        if (Hulls.TryGetValue(source, out Mesh cached) && cached != null)
        {
            return cached;
        }

        Vector3[] vertices = source.vertices;
        Vector3[] normals = source.normals;
        var sums = new System.Collections.Generic.Dictionary<Vector3Int, Vector3>();
        Vector3Int Key(Vector3 p) => new Vector3Int(Mathf.RoundToInt(p.x * 10000f), Mathf.RoundToInt(p.y * 10000f), Mathf.RoundToInt(p.z * 10000f));
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3Int k = Key(vertices[i]);
            sums[k] = sums.TryGetValue(k, out Vector3 s) ? s + normals[i] : normals[i];
        }

        var smooth = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            smooth[i] = sums[Key(vertices[i])].normalized;
        }

        var hull = new Mesh { name = source.name + "_Hull", hideFlags = HideFlags.HideAndDontSave };
        hull.vertices = vertices;
        hull.normals = smooth;
        hull.subMeshCount = source.subMeshCount;
        for (int s = 0; s < source.subMeshCount; s++)
        {
            hull.SetTriangles(source.GetTriangles(s), s);
        }

        hull.bounds = source.bounds;
        Hulls[source] = hull;
        return hull;
    }

    private void ConfigureSparks(ParticleSystem ps)
    {
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.007f, 0.014f);
        main.gravityModifier = -0.02f;
        main.maxParticles = 40;

        var emission = ps.emission;
        emission.rateOverTime = 16f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = fade;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (_assets.sparkleMaterial != null)
        {
            renderer.sharedMaterial = _assets.sparkleMaterial;
        }
    }

    /// <summary>A steady glow while something (the crosshair) is on this book.</summary>
    public void SetSteady(bool on, Color color)
    {
        _steady = on;
        _steadyColor = color;
        if (on)
        {
            EnsureBuilt();
        }
    }

    /// <summary>A pulsing glow for a number of seconds (sister glow).</summary>
    public void Pulse(Color color, float seconds)
    {
        if (!EnsureBuilt())
        {
            return;
        }

        _pulseColor = color;
        _pulseStart = Time.time;
        _pulseUntil = Time.time + seconds;
    }

    /// <summary>A quick bright flash that fades out (placement feedback), optionally with a sparkle burst.</summary>
    public void Flash(Color color, bool burst)
    {
        if (!EnsureBuilt())
        {
            return;
        }

        _flashColor = color;
        _flashStart = Time.time;
        _flashDuration = 1.0f;
        if (burst)
        {
            SparkBurst(color, 22);
        }
    }

    public void StopAll()
    {
        _steady = false;
        _pulseUntil = 0f;
        _flashDuration = 0f;
        if (_built)
        {
            _inner.enabled = false;
            _halo.enabled = false;
            _xray.enabled = false;
            _lastLayer = Layer.None;
            ReleaseLight();
            if (_sparks != null)
            {
                _sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ReleaseSparks();
            }
        }
    }

    private void SparkBurst(Color color, int count)
    {
        AcquireSparks();
        var main = _sparks.main;
        main.startColor = color;
        FollowBook();
        _sparks.Emit(count);
    }

    private void FollowBook()
    {
        Transform t = _sparks.transform;
        t.SetPositionAndRotation(transform.position, transform.rotation);
        var shape = _sparks.shape;
        shape.scale = Vector3.Scale(transform.lossyScale, new Vector3(1.1f, 1.05f, 1.1f));
    }

    private void LateUpdate()
    {
        if (!_built)
        {
            return;
        }

        Layer layer = Layer.None;
        Color color = Color.clear;
        float alpha = 0f;

        float now = Time.time;
        if (_steady)
        {
            layer = Layer.Steady;
            color = _steadyColor;
            alpha = 0.95f;
        }

        if (now < _pulseUntil)
        {
            float remaining = _pulseUntil - now;
            float fadeOut = Mathf.Clamp01(remaining / 0.5f);
            float beat = 0.7f + 0.3f * Mathf.Sin((now - _pulseStart) * 8f);
            float a = beat * fadeOut;
            if (layer == Layer.None || a > alpha * 0.5f)
            {
                layer = Layer.Pulse;
                color = _pulseColor;
                alpha = a;
            }
        }

        if (now < _flashStart + _flashDuration)
        {
            float t = (now - _flashStart) / _flashDuration;
            float a = t < 0.35f ? 1f : Mathf.Clamp01(1f - (t - 0.35f) / 0.65f);
            layer = Layer.Flash;
            color = _flashColor;
            alpha = a;
        }

        bool visible = layer != Layer.None && alpha > 0.01f;
        bool strong = visible && layer == Layer.Pulse;   // sister glow: much bigger and brighter than the other layers
        if (layer == Layer.Steady)
        {
            alpha = 0.88f + 0.12f * Mathf.Sin(now * 3f);   // a slow breath
        }

        _inner.enabled = visible;
        _halo.enabled = visible;
        _xray.enabled = strong;
        if (strong)
        {
            AcquireLight().enabled = true;
        }
        else
        {
            ReleaseLight();
        }

        if (!visible)
        {
            _lastLayer = Layer.None;
            if (_sparks != null)
            {
                if (_sparks.isEmitting)
                {
                    _sparks.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }
                else if (_sparks.particleCount == 0)
                {
                    ReleaseSparks();   // nothing left to show: give the particle system back to the pool
                }
            }

            return;
        }

        if (layer == Layer.Steady)
        {
            // the aimed book: a thin warm-white line (bloom turns the HDR value into a soft glow) over a dark underlay so it reads on pale floor tiles as well as on dark wood
            ApplyHull(_halo, SteadyUnderlayPx, new Color(0.05f, 0.035f, 0.02f), alpha * 0.55f, 0);
            ApplyHull(_inner, SteadyCorePx, new Color(color.r * SteadyHdr, color.g * SteadyHdr, color.b * SteadyHdr), alpha, 1);
            _lastLayer = layer;
            if (_sparks != null)
            {
                if (_sparks.isEmitting)
                {
                    _sparks.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }
                else if (_sparks.particleCount == 0)
                {
                    ReleaseSparks();
                }
            }

            return;
        }

        AcquireSparks();

        float boost = strong ? 2.2f : 1f;
        ApplyHull(_inner, InnerPx * boost, color, alpha, 1);
        ApplyHull(_halo, HaloPx * boost, color, alpha * (strong ? 0.5f : 0.22f), 0);

        if (strong)
        {
            _xray.transform.localScale = Vector3.one * 1.03f;
            _xray.GetPropertyBlock(_block);
            _block.SetColor(ColorId, new Color(color.r, color.g, color.b, alpha * 0.75f));
            _xray.SetPropertyBlock(_block);

            _light.transform.position = transform.position + Vector3.up * (transform.lossyScale.y * 0.5f + 0.1f);
            _light.color = color;
            _light.intensity = 4f * alpha;
        }

        if (layer != _lastLayer)
        {
            ConfigureSparkStyle(strong);
            _lastLayer = layer;
        }

        FollowBook();
        var main = _sparks.main;
        main.startColor = new Color(color.r, color.g, color.b, 1f);
        if (layer != Layer.Flash && !_sparks.isEmitting)
        {
            _sparks.Play();
        }
    }

    private void ConfigureSparkStyle(bool strong)
    {
        var main = _sparks.main;
        main.startSize = strong ? new ParticleSystem.MinMaxCurve(0.016f, 0.04f) : new ParticleSystem.MinMaxCurve(0.007f, 0.014f);
        main.startSpeed = strong ? new ParticleSystem.MinMaxCurve(0.08f, 0.35f) : new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
        main.maxParticles = strong ? 120 : 40;
        var emission = _sparks.emission;
        emission.rateOverTime = strong ? 80f : 16f;
    }

    private void ApplyHull(MeshRenderer hull, float widthPx, Color color, float alpha, int sortingOrder)
    {
        hull.sortingOrder = sortingOrder;   // the underlay (0) is drawn before the line (1)
        hull.GetPropertyBlock(_block);
        _block.SetColor(ColorId, new Color(color.r, color.g, color.b, alpha));
        _block.SetFloat(WidthId, widthPx * Mathf.Max(1f, Screen.height / 1080f));
        _block.SetFloat(PullId, 0f);   // a depth pull would bring the hull's back faces in front of the book and paint it over (docs/unity-practices/29)
        hull.SetPropertyBlock(_block);
    }

    private void OnDisable()
    {
        StopAll();
    }

    private void OnDestroy()
    {
        ReleaseLight();
        if (_sparks != null)
        {
            _sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ReleaseSparks();
        }
    }
}

/// <summary>
/// A small shared pool of the point lights and sparkle particle systems the book glows borrow. They are created on demand, returned when a glow ends, and never
/// saved. The statics are cleared at the start of every Play session (the project skips domain reload, so they would otherwise outlive the scene).
/// </summary>
public static class GlowPool
{
    private static readonly System.Collections.Generic.Stack<Light> Lights = new System.Collections.Generic.Stack<Light>();
    private static readonly System.Collections.Generic.Stack<ParticleSystem> Sparks = new System.Collections.Generic.Stack<ParticleSystem>();

    // everything the pool ever made: HideAndDontSave objects survive leaving Play Mode, so they are destroyed when the next session starts
    private static readonly System.Collections.Generic.List<GameObject> All = new System.Collections.Generic.List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        foreach (GameObject go in All)
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        All.Clear();
        Lights.Clear();
        Sparks.Clear();
    }

    public static Light RentLight()
    {
        while (Lights.Count > 0)
        {
            Light l = Lights.Pop();
            if (l != null)
            {
                l.enabled = true;
                return l;
            }
        }

        var go = new GameObject("GlowLight") { hideFlags = HideFlags.HideAndDontSave };
        All.Add(go);
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 1.8f;
        light.shadows = LightShadows.None;
        light.intensity = 0f;
        return light;
    }

    public static void ReturnLight(Light l)
    {
        if (l == null)
        {
            return;
        }

        l.enabled = false;
        Lights.Push(l);
    }

    public static ParticleSystem RentSparks()
    {
        while (Sparks.Count > 0)
        {
            ParticleSystem p = Sparks.Pop();
            if (p != null)
            {
                return p;
            }
        }

        var go = new GameObject("GlowSparks") { hideFlags = HideFlags.HideAndDontSave };
        All.Add(go);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return ps;
    }

    public static void ReturnSparks(ParticleSystem p)
    {
        if (p == null)
        {
            return;
        }

        Sparks.Push(p);
    }
}
