using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YoloSerializer.Core.Models;
using YoloSerializer.Generator.Models;

namespace YoloSerializer.Generator
{
    public class CodeGenerator
    {
        private Type[] _serializableTypes;

        public async Task GenerateSerializers(List<Type> serializableTypes, Template template, GeneratorConfig config)
        {
            _serializableTypes = serializableTypes.ToArray();
            // Generate individual serializers for each type
            foreach (var type in serializableTypes)
            {
                await GenerateSerializer(type, template, config.OutputPath, config.ForceRegeneration);
            }

            // Generate support files
            await GenerateTypeMap(serializableTypes, config.OutputPath, config.ForceRegeneration);
            await GenerateYoloSerializer(config.OutputPath, config.ForceRegeneration);
        }

        internal async Task GenerateSerializer(Type type, Template template, string outputPath, bool forceRegeneration)
        {
            string outputFile = Path.Combine(outputPath, $"{type.Name}Serializer.cs");

            // Skip generation if the file already exists to avoid overriding manual implementations
            if (File.Exists(outputFile) && !forceRegeneration)
            {
                Console.WriteLine($"Skipping {type.Name}Serializer.cs - file already exists (use --force to override)");
                return;
            }

            Console.WriteLine($"Generating serializer for {type.Name}");

            // Get all public properties of the type
            var properties = GetTypeProperties(type);

            // Generate instance variable name (camelCase)
            var instanceVarName = char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1);

            // Create property size calculation and serialization code blocks
            string sizeCalculation = CreateSizeCalculationCode(properties);
            string serializeCode = CreateSerializeCode(properties);
            string deserializeCode = CreateDeserializeCode(properties);

            // Create template context
            var templateContext = CreateTemplateContext(
                type,
                properties,
                type.Name,
                GetFullTypeName(type),
                $"{type.Name}Serializer",
                instanceVarName,
                IsReferenceType(type),
                IsReferenceType(type),
                IsReferenceType(type) ? "?" : "",
                sizeCalculation,
                serializeCode,
                deserializeCode
            );

            // Parse and render the template
            var result = await template.RenderAsync(templateContext);

            // Write the generated code to a file
            await File.WriteAllTextAsync(outputFile, result);

            Console.WriteLine($"Generated {outputFile}");
        }

