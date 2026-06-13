using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for obs_canvas functions (multiple video mixes, OBS 31+).
/// </summary>
internal static partial class ObsCanvas
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    /// <summary>
    /// Gets a strong reference to the main canvas.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_main_canvas")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsCanvasHandle obs_get_main_canvas();

    /// <summary>
    /// Creates a new canvas with its own video settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsCanvasHandle obs_canvas_create(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ref ObsVideoInfo ovi,
        uint flags);

    /// <summary>
    /// Marks the canvas as removed and signals references to release.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_canvas_remove(ObsCanvasHandle canvas);

    /// <summary>
    /// Checks if the canvas is marked as removed.
    /// </summary>
    public static bool obs_canvas_removed(ObsCanvasHandle canvas) => obs_canvas_removed_native(canvas) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_canvas_removed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_canvas_removed_native(ObsCanvasHandle canvas);

    /// <summary>
    /// Adds a strong canvas reference. Returns null if the canvas is being destroyed.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_get_ref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsCanvasHandle obs_canvas_get_ref(ObsCanvasHandle canvas);

    /// <summary>
    /// Releases a strong canvas reference.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_canvas_release(ObsCanvasHandle canvas);

    /// <summary>
    /// Sets the canvas name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_set_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_canvas_set_name(ObsCanvasHandle canvas,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets the canvas name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_get_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_canvas_get_name(ObsCanvasHandle canvas);

    /// <summary>
    /// Gets the canvas UUID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_get_uuid")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_canvas_get_uuid(ObsCanvasHandle canvas);

    /// <summary>
    /// Gets the canvas flags.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_get_flags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_canvas_get_flags(ObsCanvasHandle canvas);

    /// <summary>
    /// Sets the source rendered on a channel of this canvas.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_set_channel")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_canvas_set_channel(ObsCanvasHandle canvas, uint channel, ObsSourceHandle source);

    /// <summary>
    /// Gets the source on a channel of this canvas (incremented reference).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_get_channel")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_canvas_get_channel(ObsCanvasHandle canvas, uint channel);

    /// <summary>
    /// Creates a scene attached to this canvas.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_scene_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_canvas_scene_create(ObsCanvasHandle canvas,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Moves a scene to another canvas, detaching it from the previous one
    /// (the scene name is deduplicated if needed).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_move_scene")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_canvas_move_scene(ObsSceneHandle scene, ObsCanvasHandle dst);

    /// <summary>
    /// Resets the canvas's video mix.
    /// </summary>
    public static bool obs_canvas_reset_video(ObsCanvasHandle canvas, ref ObsVideoInfo ovi)
        => obs_canvas_reset_video_native(canvas, ref ovi) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_canvas_reset_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_canvas_reset_video_native(ObsCanvasHandle canvas, ref ObsVideoInfo ovi);

    /// <summary>
    /// Checks if the canvas has video configured.
    /// </summary>
    public static bool obs_canvas_has_video(ObsCanvasHandle canvas) => obs_canvas_has_video_native(canvas) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_canvas_has_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_canvas_has_video_native(ObsCanvasHandle canvas);

    /// <summary>
    /// Gets the canvas's video output handle.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_canvas_get_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoHandle obs_canvas_get_video(ObsCanvasHandle canvas);

    /// <summary>
    /// Gets the canvas's video settings.
    /// </summary>
    public static bool obs_canvas_get_video_info(ObsCanvasHandle canvas, ref ObsVideoInfo ovi)
        => obs_canvas_get_video_info_native(canvas, ref ovi) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_canvas_get_video_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_canvas_get_video_info_native(ObsCanvasHandle canvas, ref ObsVideoInfo ovi);

    /// <summary>
    /// Renders the canvas texture. Must be called from within a display draw callback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_render_canvas_texture")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_render_canvas_texture(ObsCanvasHandle canvas);
}
