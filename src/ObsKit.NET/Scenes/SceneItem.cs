using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Scenes;

/// <summary>
/// Type ids of the transitions bundled with OBS (obs-transitions plugin),
/// usable as scene item show/hide transitions.
/// </summary>
public static class TransitionTypes
{
    /// <summary>Instant cut.</summary>
    public const string Cut = "cut_transition";
    /// <summary>Cross-fade.</summary>
    public const string Fade = "fade_transition";
    /// <summary>Swipe.</summary>
    public const string Swipe = "swipe_transition";
    /// <summary>Slide.</summary>
    public const string Slide = "slide_transition";
    /// <summary>Fade through a solid color.</summary>
    public const string FadeToColor = "fade_to_color_transition";
    /// <summary>Luma wipe.</summary>
    public const string Wipe = "wipe_transition";
    /// <summary>Stinger (media file) transition.</summary>
    public const string Stinger = "obs_stinger_transition";
}

/// <summary>
/// Represents an item in a scene (a source positioned within a scene).
/// </summary>
public sealed class SceneItem : ObsObject
{
    private readonly Scene _scene;
    private bool _removed;

    internal SceneItem(ObsSceneItemHandle handle, Scene scene, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
        _scene = scene;
    }

    internal new ObsSceneItemHandle Handle => (ObsSceneItemHandle)base.Handle;

    /// <summary>Gets the parent scene.</summary>
    public Scene Scene => _scene;

    /// <summary>Gets the source associated with this scene item.</summary>
    public Source Source
    {
        get
        {
            var sourceHandle = ObsScene.obs_sceneitem_get_source(Handle);
            // obs_sceneitem_get_source returns a borrowed pointer; take an owning ref.
            var refHandle = ObsSource.obs_source_get_ref(sourceHandle);
            return new Source(refHandle, ownsHandle: true);
        }
    }

    /// <summary>Gets the unique ID of this scene item.</summary>
    public long Id => ObsScene.obs_sceneitem_get_id(Handle);

    /// <summary>Gets or sets whether this item is visible.</summary>
    public bool IsVisible
    {
        get => ObsScene.obs_sceneitem_visible(Handle);
        set => ObsScene.obs_sceneitem_set_visible(Handle, value);
    }

    /// <summary>
    /// Gets or sets whether this item is locked (cannot be moved/resized in UI).
    /// </summary>
    public bool IsLocked
    {
        get => ObsScene.obs_sceneitem_locked(Handle);
        set => ObsScene.obs_sceneitem_set_locked(Handle, value);
    }

    /// <summary>Gets or sets whether this item is selected.</summary>
    public bool IsSelected
    {
        get => ObsScene.obs_sceneitem_selected(Handle);
        set => ObsScene.obs_sceneitem_select(Handle, value ? (byte)1 : (byte)0);
    }

    #region Position and Transform

    /// <summary>Gets or sets the position.</summary>
    public Vec2 Position
    {
        get
        {
            ObsScene.obs_sceneitem_get_pos(Handle, out var pos);
            return pos;
        }
        set
        {
            var pos = value;
            ObsScene.obs_sceneitem_set_pos(Handle, ref pos);
        }
    }

    /// <summary>
    /// Gets or sets the rotation in degrees.
    /// </summary>
    public float Rotation
    {
        get => ObsScene.obs_sceneitem_get_rot(Handle);
        set => ObsScene.obs_sceneitem_set_rot(Handle, value);
    }

    /// <summary>Gets or sets the scale.</summary>
    public Vec2 Scale
    {
        get
        {
            ObsScene.obs_sceneitem_get_scale(Handle, out var scale);
            return scale;
        }
        set
        {
            var scale = value;
            ObsScene.obs_sceneitem_set_scale(Handle, ref scale);
        }
    }

    /// <summary>
    /// Gets or sets the position alignment — which point of the source the
    /// <see cref="Position"/> refers to. The value is a bitmask of <see cref="ObsAlignment"/>
    /// flags (e.g. <c>(uint)ObsAlignment.TopLeft</c>).
    /// </summary>
    public uint Alignment
    {
        get => ObsScene.obs_sceneitem_get_alignment(Handle);
        set => ObsScene.obs_sceneitem_set_alignment(Handle, value);
    }

