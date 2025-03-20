using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YoloSerializer.Generator.Models
{

    /// <summary>
    /// Configuration for the serializer generator
    /// </summary>
    public class GeneratorConfig
    {
        public Assembly TargetAssembly { get; set; }
        public string OutputPath { get; set; }
        public bool ForceRegeneration { get; set; }
        public string GeneratedNamespace { get; set; } = "YoloSerializer.Generated";
        public string MapsNamespace { get; set; } = "YoloSerializer.Generated.Maps";
        public string CoreNamespace { get; set; } = "YoloSerializer.Generated.Core";
        public string ModelsNamespace { get; set; } = "YoloSerializer.Core.Models";

        public string GetTypeNamespace(Type type)
        {
            return type.Namespace ?? throw new ArgumentException($"Type {type.Name} has no namespace");
        }
    }
}
