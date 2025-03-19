using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for Position struct
    /// </summary>
    public sealed class PositionSerializer : ISerializer<Position>
    {
        private static readonly PositionSerializer _instance = new PositionSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static PositionSerializer Instance => _instance;
        
        private PositionSerializer() { }
        
        /// <summary>
        /// Size of a serialized Position in bytes
        /// </summary>
        public const int SerializedSize = sizeof(float) * 3; // X, Y, Z

        /// <summary>
        /// Serializes a Position struct to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Position position, Span<byte> buffer, ref int offset)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position), "Position cannot be null");
            }

            // Serialize X coordinate (float)
            FloatSerializer.Instance.Serialize(position.X, buffer, ref offset);
            
            // Serialize Y coordinate (float)
            FloatSerializer.Instance.Serialize(position.Y, buffer, ref offset);
            
            // Serialize Z coordinate (float)
            FloatSerializer.Instance.Serialize(position.Z, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a Position struct from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out Position value, ReadOnlySpan<byte> buffer, ref int offset)
        {
            value = new Position();
            
            // Deserialize X coordinate (float)
            FloatSerializer.Instance.Deserialize(out float x, buffer, ref offset);
            value.X = x;
            
            // Deserialize Y coordinate (float)
            FloatSerializer.Instance.Deserialize(out float y, buffer, ref offset);
            value.Y = y;
            
            // Deserialize Z coordinate (float)
            FloatSerializer.Instance.Deserialize(out float z, buffer, ref offset);
            value.Z = z;
        }
        
        /// <summary>
        /// Calculates the size needed to serialize a Position
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(Position position)
        {
            // Position is always a fixed size (3 floats)
            return SerializedSize;
        }
    }
} 