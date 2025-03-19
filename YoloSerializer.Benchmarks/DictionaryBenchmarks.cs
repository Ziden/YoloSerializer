using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using YoloSerializer.Core.Serializers;
using System.Collections.Generic;

namespace YoloSerializer.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net70, launchCount: 1, warmupCount: 2, iterationCount: 3)]
    [MemoryDiagnoser(false)]
    public class DictionaryBenchmarks
    {
        private Dictionary<string, int>? _smallDict;
        private Dictionary<string, int>? _largeDict;
        private byte[] _buffer = null!;
        private DictionarySerializer<string, int> _dictionarySerializer = null!;
        private int _smallOffset;
        private int _largeOffset;

        [GlobalSetup]
        public void Setup()
        {
            _dictionarySerializer = new DictionarySerializer<string, int>(StringSerializer.Instance, Int32Serializer.Instance);
            
            // Small dictionary (under threshold)
            _smallDict = new Dictionary<string, int>();
            for (int i = 0; i < 8; i++)
                _smallDict[$"key{i}"] = i;

            // Large dictionary
            _largeDict = new Dictionary<string, int>();
            for (int i = 0; i < 1000; i++)
                _largeDict[$"key{i}"] = i;

            // Buffer large enough for any test
            _buffer = new byte[100000];

            // Pre-serialize the dictionaries to setup deserialize tests
            _smallOffset = 0;
            _largeOffset = 0;
            _dictionarySerializer.Serialize(_smallDict, _buffer, ref _smallOffset);
            _dictionarySerializer.Serialize(_largeDict, new Span<byte>(_buffer, _smallOffset, _buffer.Length - _smallOffset), ref _largeOffset);
            _largeOffset += _smallOffset; // Adjust for buffer position
        }

        [Benchmark(Baseline = true)]
        public void SerializeSmallDictionary()
        {
            int offset = 0;
            _dictionarySerializer.Serialize(_smallDict, _buffer, ref offset);
        }

        [Benchmark]
        public void DeserializeSmallDictionary()
        {
            int offset = 0;
            _dictionarySerializer.Deserialize(out Dictionary<string, int>? result, _buffer, ref offset);
        }

        [Benchmark]
        public void SerializeLargeDictionary()
        {
            int offset = _smallOffset; // Start after small dict
            _dictionarySerializer.Serialize(_largeDict, new Span<byte>(_buffer, offset, _buffer.Length - offset), ref offset);
        }

        [Benchmark]
        public void DeserializeLargeDictionary()
        {
            int offset = _smallOffset; // Start after small dict
            _dictionarySerializer.Deserialize(out Dictionary<string, int>? result, new ReadOnlySpan<byte>(_buffer, offset, _buffer.Length - offset), ref offset);
        }
    }
} 