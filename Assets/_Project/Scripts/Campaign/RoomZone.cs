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

    [Tooltip("How much bigger than life size the items are: piles are planned wider (spacing and radius) by this factor.")]
    public float pileSpread = 1f;

    [Header("Room bounds, used to tell which room the player is in")]
    public Vector3 boundsCenter;
    public Vector3 boundsSize;

    public Bounds WorldBounds => new Bounds(boundsCenter, boundsSize);

    /// <summary>Switches the whole room on or off: its walls, containers and lights (the room object) and also its items and finds, which
    /// do not stay parented to the room (shelving and tossing move them to the scene root), so they are switched one by one. A switched-off
    /// room costs nothing: no renderers, colliders, lights or scripts.</summary>
    public void SetLive(bool live)
    {
        gameObject.SetActive(live);
        if (items != null)
        {
            foreach (Book b in items)
            {
                if (b != null)
                {
                    b.gameObject.SetActive(live);
                }
            }
        }

        if (finds != null)
        {
            foreach (Book b in finds)
            {
                if (b != null)
                {
                    b.gameObject.SetActive(live);
                }
            }
        }
    }

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

    /// <summary>One item to drop: where it starts, how it is thrown, and in which round (a pile gets one item per round).</summary>
    public struct PileDrop
    {
        public Book Book;
        public Vector3 Origin;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 Spin;
        public int Round;
        public int Pile;
    }

    /// <summary>True while a pile-up is still dropping and settling.</summary>
    public bool IsPiling { get; private set; }

    /// <summary>The plan of the last pile-up (centres and sizes), for tests and tools.</summary>
    public PilePlan LastPlan { get; private set; }

    /// <summary>Takes every item off its shelf (or the floor) ready for a pile-up and plans where each drops: several random, uneven piles
    /// inside the scatter rectangle. Items must not be in anyone's hands. Returns the drops in order; nothing has been thrown yet.</summary>
    public System.Collections.Generic.List<PileDrop> PlanDrops(int seed)
    {
        var list = new System.Collections.Generic.List<Book>();
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
        }

        LastPlan = PilePlanner.Plan(list.Count, scatterMin.x, scatterMin.y, scatterMax.x, scatterMax.y, seed, 1.3f * Mathf.Max(1f, pileSpread));
        var rng = new System.Random(seed * 31 + 7);
        var rounds = new int[LastPlan.PileCount];
        var drops = new System.Collections.Generic.List<PileDrop>();
        for (int i = 0; i < list.Count; i++)
        {
            int pile = LastPlan.PileOf[i];
            int round = rounds[pile]++;
            double angle = rng.NextDouble() * System.Math.PI * 2.0;
            float radius = (0.12f + 0.05f * Mathf.Sqrt(LastPlan.ItemsIn(pile))) * Mathf.Max(1f, pileSpread) * (float)rng.NextDouble();
            float height = 0.9f + 0.15f * (float)rng.NextDouble();
            var origin = new Vector3(LastPlan.CenterX[pile] + Mathf.Cos((float)angle) * radius, height, LastPlan.CenterZ[pile] + Mathf.Sin((float)angle) * radius);
            double toss = rng.NextDouble() * System.Math.PI * 2.0;
            float speed = 0.3f + 0.6f * (float)rng.NextDouble();
            var velocity = new Vector3(Mathf.Cos((float)toss) * speed, -0.5f, Mathf.Sin((float)toss) * speed);
            var spin = new Vector3((float)(rng.NextDouble() - 0.5) * 8f, (float)(rng.NextDouble() - 0.5) * 8f, (float)(rng.NextDouble() - 0.5) * 8f);
            var rotation = Quaternion.Euler((float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 360f, (float)rng.NextDouble() * 360f);
            drops.Add(new PileDrop { Book = list[i], Origin = origin, Rotation = rotation, Velocity = velocity, Spin = spin, Round = round, Pile = pile });
        }

        drops.Sort((a, c) => a.Round != c.Round ? a.Round.CompareTo(c.Round) : a.Pile.CompareTo(c.Pile));
        return drops;
    }

    /// <summary>"Pile up the books": every item of the room is picked up off its shelf or the floor and tossed down in several random
    /// piles in the middle of the room. They really fall (a burst of one item per pile every 0.1 s) and stay solid until the whole
    /// room has landed, then become normal pick-up-able items again.</summary>
    public void PileUp(int seed)
    {
        if (items == null || items.Length == 0)
        {
            return;
        }

        if (IsPiling)
        {
            StopAllCoroutines();
            foreach (Book b in items)
            {
                if (b != null)
                {
                    b.EndSolid();
                }
            }

            IsPiling = false;
        }

        var drops = PlanDrops(seed);
        StartCoroutine(PileRoutine(drops));
    }

    private const float BurstGap = 0.1f;

    private System.Collections.IEnumerator PileRoutine(System.Collections.Generic.List<PileDrop> drops)
    {
        IsPiling = true;
        int rounds = 0;
        foreach (PileDrop d in drops)
        {
            rounds = Mathf.Max(rounds, d.Round + 1);
        }

        float stayHard = rounds * BurstGap + 2.5f;   // everything stays solid until the last item has had time to land
        int next = 0;
        for (int round = 0; round < rounds; round++)
        {
            while (next < drops.Count && drops[next].Round == round)
            {
                PileDrop d = drops[next++];
                if (d.Book == null)
                {
                    continue;
                }

                d.Book.transform.rotation = d.Rotation;
                d.Book.Toss(d.Origin, d.Velocity, d.Spin, stayHard - round * BurstGap);
            }

            yield return new WaitForSeconds(BurstGap);
        }

        yield return new WaitForSeconds(2.6f);

        // "piling" means done only when every item has settled back into a normal book (bounded: at most 20 s more)
        for (int guard = 0; guard < 200; guard++)
        {
            bool anySolid = false;
            foreach (PileDrop d in drops)
            {
                if (d.Book != null && d.Book.hasPhysicsDrop)
                {
                    anySolid = true;
                    break;
                }
            }

            if (!anySolid)
            {
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        IsPiling = false;
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
