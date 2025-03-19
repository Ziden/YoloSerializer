using System;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for System.Single
    /// </summary>
    public class FloatSerializer : ISerializer<float>
    {
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static readonly FloatSerializer Instance = new FloatSerializer();

        private FloatSerializer() { }

        /// <inheritdoc/>
        public void Serialize(float value, Span<byte> span, ref int offset)
        {
            span.WriteFloat(ref offset, value);
        }

        /// <inheritdoc/>
        public void Deserialize(out float value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadFloat(ref offset);
        }

        /// <inheritdoc/>
        public int GetSize(float value)
        {
            return sizeof(float);
        }
    }
} 