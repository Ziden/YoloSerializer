using MessagePack;
using System;
using System.Collections.Generic;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Benchmarks.Models
{
    /// <summary>
    /// Simple data model with various primitive types for benchmarking
    /// </summary>
    [MessagePackObject]
    public class SimpleData
    {
        [Key(0)]
        public int Id { get; set; }
        
        [Key(1)]
        public string Name { get; set; }
        
        [Key(2)]
        public bool IsActive { get; set; }
        
        [Key(3)]
        public double Value { get; set; }
        
        [Key(4)]
        public DateTime CreatedAt { get; set; }
        
        [Key(5)]
        public Guid UniqueId { get; set; }

        public SimpleData()
        {
            Name = string.Empty;
        }

        public SimpleData(int id, string name, bool isActive, double value, DateTime createdAt, Guid uniqueId)
        {
            Id = id;
            Name = name;
            IsActive = isActive;
            Value = value;
            CreatedAt = createdAt;
            UniqueId = uniqueId;
        }
    }
} 