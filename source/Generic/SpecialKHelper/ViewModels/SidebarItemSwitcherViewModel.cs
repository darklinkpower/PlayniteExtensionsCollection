using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.ViewModels
{
    class SidebarItemSwitcherViewModel : ObservableObject
    {
        private bool allowSkUse { get; set; } = false;
        public bool AllowSkUse
        {
            get => allowSkUse;
            set
            {
                allowSkUse = value;
                OnPropertyChanged();
            }
        }

        private string iconEnabledPath;
        public string IconEnabledPath
        {
            get => iconEnabledPath;
            set
            {
                iconEnabledPath = value;
                OnPropertyChanged();
            }
        }

        private string iconDisabledPath;
        public string IconDisabledPath
        {
            get => iconDisabledPath;
            set
            {
                iconDisabledPath = value;
                OnPropertyChanged();
            }
        }

        public SidebarItemSwitcherViewModel(bool allowSkUse, string pluginInstallPath)
        {
            AllowSkUse = allowSkUse;
            IconEnabledPath = Path.Combine(pluginInstallPath, "Resources", "SidebarEnabled.png");
            IconDisabledPath = Path.Combine(pluginInstallPath, "Resources", "SidebarDisabled.png");
        }

        public bool SwitchAllowState()
        {
            AllowSkUse = !AllowSkUse;
            return allowSkUse;
        }
    }
}
