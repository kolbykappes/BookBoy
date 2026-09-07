using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Minimal first-person interaction: raycast from the camera, carry exactly one book at a
/// time, and place it in a matching, empty ShelfSlot. No inventory, no throwing, no generic
/// item framework — just enough to prove pick up -> carry -> place feels good.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    public Camera interactionCamera;
    public Transform carryPoint;
    public Text promptText;
    public float interactionRange = 3f;

    private Book _heldBook;
    private Book _focusedBook;
    private ShelfSlot _focusedSlot;

    private void Update()
    {
        UpdateFocus();
        UpdatePrompt();
        HandleInput();
    }

    private void UpdateFocus()
    {
        _focusedBook = null;
        _focusedSlot = null;

        if (interactionCamera == null)
        {
            return;
        }

        Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            return;
        }

        if (_heldBook == null)
        {
            Book book = hit.collider.GetComponentInParent<Book>();
            if (book != null && !book.isPlaced)
            {
                _focusedBook = book;
            }
        }
        else
        {
            ShelfSlot slot = hit.collider.GetComponentInParent<ShelfSlot>();
            if (slot != null)
            {
                _focusedSlot = slot;
            }
        }
    }

    private void UpdatePrompt()
    {
        if (promptText == null)
        {
            return;
        }

        if (_heldBook == null && _focusedBook != null)
        {
            promptText.text = "E Pick Up " + DisplayName(_focusedBook);
            promptText.enabled = true;
        }
        else if (_heldBook != null && _focusedSlot != null && !_focusedSlot.occupied)
        {
            promptText.text = _focusedSlot.Accepts(_heldBook)
                ? "E Place " + DisplayName(_heldBook)
                : "This belongs elsewhere";
            promptText.enabled = true;
        }
        else
        {
            promptText.enabled = false;
        }
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (_heldBook != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            Drop(_heldBook);
            return;
        }

        if (!Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

        if (_heldBook == null && _focusedBook != null)
        {
            PickUp(_focusedBook);
        }
        else if (_heldBook != null && _focusedSlot != null && _focusedSlot.Accepts(_heldBook))
        {
            Place(_heldBook, _focusedSlot);
        }
        // A mismatched slot just keeps showing "This belongs elsewhere" — no penalty, book stays held.
    }

    private void PickUp(Book book)
    {
        _heldBook = book;

        Collider bookCollider = book.GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.enabled = false;
        }

        Transform bookTransform = book.transform;
        bookTransform.SetParent(carryPoint, worldPositionStays: false);
        bookTransform.localPosition = Vector3.zero;
        bookTransform.localRotation = Quaternion.identity;
    }

    private void Place(Book book, ShelfSlot slot)
    {
        Transform target = slot.snapTransform != null ? slot.snapTransform : slot.transform;

        Transform bookTransform = book.transform;
        bookTransform.SetParent(null);
        bookTransform.SetPositionAndRotation(target.position, target.rotation);

        Collider bookCollider = book.GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.enabled = true;
        }

        book.isPlaced = true;
        slot.MarkOccupied();
        _heldBook = null;
    }

    /// <summary>Q: let go of the held book wherever the player is looking, no penalty. Drops
    /// straight down onto the floor beneath the carry point (or right where it was, in the
    /// unlikely case there's nothing below to land on).</summary>
    private void Drop(Book book)
    {
        Transform bookTransform = book.transform;
        Vector3 dropOrigin = bookTransform.position;
        bookTransform.SetParent(null);

        float halfHeight = bookTransform.localScale.y * 0.5f;
        if (Physics.Raycast(dropOrigin, Vector3.down, out RaycastHit hit, 10f))
        {
            bookTransform.position = hit.point + Vector3.up * halfHeight;
        }

        bookTransform.rotation = Quaternion.identity;

        Collider bookCollider = book.GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.enabled = true;
        }

        _heldBook = null;
    }

    private static string DisplayName(Book book)
    {
        return book.definition != null ? book.definition.displayName : book.name;
    }
}
