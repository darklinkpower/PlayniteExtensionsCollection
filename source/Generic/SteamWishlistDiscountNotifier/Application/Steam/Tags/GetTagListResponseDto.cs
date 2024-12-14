using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.Tags
{
    public class GetTagListResponseDto
    {
        [SerializationPropertyName("response")]
        public Response Response { get; set; }
    }

    public class Response
    {
        [SerializationPropertyName("version_hash")]
        public string VersionHash { get; set; }

        [SerializationPropertyName("tags")]
        public List<Tag> Tags { get; set; }
    }

    public class Tag
    {
        [SerializationPropertyName("tagid")]
        public uint Tagid { get; set; }

        [SerializationPropertyName("name")]
        public string Name { get; set; }
    }
}
