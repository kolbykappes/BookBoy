using BookBoy.Rules;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a message once every book is in the right bookcase, on the right shelf, in the right slot. Deliberately not
/// the full Phase 6 "Books Restored: X/N" HUD — just enough to prove the system knows when cleanup is done.
/// Grades are recomputed from live state (claims move as books are added or removed), a few times a second.
/// </summary>
public class CleanupTracker : MonoBehaviour
{
    private const float CheckInterval = 0.25f;

    public Text completionText;

    private Book[] _books;
    private bool _announced;
    private float _nextCheck;

    private void Start()
    {
        _books = Object.FindObjectsByType<Book>(FindObjectsInactive.Exclude);
    }

    private void Update()
    {
        if (_announced || _books == null || _books.Length == 0 || Time.unscaledTime < _nextCheck)
        {
            return;
        }

        _nextCheck = Time.unscaledTime + CheckInterval;

        int correct = 0;
        foreach (Book book in _books)
        {
            if (book != null && book.IsCorrectlyShelved)
            {
                correct++;
            }
        }

        if (!PlacementRules.IsCleanupComplete(_books.Length, correct))
        {
            return;
        }

        _announced = true;
        if (completionText != null)
        {
            completionText.text = "All " + _books.Length + " books restored!";
            completionText.enabled = true;
        }
        Debug.Log("CleanupTracker: all " + _books.Length + " books correctly shelved — cleanup complete.");
    }
}
