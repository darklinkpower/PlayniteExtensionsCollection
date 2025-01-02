using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Application.DTOs
{
    public class AuthenticationTokenRequest
    {
        [SerializationPropertyName("email")]
        public string Email { get; }
        [SerializationPropertyName("password")]
        public string Password { get; }
        [SerializationPropertyName("remember_me")]
        public int RememberMe { get; }

        public AuthenticationTokenRequest(string email, string password, bool rememberMe)
        {
            Email = email;
            Password = password;
            RememberMe = rememberMe ? 1 : 0;
        }
    }
}