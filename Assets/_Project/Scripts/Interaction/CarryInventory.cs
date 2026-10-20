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

    private float _pull;

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
        _pull = ComputePull();
        ArrangeStack(1f);
    }

    private void LateUpdate()
    {
        if (_list.Count == 0)
        {
            return;
        }

        float blend = 1f - Mathf.Exp(-16f * Time.unscaledDeltaTime);
        _pull = Mathf.Lerp(_pull, ComputePull(), 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
        ArrangeStack(blend);
    }

    /// <summary>How far the stack is pulled back toward the body so it does not poke through a bookcase or wall in front of the player (0 = fully out).</summary>
    private float ComputePull()
    {
        Transform anchor = carryPoint;
        if (anchor == null || anchor.parent == null)
        {
            return 0f;
        }

        // a ray from the body, level with the stack and in line with it, along the player's forward: the stack's far edge must stop short of the first obstacle
        Transform body = anchor.parent;
        Vector3 local = anchor.localPosition;
        Vector3 origin = body.TransformPoint(new Vector3(local.x, local.y, 0f));
        var hits = Physics.RaycastAll(origin, body.forward, 1.5f, ~0, QueryTriggerInteraction.Ignore);
        float nearest = float.MaxValue;
        Transform root = anchor.root;
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
            return 0f;
        }

        float farEdge = local.z + stackHalfLength;
        return Mathf.Clamp(farEdge - (nearest - 0.04f), 0f, local.z + stackHalfLength);
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

            int order = (i - _list.CurrentIndex + n) % n;   // 0 = current book, on top
            Transform t = book.transform;
            Vector3 real = _originalScale.TryGetValue(book, out Vector3 s) ? s : t.localScale / carryScale;
            Vector3 scale = real * carryScale;
            float sign = order % 2 == 0 ? 1f : -1f;
            Vector3 targetPos = StackPlace(order, n, scale.x) + new Vector3(shake, 0f, -_pull);
            Quaternion targetRot = Quaternion.Euler(0f, 2.5f * sign * Mathf.Min(order, 3), 0f) * Quaternion.LookRotation(Vector3.right, Vector3.forward);
            t.localPosition = Vector3.Lerp(t.localPosition, targetPos, blend);
            t.localRotation = Quaternion.Slerp(t.localRotation, targetRot, blend);
            t.localScale = Vector3.Lerp(t.localScale, scale, blend);
        }
    }
}
