using Playnite.SDK;
using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKHandler.Domain.Enums;
using SpecialKHelper.SpecialKHandler.Domain.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SpecialKHelper.PluginSidebarItem.Application
{
    public class SidebarItemSwitcherViewModel : ObservableObject, IDisposable
    {
        private bool _isDisposed = false;
        private readonly SpecialKServiceManager _specialKServiceManager;


        public string IconEnabledPath { get; }
        public string IconDisabledPath { get; }
        public string Icon32BitsServiceOverlayPath { get; }
        public string Icon64BitsServiceOverlayPath { get; }


        private bool _allowSkUse;
        public bool AllowSkUse
        {
            get => _allowSkUse;
            set => SetValue(ref _allowSkUse, value);
        }

        private bool _is32BitsServiceRunning;
        public bool Is32BitsServiceRunning
        {
            get => _is32BitsServiceRunning;
            set => SetValue(ref _is32BitsServiceRunning, value);
        }

        private bool _is64BitsServiceRunning;
        public bool Is64BitsServiceRunning
        {
            get => _is64BitsServiceRunning;
            set => SetValue(ref _is64BitsServiceRunning, value);
        }

        public ObservableCollection<Control> ContextMenuItems { get; } = new ObservableCollection<Control>();
        public RelayCommand Start32BitsServiceCommand { get; }
        public RelayCommand Stop32BitsServiceCommand { get; }
        public RelayCommand Start64BitsServiceCommand { get; }
        public RelayCommand Stop64BitsServiceCommand { get; }
        public RelayCommand StartAllServicesCommand { get; }
        public RelayCommand StopAllServicesCommand { get; }

        public SidebarItemSwitcherViewModel(bool allowSkUse, string pluginInstallPath, SpecialKServiceManager specialKServiceManager)
        {
            _specialKServiceManager = specialKServiceManager;

            AllowSkUse = allowSkUse;

            var resourcesPath = Path.Combine(pluginInstallPath, "PluginSidebarItem", "Resources");
            IconEnabledPath = Path.Combine(resourcesPath, "SidebarEnabled.png");
            IconDisabledPath = Path.Combine(resourcesPath, "SidebarDisabled.png");
            Icon32BitsServiceOverlayPath = Path.Combine(resourcesPath, "32BitsServiceOverlay.png");
            Icon64BitsServiceOverlayPath = Path.Combine(resourcesPath, "64BitsServiceOverlay.png");

            Start32BitsServiceCommand = new RelayCommand(() => _specialKServiceManager.Start32BitsService());
            Stop32BitsServiceCommand = new RelayCommand(() => _specialKServiceManager.Stop32BitsService());
            Start64BitsServiceCommand = new RelayCommand(() => _specialKServiceManager.Start64BitsService());
            Stop64BitsServiceCommand = new RelayCommand(() => _specialKServiceManager.Stop64BitsService());

            StartAllServicesCommand = new RelayCommand(() =>
            {
                _specialKServiceManager.Start32BitsService();
                _specialKServiceManager.Start64BitsService();
            });

            StopAllServicesCommand = new RelayCommand(() =>
            {
                _specialKServiceManager.Stop32BitsService();
                _specialKServiceManager.Stop64BitsService();
            });

            Is32BitsServiceRunning = _specialKServiceManager.Service32BitsStatus == SpecialKServiceStatus.Running;
            Is64BitsServiceRunning = _specialKServiceManager.Service64BitsStatus == SpecialKServiceStatus.Running;
            _specialKServiceManager.SpecialKServiceStatusChanged += SpecialKServiceManager_SpecialKServiceStatusChanged;
            UpdateContextMenuItems();
        }

        private void UpdateContextMenuItems()
        {
            ContextMenuItems.Clear();
            var anyServiceRunning = Is32BitsServiceRunning || Is64BitsServiceRunning;
            ContextMenuItems.Add(new MenuItem
            {
                Header = anyServiceRunning
                    ? ResourceProvider.GetString("LOCSpecial_K_Helper_StopAllServices")
                    : ResourceProvider.GetString("LOCSpecial_K_Helper_StartAllServices"),
                Command = anyServiceRunning ? StopAllServicesCommand : StartAllServicesCommand
            });

            ContextMenuItems.Add(new Separator());
            ContextMenuItems.Add(new MenuItem
            {
                Header = Is32BitsServiceRunning
                    ? ResourceProvider.GetString("LOCSpecial_K_Helper_Stop32BitsService")
                    : ResourceProvider.GetString("LOCSpecial_K_Helper_Start32BitsService"),
                Command = Is32BitsServiceRunning ? Stop32BitsServiceCommand : Start32BitsServiceCommand
            });

            ContextMenuItems.Add(new MenuItem
            {
                Header = Is64BitsServiceRunning
                    ? ResourceProvider.GetString("LOCSpecial_K_Helper_Stop64BitsService")
                    : ResourceProvider.GetString("LOCSpecial_K_Helper_Start64BitsService"),
                Command = Is64BitsServiceRunning ? Stop64BitsServiceCommand : Start64BitsServiceCommand
            });
        }

        private void SpecialKServiceManager_SpecialKServiceStatusChanged(object sender, SpecialKServiceStatusChangedEventArgs e)
        {
            if (e.Architecture == CpuArchitecture.X64)
            {
                Is64BitsServiceRunning = e.Status == SpecialKServiceStatus.Running;
            }
            else
            {
                Is32BitsServiceRunning = e.Status == SpecialKServiceStatus.Running;
            }

            UpdateContextMenuItems();
        }

        public bool SwitchAllowState()
        {
            AllowSkUse = !AllowSkUse;
            return AllowSkUse;
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