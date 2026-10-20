using System;
using System.Collections.Generic;
using BookBoy.Rules;
using UnityEngine;

/// <summary>
/// The books the player is carrying, shown as a stack of flat books at the lower right (after Librarian) with the "current" book on top; cycling
/// swaps which one is current. The logic (capacity, cycling) lives in the engine-free CarryList.
/// </summary>
public class CarryInventory : MonoBehaviour
{
    public Transform carryPoint;

    [Tooltip("Carried books are shown at this fraction of their real size (1 = the size they have on a shelf).")]
    public float carryScale = 1f;

    [Tooltip("Half the length of the stack's footprint along the player's forward (half a book's height).")]
    public float stackHalfLength = 0.19f;

    [Header("Held stack (Librarian-style: it moves with the view)")]
    [Tooltip("Where the middle of the top book sits relative to the eye, in the view's space (right, up, forward). At a level gaze only the near corner of the stack is in frame.")]
    public Vector3 eyeOffset = new Vector3(0.40f, -0.27f, 0.52f);

    [Tooltip("How much of the camera's pitch the stack follows: 1 = fixed in the view like a weapon model, 0 = fixed in the room. Librarian's stack stays low at the right when looking level or up and rises into the right third when looking down, which a value around 0.7 gives.")]
    [Range(0f, 1f)]
    public float pitchFollow = 0.7f;

    [Tooltip("The same when looking UP (higher: the stack stays in the corner while the player reads the upper shelves).")]
    [Range(0f, 1f)]
    public float pitchFollowUp = 0.95f;

    [Tooltip("The smallest the stack shrinks toward the eye to stop it poking through a bookcase or wall (Epic's 'first person scale': position and size scale together, so the picture stays the same).")]
    [Range(0.2f, 1f)]
    public float minStackScale = 0.4f;

    private float _fit = 1f;   // 1 = full size; smaller when something is close in front

    private readonly CarryList<Book> _list = new CarryList<Book>(UpgradeCatalog.BaseCarryCapacity);
    private readonly Dictionary<Book, Vector3> _originalScale = new Dictionary<Book, Vector3>();

    public event Action Changed;

    public int Capacity => _list.Capacity;
    public int Count => _list.Count;
    public bool IsFull => _list.IsFull;
    public bool IsEmpty => _list.IsEmpty;
    public Book Current => _list.Current;

    /// <summary>The book's real (un-shrunk) size while it is carried: the size it will have on a shelf. Used to draw the placement ghost.</summary>
    public Vector3 RealScale(Book book)
    {
        if (book == null)
        {
            return Vector3.one;
        }

        return _originalScale.TryGetValue(book, out Vector3 s) ? s : book.transform.localScale / carryScale;
    }
    public int CurrentIndex => _list.CurrentIndex;
    public IReadOnlyList<Book> Items => _list.Items;

    public bool Contains(Book book)
    {
        return _list.Contains(book);
    }

    public void SetCapacity(int capacity)
    {
        _list.SetCapacity(capacity);
        Changed?.Invoke();
    }

    /// <summary>Takes a book into the hands (from the floor or from a shelf). False if the hands are full.</summary>
    public bool TryPickUp(Book book)
    {
        if (book == null || _list.IsFull || _list.Contains(book))
        {
            return false;
        }

        if (book.currentSlot != null)
        {
            book.currentSlot.Release();
        }

        book.StopPhysicsTumble();
        book.Outline?.StopAll();
        _originalScale[book] = book.transform.localScale;

        Collider bookCollider = book.GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.enabled = false;
        }

