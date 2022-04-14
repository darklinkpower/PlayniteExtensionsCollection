using JastUsaLibrary.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using PluginsCommon;
using PluginsCommon.Web;
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
        private string userGamesCachePath;
        private const string productsApiTemplate = @"https://app.jastusa.com/api/v2/shop/products/";
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

            var downloadedString = HttpDownloader.DownloadString(jastBaseAppUrl + apiUrl);
            if (downloadedString.IsNullOrEmpty())
            {
                return new GameMetadata();
            }

            var productResponse = Serialization.FromJson<ProductResponse>(downloadedString);
            var metadata = new GameMetadata()
            {
                Description = productResponse.Description,
                ReleaseDate = new ReleaseDate(productResponse.ReleaseDate),
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
            var urlAttributes = productResponse.Attributes.Where(x => x.Code.EndsWith("_url") && x.Value.First().StartsWith("http"));
            if (urlAttributes.Count() > 0)
            {
                metadata.Links.AddRange(urlAttributes.Select(x => new Link(x.Code.Replace("_", " ").ToTitleCase(), x.Value.First())));
            }

            var tags = GetMultipleAttributeMatch(productResponse, "tag");
            if (tags != null && tags.Count > 0)
            {
                metadata.Tags = tags.Select(x => new MetadataNameProperty(x.ToTitleCase())).Cast<MetadataProperty>().ToHashSet();
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
            return cache.FirstOrDefault(x => x.ProductVariant.GameId.ToString() == gameId)?.Id ?? null;
        }

        private string GetSingleAttributeMatch(ProductResponse productResponse, string attributeName, string locale = "en_US")
        {
            var attribute = productResponse.Attributes.FirstOrDefault(x => x.Code == attributeName && x.LocaleCode == locale);
            if (attribute == null)
            {
                return null;
            }

            var attributeValue = attribute.Value.First();
            var attributeMatches = attribute.Configuration.Choices.Where(x => x.Key == attributeValue);
            if (attributeMatches.Count() == 0)
            {
                return null;
            }

            var attributeMatch = attributeMatches.First().Value.FirstOrDefault(x => x.Key == locale);
            if (!attributeMatch.Equals(default(KeyValuePair<string, string>)))
            {
                return attributeMatch.Value;
            }

            return null;
        }

        private List<string> GetMultipleAttributeMatch(ProductResponse productResponse, string attributeName, string locale = "en_US")
        {
            var attribute = productResponse.Attributes.FirstOrDefault(x => x.Code == attributeName && x.LocaleCode == locale);
            if (attribute == null)
            {
                return null;
            }

            var matches = attribute.Configuration.Choices.Where(x => attribute.Value.Any(y => y == x.Key));
            if (matches.Count() == 0)
            {
                return null;
            }

            var values = new List<string>();
            var maxValue = attribute.Configuration.Max;
            foreach (var item in matches)
            {
                if (values.Count >= maxValue && values.Count != 0)
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