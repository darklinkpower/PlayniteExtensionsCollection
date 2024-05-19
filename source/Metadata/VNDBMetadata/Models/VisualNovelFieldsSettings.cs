using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNDBMetadata.Fields;

namespace VNDBMetadata.Models
{
    public class VisualNovelFieldsSettings
    {
        private VisualNovelFields Fields = VisualNovelFields.None;
        private VisualNovelTitlesFields TitlesFields = VisualNovelTitlesFields.None;
        private VisualNovelImageFields ImageFields = VisualNovelImageFields.None;
        private VisualNovelScreenshotsFields ScreenshotsFields = VisualNovelScreenshotsFields.None;
        private VisualNovelTagsFields TagsFields = VisualNovelTagsFields.None;

        public VisualNovelFieldsSettings SetFields(VisualNovelFields fields)
        {
            Fields = fields;
            return this;
        }

        public VisualNovelFieldsSettings SetFields(VisualNovelTitlesFields fields)
        {
            TitlesFields = fields;
            return this;
        }

        public VisualNovelFieldsSettings SetFields(VisualNovelImageFields fields)
        {
            ImageFields = fields;
            return this;
        }

        public VisualNovelFieldsSettings SetFields(VisualNovelScreenshotsFields fields)
        {
            ScreenshotsFields = fields;
            return this;
        }

        public VisualNovelFieldsSettings SetFields(VisualNovelTagsFields fields)
        {
            TagsFields = fields;
            return this;
        }

        public void AddField(VisualNovelFields field)
        {
            Fields |= field;
        }

        public void AddField(VisualNovelTitlesFields field)
        {
            TitlesFields |= field;
        }

        public void AddField(VisualNovelImageFields field)
        {
            ImageFields |= field;
        }

        public void AddField(VisualNovelScreenshotsFields field)
        {
            ScreenshotsFields |= field;
        }

        public void AddField(VisualNovelTagsFields field)
        {
            TagsFields |= field;
        }

        public override string ToString()
        {
            var fieldsMappings = new Dictionary<VisualNovelFields, string>
            {
                { VisualNovelFields.Id, "id" },
                { VisualNovelFields.Title, "title" },
                { VisualNovelFields.AltTitle, "alttitle" },
                { VisualNovelFields.Aliases, "aliases" },
                { VisualNovelFields.Olang, "olang" },
                { VisualNovelFields.DevStatus, "devstatus" },
                { VisualNovelFields.Released, "released" },
                { VisualNovelFields.Languages, "languages" },
                { VisualNovelFields.Platforms, "platforms" },
                { VisualNovelFields.Length, "length" },
                { VisualNovelFields.LengthMinutes, "length_minutes" },
                { VisualNovelFields.LengthVotes, "length_votes" },
                { VisualNovelFields.Description, "description" },
                { VisualNovelFields.Rating, "rating" },
                { VisualNovelFields.VoteCount, "votecount" }
            };

            var titlesMappings = new Dictionary<VisualNovelTitlesFields, string>
            {
                { VisualNovelTitlesFields.Lang, "titles.lang" },
                { VisualNovelTitlesFields.Title, "titles.title" },
                { VisualNovelTitlesFields.Latin, "titles.latin" },
                { VisualNovelTitlesFields.Official, "titles.official" },
                { VisualNovelTitlesFields.Main, "titles.main" }
            };

            var imageMappings = new Dictionary<VisualNovelImageFields, string>
            {
                { VisualNovelImageFields.Id, "image.id" },
                { VisualNovelImageFields.Url, "image.url" },
                { VisualNovelImageFields.Dims, "image.dims" },
                { VisualNovelImageFields.Sexual, "image.sexual" },
                { VisualNovelImageFields.Violence, "image.violence" },
                { VisualNovelImageFields.VoteCount, "image.votecount" }
            };

            var screenshotsMappings = new Dictionary<VisualNovelScreenshotsFields, string>
            {
                { VisualNovelScreenshotsFields.Id, "screenshots.id" },
                { VisualNovelScreenshotsFields.Url, "screenshots.url" },
                { VisualNovelScreenshotsFields.Dims, "screenshots.dims" },
                { VisualNovelScreenshotsFields.Sexual, "screenshots.sexual" },
                { VisualNovelScreenshotsFields.Violence, "screenshots.violence" },
                { VisualNovelScreenshotsFields.VoteCount, "screenshots.votecount" },
                { VisualNovelScreenshotsFields.Thumbnail, "screenshots.thumbnail" },
                { VisualNovelScreenshotsFields.ThumbnailDims, "screenshots.thumbnail_dims" }
            };

            var tagsMappings = new Dictionary<VisualNovelTagsFields, string>
            {
                { VisualNovelTagsFields.Rating, "tags.rating" },
                { VisualNovelTagsFields.Spoiler, "tags.spoiler" },
                { VisualNovelTagsFields.Lie, "tags.lie" },
                { VisualNovelTagsFields.Id, "tags.id" },
                { VisualNovelTagsFields.Name, "tags.name" },
                { VisualNovelTagsFields.Aliases, "tags.aliases" },
                { VisualNovelTagsFields.Description, "tags.description" },
                { VisualNovelTagsFields.Category, "tags.category" },
                { VisualNovelTagsFields.Searchable, "tags.searchable" },
                { VisualNovelTagsFields.Applicable, "tags.applicable" },
                { VisualNovelTagsFields.VnCount, "tags.vn_count" }
            };

            var selectedFields = new List<string>();
            GetSelectedFields(selectedFields, Fields, fieldsMappings);
            GetSelectedFields(selectedFields, TitlesFields, titlesMappings);
            GetSelectedFields(selectedFields, ImageFields, imageMappings);
            GetSelectedFields(selectedFields, ScreenshotsFields, screenshotsMappings);
            GetSelectedFields(selectedFields, TagsFields, tagsMappings);

            return string.Join(",", selectedFields);
        }

        public static void GetSelectedFields<TEnum>(List<string> selectedFields, TEnum fields, Dictionary<TEnum, string> fieldMappings) where TEnum : Enum
        {
            foreach (TEnum field in Enum.GetValues(typeof(TEnum)))
            {
                // Check if the setting is enabled in fields and mappings contain the field
                if ((true || fields.HasFlag(field)) && fieldMappings.TryGetValue(field, out var fieldName))
                {
                    selectedFields.Add(fieldName);
                }
            }
        }

    }

}