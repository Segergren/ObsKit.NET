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
        LibraryLoader.Initialize();

        if (_config.LogHandler != null)
        {
            SetupLogging(_config.LogHandler);
        }

        // Initialize COM in MTA mode on Windows (required for DXGI and capture sources)
        if (OperatingSystem.IsWindows())
        {
            InitializeComForWindows();
        }

        if (OperatingSystem.IsLinux())
        {
            SetNixPlatform();
        }

        if (!ObsCore.obs_startup(_config.Locale, _config.ModuleConfigPath, 0))
        {
            throw new ObsInitializationException("obs_startup failed");
        }

        // Paths must be absolute (OBS 32.2.0+ inject-helper hardening), end with a slash
        // (OBS's check_path concatenates path + filename directly), and use forward slashes
        // (libobs resolves effect #include directives by splitting on '/' only).
        // Path.GetFullPath keeps the %module% token literal.
        if (!string.IsNullOrEmpty(_config.DataPath))
        {
            ObsCore.obs_add_data_path(NormalizeObsPath(_config.DataPath));
        }

        foreach (var (bin, data) in _config.ModulePaths)
        {
            ObsCore.obs_add_module_path(NormalizeObsPath(bin), NormalizeObsPath(data));
        }

        if (_config.LoadModulesBeforeVideo)
        {
            // Non-standard order: Load modules first (may help with DXGI)
            LoadModules();
            ResetVideo(shutdownOnFailure: true);
            ResetAudio(shutdownOnFailure: true);
        }
        else
        {
            // Standard order per OBS documentation
            ResetVideo(shutdownOnFailure: true);
            ResetAudio(shutdownOnFailure: true);
            LoadModules();
        }
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

    /// <summary>
    /// Resolves a directory path to the form libobs expects: absolute (OBS 32.2.0+
    /// rejects relative module paths), forward slashes (effect #include resolution
    /// splits on '/' only), and a trailing slash (check_path concatenates directly).
    /// </summary>
    private static string NormalizeObsPath(string path)
    {
        var full = Path.GetFullPath(path);
        if (OperatingSystem.IsWindows())
            full = full.Replace('\\', '/');
        if (!full.EndsWith('/'))
            full += '/';
        return full;
    }

    private void LoadModulesFromDirectory(string binPath, string dataPathTemplate)
    {
        binPath = NormalizeObsPath(binPath);
        dataPathTemplate = NormalizeObsPath(dataPathTemplate);

        if (!Directory.Exists(binPath))
            return;

        // Get all module files based on platform
        var extension = OperatingSystem.IsWindows() ? "*.dll" :
                        OperatingSystem.IsMacOS() ? "*.so" : // macOS OBS plugins use .so, not .dylib
                        "*.so";
        var moduleFiles = Directory.GetFiles(binPath, extension);

        foreach (var moduleFile in moduleFiles)
        {
            var modulePath = OperatingSystem.IsWindows() ? moduleFile.Replace('\\', '/') : moduleFile;
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

            // OBS clears these on every obs_reset_video, so re-apply them after a successful reset.
            ObsCore.obs_set_video_levels(_config.Video.SdrWhiteLevel, _config.Video.HdrNominalPeakLevel);
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
                var message = FormatLogMessage(format, args);
                handler((ObsLogLevel)level, message);
            }
            catch
            {
                // Don't let exceptions escape from the callback
            }
        };

        ObsCore.base_set_log_handler(_logHandler, 0);
    }

    // Single-pass so we never reuse the va_list: on the SysV/AMD64 ABI (Linux/macOS) va_list is a
    // mutable struct vsnprintf advances in place, so a size-then-format second pass reads past the
    // arguments. OBS log lines are short; an overlong one truncates into this buffer.
    private const int LogFormatBufferSize = 8192;

    private static string FormatLogMessage(nint format, nint args)
    {
        if (format == nint.Zero)
            return string.Empty;

        var buffer = Marshal.AllocHGlobal(LogFormatBufferSize);
        try
        {
            int written = NativeVsnprintf(buffer, LogFormatBufferSize, format, args);
            if (written < 0)
                return Marshal.PtrToStringUTF8(format) ?? string.Empty;

            return Marshal.PtrToStringUTF8(buffer) ?? string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // vsnprintf lives in a different C runtime per OS: msvcrt on Windows, libc elsewhere (libSystem
    // on macOS). Importing msvcrt everywhere threw DllNotFoundException off Windows, dropping all logs.
    private static int NativeVsnprintf(nint buffer, nuint size, nint format, nint args)
        => OperatingSystem.IsWindows()
            ? NativeVsnprintfWindows(buffer, size, format, args)
            : NativeVsnprintfLibc(buffer, size, format, args);

    [DllImport("msvcrt.dll", EntryPoint = "vsnprintf", CallingConvention = CallingConvention.Cdecl)]
    private static extern int NativeVsnprintfWindows(nint buffer, nuint size, nint format, nint args);

    [DllImport("libc", EntryPoint = "vsnprintf", CallingConvention = CallingConvention.Cdecl)]
    private static extern int NativeVsnprintfLibc(nint buffer, nuint size, nint format, nint args);

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

    [SupportedOSPlatform("linux")]
    private static void SetNixPlatform()
    {
        // libobs defaults to X11, so a Wayland session without a reachable X display
        // (e.g. Flatpak with fallback-x11) fails obs_startup unless we select Wayland here.
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
        {
            nint wlDisplay = 0;
            try
            {
                wlDisplay = Platform.Linux.Interop.WaylandClient.wl_display_connect(0);
            }
            catch (DllNotFoundException)
            {
            }

            if (wlDisplay != 0)
            {
                // The display must be set before obs_startup: the Wayland hotkey init
                // dereferences it without a null check. Never closed; libobs holds it
                // for the process lifetime.
                ObsCore.obs_set_nix_platform((int)ObsNixPlatform.Wayland);
                ObsCore.obs_set_nix_platform_display(wlDisplay);
                return;
            }
        }

        // Hand libobs an X connection like the OBS Studio frontend does; without it,
        // EGL init opens its own and can deadlock the NVIDIA driver against the compositor.
        var display = Platform.Linux.Interop.X11.XOpenDisplay(0);
        if (display != 0)
        {
            // Never closed: libobs holds this pointer for the process lifetime.
            ObsCore.obs_set_nix_platform_display(display);
        }
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
