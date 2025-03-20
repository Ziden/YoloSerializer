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

        /// <summary>
        /// Gets the total size needed to serialize the Node
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSize(YoloSerializer.Core.Models.Node? node)
        {

            if (node == null)
                throw new ArgumentNullException(nameof(node));

            
            int size = 0;
            size += Int32Serializer.Instance.GetSize(node.Id);
                        size += StringSerializer.Instance.GetSize(node.Name);
                        size += NodeSerializer.Instance.GetSize(node.Next);

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

            Int32Serializer.Instance.Serialize(node.Id, buffer, ref offset);
                        StringSerializer.Instance.Serialize(node.Name, buffer, ref offset);
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


            Int32Serializer.Instance.Deserialize(out int _local_id, buffer, ref offset);
                        node.Id = _local_id;
                        StringSerializer.Instance.Deserialize(out string _local_name, buffer, ref offset);
                        node.Name = _local_name;
                        NodeSerializer.Instance.Deserialize(out Node? _local_next, buffer, ref offset);
                        node.Next = _local_next;

            value = node;
        }
    }
}