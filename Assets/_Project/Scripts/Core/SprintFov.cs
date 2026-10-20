using BookBoy.Rules;
using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Owns the camera's field of view while playing: widens the view a little while the player sprints, and zooms in while Z is held (smoothly in and out). Sprint: (docs/unity-practices/30). The kick follows the real horizontal speed, so it eases in and out with the controller's own
/// acceleration and does nothing when Shift is held while standing still or against a wall. It writes base FOV + kick into the scene's Cinemachine lens every frame; the base
/// FOV is whatever the lens had at start (a settings menu changes it with <see cref="SetBaseFov"/>). <see cref="strength"/> (0 to 1) is the player's comfort setting.
/// </summary>
public class SprintFov : MonoBehaviour
{
    [Tooltip("The extra vertical FOV, in degrees, at full sprint speed (a guess to tune by feel: 6 vertical is about 7 horizontal at 16:9).")]
    public float maxKickDegrees = 6f;

    [Tooltip("The player's setting: 0 = off, 1 = full.")]
    [Range(0f, 1f)]
    public float strength = 1f;

    [Tooltip("How quickly the view follows the speed (seconds to settle).")]
    public float smoothTime = 0.16f;

    [Tooltip("How much Z magnifies the view (2.5 = a bit under half the width).")]
    public float zoomFactor = 2.5f;

    [Tooltip("How quickly the zoom follows the key (seconds to settle), in and out.")]
    public float zoomSmoothTime = 0.14f;

    [Tooltip("Look speed is scaled with the zoomed FOV so aiming stays as precise as without the zoom.")]
    public bool scaleLookWithZoom = true;

    private float _zoom;            // 0 = no zoom, 1 = fully zoomed
    private float _zoomVelocity;
    private bool _zoomHeld;
    private float _baseRotationSpeed, _baseLookSensitivity;
    private CameraLookBridge _look;

    /// <summary>How far the zoom has come, 0 to 1 (tests).</summary>
    public float ZoomAmount => _zoom;

    /// <summary>Holds or releases the zoom (the Z key does this in play; tests call it).</summary>
    public void SetZoomHeld(bool held)
    {
        _zoomHeld = held;
    }

    private FirstPersonController _fpc;
    private CharacterController _controller;
    private CinemachineCamera _vcam;
    private float _baseFov;
    private float _kick;
    private float _kickVelocity;
    private bool _ready;

    /// <summary>The extra FOV applied right now, in degrees (tests and tools).</summary>
    public float CurrentKick => _kick;

    public float BaseFov => _baseFov;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        if (_ready)
        {
            return;
        }

        _fpc = GetComponent<FirstPersonController>();
        _look = FindFirstObjectByType<CameraLookBridge>();
        if (_fpc != null)
        {
            _baseRotationSpeed = _fpc.RotationSpeed;
        }

        if (_look != null)
        {
            _baseLookSensitivity = _look.lookSensitivity;
        }

        _controller = GetComponent<CharacterController>();
        var cameraObject = GameObject.Find("PlayerFollowCamera");
        _vcam = cameraObject != null ? cameraObject.GetComponent<CinemachineCamera>() : FindFirstObjectByType<CinemachineCamera>();
        if (_vcam != null)
        {
            _baseFov = _vcam.Lens.FieldOfView;
            _ready = true;
        }
    }

    /// <summary>Sets the FOV the kick is added to (the settings menu's FOV slider).</summary>
    public void SetBaseFov(float fov)
    {
        Setup();
        _baseFov = fov;
        Apply();
    }

    private void LateUpdate()
    {
        if (!_ready || _fpc == null || _controller == null)
        {
            return;
        }

        var kb = UnityEngine.InputSystem.Keyboard.current;
        _zoomHeld = kb != null && kb.zKey.isPressed && Time.timeScale > 0f;   // not while a menu or dialog has paused the game
        Vector3 v = _controller.velocity;
        float speed = new Vector2(v.x, v.z).magnitude;
        Tick(speed, Time.deltaTime);
    }

    /// <summary>One smoothing step toward the kick for <paramref name="horizontalSpeed"/> (public so a test can drive it without frames).</summary>
    public void Tick(float horizontalSpeed, float deltaTime)
    {
        Setup();
        if (!_ready || _fpc == null)
        {
            return;
        }

        float zoomTarget = _zoomHeld ? 1f : 0f;
        if (deltaTime > 0f)
        {
            _zoom = Mathf.SmoothDamp(_zoom, zoomTarget, ref _zoomVelocity, zoomSmoothTime, Mathf.Infinity, deltaTime);
        }

        float target = SprintFovRules.Kick(horizontalSpeed, _fpc.MoveSpeed, _fpc.SprintSpeed, maxKickDegrees, strength);
        if (deltaTime > 0f)
        {
            _kick = Mathf.SmoothDamp(_kick, target, ref _kickVelocity, smoothTime, Mathf.Infinity, deltaTime);
        }

        Apply();
    }

    private void Apply()
    {
        if (_vcam == null)
        {
            return;
        }

        LensSettings lens = _vcam.Lens;
        float fov = _baseFov + _kick;
        // the zoom is applied in magnification, so it eases evenly (a zoom of 1 is none; the factor is reached at 1)
        float magnification = Mathf.Pow(Mathf.Max(1f, zoomFactor), _zoom);
        float zoomed = SprintFovRules.ZoomedFov(fov, magnification);
        lens.FieldOfView = zoomed;
        _vcam.Lens = lens;
        if (scaleLookWithZoom)
        {
            float k = Mathf.Tan(zoomed * 0.5f * Mathf.Deg2Rad) / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            if (_fpc != null && _baseRotationSpeed > 0f)
            {
                _fpc.RotationSpeed = _baseRotationSpeed * k;
            }

            if (_look != null && _baseLookSensitivity > 0f)
            {
                _look.lookSensitivity = _baseLookSensitivity * k;
            }
        }
    }

    private void OnDisable()
    {
        _zoom = 0f;
        _zoomVelocity = 0f;
        _kick = 0f;
        _kickVelocity = 0f;
        Apply();
    }
}
