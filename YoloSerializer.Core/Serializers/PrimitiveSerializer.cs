using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// Serializer for 32-bit integers
    /// </summary>
    public sealed class Int32Serializer : ISerializer<int>
    {
        private static readonly Int32Serializer _instance = new Int32Serializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static Int32Serializer Instance => _instance;
        
        private Int32Serializer() { }
        
        /// <summary>
        /// Serializes an integer to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(int value, Span<byte> span, ref int offset) => span.WriteInt32(ref offset, value);
        
        /// <summary>
        /// Deserializes an integer from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out int value, ReadOnlySpan<byte> span, ref int offset) => value = span.ReadInt32(ref offset);
        
        /// <summary>
        /// Gets the size in bytes needed to serialize an integer (always 4)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(int value) => sizeof(int);
    }

    /// <summary>
    /// Serializer for single-precision floating point values
    /// </summary>
    public sealed class FloatSerializer : ISerializer<float>
    {
        private static readonly FloatSerializer _instance = new FloatSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static FloatSerializer Instance => _instance;
        
        private FloatSerializer() { }
        
        /// <summary>
        /// Serializes a float to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(float value, Span<byte> span, ref int offset) => span.WriteFloat(ref offset, value);
        
        /// <summary>
        /// Deserializes a float from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out float value, ReadOnlySpan<byte> span, ref int offset) => value = span.ReadFloat(ref offset);
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a float (always 4)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(float value) => sizeof(float);
    }

    /// <summary>
    /// Serializer for double-precision floating point values
    /// </summary>
    public sealed class DoubleSerializer : ISerializer<double>
    {
        private static readonly DoubleSerializer _instance = new DoubleSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static DoubleSerializer Instance => _instance;
        
        private DoubleSerializer() { }
        
        /// <summary>
        /// Serializes a double to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(double value, Span<byte> span, ref int offset) => span.WriteDouble(ref offset, value);
        
        /// <summary>
        /// Deserializes a double from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out double value, ReadOnlySpan<byte> span, ref int offset) => value = span.ReadDouble(ref offset);
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a double (always 8)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(double value) => sizeof(double);
    }

    /// <summary>
    /// Serializer for 64-bit integers
    /// </summary>
    public sealed class Int64Serializer : ISerializer<long>
    {
        private static readonly Int64Serializer _instance = new Int64Serializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static Int64Serializer Instance => _instance;
        
        private Int64Serializer() { }
        
        /// <summary>
        /// Serializes a long to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(long value, Span<byte> span, ref int offset) => span.WriteInt64(ref offset, value);
        
        /// <summary>
        /// Deserializes a long from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out long value, ReadOnlySpan<byte> span, ref int offset) => value = span.ReadInt64(ref offset);
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a long (always 8)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(long value) => sizeof(long);
    }

    /// <summary>
    /// Serializer for boolean values
    /// </summary>
    public sealed class BooleanSerializer : ISerializer<bool>
    {
        private static readonly BooleanSerializer _instance = new BooleanSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static BooleanSerializer Instance => _instance;
        
        private BooleanSerializer() { }
        
        /// <summary>
        /// Serializes a boolean to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(bool value, Span<byte> span, ref int offset) => span.WriteBool(ref offset, value);
        
        /// <summary>
        /// Deserializes a boolean from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out bool value, ReadOnlySpan<byte> span, ref int offset) => value = span.ReadBool(ref offset);
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a boolean (always 1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(bool value) => sizeof(byte);
    }
} 