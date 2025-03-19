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

            // Act
            Int32Serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            Int32Serializer.Deserialize(out int result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(int), Int32Serializer.GetSize(original));
        }

        [Fact]
        public void Int64Serializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            long original = 42L;
            byte[] buffer = new byte[sizeof(long)];
            int offset = 0;

            // Act
            Int64Serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            Int64Serializer.Deserialize(out long result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(long), Int64Serializer.GetSize(original));
        }

        [Fact]
        public void FloatSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            float original = 42.5f;
            byte[] buffer = new byte[sizeof(float)];
            int offset = 0;

            // Act
            FloatSerializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            FloatSerializer.Deserialize(out float result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(float), FloatSerializer.GetSize(original));
        }

        [Fact]
        public void DoubleSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            double original = 42.5;
            byte[] buffer = new byte[sizeof(double)];
            int offset = 0;

            // Act
            DoubleSerializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            DoubleSerializer.Deserialize(out double result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(double), DoubleSerializer.GetSize(original));
        }

        [Fact]
        public void BooleanSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            bool original = true;
            byte[] buffer = new byte[sizeof(byte)];
            int offset = 0;

            // Act
            BooleanSerializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            BooleanSerializer.Deserialize(out bool result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(byte), BooleanSerializer.GetSize(original));
        }

        [Fact]
        public void StringSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            string original = "Hello, World!";
            int size = StringSerializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            StringSerializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            StringSerializer.Deserialize(out string result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(int) + Encoding.UTF8.GetByteCount(original), StringSerializer.GetSize(original));
        }

        [Fact]
        public void StringSerializer_ShouldHandleNull()
        {
            // Arrange
            string original = null;
            int size = StringSerializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            StringSerializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            StringSerializer.Deserialize(out string result, buffer, ref offset);

            // Assert
            Assert.Null(result);
            Assert.Equal(sizeof(int), StringSerializer.GetSize(original));
        }

        // Note: ArraySerializer tests need to be updated to use the new generic approach
        // These tests are commented out until they can be properly implemented
        /*
        [Fact]
        public void ArraySerializer_ShouldSerializeAndDeserialize()
        {
            // To be implemented with new static API
        }

        [Fact]
        public void ArraySerializer_ShouldHandleNull()
        {
            // To be implemented with new static API
        }
        */
    }
} 