using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ObsKit.NET.Platform.Linux.Interop;

/// <summary>
/// P/Invoke bindings for X11 functions.
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

    #region Window

    [LibraryImport(LibX11, EntryPoint = "XQueryTree")]
    internal static partial int XQueryTree(
        nint display,
        nint window,
        out nint rootReturn,
        out nint parentReturn,
        out nint childrenReturn,
        out uint nChildren);

    [LibraryImport(LibX11, EntryPoint = "XFree")]
    internal static partial int XFree(nint data);

    [LibraryImport(LibX11, EntryPoint = "XGetWindowAttributes")]
    internal static partial int XGetWindowAttributes(nint display, nint window, out XWindowAttributes attributes);

    [LibraryImport(LibX11, EntryPoint = "XFetchName")]
    internal static partial int XFetchName(nint display, nint window, out nint windowName);

    [LibraryImport(LibX11, EntryPoint = "XGetClassHint")]
    internal static partial int XGetClassHint(nint display, nint window, out XClassHint classHint);

    [LibraryImport(LibX11, EntryPoint = "XInternAtom", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial nint XInternAtom(nint display, string atomName, [MarshalAs(UnmanagedType.Bool)] bool onlyIfExists);

    [LibraryImport(LibX11, EntryPoint = "XGetWindowProperty")]
    internal static partial int XGetWindowProperty(
        nint display,
        nint window,
        nint property,
        long longOffset,
        long longLength,
        [MarshalAs(UnmanagedType.Bool)] bool delete,
        nint reqType,
        out nint actualTypeReturn,
        out int actualFormatReturn,
        out nuint nItemsReturn,
        out nuint bytesAfterReturn,
        out nint propReturn);

    [LibraryImport(LibX11, EntryPoint = "XGetWMName")]
    internal static partial int XGetWMName(nint display, nint window, out XTextProperty textProp);

    [LibraryImport(LibX11, EntryPoint = "XmbTextPropertyToTextList")]
    internal static partial int XmbTextPropertyToTextList(nint display, ref XTextProperty textProp, out nint listReturn, out int countReturn);

    [LibraryImport(LibX11, EntryPoint = "XFreeStringList")]
    internal static partial void XFreeStringList(nint list);

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

    [StructLayout(LayoutKind.Sequential)]
    internal struct XWindowAttributes
    {
        public int x, y;
        public int width, height;
        public int border_width;
        public int depth;
        public nint visual;
        public nint root;
        public int c_class;
        public int bit_gravity;
        public int win_gravity;
        public int backing_store;
        public nuint backing_planes;
        public nuint backing_pixel;
        public int save_under;
        public nint colormap;
        public int map_installed;
        public int map_state;
        public nint all_event_masks;
        public nint your_event_mask;
        public nint do_not_propagate_mask;
        public int override_redirect;
        public nint screen;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XClassHint
    {
        public nint res_name;
        public nint res_class;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct XTextProperty
    {
        public nint value;
        public nint encoding;
        public int format;
        public nuint nitems;
    }

    // Map state constants
    internal const int IsUnmapped = 0;
    internal const int IsUnviewable = 1;
    internal const int IsViewable = 2;

    // Atom constants
    internal static readonly nint AnyPropertyType = 0;

    #endregion
}
