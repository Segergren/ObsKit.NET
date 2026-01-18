using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Signals;

namespace ObsKit.NET.Sources;

/// <summary>
/// Represents an OBS source (obs_source_t).
/// Sources are the core building blocks that provide video and/or audio content.
/// </summary>
public class Source : ObsObject
{
    /// <summary>
    /// Creates a new source.
    /// </summary>
    /// <param name="typeId">The source type identifier (e.g., "monitor_capture", "window_capture").</param>
    /// <param name="name">The display name for this source.</param>
    /// <param name="settings">Optional settings for the source.</param>
    /// <param name="hotkeyData">Optional hotkey data.</param>
    public Source(string typeId, string name, Settings? settings = null, Settings? hotkeyData = null)
        : base(CreateSource(typeId, name, settings, hotkeyData))
    {
        TypeId = typeId;
    }

    /// <summary>
    /// Creates a private source (not saved with scene collections).
    /// </summary>
    /// <param name="typeId">The source type identifier.</param>
    /// <param name="name">The source name.</param>
    /// <param name="settings">Optional settings.</param>
    public static Source CreatePrivate(string typeId, string name, Settings? settings = null)
    {
        ThrowIfNotInitialized();
        var handle = ObsSource.obs_source_create_private(
            typeId,
            name,
            settings?.Handle ?? default);

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create private source of type '{typeId}'");

        return new Source(handle, typeId, ownsHandle: true);
    }

    internal Source(ObsSourceHandle handle, string? typeId = null, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
        TypeId = typeId ?? ObsSource.obs_source_get_id(handle);
    }

    private static nint CreateSource(string typeId, string name, Settings? settings, Settings? hotkeyData)
    {
        ThrowIfNotInitialized();
        var handle = ObsSource.obs_source_create(
            typeId,
            name,
            settings?.Handle ?? default,
            hotkeyData?.Handle ?? default);

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create source of type '{typeId}'");

        return handle;
    }

    internal new ObsSourceHandle Handle => (ObsSourceHandle)base.Handle;

    /// <summary>
    /// Gets the source type identifier (e.g., "monitor_capture", "window_capture").
    /// </summary>
    public string? TypeId { get; }

    /// <summary>Gets or sets the source name.</summary>
    public string? Name
    {
        get => ObsSource.obs_source_get_name(Handle);
        set
        {
            if (value != null)
                ObsSource.obs_source_set_name(Handle, value);
        }
    }

    /// <summary>Gets the display name for this source type.</summary>
    public string? DisplayName => TypeId != null ? ObsSource.obs_source_get_display_name(TypeId) : null;

    /// <summary>Gets the source width in pixels.</summary>
    public uint Width => ObsSource.obs_source_get_width(Handle);

    /// <summary>Gets the source height in pixels.</summary>
    public uint Height => ObsSource.obs_source_get_height(Handle);

    /// <summary>
    /// Gets whether the source is active (being rendered in an output).
    /// </summary>
    public bool IsActive => ObsSource.obs_source_active(Handle);

    /// <summary>Gets whether the source is currently showing.</summary>
    public bool IsShowing => ObsSource.obs_source_showing(Handle);

    /// <summary>Gets whether the source has been removed.</summary>
    public bool IsRemoved => ObsSource.obs_source_removed(Handle);

    /// <summary>
    /// Gets or sets the volume level (0.0 to 1.0).
    /// </summary>
    public float Volume
    {
        get => ObsSource.obs_source_get_volume(Handle);
        set => ObsSource.obs_source_set_volume(Handle, Math.Clamp(value, 0.0f, 1.0f));
    }

    /// <summary>Gets or sets whether the source is muted.</summary>
    public bool IsMuted
    {
        get => ObsSource.obs_source_muted(Handle);
        set => ObsSource.obs_source_set_muted(Handle, value ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Gets or sets the audio mixers this source outputs to (bitmask, channels 1-6).
    /// </summary>
    public uint AudioMixers
    {
        get => ObsSource.obs_source_get_audio_mixers(Handle);
        set => ObsSource.obs_source_set_audio_mixers(Handle, value);
    }

    /// <summary>Gets or sets the source flags.</summary>
    public uint Flags
    {
        get => ObsSource.obs_source_get_flags(Handle);
        set => ObsSource.obs_source_set_flags(Handle, value);
    }

    /// <summary>Gets the current source settings.</summary>
    public Settings GetSettings()
    {
        var handle = ObsSource.obs_source_get_settings(Handle);
        return new Settings(handle, ownsHandle: true);
    }

    /// <summary>Updates the source settings.</summary>
    public void Update(Settings settings)
    {
        ObsSource.obs_source_update(Handle, settings.Handle);
    }

    /// <summary>Updates the source settings using a configuration action.</summary>
    public void Update(Action<Settings> configure)
    {
        using var settings = new Settings();
        configure(settings);
        Update(settings);
    }

    /// <summary>
    /// Gets an additional reference to this source (shares the same native handle).
    /// </summary>
    public Source GetRef()
    {
        var refHandle = ObsSource.obs_source_get_ref(Handle);
        if (refHandle.IsNull)
            throw new InvalidOperationException("Source has been released");
        return new Source(refHandle, TypeId, ownsHandle: true);
    }

    /// <summary>Adds a reference to this source.</summary>
    public void AddRef()
    {
        ObsSource.obs_source_addref(Handle);
    }

    /// <summary>Removes this source from its parent.</summary>
    public void Remove()
    {
        ObsSource.obs_source_remove(Handle);
    }

    #region Filters

    /// <summary>Adds a filter to this source.</summary>
    public void AddFilter(Source filter)
    {
        ObsSource.obs_source_filter_add(Handle, filter.Handle);
    }

    /// <summary>Removes a filter from this source.</summary>
    public void RemoveFilter(Source filter)
    {
        ObsSource.obs_source_filter_remove(Handle, filter.Handle);
    }

    /// <summary>Gets a filter by name.</summary>
    public Source? GetFilter(string name)
    {
        var handle = ObsSource.obs_source_get_filter_by_name(Handle, name);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>Gets the number of filters on this source.</summary>
    public int FilterCount => (int)ObsSource.obs_source_filter_count(Handle);

    #endregion

    #region Signal Handler

    internal SignalHandlerHandle SignalHandler => ObsSource.obs_source_get_signal_handler(Handle);

    /// <summary>
    /// Connects a callback to a source signal using a strongly-typed enum.
    /// </summary>
    /// <param name="signal">The signal to connect to.</param>
    /// <param name="callback">The callback to invoke when the signal is emitted.</param>
    /// <returns>A SignalConnection that can be disposed to disconnect the callback.</returns>
    public SignalConnection ConnectSignal(SourceSignal signal, SignalCallback callback)
    {
        return new SignalConnection(SignalHandler, signal.ToSignalName(), callback);
    }

    /// <summary>
    /// Connects a callback to a source signal using a string name.
    /// Use this overload for custom or plugin-specific signals not in the SourceSignal enum.
    /// </summary>
    /// <param name="signal">The signal name to connect to.</param>
    /// <param name="callback">The callback to invoke when the signal is emitted.</param>
    /// <returns>A SignalConnection that can be disposed to disconnect the callback.</returns>
    public SignalConnection ConnectSignal(string signal, SignalCallback callback)
    {
        return new SignalConnection(SignalHandler, signal, callback);
    }

    #endregion

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsSource.obs_source_release((ObsSourceHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Source[{TypeId}]: {Name}";
}
