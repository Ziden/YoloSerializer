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
            var serializer = GeneratedSerializerEntry.Instance;
            int size = serializer.GetSerializedSize(original);
            var buffer = new byte[size];
            int offset = 0;

            // Act - serialize
            serializer.Serialize(original, buffer, ref offset);
            
            // Verify offset advancement
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
            var serializer = GeneratedSerializerEntry.Instance;
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
            var entrySerializer = GeneratedSerializerEntry.Instance;
            
            int directSize = playerSerializer.GetSize(player) + sizeof(byte); // Add byte for type ID
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
            var entrySerializer = GeneratedSerializerEntry.Instance;
            
            int directSize = playerSerializer.GetSize(player);
            int patternSize = entrySerializer.GetSerializedSize(player);
            
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
    }
} 