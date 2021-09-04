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

namespace ImporterforAnilist
{
    public class ImporterForAnilist : LibraryPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "anilistlibImportError";

        private ImporterForAnilistSettingsViewModel settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("2366fb38-bf25-45ea-9a78-dcc797ee83c3");

        // Change to something more appropriate
        public override string Name => "Importer for AniList";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new ImporterForAnilistClient();

        public ImporterForAnilist(IPlayniteAPI api) : base(api)
        {
            settings = new ImporterForAnilistSettingsViewModel(this);
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
                GameActions = new List<GameAction> {
                    new GameAction
                    {
                        Type = GameActionType.URL,
                        Path = entry.Media.SiteUrl.ToString()
                    }
                },
                Platforms = new List<MetadataProperty> { new MetadataNameProperty(string.Format("AniList {0}", entry.Media.Type.ToString())) },
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
                game.Genres = entry.Media.Genres?.Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a))).ToList();
            }

            //ReleaseDate
            if (entry.Media.StartDate.Year != null && entry.Media.StartDate.Month != null && entry.Media.StartDate.Day != null)
            {
                game.ReleaseDate = new ReleaseDate(new DateTime((int)entry.Media.StartDate.Year, (int)entry.Media.StartDate.Month, (int)entry.Media.StartDate.Day));
            }

            //Developers and Publishers
            if (entry.Media.Type == TypeEnum.Manga)
            {
                game.Developers = entry.Media.Staff.Nodes.
                    Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a.Name.Full))).ToList();
            }
            else if (entry.Media.Type == TypeEnum.Anime)
            {
                game.Developers = entry.Media.Studios.Nodes.Where(s => s.IsAnimationStudio == true).
                    Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a.Name))).ToList();
                game.Publishers = entry.Media.Studios.Nodes.Where(s => s.IsAnimationStudio == false).
                    Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a.Name))).ToList();
            }

            //Tags
            var tags = entry.Media.Tags.
                Where(s => s.IsMediaSpoiler == false).
                Where(s => s.IsGeneralSpoiler == false).
                Select(a => new MetadataNameProperty(string.Format("{0}{1}", propertiesPrefix, a.Name))).ToList();

            if (entry.Media.Season != null)
            {
                tags.Add(new MetadataNameProperty(string.Format("{0}Season: {1}", propertiesPrefix, entry.Media.Season.ToString())));
            }
            tags.Add(new MetadataNameProperty(string.Format("{0}Status: {1}", propertiesPrefix, entry.Media.Status.ToString())));
            tags.Add(new MetadataNameProperty(string.Format("{0}Format: {1}", propertiesPrefix, entry.Media.Format.ToString())));
            game.Tags = tags;

            ////CompletionStatus
            ////TODO Completion Status matching
            //switch (entry.Status)
            //{
            //    case EntryStatus.Current:
            //        game.CompletionStatus = CompletionStatus.Playing;
            //        break;
            //    case EntryStatus.Planning:
            //        game.CompletionStatus = CompletionStatus.PlanToPlay;
            //        break;
            //    case EntryStatus.Completed:
            //        game.CompletionStatus = CompletionStatus.Completed;
            //        break;
            //    case EntryStatus.Dropped:
            //        game.CompletionStatus = CompletionStatus.Abandoned;
            //        break;
            //    case EntryStatus.Paused:
            //        game.CompletionStatus = CompletionStatus.OnHold;
            //        break;
            //    case EntryStatus.Repeating:
            //        game.CompletionStatus = CompletionStatus.Playing;
            //        break;
            //    default:
            //        game.CompletionStatus = CompletionStatus.NotPlayed;
            //        break;
            //}

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
            var game = PlayniteApi.Database.Games.Where(g => g.PluginId == Id).Where(g => g.GameId == gameMetadata.GameId).FirstOrDefault();
            if (game != null)
            {
                var updateGame = false;
                if (settings.Settings.UpdateUserScoreOnLibUpdate == true && gameMetadata.UserScore != 0 && gameMetadata.UserScore != game.UserScore)
                {
                    game.UserScore = gameMetadata.UserScore;
                    updateGame = true;
                }

                ////TODO Completion Status matching
                //if (settings.UpdateCompletionStatusOnLibUpdate == true && gameMetadata.CompletionStatus != game.CompletionStatus)
                //{
                //    game.CompletionStatus = gameMetadata.CompletionStatus;
                //    updateGame = true;
                //}

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

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {

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
            
            return new AnilistMetadataProvider(this, PlayniteApi, propertiesPrefix);
        }
    }
}