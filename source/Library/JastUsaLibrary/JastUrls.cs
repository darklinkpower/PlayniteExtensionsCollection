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
            private static string BaseApi = $"https://app.jastusa.com/api/v2/";
            public static class Authentication
            {
                private const string PathBase = "shop/";
                public static string AuthenticationToken => BaseApi + PathBase + "authentication-token";
                public static string TokenRefreshTemplate => BaseApi + PathBase + "authentication-refresh?refresh_token={0}";
            }

            public static class Account
            {
                private const string PathBase = "shop/account/";
                public static string GetGamesTemplate => BaseApi + PathBase + "user-games-dev?localeCode=en_US&phrase=&page={0}&itemsPerPage=1000";
                public static string GenerateLink => BaseApi + PathBase + "user-games/generate-link";
            }
        }

        public static class Web
        {
            public const string JastBaseAppUrl = @"https://app.jastusa.com";
            public const string MyAccountPage = "https://jastusa.com/my-account";
            public const string JastMediaUrlTemplate = @"https://app.jastusa.com/media/image/{0}";
        }
    }

}