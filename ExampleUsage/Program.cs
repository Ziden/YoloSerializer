using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.ModelsYolo;
using YoloSerializer.Generator;
using YoloSerializer.Generator.Models;

namespace YoloSerializer.Example
{
    public class Program
    {
        // Define explicitly serializable types here
        private static readonly Type[] ExplicitSerializableTypes = new[]
        {
            typeof(PlayerData),
            typeof(Node),
            typeof(Inventory),
            typeof(Position),
            typeof(AllTypesData)
        };

        static async Task Main(string[] args)
        {
            // Get list of serializable types
            var serializableTypes = ExplicitSerializableTypes.ToList();
            Console.WriteLine($"Found {serializableTypes.Count} serializable types");

            // Generate all required files
            var generator = new CodeGenerator();

            var config = new GeneratorConfig
            {
                TargetAssembly = typeof(PlayerData).Assembly,
                OutputPath = GetDefaultOutputPath(),
                ForceRegeneration = args.Contains("--force") || args.Contains("-f")
            };

            config.ForceRegeneration = true; // Faster testing

            await generator.GenerateSerializers(ExplicitSerializableTypes.ToList(), config);
        }

        private static string GetDefaultOutputPath()
        {
            var solutionDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
            return Path.GetFullPath(Path.Combine(solutionDir, "ExampleUsage", "Generated"));
        }
    }
}
