using System;

namespace YoloSerializer.Core.CodeGeneration
{
    /// <summary>
    /// Marks a class as serializable with YoloSerializer
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class YoloSerializableAttribute : Attribute
    {
        /// <summary>
        /// The unique type ID for this type
        /// </summary>
        public byte TypeId { get; }
        
        /// <summary>
        /// Whether to use object pooling for this type
        /// </summary>
        public bool UseObjectPooling { get; set; }
        
        /// <summary>
        /// Creates a new YoloSerializable attribute
        /// </summary>
        /// <param name="typeId">The unique type ID (1-255, 0 is reserved for null)</param>
        public YoloSerializableAttribute(byte typeId)
        {
            if (typeId == 0)
                throw new ArgumentException("Type ID 0 is reserved for null");
            
            TypeId = typeId;
        }
    }
    
    /// <summary>
    /// Marks a property for serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class YoloSerializeAttribute : Attribute
    {
        /// <summary>
        /// The order of serialization
        /// </summary>
        public int Order { get; set; }
    }
    
    /// <summary>
    /// Excludes a property from serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class YoloIgnoreAttribute : Attribute
    {
    }
    
    /// <summary>
    /// Specifies a custom serializer for a property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class YoloCustomSerializerAttribute : Attribute
    {
        /// <summary>
        /// The type of the custom serializer
        /// </summary>
        public Type SerializerType { get; }
        
        /// <summary>
        /// Creates a new YoloCustomSerializer attribute
        /// </summary>
        /// <param name="serializerType">The type of the custom serializer</param>
        public YoloCustomSerializerAttribute(Type serializerType)
        {
            SerializerType = serializerType ?? throw new ArgumentNullException(nameof(serializerType));
        }
    }
} 