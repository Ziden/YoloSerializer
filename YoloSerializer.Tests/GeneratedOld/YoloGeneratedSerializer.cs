using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;
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
        public void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            _serializer.Serialize(obj, buffer, ref offset);
        }
        
        /// <summary>
        /// Serializes an object to a byte span with pre-computed size
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable
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
        public T? Deserialize<T>(ReadOnlySpan<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            return _serializer.Deserialize<T>(buffer, ref offset);
        }
        
        /// <summary>
        /// Gets the serialized size of an object
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T? obj) where T : class, IYoloSerializable
        {
            return _serializer.GetSerializedSize(obj);
        }
    }
} 