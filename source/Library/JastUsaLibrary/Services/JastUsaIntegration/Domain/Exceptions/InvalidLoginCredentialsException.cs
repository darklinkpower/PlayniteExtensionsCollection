using JastUsaLibrary.JastUsaIntegration.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Exceptions
{
    public class InvalidLoginCredentialsException : Exception
    {
        public InvalidLoginCredentialsException(AuthenticationTokenRequest authenticationTokenRequest)
            : base($"Invalid login credentials . Email: \"{new string('*', authenticationTokenRequest.Email.Length)}\" Password: \"{new string('*', authenticationTokenRequest.Password.Length)}\"")
        {

        }
    }
}
