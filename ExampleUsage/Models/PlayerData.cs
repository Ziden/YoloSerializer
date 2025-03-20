using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Models
{
    /// <summary>
    /// Represents a player in the game
    /// </summary>
    public class PlayerData 
    {
        // Type ID is now managed by TypeRegistry

        public int PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public int Health { get; set; }
        public Position Position { get; set; }
        public bool IsActive { get; set; }
        public List<string> Achievements { get; set; } = new List<string>();
        public Dictionary<string, int> Stats { get; set; } = new Dictionary<string, int>();

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
            Achievements = new List<string>();
            Stats = new Dictionary<string, int>();
        }

        public PlayerData(int playerId, string? playerName, int health, Position position, bool isActive, 
                         List<string> achievements, Dictionary<string, int> stats)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            Health = health;
            Position = position;
            IsActive = isActive;
            Achievements = achievements ?? new List<string>();
            Stats = stats ?? new Dictionary<string, int>();
        }

        public override bool Equals(object? obj)
        {
            if (obj is PlayerData other)
            {
                // Basic properties equality check
                bool basicPropertiesEqual = 
                    PlayerId == other.PlayerId &&
                    PlayerName == other.PlayerName &&
                    Health == other.Health &&
                    Position.Equals(other.Position) &&
                    IsActive == other.IsActive;
                
                if (!basicPropertiesEqual) return false;
                
                // Check achievements list equality
                if (Achievements.Count != other.Achievements.Count) return false;
                for (int i = 0; i < Achievements.Count; i++)
                {
                    if (Achievements[i] != other.Achievements[i]) return false;
                }
                
                // Check stats dictionary equality
                if (Stats.Count != other.Stats.Count) return false;
                foreach (var key in Stats.Keys)
                {
                    if (!other.Stats.TryGetValue(key, out int otherValue) || 
                        Stats[key] != otherValue)
                        return false;
                }
                
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            // Create a consistent hash code that only considers basic properties
            int hash = PlayerId.GetHashCode();
            
            if (PlayerName != null)
                hash = hash * 23 + PlayerName.GetHashCode();
                
            hash = hash * 23 + Health.GetHashCode();
            hash = hash * 23 + Position.GetHashCode();
            hash = hash * 23 + IsActive.GetHashCode();
            
            // Hash the achievements in a consistent manner
            hash = hash * 23 + Achievements.Count.GetHashCode();
            
            foreach (var achievement in Achievements)
            {
                hash = hash * 23 + (achievement?.GetHashCode() ?? 0);
            }
            
            // Hash the stats in a consistent manner
            hash = hash * 23 + Stats.Count.GetHashCode();
            
            // Use sorted keys for consistent ordering
            var sortedKeys = Stats.Keys.OrderBy(k => k).ToList();
            foreach (var key in sortedKeys)
            {
                hash = hash * 23 + key.GetHashCode();
                hash = hash * 23 + Stats[key].GetHashCode();
            }
            
            return hash;
        }
    }
} 