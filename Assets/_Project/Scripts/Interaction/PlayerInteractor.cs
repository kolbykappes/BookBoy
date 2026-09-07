using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Minimal first-person interaction: raycast from the camera, carry exactly one book at a
/// time, and place it in a matching, empty ShelfSlot. No inventory, no generic item framework —
/// just enough to prove pick up -> carry -> place -> (Q to) drop feels good.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    public Camera interactionCamera;
    public Transform carryPoint;
    public Text promptTitleText;
    public Text promptHintText;
    public float interactionRange = 3f;

    [Tooltip("Held books are shown at this fraction of their real size so they don't occlude " +
        "as much of the view. Restored to full size on place/drop.")]
    public float carryScale = 0.6f;

    private Book _heldBook;
    private Vector3 _heldOriginalScale;
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
        if (promptTitleText == null || promptHintText == null)
        {
            return;
        }

        if (_heldBook == null && _focusedBook != null)
        {
            SetPrompt(DisplayName(_focusedBook), "Press (E) to pick up");
        }
        else if (_heldBook != null && _focusedSlot != null && !_focusedSlot.occupied)
        {
            SetPrompt(DisplayName(_heldBook), _focusedSlot.Accepts(_heldBook)
                ? "Press (E) to place"
                : "This belongs elsewhere");
        }
        else
        {
            SetPromptVisible(false);
        }
    }

    private void SetPrompt(string title, string hint)
    {
        promptTitleText.text = title;
        promptHintText.text = hint;
        SetPromptVisible(true);
    }

    private void SetPromptVisible(bool visible)
    {
        promptTitleText.enabled = visible;
        promptHintText.enabled = visible;
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
        _heldOriginalScale = book.transform.localScale;

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

        Transform bookTransform = book.transform;
        bookTransform.SetParent(carryPoint, worldPositionStays: false);
        bookTransform.localPosition = Vector3.zero;
        bookTransform.localRotation = Quaternion.identity;
        bookTransform.localScale = _heldOriginalScale * carryScale;
    }

    private void Place(Book book, ShelfSlot slot)
    {
        Transform target = slot.snapTransform != null ? slot.snapTransform : slot.transform;

        Transform bookTransform = book.transform;
        bookTransform.SetParent(null);
        bookTransform.SetPositionAndRotation(target.position, target.rotation);
        bookTransform.localScale = _heldOriginalScale;

        Collider bookCollider = book.GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.enabled = true;
        }

        book.isPlaced = true;
        book.PlayPlacedPulse();
        slot.MarkOccupied();
        _heldBook = null;
    }

    /// <summary>Q: let go of the held book wherever the player is looking, no penalty. Gives it
    /// real physics so it tumbles and collides with static level geometry (walls, floor, shelf)
    /// instead of just resting exactly where it's released.</summary>
    private void Drop(Book book)
    {
        Transform bookTransform = book.transform;
        bookTransform.SetParent(null);
        bookTransform.localScale = _heldOriginalScale;

        Collider bookCollider = book.GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.enabled = true;
            bookCollider.isTrigger = false;
        }

        book.hasPhysicsDrop = true;

        Rigidbody rb = book.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = book.gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        // Continuous detection: a book falling/tumbling at typical drop speed is small and fast
        // enough relative to thin colliders (walls, shelf boards) to tunnel through with the
        // default discrete detection.
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearVelocity = interactionCamera.transform.forward * 0.5f;
        rb.angularVelocity = new Vector3(
            Random.Range(-6f, 6f),
            Random.Range(-6f, 6f),
            Random.Range(-6f, 6f));

        _heldBook = null;
    }

    private static string DisplayName(Book book)
    {
        return book.definition != null ? book.definition.displayName : book.name;
    }
}
