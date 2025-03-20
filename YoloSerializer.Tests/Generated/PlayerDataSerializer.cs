using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for PlayerData objects
    /// </summary>
    public sealed class PlayerDataSerializer : ISerializer<PlayerData?>
    {
        private static readonly PlayerDataSerializer _instance = new PlayerDataSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static PlayerDataSerializer Instance => _instance;
        
        private PlayerDataSerializer() { }
        
        // Maximum size to allocate on stack
        private const int MaxStackAllocSize = 1024;


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<PlayerData> _Pool = 
            new ObjectPool<PlayerData>(() => new PlayerData());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the PlayerData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(PlayerData? playerData)
        {

            if (playerData == null)
                throw new ArgumentNullException(nameof(playerData));

            
            int size = 0;
            size += Int32Serializer.Instance.GetSize(playerData.PlayerId);
                        size += StringSerializer.Instance.GetSize(playerData.PlayerName);
                        size += Int32Serializer.Instance.GetSize(playerData.Health);
                        size += PositionSerializer.Instance.GetSize(playerData.Position);
                        size += BooleanSerializer.Instance.GetSize(playerData.IsActive);
                        size += Int32Serializer.Instance.GetSize(playerData.Achievements.Count) + playerData.Achievements.Sum(listItem => StringSerializer.Instance.GetSize(listItem));
                        size += Int32Serializer.Instance.GetSize(playerData.Stats.Count) + playerData.Stats.Sum(kvp => StringSerializer.Instance.GetSize(kvp.Key) + Int32Serializer.Instance.GetSize(kvp.Value));

            return size;
        }

        /// <summary>
        /// Serializes a PlayerData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(PlayerData? playerData, Span<byte> buffer, ref int offset)
        {

            if (playerData == null)
                throw new ArgumentNullException(nameof(playerData));

            Int32Serializer.Instance.Serialize(playerData.PlayerId, buffer, ref offset);
                        StringSerializer.Instance.Serialize(playerData.PlayerName, buffer, ref offset);
                        Int32Serializer.Instance.Serialize(playerData.Health, buffer, ref offset);
                        PositionSerializer.Instance.Serialize(playerData.Position, buffer, ref offset);
                        BooleanSerializer.Instance.Serialize(playerData.IsActive, buffer, ref offset);
                        Int32Serializer.Instance.Serialize(playerData.Achievements.Count, buffer, ref offset);
                        foreach (var listItem in playerData.Achievements)
                        {
                            StringSerializer.Instance.Serialize(listItem, buffer, ref offset);
                        }
                        Int32Serializer.Instance.Serialize(playerData.Stats.Count, buffer, ref offset);
                        foreach (var kvp in playerData.Stats)
                        {
                            StringSerializer.Instance.Serialize(kvp.Key, buffer, ref offset);
                            Int32Serializer.Instance.Serialize(kvp.Value, buffer, ref offset);
                        }
        }

        /// <summary>
        /// Deserializes a PlayerData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out PlayerData? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a PlayerData instance from pool
            var playerData = _Pool.Get();


            Int32Serializer.Instance.Deserialize(out int _local_playerId, buffer, ref offset);
                        playerData.PlayerId = _local_playerId;
                        StringSerializer.Instance.Deserialize(out string _local_playerName, buffer, ref offset);
                        playerData.PlayerName = _local_playerName;
                        Int32Serializer.Instance.Deserialize(out int _local_health, buffer, ref offset);
                        playerData.Health = _local_health;
                        PositionSerializer.Instance.Deserialize(out Position? _local_position, buffer, ref offset);
                        playerData.Position = _local_position;
                        BooleanSerializer.Instance.Deserialize(out bool _local_isActive, buffer, ref offset);
                        playerData.IsActive = _local_isActive;
                        Int32Serializer.Instance.Deserialize(out int _local_achievementsCount, buffer, ref offset);
                        playerData.Achievements.Clear();
                        for (int i = 0; i < _local_achievementsCount; i++)
                        {
                            StringSerializer.Instance.Deserialize(out String listItem, buffer, ref offset);
                            playerData.Achievements.Add(listItem);
                        }
                        Int32Serializer.Instance.Deserialize(out int _local_statsCount, buffer, ref offset);
                        playerData.Stats.Clear();
                        for (int i = 0; i < _local_statsCount; i++)
                        {
                            StringSerializer.Instance.Deserialize(out String key, buffer, ref offset);
                            Int32Serializer.Instance.Deserialize(out Int32 dictValue, buffer, ref offset);
                            playerData.Stats[key] = dictValue;
                        }

            value = playerData;
        }
    }
}