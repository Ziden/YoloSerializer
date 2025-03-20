using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using YoloSerializer.Core.Serializers;
using System.Collections.Generic;
using YoloSerializer.Benchmarks.CodeGeneration;
using System.Diagnostics;

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
            
            // Simple data
            Console.WriteLine("===== SIMPLE DATA =====");
            
            // Yolo serialization performance
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                benchmark.YoloSimpleSerialize();
            }
            sw.Stop();
            Console.WriteLine($"Yolo serialization: {sw.ElapsedMilliseconds}ms for 10,000 iterations");
            
            // MessagePack serialization performance
            sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                benchmark.MessagePackSimpleSerialize();
            }
            sw.Stop();
            Console.WriteLine($"MessagePack serialization: {sw.ElapsedMilliseconds}ms for 10,000 iterations");
            
            // Yolo deserialization performance
            sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                benchmark.YoloSimpleDeserialize();
            }
            sw.Stop();
            Console.WriteLine($"Yolo deserialization: {sw.ElapsedMilliseconds}ms for 10,000 iterations");
            
            // MessagePack deserialization performance
            sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                benchmark.MessagePackSimpleDeserialize();
            }
            sw.Stop();
            Console.WriteLine($"MessagePack deserialization: {sw.ElapsedMilliseconds}ms for 10,000 iterations");
            
            // Complex data
            Console.WriteLine("\n===== COMPLEX DATA =====");
            
            // Yolo serialization performance
            sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                benchmark.YoloComplexSerialize();
            }
            sw.Stop();
            Console.WriteLine($"Yolo serialization: {sw.ElapsedMilliseconds}ms for 1,000 iterations");
            
            // MessagePack serialization performance
            sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                benchmark.MessagePackComplexSerialize();
            }
            sw.Stop();
            Console.WriteLine($"MessagePack serialization: {sw.ElapsedMilliseconds}ms for 1,000 iterations");
            
            // Yolo deserialization performance
            sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                benchmark.YoloComplexDeserialize();
            }
            sw.Stop();
            Console.WriteLine($"Yolo deserialization: {sw.ElapsedMilliseconds}ms for 1,000 iterations");
            
            // MessagePack deserialization performance
            sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                benchmark.MessagePackComplexDeserialize();
            }
            sw.Stop();
            Console.WriteLine($"MessagePack deserialization: {sw.ElapsedMilliseconds}ms for 1,000 iterations");
        }
    }
}
