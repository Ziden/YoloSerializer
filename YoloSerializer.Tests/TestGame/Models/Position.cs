using System;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Models
{
    /// <summary>
    /// Represents a 3D position in the game world
    /// </summary>
    public class Position : IYoloSerializable
    {
        /// <summary>
        /// Type ID for serialization
        /// </summary>
        public const byte TYPE_ID = 2;

        /// <summary>
        /// Gets the type ID for serialization
        /// </summary>
        public byte TypeId => TYPE_ID;

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Position() { }
        public Position(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Position other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
    }
} 