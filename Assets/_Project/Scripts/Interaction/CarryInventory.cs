using System;
using System.Collections.Generic;
using BookBoy.Rules;
using UnityEngine;

/// <summary>
/// The books the player is carrying. Shown as a small fan at the carry point with the "current" book in front; cycling
/// swaps which one is current. The logic (capacity, cycling) lives in the engine-free CarryList.
/// </summary>
public class CarryInventory : MonoBehaviour
{
    public Transform carryPoint;

    [Tooltip("Carried books are shown at this fraction of their real size so they don't occlude as much of the view.")]
    public float carryScale = 0.6f;

    private readonly CarryList<Book> _list = new CarryList<Book>(UpgradeCatalog.BaseCarryCapacity);
    private readonly Dictionary<Book, Vector3> _originalScale = new Dictionary<Book, Vector3>();

    public event Action Changed;

    public int Capacity => _list.Capacity;
    public int Count => _list.Count;
    public bool IsFull => _list.IsFull;
    public bool IsEmpty => _list.IsEmpty;
    public Book Current => _list.Current;
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

    private void LateUpdate()
    {
        int n = _list.Count;
        if (n == 0)
        {
            return;
        }

        float blend = 1f - Mathf.Exp(-16f * Time.unscaledDeltaTime);
        for (int i = 0; i < n; i++)
        {
            Book book = _list.Items[i];
            if (book == null)
            {
                continue;
            }

            int order = (i - _list.CurrentIndex + n) % n;   // 0 = current book
            Transform t = book.transform;
            Vector3 real = _originalScale.TryGetValue(book, out Vector3 s) ? s : t.localScale;
            float size = carryScale * (order == 0 ? 1f : 0.82f);
            Vector3 targetPos = order == 0 ? Vector3.zero : new Vector3(0.08f * order, -0.02f * order, 0.025f * order);
            Quaternion targetRot = order == 0 ? Quaternion.identity : Quaternion.Euler(0f, -10f * order, 0f);
            t.localPosition = Vector3.Lerp(t.localPosition, targetPos, blend);
            t.localRotation = Quaternion.Slerp(t.localRotation, targetRot, blend);
            t.localScale = Vector3.Lerp(t.localScale, real * size, blend);
        }
    }
}
