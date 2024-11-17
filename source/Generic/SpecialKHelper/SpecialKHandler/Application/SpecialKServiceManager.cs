using Microsoft.Win32;
using Playnite.SDK;
using PluginsCommon;
using SpecialKHelper.SpecialKHandler.Domain.Enums;
using SpecialKHelper.SpecialKHandler.Domain.Events;
using SpecialKHelper.SpecialKHandler.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpecialKHelper.SpecialKHandler.Application
{
    public class SpecialKServiceManager
    {
        public event EventHandler<SpecialKServiceStatusChangedEventArgs> SpecialKServiceStatusChanged;
        private string _customSpecialKInstallationPath = string.Empty;
        private const string _32BitsPrefix = "32";
        private const string _64BitsPrefix = "64";
        private const string _32BitsServiceProcessName = "SKIFsvc32";
        private const string _64BitsServiceProcessName = "SKIFsvc64";
        private const string _specialKExecutableName = "SKIF.exe";

        private const string _specialKRegistryPath = @"SOFTWARE\Kaldaien\Special K";
        private const int _startServiceMaxRetries = 15;
        private const int _startServiceSleepDurationMs = 100;
        private const int _stopServiceMaxRetries = 15;
        private const int _stopServiceSleepDurationMs = 100;
        private const int _backgroundServiceDelay = 15000;
        private SpecialKServiceStatus _service32BitsStatus;
        private SpecialKServiceStatus _service64BitsStatus;
        private readonly ILogger _logger;

        public SpecialKServiceStatus Service32BitsStatus => _service32BitsStatus;
        public SpecialKServiceStatus Service64BitsStatus => _service64BitsStatus;

        public SpecialKServiceManager(ILogger logger)
        {
            _service32BitsStatus = Is32BitsServiceRunning() ? SpecialKServiceStatus.Running : SpecialKServiceStatus.Stopped;
            _service64BitsStatus = Is64BitsServiceRunning() ? SpecialKServiceStatus.Running : SpecialKServiceStatus.Stopped;
            _logger = logger;
            StartBackgroundServiceStatusCheck();
        }

        private async void StartBackgroundServiceStatusCheck()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(_backgroundServiceDelay);
                    UpdateServiceStatus();
                }
            });
        }

        private void UpdateServiceStatus()
        {
            var is32BitsServiceRunning = false;
            var is64BitsServiceRunning = false;
            foreach (var process in Process.GetProcesses())
            {
                if (process.ProcessName == _32BitsServiceProcessName)
                {
                    is32BitsServiceRunning = true;
                }
                else if (process.ProcessName == _64BitsServiceProcessName)
                {
                    is64BitsServiceRunning = true;
                }

                if (is32BitsServiceRunning && is64BitsServiceRunning)
                {
                    break;
                }
            }

            var current32BitsStatus = is32BitsServiceRunning ? SpecialKServiceStatus.Running : SpecialKServiceStatus.Stopped;
            var current64BitsStatus = is64BitsServiceRunning ? SpecialKServiceStatus.Running : SpecialKServiceStatus.Stopped;
            if (current32BitsStatus != _service32BitsStatus)
            {
                OnServiceStatusChanged(current32BitsStatus, CpuArchitecture.X86);
            }

            if (current64BitsStatus != _service64BitsStatus)
            {
                OnServiceStatusChanged(current64BitsStatus, CpuArchitecture.X64);
            }
        }

        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        internal void OpenSpecialK()
        {
            try
            {
                var installDir = GetInstallDirectory();
                if (!installDir.IsNullOrEmpty())
                {
                    var exePath = Path.Combine(installDir, _specialKExecutableName);
                    if (FileSystem.FileExists(exePath))
                    {
                        ProcessStarter.StartProcess(exePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start Special K software");
            }
        }

        public bool Is32BitsServiceRunning()
        {
            return IsProcessRunning(_32BitsServiceProcessName);
        }

        public bool Is64BitsServiceRunning()
        {
            return IsProcessRunning(_64BitsServiceProcessName);
        }

        public void ResetSpecialKInstallDirectory()
        {
            if (!_customSpecialKInstallationPath.IsNullOrEmpty())
            {
                _customSpecialKInstallationPath = string.Empty;
                _logger.Info($"Special K installation path has been reset");
            }
        }

        internal bool SetSpecialKInstallDirectory(string customSpecialKPath)
        {
            if (customSpecialKPath.IsNullOrWhiteSpace() || !FileSystem.DirectoryExists(customSpecialKPath))
            {
                _logger.Info($"Failed to set Special K installation path: {customSpecialKPath ?? "null"}. Directory does not exist.");
                return false;
            }

            if (_customSpecialKInstallationPath != customSpecialKPath)
            {
                _customSpecialKInstallationPath = customSpecialKPath;
                _logger.Info($"Special K installation path successfully set to: {_customSpecialKInstallationPath}");
            }

            return true;
        }

        public string GetInstallDirectory()
        {
            if (!_customSpecialKInstallationPath.IsNullOrEmpty() && FileSystem.DirectoryExists(_customSpecialKInstallationPath))
            {
                return _customSpecialKInstallationPath;
            }
            
            using (var key = Registry.CurrentUser.OpenSubKey(_specialKRegistryPath))
            {
                if (key is null)
                {
                    throw new SpecialKPathNotFoundException($"Registry key not found: {_specialKRegistryPath}");
                }

                var pathValue = key.GetValue("Path");
                if (pathValue is null)
                {
                    throw new SpecialKPathNotFoundException("Path value not found in registry.");
                }

                var directory = pathValue.ToString();
                if (!FileSystem.DirectoryExists(directory))
                {
                    throw new SpecialKPathNotFoundException($"Special K directory {directory} in registry does not exist.");
                }

                return directory;
            }
        }

        public bool Start32BitsService(CancellationToken cancellationToken = default)
        {
            return StartService(CpuArchitecture.X86, _customSpecialKInstallationPath, cancellationToken);
        }

        public bool Start64BitsService(CancellationToken cancellationToken = default)
        {
            return StartService(CpuArchitecture.X64, _customSpecialKInstallationPath, cancellationToken);
        }

        private bool StartService(CpuArchitecture cpuArchitecture, string customSpecialKPath = null, CancellationToken cancellationToken = default)
        {
            var serviceAlreadyRunning = cpuArchitecture == CpuArchitecture.X86 
               ? Is32BitsServiceRunning()
               : Is64BitsServiceRunning();
            if (serviceAlreadyRunning)
            {
                return true;
            }

            var specialKInstallDir = GetSpecialKInstallDirectory(customSpecialKPath);
            return StartServiceInternal(specialKInstallDir, cpuArchitecture, cancellationToken);
        }

        private string GetSpecialKInstallDirectory(string customPath)
        {
            return !customPath.IsNullOrEmpty() ? customPath : GetInstallDirectory();
        }

        private bool StartServiceInternal(string skifPath, CpuArchitecture cpuArchitecture, CancellationToken cancellationToken)
        {
            ValidateServiceFiles(skifPath, cpuArchitecture);

            var architectureName = cpuArchitecture == CpuArchitecture.X86
                ? _32BitsPrefix
                : _64BitsPrefix;
            var servletExe = Path.Combine(skifPath, "Servlet", "SKIFsvc" + architectureName + ".exe");
            StartServiceProcess(servletExe);

            return WaitForProcessToStart(cpuArchitecture, cancellationToken);
        }

        private void ValidateServiceFiles(string skifPath, CpuArchitecture cpuArchitecture)
        {
            var architectureName = cpuArchitecture == CpuArchitecture.X86 ? _32BitsPrefix : _64BitsPrefix;
            ValidateFileExists(Path.Combine(skifPath, $"SpecialK{architectureName}.dll"), "service DLL");
            ValidateFileExists(Path.Combine(skifPath, "Servlet", $"SKIFsvc{architectureName}.exe"), "servlet executable");
        }

        private void ValidateFileExists(string filePath, string fileDescription)
        {
            if (!FileSystem.FileExists(filePath))
            {
                var exception = new SpecialKFileNotFoundException($"The {fileDescription} was not found: {filePath}");
                _logger.Error(exception, $"Service validation failed: {fileDescription} is missing.");
                throw exception;
            }
        }

        private void StartServiceProcess(string servletExe)
        {
            var info = new ProcessStartInfo(servletExe)
            {
                WorkingDirectory = Path.GetDirectoryName(servletExe),
                UseShellExecute = true,
                Arguments = "Start",
            };

            Process.Start(info);
        }

        private bool WaitForProcessToStart(CpuArchitecture cpuArchitecture, CancellationToken cancellationToken)
        {
            for (int i = 0; i < _startServiceMaxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Thread.Sleep(_startServiceSleepDurationMs);
                var isProcessStarted = cpuArchitecture == CpuArchitecture.X86 ? Is32BitsServiceRunning() : Is64BitsServiceRunning();
                if (isProcessStarted)
                {
                    OnServiceStatusChanged(SpecialKServiceStatus.Running, cpuArchitecture);
                    return true;
                }
            }

            _logger.Info($"Special K {cpuArchitecture} did not start after set time.");
            return false;
        }

        private void StopServiceProcess(string servletExe)
        {
            var info = new ProcessStartInfo(servletExe)
            {
                WorkingDirectory = Path.GetDirectoryName(servletExe),
                UseShellExecute = true,
                Arguments = "Stop",
            };

            Process.Start(info);
        }

        public bool Stop32BitsService(CancellationToken cancellationToken = default)
        {
            return StopService(CpuArchitecture.X86, _customSpecialKInstallationPath, cancellationToken);
        }

        public bool Stop64BitsService(CancellationToken cancellationToken = default)
        {
            return StopService(CpuArchitecture.X64, _customSpecialKInstallationPath, cancellationToken);
        }

        private bool StopService(CpuArchitecture cpuArchitecture, string customSpecialKPath, CancellationToken cancellationToken)
        {
            var serviceAlreadyRunning = cpuArchitecture == CpuArchitecture.X86
                ? Is32BitsServiceRunning()
                : Is64BitsServiceRunning();
            if (!serviceAlreadyRunning)
            {
                return true;
            }

            var specialKInstallDir = GetSpecialKInstallDirectory(customSpecialKPath);
            return StopServiceInternal(specialKInstallDir, cpuArchitecture, cancellationToken);
        }

        private bool StopServiceInternal(string skifPath, CpuArchitecture cpuArchitecture, CancellationToken cancellationToken)
        {
            var architectureName = cpuArchitecture == CpuArchitecture.X86
                ? _32BitsPrefix
                : _64BitsPrefix;
            var servletExe = Path.Combine(skifPath, "Servlet", "SKIFsvc" + architectureName + ".exe");
            StopServiceProcess(servletExe);
            return WaitForProcessToStop(cpuArchitecture, cancellationToken);
        }

        private bool WaitForProcessToStop(CpuArchitecture cpuArchitecture, CancellationToken cancellationToken)
        {
            for (int i = 0; i < _stopServiceMaxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Thread.Sleep(_stopServiceSleepDurationMs);
                var isProcessStarted = cpuArchitecture == CpuArchitecture.X86 ? Is32BitsServiceRunning() : Is64BitsServiceRunning();
                if (!isProcessStarted)
                {
                    OnServiceStatusChanged(SpecialKServiceStatus.Stopped, cpuArchitecture);
                    return true;
                }
            }

            _logger.Info($"Special K {cpuArchitecture} did not stop after set time.");
            return false;
        }

        private void OnServiceStatusChanged(SpecialKServiceStatus status, CpuArchitecture architecture)
        {
            var currentStatus = (architecture == CpuArchitecture.X86)
                ? _service32BitsStatus
                : _service64BitsStatus;

            if (architecture == CpuArchitecture.X86)
            {
                _service32BitsStatus = status;
            }
            else
            {
                _service64BitsStatus = status;
            }

            _logger.Info($"Special K {architecture} service status changed from {currentStatus} to {status}");
            SpecialKServiceStatusChanged?.Invoke(this, new SpecialKServiceStatusChangedEventArgs(status, architecture));
        }

    }
}