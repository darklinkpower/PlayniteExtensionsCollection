using ImporterforAnilist.Models;
using ImporterforAnilist.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PlayniteUtilitiesCommon;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist
{
    public class LibraryUpdater
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private const string dbImportMessageId = "anilistlibImportError";
        private readonly ImporterForAnilistSettings settings;
        private Dictionary<string, int> idsCache = new Dictionary<string, int>();
        private readonly IPlayniteAPI playniteApi;
        private readonly AnilistService anilistService;
        private readonly ImporterForAnilist plugin;
        private readonly string anilistLibraryCachePath;
        private Dictionary<EntryStatus, CompletionStatus> completionStatusMap = new Dictionary<EntryStatus, CompletionStatus>();

        public LibraryUpdater(ImporterForAnilistSettings settings, IPlayniteAPI playniteApi, AnilistService anilistService, ImporterForAnilist plugin)
        {
            this.settings = settings;
            this.playniteApi = playniteApi;
            this.anilistService = anilistService;
            this.plugin = plugin;
            anilistLibraryCachePath = Path.Combine(plugin.GetPluginUserDataPath(), "libraryCache.json");
        }

        public IEnumerable<Game> ImportGames()
        {
            var importedGames = new List<Game>();
            playniteApi.Notifications.Remove(dbImportMessageId);
            if (settings.AccountAccessCode.IsNullOrEmpty())
            {
                playniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    ResourceProvider.GetString("LOCImporter_For_Anilist_NotificationMessageAccessCodeNotConfigured"),
                    NotificationType.Error,
                    () => plugin.OpenSettingsView()));

                return importedGames;
            }

            if (!anilistService.GetIsLoggedIn())
            {
                //Username could not be obtained
                playniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    ResourceProvider.GetString("LOCImporter_For_Anilist_NotificationMessageAniListUsernameNotObtained"),
                    NotificationType.Error,
                    () => plugin.OpenSettingsView()));

                return importedGames;
            }

            InitializeStatuses();
            var libraryCache = new Dictionary<string, int>();
            var tagsCache = new Dictionary<string, Tag>();
            var genresCache = new Dictionary<string, Genre>();
            var companiesCache = new Dictionary<string, Company>();
            var alreadyImportedEntries = playniteApi.Database.Games.Where(a => a.PluginId == plugin.Id)
                                         .ToDictionary(x => x.GameId, x => x);
            if (settings.ImportAnimeLibrary)
            {
                var animeEntries = anilistService.GetEntries("ANIME");
                logger.Debug($"Found {animeEntries.Count} Anime items");
                ProcessEntriesResponse(importedGames, libraryCache, tagsCache, genresCache, companiesCache, animeEntries, alreadyImportedEntries);
            }

            if (settings.ImportMangaLibrary)
            {
                var mangaEntries = anilistService.GetEntries("MANGA");
                logger.Debug($"Found {mangaEntries.Count} Manga items");
                ProcessEntriesResponse(importedGames, libraryCache, tagsCache, genresCache, companiesCache, mangaEntries, alreadyImportedEntries);
            }

            FileSystem.WriteStringToFile(anilistLibraryCachePath, Serialization.ToJson(libraryCache));
            idsCache = libraryCache;
            return importedGames;
        }

        private void InitializeStatuses()
        {
            completionStatusMap = new Dictionary<EntryStatus, CompletionStatus>
            {
                { EntryStatus.Current, playniteApi.Database.CompletionStatuses[settings.WatchingId]  },
                { EntryStatus.Planning, playniteApi.Database.CompletionStatuses[settings.PlanWatchId] },
                { EntryStatus.Completed, playniteApi.Database.CompletionStatuses[settings.CompletedId] },
                { EntryStatus.Dropped, playniteApi.Database.CompletionStatuses[settings.DroppedId] },
                { EntryStatus.Paused, playniteApi.Database.CompletionStatuses[settings.PausedId] },
                { EntryStatus.Repeating, playniteApi.Database.CompletionStatuses[settings.RewatchingId] }
            };
        }

        private void ProcessEntriesResponse(List<Game> importedGames, Dictionary<string, int> libraryCache, Dictionary<string, Tag> tagsCache, Dictionary<string, Genre> genresCache, Dictionary<string, Company> companiesCache, List<Entry> anilistUserEntry, Dictionary<string, Game> alreadyImportedEntries)
        {
            foreach (var entry in anilistUserEntry)
            {
                var mediaId = entry.Media.Id.ToString();
                // For some reason there was a report of repeated mediaId in received AniList response
                if (libraryCache.ContainsKey(mediaId))
                {
                    logger.Warn($"Library cache already contained mediaId with key {mediaId}. Current entryId {libraryCache[mediaId]} |  New {entry.Id}");
                    continue;
                }

                libraryCache.Add(mediaId, entry.Id);
                if (alreadyImportedEntries.TryGetValue(mediaId, out var existingEntry))
                {
                    UpdateExistingEntry(entry, existingEntry, tagsCache, genresCache, companiesCache);
                }
                else
                {
                    importedGames.Add(playniteApi.Database.ImportGame(EntryToGameMetadata(entry), plugin));
                }
            }
        }

        private void UpdateExistingEntry(Entry entry, Game existingEntry, Dictionary<string, Tag> tagsCache, Dictionary<string, Genre> genresCache, Dictionary<string, Company> companiesCache)
        {
            var shouldUpdateGame = false;
            AnilistResponseHelper.ApplyPrefixToMediaProperties(entry.Media, settings.PropertiesPrefix);

            //Scores
            if (settings.UpdateUserScoreOnLibUpdate && entry.Score != 0 && entry.Score != existingEntry.UserScore)
            {
                existingEntry.UserScore = entry.Score;
                shouldUpdateGame = true;
            }

            if (entry.Media.AverageScore.HasValue && entry.Media.AverageScore.Value > 0 && entry.Media.AverageScore.Value != existingEntry.CommunityScore)
            {
                existingEntry.CommunityScore = entry.Media.AverageScore;
                shouldUpdateGame = true;
            }

            //Genres
            if (entry.Media.Genres.HasItems())
            {
                var nonMatchingGenres = entry.Media.Genres
                    .Where(genre => !existingEntry.Genres.HasItems() || !existingEntry.Genres.Any(existingGenre => existingGenre.Name == genre));
                foreach (var genreName in nonMatchingGenres)
                {
                    if (!genresCache.ContainsKey(genreName))
                    {
                        genresCache.Add(genreName, playniteApi.Database.Genres.Add(genreName));
                    }

                    PlayniteUtilities.AddGenreToGame(playniteApi, existingEntry, genresCache[genreName], false);
                    shouldUpdateGame = true;
                }
            }

            //Developers and Publishers
            if (entry.Media.Type == TypeEnum.Manga && entry.Media.Staff?.Nodes?.HasItems() == true)
            {
                var nonMatchingItems = entry.Media.Staff.Nodes
                    .Where(x => !existingEntry.Developers.HasItems() || !existingEntry.Developers.Any(existingItem => existingItem.Name == x.Name.Full));
                foreach (var item in nonMatchingItems)
                {
                    if (!companiesCache.ContainsKey(item.Name.Full))
                    {
                        companiesCache.Add(item.Name.Full, playniteApi.Database.Companies.Add(item.Name.Full));
                    }

                    var company = companiesCache[item.Name.Full];
                    if (existingEntry.DeveloperIds is null)
                    {
                        existingEntry.DeveloperIds = new List<Guid> { company.Id };
                    }
                    else
                    {
                        existingEntry.DeveloperIds.Add(company.Id);
                    }

                    shouldUpdateGame = true;
                }
            }
            else if (entry.Media.Type == TypeEnum.Anime && entry.Media.Studios?.Nodes?.HasItems() == true)
            {
                var studios = entry.Media.Studios?.Nodes?.Where(s => s.IsAnimationStudio);
                if (studios.HasItems())
                {
                    var nonMatchingItems = studios
                        .Where(x => !existingEntry.Developers.HasItems() || !existingEntry.Developers.Any(existingItem => existingItem.Name == x.Name));
                    foreach (var itemName in nonMatchingItems)
                    {
                        if (!companiesCache.ContainsKey(itemName.Name))
                        {
                            companiesCache.Add(itemName.Name, playniteApi.Database.Companies.Add(itemName.Name));
                        }

                        var company = companiesCache[itemName.Name];
                        if (existingEntry.DeveloperIds is null)
                        {
                            existingEntry.DeveloperIds = new List<Guid> { company.Id };
                        }
                        else
                        {
                            existingEntry.DeveloperIds.Add(company.Id);
                        }

                        shouldUpdateGame = true;
                    }
                }

                var publishers = entry.Media.Studios?.Nodes?.Where(s => !s.IsAnimationStudio);
                if (publishers.HasItems())
                {
                    var nonMatchingItems = studios
                        .Where(x => !existingEntry.Publishers.HasItems() || !existingEntry.Publishers.Any(existingItem => existingItem.Name == x.Name));
                    foreach (var itemName in nonMatchingItems)
                    {
                        if (!companiesCache.ContainsKey(itemName.Name))
                        {
                            companiesCache.Add(itemName.Name, playniteApi.Database.Companies.Add(itemName.Name));
                        }

                        var company = companiesCache[itemName.Name];
                        if (existingEntry.PublisherIds is null)
                        {
                            existingEntry.PublisherIds = new List<Guid> { company.Id };
                        }
                        else
                        {
                            existingEntry.PublisherIds.Add(company.Id);
                        }

                        shouldUpdateGame = true;
                    }
                }
            }

            //Progress
            if (settings.UpdateProgressOnLibUpdate)
            {
                var versionString = GetEntryVersionString(entry);
                if (!existingEntry.Version.Equals(versionString))
                {
                    existingEntry.Version = versionString;
                    shouldUpdateGame = true;
                }
            }

            //LastActivity
            if (settings.UpdateLastActivityOnLibUpdate && entry.UpdatedAt != 0)
            {
                var updatedTime = DateTimeOffset.FromUnixTimeSeconds(entry.UpdatedAt).LocalDateTime;
                if (existingEntry.LastActivity is null || updatedTime > existingEntry.LastActivity)
                {
                    existingEntry.LastActivity = updatedTime;
                    shouldUpdateGame = true;
                }
            }

            //Tags
            if (entry.Media.Tags.HasItems())
            {
                var nonMatchingTags = entry.Media.Tags.Where(x => !x.IsMediaSpoiler && !x.IsGeneralSpoiler);
                if (existingEntry.Tags.HasItems())
                {
                    var tagNames = new HashSet<string>(existingEntry.Tags.Select(tag => tag.Name));
                    nonMatchingTags = nonMatchingTags.Where(tag => !tagNames.Contains(tag.Name));
                }

                foreach (var mediaTag in nonMatchingTags)
                {
                    if (!tagsCache.ContainsKey(mediaTag.Name))
                    {
                        tagsCache.Add(mediaTag.Name, playniteApi.Database.Tags.Add(mediaTag.Name));
                    }

                    PlayniteUtilities.AddTagToGame(playniteApi, existingEntry, tagsCache[mediaTag.Name], false);
                    shouldUpdateGame = true;
                }
            }

            if (entry.Media.Status.HasValue)
            {
                var progressTagName = $"{settings.PropertiesPrefix}Status: {entry.Media.Status}";
                if (existingEntry.TagIds is null)
                {
                    existingEntry.TagIds = new List<Guid>() { playniteApi.Database.Tags.Add(progressTagName).Id };
                    shouldUpdateGame = true;
                }
                else
                {
                    var tagStartStr = $"{settings.PropertiesPrefix}Status: ";
                    var existingProgressTag = existingEntry.Tags.FirstOrDefault(x => x.Name.StartsWith(tagStartStr));
                    if (existingProgressTag is null)
                    {
                        existingEntry.TagIds.Add(playniteApi.Database.Tags.Add(progressTagName).Id);
                        shouldUpdateGame = true;
                    }
                    else if (existingProgressTag.Name != progressTagName)
                    {
                        existingEntry.TagIds.Remove(existingProgressTag.Id);
                        existingEntry.TagIds.Add(playniteApi.Database.Tags.Add(progressTagName).Id);
                        shouldUpdateGame = true;
                    }
                }
            }

            if (entry.Media.Season != null)
            {
                var seasonTagName = $"{settings.PropertiesPrefix}Season: {entry.Media.Season}";
                if (!tagsCache.ContainsKey(seasonTagName))
                {
                    tagsCache.Add(seasonTagName, playniteApi.Database.Tags.Add(seasonTagName));
                }

                var tagAdded = PlayniteUtilities.AddTagToGame(playniteApi, existingEntry, tagsCache[seasonTagName], false);
                if (tagAdded)
                {
                    shouldUpdateGame = true;
                }
            }

            if (entry.Media.Format != null)
            {
                var formatTagName = $"{settings.PropertiesPrefix}Format: {entry.Media.Format}";
                if (!tagsCache.ContainsKey(formatTagName))
                {
                    tagsCache.Add(formatTagName, playniteApi.Database.Tags.Add(formatTagName));
                }

                var tagAdded = PlayniteUtilities.AddTagToGame(playniteApi, existingEntry, tagsCache[formatTagName], false);
                if (tagAdded)
                {
                    shouldUpdateGame = true;
                }
            }

            //Installation Status
            if (!existingEntry.IsInstalled)
            {
                existingEntry.IsInstalled = true;
                shouldUpdateGame = true;
            }

            //Completion Status
            if (settings.UpdateCompletionStatusOnLibUpdate && entry.Status != null)
            {
                if (UpdateGameCompletionStatusFromEntryStatus(existingEntry, entry.Status))
                {
                    shouldUpdateGame = true;
                }
            }

            if (shouldUpdateGame)
            {
                playniteApi.Database.Games.Update(existingEntry);
            }
        }

        public GameMetadata EntryToGameMetadata(Entry entry)
        {
            var gameMetadata = AnilistResponseHelper.MediaToGameMetadata(entry.Media, false, settings.PropertiesPrefix);
            if (entry.Score != 0 && gameMetadata.UserScore != entry.Score)
            {
                gameMetadata.UserScore = entry.Score;
            }

            if (entry.Status.HasValue && completionStatusMap.TryGetValue(entry.Status.Value, out var completionStatus))
            {
                if (completionStatus != null)
                {
                    gameMetadata.CompletionStatus = new MetadataNameProperty(completionStatus?.Name);
                }
            }

            if (entry.UpdatedAt != 0)
            {
                var newLastActivity = DateTimeOffset.FromUnixTimeSeconds(entry.UpdatedAt).LocalDateTime;
                if (gameMetadata.LastActivity != newLastActivity)
                {
                    gameMetadata.LastActivity = newLastActivity;
                }
            }

            if (settings.UpdateProgressOnLibUpdate)
            {
                var newVersionString = GetEntryVersionString(entry);
                if (gameMetadata.Version != newVersionString)
                {
                    gameMetadata.Version = newVersionString;
                }
            }

            return gameMetadata;
        }

        private string GetEntryVersionString(Entry entry)
        {
            var totalLength = 0;
            var progressPercentageFormat = "???";
            var totalLengthString = "???";

            if (entry.Media.Type == TypeEnum.Manga && entry.Media.Chapters.HasValue)
            {
                totalLength = entry.Media.Chapters.Value;
                totalLengthString = totalLength.ToString("D3");
            }
            else if (entry.Media.Type == TypeEnum.Anime && entry.Media.Episodes.HasValue)
            {
                totalLength = entry.Media.Episodes.Value;
                totalLengthString = totalLength.ToString("D3");
            }

            if (totalLength > 0)
            {
                var percentage = ((entry.Progress * 100) / totalLength);
                progressPercentageFormat = $"{percentage:D3}%";
            }

            return $"({progressPercentageFormat}) {entry.Progress:D3}/{totalLengthString}";
        }

        internal bool UpdateGameCompletionStatusFromEntryStatus(Game existingEntry, EntryStatus? entryStatus)
        {
            if (entryStatus.HasValue && completionStatusMap.TryGetValue(entryStatus.Value, out var completionStatus))
            {
                if (completionStatus != null && existingEntry.CompletionStatusId != completionStatus.Id)
                {
                    existingEntry.CompletionStatusId = completionStatus.Id;
                    return true;
                }
            }

            return false;
        }

        public void UpdateGamesCompletionStatus(List<Game> games, EntryStatus entryStatus)
        {
            using (playniteApi.Database.BufferedUpdate())
            {
                playniteApi.Dialogs.ActivateGlobalProgress((a) =>
                {
                    if (!idsCache.HasItems())
                    {
                        if (!FileSystem.FileExists(anilistLibraryCachePath))
                        {
                            logger.Debug("Cache not set and cache file not found");
                            return;
                        }

                        idsCache = Serialization.FromJsonFile<Dictionary<string, int>>(anilistLibraryCachePath);
                    }

                    if (!anilistService.GetIsLoggedIn())
                    {
                        //Username could not be obtained
                        playniteApi.Notifications.Add(new NotificationMessage(
                            dbImportMessageId,
                            ResourceProvider.GetString("LOCImporter_For_Anilist_NotificationMessageAniListUsernameNotObtained"),
                            NotificationType.Error));

                        return;
                    }

                    var gamesToUpdate = new Dictionary<int, Game>();
                    foreach (var game in games.Distinct())
                    {
                        if (game.PluginId != plugin.Id)
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

                    var updateIdsResult = anilistService.UpdateEntriesStatuses(gamesToUpdate.Select(x => x.Key).ToList(), entryStatus);
                    if (updateIdsResult is null)
                    {
                        return;
                    }

                    InitializeStatuses();
                    foreach (var gameToUpdate in gamesToUpdate)
                    {
                        var updatedEntry = updateIdsResult.Data.UpdateMediaListEntries.FirstOrDefault(x => x.Id == gameToUpdate.Key);
                        if (updatedEntry is null)
                        {
                            logger.Debug($"Updated entry not found. {gameToUpdate.Value.Name}, {gameToUpdate.Value.GameId}, {gameToUpdate.Key}");
                            continue;
                        }

                        var updatedCompletionStatus = UpdateGameCompletionStatusFromEntryStatus(gameToUpdate.Value, updatedEntry.Status);
                        if (updatedCompletionStatus)
                        {
                            playniteApi.Database.Games.Update(gameToUpdate.Value);
                        }
                    }

                }, new GlobalProgressOptions(ResourceProvider.GetString("LOCImporter_For_Anilist_DialogMessageUpdatingEntriesStatuses"), true));
            }

        }

    }
}