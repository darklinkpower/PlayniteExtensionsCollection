using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace ExtraMetadataLoader.MetadataProviders
{
    internal class EmuMoviesVideoService
    {
        public const string ProviderId = "emuMoviesProvider";

        private static bool _missingCredentialsNotificationShown;

        private readonly IPlayniteAPI _playniteApi;
        private readonly ExtraMetadataLoaderSettings _settings;
        private readonly EmuMoviesCredentialsStore _credentialsStore;
        private readonly ILogger _logger;

        public EmuMoviesVideoService(
            IPlayniteAPI playniteApi,
            ExtraMetadataLoaderSettings settings,
            EmuMoviesCredentialsStore credentialsStore,
            ILogger logger)
        {
            _playniteApi = playniteApi;
            _settings = settings;
            _credentialsStore = credentialsStore;
            _logger = logger;
        }

        public string DownloadVideoToTempFile(Game game, bool isBackgroundDownload, bool selectAutomatically, CancellationToken cancelToken)
        {
            var credentials = _credentialsStore.Load();
            if (credentials?.IsConfigured != true)
            {
                NotifyMissingCredentials(isBackgroundDownload);
                return null;
            }

            var client = new EmuMoviesFtpClient(credentials.Username, credentials.Password, GetQualitySearchOrder(), _logger);
            var selectedMatch = GetSelectedMatch(game, game.Name, client, isBackgroundDownload, selectAutomatically, cancelToken);
            if (selectedMatch is null)
            {
                return null;
            }

            var tempFilePath = Path.Combine(
                Path.GetTempPath(),
                $"ExtraMetadataLoader_EmuMovies_{game.Id}_{Guid.NewGuid():N}{Path.GetExtension(selectedMatch.FileName)}");
            return client.DownloadVideo(selectedMatch.FtpPath, tempFilePath, cancelToken) ? tempFilePath : null;
        }

        private EmuMoviesVideoMatch GetSelectedMatch(
            Game game,
            string searchTerm,
            EmuMoviesFtpClient client,
            bool isBackgroundDownload,
            bool selectAutomatically,
            CancellationToken cancelToken)
        {
            var matches = GetMatches(game, searchTerm, client, cancelToken);
            if (!matches.HasItems())
            {
                return null;
            }

            var exactMatches = matches
                .Where(x => IsExactMatch(x.FileName, searchTerm))
                .ToList();
            if (exactMatches.Count == 1)
            {
                return exactMatches[0];
            }

            if (exactMatches.Count > 1 && (isBackgroundDownload || selectAutomatically))
            {
                return exactMatches[0];
            }

            if (!exactMatches.HasItems() && IsConfidentMatch(matches))
            {
                return matches[0];
            }

            if (isBackgroundDownload)
            {
                return null;
            }

            if (selectAutomatically)
            {
                return matches[0];
            }

            var selectedItem = _playniteApi.Dialogs.ChooseItemWithSearch(
                ToOptions(matches),
                x => ToOptions(GetMatches(game, x, client, CancellationToken.None)),
                game.Name.NormalizeGameName(),
                ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectGame"));

            return selectedItem is null
                ? null
                : matches.FirstOrDefault(x => x.FtpPath == selectedItem.Description) ??
                  FindMatchByFtpPath(game, selectedItem.Description, client, CancellationToken.None);
        }

        private EmuMoviesVideoMatch FindMatchByFtpPath(
            Game game,
            string ftpPath,
            EmuMoviesFtpClient client,
            CancellationToken cancelToken)
        {
            foreach (var platform in EmuMoviesPlatformMapper.GetEmuMoviesPlatforms(game))
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return null;
                }

                var videoFile = client.ListVideoFiles(platform, cancelToken)
                    .FirstOrDefault(x => x.FtpPath == ftpPath);
                if (videoFile != null)
                {
                    return new EmuMoviesVideoMatch
                    {
                        FileName = videoFile.FileName,
                        FtpPath = videoFile.FtpPath,
                        PlatformName = videoFile.PlatformName,
                        PlatformDirectoryName = videoFile.PlatformDirectoryName,
                        Quality = videoFile.Quality
                    };
                }
            }

            return null;
        }

        private List<EmuMoviesVideoMatch> GetMatches(
            Game game,
            string searchTerm,
            EmuMoviesFtpClient client,
            CancellationToken cancelToken)
        {
            var platforms = EmuMoviesPlatformMapper.GetEmuMoviesPlatforms(game);
            var matches = new List<EmuMoviesVideoMatch>();
            foreach (var platform in platforms)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                foreach (var videoFile in client.ListVideoFiles(platform, cancelToken))
                {
                    var score = GetMatchScore(videoFile.FileName, searchTerm);
                    if (score < 70)
                    {
                        continue;
                    }

                    matches.Add(new EmuMoviesVideoMatch
                    {
                        FileName = videoFile.FileName,
                        FtpPath = videoFile.FtpPath,
                        PlatformName = videoFile.PlatformName,
                        PlatformDirectoryName = videoFile.PlatformDirectoryName,
                        Quality = videoFile.Quality,
                        Score = score
                    });
                }
            }

            return matches
                .OrderByDescending(x => x.Score)
                .ThenBy(x => GetQualityPriority(x.Quality))
                .ThenBy(x => x.FileName)
                .ToList();
        }

        private List<EmuMoviesVideoQuality> GetQualitySearchOrder()
        {
            if (_settings.EmuMoviesPreferBestAvailableVideoQuality)
            {
                return new List<EmuMoviesVideoQuality>
                {
                    EmuMoviesVideoQuality.HD,
                    EmuMoviesVideoQuality.HQ,
                    EmuMoviesVideoQuality.SQ
                };
            }

            return new List<EmuMoviesVideoQuality> { _settings.EmuMoviesVideoQuality };
        }

        private static int GetQualityPriority(EmuMoviesVideoQuality quality)
        {
            switch (quality)
            {
                case EmuMoviesVideoQuality.HD:
                    return 0;
                case EmuMoviesVideoQuality.HQ:
                    return 1;
                case EmuMoviesVideoQuality.SQ:
                    return 2;
                default:
                    return 3;
            }
        }

        private static List<GenericItemOption> ToOptions(IEnumerable<EmuMoviesVideoMatch> matches)
        {
            return matches
                .Select(x => new GenericItemOption(x.DisplayName, x.FtpPath))
                .ToList();
        }

        private static bool IsConfidentMatch(List<EmuMoviesVideoMatch> matches)
        {
            if (!matches.HasItems())
            {
                return false;
            }

            var topScore = matches[0].Score;
            var nextScore = matches.Count > 1 ? matches[1].Score : 0;
            return topScore >= 96 || (topScore >= 90 && topScore - nextScore >= 8);
        }

        private static bool IsExactMatch(string fileName, string searchTerm)
        {
            return NormalizeMatchValue(fileName) == NormalizeMatchValue(searchTerm);
        }

        private static int GetMatchScore(string fileName, string searchTerm)
        {
            var fileNameNormalized = NormalizeMatchValue(fileName);
            var searchTermNormalized = NormalizeMatchValue(searchTerm);
            if (fileNameNormalized.IsNullOrWhiteSpace() || searchTermNormalized.IsNullOrWhiteSpace())
            {
                return 0;
            }

            if (fileNameNormalized == searchTermNormalized)
            {
                return 100;
            }

            if (fileNameNormalized.Contains(searchTermNormalized))
            {
                return 94;
            }

            if (searchTermNormalized.Contains(fileNameNormalized))
            {
                return 90;
            }

            var score = (int)Math.Round(fileNameNormalized.GetJaroWinklerSimilarityIgnoreCase(searchTermNormalized) * 100);
            if (fileNameNormalized.MatchesAllWords(searchTermNormalized))
            {
                score = Math.Max(score, 84);
            }

            return score;
        }

        private static string NormalizeMatchValue(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return string.Empty;
            }

            var name = Path.GetFileNameWithoutExtension(value);
            name = Regex.Replace(name, @"\s*[\(\[].*?[\)\]]\s*$", " ");
            return name.NormalizeGameName().Satinize();
        }

        private void NotifyMissingCredentials(bool isBackgroundDownload)
        {
            if (isBackgroundDownload || _missingCredentialsNotificationShown)
            {
                return;
            }

            _missingCredentialsNotificationShown = true;
            _playniteApi.Notifications.Add(new NotificationMessage(
                "ExtraMetadataLoaderEmuMoviesCredentialsMissing",
                ResourceProvider.GetString("LOCExtra_Metadata_Loader_NotificationMessageEmuMoviesCredentialsMissing"),
                NotificationType.Error));
        }
    }
}
