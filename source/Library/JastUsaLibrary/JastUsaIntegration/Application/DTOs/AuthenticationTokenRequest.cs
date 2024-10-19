using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Application.DTOs
{
    public class AuthenticationTokenRequest
    {
        public string Email { get; }
        public string Password { get; }

        public AuthenticationTokenRequest(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}