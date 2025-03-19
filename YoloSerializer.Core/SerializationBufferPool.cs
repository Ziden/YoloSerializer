using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Provides reusable buffers for serialization operations to reduce GC pressure
    /// </summary>
    public static class SerializationBufferPool
    {
        // Default sizes for commonly used buffers
        private const int SMALL_BUFFER_SIZE = 1024;   // 1KB - good for small objects
        private const int MEDIUM_BUFFER_SIZE = 16384; // 16KB - good for typical game objects
        private const int LARGE_BUFFER_SIZE = 65536;  // 64KB - good for collections
        
        // Maximum number of pooled arrays per size
        private const int MAX_POOL_SIZE = 32;
        
        // Simple buffer pools - using ConcurrentBag for thread safety
        private static readonly ConcurrentBag<byte[]> _smallPool = new ConcurrentBag<byte[]>();
        private static readonly ConcurrentBag<byte[]> _mediumPool = new ConcurrentBag<byte[]>();
        private static readonly ConcurrentBag<byte[]> _largePool = new ConcurrentBag<byte[]>();
        
        // Stats - only updated during debug builds for better performance
        private static int _rentCount = 0;
        private static int _returnCount = 0;
        private static int _missCount = 0;
        
        /// <summary>
        /// Gets a buffer of the specified size. The buffer may be larger than requested.
        /// </summary>
        /// <param name="minSize">The minimum size needed</param>
        /// <returns>A buffer that is at least the requested size</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Rent(int minSize)
        {
            #if DEBUG
            _rentCount++;
            #endif
            
            // Get appropriate buffer size
            if (minSize <= SMALL_BUFFER_SIZE)
            {
                if (_smallPool.TryTake(out byte[] buffer))
                    return buffer;
                
                #if DEBUG
                _missCount++;
                #endif
                
                return new byte[SMALL_BUFFER_SIZE];
            }
            
            if (minSize <= MEDIUM_BUFFER_SIZE)
            {
                if (_mediumPool.TryTake(out byte[] buffer))
                    return buffer;
                
                #if DEBUG
                _missCount++;
                #endif
                
                return new byte[MEDIUM_BUFFER_SIZE];
            }
            
            if (minSize <= LARGE_BUFFER_SIZE)
            {
                if (_largePool.TryTake(out byte[] buffer))
                    return buffer;
                
                #if DEBUG
                _missCount++;
                #endif
                
                return new byte[LARGE_BUFFER_SIZE];
            }
            
            // For very large buffers, use ArrayPool or create a new one
            #if DEBUG
            _missCount++;
            #endif
            
            return new byte[minSize];
        }
        
        /// <summary>
        /// Returns a buffer to the pool
        /// </summary>
        /// <param name="buffer">The buffer to return</param>
        /// <param name="clearBuffer">Whether to clear sensitive data from the buffer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(byte[] buffer, bool clearBuffer = false)
        {
            if (buffer == null)
                return;
            
            #if DEBUG
            _returnCount++;
            #endif
            
            // Clear the buffer if requested
            if (clearBuffer)
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
            
            // Return to appropriate pool if not too full
            if (buffer.Length == SMALL_BUFFER_SIZE && _smallPool.Count < MAX_POOL_SIZE)
            {
                _smallPool.Add(buffer);
                return;
            }
            
            if (buffer.Length == MEDIUM_BUFFER_SIZE && _mediumPool.Count < MAX_POOL_SIZE)
            {
                _mediumPool.Add(buffer);
                return;
            }
            
            if (buffer.Length == LARGE_BUFFER_SIZE && _largePool.Count < MAX_POOL_SIZE)
            {
                _largePool.Add(buffer);
                return;
            }
            
            // Buffer is not pooled (very large or pool is full)
            // Just let it be garbage collected
        }
        
        /// <summary>
        /// Gets statistics about the buffer pool usage
        /// </summary>
        /// <returns>A string containing pool statistics</returns>
        public static string GetStatistics()
        {
            return $"Rents: {_rentCount}, Returns: {_returnCount}, Misses: {_missCount}, " +
                   $"Hit Rate: {(_rentCount > 0 ? 100 - (_missCount * 100.0 / _rentCount) : 0):F2}%, " +
                   $"Small: {_smallPool.Count}/{MAX_POOL_SIZE}, " +
                   $"Medium: {_mediumPool.Count}/{MAX_POOL_SIZE}, " +
                   $"Large: {_largePool.Count}/{MAX_POOL_SIZE}";
        }
        
        /// <summary>
        /// Clears all pooled buffers and resets statistics
        /// </summary>
        public static void Clear()
        {
            // In a real threaded environment, this wouldn't be thread-safe,
            // but for testing purposes we're clearing everything
            _smallPool.Clear();
            _mediumPool.Clear();
            _largePool.Clear();
            
            _rentCount = 0;
            _returnCount = 0;
            _missCount = 0;
        }
    }
} 