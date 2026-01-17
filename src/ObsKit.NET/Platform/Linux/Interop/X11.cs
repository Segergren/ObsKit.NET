using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ObsKit.NET.Platform.Linux.Interop;

/// <summary>
/// P/Invoke bindings for X11 functions.
/// Note: These are basic bindings. Full X11 support would require more comprehensive bindings.
/// </summary>
[SupportedOSPlatform("linux")]
internal static partial class X11
{
    private const string LibX11 = "libX11.so.6";
    private const string LibXrandr = "libXrandr.so.2";

    #region Display

    [LibraryImport(LibX11, EntryPoint = "XOpenDisplay")]
    internal static partial nint XOpenDisplay(nint displayName);

    [LibraryImport(LibX11, EntryPoint = "XCloseDisplay")]
    internal static partial int XCloseDisplay(nint display);

    [LibraryImport(LibX11, EntryPoint = "XDefaultScreen")]
    internal static partial int XDefaultScreen(nint display);

    [LibraryImport(LibX11, EntryPoint = "XDefaultRootWindow")]
    internal static partial nint XDefaultRootWindow(nint display);

    [LibraryImport(LibX11, EntryPoint = "XDisplayWidth")]
    internal static partial int XDisplayWidth(nint display, int screenNumber);

    [LibraryImport(LibX11, EntryPoint = "XDisplayHeight")]
    internal static partial int XDisplayHeight(nint display, int screenNumber);

    [LibraryImport(LibX11, EntryPoint = "XScreenCount")]
    internal static partial int XScreenCount(nint display);

    #endregion

    #region XRandR

    [LibraryImport(LibXrandr, EntryPoint = "XRRGetScreenResources")]
    internal static partial nint XRRGetScreenResources(nint display, nint window);

    [LibraryImport(LibXrandr, EntryPoint = "XRRFreeScreenResources")]
    internal static partial void XRRFreeScreenResources(nint resources);

    [LibraryImport(LibXrandr, EntryPoint = "XRRGetOutputInfo")]
    internal static partial nint XRRGetOutputInfo(nint display, nint resources, nint output);

    [LibraryImport(LibXrandr, EntryPoint = "XRRFreeOutputInfo")]
    internal static partial void XRRFreeOutputInfo(nint outputInfo);

    [LibraryImport(LibXrandr, EntryPoint = "XRRGetCrtcInfo")]
    internal static partial nint XRRGetCrtcInfo(nint display, nint resources, nint crtc);

    [LibraryImport(LibXrandr, EntryPoint = "XRRFreeCrtcInfo")]
    internal static partial void XRRFreeCrtcInfo(nint crtcInfo);

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    internal struct XRRScreenResources
    {
        public nint timestamp;
        public nint configTimestamp;
        public int ncrtc;
        public nint crtcs;
        public int noutput;
        public nint outputs;
        public int nmode;
        public nint modes;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XRROutputInfo
    {
        public nint timestamp;
        public nint crtc;
        public nint name;
        public int nameLen;
        public nint mm_width;
        public nint mm_height;
        public ushort connection;
        public ushort subpixel_order;
        public int ncrtc;
        public nint crtcs;
        public int nclone;
        public nint clones;
        public int nmode;
        public int npreferred;
        public nint modes;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XRRCrtcInfo
    {
        public nint timestamp;
        public int x;
        public int y;
        public uint width;
        public uint height;
        public nint mode;
        public ushort rotation;
        public int noutput;
        public nint outputs;
        public ushort rotations;
        public int npossible;
        public nint possible;
    }

    #endregion
}
