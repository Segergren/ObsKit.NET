using ObsKit.NET.Core;
using ObsKit.NET.Exceptions;
using ObsKit.NET.Native;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
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

    /// <summary>
    /// Gets whether OBS is currently initialized.
    /// </summary>
    public static bool IsInitialized => ObsCore.obs_initialized();

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
    /// </summary>
    public static void Shutdown()
    {
        lock (_lock)
        {
            if (_context == null)
                return;

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
    /// Sets a source for an output channel. OBS uses channels 0-5 for different purposes:
    /// Channel 0: Primary video source (scene/game capture)
    /// Channel 1: Secondary video (display capture fallback)
    /// Channels 2-5: Audio sources (microphone, desktop audio, etc.)
    /// </summary>
    /// <param name="channel">The output channel (0-5).</param>
    /// <param name="source">The source to assign, or null to clear the channel.</param>
    public static void SetOutputSource(uint channel, Source? source)
    {
        ThrowIfNotInitialized();
        var handle = source != null ? (ObsSourceHandle)(nint)source.NativeHandle : ObsSourceHandle.Null;
        ObsCore.obs_set_output_source(channel, handle);
    }

    /// <summary>
    /// Clears a source from an output channel.
    /// </summary>
    /// <param name="channel">The output channel to clear (0-5).</param>
    public static void ClearOutputSource(uint channel)
    {
        ThrowIfNotInitialized();
        ObsCore.obs_set_output_source(channel, ObsSourceHandle.Null);
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
