using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Encoders;

/// <summary>
/// Rate control mode for video encoders.
/// </summary>
public enum RateControl
{
    /// <summary>Constant Bitrate - consistent file size, may vary quality.</summary>
    CBR,
    /// <summary>Variable Bitrate - consistent quality, variable file size.</summary>
    VBR,
    /// <summary>Constant Quality Parameter - quality-based encoding.</summary>
    CQP,
    /// <summary>Constrained Quality - VBR with quality target.</summary>
    CRF
}

/// <summary>
/// Represents an OBS video encoder (obs_encoder_t).
/// Supports ref counting for automatic disposal when no outputs reference it.
/// </summary>
public sealed class VideoEncoder : ObsObject
{
    private int _refCount = 0;
    private readonly object _refLock = new();
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
        public const string QsvHevc = "obs_qsv11_hevc";
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
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="preset">x264 preset (e.g., "veryfast", "medium", "slow").</param>
    /// <param name="rateControl">Rate control mode.</param>
    /// <param name="cqLevel">CQ level (for CQP/CRF modes, 0-51, lower is better quality).</param>
    public static VideoEncoder CreateX264(string name = "Video Encoder", int bitrate = 6000, string preset = "veryfast", RateControl rateControl = RateControl.CBR, int cqLevel = 20)
    {
        using var settings = new Settings();
        // x264's constant-quality mode is CRF, so map CQP to CRF.
        settings.Set("rate_control", rateControl == RateControl.CQP ? "CRF" : rateControl.ToString());
        settings.Set("preset", preset);

        switch (rateControl)
        {
            case RateControl.CBR:
            case RateControl.VBR:
                settings.Set("bitrate", bitrate);
                break;
            case RateControl.CQP:
            case RateControl.CRF:
                settings.Set("crf", cqLevel);
                break;
        }

        return new VideoEncoder(Types.X264, name, settings);
    }

    /// <summary>
    /// Creates an NVIDIA NVENC H.264 encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="preset">NVENC preset.</param>
    /// <param name="rateControl">Rate control mode.</param>
    /// <param name="cqLevel">CQ level (for CQP mode, 0-51).</param>
    /// <param name="maxBitrate">Maximum bitrate in kbps (for VBR mode, defaults to 1.5x bitrate).</param>
    public static VideoEncoder CreateNvencH264(string name = "NVENC H.264", int bitrate = 6000, string preset = "p5", RateControl rateControl = RateControl.CBR, int cqLevel = 20, int? maxBitrate = null)
    {
        using var settings = new Settings();
        // NVENC's constant-quality mode is CQP, so map CRF to CQP.
        settings.Set("rate_control", rateControl == RateControl.CRF ? "CQP" : rateControl.ToString());
        // NVENC reads the preset from "preset2"; "preset" is set as well for compatibility.
        settings.Set("preset", preset);
        settings.Set("preset2", preset);

        switch (rateControl)
        {
            case RateControl.CBR:
                settings.Set("bitrate", bitrate);
                break;
            case RateControl.VBR:
                settings.Set("bitrate", bitrate);
                settings.Set("max_bitrate", maxBitrate ?? (int)(bitrate * 1.5));
                break;
            case RateControl.CQP:
                settings.Set("cqp", cqLevel);
                break;
            case RateControl.CRF:
                settings.Set("cqp", cqLevel); // NVENC uses cqp for quality-based encoding
                break;
        }

        return new VideoEncoder(Types.NvencH264, name, settings);
    }

    /// <summary>
    /// Creates an NVIDIA NVENC HEVC encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="preset">NVENC preset.</param>
    /// <param name="rateControl">Rate control mode.</param>
    /// <param name="cqLevel">CQ level (for CQP mode, 0-51).</param>
    /// <param name="maxBitrate">Maximum bitrate in kbps (for VBR mode, defaults to 1.5x bitrate).</param>
    public static VideoEncoder CreateNvencHevc(string name = "NVENC HEVC", int bitrate = 6000, string preset = "p5", RateControl rateControl = RateControl.CBR, int cqLevel = 20, int? maxBitrate = null)
    {
        using var settings = new Settings();
        // NVENC's constant-quality mode is CQP, so map CRF to CQP.
        settings.Set("rate_control", rateControl == RateControl.CRF ? "CQP" : rateControl.ToString());
        // NVENC reads the preset from "preset2"; "preset" is set as well for compatibility.
        settings.Set("preset", preset);
        settings.Set("preset2", preset);

        switch (rateControl)
        {
            case RateControl.CBR:
                settings.Set("bitrate", bitrate);
                break;
            case RateControl.VBR:
                settings.Set("bitrate", bitrate);
                settings.Set("max_bitrate", maxBitrate ?? (int)(bitrate * 1.5));
                break;
            case RateControl.CQP:
                settings.Set("cqp", cqLevel);
                break;
            case RateControl.CRF:
                settings.Set("cqp", cqLevel); // NVENC uses cqp for quality-based encoding
                break;
        }

        return new VideoEncoder(Types.NvencHevc, name, settings);
    }

