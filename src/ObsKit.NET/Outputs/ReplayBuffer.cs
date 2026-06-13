using ObsKit.NET.Core;
using ObsKit.NET.Encoders;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Signals;

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
    private readonly object _savedLock = new();
    private SignalConnection? _savedConnection;
    private EventHandler<ReplaySavedEventArgs>? _saved;

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
    /// Sets the video encoder for the replay buffer, taking video from a specific
    /// canvas instead of the main one (e.g. a vertical canvas).
    /// </summary>
    /// <param name="encoder">The video encoder.</param>
    /// <param name="canvas">The canvas whose video mix is buffered.</param>
    /// <param name="takeOwnership">If true, disposes the encoder when output is disposed.</param>
    public ReplayBuffer WithVideoEncoder(VideoEncoder encoder, Scenes.Canvas canvas, bool takeOwnership = false)
    {
        _videoEncoder = encoder;
        _encodersOwned = takeOwnership;

        encoder.SetVideo(canvas.Video);

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
    /// Configures with the best available hardware encoder (NVENC → AMF → QuickSync),
    /// falling back to x264 if no hardware encoder is present, plus an AAC audio encoder.
    /// </summary>
    /// <param name="videoBitrate">Video bitrate in kbps.</param>
    /// <param name="audioBitrate">Audio bitrate in kbps.</param>
    /// <param name="preferHevc">Try the vendor's HEVC encoder before H.264.</param>
    public ReplayBuffer WithBestEncoders(int videoBitrate = 6000, int audioBitrate = 192, bool preferHevc = false)
    {
        var videoEncoder = VideoEncoder.CreateBest("Replay Video", videoBitrate, preferHevc);
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

    /// <summary>
    /// Event raised when a replay has finished saving to disk after <see cref="Save"/>.
    /// The saved file path is provided in the event args.
    /// Raised on an OBS internal thread — do not block in the handler.
    /// </summary>
    public event EventHandler<ReplaySavedEventArgs>? Saved
    {
        add
        {
            lock (_savedLock)
            {
                _savedConnection ??= ConnectSignal(OutputSignal.Saved, OnSavedSignal);
                _saved += value;
            }
        }
        remove
        {
            lock (_savedLock)
            {
                _saved -= value;
            }
        }
    }

    private void OnSavedSignal(nint calldata)
    {
        _saved?.Invoke(this, new ReplaySavedEventArgs(GetLastReplayPath()));
    }

    /// <summary>
    /// Saves the current buffer and asynchronously waits for the file to finish writing.
    /// </summary>
    /// <param name="timeout">Maximum time to wait for the save to complete. Defaults to 30 seconds.</param>
    /// <param name="cancellationToken">Cancels the wait (the save itself is not aborted).</param>
    /// <returns>The saved file path, or null if OBS did not report one.</returns>
    /// <exception cref="TimeoutException">The save did not complete within the timeout.</exception>
    public async Task<string?> SaveAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var connection = ConnectSignal(OutputSignal.Saved, _ => tcs.TrySetResult(GetLastReplayPath()));

        Save();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout ?? TimeSpan.FromSeconds(30));
        using var registration = timeoutCts.Token.Register(() =>
        {
            if (cancellationToken.IsCancellationRequested)
                tcs.TrySetCanceled(cancellationToken);
            else
                tcs.TrySetException(new TimeoutException("Timed out waiting for the replay buffer to finish saving."));
        });

        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Clears the buffered footage by restarting the buffer, so the next save only
    /// contains footage recorded after this call. Saving does not clear OBS's
    /// in-memory buffer, so without a reset two saves close together contain
    /// overlapping footage. Call after <see cref="SaveAsync"/> completes.
    /// </summary>
    /// <remarks>
    /// libobs has no native flush, so this stops and restarts the output —
    /// footage is not buffered during the brief restart window. Unlike
    /// <see cref="Output.Stop"/>, this never auto-disposes the output,
    /// regardless of <c>Obs.AutoDispose</c>.
    /// </remarks>
    /// <param name="stopTimeout">Maximum time to wait for a clean stop before forcing it (default 30 seconds).</param>
    /// <param name="cancellationToken">Cancels the wait.</param>
    /// <returns>True if the buffer restarted successfully.</returns>
    public async Task<bool> ResetAsync(TimeSpan? stopTimeout = null, CancellationToken cancellationToken = default)
    {
        if (IsActive)
        {
            // Stop natively rather than via Output.Stop, which would dispose this
            // output when Obs.AutoDispose is enabled.
            ObsOutput.obs_output_stop(Handle);

            var deadline = Environment.TickCount64 + (long)(stopTimeout ?? TimeSpan.FromSeconds(30)).TotalMilliseconds;
            while (IsActive && Environment.TickCount64 < deadline)
                await Task.Delay(50, cancellationToken).ConfigureAwait(false);

            if (IsActive)
            {
                ForceStop();
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }

        return Start();
    }

    /// <summary>
    /// Gets the file path of the last saved replay.
    /// Returns null if no replay has been saved yet or if currently muxing.
    /// </summary>
    /// <returns>The file path of the last saved replay, or null if not available.</returns>
    public string? GetLastReplayPath()
    {
        var procHandler = ObsOutput.obs_output_get_proc_handler(Handle);
        if (procHandler == 0)
            return null;

        var calldata = ObsSignal.calldata_create();
        try
        {
            ObsSignal.proc_handler_call(procHandler, "get_last_replay", calldata);

            if (ObsSignal.calldata_get_string(calldata, "path", out var pathPtr) && pathPtr != nint.Zero)
            {
                return System.Runtime.InteropServices.Marshal.PtrToStringUTF8(pathPtr);
            }

            return null;
        }
        finally
        {
            ObsSignal.calldata_destroy(calldata);
        }
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
    /// <returns>True if the replay buffer stopped successfully, false if timed out.</returns>
    public bool Stop() => base.Stop();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_savedLock)
            {
                _savedConnection?.Dispose();
                _savedConnection = null;
            }

            if (_encodersOwned)
            {
                _videoEncoder?.Dispose();
                _audioEncoder?.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}

/// <summary>
/// Event arguments for <see cref="ReplayBuffer.Saved"/>.
/// </summary>
public sealed class ReplaySavedEventArgs : EventArgs
{
    /// <summary>The path of the saved replay file, or null if OBS did not report one.</summary>
    public string? Path { get; }

    internal ReplaySavedEventArgs(string? path) => Path = path;
}
