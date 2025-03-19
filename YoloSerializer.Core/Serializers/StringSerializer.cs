using System;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for System.String
    /// </summary>
    public class StringSerializer : ISerializer<string?>
    {
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static readonly StringSerializer Instance = new StringSerializer();

        private StringSerializer() { }

        /// <inheritdoc/>
        public void Serialize(string? value, Span<byte> span, ref int offset)
        {
            span.WriteString(ref offset, value);
        }

        /// <inheritdoc/>
        public void Deserialize(out string? value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadString(ref offset);
        }

        /// <inheritdoc/>
        public int GetSize(string? value)
        {
            if (value == null)
            {
                return sizeof(int);
            }

            return sizeof(int) + value.Length * sizeof(char);
        }
    }
} 