using System;
using System.Collections.Generic;
using YoloSerializer.Benchmarks.Models;

namespace YoloSerializer.Benchmarks.Generated.Core
{
    public static class YoloGeneratedConfig
    {
        public static readonly HashSet<Type> SerializableTypes = new()
        {
            typeof(SimpleData),
            typeof(ComplexData),
            typeof(NestedData),
        };
    }
}
