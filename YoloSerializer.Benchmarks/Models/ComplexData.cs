using MessagePack;
using System;
using System.Collections.Generic;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Benchmarks.Models
{
    /// <summary>
    /// Complex data model with nested objects and collections for benchmarking
    /// </summary>
    [MessagePackObject]
    public class ComplexData
    {
        [Key(0)]
        public int Id { get; set; }
        
        [Key(1)]
        public string Title { get; set; }
        
        [Key(2)]
        public List<string> Tags { get; set; } = new List<string>();
        
        [Key(3)]
        public Dictionary<string, float> Metrics { get; set; } = new Dictionary<string, float>();
        
        [Key(4)]
        public SimpleData Metadata { get; set; }
        
        [Key(5)]
        public NestedData[] Items { get; set; } = Array.Empty<NestedData>();
        
        [Key(6)]
        public DataStatus Status { get; set; }

        public ComplexData()
        {
            Title = string.Empty;
            Metadata = new SimpleData();
        }

        public ComplexData(int id, string title, SimpleData metadata, DataStatus status, List<string> tags = null, Dictionary<string, float> metrics = null, NestedData[] items = null)
        {
            Id = id;
            Title = title;
            Metadata = metadata;
            Status = status;
            Tags = tags ?? new List<string>();
            Metrics = metrics ?? new Dictionary<string, float>();
            Items = items ?? Array.Empty<NestedData>();
        }
    }

    [MessagePackObject]
    public class NestedData
    {
        [Key(0)]
        public int Index { get; set; }
        
        [Key(1)]
        public string Name { get; set; }
        
        [Key(2)]
        public double Value { get; set; }

        public NestedData()
        {
            Name = string.Empty;
        }

        public NestedData(int index, string name, double value)
        {
            Index = index;
            Name = name;
            Value = value;
        }
    }

    public enum DataStatus
    {
        New = 0,
        Processing = 1,
        Completed = 2,
        Error = 3
    }
} 