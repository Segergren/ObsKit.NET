using ObsKit.NET.Encoders;
using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Outputs;

/// <summary>
/// Container format for recording output.
/// For non-hybrid formats the container is determined by the file extension of the
/// output path; the enum value selects the output implementation and muxer flags.
/// </summary>
public enum RecordingFormat
{
    /// <summary>Hybrid MP4 - crash-resilient MP4 with chapter marker support (recommended, OBS 30.2+).</summary>
    HybridMp4,
    /// <summary>MP4 - widely compatible, good for sharing.</summary>
    Mp4,
    /// <summary>MKV - flexible container, resilient to crashes.</summary>
    Mkv,
    /// <summary>Hybrid MOV - crash-resilient MOV with chapter marker support (OBS 30.2+).</summary>
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
    /// <summary>The OBS source type ID for the ffmpeg muxer.</summary>
    public const string SourceTypeId = "ffmpeg_muxer";

    /// <summary>The OBS output type ID for the crash-resilient Hybrid MP4 muxer (OBS 30.2+).</summary>
    public const string HybridMp4TypeId = "mp4_output";

    /// <summary>The OBS output type ID for the crash-resilient Hybrid MOV muxer (OBS 30.2+).</summary>
    public const string HybridMovTypeId = "mov_output";

    private VideoEncoder? _videoEncoder;
    private AudioEncoder? _audioEncoder;
    private bool _encodersOwned;

    /// <summary>
    /// Creates a new recording output.
    /// </summary>
    /// <param name="name">The output name.</param>
    /// <param name="path">Optional output file path.</param>
    /// <param name="format">Optional container format string.</param>
    public RecordingOutput(string name = "Recording", string? path = null, string? format = null)
        : base(SourceTypeId, name)
    {
        if (!string.IsNullOrEmpty(path))
        {
            Update(s => s.Set("path", path));
        }

        if (!string.IsNullOrEmpty(format))
        {
            SetFormat(format);
        }
    }

    /// <summary>Gets or sets the output file path.</summary>
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

    /// <summary>Sets the output file path.</summary>
    public RecordingOutput SetPath(string path)
    {
        Path = path;
        return this;
    }

    /// <summary>
    /// Sets the container format. Hybrid formats switch to the crash-resilient
    /// mp4_output/mov_output implementation; other formats use the ffmpeg muxer,
    /// where the container is determined by the output path's file extension.
    /// Must be called before the output is started.
    /// </summary>
    public RecordingOutput SetFormat(RecordingFormat format)
    {
        RecreateAs(format switch
        {
            RecordingFormat.HybridMp4 => HybridMp4TypeId,
            RecordingFormat.HybridMov => HybridMovTypeId,
            _ => SourceTypeId
        });

        using var settings = GetSettings();
        var muxerSettings = settings.GetString("muxer_settings");

        if (format is RecordingFormat.FragmentedMp4 or RecordingFormat.FragmentedMov)
        {
            // Match the OBS frontend: fragmented recording is the ffmpeg muxer
            // with fragmentation movflags (unless the user set custom movflags).
            if (string.IsNullOrEmpty(muxerSettings) || !muxerSettings.Contains("movflags"))
            {
                Update(s => s.Set("muxer_settings", FragmentedMovflags));
            }
        }
        else if (muxerSettings == FragmentedMovflags)
        {
            // Clear the movflags we added for a previous fragmented format.
            Update(s => s.Set("muxer_settings", ""));
        }

        return this;
    }

    private const string FragmentedMovflags = "movflags=frag_keyframe+empty_moov+delay_moov";

    /// <summary>Sets the container format using a format string (e.g. "hybrid_mp4", "mkv").</summary>
    /// <param name="format">Container format string for advanced use.</param>
    public RecordingOutput SetFormat(string format)
    {
        return SetFormat(format switch
        {
            "hybrid_mp4" => RecordingFormat.HybridMp4,
            "hybrid_mov" => RecordingFormat.HybridMov,
            "fragmented_mp4" => RecordingFormat.FragmentedMp4,
            "fragmented_mov" => RecordingFormat.FragmentedMov,
            "mov" => RecordingFormat.Mov,
            "mkv" => RecordingFormat.Mkv,
            "flv" => RecordingFormat.Flv,
            "mpegts" => RecordingFormat.Ts,
            "avi" => RecordingFormat.Avi,
            _ => RecordingFormat.Mp4
        });
    }

