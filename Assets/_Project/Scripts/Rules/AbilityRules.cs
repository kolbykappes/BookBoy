using System;

namespace BookBoy.Rules
{
    /// <summary>The four timed abilities. Each has its own cooldown timer once it is unlocked.</summary>
    public enum AbilityId
    {
        SisterGlow,
        QuickShelve,
        GatherFamily,
        HomeBeacon
    }

    /// <summary>One cooldown timer per ability. Times are passed in (seconds on any clock), so this stays engine-free.
    /// An ability is ready from the moment it is unlocked and goes on cooldown only when it actually does something.</summary>
    public sealed class AbilityTimers
    {
        public const float BaseCooldownSeconds = 60f;
        public const float MinCooldownSeconds = 5f;

        private readonly double[] _readyAt = new double[Enum.GetValues(typeof(AbilityId)).Length];

        public void Reset()
        {
            Array.Clear(_readyAt, 0, _readyAt.Length);
        }

        /// <summary>Seconds until the ability can be used again (0 = ready). Never more than the current cooldown, so buying a faster
        /// recharge mid-cooldown shortens the wait straight away.</summary>
        public double Remaining(AbilityId id, double now, double cooldown)
        {
            double left = _readyAt[(int)id] - now;
            if (left < 0)
            {
                return 0;
            }

            return left > cooldown ? cooldown : left;
        }

        public bool IsReady(AbilityId id, double now, double cooldown)
        {
            return Remaining(id, now, cooldown) <= 0;
        }

        public void Start(AbilityId id, double now, double cooldown)
        {
            _readyAt[(int)id] = now + cooldown;
        }
    }

    public static class GatherRules
    {
        /// <summary>How many books Gather Family pulls: what the upgrade allows, limited by free hand space and by how many loose
        /// sister books there are.</summary>
        public static int PullCount(int allowed, int freeHandSlots, int availableSisters)
        {
            return Math.Max(0, Math.Min(allowed, Math.Min(freeHandSlots, availableSisters)));
        }
    }
}
