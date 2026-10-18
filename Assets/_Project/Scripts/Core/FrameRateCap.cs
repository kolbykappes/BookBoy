using UnityEngine;

/// <summary>
/// Caps the frame rate to the display's refresh rate when VSync is off. Uncapped, the game ran at 200+ fps in the
/// Editor and saturated the GPU/CPU, which queued input behind rendering and made mouse-look feel laggy and draggy.
/// Starts itself; no scene object needed. Does nothing if VSync or an explicit target is already set.
/// </summary>
public static class FrameRateCap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        if (QualitySettings.vSyncCount != 0 || Application.targetFrameRate > 0)
        {
            return;
        }

        int refresh = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
        Application.targetFrameRate = Mathf.Clamp(refresh, 30, 240);
    }
}
