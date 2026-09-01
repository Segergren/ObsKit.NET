using System.Runtime.InteropServices;
using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Signals;

namespace ObsKit.NET.Sources;

/// <summary>
/// Screenshot pixel data captured from a source.
/// </summary>
/// <param name="Pixels">BGRA pixel data (4 bytes per pixel).</param>
/// <param name="Width">Image width in pixels.</param>
/// <param name="Height">Image height in pixels.</param>
public record ScreenshotData(byte[] Pixels, uint Width, uint Height);

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

    /// <summary>Gets the source category — input, filter, transition, or scene.</summary>
    public ObsSourceType SourceType => ObsSource.obs_source_get_type(Handle);

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

    /// <summary>
    /// Gets the source UUID — a stable identifier for the source's lifetime,
    /// usable with <see cref="GetByUuid"/>.
    /// </summary>
    public string? Uuid => ObsSource.obs_source_get_uuid(Handle);

    /// <summary>
    /// Finds a source by its UUID.
    /// </summary>
    /// <param name="uuid">The source UUID (see <see cref="Uuid"/>).</param>
    /// <returns>The source, or null if no source with that UUID exists.</returns>
    public static Source? GetByUuid(string uuid)
    {
        ThrowIfNotInitialized();
        var handle = ObsSource.obs_get_source_by_uuid(uuid);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>
    /// Finds a public source by its name.
    /// </summary>
    /// <param name="name">The source name (see <see cref="Name"/>).</param>
    /// <returns>The source, or null if no source with that name exists.</returns>
    public static Source? GetByName(string name)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(name);
        var handle = ObsSource.obs_get_source_by_name(name);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>
    /// Duplicates this source with a full copy of its settings and filters.
    /// Sources that cannot be duplicated (scenes when <paramref name="createPrivate"/>
    /// is false, and capture sources flagged do-not-duplicate such as game/display
    /// capture) return a new reference to this same source instead.
    /// </summary>
    /// <param name="desiredName">The name for the duplicate, or null to auto-generate one.</param>
    /// <param name="createPrivate">Create the duplicate as a private source (not enumerated).</param>
    /// <returns>The duplicated source. Dispose it when no longer needed.</returns>
    public Source Duplicate(string? desiredName = null, bool createPrivate = false)
    {
        var handle = ObsSource.obs_source_duplicate(Handle, desiredName, createPrivate ? (byte)1 : (byte)0);
        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to duplicate source '{Name}'.");
        return new Source(handle, ownsHandle: true);
    }

    /// <summary>
    /// Registers a hotkey tied to this source (unregistered automatically when the
    /// source is destroyed). Bind key combinations with
    /// <see cref="Hotkeys.RegisteredHotkey.Bind"/>; libobs polls global key state on
    /// its own hotkey thread, so bound combinations fire system-wide.
    /// Keep the returned object alive; dispose it to unregister early.
    /// </summary>
    /// <param name="name">The internal hotkey name.</param>
    /// <param name="description">The user-facing description.</param>
    /// <param name="onChanged">Invoked with true on press and false on release.</param>
    public Hotkeys.RegisteredHotkey RegisterHotkey(string name, string description, Action<bool> onChanged)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(onChanged);

        return new Hotkeys.RegisteredHotkey(name, description, onChanged,
            callback => ObsHotkey.obs_hotkey_register_source(Handle, name, description, callback, nint.Zero));
    }

    /// <summary>Gets the source width in pixels.</summary>
    public uint Width => ObsSource.obs_source_get_width(Handle);

    /// <summary>Gets the source height in pixels.</summary>
    public uint Height => ObsSource.obs_source_get_height(Handle);

    /// <summary>
    /// Gets the source width in pixels before any filters (e.g. crop or scale) are applied.
    /// </summary>
    public uint BaseWidth => ObsSource.obs_source_get_base_width(Handle);

    /// <summary>
    /// Gets the source height in pixels before any filters (e.g. crop or scale) are applied.
    /// </summary>
    public uint BaseHeight => ObsSource.obs_source_get_base_height(Handle);

    /// <summary>
    /// Gets the source type id without its version suffix (e.g. "color_source" for
    /// "color_source_v3"). Useful for type checks that should ignore versioning.
    /// </summary>
    public string? UnversionedTypeId => ObsSource.obs_source_get_unversioned_id(Handle);

    /// <summary>
    /// Gets whether the source is active (being rendered in an output).
    /// </summary>
    public bool IsActive => ObsSource.obs_source_active(Handle);

    /// <summary>Gets whether the source is currently showing.</summary>
    public bool IsShowing => ObsSource.obs_source_showing(Handle);

    /// <summary>Gets whether the source has been removed.</summary>
    public bool IsRemoved => ObsSource.obs_source_removed(Handle);

    /// <summary>
    /// Gets or sets whether the source is enabled. Primarily used to bypass a
    /// filter without removing it from its parent's filter chain.
    /// </summary>
    public bool IsEnabled
    {
        get => ObsSource.obs_source_enabled(Handle);
        set => ObsSource.obs_source_set_enabled(Handle, value ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Gets the output channel this source is assigned to, or null if not assigned.
    /// </summary>
    public uint? AssignedChannel { get; internal set; }

    /// <summary>
    /// Gets or sets the volume level (0.0 to 1.0).
    /// </summary>
    public float Volume
    {
        get => ObsSource.obs_source_get_volume(Handle);
        set => ObsSource.obs_source_set_volume(Handle, Math.Clamp(value, 0.0f, 1.0f));
    }

    /// <summary>
    /// Gets or sets the source volume in decibels. 0 dB is unity gain; negative values
    /// attenuate. Uses the same linear/dB mapping OBS uses internally. Unlike
    /// <see cref="Volume"/>, this is not clamped to unity, so positive values apply gain.
    /// Setting <see cref="float.NegativeInfinity"/> silences the source.
    /// </summary>
    public float VolumeDb
    {
        get => ObsAudioControls.obs_mul_to_db(ObsSource.obs_source_get_volume(Handle));
        set => ObsSource.obs_source_set_volume(Handle, ObsAudioControls.obs_db_to_mul(value));
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

    /// <summary>
    /// Sets the audio tracks this source outputs to using 1-based track numbers (1-6),
    /// replacing any previous assignment.
    /// </summary>
    /// <param name="tracks">The track numbers (1-6).</param>
    public Source SetAudioTracks(params int[] tracks)
    {
        AudioMixers = AudioTracks.ToMask(tracks);
        return this;
    }

    /// <summary>
    /// Enables or disables a single audio track for this source without affecting other tracks.
    /// </summary>
    /// <param name="track">The 1-based track number (1-6).</param>
    /// <param name="enabled">Whether the source outputs to the track.</param>
    public Source SetAudioTrackEnabled(int track, bool enabled = true)
    {
        AudioTracks.ValidateTrack(track);
        var bit = 1u << (track - 1);
        AudioMixers = enabled ? AudioMixers | bit : AudioMixers & ~bit;
        return this;
    }

    /// <summary>
    /// Gets whether this source outputs to the given audio track.
    /// </summary>
    /// <param name="track">The 1-based track number (1-6).</param>
    public bool IsAudioTrackEnabled(int track)
    {
        AudioTracks.ValidateTrack(track);
        return (AudioMixers & (1u << (track - 1))) != 0;
    }

    /// <summary>
    /// Gets or sets the audio sync offset. Positive values delay the source's audio;
    /// use it to align microphones or other devices with the video.
    /// </summary>
    public TimeSpan AudioSyncOffset
    {
        get => TimeSpan.FromTicks(ObsSource.obs_source_get_sync_offset(Handle) / 100);
        set => ObsSource.obs_source_set_sync_offset(Handle, value.Ticks * 100);
    }

    /// <summary>
    /// Gets or sets the stereo balance (0.0 = left, 0.5 = center, 1.0 = right).
    /// </summary>
    public float AudioBalance
    {
        get => ObsSource.obs_source_get_balance_value(Handle);
        set => ObsSource.obs_source_set_balance_value(Handle, Math.Clamp(value, 0f, 1f));
    }

    /// <summary>
    /// Gets or sets how this source's audio is monitored.
    /// Monitoring plays the source's audio through the device configured with
    /// <see cref="Obs.SetAudioMonitoringDevice"/> (e.g. to let the user hear their microphone).
    /// </summary>
    public ObsMonitoringType MonitoringType
    {
        get => ObsSource.obs_source_get_monitoring_type(Handle);
        set => ObsSource.obs_source_set_monitoring_type(Handle, value);
    }

    /// <summary>
    /// Gets or sets whether push-to-talk is enabled for this audio source. When enabled,
    /// the source is muted unless its push-to-talk hotkey is held (subject to the release
    /// <see cref="PushToTalkDelay"/>).
    /// </summary>
    public bool PushToTalkEnabled
    {
        get => ObsSource.obs_source_push_to_talk_enabled(Handle);
        set => ObsSource.obs_source_enable_push_to_talk(Handle, value);
    }

    /// <summary>
    /// Gets or sets the push-to-talk release delay — how long the source stays unmuted
    /// after the hotkey is released.
    /// </summary>
    public TimeSpan PushToTalkDelay
    {
        get => TimeSpan.FromMilliseconds(ObsSource.obs_source_get_push_to_talk_delay(Handle));
        set => ObsSource.obs_source_set_push_to_talk_delay(Handle, (ulong)value.TotalMilliseconds);
    }

    /// <summary>
    /// Gets or sets whether push-to-mute is enabled for this audio source. When enabled,
    /// the source is muted while its push-to-mute hotkey is held (subject to the release
    /// <see cref="PushToMuteDelay"/>).
    /// </summary>
    public bool PushToMuteEnabled
    {
        get => ObsSource.obs_source_push_to_mute_enabled(Handle);
        set => ObsSource.obs_source_enable_push_to_mute(Handle, value);
    }

    /// <summary>
    /// Gets or sets the push-to-mute release delay — how long the source stays muted after
    /// the hotkey is released.
    /// </summary>
    public TimeSpan PushToMuteDelay
    {
        get => TimeSpan.FromMilliseconds(ObsSource.obs_source_get_push_to_mute_delay(Handle));
        set => ObsSource.obs_source_set_push_to_mute_delay(Handle, (ulong)value.TotalMilliseconds);
    }

    /// <summary>
    /// Gets or sets whether this async source (webcam, capture card, media) renders frames
    /// unbuffered — lowering latency at the cost of smoothness. Has no effect on
    /// non-async sources.
    /// </summary>
    public bool AsyncUnbuffered
    {
        get => ObsSource.obs_source_async_unbuffered(Handle);
        set => ObsSource.obs_source_set_async_unbuffered(Handle, value);
    }

    /// <summary>
    /// Gets or sets whether this async source's audio/video timestamps are decoupled from
    /// the OBS clock — useful for devices that produce inconsistent timestamps.
    /// </summary>
    public bool AsyncDecoupled
    {
        get => ObsSource.obs_source_async_decoupled(Handle);
        set => ObsSource.obs_source_set_async_decoupled(Handle, value);
    }

    /// <summary>
    /// Subscribes to this source's audio before mixing (planar 32-bit float at the
    /// OBS output sample rate). The callback runs on OBS's audio thread — do not block.
    /// Dispose the returned subscription to stop receiving audio.
    /// </summary>
    /// <param name="callback">Invoked for each audio block with the source's mute state.</param>
    public Audio.SourceAudioSubscription SubscribeAudio(Audio.SourceAudioCallback callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return new Audio.SourceAudioSubscription(this, callback);
    }

    #region Interaction

    /// <summary>
    /// Sends a mouse click to an interactive source (e.g. a browser source).
    /// Coordinates are in source pixels.
    /// </summary>
    /// <param name="x">The x position within the source.</param>
    /// <param name="y">The y position within the source.</param>
    /// <param name="button">The mouse button.</param>
    /// <param name="buttonUp">False for press, true for release. Send both for a full click.</param>
    /// <param name="clickCount">Click count (2 for double-click).</param>
    /// <param name="modifiers">Modifier keys held during the event.</param>
    public void SendMouseClick(int x, int y, ObsMouseButton button = ObsMouseButton.Left,
        bool buttonUp = false, uint clickCount = 1, ObsInteractionFlags modifiers = ObsInteractionFlags.None)
    {
        var mouseEvent = new ObsMouseEventNative { Modifiers = (uint)modifiers, X = x, Y = y };
        ObsSource.obs_source_send_mouse_click(Handle, ref mouseEvent, (int)button,
            buttonUp ? (byte)1 : (byte)0, clickCount);
    }

    /// <summary>
    /// Sends a mouse move to an interactive source. Coordinates are in source pixels.
    /// </summary>
    /// <param name="x">The x position within the source.</param>
    /// <param name="y">The y position within the source.</param>
    /// <param name="mouseLeave">True if the mouse left the source.</param>
    /// <param name="modifiers">Modifier keys held during the event.</param>
    public void SendMouseMove(int x, int y, bool mouseLeave = false,
        ObsInteractionFlags modifiers = ObsInteractionFlags.None)
    {
        var mouseEvent = new ObsMouseEventNative { Modifiers = (uint)modifiers, X = x, Y = y };
        ObsSource.obs_source_send_mouse_move(Handle, ref mouseEvent, mouseLeave ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Sends a mouse wheel event to an interactive source.
    /// </summary>
    /// <param name="x">The x position within the source.</param>
    /// <param name="y">The y position within the source.</param>
    /// <param name="wheelDeltaY">Vertical scroll amount (positive scrolls up; 120 per notch).</param>
    /// <param name="wheelDeltaX">Horizontal scroll amount.</param>
    /// <param name="modifiers">Modifier keys held during the event.</param>
    public void SendMouseWheel(int x, int y, int wheelDeltaY, int wheelDeltaX = 0,
        ObsInteractionFlags modifiers = ObsInteractionFlags.None)
    {
        var mouseEvent = new ObsMouseEventNative { Modifiers = (uint)modifiers, X = x, Y = y };
        ObsSource.obs_source_send_mouse_wheel(Handle, ref mouseEvent, wheelDeltaX, wheelDeltaY);
    }

    /// <summary>
    /// Sends a got-focus or lost-focus event to an interactive source.
    /// </summary>
    /// <param name="focused">Whether the source gained focus.</param>
    public void SendFocus(bool focused)
    {
        ObsSource.obs_source_send_focus(Handle, focused ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Sends a key event to an interactive source.
    /// </summary>
    /// <param name="text">The text the key produces (e.g. "a"), or null for non-text keys.</param>
    /// <param name="nativeVkey">The platform virtual key code.</param>
    /// <param name="keyUp">False for press, true for release. Send both for a full keystroke.</param>
    /// <param name="nativeScancode">The platform scancode.</param>
    /// <param name="modifiers">Modifier keys held during the event.</param>
    public void SendKeyClick(string? text, uint nativeVkey, bool keyUp,
        uint nativeScancode = 0, ObsInteractionFlags modifiers = ObsInteractionFlags.None)
    {
        var textPtr = text != null ? System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8(text) : 0;
        try
        {
            var keyEvent = new ObsKeyEventNative
            {
                Modifiers = (uint)modifiers,
                Text = textPtr,
                NativeModifiers = 0,
                NativeScancode = nativeScancode,
                NativeVkey = nativeVkey
            };
            ObsSource.obs_source_send_key_click(Handle, ref keyEvent, keyUp ? (byte)1 : (byte)0);
        }
        finally
        {
            if (textPtr != 0)
                System.Runtime.InteropServices.Marshal.FreeCoTaskMem(textPtr);
        }
    }

    #endregion

    /// <summary>
    /// Gets or sets the deinterlacing mode (for interlaced sources such as capture cards).
    /// </summary>
    public ObsDeinterlaceMode DeinterlaceMode
    {
        get => ObsSource.obs_source_get_deinterlace_mode(Handle);
        set => ObsSource.obs_source_set_deinterlace_mode(Handle, value);
    }

    /// <summary>
    /// Gets or sets the deinterlacing field order.
    /// </summary>
    public ObsDeinterlaceFieldOrder DeinterlaceFieldOrder
    {
        get => ObsSource.obs_source_get_deinterlace_field_order(Handle);
        set => ObsSource.obs_source_set_deinterlace_field_order(Handle, value);
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

    /// <summary>
    /// Gets the source's private settings — free-form data saved with the source but
    /// not passed to the source plugin (store your own per-source metadata here).
    /// Dispose when done.
    /// </summary>
    public Settings GetPrivateSettings()
    {
        var handle = ObsSource.obs_source_get_private_settings(Handle);
        if (handle.IsNull)
            throw new InvalidOperationException("Failed to get private settings.");
        return new Settings(handle, ownsHandle: true);
    }

    /// <summary>
    /// Marks the source as showing until the returned scope is disposed. Lazy
    /// capture sources (game capture, window capture) start capturing as if they
    /// were displayed — use this to warm up a game capture hook before recording
    /// starts, or to keep it hooked while not on the program scene.
    /// </summary>
    public IDisposable KeepShowing()
    {
        ObsSource.obs_source_inc_showing(Handle);
        return new ShowingScope(this, active: false);
    }

    /// <summary>
    /// Marks the source as active (as if displayed in the program output, which also
    /// implies showing) until the returned scope is disposed.
    /// </summary>
    public IDisposable KeepActive()
    {
        ObsSource.obs_source_inc_active(Handle);
        return new ShowingScope(this, active: true);
    }

    private sealed class ShowingScope(Source source, bool active) : IDisposable
    {
        private Source? _source = source;

        public void Dispose()
        {
            var source = System.Threading.Interlocked.Exchange(ref _source, null);
            if (source == null)
                return;

            if (active)
                ObsSource.obs_source_dec_active(source.Handle);
            else
                ObsSource.obs_source_dec_showing(source.Handle);
        }
    }

    /// <summary>
    /// Gets a weak reference to this source — remembers the source without keeping
    /// it alive. Upgrade with <see cref="WeakSource.TryGetSource"/>.
    /// Dispose the returned reference when done.
    /// </summary>
    public WeakSource GetWeakReference()
    {
        var weak = ObsSource.obs_source_get_weak_source(Handle);
        if (weak == nint.Zero)
            throw new InvalidOperationException($"Failed to get a weak reference to '{Name}'.");
        return new WeakSource(weak);
    }

    /// <summary>
    /// Serializes this source — type, name, UUID, settings, filters, volume, audio
    /// mixers, sync offset, flags, deinterlacing, and monitoring — for persistence
    /// (e.g. <c>source.Save().SaveToFileSafe(path)</c>). Restore with <see cref="Load"/>.
    /// Dispose the returned settings when done.
    /// </summary>
    public Settings Save()
    {
        var handle = ObsSource.obs_save_source(Handle);
        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to save source '{Name}'.");
        return new Settings(handle, ownsHandle: true);
    }

    /// <summary>
    /// Recreates a source (including its filters and audio state) from data produced
    /// by <see cref="Save"/>. The saved UUID is kept, so load a given saved source
    /// only once per session.
    /// </summary>
    /// <param name="data">The saved source data.</param>
    /// <param name="asPrivate">Create the source as private (not enumerated).</param>
    /// <returns>The restored source. Dispose it when no longer needed.</returns>
    public static Source Load(Settings data, bool asPrivate = false)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(data);

        var handle = asPrivate
            ? ObsSource.obs_load_private_source(data.Handle)
            : ObsSource.obs_load_source(data.Handle);
        if (handle.IsNull)
            throw new InvalidOperationException("Failed to load source from saved data.");
        return new Source(handle, ownsHandle: true);
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
        // Increments the source's strong reference count; the returned handle is the same
        // source and is intentionally discarded.
        ObsSource.obs_source_get_ref(Handle);
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

    /// <summary>
    /// Copies the entire filter chain from another source onto this one. Useful for
    /// applying the same processing (e.g. a mic's noise gate + compressor) to multiple sources.
    /// </summary>
    /// <param name="source">The source whose filters should be copied.</param>
    public void CopyFiltersFrom(Source source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ObsSource.obs_source_copy_filters(Handle, source.Handle);
    }

    /// <summary>
    /// Copies a single existing filter onto this source.
    /// </summary>
    /// <param name="filter">The filter to copy.</param>
    public void CopyFilter(Source filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ObsSource.obs_source_copy_single_filter(Handle, filter.Handle);
    }

    /// <summary>Gets a filter by name.</summary>
    public Source? GetFilter(string name)
    {
        var handle = ObsSource.obs_source_get_filter_by_name(Handle, name);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>Gets the number of filters on this source.</summary>
    public int FilterCount => (int)ObsSource.obs_source_filter_count(Handle);

    /// <summary>
    /// Gets all filters attached to this source, in filter-chain order.
    /// The returned sources hold their own references; dispose them when done.
    /// </summary>
    public IReadOnlyList<Source> GetFilters()
    {
        var filters = new List<Source>();
        ObsSource.EnumFilterCallback callback = (_, child, _) =>
        {
            var handle = ObsSource.obs_source_get_ref(child);
            if (!handle.IsNull)
                filters.Add(new Source(handle, ownsHandle: true));
        };

        ObsSource.obs_source_enum_filters(Handle, callback, nint.Zero);
        GC.KeepAlive(callback);
        return filters;
    }

    /// <summary>
    /// When this source is a filter, gets the source it is directly attached to (its parent
    /// in the filter chain), or null if it is not an attached filter.
    /// </summary>
    public Source? GetFilterParent()
    {
        var parent = ObsSource.obs_filter_get_parent(Handle);
        if (parent.IsNull)
            return null;
        var handle = ObsSource.obs_source_get_ref(parent);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>
    /// When this source is a filter, gets the next target it renders into down the filter
    /// chain (another filter, or the parent source at the end), or null if not applicable.
    /// </summary>
    public Source? GetFilterTarget()
    {
        var target = ObsSource.obs_filter_get_target(Handle);
        if (target.IsNull)
            return null;
        var handle = ObsSource.obs_source_get_ref(target);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>
    /// Moves a filter within this source's filter chain.
    /// </summary>
    /// <param name="filter">The filter to move.</param>
    /// <param name="movement">The direction to move the filter.</param>
    public void SetFilterOrder(Source filter, ObsOrderMovement movement)
    {
        ObsSource.obs_source_filter_set_order(Handle, filter.Handle, movement);
    }

    /// <summary>
    /// Gets the zero-based position of a filter in this source's filter chain, or -1 if the
    /// filter is not attached to this source.
    /// </summary>
    /// <param name="filter">The filter to locate.</param>
    public int GetFilterIndex(Source filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return ObsSource.obs_source_filter_get_index(Handle, filter.Handle);
    }

    /// <summary>
    /// Moves a filter to an absolute zero-based position in this source's filter chain.
    /// </summary>
    /// <param name="filter">The filter to move.</param>
    /// <param name="index">The target position (0 = first/top of the chain).</param>
    public void SetFilterIndex(Source filter, int index)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ObsSource.obs_source_filter_set_index(Handle, filter.Handle, (nuint)index);
    }

    #endregion

    #region Media Controls

    /// <summary>
    /// Gets the media playback state. Returns <see cref="ObsMediaState.None"/> for
    /// sources without media controls.
    /// </summary>
    public ObsMediaState MediaState => ObsSource.obs_source_media_get_state(Handle);

    /// <summary>
    /// Gets the media duration, or <see cref="TimeSpan.Zero"/> if unknown.
    /// </summary>
    public TimeSpan MediaDuration => TimeSpan.FromMilliseconds(ObsSource.obs_source_media_get_duration(Handle));

    /// <summary>
    /// Gets or sets the current media playback position.
    /// </summary>
    public TimeSpan MediaTime
    {
        get => TimeSpan.FromMilliseconds(ObsSource.obs_source_media_get_time(Handle));
        set => ObsSource.obs_source_media_set_time(Handle, (long)value.TotalMilliseconds);
    }

    /// <summary>Resumes media playback.</summary>
    public void PlayMedia() => ObsSource.obs_source_media_play_pause(Handle, false);

    /// <summary>Pauses media playback.</summary>
    public void PauseMedia() => ObsSource.obs_source_media_play_pause(Handle, true);

    /// <summary>Restarts media playback from the beginning.</summary>
    public void RestartMedia() => ObsSource.obs_source_media_restart(Handle);

    /// <summary>Stops media playback.</summary>
    public void StopMedia() => ObsSource.obs_source_media_stop(Handle);

    /// <summary>Skips to the next media item (playlist sources).</summary>
    public void NextMedia() => ObsSource.obs_source_media_next(Handle);

    /// <summary>Skips to the previous media item (playlist sources).</summary>
    public void PreviousMedia() => ObsSource.obs_source_media_previous(Handle);

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

    #region Properties (option lists)

    /// <summary>
    /// Enumerates the items of a list-type property exposed by this source's plugin.
    /// Returns (display name, value) pairs. Used to discover devices, resolutions, etc.
    /// </summary>
    /// <param name="propertyName">The property key (e.g. "video_device_id").</param>
    public IReadOnlyList<(string Name, string Value)> GetListPropertyItems(string propertyName)
    {
        var result = new List<(string, string)>();
        var props = ObsProperties.obs_source_properties(Handle);
        if (props == 0)
            return result;

        try
        {
            var prop = ObsProperties.obs_properties_get(props, propertyName);
            if (prop == 0)
                return result;

            var count = ObsProperties.obs_property_list_item_count(prop);
            for (nuint i = 0; i < count; i++)
            {
                var name = ObsProperties.obs_property_list_item_name(prop, i) ?? string.Empty;
                var value = ObsProperties.obs_property_list_item_string(prop, i) ?? string.Empty;
                result.Add((name, value));
            }
        }
        finally
        {
            ObsProperties.obs_properties_destroy(props);
        }

        return result;
    }

    /// <summary>
    /// Introspects every configurable property exposed by this source's plugin, in display
    /// order, with each property's name, label, type, state, numeric range, and (for list
    /// properties) its selectable items. Useful for building dynamic configuration UIs.
    /// </summary>
    public IReadOnlyList<ObsPropertyInfo> GetProperties()
        => ObsPropertyReader.ReadAllAndDestroy(ObsProperties.obs_source_properties(Handle));

    #endregion

    #region Screenshot

    // OBS graphics constants
    private const int GS_BGRA = 5;
    private const int GS_ZS_NONE = 0;
    private const uint GS_CLEAR_COLOR = 1;
    private const int GS_BLEND_ONE = 1;
    private const int GS_BLEND_ZERO = 0;

    /// <summary>
    /// Captures a screenshot of the source as BGRA pixel data.
    /// Must be called while OBS is initialized and the source is active.
    /// </summary>
    /// <returns>Screenshot data (pixels, width, height) or null if capture fails.</returns>
    public ScreenshotData? TakeScreenshot() => TakeScreenshot(0, 0, 0, 0);

    /// <summary>
    /// Captures a cropped screenshot of the source as BGRA pixel data.
    /// Only the specified region is rendered and transferred from GPU, avoiding
    /// the cost of copying the full frame.
    /// </summary>
    /// <param name="cropX">Left edge of the crop region in source pixels.</param>
    /// <param name="cropY">Top edge of the crop region in source pixels.</param>
    /// <param name="cropWidth">Width of the crop region. 0 = full width.</param>
    /// <param name="cropHeight">Height of the crop region. 0 = full height.</param>
    /// <returns>Screenshot data (pixels, width, height) or null if capture fails.</returns>
    public unsafe ScreenshotData? TakeScreenshot(uint cropX, uint cropY, uint cropWidth, uint cropHeight)
    {
        var srcWidth = Width;
        var srcHeight = Height;
        if (srcWidth == 0 || srcHeight == 0)
            return null;

        // Resolve crop region (0 = full dimension)
        if (cropWidth == 0) cropWidth = srcWidth - cropX;
        if (cropHeight == 0) cropHeight = srcHeight - cropY;

        // Clamp to source bounds
        if (cropX + cropWidth > srcWidth) cropWidth = srcWidth - cropX;
        if (cropY + cropHeight > srcHeight) cropHeight = srcHeight - cropY;
        if (cropWidth == 0 || cropHeight == 0)
            return null;

        var texrender = GsTexRenderHandle.Null;
        var stagesurface = GsStageSurfaceHandle.Null;

        try
        {
            ObsGraphics.obs_enter_graphics();

            try
            {
                texrender = ObsGraphics.gs_texrender_create(GS_BGRA, GS_ZS_NONE);
                if (texrender.IsNull)
                    return null;

                // Texture is only as large as the crop region
                if (!ObsGraphics.gs_texrender_begin(texrender, cropWidth, cropHeight))
                    return null;

                float* clearColor = stackalloc float[4];
                clearColor[0] = 0.0f;
                clearColor[1] = 0.0f;
                clearColor[2] = 0.0f;
                clearColor[3] = 0.0f;
                ObsGraphics.gs_clear(GS_CLEAR_COLOR, clearColor, 0.0f, 0);

                // Project only the crop region into the texture
                ObsGraphics.gs_ortho(cropX, cropX + cropWidth, cropY, cropY + cropHeight, -100.0f, 100.0f);

                ObsGraphics.gs_blend_state_push();
                ObsGraphics.gs_blend_function(GS_BLEND_ONE, GS_BLEND_ZERO);

                ObsGraphics.obs_source_video_render(Handle);

                ObsGraphics.gs_blend_state_pop();
                ObsGraphics.gs_texrender_end(texrender);

                var texture = ObsGraphics.gs_texrender_get_texture(texrender);
                if (texture.IsNull)
                    return null;

                // Staging surface matches the crop size
                stagesurface = ObsGraphics.gs_stagesurface_create(cropWidth, cropHeight, GS_BGRA);
                if (stagesurface.IsNull)
                    return null;

                ObsGraphics.gs_stage_texture(stagesurface, texture);

                if (!ObsGraphics.gs_stagesurface_map(stagesurface, out nint data, out uint linesize))
                    return null;

                try
                {
                    var pixels = new byte[cropWidth * cropHeight * 4];
                    var rowBytes = (int)(cropWidth * 4);

                    for (uint y = 0; y < cropHeight; y++)
                    {
                        Marshal.Copy(data + (nint)(y * linesize), pixels, (int)(y * rowBytes), rowBytes);
                    }

                    return new ScreenshotData(pixels, cropWidth, cropHeight);
                }
                finally
                {
                    ObsGraphics.gs_stagesurface_unmap(stagesurface);
                }
            }
            catch
            {
                // Source may lose its texture during alt-tab; return null gracefully
                return null;
            }
        }
        finally
        {
            if (!stagesurface.IsNull)
                ObsGraphics.gs_stagesurface_destroy(stagesurface);
            if (!texrender.IsNull)
                ObsGraphics.gs_texrender_destroy(texrender);

            ObsGraphics.obs_leave_graphics();
        }
    }

    #endregion

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        Obs.OnSourceDisposed(this);
        ObsSource.obs_source_release((ObsSourceHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Source[{TypeId}]: {Name}";

    #region Capabilities, Kind and Visibility

    /// <summary>
    /// Gets the capability flags of this source instance (video, audio, async, composite, ...).
    /// </summary>
    public ObsSourceFlags OutputFlags => (ObsSourceFlags)ObsSource.obs_source_get_output_flags(Handle);

    /// <summary>Gets whether this source is a scene (see <see cref="Scenes.Scene.FromSource"/>).</summary>
    public bool IsScene => ObsSource.obs_source_is_scene(Handle);

    /// <summary>Gets whether this source is a scene group.</summary>
    public bool IsGroup => ObsSource.obs_source_is_group(Handle);

    /// <summary>Gets whether a source type id denotes a scene.</summary>
    public static bool IsSceneType(string typeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeId);
        return ObsSource.obs_source_type_is_scene(typeId);
    }

    /// <summary>Gets whether a source type id denotes a group.</summary>
    public static bool IsGroupType(string typeId)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeId);
        return ObsSource.obs_source_type_is_group(typeId);
    }

    /// <summary>
    /// Gets or sets whether the source is hidden from UI lists (it still renders and is still
    /// enumerated; this is only a presentation hint stored with the source).
    /// </summary>
    public bool IsHidden
    {
        get => ObsSource.obs_source_is_hidden(Handle);
        set => ObsSource.obs_source_set_hidden(Handle, value);
    }

    /// <summary>Gets whether the source type exposes configurable properties.</summary>
    public bool IsConfigurable => ObsSource.obs_source_configurable(Handle);

    /// <summary>
    /// Gets the icon category the source type declares (see <see cref="Obs.GetSourceIconType"/>).
    /// </summary>
    public ObsIconType IconType => TypeId != null ? ObsCore.obs_source_get_icon_type(TypeId) : ObsIconType.Unknown;

    /// <summary>
    /// Gets the OBS version the source's settings were last saved with, as "major.minor.patch",
    /// or null if unknown. Plugins use it to migrate old settings.
    /// </summary>
    public string? LastObsVersion
    {
        get
        {
            var v = ObsSource.obs_source_get_last_obs_version(Handle);
            return v == 0 ? null : $"{v >> 24}.{(v >> 16) & 0xFF}.{v & 0xFFFF}";
        }
    }

    /// <summary>
    /// Sets the default flags the source starts with before user flags are applied
    /// (OBS_SOURCE_FLAG_* values, see <see cref="Flags"/>).
    /// </summary>
    public void SetDefaultFlags(uint flags) => ObsSource.obs_source_set_default_flags(Handle, flags);

    #endregion

    #region Settings, Properties and Rendering Hints

    /// <summary>
    /// Replaces the source's settings wholesale (existing keys not present in
    /// <paramref name="settings"/> are removed) and updates the source. Unlike
    /// <see cref="Update(Settings)"/>, which merges.
    /// </summary>
    public void ResetSettings(Settings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ObsSource.obs_source_reset_settings(Handle, settings.Handle);
    }

    /// <summary>
    /// Raises the source's <c>update_properties</c> signal, telling property UIs to re-read
    /// <see cref="GetProperties"/> (e.g. after a device list changed).
    /// </summary>
    public void NotifyPropertiesChanged() => ObsSource.obs_source_update_properties(Handle);

    /// <summary>
    /// Sets the rotation applied to async video frames (webcams, capture cards), in degrees.
    /// Only 0, 90, 180 and 270 are meaningful.
    /// </summary>
    public void SetAsyncRotation(int degrees) => ObsSource.obs_source_set_async_rotation(Handle, degrees);

    /// <summary>
    /// Gets whether the source renders with centered texel coordinates (a hint that it will
    /// blend texels, used by the compositor to pick a sampling mode).
    /// </summary>
    public bool TexcoordsCentered => ObsSource.obs_source_get_texcoords_centered(Handle);

    /// <summary>
    /// Gets the color space the source renders in, picking the first of
    /// <paramref name="preferredSpaces"/> it can honor (or the source's native space if none
    /// match). Pass nothing to get the native space.
    /// </summary>
    public GsColorSpace GetColorSpace(params GsColorSpace[] preferredSpaces)
    {
        if (preferredSpaces == null || preferredSpaces.Length == 0)
            return ObsSource.obs_source_get_color_space(Handle, 0, nint.Zero);

        var pinned = GCHandle.Alloc(preferredSpaces, GCHandleType.Pinned);
        try
        {
            return ObsSource.obs_source_get_color_space(Handle, (nuint)preferredSpaces.Length, pinned.AddrOfPinnedObject());
        }
        finally
        {
            pinned.Free();
        }
    }

    /// <summary>
    /// Gets the canvas this source belongs to (OBS 32+), or null for sources not bound to a
    /// canvas. Dispose the returned canvas when done.
    /// </summary>
    public Scenes.Canvas? GetCanvas()
    {
        var handle = ObsSource.obs_source_get_canvas(Handle);
        return handle.IsNull ? null : new Scenes.Canvas(handle);
    }

    #endregion

    #region Audio State

    /// <summary>Gets the speaker layout of the source's audio (Unknown for video-only sources).</summary>
    public SpeakerLayout SpeakerLayout => ObsSource.obs_source_get_speaker_layout(Handle);

    /// <summary>Gets the timestamp (ns) of the most recent audio the source produced.</summary>
    public ulong AudioTimestamp => ObsSource.obs_source_get_audio_timestamp(Handle);

    /// <summary>Gets whether the source has audio buffered that has not been mixed yet.</summary>
    public bool IsAudioPending => ObsSource.obs_source_audio_pending(Handle);

    /// <summary>
    /// Gets or sets whether the source is currently producing audio (as shown by OBS's mixer
    /// panel). libobs manages this for most sources; setting it is meant for sources whose
    /// audio activity you control yourself.
    /// </summary>
    public bool IsAudioActive
    {
        get => ObsSource.obs_source_audio_active(Handle);
        set => ObsSource.obs_source_set_audio_active(Handle, value);
    }

    #endregion

    #region Source Trees

    /// <summary>
    /// Gets the sources this composite source (scene, group, transition) is actively showing,
    /// one level deep. Empty for non-composite sources. Dispose the results when done.
    /// </summary>
    public List<Source> GetActiveChildren() => EnumerateTree(ObsSource.obs_source_enum_active_sources);

    /// <summary>
    /// Gets the entire active source tree under this source, depth first (children before
    /// their parents). Dispose the results when done.
    /// </summary>
    public List<Source> GetActiveTree() => EnumerateTree(ObsSource.obs_source_enum_active_tree);

    /// <summary>
    /// Gets the entire source tree under this source including hidden scene items, depth first.
    /// Dispose the results when done.
    /// </summary>
    public List<Source> GetFullTree() => EnumerateTree(ObsSource.obs_source_enum_full_tree);

    private List<Source> EnumerateTree(Action<ObsSourceHandle, ObsSource.EnumFilterCallback, nint> enumerate)
    {
        var result = new List<Source>();
        ObsSource.EnumFilterCallback callback = (_, child, _) =>
        {
            var refd = ObsSource.obs_source_get_ref(child);
            if (!refd.IsNull)
                result.Add(new Source(refd, ownsHandle: true));
        };
        enumerate(Handle, callback, nint.Zero);
        GC.KeepAlive(callback);
        return result;
    }

    #endregion

    #region Filter Backup and Missing Files

    /// <summary>
    /// Serializes every filter on the source (type, settings, order, enabled state) into an
    /// array that <see cref="RestoreFilters"/> can apply later, e.g. to undo filter edits.
    /// Dispose the array when done.
    /// </summary>
    public SettingsArray BackupFilters()
    {
        var array = ObsSource.obs_source_backup_filters(Handle);
        if (array.IsNull)
            throw new InvalidOperationException("Failed to back up filters.");
        return new SettingsArray(array, ownsHandle: true);
    }

    /// <summary>
    /// Restores filters from a <see cref="BackupFilters"/> array: filters with matching names
    /// are updated and reordered, missing ones are recreated, and extra ones are removed.
    /// </summary>
    public void RestoreFilters(SettingsArray backup)
    {
        ArgumentNullException.ThrowIfNull(backup);
        ObsSource.obs_source_restore_filters(Handle, backup.Handle);
    }

    /// <summary>
    /// Asks the source which of its files are missing (e.g. a media or image path that no
    /// longer exists). Each entry can be pointed at a replacement with
    /// <see cref="MissingFile.Resolve"/>. Dispose the collection when done.
    /// </summary>
    public MissingFileCollection GetMissingFiles()
        => new(ObsSource.obs_source_get_missing_files(Handle));

    #endregion

    #region Async Frames

    /// <summary>
    /// Takes the newest video frame an async source (webcam, capture card, media source, ...)
    /// has queued but not yet displayed, copied into managed memory, or null if none is
    /// pending. Frames are consumed: each one is returned at most once, and taking a frame
    /// removes it from the source's display queue. For rendered output of any source (including
    /// synchronous ones) use <see cref="TakeScreenshot()"/> instead.
    /// </summary>
    public unsafe SourceFrame? TryGetAsyncFrame()
    {
        var frame = ObsSource.obs_source_get_frame(Handle);
        if (frame == nint.Zero)
            return null;
        try
        {
            return new SourceFrame((ObsSourceFrameNative*)frame);
        }
        finally
        {
            ObsSource.obs_source_release_frame(Handle, frame);
        }
    }

    #endregion
}
