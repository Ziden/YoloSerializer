using System;
using Xunit;
using Xunit.Abstractions;
using YoloSerializer.Core.Models;

namespace YoloSerializer.Tests
{
    public class PlayerDataEqualityTests
    {
        private readonly ITestOutputHelper _output;
        
        public PlayerDataEqualityTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public void PlayerData_EqualsMethod_ShouldWorkCorrectly()
        {
            // Create two equal but distinct objects
            var player1 = new PlayerData { 
                PlayerId = 1, 
                PlayerName = "Player", 
                Position = new Position { X = 1, Y = 2, Z = 3 },
                Health = 100,
                IsActive = true
            };
            
            var player2 = new PlayerData { 
                PlayerId = 1, 
                PlayerName = "Player", 
                Position = new Position { X = 1, Y = 2, Z = 3 },
                Health = 100,
                IsActive = true
            };
            
            // Output values for debugging
            _output.WriteLine($"Player1: ID={player1.PlayerId}, Name={player1.PlayerName}, Health={player1.Health}, IsActive={player1.IsActive}");
            _output.WriteLine($"Player1.Position: X={player1.Position.X}, Y={player1.Position.Y}, Z={player1.Position.Z}");
            _output.WriteLine($"Player2: ID={player2.PlayerId}, Name={player2.PlayerName}, Health={player2.Health}, IsActive={player2.IsActive}");
            _output.WriteLine($"Player2.Position: X={player2.Position.X}, Y={player2.Position.Y}, Z={player2.Position.Z}");
            
            // Check equality by reference
            bool sameReference = ReferenceEquals(player1, player2);
            _output.WriteLine($"ReferenceEquals: {sameReference}");
            Assert.False(sameReference, "Should be different object references");
            
            // Check equality by Equals method
            bool equalsResult = player1.Equals(player2);
            _output.WriteLine($"Equals result: {equalsResult}");
            Assert.True(equalsResult, "Equal PlayerData objects should return true from Equals");
            
            // Check equality when objects are clearly different
            var player3 = new PlayerData { 
                PlayerId = 2, // Different ID
                PlayerName = "Player", 
                Position = new Position { X = 1, Y = 2, Z = 3 },
                Health = 100,
                IsActive = true
            };
            
            bool differentEqualsResult = player1.Equals(player3);
            _output.WriteLine($"Different objects Equals result: {differentEqualsResult}");
            Assert.False(differentEqualsResult, "Different PlayerData objects should return false from Equals");
            
            // Check GetHashCode
            int hash1 = player1.GetHashCode();
            int hash2 = player2.GetHashCode();
            _output.WriteLine($"Hash1: {hash1}, Hash2: {hash2}");
            Assert.Equal(hash1, hash2);
        }
    }
} 