using System.Collections.Generic;
using BookBoy.Rules;
using UnityEngine;

/// <summary>
/// One shelf in a <see cref="Bookcase"/> with ordered slots (slot 1 is the leftmost). The first book shelved
/// decides which series the shelf holds. Needs a trigger collider covering the slots so the player's aim ray
/// can find it.
/// </summary>
public class Shelf : MonoBehaviour
{
    public Bookcase bookcase;

    /// <summary>True once the finished-shelf celebration has played for the current state of the shelf; cleared when any book leaves it.</summary>
    [System.NonSerialized] public bool celebrated;

    /// <summary>Every slot holds a book that is correctly shelved (green): the row is finished.</summary>
    public bool IsComplete
    {
        get
        {
            if (slots == null || slots.Length == 0)
            {
                return false;
            }

            foreach (ShelfSlot slot in slots)
            {
                if (slot == null || slot.occupant == null || !slot.occupant.IsCorrectlyShelved)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Tooltip("Slots in order, index 0 = volume 1. Each is only slightly wider than a book.")]
    public ShelfSlot[] slots;

    /// <summary>The earliest-shelved book on this shelf (the one whose set owns the shelf), or null if it is empty.</summary>
    public Book ClaimingBook
    {
        get
        {
            Book best = null;
            double time = double.MaxValue;
            foreach (ShelfSlot slot in slots)
            {
                Book book = slot.occupant;
                if (book != null && book.definition != null && book.placedTime < time)
                {
                    time = book.placedTime;
                    best = book;
                }
            }

            return best;
        }
    }

    /// <summary>The series of the earliest-shelved book here, and when it was shelved. False if the shelf is empty.</summary>
    public bool TryGetSeriesClaim(out int series, out double time)
    {
        series = -1;
        time = double.MaxValue;
        foreach (ShelfSlot slot in slots)
        {
            Book book = slot.occupant;
            if (book != null && book.definition != null && book.placedTime < time)
            {
                time = book.placedTime;
                series = book.GroupKey;
            }
        }

        return series >= 0;
    }

    /// <summary>The slot nearest to where the aim ray crosses the plane the books stand in, measured along the
    /// shelf's own X axis. Slots are only a few centimetres apart, so intersecting at book depth (rather than at
    /// the front of the aim volume) keeps off-axis aiming accurate.</summary>
    public ShelfSlot NearestSlot(Ray worldRay, Vector3 fallbackWorldPoint)
    {
        if (slots == null || slots.Length == 0)
        {
            return null;
        }

        // Intersect at roughly mid-book depth: the slot origin is the front line, books extend ~0.1 m behind it.
        Vector3 slotPlanePoint = slots[0].transform.position + transform.forward * (0.08f * transform.lossyScale.z);
        var plane = new Plane(transform.forward, slotPlanePoint);
        Vector3 aimPoint = plane.Raycast(worldRay, out float distance) ? worldRay.GetPoint(distance) : fallbackWorldPoint;

        var positions = new List<float>(slots.Length);
        foreach (ShelfSlot slot in slots)
        {
            positions.Add(transform.InverseTransformPoint(slot.transform.position).x);
        }

        int index = PlacementRules.NearestSlot(positions, transform.InverseTransformPoint(aimPoint).x);
        return index >= 0 ? slots[index] : null;
    }
}
