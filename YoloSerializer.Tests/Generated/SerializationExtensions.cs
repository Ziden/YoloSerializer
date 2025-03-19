using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using YoloSerializer.Core.Models;

namespace YoloSerializer.Core
{
    /// <summary>
    /// Provides extension methods for serialization operations with buffer pooling
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Serializes an object to a byte array using a pooled buffer
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="value">The object to serialize</param>
        /// <returns>A new byte array containing the serialized data</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeToPooledArray<T>(this T value)
            where T : class, IYoloSerializable
        {
            // First calculate the required size
            int size = GeneratedSerializerEntry.GetSerializedSize(value);
            
            // Get a buffer from the pool (may be larger than needed)
            byte[] buffer = SerializationBufferPool.Rent(size);
            
            try
            {
                // Serialize to the buffer
                int offset = 0;
                GeneratedSerializerEntry.Serialize(value, buffer, ref offset);
                
                // Create a properly sized result array
                byte[] result = new byte[offset];
                Buffer.BlockCopy(buffer, 0, result, 0, offset);
                
                return result;
            }
            finally
            {
                // Return the buffer to the pool
                SerializationBufferPool.Return(buffer);
            }
        }
        
        /// <summary>
        /// Deserializes an object from a byte array
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="data">The serialized data</param>
        /// <returns>The deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Deserialize<T>(this byte[] data)
            where T : class, IYoloSerializable
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
                
            int offset = 0;
            return GeneratedSerializerEntry.Deserialize<T>(data, ref offset);
        }
        
        /// <summary>
        /// Serializes an object to a pooled buffer and executes an action with the resulting data
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="value">The object to serialize</param>
        /// <param name="action">Action to execute with the serialized data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializeToPooledBuffer<T>(this T value, Action<byte[], int> action)
            where T : class, IYoloSerializable
        {
            // Calculate size and rent buffer
            int size = GeneratedSerializerEntry.GetSerializedSize(value);
            byte[] buffer = SerializationBufferPool.Rent(size);
            
            try
            {
                // Serialize
                int offset = 0;
                GeneratedSerializerEntry.Serialize(value, buffer, ref offset);
                
                // Execute the action with the buffer and actual size
                action(buffer, offset);
            }
            finally
            {
                // Return buffer to pool
                SerializationBufferPool.Return(buffer);
            }
        }
        
        /// <summary>
        /// Determines if two objects serialize to the same binary representation
        /// </summary>
        /// <typeparam name="T">The type of objects to compare</typeparam>
        /// <param name="a">First object</param>
        /// <param name="b">Second object</param>
        /// <returns>True if the serialized representations are identical</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SerializedEquals<T>(this T? a, T? b)
            where T : class, IYoloSerializable
        {
            Debug.WriteLine($"SerializedEquals: Comparing objects of type {typeof(T).Name}");
            
            // Special case for reference equality
            if (ReferenceEquals(a, b))
            {
                Debug.WriteLine("SerializedEquals: Objects are reference equal");
                return true;
            }
                
            // If one is null but not both
            if (a == null || b == null)
            {
                Debug.WriteLine("SerializedEquals: One object is null");
                return false;
            }
                
            // Directly check model equality first, if implemented correctly
            bool regularEquals = a.Equals(b);
            Debug.WriteLine($"SerializedEquals: Regular Equals() returned {regularEquals}");
            
            if (regularEquals)
                return true;
                
            // Serialize both objects into separate arrays for direct comparison
            byte[] bytesA = SerializeToPooledArray(a);
            byte[] bytesB = SerializeToPooledArray(b);
            
            Debug.WriteLine($"SerializedEquals: bytesA.Length={bytesA.Length}, bytesB.Length={bytesB.Length}");
            
            // Compare lengths first (quick check)
            if (bytesA.Length != bytesB.Length)
            {
                Debug.WriteLine("SerializedEquals: Byte arrays have different lengths");
                return false;
            }
                
            // Compare the contents byte by byte
            for (int i = 0; i < bytesA.Length; i++)
            {
                if (bytesA[i] != bytesB[i])
                {
                    Debug.WriteLine($"SerializedEquals: Difference at byte {i}: {bytesA[i]} vs {bytesB[i]}");
                    
                    // Debug output for the arrays around the difference
                    int start = Math.Max(0, i - 5);
                    int end = Math.Min(bytesA.Length - 1, i + 5);
                    
                    var sb = new StringBuilder();
                    sb.AppendLine($"BytesA from {start} to {end}:");
                    
                    for (int j = start; j <= end; j++)
                    {
                        sb.Append($"{bytesA[j]:X2} ");
                        if (j == i) sb.Append("* ");
                    }
                    
                    sb.AppendLine();
                    sb.AppendLine($"BytesB from {start} to {end}:");
                    
                    for (int j = start; j <= end; j++)
                    {
                        sb.Append($"{bytesB[j]:X2} ");
                        if (j == i) sb.Append("* ");
                    }
                    
                    Debug.WriteLine(sb.ToString());
                    return false;
                }
            }
            
            Debug.WriteLine("SerializedEquals: Byte arrays are identical");
            return true;
        }
        
        /// <summary>
        /// Creates a deep clone of an object through serialization
        /// </summary>
        /// <typeparam name="T">The type of object to clone</typeparam>
        /// <param name="original">The object to clone</param>
        /// <returns>A deep clone of the original object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? DeepClone<T>(this T? original)
            where T : class, IYoloSerializable
        {
            if (original == null)
                return null;
                
            // Calculate size and rent buffer
            int size = GeneratedSerializerEntry.GetSerializedSize(original);
            byte[] buffer = SerializationBufferPool.Rent(size);
            
            try
            {
                // Serialize
                int offset = 0;
                GeneratedSerializerEntry.Serialize(original, buffer, ref offset);
                
                // Reset offset for deserialization
                offset = 0;
                
                // Deserialize
                return GeneratedSerializerEntry.Deserialize<T>(buffer, ref offset);
            }
            finally
            {
                SerializationBufferPool.Return(buffer);
            }
        }
    }
} 