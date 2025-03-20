using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Benchmarks.Models;

using YoloSerializer.Benchmarks.Models;

using System;

using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for NestedData objects
    /// </summary>
    public sealed class NestedDataSerializer : ISerializer<YoloSerializer.Benchmarks.Models.NestedData?>
    {
        private static readonly NestedDataSerializer _instance = new NestedDataSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static NestedDataSerializer Instance => _instance;
        
        private NestedDataSerializer() { }


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<NestedData> _nestedDataPool = 
            new ObjectPool<NestedData>(() => new NestedData());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the NestedData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Benchmarks.Models.NestedData? nestedData)
        {

            if (nestedData == null)
                throw new ArgumentNullException(nameof(nestedData));

            
            int size = 0;
            size += Int32Serializer.Instance.GetSize(nestedData.Index);
                        size += StringSerializer.Instance.GetSize(nestedData.Name);
                        size += DoubleSerializer.Instance.GetSize(nestedData.Value);

            return size;
        }

        /// <summary>
        /// Serializes a NestedData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Benchmarks.Models.NestedData? nestedData, Span<byte> buffer, ref int offset)
        {

            if (nestedData == null)
                throw new ArgumentNullException(nameof(nestedData));

            Int32Serializer.Instance.Serialize(nestedData.Index, buffer, ref offset);
                        StringSerializer.Instance.Serialize(nestedData.Name, buffer, ref offset);
                        DoubleSerializer.Instance.Serialize(nestedData.Value, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a NestedData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Benchmarks.Models.NestedData? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a NestedData instance from pool
            var nestedData = _nestedDataPool.Get();


            Int32Serializer.Instance.Deserialize(out int _local_index, buffer, ref offset);
                        nestedData.Index = _local_index;
                        StringSerializer.Instance.Deserialize(out string _local_name, buffer, ref offset);
                        nestedData.Name = _local_name;
                        DoubleSerializer.Instance.Deserialize(out double _local_value, buffer, ref offset);
                        nestedData.Value = _local_value;

            value = nestedData;
        }
    }
}