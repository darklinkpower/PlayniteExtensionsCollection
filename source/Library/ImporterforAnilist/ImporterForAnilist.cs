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
        private const string dbImportMessageId = "anilistlibImportError";
        private static readonly Regex mangadexIdRegex = new Regex(@"^https:\/\/mangadex\.org\/title\/([^\/]+)", RegexOptions.None);
        private static readonly Regex mangaseeIdRegex = new Regex(@"^https:\/\/mangasee123\.com\/manga\/([^\/]+)", RegexOptions.None);
        private ImporterForAnilistSettingsViewModel settings { get; set; }
        public override Guid Id { get; } = Guid.Parse("2366fb38-bf25-45ea-9a78-dcc797ee83c3");
        public override string Name => "Importer for AniList";
        public override string LibraryIcon { get; }
        public override LibraryClient Client { get; } = new ImporterForAnilistClient();
        public MalSyncRateLimiter MalSyncRateLimiter { get; } = new MalSyncRateLimiter();

        private string anilistLibraryCachePath;
        internal AnilistAccountClient accountApi;
        internal Dictionary<string, int> idsCache = new Dictionary<string, int>();
        public CompletionStatus CompletionStatusPlanWatch { get; private set; }
        public CompletionStatus CompletionStatusWatching { get; private set; }
        public CompletionStatus CompletionStatusPaused { get; private set; }
        public CompletionStatus CompletionStatusDropped { get; private set; }
        public CompletionStatus CompletionStatusCompleted { get; private set; }
        public CompletionStatus CompletionStatusRewatching { get; private set; }

        public ImporterForAnilist(IPlayniteAPI api) : base(api)
        {
            settings = new ImporterForAnilistSettingsViewModel(this, PlayniteApi);
            Properties = new LibraryPluginProperties
            {
                HasSettings = true,
                HasCustomizedGameImport = true
            };

            LibraryIcon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"icon.png");
            anilistLibraryCachePath = Path.Combine(GetPluginUserDataPath(), "libraryCache.json");
            accountApi = new AnilistAccountClient(PlayniteApi, settings);
        }

        public GameMetadata EntryToGameMetadata(Entry entry)
        {
            var game = AnilistResponseHelper.MediaToGameMetadata(entry.Media, false, settings.Settings.PropertiesPrefix);
            if (entry.Score != 0)
            {
                game.UserScore = entry.Score;
            }

            switch (entry.Status)
            {
                case EntryStatus.Current:
                    if (CompletionStatusWatching != null)
                    {
                        game.CompletionStatus = new MetadataNameProperty(CompletionStatusWatching.Name);
                    }
                    break;
                case EntryStatus.Planning:
                    if (CompletionStatusPlanWatch != null)
                    {
                        game.CompletionStatus = new MetadataNameProperty(CompletionStatusPlanWatch.Name);
                    }
                    break;
                case EntryStatus.Completed:
                    if (CompletionStatusCompleted != null)
                    {
                        game.CompletionStatus = new MetadataNameProperty(CompletionStatusCompleted.Name);
                    }
                    break;
                case EntryStatus.Dropped:
                    if (CompletionStatusDropped != null)
                    {
                        game.CompletionStatus = new MetadataNameProperty(CompletionStatusDropped.Name);
                    }
                    break;
                case EntryStatus.Paused:
                    if (CompletionStatusPaused != null)
                    {
                        game.CompletionStatus = new MetadataNameProperty(CompletionStatusPaused.Name);
                    }
                    break;
                case EntryStatus.Repeating:
                    if (CompletionStatusRewatching != null)
                    {
                        game.CompletionStatus = new MetadataNameProperty(CompletionStatusRewatching.Name);
                    }
                    break;
                default:
                    break;
            }

            if (entry.UpdatedAt != 0)
            {
                game.LastActivity = DateTimeOffset.FromUnixTimeSeconds(entry.UpdatedAt).LocalDateTime;
            }

            if (settings.Settings.UpdateProgressOnLibUpdate)
            {
                game.Version = GetEntryVersionString(entry);
            }

            return game;
        }

        private string GetEntryVersionString(Entry entry)
        {
            //Version (Used for progress)
            var totalLength = 0;
            var progressPercentageFormat = "???";
            var totalLenghtString = "???";
            if (entry.Media.Type == TypeEnum.Manga)
            {
                if (entry.Media.Chapters != null)
                {
                    totalLength = (int)entry.Media.Chapters;
                    totalLenghtString = totalLength.ToString("#000");
                }
            }
            else if (entry.Media.Type == TypeEnum.Anime)
            {
                if (entry.Media.Episodes != null)
                {
                    totalLength = (int)entry.Media.Episodes;
                    totalLenghtString = totalLength.ToString("#000");
                }
            }

            if (totalLength != 0)
            {
                var percentage = Convert.ToInt32((entry.Progress * 100) / totalLength);
                progressPercentageFormat = $"{percentage:000}%";
            }

            return $"({progressPercentageFormat}) {entry.Progress:#000}/{totalLenghtString}";
        }

        private void InitializeStatuses()
        {
            CompletionStatusPlanWatch = PlayniteApi.Database.CompletionStatuses[settings.Settings.PlanWatchId];
            CompletionStatusWatching = PlayniteApi.Database.CompletionStatuses[settings.Settings.WatchingId];
            CompletionStatusPaused = PlayniteApi.Database.CompletionStatuses[settings.Settings.PausedId];
            CompletionStatusDropped = PlayniteApi.Database.CompletionStatuses[settings.Settings.DroppedId];
            CompletionStatusCompleted = PlayniteApi.Database.CompletionStatuses[settings.Settings.CompletedId];
            CompletionStatusRewatching = PlayniteApi.Database.CompletionStatuses[settings.Settings.RewatchingId];
        }

        public override IEnumerable<Game> ImportGames(LibraryImportGamesArgs args)
        {
            var importedGames = new List<Game>();
            if (settings.Settings.AccountAccessCode.IsNullOrEmpty())
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    ResourceProvider.GetString("LOCImporter_For_Anilist_NotificationMessageAccessCodeNotConfigured"),
                    NotificationType.Error));

                return importedGames;
            }

            if (!accountApi.GetIsLoggedIn())
            {
                //Username could not be obtained
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    ResourceProvider.GetString("LOCImporter_For_Anilist_NotificationMessageAniListUsernameNotObtained"),
                    NotificationType.Error));

                return importedGames;
            }

            InitializeStatuses();
            var libraryCache = new Dictionary<string, int>();
            if (settings.Settings.ImportAnimeLibrary)
            {
                var animeEntries = accountApi.GetEntries("ANIME");
                logger.Debug($"Found {animeEntries.Count} Anime items");
                foreach (var entry in animeEntries)
                {
                    libraryCache.Add(entry.Media.Id.ToString(), entry.Id);
                    var existingEntry = PlayniteApi.Database.Games.FirstOrDefault(a => a.PluginId == Id && a.GameId == entry.Media.Id.ToString());
                    if (existingEntry != null)
                    {
                        UpdateExistingEntry(entry, existingEntry);
                    }
                    else
                    {
                        importedGames.Add(PlayniteApi.Database.ImportGame(EntryToGameMetadata(entry), this));
                    }
                }
            }

            if (settings.Settings.ImportMangaLibrary)
            {
                var mangaEntries = accountApi.GetEntries("MANGA");
                logger.Debug($"Found {mangaEntries.Count} Manga items");
                foreach (var entry in mangaEntries)
                {
                    libraryCache.Add(entry.Media.Id.ToString(), entry.Id);
                    var existingEntry = PlayniteApi.Database.Games.FirstOrDefault(a => a.PluginId == Id && a.GameId == entry.Media.Id.ToString());
                    if (existingEntry != null)
                    {
                        UpdateExistingEntry(entry, existingEntry);
                    }
                    else
                    {
                        importedGames.Add(PlayniteApi.Database.ImportGame(EntryToGameMetadata(entry), this));
                    }
                }
            }

            FileSystem.WriteStringToFile(anilistLibraryCachePath, Serialization.ToJson(libraryCache));
            idsCache = libraryCache;
            return importedGames;
        }

        private void UpdateExistingEntry(Entry entry, Game existingEntry)
        {
            var updateGame = false;
            if (settings.Settings.UpdateUserScoreOnLibUpdate == true && entry.Score != 0 && entry.Score != existingEntry.UserScore)
            {
                existingEntry.UserScore = entry.Score;
                updateGame = true;
            }

            if (settings.Settings.UpdateProgressOnLibUpdate == true)
            {
                var versionString = GetEntryVersionString(entry);
                if (existingEntry.Version != versionString)
                {
                    existingEntry.Version = versionString;
                    updateGame = true;
                }
            }

            if (settings.Settings.UpdateLastActivityOnLibUpdate && entry.UpdatedAt != 0)
            {
                var updatedTime = DateTimeOffset.FromUnixTimeSeconds(entry.UpdatedAt).LocalDateTime;
                if (existingEntry.LastActivity == null || updatedTime > existingEntry.LastActivity)
                {
                    existingEntry.LastActivity = updatedTime;
                    updateGame = true;
                }
            }

            var progressTagName = $"{settings.Settings.PropertiesPrefix}Status: {entry.Media.Status}";
            if (existingEntry.TagIds == null)
            {
                existingEntry.TagIds = new List<Guid>() { PlayniteApi.Database.Tags.Add(progressTagName).Id };
            }
            else
            {
                var tagStartStr = $"{settings.Settings.PropertiesPrefix}Status: ";
                var progressTag = existingEntry.Tags.FirstOrDefault(x => x.Name.StartsWith(tagStartStr));
                if (progressTag == null)
                {
                    existingEntry.TagIds.Add(PlayniteApi.Database.Tags.Add(progressTagName).Id);
                    updateGame = true;
                }
                else if (progressTag.Name != progressTagName)
                {
                    existingEntry.TagIds.Remove(progressTag.Id);
                    existingEntry.TagIds.Add(PlayniteApi.Database.Tags.Add(progressTagName).Id);
                    updateGame = true;
                }
            }

            if (!existingEntry.IsInstalled)
            {
                existingEntry.IsInstalled = true;
                updateGame = true;
            }

            if (settings.Settings.UpdateCompletionStatusOnLibUpdate == true && entry.Status != null)
            {
                if (UpdateGameCompletionStatusFromEntryStatus(existingEntry, entry.Status))
                {
                    updateGame = true;
                }
            }

            if (updateGame)
            {
                PlayniteApi.Database.Games.Update(existingEntry);
            }
        }

        internal bool UpdateGameCompletionStatusFromEntryStatus(Game existingEntry, EntryStatus? entryStatus)
        {
            switch (entryStatus)
            {
                case EntryStatus.Current:
                    if (CompletionStatusWatching != null && existingEntry.CompletionStatusId != CompletionStatusWatching.Id)
                    {
                        existingEntry.CompletionStatusId = CompletionStatusWatching.Id;
                        return true;
                    }
                    break;
                case EntryStatus.Planning:
                    if (CompletionStatusPlanWatch != null && existingEntry.CompletionStatusId != CompletionStatusPlanWatch.Id)
                    {
                        existingEntry.CompletionStatusId = CompletionStatusPlanWatch.Id;
                        return true;
                    }
                    break;
                case EntryStatus.Completed:
                    if (CompletionStatusCompleted != null && existingEntry.CompletionStatusId != CompletionStatusCompleted.Id)
                    {
                        existingEntry.CompletionStatusId = CompletionStatusCompleted.Id;
                        return true;
                    }
                    break;
                case EntryStatus.Dropped:
                    if (CompletionStatusDropped != null && existingEntry.CompletionStatusId != CompletionStatusDropped.Id)
                    {
                        existingEntry.CompletionStatusId = CompletionStatusDropped.Id;
                        return true;
                    }
                    break;
                case EntryStatus.Paused:
                    if (CompletionStatusPaused != null && existingEntry.CompletionStatusId != CompletionStatusPaused.Id)
                    {
                        existingEntry.CompletionStatusId = CompletionStatusPaused.Id;
                        return true;
                    }
                    break;
                case EntryStatus.Repeating:
                    if (CompletionStatusRewatching != null && existingEntry.CompletionStatusId != CompletionStatusRewatching.Id)
                    {
                        existingEntry.CompletionStatusId = CompletionStatusRewatching.Id;
                        return true;
                    }
                    break;
                default:
                    break;
            }

            return false;
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
            return new AnilistMetadataProvider(this, PlayniteApi, settings.Settings.PropertiesPrefix, MalSyncRateLimiter);
        }

        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (!args.Games.Any(x => x.PluginId == Id))
            {
                return null;
            }

            var menuSection = ResourceProvider.GetString("LOCImporter_For_Anilist_GameMenuItemDescriptionAniListEntriesStatus");
            return new List<GameMenuItem>
            {
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusCompletedLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        UpdateGamesCompletionStatus(a.Games, EntryStatus.Completed);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusWatchingLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        UpdateGamesCompletionStatus(a.Games, EntryStatus.Current);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusPlanWatchLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        UpdateGamesCompletionStatus(a.Games, EntryStatus.Planning);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusDroppedLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        UpdateGamesCompletionStatus(a.Games, EntryStatus.Dropped);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusPausedLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        UpdateGamesCompletionStatus(a.Games, EntryStatus.Paused);
                    }
                },
                new GameMenuItem
                {
                    Description = ResourceProvider.GetString("LOCImporter_For_Anilist_SettingStatusRewatchingLabel"),
                    MenuSection = menuSection,
                    Action = a => {
                        UpdateGamesCompletionStatus(a.Games, EntryStatus.Repeating);
                    }
                },
            };
        }

        private void UpdateGamesCompletionStatus(List<Game> games, EntryStatus entryStatus)
        {
            using (PlayniteApi.Database.BufferedUpdate())
            PlayniteApi.Dialogs.ActivateGlobalProgress((a) =>
            {
                if (!idsCache.HasItems())
                {
                    if (!File.Exists(anilistLibraryCachePath))
                    {
                        logger.Debug("Cache not set and cache file not found");
                        return;
                    }

                    idsCache = Serialization.FromJsonFile<Dictionary<string, int>>(anilistLibraryCachePath);
                }

                if (!accountApi.GetIsLoggedIn())
                {
                    //Username could not be obtained
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        dbImportMessageId,
                        ResourceProvider.GetString("LOCImporter_For_Anilist_NotificationMessageAniListUsernameNotObtained"),
                        NotificationType.Error));

                    return;
                }

                var gamesToUpdate = new Dictionary<int, Game>();
                foreach (var game in games.Distinct())
                {
                    if (game.PluginId != Id)
                    {
                        continue;
                    }

                    var gameIdInt = int.Parse(game.GameId);
                    if (idsCache.TryGetValue(game.GameId, out var id))
                    {
                        gamesToUpdate.Add(id, game);
                        continue;
                    }

                    logger.Debug($"Matching entry for {game.Name} with Id {game.GameId} not found during UpdateGamesCompletionStatus");
                }

                var updateIdsResult = accountApi.UpdateEntriesStatuses(gamesToUpdate.Select(x => x.Key).ToList(), entryStatus);
                if (updateIdsResult == null)
                {
                    return;
                }

                InitializeStatuses();
                foreach (var gameToUpdate in gamesToUpdate)
                {
                    var updatedEntry = updateIdsResult.Data.UpdateMediaListEntries.FirstOrDefault(x => x.Id == gameToUpdate.Key);
                    if (updatedEntry == null)
                    {
                        logger.Debug($"Updated entry not found. {gameToUpdate.Value.Name}, {gameToUpdate.Value.GameId}, {gameToUpdate.Key}");
                        continue;
                    }

                    var updatedCompletionStatus = UpdateGameCompletionStatusFromEntryStatus(gameToUpdate.Value, updatedEntry.Status);
                    if (updatedCompletionStatus)
                    {
                        PlayniteApi.Database.Games.Update(gameToUpdate.Value);
                    }
                }

            }, new GlobalProgressOptions(ResourceProvider.GetString("LOCImporter_For_Anilist_DialogMessageUpdatingEntriesStatuses"), true));
        }

        public override IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            var game = args.Game;
            if (game.PluginId != Id)
            {
                yield break;
            }

            if (game.Links == null || game.Links.Count == 0)
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
            if (browserPath != string.Empty)
            {
                return CreateBrowserPlayController(game, name, url);
            }
            else
            {
                return CreateUrlPlayController(game, name, url);
            }
        }

        public AutomaticPlayController CreateBrowserPlayController(Game game, string name, string url)
        {
            return new AutomaticPlayController(game)
            {
                Name = $"{ResourceProvider.GetString("LOCImporter_For_Anilist_PlayActionOpenLinkLabel")} \"{name}\"",
                Path = settings.Settings.BrowserPath,
                Type = AutomaticPlayActionType.File,
                Arguments = url,
                WorkingDir = Path.GetDirectoryName(settings.Settings.BrowserPath),
                TrackingPath = Path.GetDirectoryName(settings.Settings.BrowserPath),
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