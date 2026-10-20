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
    private FindSpot _focusedSpot;
    private CampaignController _campaign;
    private bool _campaignSearched;
    private Book _litBook;
    private ShelfSlot _litSlot;
    private static readonly System.Comparison<RaycastHit> ByDistance = (a, b) => a.distance.CompareTo(b.distance);

    public CarryInventory Carry => _carry != null ? _carry : (_carry = GetComponent<CarryInventory>());

    /// <summary>The Lamplit campaign in this scene, if any (found once).</summary>
    private CampaignController Campaign
    {
        get
        {
            if (!_campaignSearched)
            {
                _campaignSearched = true;
                _campaign = FindFirstObjectByType<CampaignController>();
            }

            return _campaign;
        }
    }

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
        _focusedSpot = null;

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
            // The camera sits inside the player's own colliders (the character controller and the capsule mesh); a ray that starts there can
            // report them at ~1 cm, and as the first hit they would hide everything behind them. They are never a target.
            if (hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            FindSpot spot = hit.collider.GetComponentInParent<FindSpot>();
            if (spot != null)
            {
                // a desk spot (apron hook, shoe rack...): aim here with a find in hand to install it
                if (hit.distance <= interactionRange)
                {
                    _focusedSpot = spot;
                }
                else
                {
                    _targetOutOfRange = true;
                }

                return;
            }

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
            _litBook.SetFocused(true, Carry.IsFull && !Collectible(_litBook));   // grey when the hands are full
        }

        ShelfSlot slotToLight = _focusedSlot;
        bool ghostOn = UpdateGhost(slotToLight);
        if (_litSlot != null && _litSlot != slotToLight)
        {
            _litSlot.SetHighlight(false);
        }

        _litSlot = slotToLight;
        if (_litSlot != null)
        {
            _litSlot.SetHighlight(!ghostOn);   // the thin bar only when there is no ghost to show
        }

        if (crosshair == null)
        {
            return;
        }

        if (_focusedBook != null && Carry.IsFull && !Collectible(_focusedBook))
        {
            crosshair.SetState(Crosshair.State.Blocked);
        }
        else if (_focusedBook != null || _focusedSlot != null || _focusedSpot != null)
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

    private SlotGhost _ghost;
    private bool _ghostLooked;
    private ShelfSlot _ghostSlot;
    private Book _ghostBook;
    private BookBoy.Rules.PlacementResult _ghostGrade;
    private float _ghostNext;

    /// <summary>Shows the translucent ghost of the held book in the aimed empty slot (graded as if it were placed there); false when there is none.</summary>
    private bool UpdateGhost(ShelfSlot slot)
    {
        if (!_ghostLooked)
        {
            _ghost = GetComponent<SlotGhost>();
            _ghostLooked = true;
        }

        Book held = Carry.IsEmpty ? null : Carry.Current;
        if (_ghost == null || slot == null || held == null || held.IsFind)
        {
            if (_ghost != null)
            {
                _ghost.Hide();
            }

            return false;
        }

        if (slot != _ghostSlot || held != _ghostBook || Time.unscaledTime >= _ghostNext)
        {
            _ghostSlot = slot;
            _ghostBook = held;
            _ghostNext = Time.unscaledTime + 0.2f;   // claims change only when books are placed or removed
            _ghostGrade = held.PreviewGrade(slot);
        }

        _ghost.Show(held, slot, _ghostGrade, Carry.RealScale(held));
        return _ghost.IsShown;
    }

    private void OnDisable()
    {
        if (_ghost != null)
        {
            _ghost.Hide();
        }

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

        if (_focusedSpot != null)
        {
            SetPrompt(_focusedSpot.label, SpotHint(_focusedSpot));
        }
        else if (_focusedBook != null)
        {
            SetPrompt(DisplayName(_focusedBook), Collectible(_focusedBook) ? "Press (E) or click to take it" : Carry.IsFull
                ? (Time.unscaledTime < _fullNoticeUntil
                    ? "Hands full (" + Carry.Count + "/" + Carry.Capacity + ")  -  put a book down first"
                    : "Hands full (" + Carry.Count + "/" + Carry.Capacity + ")")
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

    private string SpotHint(FindSpot spot)
    {
        if (spot.locked)
        {
            return "Locked for now";
        }

        BookBoy.Rules.FindDef wanted = BookBoy.Rules.CampaignCatalog.FindForSpot(spot.spotId);
        if (spot.installed != null)
        {
            return "Installed: " + DisplayName(spot.installed);
        }

        Book held = Carry.Current;
        if (held != null && held.IsFind && wanted != null && held.definition.findId == wanted.Id)
        {
            return "Press (E) or click to install the " + wanted.Name.ToLowerInvariant();
        }

        return wanted != null ? "Needs the " + wanted.Name.ToLowerInvariant() : "";
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
            Carry.Cycle(-1);   // the top book goes to the bottom, the next one comes up
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

        if (_focusedSpot != null)
        {
            Campaign?.InstallHeld(_focusedSpot, Carry);
        }
        else if (_focusedBook != null && (!Carry.IsFull || Collectible(_focusedBook)))
        {
            PickUp(_focusedBook);
        }
        else if (_focusedBook != null && Carry.IsFull)
        {
            NotifyHandsFull();
        }
        else if (_focusedSlot != null && !_focusedSlot.occupied && Carry.Current != null)
        {
            PlaceCurrent(_focusedSlot);
        }
    }

    private float _fullNoticeUntil;

    /// <summary>The pickup was refused because the hands are full: the grey crosshair punches and shakes, the stack shakes, and the prompt says so for a moment.</summary>
    public void NotifyHandsFull()
    {
        _fullNoticeUntil = Time.unscaledTime + 1.8f;
        if (crosshair != null)
        {
            crosshair.Reject();
        }

        Carry.Nudge();
    }

    /// <summary>True while the "hands full" notice is showing (tests and tools).</summary>
    public bool HandsFullNoticeShowing => Time.unscaledTime < _fullNoticeUntil;

    /// <summary>A find with a real model (the apron, the shoes) is taken, not carried: it goes straight to its place at the desk and works at once.</summary>
    private static bool Collectible(Book book)
    {
        return book != null && book.IsFind && book.definition != null && book.definition.discoveryDialog;
    }

    public bool PickUp(Book book)
    {
        if (Collectible(book) && Campaign != null)
        {
            return Campaign.CollectFind(book);
        }

        bool picked = Carry.TryPickUp(book);
        if (picked)
        {
            Campaign?.NotifyPickedUp(book);
        }

        return picked;
    }

    /// <summary>Places the current book in an empty slot. Returns false if there is nothing to place or the slot is taken.</summary>
    public bool PlaceCurrent(ShelfSlot slot)
    {
        Book book = Carry.Current;
        if (book == null || slot == null || slot.occupied)
        {
            return false;
        }

        if (book.IsFind)
        {
            Campaign?.Say("That belongs at the desk, not on a shelf.");
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
