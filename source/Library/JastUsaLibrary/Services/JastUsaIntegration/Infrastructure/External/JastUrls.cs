using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary
{
    public static class JastUrls
    {
        public static class Api
        {
            private const string BaseApiUrl = "https://app.jastusa.com/api/v2/";
            private const string LocaleCode = "en_US";

            public static class Authentication
            {
                private const string RouteBase = "shop/";
                public static string AuthenticationToken => $"{BaseApiUrl}{RouteBase}authentication-token";
                public static string TokenRefresh(string refreshToken) => $"{BaseApiUrl}{RouteBase}authentication-refresh?refresh_token={refreshToken}";
            }

            public static class Account
            {
                private const string RouteBase = "shop/account/";
                public static string GetGames(int page) => $"{BaseApiUrl}{RouteBase}user-games-dev?localeCode={LocaleCode}&phrase=&page={page}&itemsPerPage=1000";
                public static string GenerateLink => $"{BaseApiUrl}{RouteBase}user-games/generate-link";
            }
        }
    }

}