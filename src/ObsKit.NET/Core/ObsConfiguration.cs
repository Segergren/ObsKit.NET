using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Core;

/// <summary>
/// Fluent configuration builder for OBS initialization.
/// </summary>
public sealed class ObsConfiguration
{
    internal string Locale { get; private set; } = "en-US";
    internal string? ModuleConfigPath { get; private set; }
    internal VideoSettings Video { get; } = new();
    internal AudioSettings Audio { get; } = new();
    internal string? DataPath { get; private set; }
    internal List<(string Bin, string Data)> ModulePaths { get; } = [];
    internal Action<ObsLogLevel, string>? LogHandler { get; private set; }
    internal HashSet<string> ExcludedModules { get; } = new(StringComparer.OrdinalIgnoreCase);
    internal bool LoadModulesBeforeVideo { get; private set; } = false;

    /// <summary>
    /// Sets the locale for OBS (e.g., "en-US").
    /// </summary>
    public ObsConfiguration WithLocale(string locale)
    {
        Locale = locale ?? throw new ArgumentNullException(nameof(locale));
        return this;
    }

    /// <summary>
    /// Sets the module configuration path.
    /// </summary>
    public ObsConfiguration WithModuleConfigPath(string path)
    {
        ModuleConfigPath = path;
        return this;
    }

    /// <summary>
    /// Configures video settings.
    /// </summary>
    public ObsConfiguration WithVideo(Action<VideoSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Video);
        return this;
    }

    /// <summary>
    /// Configures audio settings.
    /// </summary>
    public ObsConfiguration WithAudio(Action<AudioSettings> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Audio);
        return this;
    }

    /// <summary>
    /// Sets the OBS data path (where libobs data files are located).
    /// </summary>
    public ObsConfiguration WithDataPath(string path)
    {
        DataPath = path ?? throw new ArgumentNullException(nameof(path));
        return this;
    }

    /// <summary>
    /// Adds a module search path for plugins.
    /// </summary>
    /// <param name="binPath">Path to plugin binaries (e.g., "obs-plugins/64bit").</param>
    /// <param name="dataPath">Path to plugin data (e.g., "data/obs-plugins/%module%").</param>
    public ObsConfiguration WithModulePath(string binPath, string dataPath)
    {
        ArgumentNullException.ThrowIfNull(binPath);
        ArgumentNullException.ThrowIfNull(dataPath);
        ModulePaths.Add((binPath, dataPath));
        return this;
    }

    /// <summary>
    /// Sets a custom log handler for OBS messages.
    /// </summary>
    public ObsConfiguration WithLogging(Action<ObsLogLevel, string> handler)
    {
        LogHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        return this;
    }

    /// <summary>
    /// Excludes a module from being loaded by name (without extension).
    /// </summary>
    /// <param name="moduleName">The module name without extension (e.g., "obs-browser").</param>
    public ObsConfiguration ExcludeModule(string moduleName)
    {
        ArgumentNullException.ThrowIfNull(moduleName);
        ExcludedModules.Add(moduleName);
        return this;
    }

    /// <summary>
    /// Excludes the browser source module (obs-browser).
    /// This module uses Chromium/CEF and can cause hangs in headless/CLI applications.
    /// </summary>
    public ObsConfiguration ExcludeBrowserSource()
    {
        ExcludedModules.Add("obs-browser");
        return this;
    }

    /// <summary>
    /// Excludes the frontend tools module (frontend-tools).
    /// This module requires the OBS frontend API which is not available in standalone libobs usage.
    /// </summary>
    public ObsConfiguration ExcludeFrontendTools()
    {
        ExcludedModules.Add("frontend-tools");
        return this;
    }

    /// <summary>
    /// Excludes the WebSocket module (obs-websocket).
    /// This module may cause issues in standalone libobs usage without the frontend.
    /// </summary>
    public ObsConfiguration ExcludeWebSocket()
    {
        ExcludedModules.Add("obs-websocket");
        return this;
    }

    /// <summary>
    /// Configures the library for headless/CLI operation by excluding modules that
    /// require a GUI or cause issues without the OBS frontend.
    /// Excludes: obs-browser, frontend-tools, obs-websocket
    /// </summary>
    public ObsConfiguration ForHeadlessOperation()
    {
        ExcludeBrowserSource();
        ExcludeFrontendTools();
        ExcludeWebSocket();
        return this;
    }

    /// <summary>
    /// Loads modules before initializing video/audio subsystems.
    /// This is NOT the recommended order per OBS documentation, but may be required
    /// for DXGI Desktop Duplication to work correctly in some configurations
    /// (particularly when COM is initialized in STA mode by the host application).
    /// </summary>
    public ObsConfiguration WithModulesLoadedFirst()
    {
        LoadModulesBeforeVideo = true;
        return this;
    }
}

