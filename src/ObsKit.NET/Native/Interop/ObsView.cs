using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS views (obs_view_t): auxiliary channel sets that can be rendered
/// on their own or turned into an extra video mix.
/// </summary>
internal static partial class ObsView
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    /// <summary>
    /// Creates a view with empty channels (destroy with obs_view_destroy).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_view_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_view_create();

    /// <summary>
    /// Destroys a view, releasing its channel sources. Call obs_view_remove first if a mix was added.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_view_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_view_destroy(nint view);

    /// <summary>
    /// Assigns a source to a channel (the view takes its own reference; null clears).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_view_set_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_view_set_source(nint view, uint channel, ObsSourceHandle source);

    /// <summary>
    /// Gets the source of a channel (adds a reference; release when done), or null.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_view_get_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_view_get_source(nint view, uint channel);

    /// <summary>
    /// Renders the view's channels with the current graphics context (graphics thread only).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_view_render")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_view_render(nint view);

    /// <summary>
    /// Creates a video mix that renders this view with the given video settings, returning its
    /// video output (owned by the core; freed after obs_view_remove).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_view_add2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoHandle obs_view_add2(nint view, ref ObsVideoInfo ovi);

    /// <summary>
    /// Detaches the view from its mixes; the core frees them on the next video tick.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_view_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_view_remove(nint view);

    /// <summary>
    /// Callback for <c>obs_view_enum_video_info</c>; <paramref name="ovi"/> points at an
    /// <see cref="ObsVideoInfo"/>. Return 1 to continue.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumVideoInfoCallback(nint param, nint ovi);

    [LibraryImport(Lib, EntryPoint = "obs_view_enum_video_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_view_enum_video_info(nint view, EnumVideoInfoCallback callback, nint param);
}
