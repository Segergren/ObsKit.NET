using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Filters;

/// <summary>
/// Represents an OBS filter (a source of type OBS_SOURCE_TYPE_FILTER).
/// Filters are created as private sources and attached to a source via
/// <see cref="Source.AddFilter"/>.
/// </summary>
public class Filter : Source
{
    /// <summary>
    /// Creates a filter of the given type.
    /// </summary>
    /// <param name="typeId">The filter type identifier (e.g., "noise_gate_filter").</param>
    /// <param name="name">The filter name.</param>
    /// <param name="settings">Optional initial settings.</param>
    public Filter(string typeId, string name, Settings? settings = null)
        : base(CreateFilter(typeId, name, settings), typeId, ownsHandle: true)
    {
    }

    private static ObsSourceHandle CreateFilter(string typeId, string name, Settings? settings)
    {
        ThrowIfNotInitialized();
        var handle = ObsSource.obs_source_create_private(typeId, name, settings?.Handle ?? default);

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create filter of type '{typeId}'");

        return handle;
    }
}
