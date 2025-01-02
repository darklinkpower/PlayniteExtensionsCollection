using JastUsaLibrary.JastUsaIntegration.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Application.Interfaces
{
    public interface IAuthenticationPersistence
    {
        AuthenticationTokenRequest LoadAuthentication();
        bool SaveAuthentication(AuthenticationTokenRequest authentication);
        void DeleteAuthentication();
    }
}