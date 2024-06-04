using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VndbApiDomain.ImageAggregate;
using VndbApiDomain.SharedKernel;
using VndbApiDomain.TagAggregate;
using VndbApiDomain.VisualNovelAggregate;
using VndbApiInfrastructure.ProducerAggregate;
using VndbApiInfrastructure.Services;
using VndbApiInfrastructure.TagAggregate;
using VndbApiInfrastructure.VisualNovelAggregate;

namespace VNDBNexus
{
    public class VNDBNexusMetadataProvider : OnDemandMetadataProvider
    {
        private readonly MetadataRequestOptions _requestOptions;
        private readonly VNDBNexusSettingsViewModel _settings;
        private readonly BbCodeProcessor _bbcodeProcessor;
        private static readonly ILogger _logger = LogManager.GetLogger();
        private bool _dataSearchCompleted = false;

        private List<MetadataField> availableFields = null;
        private VisualNovel _matchedVisualNovel;

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

        public VNDBNexusMetadataProvider(MetadataRequestOptions options, VNDBNexusSettingsViewModel settings, BbCodeProcessor bbcodeProcessor)
        {
            _requestOptions = options;
            _settings = settings;
            _bbcodeProcessor = bbcodeProcessor;
        }

        private List<MetadataField> GetAvailableFields()
        {
            if (_dataSearchCompleted == false)
            {
                SearchVisualNovel();
            }

            var fields = new List<MetadataField>();
            if (_matchedVisualNovel is null)
            {
                return fields;
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableName)
            {
                fields.Add(MetadataField.Name);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnablePlatform && _matchedVisualNovel.Platforms.HasItems())
            {
                fields.Add(MetadataField.Platform);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableDevelopers && _matchedVisualNovel.Developers.HasItems())
            {
                fields.Add(MetadataField.Developers);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableDescription && !_matchedVisualNovel.Description.IsNullOrEmpty())
            {
                fields.Add(MetadataField.Description);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableBackgroundImage && _matchedVisualNovel.Screenshots.HasItems())
            {
                var filteredScreenshots = _matchedVisualNovel.Screenshots.Where(img => IsImageAllowed(img));
                if (filteredScreenshots.HasItems())
                {
                    _matchedVisualNovel.Screenshots = filteredScreenshots.ToList();
                    fields.Add(MetadataField.BackgroundImage);
                }
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableCoverImage && _matchedVisualNovel.Image != null)
            {
                if (IsImageAllowed(_matchedVisualNovel.Image))
                {
                    fields.Add(MetadataField.CoverImage);
                };
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableCommunityScore && _matchedVisualNovel.Rating.HasValue)
            {
                fields.Add(MetadataField.CommunityScore);
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableReleaseDate && _matchedVisualNovel.ReleaseDate != null)
            {
                if (_settings.Settings.MetadataAllowPartialDates ||
                    (_matchedVisualNovel.ReleaseDate.Month.HasValue && _matchedVisualNovel.ReleaseDate.Day.HasValue))
                {
                    fields.Add(MetadataField.ReleaseDate);
                }
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableTags && _matchedVisualNovel.Tags.HasItems())
            {
                var maxSpoiler = _settings.Settings.TagsMaxSpoilerLevel;
                var filteredTags = _matchedVisualNovel.Tags
                    .Where(tag => tag.Rating >= _settings.Settings.TagsMinimumScore &&
                          (tag.Spoiler == SpoilerLevelEnum.None
                           || maxSpoiler == SpoilerLevelEnum.Major
                           || (maxSpoiler == SpoilerLevelEnum.Minimum && tag.Spoiler == SpoilerLevelEnum.Minimum)));

                var secondFilteredList = filteredTags
                    .Where(tag => (tag.Category == TagCategoryEnum.Content && _settings.Settings.TagsImportContentCat)
                               || (tag.Category == TagCategoryEnum.Technical && _settings.Settings.TagsImportTechnicalCat)
                               || (tag.Category == TagCategoryEnum.SexualContent && _settings.Settings.TagsImportSexualCat));

                if (secondFilteredList.HasItems())
                {
                    _matchedVisualNovel.Tags = secondFilteredList.ToList();
                    fields.Add(MetadataField.Tags);
                }

                if (_settings.Settings.MetadataFieldsConfiguration.EnableLinks)
                {
                    fields.Add(MetadataField.Links);
                }

                //if (_settings.Settings.MetadataFieldsConfiguration.EnablePublishers)
                //{
                //    fields.Add(MetadataField.Publishers);
                //}
            }

            return fields;
        }