    /// <summary>
    /// Creates an NVIDIA NVENC AV1 encoder (RTX 40-series and later).
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="preset">NVENC preset.</param>
    /// <param name="rateControl">Rate control mode.</param>
    /// <param name="cqLevel">CQ level (for CQP mode, 0-51).</param>
    /// <param name="maxBitrate">Maximum bitrate in kbps (for VBR mode, defaults to 1.5x bitrate).</param>
    public static VideoEncoder CreateNvencAv1(string name = "NVENC AV1", int bitrate = 6000, string preset = "p5", RateControl rateControl = RateControl.CBR, int cqLevel = 20, int? maxBitrate = null)
    {
        using var settings = new Settings();
        // NVENC's constant-quality mode is CQP, so map CRF to CQP.
        settings.Set("rate_control", rateControl == RateControl.CRF ? "CQP" : rateControl.ToString());
        // NVENC reads the preset from "preset2"; "preset" is set as well for compatibility.
        settings.Set("preset", preset);
        settings.Set("preset2", preset);

        switch (rateControl)
        {
            case RateControl.CBR:
                settings.Set("bitrate", bitrate);
                break;
            case RateControl.VBR:
                settings.Set("bitrate", bitrate);
                settings.Set("max_bitrate", maxBitrate ?? (int)(bitrate * 1.5));
                break;
            case RateControl.CQP:
            case RateControl.CRF:
                settings.Set("cqp", cqLevel); // NVENC uses cqp for quality-based encoding
                break;
        }

        return new VideoEncoder(Types.NvencAv1, name, settings);
    }

    /// <summary>
    /// Creates the best available hardware encoder (NVENC → AMF → QuickSync),
    /// falling back to x264 if no hardware encoder is present.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps.</param>
    /// <param name="preferHevc">Try the vendor's HEVC encoder before H.264 (better quality per bit, slightly less player compatibility).</param>
    /// <param name="rateControl">Rate control mode.</param>
    public static VideoEncoder CreateBest(string name = "Video Encoder", int bitrate = 6000, bool preferHevc = false, RateControl rateControl = RateControl.CBR)
    {
        if (preferHevc)
        {
            if (EncoderInfo.IsAvailable(Types.NvencHevc))
                return CreateNvencHevc(name, bitrate, rateControl: rateControl);
            if (EncoderInfo.IsAvailable(Types.AmfHevc))
                return CreateAmfHevc(name, bitrate, rateControl: rateControl);
            if (EncoderInfo.IsAvailable(Types.QsvHevc))
                return CreateQsvHevc(name, bitrate, rateControl: rateControl);
        }

        if (EncoderInfo.IsAvailable(Types.NvencH264))
            return CreateNvencH264(name, bitrate, rateControl: rateControl);
        if (EncoderInfo.IsAvailable(Types.AmfH264))
            return CreateAmfH264(name, bitrate, rateControl: rateControl);
        if (EncoderInfo.IsAvailable(Types.QsvH264))
            return CreateQsvH264(name, bitrate, rateControl: rateControl);

        return CreateX264(name, bitrate, rateControl: rateControl);
    }

    private static VideoEncoder CreateAmf(string typeId, string name, int bitrate, string preset, RateControl rateControl, int cqLevel)
    {
        using var settings = new Settings();
        // AMF rate controls: CBR, VBR, CQP (plus QVBR/HQVBR/HQCBR via raw settings)
        settings.Set("rate_control", rateControl == RateControl.CRF ? "CQP" : rateControl.ToString());
        settings.Set("preset", preset);

        switch (rateControl)
        {
            case RateControl.CBR:
            case RateControl.VBR:
                settings.Set("bitrate", bitrate);
                break;
            case RateControl.CQP:
            case RateControl.CRF:
                settings.Set("cqp", cqLevel);
                break;
        }

        return new VideoEncoder(typeId, name, settings);
    }

