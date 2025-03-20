using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using YoloSerializer.Core.Serializers;
using System.Collections.Generic;
using YoloSerializer.Benchmarks.CodeGeneration;
using System.Diagnostics;
using System;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;

namespace YoloSerializer.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 2, iterationCount: 3)]
    [MemoryDiagnoser(false)]
    public class SerializerBenchmarks
    {
        private List<int>? _smallList;
        private List<int>? _largeList;
        private byte[] _buffer = null!;
        private ListSerializer<int> _listSerializer = null!;
        private int _smallOffset;
        private int _largeOffset;

        [GlobalSetup]
        public void Setup()
        {
            _listSerializer = new ListSerializer<int>(Int32Serializer.Instance);
            
            // Small list (under threshold)
            _smallList = new List<int>();
            for (int i = 0; i < 8; i++)
                _smallList.Add(i);

            // Large list
            _largeList = new List<int>();
            for (int i = 0; i < 1000; i++)
                _largeList.Add(i);

            // Buffer large enough for any test
            _buffer = new byte[10000];

            // Pre-serialize the lists to setup deserialize tests
            _smallOffset = 0;
            _largeOffset = 0;
            _listSerializer.Serialize(_smallList, _buffer, ref _smallOffset);
            _listSerializer.Serialize(_largeList, new Span<byte>(_buffer, _smallOffset, _buffer.Length - _smallOffset), ref _largeOffset);
            _largeOffset += _smallOffset; // Adjust for buffer position
        }

        [Benchmark(Baseline = true)]
        public void SerializeSmallList()
        {
            int offset = 0;
            _listSerializer.Serialize(_smallList, _buffer, ref offset);
        }

        [Benchmark]
        public void DeserializeSmallList()
        {
            int offset = 0;
            _listSerializer.Deserialize(out List<int>? result, _buffer, ref offset);
        }

        [Benchmark]
        public void SerializeLargeList()
        {
            int offset = _smallOffset; // Start after small list
            _listSerializer.Serialize(_largeList, new Span<byte>(_buffer, offset, _buffer.Length - offset), ref offset);
        }

        [Benchmark]
        public void DeserializeLargeList()
        {
            int offset = _smallOffset; // Start after small list
            _listSerializer.Deserialize(out List<int>? result, new ReadOnlySpan<byte>(_buffer, offset, _buffer.Length - offset), ref offset);
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Contains("--quick-test"))
                {
                    Console.WriteLine("Running quick comparison test between Yolo and MessagePack...");
                    RunQuickComparisonTest();
                    return;
                }

                // Ensure serializers are generated for benchmark models
                Console.WriteLine("Generating serializers for benchmark models...");
                SerializerGenerator.GenerateSerializers();

                if (args.Contains("--list-only"))
                {
                    BenchmarkRunner.Run<SerializerBenchmarks>();
                }
                else if (args.Contains("--yolo-vs-msgpack"))
                {
                    BenchmarkRunner.Run<YoloVsMessagePackBenchmark>();
                }
                else
                {
                    BenchmarkRunner.Run<SerializerBenchmarks>();
                    BenchmarkRunner.Run<YoloVsMessagePackBenchmark>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        static void RunQuickComparisonTest()
        {
            // Create benchmark instance
            var benchmark = new YoloVsMessagePackBenchmark();
            benchmark.ItemCount = 10;
            benchmark.Setup();
            
            // Size comparison
            Console.WriteLine("\n===== SIZE COMPARISON =====");
            
            // Simple data serialization size
            int yoloSimpleOffset = 0;
            var yoloSimpleBuffer = new byte[10000];
            benchmark.Serializer.Serialize(benchmark.SimpleData, yoloSimpleBuffer, ref yoloSimpleOffset);
            
            var msgPackSimpleBuffer = MessagePackSerializer.Serialize(benchmark.SimpleData);
            
            Console.WriteLine($"SimpleData size:");
            Console.WriteLine($"  Yolo:       {yoloSimpleOffset} bytes");
            Console.WriteLine($"  MessagePack: {msgPackSimpleBuffer.Length} bytes");
            Console.WriteLine($"  Ratio:       {(double)yoloSimpleOffset / msgPackSimpleBuffer.Length:F2}x larger");
            
            // Complex data serialization size
            int yoloComplexOffset = 0;
            var yoloComplexBuffer = new byte[100000];
            benchmark.Serializer.Serialize(benchmark.ComplexData, yoloComplexBuffer, ref yoloComplexOffset);
            
            var msgPackComplexBuffer = MessagePackSerializer.Serialize(benchmark.ComplexData);
            
            Console.WriteLine($"\nComplexData size:");
            Console.WriteLine($"  Yolo:       {yoloComplexOffset} bytes");
            Console.WriteLine($"  MessagePack: {msgPackComplexBuffer.Length} bytes");
            Console.WriteLine($"  Ratio:       {(double)yoloComplexOffset / msgPackComplexBuffer.Length:F2}x larger");
            
            // Explain the differences
            Console.WriteLine("\nSize difference factors:");
            Console.WriteLine("1. Type metadata overhead in YoloSerializer");
            Console.WriteLine("2. MessagePack's variable-length encoding for numbers");
            Console.WriteLine("3. Different string encoding efficiency");
            Console.WriteLine("4. Different DateTime and Guid representations");
            
            // Simple data serialization performance
            Console.WriteLine("\n===== SIMPLE DATA =====");
            
            // Yolo serialization
            var sw = Stopwatch.StartNew();
            const int iterations = 100000;
            
            for (int i = 0; i < iterations; i++)
            {
                benchmark.YoloSimpleSerialize();
            }
            sw.Stop();
            double yoloSerMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"Yolo serialization: {yoloSerMs:F2}ms for {iterations:N0} iterations ({iterations/yoloSerMs:F2}k ops/ms)");
            
            // MessagePack serialization
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                benchmark.MessagePackSimpleSerialize();
            }
            sw.Stop();
            double msgpackSerMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"MessagePack serialization: {msgpackSerMs:F2}ms for {iterations:N0} iterations ({iterations/msgpackSerMs:F2}k ops/ms)");
            Console.WriteLine($"Performance ratio: Yolo is {msgpackSerMs/yoloSerMs:F2}x faster at serialization");
            
            // Yolo deserialization
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                benchmark.YoloSimpleDeserialize();
            }
            sw.Stop();
            double yoloDeserMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"Yolo deserialization: {yoloDeserMs:F2}ms for {iterations:N0} iterations ({iterations/yoloDeserMs:F2}k ops/ms)");
            
            // MessagePack deserialization
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                benchmark.MessagePackSimpleDeserialize();
            }
            sw.Stop();
            double msgpackDeserMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"MessagePack deserialization: {msgpackDeserMs:F2}ms for {iterations:N0} iterations ({iterations/msgpackDeserMs:F2}k ops/ms)");
            Console.WriteLine($"Performance ratio: Yolo is {msgpackDeserMs/yoloDeserMs:F2}x faster at deserialization");
            
            // Complex data
            Console.WriteLine("\n===== COMPLEX DATA =====");
            const int complexIterations = 10000;
            
            // Yolo serialization
            sw.Restart();
            for (int i = 0; i < complexIterations; i++)
            {
                benchmark.YoloComplexSerialize();
            }
            sw.Stop();
            double yoloComplexSerMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"Yolo serialization: {yoloComplexSerMs:F2}ms for {complexIterations:N0} iterations ({complexIterations/yoloComplexSerMs:F2}k ops/ms)");
            
            // MessagePack serialization
            sw.Restart();
            for (int i = 0; i < complexIterations; i++)
            {
                benchmark.MessagePackComplexSerialize();
            }
            sw.Stop();
            double msgpackComplexSerMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"MessagePack serialization: {msgpackComplexSerMs:F2}ms for {complexIterations:N0} iterations ({complexIterations/msgpackComplexSerMs:F2}k ops/ms)");
            Console.WriteLine($"Performance ratio: Yolo is {msgpackComplexSerMs/yoloComplexSerMs:F2}x faster at serialization");
            
            // Yolo deserialization
            sw.Restart();
            for (int i = 0; i < complexIterations; i++)
            {
                benchmark.YoloComplexDeserialize();
            }
            sw.Stop();
            double yoloComplexDeserMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"Yolo deserialization: {yoloComplexDeserMs:F2}ms for {complexIterations:N0} iterations ({complexIterations/yoloComplexDeserMs:F2}k ops/ms)");
            
            // MessagePack deserialization
            sw.Restart();
            for (int i = 0; i < complexIterations; i++)
            {
                benchmark.MessagePackComplexDeserialize();
            }
            sw.Stop();
            double msgpackComplexDeserMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"MessagePack deserialization: {msgpackComplexDeserMs:F2}ms for {complexIterations:N0} iterations ({complexIterations/msgpackComplexDeserMs:F2}k ops/ms)");
            Console.WriteLine($"Performance ratio: Yolo is {msgpackComplexDeserMs/yoloComplexDeserMs:F2}x faster at deserialization");
            
            // Summary
            Console.WriteLine("\n===== PERFORMANCE SUMMARY =====");
            Console.WriteLine($"Simple Data - Serialization: Yolo is {msgpackSerMs/yoloSerMs:F2}x faster");
            Console.WriteLine($"Simple Data - Deserialization: Yolo is {msgpackDeserMs/yoloDeserMs:F2}x faster");
            Console.WriteLine($"Complex Data - Serialization: Yolo is {msgpackComplexSerMs/yoloComplexSerMs:F2}x faster");
            Console.WriteLine($"Complex Data - Deserialization: Yolo is {msgpackComplexDeserMs/yoloComplexDeserMs:F2}x faster");
        }
    }
}
