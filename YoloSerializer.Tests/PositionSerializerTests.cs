using System;
using Xunit;
using Xunit.Abstractions;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
    public class PositionSerializerTests
    {
        private readonly ITestOutputHelper _output;
        
        public PositionSerializerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void PositionSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var original = new Position(1.5f, 2.5f, 3.5f);
            var serializer = PositionSerializer.Instance;
            
            // Calculate required buffer size
            int size = serializer.GetSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act - serialize
            serializer.Serialize(original, buffer, ref offset);
            
            // Verify offset advancement
            Assert.Equal(size, offset);
            
            // Reset offset for deserialization
            offset = 0;
            serializer.Deserialize(out Position? result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.X, result.X);
            Assert.Equal(original.Y, result.Y);
            Assert.Equal(original.Z, result.Z);
            Assert.Equal(size, offset);
        }

        [Fact]
        public void PositionSerializer_ShouldThrowOnNull()
        {
            // Arrange
            Position? position = null;
            var serializer = PositionSerializer.Instance;
            var buffer = new byte[100];
            int offset = 0;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => serializer.GetSize(position));
            Assert.Throws<ArgumentNullException>(() => serializer.Serialize(position, buffer, ref offset));
        }

        [Fact]
        public void PositionSerializer_ShouldCalculateCorrectSize()
        {
            // Arrange
            var position = new Position(1.0f, 2.0f, 3.0f);
            var serializer = PositionSerializer.Instance;
            
            // Act
            int size = serializer.GetSize(position);
            
            // Assert - size should be 3 floats (12 bytes)
            Assert.Equal(sizeof(float) * 3, size);
        }

        [Fact]
        public void PositionSerializer_WithYoloSerializer()
        {
            // Arrange
            var original = new Position(1.5f, 2.5f, 3.5f);
            var serializer = YoloGeneratedSerializer.Instance;
            
            // Calculate required buffer size
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act - serialize
            serializer.Serialize(original, buffer, ref offset);
            
            // Verify offset advancement
            Assert.Equal(size, offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = serializer.Deserialize<Position>(buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.X, result!.X);
            Assert.Equal(original.Y, result.Y);
            Assert.Equal(original.Z, result.Z);
            Assert.Equal(size, offset);
        }

        [Fact]
        public void PositionSerializer_WithYoloSerializer_ShouldHandleNull()
        {
            // Arrange
            Position? original = null;
            var serializer = YoloGeneratedSerializer.Instance;
            
            // Calculate required buffer size
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act - serialize
            serializer.Serialize(original, buffer, ref offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = serializer.Deserialize<Position>(buffer, ref offset);

            // Assert
            Assert.Null(result);
            Assert.Equal(size, offset);
        }

        [Fact]
        public void PositionSerializer_ShouldMatchYoloSerializerSize()
        {
            // Arrange
            var position = new Position(1.0f, 2.0f, 3.0f);
            var positionSerializer = PositionSerializer.Instance;
            var yoloSerializer = YoloGeneratedSerializer.Instance;
            
            // Act
            int directSize = positionSerializer.GetSize(position) + sizeof(byte); // Add type ID byte
            int yoloSize = yoloSerializer.GetSerializedSize(position);
            
            // Assert
            Assert.Equal(directSize, yoloSize);
        }
    }
} 