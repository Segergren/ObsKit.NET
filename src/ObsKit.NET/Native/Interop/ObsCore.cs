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

    /// <summary>Gets the current locale used for localized strings.</summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_locale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_get_locale();

    /// <summary>Sets the locale used for localized strings (display names, property descriptions).</summary>
    [LibraryImport(Lib, EntryPoint = "obs_set_locale")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_set_locale(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string locale);

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
    /// Gets the current video settings.
    /// </summary>
    /// <returns>True if video is initialized.</returns>
    public static bool obs_get_video_info(ref ObsVideoInfo ovi) => obs_get_video_info_native(ref ovi) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_get_video_info")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_get_video_info_native(ref ObsVideoInfo ovi);

    /// <summary>
    /// Gets the current video subsystem handle.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoHandle obs_get_video();

    /// <summary>
    /// Gets the current compositing frame rate.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_active_fps")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial double obs_get_active_fps();

    /// <summary>
    /// Gets the average time to render a frame, in nanoseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_average_frame_time_ns")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong obs_get_average_frame_time_ns();

    /// <summary>
    /// Gets the total number of composited frames.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_total_frames")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_get_total_frames();

    /// <summary>
    /// Gets the number of frames missed due to rendering lag.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_lagged_frames")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_get_lagged_frames();

    /// <summary>
    /// Gets the total number of frames delivered by the video output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "video_output_get_total_frames")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint video_output_get_total_frames(VideoHandle video);

    /// <summary>
    /// Gets the number of frames skipped due to encoding lag.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "video_output_get_skipped_frames")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint video_output_get_skipped_frames(VideoHandle video);

    /// <summary>
    /// Generates a filename from an OBS filename format template (e.g. "%CCYY-%MM-%DD").
    /// </summary>
    internal static string? os_generate_formatted_filename(string extension, bool spaces, string format)
    {
        var ptr = os_generate_formatted_filename_native(extension, spaces ? (byte)1 : (byte)0, format);
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

    [LibraryImport(Lib, EntryPoint = "os_generate_formatted_filename")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial nint os_generate_formatted_filename_native(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string extension,
        byte spaces,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string format);

    /// <summary>
    /// Callback for enumerating audio monitoring devices. Return 0 to stop enumerating.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumAudioDeviceCallback(nint data, nint name, nint id);

    /// <summary>
    /// Enumerates audio devices usable for monitoring.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_audio_monitoring_devices")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_audio_monitoring_devices(EnumAudioDeviceCallback callback, nint data);

    /// <summary>
    /// Sets the device used for audio monitoring.
    /// </summary>
    internal static bool obs_set_audio_monitoring_device(string name, string id)
        => obs_set_audio_monitoring_device_native(name, id) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_set_audio_monitoring_device")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_set_audio_monitoring_device_native(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the device used for audio monitoring.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_audio_monitoring_device")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_get_audio_monitoring_device(out nint name, out nint id);

    /// <summary>
    /// Sets the SDR white level and HDR nominal peak level (nits). Call after a successful
    /// <see cref="obs_reset_video"/>; drives HDR tone-mapping and HDR metadata.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_set_video_levels")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_set_video_levels(float sdrWhiteLevel, float hdrNominalPeakLevel);

    /// <summary>
    /// Gets the SDR white level in nits (returns 300 if no video).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_video_sdr_white_level")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_get_video_sdr_white_level();

    /// <summary>
    /// Gets the HDR nominal peak level in nits (returns 1000 if no video).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_video_hdr_nominal_peak_level")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_get_video_hdr_nominal_peak_level();

    #endregion

    #region Raw Video Callbacks

    /// <summary>
    /// Native callback signature for <c>obs_add_raw_video_callback</c>.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void RawVideoCallbackNative(nint param, nint frame);

    /// <summary>
    /// Subscribes <paramref name="callback"/> to receive raw video frames from the main canvas.
    /// Pass <paramref name="conversion"/> to request a specific format/resolution; pass <see cref="nint.Zero"/> for the canvas defaults.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_add_raw_video_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_add_raw_video_callback(
        nint conversion,
        RawVideoCallbackNative callback,
        nint param);

    /// <summary>
    /// Like <see cref="obs_add_raw_video_callback"/> but only delivers every Nth frame.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_add_raw_video_callback2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_add_raw_video_callback2(
        nint conversion,
        uint frameRateDivisor,
        RawVideoCallbackNative callback,
        nint param);

    /// <summary>
    /// Removes a previously registered raw video callback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_remove_raw_video_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_remove_raw_video_callback(
        RawVideoCallbackNative callback,
        nint param);

    #endregion

    #region Modules

    /// <summary>
    /// Callback for enumerating loaded modules.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void EnumModuleCallback(nint param, nint module);

    /// <summary>
    /// Enumerates all loaded modules.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_modules")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_modules(EnumModuleCallback callback, nint param);

    /// <summary>
    /// Gets the module's file name (e.g. "obs-browser.dll").
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_module_file_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_get_module_file_name(nint module);

    /// <summary>
    /// Gets the module's full name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_module_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_get_module_name(nint module);

    /// <summary>
    /// Gets the module's author(s).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_module_author")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_get_module_author(nint module);

    /// <summary>
    /// Gets the module's description.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_module_description")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_get_module_description(nint module);

    #endregion

    #region Raw Audio Output

    /// <summary>
    /// Native callback signature for <c>audio_output_connect</c>.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void RawAudioCallbackNative(nint param, nuint mixIdx, nint audioData);

    /// <summary>
    /// Subscribes a callback to a mix of the audio output, converting to the requested format.
    /// </summary>
    public static bool audio_output_connect(AudioHandle audio, nuint mixIdx, ref AudioConvertInfo conversion,
        RawAudioCallbackNative callback, nint param)
        => audio_output_connect_native(audio, mixIdx, ref conversion, callback, param) != 0;

    [LibraryImport(Lib, EntryPoint = "audio_output_connect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte audio_output_connect_native(AudioHandle audio, nuint mixIdx,
        ref AudioConvertInfo conversion, RawAudioCallbackNative callback, nint param);

    /// <summary>
    /// Removes a previously connected raw audio callback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "audio_output_disconnect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void audio_output_disconnect(AudioHandle audio, nuint mixIdx,
        RawAudioCallbackNative callback, nint param);

    /// <summary>
    /// Gets the sample rate of the audio output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "audio_output_get_sample_rate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint audio_output_get_sample_rate(AudioHandle audio);

    /// <summary>
    /// Gets the channel count of the audio output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "audio_output_get_channels")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint audio_output_get_channels(AudioHandle audio);

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
    /// Adds a directory path to search for OBS data files. libobs copies the string
    /// (dstr_init_copy), so the marshaled buffer can be freed after the call.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_add_data_path")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_add_data_path(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string path);

    /// <summary>
    /// Adds module search paths for plugins. libobs copies the strings (bstrdup), so the
    /// marshaled buffers can be freed after the call.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_add_module_path")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_add_module_path(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string binPath,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string dataPath);

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
