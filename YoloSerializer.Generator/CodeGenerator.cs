﻿using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YoloSerializer.Generator.Models;
using YoloSerializer.Core;
using Microsoft.VisualBasic.FileIO;

namespace YoloSerializer.Generator
{
    public class CodeGenerator
    {
        private Type[] _serializableTypes;
        private GeneratorConfig _config;

        private static class TypeHelper
        {
            public static bool IsList(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            public static bool IsDictionary(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            public static bool IsNullable(Type type) => Nullable.GetUnderlyingType(type) != null || type == typeof(string) || !type.IsValueType;
            public static bool IsReferenceType(Type type) => !type.IsValueType;
            public static Type GetElementType(Type type) => type switch
            {
                var t when IsList(t) => t.GetGenericArguments()[0],
                { IsArray: true } => type.GetElementType(),
                _ => null
            };
            public static Type GetDictionaryKeyType(Type type) => IsDictionary(type) ? type.GetGenericArguments()[0] : null;
            public static Type GetDictionaryValueType(Type type) => IsDictionary(type) ? type.GetGenericArguments()[1] : null;
            public static string GetInstanceVarName(Type type) => char.ToLowerInvariant(type.Name[0]) + type.Name.Substring(1);
            public static string GetInstanceVarName(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);
            public static string GetFullTypeName(Type type) => type switch
            {
                null => "object",
                { IsPrimitive: true } or { FullName: "System.String" } => type.Name,
                { IsEnum: true } => type.Name,
                var t when IsList(t) => $"List<{GetFullTypeName(GetElementType(t))}>",
                var t when IsDictionary(t) => $"Dictionary<{GetFullTypeName(GetDictionaryKeyType(t))}, {GetFullTypeName(GetDictionaryValueType(t))}>",
                { IsArray: true } => $"{GetFullTypeName(type.GetElementType())}[]",
                _ => type.Name
            };
            public static string GetSerializerForType(Type type) => type switch
            {
                { IsPrimitive: true } or { FullName: "System.String" } => $"{type.Name}Serializer",
                { IsEnum: true } => $"EnumSerializer<{type.Name}>",
                var t when IsList(t) => "ListSerializer",
                var t when IsDictionary(t) => "DictionarySerializer",
                { IsArray: true } => "ArraySerializer",
                _ => $"{type.Name}Serializer"
            };
        }

        private enum CollectionType
        {
            List,
            Dictionary,
            Array
        }

        private static readonly Dictionary<Type, (string Serializer, string TypeName)> PrimitiveTypeMap = new()
        {
            { typeof(int), ("Int32Serializer", "int") },
            { typeof(float), ("SingleSerializer", "float") },
            { typeof(double), ("DoubleSerializer", "double") },
            { typeof(long), ("Int64Serializer", "long") },
            { typeof(bool), ("BooleanSerializer", "bool") },
            { typeof(string), ("StringSerializer", "string") },
            { typeof(byte), ("ByteSerializer", "byte") },
            { typeof(sbyte), ("SByteSerializer", "sbyte") },
            { typeof(char), ("CharSerializer", "char") },
            { typeof(short), ("Int16Serializer", "short") },
            { typeof(ushort), ("UInt16Serializer", "ushort") },
            { typeof(uint), ("UInt32Serializer", "uint") },
            { typeof(ulong), ("UInt64Serializer", "ulong") },
            { typeof(decimal), ("DecimalSerializer", "decimal") },
            { typeof(DateTime), ("DateTimeSerializer", "DateTime") },
            { typeof(TimeSpan), ("TimeSpanSerializer", "TimeSpan") },
            { typeof(Guid), ("GuidSerializer", "Guid") }
        };

        public async Task GenerateSerializers(List<Type> serializableTypes, GeneratorConfig config)
        {

            // Parse the serializer template
            string templateContent = SerializerTemplate.GetSerializerTemplate();
            var template = Template.Parse(templateContent);

            _serializableTypes = serializableTypes.ToArray();
            _config = config;

            // Create output directories
            var outputPath = config.OutputPath;
            var serializersPath = Path.Combine(outputPath, "Serializers");
            var mapsPath = Path.Combine(outputPath, "Maps");
            var corePath = Path.Combine(outputPath, "Core");

            Directory.CreateDirectory(serializersPath);
            Directory.CreateDirectory(mapsPath);
            Directory.CreateDirectory(corePath);

            // Generate individual serializers for each type
            foreach (var type in serializableTypes)
            {
                await GenerateSerializer(type, template, serializersPath, config.ForceRegeneration);
            }

            // Generate support files
            await GenerateTypeMap(serializableTypes, mapsPath, config.ForceRegeneration);
            await GenerateYoloSerializer(corePath, config.ForceRegeneration);
            await GenerateConfiguration(serializableTypes, corePath, config.ForceRegeneration);
        }

        /// <summary>
        /// Generates serializers for types from an assembly by their type names.
        /// </summary>
        /// <param name="assemblyPath">The path to the assembly file.</param>
        /// <param name="typeNames">Array of fully qualified type names to generate serializers for.</param>
        /// <param name="config">Generator configuration options.</param>
        /// <returns>A list of types that were successfully processed.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the assembly file is not found.</exception>
        /// <exception cref="ArgumentException">Thrown when a type name cannot be found in the assembly.</exception>
        public async Task<List<Type>> GenerateSerializersFromTypeNames(string assemblyPath, string[] typeNames, GeneratorConfig config)
        {
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException($"Assembly not found at path: {assemblyPath}");
            }

            // Load the assembly from the specified path
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            
            // Find all the requested types in the assembly
            List<Type> serializableTypes = new List<Type>();
            List<string> notFoundTypes = new List<string>();
            
            foreach (string typeName in typeNames)
            {
                Type? type = assembly.GetType(typeName);
                if (type != null)
                {
                    serializableTypes.Add(type);
                }
                else
                {
                    notFoundTypes.Add(typeName);
                }
            }

            // Check if all types were found
            if (notFoundTypes.Count > 0)
            {
                throw new ArgumentException($"The following types could not be found in assembly {assembly.GetName().Name}: {string.Join(", ", notFoundTypes)}");
            }

            // Generate serializers for the found types
            await GenerateSerializers(serializableTypes, config);
            
            return serializableTypes;
        }

