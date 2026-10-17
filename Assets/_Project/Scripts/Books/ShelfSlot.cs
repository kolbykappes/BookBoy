using BookBoy.Rules;
using UnityEngine;

/// <summary>
/// One position on a <see cref="Shelf"/>, barely wider than a book. Holds at most one book. Slot N (1-based)
/// is where volume N of the shelf's series belongs. Invisible except for a thin bar on the board that lights up
/// while the player aims at it with a book in hand.
/// </summary>
public class ShelfSlot : MonoBehaviour
{
    public Shelf shelf;

    [Tooltip("0-based position on the shelf; the book that belongs here has volume slotIndex + 1.")]
    public int slotIndex;

    public Book occupant;

    [Tooltip("Thin bar on the board, shown only while this slot is the aimed placement target.")]
    public Renderer highlight;

    /// <summary>True while a book is flying toward this slot (quick shelve): nobody else may use it, but it holds no claim yet.</summary>
    public bool reserved;

    /// <summary>Taken: holds a book, or is reserved for one that is on its way.</summary>
    public bool occupied => occupant != null || reserved;

    private void OnEnable()
    {
        SetHighlight(false);
    }

    public void SetHighlight(bool on)
    {
        if (highlight != null)
        {
            highlight.enabled = on;
        }
    }

    /// <summary>Snaps the book into this empty slot, grades the placement, flashes feedback, and returns the grade.</summary>
    public PlacementResult Accept(Book book, Vector3 bookScale)
    {
        Transform bookTransform = book.transform;
        bookTransform.SetParent(null);
        bookTransform.localScale = bookScale;
        // The slot origin is on the board top at the front line: stand the book on it with its front (spine) face on
        // that line, so every book sits flush near the front of the shelf whatever its depth.
        bookTransform.SetPositionAndRotation(
            transform.position + Vector3.up * (bookScale.y * 0.5f) + transform.forward * (bookScale.z * 0.5f),
            transform.rotation);

        Collider bookCollider = book.GetComponent<Collider>();
        if (bookCollider != null)
        {
            bookCollider.enabled = true;
        }

        reserved = false;
        occupant = book;
        book.currentSlot = this;
        book.placedTime = Time.realtimeSinceStartupAsDouble;

        PlacementResult result = book.Grade();
        book.PlayFeedback(result);
        SetHighlight(false);
        return result;
    }

    /// <summary>Frees the slot because its book is being picked up.</summary>
    public void Release()
    {
        if (occupant != null)
        {
            occupant.currentSlot = null;
        }

        occupant = null;
    }
}
