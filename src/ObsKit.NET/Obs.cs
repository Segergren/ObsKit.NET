using System.Runtime.InteropServices;
using ObsKit.NET.Audio;
using ObsKit.NET.Core;
using ObsKit.NET.Encoders;
using ObsKit.NET.Exceptions;
using ObsKit.NET.Hotkeys;
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
    /// Gets the current audio settings (sample rate, speaker layout, buffering),
    /// or null if audio is not initialized.
    /// </summary>
    public static ObsAudioInfo2? GetAudioInfo()
    {
        ThrowIfNotInitialized();

        var oai = default(ObsAudioInfo2);
        return ObsCore.obs_get_audio_info2(ref oai) ? oai : null;
    }

    /// <summary>
    /// Gets the duration of one video frame at the current frame rate
    /// (e.g. ~16.67 ms at 60 FPS).
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static TimeSpan FrameInterval
    {
        get
        {
            ThrowIfNotInitialized();
            return TimeSpan.FromTicks((long)(ObsCore.obs_get_frame_interval_ns() / 100));
        }
    }

    /// <summary>
    /// Gets the timestamp in nanoseconds of the video frame currently being rendered —
    /// the same clock raw video/audio frame timestamps use.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static ulong VideoFrameTimestamp
    {
        get
        {
            ThrowIfNotInitialized();
            return ObsCore.obs_get_video_frame_time();
        }
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
    /// Queues an action to run on one of OBS's internal threads — e.g.
    /// <see cref="ObsTaskType.Graphics"/> to touch graphics resources safely, or
    /// <see cref="ObsTaskType.Destroy"/> to defer cleanup. If already on the target
    /// thread the action runs immediately.
    /// </summary>
    /// <param name="type">The target thread. <see cref="ObsTaskType.Ui"/> is not
    /// supported — libobs requires an <c>obs_set_ui_task_handler</c> callback
    /// (not exposed by this library) to marshal onto the host application's UI
    /// thread; without one, native code never invokes the queued action, and for
    /// <paramref name="wait"/> = false that would silently leak the callback.</param>
    /// <param name="action">The action to run. Exceptions are swallowed.</param>
    /// <param name="wait">True to block until the action has executed.</param>
    public static void QueueTask(ObsTaskType type, Action action, bool wait = false)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(action);
        if (type == ObsTaskType.Ui)
            throw new NotSupportedException(
                "ObsTaskType.Ui requires a UI task handler, which this library does not expose. " +
                "Use Graphics, Audio, or Destroy instead.");

        if (wait)
        {
            // Synchronous: the delegate cannot be collected while we block on the call.
            ObsCore.TaskCallback callback = _ =>
            {
                try { action(); } catch { /* don't let exceptions escape into native code */ }
            };
            ObsCore.obs_queue_task(type, callback, nint.Zero, 1);
            GC.KeepAlive(callback);
            return;
        }

        // Fire-and-forget: root the delegate until the task has run, otherwise the GC
        // could collect it before OBS's thread invokes the function pointer.
        GCHandle gcHandle = default;
        ObsCore.TaskCallback deferred = _ =>
        {
            try { action(); }
            catch { /* don't let exceptions escape into native code */ }
            finally { gcHandle.Free(); }
        };
        gcHandle = GCHandle.Alloc(deferred);
        ObsCore.obs_queue_task(type, deferred, nint.Zero, 0);
    }

    /// <summary>
    /// Gets whether the calling thread is the given OBS task thread.
    /// </summary>
    /// <param name="type">The task thread to test.</param>
    public static bool IsInTaskThread(ObsTaskType type)
    {
        ThrowIfNotInitialized();
        return ObsCore.obs_in_task_thread(type);
    }

    /// <summary>
    /// Blocks until all pending asynchronous source destroys have completed —
    /// useful before tearing down or when measuring resource usage.
    /// </summary>
    /// <returns>False if OBS has no destroy thread.</returns>
    public static bool WaitForDestroyQueue()
    {
        ThrowIfNotInitialized();
        return ObsCore.obs_wait_for_destroy_queue();
    }

    /// <summary>
    /// Gets whether video output is active — true while any output that uses a
    /// video mix (recording, streaming, virtual camera) is running.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static bool IsVideoActive
    {
        get
        {
            ThrowIfNotInitialized();
            return ObsCore.obs_video_active();
        }
    }

    /// <summary>
    /// Gets whether audio monitoring is supported by the platform's audio backend.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static bool IsAudioMonitoringAvailable
    {
        get
        {
            ThrowIfNotInitialized();
            return ObsCore.obs_audio_monitoring_available();
        }
    }

    /// <summary>
    /// Tears down and reinitializes audio monitoring for all sources — useful after
    /// the monitoring device changes or becomes unavailable.
    /// </summary>
    /// <exception cref="ObsNotInitializedException">Thrown if OBS is not initialized.</exception>
    public static void ResetAudioMonitoring()
    {
        ThrowIfNotInitialized();
        ObsCore.obs_reset_audio_monitoring();
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
    /// Registers an application-level hotkey. Bind key combinations with
    /// <see cref="RegisteredHotkey.Bind"/>, then feed key events from your input
    /// hook to <see cref="InjectHotkeyEvent"/> — OBS matches them against the
    /// bindings and invokes <paramref name="onChanged"/>.
    /// Keep the returned object alive; dispose it to unregister.
    /// </summary>
    /// <param name="name">The internal hotkey name (unique within your app).</param>
    /// <param name="description">The user-facing description.</param>
    /// <param name="onChanged">Invoked with true on press and false on release,
    /// on the thread that injects the key event.</param>
    public static RegisteredHotkey RegisterHotkey(string name, string description, Action<bool> onChanged)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(onChanged);

        return new RegisteredHotkey(name, description, onChanged,
            callback => ObsHotkey.obs_hotkey_register_frontend(name, description, callback, nint.Zero));
    }

    /// <summary>
    /// Registers a pair of mutually-exclusive application hotkeys (e.g. start/stop).
    /// When both are bound to the same key combination, only the handler that returns
    /// true consumes the press — so a single key can toggle between the two actions.
    /// </summary>
    /// <param name="primaryName">Internal name of the first hotkey.</param>
    /// <param name="primaryDescription">User-facing description of the first hotkey.</param>
    /// <param name="secondaryName">Internal name of the second hotkey.</param>
    /// <param name="secondaryDescription">User-facing description of the second hotkey.</param>
    /// <param name="onPrimary">Invoked with true on press/false on release; return true if the event took effect.</param>
    /// <param name="onSecondary">Invoked with true on press/false on release; return true if the event took effect.</param>
    public static RegisteredHotkeyPair RegisterHotkeyPair(
        string primaryName, string primaryDescription,
        string secondaryName, string secondaryDescription,
        Func<bool, bool> onPrimary, Func<bool, bool> onSecondary)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(primaryName);
        ArgumentException.ThrowIfNullOrEmpty(secondaryName);
        ArgumentNullException.ThrowIfNull(primaryDescription);
        ArgumentNullException.ThrowIfNull(secondaryDescription);
        ArgumentNullException.ThrowIfNull(onPrimary);
        ArgumentNullException.ThrowIfNull(onSecondary);

        return new RegisteredHotkeyPair(
            primaryName, primaryDescription, secondaryName, secondaryDescription, onPrimary, onSecondary);
    }

    /// <summary>
    /// Feeds a key press/release into the hotkey system. OBS matches it against all
    /// bindings and fires the affected hotkey callbacks synchronously on this thread.
    /// Usually not needed: libobs polls global key state on its own hotkey thread
    /// (every 25 ms), so bound hotkeys already work system-wide. Use this to feed
    /// events from a custom input path (e.g. a game overlay or remote control).
    /// Convert OS virtual key codes with <see cref="ObsKeys.FromVirtualKey"/>.
    /// </summary>
    /// <param name="combination">The key (and currently held modifiers).</param>
    /// <param name="pressed">True for press, false for release.</param>
    public static void InjectHotkeyEvent(ObsKeyCombination combination, bool pressed)
    {
        ThrowIfNotInitialized();
        ObsHotkey.obs_hotkey_inject_event(combination, pressed ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Replaces the key combinations bound to any hotkey by id — including hotkeys
    /// registered by libobs itself (e.g. a source's push-to-talk hotkey from
    /// <see cref="EnumerateHotkeys"/>). Pass no combinations to clear the bindings.
    /// </summary>
    /// <param name="id">The hotkey id (from <see cref="EnumerateHotkeys"/> or <see cref="RegisteredHotkey.Id"/>).</param>
    /// <param name="combinations">The key combinations that should trigger the hotkey.</param>
    public static void BindHotkey(ulong id, params ObsKeyCombination[] combinations)
    {
        ThrowIfNotInitialized();
        RegisteredHotkey.BindCore((nuint)id, combinations);
    }

    /// <summary>
    /// Gets the key combinations currently bound to a hotkey.
    /// </summary>
    /// <param name="id">The hotkey id (from <see cref="EnumerateHotkeys"/> or <see cref="RegisteredHotkey.Id"/>).</param>
    public static IReadOnlyList<ObsKeyCombination> GetHotkeyBindings(ulong id)
    {
        ThrowIfNotInitialized();
        return RegisteredHotkey.GetBindingsForId(id);
    }

    /// <summary>
    /// Controls whether libobs's background polling thread may deliver key PRESS
    /// events (releases are always delivered). Enabled by default — hotkeys fire even
    /// while your app is not focused. Disable to only fire presses you explicitly
    /// feed through <see cref="InjectHotkeyEvent"/>.
    /// </summary>
    /// <param name="enable">True to deliver background presses.</param>
    public static void EnableHotkeyBackgroundPress(bool enable = true)
    {
        ThrowIfNotInitialized();
        ObsHotkey.obs_hotkey_enable_background_press(enable ? (byte)1 : (byte)0);
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

            modules.Add(ReadModuleInfo(module));
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

    #region Frame Hooks

    /// <summary>
    /// Registers a per-frame tick callback. It runs on OBS's graphics thread once per video
    /// frame, before sources are rendered, with the seconds elapsed since the previous tick.
    /// Keep the callback fast; dispose the returned subscription to remove it.
    /// </summary>
    /// <param name="callback">Invoked with the elapsed seconds since the last tick.</param>
    public static TickSubscription SubscribeTick(Action<float> callback)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(callback);
        return new TickSubscription(callback);
    }

    /// <summary>
    /// Registers a callback that runs on the graphics thread right after the main canvas is
    /// composited, with the graphics context active and the main texture bound as the render
    /// target. Anything drawn here (via the graphics API) lands in every output that uses the
    /// main canvas, which makes it the hook for custom overlays. Dispose to remove.
    /// </summary>
    /// <param name="draw">Invoked with the canvas base width and height.</param>
    public static MainRenderSubscription SubscribeMainRender(Action<uint, uint> draw)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(draw);
        return new MainRenderSubscription(draw);
    }

    /// <summary>
    /// Registers a callback that fires on the graphics thread once every canvas has finished
    /// rendering a frame (a lightweight "frame done" signal). Dispose to remove.
    /// </summary>
    public static MainRenderedSubscription SubscribeMainRendered(Action rendered)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(rendered);
        return new MainRenderedSubscription(rendered);
    }

    #endregion

    #region Type and Protocol Discovery

    /// <summary>
    /// Enumerates input source types together with their unversioned ids, e.g.
    /// ("color_source_v3", "color_source"). Use the unversioned id for stable lookups across
    /// OBS versions and <see cref="GetLatestInputTypeId"/> to resolve it back.
    /// </summary>
    public static IReadOnlyList<(string Id, string UnversionedId)> EnumerateInputTypesWithVersions()
    {
        ThrowIfNotInitialized();
        var result = new List<(string, string)>();
        for (nuint i = 0; ObsCore.obs_enum_input_types2(i, out var id, out var unversioned); i++)
        {
            var idStr = Marshal.PtrToStringUTF8(id);
            if (idStr == null)
                continue;
            result.Add((idStr, Marshal.PtrToStringUTF8(unversioned) ?? idStr));
        }
        return result;
    }

    /// <summary>
    /// Resolves an unversioned input type id (e.g. "color_source") to the newest registered
    /// versioned id (e.g. "color_source_v3"), or null if no such type is registered.
    /// </summary>
    public static string? GetLatestInputTypeId(string unversionedId)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(unversionedId);
        return ObsCore.obs_get_latest_input_type_id(unversionedId);
    }

    /// <summary>
    /// Enumerates the streaming protocols registered by loaded output plugins
    /// (e.g. "RTMP", "RTMPS", "SRT", "RIST", "WHIP").
    /// </summary>
    public static IReadOnlyList<string> EnumerateOutputProtocols()
    {
        ThrowIfNotInitialized();
        var result = new List<string>();
        for (nuint i = 0; ObsCore.obs_enum_output_protocols(i, out var ptr); i++)
        {
            var protocol = Marshal.PtrToStringUTF8(ptr);
            if (protocol != null)
                result.Add(protocol);
        }
        return result;
    }

    /// <summary>
    /// Gets whether any loaded output plugin registered the given protocol (case-sensitive,
    /// e.g. "RTMPS" or "SRT").
    /// </summary>
    public static bool IsOutputProtocolRegistered(string protocol)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(protocol);
        return ObsCore.obs_is_output_protocol_registered(protocol);
    }

    /// <summary>
    /// Enumerates the output type ids that can stream with the given protocol, in
    /// registration order (e.g. "rtmp_output" for "RTMP").
    /// </summary>
    public static IReadOnlyList<string> EnumerateOutputTypesForProtocol(string protocol)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(protocol);

        var result = new List<string>();
        ObsCore.EnumOutputTypesWithProtocolCallback callback = (_, id) =>
        {
            var idStr = Marshal.PtrToStringUTF8(id);
            if (idStr != null)
                result.Add(idStr);
            return 1;
        };
        ObsCore.obs_enum_output_types_with_protocol(protocol, nint.Zero, callback);
        GC.KeepAlive(callback);
        return result;
    }

    /// <summary>
    /// Gets whether a source type exposes configurable properties (false for e.g. scenes and
    /// groups, or types that are not registered).
    /// </summary>
    public static bool IsSourceTypeConfigurable(string typeId)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(typeId);
        return ObsCore.obs_is_source_configurable(typeId);
    }

    /// <summary>
    /// Gets the icon category a source type declares, for picking a glyph in source lists.
    /// Returns <see cref="ObsIconType.Unknown"/> for unregistered types.
    /// </summary>
    public static ObsIconType GetSourceIconType(string typeId)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(typeId);
        return ObsCore.obs_source_get_icon_type(typeId);
    }

    #endregion

    #region Module Discovery

    /// <summary>
    /// Lists every plugin module file present in the configured module search paths, whether
    /// or not it was loaded (compare with <see cref="GetLoadedModules"/>). Useful for showing
    /// which plugins are installed, or for building an exclusion list before initialization.
    /// </summary>
    public static IReadOnlyList<ObsModuleLocation> FindModules()
    {
        ThrowIfNotInitialized();

        var modules = new List<ObsModuleLocation>();
        ObsCore.FindModuleCallback2 callback = (_, infoPtr) =>
        {
            if (infoPtr == nint.Zero)
                return;
            var info = Marshal.PtrToStructure<ObsModuleInfo2Native>(infoPtr);
            var name = Marshal.PtrToStringUTF8(info.Name);
            var bin = Marshal.PtrToStringUTF8(info.BinPath);
            if (name == null || bin == null)
                return;
            modules.Add(new ObsModuleLocation(name, bin, Marshal.PtrToStringUTF8(info.DataPath) ?? string.Empty));
        };
        ObsCore.obs_find_modules2(callback, nint.Zero);
        GC.KeepAlive(callback);
        return modules;
    }

    /// <summary>
    /// Gets a loaded module by name (file name without extension, e.g. "obs-browser"),
    /// or null if no such module is loaded.
    /// </summary>
    public static ObsModuleInfo? GetLoadedModule(string name)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(name);

        var module = ObsCore.obs_get_module(name);
        if (module == nint.Zero)
            return null;
        return ReadModuleInfo(module);
    }

    /// <summary>
    /// Gets whether a module by that name is on the disabled-modules list
    /// (see <see cref="AddDisabledModule"/>).
    /// </summary>
    public static bool IsModuleDisabled(string name)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(name);
        return ObsCore.obs_get_disabled_module(name) != nint.Zero;
    }

    private static ObsModuleInfo ReadModuleInfo(nint module)
        => new(
            ObsCore.obs_get_module_file_name(module) ?? string.Empty,
            ObsCore.obs_get_module_name(module),
            ObsCore.obs_get_module_author(module),
            ObsCore.obs_get_module_description(module),
            ObsCore.obs_get_module_binary_path(module),
            ObsCore.obs_get_module_data_path(module));

    /// <summary>
    /// Adds a module to the safe-mode allow list. When the list is non-empty, only listed
    /// modules load. Must be called before modules are loaded (i.e. from the
    /// <c>ObsConfiguration</c> callback or before <c>Obs.Initialize</c> loads modules).
    /// </summary>
    /// <param name="name">The module name (file name without extension).</param>
    public static void AddSafeModule(string name)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(name);
        ObsCore.obs_add_safe_module(name);
    }

    /// <summary>
    /// Marks a module as a core module that cannot be disabled by
    /// <see cref="AddDisabledModule"/>. Must be called before modules are loaded.
    /// </summary>
    /// <param name="name">The module name (file name without extension).</param>
    public static void AddCoreModule(string name)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(name);
        ObsCore.obs_add_core_module(name);
    }

    /// <summary>
    /// Adds a module to the disabled list so the module loader skips it (it is still
    /// discoverable via <see cref="FindModules"/>). Must be called before modules are loaded.
    /// </summary>
    /// <param name="name">The module name (file name without extension).</param>
    public static void AddDisabledModule(string name)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(name);
        ObsCore.obs_add_disabled_module(name);
    }

    /// <summary>
    /// Gets whether a module may be disabled (false for modules registered as core modules).
    /// </summary>
    public static bool IsModuleDisableAllowed(string name)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(name);
        return ObsCore.obs_get_module_allow_disable(name);
    }

    /// <summary>
    /// Resolves a file relative to the libobs data directories (e.g. an effect or LUT shipped
    /// with libobs), returning its full path or null if not found.
    /// </summary>
    public static string? FindDataFile(string file)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(file);

        var ptr = ObsCore.obs_find_data_file(file);
        if (ptr == nint.Zero)
            return null;
        try
        {
            return Marshal.PtrToStringUTF8(ptr);
        }
        finally
        {
            ObsSignal.bfree(ptr);
        }
    }

    /// <summary>
    /// Resolves a file inside a loaded module's data directory, returning its full path or
    /// null if the module is not loaded or the file does not exist.
    /// </summary>
    /// <param name="moduleName">The module name (file name without extension).</param>
    /// <param name="file">The file path relative to the module's data directory.</param>
    public static string? FindModuleFile(string moduleName, string file)
    {
        ThrowIfNotInitialized();
        ArgumentException.ThrowIfNullOrEmpty(moduleName);
        ArgumentException.ThrowIfNullOrEmpty(file);

        var module = ObsCore.obs_get_module(moduleName);
        if (module == nint.Zero)
            return null;

        var ptr = ObsCore.obs_find_module_file(module, file);
        if (ptr == nint.Zero)
            return null;
        try
        {
            return Marshal.PtrToStringUTF8(ptr);
        }
        finally
        {
            ObsSignal.bfree(ptr);
        }
    }

    #endregion

    #region Private Data

    /// <summary>
    /// Gets a reference to the global private data object: arbitrary app state that plugins
    /// and the host can share (OBS's frontend uses it for the current scene collection name,
    /// etc.). Dispose the returned object when done; changes made through it are live.
    /// </summary>
    public static Settings GetPrivateData()
    {
        ThrowIfNotInitialized();
        return new Settings(ObsCore.obs_get_private_data(), ownsHandle: true);
    }

    /// <summary>
    /// Replaces the global private data with a copy of <paramref name="settings"/>
    /// (pass null to clear it).
    /// </summary>
    public static void SetPrivateData(Settings? settings)
    {
        ThrowIfNotInitialized();
        ObsCore.obs_set_private_data(settings?.Handle ?? default);
    }

    /// <summary>
    /// Merges <paramref name="settings"/> into the global private data.
    /// </summary>
    public static void ApplyPrivateData(Settings settings)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(settings);
        ObsCore.obs_apply_private_data(settings.Handle);
    }

    #endregion

    #region Bulk Source Persistence

    /// <summary>
    /// Serializes every public, non-filter source (scenes and their items included) into an
    /// array, like an OBS scene collection. Filters attached to sources are saved with their
    /// parent. Restore with <see cref="LoadSources"/>. Dispose the returned array when done.
    /// </summary>
    /// <param name="filter">Optional predicate; return false to skip a source. The source
    /// passed in is a temporary reference valid only during the call.</param>
    public static SettingsArray SaveSources(Func<Source, bool>? filter = null)
    {
        ThrowIfNotInitialized();

        if (filter == null)
            return new SettingsArray(ObsCore.obs_save_sources(), ownsHandle: true);

        ObsCore.SaveSourceFilterCallback callback = (_, handle) =>
        {
            var refd = ObsSource.obs_source_get_ref(handle);
            if (refd.IsNull)
                return 0;
            using var source = new Source(refd, ownsHandle: true);
            try
            {
                return filter(source) ? (byte)1 : (byte)0;
            }
            catch
            {
                return 0;
            }
        };
        var array = ObsCore.obs_save_sources_filtered(callback, nint.Zero);
        GC.KeepAlive(callback);
        return new SettingsArray(array, ownsHandle: true);
    }

    /// <summary>
    /// Creates every source described in <paramref name="data"/> (as produced by
    /// <see cref="SaveSources"/>), restoring settings, filters, and scene items, then runs
    /// each source's load hook. Returns an owning reference to each loaded source in array
    /// order; dispose them when your own bookkeeping no longer needs them (scene items keep
    /// their sources alive, so disposing does not destroy a source that is still used by a
    /// scene). Sources are matched to existing ones by UUID, so loading the same data twice
    /// in one session mints new UUIDs for the duplicates.
    /// </summary>
    public static List<Source> LoadSources(SettingsArray data)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(data);

        var loaded = new List<Source>();
        ObsCore.LoadSourceCallback callback = (_, handle) =>
        {
            // libobs drops its own reference after the load completes.
            var refd = ObsSource.obs_source_get_ref(handle);
            if (refd.IsNull)
                return;
            loaded.Add(new Source(refd, ownsHandle: true));
        };
        ObsCore.obs_load_sources(data.Handle, callback, nint.Zero);
        GC.KeepAlive(callback);
        return loaded;
    }

    #endregion
}
