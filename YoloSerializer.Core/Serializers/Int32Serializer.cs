using System;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for System.Int32
    /// </summary>
    public class Int32Serializer : ISerializer<int>
    {
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static readonly Int32Serializer Instance = new Int32Serializer();

        private Int32Serializer() { }

        /// <inheritdoc/>
        public void Serialize(int value, Span<byte> span, ref int offset)
        {
            span.WriteInt32(ref offset, value);
        }

        /// <inheritdoc/>
        public void Deserialize(out int value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadInt32(ref offset);
        }

        /// <inheritdoc/>
        public int GetSize(int value)
        {
            return sizeof(int);
        }
    }
} 