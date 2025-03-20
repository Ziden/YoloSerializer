using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Tests.Generated;

namespace YoloSerializer.Tests.Generated
{
    public sealed class YoloGeneratedMap : ITypeMap
    {
        private static readonly YoloGeneratedMap _instance = new YoloGeneratedMap();
        public static YoloGeneratedMap Instance => _instance;
        private YoloGeneratedMap() { }
        public const byte NULL_TYPE_ID = 0;
        #region codegen
        public const byte PLAYERDATA_TYPE_ID = 1;
        public const byte POSITION_TYPE_ID = 2;
        public const byte ALLTYPESDATA_TYPE_ID = 3;
        #endregion
        byte ITypeMap.NullTypeId => NULL_TYPE_ID;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetTypeId<T>()
        {
            Type type = typeof(T);
            #region codegen
            if (type == typeof(PlayerData))
                return PLAYERDATA_TYPE_ID;
            if (type == typeof(Position))
                return POSITION_TYPE_ID;
            if (type == typeof(AllTypesData))
                return ALLTYPESDATA_TYPE_ID;
            #endregion
            throw new ArgumentException($"Unknown type: {type.Name}");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T obj, Span<byte> buffer, ref int offset)
        {
            switch (obj)
            {
                case PlayerData playerData:
                    PlayerDataSerializer.Instance.Serialize(playerData, buffer, ref offset);
                    break;
                case Position position:
                    PositionSerializer.Instance.Serialize(position, buffer, ref offset);
                    break;
                case AllTypesData allTypesData:
                    AllTypesDataSerializer.Instance.Serialize(allTypesData, buffer, ref offset);
                    break;
                default:
                    throw new ArgumentException($"Unknown type: {obj.GetType().Name}");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSerializedSize<T>(T obj)
        {
            switch (obj)
            {
                case PlayerData playerData:
                    return PlayerDataSerializer.Instance.GetSize(playerData);
                case Position position:
                    return PositionSerializer.Instance.GetSize(position);
                case AllTypesData allTypesData:
                    return AllTypesDataSerializer.Instance.GetSize(allTypesData);
                default:
                    throw new ArgumentException($"Unknown type: {obj.GetType().Name}");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? DeserializeById(byte typeId, ReadOnlySpan<byte> buffer, ref int offset)
        {
            switch (typeId)
            {
                case PLAYERDATA_TYPE_ID:
                    PlayerData? playerDataResult;
                    PlayerDataSerializer.Instance.Deserialize(out playerDataResult, buffer, ref offset);
                    return playerDataResult;
                case POSITION_TYPE_ID:
                    Position? positionResult;
                    PositionSerializer.Instance.Deserialize(out positionResult, buffer, ref offset);
                    return positionResult;
                case ALLTYPESDATA_TYPE_ID:
                    AllTypesData? allTypesDataResult;
                    AllTypesDataSerializer.Instance.Deserialize(out allTypesDataResult, buffer, ref offset);
                    return allTypesDataResult;
                default:
                    throw new ArgumentException($"Unknown type ID: {typeId}");
            }
        }
    }
}
