using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ObsKit.NET.Platform.Windows.Interop;

namespace ObsKit.NET.Platform.Windows;

/// <summary>
/// Windows implementation of platform services.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsPlatform : IPlatformServices
{
    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();
        int index = 0;

        User32.MonitorEnumProc callback = (hMonitor, hdcMonitor, lprcMonitor, dwData) =>
        {
            var info = new User32.MONITORINFOEX();
            info.cbSize = Marshal.SizeOf<User32.MONITORINFOEX>();

            nint infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<User32.MONITORINFOEX>());
            try
            {
                Marshal.StructureToPtr(info, infoPtr, false);
                if (User32.GetMonitorInfo(hMonitor, infoPtr) != 0)
                {
                    info = Marshal.PtrToStructure<User32.MONITORINFOEX>(infoPtr);
                    var deviceName = info.GetDeviceName();

                    monitors.Add(new MonitorInfo
                    {
                        Index = index++,
                        Handle = hMonitor,
                        DeviceName = deviceName,
                        Name = deviceName,
                        X = info.rcMonitor.Left,
                        Y = info.rcMonitor.Top,
                        Width = info.rcMonitor.Width,
                        Height = info.rcMonitor.Height,
                        IsPrimary = (info.dwFlags & User32.MONITORINFOEX.MONITORINFOF_PRIMARY) != 0,
                        RefreshRate = 60 // Default, could be retrieved via EnumDisplaySettings
                    });
                }
            }
            finally
            {
                Marshal.FreeHGlobal(infoPtr);
            }
            return 1; // TRUE = continue enumeration
        };

        User32.EnumDisplayMonitors(0, 0, callback, 0);
        GC.KeepAlive(callback);
        return monitors;
    }

    public MonitorInfo? GetPrimaryMonitor()
    {
        return GetMonitors().FirstOrDefault(m => m.IsPrimary);
    }

    public unsafe IReadOnlyList<WindowInfo> GetWindows()
    {
        var windows = new List<WindowInfo>();
        var shellWindow = User32.GetShellWindow();
        var desktopWindow = User32.GetDesktopWindow();

        User32.EnumWindowsProc callback = (hWnd, lParam) =>
        {
            // Skip shell and desktop windows
            if (hWnd == shellWindow || hWnd == desktopWindow)
                return 1;

            // Check if window is visible
            if (User32.IsWindowVisible(hWnd) == 0)
                return 1;

            // Check window styles
            var style = User32.GetWindowLong(hWnd, User32.GWL_STYLE);
            var exStyle = User32.GetWindowLong(hWnd, User32.GWL_EXSTYLE);

            // Skip tool windows that aren't app windows
            if ((exStyle & User32.WS_EX_TOOLWINDOW) != 0 && (exStyle & User32.WS_EX_APPWINDOW) == 0)
                return 1;

            // Get window title
            int titleLength = User32.GetWindowTextLength(hWnd);
            if (titleLength == 0)
                return 1;

            var title = GetWindowTitle(hWnd, titleLength);
            var className = GetWindowClassName(hWnd);

            // Get process info
            User32.GetWindowThreadProcessId(hWnd, out uint processId);
            var processName = GetProcessName(processId);
            var executablePath = GetProcessPath(processId);

            // Get window rect
            User32.GetWindowRect(hWnd, out var rect);

            windows.Add(new WindowInfo
            {
                Handle = hWnd,
                Title = title,
                ClassName = className,
                ProcessId = (int)processId,
                ProcessName = processName,
                ExecutablePath = executablePath,
                IsVisible = true,
                Width = rect.Width,
                Height = rect.Height
            });

            return 1; // TRUE = continue enumeration
        };

        User32.EnumWindows(callback, 0);
        GC.KeepAlive(callback); // Prevent delegate from being collected during P/Invoke
        return windows;
    }

    public WindowInfo? GetWindow(nint handle)
    {
        if (User32.IsWindowVisible(handle) == 0)
            return null;

        int titleLength = User32.GetWindowTextLength(handle);
        var title = GetWindowTitle(handle, titleLength);
        var className = GetWindowClassName(handle);

        User32.GetWindowThreadProcessId(handle, out uint processId);
        var processName = GetProcessName(processId);
        var executablePath = GetProcessPath(processId);

        User32.GetWindowRect(handle, out var rect);

        return new WindowInfo
        {
            Handle = handle,
            Title = title,
            ClassName = className,
            ProcessId = (int)processId,
            ProcessName = processName,
            ExecutablePath = executablePath,
            IsVisible = true,
            Width = rect.Width,
            Height = rect.Height
        };
    }

    private static unsafe string GetWindowTitle(nint hWnd, int titleLength)
    {
        if (titleLength == 0)
            return string.Empty;

        nint buffer = Marshal.AllocHGlobal((titleLength + 1) * sizeof(char));
        try
        {
            User32.GetWindowText(hWnd, buffer, titleLength + 1);
            return Marshal.PtrToStringUni(buffer) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static unsafe string GetWindowClassName(nint hWnd)
    {
        const int maxLength = 256;
        nint buffer = Marshal.AllocHGlobal(maxLength * sizeof(char));
        try
        {
            int length = User32.GetClassName(hWnd, buffer, maxLength);
            if (length > 0)
            {
                return Marshal.PtrToStringUni(buffer, length) ?? string.Empty;
            }
            return string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string GetProcessName(uint processId)
    {
        try
        {
            using var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName + ".exe";
        }
        catch
        {
            return string.Empty;
        }
    }

    private static unsafe string? GetProcessPath(uint processId)
    {
        var hProcess = Kernel32.OpenProcess(Kernel32.PROCESS_QUERY_LIMITED_INFORMATION, 0, processId);
        if (hProcess == 0)
            return null;

        try
        {
            const int maxPath = 1024;
            nint buffer = Marshal.AllocHGlobal(maxPath * sizeof(char));
            try
            {
                uint size = maxPath;
                if (Kernel32.QueryFullProcessImageName(hProcess, 0, buffer, ref size) != 0)
                {
                    return Marshal.PtrToStringUni(buffer, (int)size);
                }
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            Kernel32.CloseHandle(hProcess);
        }
    }
}
