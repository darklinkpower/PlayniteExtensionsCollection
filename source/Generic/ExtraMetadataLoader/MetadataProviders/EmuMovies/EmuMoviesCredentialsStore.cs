using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace ExtraMetadataLoader.MetadataProviders
{
    internal class EmuMoviesCredentialsStore
    {
        private readonly string _credentialsPath;
        private readonly ILogger _logger;

        public EmuMoviesCredentialsStore(string pluginUserDataPath, ILogger logger)
        {
            var credentialsDirectory = Path.Combine(pluginUserDataPath, "EmuMovies");
            Directory.CreateDirectory(credentialsDirectory);
            _credentialsPath = Path.Combine(credentialsDirectory, "credentials.dat");
            _logger = logger;
        }

        public EmuMoviesCredentials Load()
        {
            if (!FileSystem.FileExists(_credentialsPath))
            {
                return null;
            }

            try
            {
                var decryptedJson = Encryption.DecryptFromFile(_credentialsPath, Encoding.UTF8, GetEncryptionKey());
                var credentials = Serialization.FromJson<EmuMoviesCredentials>(decryptedJson);
                return credentials?.IsConfigured == true ? credentials : null;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to load EmuMovies credentials.");
                FileSystem.DeleteFileSafe(_credentialsPath);
                return null;
            }
        }

        public bool Save(EmuMoviesCredentials credentials)
        {
            if (credentials?.IsConfigured != true)
            {
                return false;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_credentialsPath));
                Encryption.EncryptToFile(
                    _credentialsPath,
                    Serialization.ToJson(credentials),
                    Encoding.UTF8,
                    GetEncryptionKey());
                _logger.Debug("EmuMovies credentials saved successfully.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to save EmuMovies credentials.");
                FileSystem.DeleteFileSafe(_credentialsPath);
                return false;
            }
        }

        public void Clear()
        {
            if (FileSystem.FileExists(_credentialsPath))
            {
                FileSystem.DeleteFileSafe(_credentialsPath);
            }
        }

        public bool HasCredentials()
        {
            return Load()?.IsConfigured == true;
        }

        private static string GetEncryptionKey()
        {
            return WindowsIdentity.GetCurrent()?.User?.Value ?? Environment.UserName;
        }
    }
}
