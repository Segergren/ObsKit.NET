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

    /// <summary>
    /// Gets the capability flags (OBS_ENCODER_CAP_*) for an encoder ID.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_encoder_caps")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_get_encoder_caps(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string encoderId);

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

    /// <summary>
    /// Enables GPU-based scaling for the encoder (ObsScaleType.Disable turns it off).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_gpu_scale_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_gpu_scale_type(ObsEncoderHandle encoder, ObsScaleType scaleType);

    /// <summary>
    /// Gets the GPU scaling type of the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_scale_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsScaleType obs_encoder_get_scale_type(ObsEncoderHandle encoder);

    /// <summary>
    /// Checks if GPU-based scaling is enabled for the encoder.
    /// </summary>
    public static bool obs_encoder_gpu_scaling_enabled(ObsEncoderHandle encoder) => obs_encoder_gpu_scaling_enabled_native(encoder) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_gpu_scaling_enabled")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_gpu_scaling_enabled_native(ObsEncoderHandle encoder);

    /// <summary>
    /// Sets the frame rate divisor (encode at base FPS / divisor). Fails on active encoders.
    /// </summary>
    public static bool obs_encoder_set_frame_rate_divisor(ObsEncoderHandle encoder, uint divisor)
        => obs_encoder_set_frame_rate_divisor_native(encoder, divisor) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_frame_rate_divisor")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_set_frame_rate_divisor_native(ObsEncoderHandle encoder, uint divisor);

    /// <summary>
    /// Gets the frame rate divisor (default 1).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_frame_rate_divisor")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_encoder_get_frame_rate_divisor(ObsEncoderHandle encoder);

    #endregion

    #region GPU Encoding

    /// <summary>
    /// Gets the capability flags of an encoder instance (OBS_ENCODER_CAP_*).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_caps")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_encoder_get_caps(ObsEncoderHandle encoder);

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

    #region ROI / Color / Stats

    /// <summary>
    /// Adds a region of interest to a video encoder. Returns false if the encoder
    /// does not support ROI (OBS_ENCODER_CAP_ROI) or the region is invalid.
    /// </summary>
    public static bool obs_encoder_add_roi(ObsEncoderHandle encoder, ref ObsEncoderRoi roi)
        => obs_encoder_add_roi_native(encoder, ref roi) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_add_roi")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_add_roi_native(ObsEncoderHandle encoder, ref ObsEncoderRoi roi);

    /// <summary>
    /// Gets whether any regions of interest are set on the encoder.
    /// </summary>
    public static bool obs_encoder_has_roi(ObsEncoderHandle encoder) => obs_encoder_has_roi_native(encoder) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_has_roi")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_has_roi_native(ObsEncoderHandle encoder);

    /// <summary>
    /// Clears all regions of interest from the encoder.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_clear_roi")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_clear_roi(ObsEncoderHandle encoder);

    /// <summary>
    /// Sets the color space the encoder prefers over the video output's own.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_preferred_color_space")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_preferred_color_space(ObsEncoderHandle encoder, VideoColorspace colorspace);

    /// <summary>
    /// Gets the encoder's preferred color space (Default when none set).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_preferred_color_space")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoColorspace obs_encoder_get_preferred_color_space(ObsEncoderHandle encoder);

    /// <summary>
    /// Sets the color range the encoder prefers over the video output's own.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_preferred_range")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_set_preferred_range(ObsEncoderHandle encoder, VideoRangeType range);

    /// <summary>
    /// Gets the encoder's preferred color range (Default when none set).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_preferred_range")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial VideoRangeType obs_encoder_get_preferred_range(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the total time (nanoseconds) the encoder has spent paused.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_pause_offset")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ulong obs_encoder_get_pause_offset(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the number of frames a video encoder has encoded.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_encoded_frames")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_encoder_get_encoded_frames(ObsEncoderHandle encoder);

    #endregion

    #region Lookup/Enumeration

    /// <summary>
    /// Callback for enumerating encoders. Return 0 to stop enumerating.
    /// The encoder pointer is borrowed — take a ref via obs_encoder_get_ref to keep it.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumEncoderCallback(nint data, ObsEncoderHandle encoder);

    /// <summary>
    /// Enumerates all encoders.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_encoders")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_encoders(EnumEncoderCallback callback, nint data);

    /// <summary>
    /// Gets an encoder by name. Returns an incremented reference.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_encoder_by_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderHandle obs_get_encoder_by_name(
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name);

    /// <summary>
    /// Gets the audio track (mixer index) an audio encoder reads from.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_mixer_index")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nuint obs_encoder_get_mixer_index(ObsEncoderHandle encoder);

    #endregion

    #region Extra Data, Defaults, Properties, ROI Enumeration

    /// <summary>
    /// Gets the encoder's codec extra data (e.g. SPS/PPS for H.264, AudioSpecificConfig for
    /// AAC). The pointer is owned by the encoder and only valid while it stays active; copy
    /// immediately. Returns false if the encoder has no extra data yet.
    /// </summary>
    public static bool obs_encoder_get_extra_data(ObsEncoderHandle encoder, out nint extraData, out nuint size)
        => obs_encoder_get_extra_data_native(encoder, out extraData, out size) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_extra_data")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_get_extra_data_native(ObsEncoderHandle encoder, out nint extraData, out nuint size);

    /// <summary>
    /// Gets a new data object with the default settings of the encoder's type (release when done).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_defaults")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsDataHandle obs_encoder_get_defaults(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the properties of an encoder instance, evaluated against its settings
    /// (destroy with obs_properties_destroy).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_properties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_encoder_properties(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets the number of priming (pre-roll) samples an audio encoder emits before real audio.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_priming_samples")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_encoder_get_priming_samples(ObsEncoderHandle encoder);

    /// <summary>
    /// Gets a counter that increments whenever the encoder's ROI list changes.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_roi_increment")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_encoder_get_roi_increment(ObsEncoderHandle encoder);

    /// <summary>
    /// Callback for <c>obs_encoder_enum_roi</c>; <paramref name="roi"/> points at an
    /// <see cref="ObsEncoderRoi"/> valid only during the call.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void EnumRoiCallback(nint param, nint roi);

    /// <summary>
    /// Enumerates the encoder's regions of interest, most recently added first.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_enum_roi")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_enum_roi(ObsEncoderHandle encoder, EnumRoiCallback callback, nint param);

    /// <summary>
    /// Returns whether the video mix feeding the encoder currently produces textures in the
    /// given format (NV12 or P010) for GPU texture encoding. The encoder must have video set.
    /// </summary>
    public static bool obs_encoder_video_tex_active(ObsEncoderHandle encoder, VideoFormat format)
        => obs_encoder_video_tex_active_native(encoder, format) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_video_tex_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_video_tex_active_native(ObsEncoderHandle encoder, VideoFormat format);

    #endregion

    #region Encoder Groups

    /// <summary>
    /// Creates an encoder group for synchronized encoder startup (destroy with
    /// obs_encoder_group_destroy).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_group_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_encoder_group_create();

    /// <summary>
    /// Destroys a group and releases its encoder references. Deferred until the group's
    /// encoders have all stopped if any are active.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_encoder_group_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_encoder_group_destroy(nint group);

    /// <summary>
    /// Moves an encoder into a group (the group takes a strong reference), or out of its
    /// group when <paramref name="group"/> is null. Fails if the encoder or group is active.
    /// </summary>
    public static bool obs_encoder_set_group(ObsEncoderHandle encoder, nint group)
        => obs_encoder_set_group_native(encoder, group) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_encoder_set_group")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_encoder_set_group_native(ObsEncoderHandle encoder, nint group);

    #endregion

    #region Weak References

    [LibraryImport(Lib, EntryPoint = "obs_encoder_get_weak_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_encoder_get_weak_encoder(ObsEncoderHandle encoder);

    [LibraryImport(Lib, EntryPoint = "obs_weak_encoder_get_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsEncoderHandle obs_weak_encoder_get_encoder(nint weak);

    [LibraryImport(Lib, EntryPoint = "obs_weak_encoder_release")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_weak_encoder_release(nint weak);

    public static bool obs_weak_encoder_references_encoder(nint weak, ObsEncoderHandle encoder)
        => obs_weak_encoder_references_encoder_native(weak, encoder) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_weak_encoder_references_encoder")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_weak_encoder_references_encoder_native(nint weak, ObsEncoderHandle encoder);

    #endregion
}
