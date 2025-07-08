using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InstallationStatusUpdater.Application
{
    public static class StringPathsDetector
    {
        private static readonly Regex PotentialPathRegex = new Regex(
            @""".+?""|\S+",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
        private static readonly char[] SlashChars = new[] { '\\', '/' };
        
        public static List<string> ExtractPathsFromArguments(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                return new List<string>();
            }

            var results = new List<string>();
            var tokens = PotentialPathRegex.Matches(args)
                              .Cast<Match>()
                              .Select(m => m.Value);

            foreach (var token in tokens)
            {
                // Try to extract the path if part of a key=value style
                var maybePath = token;
                if (token.Contains("="))
                {
                    var split = token.Split(new[] { '=' }, 2);
                    maybePath = split[1];
                }
                
                // Trim surrounding quotes and normalize slashes
                maybePath = maybePath.Trim('"').Replace('/', '\\');
                if (IsLikelyWindowsPath(maybePath))
                {
                    results.Add(maybePath);
                }
            }

            return results;
        }

        private static bool IsLikelyWindowsPath(string path)
        {
            if (IsDriveLetterPath(path))
            {
                // Drive letter path, e.g. C:\Program Files\game.exe
                return true;
            }
            else if (path.StartsWith(@"\\"))
            {
                // UNC path, e.g. \\server\share
                return true;
            }
            else if (path.StartsWith(@".\") || path.StartsWith(@"..\"))
            {
                // Relative path, e.g. .\folder\file.txt or ..\folder\file.txt
                return true;
            }
            else if (HasFolderSeparatorPattern(path))
            {
                // Relative path with folder separator, e.g. tools/patch.bat or folder\file.ext
                return true;
            }
            
            return false;
        }
        
        private static bool IsDriveLetterPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Length < 3)
            {
                return false;
            } 

            var driveLetter = path[0];
            return ((driveLetter >= 'A' && driveLetter <= 'Z') || (driveLetter >= 'a' && driveLetter <= 'z'))
                   && path[1] == ':'
                   && path[2] == '\\';
        }
        
        private static bool HasFolderSeparatorPattern(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Length < 3)
            {
                return false;
            }

            // First char must NOT be slash or backslash
            if (path[0] == '\\' || path[0] == '/')
            {
                return false;
            }

            // Find the first slash/backslash after the first character
            int slashIndex = path.IndexOfAny(SlashChars, 1);
            if (slashIndex < 1 || slashIndex == path.Length - 1)
            {
                return false;
            }

            // Ensure char after the slash is NOT slash or backslash
            char nextChar = path[slashIndex + 1];
            if (nextChar == '\\' || nextChar == '/')
            {
                return false;
            }

            return true;
        }
        
    }
}
