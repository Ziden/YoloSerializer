using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for lists of any type T
    /// </summary>
    /// <typeparam name="T">Type of the list elements</typeparam>
    public sealed class ListSerializer<T> : ISerializer<List<T>?>
    {
        // Maximum number of elements to handle on the stack
        private const int MaxStackAlloc = 64;
        
        private readonly ISerializer<T> _itemSerializer;
        
        // Singleton pattern implementation
        private static readonly Dictionary<Type, ListSerializer<T>> _instances = new Dictionary<Type, ListSerializer<T>>();
        
        /// <summary>
        /// Gets the singleton instance for the specific item serializer type
        /// </summary>
        public static ListSerializer<T> GetInstance<TSerializer>() where TSerializer : ISerializer<T>
        {
            var serializerType = typeof(TSerializer);
            
            if (!_instances.TryGetValue(serializerType, out var instance))
            {
                // Create a new instance with the appropriate item serializer
                var itemSerializer = GetItemSerializer<TSerializer>();
                instance = new ListSerializer<T>(itemSerializer);
                _instances[serializerType] = instance;
            }
            
            return instance;
        }
        
        private static ISerializer<T> GetItemSerializer<TSerializer>() where TSerializer : ISerializer<T>
        {
            // Get singleton instance from ISerializer
            var instanceProp = typeof(TSerializer).GetProperty("Instance");
            if (instanceProp != null)
            {
                return (ISerializer<T>)instanceProp.GetValue(null);
            }
            
            // Fallback to creating a new instance
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
        /// Serializes a list to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(List<T>? value, Span<byte> buffer, ref int offset)
        {
            // Write null flag (NullHandler.NullMarker for null, count for non-null)
            if (value == null)
            {
                Int32Serializer.Instance.Serialize(NullHandler.NullMarker, buffer, ref offset);
                return;
            }
            
            // Write count
            Int32Serializer.Instance.Serialize(value.Count, buffer, ref offset);
            
            // Write each element
            foreach (var item in value)
            {
                _itemSerializer.Serialize(item, buffer, ref offset);
            }
        }

        /// <summary>
        /// Deserializes a list from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out List<T>? value, ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Read count
            Int32Serializer.Instance.Deserialize(out int count, buffer, ref offset);
            
            // Handle null case
            if (count == NullHandler.NullMarker)
            {
                value = null;
                return;
            }
            
            // Create list with capacity
            value = new List<T>(count);
            
            // Read elements using the appropriate strategy based on size
            if (count <= MaxStackAlloc)
            {
                // Direct read for small lists
                for (int i = 0; i < count; i++)
                {
                    _itemSerializer.Deserialize(out T item, buffer, ref offset);
                    value.Add(item);
                }
            }
            else
            {
                // Batch processing for larger lists to improve locality
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
        /// Gets the size needed to serialize a list
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(List<T>? value)
        {
            // Null check
            if (value == null)
                return sizeof(int); // Just enough for null marker
                
            // Start with size for count
            int size = sizeof(int);
            
            // Add size for each element
            foreach (var item in value)
            {
                size += _itemSerializer.GetSize(item);
            }
            
            return size;
        }
    }
} 