using System;
using System.Collections.Concurrent;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Registry for type serializers
    /// </summary>
    public static class SerializerRegistry
    {
        private static readonly ConcurrentDictionary<Type, object> _serializers = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Registers a serializer for a specific type
        /// </summary>
        /// <typeparam name="T">The type to register the serializer for</typeparam>
        /// <param name="serializer">The serializer instance</param>
        public static void Register<T>(ISerializer<T> serializer)
        {
            _serializers[typeof(T)] = serializer;
        }

        /// <summary>
        /// Gets a registered serializer for a specific type
        /// </summary>
        /// <typeparam name="T">The type to get the serializer for</typeparam>
        /// <returns>The registered serializer</returns>
        /// <exception cref="InvalidOperationException">Thrown when no serializer is registered for the type</exception>
        public static ISerializer<T> Get<T>()
        {
            if (_serializers.TryGetValue(typeof(T), out var serializer))
            {
                return (ISerializer<T>)serializer;
            }

            throw new InvalidOperationException($"No serializer registered for type {typeof(T)}");
        }

        /// <summary>
        /// Tries to get a registered serializer for a specific type
        /// </summary>
        /// <typeparam name="T">The type to get the serializer for</typeparam>
        /// <param name="serializer">The registered serializer if found</param>
        /// <returns>True if a serializer was found, false otherwise</returns>
        public static bool TryGet<T>(out ISerializer<T> serializer)
        {
            if (_serializers.TryGetValue(typeof(T), out var value))
            {
                serializer = (ISerializer<T>)value;
                return true;
            }

            serializer = null!;
            return false;
        }

        /// <summary>
        /// Checks if a serializer is registered for a specific type
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <returns>True if a serializer is registered, false otherwise</returns>
        public static bool IsRegistered<T>()
        {
            return _serializers.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Removes a registered serializer for a specific type
        /// </summary>
        /// <typeparam name="T">The type to remove the serializer for</typeparam>
        /// <returns>True if a serializer was removed, false otherwise</returns>
        public static bool Remove<T>()
        {
            return _serializers.TryRemove(typeof(T), out _);
        }

        /// <summary>
        /// Clears all registered serializers
        /// </summary>
        public static void Clear()
        {
            _serializers.Clear();
        }
    }
} 