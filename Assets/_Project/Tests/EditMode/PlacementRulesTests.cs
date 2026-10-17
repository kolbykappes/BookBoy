using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class PlacementRulesTests
    {
        private const int None = PlacementRules.NoClaim;

        [Test]
        public void Evaluate_AllRight_IsCorrect()
        {
            Assert.AreEqual(PlacementResult.Correct, PlacementRules.Evaluate(true, true, 1, 0));
            Assert.AreEqual(PlacementResult.Correct, PlacementRules.Evaluate(true, true, 4, 3));
        }

        [Test]
        public void Evaluate_WrongBookcase_IsRed_NoMatterWhatElse()
        {
            Assert.AreEqual(PlacementResult.WrongBookcase, PlacementRules.Evaluate(false, true, 1, 0));
            Assert.AreEqual(PlacementResult.WrongBookcase, PlacementRules.Evaluate(false, false, 2, 5));
        }

        [Test]
        public void Evaluate_RightBookcaseWrongShelf_IsYellow()
        {
            Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.Evaluate(true, false, 1, 0));
        }

        [Test]
        public void Evaluate_RightShelfWrongSlot_IsYellow()
        {
            Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.Evaluate(true, true, 1, 1));
            Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.Evaluate(true, true, 4, 0));
        }

        [TestCase(0, 0)]
        [TestCase(0, -1)]
        [TestCase(-3, 0)]
        public void Evaluate_InvalidVolume_IsNeverCorrect(int volume, int slotIndex)
        {
            Assert.AreNotEqual(PlacementResult.Correct, PlacementRules.Evaluate(true, true, volume, slotIndex));
        }

        [Test]
        public void ClaimHolds_OnlyClaimant_Holds()
        {
            Assert.IsTrue(PlacementRules.ClaimHolds(1, new[] { None, 2, None }, new[] { double.MaxValue, 5.0, double.MaxValue }, 2));
        }

        [Test]
        public void ClaimHolds_DifferentValue_DoesNotHold()
        {
            Assert.IsFalse(PlacementRules.ClaimHolds(0, new[] { 1, None }, new[] { 5.0, double.MaxValue }, 2));
        }

        [Test]
        public void ClaimHolds_NoClaimOrBadIndex_DoesNotHold()
        {
            Assert.IsFalse(PlacementRules.ClaimHolds(0, new[] { None, 1 }, new[] { double.MaxValue, 1.0 }, 1));
            Assert.IsFalse(PlacementRules.ClaimHolds(-1, new[] { 1 }, new[] { 1.0 }, 1));
            Assert.IsFalse(PlacementRules.ClaimHolds(1, new[] { 1 }, new[] { 1.0 }, 1));
        }

        [Test]
        public void ClaimHolds_TwoContainersClaimSameValue_EarlierWins()
        {
            var values = new[] { 2, 2 };
            var times = new[] { 10.0, 4.0 };
            Assert.IsFalse(PlacementRules.ClaimHolds(0, values, times, 2));
            Assert.IsTrue(PlacementRules.ClaimHolds(1, values, times, 2));
        }

        [Test]
        public void ClaimHolds_SameTime_LowerIndexWins()
        {
            var values = new[] { 2, 2 };
            var times = new[] { 3.0, 3.0 };
            Assert.IsTrue(PlacementRules.ClaimHolds(0, values, times, 2));
            Assert.IsFalse(PlacementRules.ClaimHolds(1, values, times, 2));
        }

        [Test]
        public void ClaimHolds_OtherContainerClaimsDifferentValue_DoesNotInterfere()
        {
            Assert.IsTrue(PlacementRules.ClaimHolds(1, new[] { 3, 2 }, new[] { 1.0, 9.0 }, 2));
        }

        [Test]
        public void NearestSlot_PicksClosestByPosition()
        {
            var slots = new[] { -0.108f, -0.036f, 0.036f, 0.108f };
            Assert.AreEqual(0, PlacementRules.NearestSlot(slots, -2f));
            Assert.AreEqual(1, PlacementRules.NearestSlot(slots, -0.04f));
            Assert.AreEqual(2, PlacementRules.NearestSlot(slots, 0.01f));
            Assert.AreEqual(3, PlacementRules.NearestSlot(slots, 5f));
        }

        [Test]
        public void NearestSlot_Tie_GoesToLowerIndex()
        {
            Assert.AreEqual(0, PlacementRules.NearestSlot(new[] { -0.3f, 0.3f }, 0f));
        }

        [Test]
        public void NearestSlot_NoSlots_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, PlacementRules.NearestSlot(new float[0], 0f));
        }

        [Test]
        public void IsCleanupComplete_NoBooks_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.IsCleanupComplete(0, 0));
        }

        [Test]
        public void IsCleanupComplete_AllCorrect_ReturnsTrue()
        {
            Assert.IsTrue(PlacementRules.IsCleanupComplete(32, 32));
        }

        [Test]
        public void IsCleanupComplete_OneNotCorrect_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.IsCleanupComplete(32, 31));
        }

        [Test]
        public void IsCleanupComplete_MoreCorrectThanTotal_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.IsCleanupComplete(32, 33));
        }
    }
}
