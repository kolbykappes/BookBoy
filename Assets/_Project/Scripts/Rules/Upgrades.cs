using System.Collections.Generic;

namespace BookBoy.Rules
{
    public enum UpgradeEffect
    {
        CarryCapacity,
        MoveSpeed,
        QuickShelve,
        SisterGlow,
        GlowDuration,
        GlowCooldown,
        ShelveCooldown,
        GatherFamily,
        GatherCount,
        GatherCooldown,
        HomeBeacon,
        BeaconCooldown
    }

    public sealed class UpgradeNode
    {
        public UpgradeNode(string id, string name, string description, int cost, string requires, UpgradeEffect effect, float amount)
        {
            Id = id;
            Name = name;
            Description = description;
            Cost = cost;
            Requires = requires;
            Effect = effect;
            Amount = amount;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int Cost { get; }

        /// <summary>Id of the node that must be unlocked first, or null.</summary>
        public string Requires { get; }

        public UpgradeEffect Effect { get; }
        public float Amount { get; }
    }

    /// <summary>The tech tree. One point is earned per finished shelf. With 8 shelves the player must choose; with the full 32
    /// shelves (32 points) everything can be bought (the tree costs exactly 32).</summary>
    public static class UpgradeCatalog
    {
        public const int BaseCarryCapacity = 4;