        private bool IsImageAllowed(VndbImage image)
        {
            var maxSexuality = _settings.Settings.ImagesMaxSexualityLevel;
            var maxViolence = _settings.Settings.ImagesMaxViolenceLevel;

            var sexualityPassed = image.SexualityLevel == ImageSexualityLevelEnum.Safe ||
                (maxSexuality == ImageSexualityLevelEnum.Suggestive && image.SexualityLevel == ImageSexualityLevelEnum.Suggestive) ||
                maxSexuality == ImageSexualityLevelEnum.Explicit;

            if (!sexualityPassed)
            {
                return false;
            }

            var violencePassed = image.ViolenceLevel == ImageViolenceLevelEnum.Tame ||
                (maxViolence == ImageViolenceLevelEnum.Violent && image.ViolenceLevel == ImageViolenceLevelEnum.Violent) ||
                maxViolence == ImageViolenceLevelEnum.Brutal;

            if (!violencePassed)
            {
                return false;
            }

            return true;
        }

        private void SearchVisualNovel()
        {
            if (_requestOptions.IsBackgroundDownload)
            {
                var gameName = _requestOptions.GameData.Name;
                var searchResults = GetVnSearchResults(gameName);
                var normalizedGameName = gameName.Satinize();

                var matchingVisualNovel = searchResults.FirstOrDefault(
                    x => x.Title.Satinize() == normalizedGameName ||
                    x.Aliases?.Any(x => x.Satinize() == normalizedGameName) == true ||
                    x.Titles?.Any(x => x.Title.Satinize() == normalizedGameName) == true);
                if (matchingVisualNovel != null)
                {
                    _matchedVisualNovel = matchingVisualNovel;
                }
            }
            else
            {
                List<VisualNovel> searchResults = null;
                List<GenericItemOption> itemOptions = null;
                var selectedItem = API.Instance.Dialogs.ChooseItemWithSearch(null, (a) =>
                {
                    if (a.IsNullOrWhiteSpace())
                    {
                        return new List<GenericItemOption>();
                    }

                    searchResults = GetVnSearchResults(a);
                    itemOptions = searchResults.Select(x => CreateGenericItemOption(x)).ToList();
                    return itemOptions;
                }, _requestOptions.GameData.Name);

                if (selectedItem != null)
                {
                    var selectedIndex = itemOptions.IndexOf(selectedItem);
                    _matchedVisualNovel = searchResults[selectedIndex];
                }
            }

            _dataSearchCompleted = true;
        }

        private List<VisualNovel> GetVnSearchResults(string searchTerm)
        {
            var results = new List<VisualNovel>();
            var isSearchVndbId = Regex.IsMatch(searchTerm, @"^v\d+$");
            var vndbRequestFilter = isSearchVndbId
                ? VisualNovelFilterFactory.Id.EqualTo(searchTerm)
                : VisualNovelFilterFactory.Search.EqualTo(searchTerm);

            var query = new VisualNovelRequestQuery(vndbRequestFilter);
            query.Fields.DisableAllFlags(true);

            query.Fields.Flags = VnRequestFieldsFlags.Title | VnRequestFieldsFlags.Id | VnRequestFieldsFlags.Aliases | VnRequestFieldsFlags.TitlesTitle;

            if (_settings.Settings.MetadataFieldsConfiguration.EnableDescription)
            {
                query.Fields.Flags |= VnRequestFieldsFlags.Description;
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableCommunityScore)
            {
                query.Fields.Flags |= VnRequestFieldsFlags.Rating;
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableReleaseDate)
            {
                query.Fields.Flags |= VnRequestFieldsFlags.ReleaseDate;
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnablePlatform)
            {
                query.Fields.Flags |= VnRequestFieldsFlags.Platforms;
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableCoverImage)
            {
                query.Fields.Subfields.Image.Flags =
                    ImageRequestFieldsFlags.ThumbnailUrl | ImageRequestFieldsFlags.VoteCount | ImageRequestFieldsFlags.Sexual
                    | ImageRequestFieldsFlags.Violence | ImageRequestFieldsFlags.Url;
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableBackgroundImage)
            {
                query.Fields.Flags |= VnRequestFieldsFlags.TagsRating | VnRequestFieldsFlags.TagsSpoiler;
                query.Fields.Subfields.Screenshots.Flags =
                    ImageRequestFieldsFlags.ThumbnailUrl | ImageRequestFieldsFlags.VoteCount | ImageRequestFieldsFlags.Sexual
                    | ImageRequestFieldsFlags.Violence | ImageRequestFieldsFlags.Url;
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableDevelopers)
            {
                query.Fields.Subfields.Developers.Flags = ProducerRequestFieldsFlags.Name | ProducerRequestFieldsFlags.Type;
            }

            if (_settings.Settings.MetadataFieldsConfiguration.EnableTags)
            {
                query.Fields.Subfields.Tags.Flags = TagRequestFieldsFlags.Name | TagRequestFieldsFlags.Category;
            }
            

            if (_settings.Settings.MetadataFieldsConfiguration.EnableLinks)
            {
                query.Fields.Subfields.ExternalLinks.Flags = ExtLinksFieldsFlags.Label | ExtLinksFieldsFlags.Url;
            }

            query.Results = 6;
            var queryResult = VndbService.ExecutePostRequestAsync(query).GetAwaiter().GetResult();
            if (queryResult?.Results?.Count > 0)
            {
                results.AddRange(queryResult.Results);
            }

            return results;
        }

