using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Core.CodeGeneration.Generated
{
    /// <summary>
    /// Auto-generated serializer for Position
    /// </summary>
    public static class PositionSerializer
    {
        // Constant size for a Position (three floats)
        public const int SerializedSize = sizeof(float) * 3;
        
        /// <summary>
        /// Serializes a Position object to a byte buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(Position? value, Span<byte> buffer, ref int offset)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Position cannot be null");
            }
            
            // Serialize X
            FloatSerializer.Serialize(value.X, buffer, ref offset);
            
            // Serialize Y
            FloatSerializer.Serialize(value.Y, buffer, ref offset);
            
            // Serialize Z
            FloatSerializer.Serialize(value.Z, buffer, ref offset);
        }
        
        /// <summary>
        /// Deserializes a Position object from a byte buffer
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Create a new Position
            var result = new Position();
            
            // Deserialize X
            FloatSerializer.Deserialize(out float x, buffer, ref offset);
            result.X = x;
            
            // Deserialize Y
            FloatSerializer.Deserialize(out float y, buffer, ref offset);
            result.Y = y;
            
            // Deserialize Z
            FloatSerializer.Deserialize(out float z, buffer, ref offset);
            result.Z = z;
            
            return result;
        }
        
        /// <summary>
        /// Calculates the size needed to serialize a Position object (always fixed size)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSize(Position? value)
        {
            // Fixed size regardless of contents
            return SerializedSize;
        }
    }
} 