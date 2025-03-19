using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests
{
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
            int size = Core.YoloSerializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act - serialize
            Core.YoloSerializer.Serialize(original, buffer, ref offset);
            
            // Verify offset advancement
            Assert.Equal(size, offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = Core.YoloSerializer.Deserialize<PlayerData>(buffer, ref offset);

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
            var buffer = new byte[sizeof(byte)]; // Just enough for type ID marker
            int offset = 0;

            // Act - serialize
            Core.YoloSerializer.Serialize(original, buffer, ref offset);
            
            // Reset offset for deserialization
            offset = 0;
            var result = Core.YoloSerializer.Deserialize<PlayerData>(buffer, ref offset);

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
            int directSize = player.GetSerializedSize() + sizeof(byte); // Add byte for type ID
            int yoloSize = Core.YoloSerializer.GetSerializedSize(player);
            
            // Assert - Compare sizes
            Assert.Equal(directSize, yoloSize);
        }
        
        [Fact]
        public void Benchmark_PatternMatchingVsExtensionMethods()
        {
            // Arrange
            const int iterations = 1000000;
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
            for (int i = 0; i < 10000; i++)
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
            var directWatch = System.Diagnostics.Stopwatch.StartNew();
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
            var patternWatch = System.Diagnostics.Stopwatch.StartNew();
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
            
            // This test is to measure performance, not pass/fail
            Assert.True(directTime > 0);
            Assert.True(patternTime > 0);
        }
    }
} 