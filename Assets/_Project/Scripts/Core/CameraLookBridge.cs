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

    [Tooltip("Higher = faster look. 1 felt sluggish in testing; 3 is the new default.")]
    public float lookSensitivity = 3f;

    [Tooltip("Kolby's preferred default: inverted Y (mouse up looks down). The playtested build " +
        "used invertTilt=true and that read as normal (non-inverted), so this flips it. Confirm " +
        "next test that up now looks down — there was no way to verify the sign without a live test.")]
    public bool invertTilt;

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