    /// <summary>
    /// Sets the video encoder for recording.
    /// </summary>
    /// <param name="encoder">The video encoder.</param>
    /// <param name="takeOwnership">If true, disposes the encoder when output is disposed.</param>
    public RecordingOutput WithVideoEncoder(VideoEncoder encoder, bool takeOwnership = false)
    {
        _videoEncoder = encoder;
        _encodersOwned = takeOwnership;

        var video = ObsCore.obs_get_video();
        encoder.SetVideo(video);

        SetVideoEncoder(encoder);
        return this;
    }

    /// <summary>
    /// Sets the video encoder for recording, taking video from a specific canvas
    /// instead of the main one (e.g. a vertical canvas).
    /// </summary>
    /// <param name="encoder">The video encoder.</param>
    /// <param name="canvas">The canvas whose video mix is recorded.</param>
    /// <param name="takeOwnership">If true, disposes the encoder when output is disposed.</param>
    public RecordingOutput WithVideoEncoder(VideoEncoder encoder, Scenes.Canvas canvas, bool takeOwnership = false)
    {
        _videoEncoder = encoder;
        _encodersOwned = takeOwnership;

        encoder.SetVideo(canvas.Video);

        SetVideoEncoder(encoder);
        return this;
    }

    /// <summary>
    /// Sets the audio encoder for recording.
    /// </summary>
    /// <param name="encoder">The audio encoder.</param>
    /// <param name="takeOwnership">If true, disposes the encoder when output is disposed.</param>
    /// <param name="track">The audio track index.</param>
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
    /// Configures with the best available hardware encoder (NVENC → AMF → QuickSync),
    /// falling back to x264 if no hardware encoder is present, plus an AAC audio encoder.
    /// </summary>
    /// <param name="videoBitrate">Video bitrate in kbps.</param>
    /// <param name="audioBitrate">Audio bitrate in kbps.</param>
    /// <param name="preferHevc">Try the vendor's HEVC encoder before H.264.</param>
    public RecordingOutput WithBestEncoders(int videoBitrate = 6000, int audioBitrate = 192, bool preferHevc = false)
    {
        var videoEncoder = VideoEncoder.CreateBest("Recording Video", videoBitrate, preferHevc);
        var audioEncoder = AudioEncoder.CreateAac("Recording Audio", audioBitrate);

        WithVideoEncoder(videoEncoder, takeOwnership: true);
        WithAudioEncoder(audioEncoder, takeOwnership: true);

        return this;
    }

    /// <summary>
    /// Configures with NVENC encoders (NVIDIA GPU required).
    /// </summary>
    /// <param name="videoBitrate">Video bitrate in kbps.</param>
    /// <param name="audioBitrate">Audio bitrate in kbps.</param>
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

    /// <summary>
    /// Inserts a chapter marker at the current recording time.
    /// Only supported by the Hybrid MP4/MOV formats
    /// (<see cref="RecordingFormat.HybridMp4"/> / <see cref="RecordingFormat.HybridMov"/>).
    /// </summary>
    /// <param name="name">Optional chapter name. If null, OBS generates one ("Unnamed N").</param>
    /// <returns>True if the chapter marker was added.</returns>
    public bool AddChapter(string? name = null)
    {
        if (!IsActive || IsPaused)
            return false;

        var procHandler = ObsOutput.obs_output_get_proc_handler(Handle);
        if (procHandler == 0)
            return false;

        var calldata = ObsSignal.calldata_create();
        try
        {
            if (!string.IsNullOrEmpty(name))
                ObsSignal.calldata_set_string(calldata, "chapter_name", name);

            return ObsSignal.proc_handler_call(procHandler, "add_chapter", calldata);
        }
        finally
        {
            ObsSignal.calldata_destroy(calldata);
        }
    }

