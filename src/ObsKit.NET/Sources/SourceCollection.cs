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
    /// Finds a public source by its name.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <returns>The source, or null if no source with that name exists. Dispose it when done.</returns>
    public Source? Find(string name) => Source.GetByName(name);

    /// <summary>
    /// Gets all sources as a list, optionally including private sources.
    /// Note: Each source in the returned list should be disposed when no longer needed.
    /// </summary>
    /// <param name="includePrivate">Also include private sources (created via <see cref="CreatePrivate"/> or internally by OBS).</param>
    public List<Source> ToList(bool includePrivate)
    {
        if (!includePrivate)
            return ToList();

        var sources = new List<Source>();
        ObsSource.EnumSourceCallback callback = (data, handle) =>
        {
            if (!handle.IsNull)
            {
                // Borrowed pointer; take our own owning ref (see GetEnumerator).
                var refHandle = ObsSource.obs_source_get_ref(handle);
                if (!refHandle.IsNull)
                    sources.Add(new Source(refHandle, ownsHandle: true));
            }
            return 1;
        };

        ObsSource.obs_enum_all_sources(callback, 0);
        GC.KeepAlive(callback);
        return sources;
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
                // The enum hands us a borrowed pointer that libobs releases right after this
                // callback returns; take our own owning ref via the exported get_ref.
                var refHandle = ObsSource.obs_source_get_ref(handle);
                if (!refHandle.IsNull)
                    sources.Add(new Source(refHandle, ownsHandle: true));
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