/// <summary>
/// Video configuration settings for OBS initialization.
/// </summary>
public sealed class VideoSettings
{
    /// <summary>
    /// Base canvas width (source resolution).
    /// </summary>
    public uint BaseWidth { get; set; } = 1920;

    /// <summary>
    /// Base canvas height (source resolution).
    /// </summary>
    public uint BaseHeight { get; set; } = 1080;

    /// <summary>
    /// Output width (target resolution).
    /// </summary>
    public uint OutputWidth { get; set; } = 1920;

    /// <summary>
    /// Output height (target resolution).
    /// </summary>
    public uint OutputHeight { get; set; } = 1080;

    /// <summary>
    /// FPS numerator.
    /// </summary>
    public uint FpsNumerator { get; set; } = 60;

    /// <summary>
    /// FPS denominator.
    /// </summary>
    public uint FpsDenominator { get; set; } = 1;

    /// <summary>
    /// Output video format.
    /// </summary>
    public VideoFormat Format { get; set; } = VideoFormat.NV12;

    /// <summary>
    /// Video colorspace.
    /// </summary>
    public VideoColorspace Colorspace { get; set; } = VideoColorspace.Default;

    /// <summary>
    /// Video range type.
    /// </summary>
    public VideoRangeType Range { get; set; } = VideoRangeType.Default;

    /// <summary>
    /// Scale type for resizing.
    /// </summary>
    public ObsScaleType ScaleType { get; set; } = ObsScaleType.Bilinear;

    /// <summary>
    /// GPU adapter index (0 = default).
    /// </summary>
    public uint Adapter { get; set; } = 0;

    /// <summary>
    /// Use GPU for color conversion.
    /// </summary>
    public bool GpuConversion { get; set; } = true;

    /// <summary>
    /// Graphics module to use. Auto-detected if null.
    /// </summary>
    public string? GraphicsModule { get; set; }

    /// <summary>
    /// Sets both base and output resolution.
    /// </summary>
    public VideoSettings Resolution(uint width, uint height)
    {
        BaseWidth = width;
        BaseHeight = height;
        OutputWidth = width;
        OutputHeight = height;
        return this;
    }

    /// <summary>
    /// Sets the base (canvas) resolution.
    /// </summary>
    public VideoSettings BaseResolution(uint width, uint height)
    {
        BaseWidth = width;
        BaseHeight = height;
        return this;
    }

    /// <summary>
    /// Sets the output (target) resolution.
    /// </summary>
    public VideoSettings OutputResolution(uint width, uint height)
    {
        OutputWidth = width;
        OutputHeight = height;
        return this;
    }

    /// <summary>
    /// Sets the frame rate.
    /// </summary>
    public VideoSettings Fps(uint numerator, uint denominator = 1)
    {
        FpsNumerator = numerator;
        FpsDenominator = denominator;
        return this;
    }

    /// <summary>
    /// Sets the GPU adapter index to use.
    /// </summary>
    /// <param name="adapterIndex">The adapter index (0 = default).</param>
    public VideoSettings WithAdapter(uint adapterIndex)
    {
        Adapter = adapterIndex;
        return this;
    }

    /// <summary>
    /// Gets the default graphics module for the current platform.
    /// </summary>
    internal static string GetDefaultGraphicsModule()
    {
        if (OperatingSystem.IsWindows())
            return "libobs-d3d11";
        if (OperatingSystem.IsMacOS())
            return "libobs-opengl"; // Metal is used internally but loaded via opengl module
        return "libobs-opengl";
    }
}

/// <summary>
/// Audio configuration settings for OBS initialization.
/// </summary>
public sealed class AudioSettings
{
    /// <summary>
    /// Sample rate in Hz.
    /// </summary>
    public uint SampleRate { get; set; } = 48000;

    /// <summary>
    /// Speaker layout configuration.
    /// </summary>
    public SpeakerLayout Speakers { get; set; } = SpeakerLayout.Stereo;

    /// <summary>
    /// Sets the sample rate.
    /// </summary>
    public AudioSettings WithSampleRate(uint sampleRate)
    {
        SampleRate = sampleRate;
        return this;
    }

    /// <summary>
    /// Sets the speaker layout.
    /// </summary>
    public AudioSettings WithSpeakers(SpeakerLayout speakers)
    {
        Speakers = speakers;
        return this;
    }
}
