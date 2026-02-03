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
        // Note: obs_scene_from_source does not add a reference
        ObsScene.obs_scene_addref(handle);
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
    /// Gets this scene as a source.
    /// </summary>
    public Source AsSource
    {
        get
        {
            var sourceHandle = ObsScene.obs_scene_get_source(Handle);
            // obs_scene_get_source does not add a reference, but we need one
            ObsSource.obs_source_addref(sourceHandle);
            return new Source(sourceHandle, "scene", ownsHandle: true);
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
    /// Finds a scene item by source name.
    /// </summary>
    /// <param name="name">The source name to find.</param>
    /// <returns>The scene item, or null if not found.</returns>
    public SceneItem? FindSource(string name)
    {
        var itemHandle = ObsScene.obs_scene_find_source(Handle, name);
        if (itemHandle.IsNull)
            return null;
        // obs_scene_find_source adds a reference
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
        // obs_scene_find_source_recursive adds a reference
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
    /// Sets this scene as the program output (what gets recorded/streamed).
    /// </summary>
    /// <param name="channel">Output channel (0 = main, 1-5 = aux).</param>
    public void SetAsProgram(uint channel = 0)
    {
        var sourceHandle = ObsScene.obs_scene_get_source(Handle);
        ObsCore.obs_set_output_source(channel, sourceHandle);

        // Track the channel so we can clear it on disposal
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

        // Remove scene source from canvas (must be done before release)
        var sourceHandle = ObsScene.obs_scene_get_source(sceneHandle);
        if (!sourceHandle.IsNull)
        {
            try { ObsSource.obs_source_remove(sourceHandle); }
            catch { /* Ignore */ }
        }

        // Release the scene
        ObsScene.obs_scene_release(sceneHandle);
    }

    public override string ToString() => $"Scene: {Name}";
}
