using System;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for arrays of type T
    /// </summary>
    public class ArraySerializer<T> : ISerializer<T[]?>
    {
        private readonly ISerializer<T> _elementSerializer;

        /// <summary>
        /// Creates a new instance of ArraySerializer
        /// </summary>
        /// <param name="elementSerializer">The serializer for array elements</param>
        public ArraySerializer(ISerializer<T> elementSerializer)
        {
            _elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
        }

        /// <inheritdoc/>
        public void Serialize(T[]? value, Span<byte> span, ref int offset)
        {
            if (value == null)
            {
                span.WriteInt32(ref offset, -1);
                return;
            }

            span.WriteInt32(ref offset, value.Length);
            
            for (int i = 0; i < value.Length; i++)
            {
                _elementSerializer.Serialize(value[i], span, ref offset);
            }
        }

        /// <inheritdoc/>
        public void Deserialize(out T[]? value, ReadOnlySpan<byte> span, ref int offset)
        {
            int length = span.ReadInt32(ref offset);
            
            if (length == -1)
            {
                value = null;
                return;
            }

            value = new T[length];
            
            for (int i = 0; i < length; i++)
            {
                _elementSerializer.Deserialize(out value[i], span, ref offset);
            }
        }

        /// <inheritdoc/>
        public int GetSize(T[]? value)
        {
            if (value == null)
            {
                return sizeof(int);
            }

            int size = sizeof(int); // Array length
            
            for (int i = 0; i < value.Length; i++)
            {
                size += _elementSerializer.GetSize(value[i]);
            }

            return size;
        }
    }
} 