using System.Collections.Generic;

namespace BookBoy.Rules
{
    public enum PlacementResult
    {
        /// <summary>Red: the book is in a bookcase that belongs to a different color.</summary>
        WrongBookcase,

        /// <summary>Yellow: right bookcase, but the wrong shelf (another series) or the wrong slot.</summary>
        RightBookcaseWrongSpot,

        /// <summary>Green: right bookcase, right shelf (its series), right slot (volume N in slot N).</summary>
        Correct
    }

    /// <summary>How a container grades the items on its shelves (Lamplit). Ordered: number N belongs in slot N. Group: any slot is
    /// fine as long as the whole shelf holds one group.</summary>
    public enum PlacementMode
    {
        Ordered = 0,
        Group = 1
    }

    public static class PlacementRules
    {
        public const int NoClaim = -1;

        /// <summary>A container family of -1 accepts items of any family.</summary>
        public const int AnyFamily = -1;

        /// <summary>True if a container of <paramref name="containerFamily"/> may hold an item of <paramref name="itemFamily"/>.</summary>
        public static bool FamilyAccepted(int containerFamily, int itemFamily)
        {
            return containerFamily == AnyFamily || containerFamily == itemFamily;
        }

        /// <summary>True if a set of <paramref name="setSize"/> items belongs on a shelf with <paramref name="shelfSlots"/> slots: a set of 8
        /// on a row of 8, a set of 4 on a row of 4. A set size or shelf size of 0 or less means "not specified", which always fits.</summary>
        public static bool SetFitsShelf(int setSize, int shelfSlots)
        {
            return setSize <= 0 || shelfSlots <= 0 || setSize == shelfSlots;
        }

        /// <summary>Grades a placed item in the Lamplit rooms. Red: the container's family does not accept the item. Yellow: right
        /// kind of container, but another group owns the shelf (or, in Ordered mode, the number is not in its own slot). Green: all
        /// right. <paramref name="groupClaimHolds"/> comes from <see cref="ClaimHolds"/> over the room's shelves.</summary>
        public static PlacementResult EvaluateCampaign(PlacementMode mode, bool familyAccepted, bool groupClaimHolds, int number, int slotIndex)
        {
            return EvaluateCampaign(mode, familyAccepted, groupClaimHolds, number, slotIndex, true);
        }

        /// <summary>As above, plus the shelf size: <paramref name="setFitsShelf"/> (from <see cref="SetFitsShelf"/>) is false when the item's set is
        /// not the size of this row, which grades yellow (right container, wrong shelf) but never overrides red.</summary>
        public static PlacementResult EvaluateCampaign(PlacementMode mode, bool familyAccepted, bool groupClaimHolds, int number, int slotIndex, bool setFitsShelf)
        {
            if (!familyAccepted)
            {
                return PlacementResult.WrongBookcase;
            }

            if (!groupClaimHolds || !setFitsShelf)
            {
                return PlacementResult.RightBookcaseWrongSpot;
            }

            if (mode == PlacementMode.Ordered && (number < 1 || slotIndex != number - 1))
            {
                return PlacementResult.RightBookcaseWrongSpot;
            }

            return PlacementResult.Correct;
        }

        /// <summary>Grades a placed book. Bookcases and shelves start unlabeled and are claimed by the first book
        /// placed on them, so the two claim flags come from <see cref="ClaimHolds"/>. Volume N belongs in slot N
        /// (0-based index N - 1); a volume below 1 can never be Correct.</summary>
        public static PlacementResult Evaluate(bool bookcaseClaimHolds, bool shelfClaimHolds, int bookVolume, int slotIndex)
        {
            if (!bookcaseClaimHolds)
            {
                return PlacementResult.WrongBookcase;
            }

            if (!shelfClaimHolds || bookVolume < 1 || slotIndex != bookVolume - 1)
            {
                return PlacementResult.RightBookcaseWrongSpot;
            }

            return PlacementResult.Correct;
        }

        /// <summary>True if the container at <paramref name="index"/> legitimately owns <paramref name="bookValue"/>:
        /// its claim equals the value and no other container holding the same value claimed it earlier
        /// (ties go to the lower index). Values/times are per container; NoClaim means nothing placed yet.</summary>
        public static bool ClaimHolds(int index, IReadOnlyList<int> claimValues, IReadOnlyList<double> claimTimes, int bookValue)
        {
            if (index < 0 || index >= claimValues.Count || claimValues[index] != bookValue)
            {
                return false;
            }

            for (int other = 0; other < claimValues.Count; other++)
            {
                if (other == index || claimValues[other] != bookValue)
                {
                    continue;
                }

                if (claimTimes[other] < claimTimes[index] || (claimTimes[other] == claimTimes[index] && other < index))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Index of the slot whose position along the shelf is closest to x, or -1 if there are none.
        /// Ties go to the lower index.</summary>
        public static int NearestSlot(IReadOnlyList<float> slotPositions, float x)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < slotPositions.Count; i++)
            {
                float distance = slotPositions[i] > x ? slotPositions[i] - x : x - slotPositions[i];
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Cleanup is complete only when there is at least one book and every book is Correct.</summary>
        public static bool IsCleanupComplete(int totalBooks, int correctBooks)
        {
            return totalBooks > 0 && correctBooks == totalBooks;
        }
    }
}
