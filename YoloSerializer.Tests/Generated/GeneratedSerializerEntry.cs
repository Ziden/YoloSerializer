using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer using pattern matching for optimal dispatch
    /// </summary>
    public sealed class GeneratedSerializerEntry
    {
        private static readonly GeneratedSerializerEntry _instance = new GeneratedSerializerEntry();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static GeneratedSerializerEntry Instance => _instance;
        
        private GeneratedSerializerEntry() 
        {
            // Register the types we know about
            TypeRegistry.RegisterType<PlayerData>();
            TypeRegistry.RegisterType<Position>();
        }
        
        /// <summary>
        /// Serializes an object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : IYoloSerializable
        {
            // Handle null case
            if (obj == null)
            {
                EnsureBufferSize(buffer, offset, sizeof(byte));
                buffer[offset++] = TypeRegistry.NULL_TYPE_ID;
                return;
            }
            
            // Write type ID from the registry
            EnsureBufferSize(buffer, offset, sizeof(byte));
            buffer[offset++] = TypeRegistry.GetTypeId<T>();
            
            // Type-based dispatch using pattern matching
            if (obj is PlayerData playerData)
            {
                PlayerDataSerializer.Instance.Serialize(playerData, buffer, ref offset);
            }
            else
            {
                throw new ArgumentException($"Unsupported type: {obj.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Serializes an object to a byte span with pre-computed size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : IYoloSerializable
        {
            // Handle null case
            if (obj == null)
            {
                buffer[offset++] = TypeRegistry.NULL_TYPE_ID;
                return;
            }
            
            // Write type ID from the registry
            buffer[offset++] = TypeRegistry.GetTypeId<T>();
            
            // Type-based dispatch using pattern matching
            if (obj is PlayerData playerData)
            {
                PlayerDataSerializer.Instance.Serialize(playerData, buffer, ref offset);
            }
            else
            {
                throw new ArgumentException($"Unsupported type: {obj.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Deserializes an object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Ensure we have at least a byte for the type ID
            EnsureBufferSize(buffer, offset, sizeof(byte));
            
            // Read type ID
            byte typeId = buffer[offset++];
            
            // Check for null
            if (typeId == TypeRegistry.NULL_TYPE_ID)
                return null;
                
            // Type-based dispatch using type ID
            return typeId switch
            {
                #region codegen
                _ when typeId == TypeRegistry.GetTypeId<PlayerData>() => DeserializePlayerData(buffer, ref offset),
                _ when typeId == TypeRegistry.GetTypeId<Position>() => DeserializePosition(buffer, ref offset),
                _ => throw new InvalidOperationException($"Unknown type ID: {typeId}")
                #endregion
            };
        }
        
        /// <summary>
        /// Helper method to deserialize PlayerData
        /// </summary>
        private PlayerData? DeserializePlayerData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            PlayerData? result;
            PlayerDataSerializer.Instance.Deserialize(out result, buffer, ref offset);
            return result;
        }
        
        /// <summary>
        /// Helper method to deserialize Position
        /// </summary>
        private Position DeserializePosition(ReadOnlySpan<byte> buffer, ref int offset)
        {
            Position result;
            PositionSerializer.Instance.Deserialize(out result, buffer, ref offset);
            return result;
        }
        
        /// <summary>
        /// Deserializes an object from a byte span with strong typing
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Deserialize<T>(ReadOnlySpan<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            // Ensure we have at least a byte for the type ID
            EnsureBufferSize(buffer, offset, sizeof(byte));
            
            // Read type ID
            byte typeId = buffer[offset++];
            
            // Check for null
            if (typeId == TypeRegistry.NULL_TYPE_ID)
                return null;

            #region codegen
            // Type safety checks for strongly-typed deserialization
            if (typeof(T) == typeof(PlayerData))
            {
                if (typeId != TypeRegistry.GetTypeId<PlayerData>())
                    throw new InvalidCastException($"Cannot deserialize type ID {typeId} as PlayerData");
                    
                return (T)(object)DeserializePlayerData(buffer, ref offset);
            }
            else if (typeof(T) == typeof(Position))
            {
                if (typeId != TypeRegistry.GetTypeId<Position>())
                    throw new InvalidCastException($"Cannot deserialize type ID {typeId} as Position");
                    
                return (T)(object)DeserializePosition(buffer, ref offset);
            }
            #endregion

            // If we get here, we don't know how to deserialize this type
            throw new InvalidOperationException($"Unknown type: {typeof(T).Name}");
        }
        
        /// <summary>
        /// Gets the serialized size of an object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T? obj) where T : IYoloSerializable
        {
            // Handle null case
            if (obj == null)
                return sizeof(byte); // Just the null type ID marker

            #region CODEGEN
            // Type ID byte + type-specific size
            if (obj is PlayerData playerData)
            {
                return sizeof(byte) + PlayerDataSerializer.Instance.GetSize(playerData);
            }
            else if (obj is Position position)
            {
                return sizeof(byte) + PositionSerializer.Instance.GetSize(position);
            }
            #endregion

            throw new ArgumentException($"Unsupported type: {obj.GetType().Name}");
        }
        
        /// <summary>
        /// Ensures the buffer has enough space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBufferSize(Span<byte> buffer, int offset, int size)
        {
            if (buffer.Length - offset < size)
                throw new ArgumentException($"Buffer too small. Needs at least {size} more bytes.");
        }
        
        /// <summary>
        /// Ensures the buffer has enough space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBufferSize(ReadOnlySpan<byte> buffer, int offset, int size)
        {
            if (buffer.Length - offset < size)
                throw new ArgumentException($"Buffer too small. Needs at least {size} more bytes.");
        }
    }
} 