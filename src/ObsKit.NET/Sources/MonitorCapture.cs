using ObsKit.NET.Core;
using ObsKit.NET.Platform;
using ObsKit.NET.Platform.Windows.Interop;

namespace ObsKit.NET.Sources;

/// <summary>
/// Capture method for Windows monitor capture.
/// </summary>
public enum MonitorCaptureMethod
{
    /// <summary>
    /// Automatic selection (tries WGC first, falls back to DXGI).
    /// </summary>
    Auto,

    /// <summary>
    /// Windows Graphics Capture API (Windows 10 1903+, recommended).
    /// </summary>
    WindowsGraphicsCapture,

    /// <summary>
    /// DXGI Desktop Duplication (legacy, may have permission issues).
    /// </summary>
    DesktopDuplication
}

/// <summary>
/// Represents a monitor/display capture source.
/// </summary>
public sealed class MonitorCapture : Source
{
    /// <summary>
    /// The source type ID for Windows monitor capture.
    /// </summary>
    public const string WindowsTypeId = "monitor_capture";

    /// <summary>
    /// The source type ID for Linux monitor capture (PipeWire).
    /// </summary>
    public const string LinuxTypeId = "pipewire-desktop-capture-source";

    /// <summary>
    /// The source type ID for macOS display capture.
    /// </summary>
    public const string MacOSTypeId = "display_capture";

    /// <summary>
    /// Gets the platform-appropriate source type ID.
    /// </summary>
    public static string TypeIdForPlatform => OperatingSystem.IsWindows() ? WindowsTypeId :
                                               OperatingSystem.IsLinux() ? LinuxTypeId :
                                               OperatingSystem.IsMacOS() ? MacOSTypeId :
                                               WindowsTypeId;

    /// <summary>
    /// Creates a monitor capture source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="monitorIndex">The monitor index (0 = primary).</param>
    /// <param name="captureCursor">Whether to capture the cursor.</param>
    public MonitorCapture(string name, int monitorIndex = 0, bool captureCursor = true)
        : base(TypeIdForPlatform, name, BuildInitialSettings(monitorIndex, captureCursor))
    {
    }

    /// <summary>
    /// Creates a monitor capture source for the primary monitor.
    /// </summary>
    /// <param name="name">Optional source name.</param>
    /// <returns>A monitor capture source.</returns>
    public static MonitorCapture FromPrimary(string? name = null)
    {
        var primary = Platform.Platform.PrimaryMonitor;
        var monitorIndex = primary?.Index ?? 0;
        var displayName = name ?? primary?.Name ?? "Monitor Capture";
        return new MonitorCapture(displayName, monitorIndex, true);
    }

    /// <summary>
    /// Creates a monitor capture source for a specific monitor.
    /// </summary>
    /// <param name="monitorIndex">The monitor index.</param>
    /// <param name="name">Optional source name.</param>
    /// <returns>A monitor capture source.</returns>
    public static MonitorCapture FromMonitor(int monitorIndex, string? name = null)
    {
        return new MonitorCapture(name ?? $"Monitor {monitorIndex} Capture", monitorIndex, true);
    }

    /// <summary>
    /// Creates a monitor capture source from a MonitorInfo object.
    /// </summary>
    /// <param name="monitor">The monitor to capture.</param>
    /// <param name="captureCursor">Whether to capture the cursor.</param>
    /// <returns>A monitor capture source.</returns>
    public static MonitorCapture FromMonitor(MonitorInfo monitor, bool captureCursor = true)
    {
        return new MonitorCapture(monitor.Name, monitor.Index, captureCursor);
    }

    /// <summary>
    /// Gets all available monitors for capture.
    /// </summary>
    public static IReadOnlyList<MonitorInfo> AvailableMonitors => Platform.Platform.Monitors;

    private static Settings BuildInitialSettings(int monitorIndex, bool captureCursor)
    {
        var settings = new Settings();

        if (OperatingSystem.IsWindows())
        {
            var monitors = Platform.Platform.Monitors;
            var monitor = monitors.FirstOrDefault(m => m.Index == monitorIndex) ?? monitors.FirstOrDefault();
            if (monitor != null)
            {
                // Use DeviceId (full device interface ID like \\?\DISPLAY#SAM0FEC#...)
                // This is what OBS uses to match monitors for DXGI capture (duplicator_capture_info)
                settings.Set("monitor_id", monitor.DeviceId ?? monitor.DeviceName);
            }
            // Also set integer monitor index for GDI-based capture (monitor_capture_info)
            // OBS registers different source implementations with the same "monitor_capture" ID
            // depending on whether graphics_uses_d3d11 is true or false
            settings.Set("monitor", monitorIndex);
            settings.Set("capture_cursor", captureCursor);
            // Let OBS choose the best capture method automatically
            settings.Set("method", 0); // 0=auto, 1=DXGI, 2=WGC
        }
        else if (OperatingSystem.IsLinux())
        {
            // Linux PipeWire settings
            settings.Set("show_cursor", captureCursor);
        }
        else if (OperatingSystem.IsMacOS())
        {
            // macOS display_capture settings
            settings.Set("display", monitorIndex);
            settings.Set("show_cursor", captureCursor);
        }

        return settings;
    }

