using UnityEngine;

/// <summary>A place at the desk where one particular find is installed (apron hook, shoe rack, ...). Needs a trigger collider so the
/// aim ray can find it. A locked spot (the dumbwaiter hatch) takes nothing yet.</summary>
public class FindSpot : MonoBehaviour
{
    [Tooltip("Matches FindDef.SpotId in the campaign catalog.")]
    public string spotId;

    [Tooltip("What the player sees when aiming at it.")]
    public string label;

    [Tooltip("Where an installed find is placed.")]
    public Transform anchor;

    public bool locked;

    /// <summary>The find installed here, if any.</summary>
    public Book installed;
}
