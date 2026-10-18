using System.Collections.Generic;
using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class CampaignCatalogTests
    {
        [Test]
        public void EveryFindHasUniqueIdAndSpot_ARoomInRange_AndRealUpgradeNodes()
        {
            var ids = new HashSet<string>();
            var spots = new HashSet<string>();
            foreach (FindDef f in CampaignCatalog.Finds)
            {
                Assert.IsTrue(ids.Add(f.Id), "duplicate find " + f.Id);
                Assert.IsTrue(spots.Add(f.SpotId), "duplicate spot " + f.SpotId);
                Assert.GreaterOrEqual(f.Room, 1);
                Assert.LessOrEqual(f.Room, CampaignCatalog.RoomCount);
                Assert.IsNotEmpty(f.Grants);
                foreach (string node in f.Grants)
                {
                    Assert.IsNotNull(UpgradeCatalog.Find(node), f.Id + " grants a missing upgrade " + node);
                }
            }

            Assert.IsFalse(spots.Contains(CampaignCatalog.DumbwaiterSpot), "the dumbwaiter spot stays locked");
        }

        [Test]
        public void TheSixDeskEffects_Apron_Shoes_Glasses_ShelfList_PullList_AndTheLockedDumbwaiter()
        {
            Assert.AreEqual(5, CampaignCatalog.Finds.Count);
            Assert.AreEqual("apron_hook", CampaignCatalog.FindById("apron").SpotId);
            Assert.AreEqual("clipboard", CampaignCatalog.FindById("pulllist").SpotId);
            Assert.AreEqual(1, CampaignCatalog.FindById("apron").Room);
            Assert.AreEqual(2, CampaignCatalog.FindById("glasses").Room);
            Assert.AreEqual(3, CampaignCatalog.FindById("pulllist").Room);
            Assert.AreEqual("shoes", CampaignCatalog.FindForSpot("shoe_rack").Id);
            Assert.IsNull(CampaignCatalog.FindForSpot(CampaignCatalog.DumbwaiterSpot));
        }
    }

    public class CampaignStateTests
    {
        [Test]
        public void StartsEmpty_RoomOneOpen_OthersShut()
        {
            var c = new CampaignState();
            Assert.AreEqual(0, c.FoundCount);
            Assert.AreEqual(0, c.InstalledCount);
            Assert.IsTrue(c.IsDoorOpen(1));
            Assert.IsFalse(c.IsDoorOpen(2));
            Assert.IsFalse(c.IsDoorOpen(3));
        }

        [Test]
        public void MarkFound_IsTrueOnlyTheFirstTime_AndIgnoresUnknownFinds()
        {
            var c = new CampaignState();
            Assert.IsTrue(c.MarkFound("apron"));
            Assert.IsFalse(c.MarkFound("apron"));
            Assert.IsFalse(c.MarkFound("nonsense"));
            Assert.IsTrue(c.IsFound("apron"));
            Assert.IsFalse(c.IsFound("shoes"));
        }

        [Test]
        public void ASpotAcceptsOnlyItsOwnFind()
        {
            var c = new CampaignState();
            Assert.AreEqual(InstallResult.WrongSpot, c.TryInstall("apron", "shoe_rack"));
            Assert.IsFalse(c.IsInstalled("apron"));
            Assert.AreEqual(InstallResult.Installed, c.TryInstall("apron", "apron_hook"));
            Assert.IsTrue(c.IsInstalled("apron"));
            Assert.IsTrue(c.IsFound("apron"), "installing counts as found");
            Assert.AreEqual(InstallResult.AlreadyInstalled, c.TryInstall("apron", "apron_hook"));
            Assert.AreEqual(InstallResult.UnknownFind, c.TryInstall("nonsense", "apron_hook"));
        }

        [Test]
        public void TheDumbwaiterIsLockedForEveryFind()
        {
            var c = new CampaignState();
            foreach (FindDef f in CampaignCatalog.Finds)
            {
                Assert.AreEqual(InstallResult.LockedSpot, c.TryInstall(f.Id, CampaignCatalog.DumbwaiterSpot), f.Id);
            }

            Assert.AreEqual(0, c.InstalledCount);
        }

        [Test]
        public void InstalledFinds_GrantTheirUpgrades_ApronRaisesCarryBy2_ShoesRaiseSpeed()
        {
            var c = new CampaignState();
            var u = new UpgradeState();
            c.TryInstall("apron", "apron_hook");
            c.TryInstall("shoes", "shoe_rack");
            Assert.AreEqual(3, c.ApplyTo(u));
            Assert.AreEqual(6, u.CarryCapacity);
            Assert.AreEqual(1.15f, u.MoveSpeedMultiplier, 0.0001f);
            Assert.AreEqual(0, c.ApplyTo(u), "applying again grants nothing new");
            Assert.IsFalse(u.HasSisterGlow);
        }

        [Test]
        public void GlassesShelfListAndPullList_SwitchOnTheThreeAbilities_AndNotQuickShelve()
        {
            var c = new CampaignState();
            var u = new UpgradeState();
            c.TryInstall("glasses", "glasses_stand");
            c.TryInstall("shelflist", "shelf_list_board");
            c.TryInstall("pulllist", "clipboard");
            c.ApplyTo(u);
            Assert.IsTrue(u.HasSisterGlow);
            Assert.IsTrue(u.HasHomeBeacon);
            Assert.IsTrue(u.HasGatherFamily);
            Assert.IsFalse(u.HasQuickShelve);
            Assert.AreEqual(60f, u.CooldownSeconds(AbilityId.SisterGlow));
        }

        [Test]
        public void ADoorOpensWhenThePreviousRoomIsFinished_AndStaysFinished()
        {
            var c = new CampaignState();
            Assert.IsTrue(c.FinishRoom(1));
            Assert.IsFalse(c.FinishRoom(1), "only newly finished counts");
            Assert.IsTrue(c.IsDoorOpen(2));
            Assert.IsFalse(c.IsDoorOpen(3));
            c.FinishRoom(2);
            Assert.IsTrue(c.IsDoorOpen(3));
            Assert.IsFalse(c.FinishRoom(0));
            Assert.IsFalse(c.FinishRoom(4));
        }

        [Test]
        public void AdminCanForceDoorsOpenOrShut_AndClearTheOverrides()
        {
            var c = new CampaignState();
            c.ForceDoor(3, true);
            Assert.IsTrue(c.IsDoorOpen(3));
            c.FinishRoom(1);
            c.ForceDoor(2, false);
            Assert.IsFalse(c.IsDoorOpen(2), "forced shut even though room 1 is finished");
            c.ClearForcedDoors();
            Assert.IsTrue(c.IsDoorOpen(2));
            Assert.IsFalse(c.IsDoorOpen(3));
        }

        [Test]
        public void ResetRoomReopensItsFinish_AndResetClearsEverything()
        {
            var c = new CampaignState();
            c.FinishRoom(1);
            c.TryInstall("apron", "apron_hook");
            c.ResetRoom(1);
            Assert.IsFalse(c.IsRoomFinished(1));
            Assert.IsFalse(c.IsDoorOpen(2));
            Assert.IsTrue(c.IsInstalled("apron"), "resetting a room does not take finds back");
            c.FinishRoom(2);
            c.ForceDoor(3, false);
            c.Reset();
            Assert.AreEqual(0, c.InstalledCount);
            Assert.AreEqual(0, c.FoundCount);
            Assert.IsFalse(c.IsRoomFinished(2));
            Assert.IsTrue(c.IsDoorOpen(1));
            Assert.IsFalse(c.IsDoorOpen(3));
        }

        [Test]
        public void ForceInstall_InstallsWithoutASpot()
        {
            var c = new CampaignState();
            Assert.IsTrue(c.ForceInstall("glasses"));
            Assert.IsTrue(c.IsInstalled("glasses") && c.IsFound("glasses"));
            Assert.IsFalse(c.ForceInstall("nonsense"));
        }
    }

    public class UpgradeGrantTests
    {
        [Test]
        public void Grant_UnlocksForFree_IgnoringCostAndPrerequisite_ButOnlyOnce()
        {
            var u = new UpgradeState();
            Assert.IsTrue(u.Grant("arms2"));
            Assert.AreEqual(5, u.CarryCapacity);
            Assert.AreEqual(0, u.Points);
            Assert.IsFalse(u.Grant("arms2"));
            Assert.IsFalse(u.Grant("nonsense"));
        }
    }
}
