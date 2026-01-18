using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using ObsKit.NET.Core;
using ObsKit.NET.Exceptions;
using ObsKit.NET.Native;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Platform.Windows.Interop;

namespace ObsKit.NET;

/// <summary>
/// Manages the OBS context lifecycle. Dispose this object to shut down OBS.
/// </summary>
public sealed class ObsContext : IDisposable
{
    private bool _disposed;
    private bool _comInitialized;
    private readonly ObsConfiguration _config;
    private ObsCore.LogHandlerDelegate? _logHandler;

    internal ObsContext(ObsConfiguration config)
    {
        _config = config;
        Initialize();
    }

    private void Initialize()
    {
        // Initialize library loader
        LibraryLoader.Initialize();

        // Setup logging if configured
        if (_config.LogHandler != null)
        {
            SetupLogging(_config.LogHandler);
        }

        // Initialize COM in MTA mode on Windows (required for DXGI and capture sources)
        if (OperatingSystem.IsWindows())
        {
            InitializeComForWindows();
        }

        // Initialize OBS core
        if (!ObsCore.obs_startup(_config.Locale, _config.ModuleConfigPath, 0))
        {
            throw new ObsInitializationException("obs_startup failed");
        }

        // Add data path if specified
        // Note: OBS's check_path function concatenates path + filename directly,
        // so the path must end with a trailing slash.
        if (!string.IsNullOrEmpty(_config.DataPath))
        {
            var dataPath = _config.DataPath;
            if (!dataPath.EndsWith('/') && !dataPath.EndsWith('\\'))
            {
                dataPath += '/';
            }
            ObsCore.obs_add_data_path(dataPath);
        }

        // Add module paths
        // Note: Module paths may also need trailing slashes depending on usage
        foreach (var (bin, data) in _config.ModulePaths)
        {
            var binPath = bin;
            var dataPath = data;
            if (!binPath.EndsWith('/') && !binPath.EndsWith('\\'))
                binPath += '/';
            if (!dataPath.EndsWith('/') && !dataPath.EndsWith('\\'))
                dataPath += '/';
            ObsCore.obs_add_module_path(binPath, dataPath);
        }

        // Reset video
        ResetVideo(shutdownOnFailure: true);

        // Reset audio
        ResetAudio(shutdownOnFailure: true);

        // Load modules
        LoadModules();
    }

    private void LoadModules()
    {
        if (_config.ModulePaths.Count == 0)
            return;

        if (_config.ExcludedModules.Count == 0)
        {
            // No exclusions - use the fast path
            ObsCore.obs_load_all_modules();
        }
        else
        {
            // Selective loading - enumerate and load modules individually
            foreach (var (binPath, dataPathTemplate) in _config.ModulePaths)
            {
                LoadModulesFromDirectory(binPath, dataPathTemplate);
            }
        }

        ObsCore.obs_post_load_modules();
    }

    private void LoadModulesFromDirectory(string binPath, string dataPathTemplate)
    {
        if (!Directory.Exists(binPath))
            return;

        // Get all module files based on platform
        var extension = OperatingSystem.IsWindows() ? "*.dll" :
                        OperatingSystem.IsMacOS() ? "*.so" : // macOS OBS plugins use .so, not .dylib
                        "*.so";
        var moduleFiles = Directory.GetFiles(binPath, extension);

        foreach (var modulePath in moduleFiles)
        {
            var moduleName = Path.GetFileNameWithoutExtension(modulePath);

            // Skip excluded modules
            if (_config.ExcludedModules.Contains(moduleName))
                continue;

            // Skip known non-module DLLs
            if (IsNonModuleDll(moduleName))
                continue;

            // Build the data path for this module
            var dataPath = dataPathTemplate.Replace("%module%", moduleName, StringComparison.OrdinalIgnoreCase);

            // Try to load the module
            var result = ObsCore.obs_open_module(out var module, modulePath, dataPath);
            if (result == 0 && module != 0)
            {
                ObsCore.obs_init_module(module);
            }
        }
    }

