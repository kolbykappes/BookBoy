using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class AbilityTimersTests
    {
        [Test]
        public void EveryAbilityIsReadyAtFirst()
        {
            var t = new AbilityTimers();
            foreach (AbilityId id in System.Enum.GetValues(typeof(AbilityId)))
            {
                Assert.IsTrue(t.IsReady(id, 0, 60), id.ToString());
                Assert.AreEqual(0, t.Remaining(id, 0, 60), id.ToString());
            }
        }

        [Test]
        public void StartingOneAbilityOnlyAffectsThatAbility()
        {
            var t = new AbilityTimers();
            t.Start(AbilityId.QuickShelve, 100, 60);
            Assert.AreEqual(60, t.Remaining(AbilityId.QuickShelve, 100, 60), 1e-9);
            Assert.AreEqual(25, t.Remaining(AbilityId.QuickShelve, 135, 60), 1e-9);
            Assert.IsFalse(t.IsReady(AbilityId.QuickShelve, 159.9, 60));
            Assert.IsTrue(t.IsReady(AbilityId.QuickShelve, 160, 60));
            Assert.IsTrue(t.IsReady(AbilityId.SisterGlow, 100, 60), "the others keep their own timers");
            Assert.IsTrue(t.IsReady(AbilityId.GatherFamily, 100, 60));
            Assert.IsTrue(t.IsReady(AbilityId.HomeBeacon, 100, 60));
        }

        [Test]
        public void BuyingAFasterRechargeShortensTheCurrentWait()
        {
            var t = new AbilityTimers();
            t.Start(AbilityId.SisterGlow, 0, 60);
            Assert.AreEqual(50, t.Remaining(AbilityId.SisterGlow, 10, 60), 1e-9);
            Assert.AreEqual(20, t.Remaining(AbilityId.SisterGlow, 10, 20), 1e-9, "a 20 s recharge can never leave more than 20 s");
        }

        [Test]
        public void ResetMakesEverythingReady()
        {
            var t = new AbilityTimers();
            t.Start(AbilityId.HomeBeacon, 0, 60);
            t.Reset();
            Assert.IsTrue(t.IsReady(AbilityId.HomeBeacon, 0, 60));
        }
    }

    public class GatherRulesTests
    {
        [TestCase(2, 6, 3, 2)]    // plenty of room and sisters: the upgrade's amount
        [TestCase(2, 1, 3, 1)]    // only one free hand slot
        [TestCase(4, 6, 3, 3)]    // only three loose sisters exist
        [TestCase(4, 0, 3, 0)]    // hands full
        [TestCase(3, 2, 0, 0)]    // nothing to gather
        [TestCase(2, -1, 3, 0)]   // never negative
        public void PullCount_IsTheSmallestOfAllowedFreeAndAvailable(int allowed, int free, int available, int expected)
        {
            Assert.AreEqual(expected, GatherRules.PullCount(allowed, free, available));
        }
    }

    public class AbilityUpgradeTests
    {
        [Test]
        public void NoAbilityIsUnlockedAtFirst_AndEachStartsAtSixtySeconds()
        {
            var u = new UpgradeState();
            foreach (AbilityId id in System.Enum.GetValues(typeof(AbilityId)))
            {
                Assert.IsFalse(u.HasAbility(id), id + " must start locked");
                Assert.AreEqual(60f, u.CooldownSeconds(id), id.ToString());
            }
        }

        [Test]
        public void EachAbilityHasItsOwnRechargeUpgrades_60_40_20()
        {
            var u = new UpgradeState();
            u.AddPoints(100);
            var tiers = new[]
            {
                (AbilityId.SisterGlow, "glow", "glowcd1", "glowcd2"),
                (AbilityId.QuickShelve, "shelve", "shelvecd1", "shelvecd2"),
                (AbilityId.GatherFamily, "gather", "gathercd1", "gathercd2"),
                (AbilityId.HomeBeacon, "beacon", "beaconcd1", "beaconcd2"),
            };
            foreach (var (id, unlock, cd1, cd2) in tiers)
            {
                Assert.IsFalse(u.TryUnlock(cd1), cd1 + " needs the ability first");
                Assert.IsTrue(u.TryUnlock(unlock));
                Assert.IsTrue(u.HasAbility(id));
                Assert.AreEqual(60f, u.CooldownSeconds(id));
                Assert.IsTrue(u.TryUnlock(cd1));
                Assert.AreEqual(40f, u.CooldownSeconds(id));
                Assert.IsTrue(u.TryUnlock(cd2));
                Assert.AreEqual(20f, u.CooldownSeconds(id));
            }

            // upgrading one ability's timer does not touch the others'
            var v = new UpgradeState();
            v.AddPoints(10);
            v.TryUnlock("glow");
            v.TryUnlock("glowcd1");
            Assert.AreEqual(40f, v.CooldownSeconds(AbilityId.SisterGlow));
            Assert.AreEqual(60f, v.CooldownSeconds(AbilityId.QuickShelve));
            Assert.AreEqual(60f, v.CooldownSeconds(AbilityId.GatherFamily));
            Assert.AreEqual(60f, v.CooldownSeconds(AbilityId.HomeBeacon));
        }

        [Test]
        public void GatherPullsTwoThenThreeThenTheWholeSeries()
        {
            var u = new UpgradeState();
            u.AddPoints(20);
            Assert.AreEqual(2, u.GatherCount);
            Assert.IsFalse(u.TryUnlock("gather2"), "needs Gather Family first");
            Assert.IsTrue(u.TryUnlock("gather"));
            Assert.AreEqual(2, u.GatherCount);
            Assert.IsFalse(u.TryUnlock("gather3"), "needs Bigger Pull first");
            Assert.IsTrue(u.TryUnlock("gather2"));
            Assert.AreEqual(3, u.GatherCount);
            Assert.IsTrue(u.TryUnlock("gather3"));
            Assert.AreEqual(4, u.GatherCount);
        }

        [Test]
        public void HomeBeaconNeedsItsUpgrade()
        {
            var u = new UpgradeState();
            u.AddPoints(5);
            Assert.IsFalse(u.HasHomeBeacon);
            Assert.IsTrue(u.TryUnlock("beacon"));
            Assert.IsTrue(u.HasHomeBeacon);
        }

        [Test]
        public void TheWholeTreeCostsExactlyThirtyTwoPoints_OnePerFinishedShelf()
        {
            int total = 0;
            foreach (UpgradeNode n in UpgradeCatalog.All)
            {
                total += n.Cost;
            }

            Assert.AreEqual(32, total);
        }

        [Test]
        public void RechargeNeverDropsBelowFiveSeconds()
        {
            Assert.GreaterOrEqual(AbilityTimers.MinCooldownSeconds, 5f);
            var u = new UpgradeState();
            u.UnlockAll();
            foreach (AbilityId id in System.Enum.GetValues(typeof(AbilityId)))
            {
                Assert.GreaterOrEqual(u.CooldownSeconds(id), AbilityTimers.MinCooldownSeconds);
            }
        }
    }
}
