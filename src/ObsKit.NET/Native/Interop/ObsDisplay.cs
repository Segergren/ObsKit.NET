using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for obs_display functions (rendering OBS output into a native window).
/// </summary>
internal static partial class ObsDisplay
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    /// <summary>
    /// Draw callback invoked on the OBS graphics thread for each rendered frame.
    /// The graphics context is active during the callback.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DrawCallback(nint param, uint cx, uint cy);

    /// <summary>
    /// Graphics initialization data for a display (struct gs_init_data) on
    /// Windows (HWND) and macOS (NSView*), where gs_window is a single pointer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct GsInitData
    {
        public nint Window;
        public uint Cx;
        public uint Cy;
        public uint NumBackbuffers;
        public int Format;
        public int ZsFormat;
        public uint Adapter;
    }

    /// <summary>
    /// Graphics initialization data for a display (struct gs_init_data) on
    /// Linux/X11, where gs_window is a window id plus a Display pointer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct GsInitDataX11
    {
        public uint WindowId;
        public nint Display;
        public uint Cx;
        public uint Cy;
        public uint NumBackbuffers;
        public int Format;
        public int ZsFormat;
        public uint Adapter;
    }

    /// <summary>
    /// Creates a display that renders into a native window (Windows/macOS layout).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDisplayHandle obs_display_create(ref GsInitData data, uint backgroundColor);

    /// <summary>
    /// Creates a display that renders into a native window (Linux/X11 layout).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDisplayHandle obs_display_create_x11(ref GsInitDataX11 data, uint backgroundColor);

    /// <summary>
    /// Destroys a display.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_display_destroy(ObsDisplayHandle display);

    /// <summary>
    /// Changes the size of the display's swap chain.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_resize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_display_resize(ObsDisplayHandle display, uint cx, uint cy);

    /// <summary>
    /// Gets the current size of the display.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_size")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_display_size(ObsDisplayHandle display, out uint width, out uint height);

    /// <summary>
    /// Updates the color space of the display (e.g. after an HDR setting change).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_update_color_space")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_display_update_color_space(ObsDisplayHandle display);

    /// <summary>
    /// Adds a draw callback invoked each frame on the graphics thread.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_add_draw_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_display_add_draw_callback(ObsDisplayHandle display, DrawCallback draw, nint param);

    /// <summary>
    /// Removes a previously added draw callback. Blocks until any in-progress
    /// invocation of the callback has finished.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_remove_draw_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_display_remove_draw_callback(ObsDisplayHandle display, DrawCallback draw, nint param);

    /// <summary>
    /// Enables or disables rendering of the display.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_set_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_display_set_enabled(ObsDisplayHandle display, byte enable);

    /// <summary>
    /// Gets whether the display is enabled.
    /// </summary>
    public static bool obs_display_enabled(ObsDisplayHandle display) => obs_display_enabled_native(display) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_display_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_display_enabled_native(ObsDisplayHandle display);

    /// <summary>
    /// Sets the background (letterbox) color of the display.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_display_set_background_color")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_display_set_background_color(ObsDisplayHandle display, uint color);

    /// <summary>
    /// Renders the main output texture. Must be called from within a draw callback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_render_main_texture")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_render_main_texture();
}
