using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for 32-bit floating point numbers
    /// </summary>
    public static class FloatSerializer
    {
        /// <summary>
        /// Serializes a float to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(float value, Span<byte> span, ref int offset)
        {
            span.WriteFloat(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a float from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(out float value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadFloat(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a float (always 4)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(float value)
        {
            return sizeof(float);
        }
    }
} 