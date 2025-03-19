using System;
using System.Collections.Generic;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for List{T}
    /// </summary>
    public class ListSerializer<T> : ISerializer<List<T>?>
    {
        private readonly ISerializer<T> _elementSerializer;

        /// <summary>
        /// Creates a new instance of ListSerializer
        /// </summary>
        /// <param name="elementSerializer">The serializer for list elements</param>
        public ListSerializer(ISerializer<T> elementSerializer)
        {
            _elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
        }

        /// <inheritdoc/>
        public void Serialize(List<T>? value, Span<byte> span, ref int offset)
        {
            if (value == null)
            {
                span.WriteInt32(ref offset, -1);
                return;
            }

            span.WriteInt32(ref offset, value.Count);
            
            foreach (var item in value)
            {
                _elementSerializer.Serialize(item, span, ref offset);
            }
        }

        /// <inheritdoc/>
        public void Deserialize(out List<T>? value, ReadOnlySpan<byte> span, ref int offset)
        {
            int count = span.ReadInt32(ref offset);
            
            if (count == -1)
            {
                value = null;
                return;
            }

            value = new List<T>(count);
            
            for (int i = 0; i < count; i++)
            {
                T item;
                _elementSerializer.Deserialize(out item, span, ref offset);
                value.Add(item);
            }
        }

        /// <inheritdoc/>
        public int GetSize(List<T>? value)
        {
            if (value == null)
            {
                return sizeof(int);
            }

            int size = sizeof(int); // List count
            
            foreach (var item in value)
            {
                size += _elementSerializer.GetSize(item);
            }

            return size;
        }
    }
} 