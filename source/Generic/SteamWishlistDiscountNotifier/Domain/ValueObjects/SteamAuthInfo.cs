using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Domain.ValueObjects
{
    public class SteamAuthInfo
    {
        public string SteamId { get; }
        public string Token { get; }
        public DateTime TokenExpirationDate { get; }

        public SteamAuthInfo(string steamId, string jwt, DateTime tokenExpirationDate)
        {
            SteamId = steamId;
            Token = jwt;
            TokenExpirationDate = tokenExpirationDate;
        }
    }
}