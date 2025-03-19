using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for boolean values
    /// </summary>
    public static class BooleanSerializer
    {
        /// <summary>
        /// Serializes a boolean to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(bool value, Span<byte> span, ref int offset)
        {
            span.WriteBool(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a boolean from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(out bool value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadBool(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a boolean (always 1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(bool value)
        {
            return sizeof(byte);
        }
    }
} 