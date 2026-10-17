using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// First-person interaction: raycast from the camera, pick up books into the carry inventory (up to its capacity), cycle
/// the current book, place it on any shelf, or toss it. The slot the crosshair is on is used for placing; the placement is
/// graded (wrong bookcase / right bookcase / all right) and the book's outline flashes accordingly. Shelved books can be
/// picked up again. No generic item framework.
/// Input (read directly, no input-binding changes): E or left click pick up / place, Q or right click toss, R or mouse wheel
/// cycle. Once a book is placed correctly the readout in the middle of the screen moves on to the next book in your hands.
/// </summary>
[RequireComponent(typeof(CarryInventory))]
public class PlayerInteractor : MonoBehaviour
{
    public Camera interactionCamera;
    public Text promptTitleText;
    public Text promptHintText;
    public float interactionRange = 3f;

    [Tooltip("Targets beyond interactionRange but within this distance show the crosshair as 'too far'.")]
    public float maxAimRange = 12f;

    public Crosshair crosshair;

    [Tooltip("How fast a tossed book leaves the hand, in metres per second (it also gets a little lift).")]
    public float tossSpeed = 5.5f;

    private CarryInventory _carry;
    private Book _focusedBook;
    private ShelfSlot _focusedSlot;
    private bool _targetOutOfRange;
    private bool _aimingAtFinishedBook;
    private Book _litBook;
    private ShelfSlot _litSlot;
    private static readonly System.Comparison<RaycastHit> ByDistance = (a, b) => a.distance.CompareTo(b.distance);

    public CarryInventory Carry => _carry != null ? _carry : (_carry = GetComponent<CarryInventory>());

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
        _aimingAtFinishedBook = false;

        if (interactionCamera == null)
        {
            return;
        }

        bool carrying = !Carry.IsEmpty;
        Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxAimRange, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, ByDistance);

        foreach (RaycastHit hit in hits)
        {
            Shelf shelf = hit.collider.GetComponentInParent<Shelf>();
            Book book = hit.collider.GetComponentInParent<Book>();
            bool inRange = hit.distance <= interactionRange;

            if (!carrying)
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

            // Carrying: a loose book can still be picked up (if there is room); a shelf means "place the current book here".
            if (shelf == null && book != null && book.currentSlot != null)
            {
                shelf = book.currentSlot.shelf;
            }

            if (shelf != null)
            {
                if (!inRange)
                {
                    _targetOutOfRange = true;
                    return;
                }

                ShelfSlot slot = shelf.NearestSlot(ray, hit.point);
                if (slot != null && slot.occupied)
                {
                    if (slot.occupant != null && slot.occupant.IsCorrectlyShelved)
                    {
                        // a finished (green) book is done with: no point picking it up while your hands are busy, and
                        // the readout should show the book you are about to place next
                        _aimingAtFinishedBook = true;
                    }
                    else
                    {
                        _focusedBook = slot.occupant;     // pick the shelved book up instead
                    }
                }
                else
                {
                    _focusedSlot = slot;
                }

                return;
            }

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

        ShelfSlot slotToLight = _focusedSlot;
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

        if (_focusedBook != null && Carry.IsFull)
        {
            crosshair.SetState(Crosshair.State.Blocked);
        }
        else if (_focusedBook != null || _focusedSlot != null)
        {
            crosshair.SetState(Crosshair.State.Active);
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

        _litBook = null;
        _litSlot = null;
    }

    private void UpdatePrompt()
    {
        if (promptTitleText == null || promptHintText == null)
        {
            return;
        }

        if (_focusedBook != null)
        {
            SetPrompt(DisplayName(_focusedBook), Carry.IsFull
                ? "Hands full (" + Carry.Count + "/" + Carry.Capacity + ")"
                : "Press (E) or click to pick up");
        }
        else if (_focusedSlot != null && Carry.Current != null)
        {
            SetPrompt(DisplayName(Carry.Current), "Press (E) or click to place");
        }
        else if (_aimingAtFinishedBook && Carry.Current != null)
        {
            SetPrompt(DisplayName(Carry.Current), "Next up  -  aim at an open spot to place");
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
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null && mouse == null)
        {
            return;
        }

        if ((keyboard != null && keyboard.qKey.wasPressedThisFrame) || (mouse != null && mouse.rightButton.wasPressedThisFrame))
        {
            TossCurrent();
        }

        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            Carry.Cycle(+1);
        }

        if (mouse != null)
        {
            float wheel = Mouse.current.scroll.ReadValue().y;
            if (wheel > 0.1f)
            {
                Carry.Cycle(+1);
            }
            else if (wheel < -0.1f)
            {
                Carry.Cycle(-1);
            }
        }

        bool act = (keyboard != null && keyboard.eKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        if (!act)
        {
            return;
        }

        if (_focusedBook != null && !Carry.IsFull)
        {
            PickUp(_focusedBook);
        }
        else if (_focusedSlot != null && !_focusedSlot.occupied && Carry.Current != null)
        {
            PlaceCurrent(_focusedSlot);
        }
    }

    public bool PickUp(Book book)
    {
        return Carry.TryPickUp(book);
    }

    /// <summary>Places the current book in an empty slot. Returns false if there is nothing to place or the slot is taken.</summary>
    public bool PlaceCurrent(ShelfSlot slot)
    {
        Book book = Carry.Current;
        if (book == null || slot == null || slot.occupied)
        {
            return false;
        }

        if (!Carry.Release(book, out Vector3 scale))
        {
            return false;
        }

        slot.Accept(book, scale);
        return true;
    }

    /// <summary>Q: throws the current book where the player is looking. It gets real physics, tumbles, lands, and goes back to
    /// being a normal pick-up-able book once it has settled.</summary>
    public bool TossCurrent()
    {
        Book book = Carry.Current;
        if (book == null || interactionCamera == null)
        {
            return false;
        }

        if (!Carry.Release(book, out _))
        {
            return false;
        }

        Transform cam = interactionCamera.transform;
        Vector3 velocity = cam.forward * tossSpeed + Vector3.up * 1.3f;
        Vector3 spin = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f));
        book.Toss(cam.position + cam.forward * 0.5f - Vector3.up * 0.1f, velocity, spin);
        return true;
    }

    private static string DisplayName(Book book)
    {
        return book.definition != null ? book.definition.displayName : book.name;
    }
}
