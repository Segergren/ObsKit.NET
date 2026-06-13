using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Audio;

/// <summary>
/// Callback signature for per-source audio.
/// </summary>
/// <param name="frame">The audio data. Pointers inside are only valid for this call.</param>
/// <param name="muted">Whether the source is currently muted (audio is still delivered).</param>
public delegate void SourceAudioCallback(in RawAudioFrame frame, bool muted);

/// <summary>
/// An active subscription to a single source's audio, delivered before mixing
/// (e.g. the microphone alone, for voice activity detection or custom processing).
/// Audio arrives as planar 32-bit float at the OBS output sample rate.
/// Dispose to stop receiving audio.
/// </summary>
public sealed class SourceAudioSubscription : IDisposable
{
    private readonly SourceAudioCallback _userCallback;
    private readonly ObsSource.SourceAudioCaptureCallback _nativeCallback;
    private readonly ObsSourceHandle _sourceHandle;
    private readonly uint _sampleRate;
    private readonly SpeakerLayout _speakers;
    private readonly int _channels;
    private bool _disposed;

    internal SourceAudioSubscription(Source source, SourceAudioCallback callback)
    {
        _userCallback = callback;
        _nativeCallback = NativeCallback;

        var audio = ObsCore.obs_get_audio();
        _sampleRate = audio.IsNull ? 0 : ObsCore.audio_output_get_sample_rate(audio);
        _channels = audio.IsNull ? 2 : (int)ObsCore.audio_output_get_channels(audio);
        _speakers = _channels switch
        {
            1 => SpeakerLayout.Mono,
            2 => SpeakerLayout.Stereo,
            3 => SpeakerLayout.TwoPointOne,
            4 => SpeakerLayout.FourPointZero,
            5 => SpeakerLayout.FourPointOne,
            6 => SpeakerLayout.FivePointOne,
            8 => SpeakerLayout.SevenPointOne,
            _ => SpeakerLayout.Stereo
        };

        // Keep the source alive while subscribed.
        _sourceHandle = ObsSource.obs_source_get_ref(source.Handle);
        if (_sourceHandle.IsNull)
            throw new InvalidOperationException("The source is no longer valid.");

        ObsSource.obs_source_add_audio_capture_callback(_sourceHandle, _nativeCallback, nint.Zero);
    }

    /// <summary>The sample rate audio is delivered at, in Hz.</summary>
    public uint SampleRate => _sampleRate;

    /// <summary>The speaker layout audio is delivered in.</summary>
    public SpeakerLayout Speakers => _speakers;

    private unsafe void NativeCallback(nint param, ObsSourceHandle source, nint audioData, byte muted)
    {
        if (_disposed || audioData == nint.Zero)
            return;

        try
        {
            var native = (AudioDataNative*)audioData;
            var frame = new RawAudioFrame(
                data: &native->Data0,
                format: AudioFormat.FloatPlanar,
                sampleRate: _sampleRate,
                speakers: _speakers,
                channels: _channels,
                frames: native->Frames,
                timestamp: native->Timestamp);
            _userCallback(in frame, muted != 0);
        }
        catch
        {
            // Never let exceptions cross the native boundary.
        }
    }

    /// <summary>
    /// Stops the subscription and releases the source reference. Safe to call multiple times.
    /// </summary>
    public void Dispose()
    {
        ReleaseSubscription();
        GC.SuppressFinalize(this);
    }

    ~SourceAudioSubscription()
    {
        ReleaseSubscription();
    }

    private void ReleaseSubscription()
    {
        if (_disposed)
            return;

        _disposed = true;
        ObsSource.obs_source_remove_audio_capture_callback(_sourceHandle, _nativeCallback, nint.Zero);
        ObsSource.obs_source_release(_sourceHandle);
        GC.KeepAlive(_nativeCallback);
    }
}
