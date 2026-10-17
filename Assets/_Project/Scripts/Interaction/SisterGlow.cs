using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// G: every book of the held book's series that is not on a shelf (and not in your hands) gets a pulsing outline for a few
/// seconds, so you can spot its sisters in a messy room. Needs the Sister Glow upgrade; has a short cooldown.
/// </summary>
public class SisterGlow : MonoBehaviour
{
    private const float Cooldown = 6f;
    private static readonly Color GlowColor = new Color(0.45f, 0.9f, 1f);

    private GameProgress _progress;
    private PlayerInteractor _interactor;
    private GameUI _ui;
    private float _readyAt;

    public float SecondsUntilReady => Mathf.Max(0f, _readyAt - Time.time);

    private void Start()
    {
        _progress = FindFirstObjectByType<GameProgress>();
        _interactor = FindFirstObjectByType<PlayerInteractor>();
        _ui = FindFirstObjectByType<GameUI>();
        _readyAt = 0f;
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

        if (Time.time < _readyAt)
        {
            _ui?.Toast("Sister Glow is recharging (" + Mathf.CeilToInt(_readyAt - Time.time) + "s).");
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

            if (book.definition.category == current.definition.category && book.definition.series == current.definition.series)
            {
                book.Outline?.Pulse(GlowColor, seconds);
                count++;
            }
        }

        _readyAt = Time.time + Cooldown;
        _ui?.Toast(count > 0
            ? "Sister Glow: " + count + (count == 1 ? " sister book" : " sister books") + " lit up."
            : "Every sister of this book is already shelved or in your hands.");
        return count;
    }
}
