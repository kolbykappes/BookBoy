using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Minimal first-person interaction: raycast from the camera, carry exactly one book at a
/// time, and place it on any shelf. The slot the crosshair is on is used; the placement is graded
/// (wrong bookcase / right bookcase / all right) and the book flashes accordingly. Placed books
/// can be picked up again. No inventory, no generic item framework.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    public Camera interactionCamera;
    public Transform carryPoint;
    public Text promptTitleText;
    public Text promptHintText;
    public float interactionRange = 3f;

    [Tooltip("Targets beyond interactionRange but within this distance show the crosshair as 'too far'.")]
    public float maxAimRange = 12f;

    public Crosshair crosshair;

    [Tooltip("Held books are shown at this fraction of their real size so they don't occlude " +
        "as much of the view. Restored to full size on place/drop.")]
    public float carryScale = 0.6f;

    private Book _heldBook;
    private Vector3 _heldOriginalScale;
    private Book _focusedBook;
    private ShelfSlot _focusedSlot;
    private bool _targetOutOfRange;
    private Book _litBook;
    private ShelfSlot _litSlot;
    private static readonly System.Comparison<RaycastHit> ByDistance = (a, b) => a.distance.CompareTo(b.distance);

    private void Update()
    {
        UpdateFocus();
        UpdateFocusVisuals();
        UpdatePrompt();
        HandleInput();
    }

    private void UpdateFocus()
    {
        _focusedBook = null;
        _focusedSlot = null;
        _targetOutOfRange = false;

        if (interactionCamera == null)
        {
            return;
        }

        Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxAimRange, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, ByDistance);

        foreach (RaycastHit hit in hits)
        {
            Shelf shelf = hit.collider.GetComponentInParent<Shelf>();
            Book book = hit.collider.GetComponentInParent<Book>();
            bool inRange = hit.distance <= interactionRange;

            if (_heldBook == null)
            {
                if (book != null)
                {
                    if (inRange)
                    {
                        _focusedBook = book;
                    }
                    else
                    {
                        _targetOutOfRange = true;
                    }

                    return;
                }

                if (shelf != null)
                {
                    // The shelf's aim volume is invisible to pickup; keep looking for a book inside it.
                    continue;
                }

                return;
            }

            if (shelf == null && book != null && book.currentSlot != null)
            {
                shelf = book.currentSlot.shelf;
            }

            if (shelf != null)
            {
                if (inRange)
                {
                    _focusedSlot = shelf.NearestSlot(ray, hit.point);
                }
                else
                {
                    _targetOutOfRange = true;
                }
            }

            return;
        }
    }

    private void UpdateFocusVisuals()
    {
        Book bookToLight = _focusedBook;
        if (_litBook != null && _litBook != bookToLight)
        {
            _litBook.SetFocused(false);
        }

        _litBook = bookToLight;
        if (_litBook != null)
        {
            _litBook.SetFocused(true);
        }

        ShelfSlot slotToLight = _focusedSlot != null && !_focusedSlot.occupied ? _focusedSlot : null;
        if (_litSlot != null && _litSlot != slotToLight)
        {
            _litSlot.SetHighlight(false);
        }

        _litSlot = slotToLight;
        if (_litSlot != null)
        {
            _litSlot.SetHighlight(true);
        }

        if (crosshair == null)
        {
            return;
        }

        if (_focusedBook != null || (_focusedSlot != null && !_focusedSlot.occupied))
        {
            crosshair.SetState(Crosshair.State.Active);
        }
        else if (_focusedSlot != null)
        {
            crosshair.SetState(Crosshair.State.Blocked);
        }
        else if (_targetOutOfRange)
        {
            crosshair.SetState(Crosshair.State.Far);
        }
        else
        {
            crosshair.SetState(Crosshair.State.Idle);
        }
    }

    private void OnDisable()
    {
        if (_litBook != null)
        {
            _litBook.SetFocused(false);
        }

        if (_litSlot != null)
        {
            _litSlot.SetHighlight(false);
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
        else if (_heldBook != null && _focusedSlot != null)
        {
            SetPrompt(DisplayName(_heldBook), _focusedSlot.occupied
                ? "That spot is taken"
                : "Press (E) to place");
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
        else if (_heldBook != null && _focusedSlot != null && !_focusedSlot.occupied)
        {
            Place(_heldBook, _focusedSlot);
        }
        // An occupied spot just keeps showing "That spot is taken" - pick that book up first.
    }

    private void PickUp(Book book)
    {
        if (book.currentSlot != null)
        {
            book.currentSlot.Release();
        }

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
        slot.Accept(book, _heldOriginalScale);
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
