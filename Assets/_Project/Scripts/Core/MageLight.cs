using UnityEngine;

/// <summary>
/// A floating arcane "magelight": a glowing orb that bobs gently and whose light flickers like a candle. The room is lit by a
/// handful of these (soft warm point lights) instead of one big directional light.
/// </summary>
public class MageLight : MonoBehaviour
{
    public float bobHeight = 0.10f;
    public float bobSpeed = 0.55f;

    [Tooltip("How much the light flickers, as a fraction of its intensity.")]
    public float flicker = 0.07f;

    private Light _light;
    private Vector3 _home;
    private float _baseIntensity;
    private float _phase;

    private void Start()
    {
        _light = GetComponentInChildren<Light>();
        _home = transform.position;
        _baseIntensity = _light != null ? _light.intensity : 0f;
        _phase = (_home.x * 1.7f + _home.z * 2.3f) % 6.28f;
    }

    private void Update()
    {
        float t = Time.time;
        transform.position = _home + Vector3.up * (Mathf.Sin(t * bobSpeed * 2f + _phase) * bobHeight);
        if (_light != null)
        {
            float wobble = Mathf.Sin(t * 7.1f + _phase) * 0.5f + Mathf.Sin(t * 12.7f + _phase * 2f) * 0.3f + Mathf.Sin(t * 3.3f) * 0.2f;
            _light.intensity = _baseIntensity * (1f + flicker * wobble);
        }
    }
}
