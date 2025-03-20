using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.ModelsYolo;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
    [Collection("Sequential")]
    public class NullSerializationTests
    {
        private readonly ITestOutputHelper _output;
        
        public NullSerializationTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public void ShouldHandleNullString()
        {
            // Arrange
            var original = new PlayerData(
                playerId: 1,
                playerName: null, // Null string
                health: 100,
                position: new Position(1.0f, 2.0f, 3.0f),
                isActive: true
            );
            
            // Act
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;
            
            serializer.Serialize(original, buffer, ref offset);
            
            // Check serialized size consistency
            Assert.Equal(size, offset);
            
            // Deserialize
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);
            
            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.PlayerName);
            Assert.Equal(original.PlayerId, result.PlayerId);
            Assert.Equal(original.Health, result.Health);
            Assert.Equal(original.Position.X, result.Position.X);
            Assert.Equal(original.Position.Y, result.Position.Y);
            Assert.Equal(original.Position.Z, result.Position.Z);
            Assert.Equal(original.IsActive, result.IsActive);
            Assert.Equal(size, offset);
        }
        
        [Fact]
        public void ShouldHandleNullList()
        {
            // Arrange - Set up PlayerData with null list
            var original = new PlayerData(
                playerId: 2,
                playerName: "TestPlayer",
                health: 100,
                position: new Position(1.0f, 2.0f, 3.0f),
                isActive: true
            );
            
            // Use reflection to set the list to null since there's no constructor parameter for this
            typeof(PlayerData).GetProperty("Achievements")!.SetValue(original, null);
            
            // Act
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;
            
            serializer.Serialize(original, buffer, ref offset);
            
            // Check if serialized size matches
            Assert.Equal(size, offset);
            
            // Deserialize
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);
            
            // Assert
            Assert.NotNull(result);
            // Either the list should be empty or null, depending on implementation
            // Most serializers initialize empty collections on deserialization
            Assert.Null(result!.Achievements);
            Assert.Equal(size, offset);
        }
        
        [Fact]
        public void ShouldHandleEmptyList()
        {
            // Arrange
            var original = new PlayerData(
                playerId: 3,
                playerName: "TestPlayer",
                health: 100,
                position: new Position(1.0f, 2.0f, 3.0f),
                isActive: true
            );
            
            // List is already empty by default
            Assert.Empty(original.Achievements);
            
            // Act
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;
            
            serializer.Serialize(original, buffer, ref offset);
            
            // Check if serialized size matches
            Assert.Equal(size, offset);
            
            // Deserialize
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result!.Achievements);
            Assert.Empty(result.Achievements);
            Assert.Equal(size, offset);
        }
        
        [Fact]
        public void ShouldHandleListWithNullItems()
        {
            // Arrange
            var original = new PlayerData(
                playerId: 4,
                playerName: "TestPlayer",
                health: 100,
                position: new Position(1.0f, 2.0f, 3.0f),
                isActive: true
            );
            
            original.Achievements.Add("First Achievement");
            original.Achievements.Add(null); // Add null item
            original.Achievements.Add("Last Achievement");
            
            // Act
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;
            
            serializer.Serialize(original, buffer, ref offset);
            
            // Check if serialized size matches
            Assert.Equal(size, offset);
            
            // Deserialize
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result!.Achievements);
            Assert.Equal(3, result.Achievements.Count);
            Assert.Equal("First Achievement", result.Achievements[0]);
            Assert.Null(result.Achievements[1]);
            Assert.Equal("Last Achievement", result.Achievements[2]);
            Assert.Equal(size, offset);
        }

        [Fact]
        public void ShouldHandleNullDictionary()
        {
            // Arrange - Set up PlayerData with null dictionary
            var original = new PlayerData(
                playerId: 5,
                playerName: "TestPlayer",
                health: 100,
                position: new Position(1.0f, 2.0f, 3.0f),
                isActive: true
            );

            original.Stats = null!;

            // Act
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;
            
            serializer.Serialize(original, buffer, ref offset);
            
            // Check if serialized size matches
            Assert.Equal(size, offset);
            
            // Deserialize
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);
            
            // Assert
            Assert.NotNull(result);
            // The dictionary should be initialized as empty
            Assert.Null(result!.Stats);
            Assert.Equal(size, offset);
        }
        
        [Fact]
        public void ShouldHandleEmptyDictionary()
        {
            // Arrange
            var original = new PlayerData(
                playerId: 6,
                playerName: "TestPlayer",
                health: 100,
                position: new Position(1.0f, 2.0f, 3.0f),
                isActive: true
            );
            
            // Dictionary is already empty by default
            Assert.Empty(original.Stats);
            
            // Act
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;
            
            serializer.Serialize(original, buffer, ref offset);
            
            // Check if serialized size matches
            Assert.Equal(size, offset);
            
            // Deserialize
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result!.Stats);
            Assert.Empty(result.Stats);
            Assert.Equal(size, offset);
        }
        
      
        [Fact]
        public void ShouldHandleComplexNestedNulls()
        {
            // Arrange - Complex object with nested null Position
            var original = new PlayerData(
                playerId: 8,
                playerName: null,
                health: 100,
                position: new Position(0, 0, 0), // Can't be null as it's a class property without nullable annotation
                isActive: false
            );
            
            // Set up achievements with some null values
            original.Achievements = new List<string> { "Sword", null, "Shield" };
            
            // Set up stats dictionary
            original.Stats = new Dictionary<string, int>
            {
                ["Strength"] = 5,
                ["Agility"] = 10,
                ["Wisdom"] = 7
            };
            
            // Act
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;
            
            serializer.Serialize(original, buffer, ref offset);
            
            // Check if serialized size matches
            Assert.Equal(size, offset);
            
            // Deserialize
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);
            
            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.PlayerName);
            Assert.NotNull(result.Position); // Position can't be null
            
            Assert.NotNull(result.Achievements);
            Assert.Equal(3, result.Achievements.Count);
            Assert.Equal("Sword", result.Achievements[0]);
            Assert.Null(result.Achievements[1]);
            Assert.Equal("Shield", result.Achievements[2]);
            
            Assert.NotNull(result.Stats);
            Assert.Equal(3, result.Stats.Count);
            Assert.Equal(5, result.Stats["Strength"]);
            Assert.Equal(10, result.Stats["Agility"]);
            Assert.Equal(7, result.Stats["Wisdom"]);
            
            Assert.Equal(size, offset);
        }
        
        [Fact]
        public void ShouldVerifySizeConsistencyForNullValues()
        {
            // This test verifies the size calculation remains consistent
            // even with different combinations of null values
            
            // Create players with various null properties
            var player1 = new PlayerData(1, "Player", 100, new Position(1, 2, 3), true);
            var player2 = new PlayerData(2, null, 100, new Position(1, 2, 3), true);
            
            var player3 = new PlayerData(3, "Player", 100, new Position(1, 2, 3), true);
            typeof(PlayerData).GetProperty("Achievements")!.SetValue(player3, null);
            
            var player4 = new PlayerData(4, "Player", 100, new Position(1, 2, 3), true);
            typeof(PlayerData).GetProperty("Stats")!.SetValue(player4, null);
            
            var player5 = new PlayerData(5, null, 100, new Position(1, 2, 3), true);
            typeof(PlayerData).GetProperty("Achievements")!.SetValue(player5, null);
            typeof(PlayerData).GetProperty("Stats")!.SetValue(player5, null);
            
            var serializer = YoloGeneratedSerializer.Instance;
            
            // Calculate sizes
            int size1 = serializer.GetSerializedSize(player1);
            int size2 = serializer.GetSerializedSize(player2);
            int size3 = serializer.GetSerializedSize(player3);
            int size4 = serializer.GetSerializedSize(player4);
            int size5 = serializer.GetSerializedSize(player5);
            
            // Serialize and verify size consistency
            VerifySizeConsistency(player1, serializer);
            VerifySizeConsistency(player2, serializer);
            VerifySizeConsistency(player3, serializer);
            VerifySizeConsistency(player4, serializer);
            VerifySizeConsistency(player5, serializer);
            
            // Log size differences for analysis
            _output.WriteLine($"Player with name: {size1} bytes");
            _output.WriteLine($"Player with null name: {size2} bytes");
            _output.WriteLine($"Player with null achievements: {size3} bytes");
            _output.WriteLine($"Player with null stats: {size4} bytes");
            _output.WriteLine($"Player with all nulls: {size5} bytes");
            
            // Assert the size difference between player with name and without
            // At minimum, a null string should be 1 byte (null flag)
            // while a non-null string "Player" should be more than 1 byte
            Assert.True(size1 > size2);
            
            // We test the actual serialization/deserialization elsewhere,
            // here we just verify size consistency
        }
        
        private void VerifySizeConsistency<T>(T obj, YoloGeneratedSerializer serializer) where T : class
        {
            int calculatedSize = serializer.GetSerializedSize(obj);
            var buffer = new byte[calculatedSize];
            int offset = 0;
            
            serializer.Serialize(obj, buffer, ref offset);
            
            Assert.Equal(calculatedSize, offset);
        }
    }
} 