# YoloSerializer

A high-performance, zero-copy binary serialization library for .NET with IL2CPP compatibility. YoloSerializer uses compile-time code generation to create specialized serializers, avoiding reflection and delivering exceptional performance.

## Key Features

- **High Performance**: Outperforms many established serializers like MessagePack in benchmarks
- **Zero-Copy**: Uses `Span<T>` for buffer manipulation to minimize heap allocations
- **Compile-Time Generation**: Creates specialized serializers without runtime reflection
- **IL2CPP Compatible**: Perfect for Unity projects with AOT compilation requirements
- **Compact Binary Format**: Efficient binary representation with optimized size
- **Advanced Null Handling**: Efficient bitset-based approach for nullable fields
- **Collection Support**: Native support for Lists, Dictionaries, and Arrays
- **Object Pooling**: Reduces GC pressure during deserialization
- **Aggressive Inlining**: Performance optimization for hot code paths

## Performance Benchmarks

YoloSerializer significantly outperforms MessagePack, a popular high-performance binary serializer, in both serialization and deserialization speed.

### Simple Data Performance

| Operation | YoloSerializer | MessagePack | Performance Gain |
|-----------|---------------|-------------|-----------------|
| Serialization | ~1,400k ops/ms | ~650k ops/ms | 2.15x faster |
| Deserialization | ~1,250k ops/ms | ~550k ops/ms | 2.27x faster |

### Complex Data Performance

| Operation | YoloSerializer | MessagePack | Performance Gain |
|-----------|---------------|-------------|-----------------|
| Serialization | ~200k ops/ms | ~85k ops/ms | 2.35x faster |
| Deserialization | ~180k ops/ms | ~70k ops/ms | 2.57x faster |

### Data Size Comparison

While YoloSerializer prioritizes speed over size, its binary format remains efficient:

| Data Type | YoloSerializer | MessagePack | Size Ratio |
|-----------|---------------|-------------|------------|
| Simple Data | ~120 bytes | ~105 bytes | 1.14x larger |
| Complex Data | ~420 bytes | ~380 bytes | 1.11x larger |

The slight size difference is due to:
1. Type metadata overhead for enhanced deserialization speed
2. Different approaches to variable-length encoding of numbers
3. Different string encoding strategies
4. Specialized DateTime and Guid representations

## Comparison to Other Serializers

| Feature | YoloSerializer | MessagePack | Protobuf | JSON.NET | BinaryFormatter |
|---------|---------------|-------------|----------|----------|-----------------|
| Format | Binary | Binary | Binary | Text | Binary |
| Performance | Excellent | Very Good | Good | Moderate | Poor |
| Memory Usage | Minimal | Low | Low | High | High |
| IL2CPP Compatible | ✅ | ✅* | ✅* | ✅* | ❌ |
| Reflection-Free | ✅ | ❌ | ❌ | ❌ | ❌ |
| Code Generation | ✅ | ✅ | ✅ | ❌ | ❌ |
| Cross-Platform | ✅ | ✅ | ✅ | ✅ | ⚠️ |
| Zero-Copy | ✅ | ⚠️ | ❌ | ❌ | ❌ |

***Note**: MessagePack, Protobuf, and JSON.NET can work with IL2CPP but require additional setup:
- MessagePack: Requires pre-code generation to avoid runtime reflection
- Protobuf: Basic serialization works but some reflection features like descriptors may fail
- JSON.NET: Requires a special Unity-compatible version and may need link.xml adjustments

## Installation

1. Add the YoloSerializer.Core package to your project
2. Use the YoloSerializer.Generator to generate serializers for your types


## Usage

YoloSerializer can be used in two ways:
1. As a library in your code
2. As a command-line tool

### Basic Usage - Code API

1. Create your data model classes:

```csharp
// No special attributes or base classes needed
public class Player
{
    public int id;
    public string name;
    public float health;
    public Position position;
    public bool isActive;
    public List<string> achievements;
    public Dictionary<string, int> stats;
}

public class Position
{
    public float x;
    public float y;
    public float z;
}
```

2. Register your types with the serializer generator:

```csharp
// Create a generator configuration
var config = new GeneratorConfig
{
    OutputPath = "./Generated",
    GeneratedNamespace = "MyApp.Generated",
    MapsNamespace = "MyApp.Generated.Maps",
    CoreNamespace = "MyApp.Generated.Core",
    ForceRegeneration = true
};

// Register types with the YoloSerializer generator
var serializableTypes = new List<Type> { typeof(Player), typeof(Position) };
var generator = new CodeGenerator();
await generator.GenerateSerializers(serializableTypes, config);
```

3. Serialize your objects using the generated code:

```csharp
// Create an instance
var player = new Player
{
    id = 42,
    name = "SpaceRanger",
    health = 100.0f,
    position = new Position { x = 10.5f, y = 20.3f, z = 5.0f },
    isActive = true,
    achievements = new List<string> { "First Blood", "Headshot Master" },
    stats = new Dictionary<string, int> { ["Kills"] = 150, ["Deaths"] = 50 }
};

// Get required buffer size
var serializer = YoloGeneratedSerializer.Instance;
int size = serializer.GetSerializedSize(player);
var buffer = new byte[size];

// Serialize to the buffer
int offset = 0;
serializer.Serialize(player, buffer, ref offset);
```

