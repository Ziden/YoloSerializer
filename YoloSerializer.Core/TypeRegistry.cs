using System;
using System.Collections.Generic;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Central registry for automatic type ID management
    /// </summary>
    public static class TypeRegistry
    {
        /// <summary>
        /// Type ID reserved for null values
        /// </summary>
        public const byte NULL_TYPE_ID = 0;

        private static readonly Dictionary<Type, byte> _typeToId = new Dictionary<Type, byte>();
        private static readonly Dictionary<byte, Type> _idToType = new Dictionary<byte, Type>();
        private static byte _nextAvailableId = 1; // Start from 1 as 0 is reserved for null

        /// <summary>
        /// Resets the registry (for testing purposes)
        /// </summary>
        public static void Reset()
        {
            _typeToId.Clear();
            _idToType.Clear();
            _nextAvailableId = 1;
        }

        /// <summary>
        /// Registers a type with automatic ID assignment
        /// </summary>
        public static void RegisterType<T>() where T : IYoloSerializable
        {
            Type type = typeof(T);
            if (_typeToId.ContainsKey(type))
                return; // Already registered

            // Get next available ID and register
            byte id = _nextAvailableId++;
            _typeToId[type] = id;
            _idToType[id] = type;
        }

        /// <summary>
        /// Registers a type with a specific ID
        /// </summary>
        public static void RegisterType<T>(byte id) where T : IYoloSerializable
        {
            if (id == NULL_TYPE_ID)
                throw new ArgumentException($"Type ID {NULL_TYPE_ID} is reserved for null values");

            Type type = typeof(T);
            
            // Check if ID is already used
            if (_idToType.ContainsKey(id))
                throw new ArgumentException($"Type ID {id} is already registered for type {_idToType[id].Name}");

            // Check if type is already registered
            if (_typeToId.ContainsKey(type))
                throw new ArgumentException($"Type {type.Name} is already registered with ID {_typeToId[type]}");

            _typeToId[type] = id;
            _idToType[id] = type;

            // Update next available ID if needed
            if (id >= _nextAvailableId)
                _nextAvailableId = (byte)(id + 1);
        }

        /// <summary>
        /// Gets the ID for a registered type
        /// </summary>
        public static byte GetTypeId<T>() where T : IYoloSerializable
        {
            Type type = typeof(T);
            if (!_typeToId.TryGetValue(type, out byte id))
                throw new InvalidOperationException($"Type {type.Name} is not registered");
            return id;
        }

        /// <summary>
        /// Gets the type for a registered ID
        /// </summary>
        public static Type GetTypeForId(byte id)
        {
            if (id == NULL_TYPE_ID)
                return typeof(object); // Null object

            if (!_idToType.TryGetValue(id, out Type type))
                throw new InvalidOperationException($"Type ID {id} is not registered");

            return type;
        }

        /// <summary>
        /// Checks if a type is registered
        /// </summary>
        public static bool IsTypeRegistered<T>() where T : IYoloSerializable
        {
            return _typeToId.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Gets all registered type IDs
        /// </summary>
        /// <returns>Dictionary of type to ID mappings</returns>
        public static IReadOnlyDictionary<Type, byte> GetAllRegisteredTypes()
        {
            return _typeToId;
        }
    }
} 