using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Tests.Generated
{
    /// <summary>
    /// Hard-coded map of type IDs to serializers for maximum performance
    /// </summary>
    public sealed class YoloGeneratedMap : ITypeMap
    {
        // Singleton instance for performance
        private static readonly YoloGeneratedMap _instance = new YoloGeneratedMap();
        
        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        public static YoloGeneratedMap Instance => _instance;
        
        // Private constructor to enforce singleton pattern
        private YoloGeneratedMap() { }
        
        /// <summary>
        /// Type ID used for null values
        /// </summary>
        public const byte NULL_TYPE_ID = 0;

        #region codegen
        /// <summary>
        /// Type ID for AllTypesData
        /// </summary>
        public const byte ALLTYPESDATA_TYPE_ID = 1;
        
        /// <summary>
        /// Type ID for PlayerData
        /// </summary>
        public const byte PLAYERDATA_TYPE_ID = 2;
        
        /// <summary>
        /// Type ID for Position
        /// </summary>
        public const byte POSITION_TYPE_ID = 3;
        
        #endregion
        
        /// <summary>
        /// Gets the null type ID for the ITypeMap interface
        /// </summary>
        byte ITypeMap.NullTypeId => NULL_TYPE_ID;
        
        /// <summary>
        /// Gets the type ID for a type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetTypeId<T>() where T : class, IYoloSerializable
        {
            Type type = typeof(T);

            #region codegen
            if (type == typeof(AllTypesData))
                return ALLTYPESDATA_TYPE_ID;
                
            if (type == typeof(PlayerData))
                return PLAYERDATA_TYPE_ID;
                
            if (type == typeof(Position))
                return POSITION_TYPE_ID;
                
            #endregion
            throw new ArgumentException($"Unknown type: {type.Name}");
        }
        
        /// <summary>
        /// Serializes an object to a byte span without boxing
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable
        {
            switch (obj)
            {
                case AllTypesData allTypesData:
                    AllTypesDataSerializer.Instance.Serialize(allTypesData, buffer, ref offset);
                    break;
                
                case PlayerData playerData:
                    PlayerDataSerializer.Instance.Serialize(playerData, buffer, ref offset);
                    break;
                
                case Position position:
                    PositionSerializer.Instance.Serialize(position, buffer, ref offset);
                    break;
                
                default:
                    throw new ArgumentException($"Unknown type: {obj.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Gets the serialized size of an object without boxing
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T obj) where T : class, IYoloSerializable
        {
            switch (obj)
            {
                case AllTypesData allTypesData:
                    return AllTypesDataSerializer.Instance.GetSize(allTypesData);
                
                case PlayerData playerData:
                    return PlayerDataSerializer.Instance.GetSize(playerData);
                
                case Position position:
                    return PositionSerializer.Instance.GetSize(position);
                
                default:
                    throw new ArgumentException($"Unknown type: {obj.GetType().Name}");
            }
        }
        
        /// <summary>
        /// Deserializes an object from a byte span based on type ID
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? DeserializeById(byte typeId, ReadOnlySpan<byte> buffer, ref int offset)
        {
            switch (typeId)
            {
                case ALLTYPESDATA_TYPE_ID:
                    AllTypesData? allTypesDataResult;
                    AllTypesDataSerializer.Instance.Deserialize(out allTypesDataResult, buffer, ref offset);
                    return allTypesDataResult;
                
                case PLAYERDATA_TYPE_ID:
                    PlayerData? playerDataResult;
                    PlayerDataSerializer.Instance.Deserialize(out playerDataResult, buffer, ref offset);
                    return playerDataResult;
                
                case POSITION_TYPE_ID:
                    Position? positionResult;
                    PositionSerializer.Instance.Deserialize(out positionResult, buffer, ref offset);
                    return positionResult;
                
                default:
                    throw new ArgumentException($"Unknown type ID: {typeId}");
            }
        }
    }
}
