using System;
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
            Int32Serializer.Instance.Serialize(original, buffer, ref offset);
            
            offset = 0;
            Int32Serializer.Instance.Deserialize(out int result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(int), Int32Serializer.Instance.GetSize(original));
        }

        [Fact]
        public void Int64Serializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            long original = 42L;
            byte[] buffer = new byte[sizeof(long)];
            int offset = 0;

            // Act
            Int64Serializer.Instance.Serialize(original, buffer, ref offset);
            
            offset = 0;
            Int64Serializer.Instance.Deserialize(out long result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(long), Int64Serializer.Instance.GetSize(original));
        }

        [Fact]
        public void FloatSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            float original = 42.5f;
            byte[] buffer = new byte[sizeof(float)];
            int offset = 0;

            // Act
            FloatSerializer.Instance.Serialize(original, buffer, ref offset);
            
            offset = 0;
            FloatSerializer.Instance.Deserialize(out float result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(float), FloatSerializer.Instance.GetSize(original));
        }

        [Fact]
        public void DoubleSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            double original = 42.5;
            byte[] buffer = new byte[sizeof(double)];
            int offset = 0;

            // Act
            DoubleSerializer.Instance.Serialize(original, buffer, ref offset);
            
            offset = 0;
            DoubleSerializer.Instance.Deserialize(out double result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(double), DoubleSerializer.Instance.GetSize(original));
        }

        [Fact]
        public void BooleanSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            bool original = true;
            byte[] buffer = new byte[sizeof(byte)];
            int offset = 0;

            // Act
            BooleanSerializer.Instance.Serialize(original, buffer, ref offset);
            
            offset = 0;
            BooleanSerializer.Instance.Deserialize(out bool result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(byte), BooleanSerializer.Instance.GetSize(original));
        }

        [Fact]
        public void StringSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            string original = "Hello, World!";
            int size = StringSerializer.Instance.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            StringSerializer.Instance.Serialize(original, buffer, ref offset);
            
            offset = 0;
            StringSerializer.Instance.Deserialize(out string result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(int) + original.Length * sizeof(char), StringSerializer.Instance.GetSize(original));
        }

        [Fact]
        public void StringSerializer_ShouldHandleNull()
        {
            // Arrange
            string original = null;
            int size = StringSerializer.Instance.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            StringSerializer.Instance.Serialize(original, buffer, ref offset);
            
            offset = 0;
            StringSerializer.Instance.Deserialize(out string result, buffer, ref offset);

            // Assert
            Assert.Null(result);
            Assert.Equal(sizeof(int), StringSerializer.Instance.GetSize(original));
        }

        [Fact]
        public void ArraySerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            int[] original = new[] { 1, 2, 3, 4, 5 };
            var serializer = new ArraySerializer<int>(Int32Serializer.Instance);
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out int[] result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            Assert.Equal(sizeof(int) + original.Length * sizeof(int), serializer.GetSize(original));
        }

        [Fact]
        public void ArraySerializer_ShouldHandleNull()
        {
            // Arrange
            int[] original = null;
            var serializer = new ArraySerializer<int>(Int32Serializer.Instance);
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out int[] result, buffer, ref offset);

            // Assert
            Assert.Null(result);
            Assert.Equal(sizeof(int), serializer.GetSize(original));
        }
    }
} 