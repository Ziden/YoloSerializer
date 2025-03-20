using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Serializers;
using YoloSerializer.Benchmarks.Generated;
using YoloSerializer.Benchmarks.Models;

namespace YoloSerializer.Benchmarks.Generated.Maps
{
    public sealed class YoloGeneratedMap : ITypeMap
    {
        private static readonly YoloGeneratedMap _instance = new YoloGeneratedMap();
        public static YoloGeneratedMap Instance => _instance;
        private YoloGeneratedMap() { }
        public const byte NULL_TYPE_ID = 0;
        #region codegen
        public const byte SIMPLEDATA_TYPE_ID = 1;
        public const byte COMPLEXDATA_TYPE_ID = 2;
        public const byte NESTEDDATA_TYPE_ID = 3;
        #endregion
        byte ITypeMap.NullTypeId => NULL_TYPE_ID;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetTypeId<T>()
        {
            Type type = typeof(T);
            #region codegen
            if (type == typeof(SimpleData))
                return SIMPLEDATA_TYPE_ID;
            if (type == typeof(ComplexData))
                return COMPLEXDATA_TYPE_ID;
            if (type == typeof(NestedData))
                return NESTEDDATA_TYPE_ID;
            #endregion
            throw new ArgumentException($"Unknown type: {type.Name}");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize<T>(T obj, Span<byte> buffer, ref int offset)
        {
            switch (obj)
            {
                case SimpleData simpleData:
                    SimpleDataSerializer.Instance.Serialize(simpleData, buffer, ref offset);
                    break;
                case ComplexData complexData:
                    ComplexDataSerializer.Instance.Serialize(complexData, buffer, ref offset);
                    break;
                case NestedData nestedData:
                    NestedDataSerializer.Instance.Serialize(nestedData, buffer, ref offset);
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
                case SimpleData simpleData:
                    return SimpleDataSerializer.Instance.GetSize(simpleData);
                case ComplexData complexData:
                    return ComplexDataSerializer.Instance.GetSize(complexData);
                case NestedData nestedData:
                    return NestedDataSerializer.Instance.GetSize(nestedData);
                default:
                    throw new ArgumentException($"Unknown type: {obj.GetType().Name}");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? DeserializeById(byte typeId, ReadOnlySpan<byte> buffer, ref int offset)
        {
            switch (typeId)
            {
                case SIMPLEDATA_TYPE_ID:
                    SimpleData? simpleDataResult;
                    SimpleDataSerializer.Instance.Deserialize(out simpleDataResult, buffer, ref offset);
                    return simpleDataResult;
                case COMPLEXDATA_TYPE_ID:
                    ComplexData? complexDataResult;
                    ComplexDataSerializer.Instance.Deserialize(out complexDataResult, buffer, ref offset);
                    return complexDataResult;
                case NESTEDDATA_TYPE_ID:
                    NestedData? nestedDataResult;
                    NestedDataSerializer.Instance.Deserialize(out nestedDataResult, buffer, ref offset);
                    return nestedDataResult;
                default:
                    throw new ArgumentException($"Unknown type ID: {typeId}");
            }
        }
    }
}
