using UnityEngine;

/// <summary>
/// One bookcase with two shelves. It starts unlabeled: the first book shelved in it decides which color it
/// holds. If two bookcases claim the same color, the one that was claimed first is the right one.
/// </summary>
public class Bookcase : MonoBehaviour
{
    public Shelf[] shelves;

    [Tooltip("Every bookcase in the room (including this one), in a fixed order shared by all of them.")]
    public Bookcase[] all;

    /// <summary>The color of the earliest-shelved book here, and when it was shelved. False if the bookcase is empty.</summary>
    public bool TryGetColorClaim(out int color, out double time)
    {
        color = -1;
        time = double.MaxValue;
        foreach (Shelf shelf in shelves)
        {
            foreach (ShelfSlot slot in shelf.slots)
            {
                Book book = slot.occupant;
                if (book != null && book.definition != null && book.placedTime < time)
                {
                    time = book.placedTime;
                    color = (int)book.definition.category;
                }
            }
        }

        return color >= 0;
    }
}
