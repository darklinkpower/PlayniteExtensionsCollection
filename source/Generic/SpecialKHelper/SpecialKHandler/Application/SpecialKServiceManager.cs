using Microsoft.Win32;
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
        private const string _32BitsPrefix = "32";
        private const string _64BitsPrefix = "64";
        private const string _32BitsServiceProcessName = "SKIFsvc32";
        private const string _64BitsServiceProcessName = "SKIFsvc64";
        private const string _specialKRegistryPath = @"SOFTWARE\Kaldaien\Special K";
        private const int _startServiceMaxRetries = 12;
        private const int _startServiceSleepDurationMs = 100;
        private SpecialKServiceStatus _service32BitsStatus;
        private SpecialKServiceStatus _service64BitsStatus;
        public SpecialKServiceStatus Service32BitsStatus => _service32BitsStatus;
        public SpecialKServiceStatus Service64BitsStatus => _service64BitsStatus;

        public SpecialKServiceManager()
        {
            _service32BitsStatus = Is32BitsServiceRunning() ? SpecialKServiceStatus.Running : SpecialKServiceStatus.Stopped;
            _service64BitsStatus = Is64BitsServiceRunning() ? SpecialKServiceStatus.Running : SpecialKServiceStatus.Stopped;
        }

        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        public bool Is32BitsServiceRunning()
        {
            return IsProcessRunning(_32BitsServiceProcessName);
        }

        public bool Is64BitsServiceRunning()
        {
            return IsProcessRunning(_64BitsServiceProcessName);
        }

        private string GetInstallDirectory()
        {
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

        public bool Start32BitsService(string customSpecialKPath = null, CancellationToken cancellationToken = default)
        {
            return StartService(CpuArchitecture.X86, customSpecialKPath, cancellationToken);
        }

        public bool Start64BitsService(string customSpecialKPath = null, CancellationToken cancellationToken = default)
        {
            return StartService(CpuArchitecture.X64, customSpecialKPath, cancellationToken);
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
            var architectureName = cpuArchitecture == CpuArchitecture.X86
                ? _32BitsPrefix
                : _64BitsPrefix;
            var serviceDllPath = Path.Combine(skifPath, $"SpecialK{architectureName}.dll");
            if (!FileSystem.FileExists(serviceDllPath))
            {
                throw new SpecialKFileNotFoundException($"The service DLL file was not found: {serviceDllPath}");
            }

            var servletExe = Path.Combine(skifPath, "Servlet", $"SKIFsvc{architectureName}.exe");
            if (!FileSystem.FileExists(servletExe))
            {
                throw new SpecialKFileNotFoundException($"The servlet executable was not found: {servletExe}");
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
                    OnServiceStatusChangedEventArgs(SpecialKServiceStatus.Running, cpuArchitecture);
                    return true;
                }
            }

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

        public bool Stop32BitsService(string customSpecialKPath = null, CancellationToken cancellationToken = default)
        {
            return StopService(CpuArchitecture.X86, customSpecialKPath, cancellationToken);
        }

        public bool Stop64BitsService(string customSpecialKPath = null, CancellationToken cancellationToken = default)
        {
            return StopService(CpuArchitecture.X64, customSpecialKPath, cancellationToken);
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
            for (int i = 0; i < _startServiceMaxRetries; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Thread.Sleep(_startServiceSleepDurationMs);
                var isProcessStarted = cpuArchitecture == CpuArchitecture.X86 ? Is32BitsServiceRunning() : Is64BitsServiceRunning();
                if (!isProcessStarted)
                {
                    OnServiceStatusChangedEventArgs(SpecialKServiceStatus.Stopped, cpuArchitecture);
                    return true;
                }
            }

            return false;
        }

        private void OnServiceStatusChangedEventArgs(SpecialKServiceStatus status, CpuArchitecture architecture)
        {
            if (architecture == CpuArchitecture.X64)
            {
                _service32BitsStatus = status;
            }
            else
            {
                _service64BitsStatus = status;
            }

            SpecialKServiceStatusChanged?.Invoke(this, new SpecialKServiceStatusChangedEventArgs(status, architecture));
        }


    }
}