    /// <summary>
    /// Sets whether to capture the cursor.
    /// </summary>
    /// <param name="capture">Whether to capture the cursor.</param>
    public MonitorCapture SetCaptureCursor(bool capture)
    {
        Update(s =>
        {
            if (OperatingSystem.IsWindows())
                s.Set("capture_cursor", capture);
            else
                s.Set("show_cursor", capture);
        });
        return this;
    }

    /// <summary>
    /// Sets the monitor to capture by index.
    /// </summary>
    /// <param name="monitorIndex">The monitor index.</param>
    public MonitorCapture SetMonitor(int monitorIndex)
    {
        Update(s =>
        {
            if (OperatingSystem.IsWindows())
            {
                var monitors = Platform.Platform.Monitors;
                var monitor = monitors.FirstOrDefault(m => m.Index == monitorIndex) ?? monitors.FirstOrDefault();
                if (monitor != null)
                {
                    // For DXGI-based duplicator_capture_info (string monitor_id)
                    s.Set("monitor_id", monitor.DeviceId ?? monitor.DeviceName);
                }
                // For GDI-based monitor_capture_info (integer monitor)
                s.Set("monitor", monitorIndex);
            }
            else if (OperatingSystem.IsMacOS())
            {
                s.Set("display", monitorIndex);
            }
        });
        return this;
    }

    /// <summary>
    /// Sets the monitor to capture.
    /// </summary>
    /// <param name="monitor">The monitor to capture.</param>
    public MonitorCapture SetMonitor(MonitorInfo monitor)
    {
        Update(s =>
        {
            if (OperatingSystem.IsWindows())
            {
                // For DXGI-based duplicator_capture_info (string monitor_id)
                s.Set("monitor_id", monitor.DeviceId ?? monitor.DeviceName);
                // For GDI-based monitor_capture_info (integer monitor)
                s.Set("monitor", monitor.Index);
            }
            else if (OperatingSystem.IsMacOS())
            {
                s.Set("display", monitor.Index);
            }
        });
        return this;
    }

    /// <summary>
    /// Sets the capture method (Windows only).
    /// </summary>
    /// <param name="method">The capture method to use.</param>
    public MonitorCapture SetCaptureMethod(MonitorCaptureMethod method)
    {
        if (OperatingSystem.IsWindows())
        {
            // Check DPI awareness when using DXGI Desktop Duplication
            if ((method == MonitorCaptureMethod.DesktopDuplication || method == MonitorCaptureMethod.Auto) && !User32.IsPerMonitorDpiAware())
            {
                Console.Error.WriteLine("[ObsKit.NET] Warning: Desktop Duplication (DXGI) requires per-monitor DPI awareness.");
                Console.Error.WriteLine("[ObsKit.NET] Your application must include an app.manifest with DPI awareness settings.");
                Console.Error.WriteLine("[ObsKit.NET] Or use MonitorCaptureMethod.WindowsGraphicsCapture instead.");
                Console.Error.WriteLine("[ObsKit.NET] See: https://github.com/Segergren/ObsKit.NET#dpi-awareness-windows");
            }

            Update(s =>
            {
                // OBS uses integer values: 0=auto, 1=DXGI, 2=WGC
                var methodValue = method switch
                {
                    MonitorCaptureMethod.WindowsGraphicsCapture => 2,
                    MonitorCaptureMethod.DesktopDuplication => 1,
                    _ => 0
                };
                s.Set("method", methodValue);
            });
        }
        return this;
    }

    /// <summary>
    /// Forces SDR output mode (Windows only). May help with HDR-related capture issues.
    /// </summary>
    /// <param name="forceSdr">Whether to force SDR mode.</param>
    public MonitorCapture SetForceSdr(bool forceSdr)
    {
        if (OperatingSystem.IsWindows())
        {
            Update(s => s.Set("force_sdr", forceSdr));
        }
        return this;
    }
}
