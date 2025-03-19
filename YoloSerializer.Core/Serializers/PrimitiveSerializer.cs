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

    /// <summary>
    /// Serializer for byte values
    /// </summary>
    public sealed class ByteSerializer : ISerializer<byte>
    {
        private static readonly ByteSerializer _instance = new ByteSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static ByteSerializer Instance => _instance;
        
        private ByteSerializer() { }
        
        /// <summary>
        /// Serializes a byte to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(byte value, Span<byte> span, ref int offset)
        {
            span[offset] = value;
            offset += sizeof(byte);
        }
        
        /// <summary>
        /// Deserializes a byte from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out byte value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span[offset];
            offset += sizeof(byte);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a byte (always 1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(byte value) => sizeof(byte);
    }

    /// <summary>
    /// Serializer for signed byte values
    /// </summary>
    public sealed class SByteSerializer : ISerializer<sbyte>
    {
        private static readonly SByteSerializer _instance = new SByteSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static SByteSerializer Instance => _instance;
        
        private SByteSerializer() { }
        
        /// <summary>
        /// Serializes a signed byte to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(sbyte value, Span<byte> span, ref int offset)
        {
            span[offset] = (byte)value;
            offset += sizeof(sbyte);
        }
        
        /// <summary>
        /// Deserializes a signed byte from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out sbyte value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = (sbyte)span[offset];
            offset += sizeof(sbyte);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a signed byte (always 1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(sbyte value) => sizeof(sbyte);
    }

    /// <summary>
    /// Serializer for character values
    /// </summary>
    public sealed class CharSerializer : ISerializer<char>
    {
        private static readonly CharSerializer _instance = new CharSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static CharSerializer Instance => _instance;
        
        private CharSerializer() { }
        
        /// <summary>
        /// Serializes a character to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(char value, Span<byte> span, ref int offset)
        {
            span.WriteChar(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a character from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out char value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadChar(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a character (always 2)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(char value) => sizeof(char);
    }

    /// <summary>
    /// Serializer for 16-bit integers
    /// </summary>
    public sealed class Int16Serializer : ISerializer<short>
    {
        private static readonly Int16Serializer _instance = new Int16Serializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static Int16Serializer Instance => _instance;
        
        private Int16Serializer() { }
        
        /// <summary>
        /// Serializes a short to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(short value, Span<byte> span, ref int offset)
        {
            span.WriteInt16(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a short from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out short value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadInt16(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a short (always 2)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(short value) => sizeof(short);
    }

    /// <summary>
    /// Serializer for unsigned 16-bit integers
    /// </summary>
    public sealed class UInt16Serializer : ISerializer<ushort>
    {
        private static readonly UInt16Serializer _instance = new UInt16Serializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static UInt16Serializer Instance => _instance;
        
        private UInt16Serializer() { }
        
        /// <summary>
        /// Serializes an unsigned short to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ushort value, Span<byte> span, ref int offset)
        {
            span.WriteUInt16(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes an unsigned short from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out ushort value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadUInt16(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize an unsigned short (always 2)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(ushort value) => sizeof(ushort);
    }

    /// <summary>
    /// Serializer for unsigned 32-bit integers
    /// </summary>
    public sealed class UInt32Serializer : ISerializer<uint>
    {
        private static readonly UInt32Serializer _instance = new UInt32Serializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static UInt32Serializer Instance => _instance;
        
        private UInt32Serializer() { }
        
        /// <summary>
        /// Serializes an unsigned integer to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(uint value, Span<byte> span, ref int offset)
        {
            span.WriteUInt32(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes an unsigned integer from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out uint value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadUInt32(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize an unsigned integer (always 4)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(uint value) => sizeof(uint);
    }

    /// <summary>
    /// Serializer for unsigned 64-bit integers
    /// </summary>
    public sealed class UInt64Serializer : ISerializer<ulong>
    {
        private static readonly UInt64Serializer _instance = new UInt64Serializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static UInt64Serializer Instance => _instance;
        
        private UInt64Serializer() { }
        
        /// <summary>
        /// Serializes an unsigned long to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ulong value, Span<byte> span, ref int offset)
        {
            span.WriteUInt64(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes an unsigned long from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out ulong value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadUInt64(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize an unsigned long (always 8)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(ulong value) => sizeof(ulong);
    }

    /// <summary>
    /// Serializer for decimal values
    /// </summary>
    public sealed class DecimalSerializer : ISerializer<decimal>
    {
        private static readonly DecimalSerializer _instance = new DecimalSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static DecimalSerializer Instance => _instance;
        
        private DecimalSerializer() { }
        
        /// <summary>
        /// Serializes a decimal to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(decimal value, Span<byte> span, ref int offset)
        {
            span.WriteDecimal(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a decimal from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out decimal value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadDecimal(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a decimal (always 16)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(decimal value) => sizeof(decimal);
    }

    /// <summary>
    /// Serializer for DateTime values
    /// </summary>
    public sealed class DateTimeSerializer : ISerializer<DateTime>
    {
        private static readonly DateTimeSerializer _instance = new DateTimeSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static DateTimeSerializer Instance => _instance;
        
        private DateTimeSerializer() { }
        
        /// <summary>
        /// Serializes a DateTime to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(DateTime value, Span<byte> span, ref int offset)
        {
            span.WriteDateTime(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a DateTime from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out DateTime value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadDateTime(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a DateTime (always 8)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(DateTime value) => sizeof(long); // DateTime is stored as a 64-bit integer
    }

    /// <summary>
    /// Serializer for TimeSpan values
    /// </summary>
    public sealed class TimeSpanSerializer : ISerializer<TimeSpan>
    {
        private static readonly TimeSpanSerializer _instance = new TimeSpanSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static TimeSpanSerializer Instance => _instance;
        
        private TimeSpanSerializer() { }
        
        /// <summary>
        /// Serializes a TimeSpan to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(TimeSpan value, Span<byte> span, ref int offset)
        {
            span.WriteTimeSpan(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a TimeSpan from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out TimeSpan value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadTimeSpan(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a TimeSpan (always 8)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(TimeSpan value) => sizeof(long); // TimeSpan is stored as a 64-bit integer
    }

    /// <summary>
    /// Serializer for Guid values
    /// </summary>
    public sealed class GuidSerializer : ISerializer<Guid>
    {
        private static readonly GuidSerializer _instance = new GuidSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static GuidSerializer Instance => _instance;
        
        private GuidSerializer() { }
        
        /// <summary>
        /// Serializes a Guid to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(Guid value, Span<byte> span, ref int offset)
        {
            span.WriteGuid(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes a Guid from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out Guid value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadGuid(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize a Guid (always 16)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(Guid value) => 16; // Guid is 16 bytes
    }

    /// <summary>
    /// Generic serializer for enum values
    /// </summary>
    public sealed class EnumSerializer<TEnum> : ISerializer<TEnum> where TEnum : struct, Enum
    {
        private static readonly EnumSerializer<TEnum> _instance = new EnumSerializer<TEnum>();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static EnumSerializer<TEnum> Instance => _instance;
        
        private EnumSerializer() { }
        
        /// <summary>
        /// Serializes an enum to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(TEnum value, Span<byte> span, ref int offset)
        {
            span.WriteEnum(ref offset, value);
        }
        
        /// <summary>
        /// Deserializes an enum from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out TEnum value, ReadOnlySpan<byte> span, ref int offset)
        {
            value = span.ReadEnum<TEnum>(ref offset);
        }
        
        /// <summary>
        /// Gets the size in bytes needed to serialize an enum 
        /// (depends on the underlying type, defaults to 4 bytes for int)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(TEnum value) => sizeof(int); // Most enums are backed by int
    }
} 