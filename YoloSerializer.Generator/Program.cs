using System.Reflection;
using YoloSerializer.Generator.Models;

namespace YoloSerializer.Generator;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
            {
                ShowHelp();
                return 0;
            }

            var command = args[0].ToLowerInvariant();
            
            switch (command)
            {
                case "scan":
                    await ScanAssembly(args.Skip(1).ToArray());
                    break;
                case "generate":
                    await GenerateFromTypeNames(args.Skip(1).ToArray());
                    break;
                default:
                    Console.WriteLine($"Error: Unknown command '{command}'");
                    ShowHelp();
                    return 1;
            }

            Console.WriteLine("Done!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("YoloSerializer Generator");
        Console.WriteLine("=======================");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  scan <assembly-path> [options]      - Scan the assembly for serializable types");
        Console.WriteLine("  generate <assembly-path> [options]  - Generate serializers for specific types");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --output <path>               - Output directory for generated files");
        Console.WriteLine("  --force, -f                   - Force regeneration of existing files");
        Console.WriteLine("  --namespace <namespace>       - Filter types by namespace (can be specified multiple times)");
        Console.WriteLine("  --type <type-name>            - Type name to generate serializer for (can be specified multiple times)");
        Console.WriteLine("  --public-only                 - Only include public types when scanning (default)");
        Console.WriteLine("  --include-internal            - Include internal types when scanning");
        Console.WriteLine("  --include-nested              - Include nested types when scanning");
        Console.WriteLine("  --has-property <name>         - Only include types with a property of this name when scanning");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  yoloserializer-gen scan MyAssembly.dll --output ./Generated");
        Console.WriteLine("  yoloserializer-gen scan MyAssembly.dll --namespace MyNamespace --namespace MyNamespace.Models");
        Console.WriteLine("  yoloserializer-gen generate MyAssembly.dll --type MyNamespace.MyClass --type MyNamespace.AnotherClass");
    }

    private static async Task ScanAssembly(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Assembly path is required for the scan command");
        }

        var assemblyPath = args[0];
        
        // Parse options
        var outputPath = GetArgumentValue(args, "--output");
        var namespaces = GetArgumentValues(args, "--namespace");
        bool forceRegeneration = args.Contains("--force") || args.Contains("-f");
        bool includeInternal = args.Contains("--include-internal");
        bool includeNested = args.Contains("--include-nested");
        var hasProperties = GetArgumentValues(args, "--has-property");

        // Validate assembly path
        if (!File.Exists(assemblyPath))
        {
            throw new FileNotFoundException($"Assembly file not found: {assemblyPath}");
        }

        // Create config
        var config = new GeneratorConfig
        {
            OutputPath = string.IsNullOrEmpty(outputPath) ? GetDefaultOutputPath(assemblyPath) : outputPath,
            ForceRegeneration = forceRegeneration
        };

        // Create filter
        Func<Type, bool> filter = type => 
        {
            // Basic filters
            if (!type.IsClass) return false;
            if (type.IsAbstract) return false;
            if (!includeNested && type.IsNested) return false;
            if (!includeInternal && !type.IsPublic) return false;

            // Property filters
            if (hasProperties.Length > 0)
            {
                var properties = type.GetProperties();
                foreach (var propertyName in hasProperties)
                {
                    if (!properties.Any(p => p.Name == propertyName))
                        return false;
                }
            }

            return true;
        };

        Console.WriteLine($"Scanning assembly: {assemblyPath}");
        Console.WriteLine($"Output path: {config.OutputPath}");
        
        if (namespaces.Length > 0)
            Console.WriteLine($"Namespace filters: {string.Join(", ", namespaces)}");
        
        var generator = new CodeGenerator();
        var types = await generator.ScanAssemblyForSerializableTypes(
            assemblyPath, 
            filter,
            namespaces.Length > 0 ? namespaces : null,
            config);

        Console.WriteLine($"Generated serializers for {types.Count} types");
    }

    private static async Task GenerateFromTypeNames(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Assembly path is required for the generate command");
        }

        var assemblyPath = args[0];
        
        // Parse options
        var outputPath = GetArgumentValue(args, "--output");
        var typeNames = GetArgumentValues(args, "--type");
        bool forceRegeneration = args.Contains("--force") || args.Contains("-f");

        // Validate assembly path
        if (!File.Exists(assemblyPath))
        {
            throw new FileNotFoundException($"Assembly file not found: {assemblyPath}");
        }

        // Validate type names
        if (typeNames.Length == 0)
        {
            throw new ArgumentException("At least one type name is required (use --type <type-name>)");
        }

        // Create config
        var config = new GeneratorConfig
        {
            OutputPath = string.IsNullOrEmpty(outputPath) ? GetDefaultOutputPath(assemblyPath) : outputPath,
            ForceRegeneration = forceRegeneration
        };

        Console.WriteLine($"Generating serializers for assembly: {assemblyPath}");
        Console.WriteLine($"Output path: {config.OutputPath}");
        Console.WriteLine($"Types: {string.Join(", ", typeNames)}");
        
        var generator = new CodeGenerator();
        var types = await generator.GenerateSerializersFromTypeNames(assemblyPath, typeNames, config);

        Console.WriteLine($"Generated serializers for {types.Count} types");
    }

    private static string GetArgumentValue(string[] args, string argName)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argName)
            {
                return args[i + 1];
            }
        }
        return string.Empty;
    }

    private static string[] GetArgumentValues(string[] args, string argName)
    {
        var values = new List<string>();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argName)
            {
                values.Add(args[i + 1]);
            }
        }
        return values.ToArray();
    }

    private static string GetDefaultOutputPath(string assemblyPath)
    {
        return Path.Combine(Path.GetDirectoryName(assemblyPath), "Generated");
    }
}
