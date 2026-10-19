using System.Collections.Generic;
using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class PaletteTests
    {
        [Test]
        public void ContrastRatio_BlackAndWhite_Is21()
        {
            Assert.AreEqual(21.0, Palette.ContrastRatio(Palette.Luminance(1, 1, 1), Palette.Luminance(0, 0, 0)), 1e-9);
            Assert.AreEqual(1.0, Palette.ContrastRatio(0.3, 0.3), 1e-12);
        }

        [Test]
        public void MinLuminanceFor_IsTheInverseOfTheRatio()
        {
            double wood = 0.0687;
            double y = Palette.MinLuminanceFor(wood, 2.5);
            Assert.AreEqual(2.5, Palette.ContrastRatio(y, wood), 1e-9);
            Assert.AreEqual(0.247, y, 0.001, "matches research file 18: 0.247 for a ratio of 2.5 on #6B401F");
        }

        [TestCase(1.0, 1.0, 1.0)]
        [TestCase(0.0, 0.0, 0.0)]
        [TestCase(0.8, 0.3, 0.1)]
        [TestCase(0.2, 0.6, 0.9)]
        public void Oklab_RoundTripsAndWhiteIsOne(double r, double g, double b)
        {
            Palette.LinearToOklab(r, g, b, out double L, out double A, out double B);
            Palette.OklabToLinear(L, A, B, out double r2, out double g2, out double b2);
            Assert.AreEqual(r, r2, 1e-6);
            Assert.AreEqual(g, g2, 1e-6);
            Assert.AreEqual(b, b2, 1e-6);
            if (r == 1.0 && g == 1.0 && b == 1.0)
            {
                Assert.AreEqual(1.0, L, 1e-3);
            }
        }

        [Test]
        public void Simulation_LeavesGreysAlone_AndCollapsesRedGreenForDeutan()
        {
            Palette.Simulate(Vision.Deutan, 0.5, 0.5, 0.5, out double r, out double g, out double b);
            Assert.AreEqual(0.5, r, 0.01);
            Assert.AreEqual(0.5, g, 0.01);
            Assert.AreEqual(0.5, b, 0.01);
            var red = new PaletteColor(0.8, 0.2, 0.2);
            var green = new PaletteColor(0.2, 0.6, 0.2);
            Assert.Greater(Palette.Distance(Vision.Normal, red, green), Palette.Distance(Vision.Deutan, red, green), "red and green are closer under deuteranopia");
        }

        [Test]
        public void Solve_PicksDistinctInGamutColoursThatKeepTheirContrast()
        {
            var wood = new PaletteColor(0.42, 0.25, 0.12);
            double woodY = Palette.Luminance(wood.R, wood.G, wood.B);
            double minY = Palette.MinLuminanceFor(woodY, 2.5);
            List<PaletteColor> set = Palette.Solve(24, minY, wood);
            Assert.AreEqual(24, set.Count);
            double worst = double.MaxValue;
            for (int i = 0; i < set.Count; i++)
            {
                double y = Palette.Luminance(set[i].R, set[i].G, set[i].B);
                Assert.GreaterOrEqual(y, minY - 1e-6, "colour " + i + " keeps its contrast with the wood");
                Assert.That(set[i].R, Is.InRange(0.0, 1.0));
                Assert.That(set[i].G, Is.InRange(0.0, 1.0));
                Assert.That(set[i].B, Is.InRange(0.0, 1.0));
                for (int j = i + 1; j < set.Count; j++)
                {
                    worst = System.Math.Min(worst, Palette.WorstDistance(set[i], set[j]));
                }
            }

            Assert.Greater(worst, 0.035, "no two of the 24 are closer than that for anyone (research file 18 found about 0.045)");
        }

        [Test]
        public void Solve_IsDeterministic_AndTheFirstKAreTheBestK()
        {
            var wood = new PaletteColor(0.42, 0.25, 0.12);
            double minY = Palette.MinLuminanceFor(Palette.Luminance(wood.R, wood.G, wood.B), 2.5);
            var a = Palette.Solve(16, minY, wood);
            var b = Palette.Solve(24, minY, wood);
            for (int i = 0; i < 16; i++)
            {
                Assert.AreEqual(a[i].R, b[i].R, 1e-12);
                Assert.AreEqual(a[i].G, b[i].G, 1e-12);
                Assert.AreEqual(a[i].B, b[i].B, 1e-12);
            }
        }
    }
}
