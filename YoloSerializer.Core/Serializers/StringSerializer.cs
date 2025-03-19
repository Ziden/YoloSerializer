using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for string values with UTF-8 encoding
    /// </summary>
    public static class StringSerializer
    {
        // Constants for special markers
        private const int NullStringMarker = -1;
        private const int EmptyStringMarker = 0;
        
        // Size threshold for stack allocation
        private const int MaxStackAllocSize = 1024;
        
        /// <summary>
        /// Serializes a string to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(string? value, Span<byte> span, ref int offset)
        {
            // Handle null case
            if (value == null)
            {
                span.WriteInt32(ref offset, NullStringMarker);
                return;
            }
            
            // Handle empty string case
            if (value.Length == 0)
            {
                span.WriteInt32(ref offset, EmptyStringMarker);
                return;
            }
            
            // Get byte count for string using UTF-8 encoding
            int byteCount = Encoding.UTF8.GetByteCount(value);
            
            // Write string length
            span.WriteInt32(ref offset, byteCount);
            
            // For small strings, use stack allocation
            if (byteCount <= MaxStackAllocSize)
            {
                Span<byte> stringBytes = stackalloc byte[byteCount];
                Encoding.UTF8.GetBytes(value, stringBytes);
                stringBytes.CopyTo(span.Slice(offset));
                offset += byteCount;
            }
            else
            {
                // For larger strings, rent buffer from pool
                byte[] stringBytes = ArrayPool<byte>.Shared.Rent(byteCount);
                try
                {
                    Encoding.UTF8.GetBytes(value, stringBytes);
                    stringBytes.AsSpan(0, byteCount).CopyTo(span.Slice(offset));
                    offset += byteCount;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(stringBytes);
                }
            }
        }
        
        /// <summary>
        /// Deserializes a string from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deserialize(out string? value, ReadOnlySpan<byte> span, ref int offset)
        {
            // Read string length marker
            int stringLength = span.ReadInt32(ref offset);
            
            // Handle null case
            if (stringLength == NullStringMarker)
            {
                value = null;
                return;
            }
            
            // Handle empty string case
            if (stringLength == EmptyStringMarker)
            {
                value = string.Empty;
                return;
            }
            
            // For small strings, use stack allocation
            if (stringLength <= MaxStackAllocSize)
            {
                Span<byte> stringBytes = stackalloc byte[stringLength];
                span.Slice(offset, stringLength).CopyTo(stringBytes);
                value = Encoding.UTF8.GetString(stringBytes);
                offset += stringLength;
            }
            else
            {
                // For larger strings, rent buffer from pool
                byte[] stringBytes = ArrayPool<byte>.Shared.Rent(stringLength);
                try
                {
                    span.Slice(offset, stringLength).CopyTo(stringBytes);
                    value = Encoding.UTF8.GetString(stringBytes, 0, stringLength);
                    offset += stringLength;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(stringBytes);
                }
            }
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a string
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(string? value)
        {
            // Null or empty string: just the length marker
            if (value == null || value.Length == 0)
                return sizeof(int);
            
            // For non-empty strings: length marker + UTF-8 bytes
            return sizeof(int) + Encoding.UTF8.GetByteCount(value);
        }
    }
} 