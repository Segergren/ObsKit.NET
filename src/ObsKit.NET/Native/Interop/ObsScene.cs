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

    #region Item Update Control / Private Settings

    /// <summary>
    /// Begins deferring transform update signals for a scene item (batch multiple
    /// transform changes into one "item_transform" signal).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_defer_update_begin")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_defer_update_begin(ObsSceneItemHandle item);

    /// <summary>
    /// Ends deferring transform updates and emits the update signal.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_defer_update_end")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_defer_update_end(ObsSceneItemHandle item);

    /// <summary>
    /// Forces an immediate recalculation of the item's draw transform.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_force_update_transform")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_force_update_transform(ObsSceneItemHandle item);

    /// <summary>
    /// Sets whether the source is cropped to the bounding box (bounds-type transforms).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_bounds_crop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_bounds_crop(ObsSceneItemHandle item, byte crop);

    /// <summary>
    /// Gets whether the source is cropped to the bounding box.
    /// </summary>
    public static bool obs_sceneitem_get_bounds_crop(ObsSceneItemHandle item)
        => obs_sceneitem_get_bounds_crop_native(item) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_bounds_crop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_sceneitem_get_bounds_crop_native(ObsSceneItemHandle item);

    /// <summary>
    /// Gets the item's private settings (an incremented obs_data reference).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_private_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_sceneitem_get_private_settings(ObsSceneItemHandle item);

    #endregion

    #region Transforms, Ids and Group Resize

    /// <summary>
    /// Gets the item's draw transform (item-local pixels to canvas pixels, including
    /// crop, scale, rotation, position and bounds).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_draw_transform")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_get_draw_transform(ObsSceneItemHandle item, out Matrix4 transform);

    /// <summary>
    /// Gets the item's bounding-box transform (unit square to the item's on-canvas box).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_box_transform")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_get_box_transform(ObsSceneItemHandle item, out Matrix4 transform);

    /// <summary>
    /// Gets the item's bounding-box size in canvas pixels (after scale/bounds, before rotation).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_get_box_scale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_get_box_scale(ObsSceneItemHandle item, out Vec2 scale);

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_set_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_set_id(ObsSceneItemHandle item, long id);

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_defer_group_resize_begin")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_defer_group_resize_begin(ObsSceneItemHandle item);

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_defer_group_resize_end")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_defer_group_resize_end(ObsSceneItemHandle item);

    /// <summary>
    /// Ungroups, optionally without emitting the reorder/refresh signals.
    /// </summary>
    public static void obs_sceneitem_group_ungroup2(ObsSceneItemHandle group, bool signal)
        => obs_sceneitem_group_ungroup2_native(group, signal ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_group_ungroup2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void obs_sceneitem_group_ungroup2_native(ObsSceneItemHandle group, byte signal);

    #endregion

    #region Show/Hide Transitions

    /// <summary>
    /// Plays the item's show (visible=true) or hide transition, if one is set.
    /// </summary>
    public static void obs_sceneitem_do_transition(ObsSceneItemHandle item, bool visible)
        => obs_sceneitem_do_transition_native(item, visible ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_do_transition")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void obs_sceneitem_do_transition_native(ObsSceneItemHandle item, byte visible);

    /// <summary>
    /// Loads a show/hide transition (type, settings, duration) from data saved by
    /// obs_sceneitem_transition_save.
    /// </summary>
    public static void obs_sceneitem_transition_load(ObsSceneItemHandle item, ObsDataHandle data, bool show)
        => obs_sceneitem_transition_load_native(item, data, show ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_transition_load")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void obs_sceneitem_transition_load_native(ObsSceneItemHandle item, ObsDataHandle data, byte show);

    /// <summary>
    /// Saves the show/hide transition into a new data object (release when done).
    /// </summary>
    public static ObsDataHandle obs_sceneitem_transition_save(ObsSceneItemHandle item, bool show)
        => obs_sceneitem_transition_save_native(item, show ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_transition_save")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ObsDataHandle obs_sceneitem_transition_save_native(ObsSceneItemHandle item, byte show);

    #endregion

    #region Item Persistence

    /// <summary>
    /// Appends the item's serialized form (source name, transform, crop, visibility, ...)
    /// to an array.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitem_save")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitem_save(ObsSceneItemHandle item, ObsDataArrayHandle array);

    /// <summary>
    /// Adds items to a scene from an array produced by obs_sceneitem_save (sources are
    /// looked up by name).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_sceneitems_add")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_sceneitems_add(ObsSceneHandle scene, ObsDataArrayHandle array);

    #endregion

    #region Groups, Ordering and Atomic Updates

    /// <summary>
    /// Creates a group containing <paramref name="items"/> (pointer to <paramref name="count"/>
    /// obs_sceneitem_t pointers, which are moved into the group), returning the scene's own
    /// (borrowed) reference.
    /// </summary>
    public static ObsSceneItemHandle obs_scene_insert_group2(ObsSceneHandle scene, string name, nint items, nuint count, bool signal)
        => obs_scene_insert_group2_native(scene, name, items, count, signal ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_scene_insert_group2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ObsSceneItemHandle obs_scene_insert_group2_native(
        ObsSceneHandle scene,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        nint items,
        nuint count,
        byte signal);

    public static bool obs_scene_is_group(ObsSceneHandle scene) => obs_scene_is_group_native(scene) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_scene_is_group")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_scene_is_group_native(ObsSceneHandle scene);

    /// <summary>
    /// Gets the scene behind a group source (borrowed pointer, no reference added), or null
    /// if the source is not a group.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_group_from_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSceneHandle obs_group_from_source(ObsSourceHandle source);

    /// <summary>
    /// Reorders items including group membership: <paramref name="itemOrder"/> points at
    /// <paramref name="count"/> <see cref="ObsSceneItemOrderInfoNative"/> entries listing, for
    /// each item, its group (or null) in the desired order.
    /// </summary>
    public static bool obs_scene_reorder_items2(ObsSceneHandle scene, nint itemOrder, nuint count)
        => obs_scene_reorder_items2_native(scene, itemOrder, count) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_scene_reorder_items2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_scene_reorder_items2_native(ObsSceneHandle scene, nint itemOrder, nuint count);

    /// <summary>
    /// Callback for <c>obs_scene_atomic_update</c>, invoked with the scene fully locked.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SceneAtomicUpdateCallback(nint data, ObsSceneHandle scene);

    [LibraryImport(Lib, EntryPoint = "obs_scene_atomic_update")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_scene_atomic_update(ObsSceneHandle scene, SceneAtomicUpdateCallback callback, nint data);

    /// <summary>
    /// Removes items whose sources have been removed.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_prune_sources")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_scene_prune_sources(ObsSceneHandle scene);

    #endregion

    #region Transform States

    /// <summary>
    /// Saves the transform of the scene's items (all, or only selected ones) into a new data
    /// object (release when done) that obs_scene_load_transform_states accepts as JSON.
    /// </summary>
    public static ObsDataHandle obs_scene_save_transform_states(ObsSceneHandle scene, bool allItems)
        => obs_scene_save_transform_states_native(scene, allItems ? (byte)1 : (byte)0);

    [LibraryImport(Lib, EntryPoint = "obs_scene_save_transform_states")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial ObsDataHandle obs_scene_save_transform_states_native(ObsSceneHandle scene, byte allItems);

    /// <summary>
    /// Restores item transforms from the JSON produced by obs_scene_save_transform_states
    /// (scenes and items are looked up by name/id).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_scene_load_transform_states")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_scene_load_transform_states([MarshalUsing(typeof(Utf8StringMarshaler))] string json);

    #endregion
}
