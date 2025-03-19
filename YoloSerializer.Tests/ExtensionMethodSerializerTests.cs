using System;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
    public class ExtensionMethodSerializerTests
    {
        private readonly ITestOutputHelper _output;
        
        public ExtensionMethodSerializerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void PositionSerializer_ShouldSerializeAndDeserialize()
        {
            // Arrange
            var original = new Position(1.5f, 2.5f, 3.5f);
            var buffer = new byte[PositionSerializer.SerializedSize];
            int offset = 0;

            // Act
            original.Serialize(buffer, ref offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = PositionSerializer.Deserialize(buffer, ref offset);

            // Assert
            Assert.Equal(original.X, result.X);
            Assert.Equal(original.Y, result.Y);
            Assert.Equal(original.Z, result.Z);
            Assert.Equal(PositionSerializer.SerializedSize, offset);
        }

        [Fact]
        public void PlayerDataSerializer_ShouldSerializeAndDeserializeWithName()
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
            int size = original.GetSerializedSize();
            var buffer = new byte[size];
            int offset = 0;

            // Act
            original.Serialize(buffer, ref offset);
            
            // Verify offset advancement
            Assert.Equal(size, offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = PlayerDataSerializer.Deserialize(buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.PlayerId, result.PlayerId);
            Assert.Equal(original.PlayerName, result.PlayerName);
            Assert.Equal(original.Health, result.Health);
            Assert.Equal(original.Position.X, result.Position.X);
            Assert.Equal(original.Position.Y, result.Position.Y);
            Assert.Equal(original.Position.Z, result.Position.Z);
            Assert.Equal(original.IsActive, result.IsActive);
            Assert.Equal(size, offset);
        }

        [Fact]
        public void PlayerDataSerializer_ShouldSerializeAndDeserializeWithNullName()
        {
            // Arrange
            var original = new PlayerData(
                playerId: 42,
                playerName: null,
                health: 100,
                position: new Position(1.5f, 2.5f, 3.5f),
                isActive: true
            );
            
            // Calculate required buffer size
            int size = original.GetSerializedSize();
            var buffer = new byte[size];
            int offset = 0;

            // Act
            original.Serialize(buffer, ref offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = PlayerDataSerializer.Deserialize(buffer, ref offset);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.PlayerId, result.PlayerId);
            Assert.Null(result.PlayerName);
            Assert.Equal(original.Health, result.Health);
            Assert.Equal(original.Position.X, result.Position.X);
            Assert.Equal(original.Position.Y, result.Position.Y);
            Assert.Equal(original.Position.Z, result.Position.Z);
            Assert.Equal(original.IsActive, result.IsActive);
        }

        [Fact]
        public void PlayerDataSerializer_ShouldHandleNull()
        {
            // Arrange
            PlayerData original = null;
            var buffer = new byte[sizeof(int)]; // Just enough for null marker
            int offset = 0;

            // Act
            PlayerDataSerializer.Serialize(original, buffer, ref offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = PlayerDataSerializer.Deserialize(buffer, ref offset);

            // Assert
            Assert.Null(result);
            Assert.Equal(sizeof(int), offset);
        }

        [Fact]
        public void Performance_SerializeDeserialize1000Items()
        {
            // Arrange
            const int itemCount = 1000;
            var players = new PlayerData[itemCount];
            
            // Create test data
            for (int i = 0; i < itemCount; i++)
            {
                players[i] = new PlayerData(
                    playerId: i,
                    playerName: $"Player{i}",
                    health: 100 - (i % 100),
                    position: new Position(i * 1.5f, i * 2.5f, i * 3.5f),
                    isActive: i % 2 == 0
                );
            }
            
            // Pre-calculate total buffer size
            int totalSize = 0;
            for (int i = 0; i < itemCount; i++)
            {
                totalSize += players[i].GetSerializedSize();
            }
            
            var buffer = new byte[totalSize];
            var results = new PlayerData[itemCount];
            
            // Act - Serialize
            int writeOffset = 0;
            for (int i = 0; i < itemCount; i++)
            {
                players[i].Serialize(buffer, ref writeOffset);
            }
            
            // Act - Deserialize
            int readOffset = 0;
            for (int i = 0; i < itemCount; i++)
            {
                results[i] = PlayerDataSerializer.Deserialize(buffer, ref readOffset);
            }
            
            // Assert
            Assert.Equal(totalSize, writeOffset);
            Assert.Equal(totalSize, readOffset);
            
            // Verify a few random items
            for (int i = 0; i < itemCount; i += 100)
            {
                Assert.Equal(players[i].PlayerId, results[i].PlayerId);
                Assert.Equal(players[i].PlayerName, results[i].PlayerName);
                Assert.Equal(players[i].Health, results[i].Health);
                Assert.Equal(players[i].Position.X, results[i].Position.X);
                Assert.Equal(players[i].Position.Y, results[i].Position.Y);
                Assert.Equal(players[i].Position.Z, results[i].Position.Z);
                Assert.Equal(players[i].IsActive, results[i].IsActive);
            }
        }

        [Fact]
        public void Benchmark_DirectVsPatternMatching()
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
            int directSize = player.GetSerializedSize();
            int patternSize = Core.YoloSerializer.GetSerializedSize(player);
            
            var directBuffer = new byte[directSize];
            var patternBuffer = new byte[patternSize];
            
            // Warm up
            for (int i = 0; i < 1000; i++)
            {
                int offset = 0;
                player.Serialize(directBuffer, ref offset);
                
                offset = 0;
                Core.YoloSerializer.Serialize(player, patternBuffer, ref offset);
            }
            
            // Let the GC settle
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // Benchmark direct extension method serialization
            var directWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                int offset = 0;
                player.Serialize(directBuffer, ref offset);
                
                offset = 0;
                var result = PlayerDataSerializer.Deserialize(directBuffer, ref offset);
            }
            directWatch.Stop();
            
            // Let the GC settle again
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // Benchmark pattern matching serialization
            var patternWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                int offset = 0;
                Core.YoloSerializer.SerializeWithoutSizeCheck(player, patternBuffer, ref offset);
                
                offset = 0;
                var result = Core.YoloSerializer.Deserialize<PlayerData>(patternBuffer, ref offset);
            }
            patternWatch.Stop();
            
            // Output results
            double directTime = directWatch.Elapsed.TotalMilliseconds;
            double patternTime = patternWatch.Elapsed.TotalMilliseconds;
            double ratio = patternTime / directTime;
            
            _output.WriteLine($"Direct Extension Method: {directTime:F2} ms ({iterations / directTime:F0} ops/ms)");
            _output.WriteLine($"Pattern Matching: {patternTime:F2} ms ({iterations / patternTime:F0} ops/ms)");
            _output.WriteLine($"Pattern matching overhead: {ratio:F2}x compared to direct");
            
            // Basic sanity check
            Assert.True(directTime > 0);
            Assert.True(patternTime > 0);
        }
    }
} 