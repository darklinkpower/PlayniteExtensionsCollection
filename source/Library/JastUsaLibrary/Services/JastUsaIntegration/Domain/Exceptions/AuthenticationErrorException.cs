using JastUsaLibrary.JastUsaIntegration.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Exceptions
{
    public class AuthenticationErrorException : Exception
    {
        public HttpStatusCode? StatusCode { get; }

        public AuthenticationErrorException(AuthenticationTokenRequest authenticationTokenRequest, HttpStatusCode? statusCode = null)
            : base(GenerateMessage(authenticationTokenRequest, statusCode))
        {
            StatusCode = statusCode;
        }

        private static string GenerateMessage(AuthenticationTokenRequest authenticationTokenRequest, HttpStatusCode? statusCode)
        {
            var message = $"Error during authentication. Email: \"{new string('*', authenticationTokenRequest.Email.Length)}\" Password: \"{new string('*', authenticationTokenRequest.Password.Length)}\"";

            if (statusCode.HasValue)
            {
                message += $" Status Code: {statusCode.Value}";
            }

            return message;
        }
    }
}
