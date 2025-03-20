using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Scriban;
using Xunit;
using YoloSerializer.Generator;
using YoloSerializer.Generator.Models;
using YoloSerializer.Tests.Models;

namespace YoloSerializer.Tests.Generator
{
    public class CodeGeneratorTests : IDisposable
    {
        private readonly string _testOutputPath;
        private readonly CodeGenerator _generator;
        private readonly Template _template;

        public CodeGeneratorTests()
        {
            var tempPath = Path.GetTempPath();
            var baseDir = Path.Combine(tempPath, "YoloSerializerTests");
            _testOutputPath = Path.Combine(baseDir, Guid.NewGuid().ToString());

            // Only create the base directory
            Directory.CreateDirectory(baseDir);
            Directory.CreateDirectory(_testOutputPath);

            _generator = new CodeGenerator();
            _template = Template.Parse(TestTemplate.Template);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testOutputPath))
            {
                Directory.Delete(_testOutputPath, true);
            }
        }

        [Fact]
        public async Task GenerateSerializers_CreatesCorrectDirectoryStructure()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = true };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            Assert.True(Directory.Exists(Path.Combine(_testOutputPath, "Serializers")));
            Assert.True(Directory.Exists(Path.Combine(_testOutputPath, "Maps")));
            Assert.True(Directory.Exists(Path.Combine(_testOutputPath, "Core")));
        }

        [Fact]
        public async Task GenerateSerializers_GeneratesSerializerFiles()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = true };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            var serializersPath = Path.Combine(_testOutputPath, "Serializers");
            Assert.True(File.Exists(Path.Combine(serializersPath, "PersonSerializer.cs")));
            Assert.True(File.Exists(Path.Combine(serializersPath, "AddressSerializer.cs")));
        }

        [Fact]
        public async Task GenerateSerializers_GeneratesMapFile()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = true };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            var mapsPath = Path.Combine(_testOutputPath, "Maps");
            var mapFile = Path.Combine(mapsPath, "YoloGeneratedMap.cs");
            Assert.True(File.Exists(mapFile));

            var content = await File.ReadAllTextAsync(mapFile);
            Assert.Contains("namespace YoloSerializer.Generated.Maps", content);
            Assert.Contains("public sealed class YoloGeneratedMap", content);
        }

        [Fact]
        public async Task GenerateSerializers_GeneratesCoreFiles()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = true };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            var corePath = Path.Combine(_testOutputPath, "Core");
            Assert.True(File.Exists(Path.Combine(corePath, "YoloGeneratedSerializer.cs")));
            Assert.True(File.Exists(Path.Combine(corePath, "YoloGeneratedConfig.cs")));
        }

        [Fact]
        public async Task GenerateSerializers_RespectsForceRegenerationFlag()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = false };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            var serializersPath = Path.Combine(_testOutputPath, "Serializers");
            var serializerFile = Path.Combine(serializersPath, "PersonSerializer.cs");
            var fileInfo = new FileInfo(serializerFile);
            var originalWriteTime = fileInfo.LastWriteTime;

            // Wait a bit to ensure timestamp would be different
            await Task.Delay(100);

            // Generate again with force flag
            config.ForceRegeneration = true;
            await _generator.GenerateSerializers(types.ToList(), config);

            // Verify file was updated
            fileInfo.Refresh();
            Assert.True(fileInfo.LastWriteTime > originalWriteTime);
        }

        [Fact]
        public async Task GeneratedSerializer_HasCorrectNamespaceAndImports()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = true };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            var serializersPath = Path.Combine(_testOutputPath, "Serializers");
            var content = await File.ReadAllTextAsync(Path.Combine(serializersPath, "PersonSerializer.cs"));

            Assert.Contains("using System;", content);
            Assert.Contains("using System.Buffers.Binary;", content);
            Assert.Contains("using System.Runtime.CompilerServices;", content);
            Assert.Contains("using YoloSerializer.Core;", content);
            Assert.Contains($"using {typeof(Person).Namespace};", content);
            Assert.Contains("using YoloSerializer.Core.Serializers;", content);
        }

        [Fact]
        public async Task GeneratedConfig_ContainsAllSerializableTypes()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = true };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            var corePath = Path.Combine(_testOutputPath, "Core");
            var content = await File.ReadAllTextAsync(Path.Combine(corePath, "YoloGeneratedConfig.cs"));

            Assert.Contains("typeof(Person)", content);
            Assert.Contains("typeof(Address)", content);
            Assert.Contains("namespace YoloSerializer.Generated.Core", content);
        }

        [Fact]
        public async Task GeneratedMap_ContainsTypeIds()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = true };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            var mapsPath = Path.Combine(_testOutputPath, "Maps");
            var content = await File.ReadAllTextAsync(Path.Combine(mapsPath, "YoloGeneratedMap.cs"));

            Assert.Contains("public const byte PERSON_TYPE_ID = 1;", content);
            Assert.Contains("public const byte ADDRESS_TYPE_ID = 2;", content);
        }

        [Fact]
        public async Task GeneratedMap_ContainsSerializationMethods()
        {
            // Arrange
            var types = new[] { typeof(Person), typeof(Address) };
            var config = new GeneratorConfig { OutputPath = _testOutputPath, ForceRegeneration = true };

            // Act
            await _generator.GenerateSerializers(types.ToList(), config);

            // Assert
            var mapsPath = Path.Combine(_testOutputPath, "Maps");
            var content = await File.ReadAllTextAsync(Path.Combine(mapsPath, "YoloGeneratedMap.cs"));

            Assert.Contains("public void Serialize<T>", content);
            Assert.Contains("public int GetSerializedSize<T>", content);
            Assert.Contains("public object? DeserializeById", content);
        }
    }
} 