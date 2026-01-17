using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ObsKit.NET.Platform.Linux.Interop;

namespace ObsKit.NET.Platform.Linux;

/// <summary>
/// Linux implementation of platform services.
/// Uses X11/XRandR for display enumeration.
/// Note: This is a basic implementation. Full support would require PipeWire/Wayland handling.
/// </summary>
[SupportedOSPlatform("linux")]
internal sealed class LinuxPlatform : IPlatformServices
{
    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();

        try
        {
            var display = X11.XOpenDisplay(0);
            if (display == 0)
            {
                // Fallback: return a single monitor with default resolution
                return GetFallbackMonitors();
            }

            try
            {
                var root = X11.XDefaultRootWindow(display);
                var resources = X11.XRRGetScreenResources(display, root);

                if (resources != 0)
                {
                    try
                    {
                        var res = Marshal.PtrToStructure<X11.XRRScreenResources>(resources);
                        int index = 0;

                        for (int i = 0; i < res.noutput; i++)
                        {
                            var outputPtr = Marshal.ReadIntPtr(res.outputs, i * IntPtr.Size);
                            var outputInfo = X11.XRRGetOutputInfo(display, resources, outputPtr);

                            if (outputInfo != 0)
                            {
                                try
                                {
                                    var output = Marshal.PtrToStructure<X11.XRROutputInfo>(outputInfo);

                                    // Check if output is connected (connection == 0)
                                    if (output.connection == 0 && output.crtc != 0)
                                    {
                                        var crtcInfo = X11.XRRGetCrtcInfo(display, resources, output.crtc);
                                        if (crtcInfo != 0)
                                        {
                                            try
                                            {
                                                var crtc = Marshal.PtrToStructure<X11.XRRCrtcInfo>(crtcInfo);
                                                var name = output.name != 0 && output.nameLen > 0
                                                    ? Marshal.PtrToStringAnsi(output.name, output.nameLen)
                                                    : $"Display {index}";

                                                monitors.Add(new MonitorInfo
                                                {
                                                    Index = index++,
                                                    Name = name ?? $"Display {index}",
                                                    DeviceName = name ?? $"Display {index}",
                                                    Handle = outputPtr,
                                                    X = crtc.x,
                                                    Y = crtc.y,
                                                    Width = (int)crtc.width,
                                                    Height = (int)crtc.height,
                                                    IsPrimary = index == 1, // First connected display is primary
                                                    RefreshRate = 60
                                                });
                                            }
                                            finally
                                            {
                                                X11.XRRFreeCrtcInfo(crtcInfo);
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    X11.XRRFreeOutputInfo(outputInfo);
                                }
                            }
                        }
                    }
                    finally
                    {
                        X11.XRRFreeScreenResources(resources);
                    }
                }

                // Fallback if XRandR didn't work
                if (monitors.Count == 0)
                {
                    int screenCount = X11.XScreenCount(display);
                    for (int i = 0; i < screenCount; i++)
                    {
                        monitors.Add(new MonitorInfo
                        {
                            Index = i,
                            Name = $"Screen {i}",
                            DeviceName = $":0.{i}",
                            Handle = 0,
                            X = 0,
                            Y = 0,
                            Width = X11.XDisplayWidth(display, i),
                            Height = X11.XDisplayHeight(display, i),
                            IsPrimary = i == 0,
                            RefreshRate = 60
                        });
                    }
                }
            }
            finally
            {
                X11.XCloseDisplay(display);
            }
        }
        catch
        {
            // If X11 fails entirely, return fallback
            return GetFallbackMonitors();
        }

        return monitors.Count > 0 ? monitors : GetFallbackMonitors();
    }

    private static List<MonitorInfo> GetFallbackMonitors()
    {
        // Return a default monitor when X11 is not available
        return new List<MonitorInfo>
        {
            new MonitorInfo
            {
                Index = 0,
                Name = "Default Display",
                DeviceName = ":0",
                Handle = 0,
                X = 0,
                Y = 0,
                Width = 1920,
                Height = 1080,
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
        // Window enumeration on Linux requires more complex X11 calls
        // or using tools like wmctrl. For now, return empty list.
        // Full implementation would use XQueryTree and window properties.
        return Array.Empty<WindowInfo>();
    }

    public WindowInfo? GetWindow(nint handle)
    {
        // Not implemented for Linux in this basic version
        return null;
    }
}
