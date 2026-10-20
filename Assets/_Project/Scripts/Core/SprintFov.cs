using BookBoy.Rules;
using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Widens the view a little while the player sprints (docs/unity-practices/30). The kick follows the real horizontal speed, so it eases in and out with the controller's own
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
        lens.FieldOfView = _baseFov + _kick;
        _vcam.Lens = lens;
    }

    private void OnDisable()
    {
        _kick = 0f;
        _kickVelocity = 0f;
        Apply();
    }
}
