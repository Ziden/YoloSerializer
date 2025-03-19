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
    /// High-performance serializer for AllTypesData objects
    /// </summary>
    public sealed class AllTypesDataSerializer : ISerializer<AllTypesData?>
    {
        private static readonly AllTypesDataSerializer _instance = new AllTypesDataSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static AllTypesDataSerializer Instance => _instance;
        
        private AllTypesDataSerializer() { }
        
        // Maximum size to allocate on stack
        private const int MaxStackAllocSize = 1024;


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<AllTypesData> _allTypesDataPool = 
            new ObjectPool<AllTypesData>(() => new AllTypesData());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the AllTypesData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(AllTypesData? allTypesData)
        {

            if (allTypesData == null)
                throw new ArgumentNullException(nameof(allTypesData));

            
            int size = 0;
            // Size of Int32Value (int)
            size += Int32Serializer.Instance.GetSize(allTypesData.Int32Value);
                        // Size of Int64Value (long)
            size += Int64Serializer.Instance.GetSize(allTypesData.Int64Value);
                        // Size of FloatValue (float)
            size += FloatSerializer.Instance.GetSize(allTypesData.FloatValue);
                        // Size of DoubleValue (double)
            size += DoubleSerializer.Instance.GetSize(allTypesData.DoubleValue);
                        // Size of BoolValue (bool)
            size += BooleanSerializer.Instance.GetSize(allTypesData.BoolValue);
                        // Size of StringValue (string)
            size += StringSerializer.Instance.GetSize(allTypesData.StringValue);
                        // Size of ByteValue (byte)
            size += ByteSerializer.Instance.GetSize(allTypesData.ByteValue);
                        // Size of SByteValue (sbyte)
            size += SByteSerializer.Instance.GetSize(allTypesData.SByteValue);
                        // Size of CharValue (char)
            size += CharSerializer.Instance.GetSize(allTypesData.CharValue);
                        // Size of Int16Value (short)
            size += Int16Serializer.Instance.GetSize(allTypesData.Int16Value);
                        // Size of UInt16Value (ushort)
            size += UInt16Serializer.Instance.GetSize(allTypesData.UInt16Value);
                        // Size of UInt32Value (uint)
            size += UInt32Serializer.Instance.GetSize(allTypesData.UInt32Value);
                        // Size of UInt64Value (ulong)
            size += UInt64Serializer.Instance.GetSize(allTypesData.UInt64Value);
                        // Size of DecimalValue (decimal)
            size += DecimalSerializer.Instance.GetSize(allTypesData.DecimalValue);
                        // Size of DateTimeValue (DateTime)
            size += DateTimeSerializer.Instance.GetSize(allTypesData.DateTimeValue);
                        // Size of TimeSpanValue (TimeSpan)
            size += TimeSpanSerializer.Instance.GetSize(allTypesData.TimeSpanValue);
                        // Size of GuidValue (Guid)
            size += GuidSerializer.Instance.GetSize(allTypesData.GuidValue);
                        // Size of EnumValue (TestEnum)
            size += EnumSerializer<TestEnum>.Instance.GetSize(allTypesData.EnumValue);

            return size;
        }

        /// <summary>
        /// Serializes a AllTypesData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(AllTypesData? allTypesData, Span<byte> buffer, ref int offset)
        {

            if (allTypesData == null)
                throw new ArgumentNullException(nameof(allTypesData));

            // Serialize Int32Value (int)
            Int32Serializer.Instance.Serialize(allTypesData.Int32Value, buffer, ref offset);
                        // Serialize Int64Value (long)
            Int64Serializer.Instance.Serialize(allTypesData.Int64Value, buffer, ref offset);
                        // Serialize FloatValue (float)
            FloatSerializer.Instance.Serialize(allTypesData.FloatValue, buffer, ref offset);
                        // Serialize DoubleValue (double)
            DoubleSerializer.Instance.Serialize(allTypesData.DoubleValue, buffer, ref offset);
                        // Serialize BoolValue (bool)
            BooleanSerializer.Instance.Serialize(allTypesData.BoolValue, buffer, ref offset);
                        // Serialize StringValue (string)
            StringSerializer.Instance.Serialize(allTypesData.StringValue, buffer, ref offset);
                        // Serialize ByteValue (byte)
            ByteSerializer.Instance.Serialize(allTypesData.ByteValue, buffer, ref offset);
                        // Serialize SByteValue (sbyte)
            SByteSerializer.Instance.Serialize(allTypesData.SByteValue, buffer, ref offset);
                        // Serialize CharValue (char)
            CharSerializer.Instance.Serialize(allTypesData.CharValue, buffer, ref offset);
                        // Serialize Int16Value (short)
            Int16Serializer.Instance.Serialize(allTypesData.Int16Value, buffer, ref offset);
                        // Serialize UInt16Value (ushort)
            UInt16Serializer.Instance.Serialize(allTypesData.UInt16Value, buffer, ref offset);
                        // Serialize UInt32Value (uint)
            UInt32Serializer.Instance.Serialize(allTypesData.UInt32Value, buffer, ref offset);
                        // Serialize UInt64Value (ulong)
            UInt64Serializer.Instance.Serialize(allTypesData.UInt64Value, buffer, ref offset);
                        // Serialize DecimalValue (decimal)
            DecimalSerializer.Instance.Serialize(allTypesData.DecimalValue, buffer, ref offset);
                        // Serialize DateTimeValue (DateTime)
            DateTimeSerializer.Instance.Serialize(allTypesData.DateTimeValue, buffer, ref offset);
                        // Serialize TimeSpanValue (TimeSpan)
            TimeSpanSerializer.Instance.Serialize(allTypesData.TimeSpanValue, buffer, ref offset);
                        // Serialize GuidValue (Guid)
            GuidSerializer.Instance.Serialize(allTypesData.GuidValue, buffer, ref offset);
                        // Serialize EnumValue (TestEnum)
            EnumSerializer<TestEnum>.Instance.Serialize(allTypesData.EnumValue, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a AllTypesData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out AllTypesData? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a AllTypesData instance from pool
            var allTypesData = _allTypesDataPool.Get();


            // Read Int32Value
            Int32Serializer.Instance.Deserialize(out int _local_int32Value, buffer, ref offset);
                        allTypesData.Int32Value = _local_int32Value;
                        // Read Int64Value
            Int64Serializer.Instance.Deserialize(out long _local_int64Value, buffer, ref offset);
                        allTypesData.Int64Value = _local_int64Value;
                        // Read FloatValue
            FloatSerializer.Instance.Deserialize(out float _local_floatValue, buffer, ref offset);
                        allTypesData.FloatValue = _local_floatValue;
                        // Read DoubleValue
            DoubleSerializer.Instance.Deserialize(out double _local_doubleValue, buffer, ref offset);
                        allTypesData.DoubleValue = _local_doubleValue;
                        // Read BoolValue
            BooleanSerializer.Instance.Deserialize(out bool _local_boolValue, buffer, ref offset);
                        allTypesData.BoolValue = _local_boolValue;
                        // Read StringValue
            StringSerializer.Instance.Deserialize(out string? _local_stringValue, buffer, ref offset);
                        allTypesData.StringValue = _local_stringValue;
                        // Read ByteValue
            ByteSerializer.Instance.Deserialize(out byte _local_byteValue, buffer, ref offset);
                        allTypesData.ByteValue = _local_byteValue;
                        // Read SByteValue
            SByteSerializer.Instance.Deserialize(out sbyte _local_sByteValue, buffer, ref offset);
                        allTypesData.SByteValue = _local_sByteValue;
                        // Read CharValue
            CharSerializer.Instance.Deserialize(out char _local_charValue, buffer, ref offset);
                        allTypesData.CharValue = _local_charValue;
                        // Read Int16Value
            Int16Serializer.Instance.Deserialize(out short _local_int16Value, buffer, ref offset);
                        allTypesData.Int16Value = _local_int16Value;
                        // Read UInt16Value
            UInt16Serializer.Instance.Deserialize(out ushort _local_uInt16Value, buffer, ref offset);
                        allTypesData.UInt16Value = _local_uInt16Value;
                        // Read UInt32Value
            UInt32Serializer.Instance.Deserialize(out uint _local_uInt32Value, buffer, ref offset);
                        allTypesData.UInt32Value = _local_uInt32Value;
                        // Read UInt64Value
            UInt64Serializer.Instance.Deserialize(out ulong _local_uInt64Value, buffer, ref offset);
                        allTypesData.UInt64Value = _local_uInt64Value;
                        // Read DecimalValue
            DecimalSerializer.Instance.Deserialize(out decimal _local_decimalValue, buffer, ref offset);
                        allTypesData.DecimalValue = _local_decimalValue;
                        // Read DateTimeValue
            DateTimeSerializer.Instance.Deserialize(out DateTime _local_dateTimeValue, buffer, ref offset);
                        allTypesData.DateTimeValue = _local_dateTimeValue;
                        // Read TimeSpanValue
            TimeSpanSerializer.Instance.Deserialize(out TimeSpan _local_timeSpanValue, buffer, ref offset);
                        allTypesData.TimeSpanValue = _local_timeSpanValue;
                        // Read GuidValue
            GuidSerializer.Instance.Deserialize(out Guid _local_guidValue, buffer, ref offset);
                        allTypesData.GuidValue = _local_guidValue;
                        // Read EnumValue
            EnumSerializer<TestEnum>.Instance.Deserialize(out TestEnum _local_enumValue, buffer, ref offset);
                        allTypesData.EnumValue = _local_enumValue;

            value = allTypesData;
        }
    }
}