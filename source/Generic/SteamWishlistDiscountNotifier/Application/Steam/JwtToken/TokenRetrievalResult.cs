using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWishlistDiscountNotifier.Application.Steam.JwtToken
{
    public sealed class TokenRetrievalResult
    {
        public TokenRetrievalStatus Status { get; }
        public string Token { get; }

        public bool IsSuccess => Status == TokenRetrievalStatus.Success;

        private TokenRetrievalResult(TokenRetrievalStatus status, string token = null)
        {
            Status = status;
            Token = token;
        }

        public static TokenRetrievalResult Success(string token)
            => new TokenRetrievalResult(TokenRetrievalStatus.Success, token);

        public static TokenRetrievalResult Fail(TokenRetrievalStatus status)
            => new TokenRetrievalResult(status);
    }
}
