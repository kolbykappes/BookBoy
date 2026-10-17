using System.Collections;
using System.Collections.Generic;
using BookBoy.Rules;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// C: pulls loose books of the held book's series (its "family") through the air into your hands, each trailing sparkles.
/// Starts at 2 books; upgrades raise it to 3 and then the whole series. Limited by the free space in your hands (it only
/// pulls as many as fit) and by how many unshelved sisters there are. Needs the Gather Family upgrade; recharges on its own
/// 60 s timer (shorter with the recharge upgrades). It only goes on cooldown when it actually pulls something.
/// </summary>
public class GatherFamily : MonoBehaviour
{
    private static readonly Color PullColor = new Color(0.9f, 0.55f, 1f);

    private GameProgress _progress;
    private PlayerInteractor _interactor;
    private GameUI _ui;
    private GlowAssets _assets;
    private readonly HashSet<Book> _incoming = new HashSet<Book>();
    private int _flying;

    public bool IsBusy => _flying > 0;
    public int IncomingCount => _incoming.Count;

    public float SecondsUntilReady => _progress != null ? _progress.CooldownRemaining(AbilityId.GatherFamily) : 0f;

    private void Start()
    {
        _progress = FindFirstObjectByType<GameProgress>();
        _interactor = FindFirstObjectByType<PlayerInteractor>();
        _ui = FindFirstObjectByType<GameUI>();
        _assets = FindFirstObjectByType<GlowAssets>();
        _incoming.Clear();
        _flying = 0;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame && (_ui == null || !_ui.MenuOpen))
        {
            Activate();
        }
    }

    /// <summary>Returns how many books were sent flying to the hands.</summary>
    public int Activate()
    {
        if (_progress == null || _interactor == null)
        {
            return 0;
        }

        if (!_progress.Upgrades.HasGatherFamily)
        {
            _ui?.Toast("Gather Family is locked. Press Tab to unlock it.");
            return 0;
        }

        CarryInventory carry = _interactor.Carry;
        Book current = carry.Current;
        if (current == null || current.definition == null)
        {
            _ui?.Toast("Hold a book first: Gather Family pulls the rest of its series to you.");
            return 0;
        }

        if (!_progress.IsAbilityReady(AbilityId.GatherFamily))
        {
            _ui?.Toast("Gather Family is recharging (" + Mathf.CeilToInt(SecondsUntilReady) + "s).");
            return 0;
        }

        int free = carry.Capacity - carry.Count - _incoming.Count;
        var sisters = new List<Book>();
        foreach (Book book in FindObjectsByType<Book>(FindObjectsInactive.Exclude))
        {
            if (book == current || book.definition == null || book.currentSlot != null || carry.Contains(book) || _incoming.Contains(book))
            {
                continue;
            }

            if (book.definition.category == current.definition.category && book.definition.series == current.definition.series)
            {
                sisters.Add(book);
            }
        }

        int count = GatherRules.PullCount(_progress.Upgrades.GatherCount, free, sisters.Count);
        if (count == 0)
        {
            _ui?.Toast(free <= 0 ? "Hands full: no room to gather more." : "Every sister of this book is already shelved or in your hands.");
            return 0;
        }

        // closest first: the nearest sisters arrive first
        Vector3 from = _interactor.interactionCamera != null ? _interactor.interactionCamera.transform.position : transform.position;
        sisters.Sort((a, b) => (a.transform.position - from).sqrMagnitude.CompareTo((b.transform.position - from).sqrMagnitude));

        float delay = 0f;
        for (int i = 0; i < count; i++)
        {
            _incoming.Add(sisters[i]);
            StartCoroutine(Fly(sisters[i], delay));
            delay += 0.14f;
        }

        _progress.StartCooldown(AbilityId.GatherFamily);
        _ui?.Toast("Gather Family: " + count + (count == 1 ? " book" : " books") + " coming to you!");
        return count;
    }

    private IEnumerator Fly(Book book, float delay)
    {
        _flying++;
        CarryInventory carry = _interactor.Carry;
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (book == null)
        {
            _incoming.Remove(book);
            _flying--;
            yield break;
        }

        Transform t = book.transform;
        book.StopPhysicsTumble();
        Collider bookCollider = book.GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.enabled = false;
        }

        ParticleSystem trail = MakeTrail(t);
        book.Outline?.Pulse(PullColor, 1.6f);

        Vector3 start = t.position;
        Quaternion startRot = t.rotation;
        Vector3 spinAxis = Random.onUnitSphere;
        Vector3 side = Vector3.Cross(Vector3.up, (HandPoint() - start).normalized);
        if (side.sqrMagnitude < 0.01f)
        {
            side = Vector3.right;
        }

        side.Normalize();
        float swirl = Random.Range(0.35f, 0.7f) * (Random.value < 0.5f ? -1f : 1f);
        float distance = Vector3.Distance(start, HandPoint());
        float duration = Mathf.Clamp(0.55f + distance * 0.06f, 0.65f, 1.1f);
        float lift = 0.5f + distance * 0.08f;

        float elapsed = 0f;
        while (elapsed < duration && book != null)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            float ease = u * u * (3f - 2f * u);
            Vector3 end = HandPoint();   // the hands keep moving while the book flies
            Vector3 control = Vector3.Lerp(start, end, 0.5f) + Vector3.up * lift + side * swirl;
            // quadratic Bezier start -> control -> end
            Vector3 a = Vector3.Lerp(start, control, ease);
            Vector3 b = Vector3.Lerp(control, end, ease);
            t.position = Vector3.Lerp(a, b, ease);
            t.rotation = Quaternion.AngleAxis(540f * u, spinAxis) * startRot;
            yield return null;
        }

        if (trail != null)
        {
            var emission = trail.emission;
            emission.enabled = false;
            Destroy(trail.gameObject, 1.2f);
        }

        if (book != null)
        {
            if (bookCollider != null)
            {
                bookCollider.enabled = true;
            }

            if (!carry.Contains(book) && !carry.TryPickUp(book))
            {
                // the hands filled up while it was in the air: let it drop where it is
                book.Toss(t.position, Vector3.zero, Random.insideUnitSphere);
            }
        }

        _incoming.Remove(book);
        _flying--;
    }

    private Vector3 HandPoint()
    {
        CarryInventory carry = _interactor.Carry;
        return carry.carryPoint != null ? carry.carryPoint.position : _interactor.transform.position + Vector3.up;
    }

    /// <summary>Sparkles left behind the book as it flies: a stream of glowing motes that fade over about a second.</summary>
    private ParticleSystem MakeTrail(Transform parent)
    {
        var go = new GameObject("GatherTrail") { hideFlags = HideFlags.HideAndDontSave };
        go.transform.SetParent(parent, false);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.03f, 0.18f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.055f);
        main.startColor = new ParticleSystem.MinMaxGradient(PullColor, new Color(1f, 0.85f, 1f));
        main.gravityModifier = -0.05f;
        main.maxParticles = 200;

        var emission = ps.emission;
        emission.rateOverTime = 40f;
        emission.rateOverDistance = 90f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.06f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.8f, 0.5f, 1f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = fade;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (_assets != null && _assets.sparkleMaterial != null)
        {
            renderer.sharedMaterial = _assets.sparkleMaterial;
        }

        ps.Play();
        return ps;
    }
}
