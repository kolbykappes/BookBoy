using UnityEngine;

/// <summary>
/// The brass plate on a shelf front shows who owns the shelf (backlog L-023; Shelves and Sorcery does the same): once a set has claimed the shelf, the plate shows
/// that set's icon and letter, engraved; an empty shelf has a blank plate. The first book placed on a shelf claims it (the earliest placed book), so before this the
/// claim rule was invisible. The plate itself is part of the bookcase mesh; this only puts the two small labels on it and keeps them current.
/// </summary>
public class ClaimPlate : MonoBehaviour
{
    public Shelf shelf;
    public PixelDigit letter;
    public PixelDigit icon;

    private static readonly Color Engraving = new Color(0.20f, 0.11f, 0.03f);
    private Book _shown;
    private float _next;

    private void OnEnable()
    {
        _shown = null;
        _next = 0f;
        Apply(null);
    }

    private void Update()
    {
        if (Time.unscaledTime < _next || shelf == null)
        {
            return;
        }

        _next = Time.unscaledTime + 0.2f;
        Book claimant = shelf.ClaimingBook;
        if (claimant != _shown)
        {
            _shown = claimant;
            Apply(claimant);
        }
    }

    private void Apply(Book claimant)
    {
        bool on = claimant != null && claimant.definition != null && !string.IsNullOrEmpty(claimant.definition.symbol);
        if (letter != null)
        {
            letter.gameObject.SetActive(on);
            if (on)
            {
                letter.value = PixelDigit.LetterValue(claimant.definition.symbol[0]);
                letter.color = Engraving;
                letter.Rebuild();
            }
        }

        bool hasIcon = on && claimant.definition.iconIndex >= 0;
        if (icon != null)
        {
            icon.gameObject.SetActive(hasIcon);
            if (hasIcon)
            {
                icon.value = PixelDigit.IconValue(claimant.definition.iconIndex);
                icon.color = Engraving;
                icon.Rebuild();
            }
        }
    }
}
