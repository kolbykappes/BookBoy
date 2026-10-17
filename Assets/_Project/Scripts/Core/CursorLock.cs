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
    private void OnEnable()
    {
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
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Lock(true);
        }
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
