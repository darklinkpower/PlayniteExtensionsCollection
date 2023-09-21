using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNDBMetadata.Enums
{
    [Flags]
    public enum VisualNovelFields
    {
        None,
        Id,
        Title,
        AltTitle,
        Aliases,
        Olang,
        DevStatus,
        Released,
        Languages,
        Platforms,
        Length,
        LengthMinutes,
        LengthVotes,
        Description,
        Rating,
        VoteCount
    }

    [Flags]
    public enum VisualNovelTitlesFields
    {
        None,
        Lang,
        Title,
        Latin,
        Official,
        Main
    }

    [Flags]
    public enum VisualNovelImageFields
    {
        None,
        Id,
        Url,
        Dims,
        Sexual,
        Violence,
        VoteCount
    }

    [Flags]
    public enum VisualNovelScreenshotsFields
    {
        None,
        Id,
        Url,
        Dims,
        Sexual,
        Violence,
        VoteCount,
        Thumbnail,
        ThumbnailDims
    }

    [Flags]
    public enum VisualNovelTagsFields
    {
        None,
        Rating,
        Spoiler,
        Lie,
        Id,
        Name,
        Aliases,
        Description,
        Category,
        Searchable,
        Applicable,
        VnCount
    }




}