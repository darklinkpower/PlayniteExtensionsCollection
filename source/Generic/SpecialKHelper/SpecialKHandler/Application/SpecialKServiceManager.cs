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
            _logger = logger;
            _service32BitsStatus = Is32BitsServiceRunning() ? SpecialKServiceStatus.Running : SpecialKServiceStatus.Stopped;
            _service64BitsStatus = Is64BitsServiceRunning() ? SpecialKServiceStatus.Running : SpecialKServiceStatus.Stopped;

            _logger.Info($"Initialized SpecialKServiceManager. 32-bit: {_service32BitsStatus}, 64-bit: {_service64BitsStatus}");

            StartBackgroundServiceStatusCheck();
        }

        private async void StartBackgroundServiceStatusCheck()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(_backgroundServiceDelay);
                        UpdateServiceStatus();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error during background service status check.");
                    }
                }
            });
        }

        private void UpdateServiceStatus()
        {
            try
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
                    _logger.Info($"Detected change in 32-bit service: {_service32BitsStatus} -> {current32BitsStatus}");
                    OnServiceStatusChanged(current32BitsStatus, CpuArchitecture.X86);
                }

                if (current64BitsStatus != _service64BitsStatus)
                {
                    _logger.Info($"Detected change in 64-bit service: {_service64BitsStatus} -> {current64BitsStatus}");
                    OnServiceStatusChanged(current64BitsStatus, CpuArchitecture.X64);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update Special K service statuses.");
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
                    _logger.Warn("Special K installation directory is null or empty.");
                    return;
                }

                var exePath = Path.Combine(installDir, _specialKExecutableName);
                if (FileSystem.FileExists(exePath))
                {
                    _logger.Info($"Launching Special K at {exePath}");
                    ProcessStarter.StartProcess(exePath);
                }
                else
                {
                    _logger.Warn($"Special K executable not found: {exePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start Special K software.");
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
                _logger.Info("Special K installation path has been reset.");
            }
        }

        internal bool SetSpecialKInstallDirectory(string customSpecialKPath)
        {
            if (customSpecialKPath.IsNullOrWhiteSpace() || !FileSystem.DirectoryExists(customSpecialKPath))
            {
                _logger.Warn($"Failed to set Special K installation path: '{customSpecialKPath ?? "null"}'. Directory does not exist.");
                return false;
            }

            if (_customSpecialKInstallationPath != customSpecialKPath)
            {
                _customSpecialKInstallationPath = customSpecialKPath;
                _logger.Info($"Special K installation path set to: {_customSpecialKInstallationPath}");
            }

            return true;
        }

        public string GetInstallDirectory()
        {
            if (!_customSpecialKInstallationPath.IsNullOrEmpty() && FileSystem.DirectoryExists(_customSpecialKInstallationPath))
            {
                _logger.Info($"Using custom Special K installation path: {_customSpecialKInstallationPath}");
                return _customSpecialKInstallationPath;
            }

            using (var key = Registry.CurrentUser.OpenSubKey(_specialKRegistryPath))
            {
                if (key is null)
                {
                    _logger.Error($"Registry key not found: {_specialKRegistryPath}");
                    throw new SpecialKPathNotFoundException($"Registry key not found: {_specialKRegistryPath}");
                }

                var pathValue = key.GetValue("Path");
                if (pathValue is null)
                {
                    _logger.Error("Path value not found in registry.");
                    throw new SpecialKPathNotFoundException("Path value not found in registry.");
                }

                var directory = pathValue.ToString();
                if (!FileSystem.DirectoryExists(directory))
                {
                    _logger.Error($"Special K directory in registry does not exist: {directory}");
                    throw new SpecialKPathNotFoundException($"Special K directory {directory} in registry does not exist.");
                }

                _logger.Info($"Resolved Special K installation directory from registry: {directory}");
                return directory;
            }
        }

        public bool Start32BitsService(CancellationToken cancellationToken = default)
        {
            _logger.Info("Attempting to start 64-bit Special K service...");
            return StartService(CpuArchitecture.X86, _customSpecialKInstallationPath, cancellationToken);
        }

        public bool Start64BitsService(CancellationToken cancellationToken = default)
        {
            _logger.Info("Attempting to start 64-bit Special K service...");
            return StartService(CpuArchitecture.X64, _customSpecialKInstallationPath, cancellationToken);
        }

        private bool StartService(CpuArchitecture cpuArchitecture, string customSpecialKPath = null, CancellationToken cancellationToken = default)
        {
            var serviceAlreadyRunning = cpuArchitecture == CpuArchitecture.X86 
               ? Is32BitsServiceRunning()
               : Is64BitsServiceRunning();
            if (serviceAlreadyRunning)
            {
                _logger.Info($"Special K {cpuArchitecture} service is already running, no need to start.");
                return true;
            }

            var specialKInstallDir = GetSpecialKInstallDirectory(customSpecialKPath);
            _logger.Info($"Resolved Special K install directory for starting {cpuArchitecture} service: {specialKInstallDir}");
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
            _logger.Info($"Starting Special K {cpuArchitecture} service using executable: {servletExe}");
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

            _logger.Info($"Validated {fileDescription} exists at {filePath}");
        }

        private void StartServiceProcess(string servletExe)
        {
            try
            {
                _logger.Info($"Startinng Special K service using: {servletExe}");
                var info = new ProcessStartInfo(servletExe)
                {
                    WorkingDirectory = Path.GetDirectoryName(servletExe),
                    UseShellExecute = true,
                    Arguments = "Start",
                };

                Process.Start(info);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to start start process for Special K service: {servletExe}");
            }
        }

        private bool WaitForProcessToStart(CpuArchitecture cpuArchitecture, CancellationToken cancellationToken)
        {
            _logger.Info($"Waiting for Special K {cpuArchitecture} service to start...");
            for (int i = 0; i < _startServiceMaxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(_startServiceSleepDurationMs);

                var isStarted = cpuArchitecture == CpuArchitecture.X86 ? Is32BitsServiceRunning() : Is64BitsServiceRunning();
                if (isStarted)
                {
                    _logger.Info($"Special K {cpuArchitecture} service started successfully.");
                    OnServiceStatusChanged(SpecialKServiceStatus.Running, cpuArchitecture);
                    return true;
                }
            }

            _logger.Warn($"Special K {cpuArchitecture} service did not start after {_startServiceMaxRetries * _startServiceSleepDurationMs}ms.");
            return false;
        }

        private void StopServiceProcess(string servletExe)
        {
            try
            {
                _logger.Info($"Stopping Special K service using: {servletExe}");
                var info = new ProcessStartInfo(servletExe)
                {
                    WorkingDirectory = Path.GetDirectoryName(servletExe),
                    UseShellExecute = true,
                    Arguments = "Stop",
                };

                Process.Start(info);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to start stop process for Special K service: {servletExe}");
            }
        }

        public bool Stop32BitsService(CancellationToken cancellationToken = default)
        {
            _logger.Info("Attempting to stop 32-bit Special K service...");
            return StopService(CpuArchitecture.X86, _customSpecialKInstallationPath, cancellationToken);
        }

        public bool Stop64BitsService(CancellationToken cancellationToken = default)
        {
            _logger.Info("Attempting to stop 64-bit Special K service...");
            return StopService(CpuArchitecture.X64, _customSpecialKInstallationPath, cancellationToken);
        }

        private bool StopService(CpuArchitecture cpuArchitecture, string customSpecialKPath, CancellationToken cancellationToken)
        {
            var serviceAlreadyRunning = cpuArchitecture == CpuArchitecture.X86
                ? Is32BitsServiceRunning()
                : Is64BitsServiceRunning();

            if (!serviceAlreadyRunning)
            {
                _logger.Info($"Special K {cpuArchitecture} service is not running, no need to stop.");
                return true;
            }

            var specialKInstallDir = GetSpecialKInstallDirectory(customSpecialKPath);
            _logger.Info($"Resolved Special K install directory for stopping {cpuArchitecture} service: {specialKInstallDir}");

            return StopServiceInternal(specialKInstallDir, cpuArchitecture, cancellationToken);
        }

        private bool StopServiceInternal(string skifPath, CpuArchitecture cpuArchitecture, CancellationToken cancellationToken)
        {
            var architectureName = cpuArchitecture == CpuArchitecture.X86 ? _32BitsPrefix : _64BitsPrefix;
            var servletExe = Path.Combine(skifPath, "Servlet", "SKIFsvc" + architectureName + ".exe");

            _logger.Info($"Stopping Special K {cpuArchitecture} service using executable: {servletExe}");
            StopServiceProcess(servletExe);

            var stopped = WaitForProcessToStop(cpuArchitecture, cancellationToken);
            _logger.Info(stopped
                ? $"Special K {cpuArchitecture} service stopped successfully."
                : $"Special K {cpuArchitecture} service failed to stop in the expected time.");

            return stopped;
        }

        private bool WaitForProcessToStop(CpuArchitecture cpuArchitecture, CancellationToken cancellationToken)
        {
            _logger.Info($"Waiting for Special K {cpuArchitecture} service to stop...");

            for (int i = 0; i < _stopServiceMaxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Thread.Sleep(_stopServiceSleepDurationMs);

                var isRunning = cpuArchitecture == CpuArchitecture.X86 ? Is32BitsServiceRunning() : Is64BitsServiceRunning();
                if (!isRunning)
                {
                    _logger.Info($"Special K {cpuArchitecture} service stopped successfully.");
                    OnServiceStatusChanged(SpecialKServiceStatus.Stopped, cpuArchitecture);
                    return true;
                }
            }

            _logger.Warn($"Special K {cpuArchitecture} service did not stop after {_stopServiceMaxRetries * _stopServiceSleepDurationMs}ms.");
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