using FlowHttp;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.Tags
{
    public class SteamTagsService
    {
        private const string GetTagsListUrl = @"https://api.steampowered.com/IStoreService/GetTagList/v1/?language={0}";
        public SteamTagsService()
        {

        }

        public List<Tag> GetTagsList(string steamLanguague = "english", CancellationToken cancellationToken = default)
        {
            var url = string.Format(GetTagsListUrl, steamLanguague);
            var requestResult = HttpRequestFactory.GetHttpRequest().WithUrl(url).DownloadString(cancellationToken);
            if (requestResult.IsSuccess && Serialization.TryFromJson<GetTagListResponseDto>(requestResult.Content, out var response))
            {
                return response.Response.Tags;
            }

            return new List<Tag>();
        }
    }
}