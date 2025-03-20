using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.ModelsYolo;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
    public class InstanceBasedSerializerTests
    {
        private readonly ITestOutputHelper _output;
        
        public InstanceBasedSerializerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ... existing code ...

        [Fact]
        public void PlayerDataSerializer_ShouldHandleListAndDictionary()
        {
            // Arrange
            var original = new PlayerData(
                playerId: 42,
                playerName: "TestPlayer",
                health: 100,
                position: new Position(1.5f, 2.5f, 3.5f),
                isActive: true
            );
            
            // Add achievements
            original.Achievements.Add("First Kill");
            original.Achievements.Add("Level 10");
            original.Achievements.Add("Boss Defeated");
            
            // Add stats
            original.Stats["Strength"] = 10;
            original.Stats["Dexterity"] = 15;
            original.Stats["Intelligence"] = 20;
            
            // Calculate required buffer size
            var serializer = PlayerDataSerializer.Instance;
            int size = serializer.GetSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            // Reset offset for deserialization
            offset = 0;
            serializer.Deserialize(out PlayerData? result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            
            // Check basic properties
            Assert.Equal(original.PlayerId, result.PlayerId);
            Assert.Equal(original.PlayerName, result.PlayerName);
            Assert.Equal(original.Health, result.Health);
            Assert.Equal(original.Position.X, result.Position.X);
            Assert.Equal(original.Position.Y, result.Position.Y);
            Assert.Equal(original.Position.Z, result.Position.Z);
            Assert.Equal(original.IsActive, result.IsActive);
            
            // Check achievements list
            Assert.Equal(original.Achievements.Count, result.Achievements.Count);
            for (int i = 0; i < original.Achievements.Count; i++)
            {
                Assert.Equal(original.Achievements[i], result.Achievements[i]);
            }
            
            // Check stats dictionary
            Assert.Equal(original.Stats.Count, result.Stats.Count);
            foreach (var key in original.Stats.Keys)
            {
                Assert.True(result.Stats.ContainsKey(key));
                Assert.Equal(original.Stats[key], result.Stats[key]);
            }
        }

        [Fact]
        public void PlayerDataSerializer_ShouldHandleEmptyCollections()
        {
            // Arrange
            var original = new PlayerData(
                playerId: 42,
                playerName: "TestPlayer",
                health: 100,
                position: new Position(1.5f, 2.5f, 3.5f),
                isActive: true
            );
            
            // Ensure collections are empty
            original.Achievements.Clear();
            original.Stats.Clear();
            
            // Calculate required buffer size
            var serializer = PlayerDataSerializer.Instance;
            int size = serializer.GetSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act
            serializer.Serialize(original, buffer, ref offset);
            
            // Reset offset for deserialization
            offset = 0;
            serializer.Deserialize(out PlayerData? result, buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Achievements);
            Assert.Empty(result.Stats);
            
            // Check basic properties are still correct
            Assert.Equal(original.PlayerId, result.PlayerId);
            Assert.Equal(original.PlayerName, result.PlayerName);
            Assert.Equal(original.Health, result.Health);
            Assert.Equal(original.Position.X, result.Position.X);
            Assert.Equal(original.Position.Y, result.Position.Y);
            Assert.Equal(original.Position.Z, result.Position.Z);
            Assert.Equal(original.IsActive, result.IsActive);
        }
        
        // ... rest of existing code ...
    }
} 