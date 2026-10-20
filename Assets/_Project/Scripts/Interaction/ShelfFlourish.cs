using UnityEngine;

/// <summary>
/// The celebration of a finished shelf, in the world (after Witch Cabin, docs/competitive/16 screenshots 33, 35 and 42): the books on the row pop up one after another
/// in a wave and flash gold, a gold sheet with a sweeping band and a pulsing glow lies over the row, and gold motes rise from it. It builds itself, plays for about two
/// seconds and destroys itself. The HUD half (motes flying to the SHELVES counter) is <see cref="ScoreFx"/>. Nothing here touches layers, renderer features or settings.
/// </summary>
public class ShelfFlourish : MonoBehaviour
{
    public static readonly Color Gold = new Color(1f, 0.80f, 0.30f);

    private const float WaveStep = 0.075f;      // seconds between one book and the next
    private const float HopTime = 0.45f;
    private const float HopHeight = 0.035f;
    private const float SweepTime = 1.7f;

    /// <summary>The celebrations that are playing right now (tests; static, so cleared when the session starts).</summary>
    public static readonly System.Collections.Generic.List<ShelfFlourish> Active = new System.Collections.Generic.List<ShelfFlourish>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Active.Clear();
        _sweepMaterial = null;
    }

    private static Material _sweepMaterial;
    private static Shader _shader;

    private Shelf _shelf;
    private Book[] _books;
    private Vector3[] _restPosition;
    private Renderer _sheet;
    private MaterialPropertyBlock _block;
    private float _t0;
    private ParticleSystem _motes;
    private bool _finished;

    private static readonly int ProgressId = Shader.PropertyToID("_Progress");
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    /// <summary>Plays the celebration for <paramref name="shelf"/> (whose row is complete). Returns the object that plays it.</summary>
    public static ShelfFlourish Play(Shelf shelf)
    {
        if (shelf == null || shelf.slots == null || shelf.slots.Length == 0)
        {
            return null;
        }

        var go = new GameObject("ShelfFlourish") { hideFlags = HideFlags.HideAndDontSave };
        var f = go.AddComponent<ShelfFlourish>();
        Active.Add(f);
        f.Begin(shelf);
        return f;
    }

    /// <summary>True while the animation is still running (tests).</summary>
    public bool IsPlaying => !_finished;

    public Renderer Sheet => _sheet;
    public int BookCount => _books != null ? _books.Length : 0;

    private void Begin(Shelf shelf)
    {
        _shelf = shelf;
        _t0 = Time.unscaledTime;
        _block = new MaterialPropertyBlock();

        var books = new System.Collections.Generic.List<Book>();
        var rest = new System.Collections.Generic.List<Vector3>();
        Bounds all = default;
        bool first = true;
        foreach (ShelfSlot slot in shelf.slots)
        {
            if (slot == null || slot.occupant == null)
            {
                continue;
            }

            books.Add(slot.occupant);
            rest.Add(slot.occupant.transform.position);
            var r = slot.occupant.GetComponent<Renderer>();
            if (r != null)
            {
                if (first)
                {
                    all = r.bounds;
                    first = false;
                }
                else
                {
                    all.Encapsulate(r.bounds);
                }
            }
        }

        _books = books.ToArray();
        _restPosition = rest.ToArray();
        if (_books.Length == 0)
        {
            Finish();
            return;
        }

        // the sheet: a quad in front of the spines, as wide as the row and as tall as the tallest book, facing the player
        Transform slotT = shelf.slots[0].transform;
        Vector3 forward = slotT.forward;   // points into the case; the player is on the other side
        var sheet = GameObject.CreatePrimitive(PrimitiveType.Quad);
        sheet.hideFlags = HideFlags.HideAndDontSave;
        Destroy(sheet.GetComponent<Collider>());
        sheet.transform.SetParent(transform, false);
        float width = Mathf.Abs(Vector3.Dot(all.size, slotT.right)) + 0.10f;
        float height = all.size.y + 0.07f;
        sheet.transform.SetPositionAndRotation(new Vector3(all.center.x, all.center.y, all.center.z) - forward * 0.012f - slotT.forward * 0f, Quaternion.LookRotation(forward, Vector3.up));
        // keep the sheet on the front line of the row rather than at the middle of the books' depth
        sheet.transform.position = new Vector3(all.center.x, all.center.y, all.center.z) + Vector3.Project(slotT.position - all.center, forward) - forward * 0.015f;
        sheet.transform.localScale = new Vector3(width, height, 1f);
        _sheet = sheet.GetComponent<Renderer>();
        _sheet.sharedMaterial = SweepMaterial();
        _sheet.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _sheet.receiveShadows = false;

        // gold motes rising from the row
        var ps = new GameObject("Motes") { hideFlags = HideFlags.HideAndDontSave };
        ps.transform.SetParent(transform, false);
        ps.transform.position = all.center - forward * 0.03f;
        _motes = ps.AddComponent<ParticleSystem>();
        ConfigureMotes(_motes, new Vector3(width, height, 0.12f));
        _motes.Play();

        Tick();
    }

    private static void ConfigureMotes(ParticleSystem p, Vector3 box)
    {
        p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = p.main;
        main.loop = false;
        main.duration = 1.2f;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.03f);
        main.gravityModifier = -0.05f;
        main.maxParticles = 160;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.4f, 1f), new Color(1f, 0.68f, 0.25f, 1f));

        var emission = p.emission;
        emission.rateOverTime = 70f;

        var shape = p.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = box;

        var vel = p.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.y = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);

        var col = p.colorOverLifetime;
        col.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
        col.color = fade;

        var renderer = p.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        var assets = FindFirstObjectByType<GlowAssets>();
        if (assets != null && assets.sparkleMaterial != null)
        {
            renderer.sharedMaterial = assets.sparkleMaterial;
        }
    }

    private static Material SweepMaterial()
    {
        if (_sweepMaterial == null)
        {
            _shader = Shader.Find("BookBoy/ShelfSweep");
            _sweepMaterial = new Material(_shader != null ? _shader : Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };
        }

        return _sweepMaterial;
    }

    private void Update()
    {
        Tick();
    }

    /// <summary>Advances the animation to the present moment (public so a test can drive it without frames).</summary>
    public void Tick()
    {
        if (_finished)
        {
            return;
        }

        Tick(Time.unscaledTime - _t0);
    }

    /// <summary>Puts the animation at <paramref name="t"/> seconds (tests and tools).</summary>
    public void Tick(float t)
    {
        if (_finished || _books == null)
        {
            return;
        }

        // the wave of hops and gold flashes along the row
        for (int i = 0; i < _books.Length; i++)
        {
            Book b = _books[i];
            if (b == null)
            {
                continue;
            }

            float local = t - i * WaveStep;
            bool stillHere = b.currentSlot != null && b.currentSlot.shelf == _shelf;
            if (!stillHere)
            {
                continue;   // the player took it out meanwhile: leave it alone
            }

            if (local >= 0f && local <= HopTime)
            {
                float u = local / HopTime;
                b.transform.position = _restPosition[i] + Vector3.up * (HopHeight * Mathf.Sin(u * Mathf.PI));
                if (local < 0.05f && b.Outline != null && !b.Outline.IsFlashing)
                {
                    b.Outline.Flash(Gold, false);
                }
            }
            else if (local > HopTime)
            {
                b.transform.position = _restPosition[i];
            }
        }

        // the sheet
        if (_sheet != null)
        {
            float p = Mathf.Clamp01(t / SweepTime);
            float alpha = Mathf.Clamp01(Mathf.Min(t / 0.12f, (SweepTime + 0.25f - t) / 0.45f));
            _sheet.GetPropertyBlock(_block);
            _block.SetFloat(ProgressId, p);
            _block.SetFloat(AlphaId, alpha);
            _block.SetColor(ColorId, new Color(1.5f, 1.05f, 0.35f, 1f));
            _sheet.SetPropertyBlock(_block);
        }

        if (t > SweepTime + 0.3f + _books.Length * 0.0f && (_motes == null || _motes.particleCount == 0 || t > SweepTime + 1.6f))
        {
            Finish();
        }
    }

    private void Finish()
    {
        if (_finished)
        {
            return;
        }

        _finished = true;
        if (_books != null)
        {
            for (int i = 0; i < _books.Length; i++)
            {
                if (_books[i] != null && _books[i].currentSlot != null && _books[i].currentSlot.shelf == _shelf)
                {
                    _books[i].transform.position = _restPosition[i];
                }
            }
        }

        if (_sheet != null)
        {
            _sheet.enabled = false;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Active.Remove(this);
        if (!_finished && _books != null)
        {
            for (int i = 0; i < _books.Length; i++)
            {
                if (_books[i] != null && _books[i].currentSlot != null && _books[i].currentSlot.shelf == _shelf)
                {
                    _books[i].transform.position = _restPosition[i];
                }
            }
        }
    }
}
