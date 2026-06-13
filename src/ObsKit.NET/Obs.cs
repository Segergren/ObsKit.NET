using ObsKit.NET.Audio;
using ObsKit.NET.Core;
using ObsKit.NET.Encoders;
using ObsKit.NET.Exceptions;
using ObsKit.NET.Native;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Outputs;
using ObsKit.NET.Scenes;
using ObsKit.NET.Sources;
using ObsKit.NET.Video;

namespace ObsKit.NET;

/// <summary>
/// Main entry point for the ObsKit.NET library.
/// Provides static access to OBS functionality.
/// </summary>
public static class Obs
{
    private static ObsContext? _context;
    private static readonly object _lock = new();

    // Tracking for auto-management
    private static readonly Dictionary<uint, Source> _channelSources = new();
    private static readonly List<Output> _managedOutputs = new();

    /// <summary>
    /// Gets whether OBS is currently initialized.
    /// </summary>
    public static bool IsInitialized => ObsCore.obs_initialized();

    /// <summary>
    /// Gets or sets whether to automatically dispose sources and outputs on Shutdown.
    /// Default is true.
    /// </summary>
    public static bool AutoDispose { get; set; } = true;

    /// <summary>
    /// Gets all sources currently assigned to output channels.
    /// </summary>
    public static IReadOnlyDictionary<uint, Source> ChannelSources => _channelSources;

    /// <summary>
    /// Gets all outputs being managed.
    /// </summary>
    public static IReadOnlyList<Output> ManagedOutputs => _managedOutputs;

    /// <summary>
    /// Gets the OBS version string.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static string Version
    {
        get
        {
            ThrowIfNotInitialized();
            return _context!.VersionString;
        }
    }

    /// <summary>
    /// Gets the OBS version as a packed integer.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static uint VersionNumber
    {
        get
        {
            ThrowIfNotInitialized();
            return _context!.VersionNumber;
        }
    }

    /// <summary>
    /// Gets or sets the locale (e.g. "en-US") used for localized strings such as source
    /// display names and property descriptions. Set this before creating sources whose
    /// labels you want localized.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static string Locale
    {
        get
        {
            ThrowIfNotInitialized();
            return ObsCore.obs_get_locale() ?? "en-US";
        }
        set
        {
            ThrowIfNotInitialized();
            ArgumentException.ThrowIfNullOrEmpty(value);
            ObsCore.obs_set_locale(value);
        }
    }

    /// <summary>
    /// Gets the collection of all sources.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static SourceCollection Sources
    {
        get
        {
            ThrowIfNotInitialized();
            return SourceCollection.Instance;
        }
    }

    /// <summary>
    /// Gets the collection of all scenes.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static SceneCollection Scenes
    {
        get
        {
            ThrowIfNotInitialized();
            return SceneCollection.Instance;
        }
    }

    /// <summary>
    /// Initializes OBS with default settings.
    /// </summary>
    /// <returns>The OBS context. Dispose this to shut down OBS.</returns>
    /// <exception cref="InvalidOperationException">Thrown if OBS is already initialized.</exception>
    public static ObsContext Initialize()
    {
        return Initialize(null);
    }

    /// <summary>
    /// Initializes OBS with custom configuration.
    /// </summary>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The OBS context. Dispose this to shut down OBS.</returns>
    /// <exception cref="InvalidOperationException">Thrown if OBS is already initialized.</exception>
    /// <example>
    /// <code>
    /// using var obs = Obs.Initialize(config => config
    ///     .WithVideo(v => v.Resolution(1920, 1080).Fps(60))
    ///     .WithAudio(a => a.WithSampleRate(48000))
    ///     .WithDataPath("C:/Program Files/obs-studio/data/libobs")
    ///     .WithModulePath(
    ///         "C:/Program Files/obs-studio/obs-plugins/64bit",
    ///         "C:/Program Files/obs-studio/data/obs-plugins/%module%"));
    /// </code>
    /// </example>
    public static ObsContext Initialize(Action<ObsConfiguration>? configure)
    {
        lock (_lock)
        {
            if (_context != null)
                throw new InvalidOperationException("OBS is already initialized. Call Obs.Shutdown() first.");

            var config = new ObsConfiguration();
            configure?.Invoke(config);

            _context = new ObsContext(config);
            return _context;
        }
    }

