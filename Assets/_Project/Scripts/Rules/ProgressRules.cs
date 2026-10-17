using System;
using System.Collections.Generic;

namespace BookBoy.Rules
{
    /// <summary>Awards one point per shelf the first time it becomes finished. A shelf that is broken up and finished
    /// again does not pay twice.</summary>
    public sealed class FinishedTracker
    {
        private readonly HashSet<int> _awarded = new HashSet<int>();

        public int AwardedCount => _awarded.Count;

        /// <summary>finishedNow[i] says whether shelf i is finished right now. Returns how many shelves were newly awarded.</summary>
        public int Update(IReadOnlyList<bool> finishedNow, List<int> newlyAwarded = null)
        {
            int count = 0;
            for (int i = 0; i < finishedNow.Count; i++)
            {
                if (finishedNow[i] && _awarded.Add(i))
                {
                    count++;
                    newlyAwarded?.Add(i);
                }
            }

            return count;
        }

        public bool WasAwarded(int shelfIndex)
        {
            return _awarded.Contains(shelfIndex);
        }

        public void Reset()
        {
            _awarded.Clear();
        }
    }

    public static class TimeFormat
    {
        /// <summary>m:ss, or h:mm:ss from one hour on.</summary>
        public static string Format(double seconds)
        {
            if (seconds < 0 || double.IsNaN(seconds))
            {
                seconds = 0;
            }

            int total = (int)Math.Floor(seconds);
            int h = total / 3600;
            int m = total % 3600 / 60;
            int s = total % 60;
            return h > 0 ? h + ":" + m.ToString("00") + ":" + s.ToString("00") : m + ":" + s.ToString("00");
        }
    }

    public struct PlanTarget
    {
        public bool Valid;
        public int Bookcase;
        public int Shelf;
        public int Slot;
    }

    /// <summary>Decides where a book belongs, for quick-shelve. Works on a snapshot: per bookcase the claimed color (or
    /// NoClaim), per shelf the claimed series (or NoClaim), and which slots are taken. After planning a book the caller
    /// should record the claim and the taken slot (see <see cref="Commit"/>) so the next book is planned against it.</summary>
    public static class ShelfPlanner
    {
        public static PlanTarget Plan(int bookColor, int bookSeries, int bookVolume, int[] caseColor, int[][] shelfSeries, bool[][][] slotTaken)
        {
            var none = new PlanTarget();
            int bookcase = -1;
            for (int c = 0; c < caseColor.Length && bookcase < 0; c++)
            {
                if (caseColor[c] == bookColor)
                {
                    bookcase = c;
                }
            }

            for (int c = 0; c < caseColor.Length && bookcase < 0; c++)
            {
                if (caseColor[c] == PlacementRules.NoClaim)
                {
                    bookcase = c;
                }
            }

            if (bookcase < 0)
            {
                return none;
            }

            int shelf = -1;
            for (int s = 0; s < shelfSeries[bookcase].Length && shelf < 0; s++)
            {
                if (shelfSeries[bookcase][s] == bookSeries)
                {
                    shelf = s;
                }
            }

            for (int s = 0; s < shelfSeries[bookcase].Length && shelf < 0; s++)
            {
                if (shelfSeries[bookcase][s] == PlacementRules.NoClaim)
                {
                    shelf = s;
                }
            }

            if (shelf < 0)
            {
                return none;
            }

            int slot = bookVolume - 1;
            if (slot < 0 || slot >= slotTaken[bookcase][shelf].Length || slotTaken[bookcase][shelf][slot])
            {
                return none;
            }

            return new PlanTarget { Valid = true, Bookcase = bookcase, Shelf = shelf, Slot = slot };
        }

        public static void Commit(PlanTarget target, int bookColor, int bookSeries, int[] caseColor, int[][] shelfSeries, bool[][][] slotTaken)
        {
            if (!target.Valid)
            {
                return;
            }

            caseColor[target.Bookcase] = bookColor;
            shelfSeries[target.Bookcase][target.Shelf] = bookSeries;
            slotTaken[target.Bookcase][target.Shelf][target.Slot] = true;
        }
    }
}
