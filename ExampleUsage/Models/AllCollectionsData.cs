using System;
using System.Collections.Generic;
using YoloSerializer.Core.Models;

namespace YoloSerializer.Core.Models
{
    /// <summary>
    /// Test model that contains all collection types as both properties and fields
    /// </summary>
    public class AllCollectionsData
    {
        // Collection properties
        public List<int> IntListProperty { get; set; } = new();
        public List<string> StringListProperty { get; set; } = new();
        public Dictionary<int, string> IntStringDictProperty { get; set; } = new();
        public Dictionary<string, int> StringIntDictProperty { get; set; } = new();
        public int[] IntArrayProperty { get; set; } = Array.Empty<int>();
        public string[] StringArrayProperty { get; set; } = Array.Empty<string>();
        public List<TestEnum> EnumListProperty { get; set; } = new();
        
        // Collection fields
        public List<int> IntListField = new();
        public List<string> StringListField = new();
        public Dictionary<int, string> IntStringDictField = new();
        public Dictionary<string, int> StringIntDictField = new();
        public int[] IntArrayField = Array.Empty<int>();
        public string[] StringArrayField = Array.Empty<string>();
        public List<TestEnum> EnumListField = new();
        
        public AllCollectionsData() { }
        
        public override bool Equals(object? obj)
        {
            if (obj is not AllCollectionsData other) return false;
            
            // Compare properties
            if (!AreListsEqual(IntListProperty, other.IntListProperty)) return false;
            if (!AreListsEqual(StringListProperty, other.StringListProperty)) return false;
            if (!AreDictionariesEqual(IntStringDictProperty, other.IntStringDictProperty)) return false;
            if (!AreDictionariesEqual(StringIntDictProperty, other.StringIntDictProperty)) return false;
            if (!AreArraysEqual(IntArrayProperty, other.IntArrayProperty)) return false;
            if (!AreArraysEqual(StringArrayProperty, other.StringArrayProperty)) return false;
            if (!AreListsEqual(EnumListProperty, other.EnumListProperty)) return false;
            
            // Compare fields
            if (!AreListsEqual(IntListField, other.IntListField)) return false;
            if (!AreListsEqual(StringListField, other.StringListField)) return false;
            if (!AreDictionariesEqual(IntStringDictField, other.IntStringDictField)) return false;
            if (!AreDictionariesEqual(StringIntDictField, other.StringIntDictField)) return false;
            if (!AreArraysEqual(IntArrayField, other.IntArrayField)) return false;
            if (!AreArraysEqual(StringArrayField, other.StringArrayField)) return false;
            if (!AreListsEqual(EnumListField, other.EnumListField)) return false;
            
            return true;
        }
        
        private static bool AreListsEqual<T>(List<T>? list1, List<T>? list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;
            
            for (int i = 0; i < list1.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(list1[i], list2[i]))
                    return false;
            }
            
            return true;
        }
        
        private static bool AreDictionariesEqual<TKey, TValue>(Dictionary<TKey, TValue>? dict1, Dictionary<TKey, TValue>? dict2) 
            where TKey : notnull
        {
            if (dict1 == null && dict2 == null) return true;
            if (dict1 == null || dict2 == null) return false;
            if (dict1.Count != dict2.Count) return false;
            
            foreach (var key in dict1.Keys)
            {
                if (!dict2.TryGetValue(key, out var value2)) return false;
                if (!EqualityComparer<TValue>.Default.Equals(dict1[key], value2)) return false;
            }
            
            return true;
        }
        
        private static bool AreArraysEqual<T>(T[]? array1, T[]? array2)
        {
            if (array1 == null && array2 == null) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;
            
            for (int i = 0; i < array1.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(array1[i], array2[i]))
                    return false;
            }
            
            return true;
        }
        
        public override int GetHashCode()
        {
            var hash = new HashCode();
            
            // Add properties to hash
            if (IntListProperty != null)
                foreach (var item in IntListProperty) hash.Add(item);
                
            if (StringListProperty != null)
                foreach (var item in StringListProperty) hash.Add(item);
                
            if (IntStringDictProperty != null)
                foreach (var kvp in IntStringDictProperty) 
                { 
                    hash.Add(kvp.Key); 
                    hash.Add(kvp.Value); 
                }
                
            if (StringIntDictProperty != null)
                foreach (var kvp in StringIntDictProperty) 
                { 
                    hash.Add(kvp.Key); 
                    hash.Add(kvp.Value); 
                }
                
            if (IntArrayProperty != null)
                foreach (var item in IntArrayProperty) hash.Add(item);
                
            if (StringArrayProperty != null)
                foreach (var item in StringArrayProperty) hash.Add(item);
                
            if (EnumListProperty != null)
                foreach (var item in EnumListProperty) hash.Add(item);
            
            // Add fields to hash
            if (IntListField != null)
                foreach (var item in IntListField) hash.Add(item);
                
            if (StringListField != null)
                foreach (var item in StringListField) hash.Add(item);
                
            if (IntStringDictField != null)
                foreach (var kvp in IntStringDictField) 
                { 
                    hash.Add(kvp.Key); 
                    hash.Add(kvp.Value); 
                }
                
            if (StringIntDictField != null)
                foreach (var kvp in StringIntDictField) 
                { 
                    hash.Add(kvp.Key); 
                    hash.Add(kvp.Value); 
                }
                
            if (IntArrayField != null)
                foreach (var item in IntArrayField) hash.Add(item);
                
            if (StringArrayField != null)
                foreach (var item in StringArrayField) hash.Add(item);
                
            if (EnumListField != null)
                foreach (var item in EnumListField) hash.Add(item);
                
            return hash.ToHashCode();
        }
    }
} 