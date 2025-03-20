using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for arrays of type T with optimized memory allocation strategies
    /// </summary>
    /// <typeparam name="T">Type of elements in the array</typeparam>
    public static class ArraySerializer<T>
    {
        private const int MaxStackAlloc = 64;
        
        /// <summary>
        /// Serializes an array to a byte span with null handling and efficient element serialization
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<TSerializer>(T[]? value, Span<byte> span, ref int offset) 
            where TSerializer : ISerializer<T>
        {
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            if (value == null)
            {
                span.WriteInt32(ref offset, -1);
                return;
            }
            
            span.WriteInt32(ref offset, value.Length);
            
            foreach (var item in value)
            {
                serializer.Serialize(item, span, ref offset);
            }
        }
        
        /// <summary>
        /// Deserializes an array from a byte span with memory optimizations for different array sizes
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize<TSerializer>(out T[]? value, ReadOnlySpan<byte> span, ref int offset) 
            where TSerializer : ISerializer<T>
        {
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            int length = span.ReadInt32(ref offset);
            
            if (length == -1)
            {
                value = null;
                return;
            }
            
            value = new T[length];
            
            if (length <= MaxStackAlloc)
            {
                for (int i = 0; i < length; i++)
                {
                    serializer.Deserialize(out value[i], span, ref offset);
                }
            }
            else
            {
                int batchSize = Math.Min(MaxStackAlloc, length);
                
                for (int i = 0; i < length; i += batchSize)
                {
                    int currentBatch = Math.Min(batchSize, length - i);
                    
                    for (int j = 0; j < currentBatch; j++)
                    {
                        serializer.Deserialize(out value[i + j], span, ref offset);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize an array with proper handling for null arrays
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize<TSerializer>(T[]? value) 
            where TSerializer : ISerializer<T>
        {
            if (value == null)
                return sizeof(int);
                
            TSerializer serializer = (TSerializer)Activator.CreateInstance(typeof(TSerializer))!;
            
            int size = sizeof(int);
            
            foreach (var item in value)
            {
                size += serializer.GetSize(item);
            }
            
            return size;
        }
    }
} 