        /// <summary>
        /// Scans an assembly and generates serializers for types matching the specified filter.
        /// </summary>
        /// <param name="assemblyPath">The path to the assembly file.</param>
        /// <param name="filter">A predicate to determine which types to include. If null, all public non-abstract classes will be included.</param>
        /// <param name="namespaceFilter">Optional namespace filter. If provided, only types in these namespaces will be considered.</param>
        /// <param name="config">Generator configuration options.</param>
        /// <returns>A list of types that were processed.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the assembly file is not found.</exception>
        public async Task<List<Type>> ScanAssemblyForSerializableTypes(
            string assemblyPath, 
            Func<Type, bool>? filter = null, 
            string[]? namespaceFilter = null, 
            GeneratorConfig? config = null)
        {
            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException($"Assembly not found at path: {assemblyPath}");
            }

            // Use the provided config or create a default one
            config ??= new GeneratorConfig 
            { 
                OutputPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Generated"),
                ForceRegeneration = true 
            };

            // Load the assembly from the specified path
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            
            // Apply default filter if none provided (non-abstract public classes)
            filter ??= (type => type.IsClass && !type.IsAbstract && type.IsPublic);
            
            // Get all types from the assembly
            List<Type> allTypes = assembly.GetTypes().ToList();
            
            // Filter types based on namespace if filter provided
            if (namespaceFilter != null && namespaceFilter.Length > 0)
            {
                allTypes = allTypes.Where(t => 
                    t.Namespace != null && 
                    namespaceFilter.Any(ns => t.Namespace.StartsWith(ns)))
                    .ToList();
            }
            
            // Apply the type filter
            List<Type> serializableTypes = allTypes.Where(filter).ToList();
            
            if (serializableTypes.Count == 0)
            {
                Console.WriteLine($"Warning: No serializable types found in assembly {assembly.GetName().Name}");
                return serializableTypes;
            }
            
            Console.WriteLine($"Found {serializableTypes.Count} serializable types in assembly {assembly.GetName().Name}");
            
            // Generate serializers for the found types
            await GenerateSerializers(serializableTypes, config);
            
            return serializableTypes;
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

            // Collect all unique namespaces for property types
            var typeNamespaces = new HashSet<string>();
            typeNamespaces.Add(type.Namespace); // Add the namespace of the main type

            foreach (var prop in properties)
            {
                var propType = prop.PropertyType;

                // Add namespace for the property type
                if (propType.Namespace != null && propType.Namespace != type.Namespace)
                    typeNamespaces.Add(propType.Namespace);

                // For collections, add namespace for element types
                if (prop.IsList || prop.IsArray)
                {
                    var elementType = TypeHelper.GetElementType(propType);
                    if (elementType?.Namespace != null && elementType.Namespace != type.Namespace)
                        typeNamespaces.Add(elementType.Namespace);
                }
                else if (prop.IsDictionary)
                {
                    var keyType = TypeHelper.GetDictionaryKeyType(propType);
                    if (keyType?.Namespace != null && keyType.Namespace != type.Namespace)
                        typeNamespaces.Add(keyType.Namespace);

                    var valueType = TypeHelper.GetDictionaryValueType(propType);
                    if (valueType?.Namespace != null && valueType.Namespace != type.Namespace)
                        typeNamespaces.Add(valueType.Namespace);
                }
            }

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

            // Add namespaces to the template context
            templateContext.TypeNamespaces = typeNamespaces.ToList();

            // Parse and render the template
            var result = await template.RenderAsync(templateContext);

            // Write the generated code to a file
            await File.WriteAllTextAsync(outputFile, result);

            Console.WriteLine($"Generated {outputFile}");
        }

        internal List<TemplatePropertyInfo> GetTypeProperties(Type type)
        {

            // Reset all nullableFieldIndex to -1 for non-nullable fields
            int nullableFieldCount = 0;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => BuildPropertyInfo(p, ref nullableFieldCount))
                .ToList();

            // Get public fields as well
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Select(f => BuildFieldInfo(f, ref nullableFieldCount))
                .ToList();


            foreach(var prop in properties)
            {
                var unwrappedType = Nullable.GetUnderlyingType(prop.Property.PropertyType) ?? prop.Property.PropertyType;
                BuildPropertySerializationCode(prop.Property, prop, unwrappedType);
            }

            foreach (var f in fields)
            {
                var unwrappedType = Nullable.GetUnderlyingType(f.Field.FieldType) ?? f.Field.FieldType;
                BuildFieldSerializationCode(f, f.Field, unwrappedType);
            }

            // Combine properties and fields
            properties.AddRange(fields);

            return properties;
        }

        internal TemplatePropertyInfo BuildFieldInfo(System.Reflection.FieldInfo field, ref int nullableFieldCount)
        {
            var fieldType = field.FieldType;
            var unwrappedType = Nullable.GetUnderlyingType(fieldType) ?? fieldType;

            var fieldInfo = new TemplatePropertyInfo
            {
                Field = field,
                Name = field.Name,
                Type = TypeHelper.GetFullTypeName(fieldType),
                PropertyType = fieldType,
                IsList = TypeHelper.IsList(unwrappedType),
                IsDictionary = TypeHelper.IsDictionary(unwrappedType),
                IsArray = unwrappedType.IsArray,
                IsNullable = TypeHelper.IsNullable(fieldType) || (fieldType == typeof(string)),
                IsYoloSerializable = IsExplicitSerializableType(unwrappedType)
            };

            if (fieldInfo.IsNullable)
            {
                fieldInfo.NullableFieldIndex = ++nullableFieldCount;
            } else
            {
                fieldInfo.NullableFieldIndex = -1;
            }

           

            return fieldInfo;
        }

        private void BuildFieldSerializationCode(TemplatePropertyInfo fieldInfo, FieldInfo field, Type? unwrappedType)
        {
            if (fieldInfo.IsList)
            {
                fieldInfo.ElementType = TypeHelper.GetElementType(unwrappedType)?.Name;
                SetCollectionFieldCodeAndSize(fieldInfo, field, CollectionType.List);
            }
            else if (fieldInfo.IsDictionary)
            {
                fieldInfo.KeyType = TypeHelper.GetDictionaryKeyType(unwrappedType)?.Name;
                fieldInfo.ValueType = TypeHelper.GetDictionaryValueType(unwrappedType)?.Name;
                SetCollectionFieldCodeAndSize(fieldInfo, field, CollectionType.Dictionary);
            }
            else if (fieldInfo.IsArray)
            {
                fieldInfo.ElementType = unwrappedType.GetElementType()?.Name;
                SetCollectionFieldCodeAndSize(fieldInfo, field, CollectionType.Array);
            }
            else
            {
                SetStandardFieldCodeAndSize(fieldInfo, field);
            }
        }

