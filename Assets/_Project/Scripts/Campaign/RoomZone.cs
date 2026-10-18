using System.Collections.Generic;
using BookBoy.Rules;
using UnityEngine;

/// <summary>
/// One Lamplit room: its items, its shelves (which share group claims), the finds hidden in it, and an area of floor where loose items
/// are scattered. Knows how to count its correct items, scatter them again, and (for the admin menu) put every item in its home slot.
/// </summary>
public class RoomZone : MonoBehaviour
{
    public int roomNumber;
    public string displayName;

    [Tooltip("Every shelf in the room (claims are compared across all of them).")]
    public Shelf[] shelves;

    [Tooltip("The sortable items of the room (not the finds).")]
    public Book[] items;

    [Tooltip("The slot each item belongs in once the room is tidy (parallel to 'items'): used by the admin 'complete room' command.")]
    public ShelfSlot[] homeSlots;

    [Tooltip("Finds hidden in this room and where each one sits (parallel arrays).")]
    public Book[] finds;
    public Transform[] findHideSpots;

    [Header("Where loose items are scattered (world x, z) and where the player stands when jumping here")]
    public Vector2 scatterMin;
    public Vector2 scatterMax;
    public Transform entryPoint;

    [Header("Room bounds, used to tell which room the player is in")]
    public Vector3 boundsCenter;
    public Vector3 boundsSize;

    public Bounds WorldBounds => new Bounds(boundsCenter, boundsSize);

    public bool Contains(Vector3 worldPoint)
    {
        return WorldBounds.Contains(worldPoint);
    }

    public int CountCorrect()
    {
        int correct = 0;
        foreach (Book b in items)
        {
            if (b != null && b.IsCorrectlyShelved)
            {
                correct++;
            }
        }

        return correct;
    }

    /// <summary>A shelf is finished when every slot holds a correct item.</summary>
    public int CountFinishedShelves()
    {
        int finished = 0;
        foreach (Shelf shelf in shelves)
        {
            bool all = shelf.slots.Length > 0;
            foreach (ShelfSlot slot in shelf.slots)
            {
                if (slot.occupant == null || !slot.occupant.IsCorrectlyShelved)
                {
                    all = false;
                    break;
                }
            }

            if (all)
            {
                finished++;
            }
        }

        return finished;
    }

    /// <summary>Takes every item off the shelves (and out of anyone's hands is the caller's job) and drops them on a jittered grid on the floor.</summary>
    public void Scatter(int seed)
    {
        var list = new List<Book>();
        foreach (Book b in items)
        {
            if (b != null)
            {
                list.Add(b);
            }
        }

        foreach (Book b in list)
        {
            if (b.currentSlot != null)
            {
                b.currentSlot.Release();
            }

            b.StopPhysicsTumble();
            b.transform.SetParent(null, true);
            Collider c = b.GetComponent<Collider>();
            if (c != null)
            {
                c.enabled = true;
                c.isTrigger = true;
            }

            b.Outline?.StopAll();
        }

        float w = Mathf.Max(0.5f, scatterMax.x - scatterMin.x);
        float h = Mathf.Max(0.5f, scatterMax.y - scatterMin.y);
        int cols = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(list.Count * w / h)));
        int rows = Mathf.Max(1, Mathf.CeilToInt(list.Count / (float)cols));
        var cells = new List<Vector2>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                cells.Add(new Vector2(scatterMin.x + (c + 0.5f) * w / cols, scatterMin.y + (r + 0.5f) * h / rows));
            }
        }

        var rng = new System.Random(seed);
        for (int i = cells.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cells[i], cells[j]) = (cells[j], cells[i]);
        }

        float jitterX = w / cols * 0.3f, jitterZ = h / rows * 0.3f;
        for (int i = 0; i < list.Count; i++)
        {
            Book b = list[i];
            float jx = (float)(rng.NextDouble() - 0.5) * 2f * jitterX;
            float jz = (float)(rng.NextDouble() - 0.5) * 2f * jitterZ;
            b.transform.SetPositionAndRotation(new Vector3(cells[i].x + jx, b.transform.localScale.y * 0.5f, cells[i].y + jz), Quaternion.Euler(0f, rng.Next(0, 360), 0f));
        }
    }

    /// <summary>Puts every item in its home slot (admin / tests). Items in anyone's hands should be released first.</summary>
    public void PlaceAllHome()
    {
        if (homeSlots == null || homeSlots.Length != items.Length)
        {
            return;
        }

        foreach (Book b in items)
        {
            if (b != null && b.currentSlot != null)
            {
                b.currentSlot.Release();
            }
        }

        for (int i = 0; i < items.Length; i++)
        {
            Book b = items[i];
            if (b == null || homeSlots[i] == null)
            {
                continue;
            }

            b.StopPhysicsTumble();
            b.transform.SetParent(null, true);
            homeSlots[i].Accept(b, b.transform.localScale);
        }
    }
}
