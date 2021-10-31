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

        public ExtraMetadataHelper(IPlayniteAPI playniteApi)
        {
            this.playniteApi = playniteApi;
        }

        public string GetExtraMetadataDirectory(Game game, bool createDirectory = false)
        {
            var directory = Path.Combine(playniteApi.Paths.ConfigurationPath, "ExtraMetadata", "games", game.Id.ToString());
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

        public bool DeleteGameExtraMetadataDir(Game game)
        {
            var extraMetadataDir = GetExtraMetadataDirectory(game);
            if (Directory.Exists(extraMetadataDir))
            {
                try
                {
                    Directory.Delete(extraMetadataDir, true);
                    return true;
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Error while deleting removed game extra metadata directory {extraMetadataDir}");
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
