using ObsKit.NET.Encoders;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Services;
using ObsKit.NET.Signals;

namespace ObsKit.NET.Outputs;

/// <summary>
/// Streaming output for live streaming to RTMP/RTMPS servers.
/// Supports streaming to Twitch, YouTube, Facebook, and custom RTMP servers.
/// </summary>
public sealed class StreamingOutput : Output
{
    /// <summary>The OBS output type ID for RTMP streaming.</summary>
    public const string SourceTypeId = "rtmp_output";

    private VideoEncoder? _videoEncoder;
    private AudioEncoder? _audioEncoder;
    private Service? _service;
    private bool _encodersOwned;
    private bool _serviceOwned;

    /// <summary>
    /// Creates a new streaming output.
    /// </summary>
    /// <param name="name">The output name.</param>
    public StreamingOutput(string name = "Streaming")
        : base(SourceTypeId, name)
    {
    }

    /// <summary>
    /// Gets or sets the streaming service associated with this output.
    /// </summary>
    public Service? Service
    {
        get => _service;
        set
        {
            if (IsActive)
                throw new InvalidOperationException("Cannot change service while streaming is active.");

            _service = value;
            if (value != null)
            {
                ObsOutput.obs_output_set_service(Handle, value.Handle);
            }
        }
    }

    /// <summary>
    /// Gets the URL being streamed to (from the service).
    /// </summary>
    public string? Url => _service?.Url;

    /// <summary>
    /// Gets whether the output is currently reconnecting.
    /// </summary>
    public new bool IsReconnecting => base.IsReconnecting;

    /// <summary>
    /// Gets the network congestion value (0.0 to 1.0).
    /// Higher values indicate more congestion.
    /// </summary>
    public new float Congestion => base.Congestion;

    /// <summary>
    /// Gets the total bytes sent.
    /// </summary>
    public new ulong TotalBytes => base.TotalBytes;

    /// <summary>
    /// Gets the number of frames dropped due to network issues.
    /// </summary>
    public new int FramesDropped => base.FramesDropped;

    /// <summary>
    /// Gets the connection time in milliseconds.
    /// </summary>
    public new int ConnectTimeMs => base.ConnectTimeMs;

    #region Configuration Methods

    /// <summary>
    /// Sets the streaming service for this output.
    /// </summary>
    /// <param name="service">The service configuration.</param>
    /// <param name="takeOwnership">If true, disposes the service when the output is disposed.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithService(Service service, bool takeOwnership = false)
    {
        if (IsActive)
            throw new InvalidOperationException("Cannot change service while streaming is active.");

        _service = service;
        _serviceOwned = takeOwnership;
        ObsOutput.obs_output_set_service(Handle, service.Handle);
        return this;
    }

    /// <summary>
    /// Configures streaming to a custom RTMP server.
    /// </summary>
    /// <param name="serverUrl">The RTMP server URL (e.g., "rtmp://live.example.com/app").</param>
    /// <param name="streamKey">The stream key for authentication.</param>
    /// <param name="username">Optional username for authentication.</param>
    /// <param name="password">Optional password for authentication.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput ToCustomServer(string serverUrl, string streamKey, string? username = null, string? password = null)
    {
        var service = Service.CreateCustom(serverUrl, streamKey, "Custom Stream", username, password);
        return WithService(service, takeOwnership: true);
    }

    /// <summary>
    /// Configures streaming to Twitch.
    /// </summary>
    /// <param name="streamKey">Your Twitch stream key.</param>
    /// <param name="server">Server URL or "auto" for automatic selection.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput ToTwitch(string streamKey, string server = "auto")
    {
        var service = Service.CreateTwitch(streamKey, server);
        return WithService(service, takeOwnership: true);
    }

    /// <summary>
    /// Configures streaming to YouTube.
    /// </summary>
    /// <param name="streamKey">Your YouTube stream key.</param>
    /// <param name="server">Optional server URL.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput ToYouTube(string streamKey, string? server = null)
    {
        var service = Service.CreateYouTube(streamKey, server);
        return WithService(service, takeOwnership: true);
    }

