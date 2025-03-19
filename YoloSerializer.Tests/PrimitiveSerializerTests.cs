using System;
using System.Text;
using Xunit;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
    public class PrimitiveSerializerTests
    {
        [Fact]
        public void Int32Serializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            int original = 42;
            byte[] buffer = new byte[sizeof(int)];
            int offset = 0;
            var serializer = Int32Serializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out int result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(int), serializer.GetSize(original));
        }

        [Fact]
        public void Int64Serializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            long original = 42L;
            byte[] buffer = new byte[sizeof(long)];
            int offset = 0;
            var serializer = Int64Serializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out long result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(long), serializer.GetSize(original));
        }

        [Fact]
        public void FloatSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            float original = 42.5f;
            byte[] buffer = new byte[sizeof(float)];
            int offset = 0;
            var serializer = FloatSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out float result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(float), serializer.GetSize(original));
        }

        [Fact]
        public void DoubleSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            double original = 42.5;
            byte[] buffer = new byte[sizeof(double)];
            int offset = 0;
            var serializer = DoubleSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out double result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(double), serializer.GetSize(original));
        }

        [Fact]
        public void BooleanSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            bool original = true;
            byte[] buffer = new byte[sizeof(byte)];
            int offset = 0;
            var serializer = BooleanSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out bool result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(byte), serializer.GetSize(original));
        }

        [Fact]
        public void StringSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            string original = "Hello, World!";
            var serializer = StringSerializer.Instance;
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out string result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(int) + Encoding.UTF8.GetByteCount(original), serializer.GetSize(original));
        }

        [Fact]
        public void StringSerializer_ShouldHandleNull()
        {
            // Arrange
            string original = null;
            var serializer = StringSerializer.Instance;
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out string result, buffer, ref offset);

            // Assert
            Assert.Null(result);
            Assert.Equal(sizeof(int), serializer.GetSize(original));
        }

        [Fact]
        public void ByteSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            byte original = 42;
            byte[] buffer = new byte[sizeof(byte)];
            int offset = 0;
            var serializer = ByteSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out byte result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(byte), serializer.GetSize(original));
        }

        [Fact]
        public void SByteSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            sbyte original = -42;
            byte[] buffer = new byte[sizeof(sbyte)];
            int offset = 0;
            var serializer = SByteSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out sbyte result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(sbyte), serializer.GetSize(original));
        }

        [Fact]
        public void CharSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            char original = 'A';
            byte[] buffer = new byte[sizeof(char)];
            int offset = 0;
            var serializer = CharSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out char result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(char), serializer.GetSize(original));
        }

        [Fact]
        public void Int16Serializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            short original = 42;
            byte[] buffer = new byte[sizeof(short)];
            int offset = 0;
            var serializer = Int16Serializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out short result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(short), serializer.GetSize(original));
        }

        [Fact]
        public void UInt16Serializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            ushort original = 42;
            byte[] buffer = new byte[sizeof(ushort)];
            int offset = 0;
            var serializer = UInt16Serializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out ushort result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(ushort), serializer.GetSize(original));
        }

        [Fact]
        public void UInt32Serializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            uint original = 42;
            byte[] buffer = new byte[sizeof(uint)];
            int offset = 0;
            var serializer = UInt32Serializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out uint result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(uint), serializer.GetSize(original));
        }

        [Fact]
        public void UInt64Serializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            ulong original = 42;
            byte[] buffer = new byte[sizeof(ulong)];
            int offset = 0;
            var serializer = UInt64Serializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out ulong result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(ulong), serializer.GetSize(original));
        }

        [Fact]
        public void DecimalSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            decimal original = 42.5m;
            byte[] buffer = new byte[sizeof(decimal)];
            int offset = 0;
            var serializer = DecimalSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out decimal result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(decimal), serializer.GetSize(original));
        }

        [Fact]
        public void DateTimeSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            DateTime original = new DateTime(2023, 1, 1);
            byte[] buffer = new byte[sizeof(long)];
            int offset = 0;
            var serializer = DateTimeSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out DateTime result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(long), serializer.GetSize(original));
        }

        [Fact]
        public void TimeSpanSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            TimeSpan original = TimeSpan.FromHours(1);
            byte[] buffer = new byte[sizeof(long)];
            int offset = 0;
            var serializer = TimeSpanSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out TimeSpan result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(long), serializer.GetSize(original));
        }

        [Fact]
        public void GuidSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            Guid original = Guid.NewGuid();
            byte[] buffer = new byte[16];
            int offset = 0;
            var serializer = GuidSerializer.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out Guid result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(16, serializer.GetSize(original));
        }

        [Fact]
        public void EnumSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            TestEnum original = TestEnum.Value2;
            byte[] buffer = new byte[sizeof(int)];
            int offset = 0;
            var serializer = EnumSerializer<TestEnum>.Instance;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out TestEnum result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(int), serializer.GetSize(original));
        }

        // Note: ArraySerializer tests need to be updated to use the new approach
        // These tests are commented out until they can be properly implemented
        /*
        [Fact]
        public void ArraySerializer_ShouldSerializeAndDeserialize()
        {
            // To be implemented with the instance-based API
        }

        [Fact]
        public void ArraySerializer_ShouldHandleNull()
        {
            // To be implemented with the instance-based API
        }
        */

        // Helper enum for testing
        private enum TestEnum
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
    }
} 