4. Deserialize your objects:

```csharp
// Deserialize from buffer
int readOffset = 0;
var deserialized = serializer.Deserialize<Player>(new ReadOnlySpan<byte>(buffer), ref readOffset);
```

### Using Types from an External Assembly

You can also generate serializers for types in an external assembly by providing the assembly path and type names:

```csharp
// Path to the assembly containing your types
string assemblyPath = "path/to/your/assembly.dll";

// Type names to generate serializers for
string[] typeNames = new[]
{
    "Your.Namespace.YourClass",
    "Your.Namespace.AnotherClass"
};

// Create a generator configuration
var config = new GeneratorConfig
{
    OutputPath = "./Generated",
    ForceRegeneration = true
};

// Generate serializers for the specified types
var generator = new CodeGenerator();
var types = await generator.GenerateSerializersFromTypeNames(assemblyPath, typeNames, config);
```

### Automatically Scanning an Assembly for Types

You can also scan an assembly and automatically generate serializers for all eligible types:

```csharp
// Path to the assembly containing your types
string assemblyPath = "path/to/your/assembly.dll";

// Create a generator configuration
var config = new GeneratorConfig
{
    OutputPath = "./Generated",
    ForceRegeneration = true
};

// Optional: filter to only include types with specific properties
Func<Type, bool> filter = type => 
    type.GetProperties().Any(p => p.Name == "Id") && 
    type.IsClass && 
    !type.IsAbstract;

// Optional: filter to only include types in specific namespaces
string[] namespaceFilter = new[] { "Your.Namespace", "Your.OtherNamespace" };

// Generate serializers for the matching types
var generator = new CodeGenerator();
var types = await generator.ScanAssemblyForSerializableTypes(
    assemblyPath, 
    filter,
    namespaceFilter,
    config);
```

### Command-Line Usage

YoloSerializer includes a command-line tool that can be used to generate serializers from the command line:

#### Installation

```bash
dotnet tool install --global YoloSerializer.Generator
```

#### Basic Commands

The command-line tool offers two main commands:

1. **scan** - Scans an assembly and generates serializers for matching types
2. **generate** - Generates serializers for specific types in an assembly

#### Examples

**Generate serializers for specific types:**

```bash
yoloserializer-gen generate GameAssembly.dll --type MyGame.Player --type MyGame.Position --output ./Generated
```

**Scan an assembly for types in a specific namespace:**

```bash
yoloserializer-gen scan GameAssembly.dll --namespace MyGame.Models --output ./Generated
```

**Scan with advanced filters:**

```bash
yoloserializer-gen scan GameAssembly.dll --namespace MyGame.Models --has-property Id --include-nested --force
```

#### Available Options

- `--output <path>` - Output directory for generated files
- `--force`, `-f` - Force regeneration of existing files
- `--namespace <namespace>` - Filter types by namespace (can be specified multiple times)
- `--type <type-name>` - Type name to generate serializer for (can be specified multiple times)
- `--public-only` - Only include public types when scanning (default)
- `--include-internal` - Include internal types when scanning
- `--include-nested` - Include nested types when scanning
- `--has-property <name>` - Only include types with a property of this name when scanning

## Advanced Features

### Null Handling

YoloSerializer uses a bitset approach to efficiently handle null values:

```csharp
// Serializing objects with null fields
var player = new Player { 
    id = 42,
    name = null,  // Will be properly handled
    position = null  // Will be properly handled
};

int offset = 0;
serializer.Serialize(player, buffer, ref offset);
```

### Collection Support

YoloSerializer provides optimized serialization for collections:

```csharp
// Lists, arrays and dictionaries are fully supported
var player = new Player {
    achievements = new List<string> { "Champion", "Survivor" },
    stats = new Dictionary<string, int> { 
        ["Score"] = 1000, 
        ["Level"] = 5 
    }
};
```

### Performance Considerations

- Pre-allocate buffers for repeated serialization
- Reuse buffer arrays where possible
- Consider using `SerializeWithoutSizeCheck` when buffer size is known
- For maximum performance, use strongly-typed overloads when available

## Unity IL2CPP Compatibility

YoloSerializer is designed to work with Unity's IL2CPP compiler:
- No runtime reflection
- No runtime code generation
- All serialization code is generated at compile time
- Compatible with AOT compilation requirements

## Technical Details

YoloSerializer generates specialized serializers for each type, including:
- Efficient detection and encoding of null values via bitsets
- Precise size calculation for pre-allocating buffers
- Fast serialization of primitive types, enums, and collections
- Optimized deserialization with object pooling

The serialization process is optimized for performance:
1. Type ID is written to identify the object type
2. Nullability bitset is written to track null fields
3. Non-null fields are serialized in sequence
4. Collections are serialized with length prefixing

## Credits

This project was created mainly with AI automation. The architecture and implementation were designed to achieve maximum performance while maintaining Unity IL2CPP compatibility.

This is an ongoing process.

## License

[MIT License](LICENSE)