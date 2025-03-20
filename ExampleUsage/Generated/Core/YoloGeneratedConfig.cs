using System;
using System.Collections.Generic;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.ModelsYolo;

namespace YoloSerializer.Generated.Core
{
    public static class YoloGeneratedConfig
    {
        public static readonly HashSet<Type> SerializableTypes = new()
        {
            typeof(PlayerData),
            typeof(Node),
            typeof(Inventory),
            typeof(Position),
            typeof(AllTypesData),
        };
    }
}
