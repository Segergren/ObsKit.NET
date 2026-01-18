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
    /// <summary>The OBS source type ID for the replay buffer.</summary>
    public const string SourceTypeId = "replay_buffer";

    private VideoEncoder? _videoEncoder;
    private AudioEncoder? _audioEncoder;
    private bool _encodersOwned;

    /// <summary>
    /// Creates a new replay buffer.
    /// </summary>
    /// <param name="name">The output name.</param>
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
    /// Gets or sets the maximum buffer time in seconds.
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
    /// Gets or sets the maximum buffer size in megabytes.
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

    /// <summary>Sets the maximum buffer time.</summary>
    public ReplayBuffer SetMaxTime(int seconds)
    {
        MaxSeconds = seconds;
        return this;
    }

    /// <summary>Sets the maximum buffer size.</summary>
    public ReplayBuffer SetMaxSize(int megabytes)
    {
        MaxSizeMb = megabytes;
        return this;
    }

    /// <summary>Sets the output directory for saved replays.</summary>
    public ReplayBuffer SetDirectory(string directory)
    {
        Update(s => s.Set("directory", directory));
        return this;
    }

    /// <summary>
    /// Sets the filename format for saved replays.
    /// </summary>
    /// <param name="format">Filename format (e.g., "Replay %CCYY-%MM-%DD %hh-%mm-%ss").</param>
    public ReplayBuffer SetFilenameFormat(string format)
    {
        Update(s => s.Set("filename_formatting", format));
        return this;
    }

    /// <summary>
    /// Sets the video encoder for the replay buffer.
    /// </summary>
    /// <param name="encoder">The video encoder.</param>
    /// <param name="takeOwnership">If true, disposes the encoder when output is disposed.</param>
    public ReplayBuffer WithVideoEncoder(VideoEncoder encoder, bool takeOwnership = false)
    {
        _videoEncoder = encoder;
        _encodersOwned = takeOwnership;

        var video = ObsCore.obs_get_video();
        encoder.SetVideo(video);

        SetVideoEncoder(encoder);
        return this;
    }

    /// <summary>
    /// Sets the audio encoder for the replay buffer.
    /// </summary>
    /// <param name="encoder">The audio encoder.</param>
    /// <param name="takeOwnership">If true, disposes the encoder when output is disposed.</param>
    /// <param name="track">The audio track index.</param>
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
    /// <param name="videoBitrate">Video bitrate in kbps.</param>
    /// <param name="audioBitrate">Audio bitrate in kbps.</param>
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

        var procHandler = ObsOutput.obs_output_get_proc_handler(Handle);
        if (procHandler == 0)
            throw new InvalidOperationException("Failed to get proc handler for replay buffer.");

        ObsSignal.proc_handler_call(procHandler, "save");
    }

    /// <summary>Starts the replay buffer.</summary>
    public new bool Start()
    {
        if (_videoEncoder == null)
        {
            throw new InvalidOperationException("No video encoder configured. Call WithVideoEncoder() or WithDefaultEncoders() first.");
        }

        return base.Start();
    }

    /// <summary>Stops the replay buffer.</summary>
    public void Stop()
    {
        base.Stop();
    }

    /// <inheritdoc/>
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
