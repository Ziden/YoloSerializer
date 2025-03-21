﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YoloSerializer.Generator.Models
{
    /// <summary>
    /// Represents property information used in template rendering
    /// </summary>
    public class TemplatePropertyInfo
    {
        public PropertyInfo Property;
        public FieldInfo Field;

        public string Name { get; set; }
        public string Type { get; set; }
        public Type PropertyType { get; set; }
        public string SizeCalculation { get; set; }
        public string SerializeCode { get; set; }
        public string DeserializeCode { get; set; }
        public bool IsList { get; set; }
        public bool IsDictionary { get; set; }
        public bool IsArray { get; set; }
        public bool IsNullable { get; set; }
        public bool IsYoloSerializable { get; set; }
        public string ElementType { get; set; }
        public string KeyType { get; set; }
        public string ValueType { get; set; }
        
        /// <summary>
        /// Index of the field in the nullable bitset. Starts at 1 for the first nullable field.
        /// -1 means the field is not nullable or does not use the nullable bitset.
        /// </summary>
        public int NullableFieldIndex { get; set; } = -1;
    }
}
