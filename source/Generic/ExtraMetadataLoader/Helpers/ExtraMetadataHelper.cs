using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SteamCommon;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Helpers
{
    public static class ExtraMetadataHelper
    {
        private static readonly IPlayniteAPI _playniteApi;
        private static readonly ILogger _logger = LogManager.GetLogger();
        private static readonly string _baseDirectory;

        static ExtraMetadataHelper()
        {
            _playniteApi = API.Instance;
            _logger = LogManager.GetLogger();
            _baseDirectory = FileSystem.FixPathLength(Path.Combine(_playniteApi.Paths.ConfigurationPath, "ExtraMetadata"));
        }

        public static string GetExtraMetadataDirectory(Game game, bool createDirectory = false)
        {
            var directory = Path.Combine(_baseDirectory, "games", game.Id.ToString());
            if (createDirectory && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return directory;
        }

        public static string GetGameLogoPath(Game game, bool createDirectory = false)
        {
            return Path.Combine(GetExtraMetadataDirectory(game, createDirectory), "Logo.png");
        }

        public static string GetGameVideoPath(Game game, bool createDirectory = false)
        {
            return Path.Combine(GetExtraMetadataDirectory(game, createDirectory), "VideoTrailer.mp4");
        }

        public static string GetGameVideoMicroPath(Game game, bool createDirectory = false)
        {
            return Path.Combine(GetExtraMetadataDirectory(game, createDirectory), "VideoMicrotrailer.mp4");
        }

        public static bool DeleteExtraMetadataDir(Game game)
        {
            return DeleteDirectory(GetExtraMetadataDirectory(game));
        }

        public static bool DeleteExtraMetadataDir(Platform platform)
        {
            return DeleteDirectory(Path.Combine(_baseDirectory, "platforms", platform.Id.ToString()));
        }

        public static bool DeleteExtraMetadataDir(GameSource source)
        {
            return DeleteDirectory(Path.Combine(_baseDirectory, "sources", source.Id.ToString()));
        }

        private static bool DeleteDirectory(string directoryPath)
        {

            try
            {
                if (Directory.Exists(directoryPath))
                { 
                    Directory.Delete(directoryPath, true);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while deleting removed Extra Metadata directory {directoryPath}");
                _playniteApi.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageErrorDeletingDirectory"), directoryPath, e.Message),
                    NotificationType.Error)
                    );
                return false;
            }
        }

        public static bool DeleteGameLogo(Game game)
        {
            return DeleteFile(GetGameLogoPath(game));
        }

        public static bool DeleteGameVideo(Game game)
        {
            return DeleteFile(GetGameVideoPath(game));
        }

        public static bool DeleteGameVideoMicro(Game game)
        {
            return DeleteFile(GetGameVideoMicroPath(game));
        }

        public static bool DeleteFile(string sourcePath)
        {
            try
            {
                if (FileSystem.FileExists(sourcePath))
                {
                    File.Delete(FileSystem.FixPathLength(sourcePath));
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error deleting file: {sourcePath}.");
                _playniteApi.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageErrorDeletingFile"), sourcePath, e.Message),
                    NotificationType.Error)
                    );
                return false;
            }
        }

        public static string ExpandVariables(string inputString)
        {
            return _playniteApi.ExpandGameVariables(new Game(), inputString);
        }

        public static string ExpandVariables(Game game, string inputString)
        {
            return _playniteApi.ExpandGameVariables(game, inputString);
        }
    }
}
