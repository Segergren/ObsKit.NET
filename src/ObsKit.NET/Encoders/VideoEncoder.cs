using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Encoders;

/// <summary>
/// Represents an OBS video encoder (obs_encoder_t).
/// </summary>
public sealed class VideoEncoder : ObsObject
{
    public static class Types
    {
        public const string X264 = "obs_x264";
        public const string NvencH264 = "jim_nvenc";
        public const string NvencHevc = "jim_hevc_nvenc";
        public const string NvencAv1 = "jim_av1_nvenc";
        public const string AmfH264 = "h264_texture_amf";
        public const string AmfHevc = "h265_texture_amf";
        public const string AmfAv1 = "av1_texture_amf";
        public const string QsvH264 = "obs_qsv11_v2";
        public const string QsvHevc = "obs_qsv11_he_v2";
        public const string QsvAv1 = "obs_qsv11_av1";
        public const string AppleH264 = "com.apple.videotoolbox.videoencoder.h264";
        public const string AppleHevc = "com.apple.videotoolbox.videoencoder.hevc";
    }

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

    /// <param name="bitrate">Bitrate in kbps.</param>
    public static VideoEncoder CreateNvencH264(string name = "NVENC H.264", int bitrate = 6000, string preset = "hq")
    {
        using var settings = new Settings();
        settings.Set("rate_control", "CBR");
        settings.Set("bitrate", bitrate);
        settings.Set("preset", preset);
        return new VideoEncoder(Types.NvencH264, name, settings);
    }

    /// <param name="bitrate">Bitrate in kbps.</param>
    public static VideoEncoder CreateNvencHevc(string name = "NVENC HEVC", int bitrate = 6000, string preset = "hq")
    {
        using var settings = new Settings();
        settings.Set("rate_control", "CBR");
        settings.Set("bitrate", bitrate);
        settings.Set("preset", preset);
        return new VideoEncoder(Types.NvencHevc, name, settings);
    }

    internal new ObsEncoderHandle Handle => (ObsEncoderHandle)base.Handle;

    public string? TypeId { get; }

    public string? Name
    {
        get => ObsEncoder.obs_encoder_get_name(Handle);
        set
        {
            if (value != null)
                ObsEncoder.obs_encoder_set_name(Handle, value);
        }
    }

    public string? DisplayName => TypeId != null ? ObsEncoder.obs_encoder_get_display_name(TypeId) : null;

    public string? Codec => ObsEncoder.obs_encoder_get_codec(Handle);

    public bool IsActive => ObsEncoder.obs_encoder_active(Handle);

    public bool IsPaused => ObsEncoder.obs_encoder_paused(Handle);

    public uint Width => ObsEncoder.obs_encoder_get_width(Handle);

    public uint Height => ObsEncoder.obs_encoder_get_height(Handle);

    public bool GpuEncodeAvailable => ObsEncoder.obs_encoder_gpu_encode_available(Handle);

    public string? LastError => ObsEncoder.obs_encoder_get_last_error(Handle);

    public Settings GetSettings()
    {
        var handle = ObsEncoder.obs_encoder_get_settings(Handle);
        return new Settings(handle, ownsHandle: true);
    }

    public void Update(Settings settings)
    {
        ObsEncoder.obs_encoder_update(Handle, settings.Handle);
    }

    public void Update(Action<Settings> configure)
    {
        using var settings = new Settings();
        configure(settings);
        Update(settings);
    }

    public VideoEncoder SetScaledSize(uint width, uint height)
    {
        ObsEncoder.obs_encoder_set_scaled_size(Handle, width, height);
        return this;
    }

    internal void SetVideo(VideoHandle video)
    {
        ObsEncoder.obs_encoder_set_video(Handle, video);
    }

    protected override void ReleaseHandle(nint handle)
    {
        ObsEncoder.obs_encoder_release((ObsEncoderHandle)handle);
    }

    public override string ToString() => $"VideoEncoder[{TypeId}]: {Name}";
}
