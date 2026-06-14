using System.Collections;
using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Sources;
using static ObsKit.NET.Native.Interop.ObsCore;

namespace ObsKit.NET.Scenes;

/// <summary>
/// Represents an OBS scene (obs_scene_t).
/// A scene is a special source that contains other sources as scene items.
/// </summary>
public sealed class Scene : ObsObject, IEnumerable<SceneItem>
{
    private uint? _assignedChannel;
    private bool _removed;
    private readonly List<SceneItem> _ownedSceneItems = new();

    /// <summary>
    /// Creates a new scene with the specified name.
    /// </summary>
    /// <param name="name">The scene name.</param>
    public Scene(string name) : base(CreateScene(name))
    {
    }

    /// <summary>
    /// Creates a private scene (not saved with scene collections).
    /// </summary>
    /// <param name="name">The scene name.</param>
    public static Scene CreatePrivate(string name)
    {
        ThrowIfNotInitialized();
        var handle = ObsScene.obs_scene_create_private(name);
        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create private scene '{name}'");
        return new Scene(handle, ownsHandle: true);
    }

    /// <summary>
    /// Gets a scene from a source.
    /// </summary>
    /// <param name="source">The source that is a scene.</param>
    /// <returns>The scene, or null if the source is not a scene.</returns>
    public static Scene? FromSource(Source source)
    {
        var handle = ObsScene.obs_scene_from_source(source.Handle);
        if (handle.IsNull)
            return null;
        // obs_scene_from_source returns a borrowed pointer; take an owning ref via the
        // exported obs_scene_get_ref (returns null if the scene is being destroyed).
        var refd = ObsScene.obs_scene_get_ref(handle);
        if (refd.IsNull)
            return null;
        return new Scene(refd, ownsHandle: true);
    }

    /// <summary>
    /// Duplicates this scene, including item transforms, crops, and visibility.
    /// </summary>
    /// <param name="name">The name for the new scene.</param>
    /// <param name="type">Whether items reference the same sources (default) or are fully copied.</param>
    public Scene Duplicate(string name, ObsSceneDuplicateType type = ObsSceneDuplicateType.Refs)
    {
        var handle = ObsScene.obs_scene_duplicate(Handle, name, type);
        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to duplicate scene as '{name}'");
        return new Scene(handle, ownsHandle: true);
    }

    /// <summary>
    /// Internal constructor for wrapping an existing handle.
    /// </summary>
    internal Scene(ObsSceneHandle handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }

    private static nint CreateScene(string name)
    {
        ThrowIfNotInitialized();
        var handle = ObsScene.obs_scene_create(name);
        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create scene '{name}'");
        return handle;
    }

    /// <summary>
    /// Gets the internal handle for P/Invoke calls.
    /// </summary>
    internal new ObsSceneHandle Handle => (ObsSceneHandle)base.Handle;

    /// <summary>
    /// Gets this scene as a <see cref="Source"/>. In OBS a scene is itself backed by a source,
    /// so APIs that take a <see cref="Source"/> — output channels, transitions, filters —
    /// operate on this view rather than the <see cref="Scene"/> directly. Returns a new owning
    /// reference; dispose it when done.
    /// </summary>
    public Source AsSource
    {
        get
        {
            var sourceHandle = ObsScene.obs_scene_get_source(Handle);
            // obs_scene_get_source does not add a reference; take one via the exported get_ref.
            var refHandle = ObsSource.obs_source_get_ref(sourceHandle);
            return new Source(refHandle, "scene", ownsHandle: true);
        }
    }

    /// <summary>
    /// Gets the name of this scene.
    /// </summary>
    public string? Name
    {
        get
        {
            using var source = AsSource;
            return source.Name;
        }
        set
        {
            if (value != null)
            {
                using var source = AsSource;
                source.Name = value;
            }
        }
    }

    #region Scene Items

    /// <summary>
    /// Adds a source to this scene.
    /// </summary>
    /// <param name="source">The source to add.</param>
    /// <returns>The created scene item.</returns>
    public SceneItem AddSource(Source source)
    {
        var itemHandle = ObsScene.obs_scene_add(Handle, source.Handle);
        if (itemHandle.IsNull)
            throw new InvalidOperationException("Failed to add source to scene");

        // obs_scene_add returns the scene's own reference (borrowed); take our own so the
        // wrapper can release it on dispose without removing the item from the scene.
        ObsScene.obs_sceneitem_addref(itemHandle);
        var sceneItem = new SceneItem(itemHandle, this, ownsHandle: true);
        _ownedSceneItems.Add(sceneItem);
        return sceneItem;
    }

