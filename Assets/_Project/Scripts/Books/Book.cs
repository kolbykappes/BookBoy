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

    /// <summary>True once the player has dropped this book with real physics (Rigidbody,
    /// tumbling). Keeps OnEnable from forcing the collider back to a trigger after a dropped
    /// book has deliberately been made solid — otherwise a future Editor domain reload would
    /// silently revert it, the same class of bug already hit once with shelf marker colors.</summary>
    [HideInInspector]
    public bool hasPhysicsDrop;

    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _flash;
    private bool _focused;

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
        // Disabling stops the flash coroutine without letting it finish; forget it so later recoloring works.
        _flash = null;
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

        SetColor(RestingColor());
    }

    /// <summary>Brightens the book while the crosshair is on it so the player can see what is targeted.</summary>
    public void SetFocused(bool focused)
    {
        if (_focused == focused)
        {
            return;
        }

        _focused = focused;
        if (_flash == null)
        {
            ApplyColor();
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

    public bool IsCorrectlyShelved => currentSlot != null && Grade() == PlacementResult.Correct;

    /// <summary>Flashes red (wrong bookcase), yellow (right bookcase, wrong shelf or slot) or green (all right),
    /// then eases back to the resting color.</summary>
    public void PlayFeedback(PlacementResult result)
    {
        if (definition == null || !Application.isPlaying)
        {
            return;
        }

        if (_flash != null)
        {
            StopCoroutine(_flash);
        }

        _flash = StartCoroutine(FlashRoutine(ColorFor(result)));
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

    private Color RestingColor()
    {
        return _focused ? Color.Lerp(definition.spineColor, Color.white, 0.45f) : definition.spineColor;
    }

    private IEnumerator FlashRoutine(Color flash)
    {
        const float rise = 0.1f;
        const float hold = 0.35f;
        const float fall = 0.45f;

        Color from = RestingColor();
        float t = 0f;
        while (t < rise)
        {
            t += Time.deltaTime;
            SetColor(Color.Lerp(from, flash, t / rise));
            yield return null;
        }

        SetColor(flash);
        yield return new WaitForSeconds(hold);

        t = 0f;
        while (t < fall)
        {
            t += Time.deltaTime;
            SetColor(Color.Lerp(flash, RestingColor(), t / fall));
            yield return null;
        }

        _flash = null;
        SetColor(RestingColor());
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
