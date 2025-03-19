using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for Dictionary{TKey, TValue}
    /// </summary>
    public class DictionarySerializer<TKey, TValue> : ISerializer<Dictionary<TKey, TValue>?>
        where TKey : notnull
    {
        private readonly ISerializer<TKey> _keySerializer;
        private readonly ISerializer<TValue> _valueSerializer;
        private const int MaxStackAllocSize = 256;
        private const int SmallDictThreshold = 8;

        /// <summary>
        /// Creates a new instance of DictionarySerializer
        /// </summary>
        /// <param name="keySerializer">The serializer for dictionary keys</param>
        /// <param name="valueSerializer">The serializer for dictionary values</param>
        public DictionarySerializer(ISerializer<TKey> keySerializer, ISerializer<TValue> valueSerializer)
        {
            _keySerializer = keySerializer ?? throw new ArgumentNullException(nameof(keySerializer));
            _valueSerializer = valueSerializer ?? throw new ArgumentNullException(nameof(valueSerializer));
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Dictionary<TKey, TValue>? value, Span<byte> buffer, ref int offset)
        {
            if (value == null)
            {
                BitConverter.TryWriteBytes(buffer.Slice(offset), -1);
                offset += sizeof(int);
                return;
            }

            int count = value.Count;
            BitConverter.TryWriteBytes(buffer.Slice(offset), count);
            offset += sizeof(int);

            // For small dictionaries, avoid enumerator allocation
            if (count <= SmallDictThreshold)
            {
                using var enumerator = value.GetEnumerator();
                for (int i = 0; i < count && enumerator.MoveNext(); i++)
                {
                    _keySerializer.Serialize(enumerator.Current.Key, buffer, ref offset);
                    _valueSerializer.Serialize(enumerator.Current.Value, buffer, ref offset);
                }
                return;
            }

            // For larger dictionaries, use foreach to avoid enumerator allocation overhead
            foreach (var kvp in value)
            {
                _keySerializer.Serialize(kvp.Key, buffer, ref offset);
                _valueSerializer.Serialize(kvp.Value, buffer, ref offset);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out Dictionary<TKey, TValue>? value, ReadOnlySpan<byte> buffer, ref int offset)
        {
            int count = BitConverter.ToInt32(buffer.Slice(offset));
            offset += sizeof(int);
            
            if (count == -1)
            {
                value = null;
                return;
            }

            // Preallocate with exact capacity and a load factor that avoids resizing
            value = new Dictionary<TKey, TValue>(count + (count / 3), EqualityComparer<TKey>.Default);

            if (count <= SmallDictThreshold)
            {
                // For small dictionaries, deserialize directly
                for (int i = 0; i < count; i++)
                {
                    TKey key;
                    TValue val;
                    _keySerializer.Deserialize(out key, buffer, ref offset);
                    _valueSerializer.Deserialize(out val, buffer, ref offset);
                    value.Add(key, val);
                }
                return;
            }

            // For larger dictionaries, use array pooling to batch the deserialization
            int batchSize = Math.Min(count, MaxStackAllocSize / 32); // Conservative estimate for key+value size
            if (batchSize <= SmallDictThreshold)
            {
                // If batch size is small, just use direct deserialization
                for (int i = 0; i < count; i++)
                {
                    TKey key;
                    TValue val;
                    _keySerializer.Deserialize(out key, buffer, ref offset);
                    _valueSerializer.Deserialize(out val, buffer, ref offset);
                    value.Add(key, val);
                }
                return;
            }

            TKey[] keyBuffer = ArrayPool<TKey>.Shared.Rent(batchSize);
            TValue[] valueBuffer = ArrayPool<TValue>.Shared.Rent(batchSize);
            try
            {
                for (int i = 0; i < count; i += batchSize)
                {
                    int currentBatch = Math.Min(batchSize, count - i);
                    for (int j = 0; j < currentBatch; j++)
                    {
                        _keySerializer.Deserialize(out keyBuffer[j], buffer, ref offset);
                        _valueSerializer.Deserialize(out valueBuffer[j], buffer, ref offset);
                    }
                    for (int j = 0; j < currentBatch; j++)
                    {
                        value.Add(keyBuffer[j], valueBuffer[j]);
                    }
                }
            }
            finally
            {
                ArrayPool<TKey>.Shared.Return(keyBuffer);
                ArrayPool<TValue>.Shared.Return(valueBuffer);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(Dictionary<TKey, TValue>? value)
        {
            if (value == null)
            {
                return sizeof(int);
            }

            int count = value.Count;
            int size = sizeof(int); // Dictionary count

            // For small dictionaries, use enumerator
            if (count <= SmallDictThreshold)
            {
                using var enumerator = value.GetEnumerator();
                for (int i = 0; i < count && enumerator.MoveNext(); i++)
                {
                    size += _keySerializer.GetSize(enumerator.Current.Key);
                    size += _valueSerializer.GetSize(enumerator.Current.Value);
                }
                return size;
            }

            // For larger dictionaries, use foreach
            foreach (var kvp in value)
            {
                size += _keySerializer.GetSize(kvp.Key);
                size += _valueSerializer.GetSize(kvp.Value);
            }

            return size;
        }
    }
} 