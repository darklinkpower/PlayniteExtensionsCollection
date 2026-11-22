namespace ExtraMetadataLoader.MetadataProviders
{
    internal class EmuMoviesVideoFile
    {
        public string FileName { get; set; }
        public string FtpPath { get; set; }
        public string PlatformName { get; set; }
        public string PlatformDirectoryName { get; set; }
        public EmuMoviesVideoQuality Quality { get; set; }
    }
}
