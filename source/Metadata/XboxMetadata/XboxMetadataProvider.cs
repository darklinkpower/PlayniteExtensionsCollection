using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using XboxMetadata.Services;

namespace XboxMetadata
{
    public class XboxMetadataProvider : OnDemandMetadataProvider
    {
        private static readonly ILogger _logger = LogManager.GetLogger();
        private readonly MetadataRequestOptions _options;
        private readonly XboxMetadata _plugin;
        private readonly XboxMetadataSettingsViewModel _settings;
        private readonly XboxWebService _xboxWebService;
        private ProductSummary _matchedProductData;
        private bool _dataSearchCompleted = false;

        private List<MetadataField> availableFields = null;
        public override List<MetadataField> AvailableFields
        {
            get
            {
                if (availableFields is null)
                {
                    availableFields = GetAvailableFields();
                }

                return availableFields;
            }
        }

        public XboxMetadataProvider(MetadataRequestOptions options, XboxMetadata plugin, XboxMetadataSettingsViewModel settings, XboxWebService xboxWebService)
        {
            _options = options;
            _plugin = plugin;
            _settings = settings;
            _xboxWebService = xboxWebService;
        }

        private List<MetadataField> GetAvailableFields()
        {
            if (_dataSearchCompleted == false)
            {
                GetProductData();
            }

            if (_matchedProductData is null)
            {
                return new List<MetadataField>();
            }

            var fields = new List<MetadataField>();
            if (_settings.Settings.MetadataFieldsConfiguration.EnableName)
            {
                fields.Add(MetadataField.Name);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableAgeRating)
            {
                fields.Add(MetadataField.AgeRating);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnablePlatform)
            {
                fields.Add(MetadataField.Platform);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableDevelopers && !_matchedProductData.DeveloperName.IsNullOrEmpty())
            {
                fields.Add(MetadataField.Developers);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnablePublishers && !_matchedProductData.PublisherName.IsNullOrEmpty())
            {
                fields.Add(MetadataField.Publishers);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableDescription && !_matchedProductData.Description.IsNullOrEmpty())
            {
                fields.Add(MetadataField.Description);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableIcon && _matchedProductData.Images.Tile != null)
            {
                fields.Add(MetadataField.Icon);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableBackgroundImage && _matchedProductData.Images.SuperHeroArt != null)
            {
                fields.Add(MetadataField.BackgroundImage);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableCoverImage)
            {
                if (_settings.Settings.CoverFormat == CoverFormat.Vertical && _matchedProductData.Images.Poster != null)
                {
                    fields.Add(MetadataField.CoverImage);
                }
                else if (_settings.Settings.CoverFormat == CoverFormat.Square && _matchedProductData.Images.BoxArt != null)
                {
                    fields.Add(MetadataField.CoverImage);
                }
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableCommunityScore && _matchedProductData.AverageRating.HasValue && _matchedProductData.RatingCount >= 30)
            {
                fields.Add(MetadataField.CommunityScore);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableReleaseDate && _matchedProductData.ReleaseDate.HasValue)
            {
                fields.Add(MetadataField.ReleaseDate);
            }

            return fields;
        }

        public override string GetName(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Name))
            {
                return base.GetName(args);
            }

