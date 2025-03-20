using System;
using System.Collections.Generic;
using Xunit;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
    public class AllCollectionsDataSerializerTests
    {
        [Fact]
        public void AllCollectionsDataSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var original = new AllCollectionsData();
            
            // Populate collection properties
            original.IntListProperty.AddRange(new[] { 1, 2, 3, 4, 5 });
            original.StringListProperty.AddRange(new[] { "one", "two", "three" });
            original.IntStringDictProperty.Add(1, "one");
            original.IntStringDictProperty.Add(2, "two");
            original.StringIntDictProperty.Add("one", 1);
            original.StringIntDictProperty.Add("two", 2);
            original.IntArrayProperty = new[] { 10, 20, 30 };
            original.StringArrayProperty = new[] { "ten", "twenty", "thirty" };
            original.EnumListProperty.AddRange(new[] { TestEnum.Value1, TestEnum.Value2, TestEnum.Value3 });
            
            // Populate collection fields
            original.IntListField.AddRange(new[] { 6, 7, 8, 9, 10 });
            original.StringListField.AddRange(new[] { "four", "five", "six" });
            original.IntStringDictField.Add(3, "three");
            original.IntStringDictField.Add(4, "four");
            original.StringIntDictField.Add("three", 3);
            original.StringIntDictField.Add("four", 4);
            original.IntArrayField = new[] { 40, 50, 60 };
            original.StringArrayField = new[] { "forty", "fifty", "sixty" };
            original.EnumListField.AddRange(new[] { TestEnum.Value3, TestEnum.Value2, TestEnum.Value1 });

            var serializer = AllCollectionsDataSerializer.Instance;

            // Get size and create buffer
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);

            offset = 0;
            serializer.Deserialize(out AllCollectionsData result, buffer, ref offset);

            // Assert
            Assert.Equal(original, result);
            
            // Verify properties individually
            Assert.Equal(original.IntListProperty.Count, result.IntListProperty.Count);
            for (int i = 0; i < original.IntListProperty.Count; i++)
            {
                Assert.Equal(original.IntListProperty[i], result.IntListProperty[i]);
            }
            
            Assert.Equal(original.StringListProperty.Count, result.StringListProperty.Count);
            for (int i = 0; i < original.StringListProperty.Count; i++)
            {
                Assert.Equal(original.StringListProperty[i], result.StringListProperty[i]);
            }
            
            Assert.Equal(original.IntStringDictProperty.Count, result.IntStringDictProperty.Count);
            foreach (var key in original.IntStringDictProperty.Keys)
            {
                Assert.True(result.IntStringDictProperty.ContainsKey(key));
                Assert.Equal(original.IntStringDictProperty[key], result.IntStringDictProperty[key]);
            }
            
            Assert.Equal(original.StringIntDictProperty.Count, result.StringIntDictProperty.Count);
            foreach (var key in original.StringIntDictProperty.Keys)
            {
                Assert.True(result.StringIntDictProperty.ContainsKey(key));
                Assert.Equal(original.StringIntDictProperty[key], result.StringIntDictProperty[key]);
            }
            
            Assert.Equal(original.IntArrayProperty.Length, result.IntArrayProperty.Length);
            for (int i = 0; i < original.IntArrayProperty.Length; i++)
            {
                Assert.Equal(original.IntArrayProperty[i], result.IntArrayProperty[i]);
            }
            
            Assert.Equal(original.StringArrayProperty.Length, result.StringArrayProperty.Length);
            for (int i = 0; i < original.StringArrayProperty.Length; i++)
            {
                Assert.Equal(original.StringArrayProperty[i], result.StringArrayProperty[i]);
            }
            
            Assert.Equal(original.EnumListProperty.Count, result.EnumListProperty.Count);
            for (int i = 0; i < original.EnumListProperty.Count; i++)
            {
                Assert.Equal(original.EnumListProperty[i], result.EnumListProperty[i]);
            }
            
            // Verify fields individually
            Assert.Equal(original.IntListField.Count, result.IntListField.Count);
            for (int i = 0; i < original.IntListField.Count; i++)
            {
                Assert.Equal(original.IntListField[i], result.IntListField[i]);
            }
            
            Assert.Equal(original.StringListField.Count, result.StringListField.Count);
            for (int i = 0; i < original.StringListField.Count; i++)
            {
                Assert.Equal(original.StringListField[i], result.StringListField[i]);
            }
            
            Assert.Equal(original.IntStringDictField.Count, result.IntStringDictField.Count);
            foreach (var key in original.IntStringDictField.Keys)
            {
                Assert.True(result.IntStringDictField.ContainsKey(key));
                Assert.Equal(original.IntStringDictField[key], result.IntStringDictField[key]);
            }
            
            Assert.Equal(original.StringIntDictField.Count, result.StringIntDictField.Count);
            foreach (var key in original.StringIntDictField.Keys)
            {
                Assert.True(result.StringIntDictField.ContainsKey(key));
                Assert.Equal(original.StringIntDictField[key], result.StringIntDictField[key]);
            }
            
            Assert.Equal(original.IntArrayField.Length, result.IntArrayField.Length);
            for (int i = 0; i < original.IntArrayField.Length; i++)
            {
                Assert.Equal(original.IntArrayField[i], result.IntArrayField[i]);
            }
            
            Assert.Equal(original.StringArrayField.Length, result.StringArrayField.Length);
            for (int i = 0; i < original.StringArrayField.Length; i++)
            {
                Assert.Equal(original.StringArrayField[i], result.StringArrayField[i]);
            }
            
            Assert.Equal(original.EnumListField.Count, result.EnumListField.Count);
            for (int i = 0; i < original.EnumListField.Count; i++)
            {
                Assert.Equal(original.EnumListField[i], result.EnumListField[i]);
            }
        }
        
        [Fact]
        public void AllCollectionsDataSerializer_ShouldHandleNullCollections()
        {
            // Arrange
            var original = new AllCollectionsData();
            
            // Set some collections to null (both properties and fields)
            original.IntListProperty = null;
            original.StringArrayProperty = null;
            original.IntStringDictProperty = null;
            original.IntListField = null;
            original.StringArrayField = null;
            original.IntStringDictField = null;

            var serializer = AllCollectionsDataSerializer.Instance;

            // Get size and create buffer
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);

            offset = 0;
            serializer.Deserialize(out AllCollectionsData result, buffer, ref offset);

            // Assert
            Assert.Null(result.IntListProperty);
            Assert.Null(result.StringArrayProperty);
            Assert.Null(result.IntStringDictProperty);
            Assert.Null(result.IntListField);
            Assert.Null(result.StringArrayField);
            Assert.Null(result.IntStringDictField);
            
            // Non-null collections should still be intact
            Assert.NotNull(result.StringListProperty);
            Assert.NotNull(result.StringIntDictProperty);
            Assert.NotNull(result.EnumListProperty);
            Assert.NotNull(result.StringListField);
            Assert.NotNull(result.StringIntDictField);
            Assert.NotNull(result.EnumListField);
        }
        
        [Fact]
        public void AllCollectionsDataSerializer_ShouldHandleEmptyCollections()
        {
            // Arrange
            var original = new AllCollectionsData();
            
            // Keep all collections empty
            var serializer = AllCollectionsDataSerializer.Instance;

            // Get size and create buffer
            int size = serializer.GetSize(original);
            byte[] buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);

            offset = 0;
            serializer.Deserialize(out AllCollectionsData result, buffer, ref offset);

            // Assert
            Assert.NotNull(result.IntListProperty);
            Assert.Empty(result.IntListProperty);
            Assert.NotNull(result.StringListProperty);
            Assert.Empty(result.StringListProperty);
            Assert.NotNull(result.IntStringDictProperty);
            Assert.Empty(result.IntStringDictProperty);
            Assert.NotNull(result.StringIntDictProperty);
            Assert.Empty(result.StringIntDictProperty);
            Assert.NotNull(result.IntArrayProperty);
            Assert.Empty(result.IntArrayProperty);
            Assert.NotNull(result.StringArrayProperty);
            Assert.Empty(result.StringArrayProperty);
            Assert.NotNull(result.EnumListProperty);
            Assert.Empty(result.EnumListProperty);
            
            Assert.NotNull(result.IntListField);
            Assert.Empty(result.IntListField);
            Assert.NotNull(result.StringListField);
            Assert.Empty(result.StringListField);
            Assert.NotNull(result.IntStringDictField);
            Assert.Empty(result.IntStringDictField);
            Assert.NotNull(result.StringIntDictField);
            Assert.Empty(result.StringIntDictField);
            Assert.NotNull(result.IntArrayField);
            Assert.Empty(result.IntArrayField);
            Assert.NotNull(result.StringArrayField);
            Assert.Empty(result.StringArrayField);
            Assert.NotNull(result.EnumListField);
            Assert.Empty(result.EnumListField);
        }
    }
} 