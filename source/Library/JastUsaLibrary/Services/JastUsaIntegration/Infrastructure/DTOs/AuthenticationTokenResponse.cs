using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Infrastructure.DTOs
{
    public class AuthenticationTokenResponse
    {
        [SerializationPropertyName("token")]
        public string Token { get; set; }

        [SerializationPropertyName("customer")]
        public string Customer { get; set; }

        [SerializationPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        public AuthenticationTokenResponse()
        {

        }

        public AuthenticationTokenResponse(string token, string customer, string refreshToken)
        {
            Token = token;
            Customer = customer;
            RefreshToken = refreshToken;
        }

    }
}