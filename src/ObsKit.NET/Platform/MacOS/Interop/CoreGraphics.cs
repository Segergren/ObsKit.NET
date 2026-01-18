using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ObsKit.NET.Platform.MacOS.Interop;

/// <summary>
/// P/Invoke bindings for macOS CoreGraphics framework.
/// </summary>
[SupportedOSPlatform("macos")]
internal static partial class CoreGraphics
{
    private const string CoreGraphicsLib = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    #region Display

    /// <summary>
    /// Gets the list of active displays.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGGetActiveDisplayList")]
    internal static partial int CGGetActiveDisplayList(uint maxDisplays, [Out] uint[] activeDisplays, out uint displayCount);

    /// <summary>
    /// Gets the main display ID.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGMainDisplayID")]
    internal static partial uint CGMainDisplayID();

    /// <summary>
    /// Gets the bounds of a display.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGDisplayBounds")]
    internal static partial CGRect CGDisplayBounds(uint display);

    /// <summary>
    /// Gets the width of a display in pixels.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGDisplayPixelsWide")]
    internal static partial nuint CGDisplayPixelsWide(uint display);

    /// <summary>
    /// Gets the height of a display in pixels.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGDisplayPixelsHigh")]
    internal static partial nuint CGDisplayPixelsHigh(uint display);

    /// <summary>
    /// Gets the refresh rate of a display.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGDisplayModeGetRefreshRate")]
    internal static partial double CGDisplayModeGetRefreshRate(nint mode);

    /// <summary>
    /// Gets the current display mode.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGDisplayCopyDisplayMode")]
    internal static partial nint CGDisplayCopyDisplayMode(uint display);

    /// <summary>
    /// Checks if display is the main display.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGDisplayIsMain")]
    internal static partial int CGDisplayIsMain(uint display);

    #endregion

    #region Window

    /// <summary>
    /// Copies the window list.
    /// </summary>
    [LibraryImport(CoreGraphicsLib, EntryPoint = "CGWindowListCopyWindowInfo")]
    internal static partial nint CGWindowListCopyWindowInfo(uint option, uint relativeToWindow);

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    internal struct CGPoint
    {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CGSize
    {
        public double Width;
        public double Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CGRect
    {
        public CGPoint Origin;
        public CGSize Size;
    }

    #endregion

    #region Constants

    // CGWindowListOption
    internal const uint kCGWindowListOptionAll = 0;
    internal const uint kCGWindowListOptionOnScreenOnly = 1;
    internal const uint kCGWindowListOptionOnScreenAboveWindow = 2;
    internal const uint kCGWindowListOptionOnScreenBelowWindow = 4;
    internal const uint kCGWindowListOptionIncludingWindow = 8;
    internal const uint kCGWindowListExcludeDesktopElements = 16;

    // Window info dictionary keys
    internal const string kCGWindowNumber = "kCGWindowNumber";
    internal const string kCGWindowOwnerPID = "kCGWindowOwnerPID";
    internal const string kCGWindowOwnerName = "kCGWindowOwnerName";
    internal const string kCGWindowName = "kCGWindowName";
    internal const string kCGWindowBounds = "kCGWindowBounds";
    internal const string kCGWindowLayer = "kCGWindowLayer";
    internal const string kCGWindowIsOnscreen = "kCGWindowIsOnscreen";

    #endregion
}