    #endregion

    #region Bounds

    /// <summary>Gets or sets the bounds type.</summary>
    public ObsBoundsType BoundsType
    {
        get => ObsScene.obs_sceneitem_get_bounds_type(Handle);
        set => ObsScene.obs_sceneitem_set_bounds_type(Handle, value);
    }

    /// <summary>Gets or sets the bounds size.</summary>
    public Vec2 Bounds
    {
        get
        {
            ObsScene.obs_sceneitem_get_bounds(Handle, out var bounds);
            return bounds;
        }
        set
        {
            var bounds = value;
            ObsScene.obs_sceneitem_set_bounds(Handle, ref bounds);
        }
    }

    /// <summary>
    /// Gets or sets how the source is aligned within its bounding box (when
    /// <see cref="BoundsType"/> is not <see cref="ObsBoundsType.None"/>). For example,
    /// <see cref="ObsAlignment.TopLeft"/> pins the source to the top-left of its box.
    /// </summary>
    public ObsAlignment BoundsAlignment
    {
        get => (ObsAlignment)ObsScene.obs_sceneitem_get_bounds_alignment(Handle);
        set => ObsScene.obs_sceneitem_set_bounds_alignment(Handle, (uint)value);
    }

    /// <summary>
    /// Reads this item's complete transform (position, rotation, scale, alignment, and
    /// bounds) in a single call.
    /// </summary>
    public ObsTransformInfo GetTransform()
    {
        ObsScene.obs_sceneitem_get_info2(Handle, out var info);
        return info;
    }

    /// <summary>
    /// Applies a complete transform to this item atomically — avoids the intermediate
    /// states of setting position, scale, bounds, etc. individually.
    /// </summary>
    /// <param name="transform">The transform to apply.</param>
    public void SetTransform(ObsTransformInfo transform)
        => ObsScene.obs_sceneitem_set_info2(Handle, ref transform);

    #endregion

    #region Crop

    /// <summary>Gets or sets the crop values.</summary>
    public ObsSceneItemCrop Crop
    {
        get
        {
            ObsScene.obs_sceneitem_get_crop(Handle, out var crop);
            return crop;
        }
        set
        {
            var crop = value;
            ObsScene.obs_sceneitem_set_crop(Handle, ref crop);
        }
    }

    #endregion

    #region Blending and Scaling

    /// <summary>
    /// Gets or sets the scale filter used when this item is scaled
    /// (e.g. a webcam overlay rendered smaller than its native resolution).
    /// </summary>
    public ObsScaleType ScaleFilter
    {
        get => ObsScene.obs_sceneitem_get_scale_filter(Handle);
        set => ObsScene.obs_sceneitem_set_scale_filter(Handle, value);
    }

    /// <summary>Gets or sets the blending mode used to composite this item.</summary>
    public ObsBlendingType BlendingMode
    {
        get => ObsScene.obs_sceneitem_get_blending_mode(Handle);
        set => ObsScene.obs_sceneitem_set_blending_mode(Handle, value);
    }

    /// <summary>Gets or sets the blending method (sRGB handling) of this item.</summary>
    public ObsBlendingMethod BlendingMethod
    {
        get => ObsScene.obs_sceneitem_get_blending_method(Handle);
        set => ObsScene.obs_sceneitem_set_blending_method(Handle, value);
    }

    #endregion

    #region Show/Hide Transitions

    /// <summary>
    /// Sets a transition played when the item becomes visible
    /// (e.g. fade a webcam overlay in instead of popping).
    /// </summary>
    /// <param name="transitionTypeId">A transition type id, e.g. <see cref="TransitionTypes.Fade"/>.</param>
    /// <param name="duration">The transition duration (default 300 ms).</param>
    public SceneItem SetShowTransition(string transitionTypeId, TimeSpan? duration = null)
        => SetTransition(transitionTypeId, duration, show: true);

