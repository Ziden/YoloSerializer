using System;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Models
{
    /// <summary>
    /// Test model that contains all supported primitive types
    /// </summary>
    public class AllTypesData
    {
        // Original primitive types
        public int Int32Value { get; set; }
        public long Int64Value { get; set; }
        public float FloatValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }
        public string? StringValue { get; set; }
        
        // Newly added primitive types
        public byte ByteValue { get; set; }
        public sbyte SByteValue { get; set; }
        public char CharValue { get; set; }
        public short Int16Value { get; set; }
        public ushort UInt16Value { get; set; }
        public uint UInt32Value { get; set; }
        public ulong UInt64Value { get; set; }
        public decimal DecimalValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public TimeSpan TimeSpanValue { get; set; }
        public Guid GuidValue { get; set; }
        public TestEnum EnumValue { get; set; }
        
        public AllTypesData() { }
        
        public AllTypesData(
            int int32Value, 
            long int64Value, 
            float floatValue, 
            double doubleValue, 
            bool boolValue, 
            string? stringValue, 
            byte byteValue, 
            sbyte sByteValue, 
            char charValue, 
            short int16Value, 
            ushort uInt16Value, 
            uint uInt32Value, 
            ulong uInt64Value, 
            decimal decimalValue, 
            DateTime dateTimeValue, 
            TimeSpan timeSpanValue, 
            Guid guidValue, 
            TestEnum enumValue)
        {
            Int32Value = int32Value;
            Int64Value = int64Value;
            FloatValue = floatValue;
            DoubleValue = doubleValue;
            BoolValue = boolValue;
            StringValue = stringValue;
            ByteValue = byteValue;
            SByteValue = sByteValue;
            CharValue = charValue;
            Int16Value = int16Value;
            UInt16Value = uInt16Value;
            UInt32Value = uInt32Value;
            UInt64Value = uInt64Value;
            DecimalValue = decimalValue;
            DateTimeValue = dateTimeValue;
            TimeSpanValue = timeSpanValue;
            GuidValue = guidValue;
            EnumValue = enumValue;
        }
        
        public override bool Equals(object? obj)
        {
            if (obj is AllTypesData other)
            {
                return 
                    Int32Value == other.Int32Value &&
                    Int64Value == other.Int64Value &&
                    FloatValue == other.FloatValue &&
                    DoubleValue == other.DoubleValue &&
                    BoolValue == other.BoolValue &&
                    StringValue == other.StringValue &&
                    ByteValue == other.ByteValue &&
                    SByteValue == other.SByteValue &&
                    CharValue == other.CharValue &&
                    Int16Value == other.Int16Value &&
                    UInt16Value == other.UInt16Value &&
                    UInt32Value == other.UInt32Value &&
                    UInt64Value == other.UInt64Value &&
                    DecimalValue == other.DecimalValue &&
                    DateTimeValue == other.DateTimeValue &&
                    TimeSpanValue == other.TimeSpanValue &&
                    GuidValue == other.GuidValue &&
                    EnumValue == other.EnumValue;
            }
            return false;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(
                Int32Value,
                Int64Value,
                FloatValue,
                DoubleValue,
                BoolValue,
                StringValue,
                HashCode.Combine(
                    ByteValue,
                    SByteValue,
                    CharValue,
                    Int16Value,
                    UInt16Value,
                    UInt32Value,
                    UInt64Value,
                    HashCode.Combine(
                        DecimalValue,
                        DateTimeValue,
                        TimeSpanValue,
                        GuidValue,
                        EnumValue
                    )
                )
            );
        }
    }
    
    public enum TestEnum
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }
} 