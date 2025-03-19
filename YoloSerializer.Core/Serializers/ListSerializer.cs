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
    public static class ListSerializer<T>
    {
        // Maximum number of elements to handle on the stack
        private const int MaxStackAlloc = 64;
        
        /// <summary>
        /// Serializes a list to a byte span using the provided serializer type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<TSerializer>(List<T>? value, Span<byte> span, ref int offset) 
            where TSerializer : ISerializer<T>
        {
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            // Write null flag (0 for null, 1 for non-null)
            if (value == null)
            {
                span.WriteInt32(ref offset, -1);
                return;
            }
            
            span.WriteInt32(ref offset, value.Count);
            
            // Write each element
            foreach (var item in value)
            {
                serializer.Serialize(item, span, ref offset);
            }
        }

        /// <summary>
        /// Deserializes a list from a byte span using the provided serializer type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize<TSerializer>(out List<T>? value, ReadOnlySpan<byte> span, ref int offset) 
            where TSerializer : ISerializer<T>
        {
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            // Read count
            int count = span.ReadInt32(ref offset);
            
            // Handle null case
            if (count == -1)
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
                    serializer.Deserialize(out T item, span, ref offset);
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
                        serializer.Deserialize(out T item, span, ref offset);
                        value.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the size needed to serialize a list using the provided serializer type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize<TSerializer>(List<T>? value) 
            where TSerializer : ISerializer<T>
        {
            // Null check
            if (value == null)
                return sizeof(int); // Just enough for -1 count
                
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            // Start with size for count
            int size = sizeof(int);
            
            // Add size for each element
            foreach (var item in value)
            {
                size += serializer.GetSize(item);
            }
            
            return size;
        }
    }
} 