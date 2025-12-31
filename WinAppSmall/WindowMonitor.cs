using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace WinAppSmall;

public class WindowMonitor : IDisposable
{
    private HashSet<string> monitoredPrograms;
    private System.Windows.Forms.Timer timer;
    private Dictionary<IntPtr, WindowInfo> monitoredWindows = new Dictionary<IntPtr, WindowInfo>();
    private readonly object lockObject = new object();

    private class WindowInfo
    {
        public uint ProcessId { get; set; }
        public string ProcessName { get; set; } = "";
        public bool WasVisible { get; set; }
    }

    // Windows API declarations
    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    private const uint EVENT_OBJECT_DESTROY = 0x8001;
    private const uint EVENT_OBJECT_HIDE = 0x8003;
    private const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
    private const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE = 9;
    private const int SW_SHOW = 5;
    private const int GWL_STYLE = -16;
    private const long WS_VISIBLE = 0x10000000L;

    private IntPtr hookHandleDestroy = IntPtr.Zero;
    private IntPtr hookHandleHide = IntPtr.Zero;
    private IntPtr hookHandleMinimizeStart = IntPtr.Zero;
    private WinEventDelegate? hookDelegateDestroy;
    private WinEventDelegate? hookDelegateHide;
    private WinEventDelegate? hookDelegateMinimizeStart;

    public event EventHandler<string>? StatusChanged;

    public WindowMonitor(HashSet<string> programs)
    {
        monitoredPrograms = programs;
        
        // Set up hooks for multiple events
        hookDelegateDestroy = new WinEventDelegate(WinEventProcDestroy);
        hookDelegateHide = new WinEventDelegate(WinEventProcHide);
        hookDelegateMinimizeStart = new WinEventDelegate(WinEventProcMinimizeStart);
        
        // Hook destroy event (when window is being closed)
        hookHandleDestroy = SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_DESTROY,
            IntPtr.Zero, hookDelegateDestroy, 0, 0, WINEVENT_OUTOFCONTEXT);
        
        // Hook hide event (when window is being hidden)
        hookHandleHide = SetWinEventHook(EVENT_OBJECT_HIDE, EVENT_OBJECT_HIDE,
            IntPtr.Zero, hookDelegateHide, 0, 0, WINEVENT_OUTOFCONTEXT);

        // Hook minimize start event
        hookHandleMinimizeStart = SetWinEventHook(EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZESTART,
            IntPtr.Zero, hookDelegateMinimizeStart, 0, 0, WINEVENT_OUTOFCONTEXT);

        // Timer to track windows and detect when they're about to close
        timer = new System.Windows.Forms.Timer();
        timer.Interval = 1000; // Check every 1 second for better performance
        timer.Tick += Timer_Tick;
        timer.Start();
        
