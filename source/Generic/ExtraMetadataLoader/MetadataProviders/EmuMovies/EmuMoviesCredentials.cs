using System;

namespace ExtraMetadataLoader.MetadataProviders
{
    internal class EmuMoviesCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsConfigured =>
            !Username.IsNullOrWhiteSpace() &&
            !Password.IsNullOrWhiteSpace();
    }
}
