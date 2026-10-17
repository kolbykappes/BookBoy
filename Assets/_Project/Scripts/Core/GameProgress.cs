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

    private readonly FinishedTracker _tracker = new FinishedTracker();
    private Book[] _books;
    private Shelf[] _shelves;
    private bool[] _finished;
    private CarryInventory _carry;
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

    private void Start()
    {
        // Re-initialize everything: the Editor may keep component state between Play sessions (scene reload is off).
        _books = FindObjectsByType<Book>(FindObjectsInactive.Exclude);
        _shelves = FindObjectsByType<Shelf>(FindObjectsInactive.Exclude);
        Array.Sort(_shelves, (a, b) => string.CompareOrdinal(ShelfKey(a), ShelfKey(b)));
        _finished = new bool[_shelves.Length];
        _carry = FindFirstObjectByType<CarryInventory>();
        _fpc = FindFirstObjectByType<FirstPersonController>();
        if (_fpc != null)
        {
            _baseMove = _fpc.MoveSpeed;
            _baseSprint = _fpc.SprintSpeed;
        }

        Upgrades.Reset();
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

        int newly = _tracker.Update(_finished);
        if (newly > 0)
        {
            Upgrades.AddPoints(newly);
            PointsEarned?.Invoke(newly);
        }

        bool changed = correct != BooksCorrect || finishedCount != ShelvesFinished || newly > 0;
        BooksCorrect = correct;
        ShelvesFinished = finishedCount;

        if (!_complete && PlacementRules.IsCleanupComplete(_books.Length, correct))
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

    /// <summary>Admin/testing: clears all points and upgrades (shelf points already awarded stay awarded).</summary>
    public void ResetUpgrades()
    {
        Upgrades.Reset();
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
