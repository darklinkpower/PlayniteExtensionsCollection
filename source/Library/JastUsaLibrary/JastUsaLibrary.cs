using JastUsaLibrary.Models;
using JastUsaLibrary.Services;
using JastUsaLibrary.ViewModels;
using JastUsaLibrary.Views;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using JastUsaLibrary.ProgramsHelper.Models;

namespace JastUsaLibrary
{
    public class JastUsaLibrary : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        private JastUsaLibrarySettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("d407a620-5953-4ca4-a25c-8194c8559381");

        // Change to something more appropriate
        public override string Name => "JAST USA";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new JastUsaLibraryClient();
        public JastUsaAccountClient AccountClient;
        private readonly string userGamesCachePath;
        private readonly string gameInstallCachePath;
        private readonly string authenticationPath;
        private const string jastMediaUrlTemplate = @"https://app.jastusa.com/media/image/{0}";

        public JastUsaLibrary(IPlayniteAPI api) : base(api)
        {
            userGamesCachePath = Path.Combine(GetPluginUserDataPath(), "userGamesCache.json");
            gameInstallCachePath = Path.Combine(GetPluginUserDataPath(), "gameInstallCache.json");
            authenticationPath = Path.Combine(GetPluginUserDataPath(), "authentication.json");
            AccountClient = new JastUsaAccountClient(api, authenticationPath);
            settings = new JastUsaLibrarySettingsViewModel(this, PlayniteApi, AccountClient);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
        }

        private List<GameInstallCache> GetGameInstallCache()
        {
            if (FileSystem.FileExists(gameInstallCachePath))
            {
                return Serialization.FromJsonFile<List<GameInstallCache>>(gameInstallCachePath);
            }

            return new List<GameInstallCache>();
        }

        private void SaveGameInstallCache(List<GameInstallCache> gameInstallCache)
        {
            FileSystem.WriteStringToFile(gameInstallCachePath, Serialization.ToJson(gameInstallCache));
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var games = new List<GameMetadata>();
            var installedGames = GetInstalledGames();
            var authenticationToken = AccountClient.GetAuthenticationToken();
            if (authenticationToken == null) // User is not logged in
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "JAST USA Library: " + ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageNotAuthenticated"), NotificationType.Error, () => OpenSettingsView()));
                return games;
            }

            var jastProducts = AccountClient.GetGames(authenticationToken);
            if (jastProducts.Count > 0)
            {
                FileSystem.WriteStringToFile(userGamesCachePath, Serialization.ToJson(jastProducts), true);
            }

            foreach (var jastProduct in jastProducts)
            {
                if (!jastProduct.ProductVariant.Platforms.Any(x => x.Any(y => y.Value == "Windows")))
                {
                    continue;
                }

                var game = new GameMetadata
                {
                    Name = jastProduct.ProductVariant.ProductName.RemoveTrademarks(),
                    GameId = jastProduct.ProductVariant.GameId.ToString(),
                    BackgroundImage = new MetadataFile(string.Format(jastMediaUrlTemplate, jastProduct.ProductVariant.ProductImageBackground)),
                    CoverImage = new MetadataFile(string.Format(jastMediaUrlTemplate, jastProduct.ProductVariant.ProductImage)),
                    Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") }
                };

                if (installedGames.TryGetValue(game.GameId, out var installed))
                {
                    game.InstallDirectory = Path.GetDirectoryName(installed.Program.Path);
                    game.IsInstalled = true;
                }

                games.Add(game);
            }

