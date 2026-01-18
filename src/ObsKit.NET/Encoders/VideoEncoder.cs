using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Encoders;

/// <summary>
/// Represents an OBS video encoder (obs_encoder_t).
/// </summary>
public sealed class VideoEncoder : ObsObject
{
    /// <summary>Known video encoder type IDs.</summary>
    public static class Types
    {
        /// <summary>x264 software encoder.</summary>
        public const string X264 = "obs_x264";
        /// <summary>NVIDIA NVENC H.264 encoder.</summary>
        public const string NvencH264 = "jim_nvenc";
        /// <summary>NVIDIA NVENC HEVC encoder.</summary>
        public const string NvencHevc = "jim_hevc_nvenc";
        /// <summary>NVIDIA NVENC AV1 encoder.</summary>
        public const string NvencAv1 = "jim_av1_nvenc";
        /// <summary>AMD AMF H.264 encoder.</summary>
        public const string AmfH264 = "h264_texture_amf";
        /// <summary>AMD AMF HEVC encoder.</summary>
        public const string AmfHevc = "h265_texture_amf";
        /// <summary>AMD AMF AV1 encoder.</summary>
        public const string AmfAv1 = "av1_texture_amf";
        /// <summary>Intel QuickSync H.264 encoder.</summary>
        public const string QsvH264 = "obs_qsv11_v2";
        /// <summary>Intel QuickSync HEVC encoder.</summary>
        public const string QsvHevc = "obs_qsv11_he_v2";
        /// <summary>Intel QuickSync AV1 encoder.</summary>
        public const string QsvAv1 = "obs_qsv11_av1";
        /// <summary>Apple VideoToolbox H.264 encoder.</summary>
        public const string AppleH264 = "com.apple.videotoolbox.videoencoder.h264";
        /// <summary>Apple VideoToolbox HEVC encoder.</summary>
        public const string AppleHevc = "com.apple.videotoolbox.videoencoder.hevc";
    }

    /// <summary>
    /// Creates a video encoder.
    /// </summary>
    /// <param name="typeId">The encoder type ID.</param>
    /// <param name="name">The encoder name.</param>
    /// <param name="settings">Optional encoder settings.</param>
    /// <param name="hotkeyData">Optional hotkey data.</param>
    public VideoEncoder(string typeId, string name, Settings? settings = null, Settings? hotkeyData = null)
        : base(CreateEncoder(typeId, name, settings, hotkeyData))
    {
        TypeId = typeId;
    }

    internal VideoEncoder(ObsEncoderHandle handle, string? typeId = null, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
        TypeId = typeId ?? ObsEncoder.obs_encoder_get_id(handle);
    }

    private static nint CreateEncoder(string typeId, string name, Settings? settings, Settings? hotkeyData)
    {
        ThrowIfNotInitialized();
        var handle = ObsEncoder.obs_video_encoder_create(
            typeId,
            name,
            settings?.Handle ?? default,
            hotkeyData?.Handle ?? default);

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create video encoder of type '{typeId}'");

        return handle;
    }

    /// <summary>
    /// Creates an x264 software encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps.</param>
    /// <param name="preset">x264 preset (e.g., "veryfast", "medium", "slow").</param>
    public static VideoEncoder CreateX264(string name = "Video Encoder", int bitrate = 6000, string preset = "veryfast")
    {
        using var settings = new Settings();
        settings.Set("rate_control", "CBR");
        settings.Set("bitrate", bitrate);
        settings.Set("preset", preset);
        return new VideoEncoder(Types.X264, name, settings);
    }

    /// <summary>
    /// Creates an NVIDIA NVENC H.264 encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps.</param>
    /// <param name="preset">NVENC preset.</param>
    public static VideoEncoder CreateNvencH264(string name = "NVENC H.264", int bitrate = 6000, string preset = "hq")
    {
        using var settings = new Settings();
        settings.Set("rate_control", "CBR");
        settings.Set("bitrate", bitrate);
        settings.Set("preset", preset);
        return new VideoEncoder(Types.NvencH264, name, settings);
    }

    /// <summary>
    /// Creates an NVIDIA NVENC HEVC encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps.</param>
    /// <param name="preset">NVENC preset.</param>
    public static VideoEncoder CreateNvencHevc(string name = "NVENC HEVC", int bitrate = 6000, string preset = "hq")
    {
        using var settings = new Settings();
        settings.Set("rate_control", "CBR");
        settings.Set("bitrate", bitrate);
        settings.Set("preset", preset);
        return new VideoEncoder(Types.NvencHevc, name, settings);
    }

    internal new ObsEncoderHandle Handle => (ObsEncoderHandle)base.Handle;

    /// <summary>Gets the encoder type ID.</summary>
    public string? TypeId { get; }

    /// <summary>Gets or sets the encoder name.</summary>
    public string? Name
    {
        get => ObsEncoder.obs_encoder_get_name(Handle);
        set
        {
            if (value != null)
                ObsEncoder.obs_encoder_set_name(Handle, value);
        }
    }

    /// <summary>Gets the display name for this encoder type.</summary>
    public string? DisplayName => TypeId != null ? ObsEncoder.obs_encoder_get_display_name(TypeId) : null;

    /// <summary>Gets the codec identifier.</summary>
    public string? Codec => ObsEncoder.obs_encoder_get_codec(Handle);

    /// <summary>Gets whether the encoder is currently active.</summary>
    public bool IsActive => ObsEncoder.obs_encoder_active(Handle);

    /// <summary>Gets whether the encoder is paused.</summary>
    public bool IsPaused => ObsEncoder.obs_encoder_paused(Handle);

    /// <summary>Gets the video width.</summary>
    public uint Width => ObsEncoder.obs_encoder_get_width(Handle);

    /// <summary>Gets the video height.</summary>
    public uint Height => ObsEncoder.obs_encoder_get_height(Handle);

    /// <summary>Gets whether GPU encoding is available.</summary>
    public bool GpuEncodeAvailable => ObsEncoder.obs_encoder_gpu_encode_available(Handle);

    /// <summary>Gets the last error message.</summary>
    public string? LastError => ObsEncoder.obs_encoder_get_last_error(Handle);

    /// <summary>Gets the current encoder settings.</summary>
    public Settings GetSettings()
    {
        var handle = ObsEncoder.obs_encoder_get_settings(Handle);
        return new Settings(handle, ownsHandle: true);
    }

    /// <summary>Updates encoder settings.</summary>
    public void Update(Settings settings)
    {
        ObsEncoder.obs_encoder_update(Handle, settings.Handle);
    }

    /// <summary>Updates encoder settings using a configuration action.</summary>
    public void Update(Action<Settings> configure)
    {
        using var settings = new Settings();
        configure(settings);
        Update(settings);
    }

    /// <summary>Sets the scaled output size.</summary>
    public VideoEncoder SetScaledSize(uint width, uint height)
    {
        ObsEncoder.obs_encoder_set_scaled_size(Handle, width, height);
        return this;
    }

    internal void SetVideo(VideoHandle video)
    {
        ObsEncoder.obs_encoder_set_video(Handle, video);
    }

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsEncoder.obs_encoder_release((ObsEncoderHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => $"VideoEncoder[{TypeId}]: {Name}";
}
