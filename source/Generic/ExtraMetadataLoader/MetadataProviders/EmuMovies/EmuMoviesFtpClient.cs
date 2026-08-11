using Playnite.SDK;
using PluginsCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace ExtraMetadataLoader.MetadataProviders
{
    internal class EmuMoviesFtpClient
    {
        private const string PrimaryFtpHost = "files.emumovies.com";
        private const string FallbackFtpHost = "files2.emumovies.com";
        private const int TimeoutMilliseconds = 20000;
        private const int MaxListAttempts = 3;
        private const int ListRetryDelayMilliseconds = 750;

        private static readonly string[] Hosts = { PrimaryFtpHost, FallbackFtpHost };
        private static readonly HashSet<string> VideoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4",
            ".avi",
            ".mkv",
            ".mov",
            ".m4v",
            ".webm"
        };
        private static readonly object CacheLock = new object();
        private static readonly Dictionary<string, List<string>> QualityDirectoryCache = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, List<EmuMoviesVideoFile>> VideoFilesCache = new Dictionary<string, List<EmuMoviesVideoFile>>(StringComparer.OrdinalIgnoreCase);

        private readonly string _username;
        private readonly string _password;
        private readonly List<EmuMoviesVideoQuality> _qualities;
        private readonly ILogger _logger;

        public EmuMoviesFtpClient(string username, string password, EmuMoviesVideoQuality quality, ILogger logger)
            : this(username, password, new[] { quality }, logger)
        {
        }

        public EmuMoviesFtpClient(string username, string password, IEnumerable<EmuMoviesVideoQuality> qualities, ILogger logger)
        {
            _username = username;
            _password = password;
            _qualities = qualities?.Distinct().ToList() ?? new List<EmuMoviesVideoQuality>();
            if (!_qualities.HasItems())
            {
                _qualities.Add(EmuMoviesVideoQuality.HQ);
            }

            _logger = logger;
        }

        public bool TestConnection(CancellationToken cancelToken = default)
        {
            foreach (var host in Hosts)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return false;
                }

                try
                {
                    using (var response = GetFtpResponse(BuildRootUri(host), WebRequestMethods.Ftp.ListDirectory))
                    {
                        _logger.Debug($"EmuMovies FTP connection successful using {host}: {response.StatusDescription}");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    _logger.Warn(e, $"EmuMovies FTP connection test failed using {host}.");
                }
            }

            return false;
        }

        public List<EmuMoviesVideoFile> ListVideoFiles(string platformName, CancellationToken cancelToken)
        {
            if (platformName.IsNullOrWhiteSpace())
            {
                return new List<EmuMoviesVideoFile>();
            }

            var files = new List<EmuMoviesVideoFile>();
            foreach (var quality in _qualities)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                files.AddRange(ListVideoFiles(platformName, quality, cancelToken));
            }

            return files;
        }

        private List<EmuMoviesVideoFile> ListVideoFiles(string platformName, EmuMoviesVideoQuality quality, CancellationToken cancelToken)
        {
            var cacheKey = $"{quality}|{platformName}";
            lock (CacheLock)
            {
                if (VideoFilesCache.TryGetValue(cacheKey, out var cachedFiles))
                {
                    return cachedFiles.ToList();
                }
            }

            foreach (var host in Hosts)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    return new List<EmuMoviesVideoFile>();
                }

                var qualityDirectoryUri = BuildQualityDirectoryUri(host, quality);
                var platformDirectories = GetMatchingPlatformDirectories(host, quality, platformName, cancelToken);
                if (!platformDirectories.HasItems())
                {
                    _logger.Debug($"No EmuMovies platform directories matched '{platformName}' in {qualityDirectoryUri}.");
                    continue;
                }

                var files = new List<EmuMoviesVideoFile>();
                foreach (var platformDirectory in platformDirectories)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return new List<EmuMoviesVideoFile>();
                    }

                    var platformDirectoryUri = AppendPathSegment(qualityDirectoryUri, platformDirectory);
                    try
                    {
                        files.AddRange(ListVideoFiles(platformDirectoryUri, quality, platformName, platformDirectory));
                    }
                    catch (Exception e)
                    {
                        _logger.Warn(e, $"Failed to list EmuMovies FTP directory {platformDirectoryUri}.");
                    }
                }

                if (files.HasItems())
                {
                    lock (CacheLock)
                    {
                        VideoFilesCache[cacheKey] = files;
                    }

                    return files.ToList();
                }
            }

            return new List<EmuMoviesVideoFile>();
        }

        private List<string> GetMatchingPlatformDirectories(
            string host,
            EmuMoviesVideoQuality quality,
            string platformName,
            CancellationToken cancelToken)
        {
            return ListQualityDirectories(host, quality, cancelToken)
                .Select(x => new
                {
                    DirectoryName = x,
                    Score = GetPlatformDirectoryScore(x, platformName)
                })
                .Where(x => x.Score >= 80)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.DirectoryName)
                .Select(x => x.DirectoryName)
                .ToList();
        }

        private List<string> ListQualityDirectories(string host, EmuMoviesVideoQuality quality, CancellationToken cancelToken)
        {
            var cacheKey = $"{host}|{quality}";
            lock (CacheLock)
            {
                if (QualityDirectoryCache.TryGetValue(cacheKey, out var cachedDirectories))
                {
                    return cachedDirectories.ToList();
                }
            }

            if (cancelToken.IsCancellationRequested)
            {
                return new List<string>();
            }

            var qualityDirectoryUri = BuildQualityDirectoryUri(host, quality);
            try
            {
                var directories = ListDirectoryEntries(qualityDirectoryUri)
                    .Where(x => !x.IsNullOrWhiteSpace())
                    .Where(x => !VideoExtensions.Contains(Path.GetExtension(x)))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();

                lock (CacheLock)
                {
                    QualityDirectoryCache[cacheKey] = directories;
                }

                return directories.ToList();
            }
            catch (Exception e)
            {
                _logger.Warn(e, $"Failed to list EmuMovies FTP quality directory {qualityDirectoryUri}.");
                return new List<string>();
            }
        }

        public bool DownloadVideo(string ftpPath, string destinationPath, CancellationToken cancelToken)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                FileSystem.DeleteFile(destinationPath);

                var request = CreateRequest(ftpPath, WebRequestMethods.Ftp.DownloadFile);
                using (var response = (FtpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var fileStream = File.Create(destinationPath))
                {
                    var buffer = new byte[81920];
                    int bytesRead;
                    while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (cancelToken.IsCancellationRequested)
                        {
                            FileSystem.DeleteFileSafe(destinationPath);
                            return false;
                        }

                        fileStream.Write(buffer, 0, bytesRead);
                    }
                }

                return FileSystem.FileExists(destinationPath);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to download EmuMovies video from {ftpPath}.");
                FileSystem.DeleteFileSafe(destinationPath);
                return false;
            }
        }

        private List<EmuMoviesVideoFile> ListVideoFiles(
            string directoryUri,
            EmuMoviesVideoQuality quality,
            string platformName,
            string platformDirectoryName)
        {
            var files = new List<EmuMoviesVideoFile>();
            foreach (var entry in ListDirectoryEntries(directoryUri))
            {
                var fileName = GetFileName(entry);
                if (!VideoExtensions.Contains(Path.GetExtension(fileName)))
                {
                    continue;
                }

                files.Add(new EmuMoviesVideoFile
                {
                    FileName = fileName,
                    FtpPath = directoryUri + Uri.EscapeDataString(fileName),
                    PlatformName = platformName,
                    PlatformDirectoryName = platformDirectoryName,
                    Quality = quality
                });
            }

            return files;
        }

        private List<string> ListDirectoryEntries(string directoryUri)
        {
            var entries = new List<string>();
            using (var response = GetFtpResponseWithRetry(directoryUri, WebRequestMethods.Ftp.ListDirectory))
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    entries.Add(GetFileName(line));
                }
            }

            return entries;
        }

        private FtpWebResponse GetFtpResponseWithRetry(string uri, string method)
        {
            Exception lastError = null;
            for (var attempt = 1; attempt <= MaxListAttempts; attempt++)
            {
                try
                {
                    return GetFtpResponse(uri, method);
                }
                catch (Exception e)
                {
                    lastError = e;
                    if (attempt == MaxListAttempts)
                    {
                        break;
                    }

                    _logger.Debug($"EmuMovies FTP {method} failed for {uri} on attempt {attempt}; retrying.");
                    Thread.Sleep(ListRetryDelayMilliseconds * attempt);
                }
            }

            throw lastError;
        }

        private FtpWebResponse GetFtpResponse(string uri, string method)
        {
            var request = CreateRequest(uri, method);
            return (FtpWebResponse)request.GetResponse();
        }

        private FtpWebRequest CreateRequest(string uri, string method)
        {
            var request = (FtpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Credentials = new NetworkCredential(_username, _password);
            request.UseBinary = true;
            request.UsePassive = true;
            request.KeepAlive = false;
            request.Timeout = TimeoutMilliseconds;
            request.ReadWriteTimeout = TimeoutMilliseconds;
            return request;
        }

        private static int GetPlatformDirectoryScore(string directoryName, string platformName)
        {
            var directoryTokens = GetComparableTokens(GetPlatformNameFromDirectory(directoryName));
            var platformTokens = GetComparableTokens(platformName);
            if (!directoryTokens.HasItems() || !platformTokens.HasItems())
            {
                return 0;
            }

            if (!StartsWithTokens(directoryTokens, platformTokens))
            {
                return 0;
            }

            var extraTokenCount = directoryTokens.Count - platformTokens.Count;
            if (extraTokenCount > 0 && IsVersionToken(directoryTokens[platformTokens.Count]))
            {
                return 0;
            }

            return Math.Max(80, 100 - (extraTokenCount * 3));
        }

        private static bool StartsWithTokens(List<string> directoryTokens, List<string> platformTokens)
        {
            if (directoryTokens.Count < platformTokens.Count)
            {
                return false;
            }

            for (var i = 0; i < platformTokens.Count; i++)
            {
                if (!directoryTokens[i].Equals(platformTokens[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetPlatformNameFromDirectory(string directoryName)
        {
            return Regex.Replace(directoryName, @"\s*\(Video Snaps\).*", string.Empty, RegexOptions.IgnoreCase).Trim();
        }

        private static List<string> GetComparableTokens(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return new List<string>();
            }

            return Regex.Replace(value.ToLowerInvariant(), @"[^a-z0-9]+", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        private static bool IsVersionToken(string token)
        {
            return Regex.IsMatch(token, @"^\d+$") ||
                   Regex.IsMatch(token, @"^(i|ii|iii|iv|v|vi|vii|viii|ix|x)$", RegexOptions.IgnoreCase);
        }

        private static string BuildRootUri(string host)
        {
            return $"ftp://{host}/";
        }

        private static string BuildQualityDirectoryUri(string host, EmuMoviesVideoQuality quality)
        {
            var qualityPath = Uri.EscapeDataString($"Video Snaps ({quality})");
            return $"ftp://{host}/Official/{qualityPath}/";
        }

        private static string AppendPathSegment(string baseUri, string pathSegment)
        {
            return baseUri.TrimEnd('/') + "/" + Uri.EscapeDataString(pathSegment) + "/";
        }

        private static string GetFileName(string ftpListEntry)
        {
            return ftpListEntry.Trim().TrimEnd('/').Split('/').LastOrDefault() ?? ftpListEntry;
        }
    }
}
