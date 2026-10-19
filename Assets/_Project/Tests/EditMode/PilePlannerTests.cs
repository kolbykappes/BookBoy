using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class PilePlannerTests
    {
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(10, 3)]
        [TestCase(54, 3)]
        [TestCase(100, 6)]
        [TestCase(120, 7)]
        [TestCase(500, 7)]
        public void PileCount_IsAboutOnePerEighteenItems_BetweenThreeAndSeven(int items, int expected)
        {
            Assert.AreEqual(expected, PilePlanner.PileCountFor(items));
        }

        [TestCase(1000, 60, 25, 40)]
        [TestCase(3000, 60, 25, 60)]
        [TestCase(1000, 7, 18, 7)]
        [TestCase(10, 60, 25, 3)]
        public void PileCount_CanBeRaisedForABigTestRoom(int items, int maxPiles, int perPile, int expected)
        {
            Assert.AreEqual(expected, PilePlanner.PileCountFor(items, maxPiles, perPile));
        }

        [Test]
        public void ABigPlan_PutsEveryItemInOnePile()
        {
            var plan = PilePlanner.Plan(1000, -11f, -11f, 11f, 11f, 7, 1.3f, 60, 25);
            Assert.AreEqual(40, plan.PileCount);
            int sum = 0;
            for (int p = 0; p < plan.PileCount; p++)
            {
                Assert.GreaterOrEqual(plan.ItemsIn(p), 1);
                sum += plan.ItemsIn(p);
            }

            Assert.AreEqual(1000, sum);
        }

        [Test]
        public void EveryItemIsInOnePile_AndEveryPileHasAtLeastOneItem()
        {
            var plan = PilePlanner.Plan(100, -9f, 1f, -3f, 7f, 42);
            Assert.AreEqual(100, plan.PileOf.Length);
            int sum = 0;
            for (int p = 0; p < plan.PileCount; p++)
            {
                Assert.GreaterOrEqual(plan.ItemsIn(p), 1, "pile " + p);
                sum += plan.ItemsIn(p);
            }

            Assert.AreEqual(100, sum);
            foreach (int p in plan.PileOf)
            {
                Assert.That(p, Is.InRange(0, plan.PileCount - 1));
            }
        }

        [Test]
        public void PileCentresAreInsideTheRectangle_AndApart()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var plan = PilePlanner.Plan(120, 3f, 5f, 9f, 11f, seed);
                for (int a = 0; a < plan.PileCount; a++)
                {
                    Assert.That(plan.CenterX[a], Is.InRange(3f, 9f));
                    Assert.That(plan.CenterZ[a], Is.InRange(5f, 11f));
                    for (int b = a + 1; b < plan.PileCount; b++)
                    {
                        float dx = plan.CenterX[a] - plan.CenterX[b], dz = plan.CenterZ[a] - plan.CenterZ[b];
                        Assert.GreaterOrEqual(System.Math.Sqrt(dx * dx + dz * dz), 0.8, "seed " + seed + " piles " + a + "," + b);
                    }
                }
            }
        }

        [Test]
        public void SameSeedGivesTheSamePlan_DifferentSeedsDiffer()
        {
            var a = PilePlanner.Plan(100, -9f, 1f, -3f, 7f, 7);
            var b = PilePlanner.Plan(100, -9f, 1f, -3f, 7f, 7);
            var c = PilePlanner.Plan(100, -9f, 1f, -3f, 7f, 8);
            CollectionAssert.AreEqual(a.PileOf, b.PileOf);
            CollectionAssert.AreEqual(a.CenterX, b.CenterX);
            CollectionAssert.AreNotEqual(a.PileOf, c.PileOf);
        }

        [Test]
        public void PilesAreUnevenInSize()
        {
            var plan = PilePlanner.Plan(120, -9f, 1f, -3f, 7f, 3);
            int min = int.MaxValue, max = 0;
            for (int p = 0; p < plan.PileCount; p++)
            {
                min = System.Math.Min(min, plan.ItemsIn(p));
                max = System.Math.Max(max, plan.ItemsIn(p));
            }

            Assert.Greater(max, min + 2, "pile sizes should differ");
        }

        [Test]
        public void ATinyRectangleStillPlansEveryPile_WithRelaxedSpacing()
        {
            var plan = PilePlanner.Plan(100, 0f, 0f, 1f, 1f, 5);
            Assert.AreEqual(6, plan.PileCount);
            Assert.AreEqual(100, plan.PileOf.Length);
        }

        [Test]
        public void NoItemsMeansNoPiles()
        {
            var plan = PilePlanner.Plan(0, 0f, 0f, 5f, 5f, 1);
            Assert.AreEqual(0, plan.PileCount);
            Assert.AreEqual(0, plan.PileOf.Length);
        }
    }
}