    /// <summary>
    /// Shuts down OBS and releases all resources.
    /// Any remaining outputs will be stopped and sources will be disposed.
    /// </summary>
    public static void Shutdown()
    {
        lock (_lock)
        {
            if (_context == null)
                return;

            // Stop any remaining managed outputs
            foreach (var output in _managedOutputs.ToList())
            {
                try
                {
                    if (output.IsActive)
                        output.Stop();
                }
                catch { /* Ignore errors during cleanup */ }
            }
            _managedOutputs.Clear();

            // Dispose all channel sources
            foreach (var (channel, source) in _channelSources.ToList())
            {
                try
                {
                    ObsCore.obs_set_output_source(channel, ObsSourceHandle.Null);
                    source.Dispose();
                }
                catch { /* Ignore errors during cleanup */ }
            }
            _channelSources.Clear();

            _context.Dispose();
            _context = null;
        }
    }

    /// <summary>
    /// Changes video settings after initialization. Uses the same options as WithVideo() during init.
    /// Do not call while recording or streaming - stop outputs first.
    /// </summary>
    /// <param name="configure">Configuration action for video settings.</param>
    public static void SetVideo(Action<VideoSettings> configure)
    {
        ThrowIfNotInitialized();
        _context!.SetVideo(configure);
    }

    /// <summary>
    /// Changes audio settings after initialization. Uses the same options as WithAudio() during init.
    /// Do not call while recording or streaming - stop outputs first.
    /// </summary>
    /// <param name="configure">Configuration action for audio settings.</param>
    public static void SetAudio(Action<AudioSettings> configure)
    {
        ThrowIfNotInitialized();
        _context!.SetAudio(configure);
    }

    /// <summary>
    /// Sets a source for an output channel. OBS uses channels 0-63 for different purposes:
    /// Channel 0: Primary video source (scene/game capture)
    /// Channel 1: Secondary video (display capture fallback)
    /// Channels 2+: Audio sources (microphone, desktop audio, etc.)
    /// </summary>
    /// <param name="channel">The output channel (0-63).</param>
    /// <param name="source">The source to assign, or null to clear the channel.</param>
    public static void SetOutputSource(uint channel, Source? source)
    {
        ThrowIfNotInitialized();

        lock (_lock)
        {
            // Remove existing source from tracking (but don't dispose - user may still want it)
            _channelSources.Remove(channel);

            if (source != null)
            {
                _channelSources[channel] = source;
                source.AssignedChannel = channel;
            }
        }

        var handle = source != null ? (ObsSourceHandle)(nint)source.NativeHandle : ObsSourceHandle.Null;
        ObsCore.obs_set_output_source(channel, handle);
    }

    /// <summary>
    /// Sets a scene as an output channel's source. Channel 0 is the program output (what
    /// gets recorded/streamed). Pass null to clear the channel.
    /// </summary>
    /// <param name="channel">The output channel (0-63).</param>
    /// <param name="scene">The scene to assign, or null to clear the channel.</param>
    public static void SetOutputSource(uint channel, Scene? scene)
    {
        ThrowIfNotInitialized();

        lock (_lock)
        {
            // Drop any plain-source tracking for this channel; scene cleanup is tracked on the scene.
            _channelSources.Remove(channel);
        }

        if (scene != null)
            scene.AssignToChannel(channel);
        else
            ObsCore.obs_set_output_source(channel, ObsSourceHandle.Null);
    }

    /// <summary>
    /// Sets a scene as the program output (channel 0) — what gets recorded and streamed.
    /// Shorthand for <see cref="SetOutputSource(uint, Scene?)"/> with channel 0.
    /// </summary>
    /// <param name="scene">The scene to make the program output.</param>
    public static void SetOutputSource(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        SetOutputSource(0, scene);
    }

