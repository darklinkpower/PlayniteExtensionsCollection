using JastUsaLibrary.JastUsaIntegration.Application.DTOs;
using JastUsaLibrary.JastUsaIntegration.Application.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
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

        public AuthenticationPersistence(string authenticationPath)
        {
            _authenticationPath = authenticationPath;
        }

        public AuthenticationTokenRequest LoadAuthentication()
        {
            if (!FileSystem.FileExists(_authenticationPath))
            {
                return null;
            }

            try
            {
                return Serialization.FromJson<AuthenticationTokenRequest>(
                    Encryption.DecryptFromFile(_authenticationPath, Encoding.UTF8, WindowsIdentity.GetCurrent().User.Value));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to load authentication.");
                FileSystem.DeleteFileSafe(_authenticationPath);
                return null;
            }
        }

        public bool SaveAuthentication(AuthenticationTokenRequest authentication)
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