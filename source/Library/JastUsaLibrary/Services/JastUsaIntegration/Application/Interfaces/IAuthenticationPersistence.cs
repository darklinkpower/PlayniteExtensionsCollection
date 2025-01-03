using JastUsaLibrary.Services.JastUsaIntegration.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Application.Interfaces
{
    public interface IAuthenticationPersistence
    {
        AuthenticationCredentials LoadAuthentication();
        bool SaveAuthentication(AuthenticationCredentials authentication);
        void DeleteAuthentication();
    }
}