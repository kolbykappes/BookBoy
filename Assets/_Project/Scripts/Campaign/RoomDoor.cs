using UnityEngine;
using UnityEngine.UI;

/// <summary>The door to a room: a blocking leaf that slides up when the room is open, and a small sign that says which room it is
/// and why it is shut.</summary>
public class RoomDoor : MonoBehaviour
{
    public int roomNumber;

    [Tooltip("The panel that blocks the doorway.")]
    public Transform leaf;

    public Collider blocker;
    public Text label;

    public float openHeight = 2.6f;

    private Vector3 _closedLocal;
    private bool _captured;
    private bool _open;
    private float _t;

    public bool IsOpen => _open;

    private void Capture()
    {
        if (!_captured && leaf != null)
        {
            _closedLocal = leaf.localPosition;
            _captured = true;
        }
    }

    private void Start()
    {
        Capture();
        Apply(false);
    }

    /// <summary>Opens or shuts the door. With <paramref name="instant"/> it jumps straight there (no slide).</summary>
    public void SetOpen(bool open, bool instant = false)
    {
        Capture();
        _open = open;
        if (instant)
        {
            _t = open ? 1f : 0f;
            Apply(true);
        }
    }

    public void SetLabel(string text)
    {
        if (label != null)
        {
            label.text = text;
        }
    }

    private void Update()
    {
        float target = _open ? 1f : 0f;
        if (!Mathf.Approximately(_t, target))
        {
            _t = Mathf.MoveTowards(_t, target, Time.unscaledDeltaTime / 0.7f);
            Apply(true);
        }
    }

    private void Apply(bool useT)
    {
        if (leaf == null)
        {
            return;
        }

        Capture();
        float t = useT ? _t : (_open ? 1f : 0f);
        leaf.localPosition = _closedLocal + Vector3.up * (openHeight * t);
        if (blocker != null)
        {
            blocker.enabled = t < 0.7f;
        }
    }
}
