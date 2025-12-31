using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace WinAppSmall;

public class WindowMonitor : IDisposable
{
    private HashSet<string> monitoredPrograms;
    private System.Windows.Forms.Timer timer;
    private Dictionary<IntPtr, uint> monitoredWindows = new Dictionary<IntPtr, uint>();

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

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
        int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

    private const uint EVENT_OBJECT_DESTROY = 0x8001;
    private const uint EVENT_OBJECT_HIDE = 0x8003;
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
    private const int SW_MINIMIZE = 6;
    private const int SW_SHOW = 5;
    private const uint GW_OWNER = 4;

    private IntPtr hookHandleDestroy = IntPtr.Zero;
    private IntPtr hookHandleHide = IntPtr.Zero;
    private WinEventDelegate? hookDelegateDestroy;
    private WinEventDelegate? hookDelegateHide;

    public event EventHandler<string>? StatusChanged;

    public WindowMonitor(HashSet<string> programs)
    {
        monitoredPrograms = programs;
        
        // Set up hooks
        hookDelegateDestroy = new WinEventDelegate(WinEventProcDestroy);
        hookDelegateHide = new WinEventDelegate(WinEventProcHide);
        
        hookHandleDestroy = SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_DESTROY,
            IntPtr.Zero, hookDelegateDestroy, 0, 0, WINEVENT_OUTOFCONTEXT);
        
        hookHandleHide = SetWinEventHook(EVENT_OBJECT_HIDE, EVENT_OBJECT_HIDE,
            IntPtr.Zero, hookDelegateHide, 0, 0, WINEVENT_OUTOFCONTEXT);

        // Timer to track windows
        timer = new System.Windows.Forms.Timer();
        timer.Interval = 1000;
        timer.Tick += Timer_Tick;
        timer.Start();
        
        UpdateMonitoredWindows();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateMonitoredWindows();
    }

    private void UpdateMonitoredWindows()
    {
        var newWindows = new Dictionary<IntPtr, uint>();

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
                            newWindows[process.MainWindowHandle] = (uint)process.Id;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        monitoredWindows = newWindows;
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

    private void HandleWindowEvent(IntPtr hwnd, string eventType)
    {
        if (!IsWindow(hwnd))
            return;

        // Check if this window belongs to a monitored process
        GetWindowThreadProcessId(hwnd, out uint processId);

        foreach (var kvp in monitoredWindows)
        {
            if (kvp.Value == processId || kvp.Key == hwnd)
            {
                try
                {
                    var process = Process.GetProcessById((int)kvp.Value);
                    var programName = process.ProcessName + ".exe";

                    if (monitoredPrograms.Contains(programName))
                    {
                        // Instead of closing, minimize the window
                        if (IsWindowVisible(hwnd))
                        {
                            ShowWindow(hwnd, SW_MINIMIZE);
                            StatusChanged?.Invoke(this, $"已攔截 {programName} 的關閉操作，已最小化");
                        }
                    }
                }
                catch { }
                
                break;
            }
        }
    }

    public int GetMonitoredWindowCount()
    {
        return monitoredWindows.Count;
    }

    public void Dispose()
    {
        timer?.Stop();
        timer?.Dispose();
        
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
    }
}
