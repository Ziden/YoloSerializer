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

## Comparison to Other Serializers

| Feature | YoloSerializer | MessagePack | Protobuf | JSON.NET | BinaryFormatter |
|---------|---------------|-------------|----------|----------|-----------------|
| Format | Binary | Binary | Binary | Text | Binary |
| Performance | Excellent | Very Good | Good | Moderate | Poor |
| Memory Usage | Minimal | Low | Low | High | High |
| IL2CPP Compatible | ✅ | ⚠️ | ⚠️ | ⚠️ | ❌ |
| Reflection-Free | ✅ | ❌ | ❌ | ❌ | ❌ |
| Code Generation | ✅ | ⚠️ | ✅ | ❌ | ❌ |
| Cross-Platform | ✅ | ✅ | ✅ | ✅ | ⚠️ |
| Zero-Copy | ✅ | ⚠️ | ❌ | ❌ | ❌ |

## Installation

1. Add the YoloSerializer.Core package to your project
2. Use the YoloSerializer.Generator to generate serializers for your types

```xml
<ItemGroup>
    <PackageReference Include="YoloSerializer.Core" Version="1.0.0" />
</ItemGroup>
```

## Basic Usage

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

This is an ongoing proccess.

## License

[MIT License](LICENSE)