using System;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for System.Boolean
    /// </summary>
    public class BooleanSerializer : ISerializer<bool>
    {
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static readonly BooleanSerializer Instance = new BooleanSerializer();

        private BooleanSerializer() { }

        /// <inheritdoc/>
        public void Serialize(bool value, Span<byte> span, ref int offset)
        {
            span.WriteBool(ref offset, value);
        }

        /// <inheritdoc/>
        public void Deserialize(out bool value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadBool(ref offset);
        }

        /// <inheritdoc/>
        public int GetSize(bool value)
        {
            return sizeof(byte);
        }
    }
} 