        public static readonly IReadOnlyList<UpgradeNode> All = new List<UpgradeNode>
        {
            new UpgradeNode("arms1", "Strong Arms I", "Carry 1 more book (5).", 1, null, UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("arms2", "Strong Arms II", "Carry 1 more book (6).", 1, "arms1", UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("arms3", "Strong Arms III", "Carry 1 more book (7).", 2, "arms2", UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("arms4", "Strong Arms IV", "Carry 1 more book (8).", 2, "arms3", UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("arms5", "Strong Arms V", "Carry 1 more book (9).", 2, "arms4", UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("arms6", "Strong Arms VI", "Carry 1 more book (10).", 2, "arms5", UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("feet1", "Fleet Feet I", "Move 15% faster.", 1, null, UpgradeEffect.MoveSpeed, 0.15f),
            new UpgradeNode("feet2", "Fleet Feet II", "Move another 15% faster.", 2, "feet1", UpgradeEffect.MoveSpeed, 0.15f),
            new UpgradeNode("feet3", "Fleet Feet III", "Move yet another 15% faster.", 2, "feet2", UpgradeEffect.MoveSpeed, 0.15f),
            new UpgradeNode("glow", "Sister Glow", "[G] Unshelved books of the held series glow. 60 s recharge.", 1, null, UpgradeEffect.SisterGlow, 1),
            new UpgradeNode("glow2", "Lingering Glow", "Sister Glow lasts twice as long.", 1, "glow", UpgradeEffect.GlowDuration, 1),
            new UpgradeNode("glowcd1", "Glow Recharge I", "Sister Glow recharges in 40 s.", 1, "glow", UpgradeEffect.GlowCooldown, 20),
            new UpgradeNode("glowcd2", "Glow Recharge II", "Sister Glow recharges in 20 s.", 1, "glowcd1", UpgradeEffect.GlowCooldown, 20),
            new UpgradeNode("shelve", "Quick Shelve", "[F] Every held book flies to its place. 60 s recharge.", 2, null, UpgradeEffect.QuickShelve, 1),
            new UpgradeNode("shelvecd1", "Shelve Recharge I", "Quick Shelve recharges in 40 s.", 1, "shelve", UpgradeEffect.ShelveCooldown, 20),
            new UpgradeNode("shelvecd2", "Shelve Recharge II", "Quick Shelve recharges in 20 s.", 1, "shelvecd1", UpgradeEffect.ShelveCooldown, 20),
            new UpgradeNode("gather", "Gather Family", "[C] Pulls 2 loose books of the held series to you. 60 s recharge.", 1, null, UpgradeEffect.GatherFamily, 1),
            new UpgradeNode("gather2", "Bigger Pull", "Gather Family pulls 3 books.", 1, "gather", UpgradeEffect.GatherCount, 1),
            new UpgradeNode("gather3", "Whole Family", "Gather Family pulls the whole series (4).", 2, "gather2", UpgradeEffect.GatherCount, 1),
            new UpgradeNode("gathercd1", "Gather Recharge I", "Gather Family recharges in 40 s.", 1, "gather", UpgradeEffect.GatherCooldown, 20),
            new UpgradeNode("gathercd2", "Gather Recharge II", "Gather Family recharges in 20 s.", 1, "gathercd1", UpgradeEffect.GatherCooldown, 20),
            new UpgradeNode("beacon", "Home Beacon", "[B] The held book's bookcase glows. 60 s recharge.", 1, null, UpgradeEffect.HomeBeacon, 1),
            new UpgradeNode("beaconcd1", "Beacon Recharge I", "Home Beacon recharges in 40 s.", 1, "beacon", UpgradeEffect.BeaconCooldown, 20),
            new UpgradeNode("beaconcd2", "Beacon Recharge II", "Home Beacon recharges in 20 s.", 1, "beaconcd1", UpgradeEffect.BeaconCooldown, 20),
        };

        public static UpgradeNode Find(string id)
        {
            foreach (UpgradeNode node in All)
            {
                if (node.Id == id)
                {
                    return node;
                }
            }

            return null;
        }
    }

    /// <summary>Points earned and upgrades bought. One point is earned per finished shelf.</summary>
    public sealed class UpgradeState
    {
        private readonly HashSet<string> _unlocked = new HashSet<string>();

        public int Points { get; private set; }
        public int TotalEarned { get; private set; }

        public bool IsUnlocked(string id)
        {
            return id != null && _unlocked.Contains(id);
        }

        public void AddPoints(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Points += amount;
            TotalEarned += amount;
        }

        public bool CanUnlock(UpgradeNode node)
        {
            return node != null && !IsUnlocked(node.Id) && Points >= node.Cost && (node.Requires == null || IsUnlocked(node.Requires));
        }

        public bool TryUnlock(string id)
        {
            UpgradeNode node = UpgradeCatalog.Find(id);
            if (!CanUnlock(node))
            {
                return false;
            }

            Points -= node.Cost;
            _unlocked.Add(node.Id);
            return true;
        }

        /// <summary>Unlocks everything for free (admin/testing). Points are left alone.</summary>
        public void UnlockAll()
        {
            foreach (UpgradeNode node in UpgradeCatalog.All)
            {
                _unlocked.Add(node.Id);
            }
        }

        public void Reset()
        {
            _unlocked.Clear();
            Points = 0;
            TotalEarned = 0;
        }

        public bool Has(UpgradeEffect effect)
        {
            foreach (UpgradeNode node in UpgradeCatalog.All)
            {
                if (node.Effect == effect && _unlocked.Contains(node.Id))
                {
                    return true;
                }
            }

            return false;
        }

        private float SumOf(UpgradeEffect effect)
        {
            float total = 0f;
            foreach (UpgradeNode node in UpgradeCatalog.All)
            {
                if (node.Effect == effect && _unlocked.Contains(node.Id))
                {
                    total += node.Amount;
                }
            }

            return total;
        }

        public int CarryCapacity => UpgradeCatalog.BaseCarryCapacity + (int)SumOf(UpgradeEffect.CarryCapacity);
        public float MoveSpeedMultiplier => 1f + SumOf(UpgradeEffect.MoveSpeed);
        public bool HasQuickShelve => Has(UpgradeEffect.QuickShelve);
        public bool HasSisterGlow => Has(UpgradeEffect.SisterGlow);
        public bool HasGatherFamily => Has(UpgradeEffect.GatherFamily);
        public bool HasHomeBeacon => Has(UpgradeEffect.HomeBeacon);
        public float GlowSeconds => Has(UpgradeEffect.GlowDuration) ? 8f : 4f;
        public float BeaconSeconds => 8f;

        /// <summary>How many books Gather Family pulls at most: 2, then 3, then the whole series of 4.</summary>
        public int GatherCount => 2 + (int)SumOf(UpgradeEffect.GatherCount);

        public float GlowCooldownSeconds => CooldownSeconds(AbilityId.SisterGlow);

        public bool HasAbility(AbilityId id)
        {
            switch (id)
            {
                case AbilityId.SisterGlow: return HasSisterGlow;
                case AbilityId.QuickShelve: return HasQuickShelve;
                case AbilityId.GatherFamily: return HasGatherFamily;
                default: return HasHomeBeacon;
            }
        }

        /// <summary>Every ability starts at a 60 s recharge; each recharge upgrade takes 20 s off (never below 5 s).</summary>
        public float CooldownSeconds(AbilityId id)
        {
            UpgradeEffect effect;
            switch (id)
            {
                case AbilityId.SisterGlow: effect = UpgradeEffect.GlowCooldown; break;
                case AbilityId.QuickShelve: effect = UpgradeEffect.ShelveCooldown; break;
                case AbilityId.GatherFamily: effect = UpgradeEffect.GatherCooldown; break;
                default: effect = UpgradeEffect.BeaconCooldown; break;
            }

            float seconds = AbilityTimers.BaseCooldownSeconds - SumOf(effect);
            return seconds < AbilityTimers.MinCooldownSeconds ? AbilityTimers.MinCooldownSeconds : seconds;
        }
    }
}