    /// <summary>
    /// Sets a transition played when the item is hidden.
    /// </summary>
    /// <param name="transitionTypeId">A transition type id, e.g. <see cref="TransitionTypes.Fade"/>.</param>
    /// <param name="duration">The transition duration (default 300 ms).</param>
    public SceneItem SetHideTransition(string transitionTypeId, TimeSpan? duration = null)
        => SetTransition(transitionTypeId, duration, show: false);

    /// <summary>Removes the show transition.</summary>
    public SceneItem ClearShowTransition()
    {
        ObsScene.obs_sceneitem_set_transition(Handle, 1, ObsSourceHandle.Null);
        return this;
    }

    /// <summary>Removes the hide transition.</summary>
    public SceneItem ClearHideTransition()
    {
        ObsScene.obs_sceneitem_set_transition(Handle, 0, ObsSourceHandle.Null);
        return this;
    }

    private SceneItem SetTransition(string transitionTypeId, TimeSpan? duration, bool show)
    {
        // The scene item takes its own reference, so the temporary wrapper can be disposed.
        using var transition = Sources.Source.CreatePrivate(transitionTypeId,
            $"{transitionTypeId} ({(show ? "show" : "hide")})");
        ObsScene.obs_sceneitem_set_transition(Handle, show ? (byte)1 : (byte)0, transition.Handle);
        ObsScene.obs_sceneitem_set_transition_duration(Handle, show ? (byte)1 : (byte)0,
            (uint)(duration ?? TimeSpan.FromMilliseconds(300)).TotalMilliseconds);
        return this;
    }

    #endregion

    #region Order

    /// <summary>Gets or sets the order position.</summary>
    public int OrderPosition
    {
        get => ObsScene.obs_sceneitem_get_order_position(Handle);
        set => ObsScene.obs_sceneitem_set_order_position(Handle, value);
    }

    /// <summary>Sets the order movement.</summary>
    public void SetOrder(ObsOrderMovement movement)
    {
        ObsScene.obs_sceneitem_set_order(Handle, movement);
    }

    /// <summary>Moves this item to the top of the scene.</summary>
    public void MoveToTop() => SetOrder(ObsOrderMovement.MoveTop);

    /// <summary>Moves this item to the bottom of the scene.</summary>
    public void MoveToBottom() => SetOrder(ObsOrderMovement.MoveBottom);

    /// <summary>Moves this item up one position.</summary>
    public void MoveUp() => SetOrder(ObsOrderMovement.MoveUp);

    /// <summary>Moves this item down one position.</summary>
    public void MoveDown() => SetOrder(ObsOrderMovement.MoveDown);

    #endregion

    #region Fluent API

    /// <summary>Sets the position.</summary>
    public SceneItem SetPosition(float x, float y)
    {
        Position = new Vec2 { X = x, Y = y };
        return this;
    }

    /// <summary>Sets the scale.</summary>
    public SceneItem SetScale(float x, float y)
    {
        Scale = new Vec2 { X = x, Y = y };
        return this;
    }

    /// <summary>Sets the rotation in degrees.</summary>
    public SceneItem SetRotation(float degrees)
    {
        Rotation = degrees;
        return this;
    }

    /// <summary>Sets the visibility.</summary>
    public SceneItem SetVisible(bool visible)
    {
        IsVisible = visible;
        return this;
    }

    /// <summary>Sets the locked state.</summary>
    public SceneItem SetLocked(bool locked)
    {
        IsLocked = locked;
        return this;
    }

    /// <summary>Sets the bounds type and size.</summary>
    public SceneItem SetBounds(ObsBoundsType type, float width, float height)
    {
        BoundsType = type;
        Bounds = new Vec2 { X = width, Y = height };
        return this;
    }

    /// <summary>Sets the scale filter used when this item is scaled.</summary>
    public SceneItem SetScaleFilter(ObsScaleType filter)
    {
        ScaleFilter = filter;
        return this;
    }

    /// <summary>Sets the blending mode used to composite this item.</summary>
    public SceneItem SetBlendingMode(ObsBlendingType mode)
    {
        BlendingMode = mode;
        return this;
    }

