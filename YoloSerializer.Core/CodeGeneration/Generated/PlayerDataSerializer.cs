using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.CodeGeneration.Generated;

namespace YoloSerializer.Core.CodeGeneration.Generated
{
    /// <summary>
    /// Auto-generated serializer for PlayerData
    /// </summary>
    public static class PlayerDataSerializer
    {
        // Object pool for PlayerData to reduce allocations during deserialization
        private static readonly ObjectPool<PlayerData> _playerDataPool = 
            new ObjectPool<PlayerData>(() => new PlayerData());
        
        /// <summary>
        /// Serializes a PlayerData object to a byte buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(PlayerData? value, Span<byte> buffer, ref int offset)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "PlayerData cannot be null");
            }
            
            // Serialize PlayerId (int)
            Int32Serializer.Serialize(value.PlayerId, buffer, ref offset);
            
            // Serialize PlayerName (string)
            StringSerializer.Serialize(value.PlayerName, buffer, ref offset);
            
            // Serialize Health (int)
            Int32Serializer.Serialize(value.Health, buffer, ref offset);
            
            // Serialize Position (Position)
            PositionSerializer.Serialize(value.Position, buffer, ref offset);
            
            // Serialize IsActive (bool)
            BooleanSerializer.Serialize(value.IsActive, buffer, ref offset);
        }
        
        /// <summary>
        /// Deserializes a PlayerData object from a byte buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PlayerData Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Get a PlayerData from the pool
            PlayerData result = _playerDataPool.Get();
            
            // Deserialize PlayerId (int)
            Int32Serializer.Deserialize(out int playerId, buffer, ref offset);
            result.PlayerId = playerId;
            
            // Deserialize PlayerName (string)
            StringSerializer.Deserialize(out string? playerName, buffer, ref offset);
            result.PlayerName = playerName;
            
            // Deserialize Health (int)
            Int32Serializer.Deserialize(out int health, buffer, ref offset);
            result.Health = health;
            
            // Deserialize Position (Position)
            result.Position = PositionSerializer.Deserialize(buffer, ref offset);
            
            // Deserialize IsActive (bool)
            BooleanSerializer.Deserialize(out bool isActive, buffer, ref offset);
            result.IsActive = isActive;
            
            return result;
        }
        
        /// <summary>
        /// Calculates the size needed to serialize a PlayerData object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(PlayerData? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "PlayerData cannot be null");
                
            int size = 0;
            
            // Size of PlayerId (int)
            size += Int32Serializer.GetSize(value.PlayerId);
            
            // Size of PlayerName (string)
            size += StringSerializer.GetSize(value.PlayerName);
            
            // Size of Health (int)
            size += Int32Serializer.GetSize(value.Health);
            
            // Size of Position (Position)
            size += PositionSerializer.GetSize(value.Position);
            
            // Size of IsActive (bool)
            size += BooleanSerializer.GetSize(value.IsActive);
            
            return size;
        }
    }
} 