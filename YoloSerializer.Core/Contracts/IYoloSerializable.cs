using System;

namespace YoloSerializer.Core.Contracts
{
    /// <summary>
    /// Marker interface for serializable types
    /// </summary>
    public interface IYoloSerializable
    {
        /// <summary>
        /// Type ID for serialization (must be unique per type)
        /// </summary>
        byte TypeId { get; }
    }
} 