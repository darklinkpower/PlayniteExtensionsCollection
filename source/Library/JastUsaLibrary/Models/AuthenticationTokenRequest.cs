using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.Models
{
    public class AuthenticationTokenRequest
    {
        public string email;
        public string password;

        public AuthenticationTokenRequest(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }
}