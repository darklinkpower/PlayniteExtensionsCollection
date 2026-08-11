using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtraMetadataLoader.MetadataProviders
{
    internal static class EmuMoviesPlatformMapper
    {
        private static readonly Dictionary<string, string> PlatformMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "3DO", "3DO" },
            { "Amiga", "Commodore Amiga" },
            { "Arcade", "MAME" },
            { "Atari 2600", "Atari 2600" },
            { "Atari 5200", "Atari 5200" },
            { "Atari 7800", "Atari 7800" },
            { "Atari Jaguar", "Atari Jaguar" },
            { "Atari Lynx", "Atari Lynx" },
            { "ColecoVision", "Colecovision" },
            { "Commodore 64", "Commodore 64" },
            { "Dreamcast", "Sega Dreamcast" },
            { "GBA", "Game Boy Advance" },
            { "Game Boy", "Nintendo Game Boy" },
            { "Game Boy Advance", "Nintendo Game Boy Advance" },
            { "Game Boy Color", "Nintendo Game Boy Color" },
            { "GameCube", "Nintendo Gamecube" },
            { "MAME", "MAME" },
            { "Microsoft Xbox", "Microsoft Xbox" },
            { "Microsoft Xbox 360", "Microsoft Xbox 360" },
            { "N64", "Nintendo 64" },
            { "Neo Geo", "SNK Neo Geo" },
            { "Neo Geo Pocket", "SNK Neo Geo Pocket" },
            { "Neo Geo Pocket Color", "SNK Neo Geo Pocket Color" },
            { "NES", "Nintendo Entertainment System" },
            { "Nintendo 3DS", "Nintendo 3DS" },
            { "Nintendo 64", "Nintendo 64" },
            { "Nintendo DS", "Nintendo DS" },
            { "Nintendo Entertainment System", "Nintendo Entertainment System" },
            { "Nintendo GBA", "Nintendo GBA" },
            { "Nintendo Game Boy", "Nintendo Game Boy" },
            { "Nintendo Game Boy Advance", "Nintendo Game Boy Advance" },
            { "Nintendo Game Boy Color", "Nintendo Game Boy Color" },
            { "Nintendo GameCube", "Nintendo Gamecube" },
            { "Nintendo Switch", "Nintendo Switch" },
            { "Nintendo Wii", "Nintendo Wii" },
            { "Nintendo Wii U", "Nintendo Wii U" },
            { "PC", "Microsoft Windows" },
            { "PC Engine", "NEC PC Engine" },
            { "PlayStation", "Sony Playstation" },
            { "PlayStation 2", "Sony Playstation 2" },
            { "PlayStation 3", "Sony Playstation 3" },
            { "PS2", "Sony Playstation 2" },
            { "PS3", "Sony Playstation 3" },
            { "PSP", "Sony PSP" },
            { "PSX", "Sony Playstation" },
            { "Nintendo SNES", "Super Nintendo Entertainment System" },
            { "Nintendo Super Famicom", "Super Nintendo Entertainment System" },
            { "Nintendo Super Nintendo", "Super Nintendo Entertainment System" },
            { "Sega 32X", "Sega 32X" },
            { "Sega CD", "Sega CD" },
            { "Sega Dreamcast", "Sega Dreamcast" },
            { "Sega Game Gear", "Sega Game Gear" },
            { "Sega Genesis", "Sega Genesis" },
            { "Sega Master System", "Sega Master System" },
            { "Sega Mega Drive", "Sega Mega Drive" },
            { "Sega Saturn", "Sega Saturn" },
            { "SNES", "Super Nintendo Entertainment System" },
            { "Super Famicom", "Super Nintendo Entertainment System" },
            { "Super Nintendo", "Super Nintendo Entertainment System" },
            { "Sony PlayStation", "Sony Playstation" },
            { "Sony PlayStation 2", "Sony Playstation 2" },
            { "Sony PlayStation 3", "Sony Playstation 3" },
            { "Sony PSP", "Sony PSP" },
            { "Super Nintendo Entertainment System", "Super Nintendo Entertainment System" },
            { "Switch", "Nintendo Switch" },
            { "TurboGrafx-16", "NEC TurboGrafx-16" },
            { "Wii", "Nintendo Wii" },
            { "Wii U", "Nintendo Wii U" },
            { "Windows", "Microsoft Windows" },
            { "Xbox", "Microsoft Xbox" },
            { "Xbox 360", "Microsoft Xbox 360" }
        };

        private static readonly Dictionary<string, string[]> PlatformAliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Nintendo Game Boy Advance", new[] { "Nintendo GBA", "GBA", "Game Boy Advance", "Nintendo Game Boy Advance" } },
            { "Game Boy Advance", new[] { "Nintendo GBA", "GBA", "Game Boy Advance", "Nintendo Game Boy Advance" } },
            { "Nintendo GBA", new[] { "Nintendo GBA", "GBA", "Game Boy Advance", "Nintendo Game Boy Advance" } },
            { "GBA", new[] { "Nintendo GBA", "GBA", "Game Boy Advance", "Nintendo Game Boy Advance" } },
            {
                "Super Nintendo Entertainment System",
                new[]
                {
                    "Nintendo Super Nintendo",
                    "Super Nintendo",
                    "Nintendo Super Famicom",
                    "Super Famicom",
                    "Nintendo SNES",
                    "SNES"
                }
            }
        };

        public static List<string> GetEmuMoviesPlatforms(Game game)
        {
            if (game?.Platforms?.Any() != true)
            {
                return new List<string>();
            }

            return game.Platforms
                .SelectMany(x => GetEmuMoviesPlatforms(x.Name))
                .Where(x => !x.IsNullOrWhiteSpace())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static IEnumerable<string> GetEmuMoviesPlatforms(string playnitePlatformName)
        {
            if (playnitePlatformName.IsNullOrWhiteSpace())
            {
                return Enumerable.Empty<string>();
            }

            var emuMoviesPlatform = PlatformMap.TryGetValue(playnitePlatformName, out var mappedPlatform)
                ? mappedPlatform
                : playnitePlatformName;

            return PlatformAliases.TryGetValue(emuMoviesPlatform, out var aliases)
                ? aliases
                : new[] { emuMoviesPlatform };
        }
    }
}
