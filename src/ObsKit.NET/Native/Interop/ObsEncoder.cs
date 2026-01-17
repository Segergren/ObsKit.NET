using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS encoder functions.
/// </summary>
internal static partial class ObsEncoder
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Creation and Release

    /// <summary>
    /// Creates a video encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_video_encoder_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderHandle obs_video_encoder_create(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataHandle settings,
        ObsDataHandle hotkeyData);

    /// <summary>
    /// Creates an audio encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_audio_encoder_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderHandle obs_audio_encoder_create(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        ObsDataHandle settings,
        nuint mixerIdx,
        ObsDataHandle hotkeyData);

    /// <summary>
    /// Releases a reference to an encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_release(ObsEncoderHandle encoder);

    /// <summary>
    /// Adds a reference to an encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_addref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_addref(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets an additional reference to an encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_ref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderHandle obs_encoder_get_ref(ObsEncoderHandle encoder);

    #endregion

    #region Properties

    /// <summary>
    /// Gets the encoder name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_encoder_get_name(ObsEncoderHandle encoder);

    /// <summary>
    /// Sets the encoder name.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_name(
        ObsEncoderHandle encoder,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets the encoder type ID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_id")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_encoder_get_id(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the display name for an encoder type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_display_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_encoder_get_display_name(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the encoder type (video or audio).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderType obs_encoder_get_type(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the encoder codec.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_codec")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_encoder_get_codec(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the codec for an encoder type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_encoder_codec")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_get_encoder_codec(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    /// <summary>
    /// Gets the encoder type from an encoder ID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_encoder_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderType obs_get_encoder_type(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    #endregion

    #region Settings

    /// <summary>
    /// Gets the encoder settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_settings")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_encoder_get_settings(ObsEncoderHandle encoder);

    /// <summary>
    /// Updates the encoder settings.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_update")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_update(ObsEncoderHandle encoder, ObsDataHandle settings);

    /// <summary>
    /// Gets the default settings for an encoder type.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_defaults")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_encoder_defaults(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string id);

    #endregion

    #region Video/Audio

    /// <summary>
    /// Sets the video for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_video(ObsEncoderHandle encoder, VideoHandle video);

    /// <summary>
    /// Sets the audio for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_audio")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_audio(ObsEncoderHandle encoder, AudioHandle audio);

    /// <summary>
    /// Gets the video for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_video")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoHandle obs_encoder_video(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the audio for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_audio")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial AudioHandle obs_encoder_audio(ObsEncoderHandle encoder);

    #endregion

    #region State

    /// <summary>
    /// Checks if the encoder is active.
    /// </summary>
    public static bool obs_encoder_active(ObsEncoderHandle encoder) => obs_encoder_active_native(encoder) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_active_native(ObsEncoderHandle encoder);

    /// <summary>
    /// Checks if the encoder is paused.
    /// </summary>
    public static bool obs_encoder_paused(ObsEncoderHandle encoder) => obs_encoder_paused_native(encoder) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_paused")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_paused_native(ObsEncoderHandle encoder);

    #endregion

    #region Dimensions

    /// <summary>
    /// Gets the encoder width.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_width")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_encoder_get_width(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the encoder height.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_height")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_encoder_get_height(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the sample rate for an audio encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_sample_rate")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_encoder_get_sample_rate(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the frame size for an audio encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_frame_size")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_encoder_get_frame_size(ObsEncoderHandle encoder);

    #endregion

    #region Scaling

    /// <summary>
    /// Sets preferred video size for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_preferred_video_format")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_preferred_video_format(ObsEncoderHandle encoder, VideoFormat format);

    /// <summary>
    /// Gets preferred video format for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_preferred_video_format")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoFormat obs_encoder_get_preferred_video_format(ObsEncoderHandle encoder);

    /// <summary>
    /// Sets the scaled resolution for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_scaled_size")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_scaled_size(ObsEncoderHandle encoder, uint width, uint height);

    /// <summary>
    /// Checks if GPU scaling is enabled for the encoder.
    /// </summary>
    public static bool obs_encoder_scaling_enabled(ObsEncoderHandle encoder) => obs_encoder_scaling_enabled_native(encoder) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_scaling_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_scaling_enabled_native(ObsEncoderHandle encoder);

    #endregion

    #region GPU Encoding

    /// <summary>
    /// Checks if the encoder supports GPU encoding.
    /// </summary>
    public static bool obs_encoder_gpu_encode_available(ObsEncoderHandle encoder)
        => obs_encoder_gpu_encode_available_native(encoder) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_gpu_encode_available")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_gpu_encode_available_native(ObsEncoderHandle encoder);

    #endregion

    #region Last Error

    /// <summary>
    /// Gets the last error for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_last_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_encoder_get_last_error(ObsEncoderHandle encoder);

    /// <summary>
    /// Sets the last error for the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_last_error")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_last_error(
        ObsEncoderHandle encoder,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string message);

    #endregion
}
