namespace YoloSerializer.Generator
{
    public class SerializerTemplate
    {
        public static string GetSerializerTemplate()
        {
            return @"using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core;
using {{ type_namespace }};
{{ for namespace in type_namespaces }}
using {{ namespace }};
{{ end }}
using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for {{ class_name }} objects
    /// </summary>
    public sealed class {{ serializer_name }} : ISerializer<{{ full_type_name }}{{ nullable_marker }}>
    {
        private static readonly {{ serializer_name }} _instance = new {{ serializer_name }}();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static {{ serializer_name }} Instance => _instance;
        
        private {{ serializer_name }}() { }

{{ if is_class }}
        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<{{ class_name }}> _{{ class_variable_name }}Pool = 
            new ObjectPool<{{ class_name }}>(() => new {{ class_name }}());
{{ end }}
            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        /// <summary>
        /// Gets the total size needed to serialize the {{ class_name }}
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize({{ full_type_name }}{{ nullable_marker }} {{ instance_var_name }})
        {
{{ if needs_null_check }}
            if ({{ instance_var_name }} == null)
                throw new ArgumentNullException(nameof({{ instance_var_name }}));
{{ end }}
            
            int size = 0;
            {{ size_calculation }}

            return size;
        }

        /// <summary>
        /// Serializes a {{ class_name }} object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize({{ full_type_name }}{{ nullable_marker }} {{ instance_var_name }}, Span<byte> buffer, ref int offset)
        {
{{ if needs_null_check }}
            if ({{ instance_var_name }} == null)
                throw new ArgumentNullException(nameof({{ instance_var_name }}));
{{ end }}
            {{ serialize_code }}
        }

        /// <summary>
        /// Deserializes a {{ class_name }} object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out {{ full_type_name }}{{ nullable_marker }} value, ReadOnlySpan<byte> buffer, ref int offset)
        {
{{ if is_class }}
            // Get a {{ class_name }} instance from pool
            var {{ instance_var_name }} = _{{ class_variable_name }}Pool.Get();
{{ else }}
            var {{ instance_var_name }} = new {{ class_name }}();
{{ end }}

            {{ deserialize_code }}

            value = {{ instance_var_name }};
        }
    }
}";
        }
    }
}
