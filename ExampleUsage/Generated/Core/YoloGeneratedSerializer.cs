using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Generated.Maps;
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
        public void Serialize(PlayerData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(PlayerData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Position? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(Position? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(AllTypesData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(AllTypesData? obj, Span<byte> buffer, ref int offset)
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
        public PlayerData? DeserializePlayerData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<PlayerData>(buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Position? DeserializePosition(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<Position>(buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllTypesData? DeserializeAllTypesData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<AllTypesData>(buffer, ref offset);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T? obj) where T : class
        {
            return _serializer.GetSerializedSize(obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(PlayerData? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(Position? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(AllTypesData? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
    }
}
