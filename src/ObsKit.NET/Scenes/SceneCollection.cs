using System.Collections;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Scenes;

/// <summary>
/// Provides access to all scenes in OBS.
/// </summary>
public sealed class SceneCollection : IEnumerable<Scene>
{
    private static SceneCollection? _instance;
    private static readonly object _lock = new();

    internal static SceneCollection Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SceneCollection();
                }
            }
            return _instance;
        }
    }

    private SceneCollection() { }

    /// <summary>
    /// Creates a new scene.
    /// </summary>
    /// <param name="name">The scene name.</param>
    /// <returns>The created scene.</returns>
    public Scene Create(string name)
    {
        return new Scene(name);
    }

    /// <summary>
    /// Creates a private scene (not saved with scene collections).
    /// </summary>
    /// <param name="name">The scene name.</param>
    /// <returns>The created private scene.</returns>
    public Scene CreatePrivate(string name)
    {
        return Scene.CreatePrivate(name);
    }

    /// <summary>
    /// Enumerates all scenes.
    /// </summary>
    public IEnumerator<Scene> GetEnumerator()
    {
        var scenes = new List<Scene>();

        ObsSource.EnumSourceCallback callback = (data, handle) =>
        {
            if (!handle.IsNull)
            {
                var sceneHandle = ObsScene.obs_scene_from_source(handle);
                if (!sceneHandle.IsNull)
                {
                    // obs_scene_from_source doesn't add a reference, add one
                    ObsScene.obs_scene_addref(sceneHandle);
                    scenes.Add(new Scene(sceneHandle, ownsHandle: true));
                }
            }
            return 1; // Continue enumeration
        };

        ObsSource.obs_enum_scenes(callback, 0);
        GC.KeepAlive(callback); // Prevent delegate from being collected during P/Invoke

        return scenes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets all scenes as a list.
    /// Note: Each scene in the returned list should be disposed when no longer needed.
    /// </summary>
    public List<Scene> ToList()
    {
        var list = new List<Scene>();
        foreach (var scene in this)
        {
            list.Add(scene);
        }
        return list;
    }

    /// <summary>
    /// Gets a scene by index.
    /// </summary>
    public Scene this[int index]
    {
        get
        {
            var scenes = ToList();
            if (index < 0 || index >= scenes.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range. There are {scenes.Count} scenes.");

            // Dispose all scenes except the one we're returning
            for (int i = 0; i < scenes.Count; i++)
            {
                if (i != index)
                    scenes[i].Dispose();
            }

            return scenes[index];
        }
    }

    /// <summary>
    /// Finds a scene by name.
    /// </summary>
    /// <param name="name">The scene name to find.</param>
    /// <returns>The scene, or null if not found.</returns>
    public Scene? Find(string name)
    {
        foreach (var scene in this)
        {
            if (scene.Name == name)
                return scene;
            scene.Dispose();
        }
        return null;
    }

    /// <summary>
    /// Gets the count of all scenes.
    /// </summary>
    public int Count
    {
        get
        {
            int count = 0;
            ObsSource.EnumSourceCallback callback = (data, handle) =>
            {
                count++;
                return 1; // Continue enumeration
            };
            ObsSource.obs_enum_scenes(callback, 0);
            GC.KeepAlive(callback); // Prevent delegate from being collected during P/Invoke
            return count;
        }
    }

    /// <summary>
    /// Resets the singleton instance. Called during OBS shutdown.
    /// </summary>
    internal static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }
}
