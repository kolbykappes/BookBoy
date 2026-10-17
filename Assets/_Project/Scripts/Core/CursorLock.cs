using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Hides and locks the cursor during play. Starter Assets only locks it in OnApplicationFocus,
/// which does not reliably fire in the Editor when entering Play. Cursor.lockState only reports
/// what was requested, not whether the Game view actually holds the lock (Esc or losing focus
/// releases it without changing the property), so every click re-asserts the lock.
/// </summary>
public class CursorLock : MonoBehaviour
{
    /// <summary>While true (a menu is open) the cursor stays free and visible.</summary>
    public bool suspended;

    private void OnEnable()
    {
        suspended = false;
        Lock(false);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !suspended)
        {
            Lock(true);
        }
    }

    private void Update()
    {
        if (!suspended && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Lock(true);
        }
    }

    /// <summary>Frees the cursor for a menu.</summary>
    public void Suspend()
    {
        suspended = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Hides and locks the cursor again after a menu.</summary>
    public void Resume()
    {
        suspended = false;
        Lock(true);
    }

    private static void Lock(bool reassert)
    {
        if (reassert)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
