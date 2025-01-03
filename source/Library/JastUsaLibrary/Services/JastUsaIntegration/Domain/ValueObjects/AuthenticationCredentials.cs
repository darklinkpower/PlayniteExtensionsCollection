using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Services.JastUsaIntegration.Domain.ValueObjects
{
    public class AuthenticationCredentials
    {
        [SerializationPropertyName("email")]
        public string Email { get; }
        [SerializationPropertyName("password")]
        public string Password { get; }
        [SerializationPropertyName("remember_me")]
        public bool RememberMe { get; }

        public AuthenticationCredentials(string email, string password, bool rememberMe)
        {
            Email = email;
            Password = password;
            RememberMe = rememberMe;
        }
    }
}