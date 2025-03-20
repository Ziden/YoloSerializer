using System;
using Xunit;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
    public class AllTypesDataSerializerTests
    {
        [Fact]
        public void AllTypesDataSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var original = new AllTypesData(
                int32Value: 42,
                int64Value: 42L,
                floatValue: 42.5f,
                doubleValue: 42.5,
                boolValue: true,
                stringValue: "Hello, World!",
                byteValue: 255,
                sByteValue: -42,
                charValue: 'A',
                int16Value: 42,
                uInt16Value: 42,
                uInt32Value: 42,
                uInt64Value: 42,
                decimalValue: 42.5m,
                dateTimeValue: new DateTime(2023, 1, 1),
                timeSpanValue: TimeSpan.FromHours(1),
                guidValue: Guid.NewGuid(),
                enumValue: TestEnum.Value2
            );
            
            var serializer = AllTypesDataSerializer.Instance;
            
            // Get size and create buffer
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;
            
            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out AllTypesData result, buffer, ref offset);
            
            // Assert
            Assert.Equal(original, result);
            
            // Verify each field individually for better diagnostics
            Assert.Equal(original.Int32Value, result.Int32Value);
            Assert.Equal(original.Int64Value, result.Int64Value);
            Assert.Equal(original.FloatValue, result.FloatValue);
            Assert.Equal(original.DoubleValue, result.DoubleValue);
            Assert.Equal(original.BoolValue, result.BoolValue);
            Assert.Equal(original.StringValue, result.StringValue);
            Assert.Equal(original.ByteValue, result.ByteValue);
            Assert.Equal(original.SByteValue, result.SByteValue);
            Assert.Equal(original.CharValue, result.CharValue);
            Assert.Equal(original.Int16Value, result.Int16Value);
            Assert.Equal(original.UInt16Value, result.UInt16Value);
            Assert.Equal(original.UInt32Value, result.UInt32Value);
            Assert.Equal(original.UInt64Value, result.UInt64Value);
            Assert.Equal(original.DecimalValue, result.DecimalValue);
            Assert.Equal(original.DateTimeValue, result.DateTimeValue);
            Assert.Equal(original.TimeSpanValue, result.TimeSpanValue);
            Assert.Equal(original.GuidValue, result.GuidValue);
            Assert.Equal(original.EnumValue, result.EnumValue);
        }
        
        [Fact]
        public void AllTypesDataSerializer_ShouldHandleNullString()
        {
            // Arrange
            var original = new AllTypesData(
                int32Value: 42,
                int64Value: 42L,
                floatValue: 42.5f,
                doubleValue: 42.5,
                boolValue: true,
                stringValue: null,
                byteValue: 255,
                sByteValue: -42,
                charValue: 'A',
                int16Value: 42,
                uInt16Value: 42,
                uInt32Value: 42,
                uInt64Value: 42,
                decimalValue: 42.5m,
                dateTimeValue: new DateTime(2023, 1, 1),
                timeSpanValue: TimeSpan.FromHours(1),
                guidValue: Guid.NewGuid(),
                enumValue: TestEnum.Value2
            );
            
            var serializer = AllTypesDataSerializer.Instance;
            
            // Get size and create buffer
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;
            
            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out AllTypesData result, buffer, ref offset);
            
            // Assert
            Assert.Equal(original, result);
            Assert.Null(result.StringValue);
        }
        
        [Fact]
        public void AllTypesDataSerializer_ShouldHandleSpecialValues()
        {
            // Arrange
            var original = new AllTypesData(
                int32Value: int.MinValue,
                int64Value: long.MinValue,
                floatValue: float.NaN,
                doubleValue: double.PositiveInfinity,
                boolValue: false,
                stringValue: string.Empty,
                byteValue: byte.MaxValue,
                sByteValue: sbyte.MinValue,
                charValue: char.MaxValue,
                int16Value: short.MinValue,
                uInt16Value: ushort.MaxValue,
                uInt32Value: uint.MaxValue,
                uInt64Value: ulong.MaxValue,
                decimalValue: decimal.MinValue,
                dateTimeValue: DateTime.MinValue,
                timeSpanValue: TimeSpan.MinValue,
                guidValue: Guid.Empty,
                enumValue: TestEnum.Value3
            );
            
            var serializer = AllTypesDataSerializer.Instance;
            
            // Get size and create buffer
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;
            
            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out var result, buffer, ref offset);
            
            // Assert - Compare individual values instead of reference comparison
            Assert.Equal(int.MinValue, result.Int32Value);
            Assert.Equal(long.MinValue, result.Int64Value);
            Assert.True(float.IsNaN(result.FloatValue));
            Assert.True(double.IsPositiveInfinity(result.DoubleValue));
            Assert.False(result.BoolValue);
            Assert.Equal(string.Empty, result.StringValue);
            Assert.Equal(byte.MaxValue, result.ByteValue);
            Assert.Equal(sbyte.MinValue, result.SByteValue);
            Assert.Equal(char.MaxValue, result.CharValue);
            Assert.Equal(short.MinValue, result.Int16Value);
            Assert.Equal(ushort.MaxValue, result.UInt16Value);
            Assert.Equal(uint.MaxValue, result.UInt32Value);
            Assert.Equal(ulong.MaxValue, result.UInt64Value);
            Assert.Equal(decimal.MinValue, result.DecimalValue);
            Assert.Equal(DateTime.MinValue, result.DateTimeValue);
            Assert.Equal(TimeSpan.MinValue, result.TimeSpanValue);
            Assert.Equal(Guid.Empty, result.GuidValue);
            Assert.Equal(TestEnum.Value3, result.EnumValue);
        }
    }
}