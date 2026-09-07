using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Feeds Starter Assets' existing mouse-look input into the Cinemachine PanTilt component
/// driving the actual render camera.
///
/// Pan (yaw) is LOCKED to the Player transform's own rotation rather than accumulated
/// independently. FirstPersonController is already the single source of truth for yaw — it's
/// what WASD movement is relative to — so accumulating a second, separate pan value here (as
/// the first version of this script did) let the visual camera direction and the movement
/// direction drift apart over time: two independent accumulators fed by the same raw mouse
/// delta, with different scaling, compounding a tiny mismatch every frame. Locking pan directly
/// to the player's yaw eliminates that drift entirely.
///
/// Tilt (pitch) doesn't affect movement direction, so it's safe to keep as its own accumulator.
/// </summary>
[RequireComponent(typeof(CinemachinePanTilt))]
public class CameraLookBridge : MonoBehaviour
{
    public StarterAssetsInputs input;

    [Tooltip("The transform FirstPersonController rotates for yaw/movement-facing — normally " +
        "the Player root. Pan is locked to this every frame instead of being accumulated " +
        "separately, so camera-facing can never drift from movement-facing.")]
    public Transform playerYawSource;

    [Tooltip("Higher = faster vertical look. 1 felt sluggish in testing; 3 is the new default.")]
    public float lookSensitivity = 3f;

    [Tooltip("Kolby's preferred default: inverted Y (mouse up looks down). First guess " +
        "(invertTilt=false) tested as still non-inverted, so this flips back to true — the value " +
        "from the original successful pickup test, which was never explicitly confirmed either " +
        "direction at the time. No way to verify the sign without a live human test.")]
    public bool invertTilt = true;

    private CinemachinePanTilt _panTilt;

    private void Awake()
    {
        _panTilt = GetComponent<CinemachinePanTilt>();
    }

    private void LateUpdate()
    {
        if (_panTilt == null)
        {
            return;
        }

        // LateUpdate so FirstPersonController's Update has already applied this frame's yaw.
        if (playerYawSource != null)
        {
            _panTilt.PanAxis.Value = playerYawSource.eulerAngles.y;
        }

        if (input != null)
        {
            float tiltSign = invertTilt ? -1f : 1f;
            _panTilt.TiltAxis.Value += input.look.y * lookSensitivity * tiltSign;
        }
    }
}
