using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

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
        /// Serializes a Position struct to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(this Position position, Span<byte> buffer, ref int offset)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position), "Position cannot be null");
            }

            // Serialize X coordinate (float)
            FloatSerializer.Serialize(position.X, buffer, ref offset);
            
            // Serialize Y coordinate (float)
            FloatSerializer.Serialize(position.Y, buffer, ref offset);
            
            // Serialize Z coordinate (float)
            FloatSerializer.Serialize(position.Z, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a Position struct from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            var result = new Position();
            
            // Deserialize X coordinate (float)
            FloatSerializer.Deserialize(out float x, buffer, ref offset);
            result.X = x;
            
            // Deserialize Y coordinate (float)
            FloatSerializer.Deserialize(out float y, buffer, ref offset);
            result.Y = y;
            
            // Deserialize Z coordinate (float)
            FloatSerializer.Deserialize(out float z, buffer, ref offset);
            result.Z = z;
            
            return result;
        }
        
        /// <summary>
        /// Calculates the size needed to serialize a Position
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(Position position)
        {
            // Position is always a fixed size (3 floats)
            return SerializedSize;
        }
    }
} 