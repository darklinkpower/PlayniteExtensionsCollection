using Playnite.SDK;
using ProtoBuf;
using SteamWishlistDiscountNotifier.Domain.Interfaces;
using SteamWishlistDiscountNotifier.SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.Wishlist
{
    public class SteamWishlistService
    {
        private readonly ISteamJwtTokenService _steamJwtTokenService;
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;
        private const string _wishlistGetUrlTemplate = @"https://api.steampowered.com/IWishlistService/GetWishlistSortedFiltered/v1?access_token={0}&origin=https://store.steampowered.com&input_protobuf_encoded={1}==";

        public SteamWishlistService(ISteamJwtTokenService steamJwtTokenService, IPlayniteAPI playniteApi, ILogger logger)
        {
            _steamJwtTokenService = steamJwtTokenService;
            _playniteApi = playniteApi;
            _logger = logger;
        }

        public List<CWishlistGetWishlistSortedFilteredResponseWishlistItem> GetWishlist(
            string countryCode,
            ulong? steamId = null,
            string language = "english",
            StoreBrowseItemDataRequest dataRequest = null,
            CWishlistFilters filters = null,
            CancellationToken cancellationToken = default)
        {
            var jwtToken = _steamJwtTokenService.GetJwtToken();
            if (jwtToken is null)
            {
                return new List<CWishlistGetWishlistSortedFilteredResponseWishlistItem>();
            }

            var allWishlistItems = new List<CWishlistGetWishlistSortedFilteredResponseWishlistItem>();
            var startIndex = 0;
            const int pageSize = 2000;

            while (true)
            {
                var requestData = new CWishlistGetWishlistSortedFilteredRequest
                {
                    Steamid = steamId ?? ulong.Parse(jwtToken.SteamId),
                    Context = new StoreBrowseContext()
                    {
                        CountryCode = countryCode,
                        Language = language,
                        SteamRealm = 1
                    },
                    DataRequest = dataRequest ?? new StoreBrowseItemDataRequest()
                    {
                        IncludeAssets = true,
                        IncludeRelease = true,
                        IncludePlatforms = true,
                        IncludeTagCount = 20,
                        IncludeReviews = true
                    },
                    Filters = filters ?? new CWishlistFilters(),
                    StartIndex = startIndex,
                    PageSize = pageSize
                };

                var serializedRequest = ProtobufUtilities.SerializeRequest(requestData);
                var serializedRequestBase64 = Convert.ToBase64String(serializedRequest);
                var requestUrl = string.Format(_wishlistGetUrlTemplate, jwtToken.Token, serializedRequestBase64);

                var httpRequest = FlowHttp.HttpRequestFactory.GetHttpRequest().WithUrl(requestUrl);
                var requestResult = httpRequest.DownloadBytes(cancellationToken: cancellationToken);
                if (!requestResult.IsSuccess)
                {
                    return new List<CWishlistGetWishlistSortedFilteredResponseWishlistItem>();
                }

                var response = ProtobufUtilities.DeserializeResponse<CWishlistGetWishlistSortedFilteredResponse>(requestResult.Content);
                allWishlistItems.AddRange(response.Items);
                if (response.Items.Count < pageSize)
                {
                    break;
                }

                startIndex += pageSize;
            }

            return allWishlistItems;
        }


    }
}
