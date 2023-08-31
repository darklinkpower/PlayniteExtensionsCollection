using ImporterforAnilist.Models;
using ImporterforAnilist.Services;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
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
            if (settings.AccountAccessCode.IsNullOrEmpty())
            {
                playniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    ResourceProvider.GetString("LOCImporter_For_Anilist_NotificationMessageAccessCodeNotConfigured"),
                    NotificationType.Error));

                return importedGames;
            }

            if (!anilistService.GetIsLoggedIn())
            {
                //Username could not be obtained
                playniteApi.Notifications.Add(new NotificationMessage(
                    dbImportMessageId,
                    ResourceProvider.GetString("LOCImporter_For_Anilist_NotificationMessageAniListUsernameNotObtained"),
                    NotificationType.Error));

                return importedGames;
            }

            InitializeStatuses();
            var libraryCache = new Dictionary<string, int>();
            if (settings.ImportAnimeLibrary)
            {
                var animeEntries = anilistService.GetEntries("ANIME");
                logger.Debug($"Found {animeEntries.Count} Anime items");
                ProcessEntriesResponse(importedGames, libraryCache, animeEntries);
            }

            if (settings.ImportMangaLibrary)
            {
                var mangaEntries = anilistService.GetEntries("MANGA");
                logger.Debug($"Found {mangaEntries.Count} Manga items");
                ProcessEntriesResponse(importedGames, libraryCache, mangaEntries);
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

        private void ProcessEntriesResponse(List<Game> importedGames, Dictionary<string, int> libraryCache, List<Entry> anilistUserEntry)
        {
            foreach (var entry in anilistUserEntry)
            {
                var mediaId = entry.Media.Id.ToString();
                // For some reason there was a report of repeated mediaId
                if (libraryCache.ContainsKey(mediaId))
                {
                    logger.Warn($"Library cache already contained mediaId with key {mediaId}. Current entryId {libraryCache[mediaId]} |  New {entry.Id}");
                    continue;
                }

                libraryCache.Add(mediaId, entry.Id);
                var existingEntry = playniteApi.Database.Games.FirstOrDefault(a => a.PluginId == plugin.Id && a.GameId == mediaId);
                if (existingEntry != null)
                {
                    UpdateExistingEntry(entry, existingEntry);
                }
                else
                {
                    importedGames.Add(playniteApi.Database.ImportGame(EntryToGameMetadata(entry), plugin));
                }
            }
        }

        private void UpdateExistingEntry(Entry entry, Game existingEntry)
        {
            var updateGame = false;
            if (settings.UpdateUserScoreOnLibUpdate && entry.Score != 0 && entry.Score != existingEntry.UserScore)
            {
                existingEntry.UserScore = entry.Score;
                updateGame = true;
            }

            if (settings.UpdateProgressOnLibUpdate)
            {
                var versionString = GetEntryVersionString(entry);
                if (!existingEntry.Version.Equals(versionString))
                {
                    existingEntry.Version = versionString;
                    updateGame = true;
                }
            }

            if (settings.UpdateLastActivityOnLibUpdate && entry.UpdatedAt != 0)
            {
                var updatedTime = DateTimeOffset.FromUnixTimeSeconds(entry.UpdatedAt).LocalDateTime;
                if (existingEntry.LastActivity is null || updatedTime > existingEntry.LastActivity)
                {
                    existingEntry.LastActivity = updatedTime;
                    updateGame = true;
                }
            }

            var progressTagName = $"{settings.PropertiesPrefix}Status: {entry.Media.Status}";
            if (existingEntry.TagIds is null)
            {
                existingEntry.TagIds = new List<Guid>() { playniteApi.Database.Tags.Add(progressTagName).Id };
            }
            else
            {
                var tagStartStr = $"{settings.PropertiesPrefix}Status: ";
                var progressTag = existingEntry.Tags.FirstOrDefault(x => x.Name.StartsWith(tagStartStr));
                if (progressTag is null)
                {
                    existingEntry.TagIds.Add(playniteApi.Database.Tags.Add(progressTagName).Id);
                    updateGame = true;
                }
                else if (progressTag.Name != progressTagName)
                {
                    existingEntry.TagIds.Remove(progressTag.Id);
                    existingEntry.TagIds.Add(playniteApi.Database.Tags.Add(progressTagName).Id);
                    updateGame = true;
                }
            }

            if (!existingEntry.IsInstalled)
            {
                existingEntry.IsInstalled = true;
                updateGame = true;
            }

            if (settings.UpdateCompletionStatusOnLibUpdate && entry.Status != null)
            {
                if (UpdateGameCompletionStatusFromEntryStatus(existingEntry, entry.Status))
                {
                    updateGame = true;
                }
            }

            if (updateGame)
            {
                playniteApi.Database.Games.Update(existingEntry);
            }
        }

        public GameMetadata EntryToGameMetadata(Entry entry)
        {
            var game = AnilistResponseHelper.MediaToGameMetadata(entry.Media, false, settings.PropertiesPrefix);
            if (entry.Score != 0 && game.UserScore != entry.Score)
            {
                game.UserScore = entry.Score;
            }

            if (entry.Status.HasValue && completionStatusMap.TryGetValue(entry.Status.Value, out var completionStatus))
            {
                if (completionStatus != null)
                {
                    game.CompletionStatus = new MetadataNameProperty(completionStatus?.Name);
                }
            }

            if (entry.UpdatedAt != 0)
            {
                var newLastActivity = DateTimeOffset.FromUnixTimeSeconds(entry.UpdatedAt).LocalDateTime;
                if (game.LastActivity != newLastActivity)
                {
                    game.LastActivity = newLastActivity;
                }
            }

            if (settings.UpdateProgressOnLibUpdate)
            {
                var newVersionString = GetEntryVersionString(entry);
                if (game.Version != newVersionString)
                {
                    game.Version = newVersionString;
                }
            }

            return game;
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