    /// <summary>
    /// Clears a source from an output channel.
    /// </summary>
    /// <param name="channel">The output channel to clear (0-63).</param>
    public static void ClearOutputSource(uint channel)
    {
        ThrowIfNotInitialized();

        lock (_lock)
        {
            if (_channelSources.TryGetValue(channel, out var source))
            {
                source.AssignedChannel = null;
                _channelSources.Remove(channel);
            }
        }

        ObsCore.obs_set_output_source(channel, ObsSourceHandle.Null);
    }

    /// <summary>
    /// Adds an output to be managed. Managed outputs are tracked and can be auto-disposed on Shutdown.
    /// </summary>
    /// <typeparam name="T">The output type.</typeparam>
    /// <param name="output">The output to manage.</param>
    /// <returns>The same output for chaining.</returns>
    public static T AddOutput<T>(T output) where T : Output
    {
        lock (_lock)
        {
            if (!_managedOutputs.Contains(output))
            {
                _managedOutputs.Add(output);
            }
        }
        return output;
    }

    /// <summary>
    /// Called when an output is stopped to remove it from tracking.
    /// </summary>
    internal static void OnOutputStopped(Output output)
    {
        lock (_lock)
        {
            _managedOutputs.Remove(output);
        }
    }

    /// <summary>
    /// Called when a source is disposed to remove it from channel tracking.
    /// </summary>
    internal static void OnSourceDisposed(Source source)
    {
        lock (_lock)
        {
            if (source.AssignedChannel.HasValue)
            {
                _channelSources.Remove(source.AssignedChannel.Value);
            }
        }
    }

    /// <summary>
    /// Called by ObsContext when it's disposed directly (not through Obs.Shutdown).
    /// </summary>
    internal static void OnContextDisposed()
    {
        lock (_lock)
        {
            // obs_shutdown() has already freed every native source/output, so we must NOT
            // dispose the wrappers here (that would obs_*_release dangling handles). Just drop
            // the stale references so a subsequent Initialize()/Shutdown() can't iterate them
            // and release freed handles (use-after-free), and so their finalizers don't either.
            foreach (var source in _channelSources.Values)
                source.AssignedChannel = null;
            _channelSources.Clear();
            _managedOutputs.Clear();
            _context = null;
        }
    }

