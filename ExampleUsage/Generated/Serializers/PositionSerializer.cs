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
    public sealed class PositionSerializer : ISerializer<YoloSerializer.Core.Models.Position?>
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
        private static readonly ObjectPool<Position> _Pool = 
            new ObjectPool<Position>(() => new Position());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the Position
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Core.Models.Position? position)
        {

            if (position == null)
                throw new ArgumentNullException(nameof(position));

            
            int size = 0;
            size += FloatSerializer.Instance.GetSize(position.X);
                        size += FloatSerializer.Instance.GetSize(position.Y);
                        size += FloatSerializer.Instance.GetSize(position.Z);

            return size;
        }

        /// <summary>
        /// Serializes a Position object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Core.Models.Position? position, Span<byte> buffer, ref int offset)
        {

            if (position == null)
                throw new ArgumentNullException(nameof(position));

            FloatSerializer.Instance.Serialize(position.X, buffer, ref offset);
                        FloatSerializer.Instance.Serialize(position.Y, buffer, ref offset);
                        FloatSerializer.Instance.Serialize(position.Z, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a Position object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Core.Models.Position? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a Position instance from pool
            var position = _Pool.Get();


            FloatSerializer.Instance.Deserialize(out float _local_x, buffer, ref offset);
                        position.X = _local_x;
                        FloatSerializer.Instance.Deserialize(out float _local_y, buffer, ref offset);
                        position.Y = _local_y;
                        FloatSerializer.Instance.Deserialize(out float _local_z, buffer, ref offset);
                        position.Z = _local_z;

            value = position;
        }
    }
}