            return games;
        }

        private Dictionary<string, GameInstallCache> GetInstalledGames()
        {
            var installedDictionary = new Dictionary<string, GameInstallCache>();
            var installCache = GetGameInstallCache();

            var removals = 0;
            foreach (var cacheItem in installCache.ToList())
            {
                if (!FileSystem.FileExists(cacheItem.Program.Path) || PlayniteApi.Database.Games[cacheItem.Id] == null)
                {
                    installCache.Remove(cacheItem);
                    removals++;
                }
                else
                {
                    installedDictionary.Add(cacheItem.GameId, cacheItem);
                }
            }

            if (removals > 0)
            {
                SaveGameInstallCache(installCache);
            }

            return installedDictionary;
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new JastUsaLibraryMetadataProvider(userGamesCachePath);
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new JastUsaLibrarySettingsView();
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (args.Games.Last().PluginId != Id)
            {
                return null;
            }

            var game = args.Games.Last();
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = string.Format(ResourceProvider.GetString("LOCJast_Usa_Library_GameMenuItemDescriptionViewGameDownloads"), game.Name),
                    MenuSection = "JAST USA",
                    Action = a => {
                        OpenGameDownloadsWindow(game);
                    }
                }
            };
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                return null;
            }

            var game = args.Game;
            var gameInstallCache = GetGameInstallCache();
            var gameCache = gameInstallCache.FirstOrDefault(x => x.GameId == game.GameId);
            if (gameCache == null)
            {
                return null;
            }

            return new List<PlayController>
            {
                new AutomaticPlayController(game)
                {
                    Name = gameCache.Program.Name,
                    Path = gameCache.Program.Path,
                    Arguments = gameCache.Program.Arguments,
                    WorkingDir = gameCache.Program.WorkDir,
                    TrackingMode = TrackingMode.Default
                }
            };
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                return null;
            }

            var options = new List<MessageBoxOption>
            {
                new MessageBoxOption(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageDownloadOption")),
                new MessageBoxOption(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageBrowseForGameOption")),
                //new MessageBoxOption(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageSelectInstalledOption")),
                new MessageBoxOption(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageCancelOption"), false, true)
            };

            var game = args.Game;
            var selected = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageSelectActionLabel"), "JAST USA Library", System.Windows.MessageBoxImage.None, options);
            if (!selected.IsCancel)
            {
                if (selected == options[0]) // Download option
                {
                    OpenGameDownloadsWindow(game);
                }
                else if (selected == options[1]) // Browse for executable option
                {
                    var selectedProgram = SelectExecutable();
                    if (selectedProgram == null)
                    {
                        return null;
                    }

                    return AddGameToCache(game, selectedProgram);
                }
                //else if (selected == options[2]) // Select install option
                //{

                //}
            }


            return null;
        }

        public override IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            if (args.Game.PluginId != Id)
            {
                return null;
            }

            var game = args.Game;
            var gameInstallCache = GetGameInstallCache();
            var gameCache = gameInstallCache.FirstOrDefault(x => x.GameId == game.GameId);
            if (gameCache == null)
            {
                return null;
            }

            gameInstallCache.Remove(gameCache);
            SaveGameInstallCache(gameInstallCache);

            PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageUninstallNotice"), "JAST USA Library");
            if (FileSystem.FileExists(gameCache.Program.Path))
            {
                ProcessStarter.StartProcess(Path.GetDirectoryName(gameCache.Program.Path));
            }
            
            return new List<UninstallController> { new FakeUninstallController(game) };
        }

        private List<InstallController> AddGameToCache(Game game, Program selectedProgram)
        {
            var cache = new GameInstallCache
            {
                GameId = game.GameId,
                Id = game.Id,
                Program = selectedProgram
            };

            var gameInstallCache = GetGameInstallCache();

            // Remove existing game cache if found
            foreach (var savedCache in gameInstallCache.ToList())
            {
                if (savedCache.GameId == game.GameId)
                {
                    gameInstallCache.Remove(savedCache);
                }
            }

            gameInstallCache.Add(cache);
            SaveGameInstallCache(gameInstallCache);
            return GetFakeController(game, selectedProgram.WorkDir);
        }

        public Program SelectExecutable()
        {
            var path = PlayniteApi.Dialogs.SelectFile("Executable (.exe,.bat,lnk)|*.exe;*.bat;*.lnk");
            if (path.IsNullOrEmpty())
            {
                return null;
            }

            if (!path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var program = ProgramsHelper.Programs.GetProgramData(path);
            // Use shortcut name as game name for .lnk shortcuts
            if (path.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            {
                var shortcutName = Path.GetFileNameWithoutExtension(path);
                if (!shortcutName.IsNullOrEmpty())
                {
                    program.Name = shortcutName;
                }
            }

            return program;
        }

        private List<InstallController> GetFakeController(Game game, string installDir)
        {
            return new List<InstallController> { new FakeInstallController(game, installDir) };
        }

        private GameTranslationsResponse GetGameTranslations(Game game)
        {
            if (!FileSystem.FileExists(userGamesCachePath))
            {
                return null;
            }

            var authenticationToken = AccountClient.GetAuthenticationToken();
            if (authenticationToken == null) // User is not logged in
            {
                PlayniteApi.Notifications.Add(new NotificationMessage("JastNotLoggedIn", "JAST USA Library: " + ResourceProvider.GetString("LOCJast_Usa_Library_DialogMessageNotAuthenticated"), NotificationType.Error, () => OpenSettingsView()));
                return null;
            }

            var cache = Serialization.FromJsonFile<List<JastProduct>>(userGamesCachePath);
            var gameVariant = cache.FirstOrDefault(x => x.ProductVariant.GameId.ToString() == game.GameId);
            if (gameVariant == null)
            {
                return null;
            }

            var gameTranslations = gameVariant.ProductVariant.Game.Translations.Where(x => x.Key == "en_US");
            if (gameTranslations.Count() == 0)
            {
                return null;
            }

            return AccountClient.GetGameTranslations(authenticationToken, gameTranslations.First().Value.Id);
        }

        private void OpenGameDownloadsWindow(Game game)
        {
            var gameTranslations = GetGameTranslations(game);
            if (gameTranslations == null)
            {
                return;
            }

            var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true
            });

            window.Height = 700;
            window.Width = 900;
            window.Title = ResourceProvider.GetString("LOCJast_Usa_Library_JastDownloaderWindowTitle");

            window.Content = new GameDownloadsView();
            window.DataContext = new GameDownloadsViewModel(PlayniteApi, game, gameTranslations, AccountClient, settings);
            window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            window.ShowDialog();
        }
    }


}