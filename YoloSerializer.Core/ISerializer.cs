using System;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Interface for type-specific serializers
    /// </summary>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serializes a value into a byte span
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="span">The destination span</param>
        /// <param name="offset">Current position in the span, will be updated after writing</param>
        void Serialize(T value, Span<byte> span, ref int offset);

        /// <summary>
        /// Deserializes a value from a byte span
        /// </summary>
        /// <param name="value">The deserialized value</param>
        /// <param name="span">The source span</param>
        /// <param name="offset">Current position in the span, will be updated after reading</param>
        void Deserialize(out T value, ReadOnlySpan<byte> span, ref int offset);

        /// <summary>
        /// Gets the size in bytes needed to serialize a value
        /// </summary>
        /// <param name="value">The value to measure</param>
        /// <returns>The number of bytes needed</returns>
        int GetSize(T value);
    }
} 