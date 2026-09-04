namespace ObsKit.NET.Platform;

/// <summary>
/// Provides platform-specific services for monitor and window enumeration.
/// </summary>
public interface IPlatformServices
{
    /// <summary>
    /// Gets all available monitors.
    /// </summary>
    IReadOnlyList<MonitorInfo> GetMonitors();

    /// <summary>
    /// Gets the primary monitor.
    /// </summary>
    MonitorInfo? GetPrimaryMonitor();

    /// <summary>
    /// Gets all visible windows.
    /// </summary>
    IReadOnlyList<WindowInfo> GetWindows();

    /// <summary>
    /// Gets a window by its handle.
    /// </summary>
    WindowInfo? GetWindow(nint handle);
}

/// <summary>
/// The color mode a monitor is currently composing in.
/// </summary>
public enum MonitorColorMode
{
    /// <summary>Standard dynamic range.</summary>
    Sdr = 0,

    /// <summary>Wide color gamut, but not HDR.</summary>
    WideColorGamut = 1,

    /// <summary>High dynamic range.</summary>
    Hdr = 2
}

/// <summary>
/// Information about a display monitor.
/// </summary>
public sealed class MonitorInfo
{
    /// <summary>
    /// Gets the monitor index (0-based).
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Gets the monitor name/description.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the device name (e.g., "\\.\DISPLAY1").
    /// </summary>
    public string DeviceName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the full device interface ID for OBS monitor capture.
    /// This is the preferred ID for DXGI/WGC capture methods.
    /// </summary>
    public string DeviceId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the monitor handle.
    /// </summary>
    public nint Handle { get; init; }

    /// <summary>
    /// Gets the monitor width in pixels.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Gets the monitor height in pixels.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Gets the X position of the monitor.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    /// Gets the Y position of the monitor.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    /// Gets whether this is the primary monitor.
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    /// Gets the monitor's refresh rate in Hz, rounded to the nearest whole number.
    /// </summary>
    public int RefreshRate { get; init; }

    /// <summary>
    /// Gets the monitor's exact refresh rate in Hz (e.g. 59.94). Falls back to 60 when the
    /// platform cannot report one.
    /// </summary>
    public double RefreshRateExact { get; init; }

    /// <summary>
    /// Gets whether the monitor is a built-in panel (laptop display or embedded output).
    /// </summary>
    public bool IsInternal { get; init; }

    /// <summary>
    /// Gets whether the monitor supports HDR. On Windows builds older than 11 24H2 this is
    /// approximated from the advanced-color support flag, which also covers wide color gamut.
    /// </summary>
    public bool IsHdrCapable { get; init; }

    /// <summary>
    /// Gets the color mode the monitor is currently composing in. Windows 11 24H2 and later
    /// distinguish HDR from wide color gamut; older builds fall back to a best-effort guess.
    /// </summary>
    public MonitorColorMode ColorMode { get; init; }

    /// <summary>
    /// Gets whether the monitor is currently in HDR mode.
    /// </summary>
    public bool IsHdrEnabled => ColorMode == MonitorColorMode.Hdr;

    /// <summary>
    /// Gets the monitor's bits per color channel, or 0 if unknown.
    /// </summary>
    public int BitsPerColorChannel { get; init; }

    /// <summary>
    /// Gets the SDR white level in nits, used to scale SDR content on an HDR display.
    /// Defaults to 80 when the monitor does not report one.
    /// </summary>
    public int SdrWhiteLevelNits { get; init; }

    /// <summary>
    /// Gets the OBS-compatible monitor ID for capture.
    /// </summary>
    public string ObsId => Index.ToString();

    public override string ToString() => $"{Name} ({Width}x{Height})";
}

/// <summary>
/// Information about a window.
/// </summary>
public sealed class WindowInfo
{
    /// <summary>
    /// Gets the window handle.
    /// </summary>
    public nint Handle { get; init; }

    /// <summary>
    /// Gets the window title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the window class name.
    /// </summary>
    public string ClassName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the process name that owns the window.
    /// </summary>
    public string ProcessName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the process ID that owns the window.
    /// </summary>
    public int ProcessId { get; init; }

    /// <summary>
    /// Gets the executable path of the owning process.
    /// </summary>
    public string? ExecutablePath { get; init; }

    /// <summary>
    /// Gets whether the window is visible.
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// Gets the window width.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Gets the window height.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Gets the OBS-compatible window ID for capture.
    /// Format: "WindowTitle:ClassName:ProcessName.exe" (parts encoded per OBS rules).
    /// </summary>
    public string ObsId => WindowSpec.Build(Title, ClassName, ProcessName);

    public override string ToString() => $"{Title} ({ProcessName})";
}

/// <summary>
/// Provides access to platform-specific services.
/// </summary>
public static class Platform
{
    private static IPlatformServices? _services;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the platform services for the current OS.
    /// </summary>
    public static IPlatformServices Services
    {
        get
        {
            if (_services == null)
            {
                lock (_lock)
                {
                    _services ??= CreatePlatformServices();
                }
            }
            return _services;
        }
    }

    private static IPlatformServices CreatePlatformServices()
    {
        if (OperatingSystem.IsWindows())
            return new Windows.WindowsPlatform();
        if (OperatingSystem.IsLinux())
            return new Linux.LinuxPlatform();
        if (OperatingSystem.IsMacOS())
            return new MacOS.MacOSPlatform();

        throw new PlatformNotSupportedException("Current platform is not supported.");
    }

    /// <summary>
    /// Gets all available monitors.
    /// </summary>
    public static IReadOnlyList<MonitorInfo> Monitors => Services.GetMonitors();

    /// <summary>
    /// Gets the primary monitor.
    /// </summary>
    public static MonitorInfo? PrimaryMonitor => Services.GetPrimaryMonitor();

    /// <summary>
    /// Gets all visible windows.
    /// </summary>
    public static IReadOnlyList<WindowInfo> Windows => Services.GetWindows();
}
