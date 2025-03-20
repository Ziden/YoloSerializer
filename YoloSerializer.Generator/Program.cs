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
    /// Configuration for the serializer generator
    /// </summary>
    class GeneratorConfig
    {
        public Assembly TargetAssembly { get; set; }
        public string OutputPath { get; set; }
        public bool ForceRegeneration { get; set; }
    }

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
        // Define explicitly serializable types here
        private static readonly Type[] ExplicitSerializableTypes = new[]
        {
            typeof(PlayerData),
            typeof(Position),
            typeof(AllTypesData)
        };

        static async Task Main(string[] args)
        {
            var config = ParseCommandLineArgs(args);
            
            Directory.CreateDirectory(config.OutputPath);
            
            Console.WriteLine($"Generating serializers for types in {config.TargetAssembly.FullName}");
            Console.WriteLine($"Output path: {config.OutputPath}");
            Console.WriteLine($"Force regeneration: {config.ForceRegeneration}");
            
            // Parse the serializer template
            string templateContent = SerializerTemplate.GetSerializerTemplate();
            var template = Template.Parse(templateContent);
            
            // Get list of serializable types
            var serializableTypes = ExplicitSerializableTypes.ToList();
            Console.WriteLine($"Found {serializableTypes.Count} serializable types");
            
            // Generate all required files
            await GenerateSerializers(serializableTypes, template, config);
            
            Console.WriteLine("Done!");
        }

        private static GeneratorConfig ParseCommandLineArgs(string[] args)
        {
            // Default configuration
            var config = new GeneratorConfig
            {
                TargetAssembly = typeof(PlayerData).Assembly,
                OutputPath = GetDefaultOutputPath(),
                ForceRegeneration = args.Contains("--force") || args.Contains("-f")
            };
            
            // Parse positional arguments
            var filteredArgs = args.Where(a => !a.StartsWith("-")).ToArray();
            if (filteredArgs.Length >= 1)
            {
                string assemblyPath = filteredArgs[0];
                config.TargetAssembly = Assembly.LoadFrom(assemblyPath);
            }
            
            if (filteredArgs.Length >= 2)
            {
                config.OutputPath = Path.GetFullPath(filteredArgs[1]);
            }
            
            return config;
        }
        
        private static string GetDefaultOutputPath()
        {
            var solutionDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
            return Path.GetFullPath(Path.Combine(solutionDir, "YoloSerializer.Tests", "Generated"));
        }
        
        private static async Task GenerateSerializers(List<Type> serializableTypes, Template template, GeneratorConfig config)
        {
            // Generate individual serializers for each type
            foreach (var type in serializableTypes)
            {
                await GenerateSerializer(type, template, config.OutputPath, config.ForceRegeneration);
            }
            
            // Generate support files
            await GenerateTypeMap(serializableTypes, config.OutputPath, config.ForceRegeneration);
            await GenerateYoloSerializer(config.OutputPath, config.ForceRegeneration);
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
            var properties = GetTypeProperties(type);
            
            // Create template context and render
            var templateContext = CreateTemplateContext(type, properties);
            var result = await template.RenderAsync(templateContext);
            
            // Write the generated code to a file
            await File.WriteAllTextAsync(outputFile, result);
            
            Console.WriteLine($"Generated {outputFile}");
        }
        
        private static List<PropertyInfo> GetTypeProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => BuildPropertyInfo(p))
                .ToList();
        }
        
        private static object CreateTemplateContext(Type type, List<PropertyInfo> properties)
        {
            // Create property size calculation and serialization code blocks
            string sizeCalculation = CreateSizeCalculationCode(properties);
            string serializeCode = CreateSerializeCode(properties);
            string deserializeCode = CreateDeserializeCode(properties);
            
            // Generate instance variable name (camelCase)
            var instanceVarName = char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1);
            
            // Create template context
            return new
            {
                class_name = type.Name,
                full_type_name = GetFullTypeName(type),
                serializer_name = $"{type.Name}Serializer",
                instance_var_name = instanceVarName,
                class_variable_name = char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1),
                is_class = IsReferenceType(type),
                needs_null_check = IsReferenceType(type),
                nullable_marker = IsReferenceType(type) ? "?" : "",
                size_calculation = sizeCalculation,
                serialize_code = serializeCode,
                deserialize_code = deserializeCode,
                properties = properties
            };
        }
        
        private static string CreateSizeCalculationCode(List<PropertyInfo> properties)
        {
            return string.Join("\n            ", properties.Select(p => 
                $"// Size of {p.Name} ({p.Type})\nsize += {p.SizeCalculation};"));
        }
        
        private static string CreateSerializeCode(List<PropertyInfo> properties)
        {
            return string.Join("\n            ", properties.Select(p => 
                $"// Serialize {p.Name} ({p.Type})\n{p.SerializeCode}"));
        }
        
        private static string CreateDeserializeCode(List<PropertyInfo> properties)
        {
            return string.Join("\n            ", properties.Select(p => 
                $"// Read {p.Name}\n{p.DeserializeCode}"));
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
            
            // Generate imports
            AppendImports(sb);
            
            // Begin class definition
            AppendClassHeader(sb);
            
            // Generate type IDs
            AppendTypeIds(sb, serializableTypes);
            
            // Generate methods
            AppendGetTypeIdMethod(sb, serializableTypes);
            AppendSerializeMethod(sb, serializableTypes);
            AppendGetSizeMethod(sb, serializableTypes);
            AppendDeserializeMethod(sb, serializableTypes);
            
            // Close class
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            // Write the generated code to a file
            await File.WriteAllTextAsync(outputFile, sb.ToString());
            
            Console.WriteLine($"Generated {outputFile}");
        }
        
        private static void AppendImports(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Buffers.Binary;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using YoloSerializer.Core.Contracts;");
            sb.AppendLine("using YoloSerializer.Core.Models;");
            sb.AppendLine("using YoloSerializer.Core.Serializers;");
            sb.AppendLine("using YoloSerializer.Tests.Generated;");
            sb.AppendLine();
            sb.AppendLine("namespace YoloSerializer.Tests.Generated");
            sb.AppendLine("{");
        }
        
        private static void AppendClassHeader(StringBuilder sb)
        {
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
        }
        
        private static void AppendTypeIds(StringBuilder sb, List<Type> serializableTypes)
        {
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
        }
        
        private static void AppendGetTypeIdMethod(StringBuilder sb, List<Type> serializableTypes)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the type ID for a type");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public byte GetTypeId<T>()");
            sb.AppendLine("        {");
            sb.AppendLine("            Type type = typeof(T);");
            sb.AppendLine();
            sb.AppendLine("            #region codegen");
            
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
        }
        
        private static void AppendSerializeMethod(StringBuilder sb, List<Type> serializableTypes)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Serializes an object to a byte span without boxing");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public void Serialize<T>(T obj, Span<byte> buffer, ref int offset)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (obj)");
            sb.AppendLine("            {");
            
            foreach (var type in serializableTypes)
            {
                string varName = char.ToLower(type.Name[0]) + type.Name.Substring(1);
                sb.AppendLine($"                case {type.Name} {varName}:");
                sb.AppendLine($"                    {type.Name}Serializer.Instance.Serialize({varName}, buffer, ref offset);");
                sb.AppendLine("                    break;");
                sb.AppendLine("                ");
            }
            
            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type: {obj.GetType().Name}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
        }
        
        private static void AppendGetSizeMethod(StringBuilder sb, List<Type> serializableTypes)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the serialized size of an object without boxing");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public int GetSerializedSize<T>(T obj)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (obj)");
            sb.AppendLine("            {");
            
            foreach (var type in serializableTypes)
            {
                string varName = char.ToLower(type.Name[0]) + type.Name.Substring(1);
                sb.AppendLine($"                case {type.Name} {varName}:");
                sb.AppendLine($"                    return {type.Name}Serializer.Instance.GetSize({varName});");
                sb.AppendLine("                ");
            }
            
            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type: {obj.GetType().Name}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
        }
        
        private static void AppendDeserializeMethod(StringBuilder sb, List<Type> serializableTypes)
        {
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Deserializes an object from a byte span based on type ID");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public object? DeserializeById(byte typeId, ReadOnlySpan<byte> buffer, ref int offset)");
            sb.AppendLine("        {");
            sb.AppendLine("            switch (typeId)");
            sb.AppendLine("            {");
            
            foreach (var type in serializableTypes)
            {
                string typeName = type.Name.ToUpperInvariant();
                string varName = char.ToLower(type.Name[0]) + type.Name.Substring(1);
                sb.AppendLine($"                case {typeName}_TYPE_ID:");
                sb.AppendLine($"                    {type.Name}? {varName}Result;");
                sb.AppendLine($"                    {type.Name}Serializer.Instance.Deserialize(out {varName}Result, buffer, ref offset);");
                sb.AppendLine($"                    return {varName}Result;");
                sb.AppendLine("                ");
            }
            
            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type ID: {typeId}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
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
            
            // Generate imports and class definition
            AppendSerializerImports(sb);
            AppendSerializerClassHeader(sb);
            
            // Generate methods
            AppendGenericSerializeMethods(sb);
            AppendTypeSpecificSerializeMethods(sb);
            AppendGenericDeserializeMethods(sb);
            AppendTypeSpecificDeserializeMethods(sb);
            AppendSizeCalculationMethods(sb);
            
            // Close class
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            // Write the generated code to a file
            await File.WriteAllTextAsync(outputFile, sb.ToString());
            
            Console.WriteLine($"Generated {outputFile}");
        }
        
        private static void AppendSerializerImports(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Buffers.Binary;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using YoloSerializer.Core;");
            sb.AppendLine("using YoloSerializer.Core.Models;");
            sb.AppendLine("using YoloSerializer.Core.Serializers;");
            sb.AppendLine("using YoloSerializer.Core.Contracts;");
            sb.AppendLine("using YoloSerializer.Tests.Generated;");
            sb.AppendLine();
            sb.AppendLine("namespace YoloSerializer.Core.Serializers");
            sb.AppendLine("{");
        }
        
        private static void AppendSerializerClassHeader(StringBuilder sb)
        {
            sb.AppendLine("    public sealed class YoloGeneratedSerializer");
            sb.AppendLine("    {");
            sb.AppendLine("        private static readonly YoloGeneratedSerializer _instance = new YoloGeneratedSerializer();");
            sb.AppendLine("        private readonly GeneratedSerializer<YoloGeneratedMap> _serializer;");
            sb.AppendLine("        ");
            sb.AppendLine("        public static YoloGeneratedSerializer Instance => _instance;");
            sb.AppendLine("        ");
            sb.AppendLine("        private YoloGeneratedSerializer()");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer = new GeneratedSerializer<YoloGeneratedMap>(YoloGeneratedMap.Instance);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
        }
        
        private static void AppendGenericSerializeMethods(StringBuilder sb)
        {
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer.Serialize(obj, buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
        }
        
        private static void AppendTypeSpecificSerializeMethods(StringBuilder sb)
        {
            // Generate type-specific methods for serializing
            foreach (var serializableType in ExplicitSerializableTypes)
            {
                string typeName = serializableType.Name;
                sb.AppendLine($"        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"        public void Serialize({typeName}? obj, Span<byte> buffer, ref int offset)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            _serializer.Serialize(obj, buffer, ref offset);");
                sb.AppendLine($"        }}");
                sb.AppendLine($"        ");
                sb.AppendLine($"        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"        public void SerializeWithoutSizeCheck({typeName}? obj, Span<byte> buffer, ref int offset)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);");
                sb.AppendLine($"        }}");
                sb.AppendLine($"        ");
            }
        }
        
        private static void AppendGenericDeserializeMethods(StringBuilder sb)
        {
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public object? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.Deserialize(buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public T? Deserialize<T>(ReadOnlySpan<byte> buffer, ref int offset) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.Deserialize<T>(buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
        }
        
        private static void AppendTypeSpecificDeserializeMethods(StringBuilder sb)
        {
            foreach (var serializableType in ExplicitSerializableTypes)
            {
                string typeName = serializableType.Name;
                sb.AppendLine($"        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"        public {typeName}? Deserialize{typeName}(ReadOnlySpan<byte> buffer, ref int offset)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return _serializer.Deserialize<{typeName}>(buffer, ref offset);");
                sb.AppendLine($"        }}");
                sb.AppendLine($"        ");
            }
        }
        
        private static void AppendSizeCalculationMethods(StringBuilder sb)
        {
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public int GetSerializedSize<T>(T? obj) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.GetSerializedSize(obj);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            foreach (var serializableType in ExplicitSerializableTypes)
            {
                string typeName = serializableType.Name;
                sb.AppendLine($"        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"        public int GetSerializedSize({typeName}? obj)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return _serializer.GetSerializedSize(obj);");
                sb.AppendLine($"        }}");
                sb.AppendLine($"        ");
            }
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
            
            // Check if this is a serializable type (from our explicit list)
            propertyInfo.IsYoloSerializable = IsExplicitSerializableType(unwrappedType);
            
            // Determine the type of property and set appropriate serialization code
            if (IsList(unwrappedType))
            {
                propertyInfo.IsList = true;
                propertyInfo.ElementType = GetElementType(unwrappedType);
                SetListPropertyCodeAndSize(propertyInfo, property);
            }
            else if (IsDictionary(unwrappedType))
            {
                propertyInfo.IsDictionary = true;
                var genericArgs = unwrappedType.GetGenericArguments();
                propertyInfo.KeyType = GetFullTypeName(genericArgs[0]);
                propertyInfo.ValueType = GetFullTypeName(genericArgs[1]);
                SetDictionaryPropertyCodeAndSize(propertyInfo, property);
            }
            else if (unwrappedType.IsArray)
            {
                propertyInfo.IsArray = true;
                propertyInfo.ElementType = GetFullTypeName(unwrappedType.GetElementType());
                SetArrayPropertyCodeAndSize(propertyInfo, property);
            }
            else
            {
                SetStandardPropertyCodeAndSize(propertyInfo, property);
            }
            
            return propertyInfo;
        }
        
        private static bool IsExplicitSerializableType(Type type)
        {
            return ExplicitSerializableTypes.Contains(type);
        }
        
        private static bool IsList(Type type)
        {
            return typeof(IList).IsAssignableFrom(type) && type.IsGenericType;
        }
        
        private static bool IsDictionary(Type type) 
        {
            return typeof(IDictionary).IsAssignableFrom(type) && type.IsGenericType;
        }
        
        private static string GetElementType(Type listType)
        {
            return GetFullTypeName(listType.GetGenericArguments()[0]);
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
            
            propertyInfo.SizeCalculation = $"Int32Serializer.Instance.GetSize({instancePropRef}.Count) + " +
                                            $"{instancePropRef}.Sum(listItem => {GetSerializerForType(elementType)}.Instance.GetSize(listItem))";
            
            propertyInfo.SerializeCode = 
                $"Int32Serializer.Instance.Serialize({instancePropRef}.Count, buffer, ref offset);\n" +
                $"            foreach (var listItem in {instancePropRef})\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(elementType)}.Instance.Serialize(listItem, buffer, ref offset);\n" +
                $"            }}";
                
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
            
            // Check built-in primitive types first
            if (PrimitiveTypeSerializers.TryGetValue(unwrappedType, out string serializerName))
                return serializerName;
            
            // For enums
            if (unwrappedType.IsEnum) 
                return $"EnumSerializer<{GetFullTypeName(unwrappedType)}>";
            
            // For custom serializable types
            if (IsExplicitSerializableType(unwrappedType))
                return $"{unwrappedType.Name}Serializer";
            
            throw new ArgumentException($"No serializer found for type {unwrappedType.Name}");
        }
        
        // Dictionary mapping primitive types to their serializer names
        private static readonly Dictionary<Type, string> PrimitiveTypeSerializers = new Dictionary<Type, string>
        {
            { typeof(int), "Int32Serializer" },
            { typeof(float), "FloatSerializer" },
            { typeof(double), "DoubleSerializer" },
            { typeof(long), "Int64Serializer" },
            { typeof(bool), "BooleanSerializer" },
            { typeof(string), "StringSerializer" },
            { typeof(byte), "ByteSerializer" },
            { typeof(sbyte), "SByteSerializer" },
            { typeof(char), "CharSerializer" },
            { typeof(short), "Int16Serializer" },
            { typeof(ushort), "UInt16Serializer" },
            { typeof(uint), "UInt32Serializer" },
            { typeof(ulong), "UInt64Serializer" },
            { typeof(decimal), "DecimalSerializer" },
            { typeof(DateTime), "DateTimeSerializer" },
            { typeof(TimeSpan), "TimeSpanSerializer" },
            { typeof(Guid), "GuidSerializer" }
        };
        
        // Dictionary mapping types to their C# keywords
        private static readonly Dictionary<Type, string> TypeNameMappings = new Dictionary<Type, string>
        {
            { typeof(int), "int" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(long), "long" },
            { typeof(bool), "bool" },
            { typeof(string), "string" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(decimal), "decimal" },
            { typeof(DateTime), "DateTime" },
            { typeof(TimeSpan), "TimeSpan" },
            { typeof(Guid), "Guid" }
        };
        
        private static string GetFullTypeName(Type type)
        {
            // Handle primitive types with keywords
            if (TypeNameMappings.TryGetValue(type, out string typeName))
                return typeName;
            
            // Handle nullable types
            Type? nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullableUnderlyingType != null)
                return $"{GetFullTypeName(nullableUnderlyingType)}?";
            
            // Handle generic types
            if (type.IsGenericType)
            {
                var genericTypeName = type.GetGenericTypeDefinition().Name;
                genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
                
                string genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFullTypeName));
                return $"{genericTypeName}<{genericArgs}>";
            }
            
            // Handle arrays
            if (type.IsArray)
                return $"{GetFullTypeName(type.GetElementType())}[]";
            
            // Default to type name
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
