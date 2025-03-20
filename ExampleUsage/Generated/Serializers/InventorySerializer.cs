using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;

using YoloSerializer.Core.Models;

using System;

using System.Collections.Generic;

using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for Inventory objects
    /// </summary>
    public sealed class InventorySerializer : ISerializer<YoloSerializer.Core.Models.Inventory?>
    {
        private static readonly InventorySerializer _instance = new InventorySerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static InventorySerializer Instance => _instance;
        
        private InventorySerializer() { }


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<Inventory> _inventoryPool = 
            new ObjectPool<Inventory>(() => new Inventory());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the Inventory
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Core.Models.Inventory? inventory)
        {

            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            
            int size = 0;
            size += Int32Serializer.Instance.GetSize(inventory.Slots);
                        size += (inventory.Items == null ? sizeof(int) : Int32Serializer.Instance.GetSize(inventory.Items.Count) + inventory.Items.Sum(listItem => StringSerializer.Instance.GetSize(listItem)));
                        size += (inventory.ItemCounts == null ? sizeof(int) : Int32Serializer.Instance.GetSize(inventory.ItemCounts.Count) + inventory.ItemCounts.Sum(kvp => StringSerializer.Instance.GetSize(kvp.Key) + Int32Serializer.Instance.GetSize(kvp.Value)));
                        size += (inventory.Rarities == null ? sizeof(int) : Int32Serializer.Instance.GetSize(inventory.Rarities.Length) + inventory.Rarities.Sum(arrayItem => EnumSerializer<ItemRarity>.Instance.GetSize(arrayItem)));

            return size;
        }

        /// <summary>
        /// Serializes a Inventory object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Core.Models.Inventory? inventory, Span<byte> buffer, ref int offset)
        {

            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            Int32Serializer.Instance.Serialize(inventory.Slots, buffer, ref offset);
                        if (inventory.Items == null) {
                            Int32Serializer.Instance.Serialize(0, buffer, ref offset);
                        } else {
                            Int32Serializer.Instance.Serialize(inventory.Items.Count, buffer, ref offset);
                            foreach (var listItem in inventory.Items)
                            {
                                StringSerializer.Instance.Serialize(listItem, buffer, ref offset);
                            }
                        }
                        if (inventory.ItemCounts == null) {
                            Int32Serializer.Instance.Serialize(0, buffer, ref offset);
                        } else {
                            Int32Serializer.Instance.Serialize(inventory.ItemCounts.Count, buffer, ref offset);
                            foreach (var kvp in inventory.ItemCounts)
                            {
                                StringSerializer.Instance.Serialize(kvp.Key, buffer, ref offset);
                                Int32Serializer.Instance.Serialize(kvp.Value, buffer, ref offset);
                            }
                        }
                        if (inventory.Rarities == null) {
                            Int32Serializer.Instance.Serialize(0, buffer, ref offset);
                        } else {
                            Int32Serializer.Instance.Serialize(inventory.Rarities.Length, buffer, ref offset);
                            foreach (var arrayItem in inventory.Rarities)
                            {
                                EnumSerializer<ItemRarity>.Instance.Serialize(arrayItem, buffer, ref offset);
                            }
                        }
        }

        /// <summary>
        /// Deserializes a Inventory object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Core.Models.Inventory? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a Inventory instance from pool
            var inventory = _inventoryPool.Get();


            Int32Serializer.Instance.Deserialize(out int _local_slots, buffer, ref offset);
                        inventory.Slots = _local_slots;
                        Int32Serializer.Instance.Deserialize(out int _local_itemsCount, buffer, ref offset);
                        if (inventory.Items == null)
                            inventory.Items = new List<System.String>();
                        else
                            inventory.Items.Clear();
                        for (int i = 0; i < _local_itemsCount; i++)
                        {
                            StringSerializer.Instance.Deserialize(out System.String listItem, buffer, ref offset);
                            inventory.Items.Add(listItem);
                        }
                        Int32Serializer.Instance.Deserialize(out int _local_itemCountsCount, buffer, ref offset);
                        if (inventory.ItemCounts == null)
                            inventory.ItemCounts = new Dictionary<System.String, System.Int32>();
                        else
                            inventory.ItemCounts.Clear();
                        for (int i = 0; i < _local_itemCountsCount; i++)
                        {
                            StringSerializer.Instance.Deserialize(out System.String key, buffer, ref offset);
                            Int32Serializer.Instance.Deserialize(out System.Int32 dictValue, buffer, ref offset);
                            inventory.ItemCounts[key] = dictValue;
                        }
                        Int32Serializer.Instance.Deserialize(out int _local_raritiesLength, buffer, ref offset);
                        inventory.Rarities = new ItemRarity[_local_raritiesLength];
                        for (int i = 0; i < _local_raritiesLength; i++)
                        {
                            EnumSerializer<ItemRarity>.Instance.Deserialize(out ItemRarity arrayItem, buffer, ref offset);
                            inventory.Rarities[i] = arrayItem;
                        }

            value = inventory;
        }
    }
}