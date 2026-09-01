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

    /// <summary>
    /// Sets auto-reconnect parameters on the output. retryCount == 0 disables reconnecting.
    /// (Reconnect is NOT configurable via output settings data.)
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_reconnect_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_reconnect_settings(ObsOutputHandle output, int retryCount, int retrySec);

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
    /// Sets the raw media (video and audio) for a non-encoded output. Encoded outputs use
    /// the encoder setters instead; raw outputs auto-bind global media at creation, so this
    /// is only needed to override them.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_media")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_media(ObsOutputHandle output, VideoHandle video, AudioHandle audio);

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
    internal static partial int obs_output_get_total_frames(ObsOutputHandle output); // C returns signed int

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
    /// Gets the video codecs the output supports, semicolon-separated (e.g. "h264;hevc;av1").
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_supported_video_codecs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_output_get_supported_video_codecs(ObsOutputHandle output);

    /// <summary>
    /// Gets the audio codecs the output supports, semicolon-separated (e.g. "aac;opus").
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_supported_audio_codecs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_output_get_supported_audio_codecs(ObsOutputHandle output);

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

    #region Proc Handler

    /// <summary>
    /// Gets the proc handler for the output.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_proc_handler")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_output_get_proc_handler(ObsOutputHandle output);

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

    /// <summary>
    /// Gets the total time (nanoseconds) the output has spent paused.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_pause_offset")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong obs_output_get_pause_offset(ObsOutputHandle output);

    /// <summary>
    /// Queues a caption (CEA-708) on an active output with a display duration in seconds.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_output_caption_text2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_output_caption_text2(
        ObsOutputHandle output,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string text,
        double displayDuration);

    #region Lookup/Enumeration

    /// <summary>
    /// Callback for enumerating outputs. Return 0 to stop enumerating.
    /// The output pointer is borrowed — take a ref via obs_output_get_ref to keep it.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumOutputCallback(nint data, ObsOutputHandle output);

    /// <summary>
    /// Enumerates all outputs.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_outputs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_outputs(EnumOutputCallback callback, nint data);

    /// <summary>
    /// Gets an output by name. Returns an incremented reference.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_output_by_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsOutputHandle obs_get_output_by_name(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets the video codecs an output type supports as a semicolon-delimited list
    /// (e.g. "h264;hevc;av1"), or null if the output type does not exist.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_output_supported_video_codecs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_get_output_supported_video_codecs(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the audio codecs an output type supports as a semicolon-delimited list
    /// (e.g. "aac;opus"), or null if the output type does not exist.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_output_supported_audio_codecs")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_get_output_supported_audio_codecs(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    #endregion

    #region Sizing, Protocols, Defaults and Properties

    /// <summary>
    /// Sets the preferred scaled output size (0x0 disables scaling). Applied to the video
    /// encoder when the output starts; ignored with a warning if the encoder is already active.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_preferred_size")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_preferred_size(ObsOutputHandle output, uint width, uint height);

    /// <summary>
    /// Like <see cref="obs_output_set_preferred_size"/> for a specific video encoder index
    /// (multi-video outputs only).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_set_preferred_size2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_preferred_size2(ObsOutputHandle output, uint width, uint height, nuint idx);

    [LibraryImport(Lib, EntryPoint = "obs_output_get_width2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_output_get_width2(ObsOutputHandle output, nuint idx);

    [LibraryImport(Lib, EntryPoint = "obs_output_get_height2")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_output_get_height2(ObsOutputHandle output, nuint idx);

    /// <summary>
    /// Gets the semicolon-separated protocols the output type supports (OBS-owned string; null for
    /// non-streaming outputs).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_protocols")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_output_get_protocols(ObsOutputHandle output);

    /// <summary>
    /// Gets a new data object holding the default settings of an output type (release when done).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_defaults")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_output_defaults([MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the properties of an output type (destroy with obs_properties_destroy).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_output_properties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_get_output_properties([MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the properties of an output instance (destroy with obs_properties_destroy).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_properties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_output_properties(ObsOutputHandle output);

    #endregion

    #region Packet and Reconnect Callbacks

    /// <summary>
    /// Native packet callback: invoked synchronously on the output's interleave path for every
    /// encoded packet before it reaches the output implementation. <paramref name="pkt"/> points
    /// at an <see cref="EncoderPacketNative"/>; <paramref name="pktTime"/> points at an
    /// <see cref="EncoderPacketTimeNative"/> or is null. Both are valid only during the call.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PacketCallbackNative(ObsOutputHandle output, nint pkt, nint pktTime, nint param);

    [LibraryImport(Lib, EntryPoint = "obs_output_add_packet_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_add_packet_callback(ObsOutputHandle output, PacketCallbackNative callback, nint param);

    [LibraryImport(Lib, EntryPoint = "obs_output_remove_packet_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_remove_packet_callback(ObsOutputHandle output, PacketCallbackNative callback, nint param);

    /// <summary>
    /// Native reconnect gate: return 0 to veto an automatic reconnect attempt after the output
    /// stopped with <paramref name="code"/>.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte ReconnectCallbackNative(nint data, ObsOutputHandle output, int code);

    [LibraryImport(Lib, EntryPoint = "obs_output_set_reconnect_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_output_set_reconnect_callback(ObsOutputHandle output, ReconnectCallbackNative? callback, nint param);

    #endregion

    #region Weak References

    /// <summary>
    /// Gets a new weak reference to the output (release with obs_weak_output_release).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_output_get_weak_output")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_output_get_weak_output(ObsOutputHandle output);

    /// <summary>
    /// Upgrades a weak reference to a strong one (release with obs_output_release), or null.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_weak_output_get_output")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsOutputHandle obs_weak_output_get_output(nint weak);

    [LibraryImport(Lib, EntryPoint = "obs_weak_output_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_weak_output_release(nint weak);

    public static bool obs_weak_output_references_output(nint weak, ObsOutputHandle output)
        => obs_weak_output_references_output_native(weak, output) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_weak_output_references_output")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_weak_output_references_output_native(nint weak, ObsOutputHandle output);

    #endregion
}