    /// <summary>
    /// Configures streaming to Facebook Live.
    /// </summary>
    /// <param name="streamKey">Your Facebook stream key.</param>
    /// <param name="server">Optional server URL.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput ToFacebook(string streamKey, string? server = null)
    {
        var service = Service.CreateFacebook(streamKey, server);
        return WithService(service, takeOwnership: true);
    }

    /// <summary>
    /// Sets the video encoder for streaming.
    /// </summary>
    /// <param name="encoder">The video encoder.</param>
    /// <param name="takeOwnership">If true, disposes the encoder when output is disposed.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithVideoEncoder(VideoEncoder encoder, bool takeOwnership = false)
    {
        _videoEncoder = encoder;
        _encodersOwned = takeOwnership;

        var video = ObsCore.obs_get_video();
        encoder.SetVideo(video);

        SetVideoEncoder(encoder);
        return this;
    }

    /// <summary>
    /// Sets the audio encoder for streaming.
    /// </summary>
    /// <param name="encoder">The audio encoder.</param>
    /// <param name="takeOwnership">If true, disposes the encoder when output is disposed.</param>
    /// <param name="track">The audio track index.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithAudioEncoder(AudioEncoder encoder, bool takeOwnership = false, int track = 0)
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
    /// <param name="preset">x264 preset (e.g., "veryfast", "medium", "slow").</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithDefaultEncoders(int videoBitrate = 4500, int audioBitrate = 160, string preset = "veryfast")
    {
        var videoEncoder = VideoEncoder.CreateX264("Streaming Video", videoBitrate, preset);
        var audioEncoder = AudioEncoder.CreateAac("Streaming Audio", audioBitrate);

        WithVideoEncoder(videoEncoder, takeOwnership: true);
        WithAudioEncoder(audioEncoder, takeOwnership: true);

        return this;
    }

    /// <summary>
    /// Configures with NVENC encoders (NVIDIA GPU required).
    /// </summary>
    /// <param name="videoBitrate">Video bitrate in kbps.</param>
    /// <param name="audioBitrate">Audio bitrate in kbps.</param>
    /// <param name="hevc">Use HEVC instead of H.264 (requires platform support).</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithNvencEncoders(int videoBitrate = 6000, int audioBitrate = 160, bool hevc = false)
    {
        var videoEncoder = hevc
            ? VideoEncoder.CreateNvencHevc("Streaming Video", videoBitrate)
            : VideoEncoder.CreateNvencH264("Streaming Video", videoBitrate);
        var audioEncoder = AudioEncoder.CreateAac("Streaming Audio", audioBitrate);

        WithVideoEncoder(videoEncoder, takeOwnership: true);
        WithAudioEncoder(audioEncoder, takeOwnership: true);

        return this;
    }

    /// <summary>
    /// Sets the output delay in seconds.
    /// </summary>
    /// <param name="delaySec">Delay in seconds (0 to disable).</param>
    /// <param name="preserveOnStop">If true, preserves the delay buffer when stopping.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithDelay(uint delaySec, bool preserveOnStop = false)
    {
        uint flags = preserveOnStop ? 1u : 0u; // OBS_OUTPUT_DELAY_PRESERVE
        SetDelay(delaySec, flags);
        return this;
    }

    /// <summary>
    /// Configures reconnection settings via the output settings.
    /// </summary>
    /// <param name="enabled">Whether to enable auto-reconnect.</param>
    /// <param name="retryDelaySec">Delay between reconnect attempts in seconds.</param>
    /// <param name="maxRetries">Maximum number of reconnect attempts (0 for unlimited).</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithReconnect(bool enabled = true, int retryDelaySec = 10, int maxRetries = 20)
    {
        Update(s =>
        {
            s.Set("reconnect", enabled);
            s.Set("retry_delay", retryDelaySec);
            s.Set("max_retries", maxRetries);
        });
        return this;
    }

