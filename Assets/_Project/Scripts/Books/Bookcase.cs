using UnityEngine;

/// <summary>How a container grades what is on it. Prototype: the original colour-theme + series + volume rules. Group / Ordered:
/// the Lamplit rules (family check, group claim per shelf, any slot / number N in slot N).</summary>
public enum ContainerRules
{
    Prototype = 0,
    Group = 1,
    Ordered = 2
}

/// <summary>
/// One bookcase with two shelves. Each bookcase has a fixed theme (shown on the sign above it): only books of that theme
/// belong in it. Which of the two series goes on which shelf is still decided by the first book shelved on each.
/// </summary>
public class Bookcase : MonoBehaviour
{
    [Tooltip("The theme (book color family) this bookcase holds. Shown on the wall sign above it.")]
    public BookCategory theme;

    public Shelf[] shelves;

    [Header("Lamplit")]
    public ContainerRules rules = ContainerRules.Prototype;

    [Tooltip("Item family this container accepts; -1 = any family. Only used when 'rules' is not Prototype.")]
    public int containerFamily = -1;

    [Tooltip("The room this container is in; its shelves share group claims, and it only accepts items of that room.")]
    public RoomZone room;

    [Tooltip("Every bookcase in the room (including this one), in a fixed order shared by all of them.")]
    public Bookcase[] all;

    /// <summary>The theme this bookcase holds. Always claimed: the theme is fixed, not decided by the first book.</summary>
    public bool TryGetColorClaim(out int color, out double time)
    {
        color = (int)theme;
        time = 0.0;
        return true;
    }
}
