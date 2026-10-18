using System.Collections.Generic;
using BookBoy.Rules;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// B: the bookcase that the held book belongs to glows around its outer edges (a pulsing golden frame and a soft light) for a
/// few seconds, so you know where to go. Needs the Home Beacon upgrade; recharges on its own 60 s timer (shorter with the
/// recharge upgrades). Only goes on cooldown when it lights something up.
/// </summary>
public class HomeBeacon : MonoBehaviour
{
    private static readonly Color BeaconColor = new Color(1f, 0.82f, 0.3f);
    private const float Margin = 0.02f;

    private sealed class Frame
    {
        public GameObject Root;
        public LineRenderer[] Edges;
        public Light Light;
        public float Until;
        public float Start;
        public Bookcase Target;
    }

    private GameProgress _progress;
    private PlayerInteractor _interactor;
    private GameUI _ui;
    private GlowAssets _assets;
    private readonly List<Frame> _frames = new List<Frame>();

    public float SecondsUntilReady => _progress != null ? _progress.CooldownRemaining(AbilityId.HomeBeacon) : 0f;

    /// <summary>The bookcases that are glowing right now (also used by tests).</summary>
    public IEnumerable<Bookcase> LitBookcases
    {
        get
        {
            foreach (Frame f in _frames)
            {
                yield return f.Target;
            }
        }
    }

    private void Start()
    {
        _progress = FindFirstObjectByType<GameProgress>();
        _interactor = FindFirstObjectByType<PlayerInteractor>();
        _ui = FindFirstObjectByType<GameUI>();
        _assets = FindFirstObjectByType<GlowAssets>();
        ClearAll();
    }

    private void OnDisable()
    {
        ClearAll();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame && (_ui == null || !_ui.MenuOpen))
        {
            Activate();
        }

        for (int i = _frames.Count - 1; i >= 0; i--)
        {
            Frame f = _frames[i];
            if (Time.time >= f.Until)
            {
                Destroy(f.Root);
                _frames.RemoveAt(i);
                continue;
            }

            float age = Time.time - f.Start;
            float left = f.Until - Time.time;
            float fade = Mathf.Clamp01(Mathf.Min(age / 0.25f, left / 0.6f));
            float pulse = 0.65f + 0.35f * Mathf.Sin(Time.time * 5f);
            Color c = BeaconColor;
            c.a = fade * pulse;
            foreach (LineRenderer line in f.Edges)
            {
                line.startColor = c;
                line.endColor = c;
                line.widthMultiplier = 0.028f * (0.8f + 0.4f * pulse);
            }

            if (f.Light != null)
            {
                f.Light.intensity = 0.75f * fade * pulse;
            }
        }
    }

    /// <summary>Returns true if a bookcase was lit.</summary>
    public bool Activate()
    {
        if (_progress == null || _interactor == null)
        {
            return false;
        }

        if (!_progress.Upgrades.HasHomeBeacon)
        {
            _ui?.Toast("Home Beacon is locked. Press Tab to unlock it.");
            return false;
        }

        Book current = _interactor.Carry.Current;
        if (current == null || current.definition == null)
        {
            _ui?.Toast("Hold a book first: Home Beacon shows where it belongs.");
            return false;
        }

        if (!_progress.IsAbilityReady(AbilityId.HomeBeacon))
        {
            _ui?.Toast("Home Beacon is recharging (" + Mathf.CeilToInt(SecondsUntilReady) + "s).");
            return false;
        }

        Bookcase target = null;
        foreach (Bookcase bookcase in FindObjectsByType<Bookcase>(FindObjectsInactive.Exclude))
        {
            if (bookcase.theme == current.definition.category)
            {
                target = bookcase;
                break;
            }
        }

        if (target == null)
        {
            _ui?.Toast("No bookcase holds this book.");
            return false;
        }

        Show(target, _progress.Upgrades.BeaconSeconds);
        _progress.StartCooldown(AbilityId.HomeBeacon);
        _ui?.Toast("Home Beacon: " + BookThemes.ThemeName(target.theme) + " belongs in the glowing bookcase.");
        return true;
    }

    private void Show(Bookcase target, float seconds)
    {
        for (int i = _frames.Count - 1; i >= 0; i--)
        {
            if (_frames[i].Target == target)
            {
                Destroy(_frames[i].Root);
                _frames.RemoveAt(i);
            }
        }

        Bounds bounds = BookcaseBounds(target);
        bounds.Expand(Margin * 2f);
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        var root = new GameObject("BeaconFrame_" + target.name);
        var corners = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            corners[i] = new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
        }

        // the 12 edges of the box: corners that differ in exactly one axis bit
        var edges = new List<LineRenderer>();
        for (int a = 0; a < 8; a++)
        {
            for (int bit = 1; bit <= 4; bit <<= 1)
            {
                int b = a ^ bit;
                if (b < a)
                {
                    continue;
                }

                var go = new GameObject("Edge");
                go.transform.SetParent(root.transform, false);
                var line = go.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 2;
                line.SetPosition(0, corners[a]);
                line.SetPosition(1, corners[b]);
                line.numCapVertices = 4;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                if (_assets != null && _assets.beaconMaterial != null)
                {
                    line.sharedMaterial = _assets.beaconMaterial;
                }

                edges.Add(line);
            }
        }

        var lightGo = new GameObject("BeaconLight");
        lightGo.transform.SetParent(root.transform, false);
        lightGo.transform.position = new Vector3(bounds.center.x, bounds.center.y, bounds.min.z - 0.5f);
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = BeaconColor;
        light.range = 2.2f;
        light.shadows = LightShadows.None;
        light.intensity = 0f;

        _frames.Add(new Frame { Root = root, Edges = edges.ToArray(), Light = light, Start = Time.time, Until = Time.time + seconds, Target = target });
    }

    private static Bounds BookcaseBounds(Bookcase bookcase)
    {
        Renderer[] renderers = bookcase.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(bookcase.transform.position, Vector3.zero);
        bool first = true;
        foreach (Renderer r in renderers)
        {
            if (first)
            {
                bounds = r.bounds;
                first = false;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        return bounds;
    }

    private void ClearAll()
    {
        foreach (Frame f in _frames)
        {
            if (f.Root != null)
            {
                Destroy(f.Root);
            }
        }

        _frames.Clear();
    }
}