    /// <summary>
    /// Sets the bind IP address for the output connection.
    /// </summary>
    /// <param name="ip">IP address to bind to, or "default" for system default.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithBindIp(string ip)
    {
        Update(s => s.Set("bind_ip", ip));
        return this;
    }

    /// <summary>
    /// Enables or disables new network code (improved socket handling).
    /// </summary>
    /// <param name="enabled">Whether to enable new network code.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithNewNetworkCode(bool enabled = true)
    {
        Update(s => s.Set("new_socket_loop_enabled", enabled));
        return this;
    }

    /// <summary>
    /// Enables or disables low latency mode.
    /// </summary>
    /// <param name="enabled">Whether to enable low latency mode.</param>
    /// <returns>This output for method chaining.</returns>
    public StreamingOutput WithLowLatencyMode(bool enabled = true)
    {
        Update(s => s.Set("low_latency_mode_enabled", enabled));
        return this;
    }

    #endregion

    #region Start/Stop

    /// <summary>
    /// Starts streaming.
    /// </summary>
    /// <returns>True if streaming started successfully.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no service or video encoder is configured.</exception>
    public new bool Start()
    {
        if (_service == null)
            throw new InvalidOperationException("No streaming service configured. Call WithService(), ToTwitch(), ToYouTube(), ToFacebook(), or ToCustomServer() first.");

        if (_videoEncoder == null)
            throw new InvalidOperationException("No video encoder configured. Call WithVideoEncoder() or WithDefaultEncoders() first.");

        if (!_service.CanConnect)
            throw new InvalidOperationException("Service is not properly configured. Ensure server URL and stream key are set.");

        return base.Start();
    }

    /// <summary>
    /// Stops streaming.
    /// </summary>
    /// <param name="waitForCompletion">If true, waits for the stream to fully stop.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
    /// <returns>True if streaming stopped successfully, false if timed out.</returns>
    public new bool Stop(bool waitForCompletion = true, int timeoutMs = 30000)
    {
        return base.Stop(waitForCompletion, timeoutMs);
    }

    /// <summary>
    /// Force stops streaming immediately.
    /// This may result in stream data loss.
    /// </summary>
    public new void ForceStop() => base.ForceStop();

    #endregion

    #region Events

    /// <summary>
    /// Connects a callback for when streaming starts successfully.
    /// </summary>
    /// <param name="callback">The callback to invoke.</param>
    /// <returns>A disposable signal connection.</returns>
    public SignalConnection OnStarted(Action callback)
    {
        return ConnectSignal(OutputSignal.Start, _ => callback());
    }

    /// <summary>
    /// Connects a callback for when streaming stops.
    /// </summary>
    /// <param name="callback">The callback to invoke with the stop code.</param>
    /// <returns>A disposable signal connection.</returns>
    public SignalConnection OnStopped(Action<int> callback)
    {
        return ConnectSignal(OutputSignal.Stop, calldata =>
        {
            var code = (int)Calldata.GetInt(calldata, "code");
            callback(code);
        });
    }

    /// <summary>
    /// Connects a callback for when the output is attempting to reconnect.
    /// </summary>
    /// <param name="callback">The callback to invoke.</param>
    /// <returns>A disposable signal connection.</returns>
    public SignalConnection OnReconnecting(Action callback)
    {
        return ConnectSignal(OutputSignal.Reconnect, _ => callback());
    }

    /// <summary>
    /// Connects a callback for when reconnection succeeds.
    /// </summary>
    /// <param name="callback">The callback to invoke.</param>
    /// <returns>A disposable signal connection.</returns>
    public SignalConnection OnReconnected(Action callback)
    {
        return ConnectSignal(OutputSignal.ReconnectSuccess, _ => callback());
    }

    #endregion

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_encodersOwned)
            {
                _videoEncoder?.Dispose();
                _audioEncoder?.Dispose();
            }

            if (_serviceOwned)
            {
                _service?.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override string ToString() => $"StreamingOutput: {Name} -> {Url ?? "(not configured)"}";
}
