using System.IO;

namespace ExtraMetadataLoader.MetadataProviders
{
    internal class EmuMoviesVideoMatch
    {
        public string FileName { get; set; }
        public string FtpPath { get; set; }
        public string PlatformName { get; set; }
        public string PlatformDirectoryName { get; set; }
        public EmuMoviesVideoQuality Quality { get; set; }
        public int Score { get; set; }

        public string DisplayName => $"{Path.GetFileNameWithoutExtension(FileName)} ({PlatformName}, {Quality})";
        public string Description => $"{PlatformName} | {Quality} | {PlatformDirectoryName} | {FileName}";
    }
}
