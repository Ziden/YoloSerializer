using System;
using System.Collections.Generic;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Models
{
    /// <summary>
    /// Represents a player's inventory in the game
    /// </summary>
    [Serializable]
    public class Inventory
    {
        public int Slots { get; set; }
        public List<string>? Items { get; set; } = new List<string>();
        public Dictionary<string, int>? ItemCounts { get; set; } = new Dictionary<string, int>();
        public ItemRarity[]? Rarities { get; set; }

        public Inventory()
        {
            // Default constructor
        }

        public Inventory(int slots, List<string>? items = null, Dictionary<string, int>? itemCounts = null, ItemRarity[]? rarities = null)
        {
            Slots = slots;
            Items = items ?? new List<string>();
            ItemCounts = itemCounts ?? new Dictionary<string, int>();
            Rarities = rarities;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Inventory other)
            {
                // Basic properties equality check
                bool basicPropertiesEqual = Slots == other.Slots;
                
                if (!basicPropertiesEqual) return false;
                
                // Check if both Items are null
                if (Items == null && other.Items == null)
                    return true;
                
                // Check if only one is null
                if (Items == null || other.Items == null)
                    return false;
                
                // Check items list equality
                if (Items.Count != other.Items.Count) return false;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] != other.Items[i]) return false;
                }
                
                // Check if both ItemCounts are null
                if (ItemCounts == null && other.ItemCounts == null)
                    return true;
                
                // Check if only one is null
                if (ItemCounts == null || other.ItemCounts == null)
                    return false;
                
                // Check itemCounts dictionary equality
                if (ItemCounts.Count != other.ItemCounts.Count) return false;
                foreach (var key in ItemCounts.Keys)
                {
                    if (!other.ItemCounts.TryGetValue(key, out int otherValue) || 
                        ItemCounts[key] != otherValue)
                        return false;
                }

                // Check if both Rarities are null
                if (Rarities == null && other.Rarities == null)
                    return true;
                
                // Check if only one is null
                if (Rarities == null || other.Rarities == null)
                    return false;
                
                // Check rarities array equality
                if (Rarities.Length != other.Rarities.Length) return false;
                for (int i = 0; i < Rarities.Length; i++)
                {
                    if (Rarities[i] != other.Rarities[i]) return false;
                }
                
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = Slots.GetHashCode();
            
            // Hash the items in a consistent manner
            if (Items != null)
            {
                hash = hash * 23 + Items.Count.GetHashCode();
                
                foreach (var item in Items)
                {
                    hash = hash * 23 + (item?.GetHashCode() ?? 0);
                }
            }
            
            // Hash the item counts in a consistent manner
            if (ItemCounts != null)
            {
                hash = hash * 23 + ItemCounts.Count.GetHashCode();
                
                // Use sorted keys for consistent ordering
                var sortedKeys = ItemCounts.Keys.OrderBy(k => k).ToList();
                foreach (var key in sortedKeys)
                {
                    hash = hash * 23 + key.GetHashCode();
                    hash = hash * 23 + ItemCounts[key].GetHashCode();
                }
            }

            // Hash the rarities in a consistent manner
            if (Rarities != null)
            {
                hash = hash * 23 + Rarities.Length.GetHashCode();
                
                foreach (var rarity in Rarities)
                {
                    hash = hash * 23 + rarity.GetHashCode();
                }
            }
            
            return hash;
        }
    }

    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }
} 