using System.Collections;
using System.Collections.Generic;
using BookBoy.Rules;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// F: every held book flies (along an arc, spinning, trailing a glow) to its right place on a shelf and snaps in. Needs the
/// Quick Shelve upgrade and recharges on its own 60 s timer (shorter with the recharge upgrades). Where a book belongs comes from the engine-free ShelfPlanner: the bookcase and shelf that already
/// hold its color and series, else the first unclaimed ones; volume N goes in slot N. Books with no valid place stay in hand.
/// </summary>
public class QuickShelve : MonoBehaviour
{
    private static readonly Color TrailColor = new Color(0.45f, 0.9f, 1f);

    private GameProgress _progress;
    private PlayerInteractor _interactor;
    private GameUI _ui;
    private int _flying;

    public bool IsBusy => _flying > 0;

    public float SecondsUntilReady => _progress != null ? _progress.CooldownRemaining(AbilityId.QuickShelve) : 0f;

    private void Start()
    {
        _progress = FindFirstObjectByType<GameProgress>();
        _interactor = FindFirstObjectByType<PlayerInteractor>();
        _ui = FindFirstObjectByType<GameUI>();
        _flying = 0;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame && (_ui == null || !_ui.MenuOpen))
        {
            Run();
        }
    }

    /// <summary>Returns how many books were sent flying.</summary>
    public int Run()
    {
        if (_progress == null || _interactor == null)
        {
            return 0;
        }

        if (!_progress.Upgrades.HasQuickShelve)
        {
            _ui?.Toast("Quick Shelve is locked. Press Tab to unlock it.");
            return 0;
        }

        CarryInventory carry = _interactor.Carry;
        if (carry.IsEmpty)
        {
            _ui?.Toast("Nothing to shelve: pick up some books first.");
            return 0;
        }

        if (IsBusy)
        {
            return 0;
        }

        if (!_progress.IsAbilityReady(AbilityId.QuickShelve))
        {
            _ui?.Toast("Quick Shelve is recharging (" + Mathf.CeilToInt(SecondsUntilReady) + "s).");
            return 0;
        }

        // snapshot of the shelves right now
        Bookcase[] cases = FindObjectsByType<Bookcase>(FindObjectsInactive.Exclude);
        System.Array.Sort(cases, (a, b) => string.CompareOrdinal(a.name, b.name));
        var caseColor = new int[cases.Length];
        var shelfSeries = new int[cases.Length][];
        var taken = new bool[cases.Length][][];
        for (int c = 0; c < cases.Length; c++)
        {
            caseColor[c] = cases[c].TryGetColorClaim(out int color, out _) ? color : PlacementRules.NoClaim;
            shelfSeries[c] = new int[cases[c].shelves.Length];
            taken[c] = new bool[cases[c].shelves.Length][];
            for (int s = 0; s < cases[c].shelves.Length; s++)
            {
                Shelf shelf = cases[c].shelves[s];
                shelfSeries[c][s] = shelf.TryGetSeriesClaim(out int series, out _) ? series : PlacementRules.NoClaim;
                taken[c][s] = new bool[shelf.slots.Length];
                for (int k = 0; k < shelf.slots.Length; k++)
                {
                    taken[c][s][k] = shelf.slots[k].occupied;
                }
            }
        }

        var held = new List<Book>(carry.Items);
        int sent = 0;
        int left = 0;
        float delay = 0f;
        foreach (Book book in held)
        {
            if (book == null || book.definition == null)
            {
                left++;
                continue;
            }

            PlanTarget target = ShelfPlanner.Plan((int)book.definition.category, book.definition.series, book.definition.volume, caseColor, shelfSeries, taken);
            if (!target.Valid)
            {
                left++;
                continue;
            }

            ShelfPlanner.Commit(target, (int)book.definition.category, book.definition.series, caseColor, shelfSeries, taken);
            ShelfSlot slot = cases[target.Bookcase].shelves[target.Shelf].slots[target.Slot];
            slot.reserved = true;
            StartCoroutine(Fly(book, slot, delay));
            delay += 0.13f;
            sent++;
        }

        if (sent > 0)
        {
            _progress.StartCooldown(AbilityId.QuickShelve);
        }

        if (sent == 0)
        {
            _ui?.Toast("No book in your hands has a free place to go.");
        }
        else if (left > 0)
        {
            _ui?.Toast("Quick Shelve: " + sent + " flying, " + left + " could not be placed.");
        }
        else
        {
            _ui?.Toast("Quick Shelve: " + sent + (sent == 1 ? " book" : " books") + " flying home!");
        }

        return sent;
    }

    private IEnumerator Fly(Book book, ShelfSlot slot, float delay)
    {
        _flying++;
        CarryInventory carry = _interactor.Carry;
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (book == null || !carry.Contains(book))
        {
            // the player placed or tossed it in the meantime
            slot.reserved = false;
            _flying--;
            yield break;
        }

        Transform t = book.transform;
        Vector3 carriedScale = t.localScale;
        carry.Release(book, out Vector3 realScale);
        t.localScale = carriedScale;

        Vector3 startPos = t.position;
        Quaternion startRot = t.rotation;
        Vector3 endPos = slot.transform.position + Vector3.up * (realScale.y * 0.5f) + slot.transform.forward * (realScale.z * 0.5f);
        Quaternion endRot = slot.transform.rotation;
        float distance = Vector3.Distance(startPos, endPos);
        float duration = Mathf.Clamp(0.5f + distance * 0.1f, 0.6f, 1.1f);
        float arc = 0.25f + distance * 0.08f;

        BookOutline glow = book.Outline;
        glow?.Pulse(TrailColor, duration + 0.15f);

        float elapsed = 0f;
        while (elapsed < duration && book != null)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);
            float ease = u * u * (3f - 2f * u);
            t.position = Vector3.Lerp(startPos, endPos, ease) + Vector3.up * (Mathf.Sin(u * Mathf.PI) * arc);
            t.rotation = Quaternion.AngleAxis(360f * 2f * u, Vector3.up) * Quaternion.Slerp(startRot, endRot, ease);
            t.localScale = Vector3.Lerp(carriedScale, realScale, ease);
            yield return null;
        }

        if (book != null)
        {
            slot.Accept(book, realScale);
        }

        _flying--;
    }
}
