// See https://aka.ms/new-console-template for more information

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Scriban;
using YoloSerializer.Core.Contracts;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Serializers;

namespace YoloSerializer.Generator
{
    /// <summary>
    /// Represents property information used in template rendering
    /// </summary>
    class PropertyInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
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
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Default parameters
var targetAssembly = typeof(PlayerData).Assembly; // initial example
            
            // Use a relative path that works regardless of where the executable is located
            var solutionDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
            var outputPath = Path.GetFullPath(Path.Combine(solutionDir, "YoloSerializer.Tests", "Generated"));
            
            // Flag to force regeneration of serializers even if they exist
            bool forceRegeneration = args.Contains("--force") || args.Contains("-f");

            // test 
            forceRegeneration = true;

            // Parse command line arguments if provided
            var filteredArgs = args.Where(a => !a.StartsWith("-")).ToArray();
            if (filteredArgs.Length >= 1)
            {
                string assemblyPath = filteredArgs[0];
                targetAssembly = Assembly.LoadFrom(assemblyPath);
            }
            
            if (filteredArgs.Length >= 2)
            {
                outputPath = Path.GetFullPath(filteredArgs[1]);
            }
            
            // Ensure output directory exists
            Directory.CreateDirectory(outputPath);
            
            Console.WriteLine($"Generating serializers for types in {targetAssembly.FullName}");
            Console.WriteLine($"Output path: {outputPath}");
            Console.WriteLine($"Force regeneration: {forceRegeneration}");
            
            // Use embedded template instead of loading from file
            string templateContent = GetSerializerTemplate(); // TODO: Read from file
            var template = Template.Parse(templateContent);
            
            // Find all types in the target assembly that implement IYoloSerializable
            var serializableTypes = targetAssembly.GetTypes()
                .Where(t => typeof(IYoloSerializable).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();
                
            Console.WriteLine($"Found {serializableTypes.Count} serializable types");
            
            // Generate a serializer for each type
            foreach (var type in serializableTypes)
            {
                await GenerateSerializer(type, template, outputPath, forceRegeneration);
            }
            
            Console.WriteLine("Done!");
        }
        
        private static async Task GenerateSerializer(Type type, Template template, string outputPath, bool forceRegeneration)
        {
            string outputFile = Path.Combine(outputPath, $"{type.Name}Serializer.cs");
            
            // Skip generation if the file already exists to avoid overriding manual implementations
            if (File.Exists(outputFile) && !forceRegeneration)
            {
                Console.WriteLine($"Skipping {type.Name}Serializer.cs - file already exists (use --force to override)");
                return;
            }
            
            Console.WriteLine($"Generating serializer for {type.Name}");
            
            // Gather all public properties of the type
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => BuildPropertyInfo(p))
                .ToList();
                
