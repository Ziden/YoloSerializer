using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.CodeGeneration.Generated
{
    /// <summary>
    /// Sample PlayerData class for testing serialization
    /// </summary>
    public class PlayerData : IYoloSerializable
    {
        /// <summary>
        /// Type ID for PlayerData
        /// </summary>
        public const byte TYPE_ID = 2;
        
        /// <summary>
        /// Returns the type ID for serialization
        /// </summary>
        public byte TypeId => TYPE_ID;
        
        /// <summary>
        /// Unique ID for the player
        /// </summary>
        public int PlayerId { get; set; }
        
        /// <summary>
        /// Name of the player
        /// </summary>
        public string? PlayerName { get; set; }
        
        /// <summary>
        /// Current health of the player
        /// </summary>
        public int Health { get; set; }
        
        /// <summary>
        /// Current position of the player
        /// </summary>
        public Position Position { get; set; } = new Position();
        
        /// <summary>
        /// Whether the player is active
        /// </summary>
        public bool IsActive { get; set; }
    }
} 