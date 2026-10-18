using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Hides and locks the cursor during play. Starter Assets only locks it in OnApplicationFocus,
/// which does not reliably fire in the Editor when entering Play. Cursor.lockState only reports
/// what was requested, not whether the Game view actually holds the lock (Esc or losing focus
/// releases it without changing the property), so every click re-asserts the lock.
/// Menus do NOT free the Windows cursor (that would break out of the game window): the cursor stays locked and hidden and
/// the menus draw their own pointer (see GameUI), driven by the mouse movement.
/// </summary>
public class CursorLock : MonoBehaviour
{
    /// <summary>True while a menu is open. The cursor stays locked and hidden; the menu uses a software pointer.</summary>
    public bool suspended;

    private void OnEnable()
    {
        suspended = false;
        Lock(false);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Lock(true);
        }
    }

    private void Update()
    {
        if (suspended)
        {
            if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
            {
                Lock(false);
            }
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Lock(true);
        }
    }

    /// <summary>A menu opened: the game pauses, but the cursor stays locked and hidden.</summary>
    public void Suspend()
    {
        suspended = true;
        Lock(false);
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
