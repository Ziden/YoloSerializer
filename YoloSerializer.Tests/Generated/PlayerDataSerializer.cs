using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for PlayerData objects
    /// </summary>
    public static class PlayerDataSerializer
    {
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
        public static int GetSerializedSize(this PlayerData? player)
        {
            if (player == null)
                return sizeof(int); // Just a -1 marker for null
                
            int size = 0;
            
            // Size of PlayerId (int)
            size += Int32Serializer.GetSize(player.PlayerId);
            
            // Size of PlayerName (string)
            size += StringSerializer.GetSize(player.PlayerName);
            
            // Size of Health (int)
            size += Int32Serializer.GetSize(player.Health);
            
            // Size of Position (Position)
            size += PositionSerializer.SerializedSize;
            
            // Size of IsActive (bool)
            size += BooleanSerializer.GetSize(player.IsActive);
            
            return size;
        }

        /// <summary>
        /// Serializes a PlayerData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(this PlayerData? player, Span<byte> buffer, ref int offset)
        {
            if (player == null)
            {
                // Write null marker (-1)
                Int32Serializer.Serialize(-1, buffer, ref offset);
                return;
            }
            
            // Serialize PlayerId (int)
            Int32Serializer.Serialize(player.PlayerId, buffer, ref offset);
            
            // Serialize PlayerName (string)
            StringSerializer.Serialize(player.PlayerName, buffer, ref offset);
            
            // Serialize Health (int)
            Int32Serializer.Serialize(player.Health, buffer, ref offset);
            
            // Serialize Position using PositionSerializer
            PositionSerializer.Serialize(player.Position, buffer, ref offset);
            
            // Serialize IsActive (bool)
            BooleanSerializer.Serialize(player.IsActive, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a PlayerData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PlayerData? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Read first int to check for null
            Int32Serializer.Deserialize(out int marker, buffer, ref offset);
            
            // Check for null marker
            if (marker == -1)
                return null;

            // Get a PlayerData instance from pool
            var player = _playerPool.Get();

            // First int was actually the PlayerId
            player.PlayerId = marker;
            
            // Read PlayerName
            StringSerializer.Deserialize(out string? playerName, buffer, ref offset);
            player.PlayerName = playerName;
            
            // Read Health
            Int32Serializer.Deserialize(out int health, buffer, ref offset);
            player.Health = health;
            
            // Read Position using its specialized deserializer
            player.Position = PositionSerializer.Deserialize(buffer, ref offset);
            
            // Read IsActive
            BooleanSerializer.Deserialize(out bool isActive, buffer, ref offset);
            player.IsActive = isActive;

            return player;
        }
    }
} 