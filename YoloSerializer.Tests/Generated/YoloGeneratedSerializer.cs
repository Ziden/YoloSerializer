using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Tests.Generated;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer using YoloGeneratedMap for optimal dispatch
    /// </summary>
    public sealed class YoloGeneratedSerializer
    {
        private static readonly YoloGeneratedSerializer _instance = new YoloGeneratedSerializer();
        private readonly GeneratedSerializer<YoloGeneratedMap> _serializer;
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static YoloGeneratedSerializer Instance => _instance;
        
        /// <summary>
        /// Constructor - initializes with YoloGeneratedMap
        /// </summary>
        private YoloGeneratedSerializer()
        {
            _serializer = new GeneratedSerializer<YoloGeneratedMap>(YoloGeneratedMap.Instance);
        }
        
        /// <summary>
        /// Serializes an object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : class
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes an object to a byte span with pre-computed size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : class
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes a PlayerData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(PlayerData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes a Position object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Position? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes a AllTypesData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(AllTypesData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes a PlayerData object to a byte span with pre-computed size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(PlayerData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes a Position object to a byte span with pre-computed size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(Position? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes a AllTypesData object to a byte span with pre-computed size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck(AllTypesData? obj, Span<byte> buffer, ref int offset)
        {
            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Deserializes an object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize(buffer, ref offset);
        }
        
        /// <summary>
        /// Deserializes an object from a byte span with strong typing
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? Deserialize<T>(ReadOnlySpan<byte> buffer, ref int offset) where T : class
        {
            return _serializer.Deserialize<T>(buffer, ref offset);
        }
        
        /// <summary>
        /// Deserializes a PlayerData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlayerData? DeserializePlayerData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<PlayerData>(buffer, ref offset);
        }
        
        /// <summary>
        /// Deserializes a Position object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Position? DeserializePosition(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<Position>(buffer, ref offset);
        }
        
        /// <summary>
        /// Deserializes a AllTypesData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AllTypesData? DeserializeAllTypesData(ReadOnlySpan<byte> buffer, ref int offset)
        {
            return _serializer.Deserialize<AllTypesData>(buffer, ref offset);
        }
        
        /// <summary>
        /// Gets the serialized size of an object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T? obj) where T : class
        {
            return _serializer.GetSerializedSize(obj);
        }
        
        /// <summary>
        /// Gets the serialized size of a PlayerData object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(PlayerData? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
        
        /// <summary>
        /// Gets the serialized size of a Position object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(Position? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
        
        /// <summary>
        /// Gets the serialized size of a AllTypesData object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize(AllTypesData? obj)
        {
            return _serializer.GetSerializedSize(obj);
        }
        
    }
}
