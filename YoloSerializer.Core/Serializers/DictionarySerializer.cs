using System;
using System.Collections.Generic;

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
        public void Serialize(Dictionary<TKey, TValue>? value, Span<byte> span, ref int offset)
        {
            if (value == null)
            {
                span.WriteInt32(ref offset, -1);
                return;
            }

            span.WriteInt32(ref offset, value.Count);
            
            foreach (var kvp in value)
            {
                _keySerializer.Serialize(kvp.Key, span, ref offset);
                _valueSerializer.Serialize(kvp.Value, span, ref offset);
            }
        }

        /// <inheritdoc/>
        public void Deserialize(out Dictionary<TKey, TValue>? value, ReadOnlySpan<byte> span, ref int offset)
        {
            int count = span.ReadInt32(ref offset);
            
            if (count == -1)
            {
                value = null;
                return;
            }

            value = new Dictionary<TKey, TValue>(count);
            
            for (int i = 0; i < count; i++)
            {
                TKey key;
                TValue val;
                _keySerializer.Deserialize(out key, span, ref offset);
                _valueSerializer.Deserialize(out val, span, ref offset);
                value.Add(key, val);
            }
        }

        /// <inheritdoc/>
        public int GetSize(Dictionary<TKey, TValue>? value)
        {
            if (value == null)
            {
                return sizeof(int);
            }

            int size = sizeof(int); // Dictionary count
            
            foreach (var kvp in value)
            {
                size += _keySerializer.GetSize(kvp.Key);
                size += _valueSerializer.GetSize(kvp.Value);
            }

            return size;
        }
    }
} 