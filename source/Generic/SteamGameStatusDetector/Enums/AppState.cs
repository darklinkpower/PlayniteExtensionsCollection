using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamGameStatusDetector.Enums
{
    // https://github.com/SteamDatabase/SteamTracking/blob/master/Structs/EAppState.h
    public enum AppState
    {
        Invalid = 0,
        Uninstalled = 1,
        UpdateRequired = 2,
        FullyInstalled = 4,
        UpdateQueued = 8,
        UpdateOptional = 16,
        FilesMissing = 32,
        SharedOnly = 64,
        FilesCorrupt = 128,
        UpdateRunning = 256,
        UpdatePaused = 512,
        UpdateStarted = 1024,
        Uninstalling = 2048,
        BackupRunning = 4096,
        AppRunning = 8192,
        ComponentInUse = 16384,
        MovingFolder = 32768,
        PrefetchingInfo = 131072
    }
}