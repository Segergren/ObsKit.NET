using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS output functions.
/// </summary>
internal static partial class ObsOutput
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Creation and Release

    /// <summary>
    /// Creates a new output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsOutputHandle obs_output_create(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataHandle settings,
        ObsDataHandle hotkeyData);

    /// <summary>
    /// Releases a reference to an output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_release(ObsOutputHandle output);

    /// <summary>
    /// Adds a reference to an output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_addref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_addref(ObsOutputHandle output);

    /// <summary>
    /// Gets an additional reference to an output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_ref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsOutputHandle obs_output_get_ref(ObsOutputHandle output);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the output name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_output_get_name(ObsOutputHandle output);

    /// <summary>
    /// Gets the output type ID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_output_get_id(ObsOutputHandle output);

    /// <summary>
    /// Gets the display name for an output type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_display_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_output_get_display_name(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the output flags for an output type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_output_flags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_get_output_flags(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the output flags.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_flags")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_output_get_flags(ObsOutputHandle output);

    #endregion

    #region Start/Stop

    /// <summary>
    /// Starts the output.
    /// </summary>
    public static bool obs_output_start(ObsOutputHandle output) => obs_output_start_native(output) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_output_start")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_output_start_native(ObsOutputHandle output);

    /// <summary>
    /// Stops the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_stop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_stop(ObsOutputHandle output);

    /// <summary>
    /// Force stops the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_force_stop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_force_stop(ObsOutputHandle output);

    /// <summary>
    /// Checks if the output is active.
    /// </summary>
    public static bool obs_output_active(ObsOutputHandle output) => obs_output_active_native(output) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_output_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_output_active_native(ObsOutputHandle output);

    /// <summary>
    /// Checks if the output is reconnecting.
    /// </summary>
    public static bool obs_output_reconnecting(ObsOutputHandle output) => obs_output_reconnecting_native(output) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_output_reconnecting")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_output_reconnecting_native(ObsOutputHandle output);

    #endregion

    #region Settings

    /// <summary>
    /// Gets the output settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_output_get_settings(ObsOutputHandle output);

    /// <summary>
    /// Updates the output settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_update")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_update(ObsOutputHandle output, ObsDataHandle settings);

    #endregion

    #region Video/Audio

    /// <summary>
    /// Sets the video encoder for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_video_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_video_encoder(ObsOutputHandle output, ObsEncoderHandle encoder);

    /// <summary>
    /// Sets the video encoder for a specific track.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_video_encoder2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_video_encoder2(ObsOutputHandle output, ObsEncoderHandle encoder, nuint idx);

    /// <summary>
    /// Sets the audio encoder for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_audio_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_audio_encoder(ObsOutputHandle output, ObsEncoderHandle encoder, nuint idx);

    /// <summary>
    /// Gets the video encoder for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_video_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderHandle obs_output_get_video_encoder(ObsOutputHandle output);

    /// <summary>
    /// Gets the video encoder for a specific track.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_video_encoder2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderHandle obs_output_get_video_encoder2(ObsOutputHandle output, nuint idx);

    /// <summary>
    /// Gets the audio encoder for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_audio_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderHandle obs_output_get_audio_encoder(ObsOutputHandle output, nuint idx);

    /// <summary>
    /// Sets the video for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_video(ObsOutputHandle output, VideoHandle video);

    /// <summary>
    /// Sets the audio for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_audio")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_audio(ObsOutputHandle output, AudioHandle audio);

    /// <summary>
    /// Gets the video for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoHandle obs_output_video(ObsOutputHandle output);

    /// <summary>
    /// Gets the audio for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_audio")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial AudioHandle obs_output_audio(ObsOutputHandle output);

    #endregion

    #region Mixer

    /// <summary>
    /// Sets the audio mixer for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_mixer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_mixer(ObsOutputHandle output, nuint mixerIdx);

    /// <summary>
    /// Gets the audio mixer for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_mixer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_output_get_mixer(ObsOutputHandle output);

    /// <summary>
    /// Sets the audio mixers mask for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_mixers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_mixers(ObsOutputHandle output, nuint mixers);

    /// <summary>
    /// Gets the audio mixers mask for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_mixers")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_output_get_mixers(ObsOutputHandle output);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets total frames output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_total_frames")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_output_get_total_frames(ObsOutputHandle output);

    /// <summary>
    /// Gets total bytes output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_total_bytes")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong obs_output_get_total_bytes(ObsOutputHandle output);

    /// <summary>
    /// Gets frames dropped.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_frames_dropped")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_output_get_frames_dropped(ObsOutputHandle output);

    /// <summary>
    /// Gets the congestion value (0.0 to 1.0).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_congestion")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_output_get_congestion(ObsOutputHandle output);

    /// <summary>
    /// Gets the connect time in milliseconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_connect_time_ms")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_output_get_connect_time_ms(ObsOutputHandle output);

    #endregion

    #region Error Handling

    /// <summary>
    /// Gets the last error for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_last_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_output_get_last_error(ObsOutputHandle output);

    /// <summary>
    /// Sets the last error for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_last_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_last_error(
        ObsOutputHandle output,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string message);

    #endregion

    #region Pause

    /// <summary>
    /// Checks if the output can pause.
    /// </summary>
    public static bool obs_output_can_pause(ObsOutputHandle output) => obs_output_can_pause_native(output) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_output_can_pause")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_output_can_pause_native(ObsOutputHandle output);

    /// <summary>
    /// Pauses the output.
    /// </summary>
    public static bool obs_output_pause(ObsOutputHandle output, bool pause)
        => obs_output_pause_native(output, pause ? (byte)1 : (byte)0) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_output_pause")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_output_pause_native(ObsOutputHandle output, byte pause);

    /// <summary>
    /// Checks if the output is paused.
    /// </summary>
    public static bool obs_output_paused(ObsOutputHandle output) => obs_output_paused_native(output) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_output_paused")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_output_paused_native(ObsOutputHandle output);

    #endregion

    #region Signal Handler

    /// <summary>
    /// Gets the signal handler for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_signal_handler")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SignalHandlerHandle obs_output_get_signal_handler(ObsOutputHandle output);

    #endregion

    #region Service

    /// <summary>
    /// Sets the service for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_service")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_service(ObsOutputHandle output, ObsServiceHandle service);

    /// <summary>
    /// Gets the service for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_service")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsServiceHandle obs_output_get_service(ObsOutputHandle output);

    #endregion

    #region Delay

    /// <summary>
    /// Sets the delay for the output in seconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_delay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_delay(ObsOutputHandle output, uint delaySec, uint flags);

    /// <summary>
    /// Gets the delay for the output in seconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_delay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_output_get_delay(ObsOutputHandle output);

    /// <summary>
    /// Gets the active delay for the output in seconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_active_delay")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_output_get_active_delay(ObsOutputHandle output);

    #endregion

    #region Width/Height

    /// <summary>
    /// Gets the output width.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_width")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_output_get_width(ObsOutputHandle output);

    /// <summary>
    /// Gets the output height.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_height")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_output_get_height(ObsOutputHandle output);

    #endregion
}