    /// <summary>
    /// Enables automatic file splitting. Files are written to <paramref name="directory"/>
    /// using <paramref name="filenameFormat"/> and split when a limit is reached
    /// (0 disables that limit; manual splits via <see cref="SplitFile"/> remain available).
    /// The first file's path is generated from these settings when the output starts,
    /// overriding any path set via <see cref="SetPath"/>.
    /// </summary>
    /// <param name="directory">The directory to write recording files to.</param>
    /// <param name="maxTimeSeconds">Split when a file reaches this duration (0 = no time limit).</param>
    /// <param name="maxSizeMb">Split when a file reaches this size in MB (0 = no size limit).</param>
    /// <param name="filenameFormat">Filename format template (OBS specifiers like %CCYY-%MM-%DD).</param>
    /// <param name="extension">File extension, e.g. "mp4" or "mkv".</param>
    /// <param name="allowSpaces">Whether to allow spaces in generated filenames.</param>
    /// <param name="allowOverwrite">Whether existing files may be overwritten.</param>
    public RecordingOutput WithFileSplitting(string directory, int maxTimeSeconds = 0, int maxSizeMb = 0,
        string filenameFormat = "%CCYY-%MM-%DD %hh-%mm-%ss", string extension = "mp4",
        bool allowSpaces = true, bool allowOverwrite = false)
    {
        Update(s => s
            .Set("directory", directory)
            .Set("format", filenameFormat)
            .Set("extension", extension)
            .Set("allow_spaces", allowSpaces)
            .Set("allow_overwrite", allowOverwrite)
            .Set("split_file", true)
            .Set("max_time_sec", (long)maxTimeSeconds)
            .Set("max_size_mb", (long)maxSizeMb));
        return this;
    }

    /// <summary>
    /// Manually splits the recording into a new file now.
    /// Requires file splitting to be enabled via <see cref="WithFileSplitting"/>.
    /// The new file path is reported by the <see cref="Signals.OutputSignal.FileChanged"/> signal.
    /// </summary>
    /// <returns>True if the split was triggered, false if inactive or splitting is disabled.</returns>
    public bool SplitFile()
    {
        if (!IsActive)
            return false;

        var procHandler = ObsOutput.obs_output_get_proc_handler(Handle);
        if (procHandler == 0)
            return false;

        var calldata = ObsSignal.calldata_create();
        try
        {
            if (!ObsSignal.proc_handler_call(procHandler, "split_file", calldata))
                return false;

            ObsSignal.calldata_get_bool(calldata, "split_file_enabled", out var enabled);
            return enabled;
        }
        finally
        {
            ObsSignal.calldata_destroy(calldata);
        }
    }

    /// <summary>Starts the recording.</summary>
    public new bool Start()
    {
        if (_videoEncoder == null)
        {
            throw new InvalidOperationException("No video encoder configured. Call WithVideoEncoder() or WithDefaultEncoders() first.");
        }

        GenerateSplitFilePath();
        return base.Start();
    }

    /// <summary>
    /// When file splitting is enabled, generates the first file's path from the
    /// directory/format/extension settings, like the OBS frontend does. The muxer
    /// only generates names for the second file onward.
    /// </summary>
    private void GenerateSplitFilePath()
    {
        using var settings = GetSettings();
        if (!settings.GetBool("split_file"))
            return;

        var directory = settings.GetString("directory");
        if (string.IsNullOrEmpty(directory))
            return;

        var filename = ObsCore.os_generate_formatted_filename(
            settings.GetString("extension") ?? "mp4",
            settings.GetBool("allow_spaces"),
            settings.GetString("format") ?? "%CCYY-%MM-%DD %hh-%mm-%ss");

        if (string.IsNullOrEmpty(filename))
            return;

        var path = $"{directory.TrimEnd('/', '\\').Replace('\\', '/')}/{filename}";
        Update(s => s.Set("path", path));
    }

    /// <summary>Stops the recording.</summary>
    /// <returns>True if the recording stopped successfully, false if timed out.</returns>
    public bool Stop() => base.Stop();

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