        private GenericItemOption CreateGenericItemOption(VisualNovel visualNovel)
        {
            var description = visualNovel.Id;
            if (visualNovel.ReleaseDate != null)
            {
                description += Environment.NewLine + visualNovel.ReleaseDate.ToString();
            }

            if (!visualNovel.Description.IsNullOrEmpty())
            {
                var trimmedDescription = visualNovel.Description.TrimStringWithEllipsis(300, true);
                description += Environment.NewLine + Environment.NewLine + trimmedDescription;
            }

            return new GenericItemOption(visualNovel.Title, description);
        }

        public override string GetName(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Name))
            {
                return base.GetName(args);
            }

            return _matchedVisualNovel.Title.NormalizeGameName();
        }

        public override ReleaseDate? GetReleaseDate(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.ReleaseDate))
            {
                return base.GetReleaseDate(args);
            }

            if (!_matchedVisualNovel.ReleaseDate.Month.HasValue)
            {
                return new ReleaseDate(_matchedVisualNovel.ReleaseDate.Year);
            }

            if (!_matchedVisualNovel.ReleaseDate.Day.HasValue)
            {
                return new ReleaseDate(_matchedVisualNovel.ReleaseDate.Year, _matchedVisualNovel.ReleaseDate.Month.Value);
            }

            return new ReleaseDate(
                _matchedVisualNovel.ReleaseDate.Year,
                _matchedVisualNovel.ReleaseDate.Month.Value,
                _matchedVisualNovel.ReleaseDate.Day.Value);
        }

        public override IEnumerable<MetadataProperty> GetDevelopers(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Developers))
            {
                return base.GetDevelopers(args);
            }

            return _matchedVisualNovel.Developers.Select(x => new MetadataNameProperty(x.Name));
        }

        public override IEnumerable<MetadataProperty> GetPublishers(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Publishers))
            {
                return base.GetPublishers(args);
            }

            return base.GetPublishers(args);
        }

        public override IEnumerable<Link> GetLinks(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Links))
            {
                return base.GetLinks(args);
            }

            var links = new List<Link>
            {
                new Link { Name = "VNDB", Url = $"https://vndb.org/{_matchedVisualNovel.Id}" }
            };

            var externalLinks = _matchedVisualNovel.ExternalLinks?.Select(x => new Link
            {
                Name = x.Label,
                Url = x.Url.ToString()
            });

            if (externalLinks.HasItems())
            {
                links.AddRange(externalLinks);
            }
            
            return links;
        }

        public override IEnumerable<MetadataProperty> GetPlatforms(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Platform))
            {
                return base.GetPlatforms(args);
            }

            var vndbPlatformToSpecIdMapper = new Dictionary<PlatformEnum, string>
            {
                { PlatformEnum.Windows, "pc_windows" },
                { PlatformEnum.Linux, "pc_linux" },
                { PlatformEnum.MacOs, "macintosh" },
                { PlatformEnum.Website, "Placeholder" },
                { PlatformEnum.ThreeDO, "3do" },
                { PlatformEnum.AppleIProduct, "Placeholder" },
                { PlatformEnum.Android, "Placeholder" },
                { PlatformEnum.BluRayPlayer, "Placeholder" },
                { PlatformEnum.DOS, "pc_dos" },
                { PlatformEnum.DVDPlayer, "Placeholder" },
                { PlatformEnum.Dreamcast, "sega_dreamcast" },
                { PlatformEnum.Famicom, "nintendo_nes" },
                { PlatformEnum.SuperFamicom, "nintendo_super_nes" },
                { PlatformEnum.FM7, "Placeholder" },
                { PlatformEnum.FM8, "Placeholder" },
                { PlatformEnum.FMTowns, "Placeholder" },
                { PlatformEnum.GameBoyAdvance, "nintendo_gameboyadvance" },
                { PlatformEnum.GameBoyColor, "nintendo_gameboycolor" },
                { PlatformEnum.MSX, "microsoft_msx" },
                { PlatformEnum.NintendoDS, "nintendo_ds" },
                { PlatformEnum.NintendoSwitch, "nintendo_switch" },
                { PlatformEnum.NintendoWii, "nintendo_wii" },
                { PlatformEnum.NintendoWiiU, "nintendo_wiiu" },
                { PlatformEnum.Nintendo3DS, "nintendo_3ds" },
                { PlatformEnum.PC88, "Placeholder" },
                { PlatformEnum.PC98, "nec_pc98" },
                { PlatformEnum.PCEngine, "Placeholder" },
                { PlatformEnum.PCFX, "nec_pcfx" },
                { PlatformEnum.PlayStationPortable, "sony_psp" },
                { PlatformEnum.PlayStation1, "sony_playstation" },
                { PlatformEnum.PlayStation2, "sony_playstation2" },
                { PlatformEnum.PlayStation3, "sony_playstation3" },
                { PlatformEnum.PlayStation4, "sony_playstation4" },
                { PlatformEnum.PlayStation5, "sony_playstation5" },
                { PlatformEnum.PlayStationVita, "sony_vita" },
                { PlatformEnum.SegaMegaDrive, "sega_genesis" },
                { PlatformEnum.SegaMegaCD, "sega_cd" },
                { PlatformEnum.SegaSaturn, "sega_saturn" },
                { PlatformEnum.VNDS, "Placeholder" },
                { PlatformEnum.SharpX1, "Placeholder" },
                { PlatformEnum.SharpX68000, "sharp_x68000" },
                { PlatformEnum.Xbox, "xbox" },
                { PlatformEnum.Xbox360, "xbox360" },
                { PlatformEnum.XboxOne, "xbox_one" },
                { PlatformEnum.XboxX_S, "xbox_series" },
                { PlatformEnum.OtherMobile, "Placeholder" },
                { PlatformEnum.Other, "Placeholder" }
            };

            var platformsSpecIds = new HashSet<MetadataProperty>();
            foreach (var platform in _matchedVisualNovel.Platforms)
            {
                if (vndbPlatformToSpecIdMapper.TryGetValue(platform, out var specId) && specId != "Placeholder")
                {
                    platformsSpecIds.Add(new MetadataSpecProperty(specId));
                }
            }

            return platformsSpecIds;
        }

        public override string GetDescription(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Description))
            {
                return base.GetDescription(args);
            }

            return _bbcodeProcessor.ToHtml(_matchedVisualNovel.Description);
        }

        public override MetadataFile GetBackgroundImage(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.BackgroundImage))
            {
                return base.GetBackgroundImage(args);
            }
             
            if (_requestOptions.IsBackgroundDownload)
            {
                return new MetadataFile(_matchedVisualNovel.Screenshots[0].Url.ToString());
            }

            var screenshots = _matchedVisualNovel.Screenshots;
            var imageOptions = screenshots.Select(
                x => new ImageFileOption(x.ThumbnailUrl.ToString()))
                .ToList();

            var selectedImage = API.Instance.Dialogs.ChooseImageFile(
                imageOptions,
                ResourceProvider.GetString("LOC_VndbNexus_MetadataSelectBackgroundImage"));
            if (selectedImage != null)
            {
                var selectionIndex = imageOptions.IndexOf(selectedImage);
                var selectedScreenshot = screenshots[selectionIndex];
                return new MetadataFile(selectedScreenshot.Url.ToString());
            }

            return base.GetBackgroundImage(args);
        }

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.CoverImage))
            {
                return base.GetCoverImage(args);
            }
            
            return new MetadataFile(_matchedVisualNovel.Image.Url.ToString());
        }

        public override int? GetCommunityScore(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.CommunityScore))
            {
                return base.GetCommunityScore(args);
            }

            return Convert.ToInt32(_matchedVisualNovel.Rating.Value);
        }

        public override IEnumerable<MetadataProperty> GetTags(GetMetadataFieldArgs args)
        {
            if (!AvailableFields.Contains(MetadataField.Tags))
            {
                return base.GetTags(args);
            }

            var tags = new List<MetadataNameProperty>();
            foreach (var tag in _matchedVisualNovel.Tags)
            {
                if (tag.Category == TagCategoryEnum.Content)
                {
                    tags.Add(new MetadataNameProperty($"{_settings.Settings.TagsPrefixContentCat}{tag.Name}"));
                }
                else if (tag.Category == TagCategoryEnum.Technical)
                {
                    tags.Add(new MetadataNameProperty($"{_settings.Settings.TagsPrefixTechnicalCat}{tag.Name}"));
                }
                else if (tag.Category == TagCategoryEnum.SexualContent)
                {
                    tags.Add(new MetadataNameProperty($"{_settings.Settings.TagsPrefixSexualCat}{tag.Name}"));
                }
            }

            return tags;
        }

        
    }
}