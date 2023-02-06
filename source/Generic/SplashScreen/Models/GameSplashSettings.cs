using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplashScreen.Models
{
    public class GameSplashSettings
    {
        public bool EnableGameSpecificSettings { get; set; } = true;
        public GeneralSplashSettings GeneralSplashSettings { get; set; } = new GeneralSplashSettings();
    }
}