    /// <summary>
    /// Subscribes to raw video frames produced by OBS's main canvas.
    /// OBS will scale/convert each frame on the GPU to match <paramref name="format"/>/<paramref name="width"/>/<paramref name="height"/>
    /// before invoking the callback on its video thread.
    /// Dispose the returned subscription to stop receiving frames.
    /// </summary>
    /// <param name="format">Desired pixel format. Use <see cref="VideoFormat.BGRA"/> for the simplest CPU-side handling.</param>
    /// <param name="width">Desired output width in pixels. Pass 0 for the canvas width.</param>
    /// <param name="height">Desired output height in pixels. Pass 0 for the canvas height.</param>
    /// <param name="callback">Invoked on OBS's video thread for each delivered frame. Do not block.</param>
    /// <param name="frameRateDivisor">Deliver every Nth frame (1 = every frame, 2 = half rate, etc.).</param>
    /// <param name="colorspace">Color space. <see cref="VideoColorspace.Default"/> inherits the canvas setting.</param>
    /// <param name="range">Color range. <see cref="VideoRangeType.Default"/> inherits the canvas setting.</param>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static RawVideoSubscription SubscribeRawVideo(
        VideoFormat format,
        uint width,
        uint height,
        RawVideoFrameCallback callback,
        uint frameRateDivisor = 1,
        VideoColorspace colorspace = VideoColorspace.Default,
        VideoRangeType range = VideoRangeType.Default)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(callback);
        if (frameRateDivisor == 0)
            throw new ArgumentOutOfRangeException(nameof(frameRateDivisor), "Must be at least 1.");

        // OBS substitutes the canvas OUTPUT resolution for a 0 width/height and then delivers
        // full-size frames, but the native video_data carries no dimensions. Resolve 0 here so
        // RawVideoFrame reports the true size (otherwise Width/Height would be 0 and the plane
        // spans would come back empty over a fully-populated buffer).
        if (width == 0 || height == 0)
        {
            var info = GetVideoInfo();
            if (info.HasValue)
            {
                if (width == 0)
                    width = info.Value.OutputWidth;
                if (height == 0)
                    height = info.Value.OutputHeight;
            }
        }

        var conversion = new VideoScaleInfo
        {
            Format = format,
            Width = width,
            Height = height,
            Colorspace = colorspace,
            Range = range,
        };
        return new RawVideoSubscription(conversion, frameRateDivisor, callback);
    }

    /// <summary>
    /// Subscribes to a track of OBS's mixed audio output. OBS converts the audio to the
    /// requested format/sample rate/layout before invoking the callback on its audio thread.
    /// Dispose the returned subscription to stop receiving audio.
    /// </summary>
    /// <param name="callback">Invoked on OBS's audio thread for each audio block (~every 21 ms at 48 kHz). Do not block.</param>
    /// <param name="track">The 1-based audio track to tap (1-6).</param>
    /// <param name="format">Desired sample format. Defaults to planar 32-bit float (OBS native).</param>
    /// <param name="sampleRate">Desired sample rate in Hz. Pass 0 for the output's rate.</param>
    /// <param name="speakers">Desired speaker layout. <see cref="SpeakerLayout.Unknown"/> uses the output's layout.</param>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static RawAudioSubscription SubscribeRawAudio(
        RawAudioFrameCallback callback,
        int track = 1,
        AudioFormat format = AudioFormat.FloatPlanar,
        uint sampleRate = 0,
        SpeakerLayout speakers = SpeakerLayout.Unknown)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(callback);

        return new RawAudioSubscription(track, format, sampleRate, speakers, callback);
    }

    /// <summary>
    /// Gets the current video settings (canvas/output resolution, frame rate, format),
    /// or null if video is not initialized.
    /// </summary>
    public static ObsVideoInfo? GetVideoInfo()
    {
        ThrowIfNotInitialized();

        var ovi = default(ObsVideoInfo);
        return ObsCore.obs_get_video_info(ref ovi) ? ovi : null;
    }

    /// <summary>
    /// Gets a snapshot of rendering/encoding performance counters
    /// (equivalent to the OBS Studio stats dock). Use it to detect rendering
    /// lag (GPU overload) and encoding lag (encoder overload) while active.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static PerformanceStats GetPerformanceStats()
    {
        ThrowIfNotInitialized();

        var video = ObsCore.obs_get_video();
        var totalOutputFrames = video.IsNull ? 0u : ObsCore.video_output_get_total_frames(video);
        var skippedFrames = video.IsNull ? 0u : ObsCore.video_output_get_skipped_frames(video);

        return new PerformanceStats(
            ObsCore.obs_get_active_fps(),
            ObsCore.obs_get_average_frame_time_ns(),
            ObsCore.obs_get_total_frames(),
            ObsCore.obs_get_lagged_frames(),
            totalOutputFrames,
            skippedFrames);
    }

    /// <summary>
    /// Enumerates the audio devices that can be used for audio monitoring.
    /// </summary>
    /// <returns>A list of (Name, Id) pairs; pass an Id to <see cref="SetAudioMonitoringDevice"/>.</returns>
    public static IReadOnlyList<(string Name, string Id)> EnumerateAudioMonitoringDevices()
    {
        ThrowIfNotInitialized();

        var devices = new List<(string, string)>();
        ObsCore.EnumAudioDeviceCallback callback = (_, namePtr, idPtr) =>
        {
            var name = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(namePtr);
            var id = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(idPtr);
            if (name != null && id != null)
                devices.Add((name, id));
            return 1;
        };

        ObsCore.obs_enum_audio_monitoring_devices(callback, nint.Zero);
        GC.KeepAlive(callback);
        return devices;
    }

    /// <summary>
    /// Sets the output device used for audio monitoring
    /// (sources with <c>AudioMonitoring</c> enabled play through this device).
    /// </summary>
    /// <param name="id">The device ID ("default" for the system default).</param>
    /// <param name="name">The device name for display/logging.</param>
    /// <returns>True if the device was set.</returns>
    public static bool SetAudioMonitoringDevice(string id = "default", string name = "Default")
    {
        ThrowIfNotInitialized();
        return ObsCore.obs_set_audio_monitoring_device(name, id);
    }

    /// <summary>
    /// Gets the current audio monitoring device, or null if none is set.
    /// </summary>
    public static (string Name, string Id)? GetAudioMonitoringDevice()
    {
        ThrowIfNotInitialized();

        ObsCore.obs_get_audio_monitoring_device(out var namePtr, out var idPtr);
        var name = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(namePtr);
        var id = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(idPtr);

        return name != null && id != null ? (name, id) : null;
    }

    /// <summary>
    /// Enumerates all hotkeys registered with OBS (by sources, outputs, etc.).
    /// </summary>
    public static IReadOnlyList<ObsHotkeyInfo> EnumerateHotkeys()
    {
        ThrowIfNotInitialized();

        var result = new List<ObsHotkeyInfo>();
        ObsHotkey.EnumHotkeyCallback callback = (_, id, key) =>
        {
            result.Add(new ObsHotkeyInfo(
                id,
                ObsHotkey.obs_hotkey_get_name(key) ?? string.Empty,
                ObsHotkey.obs_hotkey_get_description(key),
                (ObsHotkeyRegistererType)ObsHotkey.obs_hotkey_get_registerer_type(key)));
            return 1;
        };
        ObsHotkey.obs_enum_hotkeys(callback, nint.Zero);
        GC.KeepAlive(callback);
        return result;
    }

    /// <summary>
    /// Triggers a hotkey's registered action by id (a press followed by a release).
    /// </summary>
    /// <param name="id">The hotkey id from <see cref="EnumerateHotkeys"/>.</param>
    public static void TriggerHotkey(ulong id)
    {
        ThrowIfNotInitialized();

        // Routed triggering only works while rerouting is enabled, but leaving
        // rerouting on without a router function makes libobs silently drop all
        // binding-driven hotkey callbacks — so enable it only around the trigger.
        ObsHotkey.obs_hotkey_enable_callback_rerouting(1);
        try
        {
            ObsHotkey.obs_hotkey_trigger_routed_callback((nuint)id, 1);
            ObsHotkey.obs_hotkey_trigger_routed_callback((nuint)id, 0);
        }
        finally
        {
            ObsHotkey.obs_hotkey_enable_callback_rerouting(0);
        }
    }

    /// <summary>
    /// Triggers a hotkey's registered action by name, optionally scoped to the source
    /// that registered it (several sources can register the same hotkey name).
    /// </summary>
    /// <param name="name">The internal hotkey name (e.g. "hotkey_start").</param>
    /// <param name="owner">When set, only a hotkey registered by this source matches.</param>
    /// <returns>True if a matching hotkey was found and triggered.</returns>
    public static bool TriggerHotkey(string name, Source? owner = null)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(name);

        ulong? found = null;
        ObsHotkey.EnumHotkeyCallback callback = (_, id, key) =>
        {
            if (ObsHotkey.obs_hotkey_get_name(key) != name)
                return 1;

            if (owner != null)
            {
                if ((ObsHotkeyRegistererType)ObsHotkey.obs_hotkey_get_registerer_type(key)
                    != ObsHotkeyRegistererType.Source)
                    return 1;

                // Source hotkeys store a weak source reference as the registerer.
                var strong = ObsHotkey.obs_weak_source_get_source(ObsHotkey.obs_hotkey_get_registerer(key));
                var matches = !strong.IsNull && strong.Value == owner.Handle.Value;
                if (!strong.IsNull)
                    ObsSource.obs_source_release(strong);

                if (!matches)
                    return 1;
            }

            found = id;
            return 0;
        };
        ObsHotkey.obs_enum_hotkeys(callback, nint.Zero);
        GC.KeepAlive(callback);

        if (found == null)
            return false;

        TriggerHotkey(found.Value);
        return true;
    }

    /// <summary>
    /// Gets information about all loaded plugin modules
    /// (useful for diagnostics, e.g. confirming obs-browser or encoder plugins loaded).
    /// </summary>
    public static IReadOnlyList<ObsModuleInfo> GetLoadedModules()
    {
        ThrowIfNotInitialized();

        var modules = new List<ObsModuleInfo>();
        ObsCore.EnumModuleCallback callback = (_, module) =>
        {
            var fileName = ObsCore.obs_get_module_file_name(module);
            if (fileName == null)
                return;

            modules.Add(new ObsModuleInfo(
                fileName,
                ObsCore.obs_get_module_name(module),
                ObsCore.obs_get_module_author(module),
                ObsCore.obs_get_module_description(module)));
        };
        ObsCore.obs_enum_modules(callback, nint.Zero);
        GC.KeepAlive(callback);
        return modules;
    }

    /// <summary>
    /// Enumerates all available source types.
    /// </summary>
    /// <returns>A list of source type IDs.</returns>
    public static IEnumerable<string> EnumerateSourceTypes()
    {
        ThrowIfNotInitialized();
        return EnumerateTypes(ObsCore.obs_enum_source_types);
    }

    /// <summary>
    /// Enumerates all available input source types.
    /// </summary>
    /// <returns>A list of input source type IDs.</returns>
    public static IEnumerable<string> EnumerateInputTypes()
    {
        ThrowIfNotInitialized();
        return EnumerateTypes(ObsCore.obs_enum_input_types);
    }

    /// <summary>
    /// Enumerates all available filter types.
    /// </summary>
    /// <returns>A list of filter type IDs.</returns>
    public static IEnumerable<string> EnumerateFilterTypes()
    {
        ThrowIfNotInitialized();
        return EnumerateTypes(ObsCore.obs_enum_filter_types);
    }

    /// <summary>
    /// Enumerates all available transition types.
    /// </summary>
    /// <returns>A list of transition type IDs.</returns>
    public static IEnumerable<string> EnumerateTransitionTypes()
    {
        ThrowIfNotInitialized();
        return EnumerateTypes(ObsCore.obs_enum_transition_types);
    }

    /// <summary>
    /// Enumerates all available output types.
    /// </summary>
    /// <returns>A list of output type IDs.</returns>
    public static IEnumerable<string> EnumerateOutputTypes()
    {
        ThrowIfNotInitialized();
        return EnumerateTypes(ObsCore.obs_enum_output_types);
    }

    /// <summary>
    /// Enumerates all available encoder types.
    /// </summary>
    /// <returns>A list of encoder type IDs.</returns>
    public static IEnumerable<string> EnumerateEncoderTypes()
    {
        ThrowIfNotInitialized();
        return EnumerateTypes(ObsCore.obs_enum_encoder_types);
    }

    /// <summary>
    /// Enumerates all available service types.
    /// </summary>
    /// <returns>A list of service type IDs.</returns>
    public static IEnumerable<string> EnumerateServiceTypes()
    {
        ThrowIfNotInitialized();
        return EnumerateTypes(ObsCore.obs_enum_service_types);
    }

    private delegate bool EnumTypesDelegate(nuint idx, out nint id);

    private static IEnumerable<string> EnumerateTypes(EnumTypesDelegate enumFunc)
    {
        var types = new List<string>();
        nuint idx = 0;

        while (enumFunc(idx, out var id))
        {
            if (id != 0)
            {
                var str = System.Runtime.InteropServices.Marshal.PtrToStringUTF8(id);
                if (!string.IsNullOrEmpty(str))
                    types.Add(str);
            }
            idx++;
        }

        return types;
    }

    private static void ThrowIfNotInitialized()
    {
        if (!IsInitialized)
            throw new ObsNotInitializedException();
    }
}
