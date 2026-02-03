using ObsKit.NET.Core;
using ObsKit.NET.Encoders;
using ObsKit.NET.Exceptions;
using ObsKit.NET.Native;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Outputs;
using ObsKit.NET.Scenes;
using ObsKit.NET.Sources;

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
            _context = null;
        }
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
