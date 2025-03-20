using System;
using System.Collections.Generic;

namespace YoloSerializer.Tests.Models
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        public List<string> Tags { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
        public double Height { get; set; }
        public decimal Salary { get; set; }
        public Guid Id { get; set; }
    }
} 