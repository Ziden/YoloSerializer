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
    public class YoloSerializerTests
    {
        private readonly ITestOutputHelper _output;
        
        public YoloSerializerTests(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public void YoloSerializer_ShouldSerializeAndDeserializePlayerData()
        {            
            // Arrange
            var original = new PlayerData(
                playerId: 42,
                playerName: "TestPlayer",
                health: 100,
                position: new Position(1.5f, 2.5f, 3.5f),
                isActive: true
            );
            
            // Calculate required buffer size
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act - serialize
            serializer.Serialize(original, buffer, ref offset);
            
            // Verify offset advancement matches size
            Assert.Equal(size, offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.PlayerId, result!.PlayerId);
            Assert.Equal(original.PlayerName, result.PlayerName);
            Assert.Equal(original.Health, result.Health);
            Assert.Equal(original.Position.X, result.Position.X);
            Assert.Equal(original.Position.Y, result.Position.Y);
            Assert.Equal(original.Position.Z, result.Position.Z);
            Assert.Equal(original.IsActive, result.IsActive);
            Assert.Equal(size, offset);
        }

        [Fact]
        public void YoloSerializer_ShouldHandleNull()
        {
            // Arrange
            PlayerData? original = null;
            var serializer = YoloGeneratedSerializer.Instance;
            var buffer = new byte[sizeof(byte)]; // Just enough for type ID marker
            int offset = 0;

            // Act - serialize
            serializer.Serialize(original, buffer, ref offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);

            // Assert
            Assert.Null(result);
            Assert.Equal(sizeof(byte), offset);
        }
        
        [Fact]
        public void YoloSerializer_ShouldCalculateCorrectSize()
        {       
            // Arrange - Create a test player with a known string
            var player = new PlayerData(
                playerId: 42,
                playerName: "Test",
                health: 100,
                position: new Position(1.0f, 2.0f, 3.0f),
                isActive: true
            );
            
            // Act - Calculate the size using both methods
            var playerSerializer = PlayerDataSerializer.Instance;
            var entrySerializer = YoloGeneratedSerializer.Instance;
            
            // Add type ID byte for the PlayerData object itself
            int directSize = playerSerializer.GetSize(player) + sizeof(byte);
            int yoloSize = entrySerializer.GetSerializedSize(player);
            
            // Assert - Compare sizes
            Assert.Equal(directSize, yoloSize);
        }
        
        [Fact]
        public void Benchmark_PatternMatchingVsDirectInstance()
        {
            // Arrange
            const int iterations = 100000; // Reduced to speed up test
                       
            var player = new PlayerData(
                playerId: 42,
                playerName: "TestPlayer with a somewhat longer name to make serialization work harder",
                health: 100,
                position: new Position(1.5f, 2.5f, 3.5f),
                isActive: true
            );
            
            // Pre-allocate buffers
            var playerSerializer = PlayerDataSerializer.Instance;
            var entrySerializer = YoloGeneratedSerializer.Instance;
            
            int directSize = playerSerializer.GetSize(player);
            int patternSize = entrySerializer.GetSerializedSize(player);
            
            // Adjust directBuffer size to account for type IDs
            var directBuffer = new byte[directSize];
            var patternBuffer = new byte[patternSize];
            
            // Warm up
            for (int i = 0; i < 1000; i++)
            {
                int offset = 0;
                playerSerializer.Serialize(player, directBuffer, ref offset);
                
                offset = 0;
                entrySerializer.Serialize(player, patternBuffer, ref offset);
            }
            
            // Let the GC settle
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // Benchmark direct instance-based serialization
            var directWatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                int offset = 0;
                playerSerializer.Serialize(player, directBuffer, ref offset);
                
                offset = 0;
                playerSerializer.Deserialize(out PlayerData? result, directBuffer, ref offset);
            }
            directWatch.Stop();
            
            // Let the GC settle again
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // Benchmark pattern matching serialization
            var patternWatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                int offset = 0;
                entrySerializer.SerializeWithoutSizeCheck(player, patternBuffer, ref offset);
                
                offset = 0;
                var result = entrySerializer.Deserialize<PlayerData>(patternBuffer, ref offset);
            }
            patternWatch.Stop();
            
            // Output results
            _output.WriteLine($"Direct Instance: {directWatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"Pattern Matching: {patternWatch.ElapsedMilliseconds} ms");
            _output.WriteLine($"Ratio: {(double)patternWatch.ElapsedMilliseconds / directWatch.ElapsedMilliseconds:F2}x");
            
            // This test is to measure performance, not pass/fail
            Assert.True(directWatch.ElapsedMilliseconds > 0);
            Assert.True(patternWatch.ElapsedMilliseconds > 0);
        }

        [Fact]
        public void YoloSerializer_ShouldSerializeAndDeserializeWithCollections()
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
            original.Achievements.Add("First Blood");
            original.Achievements.Add("Headshot Master");
            original.Achievements.Add("Survivor");
            
            // Add stats
            original.Stats["Kills"] = 150;
            original.Stats["Deaths"] = 50;
            original.Stats["Assists"] = 75;
            
            // Calculate required buffer size
            var serializer = YoloGeneratedSerializer.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act - serialize
            serializer.Serialize(original, buffer, ref offset);
            
            // Verify offset advancement matches size
            Assert.Equal(size, offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = serializer.Deserialize<PlayerData>(buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            
            // Check basic properties
            Assert.Equal(original.PlayerId, result!.PlayerId);
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
    }
} 