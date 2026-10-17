using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class PlacementRulesTests
    {
        [Test]
        public void Accepts_MatchingId_EmptySlot_ReturnsTrue()
        {
            Assert.IsTrue(PlacementRules.Accepts("alchemy_1", false, "alchemy_1"));
        }

        [Test]
        public void Accepts_MatchingId_OccupiedSlot_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.Accepts("alchemy_1", true, "alchemy_1"));
        }

        [Test]
        public void Accepts_DifferentId_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.Accepts("alchemy_1", false, "alchemy_2"));
        }

        [Test]
        public void Accepts_SamePrefixDifferentId_ReturnsFalse()
        {
            // Two books of one category look identical; only the exact id may fit this slot.
            Assert.IsFalse(PlacementRules.Accepts("alchemy_1", false, "alchemy_10"));
        }

        [Test]
        public void Accepts_IdsDifferingOnlyByCase_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.Accepts("Alchemy_1", false, "alchemy_1"));
        }

        [TestCase(null, "alchemy_1")]
        [TestCase("", "alchemy_1")]
        [TestCase("alchemy_1", null)]
        [TestCase("alchemy_1", "")]
        [TestCase(null, null)]
        [TestCase("", "")]
        public void Accepts_UnconfiguredId_ReturnsFalse(string slotId, string bookId)
        {
            Assert.IsFalse(PlacementRules.Accepts(slotId, false, bookId));
        }

        [Test]
        public void IsCleanupComplete_NoSlots_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.IsCleanupComplete(0, 0));
        }

        [Test]
        public void IsCleanupComplete_AllOccupied_ReturnsTrue()
        {
            Assert.IsTrue(PlacementRules.IsCleanupComplete(8, 8));
        }

        [Test]
        public void IsCleanupComplete_OneSlotEmpty_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.IsCleanupComplete(8, 7));
        }

        [Test]
        public void IsCleanupComplete_MoreOccupiedThanTotal_ReturnsFalse()
        {
            Assert.IsFalse(PlacementRules.IsCleanupComplete(8, 9));
        }
    }
}
