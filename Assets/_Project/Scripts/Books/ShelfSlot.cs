using UnityEngine;

/// <summary>
/// An explicit shelf destination for exactly one book. Matching is by BookDefinition.bookId,
/// not by category or position — a carried book can only occupy the slot that names it.
/// </summary>
public class ShelfSlot : MonoBehaviour
{
    public string acceptedBookId;
    public Transform snapTransform;
    public bool occupied;
    public BookCategory categoryMarker;

    [Tooltip("Visible placeholder shown at the slot until a book is placed here — required by " +
        "the MVP spec ('correct placement snaps reliably into a visible slot'). Hidden once occupied.")]
    public Renderer markerRenderer;

    public bool Accepts(Book book)
    {
        return !occupied && book != null && book.definition != null && book.definition.bookId == acceptedBookId;
    }

    public void MarkOccupied()
    {
        occupied = true;
        if (markerRenderer != null)
        {
            markerRenderer.enabled = false;
        }
    }
}