        Rigidbody rb = book.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        _list.TryAdd(book);
        Transform t = book.transform;
        Vector3 worldPos = t.position;
        Quaternion worldRot = t.rotation;
        t.SetParent(carryPoint, worldPositionStays: true);
        t.position = worldPos;
        t.rotation = worldRot;
        Changed?.Invoke();
        return true;
    }

    /// <summary>Removes a book from the hands (to place, toss, or fly it). Returns its real (uncarried) scale.</summary>
    public bool Release(Book book, out Vector3 originalScale)
    {
        originalScale = Vector3.one;
        if (book == null || !_list.Remove(book))
        {
            return false;
        }

        if (_originalScale.TryGetValue(book, out Vector3 scale))
        {
            originalScale = scale;
            _originalScale.Remove(book);
        }
        else
        {
            originalScale = book.transform.localScale / carryScale;
        }

        book.transform.SetParent(null, worldPositionStays: true);
        book.transform.localScale = originalScale;
        Changed?.Invoke();
        return true;
    }

    public void Cycle(int direction)
    {
        if (_list.Count < 2)
        {
            return;
        }

        _list.Cycle(direction);
        Changed?.Invoke();
    }

    /// <summary>Puts every carried book at its place in the stack at once (tests and tools; in play the books glide there).</summary>
    public void SnapStack()
    {
        _fit = ComputeFit();
        PlaceAnchor();
        ArrangeStack(1f);
    }

    private void LateUpdate()
    {
        if (_list.Count == 0)
        {
            return;
        }

        float blend = 1f - Mathf.Exp(-16f * Time.unscaledDeltaTime);
        _fit = Mathf.Lerp(_fit, ComputeFit(), 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
        PlaceAnchor();
        ArrangeStack(blend);
    }

    private Camera ViewCamera
    {
        get
        {
            var pi = GetComponent<PlayerInteractor>();
            return pi != null && pi.interactionCamera != null ? pi.interactionCamera : Camera.main;
        }
    }

    /// <summary>The camera's downward pitch in degrees (positive = looking down) and the stack's rotation in the player's space: the player root turns with the yaw,
    /// the stack follows <see cref="pitchFollow"/> of the pitch.</summary>
    private Quaternion FollowRotation(Camera cam)
    {
        float pitch = cam.transform.eulerAngles.x;
        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        return Quaternion.Euler(pitch * (pitch < 0f ? pitchFollowUp : pitchFollow), 0f, 0f);
    }

    /// <summary>Puts the anchor where the stack belongs for the current view: the eye plus <see cref="eyeOffset"/> (scaled by the fit), turned by part of the camera pitch.
    /// The anchor stays a child of the player root, so it turns with the player's yaw.</summary>
    private void PlaceAnchor()
    {
        Camera cam = ViewCamera;
        if (carryPoint == null || carryPoint.parent == null || cam == null)
        {
            return;
        }

        Transform body = carryPoint.parent;
        Quaternion follow = FollowRotation(cam);
        carryPoint.localPosition = body.InverseTransformPoint(cam.transform.position) + follow * (eyeOffset * _fit);
        carryPoint.localRotation = follow;
    }

    /// <summary>How much the stack has to shrink toward the eye (1 = not at all) so its far edge stops short of the first bookcase or wall along the view. Scaling position and
    /// size together keeps the picture the same on screen (docs/unity-practices/04: Epic's first-person scale), so the stack never leaves the view near a shelf.</summary>
    private float ComputeFit()
    {
        Camera cam = ViewCamera;
        if (carryPoint == null || carryPoint.parent == null || cam == null)
        {
            return 1f;
        }

        Transform body = carryPoint.parent;
        Quaternion follow = FollowRotation(cam);
        Quaternion rot = body.rotation * follow;
        // A box the size of the stack's cross-section, cast forward from the eye plane: a thin ray would slip into the open bay of a bookcase and miss its side panels and
        // shelf boards, which stand in front of a stack held at the right.
        Vector3 center = cam.transform.position + rot * new Vector3(eyeOffset.x, eyeOffset.y - 0.10f, 0f);
        float reach = eyeOffset.z + stackHalfLength;   // how far in front of the eye the stack's far edge is at full size
        var hits = Physics.BoxCastAll(center, new Vector3(0.15f, 0.18f, 0.01f), rot * Vector3.forward, rot, reach + 0.3f, ~0, QueryTriggerInteraction.Ignore);
        float nearest = float.MaxValue;
        Transform root = carryPoint.root;
        foreach (RaycastHit h in hits)
        {
            if (h.collider.transform.IsChildOf(root))
            {
                continue;
            }

            nearest = Mathf.Min(nearest, h.distance);
        }

        if (nearest == float.MaxValue)
        {
            return 1f;
        }

        return Mathf.Clamp((nearest - 0.08f) / reach, minStackScale, 1f);   // 8 cm of margin: the pitched stack is not square to the obstacle
    }

    /// <summary>A short shake of the stack: "your hands are full" (the pickup was refused).</summary>
    public void Nudge()
    {
        _nudgeStart = Time.unscaledTime;
    }

    /// <summary>True while the nudge shake is still playing (tests and tools).</summary>
    public bool IsNudging => Time.unscaledTime < _nudgeStart + NudgeSeconds;

    private const float NudgeSeconds = 0.35f;
    private float _nudgeStart = -10f;

    /// <summary>Where the book at stack place <paramref name="order"/> (0 = the top, the current book) of <paramref name="count"/> books sits, in the anchor's space.
    /// The stack is FANNED: each lower book is a little lower, to the player's left and toward the player, so from above every book shows a strip of its cover and its
    /// spine and the player sees the stack build as books are picked up. The offsets shrink when many books are held so the whole stack stays near the corner.</summary>
    public static Vector3 StackPlace(int order, int count, float thickness)
    {
        int span = Mathf.Max(1, count - 1);
        float drop = Mathf.Min(thickness * 1.04f, 0.45f / span);      // total drop at most 0.45 m
        float left = Mathf.Min(0.030f, 0.12f / span);                  // total shift left at most 0.12 m
        float back = Mathf.Min(0.055f, 0.22f / span);                  // total shift toward the player at most 0.22 m
        return new Vector3(-left * order, -drop * order, -back * order);
    }

    /// <summary>The Librarian-style stack: the books lie flat, their spines toward the player's left, the current book on top (the newest pick-up, or the one the wheel
    /// brought up) and the others fanned below it. The anchor (carryPoint) is a child of the player root, so it turns with the player but not with the camera pitch.</summary>
    private void ArrangeStack(float blend)
    {
        int n = _list.Count;
        float since = Time.unscaledTime - _nudgeStart;
        float shake = since < NudgeSeconds ? Mathf.Sin(since * 60f) * 0.025f * (1f - since / NudgeSeconds) : 0f;
        for (int i = 0; i < n; i++)
        {
            Book book = _list.Items[i];
            if (book == null)
            {
                continue;
            }

            int order = (_list.CurrentIndex - i + n) % n;   // 0 = current book, on top; then the books picked up before it, newest first (the stack as it was built)
            Transform t = book.transform;
            Vector3 real = _originalScale.TryGetValue(book, out Vector3 s) ? s : t.localScale / carryScale;
            Vector3 scale = real * carryScale * _fit;
            float sign = order % 2 == 0 ? 1f : -1f;
            Vector3 targetPos = StackPlace(order, n, real.x * carryScale) * _fit + new Vector3(shake * _fit, 0f, 0f);   // the places shrink with the books
            Quaternion targetRot = Quaternion.Euler(0f, 2.5f * sign * Mathf.Min(order, 3), 0f) * Quaternion.LookRotation(Vector3.right, Vector3.forward);
            t.localPosition = Vector3.Lerp(t.localPosition, targetPos, blend);
            t.localRotation = Quaternion.Slerp(t.localRotation, targetRot, blend);
            t.localScale = Vector3.Lerp(t.localScale, scale, blend);
        }
    }
}
