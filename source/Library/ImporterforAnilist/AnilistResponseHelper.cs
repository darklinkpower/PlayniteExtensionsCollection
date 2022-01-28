using ImporterforAnilist.Models;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImporterforAnilist
{
    public class AnilistResponseHelper
    {
        public static GameMetadata MediaToGameMetadata(Media media, bool addLinksAndImages, string propertiesPrefix)
        {
            var game = new GameMetadata()
            {
                Source = new MetadataNameProperty("Anilist"),
                GameId = media.Id.ToString(),
                Name = media.Title.Romaji ?? media.Title.English ?? media.Title.Native ?? string.Empty,
                IsInstalled = true,
                Platforms = new HashSet<MetadataProperty> { new MetadataNameProperty($"AniList {media.Type}") },
                Description = media.Description ?? string.Empty,
                CommunityScore = media.AverageScore ?? null,
                Genres = media.Genres?.Select(a => new MetadataNameProperty($"{propertiesPrefix}{a}")).Cast<MetadataProperty>().ToHashSet() ?? null
            };

            //Links and images
            if (addLinksAndImages)
            {
                game.Links = new List<Link>() { new Link("AniList", media.SiteUrl.ToString()) };
                if (media.IdMal != null)
                {
                    game.Links.Add(new Link("MyAnimeList", string.Format("https://myanimelist.net/{0}/{1}/", media.Type.ToString().ToLower(), media.IdMal.ToString())));
                }

                game.BackgroundImage = new MetadataFile(media.BannerImage ?? string.Empty);
                game.CoverImage = new MetadataFile(media.CoverImage.ExtraLarge ?? string.Empty);
            }

            //ReleaseDate
            if (media.StartDate.Year != null && media.StartDate.Month != null && media.StartDate.Day != null)
            {
                game.ReleaseDate = new ReleaseDate(new DateTime((int)media.StartDate.Year, (int)media.StartDate.Month, (int)media.StartDate.Day));
            }

            //Developers and Publishers
            if (media.Type == TypeEnum.Manga)
            {
                game.Developers = media.Staff.Nodes?.
                    Select(a => new MetadataNameProperty($"{propertiesPrefix}{a.Name.Full}")).Cast<MetadataProperty>().ToHashSet();
            }
            else if (media.Type == TypeEnum.Anime)
            {
                game.Developers = media.Studios.Nodes.Where(s => s.IsAnimationStudio == true)?.
                    Select(a => new MetadataNameProperty($"{propertiesPrefix}{a.Name}")).Cast<MetadataProperty>().ToHashSet();
                game.Publishers = media.Studios.Nodes.Where(s => s.IsAnimationStudio == false)?.
                    Select(a => new MetadataNameProperty($"{propertiesPrefix}{a.Name}")).Cast<MetadataProperty>().ToHashSet();
            }

            //Tags
            var tags = media.Tags.
                Where(s => s.IsMediaSpoiler == false).
                Where(s => s.IsGeneralSpoiler == false)?.
                Select(a => new MetadataNameProperty($"{propertiesPrefix}{a.Name}")).Cast<MetadataProperty>().ToHashSet();
            tags.Add(new MetadataNameProperty($"{propertiesPrefix}Status: {media.Status}"));
            if (media.Season != null)
            {
                tags.Add(new MetadataNameProperty($"{propertiesPrefix}Season: {media.Season}"));
            }
            
            if (media.Format != null)
            {
                tags.Add(new MetadataNameProperty($"{propertiesPrefix}Format: {media.Format}"));
            }

            game.Tags = tags;

            return game;
        }

    }
}
