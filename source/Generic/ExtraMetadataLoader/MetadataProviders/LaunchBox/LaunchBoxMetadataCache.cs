using Playnite.SDK;
using Playnite.SDK.Data;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace ExtraMetadataLoader.MetadataProviders.LaunchBox
{
    internal class LaunchBoxMetadataCache
    {
        internal const string MetadataUrl = "https://gamesdb.launchbox-app.com/Metadata.zip";

        private const int CacheSchemaVersion = 1;
        private const string MetadataZipFileName = "Metadata.zip";
        private const string IndexFileName = "clearLogoIndex.json";
        private const string StateFileName = "clearLogoCacheState.json";
        private static readonly TimeSpan CacheCheckInterval = TimeSpan.FromDays(1);
        private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(10);
        private readonly string _cacheDirectory;
        private readonly string _metadataZipPath;
        private readonly string _indexPath;
        private readonly string _statePath;
        private readonly ILogger _logger;
        private readonly object _lock = new object();
        private LaunchBoxMetadataIndex _loadedIndex;

        public LaunchBoxMetadataCache(string cacheDirectory, ILogger logger)
        {
            _cacheDirectory = cacheDirectory;
            _metadataZipPath = Path.Combine(cacheDirectory, MetadataZipFileName);
            _indexPath = Path.Combine(cacheDirectory, IndexFileName);
            _statePath = Path.Combine(cacheDirectory, StateFileName);
            _logger = logger;
        }

        public LaunchBoxMetadataIndex GetIndex(CancellationToken cancelToken)
        {
            lock (_lock)
            {
                cancelToken.ThrowIfCancellationRequested();
                if (_loadedIndex != null)
                {
                    return _loadedIndex;
                }

                FileSystem.CreateDirectory(_cacheDirectory);
                var state = LoadState();
                if (ShouldRefresh(state))
                {
                    var refreshed = RefreshMetadata(state, cancelToken);
                    if (refreshed)
                    {
                        _loadedIndex = ParseAndSaveIndex(cancelToken);
                    }
                }

                if (_loadedIndex == null)
                {
                    _loadedIndex = LoadIndex();
                }

                if (_loadedIndex == null && FileSystem.FileExists(_metadataZipPath))
                {
                    _loadedIndex = ParseAndSaveIndex(cancelToken);
                }

                if (_loadedIndex == null)
                {
                    _logger.Debug("LaunchBox metadata cache is unavailable. No cached index could be loaded.");
                    _loadedIndex = CreateEmptyIndex();
                }

                return _loadedIndex;
            }
        }

        private bool ShouldRefresh(LaunchBoxMetadataCacheState state)
        {
            if (!FileSystem.FileExists(_indexPath) && !FileSystem.FileExists(_metadataZipPath))
            {
                return true;
            }

            if (state.SchemaVersion != CacheSchemaVersion)
            {
                return true;
            }

            if (state.LastCheckedUtc == DateTime.MinValue)
            {
                return true;
            }

            return DateTime.UtcNow - state.LastCheckedUtc > CacheCheckInterval;
        }

        private bool RefreshMetadata(LaunchBoxMetadataCacheState state, CancellationToken cancelToken)
        {
            try
            {
                _logger.Debug("Checking LaunchBox metadata cache.");
                using (var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
                using (var client = new HttpClient(handler))
                using (var request = new HttpRequestMessage(HttpMethod.Get, MetadataUrl))
                {
                    client.Timeout = DownloadTimeout;
                    request.Headers.TryAddWithoutValidation("User-Agent", "ExtraMetadataLoader LaunchBoxProvider");
                    request.Headers.TryAddWithoutValidation("Accept", "application/zip, application/octet-stream");
                    if (!state.MetadataETag.IsNullOrWhiteSpace())
                    {
                        request.Headers.TryAddWithoutValidation("If-None-Match", state.MetadataETag);
                    }

                    if (!state.MetadataLastModified.IsNullOrWhiteSpace())
                    {
                        request.Headers.TryAddWithoutValidation("If-Modified-Since", state.MetadataLastModified);
                    }

                    using (var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelToken).ConfigureAwait(false).GetAwaiter().GetResult())
                    {
                        state.SchemaVersion = CacheSchemaVersion;
                        state.LastCheckedUtc = DateTime.UtcNow;

                        if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            SaveState(state);
                            _logger.Debug("LaunchBox metadata cache is current.");
                            return false;
                        }

                        if (response.StatusCode == (HttpStatusCode)429)
                        {
                            SaveState(state);
                            _logger.Debug("LaunchBox metadata request was rate limited. Existing cache will be used if available.");
                            return false;
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            SaveState(state);
                            _logger.Debug($"LaunchBox metadata download failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");
                            return false;
                        }

                        var tempPath = _metadataZipPath + ".tmp";
                        FileSystem.DeleteFile(tempPath, true);
                        using (var responseStream = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                        using (var fileStream = new FileStream(FileSystem.FixPathLength(tempPath), FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            CopyStream(responseStream, fileStream, cancelToken);
                        }

                        FileSystem.DeleteFile(_metadataZipPath, true);
                        File.Move(FileSystem.FixPathLength(tempPath), FileSystem.FixPathLength(_metadataZipPath));

                        state.MetadataETag = response.Headers.ETag?.ToString();
                        state.MetadataLastModified = response.Content.Headers.LastModified?.ToString("R", CultureInfo.InvariantCulture);
                        state.MetadataContentLength = response.Content.Headers.ContentLength;
                        state.MetadataDownloadedUtc = DateTime.UtcNow;
                        state.SourceTimestampUtc = response.Content.Headers.LastModified?.UtcDateTime ?? state.MetadataDownloadedUtc;
                        SaveState(state);
                        _logger.Debug("LaunchBox metadata cache downloaded successfully.");
                        return true;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Debug($"LaunchBox metadata refresh failed. Existing cache will be used if available. Error: {ex.Message}");
                return false;
            }
        }

        private LaunchBoxMetadataIndex LoadIndex()
        {
            if (!FileSystem.FileExists(_indexPath))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(FileSystem.FixPathLength(_indexPath), Encoding.UTF8);
                var index = Serialization.FromJson<LaunchBoxMetadataIndex>(json);
                if (index?.SchemaVersion != CacheSchemaVersion || index.Games == null)
                {
                    _logger.Debug("LaunchBox metadata index has an unsupported schema version and will be rebuilt.");
                    return null;
                }

                return index;
            }
            catch (Exception ex)
            {
                _logger.Debug($"LaunchBox metadata index could not be loaded and will be rebuilt if possible. Error: {ex.Message}");
                return null;
            }
        }

        private LaunchBoxMetadataCacheState LoadState()
        {
            if (!FileSystem.FileExists(_statePath))
            {
                return new LaunchBoxMetadataCacheState();
            }

            try
            {
                var json = File.ReadAllText(FileSystem.FixPathLength(_statePath), Encoding.UTF8);
                return Serialization.FromJson<LaunchBoxMetadataCacheState>(json) ?? new LaunchBoxMetadataCacheState();
            }
            catch (Exception ex)
            {
                _logger.Debug($"LaunchBox metadata cache state could not be loaded. Error: {ex.Message}");
                return new LaunchBoxMetadataCacheState();
            }
        }

        private void SaveState(LaunchBoxMetadataCacheState state)
        {
            FileSystem.WriteStringToFile(_statePath, Serialization.ToJson(state), true);
        }

        private LaunchBoxMetadataIndex ParseAndSaveIndex(CancellationToken cancelToken)
        {
            try
            {
                var index = ParseMetadataZip(cancelToken);
                FileSystem.WriteStringToFile(_indexPath, Serialization.ToJson(index), true);
                _logger.Debug($"LaunchBox clear-logo metadata index saved with {index.Games.Count} games.");
                return index;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Debug($"LaunchBox metadata cache could not be parsed. Error: {ex.Message}");
                return null;
            }
        }

        private LaunchBoxMetadataIndex ParseMetadataZip(CancellationToken cancelToken)
        {
            var games = new Dictionary<string, LaunchBoxGameEntry>(StringComparer.OrdinalIgnoreCase);
            var alternateNames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var logos = new Dictionary<string, List<LaunchBoxLogoEntry>>(StringComparer.OrdinalIgnoreCase);

            using (var archive = ZipFile.OpenRead(FileSystem.FixPathLength(_metadataZipPath)))
            {
                var metadataEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals("Metadata.xml", StringComparison.OrdinalIgnoreCase)) ??
                                    archive.Entries.FirstOrDefault(x => x.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
                if (metadataEntry == null)
                {
                    throw new InvalidDataException("Metadata.zip did not contain an XML metadata file.");
                }

                var xmlSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreComments = true,
                    IgnoreWhitespace = true
                };

                using (var stream = metadataEntry.Open())
                using (var reader = XmlReader.Create(stream, xmlSettings))
                {
                    while (reader.Read())
                    {
                        cancelToken.ThrowIfCancellationRequested();
                        if (reader.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        if (reader.LocalName.Equals("Game", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var subtree = reader.ReadSubtree())
                            {
                                var game = ReadGame(ReadElementValues(subtree));
                                if (game != null && !games.ContainsKey(game.DatabaseId))
                                {
                                    games.Add(game.DatabaseId, game);
                                }
                            }
                        }
                        else if (reader.LocalName.Equals("GameAlternateName", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var subtree = reader.ReadSubtree())
                            {
                                ReadAlternateName(ReadElementValues(subtree), alternateNames);
                            }
                        }
                        else if (reader.LocalName.Equals("GameImage", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var subtree = reader.ReadSubtree())
                            {
                                ReadImage(ReadElementValues(subtree), logos);
                            }
                        }
                    }
                }
            }

            foreach (var alternateName in alternateNames)
            {
                LaunchBoxGameEntry game;
                if (games.TryGetValue(alternateName.Key, out game))
                {
                    game.AlternateNames = alternateName.Value
                        .Where(x => !x.IsNullOrWhiteSpace())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x)
                        .ToList();
                }
            }

            foreach (var logoGroup in logos)
            {
                LaunchBoxGameEntry game;
                if (games.TryGetValue(logoGroup.Key, out game))
                {
                    game.Logos = logoGroup.Value
                        .GroupBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
                        .Select(x => x.First())
                        .OrderByDescending(GetLogoArea)
                        .ThenBy(x => x.FileName)
                        .ToList();
                }
            }

            var index = new LaunchBoxMetadataIndex
            {
                SchemaVersion = CacheSchemaVersion,
                CreatedUtc = DateTime.UtcNow,
                Games = games.Values
                    .Where(x => x.Logos != null && x.Logos.Count > 0)
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.Platform)
                    .ToList()
            };

            _logger.Debug($"LaunchBox metadata parsed. Games with clear logos: {index.Games.Count}.");
            return index;
        }

        private Dictionary<string, string> ReadElementValues(XmlReader reader)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var rootRead = false;
            while (reader.Read())
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (!rootRead)
                {
                    rootRead = true;
                    continue;
                }

                if (reader.Depth != 1)
                {
                    continue;
                }

                var key = reader.LocalName;
                if (reader.IsEmptyElement)
                {
                    values[key] = string.Empty;
                    continue;
                }

                try
                {
                    values[key] = reader.ReadElementContentAsString();
                }
                catch (XmlException ex)
                {
                    _logger.Debug($"LaunchBox metadata XML element '{key}' could not be read. Error: {ex.Message}");
                }
            }

            return values;
        }

        private LaunchBoxGameEntry ReadGame(Dictionary<string, string> values)
        {
            var databaseId = GetValue(values, "DatabaseID", "DatabaseId", "GameID", "GameId", "ID", "Id");
            var name = GetValue(values, "Name", "Title");
            if (databaseId.IsNullOrWhiteSpace() || name.IsNullOrWhiteSpace())
            {
                return null;
            }

            return new LaunchBoxGameEntry
            {
                DatabaseId = databaseId,
                Name = name,
                Platform = GetValue(values, "Platform", "PlatformName"),
                ReleaseYear = GetReleaseYear(GetValue(values, "ReleaseDate", "ReleaseYear", "Year")),
                Developer = GetValue(values, "Developer", "Developers"),
                Publisher = GetValue(values, "Publisher", "Publishers")
            };
        }

        private void ReadAlternateName(Dictionary<string, string> values, Dictionary<string, List<string>> alternateNames)
        {
            var databaseId = GetValue(values, "DatabaseID", "DatabaseId", "GameID", "GameId", "ID", "Id");
            var name = GetValue(values, "Name", "AlternateName", "Alternate Name", "Title");
            if (databaseId.IsNullOrWhiteSpace() || name.IsNullOrWhiteSpace())
            {
                return;
            }

            List<string> names;
            if (!alternateNames.TryGetValue(databaseId, out names))
            {
                names = new List<string>();
                alternateNames.Add(databaseId, names);
            }

            names.Add(name);
        }

        private void ReadImage(Dictionary<string, string> values, Dictionary<string, List<LaunchBoxLogoEntry>> logos)
        {
            var databaseId = GetValue(values, "DatabaseID", "DatabaseId", "GameID", "GameId", "ID", "Id");
            var imageType = GetValue(values, "Type", "ImageType", "Image Type");
            var fileName = GetValue(values, "FileName", "File Name", "Name");
            if (databaseId.IsNullOrWhiteSpace() || !IsClearLogoType(imageType) || !IsValidImageFileName(fileName))
            {
                return;
            }

            List<LaunchBoxLogoEntry> gameLogos;
            if (!logos.TryGetValue(databaseId, out gameLogos))
            {
                gameLogos = new List<LaunchBoxLogoEntry>();
                logos.Add(databaseId, gameLogos);
            }

            gameLogos.Add(new LaunchBoxLogoEntry
            {
                FileName = fileName,
                Region = GetValue(values, "Region"),
                Width = GetNullableInt(GetValue(values, "Width")),
                Height = GetNullableInt(GetValue(values, "Height"))
            });
        }

        internal static bool IsValidImageFileName(string fileName)
        {
            if (fileName.IsNullOrWhiteSpace() || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
            {
                return false;
            }

            if (fileName.Any(char.IsControl))
            {
                return false;
            }

            var extension = Path.GetExtension(fileName);
            return extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsClearLogoType(string imageType)
        {
            return imageType?.Satinize() == "clearlogo";
        }

        private static string GetValue(Dictionary<string, string> values, params string[] keys)
        {
            foreach (var key in keys)
            {
                string value;
                if (values.TryGetValue(key, out value))
                {
                    return value?.Trim();
                }
            }

            return null;
        }

        private static int? GetNullableInt(string value)
        {
            int parsedValue;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        private static int? GetReleaseYear(string releaseDate)
        {
            if (releaseDate.IsNullOrWhiteSpace())
            {
                return null;
            }

            int year;
            if (int.TryParse(releaseDate, NumberStyles.Integer, CultureInfo.InvariantCulture, out year) &&
                year > 0)
            {
                return year;
            }

            DateTime parsedDate;
            if (DateTime.TryParse(releaseDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsedDate))
            {
                return parsedDate.Year;
            }

            var match = Regex.Match(releaseDate, @"\b(?<year>\d{4})\b");
            if (match.Success && int.TryParse(match.Groups["year"].Value, out year))
            {
                return year;
            }

            return null;
        }

        private static long GetLogoArea(LaunchBoxLogoEntry logo)
        {
            if (logo.Width.HasValue && logo.Height.HasValue)
            {
                return (long)logo.Width.Value * logo.Height.Value;
            }

            return 0;
        }

        private static void CopyStream(Stream source, Stream target, CancellationToken cancelToken)
        {
            var buffer = new byte[81920];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                cancelToken.ThrowIfCancellationRequested();
                target.Write(buffer, 0, bytesRead);
            }
        }

        private static LaunchBoxMetadataIndex CreateEmptyIndex()
        {
            return new LaunchBoxMetadataIndex
            {
                SchemaVersion = CacheSchemaVersion,
                CreatedUtc = DateTime.UtcNow,
                Games = new List<LaunchBoxGameEntry>()
            };
        }
    }
}
