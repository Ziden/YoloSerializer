using System;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for System.Double
    /// </summary>
    public class DoubleSerializer : ISerializer<double>
    {
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static readonly DoubleSerializer Instance = new DoubleSerializer();

        private DoubleSerializer() { }

        /// <inheritdoc/>
        public void Serialize(double value, Span<byte> span, ref int offset)
        {
            span.WriteDouble(ref offset, value);
        }

        /// <inheritdoc/>
        public void Deserialize(out double value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadDouble(ref offset);
        }

        /// <inheritdoc/>
        public int GetSize(double value)
        {
            return sizeof(double);
        }
    }
} 