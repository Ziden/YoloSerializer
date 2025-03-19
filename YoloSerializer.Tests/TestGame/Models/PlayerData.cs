using System;

namespace YoloSerializer.Core.Models
{
    /// <summary>
    /// Represents a player in the game
    /// </summary>
    public class PlayerData : IYoloSerializable
    {
        /// <summary>
        /// Type ID for serialization
        /// </summary>
        public const byte TYPE_ID = 1;

        /// <summary>
        /// Gets the type ID for serialization
        /// </summary>
        public byte TypeId => TYPE_ID;

        public int PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public int Health { get; set; }
        public Position Position { get; set; }
        public bool IsActive { get; set; }

        public PlayerData()
        {
            // Default constructor
        }

        public PlayerData(int playerId, string? playerName, int health, Position position, bool isActive)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Health = health;
            Position = position;
            IsActive = isActive;
        }

        public override bool Equals(object? obj)
        {
            if (obj is PlayerData other)
            {
                return PlayerId == other.PlayerId &&
                       PlayerName == other.PlayerName &&
                       Health == other.Health &&
                       Position.Equals(other.Position) &&
                       IsActive == other.IsActive;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PlayerId, PlayerName, Health, Position, IsActive);
        }
    }
} 