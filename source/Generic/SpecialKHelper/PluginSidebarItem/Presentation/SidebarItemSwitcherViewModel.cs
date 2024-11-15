using Playnite.SDK;
using SpecialKHelper.SpecialKHandler.Application;
using SpecialKHelper.SpecialKHandler.Domain.Enums;
using SpecialKHelper.SpecialKHandler.Domain.Events;
using SpecialKHelper.SpecialKProfilesEditorService.Application;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace SpecialKHelper.PluginSidebarItem.Application
{
    public class SidebarItemSwitcherViewModel : ObservableObject, IDisposable
    {
        private bool _isDisposed = false;
        private readonly SpecialKServiceManager _specialKServiceManager;
        private readonly SpecialKProfilesEditor _specialKProfilesEditor;
        private readonly Dispatcher _uiDispatcher;

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
        public RelayCommand OpenSpecialKCommand { get; }
        public RelayCommand OpenProfilesEditorCommand { get; }

        public SidebarItemSwitcherViewModel(
            bool allowSkUse,
            string pluginInstallPath,
            SpecialKServiceManager specialKServiceManager,
            SpecialKProfilesEditor specialKProfilesEditor)
        {
            _specialKServiceManager = specialKServiceManager;
            _specialKProfilesEditor = specialKProfilesEditor;

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
            OpenSpecialKCommand = new RelayCommand(() => _specialKServiceManager.OpenSpecialK());
            OpenProfilesEditorCommand = new RelayCommand(() => _specialKProfilesEditor.OpenEditorWindow());

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
            _uiDispatcher = System.Windows.Application.Current.Dispatcher;
            RefreshContextMenuItemsOnUIThread();
        }

        private void RefreshContextMenuItemsOnUIThread()
        {
            if (_uiDispatcher.CheckAccess())
            {
                RefreshContextMenuItems();
            }
            else
            {
                _uiDispatcher.Invoke(() =>
                {
                    RefreshContextMenuItems();
                });
            }
        }

        private void RefreshContextMenuItems()
        {
            ContextMenuItems.Clear();
            ContextMenuItems.Add(new MenuItem
            {
                Header = ResourceProvider.GetString("LOCSpecial_K_Helper_OpenSpecialK"),
                Command = OpenSpecialKCommand
            });

            ContextMenuItems.Add(new MenuItem
            {
                Header = ResourceProvider.GetString("LOCSpecial_K_Helper_MenuItemDescriptionOpenEditor"),
                Command = OpenProfilesEditorCommand
            });

            ContextMenuItems.Add(new Separator());
            var bothServicesRunning = Is32BitsServiceRunning && Is64BitsServiceRunning;
            if (bothServicesRunning)
            {
                ContextMenuItems.Add(new MenuItem
                {
                    Header = ResourceProvider.GetString("LOCSpecial_K_Helper_StopAllServices"),
                    Command = StopAllServicesCommand
                });
                ContextMenuItems.Add(new Separator());
            }

            var bothServicesNotRunning = !Is32BitsServiceRunning && !Is64BitsServiceRunning;
            if (bothServicesNotRunning)
            {
                ContextMenuItems.Add(new MenuItem
                {
                    Header = ResourceProvider.GetString("LOCSpecial_K_Helper_StartAllServices"),
                    Command = StartAllServicesCommand
                });
                ContextMenuItems.Add(new Separator());
            }

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

            RefreshContextMenuItemsOnUIThread();
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