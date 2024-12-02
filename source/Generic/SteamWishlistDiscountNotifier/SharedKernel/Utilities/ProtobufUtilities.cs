using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.SharedKernel.Utilities
{
    public static class ProtobufUtilities
    {
        public static byte[] SerializeRequest<T>(T requestData)
        {
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, requestData);
                return memoryStream.ToArray();
            }
        }

        public static T DeserializeResponse<T>(byte[] responseBytes)
        {
            using (var memoryStream = new MemoryStream(responseBytes))
            {
                return Serializer.Deserialize<T>(memoryStream);
            }
        }
    }
}
