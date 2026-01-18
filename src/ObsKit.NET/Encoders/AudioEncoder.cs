using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Encoders;

/// <summary>
/// Represents an OBS audio encoder (obs_encoder_t).
/// </summary>
public sealed class AudioEncoder : ObsObject
{
    /// <summary>Known audio encoder type IDs.</summary>
    public static class Types
    {
        /// <summary>FFmpeg AAC encoder.</summary>
        public const string FfmpegAac = "ffmpeg_aac";
        /// <summary>Core Audio AAC encoder (macOS).</summary>
        public const string CoreAudioAac = "CoreAudio_AAC";
        /// <summary>Media Foundation AAC encoder (Windows).</summary>
        public const string MfAac = "mf_aac";
        /// <summary>Opus encoder.</summary>
        public const string Opus = "ffmpeg_opus";
        /// <summary>FLAC lossless encoder.</summary>
        public const string Flac = "ffmpeg_flac";
        /// <summary>PCM 16-bit encoder.</summary>
        public const string Pcm = "ffmpeg_pcm_s16le";
    }

    /// <summary>
    /// Creates an audio encoder.
    /// </summary>
    /// <param name="typeId">The encoder type ID.</param>
    /// <param name="name">The encoder name.</param>
    /// <param name="mixerIdx">The audio mixer index (0-5).</param>
    /// <param name="settings">Optional encoder settings.</param>
    /// <param name="hotkeyData">Optional hotkey data.</param>
    public AudioEncoder(string typeId, string name, int mixerIdx = 0, Settings? settings = null, Settings? hotkeyData = null)
        : base(CreateEncoder(typeId, name, mixerIdx, settings, hotkeyData))
    {
        TypeId = typeId;
        MixerIndex = mixerIdx;
    }

    internal AudioEncoder(ObsEncoderHandle handle, string? typeId = null, int mixerIdx = 0, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
        TypeId = typeId ?? ObsEncoder.obs_encoder_get_id(handle);
        MixerIndex = mixerIdx;
    }

    private static nint CreateEncoder(string typeId, string name, int mixerIdx, Settings? settings, Settings? hotkeyData)
    {
        ThrowIfNotInitialized();
        var handle = ObsEncoder.obs_audio_encoder_create(
            typeId,
            name,
            settings?.Handle ?? default,
            (nuint)mixerIdx,
            hotkeyData?.Handle ?? default);

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create audio encoder of type '{typeId}'");

        return handle;
    }

    /// <summary>
    /// Creates an AAC audio encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps.</param>
    /// <param name="mixerIdx">The audio mixer index (0-5).</param>
    public static AudioEncoder CreateAac(string name = "Audio Encoder", int bitrate = 192, int mixerIdx = 0)
    {
        using var settings = new Settings();
        settings.Set("bitrate", bitrate);
        return new AudioEncoder(Types.FfmpegAac, name, mixerIdx, settings);
    }

    /// <summary>
    /// Creates an Opus audio encoder.
    /// </summary>
    /// <param name="name">The encoder name.</param>
    /// <param name="bitrate">Bitrate in kbps.</param>
    /// <param name="mixerIdx">The audio mixer index (0-5).</param>
    public static AudioEncoder CreateOpus(string name = "Opus Encoder", int bitrate = 192, int mixerIdx = 0)
    {
        using var settings = new Settings();
        settings.Set("bitrate", bitrate);
        return new AudioEncoder(Types.Opus, name, mixerIdx, settings);
    }

    internal new ObsEncoderHandle Handle => (ObsEncoderHandle)base.Handle;

    /// <summary>Gets the encoder type ID.</summary>
    public string? TypeId { get; }

    /// <summary>Gets the audio mixer index.</summary>
    public int MixerIndex { get; }

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

    /// <summary>Gets the audio sample rate.</summary>
    public uint SampleRate => ObsEncoder.obs_encoder_get_sample_rate(Handle);

    /// <summary>Gets the audio frame size.</summary>
    public nuint FrameSize => ObsEncoder.obs_encoder_get_frame_size(Handle);

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

    internal void SetAudio(AudioHandle audio)
    {
        ObsEncoder.obs_encoder_set_audio(Handle, audio);
    }

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsEncoder.obs_encoder_release((ObsEncoderHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => $"AudioEncoder[{TypeId}]: {Name}";
}
