using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Benchmarks.Generated.Maps;
using YoloSerializer.Benchmarks.Models;
namespace YoloSerializer.Core.Serializers
{
    public sealed class YoloGeneratedSerializer
    {
        public static readonly YoloGeneratedSerializer _instance = new YoloGeneratedSerializer();
        private readonly GeneratedSerializer<YoloGeneratedMap> _serializer;
        public static YoloGeneratedSerializer Instance => _instance;
        private YoloGeneratedSerializer()
        {
            _serializer = new GeneratedSerializer<YoloGeneratedMap>(YoloGeneratedMap.Instance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : class
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : class
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(SimpleData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(SimpleData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ComplexData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(ComplexData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(NestedData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(NestedData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize(buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Deserialize<T>(ReadOnlySpan<byte> buffer, ref int offset) where T : class
        {
            return _serializer.Deserialize<T>(buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleData? DeserializeSimpleData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<SimpleData>(buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ComplexData? DeserializeComplexData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<ComplexData>(buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NestedData? DeserializeNestedData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<NestedData>(buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T? obj) where T : class
        {
            return _serializer.GetSerializedSize(obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(SimpleData? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(ComplexData? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(NestedData? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
    }
}