        UpdateMonitoredWindows();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateMonitoredWindows();
        CheckForClosedWindows();
    }

    private void UpdateMonitoredWindows()
    {
        var currentWindows = new Dictionary<IntPtr, WindowInfo>();

        foreach (var programName in monitoredPrograms)
        {
            try
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(programName));
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.MainWindowHandle != IntPtr.Zero && IsWindow(process.MainWindowHandle))
                        {
                            var info = new WindowInfo
                            {
                                ProcessId = (uint)process.Id,
                                ProcessName = programName,
                                WasVisible = IsWindowVisible(process.MainWindowHandle)
                            };
                            currentWindows[process.MainWindowHandle] = info;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        lock (lockObject)
        {
            monitoredWindows = currentWindows;
        }
    }

    private void CheckForClosedWindows()
    {
        // Check if any monitored windows have become invisible or closed
        var windowsToRestore = new List<IntPtr>();
        Dictionary<IntPtr, WindowInfo> currentMonitoredWindows;

        lock (lockObject)
        {
            currentMonitoredWindows = new Dictionary<IntPtr, WindowInfo>(monitoredWindows);
        }

        foreach (var kvp in currentMonitoredWindows)
        {
            var hwnd = kvp.Key;
            var info = kvp.Value;

            if (!IsWindow(hwnd))
            {
                // Window no longer exists - try to find new main window for this process
                try
                {
                    var process = Process.GetProcessById((int)info.ProcessId);
                    if (process != null && !process.HasExited)
                    {
                        // Process is still alive but window was closed
                        // This might mean user tried to close it
                        StatusChanged?.Invoke(this, $"偵測到 {info.ProcessName} 視窗關閉嘗試");
                    }
                }
                catch { }
            }
            else if (info.WasVisible && !IsWindowVisible(hwnd) && !IsIconic(hwnd))
            {
                // Window was visible but now is hidden (not minimized)
                // This might be a close attempt - show it minimized instead
                windowsToRestore.Add(hwnd);
            }
        }

        foreach (var hwnd in windowsToRestore)
        {
            if (IsWindow(hwnd))
            {
                ShowWindow(hwnd, SW_MINIMIZE);
                
                lock (lockObject)
                {
                    if (monitoredWindows.ContainsKey(hwnd))
                    {
                        var programName = monitoredWindows[hwnd].ProcessName;
                        StatusChanged?.Invoke(this, $"已攔截 {programName} 的關閉操作，已最小化");
                    }
                }
            }
        }
    }

    private void WinEventProcDestroy(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        // When a window is about to be destroyed
        if (idObject == 0 && idChild == 0) // OBJID_WINDOW
        {
            HandleWindowEvent(hwnd, "destroy");
        }
    }

    private void WinEventProcHide(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        // When a window is hidden
        if (idObject == 0 && idChild == 0) // OBJID_WINDOW
        {
            HandleWindowEvent(hwnd, "hide");
        }
    }

    private void WinEventProcMinimizeStart(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        // When a window starts to minimize - this is OK, user intended to minimize
        // We don't need to do anything special here
    }

    private void HandleWindowEvent(IntPtr hwnd, string eventType)
    {
        if (!IsWindow(hwnd))
            return;

        // Check if this window belongs to a monitored process
        GetWindowThreadProcessId(hwnd, out uint processId);

        // Check if it's in our monitored windows or if the process is monitored
        bool isMonitored = false;
        string programName = "";

        lock (lockObject)
        {
            if (monitoredWindows.ContainsKey(hwnd))
            {
                isMonitored = true;
                programName = monitoredWindows[hwnd].ProcessName;
            }
            else
            {
                // Check by process ID
                foreach (var kvp in monitoredWindows)
                {
                    if (kvp.Value.ProcessId == processId)
                    {
                        isMonitored = true;
                        programName = kvp.Value.ProcessName;
                        break;
                    }
                }
            }
        }

        if (isMonitored && !string.IsNullOrEmpty(programName))
        {
            try
            {
                // Check if the process is still running
                var process = Process.GetProcessById((int)processId);
                if (process != null && !process.HasExited)
                {
                    // Check if window is being hidden/destroyed but not minimized
                    if (eventType == "hide" && !IsIconic(hwnd) && IsWindow(hwnd))
                    {
                        // Window is being hidden but not minimized - might be a close attempt
                        // Minimize it instead
                        ShowWindow(hwnd, SW_MINIMIZE);
                        StatusChanged?.Invoke(this, $"已攔截 {programName} 的關閉操作，已最小化");
                    }
                    else if (eventType == "destroy")
                    {
                        // Window is being destroyed
                        // Try to show the main window of the process as minimized if process still exists
                        var newHandle = process.MainWindowHandle;
                        if (newHandle != IntPtr.Zero && newHandle != hwnd && IsWindow(newHandle))
                        {
                            ShowWindow(newHandle, SW_MINIMIZE);
                            StatusChanged?.Invoke(this, $"已攔截 {programName} 的關閉操作，已最小化");
                        }
                    }
                }
            }
            catch { }
        }
    }

    public int GetMonitoredWindowCount()
    {
        lock (lockObject)
        {
            return monitoredWindows.Count;
        }
    }

    public void UpdateMonitoredPrograms(HashSet<string> programs)
    {
        monitoredPrograms = programs;
        UpdateMonitoredWindows();
    }

    public void Dispose()
    {
        timer?.Stop();
        timer?.Dispose();
        
        lock (lockObject)
        {
            if (hookHandleDestroy != IntPtr.Zero)
            {
                UnhookWinEvent(hookHandleDestroy);
                hookHandleDestroy = IntPtr.Zero;
            }
            
            if (hookHandleHide != IntPtr.Zero)
            {
                UnhookWinEvent(hookHandleHide);
                hookHandleHide = IntPtr.Zero;
            }

            if (hookHandleMinimizeStart != IntPtr.Zero)
            {
                UnhookWinEvent(hookHandleMinimizeStart);
                hookHandleMinimizeStart = IntPtr.Zero;
            }
        }
    }
}
