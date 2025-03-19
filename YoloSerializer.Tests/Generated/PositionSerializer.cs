using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using YoloSerializer.Core.Models;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for Position struct
    /// </summary>
    public static class PositionSerializer
    {
        /// <summary>
        /// Size of a serialized Position in bytes
        /// </summary>
        public const int SerializedSize = sizeof(float) * 3; // X, Y, Z

        /// <summary>
        /// Serializes a Position struct to a byte span using direct memory access
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Serialize(this Position position, Span<byte> buffer, ref int offset)
        {
            // Ensure we have enough space (single bounds check)
            if (buffer.Length - offset < SerializedSize)
                throw new ArgumentException("Buffer too small for Position");

            // Fastest approach: direct memory copy
            // This avoids any conversion to/from byte arrays
            fixed (byte* bufferPtr = &MemoryMarshal.GetReference(buffer.Slice(offset)))
            {
                // Write X, Y, Z directly to memory
                *(float*)bufferPtr = position.X;
                *(float*)(bufferPtr + sizeof(float)) = position.Y;
                *(float*)(bufferPtr + sizeof(float) * 2) = position.Z;
            }
            
            offset += SerializedSize;
        }

        /// <summary>
        /// Deserializes a Position struct from a byte span using direct memory access
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Position Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Ensure we have enough data (single bounds check)
            if (buffer.Length - offset < SerializedSize)
                throw new ArgumentException("Buffer too small for Position");

            // Direct memory read
            var result = new Position();
            fixed (byte* bufferPtr = &MemoryMarshal.GetReference(buffer.Slice(offset)))
            {
                // Read X, Y, Z directly from memory
                result.X = *(float*)bufferPtr;
                result.Y = *(float*)(bufferPtr + sizeof(float));
                result.Z = *(float*)(bufferPtr + sizeof(float) * 2);
            }
            
            offset += SerializedSize;
            return result;
        }
    }
} 