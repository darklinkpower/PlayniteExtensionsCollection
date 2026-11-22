using System;
using System.Collections.Generic;

namespace ExtraMetadataLoader.MetadataProviders.LaunchBox
{
    internal class LaunchBoxMetadataIndex
    {
        public int SchemaVersion { get; set; }
        public DateTime CreatedUtc { get; set; }
        public List<LaunchBoxGameEntry> Games { get; set; } = new List<LaunchBoxGameEntry>();
    }

    internal class LaunchBoxGameEntry
    {
        public string DatabaseId { get; set; }
        public string Name { get; set; }
        public string Platform { get; set; }
        public int? ReleaseYear { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public List<string> AlternateNames { get; set; } = new List<string>();
        public List<LaunchBoxLogoEntry> Logos { get; set; } = new List<LaunchBoxLogoEntry>();
    }

    internal class LaunchBoxLogoEntry
    {
        public string FileName { get; set; }
        public string Region { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    internal class LaunchBoxMetadataCacheState
    {
        public int SchemaVersion { get; set; }
        public DateTime LastCheckedUtc { get; set; }
        public DateTime SourceTimestampUtc { get; set; }
        public DateTime MetadataDownloadedUtc { get; set; }
        public string MetadataETag { get; set; }
        public string MetadataLastModified { get; set; }
        public long? MetadataContentLength { get; set; }
    }

    internal class LaunchBoxMatch
    {
        public LaunchBoxGameEntry Game { get; set; }
        public int Score { get; set; }
        public int NameScore { get; set; }
        public string MatchReason { get; set; }
    }
}
