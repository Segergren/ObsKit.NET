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
    /// Duplicates a scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_duplicate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_scene_duplicate(ObsSceneHandle scene,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsSceneDuplicateType type);

    /// <summary>
    /// Releases a scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_scene_release(ObsSceneHandle scene);

    /// <summary>
    /// Returns an owning reference to the scene (the same handle), or null if the scene is
    /// being destroyed.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_get_ref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_scene_get_ref(ObsSceneHandle scene);

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

    /// <summary>
    /// Sets the complete z-order of a scene's items. <paramref name="itemOrder"/> must
    /// contain exactly the scene's current items (top to bottom); returns false if the set
    /// does not match or the order is unchanged.
    /// </summary>
    public static bool obs_scene_reorder_items(ObsSceneHandle scene, nint[] itemOrder)
    {
        var gch = GCHandle.Alloc(itemOrder, GCHandleType.Pinned);
        try
        {
            return obs_scene_reorder_items_native(scene, gch.AddrOfPinnedObject(), (nuint)itemOrder.Length) != 0;
        }
        finally
        {
            gch.Free();
        }
    }

    [LibraryImport(Lib, EntryPoint = "obs_scene_reorder_items")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_scene_reorder_items_native(ObsSceneHandle scene, nint itemOrder, nuint count);

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

    /// <summary>Sets how the source is aligned within its bounding box.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_bounds_alignment")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_bounds_alignment(ObsSceneItemHandle item, uint alignment);

    /// <summary>Gets how the source is aligned within its bounding box.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_bounds_alignment")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_sceneitem_get_bounds_alignment(ObsSceneItemHandle item);

    /// <summary>Reads the item's full transform into <paramref name="info"/>.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_info2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_get_info2(ObsSceneItemHandle item, out ObsTransformInfo info);

    /// <summary>Applies a full transform to the item.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_info2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_info2(ObsSceneItemHandle item, ref ObsTransformInfo info);

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

    #region Blending and Scaling

    /// <summary>
    /// Sets the scale filter used when the item is scaled.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_scale_filter")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_scale_filter(ObsSceneItemHandle item, ObsScaleType filter);

    /// <summary>
    /// Gets the scale filter used when the item is scaled.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_scale_filter")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsScaleType obs_sceneitem_get_scale_filter(ObsSceneItemHandle item);

    /// <summary>
    /// Sets the blending method (sRGB handling) of the item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_blending_method")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_blending_method(ObsSceneItemHandle item, ObsBlendingMethod method);

    /// <summary>
    /// Gets the blending method (sRGB handling) of the item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_blending_method")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsBlendingMethod obs_sceneitem_get_blending_method(ObsSceneItemHandle item);

    /// <summary>
    /// Sets the blending mode of the item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_blending_mode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_blending_mode(ObsSceneItemHandle item, ObsBlendingType type);

    /// <summary>
    /// Gets the blending mode of the item.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_blending_mode")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsBlendingType obs_sceneitem_get_blending_mode(ObsSceneItemHandle item);

    #endregion

    #region Show/Hide Transitions

    /// <summary>
    /// Sets the show (true) or hide (false) transition of the item. The item takes its own
    /// reference to the transition source; pass null to clear.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_transition")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_transition(ObsSceneItemHandle item, byte show, ObsSourceHandle transition);

    /// <summary>
    /// Gets the show (true) or hide (false) transition of the item (not an added reference).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_transition")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_sceneitem_get_transition(ObsSceneItemHandle item, byte show);

    /// <summary>
    /// Sets the show/hide transition duration in milliseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_transition_duration")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_transition_duration(ObsSceneItemHandle item, byte show, uint durationMs);

    /// <summary>
    /// Gets the show/hide transition duration in milliseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_transition_duration")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_sceneitem_get_transition_duration(ObsSceneItemHandle item, byte show);

    #endregion

    #region Groups

    /// <summary>
    /// Creates an empty group in the scene. The returned scene item is owned by the
    /// scene (same ownership as obs_scene_add). Passes signal=true to emit item_add.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_add_group2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneItemHandle obs_scene_add_group2(
        ObsSceneHandle scene,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalAs(UnmanagedType.U1)] bool signal);

    /// <summary>
    /// Gets a group scene item by name. Does NOT increment the reference count.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_get_group")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneItemHandle obs_scene_get_group(
        ObsSceneHandle scene,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets the parent group of a sub-item, or null. Does NOT increment the reference count.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_group")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneItemHandle obs_sceneitem_get_group(ObsSceneHandle scene, ObsSceneItemHandle item);

    /// <summary>
    /// Returns true if the scene item is a group.
    /// </summary>
    public static bool obs_sceneitem_is_group(ObsSceneItemHandle item) => obs_sceneitem_is_group_native(item) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_is_group")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_sceneitem_is_group_native(ObsSceneItemHandle item);

    /// <summary>
    /// Gets the inner scene backing a group, or null. Does NOT increment the reference count.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_group_get_scene")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_sceneitem_group_get_scene(ObsSceneItemHandle group);

    /// <summary>
    /// Disbands a group, moving its items back into the parent scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_group_ungroup")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_group_ungroup(ObsSceneItemHandle group);

    /// <summary>
    /// Moves an existing scene item into a group.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_group_add_item")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_group_add_item(ObsSceneItemHandle group, ObsSceneItemHandle item);

    /// <summary>
    /// Removes a scene item from a group, moving it back into the parent scene.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_group_remove_item")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_group_remove_item(ObsSceneItemHandle group, ObsSceneItemHandle item);

    /// <summary>
    /// Enumerates the items within a group (delegates to obs_scene_enum_items, which
    /// references each item for the duration of the callback).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_group_enum_items")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_group_enum_items(ObsSceneItemHandle group, EnumSceneItemCallback callback, nint data);

    #endregion
}