        private void SetCollectionFieldCodeAndSize(TemplatePropertyInfo fieldInfo, FieldInfo field, CollectionType type)
        {
            var fieldType = field.FieldType;
            var isNullable = TypeHelper.IsNullable(fieldType) || Nullable.GetUnderlyingType(fieldType) != null;

            var elementType = type switch
            {
                CollectionType.List => TypeHelper.GetElementType(field.FieldType),
                CollectionType.Dictionary => TypeHelper.GetDictionaryValueType(field.FieldType),
                CollectionType.Array => field.FieldType.GetElementType(),
                _ => throw new ArgumentException("Invalid collection type")
            };

            var keyType = type == CollectionType.Dictionary ? TypeHelper.GetDictionaryKeyType(field.FieldType) : null;

            var fieldName = field.Name;
            var instanceFieldRef = $"{TypeHelper.GetInstanceVarName(field.DeclaringType.Name)}.{fieldName}";
            var localVarName = "_local_" + TypeHelper.GetInstanceVarName(fieldName);

            // Get declaring type to check if we need to use fully qualified names
            Type declaringType = field.DeclaringType;

            // Use fully qualified names if the types are in different namespaces
            string elementTypeName = TypeHelper.GetFullTypeName(elementType);
            string fullElementTypeName = elementTypeName;
            if (declaringType != null && elementType != null && elementType.Namespace != declaringType.Namespace)
            {
                fullElementTypeName = $"{elementType.Namespace}.{elementTypeName}";
            }

            string keyTypeName = keyType != null ? TypeHelper.GetFullTypeName(keyType) : null;
            string fullKeyTypeName = keyTypeName;
            if (declaringType != null && keyType != null && keyType.Namespace != declaringType.Namespace)
            {
                fullKeyTypeName = $"{keyType.Namespace}.{keyTypeName}";
            }

            // Set IsNullable flag
            fieldInfo.IsNullable = isNullable;

            // Generate size calculation
            string sizingCode = GenerateCollectionSizeCalculation(instanceFieldRef, elementType, keyType, type);
            if (isNullable)
            {
                fieldInfo.SizeCalculation = $"{instanceFieldRef} == null ? 0 : {sizingCode}";
            }
            else
            {
                fieldInfo.SizeCalculation = sizingCode;
            }

            // Generate serialization code
            string serializationCode = GenerateCollectionSerializationCode(instanceFieldRef, elementType, keyType, type);
            if (isNullable)
            {
                fieldInfo.SerializeCode = $"if (!NullableBitset.IsNull(bitset, {fieldInfo.NullableFieldIndex}))\n" +
                                          $"            {{\n" +
                                          $"                {serializationCode}\n" +
                                          $"            }}";
            }
            else
            {
                fieldInfo.SerializeCode = serializationCode;
            }

            // Generate deserialization code
            string baseDeserCode = type switch
            {
                CollectionType.List => $"Int32Serializer.Instance.Deserialize(out int {localVarName}Count, buffer, ref offset);\n" +
                                     $"            {instanceFieldRef}.Clear();\n" +
                                     $"            for (int i = 0; i < {localVarName}Count; i++)\n" +
                                     $"            {{\n" +
                                     $"                {TypeHelper.GetSerializerForType(elementType)}.Instance.Deserialize(out {fullElementTypeName} listItem, buffer, ref offset);\n" +
                                     $"                {instanceFieldRef}.Add(listItem);\n" +
                                     $"            }}",
                CollectionType.Dictionary => $"Int32Serializer.Instance.Deserialize(out int {localVarName}Count, buffer, ref offset);\n" +
                                   $"            {instanceFieldRef}.Clear();\n" +
                                   $"            for (int i = 0; i < {localVarName}Count; i++)\n" +
                                   $"            {{\n" +
                                   $"                {TypeHelper.GetSerializerForType(keyType)}.Instance.Deserialize(out {fullKeyTypeName} key, buffer, ref offset);\n" +
                                   $"                {TypeHelper.GetSerializerForType(elementType)}.Instance.Deserialize(out {fullElementTypeName} dictValue, buffer, ref offset);\n" +
                                   $"                {instanceFieldRef}[key] = dictValue;\n" +
                                   $"            }}",
                CollectionType.Array => $"Int32Serializer.Instance.Deserialize(out int {localVarName}Length, buffer, ref offset);\n" +
                              $"            {instanceFieldRef} = new {fullElementTypeName}[{localVarName}Length];\n" +
                              $"            for (int i = 0; i < {localVarName}Length; i++)\n" +
                              $"            {{\n" +
                              $"                {TypeHelper.GetSerializerForType(elementType)}.Instance.Deserialize(out {fullElementTypeName} arrayItem, buffer, ref offset);\n" +
                              $"                {instanceFieldRef}[i] = arrayItem;\n" +
                              $"            }}",
                _ => throw new ArgumentException("Invalid collection type")
            };

            if (isNullable)
            {
                fieldInfo.DeserializeCode = $"if (NullableBitset.IsNull(bitset, {fieldInfo.NullableFieldIndex}))\n" +
                                            $"                {instanceFieldRef} = null;\n" +
                                            $"            else\n" +
                                            $"            {{\n" +
                                            $"                {baseDeserCode}\n" +
                                            $"            }}";
            }
            else
            {
                fieldInfo.DeserializeCode = baseDeserCode;
            }
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

            var nullableFieldCount = properties.Count(p => p.IsNullable);

            // Calculate nullability bitset size in bytes (if any nullable fields exist)
            int nullableBitsetSize = NullableBitset.GetBitsetSize(nullableFieldCount);

            // Generate nullability check code
            var nullabilityCheckCode = CreateNullabilityCheckCode(properties, instanceVarName);

            var templateContext = new SerializerTemplateModel
            {
                ClassName = className,
                FullTypeName = type.FullName,
                SerializerName = serializerName,
                InstanceVarName = instanceVarName,
                IsClass = isClass,
                NeedsNullCheck = needsNullCheck,
                NullableMarker = nullableMarker,
                SizeCalculation = sizeCalculation,
                SerializeCode = serializeCode,
                DeserializeCode = deserializeCode,
                Namespace = _config.GeneratedNamespace,
                TypeNamespace = type.Namespace,
                NullableFieldCount = nullableFieldCount,
                NullableBitsetSize = nullableBitsetSize,
                NullabilityCheckCode = nullabilityCheckCode,
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
                    DeserializeCode = p.DeserializeCode,
                    NullableFieldIndex = p.NullableFieldIndex
                }).ToList()
            };

            return templateContext;
        }

        internal string CreateNullabilityCheckCode(List<TemplatePropertyInfo> properties, string instanceName)
        {
            var sb = new StringBuilder();

            foreach (var prop in properties)
            {
                if (prop.IsNullable)
                {
                    string instancePropRef = $"{instanceName}.{prop.Name}";
                    sb.AppendLine($"            NullableBitset.SetBit(bitset, {prop.NullableFieldIndex}, {instancePropRef} == null);");
                }
            }

            return sb.ToString();
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
            sb.AppendLine("using YoloSerializer.Core.Serializers;");
            sb.AppendLine($"using {_config.GeneratedNamespace};");

            // Add imports for each type's namespace
            var namespaces = _serializableTypes.Select(t => t.Namespace).Distinct();
            foreach (var ns in namespaces)
            {
                if (!string.IsNullOrEmpty(ns))
                {
                    sb.AppendLine($"using {ns};");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {_config.MapsNamespace}");
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
            sb.AppendLine("using YoloSerializer.Core.Serializers;");
            sb.AppendLine("using YoloSerializer.Core.Contracts;");
            sb.AppendLine($"using {_config.MapsNamespace};");

            // Add imports for each type's namespace
            var namespaces = _serializableTypes.Select(t => t.Namespace).Distinct();
            foreach (var ns in namespaces)
            {
                if (!string.IsNullOrEmpty(ns))
                {
                    sb.AppendLine($"using {ns};");
                }
            }

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

        internal TemplatePropertyInfo BuildPropertyInfo(System.Reflection.PropertyInfo property, ref int nullableFieldCount)
        {
            var propertyType = property.PropertyType;
            var unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var propertyInfo = new TemplatePropertyInfo
            {
                Property = property,
                Name = property.Name,
                Type = TypeHelper.GetFullTypeName(propertyType),
                PropertyType = propertyType,
                IsList = TypeHelper.IsList(unwrappedType),
                IsDictionary = TypeHelper.IsDictionary(unwrappedType),
                IsArray = unwrappedType.IsArray,
                IsNullable = TypeHelper.IsNullable(propertyType) || (propertyType == typeof(string)),
                IsYoloSerializable = IsExplicitSerializableType(unwrappedType)
            };

            if (propertyInfo.IsNullable)
            {
                propertyInfo.NullableFieldIndex = ++nullableFieldCount;
            }
            else
            {
                propertyInfo.NullableFieldIndex = -1;
            }

           
            return propertyInfo;
        }

        private void BuildPropertySerializationCode(PropertyInfo property, TemplatePropertyInfo propertyInfo, Type? unwrappedType)
        {
            if (propertyInfo.IsList)
            {
                propertyInfo.ElementType = TypeHelper.GetElementType(unwrappedType)?.Name;
                SetCollectionPropertyCodeAndSize(propertyInfo, property, CollectionType.List);
            }
            else if (propertyInfo.IsDictionary)
            {
                propertyInfo.KeyType = TypeHelper.GetDictionaryKeyType(unwrappedType)?.Name;
                propertyInfo.ValueType = TypeHelper.GetDictionaryValueType(unwrappedType)?.Name;
                SetCollectionPropertyCodeAndSize(propertyInfo, property, CollectionType.Dictionary);
            }
            else if (propertyInfo.IsArray)
            {
                propertyInfo.ElementType = unwrappedType.GetElementType()?.Name;
                SetCollectionPropertyCodeAndSize(propertyInfo, property, CollectionType.Array);
            }
            else
            {
                SetStandardPropertyCodeAndSize(propertyInfo, property);
            }


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
            var propertyType = property.PropertyType;
            var unwrappedType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            var isNullable = propertyInfo.IsNullable;
            var nullableMarker = isNullable ? "?" : "";
            var propName = property.Name;
            var instancePropRef = $"{TypeHelper.GetInstanceVarName(property.DeclaringType.Name)}.{propName}";
            var localVarName = "_local_" + TypeHelper.GetInstanceVarName(propName);

            // Set IsNullable flag
            propertyInfo.IsNullable = isNullable;

            if (PrimitiveTypeMap.TryGetValue(unwrappedType, out var typeInfo))
            {
                // Update sizing calculation to handle nulls
                if (isNullable)
                {
                    propertyInfo.SizeCalculation = $"{instancePropRef} == null ? 0 : {typeInfo.Serializer}.Instance.GetSize({instancePropRef})";
                }
                else
                {
                    propertyInfo.SizeCalculation = $"{typeInfo.Serializer}.Instance.GetSize({instancePropRef})";
                }

                // Update serialization code to use bitset
                if (isNullable)
                {
                    propertyInfo.SerializeCode = $"if (!NullableBitset.IsNull(bitset, {propertyInfo.NullableFieldIndex}))\n" +
                                               $"                {typeInfo.Serializer}.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                }
                else
                {
                    propertyInfo.SerializeCode = $"{typeInfo.Serializer}.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                }

                // Update deserialization code to use bitset
                if (isNullable)
                {
                    propertyInfo.DeserializeCode = $"if (NullableBitset.IsNull(bitset, {propertyInfo.NullableFieldIndex}))\n" +
                                                 $"                {instancePropRef} = null;\n" +
                                                 $"            else\n" +
                                                 $"            {{\n" +
                                                 $"                {typeInfo.Serializer}.Instance.Deserialize(out {typeInfo.TypeName} {localVarName}, buffer, ref offset);\n" +
                                                 $"                {instancePropRef} = {localVarName};\n" +
                                                 $"            }}";
                }
                else
                {
                    propertyInfo.DeserializeCode = $"{typeInfo.Serializer}.Instance.Deserialize(out {typeInfo.TypeName} {localVarName}, buffer, ref offset);\n" +
                                                  $"            {instancePropRef} = {localVarName};";
                }
            }
            else if (unwrappedType.IsEnum)
            {
                var fullTypeName = TypeHelper.GetFullTypeName(unwrappedType);
                var fullTypeWithNamespace = unwrappedType.Namespace != property.DeclaringType.Namespace ?
                    $"{unwrappedType.Namespace}.{fullTypeName}" : fullTypeName;

                // Update sizing calculation to handle nulls
                if (isNullable)
                {
                    propertyInfo.SizeCalculation = $"{instancePropRef} == null ? 0 : EnumSerializer<{fullTypeWithNamespace}>.Instance.GetSize({instancePropRef})";
                }
                else
                {
                    propertyInfo.SizeCalculation = $"EnumSerializer<{fullTypeWithNamespace}>.Instance.GetSize({instancePropRef})";
                }

                // Update serialization code to use bitset
                if (isNullable)
                {
                    propertyInfo.SerializeCode = $"if (!NullableBitset.IsNull(bitset, {propertyInfo.NullableFieldIndex}))\n" +
                                               $"                EnumSerializer<{fullTypeWithNamespace}>.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                }
                else
                {
                    propertyInfo.SerializeCode = $"EnumSerializer<{fullTypeWithNamespace}>.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                }

                // Update deserialization code to use bitset
                if (isNullable)
                {
                    propertyInfo.DeserializeCode = $"if (NullableBitset.IsNull(bitset, {propertyInfo.NullableFieldIndex}))\n" +
                                                 $"                {instancePropRef} = null;\n" +
                                                 $"            else\n" +
                                                 $"            {{\n" +
                                                 $"                EnumSerializer<{fullTypeWithNamespace}>.Instance.Deserialize(out {fullTypeWithNamespace} {localVarName}, buffer, ref offset);\n" +
                                                 $"                {instancePropRef} = {localVarName};\n" +
                                                 $"            }}";
                }
                else
                {
                    propertyInfo.DeserializeCode = $"EnumSerializer<{fullTypeWithNamespace}>.Instance.Deserialize(out {fullTypeWithNamespace} {localVarName}, buffer, ref offset);\n" +
                                                  $"            {instancePropRef} = {localVarName};";
                }
            }
            else if (propertyInfo.IsYoloSerializable)
            {
                var serializerName = $"{unwrappedType.Name}Serializer";
                var fullTypeName = TypeHelper.GetFullTypeName(unwrappedType);
                var fullTypeWithNamespace = unwrappedType.Namespace != property.DeclaringType.Namespace ?
                    $"{unwrappedType.Namespace}.{fullTypeName}" : fullTypeName;

                // Update sizing calculation to handle nulls
                if (isNullable)
                {
                    propertyInfo.SizeCalculation = $"{instancePropRef} == null ? 0 : {serializerName}.Instance.GetSize({instancePropRef})";
                }
                else
                {
                    propertyInfo.SizeCalculation = $"{serializerName}.Instance.GetSize({instancePropRef})";
                }

                // Update serialization code to use bitset
                if (isNullable)
                {
                    propertyInfo.SerializeCode = $"if (!NullableBitset.IsNull(bitset, {propertyInfo.NullableFieldIndex}))\n" +
                                               $"                {serializerName}.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                }
                else
                {
                    propertyInfo.SerializeCode = $"{serializerName}.Instance.Serialize({instancePropRef}, buffer, ref offset);";
                }

                // Update deserialization code to use bitset
                if (isNullable)
                {
                    propertyInfo.DeserializeCode = $"if (NullableBitset.IsNull(bitset, {propertyInfo.NullableFieldIndex}))\n" +
                                                 $"                {instancePropRef} = null;\n" +
                                                 $"            else\n" +
                                                 $"            {{\n" +
                                                 $"                {serializerName}.Instance.Deserialize(out {fullTypeWithNamespace}{nullableMarker} {localVarName}, buffer, ref offset);\n" +
                                                 $"                {instancePropRef} = {localVarName};\n" +
                                                 $"            }}";
                }
                else
                {
                    propertyInfo.DeserializeCode = $"{serializerName}.Instance.Deserialize(out {fullTypeWithNamespace}{nullableMarker} {localVarName}, buffer, ref offset);\n" +
                                                  $"            {instancePropRef} = {localVarName};";
                }
            }
            else
            {
                throw new NotSupportedException($"No serializer available for type {unwrappedType.FullName}");
            }
        }

        private void SetCollectionPropertyCodeAndSize(TemplatePropertyInfo propertyInfo, PropertyInfo property, CollectionType type)
        {
            var propertyType = property.PropertyType;
            var isNullable = !propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null;

            var elementType = type switch
            {
                CollectionType.List => TypeHelper.GetElementType(property.PropertyType),
                CollectionType.Dictionary => TypeHelper.GetDictionaryValueType(property.PropertyType),
                CollectionType.Array => property.PropertyType.GetElementType(),
                _ => throw new ArgumentException("Invalid collection type")
            };

            var keyType = type == CollectionType.Dictionary ? TypeHelper.GetDictionaryKeyType(property.PropertyType) : null;

            var propName = property.Name;
            var instancePropRef = $"{TypeHelper.GetInstanceVarName(property.DeclaringType.Name)}.{propName}";
            var localVarName = "_local_" + TypeHelper.GetInstanceVarName(propName);

            // Set IsNullable flag
            propertyInfo.IsNullable = isNullable;

            // Update sizing calculation to handle nulls
            string sizingCode;
            if (isNullable)
            {
                // For nullable collections, we only include size if non-null (bitset handles checking null state)
                sizingCode = GenerateCollectionSizeCalculation(instancePropRef, elementType, keyType, type);
                propertyInfo.SizeCalculation = $"{instancePropRef} == null ? 0 : {sizingCode}";
            }
            else
            {
                sizingCode = GenerateCollectionSizeCalculation(instancePropRef, elementType, keyType, type);
                propertyInfo.SizeCalculation = sizingCode;
            }

            // Update serialization code
            string serializationCode = GenerateCollectionSerializationCode(instancePropRef, elementType, keyType, type);

            if (isNullable)
            {
                propertyInfo.SerializeCode = $"if (!NullableBitset.IsNull(bitset, {propertyInfo.NullableFieldIndex}))\n" +
                                           $"            {{\n" +
                                           $"                {serializationCode}\n" +
                                           $"            }}";
            }
            else
            {
                propertyInfo.SerializeCode = serializationCode;
            }

            // Update deserialization code
            string deserCode = GenerateCollectionDeserializationCode(instancePropRef, elementType, keyType, localVarName, type);

            if (isNullable)
            {
                propertyInfo.DeserializeCode = $"if (NullableBitset.IsNull(bitset, {propertyInfo.NullableFieldIndex}))\n" +
                                              $"                {instancePropRef} = null;\n" +
                                              $"            else\n" +
                                              $"            {{\n" +
                                              $"                {deserCode}\n" +
                                              $"            }}";
            }
            else
            {
                propertyInfo.DeserializeCode = deserCode;
            }
        }

        private string GenerateCollectionSizeCalculation(string instancePropRef, Type elementType, Type keyType, CollectionType type)
        {
            string elementSerializer;
            string keySerializer = null;

            // Check if the element type is in the primitive type map
            if (PrimitiveTypeMap.TryGetValue(elementType, out var elementTypeInfo))
            {
                elementSerializer = elementTypeInfo.Serializer;
            }
            else
            {
                elementSerializer = TypeHelper.GetSerializerForType(elementType);
            }

            // For dictionaries, also check if the key type is in the primitive type map
            if (keyType != null)
            {
                if (PrimitiveTypeMap.TryGetValue(keyType, out var keyTypeInfo))
                {
                    keySerializer = keyTypeInfo.Serializer;
                }
                else
                {
                    keySerializer = TypeHelper.GetSerializerForType(keyType);
                }
            }

            return type switch
            {
                CollectionType.List => $"({instancePropRef} == null ? sizeof(int) : " +
                                     $"Int32Serializer.Instance.GetSize({instancePropRef}.Count) + " +
                                     $"{instancePropRef}.Sum(listItem => {elementSerializer}.Instance.GetSize(listItem)))",

                CollectionType.Dictionary => $"({instancePropRef} == null ? sizeof(int) : " +
                                          $"Int32Serializer.Instance.GetSize({instancePropRef}.Count) + " +
                                          $"{instancePropRef}.Sum(kvp => {keySerializer}.Instance.GetSize(kvp.Key) + " +
                                          $"{elementSerializer}.Instance.GetSize(kvp.Value)))",

                CollectionType.Array => $"({instancePropRef} == null ? sizeof(int) : " +
                                      $"Int32Serializer.Instance.GetSize({instancePropRef}.Length) + " +
                                      $"{instancePropRef}.Sum(arrayItem => {elementSerializer}.Instance.GetSize(arrayItem)))",

                _ => throw new ArgumentException("Invalid collection type")
            };
        }

        private string GenerateCollectionSerializationCode(string instancePropRef, Type elementType, Type keyType, CollectionType type)
        {
            string elementSerializer;
            string keySerializer = null;

            // Check if the element type is in the primitive type map
            if (PrimitiveTypeMap.TryGetValue(elementType, out var elementTypeInfo))
            {
                elementSerializer = elementTypeInfo.Serializer;
            }
            else
            {
                elementSerializer = TypeHelper.GetSerializerForType(elementType);
            }

            // For dictionaries, also check if the key type is in the primitive type map
            if (keyType != null)
            {
                if (PrimitiveTypeMap.TryGetValue(keyType, out var keyTypeInfo))
                {
                    keySerializer = keyTypeInfo.Serializer;
                }
                else
                {
                    keySerializer = TypeHelper.GetSerializerForType(keyType);
                }
            }

            return type switch
            {
                CollectionType.List => $"if ({instancePropRef} == null) {{\n" +
                                     $"                Int32Serializer.Instance.Serialize(0, buffer, ref offset);\n" +
                                     $"            }} else {{\n" +
                                     $"                Int32Serializer.Instance.Serialize({instancePropRef}.Count, buffer, ref offset);\n" +
                                     $"                foreach (var listItem in {instancePropRef})\n" +
                                     $"                {{\n" +
                                     $"                    {elementSerializer}.Instance.Serialize(listItem, buffer, ref offset);\n" +
                                     $"                }}\n" +
                                     $"            }}",

                CollectionType.Dictionary => $"if ({instancePropRef} == null) {{\n" +
                                          $"                Int32Serializer.Instance.Serialize(0, buffer, ref offset);\n" +
                                          $"            }} else {{\n" +
                                          $"                Int32Serializer.Instance.Serialize({instancePropRef}.Count, buffer, ref offset);\n" +
                                          $"                foreach (var kvp in {instancePropRef})\n" +
                                          $"                {{\n" +
                                          $"                    {keySerializer}.Instance.Serialize(kvp.Key, buffer, ref offset);\n" +
                                          $"                    {elementSerializer}.Instance.Serialize(kvp.Value, buffer, ref offset);\n" +
                                          $"                }}\n" +
                                          $"            }}",

                CollectionType.Array => $"if ({instancePropRef} == null) {{\n" +
                                      $"                Int32Serializer.Instance.Serialize(0, buffer, ref offset);\n" +
                                      $"            }} else {{\n" +
                                      $"                Int32Serializer.Instance.Serialize({instancePropRef}.Length, buffer, ref offset);\n" +
                                      $"                foreach (var arrayItem in {instancePropRef})\n" +
                                      $"                {{\n" +
                                      $"                    {elementSerializer}.Instance.Serialize(arrayItem, buffer, ref offset);\n" +
                                      $"                }}\n" +
                                      $"            }}",

                _ => throw new ArgumentException("Invalid collection type")
            };
        }

        private string GenerateCollectionDeserializationCode(string instancePropRef, Type elementType, Type keyType, string localVarName, CollectionType type)
        {
            var elementSerializer = TypeHelper.GetSerializerForType(elementType);
            var keySerializer = keyType != null ? TypeHelper.GetSerializerForType(keyType) : null;
            var elementTypeName = TypeHelper.GetFullTypeName(elementType);
            var keyTypeName = keyType != null ? TypeHelper.GetFullTypeName(keyType) : null;

            // Get declaring type to check if we need to use fully qualified names
            Type declaringType = null;
            if (instancePropRef.Contains('.'))
            {
                var parts = instancePropRef.Split('.');
                var instanceName = parts[0];
                // Find the declaring type from the serializable types
                declaringType = _serializableTypes.FirstOrDefault(t =>
                    TypeHelper.GetInstanceVarName(t.Name) == instanceName);
            }

            // Use fully qualified names if the types are in different namespaces
            string fullElementTypeName = elementTypeName;
            if (declaringType != null && elementType.Namespace != declaringType.Namespace)
            {
                fullElementTypeName = $"{elementType.Namespace}.{elementTypeName}";
            }

            string fullKeyTypeName = keyTypeName;
            if (declaringType != null && keyType != null && keyType.Namespace != declaringType.Namespace)
            {
                fullKeyTypeName = $"{keyType.Namespace}.{keyTypeName}";
            }

            return type switch
            {
                CollectionType.List => $"Int32Serializer.Instance.Deserialize(out int {localVarName}Count, buffer, ref offset);\n" +
                                     $"            if ({instancePropRef} == null)\n" +
                                     $"                {instancePropRef} = new List<{fullElementTypeName}>();\n" +
                                     $"            else\n" +
                                     $"                {instancePropRef}.Clear();\n" +
                                     $"            for (int i = 0; i < {localVarName}Count; i++)\n" +
                                     $"            {{\n" +
                                     $"                {elementSerializer}.Instance.Deserialize(out {fullElementTypeName} listItem, buffer, ref offset);\n" +
                                     $"                {instancePropRef}.Add(listItem);\n" +
                                     $"            }}",
                CollectionType.Dictionary => $"Int32Serializer.Instance.Deserialize(out int {localVarName}Count, buffer, ref offset);\n" +
                                           $"            if ({instancePropRef} == null)\n" +
                                           $"                {instancePropRef} = new Dictionary<{fullKeyTypeName}, {fullElementTypeName}>();\n" +
                                           $"            else\n" +
                                           $"                {instancePropRef}.Clear();\n" +
                                           $"            for (int i = 0; i < {localVarName}Count; i++)\n" +
                                           $"            {{\n" +
                                           $"                {keySerializer}.Instance.Deserialize(out {fullKeyTypeName} key, buffer, ref offset);\n" +
                                           $"                {elementSerializer}.Instance.Deserialize(out {fullElementTypeName} dictValue, buffer, ref offset);\n" +
                                           $"                {instancePropRef}[key] = dictValue;\n" +
                                           $"            }}",
                CollectionType.Array => $"Int32Serializer.Instance.Deserialize(out int {localVarName}Length, buffer, ref offset);\n" +
                                      $"            {instancePropRef} = new {fullElementTypeName}[{localVarName}Length];\n" +
                                      $"            for (int i = 0; i < {localVarName}Length; i++)\n" +
                                      $"            {{\n" +
                                      $"                {elementSerializer}.Instance.Deserialize(out {fullElementTypeName} arrayItem, buffer, ref offset);\n" +
                                      $"                {instancePropRef}[i] = arrayItem;\n" +
                                      $"            }}",
                _ => throw new ArgumentException("Invalid collection type")
            };
        }

        internal void SetListPropertyCodeAndSize(TemplatePropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            SetCollectionPropertyCodeAndSize(propertyInfo, property, CollectionType.List);
        }

        internal void SetDictionaryPropertyCodeAndSize(TemplatePropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            SetCollectionPropertyCodeAndSize(propertyInfo, property, CollectionType.Dictionary);
        }

        public void SetArrayPropertyCodeAndSize(TemplatePropertyInfo propertyInfo, System.Reflection.PropertyInfo property)
        {
            SetCollectionPropertyCodeAndSize(propertyInfo, property, CollectionType.Array);
        }

        public string GetSerializerForType(Type type) => type switch
        {
            { IsPrimitive: true } or { FullName: "System.String" } => $"{type.Name}Serializer",
            { IsEnum: true } => "EnumSerializer<TEnum>",
            var t when IsList(t) => "ListSerializer",
            var t when IsDictionary(t) => "DictionarySerializer",
            { IsArray: true } => "ArraySerializer",
            _ => $"{type.Name}Serializer"
        };

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

        public string GetFullTypeName(Type type) => type switch
        {
            null => "object",
            { IsPrimitive: true } or { FullName: "System.String" } => type.Name,
            { IsEnum: true } => type.Name,
            var t when IsList(t) => $"List<{GetFullTypeName(GetElementType(t))}>",
            var t when IsDictionary(t) => $"Dictionary<{GetFullTypeName(GetDictionaryKeyType(t))}, {GetFullTypeName(GetDictionaryValueType(t))}>",
            { IsArray: true } => $"{GetFullTypeName(type.GetElementType())}[]",
            _ => type.Name
        };

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

        internal async Task GenerateConfiguration(List<Type> serializableTypes, string outputPath, bool forceRegeneration)
        {
            string outputFile = Path.Combine(outputPath, "YoloGeneratedConfig.cs");
            if (File.Exists(outputFile) && !forceRegeneration)
            {
                Console.WriteLine("Skipping YoloGeneratedConfig.cs - file already exists (use --force to override)");
                return;
            }
            Console.WriteLine("Generating YoloGeneratedConfig.cs");

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");

            // Add imports for each type's namespace
            var namespaces = serializableTypes.Select(t => t.Namespace).Distinct();
            foreach (var ns in namespaces)
            {
                if (!string.IsNullOrEmpty(ns))
                {
                    sb.AppendLine($"using {ns};");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {_config.CoreNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class YoloGeneratedConfig");
            sb.AppendLine("    {");
            sb.AppendLine("        public static readonly HashSet<Type> SerializableTypes = new()");
            sb.AppendLine("        {");

            foreach (var type in serializableTypes)
            {
                sb.AppendLine($"            typeof({type.Name}),");
            }

            sb.AppendLine("        };");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            await File.WriteAllTextAsync(outputFile, sb.ToString());
            Console.WriteLine($"Generated {outputFile}");
        }

        internal void SetStandardFieldCodeAndSize(TemplatePropertyInfo fieldInfo, System.Reflection.FieldInfo field)
        {
            var fieldType = field.FieldType;
            var unwrappedType = Nullable.GetUnderlyingType(fieldType) ?? fieldType;
            var isNullable = fieldType != unwrappedType || !fieldType.IsValueType || fieldType == typeof(string);
            var nullableMarker = isNullable ? "?" : "";
            var fieldName = field.Name;
            var instanceFieldRef = $"{TypeHelper.GetInstanceVarName(field.DeclaringType.Name)}.{fieldName}";
            var localVarName = "_local_" + TypeHelper.GetInstanceVarName(fieldName);

            // Set IsNullable flag
            fieldInfo.IsNullable = isNullable;

            if (PrimitiveTypeMap.TryGetValue(unwrappedType, out var typeInfo))
            {
                // Update sizing calculation to handle nulls
                if (isNullable)
                {
                    fieldInfo.SizeCalculation = $"{instanceFieldRef} == null ? 0 : {typeInfo.Serializer}.Instance.GetSize({instanceFieldRef})";
                }
                else
                {
                    fieldInfo.SizeCalculation = $"{typeInfo.Serializer}.Instance.GetSize({instanceFieldRef})";
                }

                // Update serialization code to use bitset
                if (isNullable)
                {
                    fieldInfo.SerializeCode = $"if (!NullableBitset.IsNull(bitset, {fieldInfo.NullableFieldIndex}))\n" +
                                             $"                {typeInfo.Serializer}.Instance.Serialize({instanceFieldRef}, buffer, ref offset);";
                }
                else
                {
                    fieldInfo.SerializeCode = $"{typeInfo.Serializer}.Instance.Serialize({instanceFieldRef}, buffer, ref offset);";
                }

                // Update deserialization code to use bitset
                if (isNullable)
                {
                    fieldInfo.DeserializeCode = $"if (NullableBitset.IsNull(bitset, {fieldInfo.NullableFieldIndex}))\n" +
                                               $"                {instanceFieldRef} = null;\n" +
                                               $"            else\n" +
                                               $"            {{\n" +
                                               $"                {typeInfo.Serializer}.Instance.Deserialize(out {typeInfo.TypeName} {localVarName}, buffer, ref offset);\n" +
                                               $"                {instanceFieldRef} = {localVarName};\n" +
                                               $"            }}";
                }
                else
                {
                    fieldInfo.DeserializeCode = $"{typeInfo.Serializer}.Instance.Deserialize(out {typeInfo.TypeName} {localVarName}, buffer, ref offset);\n" +
                                               $"            {instanceFieldRef} = {localVarName};";
                }
            }
            else if (unwrappedType.IsEnum)
            {
                var fullTypeName = TypeHelper.GetFullTypeName(unwrappedType);
                var fullTypeWithNamespace = unwrappedType.Namespace != field.DeclaringType.Namespace ?
                    $"{unwrappedType.Namespace}.{fullTypeName}" : fullTypeName;

                // Update sizing calculation to handle nulls
                if (isNullable)
                {
                    fieldInfo.SizeCalculation = $"{instanceFieldRef} == null ? 0 : EnumSerializer<{fullTypeWithNamespace}>.Instance.GetSize({instanceFieldRef})";
                }
                else
                {
                    fieldInfo.SizeCalculation = $"EnumSerializer<{fullTypeWithNamespace}>.Instance.GetSize({instanceFieldRef})";
                }

                // Update serialization code to use bitset
                if (isNullable)
                {
                    fieldInfo.SerializeCode = $"if (!NullableBitset.IsNull(bitset, {fieldInfo.NullableFieldIndex}))\n" +
                                             $"                EnumSerializer<{fullTypeWithNamespace}>.Instance.Serialize({instanceFieldRef}, buffer, ref offset);";
                }
                else
                {
                    fieldInfo.SerializeCode = $"EnumSerializer<{fullTypeWithNamespace}>.Instance.Serialize({instanceFieldRef}, buffer, ref offset);";
                }

                // Update deserialization code to use bitset
                if (isNullable)
                {
                    fieldInfo.DeserializeCode = $"if (NullableBitset.IsNull(bitset, {fieldInfo.NullableFieldIndex}))\n" +
                                                 $"                {instanceFieldRef} = null;\n" +
                                                 $"            else\n" +
                                                 $"            {{\n" +
                                                 $"                EnumSerializer<{fullTypeWithNamespace}>.Instance.Deserialize(out {fullTypeWithNamespace} {localVarName}, buffer, ref offset);\n" +
                                                 $"                {instanceFieldRef} = {localVarName};\n" +
                                                 $"            }}";
                }
                else
                {
                    fieldInfo.DeserializeCode = $"EnumSerializer<{fullTypeWithNamespace}>.Instance.Deserialize(out {fullTypeWithNamespace} {localVarName}, buffer, ref offset);\n" +
                                               $"            {instanceFieldRef} = {localVarName};";
                }
            }
            else if (fieldInfo.IsYoloSerializable)
            {
                var serializerName = $"{unwrappedType.Name}Serializer";
                var fullTypeName = TypeHelper.GetFullTypeName(unwrappedType);
                var fullTypeWithNamespace = unwrappedType.Namespace != field.DeclaringType.Namespace ?
                    $"{unwrappedType.Namespace}.{fullTypeName}" : fullTypeName;

                // Update sizing calculation to handle nulls
                if (isNullable)
                {
                    fieldInfo.SizeCalculation = $"{instanceFieldRef} == null ? 0 : {serializerName}.Instance.GetSize({instanceFieldRef})";
                }
                else
                {
                    fieldInfo.SizeCalculation = $"{serializerName}.Instance.GetSize({instanceFieldRef})";
                }

                // Update serialization code to use bitset
                if (isNullable)
                {
                    fieldInfo.SerializeCode = $"if (!NullableBitset.IsNull(bitset, {fieldInfo.NullableFieldIndex}))\n" +
                                             $"                {serializerName}.Instance.Serialize({instanceFieldRef}, buffer, ref offset);";
                }
                else
                {
                    fieldInfo.SerializeCode = $"{serializerName}.Instance.Serialize({instanceFieldRef}, buffer, ref offset);";
                }

                // Update deserialization code to use bitset
                if (isNullable)
                {
                    fieldInfo.DeserializeCode = $"if (NullableBitset.IsNull(bitset, {fieldInfo.NullableFieldIndex}))\n" +
                                                 $"                {instanceFieldRef} = null;\n" +
                                                 $"            else\n" +
                                                 $"            {{\n" +
                                                 $"                {serializerName}.Instance.Deserialize(out {fullTypeWithNamespace}{nullableMarker} {localVarName}, buffer, ref offset);\n" +
                                                 $"                {instanceFieldRef} = {localVarName};\n" +
                                                 $"            }}";
                }
                else
                {
                    fieldInfo.DeserializeCode = $"{serializerName}.Instance.Deserialize(out {fullTypeWithNamespace}{nullableMarker} {localVarName}, buffer, ref offset);\n" +
                                               $"            {instanceFieldRef} = {localVarName};";
                }
            }
            else
            {
                throw new NotSupportedException($"No serializer available for type {unwrappedType.FullName}");
            }
        }
    }
}
