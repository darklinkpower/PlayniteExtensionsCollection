using Playnite.SDK;
using Playnite.SDK.Models;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace ExtraMetadataLoader.MetadataProviders.LaunchBox
{
    internal class LaunchBoxClearLogoProvider : ILogoProvider
    {
        // Metadata.zip stores image filenames; LaunchBox currently serves those files from this CDN host.
        private const string ImageBaseUrl = "https://images.launchbox-app.com/";
        private const int MaxCandidateResults = 25;
        private const int AutomaticMinimumScore = 110;
        private const int AutomaticMinimumScoreGap = 10;
        private static readonly TimeSpan ImageProbeTimeout = TimeSpan.FromSeconds(15);
        private readonly IPlayniteAPI _playniteApi;
        private readonly ExtraMetadataLoaderSettings _settings;
        private readonly ILogger _logger;
        private readonly LaunchBoxMetadataCache _metadataCache;

        public string Id => "launchBoxProvider";

        public LaunchBoxClearLogoProvider(
            IPlayniteAPI playniteApi,
            ExtraMetadataLoaderSettings settings,
            ILogger logger,
            LaunchBoxMetadataCache metadataCache)
        {
            _playniteApi = playniteApi;
            _settings = settings;
            _logger = logger;
            _metadataCache = metadataCache;
        }

        public string GetLogoUrl(Game game, LogoDownloadOptions downloadOptions, CancellationToken cancelToken = default)
        {
            if (downloadOptions.IsLibraryUpdateDownload && !_settings.UseLaunchBoxForAutomaticLogoDownloads)
            {
                _logger.Debug("LaunchBox automatic logo download skipped because the setting is disabled.");
                return null;
            }

            if (cancelToken.IsCancellationRequested)
            {
                return null;
            }

            var index = _metadataCache.GetIndex(cancelToken);
            if (index?.Games == null || index.Games.Count == 0)
            {
                _logger.Debug("LaunchBox clear-logo lookup skipped because no metadata index is available.");
                return null;
            }

            var matches = GetMatches(game, index.Games, game.Name).Take(MaxCandidateResults).ToList();
            if (matches.Count == 0)
            {
                _logger.Debug($"LaunchBox clear-logo lookup returned no candidates for '{game.Name}'.");
                return null;
            }

            LaunchBoxMatch selectedMatch;
            if (downloadOptions.IsBackgroundDownload)
            {
                selectedMatch = GetAutomaticMatch(game, matches);
            }
            else
            {
                selectedMatch = GetManualMatch(game, index, matches);
            }

            if (selectedMatch == null)
            {
                return null;
            }

            var logoUrl = downloadOptions.IsBackgroundDownload
                ? GetBestLogoUrl(selectedMatch.Game, cancelToken)
                : GetLogoUrlFromSelection(game, selectedMatch.Game, cancelToken);

            if (logoUrl.IsNullOrWhiteSpace())
            {
                _logger.Debug($"LaunchBox clear-logo lookup found no usable image URL for '{game.Name}' from '{FormatMatchForLog(selectedMatch)}'.");
                return null;
            }

            _logger.Debug($"LaunchBox clear-logo URL selected for '{game.Name}' from '{FormatMatchForLog(selectedMatch)}'.");
            return logoUrl;
        }

        private LaunchBoxMatch GetAutomaticMatch(Game game, List<LaunchBoxMatch> matches)
        {
            var bestMatch = matches.FirstOrDefault();
            if (bestMatch == null)
            {
                return null;
            }

            var runnerUp = matches.Skip(1).FirstOrDefault();
            var scoreGap = runnerUp == null ? int.MaxValue : bestMatch.Score - runnerUp.Score;
            if ((bestMatch.Score >= AutomaticMinimumScore || bestMatch.NameScore >= 95) &&
                scoreGap >= AutomaticMinimumScoreGap)
            {
                return bestMatch;
            }

            _logger.Debug($"LaunchBox automatic logo lookup skipped ambiguous match for '{game.Name}'. Top score: {bestMatch.Score}. Runner-up score: {runnerUp?.Score.ToString(CultureInfo.InvariantCulture) ?? "none"}.");
            return null;
        }

        private LaunchBoxMatch GetManualMatch(Game game, LaunchBoxMetadataIndex index, List<LaunchBoxMatch> matches)
        {
            var bestMatch = matches.First();
            var runnerUp = matches.Skip(1).FirstOrDefault();
            if (runnerUp == null || bestMatch.Score - runnerUp.Score >= AutomaticMinimumScoreGap && bestMatch.NameScore >= 95)
            {
                return bestMatch;
            }

            var selectedResult = _playniteApi.Dialogs.ChooseItemWithSearch(
                ToGenericItemOptions(matches),
                searchTerm => ToGenericItemOptions(GetMatches(game, index.Games, searchTerm).Take(MaxCandidateResults)),
                game.Name.NormalizeGameName(),
                ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectGame"));

            if (selectedResult == null || selectedResult.Description.IsNullOrWhiteSpace())
            {
                return null;
            }

            var selectedMatch = GetMatches(game, index.Games, selectedResult.Name).FirstOrDefault(x => x.Game.DatabaseId == selectedResult.Description);
            if (selectedMatch != null)
            {
                return selectedMatch;
            }

            var selectedGame = index.Games.FirstOrDefault(x => x.DatabaseId == selectedResult.Description);
            if (selectedGame == null)
            {
                return null;
            }

            return new LaunchBoxMatch
            {
                Game = selectedGame,
                Score = 0,
                NameScore = 0,
                MatchReason = "manual selection"
            };
        }

        private string GetBestLogoUrl(LaunchBoxGameEntry game, CancellationToken cancelToken)
        {
            foreach (var logo in GetOrderedLogos(game))
            {
                var url = GetImageUrl(logo.FileName);
                if (IsImageUrlAvailable(url, logo.FileName, cancelToken))
                {
                    return url;
                }
            }

            return null;
        }

        private string GetLogoUrlFromSelection(Game playniteGame, LaunchBoxGameEntry launchBoxGame, CancellationToken cancelToken)
        {
            var logos = GetOrderedLogos(launchBoxGame).ToList();
            if (logos.Count == 0)
            {
                return null;
            }

            if (logos.Count == 1)
            {
                var singleUrl = GetImageUrl(logos[0].FileName);
                return IsImageUrlAvailable(singleUrl, logos[0].FileName, cancelToken) ? singleUrl : null;
            }

            var imageFileOptions = new List<ImageFileOption>();
            foreach (var logo in logos)
            {
                var url = GetImageUrl(logo.FileName);
                if (IsImageUrlAvailable(url, logo.FileName, cancelToken))
                {
                    imageFileOptions.Add(new ImageFileOption
                    {
                        Path = url
                    });
                }
            }

            if (imageFileOptions.Count == 0)
            {
                return null;
            }

            var selectedOption = _playniteApi.Dialogs.ChooseImageFile(
                imageFileOptions,
                string.Format(ResourceProvider.GetString("LOCExtra_Metadata_Loader_DialogCaptionSelectLogo"), playniteGame.Name));

            return selectedOption?.Path;
        }

        private IEnumerable<LaunchBoxMatch> GetMatches(Game game, IEnumerable<LaunchBoxGameEntry> games, string searchTerm)
        {
            if (searchTerm.IsNullOrWhiteSpace())
            {
                searchTerm = game.Name;
            }

            var gamePlatforms = GetGamePlatforms(game);
            var gameReleaseYear = GetGameReleaseYear(game);
            var developers = GetCompanyNames(game.Developers);
            var publishers = GetCompanyNames(game.Publishers);

            return games
                .Select(x => ScoreMatch(searchTerm, gamePlatforms, gameReleaseYear, developers, publishers, x))
                .Where(x => x != null)
                .GroupBy(x => x.Game.DatabaseId)
                .Select(x => x.OrderByDescending(y => y.Score).First())
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.NameScore)
                .ThenBy(x => x.Game.Name)
                .ThenBy(x => x.Game.Platform);
        }

        private LaunchBoxMatch ScoreMatch(
            string searchTerm,
            List<string> gamePlatforms,
            int? gameReleaseYear,
            List<string> developers,
            List<string> publishers,
            LaunchBoxGameEntry launchBoxGame)
        {
            string matchReason;
            var nameScore = GetNameScore(searchTerm, launchBoxGame, out matchReason);
            if (nameScore < 70)
            {
                return null;
            }

            var score = nameScore;
            score += GetPlatformScore(gamePlatforms, launchBoxGame.Platform);
            score += GetReleaseYearScore(gameReleaseYear, launchBoxGame.ReleaseYear);
            score += GetCompanyScore(developers, launchBoxGame.Developer);
            score += GetCompanyScore(publishers, launchBoxGame.Publisher);

            return new LaunchBoxMatch
            {
                Game = launchBoxGame,
                Score = score,
                NameScore = nameScore,
                MatchReason = matchReason
            };
        }

        private int GetNameScore(string searchTerm, LaunchBoxGameEntry launchBoxGame, out string matchReason)
        {
            matchReason = null;
            var searchName = searchTerm.NormalizeGameName();
            var searchKey = NormalizeForMatch(searchTerm);
            if (searchKey.IsNullOrWhiteSpace())
            {
                return 0;
            }

            var launchBoxName = launchBoxGame.Name.NormalizeGameName();
            var launchBoxKey = NormalizeForMatch(launchBoxGame.Name);
            if (searchKey == launchBoxKey)
            {
                matchReason = "exact title";
                return 100;
            }

            foreach (var alternateName in launchBoxGame.AlternateNames ?? new List<string>())
            {
                if (searchKey == NormalizeForMatch(alternateName))
                {
                    matchReason = "exact alternate title";
                    return 98;
                }
            }

            if (searchKey.Length > 3 && launchBoxKey.Contains(searchKey))
            {
                matchReason = "contained title";
                return 84;
            }

            if (launchBoxKey.Length > 3 && searchKey.Contains(launchBoxKey))
            {
                matchReason = "contained title";
                return 82;
            }

            if (searchName.MatchesAllWords(launchBoxName) || launchBoxName.MatchesAllWords(searchName))
            {
                matchReason = "word match";
                return 78;
            }

            var similarity = searchName.GetJaroWinklerSimilarityIgnoreCase(launchBoxName);
            if (similarity >= 0.94)
            {
                matchReason = "very similar title";
                return 88;
            }

            if (similarity >= 0.88)
            {
                matchReason = "similar title";
                return 72;
            }

            return 0;
        }

        private static List<string> GetGamePlatforms(Game game)
        {
            if (game.Platforms == null || game.Platforms.Count == 0)
            {
                return new List<string>();
            }

            return game.Platforms
                .Where(x => !x.Name.IsNullOrWhiteSpace())
                .Select(x => NormalizePlatformName(x.Name))
                .Distinct()
                .ToList();
        }

        private static int? GetGameReleaseYear(Game game)
        {
            if (!game.ReleaseDate.HasValue)
            {
                return null;
            }

            var year = game.ReleaseDate.Value.Year;
            return year > 0 ? year : (int?)null;
        }

        private static List<string> GetCompanyNames(IEnumerable<Company> companies)
        {
            if (companies == null)
            {
                return new List<string>();
            }

            return companies
                .Where(x => !x.Name.IsNullOrWhiteSpace())
                .Select(x => NormalizeForMatch(x.Name))
                .Distinct()
                .ToList();
        }

        private static int GetPlatformScore(List<string> gamePlatforms, string launchBoxPlatform)
        {
            if (gamePlatforms.Count == 0 || launchBoxPlatform.IsNullOrWhiteSpace())
            {
                return 0;
            }

            var normalizedLaunchBoxPlatform = NormalizePlatformName(launchBoxPlatform);
            if (gamePlatforms.Contains(normalizedLaunchBoxPlatform))
            {
                return 18;
            }

            if (gamePlatforms.Any(x => x.Contains(normalizedLaunchBoxPlatform) || normalizedLaunchBoxPlatform.Contains(x)))
            {
                return 8;
            }

            return -8;
        }

        private static int GetReleaseYearScore(int? playniteYear, int? launchBoxYear)
        {
            if (!playniteYear.HasValue || !launchBoxYear.HasValue)
            {
                return 0;
            }

            var difference = Math.Abs(playniteYear.Value - launchBoxYear.Value);
            if (difference == 0)
            {
                return 8;
            }

            if (difference == 1)
            {
                return 4;
            }

            return -3;
        }

        private static int GetCompanyScore(List<string> playniteCompanies, string launchBoxCompany)
        {
            if (playniteCompanies.Count == 0 || launchBoxCompany.IsNullOrWhiteSpace())
            {
                return 0;
            }

            var normalizedLaunchBoxCompany = NormalizeForMatch(launchBoxCompany);
            foreach (var company in playniteCompanies)
            {
                if (company == normalizedLaunchBoxCompany)
                {
                    return 4;
                }

                if (normalizedLaunchBoxCompany.Contains(company) || company.Contains(normalizedLaunchBoxCompany))
                {
                    return 2;
                }
            }

            return 0;
        }

        private static string NormalizeForMatch(string value)
        {
            return value?.NormalizeGameName().Satinize() ?? string.Empty;
        }

        private static string NormalizePlatformName(string platformName)
        {
            var normalized = NormalizeForMatch(platformName);
            switch (normalized)
            {
                case "pc":
                case "pcwindows":
                case "windows":
                case "microsoftwindows":
                case "ibmpccompatible":
                    return "windows";
                case "mac":
                case "macintosh":
                case "macos":
                case "applemacos":
                    return "macos";
                case "sonyplaystation":
                case "playstation":
                case "ps1":
                case "psx":
                    return "playstation";
                case "sonyplaystation2":
                case "playstation2":
                case "ps2":
                    return "playstation2";
                case "sonyplaystation3":
                case "playstation3":
                case "ps3":
                    return "playstation3";
                case "sonyplaystation4":
                case "playstation4":
                case "ps4":
                    return "playstation4";
                case "sonyplaystation5":
                case "playstation5":
                case "ps5":
                    return "playstation5";
                case "microsoftxbox":
                case "xbox":
                    return "xbox";
                case "microsoftxbox360":
                case "xbox360":
                    return "xbox360";
                case "microsoftxboxone":
                case "xboxone":
                    return "xboxone";
                case "microsoftxboxseriesxs":
                case "xboxseriesxs":
                case "xboxseriesx":
                    return "xboxseriesxs";
                case "nintendones":
                case "nintendoentertainmentsystem":
                    return "nintendoentertainmentsystem";
                case "supernintendo":
                case "supernintendoentertainmentsystem":
                case "snes":
                    return "supernintendoentertainmentsystem";
                case "nintendo64":
                case "n64":
                    return "nintendo64";
                case "nintendogamecube":
                case "gamecube":
                    return "nintendogamecube";
                case "nintendowii":
                case "wii":
                    return "nintendowii";
                case "nintendowiiu":
                case "wiiu":
                    return "nintendowiiu";
                case "nintendoswitch":
                case "switch":
                    return "nintendoswitch";
                case "segagenesis":
                case "segamegadrive":
                case "megadrive":
                case "genesis":
                    return "segagenesis";
                default:
                    return normalized;
            }
        }

        private static IEnumerable<LaunchBoxLogoEntry> GetOrderedLogos(LaunchBoxGameEntry game)
        {
            return (game.Logos ?? new List<LaunchBoxLogoEntry>())
                .Where(x => LaunchBoxMetadataCache.IsValidImageFileName(x.FileName))
                .GroupBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .OrderByDescending(GetRegionScore)
                .ThenByDescending(GetLogoArea)
                .ThenBy(x => x.FileName);
        }

        private static int GetRegionScore(LaunchBoxLogoEntry logo)
        {
            if (logo.Region.IsNullOrWhiteSpace() || logo.Region.Equals("World", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            return 1;
        }

        private static long GetLogoArea(LaunchBoxLogoEntry logo)
        {
            if (logo.Width.HasValue && logo.Height.HasValue)
            {
                return (long)logo.Width.Value * logo.Height.Value;
            }

            return 0;
        }

        private static string GetImageUrl(string fileName)
        {
            return ImageBaseUrl + Uri.EscapeDataString(fileName);
        }

        private bool IsImageUrlAvailable(string url, string fileName, CancellationToken cancelToken)
        {
            try
            {
                using (var handler = new HttpClientHandler { AllowAutoRedirect = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = ImageProbeTimeout;
                    var response = SendImageProbe(client, HttpMethod.Head, url, cancelToken);
                    if (response.StatusCode == HttpStatusCode.MethodNotAllowed || response.StatusCode == HttpStatusCode.NotImplemented)
                    {
                        response.Dispose();
                        response = SendImageProbe(client, HttpMethod.Get, url, cancelToken);
                    }

                    using (response)
                    {
                        if (response.StatusCode == (HttpStatusCode)429)
                        {
                            _logger.Debug($"LaunchBox image request was rate limited for '{fileName}'.");
                            return false;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.Debug($"LaunchBox image URL failed for '{fileName}' with HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");
                            return false;
                        }

                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        if (contentType.IsNullOrWhiteSpace())
                        {
                            return LaunchBoxMetadataCache.IsValidImageFileName(fileName);
                        }

                        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        _logger.Debug($"LaunchBox image URL skipped for '{fileName}' because content type '{contentType}' is not an image.");
                        return false;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Debug($"LaunchBox image URL validation failed for '{fileName}'. Error: {ex.Message}");
                return false;
            }
        }

        private static HttpResponseMessage SendImageProbe(HttpClient client, HttpMethod method, string url, CancellationToken cancelToken)
        {
            using (var request = new HttpRequestMessage(method, url))
            {
                request.Headers.TryAddWithoutValidation("User-Agent", "ExtraMetadataLoader LaunchBoxProvider");
                request.Headers.TryAddWithoutValidation("Accept", "image/*,*/*;q=0.8");
                return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelToken).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private static List<GenericItemOption> ToGenericItemOptions(IEnumerable<LaunchBoxMatch> matches)
        {
            return matches
                .Select(x => new GenericItemOption(FormatMatchForSelection(x), x.Game.DatabaseId))
                .ToList();
        }

        private static string FormatMatchForSelection(LaunchBoxMatch match)
        {
            var parts = new List<string>();
            if (!match.Game.Platform.IsNullOrWhiteSpace())
            {
                parts.Add(match.Game.Platform);
            }

            if (match.Game.ReleaseYear.HasValue)
            {
                parts.Add(match.Game.ReleaseYear.Value.ToString(CultureInfo.InvariantCulture));
            }

            var suffix = parts.Count > 0 ? $" ({string.Join(", ", parts)})" : string.Empty;
            return $"{match.Game.Name}{suffix}";
        }

        private static string FormatMatchForLog(LaunchBoxMatch match)
        {
            return $"{FormatMatchForSelection(match)} [{match.MatchReason}, score {match.Score}, id {match.Game.DatabaseId}]";
        }
    }
}
