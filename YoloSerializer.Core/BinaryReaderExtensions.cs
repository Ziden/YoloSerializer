using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace YoloSerializer.Core
{
    public static class BinaryReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32(this ReadOnlySpan<byte> span, ref int offset)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(offset, sizeof(int)));
            offset += sizeof(int);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64(this ReadOnlySpan<byte> span, ref int offset)
        {
            var value = BinaryPrimitives.ReadInt64LittleEndian(span.Slice(offset, sizeof(long)));
            offset += sizeof(long);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(this ReadOnlySpan<byte> span, ref int offset)
        {
            var intBits = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(offset, sizeof(float)));
            offset += sizeof(float);
            return BitConverter.Int32BitsToSingle(intBits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ReadDouble(this ReadOnlySpan<byte> span, ref int offset)
        {
            var longBits = BinaryPrimitives.ReadInt64LittleEndian(span.Slice(offset, sizeof(double)));
            offset += sizeof(double);
            return BitConverter.Int64BitsToDouble(longBits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadBool(this ReadOnlySpan<byte> span, ref int offset)
        {
            var value = span[offset] != 0;
            offset += sizeof(byte);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? ReadString(this ReadOnlySpan<byte> span, ref int offset)
        {
            var length = span.ReadInt32(ref offset);
            if (length == -1)
            {
                return null;
            }

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = (char)BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(char)));
                offset += sizeof(char);
            }

            return new string(chars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ReadChar(this ReadOnlySpan<byte> span, ref int offset)
        {
            var value = (char)BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(char)));
            offset += sizeof(char);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16(this ReadOnlySpan<byte> span, ref int offset)
        {
            var value = BinaryPrimitives.ReadInt16LittleEndian(span.Slice(offset, sizeof(short)));
            offset += sizeof(short);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16(this ReadOnlySpan<byte> span, ref int offset)
        {
            var value = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, sizeof(ushort)));
            offset += sizeof(ushort);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32(this ReadOnlySpan<byte> span, ref int offset)
        {
            var value = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, sizeof(uint)));
            offset += sizeof(uint);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64(this ReadOnlySpan<byte> span, ref int offset)
        {
            var value = BinaryPrimitives.ReadUInt64LittleEndian(span.Slice(offset, sizeof(ulong)));
            offset += sizeof(ulong);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal ReadDecimal(this ReadOnlySpan<byte> span, ref int offset)
        {
            int[] bits = new int[4];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(offset, sizeof(int)));
                offset += sizeof(int);
            }
            return new decimal(bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ReadDateTime(this ReadOnlySpan<byte> span, ref int offset)
        {
            long ticks = span.ReadInt64(ref offset);
            return new DateTime(ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ReadTimeSpan(this ReadOnlySpan<byte> span, ref int offset)
        {
            long ticks = span.ReadInt64(ref offset);
            return new TimeSpan(ticks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid ReadGuid(this ReadOnlySpan<byte> span, ref int offset)
        {
            var result = new Guid(span.Slice(offset, 16));
            offset += 16;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum ReadEnum<TEnum>(this ReadOnlySpan<byte> span, ref int offset) 
            where TEnum : struct, Enum
        {
            int value = span.ReadInt32(ref offset);
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }
    }
} 