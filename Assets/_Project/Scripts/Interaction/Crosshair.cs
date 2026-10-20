using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A small dot at the center of the screen. Its look says what the player is aiming at:
/// idle (nothing), active (something in reach), blocked (in reach but the hands are full: grey), far (a target out of reach). Reject() punches and shakes the dot
/// when a pickup is refused.
/// </summary>
public class Crosshair : MonoBehaviour
{
    public enum State
    {
        Idle,
        Active,
        Blocked,
        Far
    }

    public Image dot;

    private State _state = (State)(-1);
    private float _rejectStart = -10f;
    private const float RejectSeconds = 0.4f;

    /// <summary>The pickup was refused (hands full): the dot punches out and shakes sideways for a moment.</summary>
    public void Reject()
    {
        _rejectStart = Time.unscaledTime;
    }

    public bool IsRejecting => Time.unscaledTime < _rejectStart + RejectSeconds;

    private void Update()
    {
        if (dot == null)
        {
            return;
        }

        float since = Time.unscaledTime - _rejectStart;
        RectTransform rt = dot.rectTransform;
        if (since < RejectSeconds)
        {
            float k = 1f - since / RejectSeconds;
            rt.localScale = Vector3.one * (1f + 0.9f * k);
            rt.anchoredPosition = new Vector2(Mathf.Sin(since * 70f) * 9f * k, 0f);
        }
        else if (rt.localScale != Vector3.one || rt.anchoredPosition != Vector2.zero)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
        }
    }

    public void SetState(State state)
    {
        if (dot == null || state == _state)
        {
            return;
        }

        _state = state;
        switch (state)
        {
            case State.Active:
                Apply(new Color(1f, 0.85f, 0.2f, 1f), 18f);
                break;
            case State.Blocked:
                Apply(new Color(0.62f, 0.62f, 0.62f, 0.95f), 16f);   // grey: in reach, but the hands are full
                break;
            case State.Far:
                Apply(new Color(1f, 1f, 1f, 0.45f), 12f);
                break;
            default:
                Apply(new Color(1f, 1f, 1f, 0.9f), 10f);
                break;
        }
    }

    private void Apply(Color color, float size)
    {
        dot.color = color;
        dot.rectTransform.sizeDelta = new Vector2(size, size);
    }
}
