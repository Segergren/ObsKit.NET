using ObsKit.NET.Encoders;
using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Outputs;

/// <summary>
/// Container format for recording output.
/// </summary>
public enum RecordingFormat
{
    /// <summary>Hybrid MP4 - crash-resilient MP4 with chapter markers support (recommended).</summary>
    HybridMp4,
    /// <summary>MP4 - widely compatible, good for sharing.</summary>
    Mp4,
    /// <summary>MKV - flexible container, resilient to crashes.</summary>
    Mkv,
    /// <summary>Hybrid MOV - crash-resilient MOV with chapter markers support.</summary>
    HybridMov,
    /// <summary>MOV - Apple QuickTime format.</summary>
    Mov,
    /// <summary>FLV - Flash video, good for streaming.</summary>
    Flv,
    /// <summary>MPEG-TS - transport stream, resilient to crashes.</summary>
    Ts,
    /// <summary>AVI - legacy format.</summary>
    Avi,
    /// <summary>Fragmented MP4 - streaming-friendly MP4.</summary>
    FragmentedMp4,
    /// <summary>Fragmented MOV - streaming-friendly MOV.</summary>
    FragmentedMov
}

/// <summary>
/// Recording output for saving video/audio to a file.
/// </summary>
public sealed class RecordingOutput : Output
{
    public const string SourceTypeId = "ffmpeg_muxer";

    private VideoEncoder? _videoEncoder;
    private AudioEncoder? _audioEncoder;
    private bool _encodersOwned;

    /// <param name="format">Container format (e.g., "mp4", "mkv", "flv").</param>
    public RecordingOutput(string name = "Recording", string? path = null, string? format = null)
        : base(SourceTypeId, name)
    {
        if (!string.IsNullOrEmpty(path) || !string.IsNullOrEmpty(format))
        {
            Update(s =>
            {
                if (!string.IsNullOrEmpty(path))
                    s.Set("path", path);
                if (!string.IsNullOrEmpty(format))
                    s.Set("format", format);
            });
        }
    }

    public string? Path
    {
        get
        {
            using var settings = GetSettings();
            return settings.GetString("path");
        }
        set
        {
            if (value != null)
            {
                Update(s => s.Set("path", value));
            }
        }
    }

    public RecordingOutput SetPath(string path)
    {
        Path = path;
        return this;
    }

    /// <summary>
    /// Sets the container format.
    /// </summary>
    public RecordingOutput SetFormat(RecordingFormat format)
    {
        var formatString = format switch
        {
            RecordingFormat.HybridMp4 => "hybrid_mp4",
            RecordingFormat.Mp4 => "mp4",
            RecordingFormat.Mkv => "mkv",
            RecordingFormat.HybridMov => "hybrid_mov",
            RecordingFormat.Mov => "mov",
            RecordingFormat.Flv => "flv",
            RecordingFormat.Ts => "mpegts",
            RecordingFormat.Avi => "avi",
            RecordingFormat.FragmentedMp4 => "fragmented_mp4",
            RecordingFormat.FragmentedMov => "fragmented_mov",
            _ => "hybrid_mp4"
        };
        return SetFormat(formatString);
    }

    /// <param name="format">Container format string for advanced use.</param>
    public RecordingOutput SetFormat(string format)
    {
        Update(s => s.Set("format", format));
        return this;
    }

    /// <param name="takeOwnership">Dispose encoder when output is disposed.</param>
    public RecordingOutput WithVideoEncoder(VideoEncoder encoder, bool takeOwnership = false)
    {
        _videoEncoder = encoder;
        _encodersOwned = takeOwnership;

        var video = ObsCore.obs_get_video();
        encoder.SetVideo(video);

        SetVideoEncoder(encoder);
        return this;
    }

    /// <param name="takeOwnership">Dispose encoder when output is disposed.</param>
    public RecordingOutput WithAudioEncoder(AudioEncoder encoder, bool takeOwnership = false, int track = 0)
    {
        _audioEncoder = encoder;
        _encodersOwned = takeOwnership;

        var audio = ObsCore.obs_get_audio();
        encoder.SetAudio(audio);

        SetAudioEncoder(encoder, track);
        return this;
    }

    /// <summary>
    /// Configures with default encoders (x264 video, AAC audio).
    /// </summary>
    /// <param name="videoBitrate">Video bitrate in kbps.</param>
    /// <param name="audioBitrate">Audio bitrate in kbps.</param>
    public RecordingOutput WithDefaultEncoders(int videoBitrate = 6000, int audioBitrate = 192)
    {
        var videoEncoder = VideoEncoder.CreateX264("Recording Video", videoBitrate);
        var audioEncoder = AudioEncoder.CreateAac("Recording Audio", audioBitrate);

        WithVideoEncoder(videoEncoder, takeOwnership: true);
        WithAudioEncoder(audioEncoder, takeOwnership: true);

        return this;
    }

    /// <summary>
    /// Configures with NVENC encoders (NVIDIA GPU required).
    /// </summary>
    /// <param name="hevc">Use HEVC instead of H.264.</param>
    public RecordingOutput WithNvencEncoders(int videoBitrate = 6000, int audioBitrate = 192, bool hevc = false)
    {
        var videoEncoder = hevc
            ? VideoEncoder.CreateNvencHevc("Recording Video", videoBitrate)
            : VideoEncoder.CreateNvencH264("Recording Video", videoBitrate);
        var audioEncoder = AudioEncoder.CreateAac("Recording Audio", audioBitrate);

        WithVideoEncoder(videoEncoder, takeOwnership: true);
        WithAudioEncoder(audioEncoder, takeOwnership: true);

        return this;
    }

    public new bool Start()
    {
        if (_videoEncoder == null)
        {
            throw new InvalidOperationException("No video encoder configured. Call WithVideoEncoder() or WithDefaultEncoders() first.");
        }

        return base.Start();
    }

    public new void Stop()
    {
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _encodersOwned)
        {
            _videoEncoder?.Dispose();
            _audioEncoder?.Dispose();
        }

        base.Dispose(disposing);
    }
}
