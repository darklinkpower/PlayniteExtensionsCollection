using FlowHttp.Constants;
using Playnite.SDK;
using Playnite.SDK.Data;
using SteamWishlistDiscountNotifier.Domain.Interfaces;
using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.UserAccount
{
    public class SteamUserAccountService
    {
        private const string _postGetClientWalletDetails = @"https://api.steampowered.com/IUserAccountService/GetClientWalletDetails/v1/?access_token={0}&include_formatted_balance=true";
        private readonly ISteamJwtTokenService _steamJwtTokenService;
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;

        public SteamUserAccountService(ISteamJwtTokenService steamJwtTokenService, IPlayniteAPI playniteApi, ILogger logger)
        {
            _steamJwtTokenService = steamJwtTokenService;
            _playniteApi = playniteApi;
            _logger = logger;
        }

        public SteamWalletDetails GetClientWalletDetails(CancellationToken cancellationToken = default)
        {
            var jwtToken = _steamJwtTokenService.GetJwtToken();
            if (jwtToken is null)
            {
                return null;
            }

            var valueKeys = new Dictionary<string, string>
            {
                ["access_token"] = jwtToken.Token,
                ["include_formatted_balance"] = "true"
            };

            var requestUrl = string.Format(_postGetClientWalletDetails, jwtToken.Token);
            var httpRequest = FlowHttp.HttpRequestFactory.GetHttpRequest()
                .WithUrl(requestUrl)
                .WithPostHttpMethod()
                .WithContent(valueKeys, HttpContentTypes.FormUrlEncoded, Encoding.UTF8);
            var requestResult = httpRequest.DownloadString(cancellationToken: cancellationToken);
            if (!requestResult.IsSuccess)
            {
                return null;
            }

            var response = Serialization.FromJson<GetClientWalletDetailsResponseDto>(requestResult.Content);
            return new SteamWalletDetails(
                response.Response.HasWallet,
                response.Response.UserCountryCode,
                response.Response.WalletCountryCode,
                response.Response.WalletState,
                response.Response.Balance,
                response.Response.DelayedBalance,
                response.Response.CurrencyCode,
                response.Response.TimeMostRecentTxn,
                response.Response.MostRecentTxnid,
                response.Response.HasWalletInOtherRegions,
                response.Response.FormattedBalance,
                response.Response.FormattedDelayedBalance
            );
        }

    }
}