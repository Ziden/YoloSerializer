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
    }
} 