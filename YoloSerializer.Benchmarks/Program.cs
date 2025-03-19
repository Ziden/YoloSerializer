using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using YoloSerializer.Core.Serializers;
using System.Collections.Generic;

namespace YoloSerializer.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net70, launchCount: 1, warmupCount: 2, iterationCount: 3)]
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
        public static void Main(string[] args)
        {
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .WithOptions(ConfigOptions.DisableOptimizationsValidator);
            
            Console.WriteLine("=== Running List Serializer Benchmarks ===");
            BenchmarkRunner.Run<SerializerBenchmarks>(config);
            
            Console.WriteLine("\n=== Running Dictionary Serializer Benchmarks ===");
           
        }
    }
}
