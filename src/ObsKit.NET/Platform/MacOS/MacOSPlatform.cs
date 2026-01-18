using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ObsKit.NET.Platform.MacOS.Interop;

namespace ObsKit.NET.Platform.MacOS;

/// <summary>
/// macOS implementation of platform services.
/// Uses CoreGraphics for display and window enumeration.
/// </summary>
[SupportedOSPlatform("macos")]
internal sealed partial class MacOSPlatform : IPlatformServices
{
    // Cache CFString keys to avoid repeated allocations
    private static readonly Lazy<nint> WindowNumberKey = new(() =>
        CoreFoundation.CFStringCreateWithCString(0, CoreGraphics.kCGWindowNumber, CoreFoundation.kCFStringEncodingUTF8));
    private static readonly Lazy<nint> WindowOwnerPIDKey = new(() =>
        CoreFoundation.CFStringCreateWithCString(0, CoreGraphics.kCGWindowOwnerPID, CoreFoundation.kCFStringEncodingUTF8));
    private static readonly Lazy<nint> WindowOwnerNameKey = new(() =>
        CoreFoundation.CFStringCreateWithCString(0, CoreGraphics.kCGWindowOwnerName, CoreFoundation.kCFStringEncodingUTF8));
    private static readonly Lazy<nint> WindowNameKey = new(() =>
        CoreFoundation.CFStringCreateWithCString(0, CoreGraphics.kCGWindowName, CoreFoundation.kCFStringEncodingUTF8));
    private static readonly Lazy<nint> WindowBoundsKey = new(() =>
        CoreFoundation.CFStringCreateWithCString(0, CoreGraphics.kCGWindowBounds, CoreFoundation.kCFStringEncodingUTF8));
    private static readonly Lazy<nint> WindowLayerKey = new(() =>
        CoreFoundation.CFStringCreateWithCString(0, CoreGraphics.kCGWindowLayer, CoreFoundation.kCFStringEncodingUTF8));
    private static readonly Lazy<nint> WindowIsOnscreenKey = new(() =>
        CoreFoundation.CFStringCreateWithCString(0, CoreGraphics.kCGWindowIsOnscreen, CoreFoundation.kCFStringEncodingUTF8));

