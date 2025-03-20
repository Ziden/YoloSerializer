using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for lists of any type T with optimized memory usage strategies
    /// </summary>
    /// <typeparam name="T">Type of the list elements</typeparam>
    public sealed class ListSerializer<T> : ISerializer<List<T>?>
    {
        private const int MaxStackAlloc = 64;
        
        private readonly ISerializer<T> _itemSerializer;
        
        private static readonly Dictionary<Type, ListSerializer<T>> _instances = new Dictionary<Type, ListSerializer<T>>();
        
        /// <summary>
        /// Gets the singleton instance for the specific item serializer type with efficient caching
        /// </summary>
        public static ListSerializer<T> GetInstance<TSerializer>() where TSerializer : ISerializer<T>
        {
            var serializerType = typeof(TSerializer);
            
            if (!_instances.TryGetValue(serializerType, out var instance))
            {
                var itemSerializer = GetItemSerializer<TSerializer>();
                instance = new ListSerializer<T>(itemSerializer);
                _instances[serializerType] = instance;
            }
            
            return instance;
        }
        
        private static ISerializer<T> GetItemSerializer<TSerializer>() where TSerializer : ISerializer<T>
        {
            var instanceProp = typeof(TSerializer).GetProperty("Instance");
            if (instanceProp != null)
            {
                return (ISerializer<T>)instanceProp.GetValue(null);
            }
            
            return (ISerializer<T>)Activator.CreateInstance(typeof(TSerializer));
        }
        
        /// <summary>
        /// Creates a new instance of the ListSerializer with the specified item serializer
        /// </summary>
        public ListSerializer(ISerializer<T> itemSerializer)
        {
            _itemSerializer = itemSerializer ?? throw new ArgumentNullException(nameof(itemSerializer));
        }
        
        /// <summary>
        /// Serializes a list to a byte span with proper null handling and efficient element serialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(List<T>? value, Span<byte> buffer, ref int offset)
        {
            if (value == null)
            {
                Int32Serializer.Instance.Serialize(NullHandler.NullMarker, buffer, ref offset);
                return;
            }
            
            Int32Serializer.Instance.Serialize(value.Count, buffer, ref offset);
            
            foreach (var item in value)
            {
                _itemSerializer.Serialize(item, buffer, ref offset);
            }
        }

        /// <summary>
        /// Deserializes a list from a byte span with optimized memory usage for different list sizes
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out List<T>? value, ReadOnlySpan<byte> buffer, ref int offset)
        {
            Int32Serializer.Instance.Deserialize(out int count, buffer, ref offset);
            
            if (count == NullHandler.NullMarker)
            {
                value = null;
                return;
            }
            
            value = new List<T>(count);
            
            if (count <= MaxStackAlloc)
            {
                for (int i = 0; i < count; i++)
                {
                    _itemSerializer.Deserialize(out T item, buffer, ref offset);
                    value.Add(item);
                }
            }
            else
            {
                int batchSize = Math.Min(MaxStackAlloc, count);
                
                for (int i = 0; i < count; i += batchSize)
                {
                    int currentBatch = Math.Min(batchSize, count - i);
                    
                    for (int j = 0; j < currentBatch; j++)
                    {
                        _itemSerializer.Deserialize(out T item, buffer, ref offset);
                        value.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the size needed to serialize a list with proper null handling
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(List<T>? value)
        {
            if (value == null)
                return sizeof(int);
                
            int size = sizeof(int);
            
            foreach (var item in value)
            {
                size += _itemSerializer.GetSize(item);
            }
            
            return size;
        }
    }
} 