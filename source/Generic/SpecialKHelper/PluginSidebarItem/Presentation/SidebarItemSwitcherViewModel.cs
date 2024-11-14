using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKHandler.Domain.Enums;
using SpecialKHelper.SpecialKHandler.Domain.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialKHelper.PluginSidebarItem.Application
{
    public class SidebarItemSwitcherViewModel : ObservableObject, IDisposable
    {
        private bool _isDisposed = false;
        private readonly SpecialKServiceManager _specialKServiceManager;
        public string IconEnabledPath { get; }
        public string Icon32BitsServiceOverlayPath { get; }
        public string Icon64BitsServiceOverlayPath { get; }
        public string IconDisabledPath { get; }

        private bool _allowSkUse = false;
        public bool AllowSkUse
        {
            get => _allowSkUse;
            set
            {
                _allowSkUse = value;
                OnPropertyChanged();
            }
        }

        private bool _show32BitsIcon = false;
        public bool Show32BitsIcon
        {
            get => _show32BitsIcon;
            set
            {
                _show32BitsIcon = value;
                OnPropertyChanged();
            }
        }

        private bool _show64BitsIcon = false;
        public bool Show64BitsIcon
        {
            get => _show64BitsIcon;
            set
            {
                _show64BitsIcon = value;
                OnPropertyChanged();
            }
        }

        public SidebarItemSwitcherViewModel(bool allowSkUse, string pluginInstallPath, SpecialKServiceManager specialKServiceManager)
        {
            AllowSkUse = allowSkUse;
            var resourcesPath = Path.Combine(pluginInstallPath, "PluginSidebarItem", "Resources");
            IconEnabledPath = Path.Combine(resourcesPath, "SidebarEnabled.png");
            IconDisabledPath = Path.Combine(resourcesPath, "SidebarDisabled.png");
            Icon32BitsServiceOverlayPath = Path.Combine(resourcesPath, "32BitsServiceOverlay.png");
            Icon64BitsServiceOverlayPath = Path.Combine(resourcesPath, "64BitsServiceOverlay.png");
            _specialKServiceManager = specialKServiceManager;
            specialKServiceManager.SpecialKServiceStatusChanged += SpecialKServiceManager_SpecialKServiceStatusChanged;
            Show32BitsIcon = specialKServiceManager.Service32BitsStatus == SpecialKServiceStatus.Running;
            Show64BitsIcon = specialKServiceManager.Service64BitsStatus == SpecialKServiceStatus.Running;
        }

        private void SpecialKServiceManager_SpecialKServiceStatusChanged(object sender, SpecialKServiceStatusChangedEventArgs e)
        {
            if (e.Architecture == CpuArchitecture.X64)
            {
                if (e.Status == SpecialKServiceStatus.Running)
                {
                    Show64BitsIcon = true;
                }
                else if (e.Status == SpecialKServiceStatus.Stopped)
                {
                    Show64BitsIcon = false;
                }
            }
            else
            {
                if (e.Status == SpecialKServiceStatus.Running)
                {
                    Show32BitsIcon = true;
                }
                else if (e.Status == SpecialKServiceStatus.Stopped)
                {
                    Show32BitsIcon = false;
                }
            }
        }

        public bool SwitchAllowState()
        {
            AllowSkUse = !AllowSkUse;
            return _allowSkUse;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _specialKServiceManager.SpecialKServiceStatusChanged -= SpecialKServiceManager_SpecialKServiceStatusChanged;
                _isDisposed = true;
            }
        }
    }
}