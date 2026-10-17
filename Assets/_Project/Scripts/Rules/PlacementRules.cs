using System;

namespace BookBoy.Rules
{
    public static class PlacementRules
    {
        /// <summary>A slot accepts a book only by exact bookId, and only while empty. Category and position never matter.
        /// An unconfigured (null/empty) id on either side never matches.</summary>
        public static bool Accepts(string slotAcceptedBookId, bool slotOccupied, string bookId)
        {
            if (slotOccupied || string.IsNullOrEmpty(slotAcceptedBookId) || string.IsNullOrEmpty(bookId))
            {
                return false;
            }

            return string.Equals(slotAcceptedBookId, bookId, StringComparison.Ordinal);
        }

        /// <summary>Cleanup is complete only when there is at least one slot and every slot is occupied.</summary>
        public static bool IsCleanupComplete(int totalSlots, int occupiedSlots)
        {
            return totalSlots > 0 && occupiedSlots == totalSlots;
        }
    }
}