    /// <summary>
    /// Creates an AMD AMF H.264 encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="preset">AMF preset: "highQuality", "quality", "balanced", or "speed".</param>
    /// <param name="rateControl">Rate control mode (CRF maps to CQP).</param>
    /// <param name="cqLevel">QP level (for CQP mode, 0-51).</param>
    public static VideoEncoder CreateAmfH264(string name = "AMF H.264", int bitrate = 6000, string preset = "quality", RateControl rateControl = RateControl.CBR, int cqLevel = 20)
        => CreateAmf(Types.AmfH264, name, bitrate, preset, rateControl, cqLevel);

    /// <summary>
    /// Creates an AMD AMF HEVC encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="preset">AMF preset: "highQuality", "quality", "balanced", or "speed".</param>
    /// <param name="rateControl">Rate control mode (CRF maps to CQP).</param>
    /// <param name="cqLevel">QP level (for CQP mode, 0-51).</param>
    public static VideoEncoder CreateAmfHevc(string name = "AMF HEVC", int bitrate = 6000, string preset = "quality", RateControl rateControl = RateControl.CBR, int cqLevel = 20)
        => CreateAmf(Types.AmfHevc, name, bitrate, preset, rateControl, cqLevel);

    /// <summary>
    /// Creates an AMD AMF AV1 encoder (RX 7000-series and later).
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="preset">AMF preset: "highQuality", "quality", "balanced", or "speed".</param>
    /// <param name="rateControl">Rate control mode (CRF maps to CQP).</param>
    /// <param name="cqLevel">QP level (for CQP mode, 0-63).</param>
    public static VideoEncoder CreateAmfAv1(string name = "AMF AV1", int bitrate = 6000, string preset = "quality", RateControl rateControl = RateControl.CBR, int cqLevel = 20)
        => CreateAmf(Types.AmfAv1, name, bitrate, preset, rateControl, cqLevel);

    private static VideoEncoder CreateQsv(string typeId, string name, int bitrate, string targetUsage, RateControl rateControl, int cqLevel, int? maxBitrate)
    {
        using var settings = new Settings();
        // QSV rate controls: CBR, VBR, CQP, ICQ (CRF maps to ICQ, QSV's quality mode)
        settings.Set("rate_control", rateControl == RateControl.CRF ? "ICQ" : rateControl.ToString());
        settings.Set("target_usage", targetUsage);

        switch (rateControl)
        {
            case RateControl.CBR:
                settings.Set("bitrate", bitrate);
                break;
            case RateControl.VBR:
                settings.Set("bitrate", bitrate);
                settings.Set("max_bitrate", maxBitrate ?? (int)(bitrate * 1.5));
                break;
            case RateControl.CQP:
                settings.Set("cqp", cqLevel);
                break;
            case RateControl.CRF:
                settings.Set("icq_quality", cqLevel);
                break;
        }

        return new VideoEncoder(typeId, name, settings);
    }

    /// <summary>
    /// Creates an Intel QuickSync H.264 encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="targetUsage">Quality/speed trade-off: "TU1" (best quality) to "TU7" (fastest).</param>
    /// <param name="rateControl">Rate control mode (CRF maps to ICQ).</param>
    /// <param name="cqLevel">QP/ICQ level (1-51, lower is better quality).</param>
    /// <param name="maxBitrate">Maximum bitrate in kbps (for VBR mode, defaults to 1.5x bitrate).</param>
    public static VideoEncoder CreateQsvH264(string name = "QSV H.264", int bitrate = 6000, string targetUsage = "TU4", RateControl rateControl = RateControl.CBR, int cqLevel = 23, int? maxBitrate = null)
        => CreateQsv(Types.QsvH264, name, bitrate, targetUsage, rateControl, cqLevel, maxBitrate);

    /// <summary>
    /// Creates an Intel QuickSync HEVC encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="targetUsage">Quality/speed trade-off: "TU1" (best quality) to "TU7" (fastest).</param>
    /// <param name="rateControl">Rate control mode (CRF maps to ICQ).</param>
    /// <param name="cqLevel">QP/ICQ level (1-51, lower is better quality).</param>
    /// <param name="maxBitrate">Maximum bitrate in kbps (for VBR mode, defaults to 1.5x bitrate).</param>
    public static VideoEncoder CreateQsvHevc(string name = "QSV HEVC", int bitrate = 6000, string targetUsage = "TU4", RateControl rateControl = RateControl.CBR, int cqLevel = 23, int? maxBitrate = null)
        => CreateQsv(Types.QsvHevc, name, bitrate, targetUsage, rateControl, cqLevel, maxBitrate);

