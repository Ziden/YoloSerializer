using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Marker interface for serializable types
    /// </summary>
    public interface IYoloSerializable
    {
        /// <summary>
        /// Type ID for serialization (must be unique per type)
        /// </summary>
        byte TypeId { get; }
    }
    
    /// <summary>
    /// High-performance serializer using pattern matching for optimal dispatch
    /// </summary>
    public static class YoloSerializer
    {
        /// <summary>
        /// Type ID reserved for null values
        /// </summary>
        public const byte NULL_TYPE_ID = 0;
        
        /// <summary>
        /// Serializes an object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            // Handle null case
            if (obj == null)
            {
                EnsureBufferSize(buffer, offset, sizeof(byte));
                buffer[offset++] = NULL_TYPE_ID;
                return;
            }
            
            // Write type ID from the interface
            EnsureBufferSize(buffer, offset, sizeof(byte));
            buffer[offset++] = obj.TypeId;
            
            // Type-based dispatch using pattern matching
            if (obj is PlayerData playerData)
            {
                PlayerDataSerializer.Serialize(playerData, buffer, ref offset);
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
        public static void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            // Handle null case
            if (obj == null)
            {
                buffer[offset++] = NULL_TYPE_ID;
                return;
            }
            
            // Write type ID from the interface
            buffer[offset++] = obj.TypeId;
            
            // Type-based dispatch using pattern matching
            if (obj is PlayerData playerData)
            {
                PlayerDataSerializer.Serialize(playerData, buffer, ref offset);
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
        public static object? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Ensure we have at least a byte for the type ID
            EnsureBufferSize(buffer, offset, sizeof(byte));
            
            // Read type ID
            byte typeId = buffer[offset++];
            
            // Check for null
            if (typeId == NULL_TYPE_ID)
                return null;
                
            // Type-based dispatch using type ID
            return typeId switch
            {
                PlayerData.TYPE_ID => PlayerDataSerializer.Deserialize(buffer, ref offset),
                _ => throw new InvalidOperationException($"Unknown type ID: {typeId}")
            };
        }
        
        /// <summary>
        /// Deserializes an object from a byte span with strong typing
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Deserialize<T>(ReadOnlySpan<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            // Ensure we have at least a byte for the type ID
            EnsureBufferSize(buffer, offset, sizeof(byte));
            
            // Read type ID
            byte typeId = buffer[offset++];
            
            // Check for null
            if (typeId == NULL_TYPE_ID)
                return null;
                
            // Type safety checks for strongly-typed deserialization
            if (typeof(T) == typeof(PlayerData))
            {
                if (typeId != PlayerData.TYPE_ID)
                    throw new InvalidCastException($"Cannot deserialize type ID {typeId} as PlayerData");
                    
                return (T)(object)PlayerDataSerializer.Deserialize(buffer, ref offset);
            }
            
            // If we get here, we don't know how to deserialize this type
            throw new InvalidOperationException($"Unknown type: {typeof(T).Name}");
        }
        
        /// <summary>
        /// Gets the serialized size of an object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSerializedSize<T>(T? obj) where T : class, IYoloSerializable
        {
            // Handle null case
            if (obj == null)
                return sizeof(byte); // Just the null type ID marker
                
            // Type ID byte + type-specific size
            if (obj is PlayerData playerData)
            {
                return sizeof(byte) + playerData.GetSerializedSize();
            }
            
            throw new ArgumentException($"Unsupported type: {obj.GetType().Name}");
        }
        
        /// <summary>
        /// Ensures the buffer has enough space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureBufferSize(Span<byte> buffer, int offset, int size)
        {
            if (buffer.Length - offset < size)
                throw new ArgumentException($"Buffer too small. Needs at least {size} more bytes.");
        }
        
        /// <summary>
        /// Ensures the buffer has enough space
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureBufferSize(ReadOnlySpan<byte> buffer, int offset, int size)
        {
            if (buffer.Length - offset < size)
                throw new ArgumentException($"Buffer too small. Needs at least {size} more bytes.");
        }
    }
} 