    /// <summary>
    /// Adds a source to this scene with transform configuration.
    /// </summary>
    /// <param name="source">The source to add.</param>
    /// <param name="configure">Action to configure the scene item.</param>
    /// <returns>The created scene item.</returns>
    public SceneItem AddSource(Source source, Action<SceneItem> configure)
    {
        var item = AddSource(source);
        configure(item);
        return item;
    }

    /// <summary>
    /// Creates an empty group in this scene. Existing scene items can be moved into it
    /// with <see cref="SceneItem.AddItem"/>, and the group can be transformed as a unit.
    /// </summary>
    /// <param name="name">The group name (must be unique within the scene).</param>
    /// <returns>The created group scene item.</returns>
    public SceneItem AddGroup(string name)
    {
        var itemHandle = ObsScene.obs_scene_add_group2(Handle, name, true);
        if (itemHandle.IsNull)
            throw new InvalidOperationException($"Failed to add group '{name}' to scene");

        // obs_scene_add_group2 returns the scene's own reference (borrowed); take our own so
        // the wrapper can release it on dispose without removing the group from the scene.
        ObsScene.obs_sceneitem_addref(itemHandle);
        var sceneItem = new SceneItem(itemHandle, this, ownsHandle: true);
        _ownedSceneItems.Add(sceneItem);
        return sceneItem;
    }

    /// <summary>
    /// Gets a group in this scene by name.
    /// </summary>
    /// <param name="name">The group name.</param>
    /// <returns>The group scene item, or null if no group with that name exists.</returns>
    public SceneItem? GetGroup(string name)
    {
        var itemHandle = ObsScene.obs_scene_get_group(Handle, name);
        if (itemHandle.IsNull)
            return null;
        // obs_scene_get_group does not add a reference.
        ObsScene.obs_sceneitem_addref(itemHandle);
        return new SceneItem(itemHandle, this, ownsHandle: true);
    }

    /// <summary>
    /// Finds a scene item by source name.
    /// </summary>
    /// <param name="name">The source name to find.</param>
    /// <returns>The scene item, or null if not found.</returns>
    public SceneItem? FindSource(string name)
    {
        var itemHandle = ObsScene.obs_scene_find_source(Handle, name);
        if (itemHandle.IsNull)
            return null;
        // obs_scene_find_source does NOT add a reference; take one for the wrapper.
        ObsScene.obs_sceneitem_addref(itemHandle);
        return new SceneItem(itemHandle, this, ownsHandle: true);
    }

    /// <summary>
    /// Finds a scene item by source name, searching recursively through nested scenes.
    /// </summary>
    /// <param name="name">The source name to find.</param>
    /// <returns>The scene item, or null if not found.</returns>
    public SceneItem? FindSourceRecursive(string name)
    {
        var itemHandle = ObsScene.obs_scene_find_source_recursive(Handle, name);
        if (itemHandle.IsNull)
            return null;
        // obs_scene_find_source_recursive does NOT add a reference; take one for the wrapper.
        ObsScene.obs_sceneitem_addref(itemHandle);
        return new SceneItem(itemHandle, this, ownsHandle: true);
    }

    /// <summary>
    /// Finds a scene item by its ID.
    /// </summary>
    /// <param name="id">The scene item ID.</param>
    /// <returns>The scene item, or null if not found.</returns>
    public SceneItem? FindById(long id)
    {
        var itemHandle = ObsScene.obs_scene_find_sceneitem_by_id(Handle, id);
        if (itemHandle.IsNull)
            return null;
        // Add reference for the wrapper
        ObsScene.obs_sceneitem_addref(itemHandle);
        return new SceneItem(itemHandle, this, ownsHandle: true);
    }

