using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Audio;

/// <summary>
/// Callback signature for raw audio blocks.
/// </summary>
/// <param name="frame">The audio data. Pointers inside are only valid for this call.</param>
public delegate void RawAudioFrameCallback(in RawAudioFrame frame);

/// <summary>
/// An active subscription to a track of OBS's mixed audio output.
/// Dispose to stop receiving audio.
/// </summary>
public sealed class RawAudioSubscription : IDisposable
{
    private readonly RawAudioFrameCallback _userCallback;
    private readonly ObsCore.RawAudioCallbackNative _nativeCallback;
    private readonly AudioHandle _audio;
    private readonly nuint _mixIdx;
    private readonly AudioFormat _format;
    private readonly uint _sampleRate;
    private readonly SpeakerLayout _speakers;
    private readonly int _channels;
    private bool _disposed;

    internal RawAudioSubscription(int track, AudioFormat format, uint sampleRate, SpeakerLayout speakers,
        RawAudioFrameCallback callback)
    {
        AudioTracks.ValidateTrack(track);
        _userCallback = callback;
        _nativeCallback = NativeCallback;
        _mixIdx = (nuint)(track - 1);
        _audio = ObsCore.obs_get_audio();

        if (_audio.IsNull)
            throw new InvalidOperationException("OBS audio is not initialized.");

        _format = format == AudioFormat.Unknown ? AudioFormat.FloatPlanar : format;
        _sampleRate = sampleRate == 0 ? ObsCore.audio_output_get_sample_rate(_audio) : sampleRate;
        _speakers = speakers == SpeakerLayout.Unknown
            ? ChannelsToLayout((int)ObsCore.audio_output_get_channels(_audio))
            : speakers;
        _channels = LayoutToChannels(_speakers);

        var conversion = new AudioConvertInfo
        {
            SamplesPerSec = _sampleRate,
            Format = _format,
            Speakers = _speakers
        };

        if (!ObsCore.audio_output_connect(_audio, _mixIdx, ref conversion, _nativeCallback, nint.Zero))
            throw new InvalidOperationException("Failed to connect the raw audio callback.");
    }

    /// <summary>The sample format audio is delivered in.</summary>
    public AudioFormat Format => _format;

    /// <summary>The sample rate audio is delivered at, in Hz.</summary>
    public uint SampleRate => _sampleRate;

    /// <summary>The speaker layout audio is delivered in.</summary>
    public SpeakerLayout Speakers => _speakers;

    private static SpeakerLayout ChannelsToLayout(int channels) => channels switch
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

    private static int LayoutToChannels(SpeakerLayout layout) => layout switch
    {
        SpeakerLayout.Mono => 1,
        SpeakerLayout.Stereo => 2,
        SpeakerLayout.TwoPointOne => 3,
        SpeakerLayout.FourPointZero => 4,
        SpeakerLayout.FourPointOne => 5,
        SpeakerLayout.FivePointOne => 6,
        SpeakerLayout.SevenPointOne => 8,
        _ => 2
    };

    private unsafe void NativeCallback(nint param, nuint mixIdx, nint audioData)
    {
        if (_disposed || audioData == nint.Zero)
            return;

        try
        {
            var native = (AudioDataNative*)audioData;
            var frame = new RawAudioFrame(
                data: &native->Data0,
                format: _format,
                sampleRate: _sampleRate,
                speakers: _speakers,
                channels: _channels,
                frames: native->Frames,
                timestamp: native->Timestamp);
            _userCallback(in frame);
        }
        catch
        {
            // Never let exceptions cross the native boundary.
        }
    }

    /// <summary>
    /// Stops the subscription. Safe to call multiple times.
    /// </summary>
    public void Dispose()
    {
        ReleaseSubscription();
        GC.SuppressFinalize(this);
    }

    ~RawAudioSubscription()
    {
        ReleaseSubscription();
    }

    private void ReleaseSubscription()
    {
        if (_disposed)
            return;

        _disposed = true;
        ObsCore.audio_output_disconnect(_audio, _mixIdx, _nativeCallback, nint.Zero);
        GC.KeepAlive(_nativeCallback);
    }
}
