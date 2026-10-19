using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class CampaignPlacementTests
    {
        private const PlacementMode Group = PlacementMode.Group;
        private const PlacementMode Ordered = PlacementMode.Ordered;

        [Test]
        public void AnyFamilyContainerAcceptsEverything_OtherwiseOnlyItsOwn()
        {
            Assert.IsTrue(PlacementRules.FamilyAccepted(PlacementRules.AnyFamily, 3));
            Assert.IsTrue(PlacementRules.FamilyAccepted(2, 2));
            Assert.IsFalse(PlacementRules.FamilyAccepted(2, 3));
        }

        [Test]
        public void SetFitsShelf_OnlyWhenTheSetIsTheSizeOfTheRow_OrNothingIsSpecified()
        {
            Assert.IsTrue(PlacementRules.SetFitsShelf(8, 8));
            Assert.IsTrue(PlacementRules.SetFitsShelf(4, 4));
            Assert.IsFalse(PlacementRules.SetFitsShelf(4, 8), "a set of 4 does not belong on a row of 8");
            Assert.IsFalse(PlacementRules.SetFitsShelf(8, 4), "a set of 8 does not belong on a row of 4");
            Assert.IsTrue(PlacementRules.SetFitsShelf(0, 8), "no set size means any shelf");
            Assert.IsTrue(PlacementRules.SetFitsShelf(8, 0), "unknown shelf size means any set");
        }

        [Test]
        public void SetOfTheWrongSizeForTheRow_IsYellow_InBothModes_ButNeverOverridesRed()
        {
            Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.EvaluateCampaign(Group, true, true, 1, 0, false));
            Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.EvaluateCampaign(Ordered, true, true, 1, 0, false));
            Assert.AreEqual(PlacementResult.WrongBookcase, PlacementRules.EvaluateCampaign(Group, false, true, 1, 0, false), "wrong family stays red");
            Assert.AreEqual(PlacementResult.Correct, PlacementRules.EvaluateCampaign(Group, true, true, 1, 0, true));
        }

        [Test]
        public void Ordered_NumbersOneToEightFillARowOfEight_InTheirOwnSlots()
        {
            for (int n = 1; n <= 8; n++)
            {
                Assert.AreEqual(PlacementResult.Correct, PlacementRules.EvaluateCampaign(Ordered, true, true, n, n - 1, true), "number " + n);
                Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.EvaluateCampaign(Ordered, true, true, n, n % 8, true), "number " + n + " in the next slot");
            }
        }

        [Test]
        public void Group_AnySlotIsGreen_WhenTheShelfIsOwnedByTheGroup()
        {
            for (int slot = 0; slot < 5; slot++)
            {
                Assert.AreEqual(PlacementResult.Correct, PlacementRules.EvaluateCampaign(Group, true, true, 4, slot), "slot " + slot);
            }
        }

        [Test]
        public void Group_AnotherGroupOnTheShelf_IsYellow()
        {
            Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.EvaluateCampaign(Group, true, false, 1, 0));
        }

        [Test]
        public void WrongFamilyContainer_IsRed_EvenIfEmpty_InBothModes()
        {
            Assert.AreEqual(PlacementResult.WrongBookcase, PlacementRules.EvaluateCampaign(Group, false, true, 1, 0));
            Assert.AreEqual(PlacementResult.WrongBookcase, PlacementRules.EvaluateCampaign(Ordered, false, true, 1, 0));
            Assert.AreEqual(PlacementResult.WrongBookcase, PlacementRules.EvaluateCampaign(Group, false, false, 1, 0));
        }

        [Test]
        public void Ordered_NumberNBelongsInSlotN()
        {
            for (int n = 1; n <= 5; n++)
            {
                Assert.AreEqual(PlacementResult.Correct, PlacementRules.EvaluateCampaign(Ordered, true, true, n, n - 1));
                Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.EvaluateCampaign(Ordered, true, true, n, n % 5));
            }
        }

        [Test]
        public void Ordered_WrongGroupIsYellowEvenInTheRightSlot_AndNumberBelowOneIsNeverGreen()
        {
            Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.EvaluateCampaign(Ordered, true, false, 2, 1));
            Assert.AreEqual(PlacementResult.RightBookcaseWrongSpot, PlacementRules.EvaluateCampaign(Ordered, true, true, 0, 0));
        }

        [Test]
        public void EarliestGroupClaimWinsAcrossTheRoomsShelves()
        {
            // two shelves both claim group 7; the earlier one owns it, the later one's books are yellow
            var values = new[] { 7, 7, 3 };
            var times = new[] { 10.0, 5.0, 1.0 };
            Assert.IsFalse(PlacementRules.ClaimHolds(0, values, times, 7));
            Assert.IsTrue(PlacementRules.ClaimHolds(1, values, times, 7));
            Assert.IsTrue(PlacementRules.ClaimHolds(2, values, times, 3));
        }

        [Test]
        public void RoomIsTidyOnlyWhenEveryItemIsCorrect()
        {
            Assert.IsFalse(PlacementRules.IsCleanupComplete(0, 0));
            Assert.IsFalse(PlacementRules.IsCleanupComplete(100, 99));
            Assert.IsTrue(PlacementRules.IsCleanupComplete(100, 100));
        }
    }
}
