using System.Collections.Generic;

namespace YoloSerializer.Generator.Models
{
    public class SerializerTemplateModel
    {
        public string Namespace { get; set; }
        public string TypeNamespace { get; set; }
        public string ClassName { get; set; }
        public string FullTypeName { get; set; }
        public string SerializerName { get; set; }
        public string InstanceVarName { get; set; }
        public bool IsClass { get; set; }
        public bool NeedsNullCheck { get; set; }
        public string NullableMarker { get; set; }
        public string SizeCalculation { get; set; }
        public string SerializeCode { get; set; }
        public string DeserializeCode { get; set; }
        public List<PropertyTemplateModel> Properties { get; set; }
        public List<string> TypeNamespaces { get; set; }
        
        // New properties for nullable bitset support
        public int NullableFieldCount { get; set; }
        public int NullableBitsetSize { get; set; }
        public string NullabilityCheckCode { get; set; }
    }

    public class PropertyTemplateModel
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string SerializerName { get; set; }
        public string InstanceVarName { get; set; }
        public bool IsNullable { get; set; }
        public bool IsList { get; set; }
        public bool IsDictionary { get; set; }
        public bool IsArray { get; set; }
        public string ElementTypeName { get; set; }
        public string KeyTypeName { get; set; }
        public string ValueTypeName { get; set; }
        public string SizeCalculation { get; set; }
        public string SerializeCode { get; set; }
        public string DeserializeCode { get; set; }
        
        // New property for nullable fields
        public int NullableFieldIndex { get; set; } = -1;
    }
} 