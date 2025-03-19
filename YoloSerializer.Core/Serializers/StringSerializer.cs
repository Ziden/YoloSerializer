using System;
using System.Runtime.CompilerServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for string values
    /// </summary>
    public sealed class StringSerializer : ISerializer<string?>
    {
        // Constant for empty string
        private const int EmptyStringMarker = 0;
        
        private static readonly StringSerializer _instance = new StringSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static StringSerializer Instance => _instance;
        
        private StringSerializer() { }
        
        /// <summary>
        /// Serializes a string to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(string? value, Span<byte> span, ref int offset)
        {
            // Handle null case
            if (NullHandler.WriteNullIfNeeded(value, span, ref offset))
                return;
                
            // Special case for empty string
            if (value!.Length == 0)
            {
                span.WriteInt32(ref offset, EmptyStringMarker);
                return;
            }
            
            // Get UTF-8 bytes for the string
            int byteCount = Encoding.UTF8.GetByteCount(value);
            
            // Write the length first (non-zero indicates a non-empty string)
            span.WriteInt32(ref offset, byteCount);
            
            // For small strings, use stack allocation
            if (byteCount <= 256)
            {
                Span<byte> bytes = stackalloc byte[byteCount];
                Encoding.UTF8.GetBytes(value, bytes);
                bytes.CopyTo(span.Slice(offset));
            }
            else
            {
                // For larger strings, use heap allocation
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                bytes.CopyTo(span.Slice(offset));
            }
            
            // Advance the offset
            offset += byteCount;
        }
        
        /// <summary>
        /// Deserializes a string from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out string? value, ReadOnlySpan<byte> span, ref int offset)
        {
            // Check for null
            if (NullHandler.ReadAndCheckNull(span, ref offset, out int byteCount))
            {
                value = null;
                return;
            }
            
            // Check for empty string
            if (byteCount == EmptyStringMarker)
            {
                value = string.Empty;
                return;
            }
            
            // Ensure the byte count is reasonable
            if (byteCount < 0 || byteCount > span.Length - offset)
                throw new ArgumentException("Invalid string length or buffer too small");
                
            // For small strings, use stack allocation
            if (byteCount <= 256)
            {
                Span<char> chars = stackalloc char[byteCount];
                int charCount = Encoding.UTF8.GetChars(span.Slice(offset, byteCount), chars);
                value = new string(chars.Slice(0, charCount));
            }
            else
            {
                // For larger strings, use heap allocation
                value = Encoding.UTF8.GetString(span.Slice(offset, byteCount));
            }
            
            // Advance the offset
            offset += byteCount;
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a string
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(string? value)
        {
            // Size for null or empty string (just an int)
            if (value == null || value.Length == 0)
                return sizeof(int);
                
            // Size for string: length (int) + UTF-8 bytes
            return sizeof(int) + Encoding.UTF8.GetByteCount(value);
        }
    }
} 