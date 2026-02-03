using ObsKit.NET.Core;
using ObsKit.NET.Encoders;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Signals;

namespace ObsKit.NET.Outputs;

/// <summary>
/// Represents an OBS output (obs_output_t).
/// Outputs handle the encoding and delivery of video and audio data.
/// </summary>
public class Output : ObsObject
{
    private VideoEncoder? _videoEncoder;
    private readonly Dictionary<int, AudioEncoder> _audioEncoders = new();
    /// <summary>
    /// Creates a new output with the specified type ID and name.
    /// </summary>
    /// <param name="typeId">The output type identifier (e.g., "ffmpeg_muxer", "rtmp_output").</param>
    /// <param name="name">The display name for this output.</param>
    /// <param name="settings">Optional settings for the output.</param>
    /// <param name="hotkeyData">Optional hotkey data.</param>
    public Output(string typeId, string name, Settings? settings = null, Settings? hotkeyData = null)
        : base(CreateOutput(typeId, name, settings, hotkeyData))
    {
        TypeId = typeId;
    }

    /// <summary>
    /// Internal constructor for wrapping an existing handle.
    /// </summary>
    internal Output(ObsOutputHandle handle, string? typeId = null, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
        TypeId = typeId ?? ObsOutput.obs_output_get_id(handle);
    }

    private static nint CreateOutput(string typeId, string name, Settings? settings, Settings? hotkeyData)
    {
        ThrowIfNotInitialized();
        var handle = ObsOutput.obs_output_create(
            typeId,
            name,
            settings?.Handle ?? default,
            hotkeyData?.Handle ?? default);

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create output of type '{typeId}'");

        return handle;
    }

    /// <summary>
    /// Gets the internal handle for P/Invoke calls.
    /// </summary>
    internal new ObsOutputHandle Handle => (ObsOutputHandle)base.Handle;

    /// <summary>
    /// Gets the output type identifier.
    /// </summary>
    public string? TypeId { get; }

    /// <summary>
    /// Gets the output name.
    /// </summary>
    public string? Name => ObsOutput.obs_output_get_name(Handle);

    /// <summary>
    /// Gets the display name for this output type.
    /// </summary>
    public string? DisplayName => TypeId != null ? ObsOutput.obs_output_get_display_name(TypeId) : null;

    /// <summary>
    /// Gets the output flags.
    /// </summary>
    public uint Flags => ObsOutput.obs_output_get_flags(Handle);

    /// <summary>
    /// Gets whether the output is active.
    /// </summary>
    public bool IsActive => ObsOutput.obs_output_active(Handle);

    /// <summary>
    /// Gets whether the output is reconnecting.
    /// </summary>
    public bool IsReconnecting => ObsOutput.obs_output_reconnecting(Handle);

    /// <summary>
    /// Gets whether the output can be paused.
    /// </summary>
    public bool CanPause => ObsOutput.obs_output_can_pause(Handle);

    /// <summary>
    /// Gets whether the output is paused.
    /// </summary>
    public bool IsPaused => ObsOutput.obs_output_paused(Handle);

    /// <summary>
    /// Gets the output width.
    /// </summary>
    public uint Width => ObsOutput.obs_output_get_width(Handle);

    /// <summary>
    /// Gets the output height.
    /// </summary>
    public uint Height => ObsOutput.obs_output_get_height(Handle);

    /// <summary>
    /// Gets the last error message.
    /// </summary>
    public string? LastError => ObsOutput.obs_output_get_last_error(Handle);

    #region Start/Stop

    /// <summary>
    /// Starts the output.
    /// </summary>
    /// <returns>True if the output started successfully.</returns>
    public bool Start()
    {
        return ObsOutput.obs_output_start(Handle);
    }

    /// <summary>
    /// Stops the output.
    /// If Obs.AutoDispose is true, the output is disposed and encoders are detached
    /// (encoders auto-dispose when their ref count reaches 0).
    /// </summary>
    /// <param name="waitForCompletion">If true, waits for the output to fully stop.</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds (default 30000).</param>
    /// <returns>True if the output stopped successfully, false if timed out.</returns>
    public bool Stop(bool waitForCompletion = true, int timeoutMs = 30000)
    {
        if (!IsActive && !Obs.AutoDispose) return true;

        if (IsActive)
        {
            ObsOutput.obs_output_stop(Handle);

            if (waitForCompletion)
            {
                var startTime = Environment.TickCount64;
                while (IsActive && (Environment.TickCount64 - startTime) < timeoutMs)
                {
                    Thread.Sleep(50);
                }
            }
        }

        var stopped = !IsActive;

        // Auto-dispose: detach encoders and dispose output
        if (Obs.AutoDispose)
        {
            DetachEncoders();
            Obs.OnOutputStopped(this);
            Dispose();
        }

        return stopped;
    }

    /// <summary>
    /// Detaches all encoders from this output, decrementing their ref counts.
    /// </summary>
    private void DetachEncoders()
    {
        _videoEncoder?.Detach();
        _videoEncoder = null;

        foreach (var encoder in _audioEncoders.Values)
        {
            encoder.Detach();
        }
        _audioEncoders.Clear();
    }

    /// <summary>
    /// Force stops the output immediately.
    /// </summary>
    public void ForceStop()
    {
        ObsOutput.obs_output_force_stop(Handle);
    }

    /// <summary>
    /// Pauses or resumes the output.
    /// </summary>
    /// <param name="pause">Whether to pause (true) or resume (false).</param>
    /// <returns>True if the operation succeeded.</returns>
    public bool Pause(bool pause)
    {
        return ObsOutput.obs_output_pause(Handle, pause);
    }

