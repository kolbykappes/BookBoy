using BookBoy.Rules;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// G: every book of the held book's series that is not on a shelf (and not in your hands) gets a pulsing outline for a few
/// seconds, so you can spot its sisters in a messy room. Needs the Sister Glow upgrade; recharges on its own 60 s timer
/// (shorter with the recharge upgrades).
/// </summary>
public class SisterGlow : MonoBehaviour
{
    private static readonly Color GlowColor = new Color(0.45f, 0.9f, 1f);

    private GameProgress _progress;
    private PlayerInteractor _interactor;
    private GameUI _ui;

    public float SecondsUntilReady => _progress != null ? _progress.CooldownRemaining(AbilityId.SisterGlow) : 0f;

    private void Start()
    {
        _progress = FindFirstObjectByType<GameProgress>();
        _interactor = FindFirstObjectByType<PlayerInteractor>();
        _ui = FindFirstObjectByType<GameUI>();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame && (_ui == null || !_ui.MenuOpen))
        {
            Activate();
        }
    }

    /// <summary>Returns how many books were lit up.</summary>
    public int Activate()
    {
        if (_progress == null || _interactor == null)
        {
            return 0;
        }

        if (!_progress.Upgrades.HasSisterGlow)
        {
            _ui?.Toast("Sister Glow is locked. Press Tab to unlock it.");
            return 0;
        }

        Book current = _interactor.Carry.Current;
        if (current == null || current.definition == null)
        {
            _ui?.Toast("Hold a book first: Sister Glow lights up its series.");
            return 0;
        }

        if (!_progress.IsAbilityReady(AbilityId.SisterGlow))
        {
            _ui?.Toast("Sister Glow is recharging (" + Mathf.CeilToInt(SecondsUntilReady) + "s).");
            return 0;
        }

        int count = 0;
        float seconds = _progress.Upgrades.GlowSeconds;
        foreach (Book book in FindObjectsByType<Book>(FindObjectsInactive.Exclude))
        {
            if (book == current || book.definition == null || book.currentSlot != null || _interactor.Carry.Contains(book))
            {
                continue;
            }

            if (current.SameGroupAs(book))
            {
                book.Outline?.Pulse(GlowColor, seconds);
                count++;
            }
        }

        if (count > 0)
        {
            _progress.StartCooldown(AbilityId.SisterGlow);
        }

        _ui?.Toast(count > 0
            ? "Sister Glow: " + count + (count == 1 ? " sister book" : " sister books") + " lit up."
            : "Every sister of this book is already shelved or in your hands.");
        return count;
    }
}
