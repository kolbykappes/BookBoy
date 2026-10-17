using System.Collections.Generic;

namespace BookBoy.Rules
{
    public enum UpgradeEffect
    {
        CarryCapacity,
        MoveSpeed,
        QuickShelve,
        SisterGlow,
        GlowDuration
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

    /// <summary>The tech tree. Sized for the 8 shelves in the room today (8 points), so the total cost is higher than the
    /// points available and the player has to choose.</summary>
    public static class UpgradeCatalog
    {
        public const int BaseCarryCapacity = 4;

        public static readonly IReadOnlyList<UpgradeNode> All = new List<UpgradeNode>
        {
            new UpgradeNode("arms1", "Strong Arms I", "Carry 1 more book (5).", 1, null, UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("arms2", "Strong Arms II", "Carry 1 more book (6).", 1, "arms1", UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("arms3", "Strong Arms III", "Carry 1 more book (7).", 2, "arms2", UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("arms4", "Strong Arms IV", "Carry 1 more book (8).", 2, "arms3", UpgradeEffect.CarryCapacity, 1),
            new UpgradeNode("feet1", "Fleet Feet I", "Move 15% faster.", 1, null, UpgradeEffect.MoveSpeed, 0.15f),
            new UpgradeNode("feet2", "Fleet Feet II", "Move another 15% faster.", 2, "feet1", UpgradeEffect.MoveSpeed, 0.15f),
            new UpgradeNode("glow", "Sister Glow", "Press G: books of the held book's series that are not on a shelf glow.", 1, null, UpgradeEffect.SisterGlow, 1),
            new UpgradeNode("glow2", "Lingering Glow", "Sister Glow lasts twice as long.", 1, "glow", UpgradeEffect.GlowDuration, 1),
            new UpgradeNode("shelve", "Quick Shelve", "Press F: every held book flies to its right place on a shelf.", 2, null, UpgradeEffect.QuickShelve, 1),
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
        public float GlowSeconds => Has(UpgradeEffect.GlowDuration) ? 8f : 4f;
    }
}
