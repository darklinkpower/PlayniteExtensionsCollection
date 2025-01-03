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

        public AuthenticationErrorException(string email, string password, HttpStatusCode? statusCode = null)
            : base(GenerateMessage(email, password, statusCode))
        {
            StatusCode = statusCode;
        }

        private static string GenerateMessage(string email, string password, HttpStatusCode? statusCode)
        {
            var message = $"Error during authentication. Email: \"{new string('*', email.Length)}\" Password: \"{new string('*', password.Length)}\"";

            if (statusCode.HasValue)
            {
                message += $" Status Code: {statusCode.Value}";
            }

            return message;
        }
    }
}
