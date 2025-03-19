using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for double-precision floating point numbers
    /// </summary>
    public static class DoubleSerializer
    {
        /// <summary>
        /// Serializes a double to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(double value, Span<byte> span, ref int offset)
        {
            span.WriteDouble(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a double from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(out double value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadDouble(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a double (always 8)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(double value)
        {
            return sizeof(double);
        }
    }
} 