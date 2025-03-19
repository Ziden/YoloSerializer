using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace YoloSerializer.Core
{
    public static class BinaryWriterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32(this Span<byte> span, ref int offset, int value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(offset, sizeof(int)), value);
            offset += sizeof(int);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64(this Span<byte> span, ref int offset, long value)
        {
            BinaryPrimitives.WriteInt64LittleEndian(span.Slice(offset, sizeof(long)), value);
            offset += sizeof(long);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFloat(this Span<byte> span, ref int offset, float value)
        {
            var intBits = BitConverter.SingleToInt32Bits(value);
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(offset, sizeof(float)), intBits);
            offset += sizeof(float);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDouble(this Span<byte> span, ref int offset, double value)
        {
            var longBits = BitConverter.DoubleToInt64Bits(value);
            BinaryPrimitives.WriteInt64LittleEndian(span.Slice(offset, sizeof(double)), longBits);
            offset += sizeof(double);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteBool(this Span<byte> span, ref int offset, bool value)
        {
            span[offset] = value ? (byte)1 : (byte)0;
            offset += sizeof(byte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteString(this Span<byte> span, ref int offset, string? value)
        {
            if (value == null)
            {
                span.WriteInt32(ref offset, -1);
                return;
            }

            var stringLength = value.Length;
            span.WriteInt32(ref offset, stringLength);
            
            for (int i = 0; i < stringLength; i++)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(offset, sizeof(char)), value[i]);
                offset += sizeof(char);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteChar(this Span<byte> span, ref int offset, char value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(offset, sizeof(char)), value);
            offset += sizeof(char);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16(this Span<byte> span, ref int offset, short value)
        {
            BinaryPrimitives.WriteInt16LittleEndian(span.Slice(offset, sizeof(short)), value);
            offset += sizeof(short);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16(this Span<byte> span, ref int offset, ushort value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(offset, sizeof(ushort)), value);
            offset += sizeof(ushort);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32(this Span<byte> span, ref int offset, uint value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(offset, sizeof(uint)), value);
            offset += sizeof(uint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64(this Span<byte> span, ref int offset, ulong value)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(span.Slice(offset, sizeof(ulong)), value);
            offset += sizeof(ulong);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDecimal(this Span<byte> span, ref int offset, decimal value)
        {
            var bits = decimal.GetBits(value);
            for (int i = 0; i < bits.Length; i++)
            {
                BinaryPrimitives.WriteInt32LittleEndian(span.Slice(offset, sizeof(int)), bits[i]);
                offset += sizeof(int);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDateTime(this Span<byte> span, ref int offset, DateTime value)
        {
            span.WriteInt64(ref offset, value.Ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteTimeSpan(this Span<byte> span, ref int offset, TimeSpan value)
        {
            span.WriteInt64(ref offset, value.Ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteGuid(this Span<byte> span, ref int offset, Guid value)
        {
            value.TryWriteBytes(span.Slice(offset, 16));
            offset += 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteEnum<TEnum>(this Span<byte> span, ref int offset, TEnum value) 
            where TEnum : struct, Enum
        {
            span.WriteInt32(ref offset, Convert.ToInt32(value));
        }
    }
} 