using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Watches every ShelfSlot in the scene and shows a message once all of them are occupied.
/// Deliberately not the full Phase 6 "Books Restored: X/N" HUD — just enough to prove the
/// system knows when cleanup is actually finished.
/// </summary>
public class CleanupTracker : MonoBehaviour
{
    public Text completionText;

    private ShelfSlot[] _slots;
    private bool _announced;

    private void Start()
    {
        _slots = Object.FindObjectsByType<ShelfSlot>(FindObjectsInactive.Exclude);
    }

    private void Update()
    {
        if (_announced || _slots == null || _slots.Length == 0)
        {
            return;
        }

        foreach (ShelfSlot slot in _slots)
        {
            if (slot == null || !slot.occupied)
            {
                return;
            }
        }

        _announced = true;
        if (completionText != null)
        {
            completionText.text = "All " + _slots.Length + " books restored!";
            completionText.enabled = true;
        }
        Debug.Log("CleanupTracker: all " + _slots.Length + " shelf slots occupied — cleanup complete.");
    }
}
