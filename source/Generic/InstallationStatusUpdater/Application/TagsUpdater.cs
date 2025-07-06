using Playnite.SDK;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Application
{
    public class TagsUpdater
    {
        private readonly IPlayniteAPI _playniteApi;
        private readonly ILogger _logger;
        private const string DriveTagPrefix = "[Install Drive]";

        public TagsUpdater(
            IPlayniteAPI playniteApi,
            ILogger logger,
            InstallationStatusUpdaterSettingsViewModel settings)
        {
            _playniteApi = playniteApi ?? throw new ArgumentNullException(nameof(playniteApi));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void UpdateTagsWithInstallationRoot(IEnumerable<Game> games)
        {
            var progressOptions = new GlobalProgressOptions(
                    ResourceProvider.GetString("LOCInstallation_Status_Updater_StatusUpdaterUpdatingTagsProgressMessage"))
            {
                Cancelable = true
            };

            _playniteApi.Dialogs.ActivateGlobalProgress(
                args => UpdateTagsWithInstallationRoot(args, games.ToList()),
                progressOptions);
        }

        private void UpdateTagsWithInstallationRoot(GlobalProgressActionArgs args, List<Game> games)
        {
            var driveTags = new Dictionary<string, Tag>();
            using (_playniteApi.Database.BufferedUpdate())
            {
                foreach (var game in games)
                {
                    if (args.CancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var tagName = string.Empty;
                    if (!game.InstallDirectory.IsNullOrEmpty() && game.IsInstalled)
                    {
                        try
                        {
                            var drive = Path.GetPathRoot(new FileInfo(game.InstallDirectory).FullName)?.ToUpperInvariant();
                            if (!drive.IsNullOrEmpty())
                            {
                                tagName = $"{DriveTagPrefix} {drive}";
                                if (!driveTags.ContainsKey(tagName))
                                {
                                    var tag = _playniteApi.Database.Tags.Add(tagName);
                                    driveTags.Add(tagName, tag);
                                }

                                PlayniteUtilities.AddTagToGame(_playniteApi, game, driveTags[tagName]);
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, $"[TagUpdater] Failed to process directory: {game.InstallDirectory}");
                        }
                    }

                    if (!game.Tags.Any())
                    {
                        continue;
                    }

                    foreach (var tag in game.Tags.Where(t => t.Name.StartsWith(DriveTagPrefix)).ToList())
                    {
                        if (tagName.IsNullOrEmpty() || !tag.Name.Equals(tagName))
                        {
                            PlayniteUtilities.RemoveTagFromGame(_playniteApi, game, tag);
                        }
                    }
                }
            }
        }
    }
}
