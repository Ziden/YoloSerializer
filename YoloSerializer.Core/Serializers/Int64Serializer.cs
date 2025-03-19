using System;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for System.Int64
    /// </summary>
    public class Int64Serializer : ISerializer<long>
    {
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static readonly Int64Serializer Instance = new Int64Serializer();

        private Int64Serializer() { }

        /// <inheritdoc/>
        public void Serialize(long value, Span<byte> span, ref int offset)
        {
            span.WriteInt64(ref offset, value);
        }

        /// <inheritdoc/>
        public void Deserialize(out long value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadInt64(ref offset);
        }

        /// <inheritdoc/>
        public int GetSize(long value)
        {
            return sizeof(long);
        }
    }
} 