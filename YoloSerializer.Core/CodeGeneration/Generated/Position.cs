using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.CodeGeneration.Generated
{
    /// <summary>
    /// Sample Position class for testing serialization
    /// </summary>
    public class Position : IYoloSerializable
    {
        /// <summary>
        /// Type ID for Position
        /// </summary>
        public const byte TYPE_ID = 1;
        
        /// <summary>
        /// Returns the type ID for serialization
        /// </summary>
        public byte TypeId => TYPE_ID;
        
        /// <summary>
        /// X coordinate
        /// </summary>
        public float X { get; set; }
        
        /// <summary>
        /// Y coordinate
        /// </summary>
        public float Y { get; set; }
        
        /// <summary>
        /// Z coordinate
        /// </summary>
        public float Z { get; set; }
    }
} 