    private static bool IsNonModuleDll(string name)
    {
        // List of known DLLs that are not OBS modules
        return name.Equals("chrome_elf", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("libcef", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("libEGL", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("libGLESv2", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("d3dcompiler", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("vk_swiftshader", StringComparison.OrdinalIgnoreCase);
    }

    private void ResetVideo(bool shutdownOnFailure = false)
    {
        var graphicsModule = _config.Video.GraphicsModule ?? VideoSettings.GetDefaultGraphicsModule();
        var graphicsModulePtr = Marshal.StringToHGlobalAnsi(graphicsModule);

        try
        {
            var ovi = new ObsVideoInfo
            {
                GraphicsModule = graphicsModulePtr,
                FpsNum = _config.Video.FpsNumerator,
                FpsDen = _config.Video.FpsDenominator,
                BaseWidth = _config.Video.BaseWidth,
                BaseHeight = _config.Video.BaseHeight,
                OutputWidth = _config.Video.OutputWidth,
                OutputHeight = _config.Video.OutputHeight,
                OutputFormat = _config.Video.Format,
                Adapter = _config.Video.Adapter,
                GpuConversion = _config.Video.GpuConversion,
                Colorspace = _config.Video.Colorspace,
                Range = _config.Video.Range,
                ScaleType = _config.Video.ScaleType
            };

            var result = ObsCore.obs_reset_video(ref ovi);
            if (result != 0)
            {
                if (shutdownOnFailure)
                    ObsCore.obs_shutdown();
                throw new ObsVideoResetException(result);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(graphicsModulePtr);
        }
    }

    private void ResetAudio(bool shutdownOnFailure = false)
    {
        var oai = new ObsAudioInfo
        {
            SamplesPerSec = _config.Audio.SampleRate,
            Speakers = _config.Audio.Speakers
        };

        if (!ObsCore.obs_reset_audio(ref oai))
        {
            if (shutdownOnFailure)
                ObsCore.obs_shutdown();
            throw new ObsAudioResetException();
        }
    }

    private void SetupLogging(Action<ObsLogLevel, string> handler)
    {
        // Keep a reference to prevent GC
        _logHandler = (level, format, args, param) =>
        {
            try
            {
                // For now, just pass the format string - proper va_list handling is complex
                var message = Marshal.PtrToStringUTF8(format) ?? string.Empty;
                handler((ObsLogLevel)level, message);
            }
            catch
            {
                // Don't let exceptions escape from the callback
            }
        };

        ObsCore.base_set_log_handler(_logHandler, 0);
    }

    [SupportedOSPlatform("windows")]
    private void InitializeComForWindows()
    {
        // Initialize COM in MTA mode for DXGI/WGC capture sources
        // This must happen before OBS initializes its capture plugins
        var hr = Ole32.CoInitializeEx(0, Ole32.COINIT_MULTITHREADED | Ole32.COINIT_DISABLE_OLE1DDE);

        if (hr == Ole32.S_OK)
        {
            // We successfully initialized COM - track it for cleanup
            _comInitialized = true;
        }
        else if (hr == Ole32.S_FALSE)
        {
            // COM was already initialized in MTA mode - that's fine
            _comInitialized = true;
        }
        // If hr == RPC_E_CHANGED_MODE, COM is already initialized in STA mode
        // OBS will handle this internally - capture may still work via WGC
    }

    /// <summary>
    /// Gets the OBS version as a packed integer.
    /// </summary>
    public uint VersionNumber => ObsCore.obs_get_version();

    /// <summary>
    /// Gets the OBS version string.
    /// </summary>
    public string VersionString => ObsCore.obs_get_version_string();

    /// <summary>
    /// Changes video settings after initialization. Uses the same options as WithVideo() during init.
    /// Do not call while recording or streaming - stop outputs first.
    /// </summary>
    /// <param name="configure">Configuration action for video settings.</param>
    public void SetVideo(Action<VideoSettings> configure)
    {
        configure(_config.Video);
        ResetVideo();
    }

    /// <summary>
    /// Changes audio settings after initialization. Uses the same options as WithAudio() during init.
    /// Do not call while recording or streaming - stop outputs first.
    /// </summary>
    /// <param name="configure">Configuration action for audio settings.</param>
    public void SetAudio(Action<AudioSettings> configure)
    {
        configure(_config.Audio);
        ResetAudio();
    }

    /// <summary>
    /// Disposes the OBS context and shuts down OBS.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        // Clear log handler
        if (_logHandler != null)
        {
            ObsCore.base_set_log_handler(null!, 0);
            _logHandler = null;
        }

        // Clear all output source channels (0-5) to release scene references
        for (uint i = 0; i < 6; i++)
        {
            ObsCore.obs_set_output_source(i, default);
        }

        // Reset singleton collections before shutdown
        Sources.SourceCollection.Reset();
        Scenes.SceneCollection.Reset();

        // Shutdown OBS
        ObsCore.obs_shutdown();

        // Uninitialize COM if we initialized it
        if (_comInitialized && OperatingSystem.IsWindows())
        {
            UninitializeComForWindows();
        }

        // Notify Obs class that context was disposed
        Obs.OnContextDisposed();

        _disposed = true;
    }

    [SupportedOSPlatform("windows")]
    private void UninitializeComForWindows()
    {
        Ole32.CoUninitialize();
        _comInitialized = false;
    }
}
