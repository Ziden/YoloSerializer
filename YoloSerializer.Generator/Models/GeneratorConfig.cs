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
    }
}
