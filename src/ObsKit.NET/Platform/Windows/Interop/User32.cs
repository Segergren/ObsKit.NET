using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ObsKit.NET.Platform.Windows.Interop;

/// <summary>
/// P/Invoke bindings for Windows User32 functions.
/// </summary>
[SupportedOSPlatform("windows")]
internal static partial class User32
{
    private const string Lib = "user32.dll";

    #region Monitor Enumeration

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate byte MonitorEnumProc(nint hMonitor, nint hdcMonitor, nint lprcMonitor, nint dwData);

    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial byte EnumDisplayMonitors(
        nint hdc,
        nint lprcClip,
        MonitorEnumProc lpfnEnum,
        nint dwData);

    [LibraryImport(Lib, EntryPoint = "GetMonitorInfoW", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial byte GetMonitorInfo(nint hMonitor, nint lpmi);

    #endregion

    #region Window Enumeration

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate byte EnumWindowsProc(nint hWnd, nint lParam);

    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial byte EnumWindows(EnumWindowsProc lpEnumFunc, nint lParam);

    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial byte IsWindowVisible(nint hWnd);

    [LibraryImport(Lib, EntryPoint = "GetWindowTextW", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int GetWindowText(nint hWnd, nint lpString, int nMaxCount);

    [LibraryImport(Lib, EntryPoint = "GetWindowTextLengthW", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int GetWindowTextLength(nint hWnd);

    [LibraryImport(Lib, EntryPoint = "GetClassNameW", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int GetClassName(nint hWnd, nint lpClassName, int nMaxCount);

    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial byte GetWindowRect(nint hWnd, out RECT lpRect);

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial nint GetShellWindow();

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial nint GetDesktopWindow();

    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int GetWindowLong(nint hWnd, int nIndex);

    internal const int GWL_STYLE = -16;
    internal const int GWL_EXSTYLE = -20;
    internal const uint WS_VISIBLE = 0x10000000;
    internal const uint WS_EX_TOOLWINDOW = 0x00000080;
    internal const uint WS_EX_APPWINDOW = 0x00040000;

    #endregion

    #region Display Device Enumeration

    [LibraryImport(Lib, EntryPoint = "EnumDisplayDevicesA", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial byte EnumDisplayDevices(
        nint lpDevice,
        uint iDevNum,
        nint lpDisplayDevice,
        uint dwFlags);

    internal const uint EDD_GET_DEVICE_INTERFACE_NAME = 0x00000001;

    #endregion

    #region DPI Awareness

    /// <summary>
    /// Gets the DPI awareness context for the current thread.
    /// </summary>
    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial nint GetThreadDpiAwarenessContext();

    /// <summary>
    /// Gets the DPI_AWARENESS value from a DPI_AWARENESS_CONTEXT.
    /// </summary>
    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int GetAwarenessFromDpiAwarenessContext(nint value);

    /// <summary>
    /// DPI unaware. The thread does not scale for DPI changes.
    /// </summary>
    internal const int DPI_AWARENESS_UNAWARE = 0;

    /// <summary>
    /// System DPI aware. The thread queries for DPI once at startup.
    /// </summary>
    internal const int DPI_AWARENESS_SYSTEM_AWARE = 1;

    /// <summary>
    /// Per-monitor DPI aware. The thread receives DPI change notifications.
    /// </summary>
    internal const int DPI_AWARENESS_PER_MONITOR_AWARE = 2;

    /// <summary>
    /// Checks if the current thread is per-monitor DPI aware.
    /// </summary>
    internal static bool IsPerMonitorDpiAware()
    {
        try
        {
            var context = GetThreadDpiAwarenessContext();
            var awareness = GetAwarenessFromDpiAwarenessContext(context);
            return awareness >= DPI_AWARENESS_PER_MONITOR_AWARE;
        }
        catch
        {
            // API not available (pre-Windows 10 1607)
            return false;
        }
    }

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szDevice;

        public const uint MONITORINFOF_PRIMARY = 1;

        public readonly string GetDeviceName() => szDevice ?? string.Empty;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct DISPLAY_DEVICE
    {
        public int cb;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;

        public uint StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    #endregion
}

/// <summary>
/// P/Invoke bindings for Windows Kernel32 functions.
/// </summary>
[SupportedOSPlatform("windows")]
internal static partial class Kernel32
{
    private const string Lib = "kernel32.dll";

    [LibraryImport(Lib, EntryPoint = "OpenProcess", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial nint OpenProcess(uint dwDesiredAccess, byte bInheritHandle, uint dwProcessId);

    [LibraryImport(Lib, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial byte CloseHandle(nint hObject);

    [LibraryImport(Lib, EntryPoint = "QueryFullProcessImageNameW", SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial byte QueryFullProcessImageName(nint hProcess, uint dwFlags, nint lpExeName, ref uint lpdwSize);

    internal const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
}

/// <summary>
/// P/Invoke bindings for Windows Ole32 COM functions.
/// </summary>
[SupportedOSPlatform("windows")]
internal static partial class Ole32
{
    private const string Lib = "ole32.dll";

    /// <summary>
    /// Initializes the COM library for use by the calling thread.
    /// </summary>
    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int CoInitializeEx(nint reserved, uint dwCoInit);

    /// <summary>
    /// Closes the COM library on the current thread.
    /// </summary>
    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial void CoUninitialize();

    /// <summary>
    /// Apartment-threaded object concurrency.
    /// </summary>
    internal const uint COINIT_APARTMENTTHREADED = 0x2;

    /// <summary>
    /// Multi-threaded object concurrency (required for DXGI).
    /// </summary>
    internal const uint COINIT_MULTITHREADED = 0x0;

    /// <summary>
    /// Disables DDE for OLE1 support.
    /// </summary>
    internal const uint COINIT_DISABLE_OLE1DDE = 0x4;

    /// <summary>
    /// Trade memory for speed.
    /// </summary>
    internal const uint COINIT_SPEED_OVER_MEMORY = 0x8;

    /// <summary>
    /// S_OK - Success.
    /// </summary>
    internal const int S_OK = 0;

    /// <summary>
    /// S_FALSE - The COM library is already initialized.
    /// </summary>
    internal const int S_FALSE = 1;

    /// <summary>
    /// RPC_E_CHANGED_MODE - A previous call specified a different concurrency model.
    /// </summary>
    internal const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106);
}
