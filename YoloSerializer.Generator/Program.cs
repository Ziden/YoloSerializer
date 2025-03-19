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

            // Make sure AllTypesData is included
            var allTypesDataType = typeof(AllTypesData);
            
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
            string templateContent = SerializerTemplate.GetSerializerTemplate();
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
            
            // Generate YoloGeneratedMap and YoloGeneratedSerializer
            await GenerateTypeMap(serializableTypes, outputPath, forceRegeneration);
            await GenerateYoloSerializer(outputPath, forceRegeneration);
            
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
        
        private static async Task GenerateTypeMap(List<Type> serializableTypes, string outputPath, bool forceRegeneration)
        {
            string outputFile = Path.Combine(outputPath, "YoloGeneratedMap.cs");
            
            // Skip generation if the file already exists to avoid overriding manual implementations
            if (File.Exists(outputFile) && !forceRegeneration)
            {
                Console.WriteLine("Skipping YoloGeneratedMap.cs - file already exists (use --force to override)");
                return;
            }
            
            Console.WriteLine("Generating YoloGeneratedMap.cs");
            
            // Build the map file content
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Buffers.Binary;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using YoloSerializer.Core.Contracts;");
            sb.AppendLine("using YoloSerializer.Core.Models;");
            sb.AppendLine("using YoloSerializer.Core.Serializers;");
            sb.AppendLine();
            sb.AppendLine("namespace YoloSerializer.Tests.Generated");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Hard-coded map of type IDs to serializers for maximum performance");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public sealed class YoloGeneratedMap : ITypeMap");
            sb.AppendLine("    {");
            sb.AppendLine("        // Singleton instance for performance");
            sb.AppendLine("        private static readonly YoloGeneratedMap _instance = new YoloGeneratedMap();");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the singleton instance");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static YoloGeneratedMap Instance => _instance;");
            sb.AppendLine("        ");
            sb.AppendLine("        // Private constructor to enforce singleton pattern");
            sb.AppendLine("        private YoloGeneratedMap() { }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Type ID used for null values");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public const byte NULL_TYPE_ID = 0;");
            sb.AppendLine();
            sb.AppendLine("        #region codegen");
            
            // Add type IDs for each serializable type
            byte typeId = 1;
            foreach (var type in serializableTypes)
            {
                string typeName = type.Name.ToUpperInvariant();
                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// Type ID for {type.Name}");
                sb.AppendLine($"        /// </summary>");
                sb.AppendLine($"        public const byte {typeName}_TYPE_ID = {typeId++};");
                sb.AppendLine("        ");
            }
            
            sb.AppendLine("        #endregion");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the null type ID for the ITypeMap interface");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        byte ITypeMap.NullTypeId => NULL_TYPE_ID;");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the type ID for a type");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public byte GetTypeId<T>() where T : class, IYoloSerializable");
            sb.AppendLine("        {");
            sb.AppendLine("            Type type = typeof(T);");
            sb.AppendLine();
            sb.AppendLine("            #region codegen");
            
            // Add type ID lookup for each serializable type
            foreach (var type in serializableTypes)
            {
                string typeName = type.Name.ToUpperInvariant();
                sb.AppendLine($"            if (type == typeof({type.Name}))");
                sb.AppendLine($"                return {typeName}_TYPE_ID;");
                sb.AppendLine("                ");
            }
            
            sb.AppendLine("            #endregion");
            sb.AppendLine("            throw new ArgumentException($\"Unknown type: {type.Name}\");");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Serializes an object to a byte span without boxing");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public void Serialize<T>(T obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (obj)");
            sb.AppendLine("            {");
            
            // Add serialization cases for each type
            foreach (var type in serializableTypes)
            {
                sb.AppendLine($"                case {type.Name} {char.ToLowerInvariant(type.Name[0])}{type.Name.Substring(1)}:");
                sb.AppendLine($"                    {type.Name}Serializer.Instance.Serialize({char.ToLowerInvariant(type.Name[0])}{type.Name.Substring(1)}, buffer, ref offset);");
                sb.AppendLine("                    break;");
                sb.AppendLine("                ");
            }
            
            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type: {obj.GetType().Name}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the serialized size of an object without boxing");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public int GetSerializedSize<T>(T obj) where T : class, IYoloSerializable");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (obj)");
            sb.AppendLine("            {");
            
            // Add size calculation cases for each type
            foreach (var type in serializableTypes)
            {
                sb.AppendLine($"                case {type.Name} {char.ToLowerInvariant(type.Name[0])}{type.Name.Substring(1)}:");
                sb.AppendLine($"                    return {type.Name}Serializer.Instance.GetSize({char.ToLowerInvariant(type.Name[0])}{type.Name.Substring(1)});");
                sb.AppendLine("                ");
            }
            
            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type: {obj.GetType().Name}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deserializes an object from a byte span based on type ID");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public object? DeserializeById(byte typeId, ReadOnlySpan<byte> buffer, ref int offset)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (typeId)");
            sb.AppendLine("            {");
            
            // Add deserialization cases for each type
            foreach (var type in serializableTypes)
            {
                string typeName = type.Name.ToUpperInvariant();
                string isValueType = !IsReferenceType(type) ? "" : "?";
                
                sb.AppendLine($"                case {typeName}_TYPE_ID:");
                sb.AppendLine($"                    {type.Name}{isValueType} {char.ToLowerInvariant(type.Name[0])}{type.Name.Substring(1)}Result;");
                sb.AppendLine($"                    {type.Name}Serializer.Instance.Deserialize(out {char.ToLowerInvariant(type.Name[0])}{type.Name.Substring(1)}Result, buffer, ref offset);");
                sb.AppendLine($"                    return {char.ToLowerInvariant(type.Name[0])}{type.Name.Substring(1)}Result;");
                sb.AppendLine("                ");
            }
            
            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type ID: {typeId}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            // Write the generated code to a file
            await File.WriteAllTextAsync(outputFile, sb.ToString());
            
            Console.WriteLine($"Generated {outputFile}");
        }

        private static async Task GenerateYoloSerializer(string outputPath, bool forceRegeneration)
        {
            string outputFile = Path.Combine(outputPath, "YoloGeneratedSerializer.cs");
            
            // Skip generation if the file already exists to avoid overriding manual implementations
            if (File.Exists(outputFile) && !forceRegeneration)
            {
                Console.WriteLine("Skipping YoloGeneratedSerializer.cs - file already exists (use --force to override)");
                return;
            }
            
            Console.WriteLine("Generating YoloGeneratedSerializer.cs");
            
            // Build the serializer file content
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Buffers.Binary;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using YoloSerializer.Core;");
            sb.AppendLine("using YoloSerializer.Core.Contracts;");
            sb.AppendLine("using YoloSerializer.Core.Models;");
            sb.AppendLine("using YoloSerializer.Core.Serializers;");
            sb.AppendLine("using YoloSerializer.Tests.Generated;");
            sb.AppendLine();
            sb.AppendLine("namespace YoloSerializer.Core.Serializers");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// High-performance serializer using YoloGeneratedMap for optimal dispatch");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public sealed class YoloGeneratedSerializer");
            sb.AppendLine("    {");
            sb.AppendLine("        private static readonly YoloGeneratedSerializer _instance = new YoloGeneratedSerializer();");
            sb.AppendLine("        private readonly GeneratedSerializer<YoloGeneratedMap> _serializer;");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Singleton instance for performance");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static YoloGeneratedSerializer Instance => _instance;");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Constructor - initializes with YoloGeneratedMap");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        private YoloGeneratedSerializer()");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer = new GeneratedSerializer<YoloGeneratedMap>(YoloGeneratedMap.Instance);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Serializes an object to a byte span");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer.Serialize(obj, buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Serializes an object to a byte span with pre-computed size");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : class, IYoloSerializable");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deserializes an object from a byte span");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public object? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.Deserialize(buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deserializes an object from a byte span with strong typing");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public T? Deserialize<T>(ReadOnlySpan<byte> buffer, ref int offset) where T : class, IYoloSerializable");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.Deserialize<T>(buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the serialized size of an object");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public int GetSerializedSize<T>(T? obj) where T : class, IYoloSerializable");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.GetSerializedSize(obj);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            // Write the generated code to a file
            await File.WriteAllTextAsync(outputFile, sb.ToString());
            
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
            // New primitive types
            else if (unwrappedType == typeof(byte))
            {
                propertyInfo.SizeCalculation = $"ByteSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"ByteSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"ByteSerializer.Instance.Deserialize(out byte {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(sbyte))
            {
                propertyInfo.SizeCalculation = $"SByteSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"SByteSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"SByteSerializer.Instance.Deserialize(out sbyte {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(char))
            {
                propertyInfo.SizeCalculation = $"CharSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"CharSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"CharSerializer.Instance.Deserialize(out char {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(short))
            {
                propertyInfo.SizeCalculation = $"Int16Serializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"Int16Serializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"Int16Serializer.Instance.Deserialize(out short {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(ushort))
            {
                propertyInfo.SizeCalculation = $"UInt16Serializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"UInt16Serializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"UInt16Serializer.Instance.Deserialize(out ushort {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(uint))
            {
                propertyInfo.SizeCalculation = $"UInt32Serializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"UInt32Serializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"UInt32Serializer.Instance.Deserialize(out uint {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(ulong))
            {
                propertyInfo.SizeCalculation = $"UInt64Serializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"UInt64Serializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"UInt64Serializer.Instance.Deserialize(out ulong {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(decimal))
            {
                propertyInfo.SizeCalculation = $"DecimalSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"DecimalSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"DecimalSerializer.Instance.Deserialize(out decimal {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(DateTime))
            {
                propertyInfo.SizeCalculation = $"DateTimeSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"DateTimeSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"DateTimeSerializer.Instance.Deserialize(out DateTime {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(TimeSpan))
            {
                propertyInfo.SizeCalculation = $"TimeSpanSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"TimeSpanSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"TimeSpanSerializer.Instance.Deserialize(out TimeSpan {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType == typeof(Guid))
            {
                propertyInfo.SizeCalculation = $"GuidSerializer.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"GuidSerializer.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"GuidSerializer.Instance.Deserialize(out Guid {localVarName}, buffer, ref offset);\n" +
                                               $"            {instancePropRef} = {localVarName};";
            }
            else if (unwrappedType.IsEnum)
            {
                propertyInfo.SizeCalculation = $"EnumSerializer<{GetFullTypeName(unwrappedType)}>.Instance.GetSize({instancePropRef})";
                propertyInfo.SerializeCode = $"EnumSerializer<{GetFullTypeName(unwrappedType)}>.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                propertyInfo.DeserializeCode = $"EnumSerializer<{GetFullTypeName(unwrappedType)}>.Instance.Deserialize(out {GetFullTypeName(unwrappedType)} {localVarName}, buffer, ref offset);\n" +
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
            
            // New primitive types
            if (unwrappedType == typeof(byte)) return "ByteSerializer";
            if (unwrappedType == typeof(sbyte)) return "SByteSerializer";
            if (unwrappedType == typeof(char)) return "CharSerializer";
            if (unwrappedType == typeof(short)) return "Int16Serializer";
            if (unwrappedType == typeof(ushort)) return "UInt16Serializer";
            if (unwrappedType == typeof(uint)) return "UInt32Serializer";
            if (unwrappedType == typeof(ulong)) return "UInt64Serializer";
            if (unwrappedType == typeof(decimal)) return "DecimalSerializer";
            if (unwrappedType == typeof(DateTime)) return "DateTimeSerializer";
            if (unwrappedType == typeof(TimeSpan)) return "TimeSpanSerializer";
            if (unwrappedType == typeof(Guid)) return "GuidSerializer";
            
            // For enums
            if (unwrappedType.IsEnum) return $"EnumSerializer<{GetFullTypeName(unwrappedType)}>";
            
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
            
            // New primitive types
            if (type == typeof(byte)) return "byte";
            if (type == typeof(sbyte)) return "sbyte";
            if (type == typeof(char)) return "char";
            if (type == typeof(short)) return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(DateTime)) return "DateTime";
            if (type == typeof(TimeSpan)) return "TimeSpan";
            if (type == typeof(Guid)) return "Guid";
            
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
    }
}
