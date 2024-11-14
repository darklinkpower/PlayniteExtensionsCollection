using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKProfilesEditor.Domain
{
    public class SpecialKProfileData
    {
        public Guid Id { get; }
        public string ProfileDirectory { get; }
        public string Name => Path.GetFileName(ProfileDirectory);
        public string ConfigurationPath => Path.Combine(ProfileDirectory, "SpecialK.ini");

        public SpecialKProfileData(string profileDirectory)
        {
            Id = Guid.NewGuid();
            ProfileDirectory = profileDirectory;
        }
    }
}