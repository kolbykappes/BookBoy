using UnityEngine;

/// <summary>
/// The glow around a book: a bright thin outline plus a wider, fainter halo (two slightly enlarged back-face copies of
/// the book's mesh) and drifting sparkles. The book's own color never changes. Three independent layers, strongest wins:
/// a steady highlight (what the crosshair is on), a timed pulse (sister glow), and a short flash (placement feedback).
/// Everything it creates at runtime is hidden and not saved.
/// </summary>
public class BookOutline : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private const float InnerWidth = 0.007f;
    private const float HaloWidth = 0.014f;

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
            _xray.sharedMaterial = _assets.xrayMaterial;
        }

        var lightGo = new GameObject("GlowLight") { hideFlags = HideFlags.HideAndDontSave };
        _light = lightGo.AddComponent<Light>();
        _light.type = LightType.Point;
        _light.range = 1.8f;
        _light.shadows = LightShadows.None;
        _light.intensity = 0f;
        _light.enabled = false;

        var sparkGo = new GameObject("GlowSparks") { hideFlags = HideFlags.HideAndDontSave };
        _sparks = sparkGo.AddComponent<ParticleSystem>();
        ConfigureSparks(_sparks);
        _sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        _block = new MaterialPropertyBlock();
        _built = true;
        return true;
    }

    private MeshRenderer MakeHull(string hullName)
    {
        var go = new GameObject(hullName) { hideFlags = HideFlags.HideAndDontSave };
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = _bookMesh.sharedMesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = _assets.outlineMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.enabled = false;
        return renderer;
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
            _light.enabled = false;
            _lastLayer = Layer.None;
            _sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void SparkBurst(Color color, int count)
    {
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
        _inner.enabled = visible;
        _halo.enabled = visible;
        _xray.enabled = strong;
        _light.enabled = strong;

        if (!visible)
        {
            _lastLayer = Layer.None;
            if (_sparks.isEmitting)
            {
                _sparks.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }

            return;
        }

        float boost = strong ? 3.4f : 1f;
        ApplyHull(_inner, InnerWidth * boost, color, alpha);
        ApplyHull(_halo, HaloWidth * boost, color, alpha * (strong ? 0.5f : 0.22f));

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

    private void ApplyHull(MeshRenderer hull, float width, Color color, float alpha)
    {
        Vector3 size = transform.lossyScale;
        hull.transform.localScale = new Vector3(
            1f + 2f * width / Mathf.Max(0.001f, size.x),
            1f + 2f * width / Mathf.Max(0.001f, size.y),
            1f);   // no growth in depth: otherwise the shell shows as a slab trailing behind the book

        hull.GetPropertyBlock(_block);
        _block.SetColor(ColorId, new Color(color.r, color.g, color.b, alpha));
        hull.SetPropertyBlock(_block);
    }

    private void OnDisable()
    {
        StopAll();
    }

    private void OnDestroy()
    {
        if (_sparks != null)
        {
            Destroy(_sparks.gameObject);
        }

        if (_light != null)
        {
            Destroy(_light.gameObject);
        }
    }
}
