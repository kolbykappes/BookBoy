using System;

namespace BookBoy.Rules
{
    /// <summary>Where the piles of a room go and which item goes in which pile.</summary>
    public sealed class PilePlan
    {
        public PilePlan(float[] centerX, float[] centerZ, int[] pileOf)
        {
            CenterX = centerX;
            CenterZ = centerZ;
            PileOf = pileOf;
        }

        public float[] CenterX { get; }
        public float[] CenterZ { get; }

        /// <summary>For each item (by index), the pile it goes in.</summary>
        public int[] PileOf { get; }

        public int PileCount => CenterX.Length;

        public int ItemsIn(int pile)
        {
            int n = 0;
            foreach (int p in PileOf)
            {
                if (p == pile)
                {
                    n++;
                }
            }

            return n;
        }
    }

    /// <summary>Plans a room's items into several randomly placed, unevenly sized piles inside a rectangle. Engine-free and seeded,
    /// so the same seed always gives the same layout.</summary>
    public static class PilePlanner
    {
        public const int MinPiles = 3;
        public const int MaxPiles = 7;

        /// <summary>About one pile per <paramref name="itemsPerPile"/> items (18 by default), between 3 and <paramref name="maxPiles"/> (7 by default,
        /// raised for the big performance test room), never more piles than items.</summary>
        public static int PileCountFor(int items, int maxPiles = MaxPiles, int itemsPerPile = 18)
        {
            if (items <= 0)
            {
                return 0;
            }

            int piles = (int)Math.Round(items / (double)Math.Max(1, itemsPerPile));
            piles = Math.Max(MinPiles, Math.Min(Math.Max(MinPiles, maxPiles), piles));
            return Math.Min(piles, items);
        }

        public static PilePlan Plan(int items, float minX, float minZ, float maxX, float maxZ, int seed, float minSeparation = 1.3f, int maxPiles = MaxPiles, int itemsPerPile = 18)
        {
            int piles = PileCountFor(items, maxPiles, itemsPerPile);
            var rng = new Random(seed);
            var cx = new float[piles];
            var cz = new float[piles];

            // pile centres: random points in the rectangle, at least minSeparation apart (relaxed if the rectangle is too small)
            float separation = minSeparation;
            for (int p = 0; p < piles; p++)
            {
                for (int attempt = 0; ; attempt++)
                {
                    float x = minX + (float)rng.NextDouble() * (maxX - minX);
                    float z = minZ + (float)rng.NextDouble() * (maxZ - minZ);
                    bool ok = true;
                    for (int q = 0; q < p && ok; q++)
                    {
                        float dx = x - cx[q], dz = z - cz[q];
                        ok = dx * dx + dz * dz >= separation * separation;
                    }

                    if (ok || attempt >= 60)
                    {
                        cx[p] = x;
                        cz[p] = z;
                        break;
                    }

                    if (attempt % 20 == 19)
                    {
                        separation *= 0.8f;
                    }
                }
            }

            // uneven pile sizes: every pile gets at least one item, the rest are dealt by random weights
            var weight = new double[piles];
            double total = 0;
            for (int p = 0; p < piles; p++)
            {
                weight[p] = 0.4 + rng.NextDouble() * 1.6;
                total += weight[p];
            }

            var order = new int[items];
            for (int i = 0; i < items; i++)
            {
                order[i] = i;
            }

            for (int i = items - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }

            var pileOf = new int[items];
            for (int k = 0; k < items; k++)
            {
                int pile;
                if (k < piles)
                {
                    pile = k;
                }
                else
                {
                    double r = rng.NextDouble() * total;
                    pile = 0;
                    while (pile < piles - 1 && r >= weight[pile])
                    {
                        r -= weight[pile];
                        pile++;
                    }
                }

                pileOf[order[k]] = pile;
            }

            return new PilePlan(cx, cz, pileOf);
        }
    }
}
