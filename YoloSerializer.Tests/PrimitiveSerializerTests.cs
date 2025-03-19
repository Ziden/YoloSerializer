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
    }
} 