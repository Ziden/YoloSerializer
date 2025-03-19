using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for List{T}
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    public class ListSerializer<T> : ISerializer<List<T>?>
    {
        private readonly ISerializer<T> _elementSerializer;
        private const int SmallListThreshold = 8;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListSerializer{T}"/> class
        /// </summary>
        /// <param name="elementSerializer">The serializer for each element in the list</param>
        public ListSerializer(ISerializer<T> elementSerializer)
        {
            _elementSerializer = elementSerializer ?? throw new ArgumentNullException(nameof(elementSerializer));
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(List<T>? value, Span<byte> buffer, ref int offset)
        {
            if (value == null)
            {
                // Write -1 to indicate null list
                BitConverter.TryWriteBytes(buffer.Slice(offset), -1);
                offset += sizeof(int);
                return;
            }

            // Write count
            BitConverter.TryWriteBytes(buffer.Slice(offset), value.Count);
            offset += sizeof(int);

            // Write elements
            foreach (var item in value)
            {
                _elementSerializer.Serialize(item, buffer, ref offset);
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out List<T>? value, ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Read count
            int count = BitConverter.ToInt32(buffer.Slice(offset));
            offset += sizeof(int);

            if (count == -1)
            {
                // Null list
                value = null;
                return;
            }

            // Create list with capacity
            value = new List<T>(count);

            // Read elements
            for (int i = 0; i < count; i++)
            {
                T item;
                _elementSerializer.Deserialize(out item, buffer, ref offset);
                value.Add(item);
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(List<T>? value)
        {
            if (value == null)
            {
                return sizeof(int); // Just the count (-1)
            }

            int size = sizeof(int); // Count

            // Add size of each element
            foreach (var item in value)
            {
                size += _elementSerializer.GetSize(item);
            }

            return size;
        }
    }
} 