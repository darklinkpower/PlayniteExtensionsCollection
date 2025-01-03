using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Domain.Exceptions
{
    public class InvalidLoginCredentialsException : Exception
    {
        public InvalidLoginCredentialsException(string email, string password)
            : base($"Invalid login credentials . Email: \"{new string('*', email.Length)}\" Password: \"{new string('*', password.Length)}\"")
        {

        }
    }
}