        internal List<TemplatePropertyInfo> GetTypeProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => BuildPropertyInfo(p))
                .ToList();
        }

        internal SerializerTemplateModel CreateTemplateContext(
            Type type,
            List<TemplatePropertyInfo> properties,
            string className,
            string fullTypeName,
            string serializerName,
            string instanceVarName,
            bool isClass,
            bool needsNullCheck,
            string nullableMarker,
            string sizeCalculation,
            string serializeCode,
            string deserializeCode)
        {
            return new SerializerTemplateModel
            {
                ClassName = className,
                FullTypeName = fullTypeName,
                SerializerName = serializerName,
                InstanceVarName = instanceVarName,
                IsClass = isClass,
                NeedsNullCheck = needsNullCheck,
                NullableMarker = nullableMarker,
                SizeCalculation = sizeCalculation,
                SerializeCode = serializeCode,
                DeserializeCode = deserializeCode,
                Properties = properties.Select(p => new PropertyTemplateModel
                {
                    Name = p.Name,
                    TypeName = p.Type,
                    SerializerName = GetSerializerForType(p.PropertyType),
                    InstanceVarName = GetInstanceVarName(p.PropertyType),
                    IsNullable = p.IsNullable,
                    IsList = p.IsList,
                    IsDictionary = p.IsDictionary,
                    IsArray = p.IsArray,
                    ElementTypeName = p.ElementType,
                    KeyTypeName = p.KeyType,
                    ValueTypeName = p.ValueType,
                    SizeCalculation = p.SizeCalculation,
                    SerializeCode = p.SerializeCode,
                    DeserializeCode = p.DeserializeCode
                }).ToList()
            };
        }

        internal string CreateSizeCalculationCode(List<TemplatePropertyInfo> properties)
        {
            return string.Join("\n            ", properties.Select(p =>
                $"size += {p.SizeCalculation};"));
        }

        internal string CreateSerializeCode(List<TemplatePropertyInfo> properties)
        {
            return string.Join("\n            ", properties.Select(p =>
                $"{p.SerializeCode}"));
        }

        internal string CreateDeserializeCode(List<TemplatePropertyInfo> properties)
        {
            return string.Join("\n            ", properties.Select(p =>
                $"{p.DeserializeCode}"));
        }

        internal async Task GenerateTypeMap(List<Type> serializableTypes, string outputPath, bool forceRegeneration)
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

        internal void AppendImports(StringBuilder sb)
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

        internal void AppendClassHeader(StringBuilder sb)
        {
            sb.AppendLine("    public sealed class YoloGeneratedMap : ITypeMap");
            sb.AppendLine("    {");
            sb.AppendLine("        private static readonly YoloGeneratedMap _instance = new YoloGeneratedMap();");
            sb.AppendLine("        public static YoloGeneratedMap Instance => _instance;");
            sb.AppendLine("        private YoloGeneratedMap() { }");
            sb.AppendLine("        public const byte NULL_TYPE_ID = 0;");
            sb.AppendLine("        #region codegen");
        }

        internal void AppendTypeIds(StringBuilder sb, List<Type> serializableTypes)
        {
            byte typeId = 1;
            foreach (var type in serializableTypes)
            {
                string typeName = type.Name.ToUpperInvariant();
                sb.AppendLine($"        public const byte {typeName}_TYPE_ID = {typeId++};");
            }
            sb.AppendLine("        #endregion");
            sb.AppendLine("        byte ITypeMap.NullTypeId => NULL_TYPE_ID;");
        }

        internal void AppendGetTypeIdMethod(StringBuilder sb, List<Type> serializableTypes)
        {
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public byte GetTypeId<T>()");
            sb.AppendLine("        {");
            sb.AppendLine("            Type type = typeof(T);");
            sb.AppendLine("            #region codegen");

            foreach (var type in serializableTypes)
            {
                string typeName = type.Name.ToUpperInvariant();
                sb.AppendLine($"            if (type == typeof({type.Name}))");
                sb.AppendLine($"                return {typeName}_TYPE_ID;");
            }

            sb.AppendLine("            #endregion");
            sb.AppendLine("            throw new ArgumentException($\"Unknown type: {type.Name}\");");
            sb.AppendLine("        }");
        }

        internal void AppendSerializeMethod(StringBuilder sb, List<Type> serializableTypes)
        {
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
            }

            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type: {obj.GetType().Name}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }

        internal void AppendGetSizeMethod(StringBuilder sb, List<Type> serializableTypes)
        {
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
            }

            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type: {obj.GetType().Name}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }

        internal void AppendDeserializeMethod(StringBuilder sb, List<Type> serializableTypes)
        {
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
            }

            sb.AppendLine("                default:");
            sb.AppendLine("                    throw new ArgumentException($\"Unknown type ID: {typeId}\");");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }

        internal async Task GenerateYoloSerializer(string outputPath, bool forceRegeneration)
        {
            string outputFile = Path.Combine(outputPath, "YoloGeneratedSerializer.cs");
            if (File.Exists(outputFile) && !forceRegeneration)
            {
                Console.WriteLine("Skipping YoloGeneratedSerializer.cs - file already exists (use --force to override)");
                return;
            }
            Console.WriteLine("Generating YoloGeneratedSerializer.cs");
            var sb = new StringBuilder();
            AppendSerializerImports(sb);
            AppendSerializerClassHeader(sb);
            AppendGenericSerializeMethods(sb);
            AppendTypeSpecificSerializeMethods(sb);
            AppendGenericDeserializeMethods(sb);
            AppendTypeSpecificDeserializeMethods(sb);
            AppendSizeCalculationMethods(sb);
            sb.AppendLine("    }");
            sb.AppendLine("}");
            await File.WriteAllTextAsync(outputFile, sb.ToString());
            Console.WriteLine($"Generated {outputFile}");
        }

        internal void AppendSerializerImports(StringBuilder sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Buffers.Binary;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using YoloSerializer.Core;");
            sb.AppendLine("using YoloSerializer.Core.Models;");
            sb.AppendLine("using YoloSerializer.Core.Serializers;");
            sb.AppendLine("using YoloSerializer.Core.Contracts;");
            sb.AppendLine("using YoloSerializer.Tests.Generated;");
            sb.AppendLine("namespace YoloSerializer.Core.Serializers");
            sb.AppendLine("{");
        }

        internal void AppendSerializerClassHeader(StringBuilder sb)
        {
            sb.AppendLine("    public sealed class YoloGeneratedSerializer");
            sb.AppendLine("    {");
            sb.AppendLine("        public static readonly YoloGeneratedSerializer _instance = new YoloGeneratedSerializer();");
            sb.AppendLine("        private readonly GeneratedSerializer<YoloGeneratedMap> _serializer;");
            sb.AppendLine("        public static YoloGeneratedSerializer Instance => _instance;");
            sb.AppendLine("        private YoloGeneratedSerializer()");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer = new GeneratedSerializer<YoloGeneratedMap>(YoloGeneratedMap.Instance);");
            sb.AppendLine("        }");
        }

        internal void AppendGenericSerializeMethods(StringBuilder sb)
        {
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public void Serialize<T>(T? obj, Span<byte> buffer, ref int offset) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer.Serialize(obj, buffer, ref offset);");
            sb.AppendLine("        }");

            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public void SerializeWithoutSizeCheck<T>(T? obj, Span<byte> buffer, ref int offset) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);");
            sb.AppendLine("        }");
        }

        internal void AppendTypeSpecificSerializeMethods(StringBuilder sb)
        {
            foreach (var serializableType in _serializableTypes)
            {
                string typeName = serializableType.Name;
                sb.AppendLine($"        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"        public void Serialize({typeName}? obj, Span<byte> buffer, ref int offset)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            _serializer.Serialize(obj, buffer, ref offset);");
                sb.AppendLine($"        }}");
                sb.AppendLine($"        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"        public void SerializeWithoutSizeCheck({typeName}? obj, Span<byte> buffer, ref int offset)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            _serializer.SerializeWithoutSizeCheck(obj, buffer, ref offset);");
                sb.AppendLine($"        }}");
            }
        }

        internal void AppendGenericDeserializeMethods(StringBuilder sb)
        {
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public object? Deserialize(ReadOnlySpan<byte> buffer, ref int offset)");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.Deserialize(buffer, ref offset);");
            sb.AppendLine("        }");
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public T? Deserialize<T>(ReadOnlySpan<byte> buffer, ref int offset) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.Deserialize<T>(buffer, ref offset);");
            sb.AppendLine("        }");
        }

        internal void AppendTypeSpecificDeserializeMethods(StringBuilder sb)
        {
            foreach (var serializableType in _serializableTypes)
            {
                string typeName = serializableType.Name;
                sb.AppendLine($"        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"        public {typeName}? Deserialize{typeName}(ReadOnlySpan<byte> buffer, ref int offset)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return _serializer.Deserialize<{typeName}>(buffer, ref offset);");
                sb.AppendLine($"        }}");
            }
        }

        internal void AppendSizeCalculationMethods(StringBuilder sb)
        {
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.AppendLine("        public int GetSerializedSize<T>(T? obj) where T : class");
            sb.AppendLine("        {");
            sb.AppendLine("            return _serializer.GetSerializedSize(obj);");
            sb.AppendLine("        }");
            foreach (var serializableType in _serializableTypes)
            {
                string typeName = serializableType.Name;
                sb.AppendLine($"        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.AppendLine($"        public int GetSerializedSize({typeName}? obj)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return _serializer.GetSerializedSize(obj);");
                sb.AppendLine($"        }}");
            }
        }

        internal TemplatePropertyInfo BuildPropertyInfo(System.Reflection.PropertyInfo property)
        {
            var propertyInfo = new TemplatePropertyInfo
            {
                Name = property.Name,
                Type = GetFullTypeName(property.PropertyType),
                PropertyType = property.PropertyType,
                IsList = IsList(property.PropertyType),
                IsDictionary = IsDictionary(property.PropertyType),
                IsArray = property.PropertyType.IsArray,
                IsNullable = IsNullableType(property.PropertyType),
                IsYoloSerializable = IsExplicitSerializableType(property.PropertyType)
            };

            if (propertyInfo.IsList)
            {
                var elementType = GetElementType(property.PropertyType);
                propertyInfo.ElementType = elementType?.Name;
                SetListPropertyCodeAndSize(propertyInfo, property);
            }
            else if (propertyInfo.IsDictionary)
            {
                var keyType = GetDictionaryKeyType(property.PropertyType);
                var valueType = GetDictionaryValueType(property.PropertyType);
                propertyInfo.KeyType = keyType?.Name;
                propertyInfo.ValueType = valueType?.Name;
                SetDictionaryPropertyCodeAndSize(propertyInfo, property);
            }
            else if (propertyInfo.IsArray)
            {
                var elementType = property.PropertyType.GetElementType();
                propertyInfo.ElementType = elementType?.Name;
                SetArrayPropertyCodeAndSize(propertyInfo, property);
            }
            else
            {
                SetStandardPropertyCodeAndSize(propertyInfo, property);
            }

            return propertyInfo;
        }

        internal bool IsExplicitSerializableType(Type type)
        {
            return _serializableTypes.Contains(type);
        }

        internal bool IsList(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        internal bool IsDictionary(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        internal Type GetElementType(Type type)
        {
            if (IsList(type))
            {
                var genericArgs = type.GetGenericArguments();
                return genericArgs.Length >= 1 ? genericArgs[0] : null;
            }
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            return null;
        }

        internal void SetStandardPropertyCodeAndSize(TemplatePropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
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

        internal void SetListPropertyCodeAndSize(TemplatePropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
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

        internal void SetDictionaryPropertyCodeAndSize(TemplatePropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            Type unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var genericArgs = unwrappedType.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];

            string propName = property.Name;
            string instancePropRef = $"{char.ToLowerInvariant(property.DeclaringType.Name[0])}{property.DeclaringType.Name.Substring(1)}.{propName}";
            string localVarName = "_local_" + char.ToLowerInvariant(propName[0]) + propName.Substring(1);

            propertyInfo.SizeCalculation =
                $"Int32Serializer.Instance.GetSize({instancePropRef}.Count) + " +
                $"{instancePropRef}.Sum(kvp => {GetSerializerForType(keyType)}.Instance.GetSize(kvp.Key) + " +
                $"{GetSerializerForType(valueType)}.Instance.GetSize(kvp.Value))";

            propertyInfo.SerializeCode =
                $"Int32Serializer.Instance.Serialize({instancePropRef}.Count, buffer, ref offset);\n" +
                $"            foreach (var kvp in {instancePropRef})\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(keyType)}.Instance.Serialize(kvp.Key, buffer, ref offset);\n" +
                $"                {GetSerializerForType(valueType)}.Instance.Serialize(kvp.Value, buffer, ref offset);\n" +
                $"            }}";

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

        public void SetArrayPropertyCodeAndSize(TemplatePropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            Type unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var elementType = unwrappedType.GetElementType();

            string propName = property.Name;
            string instancePropRef = $"{char.ToLowerInvariant(property.DeclaringType.Name[0])}{property.DeclaringType.Name.Substring(1)}.{propName}";
            string localVarName = "_local_" + char.ToLowerInvariant(propName[0]) + propName.Substring(1);

            propertyInfo.SizeCalculation =
                $"Int32Serializer.Instance.GetSize({instancePropRef}.Length) + " +
                $"{instancePropRef}.Sum(arrayItem => {GetSerializerForType(elementType)}.Instance.GetSize(arrayItem))";

            propertyInfo.SerializeCode =
                $"Int32Serializer.Instance.Serialize({instancePropRef}.Length, buffer, ref offset);\n" +
                $"            foreach (var arrayItem in {instancePropRef})\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(elementType)}.Instance.Serialize(arrayItem, buffer, ref offset);\n" +
                $"            }}";

            propertyInfo.DeserializeCode =
                $"Int32Serializer.Instance.Deserialize(out int {localVarName}Length, buffer, ref offset);\n" +
                $"            {instancePropRef} = new {GetFullTypeName(elementType)}[{localVarName}Length];\n" +
                $"            for (int i = 0; i < {localVarName}Length; i++)\n" +
                $"            {{\n" +
                $"                {GetSerializerForType(elementType)}.Instance.Deserialize(out {GetFullTypeName(elementType)} arrayItem, buffer, ref offset);\n" +
                $"                {instancePropRef}[i] = arrayItem;\n" +
                $"            }}";
        }

        public string GetSerializerForType(Type type)
        {
            if (type.IsPrimitive || type == typeof(string))
            {
                return $"{type.Name}Serializer";
            }
            if (type.IsEnum)
            {
                return "EnumSerializer";
            }
            if (IsList(type))
            {
                return "ListSerializer";
            }
            if (IsDictionary(type))
            {
                return "DictionarySerializer";
            }
            if (type.IsArray)
            {
                return "ArraySerializer";
            }
            return $"{type.Name}Serializer";
        }

        public Type GetDictionaryKeyType(Type type)
        {
            if (!IsDictionary(type)) return null;
            var genericArgs = type.GetGenericArguments();
            return genericArgs.Length >= 1 ? genericArgs[0] : null;
        }

        public Type GetDictionaryValueType(Type type)
        {
            if (!IsDictionary(type)) return null;
            var genericArgs = type.GetGenericArguments();
            return genericArgs.Length >= 2 ? genericArgs[1] : null;
        }

        public string GetFullTypeName(Type type)
        {
            if (type == null) return "object";

            if (type.IsPrimitive || type == typeof(string))
            {
                return type.Name;
            }
            if (type.IsEnum)
            {
                return type.Name;
            }
            if (IsList(type))
            {
                var elementType = GetElementType(type);
                return $"List<{GetFullTypeName(elementType)}>";
            }
            if (IsDictionary(type))
            {
                var keyType = GetDictionaryKeyType(type);
                var valueType = GetDictionaryValueType(type);
                return $"Dictionary<{GetFullTypeName(keyType)}, {GetFullTypeName(valueType)}>";
            }
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return $"{GetFullTypeName(elementType)}[]";
            }
            return type.Name;
        }

        public bool IsReferenceType(Type type)
        {
            return !type.IsValueType;
        }

        public bool IsNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public string GetInstanceVarName(Type type)
        {
            return char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1);
        }
    }
}
