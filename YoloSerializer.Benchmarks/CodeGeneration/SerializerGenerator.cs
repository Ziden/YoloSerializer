using Scriban;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YoloSerializer.Benchmarks.Models;
using YoloSerializer.Generator;
using YoloSerializer.Generator.Models;

namespace YoloSerializer.Benchmarks.CodeGeneration
{
    public static class SerializerGenerator
    {
        public static void GenerateSerializers()
        {
            // Create a list of all serializable types we need
            var types = new List<Type>
            {
                typeof(SimpleData),
                typeof(ComplexData),
                typeof(NestedData)
            };

            // Load the template
            var assembly = Assembly.GetExecutingAssembly();
            string templatePath = "YoloSerializer.Benchmarks.Templates.SerializerTemplate.scriban";
            using var stream = assembly.GetManifestResourceStream(templatePath);
            using var reader = new StreamReader(stream);
            var templateContent = reader.ReadToEnd();
            var template = Template.Parse(templateContent);

            // Configure generator settings for benchmark project
            var sourceDir = GetSourceCodeDirectory();
            var outputPath = Path.Combine(sourceDir, "Generated");
            var config = new GeneratorConfig
            {
                OutputPath = outputPath,
                GeneratedNamespace = "YoloSerializer.Benchmarks.Generated",
                MapsNamespace = "YoloSerializer.Benchmarks.Generated.Maps",
                CoreNamespace = "YoloSerializer.Benchmarks.Generated.Core"
            };

            // Generate the code
            var generator = new CodeGenerator();
            generator.GenerateSerializers(types, config).GetAwaiter().GetResult();
        }

        private static string GetSourceCodeDirectory()
        {
            // Find the project directory by walking up from the current assembly location
            string? directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            // In case we're running in the 'bin/Debug/net8.0' directory, go up three levels
            if (directoryPath != null)
            {
                if (Path.GetFileName(directoryPath).Equals("net8.0", StringComparison.OrdinalIgnoreCase))
                {
                    directoryPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(directoryPath)));
                }
            }

            return directoryPath ?? Directory.GetCurrentDirectory();
        }
    }
} 