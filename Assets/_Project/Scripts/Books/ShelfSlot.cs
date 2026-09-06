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

    [Tooltip("Optional — not used for matching yet, just documents which category this slot belongs to.")]
    public BookCategory categoryMarker;

    public bool Accepts(Book book)
    {
        return !occupied && book != null && book.definition != null && book.definition.bookId == acceptedBookId;
    }
}