    public IReadOnlyList<MonitorInfo> GetMonitors()
    {
        var monitors = new List<MonitorInfo>();

        try
        {
            // Get display count first
            if (CoreGraphics.CGGetActiveDisplayList(0, null!, out var displayCount) != 0)
                return GetFallbackMonitors();

            if (displayCount == 0)
                return GetFallbackMonitors();

            // Get display IDs
            var displays = new uint[displayCount];
            if (CoreGraphics.CGGetActiveDisplayList(displayCount, displays, out displayCount) != 0)
                return GetFallbackMonitors();

            var mainDisplayId = CoreGraphics.CGMainDisplayID();

            for (int i = 0; i < displayCount; i++)
            {
                var displayId = displays[i];
                var bounds = CoreGraphics.CGDisplayBounds(displayId);
                var width = (int)CoreGraphics.CGDisplayPixelsWide(displayId);
                var height = (int)CoreGraphics.CGDisplayPixelsHigh(displayId);

                // Get refresh rate from display mode
                int refreshRate = 60; // Default
                var mode = CoreGraphics.CGDisplayCopyDisplayMode(displayId);
                if (mode != 0)
                {
                    var rate = CoreGraphics.CGDisplayModeGetRefreshRate(mode);
                    if (rate > 0)
                        refreshRate = (int)Math.Round(rate);
                    CoreFoundation.CFRelease(mode);
                }

                var isPrimary = displayId == mainDisplayId;

                monitors.Add(new MonitorInfo
                {
                    Index = i,
                    Name = isPrimary ? "Built-in Display" : $"Display {i + 1}",
                    DeviceName = $"display{displayId}",
                    Handle = (nint)displayId,
                    X = (int)bounds.Origin.X,
                    Y = (int)bounds.Origin.Y,
                    Width = width > 0 ? width : (int)bounds.Size.Width,
                    Height = height > 0 ? height : (int)bounds.Size.Height,
                    IsPrimary = isPrimary,
                    RefreshRate = refreshRate
                });
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
                Name = "Built-in Display",
                DeviceName = "display0",
                Handle = 0,
                X = 0,
                Y = 0,
                Width = 2560,
                Height = 1600,
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
            // Get on-screen windows, excluding desktop elements
            var windowList = CoreGraphics.CGWindowListCopyWindowInfo(
                CoreGraphics.kCGWindowListOptionOnScreenOnly | CoreGraphics.kCGWindowListExcludeDesktopElements,
                0);

            if (windowList == 0)
                return windows;

            try
            {
                var count = CoreFoundation.CFArrayGetCount(windowList);

                for (nint i = 0; i < count; i++)
                {
                    var windowDict = CoreFoundation.CFArrayGetValueAtIndex(windowList, i);
                    if (windowDict == 0)
                        continue;

                    var windowInfo = ParseWindowInfo(windowDict);
                    if (windowInfo != null)
                    {
                        windows.Add(windowInfo);
                    }
                }
            }
            finally
            {
                CoreFoundation.CFRelease(windowList);
            }
        }
        catch
        {
            // Return what we have on error
        }

        return windows;
    }

    private WindowInfo? ParseWindowInfo(nint windowDict)
    {
        // Get window layer - skip windows with layer != 0 (menus, popups, etc.)
        if (CoreFoundation.CFDictionaryGetValueIfPresent(windowDict, WindowLayerKey.Value, out var layerValue))
        {
            var layer = CoreFoundation.GetInt(layerValue);
            if (layer.HasValue && layer.Value != 0)
                return null;
        }

        // Get window number (handle)
        nint windowNumber = 0;
        if (CoreFoundation.CFDictionaryGetValueIfPresent(windowDict, WindowNumberKey.Value, out var numberValue))
        {
            var num = CoreFoundation.GetInt(numberValue);
            if (num.HasValue)
                windowNumber = num.Value;
        }

        if (windowNumber == 0)
            return null;

        // Get owner PID
        int ownerPid = 0;
        if (CoreFoundation.CFDictionaryGetValueIfPresent(windowDict, WindowOwnerPIDKey.Value, out var pidValue))
        {
            var pid = CoreFoundation.GetInt(pidValue);
            if (pid.HasValue)
                ownerPid = pid.Value;
        }

        // Get owner name (process name)
        string ownerName = "";
        if (CoreFoundation.CFDictionaryGetValueIfPresent(windowDict, WindowOwnerNameKey.Value, out var ownerNameValue))
        {
            ownerName = CoreFoundation.GetString(ownerNameValue) ?? "";
        }

        // Get window name (title)
        string windowName = "";
        if (CoreFoundation.CFDictionaryGetValueIfPresent(windowDict, WindowNameKey.Value, out var windowNameValue))
        {
            windowName = CoreFoundation.GetString(windowNameValue) ?? "";
        }

        // Skip windows without a title or owner
        if (string.IsNullOrEmpty(windowName) && string.IsNullOrEmpty(ownerName))
            return null;

        // Get window bounds
        int width = 0, height = 0;
        if (CoreFoundation.CFDictionaryGetValueIfPresent(windowDict, WindowBoundsKey.Value, out var boundsValue))
        {
            if (CoreFoundation.CGRectMakeWithDictionaryRepresentation(boundsValue, out var rect))
            {
                width = (int)rect.Size.Width;
                height = (int)rect.Size.Height;
            }
        }

        // Skip very small windows
        if (width < 10 || height < 10)
            return null;

        // Check if onscreen
        bool isVisible = true;
        if (CoreFoundation.CFDictionaryGetValueIfPresent(windowDict, WindowIsOnscreenKey.Value, out var onscreenValue))
        {
            isVisible = CoreFoundation.CFBooleanGetValue(onscreenValue);
        }

        if (!isVisible)
            return null;

        return new WindowInfo
        {
            Handle = windowNumber,
            Title = string.IsNullOrEmpty(windowName) ? ownerName : windowName,
            ClassName = ownerName, // macOS doesn't have window classes like Windows
            ProcessName = ownerName,
            ProcessId = ownerPid,
            ExecutablePath = GetProcessPath(ownerPid),
            IsVisible = isVisible,
            Width = width,
            Height = height
        };
    }

    private static string? GetProcessPath(int pid)
    {
        if (pid <= 0)
            return null;

        try
        {
            // Use proc_pidpath to get the executable path
            var buffer = new byte[4096];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                var result = proc_pidpath(pid, handle.AddrOfPinnedObject(), (uint)buffer.Length);
                if (result > 0)
                {
                    return Marshal.PtrToStringUTF8(handle.AddrOfPinnedObject());
                }
            }
            finally
            {
                handle.Free();
            }
        }
        catch
        {
            // Ignore errors
        }
        return null;
    }

    [LibraryImport("libproc.dylib", EntryPoint = "proc_pidpath")]
    private static partial int proc_pidpath(int pid, nint buffer, uint bufferSize);

    public WindowInfo? GetWindow(nint handle)
    {
        var windows = GetWindows();
        return windows.FirstOrDefault(w => w.Handle == handle);
    }
}
