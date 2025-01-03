using JastUsaLibrary.JastUsaIntegration.Application.Interfaces;
using JastUsaLibrary.Services.JastUsaIntegration.Domain.ValueObjects;
using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary.JastUsaIntegration.Infrastructure.Persistence
{
    public class AuthenticationPersistence : IAuthenticationPersistence
    {
        private readonly string _authenticationPath;
        private readonly ILogger _logger = LogManager.GetLogger();

        public AuthenticationPersistence(string authenticationDirectory)
        {
            _authenticationPath = Path.Combine(authenticationDirectory, "authentication.json");
        }

        public AuthenticationCredentials LoadAuthentication()
        {
            if (!FileSystem.FileExists(_authenticationPath))
            {
                return null;
            }

            try
            {
                return Serialization.FromJson<AuthenticationCredentials>(
                    Encryption.DecryptFromFile(_authenticationPath, Encoding.UTF8, WindowsIdentity.GetCurrent().User.Value));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to load authentication.");
                FileSystem.DeleteFileSafe(_authenticationPath);
                return null;
            }
        }

        public bool SaveAuthentication(AuthenticationCredentials authentication)
        {
            try
            {
                var serializedJson = Serialization.ToJson(authentication);
                Encryption.EncryptToFile(
                    _authenticationPath,
                    serializedJson,
                    Encoding.UTF8,
                    WindowsIdentity.GetCurrent().User.Value);
                _logger.Debug("Authentication saved successfully.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to save authentication.");
                FileSystem.DeleteFileSafe(_authenticationPath);
                return false;
            }
        }

        public void DeleteAuthentication()
        {
            if (FileSystem.FileExists(_authenticationPath))
            {
                FileSystem.DeleteFileSafe(_authenticationPath);
            }        
        }
    }
}