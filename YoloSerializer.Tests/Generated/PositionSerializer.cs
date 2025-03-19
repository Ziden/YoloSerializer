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
    /// High-performance serializer for Position objects
    /// </summary>
    public sealed class PositionSerializer : ISerializer<Position?>
    {
        private static readonly PositionSerializer _instance = new PositionSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static PositionSerializer Instance => _instance;
        
        private PositionSerializer() { }
        
        // Maximum size to allocate on stack
        private const int MaxStackAllocSize = 1024;


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<Position> _positionPool = 
            new ObjectPool<Position>(() => new Position());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the Position
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(Position? position)
        {

            if (position == null)
                throw new ArgumentNullException(nameof(position));

            
            int size = 0;
            

            // Size of X (float)
            size += FloatSerializer.Instance.GetSize(position.X);

            // Size of Y (float)
            size += FloatSerializer.Instance.GetSize(position.Y);

            // Size of Z (float)
            size += FloatSerializer.Instance.GetSize(position.Z);

            
            return size;
        }

        /// <summary>
        /// Serializes a Position object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Position? position, Span<byte> buffer, ref int offset)
        {

            if (position == null)
                throw new ArgumentNullException(nameof(position));

            

            // Serialize X (float)
            FloatSerializer.Instance.Serialize(position.X, buffer, ref offset);

            // Serialize Y (float)
            FloatSerializer.Instance.Serialize(position.Y, buffer, ref offset);

            // Serialize Z (float)
            FloatSerializer.Instance.Serialize(position.Z, buffer, ref offset);

        }

        /// <summary>
        /// Deserializes a Position object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out Position? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a Position instance from pool
            var position = _positionPool.Get();



            // Read X
            FloatSerializer.Instance.Deserialize(out float _local_x, buffer, ref offset);
                        position.X = _local_x;

            // Read Y
            FloatSerializer.Instance.Deserialize(out float _local_y, buffer, ref offset);
                        position.Y = _local_y;

            // Read Z
            FloatSerializer.Instance.Deserialize(out float _local_z, buffer, ref offset);
                        position.Z = _local_z;


            value = position;
        }
    }
}