            // Render template with type and property information
            var templateContext = new
            {
                class_name = type.Name,
                full_type_name = GetFullTypeName(type),
                serializer_name = $"{type.Name}Serializer",
                instance_name = char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1),
                class_variable_name = char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1),
                properties = properties,
                is_nullable = IsReferenceType(type),
                has_object_pool = IsReferenceType(type),
                nullable_marker = IsReferenceType(type) ? "?" : "",
                null_marker = "0",
                remaining_properties = properties
            };
            
            var result = await template.RenderAsync(templateContext);
            
            // Write the generated code to a file
            await File.WriteAllTextAsync(outputFile, result);
            
            Console.WriteLine($"Generated {outputFile}");
        }
        
        private static PropertyInfo BuildPropertyInfo(System.Reflection.PropertyInfo property)
        {
            var propertyInfo = new PropertyInfo
            {
                Name = property.Name,
                Type = GetFullTypeName(property.PropertyType),
                IsNullable = IsNullableType(property.PropertyType)
            };
            
            Type propertyType = property.PropertyType;
            Type unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            
            // Check if the property type implements IYoloSerializable
            bool isYoloSerializable = typeof(IYoloSerializable).IsAssignableFrom(unwrappedType);
            propertyInfo.IsYoloSerializable = isYoloSerializable;
            
            // Handle lists
            if (typeof(IList).IsAssignableFrom(unwrappedType) && unwrappedType.IsGenericType)
            {
                propertyInfo.IsList = true;
                var elementType = unwrappedType.GetGenericArguments()[0];
                propertyInfo.ElementType = GetFullTypeName(elementType);
                
                SetListPropertyCodeAndSize(propertyInfo, property);
            }
            // Handle dictionaries
            else if (typeof(IDictionary).IsAssignableFrom(unwrappedType) && unwrappedType.IsGenericType)
            {
                propertyInfo.IsDictionary = true;
                var genericArgs = unwrappedType.GetGenericArguments();
                propertyInfo.KeyType = GetFullTypeName(genericArgs[0]);
                propertyInfo.ValueType = GetFullTypeName(genericArgs[1]);
                
                SetDictionaryPropertyCodeAndSize(propertyInfo, property);
            }
            // Handle arrays
            else if (unwrappedType.IsArray)
            {
                propertyInfo.IsArray = true;
                propertyInfo.ElementType = GetFullTypeName(unwrappedType.GetElementType());
                
                SetArrayPropertyCodeAndSize(propertyInfo, property);
            }
            // Handle standard types
            else
            {
                SetStandardPropertyCodeAndSize(propertyInfo, property);
            }
            
            return propertyInfo;
        }
        
        private static void SetStandardPropertyCodeAndSize(PropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            Type unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            bool isNullable = propertyType != unwrappedType || !propertyType.IsValueType;
            string nullableMarker = isNullable ? "?" : "";
            string propName = property.Name;
            string instancePropRef = $"{char.ToLowerInvariant(property.DeclaringType.Name[0])}{property.DeclaringType.Name.Substring(1)}.{propName}";
            string localVarName = "_local_" + char.ToLowerInvariant(propName[0]) + propName.Substring(1);
            
            // Handle primitive types
            if (unwrappedType == typeof(int))
            {
                propertyInfo.SizeCalculation = $"Int32Serializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"Int32Serializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"Int32Serializer.Instance.Deserialize(out int {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(float))
            {
                propertyInfo.SizeCalculation = $"FloatSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"FloatSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"FloatSerializer.Instance.Deserialize(out float {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(double))
            {
                propertyInfo.SizeCalculation = $"DoubleSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"DoubleSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"DoubleSerializer.Instance.Deserialize(out double {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(long))
            {
                propertyInfo.SizeCalculation = $"Int64Serializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"Int64Serializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"Int64Serializer.Instance.Deserialize(out long {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(bool))
            {
                propertyInfo.SizeCalculation = $"BooleanSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"BooleanSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"BooleanSerializer.Instance.Deserialize(out bool {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(string))
            {
                propertyInfo.SizeCalculation = $"StringSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"StringSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"StringSerializer.Instance.Deserialize(out string? {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            // Handle custom serializable types
            else if (propertyInfo.IsYoloSerializable)
            {
                string serializerName = $"{unwrappedType.Name}Serializer";
                propertyInfo.SizeCalculation = $"{serializerName}.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"{serializerName}.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"{serializerName}.Instance.Deserialize(out {GetFullTypeName(unwrappedType)}{nullableMarker} {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else
            {
                throw new NotSupportedException($"No serializer available for type {unwrappedType.FullName}");
            }
        }
        
        private static void SetListPropertyCodeAndSize(PropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            Type unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var elementType = unwrappedType.GetGenericArguments()[0];
            
            string propName = property.Name;
            string instancePropRef = $"{char.ToLowerInvariant(property.DeclaringType.Name[0])}{property.DeclaringType.Name.Substring(1)}.{propName}";
            string localVarName = "_local_" + char.ToLowerInvariant(propName[0]) + propName.Substring(1);
            
            // Size calculation
            propertyInfo.SizeCalculation = $"Int32Serializer.Instance.GetSize({instancePropRef}.Count) + " +
                                            $"{instancePropRef}.Sum(listItem => {GetSerializerForType(elementType)}.Instance.GetSize(listItem))";
            
            // Serialize code
            propertyInfo.SerializeCode = 
                $"Int32Serializer.Instance.Serialize({instancePropRef}.Count, buffer, ref offset);\n" +
                $"            foreach (var listItem in {instancePropRef})\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(elementType)}.Instance.Serialize(listItem, buffer, ref offset);\n" +
                $"            }}";
                
            // Deserialize code
            propertyInfo.DeserializeCode = 
                $"Int32Serializer.Instance.Deserialize(out int {localVarName}Count, buffer, ref offset);\n" +
                $"            {instancePropRef}.Clear();\n" +
                $"            for (int i = 0; i < {localVarName}Count; i++)\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(elementType)}.Instance.Deserialize(out {GetFullTypeName(elementType)} listItem, buffer, ref offset);\n" +
                $"                {instancePropRef}.Add(listItem);\n" +
                $"            }}";
        }
        
        private static void SetDictionaryPropertyCodeAndSize(PropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            Type unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var genericArgs = unwrappedType.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];
            
            string propName = property.Name;
            string instancePropRef = $"{char.ToLowerInvariant(property.DeclaringType.Name[0])}{property.DeclaringType.Name.Substring(1)}.{propName}";
            string localVarName = "_local_" + char.ToLowerInvariant(propName[0]) + propName.Substring(1);
            
            // Size calculation
            propertyInfo.SizeCalculation = 
                $"Int32Serializer.Instance.GetSize({instancePropRef}.Count) + " +
                $"{instancePropRef}.Sum(kvp => {GetSerializerForType(keyType)}.Instance.GetSize(kvp.Key) + " +
                $"{GetSerializerForType(valueType)}.Instance.GetSize(kvp.Value))";
                
            // Serialize code
            propertyInfo.SerializeCode = 
                $"Int32Serializer.Instance.Serialize({instancePropRef}.Count, buffer, ref offset);\n" +
                $"            foreach (var kvp in {instancePropRef})\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(keyType)}.Instance.Serialize(kvp.Key, buffer, ref offset);\n" +
                $"                {GetSerializerForType(valueType)}.Instance.Serialize(kvp.Value, buffer, ref offset);\n" +
                $"            }}";
                
            // Deserialize code - use dictValue instead of value to avoid conflicts
            propertyInfo.DeserializeCode = 
                $"Int32Serializer.Instance.Deserialize(out int {localVarName}Count, buffer, ref offset);\n" +
                $"            {instancePropRef}.Clear();\n" +
                $"            for (int i = 0; i < {localVarName}Count; i++)\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(keyType)}.Instance.Deserialize(out {GetFullTypeName(keyType)} key, buffer, ref offset);\n" +
                $"                {GetSerializerForType(valueType)}.Instance.Deserialize(out {GetFullTypeName(valueType)} dictValue, buffer, ref offset);\n" +
                $"                {instancePropRef}[key] = dictValue;\n" +
                $"            }}";
        }
        
        private static void SetArrayPropertyCodeAndSize(PropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            Type unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var elementType = unwrappedType.GetElementType();
            
            string propName = property.Name;
            string instancePropRef = $"{char.ToLowerInvariant(property.DeclaringType.Name[0])}{property.DeclaringType.Name.Substring(1)}.{propName}";
            string localVarName = "_local_" + char.ToLowerInvariant(propName[0]) + propName.Substring(1);
            
            // Size calculation
            propertyInfo.SizeCalculation = 
                $"Int32Serializer.Instance.GetSize({instancePropRef}.Length) + " +
                $"{instancePropRef}.Sum(arrayItem => {GetSerializerForType(elementType)}.Instance.GetSize(arrayItem))";
                
            // Serialize code
            propertyInfo.SerializeCode = 
                $"Int32Serializer.Instance.Serialize({instancePropRef}.Length, buffer, ref offset);\n" +
                $"            foreach (var arrayItem in {instancePropRef})\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(elementType)}.Instance.Serialize(arrayItem, buffer, ref offset);\n" +
                $"            }}";
                
            // Deserialize code
            propertyInfo.DeserializeCode = 
                $"Int32Serializer.Instance.Deserialize(out int {localVarName}Length, buffer, ref offset);\n" +
                $"            {instancePropRef} = new {GetFullTypeName(elementType)}[{localVarName}Length];\n" +
                $"            for (int i = 0; i < {localVarName}Length; i++)\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(elementType)}.Instance.Deserialize(out {GetFullTypeName(elementType)} arrayItem, buffer, ref offset);\n" +
                $"                {instancePropRef}[i] = arrayItem;\n" +
                $"            }}";
        }
        
        private static string GetSerializerForType(Type type)
        {
            Type unwrappedType = Nullable.GetUnderlyingType(type) ?? type;
            
            if (unwrappedType == typeof(int)) return "Int32Serializer";
            if (unwrappedType == typeof(float)) return "FloatSerializer";
            if (unwrappedType == typeof(double)) return "DoubleSerializer";
            if (unwrappedType == typeof(long)) return "Int64Serializer";
            if (unwrappedType == typeof(bool)) return "BooleanSerializer";
            if (unwrappedType == typeof(string)) return "StringSerializer";
            
            // For custom IYoloSerializable types
            if (typeof(IYoloSerializable).IsAssignableFrom(unwrappedType))
            {
                return $"{unwrappedType.Name}Serializer";
            }
            
            throw new NotSupportedException($"No serializer available for type {unwrappedType.FullName}");
        }
        
        private static string GetFullTypeName(Type type)
        {
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(long)) return "long";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            
            Type? nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullableUnderlyingType != null)
            {
                return $"{GetFullTypeName(nullableUnderlyingType)}?";
            }
            
            if (type.IsGenericType)
            {
                var genericTypeName = type.GetGenericTypeDefinition().Name;
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                
                string genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFullTypeName));
                return $"{genericTypeName}<{genericArgs}>";
            }
            
            if (type.IsArray)
            {
                return $"{GetFullTypeName(type.GetElementType())}[]";
            }
            
            return type.Name;
        }
        
        private static bool IsReferenceType(Type type)
        {
            return !type.IsValueType;
        }
        
        private static bool IsNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
        }

        // Embedded template
        private static string GetSerializerTemplate()
        {
            return @"using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
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
        
        // Maximum size to allocate on stack
        private const int MaxStackAllocSize = 1024;

{{ if has_object_pool }}
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
        public int GetSize({{ full_type_name }}{{ nullable_marker }} {{ instance_name }})
        {
{{ if is_nullable }}
            if ({{ instance_name }} == null)
                throw new ArgumentNullException(nameof({{ instance_name }}));
{{ end }}
            
            int size = 0;
            
{{ for prop in properties }}
            // Size of {{ prop.name }} ({{ prop.type }})
            size += {{ prop.size_calculation }};
{{ end }}
            
            return size;
        }

        /// <summary>
        /// Serializes a {{ class_name }} object to a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize({{ full_type_name }}{{ nullable_marker }} {{ instance_name }}, Span<byte> buffer, ref int offset)
        {
{{ if is_nullable }}
            if ({{ instance_name }} == null)
                throw new ArgumentNullException(nameof({{ instance_name }}));
{{ end }}
            
{{ for prop in properties }}
            // Serialize {{ prop.name }} ({{ prop.type }})
            {{ prop.serialize_code }}
{{ end }}
        }

        /// <summary>
        /// Deserializes a {{ class_name }} object from a byte span
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(out {{ full_type_name }}{{ nullable_marker }} value, ReadOnlySpan<byte> buffer, ref int offset)
        {
{{ if has_object_pool }}
            // Get a {{ class_name }} instance from pool
            var {{ instance_name }} = _{{ class_variable_name }}Pool.Get();
{{ else }}
            var {{ instance_name }} = new {{ class_name }}();
{{ end }}

{{ for prop in properties }}
            // Read {{ prop.name }}
            {{ prop.deserialize_code }}
{{ end }}

            value = {{ instance_name }};
        }
    }
}";
        }
    }
}
