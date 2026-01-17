using System.Runtime.Versioning;

namespace ObsKit.NET.Platform.MacOS;

/// <summary>
/// macOS implementation of platform services.
/// This is a stub implementation for future development.
/// </summary>
[SupportedOSPlatform("macos")]
internal sealed class MacOSPlatform : IPlatformServices
{
    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        // Stub implementation - returns a default monitor
        // Full implementation would use CoreGraphics (CGGetActiveDisplayList, etc.)
        return new List<MonitorInfo>
        {
            new MonitorInfo
            {
                Index = 0,
                Name = "Built-in Display",
                DeviceName = "display0",
                Handle = 0,
                X = 0,
                Y = 0,
                Width = 2560, // Common MacBook resolution
                Height = 1600,
                IsPrimary = true,
                RefreshRate = 60
            }
        };
    }

    public MonitorInfo? GetPrimaryMonitor()
    {
        return GetMonitors().FirstOrDefault(m => m.IsPrimary);
    }

    public IReadOnlyList<WindowInfo> GetWindows()
    {
        // Stub implementation
        // Full implementation would use CGWindowListCopyWindowInfo
        return Array.Empty<WindowInfo>();
    }

    public WindowInfo? GetWindow(nint handle)
    {
        // Stub implementation
        return null;
    }
}
