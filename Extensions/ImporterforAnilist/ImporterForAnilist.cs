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
        private readonly IPlayniteAPI playniteApi;
        private const string dbImportMessageId = "anilistlibImportError";

        private ImporterForAnilistSettings settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("2366fb38-bf25-45ea-9a78-dcc797ee83c3");

        // Change to something more appropriate
        public override string Name => "Importer for AniList";

        // Implementing Client adds ability to open it via special menu in playnite.
        public override LibraryClient Client { get; } = new ImporterForAnilistClient();

        public ImporterForAnilist(IPlayniteAPI api) : base(api)
        {
            playniteApi = api; 
            settings = new ImporterForAnilistSettings(this);
        }

        public GameInfo EntryToGameInfo(Entry entry, string propertiesPrefix)
        {
            var game = new GameInfo()
            {
                Source = "Anilist",
                GameId = entry.Media.Id.ToString(),
                Name = entry.Media.Title.Romaji ?? entry.Media.Title.English ?? entry.Media.Title.Native ?? string.Empty,
                IsInstalled = true,
                PlayAction = new GameAction()
                {
                    Type = GameActionType.URL,
                    Path = entry.Media.SiteUrl.ToString(),
                    IsHandledByPlugin = false
                },
                Platform = string.Format("AniList {0}", entry.Media.Type.ToString()),
                BackgroundImage = entry.Media.BannerImage ?? string.Empty,
                CommunityScore = entry.Media.AverageScore ?? null,
                CoverImage = entry.Media.CoverImage.ExtraLarge ?? string.Empty,
                Description = entry.Media.Description ?? string.Empty,
                Links = new List<Link>()
            };
            game.Links.Add(new Link("AniList", entry.Media.SiteUrl.ToString()));

            if (entry.Media.Genres != null)
            {
                game.Genres = entry.Media.Genres.Select(a => string.Format("{0}{1}", propertiesPrefix, a)).ToList();
            }

            if (entry.Media.StartDate.Year != null && entry.Media.StartDate.Month != null && entry.Media.StartDate.Day != null)
            {
                game.ReleaseDate = new DateTime((int)entry.Media.StartDate.Year, (int)entry.Media.StartDate.Month, (int)entry.Media.StartDate.Day);
            }

            if (entry.Media.IdMal != null)
            {
                game.Links.Add(new Link("MyAnimeList", string.Format("https://myanimelist.net/{0}/{1}/", entry.Media.Type.ToString().ToLower(), entry.Media.IdMal.ToString())));
            }

            if (entry.Media.Type == TypeEnum.Manga)
            {
                game.Developers = entry.Media.Staff.Nodes.
                    Select(a => string.Format("{0}{1}", propertiesPrefix, a.Name.Full)).ToList();
            }
            else if (entry.Media.Type == TypeEnum.Anime)
            {
                game.Developers = entry.Media.Studios.Nodes.Where(s => s.IsAnimationStudio == true).
                    Select(a => string.Format("{0}{1}", propertiesPrefix, a.Name)).ToList();
                game.Publishers = entry.Media.Studios.Nodes.Where(s => s.IsAnimationStudio == false).
                    Select(a => string.Format("{0}{1}", propertiesPrefix, a.Name)).ToList();
            }
            var tags = entry.Media.Tags.
                Where(s => s.IsMediaSpoiler == false).
                Where(s => s.IsGeneralSpoiler == false).
                Select(a => string.Format("{0}{1}", propertiesPrefix, a.Name)).ToList();

            if (entry.Media.Season != null)
            {
                tags.Add(string.Format("Season: {0}", entry.Media.Season.ToString()));
            }
            game.Tags = tags;

            switch (entry.Status)
            {
                case EntryStatus.Current:
                    game.CompletionStatus = CompletionStatus.Playing;
                    break;
                case EntryStatus.Planning:
                    game.CompletionStatus = CompletionStatus.PlanToPlay;
                    break;
                case EntryStatus.Completed:
                    game.CompletionStatus = CompletionStatus.Completed;
                    break;
                case EntryStatus.Dropped:
                    game.CompletionStatus = CompletionStatus.Abandoned;
                    break;
                case EntryStatus.Paused:
                    game.CompletionStatus = CompletionStatus.OnHold;
                    break;
                case EntryStatus.Repeating:
                    game.CompletionStatus = CompletionStatus.Playing;
                    break;
                default:
                    break;
            }

            return game;
    }

        public override IEnumerable<GameInfo> GetGames()
        {

            var gamesList = new List<GameInfo>() { }; 
            
            if (string.IsNullOrEmpty(settings.AccountAccessCode))
            {
                playniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    "Anilist access code has not been configured in settings",
                    NotificationType.Error));
            }
            else
            {
                string propertiesPrefix = settings.PropertiesPrefix;
                if (!string.IsNullOrEmpty(propertiesPrefix))
                {
                    propertiesPrefix = string.Format("{0} ", propertiesPrefix);
                }
                var accountApi = new AnilistAccountClient(playniteApi, settings.AccountAccessCode);
                if (string.IsNullOrEmpty(accountApi.anilistUsername))
                {
                    //Username could not be obtained
                    playniteApi.Notifications.Add(new NotificationMessage(
                        dbImportMessageId,
                        "Could not obtain Anilist username. Verify that the configured access code is valid",
                        NotificationType.Error));
                }
                else
                {
                    if (settings.ImportAnimeLibrary == true)
                    {
                        var animeEntries = accountApi.GetEntries("ANIME");
                        logger.Debug($"Found {animeEntries.Count} Anime items.");
                        foreach (var entry in animeEntries)
                        {
                            gamesList.Add(EntryToGameInfo(entry, propertiesPrefix));
                        }
                    }

                    if (settings.ImportMangaLibrary == true)
                    {
                        var mangaEntries = accountApi.GetEntries("MANGA");
                        logger.Debug($"Found {mangaEntries.Count} Manga items.");
                        foreach (var entry in mangaEntries)
                        {
                            gamesList.Add(EntryToGameInfo(entry, propertiesPrefix));
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
            string propertiesPrefix = settings.PropertiesPrefix;
            if (!string.IsNullOrEmpty(propertiesPrefix))
            {
                propertiesPrefix = string.Format("{0} ", propertiesPrefix);
            }
            
            return new AnilistMetadataProvider(this, PlayniteApi, propertiesPrefix);
        }
    }
}