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
    /// Provides standard null handling for reference types
    /// </summary>
    public static class NullHandler
    {
        /// <summary>
        /// Standard null marker value for all serializers
        /// </summary>
        public const int NullMarker = -1;
        
        /// <summary>
        /// Writes a null marker if the value is null
        /// </summary>
        /// <returns>True if the value was null and a marker was written</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WriteNullIfNeeded<T>(T? value, Span<byte> buffer, ref int offset) where T : class
        {
            if (value == null)
            {
                Int32Serializer.Instance.Serialize(NullMarker, buffer, ref offset);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Reads a null marker and returns whether the value was null
        /// </summary>
        /// <returns>True if the value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadAndCheckNull(ReadOnlySpan<byte> buffer, ref int offset, out int marker)
        {
            Int32Serializer.Instance.Deserialize(out marker, buffer, ref offset);
            return marker == NullMarker;
        }
    }

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
        private static readonly ObjectPool<PlayerData> _playerPool = 
            new ObjectPool<PlayerData>(() => new PlayerData());
            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the PlayerData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(PlayerData? player)
        {
            if (player == null)
                return sizeof(int); // Just a null marker
                
            int size = 0;
            
            // Size of PlayerId (int)
            size += Int32Serializer.Instance.GetSize(player.PlayerId);
            
            // Size of PlayerName (string)
            size += StringSerializer.Instance.GetSize(player.PlayerName);
            
            // Size of Health (int)
            size += Int32Serializer.Instance.GetSize(player.Health);
            
            // Size of Position (Position)
            size += PositionSerializer.Instance.GetSize(player.Position);
            
            // Size of IsActive (bool)
            size += BooleanSerializer.Instance.GetSize(player.IsActive);

            // Size of Achievements list
            size += Int32Serializer.Instance.GetSize(player.Achievements.Count); // Count
            foreach (var achievement in player.Achievements)
            {
                size += StringSerializer.Instance.GetSize(achievement);
            }
            
            // Size of Stats dictionary
            size += Int32Serializer.Instance.GetSize(player.Stats.Count); // Count
            foreach (var kvp in player.Stats)
            {
                size += StringSerializer.Instance.GetSize(kvp.Key);
                size += Int32Serializer.Instance.GetSize(kvp.Value);
            }
            
            return size;
        }

        /// <summary>
        /// Serializes a PlayerData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(PlayerData? player, Span<byte> buffer, ref int offset)
        {
            // Handle null case with standard pattern
            if (NullHandler.WriteNullIfNeeded(player, buffer, ref offset))
                return;
            
            // Serialize PlayerId (int)
            Int32Serializer.Instance.Serialize(player.PlayerId, buffer, ref offset);
            
            // Serialize PlayerName (string)
            StringSerializer.Instance.Serialize(player.PlayerName, buffer, ref offset);
            
            // Serialize Health (int)
            Int32Serializer.Instance.Serialize(player.Health, buffer, ref offset);
            
            // Serialize Position using PositionSerializer
            PositionSerializer.Instance.Serialize(player.Position, buffer, ref offset);
            
            // Serialize IsActive (bool)
            BooleanSerializer.Instance.Serialize(player.IsActive, buffer, ref offset);
            
            // Serialize Achievements list
            Int32Serializer.Instance.Serialize(player.Achievements.Count, buffer, ref offset);
            foreach (var achievement in player.Achievements)
            {
                StringSerializer.Instance.Serialize(achievement, buffer, ref offset);
            }
            
            // Serialize Stats dictionary
            Int32Serializer.Instance.Serialize(player.Stats.Count, buffer, ref offset);
            foreach (var kvp in player.Stats)
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
            // Check for null with standard pattern
            if (NullHandler.ReadAndCheckNull(buffer, ref offset, out int playerId))
            {
                value = null;
                return;
            }

            // Get a PlayerData instance from pool
            var player = _playerPool.Get();

            // First int was actually the PlayerId
            player.PlayerId = playerId;
            
            // Read PlayerName
            StringSerializer.Instance.Deserialize(out string? playerName, buffer, ref offset);
            player.PlayerName = playerName;
            
            // Read Health
            Int32Serializer.Instance.Deserialize(out int health, buffer, ref offset);
            player.Health = health;
            
            // Read Position
            PositionSerializer.Instance.Deserialize(out Position position, buffer, ref offset);
            player.Position = position;
            
            // Read IsActive
            BooleanSerializer.Instance.Deserialize(out bool isActive, buffer, ref offset);
            player.IsActive = isActive;
            
            // Read Achievements list
            Int32Serializer.Instance.Deserialize(out int achievementsCount, buffer, ref offset);
            player.Achievements.Clear();
            for (int i = 0; i < achievementsCount; i++)
            {
                StringSerializer.Instance.Deserialize(out string? achievement, buffer, ref offset);
                player.Achievements.Add(achievement);
            }
            
            // Read Stats dictionary
            Int32Serializer.Instance.Deserialize(out int statsCount, buffer, ref offset);
            player.Stats.Clear();
            for (int i = 0; i < statsCount; i++)
            {
                StringSerializer.Instance.Deserialize(out string? key, buffer, ref offset);
                Int32Serializer.Instance.Deserialize(out int statValue, buffer, ref offset);
                player.Stats[key] = statValue;
            }

            value = player;
        }
    }
} 