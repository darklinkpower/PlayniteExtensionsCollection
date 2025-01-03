using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Domain.ValueObjects
{
    public class AuthenticationToken
    {
        public string Token { get; }
        public string CustomerId { get; }
        public string RefreshToken { get; }

        public AuthenticationToken(string token, string customerId, string refreshToken)
        {
            Token = token;
            CustomerId = customerId;
            RefreshToken = refreshToken;
        }
    }
}
