using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ObsKit.NET.Platform.Linux.Interop;

namespace ObsKit.NET.Platform.Linux;

/// <summary>
/// Linux implementation of platform services.
/// Uses X11/XRandR for display enumeration and X11 for window enumeration.
/// Note: This implementation supports X11. Wayland requires different APIs.
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
                                                    IsPrimary = index == 1,
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
            return GetFallbackMonitors();
        }

        return monitors.Count > 0 ? monitors : GetFallbackMonitors();
    }

    private static List<MonitorInfo> GetFallbackMonitors()
    {
        return
        [
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
        ];
    }

    public MonitorInfo? GetPrimaryMonitor()
    {
        return GetMonitors().FirstOrDefault(m => m.IsPrimary);
    }

    public IReadOnlyList<WindowInfo> GetWindows()
    {
        var windows = new List<WindowInfo>();

        try
        {
            var display = X11.XOpenDisplay(0);
            if (display == 0)
                return windows;

            try
            {
                var root = X11.XDefaultRootWindow(display);
                EnumerateWindows(display, root, windows);
            }
            finally
            {
                X11.XCloseDisplay(display);
            }
        }
        catch
        {
            // Return what we have on error
        }

        return windows;
    }

    private void EnumerateWindows(nint display, nint window, List<WindowInfo> windows)
    {
        if (X11.XQueryTree(display, window, out _, out _, out var children, out var nChildren) == 0)
            return;

        if (children == 0)
            return;

        try
        {
            for (uint i = 0; i < nChildren; i++)
            {
                var child = Marshal.ReadIntPtr(children, (int)(i * (uint)IntPtr.Size));

                var windowInfo = GetWindowInfo(display, child);
                if (windowInfo != null)
                {
                    windows.Add(windowInfo);
                }

                // Recursively enumerate children
                EnumerateWindows(display, child, windows);
            }
        }
        finally
        {
            X11.XFree(children);
        }
    }

    private WindowInfo? GetWindowInfo(nint display, nint window)
    {
        // Get window attributes
        if (X11.XGetWindowAttributes(display, window, out var attrs) == 0)
            return null;

        // Only include viewable windows with reasonable size
        if (attrs.map_state != X11.IsViewable || attrs.width < 10 || attrs.height < 10)
            return null;

        // Skip override-redirect windows (popups, tooltips, etc.)
        if (attrs.override_redirect != 0)
            return null;

        // Get window title
        string title = GetWindowTitle(display, window);
        if (string.IsNullOrEmpty(title))
            return null;

        // Get window class
        string className = "";
        string processName = "";
        if (X11.XGetClassHint(display, window, out var classHint) != 0)
        {
            if (classHint.res_class != 0)
            {
                className = Marshal.PtrToStringAnsi(classHint.res_class) ?? "";
                X11.XFree(classHint.res_class);
            }
            if (classHint.res_name != 0)
            {
                processName = Marshal.PtrToStringAnsi(classHint.res_name) ?? "";
                X11.XFree(classHint.res_name);
            }
        }

        // Get PID from _NET_WM_PID property
        int pid = GetWindowPid(display, window);

        return new WindowInfo
        {
            Handle = window,
            Title = title,
            ClassName = className,
            ProcessName = string.IsNullOrEmpty(processName) ? className : processName,
            ProcessId = pid,
            ExecutablePath = pid > 0 ? GetProcessPath(pid) : null,
            IsVisible = true,
            Width = attrs.width,
            Height = attrs.height
        };
    }

    private string GetWindowTitle(nint display, nint window)
    {
        // Try _NET_WM_NAME first (UTF-8)
        var netWmName = X11.XInternAtom(display, "_NET_WM_NAME", true);
        var utf8String = X11.XInternAtom(display, "UTF8_STRING", true);

        if (netWmName != 0 && utf8String != 0)
        {
            if (X11.XGetWindowProperty(display, window, netWmName, 0, 1024, false, utf8String,
                out _, out var format, out var nItems, out _, out var prop) == 0 && prop != 0)
            {
                try
                {
                    if (format == 8 && nItems > 0)
                    {
                        var title = Marshal.PtrToStringUTF8(prop);
                        if (!string.IsNullOrEmpty(title))
                            return title;
                    }
                }
                finally
                {
                    X11.XFree(prop);
                }
            }
        }

        // Fall back to XFetchName
        if (X11.XFetchName(display, window, out var name) != 0 && name != 0)
        {
            try
            {
                return Marshal.PtrToStringAnsi(name) ?? "";
            }
            finally
            {
                X11.XFree(name);
            }
        }

        return "";
    }

    private int GetWindowPid(nint display, nint window)
    {
        var netWmPid = X11.XInternAtom(display, "_NET_WM_PID", true);
        if (netWmPid == 0)
            return 0;

        var cardinal = X11.XInternAtom(display, "CARDINAL", true);

        if (X11.XGetWindowProperty(display, window, netWmPid, 0, 1, false, cardinal,
            out _, out var format, out var nItems, out _, out var prop) == 0 && prop != 0)
        {
            try
            {
                if (format == 32 && nItems > 0)
                {
                    return Marshal.ReadInt32(prop);
                }
            }
            finally
            {
                X11.XFree(prop);
            }
        }

        return 0;
    }

    private static string? GetProcessPath(int pid)
    {
        try
        {
            var exePath = $"/proc/{pid}/exe";
            if (File.Exists(exePath))
            {
                return Path.GetFullPath(exePath);
            }
        }
        catch
        {
            // Ignore errors reading proc
        }
        return null;
    }

    public WindowInfo? GetWindow(nint handle)
    {
        try
        {
            var display = X11.XOpenDisplay(0);
            if (display == 0)
                return null;

            try
            {
                return GetWindowInfo(display, handle);
            }
            finally
            {
                X11.XCloseDisplay(display);
            }
        }
        catch
        {
            return null;
        }
    }
}
