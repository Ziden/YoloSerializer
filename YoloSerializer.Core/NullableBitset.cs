using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Handles bitsets for nullable fields in a serialized object
    /// </summary>
    public static class NullableBitset
    {
        /// <summary>
        /// Maximum number of nullable fields that can be represented in a single byte
        /// </summary>
        private const int BitsPerByte = 8;

        /// <summary>
        /// Calculates the number of bytes needed to store a bitset for a given number of nullable fields
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBitsetSize(int nullableFieldCount)
        {
            if (nullableFieldCount <= 0)
                return 0;
                
            return (nullableFieldCount + BitsPerByte - 1) / BitsPerByte;
        }

        /// <summary>
        /// Sets a bit in the bitset indicating whether a nullable field is null or not
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(Span<byte> bitset, int fieldIndex, bool isNull)
        {
            int byteIndex = fieldIndex / BitsPerByte;
            int bitPosition = fieldIndex % BitsPerByte;
            
            if (isNull)
                bitset[byteIndex] |= (byte)(1 << bitPosition); // Set bit to 1 for null
            else
                bitset[byteIndex] &= (byte)~(1 << bitPosition); // Set bit to 0 for non-null
        }

        /// <summary>
        /// Checks if a particular field is null based on the bitset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(ReadOnlySpan<byte> bitset, int fieldIndex)
        {
            int byteIndex = fieldIndex / BitsPerByte;
            int bitPosition = fieldIndex % BitsPerByte;
            
            return (bitset[byteIndex] & (1 << bitPosition)) != 0;
        }

        /// <summary>
        /// Serializes the bitset to the output buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeBitset(Span<byte> bitset, int bitsetSize, Span<byte> buffer, ref int offset)
        {
            if (bitsetSize <= 0)
                return;
                
            bitset.Slice(0, bitsetSize).CopyTo(buffer.Slice(offset));
            offset += bitsetSize;
        }

        /// <summary>
        /// Deserializes the bitset from the input buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeserializeBitset(ReadOnlySpan<byte> buffer, ref int offset, Span<byte> bitset, int bitsetSize)
        {
            if (bitsetSize <= 0)
                return;
                
            buffer.Slice(offset, bitsetSize).CopyTo(bitset);
            offset += bitsetSize;
        }
    }
} 