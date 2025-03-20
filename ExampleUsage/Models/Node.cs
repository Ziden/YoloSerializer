using System;
using System.Collections.Generic;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Core.Models
{
    /// <summary>
    /// Represents a node in a graph, used for testing circular references
    /// </summary>
    [Serializable]
    public class Node
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public Node? Next { get; set; }

        public Node() 
        {
            // Default constructor
        }

        public Node(int id, string? name, Node? next = null)
        {
            Id = id;
            Name = name;
            Next = next;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Node other)
            {
                // Basic properties equality check
                bool basicPropertiesEqual = 
                    Id == other.Id &&
                    Name == other.Name;
                
                if (!basicPropertiesEqual) return false;

                // To avoid infinite recursion in case of circular references,
                // we only check the immediate next node's Id
                if (Next == null && other.Next == null)
                    return true;
                    
                if (Next == null || other.Next == null)
                    return false;
                    
                return Next.Id == other.Next.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = Id.GetHashCode();
            
            if (Name != null)
                hash = hash * 23 + Name.GetHashCode();
            
            // To avoid infinite recursion, only include Next.Id in hash code
            if (Next != null)
                hash = hash * 23 + Next.Id.GetHashCode();
            
            return hash;
        }
    }

    /// <summary>
    /// Container for a list of nodes
    /// </summary>
    [Serializable]
    public class NodeContainer
    {
        public List<Node>? Nodes { get; set; } = new List<Node>();

        public NodeContainer() { }

        public NodeContainer(List<Node>? nodes)
        {
            Nodes = nodes ?? new List<Node>();
        }
    }
} 