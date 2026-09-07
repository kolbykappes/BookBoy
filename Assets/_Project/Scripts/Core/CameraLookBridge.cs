using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Feeds Starter Assets' existing mouse-look input into the Cinemachine PanTilt component
/// driving the actual render camera.
///
/// Without this, FirstPersonController correctly rotates PlayerCameraRoot from mouse input
/// (confirmed: StarterAssetsInputs.look updates fine, and PlayerCameraRoot's rotation changes),
/// but the Cinemachine camera that's actually rendered (PlayerFollowCamera -> MainCamera via
/// CinemachineBrain) never rotates, because CinemachinePanTilt has no input source of its own
/// wired up (PanAxis/TiltAxis stay at 0 forever). This bridges the two so there's a single
/// source of truth for look input rather than a second, separate Cinemachine input reader.
/// </summary>
[RequireComponent(typeof(CinemachinePanTilt))]
public class CameraLookBridge : MonoBehaviour
{
    public StarterAssetsInputs input;
    public float lookSensitivity = 1f;
    public bool invertTilt = true;

    private CinemachinePanTilt _panTilt;

    private void Awake()
    {
        _panTilt = GetComponent<CinemachinePanTilt>();
    }

    private void Update()
    {
        if (_panTilt == null || input == null)
        {
            return;
        }

        float tiltSign = invertTilt ? -1f : 1f;
        _panTilt.PanAxis.Value += input.look.x * lookSensitivity;
        _panTilt.TiltAxis.Value += input.look.y * lookSensitivity * tiltSign;
    }
}
