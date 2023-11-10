using JastUsaLibrary.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using WebCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JastUsaLibrary
{
    public class JastUsaLibraryMetadataProvider : LibraryMetadataProvider
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly string userGamesCachePath;
        private const string jastMediaUrlTemplate = @"https://app.jastusa.com/media/image/{0}";
        private const string jastBaseAppUrl = @"https://app.jastusa.com";

        public JastUsaLibraryMetadataProvider(string userGamesCachePath)
        {
            this.userGamesCachePath = userGamesCachePath;
        }

        public override GameMetadata GetMetadata(Game game)
        {
            var apiUrl = GetProductApiIdFromGameId(game.GameId);
            if (apiUrl.IsNullOrEmpty())
            {
                return new GameMetadata();
            }

            var url = jastBaseAppUrl + apiUrl;
            var downloadedString = HttpDownloader.GetRequestBuilder().WithUrl(url).DownloadString();
            if (!downloadedString.IsSuccessful)
            {
                return new GameMetadata();
            }

            var productResponse = Serialization.FromJson<ProductResponse>(downloadedString.Response.Content);
            var metadata = new GameMetadata()
            {
                Name = productResponse.Name,
                Description = productResponse.Description,
                ReleaseDate = new ReleaseDate(productResponse.OriginalReleaseDate),
                Platforms = new HashSet<MetadataProperty> { new MetadataSpecProperty("pc_windows") }
            };

            var developer = GetSingleAttributeMatch(productResponse, "studio");
            if (!developer.IsNullOrEmpty())
            {
                metadata.Developers = new HashSet<MetadataProperty> { new MetadataNameProperty(developer) };
            }

            var publisher = GetSingleAttributeMatch(productResponse, "publisher");
            if (!publisher.IsNullOrEmpty())
            {
                metadata.Publishers = new HashSet<MetadataProperty> { new MetadataNameProperty(publisher) };
            }

            var coverImage = productResponse.Images.FirstOrDefault(x => x.ImageType == "TAIL_PACKAGE_THUMBNAIL_PRODUCT_en_US");
            if (coverImage != null)
            {
                metadata.CoverImage = new MetadataFile(string.Format(jastMediaUrlTemplate, coverImage.Path));
            }

            var backgroundImage = productResponse.Images.FirstOrDefault(x => x.ImageType == "BACKGROUND_PRODUCT_en_US");
            if (backgroundImage != null)
            {
                metadata.BackgroundImage = new MetadataFile(string.Format(jastMediaUrlTemplate, backgroundImage.Path));
            }

            metadata.Links = new List<Link> { new Link("Store", @"https://jastusa.com/games/" + productResponse.Code) };
            var urlAttributes = productResponse.Attributes.Where(x => x.Code.EndsWith("_url") && x.Value.HasItems() &&
                                x.Value.First().StartsWith("http"));
            if (urlAttributes.HasItems())
            {
                metadata.Links.AddRange(urlAttributes.Select(x => new Link(x.Code.Replace("_", " ").ToTitleCase(), x.Value.First())));
            }

            var tags = GetMultipleAttributeMatch(productResponse, "tag");
            if (tags.HasItems())
            {
                metadata.Tags = tags.Select(x => new MetadataNameProperty(x.ToTitleCase())).Cast<MetadataProperty>().ToHashSet();
            }

            var platforms = GetMultipleAttributeMatch(productResponse, "platform");
            if (platforms.HasItems())
            {
                var platformToSpecIdMapper = new Dictionary<string, string>
                {
                    {"Windows", "pc_windows" },
                    {"Mac", "macintosh" },
                    {"Linux", "pc_linux" }
                };

                foreach (var platform in platforms)
                {
                    if (platformToSpecIdMapper.TryGetValue(platform, out var platformSpecId))
                    {
                        metadata.Platforms.Add(new MetadataSpecProperty(platformSpecId));
                    }
                }
            }

            return metadata;
        }

        private string GetProductApiIdFromGameId(string gameId)
        {
            if (!FileSystem.FileExists(userGamesCachePath))
            {
                return null;
            }

            var cache = Serialization.FromJsonFile<List<JastProduct>>(userGamesCachePath);
            return cache.FirstOrDefault(x => x.ProductVariant.GameId.ToString() == gameId)?.IdApiEndpoint ?? null;
        }

        private string GetSingleAttributeMatch(ProductResponse productResponse, string attributeName, Locale locale = Locale.En_Us)
        {
            var attribute = productResponse.Attributes.FirstOrDefault(x => x.Code == attributeName && x.LocaleCode == locale);
            if (attribute is null || !attribute.Value.HasItems())
            {
                return null;
            }

            var attributeValue = attribute.Value.First();
            var attributeMatch = attribute.Configuration.Choices.FirstOrDefault(x => x.Key == attributeValue);
            if (attributeMatch.Equals(default(KeyValuePair<string, Dictionary<Locale, string>>)))
            {
                return null;
            }

            var localizedChoice = attributeMatch.Value.FirstOrDefault(x => x.Key == locale);
            if (!attributeMatch.Equals(default(KeyValuePair<Locale, string>)))
            {
                return localizedChoice.Value;
            }

            return null;
        }

        private List<string> GetMultipleAttributeMatch(ProductResponse productResponse, string attributeName, Locale locale = Locale.En_Us)
        {
            var values = new List<string>();
            var attribute = productResponse.Attributes.FirstOrDefault(x => x.Code == attributeName && x.LocaleCode == locale);
            if (attribute is null || !attribute.Value.HasItems())
            {
                return values;
            }

            var matches = attribute.Configuration.Choices.Where(x => attribute.Value.Any(y => y == x.Key));
            if (!matches.HasItems())
            {
                return values;
            }

            var maxValue = attribute.Configuration.Max;
            foreach (var item in matches)
            {
                if (maxValue.HasValue && values.Count >= maxValue && values.Count != 0)
                {
                    break;
                }

                foreach (var subItem in item.Value)
                {
                    if (subItem.Key == locale)
                    {
                        values.Add(subItem.Value);
                        break;
                    }
                }
            }

            return values;
        }
    }

}