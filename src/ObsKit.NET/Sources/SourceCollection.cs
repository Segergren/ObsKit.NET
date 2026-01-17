using System.Collections;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Sources;

/// <summary>
/// Provides access to all sources in OBS.
/// </summary>
public sealed class SourceCollection : IEnumerable<Source>
{
    private static SourceCollection? _instance;
    private static readonly object _lock = new();

    internal static SourceCollection Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SourceCollection();
                }
            }
            return _instance;
        }
    }

    private SourceCollection() { }

    /// <summary>
    /// Creates a new source and adds it to OBS.
    /// </summary>
    /// <param name="typeId">The source type identifier.</param>
    /// <param name="name">The source name.</param>
    /// <param name="configure">Optional action to configure the source settings.</param>
    /// <returns>The created source.</returns>
    public Source Create(string typeId, string name, Action<Core.Settings>? configure = null)
    {
        Core.Settings? settings = null;
        if (configure != null)
        {
            settings = new Core.Settings();
            configure(settings);
        }

        try
        {
            return new Source(typeId, name, settings);
        }
        finally
        {
            settings?.Dispose();
        }
    }

    /// <summary>
    /// Creates a private source (not saved with scene collections).
    /// </summary>
    /// <param name="typeId">The source type identifier.</param>
    /// <param name="name">The source name.</param>
    /// <param name="configure">Optional action to configure the source settings.</param>
    /// <returns>The created private source.</returns>
    public Source CreatePrivate(string typeId, string name, Action<Core.Settings>? configure = null)
    {
        Core.Settings? settings = null;
        if (configure != null)
        {
            settings = new Core.Settings();
            configure(settings);
        }

        try
        {
            return Source.CreatePrivate(typeId, name, settings);
        }
        finally
        {
            settings?.Dispose();
        }
    }

    /// <summary>
    /// Enumerates all sources.
    /// </summary>
    public IEnumerator<Source> GetEnumerator()
    {
        var sources = new List<Source>();

        ObsSource.EnumSourceCallback callback = (data, handle) =>
        {
            if (!handle.IsNull)
            {
                // Add a reference since we're keeping it
                ObsSource.obs_source_addref(handle);
                sources.Add(new Source(handle, ownsHandle: true));
            }
            return 1; // Continue enumeration
        };

        ObsSource.obs_enum_sources(callback, 0);
        GC.KeepAlive(callback); // Prevent delegate from being collected during P/Invoke

        return sources.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets all sources as a list.
    /// Note: Each source in the returned list should be disposed when no longer needed.
    /// </summary>
    public List<Source> ToList()
    {
        var list = new List<Source>();
        foreach (var source in this)
        {
            list.Add(source);
        }
        return list;
    }

    /// <summary>
    /// Gets the count of all sources.
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
            ObsSource.obs_enum_sources(callback, 0);
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
