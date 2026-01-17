using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for core OBS functions (initialization, shutdown, video/audio configuration).
/// </summary>
internal static partial class ObsCore
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Initialization and Shutdown

    /// <summary>
    /// Initializes the OBS core context.
    /// </summary>
    public static bool obs_startup(string locale, string? moduleConfigPath, nint store)
        => obs_startup_native(locale, moduleConfigPath, store) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_startup")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_startup_native(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string locale,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string? moduleConfigPath,
        nint store);

    /// <summary>
    /// Shuts down the OBS core context and releases all resources.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_shutdown")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_shutdown();

    /// <summary>
    /// Checks if OBS is currently initialized.
    /// </summary>
    public static bool obs_initialized() => obs_initialized_native() != 0;

    [LibraryImport(Lib, EntryPoint = "obs_initialized")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_initialized_native();

    #endregion

    #region Version

    /// <summary>
    /// Gets the OBS version as a packed integer.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_version")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_get_version();

    /// <summary>
    /// Gets the OBS version as a string.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_version_string")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string obs_get_version_string();

    #endregion

    #region Video Configuration

    /// <summary>
    /// Resets the video subsystem with the specified settings.
    /// </summary>
    /// <param name="ovi">Video initialization structure.</param>
    /// <returns>Result code (0 = success, negative = error).</returns>
    [LibraryImport(Lib, EntryPoint = "obs_reset_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_reset_video(ref ObsVideoInfo ovi);

    /// <summary>
    /// Gets the current video subsystem handle.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoHandle obs_get_video();

    #endregion

    #region Audio Configuration

    /// <summary>
    /// Resets the audio subsystem with the specified settings.
    /// </summary>
    public static bool obs_reset_audio(ref ObsAudioInfo oai) => obs_reset_audio_native(ref oai) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_reset_audio")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_reset_audio_native(ref ObsAudioInfo oai);

    /// <summary>
    /// Resets the audio subsystem with extended settings.
    /// </summary>
    public static bool obs_reset_audio2(ref ObsAudioInfo2 oai) => obs_reset_audio2_native(ref oai) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_reset_audio2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_reset_audio2_native(ref ObsAudioInfo2 oai);

    /// <summary>
    /// Gets the current audio subsystem handle.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_audio")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial AudioHandle obs_get_audio();

    #endregion

    #region Module Loading

    /// <summary>
    /// Adds a directory path to search for OBS data files.
    /// Note: OBS stores the path pointer directly without copying, so we use a persistent marshaler.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_add_data_path")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_add_data_path(
        [MarshalUsing(typeof(Utf8StringMarshalerPersistent))] string path);

    /// <summary>
    /// Adds module search paths for plugins.
    /// Note: OBS stores the path pointers directly without copying, so we use a persistent marshaler.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_add_module_path")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_add_module_path(
        [MarshalUsing(typeof(Utf8StringMarshalerPersistent))] string binPath,
        [MarshalUsing(typeof(Utf8StringMarshalerPersistent))] string dataPath);

    /// <summary>
    /// Loads all available modules/plugins.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_load_all_modules")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_load_all_modules();

    /// <summary>
    /// Loads all modules from a specific directory with optional exclusions.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_load_all_modules2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_load_all_modules2(ref ObsModuleFailureInfo failureInfo);

    /// <summary>
    /// Opens a module from a file path.
    /// </summary>
    /// <returns>0 on success, error code on failure.</returns>
    [LibraryImport(Lib, EntryPoint = "obs_open_module")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_open_module(
        out nint module,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string path,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string dataPath);

    /// <summary>
    /// Initializes an opened module.
    /// </summary>
    /// <returns>true if successful.</returns>
    public static bool obs_init_module(nint module) => obs_init_module_native(module) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_init_module")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_init_module_native(nint module);

    /// <summary>
    /// Called after all modules are loaded to perform post-load initialization.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_post_load_modules")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_post_load_modules();

    /// <summary>
    /// Logs all loaded modules.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_log_loaded_modules")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_log_loaded_modules();

    #endregion

    #region Output Channels

    /// <summary>
    /// Sets the source for a specific output channel.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_set_output_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_set_output_source(uint channel, ObsSourceHandle source);

    /// <summary>
    /// Gets the source for a specific output channel.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_output_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_get_output_source(uint channel);

    #endregion

    #region Type Enumeration

    /// <summary>
    /// Enumerates available source types.
    /// </summary>
    public static bool obs_enum_source_types(nuint idx, out nint id) => obs_enum_source_types_native(idx, out id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_enum_source_types")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_enum_source_types_native(nuint idx, out nint id);

    /// <summary>
    /// Enumerates available input source types.
    /// </summary>
    public static bool obs_enum_input_types(nuint idx, out nint id) => obs_enum_input_types_native(idx, out id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_enum_input_types")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_enum_input_types_native(nuint idx, out nint id);

    /// <summary>
    /// Enumerates available filter types.
    /// </summary>
    public static bool obs_enum_filter_types(nuint idx, out nint id) => obs_enum_filter_types_native(idx, out id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_enum_filter_types")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_enum_filter_types_native(nuint idx, out nint id);

    /// <summary>
    /// Enumerates available transition types.
    /// </summary>
    public static bool obs_enum_transition_types(nuint idx, out nint id) => obs_enum_transition_types_native(idx, out id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_enum_transition_types")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_enum_transition_types_native(nuint idx, out nint id);

    /// <summary>
    /// Enumerates available output types.
    /// </summary>
    public static bool obs_enum_output_types(nuint idx, out nint id) => obs_enum_output_types_native(idx, out id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_enum_output_types")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_enum_output_types_native(nuint idx, out nint id);

    /// <summary>
    /// Enumerates available encoder types.
    /// </summary>
    public static bool obs_enum_encoder_types(nuint idx, out nint id) => obs_enum_encoder_types_native(idx, out id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_enum_encoder_types")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_enum_encoder_types_native(nuint idx, out nint id);

    /// <summary>
    /// Enumerates available service types.
    /// </summary>
    public static bool obs_enum_service_types(nuint idx, out nint id) => obs_enum_service_types_native(idx, out id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_enum_service_types")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_enum_service_types_native(nuint idx, out nint id);

    #endregion

    #region Logging

    /// <summary>
    /// Delegate for OBS log handler.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void LogHandlerDelegate(int level, nint format, nint args, nint param);

    /// <summary>
    /// Sets the log handler for OBS messages.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "base_set_log_handler")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void base_set_log_handler(LogHandlerDelegate handler, nint param);

    #endregion
}
