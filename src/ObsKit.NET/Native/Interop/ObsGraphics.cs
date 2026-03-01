using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS graphics functions (texture rendering, staging surfaces).
/// </summary>
internal static partial class ObsGraphics
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Graphics Context

    /// <summary>
    /// Locks the graphics mutex. Must be called before any gs_* operations.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enter_graphics")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enter_graphics();

    /// <summary>
    /// Unlocks the graphics mutex. Must be called after gs_* operations complete.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_leave_graphics")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_leave_graphics();

    #endregion

    #region Texture Render

    /// <summary>
    /// Creates a texture render target.
    /// </summary>
    /// <param name="format">The color format (GS_BGRA = 5).</param>
    /// <param name="zformat">The depth/stencil format (GS_ZS_NONE = 0).</param>
    [LibraryImport(Lib, EntryPoint = "gs_texrender_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial GsTexRenderHandle gs_texrender_create(int format, int zformat);

    /// <summary>
    /// Destroys a texture render target.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_texrender_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_texrender_destroy(GsTexRenderHandle texrender);

    /// <summary>
    /// Begins rendering to a texture render target.
    /// </summary>
    public static bool gs_texrender_begin(GsTexRenderHandle texrender, uint cx, uint cy)
        => gs_texrender_begin_native(texrender, cx, cy) != 0;

    [LibraryImport(Lib, EntryPoint = "gs_texrender_begin")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte gs_texrender_begin_native(GsTexRenderHandle texrender, uint cx, uint cy);

    /// <summary>
    /// Ends rendering to a texture render target.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_texrender_end")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_texrender_end(GsTexRenderHandle texrender);

    /// <summary>
    /// Gets the rendered texture from a texture render target.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_texrender_get_texture")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial GsTextureHandle gs_texrender_get_texture(GsTexRenderHandle texrender);

    #endregion

    #region Staging Surface

    /// <summary>
    /// Creates a CPU-accessible staging surface.
    /// </summary>
    /// <param name="cx">Width in pixels.</param>
    /// <param name="cy">Height in pixels.</param>
    /// <param name="colorFormat">The color format (GS_BGRA = 5).</param>
    [LibraryImport(Lib, EntryPoint = "gs_stagesurface_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial GsStageSurfaceHandle gs_stagesurface_create(uint cx, uint cy, int colorFormat);

    /// <summary>
    /// Destroys a staging surface.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_stagesurface_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_stagesurface_destroy(GsStageSurfaceHandle stagesurface);

    /// <summary>
    /// Copies a texture to a staging surface for CPU access.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_stage_texture")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_stage_texture(GsStageSurfaceHandle dst, GsTextureHandle src);

    /// <summary>
    /// Maps a staging surface to a CPU-accessible byte pointer.
    /// </summary>
    /// <param name="stagesurface">The staging surface to map.</param>
    /// <param name="data">Output pointer to pixel data.</param>
    /// <param name="linesize">Output row pitch in bytes.</param>
    /// <returns>True if mapping succeeded.</returns>
    public static bool gs_stagesurface_map(GsStageSurfaceHandle stagesurface, out nint data, out uint linesize)
        => gs_stagesurface_map_native(stagesurface, out data, out linesize) != 0;

    [LibraryImport(Lib, EntryPoint = "gs_stagesurface_map")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte gs_stagesurface_map_native(GsStageSurfaceHandle stagesurface, out nint data, out uint linesize);

    /// <summary>
    /// Unmaps a previously mapped staging surface.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_stagesurface_unmap")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_stagesurface_unmap(GsStageSurfaceHandle stagesurface);

    #endregion

    #region Render State

    /// <summary>
    /// Clears the current render target.
    /// </summary>
    /// <param name="clearFlags">Flags for what to clear (GS_CLEAR_COLOR = 1).</param>
    /// <param name="color">Pointer to vec4 color. Must NOT be null when GS_CLEAR_COLOR is set.</param>
    /// <param name="depth">Depth clear value.</param>
    /// <param name="stencil">Stencil clear value.</param>
    [LibraryImport(Lib, EntryPoint = "gs_clear")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static unsafe partial void gs_clear(uint clearFlags, float* color, float depth, byte stencil);

    /// <summary>
    /// Sets the orthographic projection matrix.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_ortho")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_ortho(float left, float right, float top, float bottom, float znear, float zfar);

    /// <summary>
    /// Saves the current blend state onto a stack.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_blend_state_push")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_blend_state_push();

    /// <summary>
    /// Restores the previous blend state from the stack.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "gs_blend_state_pop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_blend_state_pop();

    /// <summary>
    /// Sets the blend function for source and destination.
    /// </summary>
    /// <param name="src">Source blend factor.</param>
    /// <param name="dest">Destination blend factor.</param>
    [LibraryImport(Lib, EntryPoint = "gs_blend_function")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void gs_blend_function(int src, int dest);

    #endregion

    #region Source Rendering

    /// <summary>
    /// Renders a source's video output. Must be called within a graphics context.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_source_video_render")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_source_video_render(ObsSourceHandle source);

    #endregion
}
