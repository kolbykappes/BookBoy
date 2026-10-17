using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A small dot at the center of the screen. Its look says what the player is aiming at:
/// idle (nothing), active (something in reach), blocked (in reach but not usable), far (a target out of reach).
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
                Apply(new Color(1f, 0.35f, 0.3f, 1f), 18f);
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
