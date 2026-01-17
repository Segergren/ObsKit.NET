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

    internal SceneItem(ObsSceneItemHandle handle, Scene scene, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
        _scene = scene;
    }

    internal new ObsSceneItemHandle Handle => (ObsSceneItemHandle)base.Handle;

    public Scene Scene => _scene;

    public Source Source
    {
        get
        {
            var sourceHandle = ObsScene.obs_sceneitem_get_source(Handle);
            ObsSource.obs_source_addref(sourceHandle);
            return new Source(sourceHandle, ownsHandle: true);
        }
    }

    public long Id => ObsScene.obs_sceneitem_get_id(Handle);

    public bool IsVisible
    {
        get => ObsScene.obs_sceneitem_visible(Handle);
        set => ObsScene.obs_sceneitem_set_visible(Handle, value);
    }

    /// <summary>
    /// Whether this item is locked (cannot be moved/resized in UI).
    /// </summary>
    public bool IsLocked
    {
        get => ObsScene.obs_sceneitem_locked(Handle);
        set => ObsScene.obs_sceneitem_set_locked(Handle, value);
    }

    public bool IsSelected
    {
        get => ObsScene.obs_sceneitem_selected(Handle);
        set => ObsScene.obs_sceneitem_select(Handle, value ? (byte)1 : (byte)0);
    }

    #region Position and Transform

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
    /// Rotation in degrees.
    /// </summary>
    public float Rotation
    {
        get => ObsScene.obs_sceneitem_get_rot(Handle);
        set => ObsScene.obs_sceneitem_set_rot(Handle, value);
    }

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

    public uint Alignment
    {
        get => ObsScene.obs_sceneitem_get_alignment(Handle);
        set => ObsScene.obs_sceneitem_set_alignment(Handle, value);
    }

    #endregion

    #region Bounds

    public ObsBoundsType BoundsType
    {
        get => ObsScene.obs_sceneitem_get_bounds_type(Handle);
        set => ObsScene.obs_sceneitem_set_bounds_type(Handle, value);
    }

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

    public int OrderPosition
    {
        get => ObsScene.obs_sceneitem_get_order_position(Handle);
        set => ObsScene.obs_sceneitem_set_order_position(Handle, value);
    }

    public void SetOrder(ObsOrderMovement movement)
    {
        ObsScene.obs_sceneitem_set_order(Handle, movement);
    }

    public void MoveToTop() => SetOrder(ObsOrderMovement.MoveTop);

    public void MoveToBottom() => SetOrder(ObsOrderMovement.MoveBottom);

    public void MoveUp() => SetOrder(ObsOrderMovement.MoveUp);

    public void MoveDown() => SetOrder(ObsOrderMovement.MoveDown);

    #endregion

    #region Fluent API

    public SceneItem SetPosition(float x, float y)
    {
        Position = new Vec2 { X = x, Y = y };
        return this;
    }

    public SceneItem SetScale(float x, float y)
    {
        Scale = new Vec2 { X = x, Y = y };
        return this;
    }

    public SceneItem SetRotation(float degrees)
    {
        Rotation = degrees;
        return this;
    }

    public SceneItem SetVisible(bool visible)
    {
        IsVisible = visible;
        return this;
    }

    public SceneItem SetLocked(bool locked)
    {
        IsLocked = locked;
        return this;
    }

    public SceneItem SetBounds(ObsBoundsType type, float width, float height)
    {
        BoundsType = type;
        Bounds = new Vec2 { X = width, Y = height };
        return this;
    }

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

    public void Remove()
    {
        ObsScene.obs_sceneitem_remove(Handle);
    }

    protected override void ReleaseHandle(nint handle)
    {
        ObsScene.obs_sceneitem_release((ObsSceneItemHandle)handle);
    }

    public override string ToString()
    {
        using var source = Source;
        return $"SceneItem[{Id}]: {source.Name}";
    }
}
