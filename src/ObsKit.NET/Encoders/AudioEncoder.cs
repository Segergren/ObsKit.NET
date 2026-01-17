using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Encoders;

/// <summary>
/// Represents an OBS audio encoder (obs_encoder_t).
/// </summary>
public sealed class AudioEncoder : ObsObject
{
    public static class Types
    {
        public const string FfmpegAac = "ffmpeg_aac";
        public const string CoreAudioAac = "CoreAudio_AAC";
        public const string MfAac = "mf_aac";
        public const string Opus = "ffmpeg_opus";
        public const string Flac = "ffmpeg_flac";
        public const string Pcm = "ffmpeg_pcm_s16le";
    }

    /// <param name="mixerIdx">The audio mixer index (0-5).</param>
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

    /// <param name="bitrate">Bitrate in kbps.</param>
    public static AudioEncoder CreateAac(string name = "Audio Encoder", int bitrate = 192, int mixerIdx = 0)
    {
        using var settings = new Settings();
        settings.Set("bitrate", bitrate);
        return new AudioEncoder(Types.FfmpegAac, name, mixerIdx, settings);
    }

    /// <param name="bitrate">Bitrate in kbps.</param>
    public static AudioEncoder CreateOpus(string name = "Opus Encoder", int bitrate = 192, int mixerIdx = 0)
    {
        using var settings = new Settings();
        settings.Set("bitrate", bitrate);
        return new AudioEncoder(Types.Opus, name, mixerIdx, settings);
    }

    internal new ObsEncoderHandle Handle => (ObsEncoderHandle)base.Handle;

    public string? TypeId { get; }

    public int MixerIndex { get; }

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

    public uint SampleRate => ObsEncoder.obs_encoder_get_sample_rate(Handle);

    public nuint FrameSize => ObsEncoder.obs_encoder_get_frame_size(Handle);

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

    internal void SetAudio(AudioHandle audio)
    {
        ObsEncoder.obs_encoder_set_audio(Handle, audio);
    }

    protected override void ReleaseHandle(nint handle)
    {
        ObsEncoder.obs_encoder_release((ObsEncoderHandle)handle);
    }

    public override string ToString() => $"AudioEncoder[{TypeId}]: {Name}";
}
