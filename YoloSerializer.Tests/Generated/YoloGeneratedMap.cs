using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests.Generated
{
    /// <summary>
    /// Hard-coded map of type IDs to serializers for maximum performance
    /// </summary>
    public sealed class YoloGeneratedMap : ITypeMap
    {
        // Singleton instance for performance
        private static readonly YoloGeneratedMap _instance = new YoloGeneratedMap();
        
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static YoloGeneratedMap Instance => _instance;
        
        // Private constructor to enforce singleton pattern
        private YoloGeneratedMap() { }
        
        /// <summary>
        /// Type ID used for null values
        /// </summary>
        public const byte NULL_TYPE_ID = 0;
        
        /// <summary>
        /// Type ID for PlayerData
        /// </summary>
        public const byte PLAYER_DATA_TYPE_ID = 1;
        
        /// <summary>
        /// Type ID for Position
        /// </summary>
        public const byte POSITION_TYPE_ID = 2;
        
        /// <summary>
        /// Gets the null type ID for the ITypeMap interface
        /// </summary>
        byte ITypeMap.NullTypeId => NULL_TYPE_ID;
        
        /// <summary>
        /// Gets the type ID for a type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetTypeId<T>() where T : class, IYoloSerializable
        {
            Type type = typeof(T);
            
            if (type == typeof(PlayerData))
                return PLAYER_DATA_TYPE_ID;
                
            if (type == typeof(Position))
                return POSITION_TYPE_ID;
                
            throw new ArgumentException($"Unknown type: {type.Name}");
        }
        
        /// <summary>
        /// Serializes an object to a byte span without boxing
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            if (obj is PlayerData playerData)
            {
                PlayerDataSerializer.Instance.Serialize(playerData, buffer, ref offset);
                return;
            }
            
            if (obj is Position position)
            {
                PositionSerializer.Instance.Serialize(position, buffer, ref offset);
                return;
            }
            
            throw new ArgumentException($"Unknown type: {obj.GetType().Name}");
        }
        
        /// <summary>
        /// Gets the serialized size of an object without boxing
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T obj) where T : class, IYoloSerializable
        {
            if (obj is PlayerData playerData)
                return PlayerDataSerializer.Instance.GetSize(playerData);
                
            if (obj is Position position)
                return PositionSerializer.Instance.GetSize(position);
                
            throw new ArgumentException($"Unknown type: {obj.GetType().Name}");
        }
        
        /// <summary>
        /// Deserializes an object from a byte span based on type ID
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? DeserializeById(byte typeId, ReadOnlySpan<byte> buffer, ref int offset)
        {
            return typeId switch
            {
                PLAYER_DATA_TYPE_ID => DeserializePlayerData(buffer, ref offset),
                POSITION_TYPE_ID => DeserializePosition(buffer, ref offset),
                _ => throw new ArgumentException($"Unknown type ID: {typeId}")
            };
        }
        
        /// <summary>
        /// Helper method to deserialize PlayerData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PlayerData? DeserializePlayerData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            PlayerData? result;
            PlayerDataSerializer.Instance.Deserialize(out result, buffer, ref offset);
            return result;
        }
        
        /// <summary>
        /// Helper method to deserialize Position
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Position DeserializePosition(ReadOnlySpan<byte> buffer, ref int offset)
        {
            Position result;
            PositionSerializer.Instance.Deserialize(out result, buffer, ref offset);
            return result;
        }
    }
} 