using Playnite.SDK;
using Playnite.SDK.Data;
using SteamWishlistDiscountNotifier.Domain.Interfaces;
using SteamWishlistDiscountNotifier.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.JwtToken
{
    public class SteamJwtTokenService : ISteamJwtTokenService
    {
        private string _jwtToken;
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;
        private DateTime _tokenExpiration;
        private SteamAuthInfo _steamAuthInfo;

        private readonly List<Action> _onUserLoggedInCallbacks = new List<Action>();
        private readonly List<Action> _onUserNotLoggedInCallbacks = new List<Action>();

        public SteamJwtTokenService(IPlayniteAPI playniteApi, ILogger logger)
        {
            _playniteApi = playniteApi;
            _logger = logger;
        }

        public void AddUserNotLoggedInCallback(Action callback)
        {
            if (callback != null && !_onUserNotLoggedInCallbacks.Contains(callback))
            {
                _onUserNotLoggedInCallbacks.Add(callback);
            }
        }

        public void RemoveUserNotLoggedInCallback(Action callback)
        {
            if (callback != null)
            {
                _onUserNotLoggedInCallbacks.Remove(callback);
            }
        }

        public void AddUserLoggedInCallback(Action callback)
        {
            if (callback != null && !_onUserNotLoggedInCallbacks.Contains(callback))
            {
                _onUserLoggedInCallbacks.Add(callback);
            }
        }

        public void RemoveUserLoggedInCallback(Action callback)
        {
            if (callback != null)
            {
                _onUserLoggedInCallbacks.Remove(callback);
            }
        }

        private void InvokeCallbacks(List<Action> callbacks)
        {
            foreach (var callback in callbacks)
            {
                try
                {
                    callback?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Error occurred while invoking callbacks.");
                }
            }
        }

        public SteamAuthInfo GetJwtToken()
        {
            if (IsTokenValid())
            {
                return _steamAuthInfo;
            }

            var result = RetrieveNewToken();
            switch (result.Status)
            {
                case TokenRetrievalStatus.NoInternet:
                    return null;
                case TokenRetrievalStatus.Success:
                    {
                        var payload = DecodeJwtPayload(result.Token);
                        if (payload is null)
                        {
                            _logger.Error("Invalid JWT payload.");
                            InvalidateToken();
                            return null;
                        }

                        _tokenExpiration = DateTimeOffset
                            .FromUnixTimeSeconds(payload.Exp)
                            .DateTime;

                        _steamAuthInfo = new SteamAuthInfo(payload.Sub, result.Token, _tokenExpiration);

                        InvokeCallbacks(_onUserLoggedInCallbacks);
                        return _steamAuthInfo;
                    }
                    case TokenRetrievalStatus.NotLoggedIn:
                        InvokeCallbacks(_onUserNotLoggedInCallbacks);
                        return null;
                default:
                    return null;
            }
        }

        public void InvalidateToken()
        {
            _jwtToken = null;
            _steamAuthInfo = null;
            _tokenExpiration = DateTime.MinValue;
        }

        public bool IsTokenValid()
        {
            return !string.IsNullOrEmpty(_jwtToken) && _tokenExpiration > DateTime.UtcNow;
        }

        private TokenRetrievalResult RetrieveNewToken()
        {
            using (var webView = _playniteApi.WebViews.CreateOffscreenView())
            {
                webView.NavigateAndWait(@"https://store.steampowered.com/account/?l=english");
                var address = webView.GetCurrentAddress();
                if (address.IsNullOrEmpty()) // Adress is empty when there's no internet connection, so we can use this to detect that specific case
                {
                    return TokenRetrievalResult.Fail(TokenRetrievalStatus.NoInternet);
                }

                if (!address.StartsWith(@"https://store.steampowered.com/account/"))
                {
                    _logger.Error($"Unexpected URL during JWT retrieval: {address}");
                    return TokenRetrievalResult.Fail(TokenRetrievalStatus.NotLoggedIn); // If the user is not logged in, Steam redirects to the login page, which has a different URL. So we can use this to detect that specific case.
                }

                var source = webView.GetPageSource();
                var match = Regex.Match(source, @"webapi_token&quot;:&quot;(.*?)(?=&quot;)");
                if (!match.Success)
                {
                    _logger.Error($"JWT token not found in page source: {address}");
                    return TokenRetrievalResult.Fail(TokenRetrievalStatus.ParseFailure);
                }

                return TokenRetrievalResult.Success(match.Groups[1].Value);
            }
        }

        private SteamJwtTokenPayload DecodeJwtPayload(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3)
            {
                _logger.Error($"Invalid JWT format. Total parts: {parts.Length}, Expected 3.");
                return null;
            }

            string jsonPayload;
            try
            {
                var payload = parts[1];
                jsonPayload = payload.Base64Decode();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to decode JWT payload.");
                return null;
            }

            if (Serialization.TryFromJson<SteamJwtTokenPayload>(jsonPayload, out var token, out var ex))
            {
                return token;
            }
            else
            {
                _logger.Error(ex, "Failed to deserialize JWT payload.");
                return null;
            }
        }
    }

}