    /// <summary>
    /// Creates an Intel QuickSync AV1 encoder (Arc GPUs and later).
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps (for CBR/VBR).</param>
    /// <param name="targetUsage">Quality/speed trade-off: "TU1" (best quality) to "TU7" (fastest).</param>
    /// <param name="rateControl">Rate control mode (CRF maps to ICQ).</param>
    /// <param name="cqLevel">QP level (1-63 for CQP, 1-51 for ICQ; lower is better quality).</param>
    /// <param name="maxBitrate">Maximum bitrate in kbps (for VBR mode, defaults to 1.5x bitrate).</param>
    public static VideoEncoder CreateQsvAv1(string name = "QSV AV1", int bitrate = 6000, string targetUsage = "TU4", RateControl rateControl = RateControl.CBR, int cqLevel = 23, int? maxBitrate = null)
        => CreateQsv(Types.QsvAv1, name, bitrate, targetUsage, rateControl, cqLevel, maxBitrate);

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

    /// <summary>Gets this encoder instance's capability flags (OBS_ENCODER_CAP_*).</summary>
    public EncoderCaps Caps => (EncoderCaps)ObsEncoder.obs_encoder_get_caps(Handle);

    /// <summary>
    /// Gets whether this encoder consumes GPU textures directly (a hardware/GPU encoder),
    /// i.e. it advertises the <see cref="EncoderCaps.PassTexture"/> capability.
    /// </summary>
    public bool SupportsTextureEncode => (Caps & EncoderCaps.PassTexture) != 0;

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

    /// <summary>
    /// Scales on the GPU instead of the CPU when a scaled size is set
    /// (e.g. record a 1440p canvas at 1080p without the CPU cost).
    /// Must be configured before the encoder becomes active.
    /// </summary>
    /// <param name="scaleType">The scaling filter, or <see cref="ObsScaleType.Disable"/> to scale on the CPU.</param>
    public VideoEncoder SetGpuScaleType(ObsScaleType scaleType)
    {
        ObsEncoder.obs_encoder_set_gpu_scale_type(Handle, scaleType);
        return this;
    }

    /// <summary>
    /// Gets whether GPU-based scaling is enabled for this encoder.
    /// </summary>
    public bool IsGpuScalingEnabled => ObsEncoder.obs_encoder_gpu_scaling_enabled(Handle);

    /// <summary>
    /// Gets or sets the frame rate divisor: the encoder runs at the base frame rate
    /// divided by this value (e.g. 2 records a 60 FPS canvas at 30 FPS).
    /// Can only be changed while the encoder is stopped.
    /// </summary>
    /// <exception cref="InvalidOperationException">The divisor was rejected (encoder active or divisor 0).</exception>
    public uint FrameRateDivisor
    {
        get => ObsEncoder.obs_encoder_get_frame_rate_divisor(Handle);
        set
        {
            if (!ObsEncoder.obs_encoder_set_frame_rate_divisor(Handle, value))
                throw new InvalidOperationException(
                    "Failed to set the frame rate divisor. It must be non-zero and the encoder must not be active.");
        }
    }

    /// <summary>
    /// Sets the scaled output size and enables GPU scaling in one call.
    /// </summary>
    /// <param name="width">The output width.</param>
    /// <param name="height">The output height.</param>
    /// <param name="scaleType">The GPU scaling filter (default bicubic, matching OBS Studio).</param>
    public VideoEncoder SetGpuScaledSize(uint width, uint height, ObsScaleType scaleType = ObsScaleType.Bicubic)
    {
        ObsEncoder.obs_encoder_set_scaled_size(Handle, width, height);
        ObsEncoder.obs_encoder_set_gpu_scale_type(Handle, scaleType);
        return this;
    }

    internal void SetVideo(VideoHandle video)
    {
        ObsEncoder.obs_encoder_set_video(Handle, video);
    }

    /// <summary>
    /// Attaches this encoder to an output, incrementing the ref count.
    /// </summary>
    internal void Attach()
    {
        lock (_refLock)
        {
            _refCount++;
        }
    }

    /// <summary>
    /// Detaches this encoder from an output, decrementing the ref count.
    /// When ref count reaches 0 and AutoDispose is enabled, the encoder is disposed.
    /// </summary>
    internal void Detach()
    {
        bool shouldDispose = false;
        lock (_refLock)
        {
            _refCount--;
            if (_refCount <= 0 && Obs.AutoDispose)
            {
                shouldDispose = true;
            }
        }

        if (shouldDispose)
        {
            Dispose();
        }
    }

    /// <summary>
    /// Gets the current reference count (number of outputs using this encoder).
    /// </summary>
    public int RefCount
    {
        get
        {
            lock (_refLock)
            {
                return _refCount;
            }
        }
    }

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsEncoder.obs_encoder_release((ObsEncoderHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => $"VideoEncoder[{TypeId}]: {Name}";
}
