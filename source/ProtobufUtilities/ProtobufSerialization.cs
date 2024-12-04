using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtobufUtilities
{
    public static class ProtobufSerialization
    {
        public static byte[] Serialize<T>(T data)
        {
            using (var memoryStream = SerializeToStream(data))
            {
                return memoryStream.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] dataBytes)
        {
            using (var memoryStream = new MemoryStream(dataBytes))
            {
                return DeserializeFromStream<T>(memoryStream);
            }
        }

        public static async Task<byte[]> SerializeAsync<T>(T data)
        {
            using (var memoryStream = SerializeToStream(data))
            {
                return await Task.FromResult(memoryStream.ToArray());
            }
        }

        public static async Task<T> DeserializeAsync<T>(byte[] dataBytes)
        {
            using (var memoryStream = new MemoryStream(dataBytes))
            {
                return await Task.FromResult(DeserializeFromStream<T>(memoryStream));
            }
        }

        public static void SerializeToFile<T>(T data, string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                using (var memoryStream = SerializeToStream(data))
                {
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        public static T DeserializeFromFile<T>(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return DeserializeFromStream<T>(fileStream);
            }
        }

        public static bool TryDeserialize<T>(byte[] dataBytes, out T result, out Exception exception)
        {
            result = default;
            exception = null;

            try
            {
                result = Deserialize<T>(dataBytes);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool TryDeserializeFile<T>(string filePath, out T result, out Exception exception)
        {
            result = default;
            exception = null;

            try
            {
                result = DeserializeFromFile<T>(filePath);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool TrySerialize<T>(T data, out byte[] result, out Exception exception)
        {
            result = null;
            exception = null;

            try
            {
                result = Serialize<T>(data);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool TrySerializeToFile<T>(T data, string filePath, out Exception exception)
        {
            exception = null;

            try
            {
                SerializeToFile(data, filePath);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        private static MemoryStream SerializeToStream<T>(T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, data);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private static T DeserializeFromStream<T>(Stream stream)
        {
            if (stream == null || stream.Length == 0)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return Serializer.Deserialize<T>(stream);
        }
    }


}
