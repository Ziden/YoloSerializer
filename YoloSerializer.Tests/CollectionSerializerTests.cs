using System;
using System.Collections.Generic;
using Xunit;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
    public class CollectionSerializerTests
    {
        [Fact]
        public void ListSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var original = new List<int> { 1, 2, 3, 4, 5 };
            var serializer = new ListSerializer<int>(Int32Serializer.Instance);
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out List<int>? result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Count, result!.Count);
            for (int i = 0; i < original.Count; i++)
            {
                Assert.Equal(original[i], result[i]);
            }
        }

        [Fact]
        public void ListSerializer_ShouldHandleNull()
        {
            // Arrange
            List<int>? original = null;
            var serializer = new ListSerializer<int>(Int32Serializer.Instance);
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out List<int>? result, buffer, ref offset);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ListSerializer_ShouldHandleNestedLists()
        {
            // Arrange
            var original = new List<List<int>>
            {
                new List<int> { 1, 2, 3 },
                new List<int> { 4, 5, 6 },
                new List<int> { 7, 8, 9 }
            };

            var elementSerializer = new ListSerializer<int>(Int32Serializer.Instance);
            var serializer = new ListSerializer<List<int>>(elementSerializer);
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out List<List<int>>? result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Count, result!.Count);
            for (int i = 0; i < original.Count; i++)
            {
                Assert.Equal(original[i].Count, result[i].Count);
                for (int j = 0; j < original[i].Count; j++)
                {
                    Assert.Equal(original[i][j], result[i][j]);
                }
            }
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

            var serializer = new DictionarySerializer<string, int>(StringSerializer.Instance, Int32Serializer.Instance);
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out Dictionary<string, int>? result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Count, result!.Count);
            foreach (var kvp in original)
            {
                Assert.True(result.ContainsKey(kvp.Key));
                Assert.Equal(kvp.Value, result[kvp.Key]);
            }
        }

        [Fact]
        public void DictionarySerializer_ShouldHandleNull()
        {
            // Arrange
            Dictionary<string, int>? original = null;
            var serializer = new DictionarySerializer<string, int>(StringSerializer.Instance, Int32Serializer.Instance);
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out Dictionary<string, int>? result, buffer, ref offset);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DictionarySerializer_ShouldHandleNestedDictionaries()
        {
            // Arrange
            var original = new Dictionary<string, Dictionary<int, string>>
            {
                { "first", new Dictionary<int, string> { { 1, "one" }, { 2, "two" } } },
                { "second", new Dictionary<int, string> { { 3, "three" }, { 4, "four" } } }
            };

            var valueSerializer = new DictionarySerializer<int, string>(Int32Serializer.Instance, StringSerializer.Instance);
            var serializer = new DictionarySerializer<string, Dictionary<int, string>>(StringSerializer.Instance, valueSerializer);
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            offset = 0;
            serializer.Deserialize(out Dictionary<string, Dictionary<int, string>>? result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Count, result!.Count);
            foreach (var kvp in original)
            {
                Assert.True(result.ContainsKey(kvp.Key));
                var innerDict = result[kvp.Key];
                Assert.Equal(kvp.Value.Count, innerDict.Count);
                foreach (var innerKvp in kvp.Value)
                {
                    Assert.True(innerDict.ContainsKey(innerKvp.Key));
                    Assert.Equal(innerKvp.Value, innerDict[innerKvp.Key]);
                }
            }
        }
    }
} 