using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;

namespace InstallationStatusUpdater.Application
{
    public class DeviceEventWatcherService : IDisposable
    {
        private readonly ILogger _logger;
        private readonly InstallationStatusUpdaterSettingsViewModel _settings;
        private bool _isHooked = false;
        public Action OnTrigger { get; set; }

        private HwndSource _hwndSource;

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        public DeviceEventWatcherService(
            ILogger logger,
            InstallationStatusUpdaterSettingsViewModel settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void HookDeviceEvents(WindowInteropHelper interopHelper)
        {
            if (_isHooked)
            {
                _logger.Debug("[DeviceEventWatcher] Service was already hooked."); 
                return;
            }

            _hwndSource = HwndSource.FromHwnd(interopHelper.Handle);
            if (_hwndSource is null)
            {
                _logger.Debug("[DeviceEventWatcher] Failed to get HwndSource from main window.");
                return;
            }

            _hwndSource.AddHook(WndProc);
            _isHooked = true;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!_settings.Settings.UpdateStatusOnUsbChanges)
            {
                return IntPtr.Zero;
            }

            if (msg == WM_DEVICECHANGE)
            {
                int eventType = wParam.ToInt32();
                if (eventType == DBT_DEVICEARRIVAL || eventType == DBT_DEVICEREMOVECOMPLETE)
                {
                    _logger.Debug("[DeviceEventWatcher] USB device event detected, triggering action.");
                    OnTrigger?.Invoke();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void UnhookDeviceEvents()
        {
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource = null;
                _isHooked = false;
            }
        }

        public void Dispose()
        {
            UnhookDeviceEvents();
        }
    }
}