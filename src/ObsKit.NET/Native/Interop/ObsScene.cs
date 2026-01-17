using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS scene functions.
/// </summary>
internal static partial class ObsScene
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Scene Creation and Release

    /// <summary>
    /// Creates a new scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_scene_create(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Creates a private scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_create_private")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_scene_create_private(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Releases a scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_scene_release(ObsSceneHandle scene);

    /// <summary>
    /// Adds a reference to a scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_addref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_scene_addref(ObsSceneHandle scene);

    /// <summary>
    /// Gets the scene as a source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_get_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_scene_get_source(ObsSceneHandle scene);

    /// <summary>
    /// Gets a scene from a source.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_from_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_scene_from_source(ObsSourceHandle source);

    #endregion

    #region Scene Items

    /// <summary>
    /// Adds a source to a scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_add")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneItemHandle obs_scene_add(ObsSceneHandle scene, ObsSourceHandle source);

    /// <summary>
    /// Finds a source in a scene by name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_find_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneItemHandle obs_scene_find_source(
        ObsSceneHandle scene,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Finds a source in a scene recursively.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_find_source_recursive")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneItemHandle obs_scene_find_source_recursive(
        ObsSceneHandle scene,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Finds a scene item by ID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_find_sceneitem_by_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneItemHandle obs_scene_find_sceneitem_by_id(ObsSceneHandle scene, long id);

    /// <summary>
    /// Callback for enumerating scene items.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumSceneItemCallback(ObsSceneHandle scene, ObsSceneItemHandle item, nint data);

    /// <summary>
    /// Enumerates scene items.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_enum_items")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_scene_enum_items(ObsSceneHandle scene, EnumSceneItemCallback callback, nint data);

    #endregion

    #region Scene Item Operations

    /// <summary>
    /// Adds a reference to a scene item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_addref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_addref(ObsSceneItemHandle item);

    /// <summary>
    /// Releases a scene item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_release(ObsSceneItemHandle item);

    /// <summary>
    /// Removes a scene item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_remove")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_remove(ObsSceneItemHandle item);

    /// <summary>
    /// Gets the scene for a scene item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_scene")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_sceneitem_get_scene(ObsSceneItemHandle item);

    /// <summary>
    /// Gets the source for a scene item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_sceneitem_get_source(ObsSceneItemHandle item);

    /// <summary>
    /// Gets the scene item ID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial long obs_sceneitem_get_id(ObsSceneItemHandle item);

    #endregion

    #region Visibility and State

    /// <summary>
    /// Gets scene item visibility.
    /// </summary>
    public static bool obs_sceneitem_visible(ObsSceneItemHandle item) => obs_sceneitem_visible_native(item) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_visible")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_sceneitem_visible_native(ObsSceneItemHandle item);

    /// <summary>
    /// Sets scene item visibility.
    /// </summary>
    public static bool obs_sceneitem_set_visible(ObsSceneItemHandle item, bool visible)
        => obs_sceneitem_set_visible_native(item, visible ? (byte)1 : (byte)0) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_visible")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_sceneitem_set_visible_native(ObsSceneItemHandle item, byte visible);

    /// <summary>
    /// Gets scene item locked state.
    /// </summary>
    public static bool obs_sceneitem_locked(ObsSceneItemHandle item) => obs_sceneitem_locked_native(item) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_locked")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_sceneitem_locked_native(ObsSceneItemHandle item);

    /// <summary>
    /// Sets scene item locked state.
    /// </summary>
    public static bool obs_sceneitem_set_locked(ObsSceneItemHandle item, bool locked)
        => obs_sceneitem_set_locked_native(item, locked ? (byte)1 : (byte)0) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_locked")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_sceneitem_set_locked_native(ObsSceneItemHandle item, byte locked);

    /// <summary>
    /// Gets scene item selected state.
    /// </summary>
    public static bool obs_sceneitem_selected(ObsSceneItemHandle item) => obs_sceneitem_selected_native(item) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_selected")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_sceneitem_selected_native(ObsSceneItemHandle item);

    /// <summary>
    /// Sets scene item selected state.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_select")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_select(ObsSceneItemHandle item, byte select);

    #endregion

    #region Transform

    /// <summary>
    /// Sets scene item position.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_pos")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_pos(ObsSceneItemHandle item, ref Vec2 pos);

    /// <summary>
    /// Gets scene item position.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_pos")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_get_pos(ObsSceneItemHandle item, out Vec2 pos);

    /// <summary>
    /// Sets scene item rotation.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_rot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_rot(ObsSceneItemHandle item, float rotDeg);

    /// <summary>
    /// Gets scene item rotation.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_rot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_sceneitem_get_rot(ObsSceneItemHandle item);

    /// <summary>
    /// Sets scene item scale.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_scale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_scale(ObsSceneItemHandle item, ref Vec2 scale);

    /// <summary>
    /// Gets scene item scale.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_scale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_get_scale(ObsSceneItemHandle item, out Vec2 scale);

    /// <summary>
    /// Sets scene item alignment.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_alignment")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_alignment(ObsSceneItemHandle item, uint alignment);

    /// <summary>
    /// Gets scene item alignment.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_alignment")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_sceneitem_get_alignment(ObsSceneItemHandle item);

    #endregion

    #region Bounds

    /// <summary>
    /// Sets scene item bounds type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_bounds_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_bounds_type(ObsSceneItemHandle item, ObsBoundsType type);

    /// <summary>
    /// Gets scene item bounds type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_bounds_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsBoundsType obs_sceneitem_get_bounds_type(ObsSceneItemHandle item);

    /// <summary>
    /// Sets scene item bounds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_bounds")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_bounds(ObsSceneItemHandle item, ref Vec2 bounds);

    /// <summary>
    /// Gets scene item bounds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_bounds")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_get_bounds(ObsSceneItemHandle item, out Vec2 bounds);

    #endregion

    #region Crop

    /// <summary>
    /// Sets scene item crop.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_crop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_crop(ObsSceneItemHandle item, ref ObsSceneItemCrop crop);

    /// <summary>
    /// Gets scene item crop.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_crop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_get_crop(ObsSceneItemHandle item, out ObsSceneItemCrop crop);

    #endregion

    #region Order

    /// <summary>
    /// Sets scene item order.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_order")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_order(ObsSceneItemHandle item, ObsOrderMovement movement);

    /// <summary>
    /// Sets scene item order position.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_order_position")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_order_position(ObsSceneItemHandle item, int position);

    /// <summary>
    /// Gets scene item order position.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_order_position")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_sceneitem_get_order_position(ObsSceneItemHandle item);

    #endregion
}
