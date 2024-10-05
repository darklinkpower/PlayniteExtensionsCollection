using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.PluginSidebarItem.Application
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
            var resourcesPath = Path.Combine(pluginInstallPath, "PluginSidebarItem", "Resources");
            IconEnabledPath = Path.Combine(resourcesPath, "SidebarEnabled.png");
            IconDisabledPath = Path.Combine(resourcesPath, "SidebarDisabled.png");
        }

        public bool SwitchAllowState()
        {
            AllowSkUse = !AllowSkUse;
            return allowSkUse;
        }
    }
}