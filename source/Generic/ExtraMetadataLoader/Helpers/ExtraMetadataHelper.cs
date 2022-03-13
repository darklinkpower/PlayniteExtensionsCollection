using ExtraMetadataLoader.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtraMetadataLoader.Helpers
{
    public class ExtraMetadataHelper
    {
        private readonly IPlayniteAPI playniteApi;
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly string baseDirectory;

        public ExtraMetadataHelper(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
            baseDirectory = Path.Combine(playniteApi.Paths.ConfigurationPath, "ExtraMetadata");
        }

        public bool IsGamePcGame(Game game)
        {
            if (game.Platforms != null &&
                game.Platforms.Any(p => p.SpecificationId == "pc_windows"))
            {
                return true;
            }

            return false;
        }

        public string GetExtraMetadataDirectory(Game game, bool createDirectory = false)
        {
            var directory = Path.Combine(baseDirectory, "games", game.Id.ToString());
            if (createDirectory && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }

        public string GetGameLogoPath(Game game, bool createDirectory = false)
        {
            return Path.Combine(GetExtraMetadataDirectory(game, createDirectory), "Logo.png");
        }

        public string GetGameVideoPath(Game game, bool createDirectory = false)
        {
            return Path.Combine(GetExtraMetadataDirectory(game, createDirectory), "VideoTrailer.mp4");
        }

        public string GetGameVideoMicroPath(Game game, bool createDirectory = false)
        {
            return Path.Combine(GetExtraMetadataDirectory(game, createDirectory), "VideoMicrotrailer.mp4");
        }

        public bool DeleteExtraMetadataDir(Game game)
        {
            return DeleteDirectory(GetExtraMetadataDirectory(game));
        }

        public bool DeleteExtraMetadataDir(Platform platform)
        {
            return DeleteDirectory(Path.Combine(baseDirectory, "platforms", platform.Id.ToString()));
        }

        public bool DeleteExtraMetadataDir(GameSource source)
        {
            return DeleteDirectory(Path.Combine(baseDirectory, "sources", source.Id.ToString()));
        }

        private bool DeleteDirectory(string directoryPath)
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
                logger.Error(e, $"Error while deleting removed extra metadata directory {directoryPath}");
                playniteApi.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageErrorDeletingDirectory"), directoryPath, e.Message),
                    NotificationType.Error)
                    );
                return false;
            }
        }

        public bool DeleteGameLogo(Game game)
        {
            return DeleteFile(GetGameLogoPath(game));
        }

        public bool DeleteGameVideo(Game game)
        {
            return DeleteFile(GetGameVideoPath(game));
        }

        public bool DeleteGameVideoMicro(Game game)
        {
            return DeleteFile(GetGameVideoMicroPath(game));
        }

        public string GetSteamIdFromSearch(Game game, bool isBackgroundDownload)
        {
            var normalizedName = game.Name.NormalizeGameName();
            var results = SteamCommon.GetSteamSearchResults(normalizedName);
            results.ForEach(a => a.Name = a.Name.NormalizeGameName());

            // Try to see if there's an exact match, to not prompt the user unless needed
            var matchingGameName = normalizedName.GetMatchModifiedName();
            var exactMatch = results.FirstOrDefault(x => x.Name.GetMatchModifiedName() == matchingGameName);
            if (exactMatch != null)
            {
                return exactMatch.GameId;
            }
            else if (!isBackgroundDownload)
            {
                var selectedGame = playniteApi.Dialogs.ChooseItemWithSearch(
                    results.Select(x => new GenericItemOption(x.Name, x.GameId)).ToList(),
                    (a) => SteamCommon.GetSteamSearchGenericItemOptions(a),
                    normalizedName,
                    ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectGame"));
                if (selectedGame != null)
                {
                    return selectedGame.Description;
                }
            }

            return string.Empty;
        }

        public bool DeleteFile(string sourcePath)
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
                logger.Error(e, $"Error deleting file: {sourcePath}.");
                playniteApi.Notifications.Add(new NotificationMessage(
                    Guid.NewGuid().ToString(),
                    string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageErrorDeletingFile"), sourcePath, e.Message),
                    NotificationType.Error)
                    );
                return false;
            }
        }


    }
}
