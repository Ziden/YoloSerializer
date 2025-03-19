using System;
using System.Collections.Concurrent;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Simple object pool to reduce allocations during deserialization
    /// </summary>
    /// <typeparam name="T">Type of objects to pool</typeparam>
    public class ObjectPool<T> where T : class
    {
        // Max number of objects to keep in the pool
        private const int MaxPoolSize = 32;
        
        // Concurrent bag for thread safety
        private readonly ConcurrentBag<T> _objects = new ConcurrentBag<T>();
        
        // Factory to create new objects
        private readonly Func<T> _objectFactory;
        
        /// <summary>
        /// Creates a new object pool with the given factory
        /// </summary>
        public ObjectPool(Func<T> objectFactory)
        {
            _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
        }
        
        /// <summary>
        /// Gets an object from the pool or creates a new one
        /// </summary>
        public T Get()
        {
            if (_objects.TryTake(out T? result))
                return result;
                
            return _objectFactory();
        }
        
        /// <summary>
        /// Returns an object to the pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
                
            // Only add to pool if not too full
            if (_objects.Count < MaxPoolSize)
                _objects.Add(obj);
        }
        
        /// <summary>
        /// Clears the pool
        /// </summary>
        public void Clear()
        {
            while (_objects.TryTake(out _)) { }
        }
    }
} 