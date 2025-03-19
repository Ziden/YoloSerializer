# YoloSerializer

A high-performance, zero-copy serializer for .NET Standard 2.1 with IL2CPP compatibility. Perfect for Unity projects that need fast serialization without reflection.

## Features

- Zero-copy serialization using `Span<T>`
- IL2CPP compatible (no reflection)
- Source generator for automatic serialization code
- Support for primitive types and nested objects
- Null value handling
- High performance with minimal allocations
- Compatible with .NET Standard 2.1 and .NET Core 8

## Installation

1. Add the YoloSerializer.Core package to your project
2. Add the YoloSerializer.Generator package as an analyzer to your project

```xml
<ItemGroup>
    <PackageReference Include="YoloSerializer.Core" Version="1.0.0" />
    <PackageReference Include="YoloSerializer.Generator" Version="1.0.0" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Usage

1. Mark your class with the `[YoloSerializable]` attribute and make it `partial`:

```csharp
[YoloSerializable]
public partial class Person
{
    private string name;
    private int age;
    private bool isActive;
    private Address address;  // Nested objects must also be marked with [YoloSerializable]
}

[YoloSerializable]
public partial class Address
{
    private string street;
    private int number;
}
```

2. Serialize your object:

```csharp
var person = new Person
{
    name = "John Doe",
    age = 30,
    isActive = true,
    address = new Address
    {
        street = "Main St",
        number = 123
    }
};

// Get required size
var size = person.GetSerializedSize();
var buffer = new byte[size];

// Serialize
var span = new Span<byte>(buffer);
var bytesWritten = person.Serialize(span);
```

3. Deserialize:

```csharp
var newPerson = new Person();
var bytesRead = newPerson.Deserialize(new ReadOnlySpan<byte>(buffer));
```

## Supported Types

- Primitive types:
  - `int`
  - `long`
  - `float`
  - `double`
  - `bool`
  - `string`
- Nested objects (must be marked with `[YoloSerializable]`)
- Null values

## Performance Considerations

- Uses `Span<T>` for zero-copy operations
- No reflection at runtime
- All serialization code is generated at compile time
- Minimal allocations during serialization/deserialization
- Aggressive inlining for primitive type operations

## Unity IL2CPP Compatibility

The serializer is designed to work with Unity's IL2CPP compiler:
- No reflection usage
- No dynamic code generation
- All code is generated at compile time
- Compatible with AOT compilation

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. 