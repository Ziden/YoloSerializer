using System;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Provides standard null handling for reference types
    /// </summary>
    public static class NullHandler
    {
        /// <summary>
        /// Standard null marker value for all serializers
        /// </summary>
        public const int NullMarker = -1;
        
        /// <summary>
        /// Writes a null marker if the value is null
        /// </summary>
        /// <returns>True if the value was null and a marker was written</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WriteNullIfNeeded<T>(T? value, Span<byte> buffer, ref int offset) where T : class
        {
            if (value == null)
            {
                Int32Serializer.Instance.Serialize(NullMarker, buffer, ref offset);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Reads a null marker and returns whether the value was null
        /// </summary>
        /// <returns>True if the value was null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ReadAndCheckNull(ReadOnlySpan<byte> buffer, ref int offset, out int marker)
        {
            Int32Serializer.Instance.Deserialize(out marker, buffer, ref offset);
            return marker == NullMarker;
        }
    }
} 