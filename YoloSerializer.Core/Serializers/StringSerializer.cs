using System;
using System.Runtime.CompilerServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for string values with efficient memory usage
    /// </summary>
    public sealed class StringSerializer : ISerializer<string?>
    {
        private const int EmptyStringMarker = 0;
        
        private static readonly StringSerializer _instance = new StringSerializer();
        
        /// <summary>
        /// Singleton instance for performance optimization
        /// </summary>
        public static StringSerializer Instance => _instance;
        
        private StringSerializer() { }
        
        /// <summary>
        /// Serializes a string to a byte span with special handling for null and empty strings
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(string? value, Span<byte> span, ref int offset)
        {
            // Note: string null handling is managed by the BitSet in the containing object serializer
            // We only need to handle null here when string is used directly without a containing object
            
            if (value == null)
            {
                span.WriteInt32(ref offset, NullHandler.NullMarker);
                return;
            }
                
            if (value.Length == 0)
            {
                span.WriteInt32(ref offset, EmptyStringMarker);
                return;
            }
            
            int byteCount = Encoding.UTF8.GetByteCount(value);
            
            span.WriteInt32(ref offset, byteCount);
            
            if (byteCount <= 256)
            {
                Span<byte> bytes = stackalloc byte[byteCount];
                Encoding.UTF8.GetBytes(value, bytes);
                bytes.CopyTo(span.Slice(offset));
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                bytes.CopyTo(span.Slice(offset));
            }
            
            offset += byteCount;
        }
        
        /// <summary>
        /// Deserializes a string from a byte span with optimized handling for different string lengths
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out string? value, ReadOnlySpan<byte> span, ref int offset)
        {
            // Read the length marker
            Int32Serializer.Instance.Deserialize(out int byteCount, span, ref offset);
            
            // Handle null marker case
            if (byteCount == NullHandler.NullMarker)
            {
                value = null;
                return;
            }
            
            // Handle empty string case
            if (byteCount == EmptyStringMarker)
            {
                value = string.Empty;
                return;
            }
            
            if (byteCount < 0 || byteCount > span.Length - offset)
                throw new ArgumentException("Invalid string length or buffer too small");
                
            if (byteCount <= 256)
            {
                Span<char> chars = stackalloc char[byteCount];
                int charCount = Encoding.UTF8.GetChars(span.Slice(offset, byteCount), chars);
                value = new string(chars.Slice(0, charCount));
            }
            else
            {
                value = Encoding.UTF8.GetString(span.Slice(offset, byteCount));
            }
            
            offset += byteCount;
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a string, optimized for null and empty strings
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(string? value)
        {
            if (value == null || value.Length == 0)
                return sizeof(int);
                
            return sizeof(int) + Encoding.UTF8.GetByteCount(value);
        }
    }
} 