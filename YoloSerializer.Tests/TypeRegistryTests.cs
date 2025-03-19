using System;
using Xunit;
using YoloSerializer.Core;
using YoloSerializer.Core.Models;
using YoloSerializer.Core.Contracts;

namespace YoloSerializer.Tests
{
    [Collection("Sequential")]
    public class TypeRegistryTests
    {
        [Fact]
        public void RegisterType_ShouldAssignUniqueIds()
        {
            // Arrange
            TypeRegistry.Reset();
            
            // Act
            TypeRegistry.RegisterType<Position>();
            TypeRegistry.RegisterType<PlayerData>();
            
            // Assert
            Assert.True(TypeRegistry.IsTypeRegistered<Position>());
            Assert.True(TypeRegistry.IsTypeRegistered<PlayerData>());
            
            byte positionId = TypeRegistry.GetTypeId<Position>();
            byte playerDataId = TypeRegistry.GetTypeId<PlayerData>();
            
            Assert.NotEqual(positionId, playerDataId);
            Assert.NotEqual(TypeRegistry.NULL_TYPE_ID, positionId);
            Assert.NotEqual(TypeRegistry.NULL_TYPE_ID, playerDataId);
        }
        
        [Fact]
        public void RegisterType_WithSpecificId_ShouldUseProvidedId()
        {
            // Arrange
            TypeRegistry.Reset();
            
            // Create a test class that implements IYoloSerializable
            var testType = typeof(TestSerializable);
            const byte testTypeId = 42;
            
            // Act
            TypeRegistry.RegisterType<TestSerializable>(testTypeId);
            
            // Assert
            Assert.True(TypeRegistry.IsTypeRegistered<TestSerializable>());
            Assert.Equal(testTypeId, TypeRegistry.GetTypeId<TestSerializable>());
            
            // Verify we can retrieve the type from ID
            Type retrievedType = TypeRegistry.GetTypeForId(testTypeId);
            Assert.Equal(testType, retrievedType);
        }
        
        [Fact]
        public void RegisterType_WithReservedId_ShouldThrowException()
        {
            // Arrange
            TypeRegistry.Reset();
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                TypeRegistry.RegisterType<TestSerializable>(TypeRegistry.NULL_TYPE_ID));
                
            Assert.Contains("reserved for null values", exception.Message);
        }
        
        [Fact]
        public void RegisterType_WithDuplicateId_ShouldThrowException()
        {
            // Arrange
            TypeRegistry.Reset();
            const byte duplicateId = 50;
            TypeRegistry.RegisterType<TestSerializable>(duplicateId);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                TypeRegistry.RegisterType<AnotherTestSerializable>(duplicateId));
                
            Assert.Contains("already registered", exception.Message);
        }
        
        [Fact]
        public void GetTypeId_ForUnregisteredType_ShouldThrowException()
        {
            // Arrange
            TypeRegistry.Reset();
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                TypeRegistry.GetTypeId<UnregisteredTestSerializable>());
                
            Assert.Contains("is not registered", exception.Message);
        }
        
        [Fact]
        public void GetTypeForId_ForUnregisteredId_ShouldThrowException()
        {
            // Arrange
            TypeRegistry.Reset();
            
            // Use an ID that isn't registered
            byte unregisteredId = 99;
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                TypeRegistry.GetTypeForId(unregisteredId));
                
            Assert.Contains("is not registered", exception.Message);
        }
        
        [Fact]
        public void GetTypeForId_ForNullTypeId_ShouldReturnObjectType()
        {
            // Arrange
            TypeRegistry.Reset();
            
            // Act
            Type type = TypeRegistry.GetTypeForId(TypeRegistry.NULL_TYPE_ID);
            
            // Assert
            Assert.Equal(typeof(object), type);
        }
        
        [Fact]
        public void GetAllRegisteredTypes_ShouldReturnAllRegisteredTypes()
        {
            // Arrange
            TypeRegistry.Reset();
            TypeRegistry.RegisterType<TestSerializable>(60);
            TypeRegistry.RegisterType<AnotherTestSerializable>(61);
            
            // Act
            var registeredTypes = TypeRegistry.GetAllRegisteredTypes();
            
            // Assert
            Assert.Contains(typeof(TestSerializable), registeredTypes.Keys);
            Assert.Contains(typeof(AnotherTestSerializable), registeredTypes.Keys);
            Assert.Equal(60, registeredTypes[typeof(TestSerializable)]);
            Assert.Equal(61, registeredTypes[typeof(AnotherTestSerializable)]);
        }
        
        #region Test Classes
        private class TestSerializable : IYoloSerializable
        {
            public byte TypeId => 42;
        }
        
        private class AnotherTestSerializable : IYoloSerializable
        {
            public byte TypeId => 43;
        }
        
        private class UnregisteredTestSerializable : IYoloSerializable
        {
            public byte TypeId => 44;
        }
        #endregion
    }
} 