    #endregion

    #region Settings

    /// <summary>
    /// Gets the current settings for this output.
    /// </summary>
    public Settings GetSettings()
    {
        var handle = ObsOutput.obs_output_get_settings(Handle);
        return new Settings(handle, ownsHandle: true);
    }

    /// <summary>
    /// Updates the output with new settings.
    /// </summary>
    /// <param name="settings">The settings to apply.</param>
    public void Update(Settings settings)
    {
        ObsOutput.obs_output_update(Handle, settings.Handle);
    }

    /// <summary>
    /// Updates the output with settings configured via a builder action.
    /// </summary>
    /// <param name="configure">Action to configure the settings.</param>
    public void Update(Action<Settings> configure)
    {
        using var settings = new Settings();
        configure(settings);
        Update(settings);
    }

    #endregion

    #region Encoders

    /// <summary>
    /// Sets the video encoder for this output.
    /// The encoder's ref count is incremented for automatic lifecycle management.
    /// </summary>
    /// <param name="encoder">The video encoder.</param>
    public void SetVideoEncoder(VideoEncoder encoder)
    {
        // Detach previous encoder if any
        _videoEncoder?.Detach();

        _videoEncoder = encoder;
        encoder.Attach();

        ObsOutput.obs_output_set_video_encoder(Handle, encoder.Handle);
    }

    /// <summary>
    /// Sets the audio encoder for this output.
    /// The encoder's ref count is incremented for automatic lifecycle management.
    /// If a different encoder was previously set for this track, it will be detached.
    /// </summary>
    /// <param name="encoder">The audio encoder.</param>
    /// <param name="track">The audio track index (default 0).</param>
    public void SetAudioEncoder(AudioEncoder encoder, int track = 0)
    {
        // Detach previous encoder for this track if any
        if (_audioEncoders.TryGetValue(track, out var previous) && previous != encoder)
        {
            previous.Detach();
        }

        _audioEncoders[track] = encoder;
        encoder.Attach();

        ObsOutput.obs_output_set_audio_encoder(Handle, encoder.Handle, (nuint)track);
    }

    /// <summary>
    /// Sets the mixers for the output.
    /// </summary>
    /// <param name="mixersMask">Bitmask of mixers to use.</param>
    public void SetMixers(uint mixersMask)
    {
        ObsOutput.obs_output_set_mixers(Handle, mixersMask);
    }

    /// <summary>
    /// Gets the mixers mask for the output.
    /// </summary>
    public uint GetMixers()
    {
        return (uint)ObsOutput.obs_output_get_mixers(Handle);
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets the total number of frames output.
    /// </summary>
    public uint TotalFrames => ObsOutput.obs_output_get_total_frames(Handle);

    /// <summary>
    /// Gets the total bytes output.
    /// </summary>
    public ulong TotalBytes => ObsOutput.obs_output_get_total_bytes(Handle);

    /// <summary>
    /// Gets the number of frames dropped.
    /// </summary>
    public int FramesDropped => ObsOutput.obs_output_get_frames_dropped(Handle);

    /// <summary>
    /// Gets the congestion value (0.0 to 1.0).
    /// </summary>
    public float Congestion => ObsOutput.obs_output_get_congestion(Handle);

    /// <summary>
    /// Gets the connection time in milliseconds.
    /// </summary>
    public int ConnectTimeMs => ObsOutput.obs_output_get_connect_time_ms(Handle);

    #endregion

    #region Delay

    /// <summary>
    /// Sets the output delay.
    /// </summary>
    /// <param name="delaySec">Delay in seconds.</param>
    /// <param name="flags">Delay flags.</param>
    public void SetDelay(uint delaySec, uint flags = 0)
    {
        ObsOutput.obs_output_set_delay(Handle, delaySec, flags);
    }

    /// <summary>
    /// Gets the configured output delay in seconds.
    /// </summary>
    public uint Delay => ObsOutput.obs_output_get_delay(Handle);

    /// <summary>
    /// Gets the active output delay in seconds.
    /// </summary>
    public uint ActiveDelay => ObsOutput.obs_output_get_active_delay(Handle);

    #endregion

    #region Signal Handler

    /// <summary>
    /// Connects a callback to an output signal using a strongly-typed enum.
    /// </summary>
    /// <param name="signal">The signal to connect to.</param>
    /// <param name="callback">The callback to invoke when the signal is emitted.</param>
    /// <returns>A SignalConnection that can be disposed to disconnect the callback.</returns>
    public SignalConnection ConnectSignal(OutputSignal signal, SignalCallback callback)
    {
        var signalHandler = ObsOutput.obs_output_get_signal_handler(Handle);
        return new SignalConnection(signalHandler, signal.ToSignalName(), callback);
    }

    /// <summary>
    /// Connects a callback to an output signal using a string name.
    /// Use this overload for custom or plugin-specific signals not in the OutputSignal enum.
    /// </summary>
    /// <param name="signal">The signal name to connect to.</param>
    /// <param name="callback">The callback to invoke when the signal is emitted.</param>
    /// <returns>A SignalConnection that can be disposed to disconnect the callback.</returns>
    public SignalConnection ConnectSignal(string signal, SignalCallback callback)
    {
        var signalHandler = ObsOutput.obs_output_get_signal_handler(Handle);
        return new SignalConnection(signalHandler, signal, callback);
    }

    #endregion

    protected override void ReleaseHandle(nint handle)
    {
        DetachEncoders();
        ObsOutput.obs_output_release((ObsOutputHandle)handle);
    }

    public override string ToString() => $"Output[{TypeId}]: {Name}";
}
