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

using YoloSerializer.Core.ModelsYolo;

using System.Collections.Generic;

using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for PlayerData objects
    /// </summary>
    public sealed class PlayerDataSerializer : ISerializer<YoloSerializer.Core.Models.PlayerData?>
    {
        private static readonly PlayerDataSerializer _instance = new PlayerDataSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static PlayerDataSerializer Instance => _instance;
        
        private PlayerDataSerializer() { }


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<PlayerData> _playerDataPool = 
            new ObjectPool<PlayerData>(() => new PlayerData());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        // Number of nullable fields in this type
        private const int NullableFieldCount = 4;
        
        // Size of the nullability bitset in bytes
        private const int NullableBitsetSize = 1;
        
        // Create stack allocated bitset array for nullability tracking
        private Span<byte> GetBitsetArray(Span<byte> tempBuffer) => 
            NullableBitsetSize > 0 ? tempBuffer.Slice(0, NullableBitsetSize) : default;

        /// <summary>
        /// Gets the total size needed to serialize the PlayerData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Core.Models.PlayerData? playerData)
        {

            if (playerData == null)
                throw new ArgumentNullException(nameof(playerData));

            
            int size = 0;
            
            // Add size for nullability bitset if needed
            size += NullableBitsetSize;
            
            size += Int32Serializer.Instance.GetSize(playerData.PlayerId);
                        size += playerData.PlayerName == null ? 0 : StringSerializer.Instance.GetSize(playerData.PlayerName);
                        size += Int32Serializer.Instance.GetSize(playerData.Health);
                        size += playerData.Position == null ? 0 : PositionSerializer.Instance.GetSize(playerData.Position);
                        size += BooleanSerializer.Instance.GetSize(playerData.IsActive);
                        size += playerData.Achievements == null ? 0 : (playerData.Achievements == null ? sizeof(int) : Int32Serializer.Instance.GetSize(playerData.Achievements.Count) + playerData.Achievements.Sum(listItem => StringSerializer.Instance.GetSize(listItem)));
                        size += playerData.Stats == null ? 0 : (playerData.Stats == null ? sizeof(int) : Int32Serializer.Instance.GetSize(playerData.Stats.Count) + playerData.Stats.Sum(kvp => StringSerializer.Instance.GetSize(kvp.Key) + Int32Serializer.Instance.GetSize(kvp.Value)));

            return size;
        }

        /// <summary>
        /// Serializes a PlayerData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Core.Models.PlayerData? playerData, Span<byte> buffer, ref int offset)
        {

            if (playerData == null)
                throw new ArgumentNullException(nameof(playerData));


            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Initialize all bits to 0 (non-null)
            for (int i = 0; i < bitset.Length; i++)
                bitset[i] = 0;
                
            // First determine the nullability of each field
                        NullableBitset.SetBit(bitset, 1, playerData.PlayerName == null);
                        NullableBitset.SetBit(bitset, 2, playerData.Position == null);
                        NullableBitset.SetBit(bitset, 3, playerData.Achievements == null);
                        NullableBitset.SetBit(bitset, 4, playerData.Stats == null);

            
            // Write the nullability bitset to the buffer
            NullableBitset.SerializeBitset(bitset, NullableBitsetSize, buffer, ref offset);
            
            // Write the actual field values
            Int32Serializer.Instance.Serialize(playerData.PlayerId, buffer, ref offset);
                        if (!NullableBitset.IsNull(bitset, 1))
                            StringSerializer.Instance.Serialize(playerData.PlayerName, buffer, ref offset);
                        Int32Serializer.Instance.Serialize(playerData.Health, buffer, ref offset);
                        if (!NullableBitset.IsNull(bitset, 2))
                            PositionSerializer.Instance.Serialize(playerData.Position, buffer, ref offset);
                        BooleanSerializer.Instance.Serialize(playerData.IsActive, buffer, ref offset);
                        if (!NullableBitset.IsNull(bitset, 3))
                        {
                            if (playerData.Achievements == null) {
                            Int32Serializer.Instance.Serialize(0, buffer, ref offset);
                        } else {
                            Int32Serializer.Instance.Serialize(playerData.Achievements.Count, buffer, ref offset);
                            foreach (var listItem in playerData.Achievements)
                            {
                                StringSerializer.Instance.Serialize(listItem, buffer, ref offset);
                            }
                        }
                        }
                        if (!NullableBitset.IsNull(bitset, 4))
                        {
                            if (playerData.Stats == null) {
                            Int32Serializer.Instance.Serialize(0, buffer, ref offset);
                        } else {
                            Int32Serializer.Instance.Serialize(playerData.Stats.Count, buffer, ref offset);
                            foreach (var kvp in playerData.Stats)
                            {
                                StringSerializer.Instance.Serialize(kvp.Key, buffer, ref offset);
                                Int32Serializer.Instance.Serialize(kvp.Value, buffer, ref offset);
                            }
                        }
                        }
        }

        /// <summary>
        /// Deserializes a PlayerData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Core.Models.PlayerData? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a PlayerData instance from pool
            var playerData = _playerDataPool.Get();


            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Read the nullability bitset from the buffer
            NullableBitset.DeserializeBitset(buffer, ref offset, bitset, NullableBitsetSize);

            Int32Serializer.Instance.Deserialize(out int _local_playerId, buffer, ref offset);
                        playerData.PlayerId = _local_playerId;
                        if (NullableBitset.IsNull(bitset, 1))
                            playerData.PlayerName = null;
                        else
                        {
                            StringSerializer.Instance.Deserialize(out string _local_playerName, buffer, ref offset);
                            playerData.PlayerName = _local_playerName;
                        }
                        Int32Serializer.Instance.Deserialize(out int _local_health, buffer, ref offset);
                        playerData.Health = _local_health;
                        if (NullableBitset.IsNull(bitset, 2))
                            playerData.Position = null;
                        else
                        {
                            PositionSerializer.Instance.Deserialize(out YoloSerializer.Core.ModelsYolo.Position? _local_position, buffer, ref offset);
                            playerData.Position = _local_position;
                        }
                        BooleanSerializer.Instance.Deserialize(out bool _local_isActive, buffer, ref offset);
                        playerData.IsActive = _local_isActive;
                        if (NullableBitset.IsNull(bitset, 3))
                            playerData.Achievements = null;
                        else
                        {
                            Int32Serializer.Instance.Deserialize(out int _local_achievementsCount, buffer, ref offset);
                        if (playerData.Achievements == null)
                            playerData.Achievements = new List<System.String>();
                        else
                            playerData.Achievements.Clear();
                        for (int i = 0; i < _local_achievementsCount; i++)
                        {
                            StringSerializer.Instance.Deserialize(out System.String listItem, buffer, ref offset);
                            playerData.Achievements.Add(listItem);
                        }
                        }
                        if (NullableBitset.IsNull(bitset, 4))
                            playerData.Stats = null;
                        else
                        {
                            Int32Serializer.Instance.Deserialize(out int _local_statsCount, buffer, ref offset);
                        if (playerData.Stats == null)
                            playerData.Stats = new Dictionary<System.String, System.Int32>();
                        else
                            playerData.Stats.Clear();
                        for (int i = 0; i < _local_statsCount; i++)
                        {
                            StringSerializer.Instance.Deserialize(out System.String key, buffer, ref offset);
                            Int32Serializer.Instance.Deserialize(out System.Int32 dictValue, buffer, ref offset);
                            playerData.Stats[key] = dictValue;
                        }
                        }

            value = playerData;
        }
    }
}