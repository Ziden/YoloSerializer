using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for 64-bit integers
    /// </summary>
    public static class Int64Serializer
    {
        /// <summary>
        /// Serializes a long to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(long value, Span<byte> span, ref int offset)
        {
            span.WriteInt64(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a long from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(out long value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadInt64(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a long (always 8)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(long value)
        {
            return sizeof(long);
        }
    }
} 