            return _matchedProductData.Title.NormalizeGameName();
        }

        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.ReleaseDate))
            {
                return base.GetReleaseDate(args);
            }

            return new ReleaseDate(_matchedProductData.ReleaseDate.Value.LocalDateTime);
        }

        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Developers))
            {
                return base.GetDevelopers(args);
            }

            return new List<MetadataProperty> { new MetadataNameProperty(_matchedProductData.DeveloperName) };
        }

        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Developers))
            {
                return base.GetPublishers(args);
            }

            return new List<MetadataProperty> { new MetadataNameProperty(_matchedProductData.PublisherName) };
        }

        public override IEnumerable<MetadataProperty> GetAgeRatings(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.AgeRating))
            {
                return base.GetAgeRatings(args);
            }

            var ageRatingMappers = GetClassificationBoardRatingMappers();
            var ageRatingMapper = ageRatingMappers[_matchedProductData.ContentRating.BoardName];
            var ageRatingPrefixMapper = new Dictionary<BoardName, string>
            {
                { BoardName.Cero, "CERO" },
                { BoardName.Esrb, "ESRB" },
                { BoardName.Iarc, "IARC" },
                { BoardName.Pegi, "PEGI" },
                { BoardName.Microsoft, "Microsoft" },
                { BoardName.Cob_Au, "ACB" },
                { BoardName.Usk, "USK" },
                { BoardName.Grb, "GRAC" },
                { BoardName.Djctq, "DJCTQ" }
            };

            if (ageRatingMapper.TryGetValue(_matchedProductData.ContentRating.RatingAge, out var mappedRating))
            {
                var prefix = ageRatingPrefixMapper[_matchedProductData.ContentRating.BoardName];
                return new List<MetadataProperty> { new MetadataNameProperty($"{prefix} {mappedRating}") };
            }
            else
            {
                _logger.Debug($"Couldn't find {_matchedProductData.ContentRating.BoardName} age rating mapping for \"{_matchedProductData.ContentRating.RatingAge}\"");
            }

            return base.GetAgeRatings(args);
        }

        public override IEnumerable<MetadataProperty> GetPlatforms(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Platform))
            {
                return base.GetPlatforms(args);
            }

            var platoformsMetadata = new HashSet<MetadataProperty>();
            if (_matchedProductData.AvailableOn.Contains(AvailableOn.Pc))
            {
                platoformsMetadata.Add(new MetadataSpecProperty("pc_windows"));
            }

            if (_matchedProductData.AvailableOn.Contains(AvailableOn.XboxOne))
            {
                platoformsMetadata.Add(new MetadataSpecProperty("xbox_one"));
            }

            if (_matchedProductData.AvailableOn.Contains(AvailableOn.XboxSeriesX))
            {
                platoformsMetadata.Add(new MetadataSpecProperty("xbox_series"));
            }

            return platoformsMetadata;
        }

        public override string GetDescription(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Description))
            {
                return base.GetDescription(args);
            }

            return _matchedProductData.Description;
        }

        public override MetadataFile GetIcon(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Icon))
            {
                return base.GetIcon(args);
            }

            return new MetadataFile(_matchedProductData.Images.Tile.Url.ToString());
        }

        public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.BackgroundImage))
            {
                return base.GetBackgroundImage(args);
            }

            var image = _matchedProductData.Images.SuperHeroArt;
            int? targetWidth = null;
            int? targetHeight = null;
            if (_settings.Settings.BackgroundImageResolution == BackgroundImageResolution.Resolution1920x1080)
            {
                targetWidth = 1920;
                targetHeight = 1080;
            }

            var modifiedUri = GetModifiedImageUri(image, _settings.Settings.BackgroundImageJpegQuality, targetWidth, targetHeight);
            return new MetadataFile(modifiedUri.ToString());
        }

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.CoverImage))
            {
                return base.GetCoverImage(args);
            }

            if (_settings.Settings.CoverFormat == CoverFormat.Vertical && _matchedProductData.Images.Poster != null)
            {
                var image = _matchedProductData.Images.Poster;
                int? targetWidth = null;
                int? targetHeight = null;
                if (_settings.Settings.VerticalCoverResolution == VerticalCoverResolution.Resolution600x900)
                {
                    targetWidth = 600;
                    targetHeight = 900;
                }
                else if (_settings.Settings.VerticalCoverResolution == VerticalCoverResolution.Resolution720x1080)
                {
                    targetWidth = 720;
                    targetHeight = 1080;
                }

                var modifiedUri = GetModifiedImageUri(image, _settings.Settings.CoverImageJpegQuality, targetWidth, targetHeight);
                return new MetadataFile(modifiedUri.ToString());
            }
            else if (_settings.Settings.CoverFormat == CoverFormat.Square && _matchedProductData.Images.BoxArt != null)
            {
                var image = _matchedProductData.Images.BoxArt;
                int? targetWidth = null;
                int? targetHeight = null;

                if (_settings.Settings.SquareCoverResolution == SquareCoverResolution.Resolution2160x2160)
                {
                    targetWidth = 2160;
                    targetHeight = 2160;
                }
                else if (_settings.Settings.SquareCoverResolution == SquareCoverResolution.Resolution1080x1080)
                {
                    targetWidth = 1080;
                    targetHeight = 1080;
                }
                else if (_settings.Settings.SquareCoverResolution == SquareCoverResolution.Resolution600x600)
                {
                    targetWidth = 600;
                    targetHeight = 600;
                }
                else if (_settings.Settings.SquareCoverResolution == SquareCoverResolution.Resolution300x300)
                {
                    targetWidth = 300;
                    targetHeight = 300;
                }

                var modifiedUri = GetModifiedImageUri(image, _settings.Settings.CoverImageJpegQuality, targetWidth, targetHeight);
                return new MetadataFile(modifiedUri.ToString());
            }

            return base.GetCoverImage(args);
        }

        private Uri GetModifiedImageUri(BoxArt image, int quality, int? targetWidth, int? targetHeight)
        {
            var uriBuilder = new UriBuilder(image.Url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            
            // Default quality is 90
            query["q"] = quality.ToString();
            if (targetWidth.HasValue && targetHeight.HasValue)
            {
                ApplyResolutionParameters(image, query, targetWidth.Value, targetHeight.Value);
            }

            uriBuilder.Query = query.ToString();
            var modifiedUri = uriBuilder.Uri;
            return modifiedUri;
        }

        private void ApplyResolutionParameters(BoxArt image, NameValueCollection query, int targetWidth, int targetHeight)
        {
            var isTargetSameAspectRatio = (image.Width / image.Height) == (targetWidth / targetHeight);
            if (!isTargetSameAspectRatio)
            {
                return;
            }

            if (image.Width > targetWidth && image.Height > targetHeight)
            {
                query["w"] = targetWidth.ToString();
                query["h"] = targetHeight.ToString();
            }
        }

        public override int? GetCommunityScore(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.CommunityScore))
            {
                return base.GetCommunityScore(args);
            }

            // Obtained ratings range is 0.0 to 5.0 and need to be converted to 100 scale
            var ratingOnHundredScale = _matchedProductData.AverageRating * 2 * 10;
            var adjustedScore = CalculateWilsonScore(ratingOnHundredScale.Value, _matchedProductData.RatingCount);
            return Convert.ToInt32(adjustedScore);
        }

        private void GetProductData()
        {
            if (_options.IsBackgroundDownload)
            {
                var gameName = _options.GameData.Name;
                var results = _xboxWebService.GetGameSearchResults(gameName);
                var normalizedGameName = gameName.Normalize();
                var matchingProduct = results.FirstOrDefault(x => x.Title.Normalize() == normalizedGameName);
                if (matchingProduct != null)
                {
                    _matchedProductData = matchingProduct;
                }
            }
            else
            {
                List<ProductSummary> results = null;
                List<GenericItemOption> itemOptions = null;
                var selectedItem = _plugin.PlayniteApi.Dialogs.ChooseItemWithSearch(null, (a) =>
                {
                    if (a.IsNullOrWhiteSpace())
                    {
                        return new List<GenericItemOption>();
                    }

                    results = _xboxWebService.GetGameSearchResults(a);
                    itemOptions = results.Select(x => CreateGenericItemOption(x)).ToList();
                    return itemOptions;
                }, _options.GameData.Name);

                if (selectedItem != null)
                {
                    var selectedIndex = itemOptions.IndexOf(selectedItem);
                    _matchedProductData = results[selectedIndex];
                }
            }

            _dataSearchCompleted = true;
        }

        private GenericItemOption CreateGenericItemOption(ProductSummary productSummary)
        {
            var descriptionLines = new List<string>();
            var availabilityPlatforms = new List<string>();
            if (productSummary.ReleaseDate.HasValue)
            {
                descriptionLines.Add(productSummary.ReleaseDate.Value.LocalDateTime.ToString("yyyy-MM-dd"));
            }

            if (productSummary.AvailableOn.Contains(AvailableOn.Pc))
            {
                availabilityPlatforms.Add("PC");
            }

            if (productSummary.AvailableOn.Contains(AvailableOn.XboxOne))
            {
                availabilityPlatforms.Add("Xbox One");
            }

            if (productSummary.AvailableOn.Contains(AvailableOn.XboxSeriesX))
            {
                availabilityPlatforms.Add("Xbox Series X");
            }

            descriptionLines.Add(string.Join(", ", availabilityPlatforms));
            if (!productSummary.ShortDescription.IsNullOrEmpty())
            {
                descriptionLines.Add(productSummary.ShortDescription);
            }
            else if (!productSummary.Description.IsNullOrEmpty())
            {
                descriptionLines.Add(productSummary.Description);
            }

            var description = string.Join("\n\n", descriptionLines);
            return new GenericItemOption(productSummary.Title, description);
        }

        /// <summary>
        /// Calculates a confidence interval-adjusted score using the Wilson Score Interval method.
        /// </summary>
        /// <param name="averageRating">The average rating of the product, ranging from 0 to 100.</param>
        /// <param name="numberOfVotes">The total number of votes received for the product.</param>
        /// <returns>The adjusted score representing the confidence interval of the rating.</returns>
        public static double CalculateWilsonScore(double averageRating, int numberOfVotes)
        {
            const double z = 1.96; // Z-score for 95% confidence level
            double phat = averageRating / 100.0;
            double n = numberOfVotes;

            double numerator = phat + z * z / (2 * n) - z * Math.Sqrt((phat * (1 - phat) + z * z / (4 * n)) / n);
            double denominator = 1 + z * z / n;

            return 100 * numerator / denominator;
        }

        private static Dictionary<BoardName, Dictionary<int, string>> GetClassificationBoardRatingMappers()
        {
            return new Dictionary<BoardName, Dictionary<int, string>>
            {
                [BoardName.Esrb] = new Dictionary<int, string>
                {
                    [1] = "E",
                    [10] = "E10",
                    [13] = "T",
                    [17] = "M"
                },

                [BoardName.Pegi] = new Dictionary<int, string>
                {
                    [3] = "3",
                    [7] = "7",
                    [12] = "12",
                    [16] = "16",
                    [18] = "18"
                },

                [BoardName.Cero] = new Dictionary<int, string>
                {
                    [1] = "A",
                    [12] = "B",
                    [15] = "C",
                    [17] = "D",
                    [18] = "Z"
                },

                [BoardName.Iarc] = new Dictionary<int, string>
                {
                    [0] = "3+",
                    [7] = "7+",
                    [12] = "12+",
                    [16] = "16+",
                    [18] = "18+"
                },

                [BoardName.Cob_Au] = new Dictionary<int, string>
                {
                    [1] = "G",
                    [8] = "PG",
                    [15] = "M",
                    [18] = "R18"
                },

                [BoardName.Usk] = new Dictionary<int, string>
                {
                    [1] = "0",
                    [6] = "6",
                    [12] = "12",
                    [16] = "16",
                    [18] = "18"
                },

                [BoardName.Grb] = new Dictionary<int, string>
                {
                    [1] = "All",
                    [12] = "12",
                    [15] = "15",
                    [18] = "18"
                },

                [BoardName.Djctq] = new Dictionary<int, string>
                {
                    [1] = "L",
                    [10] = "10",
                    [12] = "12",
                    [14] = "14",
                    [16] = "16",
                    [18] = "18",
                },

                [BoardName.Microsoft] = new Dictionary<int, string>
                {

                }
            };
        }

    }
}