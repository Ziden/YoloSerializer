using System;
using System.Collections.Generic;
using Xunit;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Tests
{
    [Collection("Sequential")]
    // Create custom serializer classes for testing that implement the required interfaces
    // and provide parameterless constructors that the ArraySerializer and DictionarySerializer need
    public class TestInt32Serializer : ISerializer<int>
    {
        public void Serialize(int value, Span<byte> buffer, ref int offset)
        {
            Int32Serializer.Instance.Serialize(value, buffer, ref offset);
        }

        public void Deserialize(out int value, ReadOnlySpan<byte> buffer, ref int offset)
        {
            Int32Serializer.Instance.Deserialize(out value, buffer, ref offset);
        }

        public int GetSize(int value)
        {
            return Int32Serializer.Instance.GetSize(value);
        }
    }

    public class TestStringSerializer : ISerializer<string>
    {
        public void Serialize(string value, Span<byte> buffer, ref int offset)
        {
            StringSerializer.Instance.Serialize(value, buffer, ref offset);
        }

        public void Deserialize(out string value, ReadOnlySpan<byte> buffer, ref int offset)
        {
            StringSerializer.Instance.Deserialize(out value, buffer, ref offset);
        }

        public int GetSize(string value)
        {
            return StringSerializer.Instance.GetSize(value);
        }
    }

    public class CollectionSerializerTests
    {
        [Fact]
        public void ArraySerializer_ShouldSerializeAndDeserializeIntArray()
        {
            // Arrange
            int[] original = new int[] { 1, 2, 3, 4, 5 };
            int size = ArraySerializer<int>.GetSize<TestInt32Serializer>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            ArraySerializer<int>.Serialize<TestInt32Serializer>(original, buffer, ref offset);
            
            offset = 0;
            ArraySerializer<int>.Deserialize<TestInt32Serializer>(out int[] result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Length, result.Length);
            for (int i = 0; i < original.Length; i++)
            {
                Assert.Equal(original[i], result[i]);
            }
        }

        [Fact]
        public void ArraySerializer_ShouldHandleEmptyArray()
        {
            // Arrange
            int[] original = Array.Empty<int>();
            int size = ArraySerializer<int>.GetSize<TestInt32Serializer>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            ArraySerializer<int>.Serialize<TestInt32Serializer>(original, buffer, ref offset);
            
            offset = 0;
            ArraySerializer<int>.Deserialize<TestInt32Serializer>(out int[] result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ArraySerializer_ShouldHandleNull()
        {
            // Arrange
            int[]? original = null;
            int size = ArraySerializer<int>.GetSize<TestInt32Serializer>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            ArraySerializer<int>.Serialize<TestInt32Serializer>(original, buffer, ref offset);
            
            offset = 0;
            ArraySerializer<int>.Deserialize<TestInt32Serializer>(out int[]? result, buffer, ref offset);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DictionarySerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var original = new Dictionary<string, int>
            {
                { "one", 1 },
                { "two", 2 },
                { "three", 3 }
            };
            int size = DictionarySerializer<string, int>.GetSize<TestStringSerializer, TestInt32Serializer>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            DictionarySerializer<string, int>.Serialize<TestStringSerializer, TestInt32Serializer>(original, buffer, ref offset);
            
            offset = 0;
            DictionarySerializer<string, int>.Deserialize<TestStringSerializer, TestInt32Serializer>(out Dictionary<string, int> result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Count, result.Count);
            foreach (var key in original.Keys)
            {
                Assert.True(result.ContainsKey(key));
                Assert.Equal(original[key], result[key]);
            }
        }

        [Fact]
        public void DictionarySerializer_ShouldHandleEmptyDictionary()
        {
            // Arrange
            var original = new Dictionary<string, int>();
            int size = DictionarySerializer<string, int>.GetSize<TestStringSerializer, TestInt32Serializer>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            DictionarySerializer<string, int>.Serialize<TestStringSerializer, TestInt32Serializer>(original, buffer, ref offset);
            
            offset = 0;
            DictionarySerializer<string, int>.Deserialize<TestStringSerializer, TestInt32Serializer>(out Dictionary<string, int> result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void DictionarySerializer_ShouldHandleNull()
        {
            // Arrange
            Dictionary<string, int>? original = null;
            int size = DictionarySerializer<string, int>.GetSize<TestStringSerializer, TestInt32Serializer>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            DictionarySerializer<string, int>.Serialize<TestStringSerializer, TestInt32Serializer>(original, buffer, ref offset);
            
            offset = 0;
            DictionarySerializer<string, int>.Deserialize<TestStringSerializer, TestInt32Serializer>(out Dictionary<string, int>? result, buffer, ref offset);

            // Assert
            Assert.Null(result);
        }
    }
} 