using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for dictionaries of any key/value type
    /// </summary>
    /// <typeparam name="TKey">Type of the dictionary key</typeparam>
    /// <typeparam name="TValue">Type of the dictionary value</typeparam>
    public static class DictionarySerializer<TKey, TValue> 
        where TKey : notnull
    {
        private const int MaxBatchSize = 64;
        
        /// <summary>
        /// Serializes a dictionary to a byte span using the provided serializer types
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize<TKeySerializer, TValueSerializer>(
            Dictionary<TKey, TValue>? value, 
            Span<byte> span, 
            ref int offset)
            where TKeySerializer : ISerializer<TKey>
            where TValueSerializer : ISerializer<TValue>
        {
            TKeySerializer keySerializer = (TKeySerializer)Activator.CreateInstance(typeof(TKeySerializer))!;
            TValueSerializer valueSerializer = (TValueSerializer)Activator.CreateInstance(typeof(TValueSerializer))!;
            
            if (value == null)
            {
                span.WriteInt32(ref offset, -1);
                return;
            }

            span.WriteInt32(ref offset, value.Count);

            foreach (var kvp in value)
            {
                keySerializer.Serialize(kvp.Key, span, ref offset);
                valueSerializer.Serialize(kvp.Value, span, ref offset);
            }
        }

        /// <summary>
        /// Deserializes a dictionary from a byte span using the provided serializer types
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize<TKeySerializer, TValueSerializer>(
            out Dictionary<TKey, TValue>? value,
            ReadOnlySpan<byte> span,
            ref int offset)
            where TKeySerializer : ISerializer<TKey>
            where TValueSerializer : ISerializer<TValue>
        {
            TKeySerializer keySerializer = (TKeySerializer)Activator.CreateInstance(typeof(TKeySerializer))!;
            TValueSerializer valueSerializer = (TValueSerializer)Activator.CreateInstance(typeof(TValueSerializer))!;

            int count = span.ReadInt32(ref offset);

            if (count == -1)
            {
                value = null;
                return;
            }

            value = new Dictionary<TKey, TValue>(count);

            if (count <= MaxBatchSize)
            {
                for (int i = 0; i < count; i++)
                {
                    keySerializer.Deserialize(out TKey key, span, ref offset);
                    valueSerializer.Deserialize(out TValue val, span, ref offset);
                    value.Add(key, val);
                }
            }
            else
            {
                int batchSize = Math.Min(MaxBatchSize, count);

                for (int i = 0; i < count; i += batchSize)
                {
                    int currentBatch = Math.Min(batchSize, count - i);

                    for (int j = 0; j < currentBatch; j++)
                    {
                        keySerializer.Deserialize(out TKey key, span, ref offset);
                        valueSerializer.Deserialize(out TValue val, span, ref offset);
                        value.Add(key, val);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the size needed to serialize a dictionary using the provided serializer types
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize<TKeySerializer, TValueSerializer>(Dictionary<TKey, TValue>? value)     
            where TKeySerializer : ISerializer<TKey>
            where TValueSerializer : ISerializer<TValue>
        {
            if (value == null)
                return sizeof(int);

            TKeySerializer keySerializer = (TKeySerializer)Activator.CreateInstance(typeof(TKeySerializer))!;
            TValueSerializer valueSerializer = (TValueSerializer)Activator.CreateInstance(typeof(TValueSerializer))!;

            int size = sizeof(int);

            foreach (var kvp in value)
            {
                size += keySerializer.GetSize(kvp.Key);
                size += valueSerializer.GetSize(kvp.Value);
            }

            return size;
        }
    }
} 