    /// <summary>Sets the crop values.</summary>
    public SceneItem SetCrop(int left, int top, int right, int bottom)
    {
        Crop = new ObsSceneItemCrop
        {
            Left = left,
            Top = top,
            Right = right,
            Bottom = bottom
        };
        return this;
    }

    #endregion

    #region Groups

    /// <summary>
    /// True if this scene item is a group (a container of other scene items that can be
    /// transformed together).
    /// </summary>
    public bool IsGroup => ObsScene.obs_sceneitem_is_group(Handle);

    /// <summary>
    /// Gets the inner scene backing this group, or null if this item is not a group.
    /// The returned <see cref="Scene"/> is a non-owning view: use it to add or enumerate
    /// the group's members; its lifetime is owned by the group.
    /// </summary>
    public Scene? GroupScene
    {
        get
        {
            var innerHandle = ObsScene.obs_sceneitem_group_get_scene(Handle);
            // Does not add a reference; the group owns the scene, so wrap as non-owning.
            return innerHandle.IsNull ? null : new Scene(innerHandle, ownsHandle: false);
        }
    }

    /// <summary>
    /// Disbands this group, moving its members back into the parent scene. No-op if this
    /// item is not a group.
    /// </summary>
    public void Ungroup() => ObsScene.obs_sceneitem_group_ungroup(Handle);

    /// <summary>
    /// Moves an existing scene item (from the same scene) into this group. This item must
    /// be a group.
    /// </summary>
    /// <param name="item">The scene item to move into the group.</param>
    public void AddItem(SceneItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ObsScene.obs_sceneitem_group_add_item(Handle, item.Handle);
    }

    /// <summary>
    /// Removes a member from this group, moving it back into the parent scene. This item
    /// must be a group.
    /// </summary>
    /// <param name="item">The member to remove from the group.</param>
    public void RemoveItem(SceneItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ObsScene.obs_sceneitem_group_remove_item(Handle, item.Handle);
    }

    /// <summary>
    /// Enumerates the members of this group. Returns an empty list if this item is not a group.
    /// </summary>
    public List<SceneItem> GetGroupItems()
    {
        var items = new List<SceneItem>();

        var innerHandle = ObsScene.obs_sceneitem_group_get_scene(Handle);
        if (innerHandle.IsNull)
            return items;
        var innerScene = new Scene(innerHandle, ownsHandle: false);

        ObsScene.EnumSceneItemCallback callback = (scene, item, data) =>
        {
            if (!item.IsNull)
            {
                // obs_scene_enum_items references each item only for the callback's
                // duration, so take our own reference to keep the wrapper valid.
                ObsScene.obs_sceneitem_addref(item);
                items.Add(new SceneItem(item, innerScene, ownsHandle: true));
            }
            return 1; // Continue enumeration
        };

        ObsScene.obs_sceneitem_group_enum_items(Handle, callback, 0);
        GC.KeepAlive(callback); // Prevent delegate from being collected during P/Invoke

        return items;
    }

    /// <summary>
    /// Gets the group in the parent scene that contains this item, or null if this item is
    /// not inside a group.
    /// </summary>
    public SceneItem? GetParentGroup()
    {
        var groupHandle = ObsScene.obs_sceneitem_get_group(_scene.Handle, Handle);
        if (groupHandle.IsNull)
            return null;
        // obs_sceneitem_get_group does not add a reference.
        ObsScene.obs_sceneitem_addref(groupHandle);
        return new SceneItem(groupHandle, _scene, ownsHandle: true);
    }

    #endregion

    /// <summary>
    /// Removes this item (and its source) from the scene. This is distinct from disposing
    /// the wrapper: disposing only releases this wrapper's reference and leaves the item in
    /// the scene, whereas <see cref="Remove"/> unlinks it so it stops rendering.
    /// </summary>
    public void Remove()
    {
        if (_removed)
            return;

        _removed = true;
        ObsScene.obs_sceneitem_remove(Handle);
    }

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        // Release the single reference this wrapper owns. Removing the item from the scene
        // is the explicit job of Remove(), not of disposal/finalization.
        ObsScene.obs_sceneitem_release((ObsSceneItemHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        using var source = Source;
        return $"SceneItem[{Id}]: {source.Name}";
    }
}
