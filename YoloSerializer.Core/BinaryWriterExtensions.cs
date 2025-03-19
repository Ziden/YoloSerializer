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
    }
} 