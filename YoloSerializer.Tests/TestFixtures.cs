using System;
using Xunit;
using YoloSerializer.Core;

namespace YoloSerializer.Tests
{
    // Collection definition for tests that need to run sequentially
    // to avoid conflicts when accessing shared static resources like TypeRegistry
    [CollectionDefinition("Sequential")]
    public class SequentialTestCollection : ICollectionFixture<TypeRegistryFixture>
    {
        // This class has no code, and is never created.
        // Its purpose is to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    // Fixture for TypeRegistry to reset between test classes
    public class TypeRegistryFixture : IDisposable
    {
        public TypeRegistryFixture()
        {
            // Reset registry at the start of each test collection
            TypeRegistry.Reset();
        }

        public void Dispose()
        {
            // Reset registry at the end of each test collection
            TypeRegistry.Reset();
        }
    }
} 