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
    /// High-performance serializer for SimpleData objects
    /// </summary>
    public sealed class SimpleDataSerializer : ISerializer<YoloSerializer.Benchmarks.Models.SimpleData?>
    {
        private static readonly SimpleDataSerializer _instance = new SimpleDataSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static SimpleDataSerializer Instance => _instance;
        
        private SimpleDataSerializer() { }


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<SimpleData> _simpleDataPool = 
            new ObjectPool<SimpleData>(() => new SimpleData());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the SimpleData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Benchmarks.Models.SimpleData? simpleData)
        {

            if (simpleData == null)
                throw new ArgumentNullException(nameof(simpleData));

            
            int size = 0;
            size += Int32Serializer.Instance.GetSize(simpleData.Id);
                        size += StringSerializer.Instance.GetSize(simpleData.Name);
                        size += BooleanSerializer.Instance.GetSize(simpleData.IsActive);
                        size += DoubleSerializer.Instance.GetSize(simpleData.Value);
                        size += DateTimeSerializer.Instance.GetSize(simpleData.CreatedAt);
                        size += GuidSerializer.Instance.GetSize(simpleData.UniqueId);

            return size;
        }

        /// <summary>
        /// Serializes a SimpleData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Benchmarks.Models.SimpleData? simpleData, Span<byte> buffer, ref int offset)
        {

            if (simpleData == null)
                throw new ArgumentNullException(nameof(simpleData));

            Int32Serializer.Instance.Serialize(simpleData.Id, buffer, ref offset);
                        StringSerializer.Instance.Serialize(simpleData.Name, buffer, ref offset);
                        BooleanSerializer.Instance.Serialize(simpleData.IsActive, buffer, ref offset);
                        DoubleSerializer.Instance.Serialize(simpleData.Value, buffer, ref offset);
                        DateTimeSerializer.Instance.Serialize(simpleData.CreatedAt, buffer, ref offset);
                        GuidSerializer.Instance.Serialize(simpleData.UniqueId, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a SimpleData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Benchmarks.Models.SimpleData? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a SimpleData instance from pool
            var simpleData = _simpleDataPool.Get();


            Int32Serializer.Instance.Deserialize(out int _local_id, buffer, ref offset);
                        simpleData.Id = _local_id;
                        StringSerializer.Instance.Deserialize(out string _local_name, buffer, ref offset);
                        simpleData.Name = _local_name;
                        BooleanSerializer.Instance.Deserialize(out bool _local_isActive, buffer, ref offset);
                        simpleData.IsActive = _local_isActive;
                        DoubleSerializer.Instance.Deserialize(out double _local_value, buffer, ref offset);
                        simpleData.Value = _local_value;
                        DateTimeSerializer.Instance.Deserialize(out DateTime _local_createdAt, buffer, ref offset);
                        simpleData.CreatedAt = _local_createdAt;
                        GuidSerializer.Instance.Deserialize(out Guid _local_uniqueId, buffer, ref offset);
                        simpleData.UniqueId = _local_uniqueId;

            value = simpleData;
        }
    }
}