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

using System.Collections.Generic;

using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for ComplexData objects
    /// </summary>
    public sealed class ComplexDataSerializer : ISerializer<YoloSerializer.Benchmarks.Models.ComplexData?>
    {
        private static readonly ComplexDataSerializer _instance = new ComplexDataSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static ComplexDataSerializer Instance => _instance;
        
        private ComplexDataSerializer() { }


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<ComplexData> _complexDataPool = 
            new ObjectPool<ComplexData>(() => new ComplexData());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the ComplexData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Benchmarks.Models.ComplexData? complexData)
        {

            if (complexData == null)
                throw new ArgumentNullException(nameof(complexData));

            
            int size = 0;
            size += Int32Serializer.Instance.GetSize(complexData.Id);
                        size += StringSerializer.Instance.GetSize(complexData.Title);
                        size += Int32Serializer.Instance.GetSize(complexData.Tags.Count) + complexData.Tags.Sum(listItem => StringSerializer.Instance.GetSize(listItem));
                        size += Int32Serializer.Instance.GetSize(complexData.Metrics.Count) + complexData.Metrics.Sum(kvp => StringSerializer.Instance.GetSize(kvp.Key) + SingleSerializer.Instance.GetSize(kvp.Value));
                        size += SimpleDataSerializer.Instance.GetSize(complexData.Metadata);
                        size += Int32Serializer.Instance.GetSize(complexData.Items.Length) + complexData.Items.Sum(arrayItem => NestedDataSerializer.Instance.GetSize(arrayItem));
                        size += EnumSerializer<DataStatus>.Instance.GetSize(complexData.Status);

            return size;
        }

        /// <summary>
        /// Serializes a ComplexData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Benchmarks.Models.ComplexData? complexData, Span<byte> buffer, ref int offset)
        {

            if (complexData == null)
                throw new ArgumentNullException(nameof(complexData));

            Int32Serializer.Instance.Serialize(complexData.Id, buffer, ref offset);
                        StringSerializer.Instance.Serialize(complexData.Title, buffer, ref offset);
                        Int32Serializer.Instance.Serialize(complexData.Tags.Count, buffer, ref offset);
                        foreach (var listItem in complexData.Tags)
                        {
                            StringSerializer.Instance.Serialize(listItem, buffer, ref offset);
                        }
                        Int32Serializer.Instance.Serialize(complexData.Metrics.Count, buffer, ref offset);
                        foreach (var kvp in complexData.Metrics)
                        {
                            StringSerializer.Instance.Serialize(kvp.Key, buffer, ref offset);
                            SingleSerializer.Instance.Serialize(kvp.Value, buffer, ref offset);
                        }
                        SimpleDataSerializer.Instance.Serialize(complexData.Metadata, buffer, ref offset);
                        Int32Serializer.Instance.Serialize(complexData.Items.Length, buffer, ref offset);
                        foreach (var arrayItem in complexData.Items)
                        {
                            NestedDataSerializer.Instance.Serialize(arrayItem, buffer, ref offset);
                        }
                        EnumSerializer<DataStatus>.Instance.Serialize(complexData.Status, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a ComplexData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Benchmarks.Models.ComplexData? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a ComplexData instance from pool
            var complexData = _complexDataPool.Get();


            Int32Serializer.Instance.Deserialize(out int _local_id, buffer, ref offset);
                        complexData.Id = _local_id;
                        StringSerializer.Instance.Deserialize(out string _local_title, buffer, ref offset);
                        complexData.Title = _local_title;
                        Int32Serializer.Instance.Deserialize(out int _local_tagsCount, buffer, ref offset);
                        complexData.Tags.Clear();
                        for (int i = 0; i < _local_tagsCount; i++)
                        {
                            StringSerializer.Instance.Deserialize(out System.String listItem, buffer, ref offset);
                            complexData.Tags.Add(listItem);
                        }
                        Int32Serializer.Instance.Deserialize(out int _local_metricsCount, buffer, ref offset);
                        complexData.Metrics.Clear();
                        for (int i = 0; i < _local_metricsCount; i++)
                        {
                            StringSerializer.Instance.Deserialize(out System.String key, buffer, ref offset);
                            SingleSerializer.Instance.Deserialize(out System.Single dictValue, buffer, ref offset);
                            complexData.Metrics[key] = dictValue;
                        }
                        SimpleDataSerializer.Instance.Deserialize(out SimpleData? _local_metadata, buffer, ref offset);
                        complexData.Metadata = _local_metadata;
                        Int32Serializer.Instance.Deserialize(out int _local_itemsLength, buffer, ref offset);
                        complexData.Items = new NestedData[_local_itemsLength];
                        for (int i = 0; i < _local_itemsLength; i++)
                        {
                            NestedDataSerializer.Instance.Deserialize(out NestedData arrayItem, buffer, ref offset);
                            complexData.Items[i] = arrayItem;
                        }
                        EnumSerializer<DataStatus>.Instance.Deserialize(out DataStatus _local_status, buffer, ref offset);
                        complexData.Status = _local_status;

            value = complexData;
        }
    }
}