using BookBoy.Rules;
using NUnit.Framework;

namespace BookBoy.Tests
{
    public class SprintFovRulesTests
    {
        [Test]
        public void NoKick_AtOrBelowWalkingSpeed_OrWhenStandingStill()
        {
            Assert.AreEqual(0f, SprintFovRules.Kick(0f, 4f, 6f, 6f, 1f), 1e-5f);
            Assert.AreEqual(0f, SprintFovRules.Kick(4f, 4f, 6f, 6f, 1f), 1e-5f);
            Assert.AreEqual(0f, SprintFovRules.Kick(2f, 4f, 6f, 6f, 1f), 1e-5f);
        }

        [Test]
        public void FullKick_AtOrAboveSprintSpeed()
        {
            Assert.AreEqual(6f, SprintFovRules.Kick(6f, 4f, 6f, 6f, 1f), 1e-4f);
            Assert.AreEqual(6f, SprintFovRules.Kick(9f, 4f, 6f, 6f, 1f), 1e-4f);
        }

        [Test]
        public void EasesBetween_AndNeverFallsWhenSpeedRises()
        {
            float last = -1f;
            for (float speed = 4f; speed <= 6f; speed += 0.1f)
            {
                float k = SprintFovRules.Kick(speed, 4f, 6f, 6f, 1f);
                Assert.GreaterOrEqual(k, last - 1e-5f);
                last = k;
            }

            Assert.AreEqual(3f, SprintFovRules.Kick(5f, 4f, 6f, 6f, 1f), 1e-4f, "the middle of the range is half the kick");
        }

        [Test]
        public void StrengthScalesIt_AndZeroTurnsItOff_AndBadRangesGiveNothing()
        {
            Assert.AreEqual(3f, SprintFovRules.Kick(6f, 4f, 6f, 6f, 0.5f), 1e-4f);
            Assert.AreEqual(0f, SprintFovRules.Kick(6f, 4f, 6f, 6f, 0f), 1e-5f);
            Assert.AreEqual(0f, SprintFovRules.Kick(6f, 6f, 6f, 6f, 1f), 1e-5f);
            Assert.AreEqual(0f, SprintFovRules.Kick(6f, 4f, 6f, 0f, 1f), 1e-5f);
        }

        [Test]
        public void ZoomedFov_NarrowsByTheMagnification_AndNeverWidens()
        {
            Assert.AreEqual(58.7f, SprintFovRules.ZoomedFov(58.7f, 1f), 1e-4f);
            Assert.AreEqual(58.7f, SprintFovRules.ZoomedFov(58.7f, 0.5f), 1e-4f, "a zoom below 1 is no zoom");
            float twice = SprintFovRules.ZoomedFov(58.7f, 2f);
            Assert.Less(twice, 58.7f);
            Assert.AreEqual(0.5f, (float)System.Math.Tan(twice * 0.5f * System.Math.PI / 180.0) / (float)System.Math.Tan(58.7f * 0.5f * System.Math.PI / 180.0), 1e-3f, "2x shows half the width");
            Assert.Less(SprintFovRules.ZoomedFov(58.7f, 2.5f), twice);
        }
    }
}
