using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using YoloSerializer.Benchmarks.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net80, baseline: true, launchCount: 1, warmupCount: 1, iterationCount: 3)]
    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [MarkdownExporterAttribute.GitHub]
    public class YoloVsMessagePackBenchmark
    {
        // Test data
        private SimpleData _simpleData;
        private ComplexData _complexData;

        // Serialization buffers
        private byte[] _yoloBuffer;
        private byte[] _msgPackBuffer;

        // Additional buffers for complex data
        private byte[] _complexYoloBuffer;
        private byte[] _complexMsgPackBuffer;

        // Size tracking
        [Params(1, 10)]
        public int ItemCount { get; set; }

        // YoloSerializer instances
        private YoloGeneratedSerializer _yoloSerializer;

        // MessagePack serialization options
        private MessagePackSerializerOptions _msgPackOptions;

        // Properties to expose private fields for debugging
        public SimpleData SimpleData => _simpleData;
        public YoloGeneratedSerializer Serializer => _yoloSerializer;

        [GlobalSetup]
        public void Setup()
        {
            // Initialize the YoloSerializer
            _yoloSerializer = YoloGeneratedSerializer.Instance;

            // Set MessagePack options
            _msgPackOptions = MessagePackSerializerOptions.Standard;

            // Create test data
            CreateTestData();

            // Create buffers
            _yoloBuffer = new byte[100000]; // Ensure they're large enough for complex data
            _msgPackBuffer = new byte[100000];
            
            // Allocate separate buffers for complex data
            _complexYoloBuffer = new byte[1000000];
            _complexMsgPackBuffer = new byte[1000000];

            // Pre-serialize the simple data for deserialization benchmarks
            int offset = 0;
            _yoloSerializer.Serialize(_simpleData, _yoloBuffer, ref offset);
            int simpleDataSize = offset;
            _msgPackBuffer = MessagePackSerializer.Serialize(_simpleData, _msgPackOptions);
            
            // Pre-serialize the complex data for deserialization benchmarks
            offset = 0;
            _yoloSerializer.Serialize(_complexData, _complexYoloBuffer, ref offset);
            int complexDataSize = offset;
            _complexMsgPackBuffer = MessagePackSerializer.Serialize(_complexData, _msgPackOptions);
            
            Console.WriteLine($"Yolo SimpleData size: {simpleDataSize} bytes");
            Console.WriteLine($"MessagePack SimpleData size: {_msgPackBuffer.Length} bytes");
            
            Console.WriteLine($"Yolo ComplexData size: {complexDataSize} bytes");
            Console.WriteLine($"MessagePack ComplexData size: {_complexMsgPackBuffer.Length} bytes");
        }

        private void CreateTestData()
        {
            // Create simple data
            _simpleData = new SimpleData(
                id: 42,
                name: "Benchmark Test",
                isActive: true,
                value: 123.456,
                createdAt: DateTime.UtcNow,
                uniqueId: Guid.NewGuid()
            );

            // Create nested items
            var items = new NestedData[ItemCount];
            for (int i = 0; i < ItemCount; i++)
            {
                items[i] = new NestedData(
                    index: i,
                    name: $"Item {i}",
                    value: i * 10.5
                );
            }

            // Create metrics
            var metrics = new Dictionary<string, float>();
            for (int i = 0; i < Math.Min(ItemCount, 20); i++)
            {
                metrics.Add($"Metric{i}", i * 5.5f);
            }

            // Create tags
            var tags = new List<string>();
            for (int i = 0; i < Math.Min(ItemCount, 20); i++)
            {
                tags.Add($"tag{i}");
            }

            // Create complex data
            _complexData = new ComplexData(
                id: 100,
                title: $"Complex Benchmark Test with {ItemCount} items",
                metadata: _simpleData,
                status: DataStatus.Processing,
                tags: tags,
                metrics: metrics,
                items: items
            );
        }

        [Benchmark(Description = "MessagePack - Simple Serialize")]
        public byte[] MessagePackSimpleSerialize()
        {
            return MessagePackSerializer.Serialize(_simpleData, _msgPackOptions);
        }

        [Benchmark(Description = "Yolo - Simple Serialize")]
        public void YoloSimpleSerialize()
        {
            int offset = 0;
            _yoloSerializer.Serialize(_simpleData, _yoloBuffer, ref offset);
        }

        [Benchmark(Description = "MessagePack - Simple Deserialize")]
        public SimpleData MessagePackSimpleDeserialize()
        {
            return MessagePackSerializer.Deserialize<SimpleData>(_msgPackBuffer, _msgPackOptions);
        }

        [Benchmark(Description = "Yolo - Simple Deserialize")]
        public SimpleData YoloSimpleDeserialize()
        {
            int offset = 0;
            return _yoloSerializer.Deserialize<SimpleData>(new ReadOnlySpan<byte>(_yoloBuffer), ref offset);
        }

        [Benchmark(Description = "MessagePack - Complex Serialize")]
        public byte[] MessagePackComplexSerialize()
        {
            return MessagePackSerializer.Serialize(_complexData, _msgPackOptions);
        }

        [Benchmark(Description = "Yolo - Complex Serialize")]
        public void YoloComplexSerialize()
        {
            int offset = 0;
            _yoloSerializer.Serialize(_complexData, _complexYoloBuffer, ref offset);
        }

        [Benchmark(Description = "MessagePack - Complex Deserialize")]
        public ComplexData MessagePackComplexDeserialize()
        {
            return MessagePackSerializer.Deserialize<ComplexData>(_complexMsgPackBuffer, _msgPackOptions);
        }

        [Benchmark(Description = "Yolo - Complex Deserialize")]
        public ComplexData YoloComplexDeserialize()
        {
            int offset = 0;
            return _yoloSerializer.Deserialize<ComplexData>(new ReadOnlySpan<byte>(_complexYoloBuffer), ref offset);
        }
    }
} 