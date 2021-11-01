using Playnite.SDK;
using Playnite.SDK.Models;
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
            return Path.Combine(GetExtraMetadataDirectory(game, true), "Logo.png");
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
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.Delete(directoryPath, true);
                    return true;
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error while deleting removed extra metadata directory {directoryPath}");
                    return false;
                }
            }
            return true;
        }

        public bool DeleteGameLogo(Game game)
        {
            var logoPath = Path.Combine(GetExtraMetadataDirectory(game), "Logo.png");
            if (File.Exists(logoPath))
            {
                try
                {
                    File.Delete(logoPath);
                    return true;
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error while deleting game logo {logoPath}");
                    return false;
                }
            }
            return true;
        }
    }
}
