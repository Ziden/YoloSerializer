using System;

namespace YoloSerializer.Core.Contracts
{
    /// <summary>
    /// Interface for type-specific serializers
    /// </summary>
    /// <typeparam name="T">The type that can be serialized and deserialized</typeparam>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serializes a value to a byte span
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="offset">The current offset in the buffer, updated after writing</param>
        void Serialize(T value, Span<byte> buffer, ref int offset);
        
        /// <summary>
        /// Deserializes a value from a byte span
        /// </summary>
        /// <param name="value">The deserialized value, set on success</param>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="offset">The current offset in the buffer, updated after reading</param>
        void Deserialize(out T value, ReadOnlySpan<byte> buffer, ref int offset);
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a value
        /// </summary>
        /// <param name="value">The value to calculate size for</param>
        /// <returns>The number of bytes needed</returns>
        int GetSize(T value);
    }
} 