    /// <summary>
    /// Enumerates all scene items in this scene.
    /// </summary>
    public IEnumerator<SceneItem> GetEnumerator()
    {
        var items = new List<SceneItem>();

        ObsScene.EnumSceneItemCallback callback = (scene, item, data) =>
        {
            if (!item.IsNull)
            {
                // Add a reference since we're keeping it
                ObsScene.obs_sceneitem_addref(item);
                items.Add(new SceneItem(item, this, ownsHandle: true));
            }
            return 1; // Continue enumeration
        };

        ObsScene.obs_scene_enum_items(Handle, callback, 0);
        GC.KeepAlive(callback); // Prevent delegate from being collected during P/Invoke

        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets all scene items as a list.
    /// </summary>
    public List<SceneItem> GetItems()
    {
        var list = new List<SceneItem>();
        foreach (var item in this)
        {
            list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Sets the complete top-to-bottom z-order of this scene's items in one call (e.g. after
    /// a drag-to-reorder in a UI). The supplied items must be exactly the scene's current
    /// items — pass a reordering of <see cref="GetItems"/>.
    /// </summary>
    /// <param name="orderedItems">All of the scene's items, in the desired order (index 0 = top).</param>
    /// <returns>
    /// True if the order was changed; false if <paramref name="orderedItems"/> does not match
    /// the scene's current item set or the order was already identical.
    /// </returns>
    public bool ReorderItems(IReadOnlyList<SceneItem> orderedItems)
    {
        ArgumentNullException.ThrowIfNull(orderedItems);
        if (orderedItems.Count == 0)
            return false;

        var handles = new nint[orderedItems.Count];
        for (int i = 0; i < orderedItems.Count; i++)
            handles[i] = orderedItems[i].Handle.Value;

        return ObsScene.obs_scene_reorder_items(Handle, handles);
    }

    /// <summary>
    /// Gets the count of scene items.
    /// </summary>
    public int ItemCount
    {
        get
        {
            int count = 0;
            ObsScene.EnumSceneItemCallback callback = (scene, item, data) =>
            {
                count++;
                return 1; // Continue enumeration
            };
            ObsScene.obs_scene_enum_items(Handle, callback, 0);
            GC.KeepAlive(callback); // Prevent delegate from being collected during P/Invoke
            return count;
        }
    }

    /// <summary>
    /// Gets a scene item by index.
    /// </summary>
    public SceneItem this[int index]
    {
        get
        {
            var items = GetItems();
            if (index < 0 || index >= items.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range. Scene has {items.Count} items.");

            // Dispose all items except the one we're returning
            for (int i = 0; i < items.Count; i++)
            {
                if (i != index)
                    items[i].Dispose();
            }

            return items[index];
        }
    }

    #endregion

    /// <summary>
    /// Assigns this scene's source to an output channel and records the channel so it is
    /// cleared on disposal. Invoked by <see cref="Obs.SetOutputSource(uint, Scene?)"/>.
    /// </summary>
    internal void AssignToChannel(uint channel)
    {
        var sourceHandle = ObsScene.obs_scene_get_source(Handle);
        ObsCore.obs_set_output_source(channel, sourceHandle);
        _assignedChannel = channel;
    }

    /// <summary>
    /// Clears this scene from its assigned output channel (if any).
    /// This is called automatically on disposal, but can be called manually if needed.
    /// </summary>
    public void ClearFromProgram()
    {
        if (_assignedChannel.HasValue)
        {
            ObsCore.obs_set_output_source(_assignedChannel.Value, ObsSourceHandle.Null);
            _assignedChannel = null;
        }
    }

    /// <summary>
    /// Removes this scene from OBS — unlinks it from the canvas so it stops rendering and
    /// is destroyed once all references are released. This is distinct from disposing the
    /// wrapper: disposing only releases this wrapper's reference and leaves the scene in the
    /// session, whereas <see cref="Remove"/> deletes it. Disposing a wrapper that merely
    /// references an existing scene (e.g. one obtained via <see cref="FromSource"/> or
    /// <c>SceneCollection</c> enumeration) must therefore never delete that scene.
    /// </summary>
    public void Remove()
    {
        if (_removed)
            return;
        _removed = true;

        var sourceHandle = ObsScene.obs_scene_get_source(Handle);
        if (!sourceHandle.IsNull)
            ObsSource.obs_source_remove(sourceHandle);
    }

    protected override void ReleaseHandle(nint handle)
    {
        var sceneHandle = (ObsSceneHandle)handle;

        // Clear from output channel
        if (_assignedChannel.HasValue)
        {
            try { ObsCore.obs_set_output_source(_assignedChannel.Value, ObsSourceHandle.Null); }
            catch { /* Ignore */ }
            _assignedChannel = null;
        }

        // Dispose all tracked SceneItems
        foreach (var item in _ownedSceneItems)
        {
            try { if (!item.IsDisposed) item.Dispose(); }
            catch { /* Ignore */ }
        }
        _ownedSceneItems.Clear();

        // Release the single reference this wrapper owns. Removing the scene from OBS is the
        // explicit job of Remove(), not of disposal/finalization: a created public scene is
        // also held by the main canvas, so releasing our reference does not destroy it, and a
        // wrapper that merely references an existing scene must not delete it on dispose.
        ObsScene.obs_scene_release(sceneHandle);
    }

    public override string ToString() => $"Scene: {Name}";
}
