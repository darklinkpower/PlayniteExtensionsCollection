using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser.Model;

namespace SpecialKHelper.Models
{
    class SpecialKProfile
    {
        public string ProfileName { get; set; }
        public string ProfileIniPath { get; set; }
        public IniData IniData { get; set; }

        public class ProfileSection
        {
            public string Name { get; set; } = string.Empty;
            public Render_FrameRate FrameRate { get; set; } = new Render_FrameRate();
            public Render_OSD OSD { get; set; } = new Render_OSD();

            public class Render_DXGI
            {
                public bool? UseFlipDiscard { get; set; } = null;
            }

            public class Render_FrameRate
            {
                public double TargetFPS { get; set; } = 0.0;
                public bool? SleeplessRenderThread { get; set ; } = null;
            }

            public class Render_OSD
            {
                public bool? ShowInVideoCapture { get; set; } = null;
            }
        }

        public class SteamSection
        {
            public Steam_System System { get; set; } = new Steam_System();

            public class Steam_System
            {
                public string AppID { get; set; } = string.Empty;
                public bool? PreLoadSteamOverlay { get; set; } = null;
            }
        }

        public class CompatibilitySection
        {
            public Compatibility_General General { get; set; } = new Compatibility_General();

            public class Compatibility_General
            {
                public bool? DisableBloatWare_NVIDIA { get; set; } = null;
            }
        }

        public class ImportSection
        {
            public Import_ReShade64 ReShade64 { get; set; } = new Import_ReShade64();
            public Import_ReShade32 ReShade32 { get; set; } = new Import_ReShade32();

            public class Import_ReShade64 : ObservableObject
            {
                public string Architecture { get; set; } = "x64";
                public string Role { get; set; } = "ThirdParty";
                private string when { get; set; } = null;
                public string When
                {
                    get => when;
                    set
                    {
                        when = value;
                        OnPropertyChanged();
                    }
                }
                public string Filename { get; set; } = @"..\..\PlugIns\ThirdParty\ReShade\ReShade64.dll";
            }

            public class Import_ReShade32 : ObservableObject
            {
                public string Architecture { get; set; } = "Win32";
                public string Role { get; set; } = "ThirdParty";
                private string when { get; set; } = null;
                public string When
                {
                    get => when;
                    set
                    {
                        when = value;
                        OnPropertyChanged();
                    }
                }
                public string Filename { get; set; } = @"..\..\PlugIns\ThirdParty\ReShade\ReShade32.dll";
            }
        }
    }

    public enum PluginLoadMode
    {
        [Description("Early")]
        Early,
        [Description("PlugIn")]
        PlugIn
    }
}