using JastUsaLibrary.JastUsaIntegration.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Exceptions
{
    public class AuthenticationErrorException : Exception
    {
        public AuthenticationErrorException(AuthenticationTokenRequest authenticationTokenRequest)
            : base($"Error during authentication . Email: \"{new string('*', authenticationTokenRequest.Email.Length)}\" Password: \"{new string('*', authenticationTokenRequest.Password.Length)}\"")
        {

        }
    }
}
