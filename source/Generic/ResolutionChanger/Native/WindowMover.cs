using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DisplayHelper.Native
{
    /// <summary>
    /// Moves a game's main window to a target display and makes it borderless,
    /// without changing the primary display.
    ///
    /// Uses a WinEvent hook so the window is moved the moment it is shown
    /// (instead of polling), plus a slow fallback scan for windows that were
    /// already visible or events that were missed. Stops on its own after a timeout.
    /// </summary>
    public sealed class WindowMover
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        // How long to wait for the game window to appear.
        private static readonly TimeSpan FindTimeout = TimeSpan.FromSeconds(120);
        // How long to keep enforcing the position after the window is first moved
        // (some games move/resize their window during startup).
        private static readonly TimeSpan EnforceDuration = TimeSpan.FromSeconds(45);
        private const uint FallbackScanIntervalMs = 1000;
        private const int MaxReapplies = 15;
        private const int MinWindowSize = 320;

        private readonly string installDirectory;
        private readonly string targetDisplayName;
        private readonly int initialProcessId;
        private readonly int playniteProcessId = Process.GetCurrentProcess().Id;

        // pid -> is a game process. Avoids re-querying every event.
        private readonly Dictionary<uint, bool> processCache = new Dictionary<uint, bool>();
        private readonly HashSet<IntPtr> movedWindows = new HashSet<IntPtr>();

        // Must be kept alive while hooks are installed.
        private WinEventDelegate winEventDelegate;
        private Thread thread;
        private uint threadId;
        private volatile bool stopRequested;

        private DateTime findDeadline;
        private DateTime? enforceDeadline;
        private int applies;

        private int knownProcessId;
        public int KnownProcessId
        {
            get => knownProcessId;
            set
            {
                knownProcessId = value;
                lock (processCache)
                {
                    processCache.Remove((uint)value);
                }
            }
        }

        public WindowMover(string installDirectory, string targetDisplayName, int processId = 0)
        {
            this.installDirectory = NormalizeDirectory(installDirectory);
            this.targetDisplayName = targetDisplayName;
            initialProcessId = processId;
        }

        public void Start()
        {
            thread = new Thread(ThreadMain)
            {
                IsBackground = true,
                Name = "DisplayHelper WindowMover"
            };
            thread.Start();
        }

        public void Stop()
        {
            stopRequested = true;
            if (threadId != 0)
            {
                PostThreadMessage(threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private void ThreadMain()
        {
            var hooks = new List<IntPtr>();
            var timerId = UIntPtr.Zero;
            try
            {
                threadId = GetCurrentThreadId();
                TrySetPerMonitorDpiAwareness();
                logger.Info($"Waiting for game window to move it to {targetDisplayName}. Install dir: \"{installDirectory}\"");

                findDeadline = DateTime.UtcNow + FindTimeout;
                winEventDelegate = OnWinEvent;
                hooks.Add(SetWinEventHook(EVENT_OBJECT_SHOW, EVENT_OBJECT_SHOW, IntPtr.Zero, winEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS));
                hooks.Add(SetWinEventHook(EVENT_OBJECT_LOCATIONCHANGE, EVENT_OBJECT_LOCATIONCHANGE, IntPtr.Zero, winEventDelegate, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS));
                timerId = SetTimer(IntPtr.Zero, UIntPtr.Zero, FallbackScanIntervalMs, IntPtr.Zero);

                // Window may already be visible before hooks were installed.
                FallbackScan();

                while (!stopRequested && GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
                {
                    if (msg.message == WM_TIMER && msg.hwnd == IntPtr.Zero)
                    {
                        OnTimer();
                        continue;
                    }

                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while moving game window");
            }
            finally
            {
                foreach (var hook in hooks)
                {
                    if (hook != IntPtr.Zero)
                    {
                        UnhookWinEvent(hook);
                    }
                }

                if (timerId != UIntPtr.Zero)
                {
                    KillTimer(IntPtr.Zero, timerId);
                }

                logger.Info($"Window mover finished. Moves applied: {applies}");
            }
        }

        private void Quit()
        {
            stopRequested = true;
            PostQuitMessage(0);
        }

        private void OnTimer()
        {
            if (stopRequested)
            {
                Quit();
                return;
            }

            var now = DateTime.UtcNow;
            if (enforceDeadline.HasValue)
            {
                if (now > enforceDeadline.Value || applies > MaxReapplies)
                {
                    Quit();
                    return;
                }
            }
            else if (now > findDeadline)
            {
                logger.Warn("Game window was not found. It was not moved.");
                Quit();
                return;
            }

            FallbackScan();
        }

        private void OnWinEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime)
        {
            try
            {
                if (stopRequested || hwnd == IntPtr.Zero || idObject != OBJID_WINDOW || idChild != 0)
                {
                    return;
                }

                // Location changes fire constantly system-wide; only care about windows we already moved.
                if (eventType == EVENT_OBJECT_LOCATIONCHANGE && !movedWindows.Contains(hwnd))
                {
                    return;
                }

                if (!IsCandidateWindow(hwnd))
                {
                    return;
                }

                TryMove(hwnd, eventType == EVENT_OBJECT_SHOW ? "shown" : "moved by game");
            }
            catch (Exception e)
            {
                logger.Error(e, "Error handling window event");
            }
        }

        private void FallbackScan()
        {
            var hwnd = FindGameWindow();
            if (hwnd != IntPtr.Zero)
            {
                TryMove(hwnd, "scan");
            }
        }

        private void TryMove(IntPtr hwnd, string reason)
        {
            if (applies > MaxReapplies)
            {
                return;
            }

            if (!TryGetTargetBounds(out var bounds))
            {
                logger.Warn($"Target display {targetDisplayName} not found");
                Quit();
                return;
            }

            if (IsWindowAtBounds(hwnd, bounds))
            {
                movedWindows.Add(hwnd);
                return;
            }

            MakeBorderlessAndMove(hwnd, bounds, reason);
            movedWindows.Add(hwnd);
            applies++;
            if (!enforceDeadline.HasValue)
            {
                enforceDeadline = DateTime.UtcNow + EnforceDuration;
            }
        }

        private bool IsCandidateWindow(IntPtr hWnd)
        {
            if (!IsWindowVisible(hWnd) || GetAncestor(hWnd, GA_ROOT) != hWnd)
            {
                return false;
            }

            if (GetWindow(hWnd, GW_OWNER) != IntPtr.Zero)
            {
                return false;
            }

            var exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt64();
            if ((exStyle & WS_EX_TOOLWINDOW) != 0)
            {
                return false;
            }

            if (!GetWindowRect(hWnd, out var rect) ||
                rect.Right - rect.Left < MinWindowSize ||
                rect.Bottom - rect.Top < MinWindowSize)
            {
                return false;
            }

            GetWindowThreadProcessId(hWnd, out var pid);
            return IsGameProcess(pid);
        }

        private IntPtr FindGameWindow()
        {
            IntPtr best = IntPtr.Zero;
            long bestArea = 0;
            EnumWindows((hWnd, lParam) =>
            {
                if (!IsCandidateWindow(hWnd) || !GetWindowRect(hWnd, out var rect))
                {
                    return true;
                }

                var area = (long)(rect.Right - rect.Left) * (rect.Bottom - rect.Top);
                if (area > bestArea)
                {
                    bestArea = area;
                    best = hWnd;
                }

                return true;
            }, IntPtr.Zero);

            return best;
        }

        private bool IsGameProcess(uint pid)
        {
            if (pid == 0 || pid <= 4 || pid == playniteProcessId)
            {
                return false;
            }

            if (pid == initialProcessId || pid == knownProcessId)
            {
                return true;
            }

            lock (processCache)
            {
                if (processCache.TryGetValue(pid, out var cached))
                {
                    return cached;
                }
            }

            var result = false;
            if (!string.IsNullOrEmpty(installDirectory))
            {
                var path = GetProcessPath((int)pid);
                if (!string.IsNullOrEmpty(path))
                {
                    result = IsPathInInstallDirectory(path);
                }
                else
                {
                    // Path can't be read (e.g. DRM-protected processes deny access),
                    // fall back to matching by executable name.
                    try
                    {
                        using (var process = Process.GetProcessById((int)pid))
                        {
                            result = GetInstallDirExeNames().Contains(process.ProcessName);
                        }
                    }
                    catch
                    {
                        result = false;
                    }
                }
            }

            lock (processCache)
            {
                processCache[pid] = result;
            }

            return result;
        }

        private bool IsPathInInstallDirectory(string path)
        {
            if (path.StartsWith(installDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Mapped network drives and subst drives: Windows reports the process path as
            // \\server\share\... or \Device\... instead of D:\..., so compare the part
            // after the drive letter, e.g. "\Games\Crash Bandicoot 4\".
            var tail = GetInstallDirectoryTail();
            return tail != null && path.IndexOf(tail, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string installDirectoryTail;
        private bool installDirectoryTailComputed;

        private string GetInstallDirectoryTail()
        {
            if (installDirectoryTailComputed)
            {
                return installDirectoryTail;
            }

            installDirectoryTailComputed = true;
            // "D:\Games\Crash Bandicoot 4\" -> "\Games\Crash Bandicoot 4\"
            // Require at least one folder below the drive root to avoid matching everything.
            if (installDirectory.Length > 3 && installDirectory[1] == ':')
            {
                var tail = installDirectory.Substring(2);
                if (tail.Trim('\\').Length > 0)
                {
                    installDirectoryTail = tail;
                }
            }

            return installDirectoryTail;
        }

        private HashSet<string> installDirExeNames;

        private HashSet<string> GetInstallDirExeNames()
        {
            if (installDirExeNames != null)
            {
                return installDirExeNames;
            }

            installDirExeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var pending = new Stack<KeyValuePair<string, int>>();
                pending.Push(new KeyValuePair<string, int>(installDirectory, 0));
                var visited = 0;
                while (pending.Count > 0 && visited < 3000)
                {
                    var current = pending.Pop();
                    visited++;
                    try
                    {
                        foreach (var file in Directory.EnumerateFiles(current.Key, "*.exe"))
                        {
                            installDirExeNames.Add(Path.GetFileNameWithoutExtension(file));
                        }

                        if (current.Value < 5)
                        {
                            foreach (var dir in Directory.EnumerateDirectories(current.Key))
                            {
                                pending.Push(new KeyValuePair<string, int>(dir, current.Value + 1));
                            }
                        }
                    }
                    catch
                    {
                        // Skip folders we can't read.
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error while listing game executables");
            }

            logger.Info($"Game executables found in install dir: {string.Join(", ", installDirExeNames)}");
            return installDirExeNames;
        }

        private bool TryGetTargetBounds(out RECT bounds)
        {
            bounds = new RECT();
            var devMode = DisplayUtilities.GetScreenDevMode(targetDisplayName);
            if (devMode.dmPelsWidth == 0 || devMode.dmPelsHeight == 0)
            {
                return false;
            }

            bounds.Left = devMode.dmPosition.x;
            bounds.Top = devMode.dmPosition.y;
            bounds.Right = bounds.Left + devMode.dmPelsWidth;
            bounds.Bottom = bounds.Top + devMode.dmPelsHeight;
            return true;
        }

        private static bool IsWindowAtBounds(IntPtr hwnd, RECT bounds)
        {
            if (!GetWindowRect(hwnd, out var rect))
            {
                return true;
            }

            var style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64();
            var hasFrame = (style & (WS_CAPTION | WS_THICKFRAME)) != 0;
            return !hasFrame &&
                   rect.Left == bounds.Left && rect.Top == bounds.Top &&
                   rect.Right == bounds.Right && rect.Bottom == bounds.Bottom;
        }

        private static void MakeBorderlessAndMove(IntPtr hwnd, RECT bounds, string reason)
        {
            // Un-maximize first, otherwise Windows may snap it back to the old monitor.
            if (IsZoomed(hwnd) || IsIconic(hwnd))
            {
                ShowWindow(hwnd, SW_RESTORE);
            }

            var style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64();
            var newStyle = style & ~(WS_CAPTION | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
            if (newStyle != style)
            {
                SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(newStyle));
            }

            var exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE).ToInt64();
            var newExStyle = exStyle & ~(WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE);
            if (newExStyle != exStyle)
            {
                SetWindowLongPtr(hwnd, GWL_EXSTYLE, new IntPtr(newExStyle));
            }

            var ok = SetWindowPos(
                hwnd,
                HWND_TOP,
                bounds.Left,
                bounds.Top,
                bounds.Right - bounds.Left,
                bounds.Bottom - bounds.Top,
                SWP_FRAMECHANGED | SWP_SHOWWINDOW | SWP_NOOWNERZORDER);

            logger.Info($"Moved game window {hwnd} ({reason}) to {bounds.Left},{bounds.Top} {bounds.Right - bounds.Left}x{bounds.Bottom - bounds.Top}. Success: {ok}");
        }

        private static string NormalizeDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                return null;
            }

            try
            {
                var full = Path.GetFullPath(directory.Trim());
                return full.EndsWith("\\") ? full : full + "\\";
            }
            catch
            {
                return null;
            }
        }

        private static string GetProcessPath(int processId)
        {
            var handle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (handle == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                var builder = new StringBuilder(1024);
                var size = builder.Capacity;
                return QueryFullProcessImageName(handle, 0, builder, ref size) ? builder.ToString() : null;
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private static void TrySetPerMonitorDpiAwareness()
        {
            try
            {
                // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 so coordinates are physical pixels.
                SetThreadDpiAwarenessContext(new IntPtr(-4));
            }
            catch
            {
                // Not available before Windows 10 1607.
            }
        }

        #region Native

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;
        private const uint GW_OWNER = 4;
        private const uint GA_ROOT = 2;
        private const long WS_CAPTION = 0x00C00000L;
        private const long WS_THICKFRAME = 0x00040000L;
        private const long WS_SYSMENU = 0x00080000L;
        private const long WS_MINIMIZEBOX = 0x00020000L;
        private const long WS_MAXIMIZEBOX = 0x00010000L;
        private const long WS_EX_DLGMODALFRAME = 0x00000001L;
        private const long WS_EX_CLIENTEDGE = 0x00000200L;
        private const long WS_EX_STATICEDGE = 0x00020000L;
        private const long WS_EX_WINDOWEDGE = 0x00000100L;
        private const long WS_EX_TOOLWINDOW = 0x00000080L;
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOOWNERZORDER = 0x0200;
        private const int SW_RESTORE = 9;
        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        private static readonly IntPtr HWND_TOP = IntPtr.Zero;

        private const uint EVENT_OBJECT_SHOW = 0x8002;
        private const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;
        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        private const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
        private const int OBJID_WINDOW = 0;
        private const uint WM_QUIT = 0x0012;
        private const uint WM_TIMER = 0x0113;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool PostThreadMessage(uint threadId, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void PostQuitMessage(int exitCode);

        [DllImport("user32.dll")]
        private static extern UIntPtr SetTimer(IntPtr hWnd, UIntPtr nIDEvent, uint uElapse, IntPtr lpTimerFunc);

        [DllImport("user32.dll")]
        private static extern bool KillTimer(IntPtr hWnd, UIntPtr uIDEvent);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsZoomed(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr value);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int value);

        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, int flags, StringBuilder exeName, ref int size);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));
        }

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr value)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, value) : new IntPtr(SetWindowLong32(hWnd, nIndex, value.ToInt32()));
        }

        #endregion
    }
}
