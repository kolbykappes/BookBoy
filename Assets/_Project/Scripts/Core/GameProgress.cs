using System;
using BookBoy.Rules;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The run: a timer (your score is the time it takes to finish), how many books are correctly placed, how many shelves are
/// finished, the upgrade points earned (1 per finished shelf) and the upgrades bought. Applies upgrades to the player.
/// Grades are recomputed from live state a few times a second.
/// </summary>
public class GameProgress : MonoBehaviour
{
    private const float CheckInterval = 0.25f;

    [Tooltip("Text shown when every book is in its right place (replaces the old CleanupTracker message).")]
    public Text completionText;

    public readonly UpgradeState Upgrades = new UpgradeState();

    /// <summary>One recharge timer per ability (Time.time based, so menus that pause the game pause the timers too).</summary>
    public readonly AbilityTimers Timers = new AbilityTimers();

    private readonly FinishedTracker _tracker = new FinishedTracker();
    private Book[] _books;
    private Shelf[] _shelves;
    private bool[] _finished;
    private CarryInventory _carry;
    private CampaignController _campaign;
    private FirstPersonController _fpc;
    private float _baseMove;
    private float _baseSprint;
    private double _elapsed;
    private double _finalTime;
    private bool _complete;
    private float _nextCheck;

    public event Action Changed;
    public event Action<int> PointsEarned;

    public double ElapsedSeconds => _complete ? _finalTime : _elapsed;
    public bool LevelComplete => _complete;
    public int BooksCorrect { get; private set; }
    public int BooksTotal => _books != null ? _books.Length : 0;
    public int ShelvesFinished { get; private set; }
    public int ShelvesTotal => _shelves != null ? _shelves.Length : 0;
    public CarryInventory Carry => _carry;

    /// <summary>The Lamplit campaign, if this scene has one. In a campaign scene shelves do not earn points and finds (not the tech
    /// tree) switch abilities on.</summary>
    public CampaignController Campaign => _campaign;

    public void RaiseChanged()
    {
        Changed?.Invoke();
    }

    private void Start()
    {
        // Re-initialize everything: the Editor may keep component state between Play sessions (scene reload is off).
        _books = FindObjectsByType<Book>(FindObjectsInactive.Exclude);
        _shelves = FindObjectsByType<Shelf>(FindObjectsInactive.Exclude);
        Array.Sort(_shelves, (a, b) => string.CompareOrdinal(ShelfKey(a), ShelfKey(b)));
        _finished = new bool[_shelves.Length];
        _carry = FindFirstObjectByType<CarryInventory>();
        _campaign = FindFirstObjectByType<CampaignController>();
        _fpc = FindFirstObjectByType<FirstPersonController>();
        if (_fpc != null)
        {
            _baseMove = _fpc.MoveSpeed;
            _baseSprint = _fpc.SprintSpeed;
        }

        Upgrades.Reset();
        Timers.Reset();
        _tracker.Reset();
        _elapsed = 0;
        _finalTime = 0;
        _complete = false;
        BooksCorrect = 0;
        ShelvesFinished = 0;
        _nextCheck = 0f;
        if (completionText != null)
        {
            completionText.enabled = false;
        }

        ApplyUpgrades();
        Recompute();
    }

    private static string ShelfKey(Shelf s)
    {
        return s.bookcase != null ? s.bookcase.name + "/" + s.name : s.name;
    }

    private void Update()
    {
        if (!_complete)
        {
            _elapsed += Time.deltaTime;
        }

        if (Time.unscaledTime >= _nextCheck)
        {
            _nextCheck = Time.unscaledTime + CheckInterval;
            Recompute();
        }
    }

    /// <summary>Recounts correct books and finished shelves, awards points for newly finished shelves, and checks for completion.</summary>
    public void Recompute()
    {
        if (_books == null)
        {
            return;
        }

        int correct = 0;
        foreach (Book book in _books)
        {
            if (book != null && book.IsCorrectlyShelved)
            {
                correct++;
            }
        }

        int finishedCount = 0;
        for (int i = 0; i < _shelves.Length; i++)
        {
            _finished[i] = IsFinished(_shelves[i]);
            if (_finished[i])
            {
                finishedCount++;
            }
        }

        int newly = _campaign == null ? _tracker.Update(_finished) : 0;
        if (newly > 0)
        {
            Upgrades.AddPoints(newly);
            PointsEarned?.Invoke(newly);
        }

        bool changed = correct != BooksCorrect || finishedCount != ShelvesFinished || newly > 0;
        BooksCorrect = correct;
        ShelvesFinished = finishedCount;

        if (!_complete && _campaign == null && PlacementRules.IsCleanupComplete(_books.Length, correct))
        {
            _complete = true;
            _finalTime = _elapsed;
            if (completionText != null)
            {
                completionText.text = "All " + _books.Length + " books restored!   Time " + TimeFormat.Format(_finalTime);
                completionText.enabled = true;
            }

            Debug.Log("GameProgress: all " + _books.Length + " books correctly shelved in " + TimeFormat.Format(_finalTime));
            changed = true;
        }

        if (changed)
        {
            Changed?.Invoke();
        }
    }

    private static bool IsFinished(Shelf shelf)
    {
        foreach (ShelfSlot slot in shelf.slots)
        {
            if (slot.occupant == null || !slot.occupant.IsCorrectlyShelved)
            {
                return false;
            }
        }

        return shelf.slots.Length > 0;
    }

    public bool TryBuy(string id)
    {
        if (!Upgrades.TryUnlock(id))
        {
            return false;
        }

        ApplyUpgrades();
        Changed?.Invoke();
        return true;
    }

    /// <summary>Admin/testing: grants points.</summary>
    public void AddPoints(int amount)
    {
        Upgrades.AddPoints(amount);
        Changed?.Invoke();
    }

    /// <summary>Admin/testing: unlocks every upgrade for free.</summary>
    public void UnlockAll()
    {
        Upgrades.UnlockAll();
        ApplyUpgrades();
        Changed?.Invoke();
    }

    /// <summary>Seconds until the ability can be used again; 0 when it is ready (or locked).</summary>
    public float CooldownRemaining(AbilityId id)
    {
        return (float)Timers.Remaining(id, Time.time, Upgrades.CooldownSeconds(id));
    }

    /// <summary>Unlocked and off cooldown.</summary>
    public bool IsAbilityReady(AbilityId id)
    {
        return Upgrades.HasAbility(id) && CooldownRemaining(id) <= 0f;
    }

    /// <summary>Puts the ability on its (upgrade-adjusted) cooldown. Called only when the ability actually did something.</summary>
    public void StartCooldown(AbilityId id)
    {
        Timers.Start(id, Time.time, Upgrades.CooldownSeconds(id));
        Changed?.Invoke();
    }

    /// <summary>Admin/testing: every ability is ready again.</summary>
    public void ResetCooldowns()
    {
        Timers.Reset();
        Changed?.Invoke();
    }

    /// <summary>Admin/testing: clears all points and upgrades (shelf points already awarded stay awarded).</summary>
    public void ResetUpgrades()
    {
        Upgrades.Reset();
        Timers.Reset();
        ApplyUpgrades();
        Changed?.Invoke();
    }

    public void ApplyUpgrades()
    {
        if (_carry != null)
        {
            _carry.SetCapacity(Upgrades.CarryCapacity);
        }

        if (_fpc != null)
        {
            _fpc.MoveSpeed = _baseMove * Upgrades.MoveSpeedMultiplier;
            _fpc.SprintSpeed = _baseSprint * Upgrades.MoveSpeedMultiplier;
        }
    }
}
