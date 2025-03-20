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
        private static readonly ObjectPool<{{ class_name }}> _{{ instance_var_name }}Pool = 
            new ObjectPool<{{ class_name }}>(() => new {{ class_name }}());
{{ end }}
            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        // Number of nullable fields in this type
        private const int NullableFieldCount = {{ nullable_field_count }};
        
        // Size of the nullability bitset in bytes
        private const int NullableBitsetSize = {{ nullable_bitset_size }};
        
        // Create stack allocated bitset array for nullability tracking
        private Span<byte> GetBitsetArray(Span<byte> tempBuffer) => 
            NullableBitsetSize > 0 ? tempBuffer.Slice(0, NullableBitsetSize) : default;

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
            
            // Add size for nullability bitset if needed
            size += NullableBitsetSize;
            
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

            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Initialize all bits to 0 (non-null)
            for (int i = 0; i < bitset.Length; i++)
                bitset[i] = 0;
                
            // First determine the nullability of each field
            {{ nullability_check_code }}
            
            // Write the nullability bitset to the buffer
            NullableBitset.SerializeBitset(bitset, NullableBitsetSize, buffer, ref offset);
            
            // Write the actual field values
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
            var {{ instance_var_name }} = _{{ instance_var_name }}Pool.Get();
{{ else }}
            var {{ instance_var_name }} = new {{ class_name }}();
{{ end }}

            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Read the nullability bitset from the buffer
            NullableBitset.DeserializeBitset(buffer, ref offset, bitset, NullableBitsetSize);

            {{ deserialize_code }}

            value = {{ instance_var_name }};
        }
    }
}";
        }
    }
}
