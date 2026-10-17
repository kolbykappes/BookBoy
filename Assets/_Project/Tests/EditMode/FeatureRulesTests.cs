using System.Collections.Generic;
using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class CarryListTests
    {
        private class Thing { public string Name; public Thing(string n) { Name = n; } }

        [Test]
        public void TryAdd_FillsToCapacity_ThenRefuses()
        {
            var list = new CarryList<Thing>(2);
            Assert.IsTrue(list.TryAdd(new Thing("a")));
            Assert.IsTrue(list.TryAdd(new Thing("b")));
            Assert.IsTrue(list.IsFull);
            Assert.IsFalse(list.TryAdd(new Thing("c")));
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void TryAdd_RejectsNullAndDuplicates()
        {
            var list = new CarryList<Thing>(4);
            var a = new Thing("a");
            Assert.IsFalse(list.TryAdd(null));
            Assert.IsTrue(list.TryAdd(a));
            Assert.IsFalse(list.TryAdd(a));
        }

        [Test]
        public void NewestPickupBecomesCurrent()
        {
            var list = new CarryList<Thing>(4);
            var a = new Thing("a"); var b = new Thing("b");
            list.TryAdd(a); list.TryAdd(b);
            Assert.AreSame(b, list.Current);
        }

        [Test]
        public void Cycle_WrapsBothWays()
        {
            var list = new CarryList<Thing>(4);
            var a = new Thing("a"); var b = new Thing("b"); var c = new Thing("c");
            list.TryAdd(a); list.TryAdd(b); list.TryAdd(c);
            Assert.AreSame(c, list.Current);
            list.Cycle(1); Assert.AreSame(a, list.Current);
            list.Cycle(-1); Assert.AreSame(c, list.Current);
            list.Cycle(-1); Assert.AreSame(b, list.Current);
        }

        [Test]
        public void Cycle_WithOneOrZeroItems_DoesNothing()
        {
            var list = new CarryList<Thing>(4);
            list.Cycle(1);
            Assert.IsNull(list.Current);
            var a = new Thing("a");
            list.TryAdd(a);
            list.Cycle(1);
            Assert.AreSame(a, list.Current);
        }

        [Test]
        public void RemoveCurrent_KeepsAValidCurrent()
        {
            var list = new CarryList<Thing>(4);
            var a = new Thing("a"); var b = new Thing("b"); var c = new Thing("c");
            list.TryAdd(a); list.TryAdd(b); list.TryAdd(c);
            Assert.AreSame(c, list.RemoveCurrent());
            Assert.AreEqual(2, list.Count);
            Assert.IsNotNull(list.Current);
            list.RemoveCurrent(); list.RemoveCurrent();
            Assert.IsNull(list.Current);
            Assert.AreEqual(-1, list.CurrentIndex);
            Assert.IsTrue(list.IsEmpty);
        }

        [Test]
        public void Remove_ItemBeforeCurrent_KeepsSameCurrentItem()
        {
            var list = new CarryList<Thing>(4);
            var a = new Thing("a"); var b = new Thing("b"); var c = new Thing("c");
            list.TryAdd(a); list.TryAdd(b); list.TryAdd(c);
            list.Remove(a);
            Assert.AreSame(c, list.Current);
        }

        [Test]
        public void Remove_UnknownItem_ReturnsFalse()
        {
            var list = new CarryList<Thing>(4);
            Assert.IsFalse(list.Remove(new Thing("x")));
        }

        [Test]
        public void SetCapacity_Grows_AndShrinkNeverDropsItems()
        {
            var list = new CarryList<Thing>(2);
            list.TryAdd(new Thing("a")); list.TryAdd(new Thing("b"));
            list.SetCapacity(4);
            Assert.IsTrue(list.TryAdd(new Thing("c")));
            list.SetCapacity(1);
            Assert.AreEqual(3, list.Count);
            Assert.IsFalse(list.TryAdd(new Thing("d")));
        }
    }

    public class UpgradeStateTests
    {
        [Test]
        public void StartsWithBaseCapacityAndNothingUnlocked()
        {
            var u = new UpgradeState();
            Assert.AreEqual(4, u.CarryCapacity);
            Assert.AreEqual(1f, u.MoveSpeedMultiplier);
            Assert.IsFalse(u.HasQuickShelve);
            Assert.IsFalse(u.HasSisterGlow);
            Assert.AreEqual(0, u.Points);
        }

        [Test]
        public void CannotUnlockWithoutPoints()
        {
            var u = new UpgradeState();
            Assert.IsFalse(u.TryUnlock("arms1"));
            Assert.AreEqual(4, u.CarryCapacity);
        }

        [Test]
        public void Unlock_SpendsPointsAndAppliesEffect()
        {
            var u = new UpgradeState();
            u.AddPoints(3);
            Assert.IsTrue(u.TryUnlock("arms1"));
            Assert.AreEqual(2, u.Points);
            Assert.AreEqual(5, u.CarryCapacity);
            Assert.IsFalse(u.TryUnlock("arms1"), "already owned");
        }

        [Test]
        public void Unlock_RequiresThePreviousTier()
        {
            var u = new UpgradeState();
            u.AddPoints(10);
            Assert.IsFalse(u.TryUnlock("arms2"));
            Assert.IsTrue(u.TryUnlock("arms1"));
            Assert.IsTrue(u.TryUnlock("arms2"));
            Assert.AreEqual(6, u.CarryCapacity);
        }

        [Test]
        public void SpeedTiersStack()
        {
            var u = new UpgradeState();
            u.AddPoints(10);
            u.TryUnlock("feet1");
            u.TryUnlock("feet2");
            Assert.AreEqual(1.3f, u.MoveSpeedMultiplier, 0.0001f);
        }

        [Test]
        public void AbilityUnlocks()
        {
            var u = new UpgradeState();
            u.AddPoints(10);
            Assert.IsTrue(u.TryUnlock("glow"));
            Assert.IsTrue(u.HasSisterGlow);
            Assert.AreEqual(4f, u.GlowSeconds);
            Assert.IsTrue(u.TryUnlock("glow2"));
            Assert.AreEqual(8f, u.GlowSeconds);
            Assert.IsTrue(u.TryUnlock("shelve"));
            Assert.IsTrue(u.HasQuickShelve);
        }

        [Test]
        public void AddPoints_IgnoresNonPositive()
        {
            var u = new UpgradeState();
            u.AddPoints(0); u.AddPoints(-5);
            Assert.AreEqual(0, u.Points);
            Assert.AreEqual(0, u.TotalEarned);
        }

        [Test]
        public void UnlockAll_And_Reset()
        {
            var u = new UpgradeState();
            u.UnlockAll();
            Assert.AreEqual(8, u.CarryCapacity);
            Assert.IsTrue(u.HasQuickShelve && u.HasSisterGlow);
            u.Reset();
            Assert.AreEqual(4, u.CarryCapacity);
            Assert.IsFalse(u.HasQuickShelve);
        }

        [Test]
        public void Catalog_HasUniqueIds_ValidPrerequisites_AndExceedsTheEightPointsInTheRoom()
        {
            var ids = new HashSet<string>();
            int total = 0;
            foreach (UpgradeNode n in UpgradeCatalog.All)
            {
                Assert.IsTrue(ids.Add(n.Id), "duplicate id " + n.Id);
                if (n.Requires != null)
                {
                    Assert.IsNotNull(UpgradeCatalog.Find(n.Requires), n.Id + " requires a missing node");
                }

                Assert.Greater(n.Cost, 0);
                total += n.Cost;
            }

            Assert.Greater(total, 8, "the tree should force choices with only 8 points available");
        }
    }

    public class ProgressRulesTests
    {
        [Test]
        public void FinishedTracker_AwardsEachShelfOnce()
        {
            var t = new FinishedTracker();
            Assert.AreEqual(1, t.Update(new[] { true, false, false }));
            Assert.AreEqual(0, t.Update(new[] { true, false, false }));
            Assert.AreEqual(2, t.Update(new[] { true, true, true }));
            Assert.AreEqual(3, t.AwardedCount);
        }

        [Test]
        public void FinishedTracker_BreakingAndRefinishingDoesNotPayTwice()
        {
            var t = new FinishedTracker();
            t.Update(new[] { true });
            t.Update(new[] { false });
            Assert.AreEqual(0, t.Update(new[] { true }));
        }

        [Test]
        public void FinishedTracker_ReportsWhichShelvesWereNew()
        {
            var t = new FinishedTracker();
            var fresh = new List<int>();
            t.Update(new[] { false, true, true }, fresh);
            CollectionAssert.AreEqual(new[] { 1, 2 }, fresh);
        }

        [TestCase(0.0, "0:00")]
        [TestCase(5.9, "0:05")]
        [TestCase(65, "1:05")]
        [TestCase(599.99, "9:59")]
        [TestCase(3600, "1:00:00")]
        [TestCase(3725, "1:02:05")]
        [TestCase(-3, "0:00")]
        public void TimeFormat_Formats(double seconds, string expected)
        {
            Assert.AreEqual(expected, TimeFormat.Format(seconds));
        }
    }

    public class ShelfPlannerTests
    {
        private const int None = PlacementRules.NoClaim;

        // 3 bookcases x 2 shelves x 4 slots
        private static (int[] caseColor, int[][] series, bool[][][] taken) Empty()
        {
            var cc = new[] { None, None, None };
            var ss = new[] { new[] { None, None }, new[] { None, None }, new[] { None, None } };
            var tk = new bool[3][][];
            for (int c = 0; c < 3; c++) { tk[c] = new bool[2][]; for (int s = 0; s < 2; s++) tk[c][s] = new bool[4]; }
            return (cc, ss, tk);
        }

        [Test]
        public void FirstBook_GoesToTheFirstEmptyCase_FirstEmptyShelf_AndItsVolumeSlot()
        {
            var (cc, ss, tk) = Empty();
            var t = ShelfPlanner.Plan(2, 0, 3, cc, ss, tk);
            Assert.IsTrue(t.Valid);
            Assert.AreEqual(0, t.Bookcase); Assert.AreEqual(0, t.Shelf); Assert.AreEqual(2, t.Slot);
        }

        [Test]
        public void ClaimedCase_AndClaimedShelf_AreReused()
        {
            var (cc, ss, tk) = Empty();
            cc[1] = 2; ss[1][1] = 0; tk[1][1][0] = true;
            var t = ShelfPlanner.Plan(2, 0, 2, cc, ss, tk);
            Assert.IsTrue(t.Valid);
            Assert.AreEqual(1, t.Bookcase); Assert.AreEqual(1, t.Shelf); Assert.AreEqual(1, t.Slot);
        }

        [Test]
        public void SecondSeriesOfAColor_TakesTheOtherShelfInThatCase()
        {
            var (cc, ss, tk) = Empty();
            cc[0] = 2; ss[0][0] = 0;
            var t = ShelfPlanner.Plan(2, 1, 1, cc, ss, tk);
            Assert.AreEqual(0, t.Bookcase); Assert.AreEqual(1, t.Shelf); Assert.AreEqual(0, t.Slot);
        }

        [Test]
        public void ADifferentColor_GetsAnUnclaimedCase()
        {
            var (cc, ss, tk) = Empty();
            cc[0] = 2; ss[0][0] = 0;
            var t = ShelfPlanner.Plan(1, 0, 1, cc, ss, tk);
            Assert.AreEqual(1, t.Bookcase);
        }

        [Test]
        public void NoUnclaimedCaseLeft_IsInvalid()
        {
            var (cc, ss, tk) = Empty();
            cc[0] = 0; cc[1] = 1; cc[2] = 2;
            Assert.IsFalse(ShelfPlanner.Plan(3, 0, 1, cc, ss, tk).Valid);
        }

        [Test]
        public void BothShelvesClaimedByOtherSeries_IsInvalid()
        {
            var (cc, ss, tk) = Empty();
            cc[0] = 2; ss[0][0] = 0; ss[0][1] = 1;
            Assert.IsFalse(ShelfPlanner.Plan(2, 5, 1, cc, ss, tk).Valid);
        }

        [Test]
        public void TakenSlot_AndBadVolumes_AreInvalid()
        {
            var (cc, ss, tk) = Empty();
            tk[0][0][1] = true;
            Assert.IsFalse(ShelfPlanner.Plan(2, 0, 2, cc, ss, tk).Valid);
            Assert.IsFalse(ShelfPlanner.Plan(2, 0, 0, cc, ss, tk).Valid);
            Assert.IsFalse(ShelfPlanner.Plan(2, 0, 5, cc, ss, tk).Valid);
        }

        [Test]
        public void Commit_MakesTheNextBookPlanAgainstIt()
        {
            var (cc, ss, tk) = Empty();
            var first = ShelfPlanner.Plan(2, 0, 1, cc, ss, tk);
            ShelfPlanner.Commit(first, 2, 0, cc, ss, tk);
            var second = ShelfPlanner.Plan(2, 0, 2, cc, ss, tk);
            Assert.AreEqual(first.Bookcase, second.Bookcase);
            Assert.AreEqual(first.Shelf, second.Shelf);
            Assert.AreEqual(1, second.Slot);
            var other = ShelfPlanner.Plan(7, 0, 1, cc, ss, tk);
            Assert.AreNotEqual(first.Bookcase, other.Bookcase);
        }
    }
}
