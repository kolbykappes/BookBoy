using UnityEngine;

/// <summary>
/// An explicit shelf destination for exactly one book. Matching is by BookDefinition.bookId,
/// not by category or position — a carried book can only occupy the slot that names it.
/// </summary>
[ExecuteAlways]
public class ShelfSlot : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    public string acceptedBookId;
    public Transform snapTransform;
    public bool occupied;
    public BookCategory categoryMarker;

    [Tooltip("Visible placeholder shown at the slot until a book is placed here — required by " +
        "the MVP spec ('correct placement snaps reliably into a visible slot'). Hidden once occupied.")]
    public Renderer markerRenderer;

    [Tooltip("The marker's color, matching the accepted book's spine color. Stored as a plain " +
        "field (not just set once via MaterialPropertyBlock) because property blocks set purely " +
        "via script do not survive an Editor domain reload — a real bug hit during development: " +
        "a marker silently went back to the shader's default color after an unrelated script " +
        "recompiled. OnEnable/OnValidate re-apply this every time, so it's self-healing.")]
    public Color markerColor = Color.white;

    private MaterialPropertyBlock _block;

    private void OnEnable()
    {
        ApplyMarkerColor();
    }

    private void OnValidate()
    {
        ApplyMarkerColor();
    }

    public void ApplyMarkerColor()
    {
        if (markerRenderer == null)
        {
            return;
        }

        _block ??= new MaterialPropertyBlock();
        markerRenderer.GetPropertyBlock(_block);
        _block.SetColor(BaseColorId, markerColor);
        _block.SetColor(ColorId, markerColor);
        markerRenderer.SetPropertyBlock(_block);
    }

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
