using System;
using System.Runtime.CompilerServices;

namespace YoloSerializer.Core.CodeGeneration
{
    /// <summary>
    /// Interface for generated type serializers to ensure consistency across all serializer implementations
    /// </summary>
    /// <typeparam name="T">The type being serialized</typeparam>
    public interface IGeneratedTypeSerializer<T> where T : class
    {
        /// <summary>
        /// Serializes an object to a byte buffer
        /// </summary>
        /// <param name="value">The object to serialize (may be null)</param>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="offset">The current offset, updated after writing</param>
        void Serialize(T? value, Span<byte> buffer, ref int offset);
        
        /// <summary>
        /// Deserializes an object from a byte buffer
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="offset">The current offset, updated after reading</param>
        /// <returns>The deserialized object (may be null)</returns>
        T? Deserialize(ReadOnlySpan<byte> buffer, ref int offset);
        
        /// <summary>
        /// Calculates the size needed to serialize an object
        /// </summary>
        /// <param name="value">The object to calculate size for (may be null)</param>
        /// <returns>The size in bytes</returns>
        int GetSize(T? value);
    }
} 