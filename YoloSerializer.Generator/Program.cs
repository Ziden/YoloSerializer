using System.Reflection;
using Scriban;
using YoloSerializer.Generator.Models;

namespace YoloSerializer.Generator;

public class Program
{
    static async Task Main(string[] args)
    {
        var config = ParseCommandLineArgs(args);

        Directory.CreateDirectory(config.OutputPath);

        Console.WriteLine($"Generating serializers for types in {config.TargetAssembly.FullName}");
        Console.WriteLine($"Output path: {config.OutputPath}");
        Console.WriteLine($"Force regeneration: {config.ForceRegeneration}");

        var generator = new CodeGenerator();
        await generator.GenerateSerializers(serializableTypes, config);

        Console.WriteLine("Done!");
    }

    private static GeneratorConfig ParseCommandLineArgs(string[] args)
    {
        // Default configuration
        var config = new GeneratorConfig
        {
            //TargetAssembly = typeof(YoloSerializer.Core.Models.PlayerData).Assembly,
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
        return Path.GetFullPath(Path.Combine(solutionDir, "ExampleUsage", "Generated"));
    }
}
