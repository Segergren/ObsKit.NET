using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Scenes;

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
            ObsSource.obs_source_addref(sourceHandle);
            return new Source(sourceHandle, ownsHandle: true);
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

    /// <summary>Gets or sets the alignment.</summary>
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

    /// <summary>Removes this item from the scene.</summary>
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
        // obs_sceneitem_remove internally calls obs_sceneitem_release
        if (!_removed)
        {
            _removed = true;
            ObsScene.obs_sceneitem_remove((ObsSceneItemHandle)handle);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        using var source = Source;
        return $"SceneItem[{Id}]: {source.Name}";
    }
}
