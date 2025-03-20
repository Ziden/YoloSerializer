using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;

using YoloSerializer.Core.Models;

using System;

using YoloSerializer.Core.Serializers;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Serializers
{
    /// <summary>
    /// High-performance serializer for Node objects
    /// </summary>
    public sealed class NodeSerializer : ISerializer<YoloSerializer.Core.Models.Node?>
    {
        private static readonly NodeSerializer _instance = new NodeSerializer();
        
        /// <summary>
        /// Singleton instance for performance
        /// </summary>
        public static NodeSerializer Instance => _instance;
        
        private NodeSerializer() { }


        // Object pooling to avoid allocations during deserialization
        private static readonly ObjectPool<Node> _nodePool = 
            new ObjectPool<Node>(() => new Node());

            
        // Shared empty array for optimization
        private static readonly byte[] EmptyArray = Array.Empty<byte>();

        // Number of nullable fields in this type
        private const int NullableFieldCount = 2;
        
        // Size of the nullability bitset in bytes
        private const int NullableBitsetSize = 1;
        
        // Create stack allocated bitset array for nullability tracking
        private Span<byte> GetBitsetArray(Span<byte> tempBuffer) => 
            NullableBitsetSize > 0 ? tempBuffer.Slice(0, NullableBitsetSize) : default;

        /// <summary>
        /// Gets the total size needed to serialize the Node
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Core.Models.Node? node)
        {

            if (node == null)
                throw new ArgumentNullException(nameof(node));

            
            int size = 0;
            
            // Add size for nullability bitset if needed
            size += NullableBitsetSize;
            
            size += Int32Serializer.Instance.GetSize(node.Id);
                        size += node.Name == null ? 0 : StringSerializer.Instance.GetSize(node.Name);
                        size += node.Next == null ? 0 : NodeSerializer.Instance.GetSize(node.Next);

            return size;
        }

        /// <summary>
        /// Serializes a Node object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(YoloSerializer.Core.Models.Node? node, Span<byte> buffer, ref int offset)
        {

            if (node == null)
                throw new ArgumentNullException(nameof(node));


            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Initialize all bits to 0 (non-null)
            for (int i = 0; i < bitset.Length; i++)
                bitset[i] = 0;
                
            // First determine the nullability of each field
                        NullableBitset.SetBit(bitset, 1, node.Name == null);
                        NullableBitset.SetBit(bitset, 2, node.Next == null);

            
            // Write the nullability bitset to the buffer
            NullableBitset.SerializeBitset(bitset, NullableBitsetSize, buffer, ref offset);
            
            // Write the actual field values
            Int32Serializer.Instance.Serialize(node.Id, buffer, ref offset);
                        if (!NullableBitset.IsNull(bitset, 1))
                            StringSerializer.Instance.Serialize(node.Name, buffer, ref offset);
                        if (!NullableBitset.IsNull(bitset, 2))
                            NodeSerializer.Instance.Serialize(node.Next, buffer, ref offset);
        }

        /// <summary>
        /// Deserializes a Node object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out YoloSerializer.Core.Models.Node? value, ReadOnlySpan<byte> buffer, ref int offset)
        {

            // Get a Node instance from pool
            var node = _nodePool.Get();


            // Create temporary buffer for nullability bitset
            Span<byte> tempBuffer = stackalloc byte[32]; // Enough for 256 nullable fields
            Span<byte> bitset = GetBitsetArray(tempBuffer);
            
            // Read the nullability bitset from the buffer
            NullableBitset.DeserializeBitset(buffer, ref offset, bitset, NullableBitsetSize);

            Int32Serializer.Instance.Deserialize(out int _local_id, buffer, ref offset);
                        node.Id = _local_id;
                        if (NullableBitset.IsNull(bitset, 1))
                            node.Name = null;
                        else
                        {
                            StringSerializer.Instance.Deserialize(out string _local_name, buffer, ref offset);
                            node.Name = _local_name;
                        }
                        if (NullableBitset.IsNull(bitset, 2))
                            node.Next = null;
                        else
                        {
                            NodeSerializer.Instance.Deserialize(out Node? _local_next, buffer, ref offset);
                            node.Next = _local_next;
                        }

            value = node;
        }
    }
}