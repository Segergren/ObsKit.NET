using ObsKit.NET.Core;
using ObsKit.NET.Encoders;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Outputs;

/// <summary>
/// Replay buffer output for saving the last N seconds of video on demand.
/// </summary>
public sealed class ReplayBuffer : Output
{
    public const string SourceTypeId = "replay_buffer";

    private VideoEncoder? _videoEncoder;
    private AudioEncoder? _audioEncoder;
    private bool _encodersOwned;

    /// <param name="maxSeconds">Maximum seconds to buffer.</param>
    /// <param name="maxSizeMb">Maximum buffer size in MB.</param>
    public ReplayBuffer(string name = "Replay Buffer", int maxSeconds = 20, int maxSizeMb = 512)
        : base(SourceTypeId, name)
    {
        Update(s =>
        {
            s.Set("max_time_sec", maxSeconds);
            s.Set("max_size_mb", maxSizeMb);
        });
    }

    /// <summary>
    /// Maximum buffer time in seconds.
    /// </summary>
    public int MaxSeconds
    {
        get
        {
            using var settings = GetSettings();
            return (int)settings.GetInt("max_time_sec");
        }
        set => Update(s => s.Set("max_time_sec", value));
    }

    /// <summary>
    /// Maximum buffer size in megabytes.
    /// </summary>
    public int MaxSizeMb
    {
        get
        {
            using var settings = GetSettings();
            return (int)settings.GetInt("max_size_mb");
        }
        set => Update(s => s.Set("max_size_mb", value));
    }

    public ReplayBuffer SetMaxTime(int seconds)
    {
        MaxSeconds = seconds;
        return this;
    }

    public ReplayBuffer SetMaxSize(int megabytes)
    {
        MaxSizeMb = megabytes;
        return this;
    }

    public ReplayBuffer SetDirectory(string directory)
    {
        Update(s => s.Set("directory", directory));
        return this;
    }

    /// <param name="format">Filename format (e.g., "Replay %CCYY-%MM-%DD %hh-%mm-%ss").</param>
    public ReplayBuffer SetFilenameFormat(string format)
    {
        Update(s => s.Set("filename_formatting", format));
        return this;
    }

    /// <param name="takeOwnership">Dispose encoder when output is disposed.</param>
    public ReplayBuffer WithVideoEncoder(VideoEncoder encoder, bool takeOwnership = false)
    {
        _videoEncoder = encoder;
        _encodersOwned = takeOwnership;

        var video = ObsCore.obs_get_video();
        encoder.SetVideo(video);

        SetVideoEncoder(encoder);
        return this;
    }

    /// <param name="takeOwnership">Dispose encoder when output is disposed.</param>
    public ReplayBuffer WithAudioEncoder(AudioEncoder encoder, bool takeOwnership = false, int track = 0)
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
    public ReplayBuffer WithDefaultEncoders(int videoBitrate = 6000, int audioBitrate = 192)
    {
        var videoEncoder = VideoEncoder.CreateX264("Replay Video", videoBitrate);
        var audioEncoder = AudioEncoder.CreateAac("Replay Audio", audioBitrate);

        WithVideoEncoder(videoEncoder, takeOwnership: true);
        WithAudioEncoder(audioEncoder, takeOwnership: true);

        return this;
    }

    /// <summary>
    /// Saves the current buffer to a file. Must be called while buffer is running.
    /// </summary>
    public void Save()
    {
        if (!IsActive)
            throw new InvalidOperationException("Replay buffer is not active. Call Start() first.");

        // Save is triggered via proc handler - currently requires frontend API or hotkey
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
