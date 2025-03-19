using System;
using System.Collections.Generic;
using Xunit;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Tests
{
    public class CollectionSerializerTests
    {
        // Helper serializer adapters
        private class IntSerializerAdapter : ISerializer<int>
        {
            public void Serialize(int value, Span<byte> span, ref int offset)
            {
                Int32Serializer.Serialize(value, span, ref offset);
            }
            
            public void Deserialize(out int value, ReadOnlySpan<byte> span, ref int offset)
            {
                Int32Serializer.Deserialize(out value, span, ref offset);
            }
            
            public int GetSize(int value)
            {
                return Int32Serializer.GetSize(value);
            }
        }
        
        private class StringSerializerAdapter : ISerializer<string?>
        {
            public void Serialize(string? value, Span<byte> span, ref int offset)
            {
                StringSerializer.Serialize(value, span, ref offset);
            }
            
            public void Deserialize(out string? value, ReadOnlySpan<byte> span, ref int offset)
            {
                StringSerializer.Deserialize(out value, span, ref offset);
            }
            
            public int GetSize(string? value)
            {
                return StringSerializer.GetSize(value);
            }
        }
        
        private class ListIntSerializerAdapter : ISerializer<List<int>>
        {
            public void Serialize(List<int> value, Span<byte> span, ref int offset)
            {
                ListSerializer<int>.Serialize<IntSerializerAdapter>(value, span, ref offset);
            }
            
            public void Deserialize(out List<int> value, ReadOnlySpan<byte> span, ref int offset)
            {
                ListSerializer<int>.Deserialize<IntSerializerAdapter>(out value, span, ref offset);
            }
            
            public int GetSize(List<int> value)
            {
                return ListSerializer<int>.GetSize<IntSerializerAdapter>(value);
            }
        }
        
        private class DictIntStringSerializerAdapter : ISerializer<Dictionary<int, string>>
        {
            public void Serialize(Dictionary<int, string> value, Span<byte> span, ref int offset)
            {
                DictionarySerializer<int, string>.Serialize<IntSerializerAdapter, StringSerializerAdapter>(
                    value, span, ref offset);
            }
            
            public void Deserialize(out Dictionary<int, string> value, ReadOnlySpan<byte> span, ref int offset)
            {
                DictionarySerializer<int, string>.Deserialize<IntSerializerAdapter, StringSerializerAdapter>(
                    out value, span, ref offset);
            }
            
            public int GetSize(Dictionary<int, string> value)
            {
                return DictionarySerializer<int, string>.GetSize<IntSerializerAdapter, StringSerializerAdapter>(value);
            }
        }
        
        [Fact]
        public void ArraySerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            int[] original = new[] { 1, 2, 3, 4, 5 };
            int size = ArraySerializer<int>.GetSize<IntSerializerAdapter>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            ArraySerializer<int>.Serialize<IntSerializerAdapter>(original, buffer, ref offset);
            
            offset = 0;
            ArraySerializer<int>.Deserialize<IntSerializerAdapter>(out int[]? result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Length, result!.Length);
            for (int i = 0; i < original.Length; i++)
            {
                Assert.Equal(original[i], result[i]);
            }
        }
        
        [Fact]
        public void ArraySerializer_ShouldHandleNull()
        {
            // Arrange
            int[]? original = null;
            int size = ArraySerializer<int>.GetSize<IntSerializerAdapter>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            ArraySerializer<int>.Serialize<IntSerializerAdapter>(original, buffer, ref offset);
            
            offset = 0;
            ArraySerializer<int>.Deserialize<IntSerializerAdapter>(out int[]? result, buffer, ref offset);

            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public void ListSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var original = new List<int> { 1, 2, 3, 4, 5 };
            int size = ListSerializer<int>.GetSize<IntSerializerAdapter>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            ListSerializer<int>.Serialize<IntSerializerAdapter>(original, buffer, ref offset);
            
            offset = 0;
            ListSerializer<int>.Deserialize<IntSerializerAdapter>(out List<int>? result, buffer, ref offset);

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
            int size = ListSerializer<int>.GetSize<IntSerializerAdapter>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            ListSerializer<int>.Serialize<IntSerializerAdapter>(original, buffer, ref offset);
            
            offset = 0;
            ListSerializer<int>.Deserialize<IntSerializerAdapter>(out List<int>? result, buffer, ref offset);

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

            int size = ListSerializer<List<int>>.GetSize<ListIntSerializerAdapter>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            ListSerializer<List<int>>.Serialize<ListIntSerializerAdapter>(original, buffer, ref offset);
            
            offset = 0;
            ListSerializer<List<int>>.Deserialize<ListIntSerializerAdapter>(out List<List<int>>? result, buffer, ref offset);

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

            int size = DictionarySerializer<string, int>.GetSize<StringSerializerAdapter, IntSerializerAdapter>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            DictionarySerializer<string, int>.Serialize<StringSerializerAdapter, IntSerializerAdapter>(
                original, buffer, ref offset);
            
            offset = 0;
            DictionarySerializer<string, int>.Deserialize<StringSerializerAdapter, IntSerializerAdapter>(
                out Dictionary<string, int>? result, buffer, ref offset);

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
            int size = DictionarySerializer<string, int>.GetSize<StringSerializerAdapter, IntSerializerAdapter>(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            DictionarySerializer<string, int>.Serialize<StringSerializerAdapter, IntSerializerAdapter>(
                original, buffer, ref offset);
            
            offset = 0;
            DictionarySerializer<string, int>.Deserialize<StringSerializerAdapter, IntSerializerAdapter>(
                out Dictionary<string, int>? result, buffer, ref offset);

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

            // Create the test
            var adapter = new DictionarySerializerNestedAdapter();
            int size = adapter.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            adapter.Serialize(original, buffer, ref offset);
            
            offset = 0;
            adapter.Deserialize(out Dictionary<string, Dictionary<int, string>>? result, buffer, ref offset);

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
        
        // Helper class for nested dictionary serialization
        private class DictionarySerializerNestedAdapter : ISerializer<Dictionary<string, Dictionary<int, string>>>
        {
            public void Serialize(Dictionary<string, Dictionary<int, string>> value, Span<byte> span, ref int offset)
            {
                DictionarySerializer<string, Dictionary<int, string>>.Serialize<StringSerializerAdapter, DictIntStringSerializerAdapter>(
                    value, span, ref offset);
            }
            
            public void Deserialize(out Dictionary<string, Dictionary<int, string>> value, ReadOnlySpan<byte> span, ref int offset)
            {
                DictionarySerializer<string, Dictionary<int, string>>.Deserialize<StringSerializerAdapter, DictIntStringSerializerAdapter>(
                    out value, span, ref offset);
            }
            
            public int GetSize(Dictionary<string, Dictionary<int, string>> value)
            {
                return DictionarySerializer<string, Dictionary<int, string>>.GetSize<StringSerializerAdapter, DictIntStringSerializerAdapter>(value);
            }
        }
    }
} 