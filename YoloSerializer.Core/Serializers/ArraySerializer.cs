using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for arrays of type T
    /// </summary>
    /// <typeparam name="T">Type of elements in the array</typeparam>
    public static class ArraySerializer<T>
    {
        // Maximum number of elements to handle on the stack
        private const int MaxStackAlloc = 64;
        
        /// <summary>
        /// Serializes an array to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<TSerializer>(T[]? value, Span<byte> span, ref int offset) 
            where TSerializer : ISerializer<T>
        {
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            // Write -1 length for null array
            if (value == null)
            {
                span.WriteInt32(ref offset, -1);
                return;
            }
            
            // Write array length
            span.WriteInt32(ref offset, value.Length);
            
            // Write each element
            foreach (var item in value)
            {
                serializer.Serialize(item, span, ref offset);
            }
        }
        
        /// <summary>
        /// Deserializes an array from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize<TSerializer>(out T[]? value, ReadOnlySpan<byte> span, ref int offset) 
            where TSerializer : ISerializer<T>
        {
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            // Read array length
            int length = span.ReadInt32(ref offset);
            
            // Handle null array
            if (length == -1)
            {
                value = null;
                return;
            }
            
            // Create array with correct size
            value = new T[length];
            
            // Small arrays can use stack for indices
            if (length <= MaxStackAlloc)
            {
                // Read each element
                for (int i = 0; i < length; i++)
                {
                    serializer.Deserialize(out value[i], span, ref offset);
                }
            }
            else
            {
                // For large arrays, process in batches to improve locality
                int batchSize = Math.Min(MaxStackAlloc, length);
                
                for (int i = 0; i < length; i += batchSize)
                {
                    int currentBatch = Math.Min(batchSize, length - i);
                    
                    // Process each batch of elements
                    for (int j = 0; j < currentBatch; j++)
                    {
                        serializer.Deserialize(out value[i + j], span, ref offset);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize an array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize<TSerializer>(T[]? value) 
            where TSerializer : ISerializer<T>
        {
            // Null check
            if (value == null)
                return sizeof(int); // Just enough for -1 length
                
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            // Start with size for length
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