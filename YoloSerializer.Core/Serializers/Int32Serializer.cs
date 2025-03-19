using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for 32-bit integers
    /// </summary>
    public static class Int32Serializer
    {
        /// <summary>
        /// Serializes an integer to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(int value, Span<byte> span, ref int offset)
        {
            span.WriteInt32(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes an integer from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(out int value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadInt32(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize an integer (always 4)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(int value)
        {
            return sizeof(int);
        }
    }
} 