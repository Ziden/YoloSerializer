using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Interface for type maps that provide serialization capabilities for a set of types
    /// </summary>
    public interface ITypeMap
    {
        /// <summary>
        /// Type ID used for null values
        /// </summary>
        byte NullTypeId { get; }
        
        /// <summary>
        /// Gets the type ID for a type
        /// </summary>
        byte GetTypeId<T>() where T : class, IYoloSerializable;
        
        /// <summary>
        /// Serializes an object to a byte span
        /// </summary>
        void Serialize<T>(T obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable;
        
        /// <summary>
        /// Gets the serialized size of an object
        /// </summary>
        int GetSerializedSize<T>(T obj) where T : class, IYoloSerializable;
        
        /// <summary>
        /// Deserializes an object based on a type ID
        /// </summary>
        object? DeserializeById(byte typeId, ReadOnlySpan<byte> buffer, ref int offset);
    }

    /// <summary>
    /// High-performance serializer that delegates to a type map for optimal dispatch
    /// </summary>
    /// <typeparam name="TMap">The type map implementation</typeparam>
    public sealed class GeneratedSerializer<TMap> where TMap : ITypeMap
    {
        private readonly TMap _typeMap;
        
        /// <summary>
        /// Creates a new serializer with the given type map
        /// </summary>
        public GeneratedSerializer(TMap typeMap)
        {
            _typeMap = typeMap ?? throw new ArgumentNullException(nameof(typeMap));
        }
        
        /// <summary>
        /// Serializes an object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            // Handle null case
            if (obj == null)
            {
                EnsureBufferSize(buffer, offset, sizeof(byte));
                buffer[offset++] = _typeMap.NullTypeId;
                return;
            }
            
            // Write type ID from the map
            EnsureBufferSize(buffer, offset, sizeof(byte));
            buffer[offset++] = _typeMap.GetTypeId<T>();
            
            // Delegate to the type map for type-specific serialization
            _typeMap.Serialize(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes an object to a byte span with pre-computed size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            // Handle null case
            if (obj == null)
            {
                buffer[offset++] = _typeMap.NullTypeId;
                return;
            }
            
            // Write type ID from the map
            buffer[offset++] = _typeMap.GetTypeId<T>();
            
            // Delegate to the type map for type-specific serialization
            _typeMap.Serialize(obj, buffer, ref offset);
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
            if (typeId == _typeMap.NullTypeId)
                return null;
                
            // Delegate to the type map for type-specific deserialization
            return _typeMap.DeserializeById(typeId, buffer, ref offset);
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
            if (typeId == _typeMap.NullTypeId)
                return null;

            // Delegate to the type map for deserialization
            object? result = _typeMap.DeserializeById(typeId, buffer, ref offset);
            
            // Type safety check
            if (result is T typedResult)
                return typedResult;
                
            throw new InvalidCastException($"Cannot cast deserialized object of type {result?.GetType().Name} to {typeof(T).Name}");
        }
        
        /// <summary>
        /// Gets the serialized size of an object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T? obj) where T : class, IYoloSerializable
        {
            // Handle null case
            if (obj == null)
                return sizeof(byte); // Just the null type ID marker

            // Type ID byte + delegate to the type map for type-specific size calculation
            return sizeof(byte) + _typeMap.GetSerializedSize(obj);
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