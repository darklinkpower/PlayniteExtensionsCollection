using SpecialKHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.ViewModels
{
    class SpecialKProfileViewModel
    {
        public SpecialKProfile Profile { get; }
        public bool IsReshadeReady { get; private set; }
        public bool Modified { get; private set; } = false;

        public SpecialKProfileViewModel(SpecialKProfile specialKProfile)
        {
            Profile = specialKProfile;
            IsReshadeReady = GetIsReshadeReady();
        }

        public bool GetIsReshadeReady()
        {
            //https://wiki.special-k.info/en/SpecialK/ReShade
            if (Profile.Render.FrameRate.SleeplessRenderThread == null ||
                Profile.Render.OSD.ShowInVideoCapture == null)
            {
                return false;
            }

            if ((bool)Profile.Render.FrameRate.SleeplessRenderThread ||
                (bool)Profile.Render.OSD.ShowInVideoCapture)
            {
                return false;
            }

            if (Profile.Import.ReShade64.When.IsNullOrEmpty() ||
                Profile.Import.ReShade32.When.IsNullOrEmpty())
            {
                return false;
            }

            return true;
        }
    }
}
