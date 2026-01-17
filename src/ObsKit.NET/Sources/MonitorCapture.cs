using ObsKit.NET.Core;
using ObsKit.NET.Platform;

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
                settings.Set("monitor_id", monitor.DeviceName);
            }
            settings.Set("capture_cursor", captureCursor);
            // Default to WGC (Windows Graphics Capture) to avoid DXGI errors
            // DXGI Desktop Duplication can fail with 0x887A0004 in certain scenarios
            settings.Set("method", 2); // 0=auto, 1=DXGI, 2=WGC
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
                    s.Set("monitor_id", monitor.DeviceName);
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                s.Set("display", monitorIndex);
            }
        });
        return this;
    }

    public MonitorCapture SetMonitor(MonitorInfo monitor)
    {
        Update(s =>
        {
            if (OperatingSystem.IsWindows())
            {
                s.Set("monitor_id", monitor.DeviceName);
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
}
