using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core.Models;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for PlayerData objects
    /// </summary>
    public static class PlayerDataSerializer
    {
        // Basic field sizes plus variable string size
        private const int FixedSize = sizeof(int) + // PlayerId
                                      sizeof(int) + // String length marker
                                      sizeof(int) + // Health
                                      PositionSerializer.SerializedSize + // Position
                                      sizeof(byte); // IsActive (using byte explicitly)

        // Maximum size to allocate on stack
        private const int MaxStackAllocSize = 1024;
        
        // Maximum size for ASCII fast path
        private const int MaxFastAsciiSize = 64;

        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<PlayerData> _playerPool = 
            new ObjectPool<PlayerData>(() => new PlayerData());
            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the PlayerData
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSerializedSize(this PlayerData? player)
        {
            if (player == null)
                return sizeof(int); // Just a -1 marker for null

            // Calculate string size (UTF-8 encoding)
            int stringSize = 0;
            if (player.PlayerName != null)
            {
                // Fast path for ASCII strings
                if (IsAsciiString(player.PlayerName))
                {
                    stringSize = player.PlayerName.Length;
                }
                else
                {
                    // Only allocate for actual UTF-8 strings
                    stringSize = Encoding.UTF8.GetByteCount(player.PlayerName);
                }
            }
            
            // Fixed size + actual string bytes
            return FixedSize + stringSize;
        }

        /// <summary>
        /// Serializes a PlayerData object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(this PlayerData? player, Span<byte> buffer, ref int offset)
        {
            // Handle null case
            if (player == null)
            {
                if (buffer.Length - offset < sizeof(int))
                    throw new ArgumentException("Buffer too small for null marker");
                
                // Write -1 marker for null
                BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), -1);
                offset += sizeof(int);
                return;
            }

            // Get size and ensure buffer is large enough (single bounds check)
            int size = player.GetSerializedSize();
            if (buffer.Length - offset < size)
                throw new ArgumentException("Buffer too small for PlayerData");

            // Write PlayerId
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), player.PlayerId);
            offset += sizeof(int);

            // Write PlayerName (length-prefixed string)
            WriteStringFast(player.PlayerName, buffer, ref offset);
            
            // Write Health
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), player.Health);
            offset += sizeof(int);
            
            // Write Position using its specialized serializer
            player.Position.Serialize(buffer, ref offset);
            
            // Write IsActive as a byte (1 for true, 0 for false)
            buffer[offset] = player.IsActive ? (byte)1 : (byte)0;
            offset += sizeof(byte);
        }

        /// <summary>
        /// Deserializes a PlayerData object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PlayerData? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Ensure we have at least an int for null check
            if (buffer.Length - offset < sizeof(int))
                throw new ArgumentException("Buffer too small for null check");

            // Check for null
            int marker = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset));
            if (marker == -1)
            {
                offset += sizeof(int);
                return null;
            }

            // Ensure we have enough data for fixed size
            // String size will be checked later
            if (buffer.Length - offset < sizeof(int)) // At least one more int for string length
                throw new ArgumentException("Buffer too small for PlayerData");

            // Get a PlayerData instance from pool
            var player = _playerPool.Get();

            // Read PlayerId (marker was already read for null check)
            player.PlayerId = marker;
            offset += sizeof(int);
            
            // Read PlayerName
            player.PlayerName = ReadStringFast(buffer, ref offset);
            
            // Read Health
            player.Health = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset));
            offset += sizeof(int);
            
            // Read Position using its specialized deserializer
            player.Position = PositionSerializer.Deserialize(buffer, ref offset);
            
            // Read IsActive - using explicit byte (1 is true, 0 is false)
            player.IsActive = buffer[offset] != 0;
            offset += sizeof(byte);

            return player;
        }
        
        /// <summary>
        /// Fast string serialization with multiple optimized paths
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteStringFast(string? value, Span<byte> buffer, ref int offset)
        {
            if (value == null)
            {
                // Write -1 to indicate null string
                BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), -1);
                offset += sizeof(int);
                return;
            }
            
            if (value.Length == 0)
            {
                // Empty string optimization (0 length, no bytes)
                BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), 0);
                offset += sizeof(int);
                return;
            }
            
            // Fast path for ASCII strings (most common in gaming)
            if (value.Length <= MaxFastAsciiSize && IsAsciiString(value))
            {
                // Write string length
                BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), value.Length);
                offset += sizeof(int);
                
                // Direct character-to-byte conversion for ASCII
                for (int i = 0; i < value.Length; i++)
                {
                    buffer[offset++] = (byte)value[i];
                }
            }
            else 
            {
                // Standard handling for UTF-8 strings
                byte[] stringBytes = Encoding.UTF8.GetBytes(value);
                
                // Write string length
                BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(offset), stringBytes.Length);
                offset += sizeof(int);
                
                // Copy string bytes
                stringBytes.CopyTo(buffer.Slice(offset));
                offset += stringBytes.Length;
            }
        }
        
        /// <summary>
        /// Fast string deserialization with multiple optimized paths
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? ReadStringFast(ReadOnlySpan<byte> buffer, ref int offset)
        {
            // Read string length
            int stringLength = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(offset));
            offset += sizeof(int);
            
            if (stringLength == -1)
            {
                // Null string
                return null;
            }
            
            if (stringLength == 0)
            {
                // Empty string
                return string.Empty;
            }
            
            // Check buffer size
            if (buffer.Length - offset < stringLength)
                throw new ArgumentException("Buffer too small for string");
            
            // Fast path for ASCII strings
            bool isAscii = true;
            for (int i = 0; i < Math.Min(stringLength, MaxFastAsciiSize); i++)
            {
                if (buffer[offset + i] > 127)
                {
                    isAscii = false;
                    break;
                }
            }
            
            if (stringLength <= MaxFastAsciiSize && isAscii)
            {
                // Stack allocate small strings
                Span<char> chars = stackalloc char[stringLength];
                
                // Direct byte-to-char conversion for ASCII
                for (int i = 0; i < stringLength; i++)
                {
                    chars[i] = (char)buffer[offset + i];
                }
                
                string result = new string(chars);
                offset += stringLength;
                return result;
            }
            else
            {
                // UTF-8 decoding for non-ASCII
                string result;
                
                if (stringLength <= MaxStackAllocSize)
                {
                    // Stack allocate
                    Span<byte> stringBytes = stackalloc byte[stringLength];
                    buffer.Slice(offset, stringLength).CopyTo(stringBytes);
                    result = Encoding.UTF8.GetString(stringBytes);
                }
                else
                {
                    // Heap allocate
                    byte[] stringBytes = ArrayPool<byte>.Shared.Rent(stringLength);
                    try
                    {
                        buffer.Slice(offset, stringLength).CopyTo(stringBytes);
                        result = Encoding.UTF8.GetString(stringBytes, 0, stringLength);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(stringBytes);
                    }
                }
                
                offset += stringLength;
                return result;
            }
        }
        
        /// <summary>
        /// Checks if a string contains only ASCII characters
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAsciiString(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] > 127)
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Simple object pool implementation to reduce allocations
    /// </summary>
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Func<T> _factory;
        private readonly T[] _items;
        private int _count;

        public ObjectPool(Func<T>? factory = null, int size = 32)
        {
            _factory = factory ?? (() => new T());
            _items = new T[size];
        }

        public T Get()
        {
            T? item = null;
            
            lock (_items)
            {
                if (_count > 0)
                {
                    item = _items[--_count];
                    _items[_count] = null!;
                }
            }

            return item ?? _factory();
        }

        public void Return(T item)
        {
            lock (_items)
            {
                if (_count < _items.Length)
                {
                    _items[_count++] = item;
                }
            }
        }
    }
} 