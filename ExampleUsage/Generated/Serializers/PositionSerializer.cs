using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Core.ModelsYolo;

using YoloSerializer.Core.ModelsYolo;

using System;

using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for Position objects
    /// </summary>
    public sealed class PositionSerializer : ISerializer<YoloSerializer.Core.ModelsYolo.Position?>
    {
        private static readonly PositionSerializer _instance = new PositionSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static PositionSerializer Instance => _instance;
        
        private PositionSerializer() { }


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<Position> _positionPool = 
            new ObjectPool<Position>(() => new Position());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        // Number of nullable fields in this type
        private const int NullableFieldCount = 0;
        
        // Size of the nullability bitset in bytes
        private const int NullableBitsetSize = 0;
        
        // Create stack allocated bitset array for nullability tracking
        private Span<byte> GetBitsetArray(Span<byte> tempBuffer) => 
            NullableBitsetSize > 0 ? tempBuffer.Slice(0, NullableBitsetSize) : default;

        /// <summary>
        /// Gets the total size needed to serialize the Position
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Core.ModelsYolo.Position? position)
        {

            if (position == null)
                throw new ArgumentNullException(nameof(position));

            
            int size = 0;
            
            // Add size for nullability bitset if needed
            size += NullableBitsetSize;
            
            size += SingleSerializer.Instance.GetSize(position.X);
                        size += SingleSerializer.Instance.GetSize(position.Y);
                        size += SingleSerializer.Instance.GetSize(position.Z);

            return size;
        }

        /// <summary>
        /// Serializes a Position object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Core.ModelsYolo.Position? position, Span<byte> buffer, ref int offset)
        {

            if (position == null)
                throw new ArgumentNullException(nameof(position));


            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Initialize all bits to 0 (non-null)
            for (int i = 0; i < bitset.Length; i++)
                bitset[i] = 0;
                
            // First determine the nullability of each field
            
            
            // Write the nullability bitset to the buffer
            NullableBitset.SerializeBitset(bitset, NullableBitsetSize, buffer, ref offset);
            
            // Write the actual field values
            SingleSerializer.Instance.Serialize(position.X, buffer, ref offset);
                        SingleSerializer.Instance.Serialize(position.Y, buffer, ref offset);
                        SingleSerializer.Instance.Serialize(position.Z, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a Position object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Core.ModelsYolo.Position? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a Position instance from pool
            var position = _positionPool.Get();


            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Read the nullability bitset from the buffer
            NullableBitset.DeserializeBitset(buffer, ref offset, bitset, NullableBitsetSize);

            SingleSerializer.Instance.Deserialize(out float _local_x, buffer, ref offset);
                        position.X = _local_x;
                        SingleSerializer.Instance.Deserialize(out float _local_y, buffer, ref offset);
                        position.Y = _local_y;
                        SingleSerializer.Instance.Deserialize(out float _local_z, buffer, ref offset);
                        position.Z = _local_z;

            value = position;
        }
    }
}