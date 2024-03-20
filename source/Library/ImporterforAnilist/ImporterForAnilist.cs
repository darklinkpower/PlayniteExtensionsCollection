using ImporterforAnilist.Services;
using ImporterforAnilist.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Text.RegularExpressions;
using PluginsCommon;
using Playnite.SDK.Data;
using System.Reflection;

namespace ImporterforAnilist
{
    public class ImporterForAnilist : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly Regex mangadexIdRegex = new Regex(@"^https:\/\/mangadex\.org\/title\/([^\/]+)", RegexOptions.None);
        private static readonly Regex mangaseeIdRegex = new Regex(@"^https:\/\/mangasee123\.com\/manga\/([^\/]+)", RegexOptions.None);
        private ImporterForAnilistSettingsViewModel settings { get; set; }
        public override Guid Id { get; } = Guid.Parse("2366fb38-bf25-45ea-9a78-dcc797ee83c3");
        public override string Name => "Importer for AniList";
        public override string LibraryIcon { get; }
        public override LibraryClient Client { get; } = new ImporterForAnilistClient();
        internal AnilistService anilistService;
        private readonly MalSyncService malSyncService;
        private readonly LibraryUpdater libraryUpdater;

        public ImporterForAnilist(IPlayniteAPI api) : base(api)
        {
            settings = new ImporterForAnilistSettingsViewModel(this, PlayniteApi);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true,
                HasCustomizedGameImport = true
            };

            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
            
            anilistService = new AnilistService(settings);
            malSyncService = new MalSyncService();
            libraryUpdater = new LibraryUpdater(settings.Settings, PlayniteApi, anilistService, this);
        }

        public override IEnumerable<Game> ImportGames(LibraryImportGamesArgs args)
        {
            return libraryUpdater.ImportGames();
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ImporterforAnilistSettingsView();
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new AnilistMetadataProvider(settings.Settings, anilistService, malSyncService);
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (!args.Games.Any(x => x.PluginId == Id))
            {
                return Enumerable.Empty<GameMenuItem>();
            }

            var menuSection = ResourceProvider.GetString("LOCImporter_For_Anilist_GameMenuItemDescriptionAniListEntriesStatus");
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusCompletedLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        libraryUpdater.UpdateGamesCompletionStatus(a.Games, EntryStatus.Completed);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusWatchingLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        libraryUpdater.UpdateGamesCompletionStatus(a.Games, EntryStatus.Current);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusPlanWatchLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        libraryUpdater.UpdateGamesCompletionStatus(a.Games, EntryStatus.Planning);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusDroppedLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        libraryUpdater.UpdateGamesCompletionStatus(a.Games, EntryStatus.Dropped);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusPausedLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        libraryUpdater.UpdateGamesCompletionStatus(a.Games, EntryStatus.Paused);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusRewatchingLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        libraryUpdater.UpdateGamesCompletionStatus(a.Games, EntryStatus.Repeating);
                    }
                },
            };
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            var game = args.Game;
            if (game.PluginId != Id)
            {
                yield break;
            }

            if (!game.Links.HasItems())
            {
                PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCImporter_For_Anilist_PlayActionNoLinksAvailableLabel"));
                yield break;
            }

            var browserPath = string.Empty;
            if (!settings.Settings.BrowserPath.IsNullOrEmpty() && FileSystem.FileExists(settings.Settings.BrowserPath))
            {
                browserPath = settings.Settings.BrowserPath;
            }

            var cubariLinks = new List<Link>();
            foreach (Link link in game.Links)
            {
                if (link.Name == string.Empty || link.Name == "AniList" || link.Name == "MyAnimeList")
                {
                    continue;
                }

                if (link.Url.IsNullOrEmpty())
                {
                    continue;
                }

                var match = mangadexIdRegex.Match(link.Url);
                if (match.Success)
                {
                    var actionName = string.Format("Cubari (MangaDex) {0}", link.Name.Replace("Mangadex - ", ""));
                    var actionUrl = string.Format(@"https://cubari.moe/read/mangadex/{0}/", match.Groups[1]);
                    cubariLinks.Add(new Link { Name = actionName, Url = actionUrl });
                }
                else
                {
                    var match2 = mangaseeIdRegex.Match(link.Url);
                    if (match2.Success)
                    {
                        var actionName = string.Format("Cubari (MangaSee) {0}", link.Name.Replace("MangaSee - ", ""));
                        var actionUrl = string.Format(@"https://cubari.moe/read/mangasee/{0}/", match2.Groups[1]);
                        cubariLinks.Add(new Link { Name = actionName, Url = actionUrl });
                    }
                }

                yield return CreatePlayController(game, link.Name, link.Url, browserPath);
            }

            foreach (Link link in cubariLinks)
            {
                yield return CreatePlayController(game, link.Name, link.Url, browserPath);
            }
        }

        public AutomaticPlayController CreatePlayController(Game game, string name, string url, string browserPath)
        {
            if (!browserPath.IsNullOrEmpty())
            {
                return CreateBrowserPlayController(game, name, url, browserPath);
            }
            else
            {
                return CreateUrlPlayController(game, name, url);
            }
        }

        public AutomaticPlayController CreateBrowserPlayController(Game game, string name, string url, string browserPath)
        {
            return new AutomaticPlayController(game)
            {
                Name = $"{ResourceProvider.GetString("LOCImporter_For_Anilist_PlayActionOpenLinkLabel")} \"{name}\"",
                Path = browserPath,
                Type = AutomaticPlayActionType.File,
                Arguments = url,
                WorkingDir = Path.GetDirectoryName(browserPath),
                TrackingPath = Path.GetDirectoryName(browserPath),
                TrackingMode = TrackingMode.Directory
            };
        }

        public AutomaticPlayController CreateUrlPlayController(Game game, string name, string url)
        {
            return new AutomaticPlayController(game)
            {
                Name = $"{ResourceProvider.GetString("LOCImporter_For_Anilist_PlayActionOpenLinkLabel")} \"{name}\"",
                Path = url,
                Type = AutomaticPlayActionType.Url,
                TrackingMode = TrackingMode.Default
            };
        }
    }
}