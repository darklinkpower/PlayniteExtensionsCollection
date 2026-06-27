using HoYoPlayLibrary.Domain.Interfaces;
using Microsoft.Win32;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoYoPlayLibrary.Infrastructure
{
    internal class RegistryVersionResolver : IRegistryVersionResolver
    {
        private const string BaseKey = @"Software\Cognosphere\HYP";
        private readonly ILogger _logger;

        public RegistryVersionResolver(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<string> GetActiveRootKeyPaths()
        {
            using (var hypKey = Registry.CurrentUser.OpenSubKey(BaseKey))
            {
                if (hypKey is null)
                {
                    _logger.Warn($"HoYoPlay registry base key not found: {BaseKey}");
                    return null;
                }

                var candidates = hypKey.GetSubKeyNames()
                    .Where(name => Regex.IsMatch(name, @"^\d+_\d+$"))
                    .OrderByDescending(name => name)
                    .ToList();

                if (candidates.Count == 0)
                {
                    _logger.Warn($"No versioned subkeys found under '{BaseKey}'.");
                    return null;
                }

                var paths = candidates.Select(selected => $@"{BaseKey}\{selected}").ToList();
                foreach (var path in paths)
                {
                    _logger.Debug($"Found HoYoPlay registry version: '{path}'");
                }

                _logger.Info($"Using HoYoPlay registry versions: {string.Join(", ", paths)}");
                return paths;
            }
        }
    }
}
