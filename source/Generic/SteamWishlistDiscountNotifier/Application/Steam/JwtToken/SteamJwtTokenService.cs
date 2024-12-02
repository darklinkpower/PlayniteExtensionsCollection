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

        public SteamAuthInfo GetJwtToken()
        {
            if (!IsTokenValid())
            {
                _jwtToken = RetrieveNewToken();
                if (!_jwtToken.IsNullOrEmpty())
                {
                    var payload = DecodeJwtPayload(_jwtToken);
                    _tokenExpiration = DateTimeOffset.FromUnixTimeSeconds(payload.Exp).DateTime;
                    _steamAuthInfo = new SteamAuthInfo(payload.Sub, _jwtToken, _tokenExpiration);
                    foreach (var callback in _onUserLoggedInCallbacks)
                    {
                        try
                        {
                            callback?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Error occurred while invoking user logged-in callback.");
                        }
                    }
                }
                else
                {
                    foreach (var callback in _onUserNotLoggedInCallbacks)
                    {
                        try
                        {
                            callback?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Error occurred while invoking user not logged-in callback.");
                        }
                    }
                }
            }

            return _steamAuthInfo;
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

        private string RetrieveNewToken()
        {
            using (var webView = _playniteApi.WebViews.CreateOffscreenView())
            {
                webView.NavigateAndWait(@"https://store.steampowered.com/account/?l=english");
                var address = webView.GetCurrentAddress();
                if (address.StartsWith(@"https://store.steampowered.com/account/"))
                {
                    var source = webView.GetPageSource();
                    var regeMatch = Regex.Match(source, @"webapi_token&quot;:&quot;(.*?)(?=&quot;)");
                    if (regeMatch.Success)
                    {
                        return regeMatch.Groups[1].Value;
                    }
                }
            }

            return null;
        }

        private static SteamJwtTokenPayload DecodeJwtPayload(string jwt)
        {
            var parts = jwt.Split('.');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid JWT format.");
            }

            var payload = parts[1];
            var jsonPayload = payload.Base64Decode();
            var token = Serialization.FromJson<SteamJwtTokenPayload>(jsonPayload);
            return token;
        }
    }

}