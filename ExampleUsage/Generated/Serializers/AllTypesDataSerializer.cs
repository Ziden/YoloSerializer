using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;

using YoloSerializer.Core.Models;

using System;

using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for AllTypesData objects
    /// </summary>
    public sealed class AllTypesDataSerializer : ISerializer<YoloSerializer.Core.Models.AllTypesData?>
    {
        private static readonly AllTypesDataSerializer _instance = new AllTypesDataSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static AllTypesDataSerializer Instance => _instance;
        
        private AllTypesDataSerializer() { }


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<AllTypesData> _allTypesDataPool = 
            new ObjectPool<AllTypesData>(() => new AllTypesData());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        // Number of nullable fields in this type
        private const int NullableFieldCount = 1;
        
        // Size of the nullability bitset in bytes
        private const int NullableBitsetSize = 1;
        
        // Create stack allocated bitset array for nullability tracking
        private Span<byte> GetBitsetArray(Span<byte> tempBuffer) => 
            NullableBitsetSize > 0 ? tempBuffer.Slice(0, NullableBitsetSize) : default;

        /// <summary>
        /// Gets the total size needed to serialize the AllTypesData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Core.Models.AllTypesData? allTypesData)
        {

            if (allTypesData == null)
                throw new ArgumentNullException(nameof(allTypesData));

            
            int size = 0;
            
            // Add size for nullability bitset if needed
            size += NullableBitsetSize;
            
            size += Int32Serializer.Instance.GetSize(allTypesData.Int32Value);
                        size += Int64Serializer.Instance.GetSize(allTypesData.Int64Value);
                        size += SingleSerializer.Instance.GetSize(allTypesData.FloatValue);
                        size += DoubleSerializer.Instance.GetSize(allTypesData.DoubleValue);
                        size += BooleanSerializer.Instance.GetSize(allTypesData.BoolValue);
                        size += allTypesData.StringValue == null ? 0 : StringSerializer.Instance.GetSize(allTypesData.StringValue);
                        size += ByteSerializer.Instance.GetSize(allTypesData.ByteValue);
                        size += SByteSerializer.Instance.GetSize(allTypesData.SByteValue);
                        size += CharSerializer.Instance.GetSize(allTypesData.CharValue);
                        size += Int16Serializer.Instance.GetSize(allTypesData.Int16Value);
                        size += UInt16Serializer.Instance.GetSize(allTypesData.UInt16Value);
                        size += UInt32Serializer.Instance.GetSize(allTypesData.UInt32Value);
                        size += UInt64Serializer.Instance.GetSize(allTypesData.UInt64Value);
                        size += DecimalSerializer.Instance.GetSize(allTypesData.DecimalValue);
                        size += DateTimeSerializer.Instance.GetSize(allTypesData.DateTimeValue);
                        size += TimeSpanSerializer.Instance.GetSize(allTypesData.TimeSpanValue);
                        size += GuidSerializer.Instance.GetSize(allTypesData.GuidValue);
                        size += EnumSerializer<TestEnum>.Instance.GetSize(allTypesData.EnumValue);

            return size;
        }

        /// <summary>
        /// Serializes a AllTypesData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Core.Models.AllTypesData? allTypesData, Span<byte> buffer, ref int offset)
        {

            if (allTypesData == null)
                throw new ArgumentNullException(nameof(allTypesData));


            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Initialize all bits to 0 (non-null)
            for (int i = 0; i < bitset.Length; i++)
                bitset[i] = 0;
                
            // First determine the nullability of each field
                        NullableBitset.SetBit(bitset, 1, allTypesData.StringValue == null);

            
            // Write the nullability bitset to the buffer
            NullableBitset.SerializeBitset(bitset, NullableBitsetSize, buffer, ref offset);
            
            // Write the actual field values
            Int32Serializer.Instance.Serialize(allTypesData.Int32Value, buffer, ref offset);
                        Int64Serializer.Instance.Serialize(allTypesData.Int64Value, buffer, ref offset);
                        SingleSerializer.Instance.Serialize(allTypesData.FloatValue, buffer, ref offset);
                        DoubleSerializer.Instance.Serialize(allTypesData.DoubleValue, buffer, ref offset);
                        BooleanSerializer.Instance.Serialize(allTypesData.BoolValue, buffer, ref offset);
                        if (!NullableBitset.IsNull(bitset, 1))
                            StringSerializer.Instance.Serialize(allTypesData.StringValue, buffer, ref offset);
                        ByteSerializer.Instance.Serialize(allTypesData.ByteValue, buffer, ref offset);
                        SByteSerializer.Instance.Serialize(allTypesData.SByteValue, buffer, ref offset);
                        CharSerializer.Instance.Serialize(allTypesData.CharValue, buffer, ref offset);
                        Int16Serializer.Instance.Serialize(allTypesData.Int16Value, buffer, ref offset);
                        UInt16Serializer.Instance.Serialize(allTypesData.UInt16Value, buffer, ref offset);
                        UInt32Serializer.Instance.Serialize(allTypesData.UInt32Value, buffer, ref offset);
                        UInt64Serializer.Instance.Serialize(allTypesData.UInt64Value, buffer, ref offset);
                        DecimalSerializer.Instance.Serialize(allTypesData.DecimalValue, buffer, ref offset);
                        DateTimeSerializer.Instance.Serialize(allTypesData.DateTimeValue, buffer, ref offset);
                        TimeSpanSerializer.Instance.Serialize(allTypesData.TimeSpanValue, buffer, ref offset);
                        GuidSerializer.Instance.Serialize(allTypesData.GuidValue, buffer, ref offset);
                        EnumSerializer<TestEnum>.Instance.Serialize(allTypesData.EnumValue, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a AllTypesData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Core.Models.AllTypesData? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a AllTypesData instance from pool
            var allTypesData = _allTypesDataPool.Get();


            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Read the nullability bitset from the buffer
            NullableBitset.DeserializeBitset(buffer, ref offset, bitset, NullableBitsetSize);

            Int32Serializer.Instance.Deserialize(out int _local_int32Value, buffer, ref offset);
                        allTypesData.Int32Value = _local_int32Value;
                        Int64Serializer.Instance.Deserialize(out long _local_int64Value, buffer, ref offset);
                        allTypesData.Int64Value = _local_int64Value;
                        SingleSerializer.Instance.Deserialize(out float _local_floatValue, buffer, ref offset);
                        allTypesData.FloatValue = _local_floatValue;
                        DoubleSerializer.Instance.Deserialize(out double _local_doubleValue, buffer, ref offset);
                        allTypesData.DoubleValue = _local_doubleValue;
                        BooleanSerializer.Instance.Deserialize(out bool _local_boolValue, buffer, ref offset);
                        allTypesData.BoolValue = _local_boolValue;
                        if (NullableBitset.IsNull(bitset, 1))
                            allTypesData.StringValue = null;
                        else
                        {
                            StringSerializer.Instance.Deserialize(out string _local_stringValue, buffer, ref offset);
                            allTypesData.StringValue = _local_stringValue;
                        }
                        ByteSerializer.Instance.Deserialize(out byte _local_byteValue, buffer, ref offset);
                        allTypesData.ByteValue = _local_byteValue;
                        SByteSerializer.Instance.Deserialize(out sbyte _local_sByteValue, buffer, ref offset);
                        allTypesData.SByteValue = _local_sByteValue;
                        CharSerializer.Instance.Deserialize(out char _local_charValue, buffer, ref offset);
                        allTypesData.CharValue = _local_charValue;
                        Int16Serializer.Instance.Deserialize(out short _local_int16Value, buffer, ref offset);
                        allTypesData.Int16Value = _local_int16Value;
                        UInt16Serializer.Instance.Deserialize(out ushort _local_uInt16Value, buffer, ref offset);
                        allTypesData.UInt16Value = _local_uInt16Value;
                        UInt32Serializer.Instance.Deserialize(out uint _local_uInt32Value, buffer, ref offset);
                        allTypesData.UInt32Value = _local_uInt32Value;
                        UInt64Serializer.Instance.Deserialize(out ulong _local_uInt64Value, buffer, ref offset);
                        allTypesData.UInt64Value = _local_uInt64Value;
                        DecimalSerializer.Instance.Deserialize(out decimal _local_decimalValue, buffer, ref offset);
                        allTypesData.DecimalValue = _local_decimalValue;
                        DateTimeSerializer.Instance.Deserialize(out DateTime _local_dateTimeValue, buffer, ref offset);
                        allTypesData.DateTimeValue = _local_dateTimeValue;
                        TimeSpanSerializer.Instance.Deserialize(out TimeSpan _local_timeSpanValue, buffer, ref offset);
                        allTypesData.TimeSpanValue = _local_timeSpanValue;
                        GuidSerializer.Instance.Deserialize(out Guid _local_guidValue, buffer, ref offset);
                        allTypesData.GuidValue = _local_guidValue;
                        EnumSerializer<TestEnum>.Instance.Deserialize(out TestEnum _local_enumValue, buffer, ref offset);
                        allTypesData.EnumValue = _local_enumValue;

            value = allTypesData;
        }
    }
}