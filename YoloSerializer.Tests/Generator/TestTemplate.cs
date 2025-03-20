namespace YoloSerializer.Tests.Generator
{
    public static class TestTemplate
    {
        public const string Template = @"using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;
using {{ type_namespace }};
{{ for property in properties }}
using {{ property.type_name | string.split '.' | array.first }};
{{ end }}

namespace {{ namespace }}
{
    public sealed class {{ class_name }}Serializer : ISerializer<{{ full_type_name }}{{ nullable_marker }}>
    {
        private static readonly {{ class_name }}Serializer _instance = new {{ class_name }}Serializer();
        public static {{ class_name }}Serializer Instance => _instance;
        private {{ class_name }}Serializer() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize({{ full_type_name }}{{ nullable_marker }} obj)
        {
            if (obj == null) return 1;
            int size = 1;
            {{ size_calculation }}
            return size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize({{ full_type_name }}{{ nullable_marker }} obj, Span<byte> buffer, ref int offset)
        {
            if (obj == null)
            {
                buffer[offset++] = 0;
                return;
            }
            buffer[offset++] = 1;
            {{ serialize_code }}
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out {{ full_type_name }}{{ nullable_marker }} obj, ReadOnlySpan<byte> buffer, ref int offset)
        {
            if (buffer[offset++] == 0)
            {
                obj = null;
                return;
            }
            obj = new {{ full_type_name }}();
            {{ deserialize_code }}
        }
    }
}";
    }
} 