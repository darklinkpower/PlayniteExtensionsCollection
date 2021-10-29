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

namespace ImporterforAnilist
{
    public class ImporterForAnilist : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "anilistlibImportError";
        private static readonly Regex mangadexIdRegex = new Regex(@"^https:\/\/mangadex\.org\/title\/([^\/]+)", RegexOptions.Compiled);
        private static readonly Regex mangaseeIdRegex = new Regex(@"^https:\/\/mangasee123\.com\/manga\/([^\/]+)", RegexOptions.Compiled);

        private ImporterForAnilistSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("2366fb38-bf25-45ea-9a78-dcc797ee83c3");

        // Change to something more appropriate
        public override string Name => "Importer for AniList";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new ImporterForAnilistClient();

        public MalSyncRateLimiter MalSyncRateLimiter { get; } = new MalSyncRateLimiter();

        private Dictionary<string, Guid> completionStatusesDict;

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
                HasSettings = true
            };
        }

        public string intToThreeDigitsString(int number)
        {
            if (number < 10)
            {
                return string.Format("00{0}", number.ToString());
            }
            else if (number < 100)
            {
                return string.Format("0{0}", number.ToString());
            }
            else
            {
                return number.ToString();
            }
        }

        public GameMetadata EntryToGameMetadata(Entry entry, string propertiesPrefix)
        {
            var game = new GameMetadata()
            {
                Source = new MetadataNameProperty("Anilist"),
                GameId = entry.Media.Id.ToString(),
                Name = entry.Media.Title.Romaji ?? entry.Media.Title.English ?? entry.Media.Title.Native ?? string.Empty,
                IsInstalled = true,
                Platforms = new HashSet<MetadataProperty> { new MetadataNameProperty(string.Format("AniList {0}", entry.Media.Type.ToString())) },
                //Description
                Description = entry.Media.Description ?? string.Empty,
            };

            //Scores
            game.CommunityScore = entry.Media.AverageScore ?? null;
            if (entry.Score != 0)
            {
                game.UserScore = entry.Score;
            }
            
            //Genres
            if (entry.Media.Genres != null)
            {
                game.Genres = entry.Media.Genres?.Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a))).Cast<MetadataProperty>().ToHashSet();
            }

            //ReleaseDate
            if (entry.Media.StartDate.Year != null && entry.Media.StartDate.Month != null && entry.Media.StartDate.Day != null)
            {
                game.ReleaseDate = new ReleaseDate(new DateTime((int)entry.Media.StartDate.Year, (int)entry.Media.StartDate.Month, (int)entry.Media.StartDate.Day));
            }

            //Developers and Publishers
            if (entry.Media.Type == TypeEnum.Manga)
            {
                game.Developers = entry.Media.Staff.Nodes?.
                    Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a.Name.Full))).Cast<MetadataProperty>().ToHashSet();
            }
            else if (entry.Media.Type == TypeEnum.Anime)
            {
                game.Developers = entry.Media.Studios.Nodes.Where(s => s.IsAnimationStudio == true)?.
                    Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a.Name))).Cast<MetadataProperty>().ToHashSet();
                game.Publishers = entry.Media.Studios.Nodes.Where(s => s.IsAnimationStudio == false)?.
                    Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a.Name))).Cast<MetadataProperty>().ToHashSet();
            }

            //Tags
            var tags = entry.Media.Tags.
                Where(s => s.IsMediaSpoiler == false).
                Where(s => s.IsGeneralSpoiler == false)?.
                Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a.Name))).Cast<MetadataProperty>().ToHashSet();

            if (entry.Media.Season != null)
            {
                tags.Add(new MetadataNameProperty(string.Format("{0}Season: {1}", propertiesPrefix, entry.Media.Season.ToString())));
            }
            tags.Add(new MetadataNameProperty(string.Format("{0}Status: {1}", propertiesPrefix, entry.Media.Status.ToString())));
            tags.Add(new MetadataNameProperty(string.Format("{0}Format: {1}", propertiesPrefix, entry.Media.Format.ToString())));
            game.Tags = tags;

            //CompletionStatus
            //TODO Completion Status matching
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

            if (settings.Settings.UpdateProgressOnLibUpdate == true)
            {
                //Version (Used for progress)
                var totalLength = 0;
                var progressPercentageString = string.Empty;
                var progressPercentageFormat = string.Empty;
                var totalLenghtString = "??";
                if (entry.Media.Type == TypeEnum.Manga)
                {
                    if (entry.Media.Chapters != null)
                    {
                        totalLength = (int)entry.Media.Chapters;
                        totalLenghtString = intToThreeDigitsString(totalLength);
                    }
                }
                else if (entry.Media.Type == TypeEnum.Anime)
                {
                    if (entry.Media.Episodes != null)
                    {
                        totalLength = (int)entry.Media.Episodes;
                        totalLenghtString = intToThreeDigitsString(totalLength);
                    }
                }
                if (totalLength != 0)
                {
                    int percentage = Convert.ToInt32((entry.Progress * 100) / totalLength);
                    progressPercentageFormat = string.Format("({0}%) ", intToThreeDigitsString(percentage));
                }

                game.Version = string.Format("{0}{1}/{2}", progressPercentageFormat, intToThreeDigitsString(entry.Progress), totalLenghtString);
            }

            return game;
        }

        public void overrideGameProperties(GameMetadata gameMetadata)
        {
            if (gameMetadata.GameId == "1698")
            {
                var asd = "SD";
            }

            var game = PlayniteApi.Database.Games.Where(g => g.PluginId == Id).Where(g => g.GameId == gameMetadata.GameId).FirstOrDefault();
            if (game != null)
            {
                var updateGame = false;
                if (settings.Settings.UpdateUserScoreOnLibUpdate == true && gameMetadata.UserScore != 0 && gameMetadata.UserScore != game.UserScore)
                {
                    game.UserScore = gameMetadata.UserScore;
                    updateGame = true;
                }

                //TODO Completion Status matching
                if (settings.Settings.UpdateCompletionStatusOnLibUpdate == true && gameMetadata.CompletionStatus != null)
                {
                    var ss = gameMetadata.CompletionStatus.ToString();
                    var completionStatusId = completionStatusesDict[gameMetadata.CompletionStatus.ToString()];
                    if (game.CompletionStatusId == null || game.CompletionStatusId != completionStatusId)
                    {
                        game.CompletionStatusId = completionStatusId;
                        updateGame = true;
                    }
                }

                if (settings.Settings.UpdateProgressOnLibUpdate == true && gameMetadata.Version != game.Version)
                {
                    game.Version = gameMetadata.Version;
                    updateGame = true;
                }

                if (updateGame == true)
                {
                    PlayniteApi.Database.Games.Update(game);
                }
            }
        }

        private void InitializeStatuses()
        {
            completionStatusesDict = new Dictionary<string, Guid>();

            CompletionStatusPlanWatch = PlayniteApi.Database.CompletionStatuses[settings.Settings.PlanWatchId];
            CompletionStatusWatching = PlayniteApi.Database.CompletionStatuses[settings.Settings.WatchingId];
            CompletionStatusPaused = PlayniteApi.Database.CompletionStatuses[settings.Settings.PausedId];
            CompletionStatusDropped = PlayniteApi.Database.CompletionStatuses[settings.Settings.DroppedId];
            CompletionStatusCompleted = PlayniteApi.Database.CompletionStatuses[settings.Settings.CompletedId];
            CompletionStatusRewatching = PlayniteApi.Database.CompletionStatuses[settings.Settings.RewatchingId];

            if (CompletionStatusPlanWatch != null)
            {
                completionStatusesDict.Add(CompletionStatusPlanWatch.Name, CompletionStatusPlanWatch.Id);
            }
            
            if (CompletionStatusWatching != null && !completionStatusesDict.ContainsKey(CompletionStatusWatching.Name))
            {
                completionStatusesDict.Add(CompletionStatusWatching.Name, CompletionStatusWatching.Id);
            }
            
            if (CompletionStatusPaused != null && !completionStatusesDict.ContainsKey(CompletionStatusPaused.Name))
            {
                completionStatusesDict.Add(CompletionStatusPaused.Name, CompletionStatusPaused.Id);
            }
            
            if (CompletionStatusDropped != null && !completionStatusesDict.ContainsKey(CompletionStatusDropped.Name))
            {
                completionStatusesDict.Add(CompletionStatusDropped.Name, CompletionStatusDropped.Id);
            }
            
            if (CompletionStatusCompleted != null && !completionStatusesDict.ContainsKey(CompletionStatusCompleted.Name))
            {
                completionStatusesDict.Add(CompletionStatusCompleted.Name, CompletionStatusCompleted.Id);
            }
            
            if (CompletionStatusRewatching != null && !completionStatusesDict.ContainsKey(CompletionStatusRewatching.Name))
            {
                completionStatusesDict.Add(CompletionStatusRewatching.Name, CompletionStatusRewatching.Id);
            }
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            InitializeStatuses();
            var gamesList = new List<GameMetadata>() { }; 
            
            if (string.IsNullOrEmpty(settings.Settings.AccountAccessCode))
            {
                PlayniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    "AniList access code has not been configured in the library settings",
                    NotificationType.Error));
            }
            else
            {
                string propertiesPrefix = settings.Settings.PropertiesPrefix;
                if (!string.IsNullOrEmpty(propertiesPrefix))
                {
                    propertiesPrefix = string.Format("{0} ", propertiesPrefix);
                }

                var accountApi = new AnilistAccountClient(PlayniteApi, settings.Settings.AccountAccessCode);
                if (string.IsNullOrEmpty(accountApi.anilistUsername))
                {
                    //Username could not be obtained
                    PlayniteApi.Notifications.Add(new NotificationMessage(
                        dbImportMessageId,
                        "Could not obtain AniList username. Verify that the configured access code is valid",
                        NotificationType.Error));
                }
                else
                {
                    logger.Info($"AniList username: {accountApi.anilistUsername}");
                    if (settings.Settings.ImportAnimeLibrary == true)
                    {
                        var animeEntries = accountApi.GetEntries("ANIME");
                        logger.Debug($"Found {animeEntries.Count} Anime items");
                        foreach (var entry in animeEntries)
                        {
                            var gameInfo = EntryToGameMetadata(entry, propertiesPrefix);
                            gamesList.Add(gameInfo);
                            overrideGameProperties(gameInfo);

                        }
                    }

                    if (settings.Settings.ImportMangaLibrary == true)
                    {
                        var mangaEntries = accountApi.GetEntries("MANGA");
                        logger.Debug($"Found {mangaEntries.Count} Manga items");
                        foreach (var entry in mangaEntries)
                        {
                            var gameInfo = EntryToGameMetadata(entry, propertiesPrefix);
                            gamesList.Add(gameInfo);
                            overrideGameProperties(gameInfo);
                        }
                    }
                }
            }

            return gamesList;
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
            string propertiesPrefix = settings.Settings.PropertiesPrefix;
            if (!string.IsNullOrEmpty(propertiesPrefix))
            {
                propertiesPrefix = string.Format("{0} ", propertiesPrefix);
            }
            
            return new AnilistMetadataProvider(this, PlayniteApi, propertiesPrefix, MalSyncRateLimiter);
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
                PlayniteApi.Dialogs.ShowMessage("Game doesn't have links available. Download metadata.");
                yield break;
            }

            var browserPath = string.Empty;
            if (!string.IsNullOrEmpty(settings.Settings.BrowserPath) && File.Exists(settings.Settings.BrowserPath))
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
                Name = $"Open link \"{name}\"",
                Path = settings.Settings.BrowserPath,
                Type = AutomaticPlayActionType.File,
                Arguments = url,
                WorkingDir = Path.GetDirectoryName(settings.Settings.BrowserPath),
                TrackingMode = TrackingMode.Process
            };
        }

        public AutomaticPlayController CreateUrlPlayController(Game game, string name, string url)
        {
            return new AutomaticPlayController(game)
            {
                Name = $"Open link \"{name}\"",
                Path = url,
                Type = AutomaticPlayActionType.Url,
                TrackingMode = TrackingMode.Default
            };
        }
    }
}