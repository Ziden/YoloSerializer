using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core
{
    /// <summary>
    /// High-performance serializer using pattern matching for optimal dispatch
    /// </summary>
    public static class YoloSerializer
    {
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
                buffer[offset++] = TypeRegistry.NULL_TYPE_ID;
                return;
            }
            
            // Write type ID from the TypeRegistry
            EnsureBufferSize(buffer, offset, sizeof(byte));
            buffer[offset++] = TypeRegistry.GetTypeId<T>();
            
            // Dispatch to the appropriate serializer
            SerializeObject(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Internal method to serialize objects with pattern matching
        /// This will be code-generated in the future
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SerializeObject(IYoloSerializable obj, Span<byte> buffer, ref int offset)
        {
            // This will be replaced by generated code
            throw new NotImplementedException($"No serializer registered for type {obj.GetType().Name}");
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
            if (typeId == TypeRegistry.NULL_TYPE_ID)
                return null;
                
            // Dispatch to appropriate deserializer based on type ID
            return DeserializeObject(typeId, buffer, ref offset);
        }
        
        /// <summary>
        /// Internal method to deserialize objects based on type ID
        /// This will be code-generated in the future
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object? DeserializeObject(byte typeId, ReadOnlySpan<byte> buffer, ref int offset)
        {
            // This will be replaced by generated code
            throw new NotImplementedException($"No deserializer registered for type ID {typeId}");
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
            if (typeId == TypeRegistry.NULL_TYPE_ID)
                return null;
                
            // Deserialize object
            object? obj = DeserializeObject(typeId, buffer, ref offset);
            
            // Verify type before returning
            if (obj is T result)
                return result;
                
            throw new InvalidCastException($"Cannot cast type ID {typeId} to {typeof(T).Name}");
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
                
            // Type ID byte + object-specific size
            return sizeof(byte) + GetObjectSize(obj);
        }
        
        /// <summary>
        /// Internal method to get object size with pattern matching
        /// This will be code-generated in the future
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetObjectSize(IYoloSerializable obj)
        {
            // This will be replaced by generated code
            throw new NotImplementedException($"No size calculator registered for type {obj.GetType().Name}");
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