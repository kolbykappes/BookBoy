using System.Collections;
using BookBoy.Rules;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class Book : MonoBehaviour
{
    public static readonly Color WrongColor = new Color(1f, 0.15f, 0.1f);
    public static readonly Color YellowColor = new Color(1f, 0.9f, 0.1f);
    public static readonly Color CorrectColor = new Color(0.2f, 1f, 0.3f);

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public BookDefinition definition;

    /// <summary>The shelf slot this book currently sits in, or null when it is loose or carried.
    /// Serialized (not just runtime state) so the Editor's play-mode restore resets it.</summary>
    public ShelfSlot currentSlot;

    /// <summary>When this book was last shelved (Time.realtimeSinceStartupAsDouble, so two placements never tie). The earliest book on a bookcase/shelf
    /// decides which color/series that bookcase/shelf belongs to.</summary>
    public double placedTime;

    public bool isPlaced => currentSlot != null;

    /// <summary>Item family: the Lamplit family if set, else the library colour category.</summary>
    public int FamilyKey => definition == null ? -1 : definition.family >= 0 ? definition.family : (int)definition.category;

    /// <summary>Group: the Lamplit group if set, else the library series.</summary>
    public int GroupKey => definition == null ? -1 : definition.group >= 0 ? definition.group : definition.series;

    /// <summary>Number in the set: the Lamplit number if set, else the library volume.</summary>
    public int NumberKey => definition == null ? 0 : definition.number > 0 ? definition.number : definition.volume;

    /// <summary>A find (apron, shoes...) is carried to the desk and is never shelved.</summary>
    public bool IsFind => definition != null && !string.IsNullOrEmpty(definition.findId);

    /// <summary>True if the other book is of the same family and group (its "sisters"). Finds never match.</summary>
    public bool SameGroupAs(Book other)
    {
        return other != null && !IsFind && !other.IsFind && definition != null && other.definition != null
            && FamilyKey == other.FamilyKey && GroupKey == other.GroupKey && definition.room == other.definition.room;
    }

    /// <summary>True once the player has dropped this book with real physics (Rigidbody,
    /// tumbling). Keeps OnEnable from forcing the collider back to a trigger after a dropped
    /// book has deliberately been made solid — otherwise a future Editor domain reload would
    /// silently revert it, the same class of bug already hit once with shelf marker colors.</summary>
    [HideInInspector]
    public bool hasPhysicsDrop;

    public static readonly Color FocusColor = new Color(1f, 0.93f, 0.55f);

    private static PhysicsMaterial _tossMaterial;

    private MaterialPropertyBlock _propertyBlock;
    private BookOutline _outline;
    private Coroutine _settle;

    private void OnEnable()
    {
        ApplyColor();

        // Books are triggers by default — never physically block the player's walk. Solid
        // Rigidbody physics for scattered/carried items isn't the goal here (see "tactile
        // reliability over physical realism"); this only affects collision response, raycasts
        // still hit triggers by default so pickup/placement detection is unaffected. A dropped
        // book (see PlayerInteractor.Drop) is the one deliberate exception.
        if (!hasPhysicsDrop)
        {
            Collider bookCollider = GetComponent<Collider>();
            if (bookCollider != null)
            {
                bookCollider.isTrigger = true;
            }
        }
    }

    private void OnDisable()
    {
        // Disabling stops a tumble in progress without letting it finish; forget it so a later toss works.
        _settle = null;
    }

    private void OnValidate()
    {
        ApplyColor();
    }

    /// <summary>
    /// Re-applies the spine color via a MaterialPropertyBlock, so every book can show its own
    /// color while still sharing one base material — no per-object material instances, no
    /// leaks in edit mode. Call this after assigning <see cref="definition"/> from code (e.g.
    /// editor tooling) — a plain field assignment does not trigger OnValidate.
    /// </summary>
    public void ApplyColor()
    {
        if (definition == null)
        {
            return;
        }

        SetColor(definition.spineColor);
    }

    /// <summary>The glow around this book (created on first use, in Play Mode only).</summary>
    public BookOutline Outline
    {
        get
        {
            if (_outline == null && Application.isPlaying)
            {
                _outline = GetComponent<BookOutline>();
                if (_outline == null)
                {
                    _outline = gameObject.AddComponent<BookOutline>();
                }
            }

            return _outline;
        }
    }

    /// <summary>Outlines the book while the crosshair is on it so the player can see what is targeted. The book's own color
    /// never changes.</summary>
    public void SetFocused(bool focused)
    {
        BookOutline outline = Outline;
        if (outline != null)
        {
            outline.SetSteady(focused, FocusColor);
        }
    }

    /// <summary>Current grade, recomputed from the live shelves: claims move when books are added or removed.</summary>
    public PlacementResult Grade()
    {
        if (currentSlot == null || definition == null || currentSlot.shelf == null || currentSlot.shelf.bookcase == null)
        {
            return PlacementResult.WrongBookcase;
        }

        Shelf shelf = currentSlot.shelf;
        Bookcase bookcase = shelf.bookcase;
        if (bookcase.rules != ContainerRules.Prototype)
        {
            return GradeCampaign(shelf, bookcase);
        }

        Bookcase[] cases = bookcase.all;
        int myCase = -1;
        var caseValues = new int[cases.Length];
        var caseTimes = new double[cases.Length];
        for (int i = 0; i < cases.Length; i++)
        {
            cases[i].TryGetColorClaim(out caseValues[i], out caseTimes[i]);
            if (cases[i] == bookcase)
            {
                myCase = i;
            }
        }

        bool bookcaseOk = PlacementRules.ClaimHolds(myCase, caseValues, caseTimes, (int)definition.category);

        Shelf[] shelves = bookcase.shelves;
        int myShelf = -1;
        var shelfValues = new int[shelves.Length];
        var shelfTimes = new double[shelves.Length];
        for (int i = 0; i < shelves.Length; i++)
        {
            shelves[i].TryGetSeriesClaim(out shelfValues[i], out shelfTimes[i]);
            if (shelves[i] == shelf)
            {
                myShelf = i;
            }
        }

        bool shelfOk = PlacementRules.ClaimHolds(myShelf, shelfValues, shelfTimes, definition.series);
        return PlacementRules.Evaluate(bookcaseOk, shelfOk, definition.volume, currentSlot.slotIndex);
    }

    /// <summary>The Lamplit grading: red if the container's family (or room) does not accept the item, yellow if another group owns
    /// the shelf (or, in an Ordered container, the number is not in its own slot), green otherwise. Group claims are compared across
    /// every shelf in the room, so a group cannot be split over two shelves and still look right.</summary>
    private PlacementResult GradeCampaign(Shelf shelf, Bookcase bookcase)
    {
        if (IsFind)
        {
            return PlacementResult.WrongBookcase;
        }

        bool roomOk = definition.room == 0 || bookcase.room == null || bookcase.room.roomNumber == definition.room;
        bool familyOk = roomOk && PlacementRules.FamilyAccepted(bookcase.containerFamily, FamilyKey);

        Shelf[] shelves = bookcase.room != null && bookcase.room.shelves != null && bookcase.room.shelves.Length > 0 ? bookcase.room.shelves : bookcase.shelves;
        int mine = -1;
        var values = new int[shelves.Length];
        var times = new double[shelves.Length];
        for (int i = 0; i < shelves.Length; i++)
        {
            shelves[i].TryGetSeriesClaim(out values[i], out times[i]);
            if (shelves[i] == shelf)
            {
                mine = i;
            }
        }

        bool groupOk = PlacementRules.ClaimHolds(mine, values, times, GroupKey);
        PlacementMode mode = bookcase.rules == ContainerRules.Ordered ? PlacementMode.Ordered : PlacementMode.Group;
        return PlacementRules.EvaluateCampaign(mode, familyOk, groupOk, NumberKey, currentSlot.slotIndex);
    }

    public bool IsCorrectlyShelved => currentSlot != null && Grade() == PlacementResult.Correct;

    /// <summary>Flashes the outline red (wrong bookcase), yellow (right bookcase, wrong shelf or slot) or green (all right), with a
    /// sparkle burst on green. The book's own color does not change.</summary>
    public void PlayFeedback(PlacementResult result)
    {
        if (definition == null || !Application.isPlaying)
        {
            return;
        }

        BookOutline outline = Outline;
        if (outline != null)
        {
            outline.Flash(ColorFor(result), result == PlacementResult.Correct);
        }
    }

    private static PhysicsMaterial TossMaterial()
    {
        if (_tossMaterial == null)
        {
            _tossMaterial = new PhysicsMaterial("BookToss")
            {
                bounciness = 0.05f,
                dynamicFriction = 0.6f,
                staticFriction = 0.7f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                hideFlags = HideFlags.HideAndDontSave
            };
        }

        return _tossMaterial;
    }

    /// <summary>Throws the book with real physics: it tumbles, lands, and once it has been still for a moment it goes back to
    /// being a trigger (so it never blocks the player's walk) that can be picked up again.</summary>
    /// <param name="minSolidSeconds">Stay a solid physics body at least this long before settling (a pile being built needs the
    /// books underneath to still be solid while the next ones land on them).</param>
    public void Toss(Vector3 origin, Vector3 velocity, Vector3 spin, float minSolidSeconds = 0f)
    {
        if (currentSlot != null)
        {
            currentSlot.Release();
        }

        transform.SetParent(null, worldPositionStays: true);
        transform.position = origin;

        Collider bookCollider = GetComponent<Collider>();
        bookCollider.enabled = true;
        bookCollider.isTrigger = false;
        bookCollider.sharedMaterial = TossMaterial();
        hasPhysicsDrop = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.mass = 0.4f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.6f;
        // Continuous: a small, fast book tunnels through thin colliders (walls, shelf boards) with discrete detection.
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearVelocity = velocity;
        rb.angularVelocity = spin;

        if (_settle != null)
        {
            StopCoroutine(_settle);
        }

        _settle = StartCoroutine(SettleRoutine(rb, bookCollider, minSolidSeconds));
    }

    /// <summary>Cancels a tumble in progress (the book is being picked up mid-air or mid-roll).</summary>
    public void StopPhysicsTumble()
    {
        if (_settle != null)
        {
            StopCoroutine(_settle);
            _settle = null;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

        Collider bookCollider = GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.sharedMaterial = null;
        }

        hasPhysicsDrop = false;
    }

    private IEnumerator SettleRoutine(Rigidbody rb, Collider bookCollider, float minSolidSeconds)
    {
        float elapsed = 0f;
        float still = 0f;
        float limit = Mathf.Max(10f, minSolidSeconds + 6f);
        while (elapsed < limit && (still < 0.5f || elapsed < minSolidSeconds))
        {
            yield return new WaitForFixedUpdate();
            elapsed += Time.fixedDeltaTime;

            if (transform.position.y < -2f)
            {
                // fell out of the world: put it back on the floor near where it left
                transform.position = new Vector3(Mathf.Clamp(transform.position.x, -6f, 6f), 0.3f, Mathf.Clamp(transform.position.z, -4f, 4f));
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            bool resting = rb.linearVelocity.sqrMagnitude < 0.0025f && rb.angularVelocity.sqrMagnitude < 0.04f;
            still = resting ? still + Time.fixedDeltaTime : 0f;
        }

        _settle = null;
        Destroy(rb);
        bookCollider.sharedMaterial = null;
        bookCollider.isTrigger = true;
        hasPhysicsDrop = false;
    }

    public static Color ColorFor(PlacementResult result)
    {
        switch (result)
        {
            case PlacementResult.Correct: return CorrectColor;
            case PlacementResult.RightBookcaseWrongSpot: return YellowColor;
            default: return WrongColor;
        }
    }

    private void SetColor(Color color)
    {
        Renderer bookRenderer = GetComponent<Renderer>();
        if (bookRenderer == null)
        {
            return;
        }

        _propertyBlock ??= new MaterialPropertyBlock();
        bookRenderer.GetPropertyBlock(_propertyBlock);
        // Set both property names so this works whether the shared material uses URP
        // ("_BaseColor") or a legacy/Standard shader ("_Color").
        _propertyBlock.SetColor(BaseColorId, color);
        _propertyBlock.SetColor(ColorId, color);
        bookRenderer.SetPropertyBlock(_propertyBlock);
    }
}
