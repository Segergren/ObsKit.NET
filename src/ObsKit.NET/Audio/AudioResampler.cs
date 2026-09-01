using System.Runtime.InteropServices;
using ObsKit.NET.Native;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Audio;

/// <summary>
/// Converts audio between sample formats, sample rates and speaker layouts
/// (libobs <c>audio_resampler_t</c>, backed by swresample). Useful for feeding raw audio from
/// <see cref="Obs.SubscribeRawAudio"/> or <see cref="Sources.Source.SubscribeAudio"/> into a
/// component that needs, say, 16 kHz mono 16-bit PCM. Instances are not thread-safe.
/// </summary>
public sealed class AudioResampler : IDisposable
{
    private const int MaxPlanes = AudioDataNative.MaxAvPlanes;

    private nint _resampler;
    private readonly nint _scratch;
    private bool _disposed;

    /// <summary>
    /// Creates a resampler.
    /// </summary>
    /// <param name="source">Format of the input audio.</param>
    /// <param name="destination">Format to produce.</param>
    /// <exception cref="NotSupportedException">The conversion is not supported.</exception>
    public AudioResampler(ResampleInfo source, ResampleInfo destination)
    {
        if (source.SampleRate == 0 || destination.SampleRate == 0)
            throw new ArgumentException("Sample rates must be non-zero.");

        LibraryLoader.Initialize();
        Source = source;
        Destination = destination;

        _resampler = MediaIo.audio_resampler_create(in destination, in source);
        if (_resampler == nint.Zero)
            throw new NotSupportedException($"Cannot resample {source} to {destination}.");

        // Two arrays of MaxPlanes pointers: output (filled by libobs) and input.
        _scratch = Marshal.AllocHGlobal(MaxPlanes * nint.Size * 2);
    }

    /// <summary>The input configuration.</summary>
    public ResampleInfo Source { get; }

    /// <summary>The output configuration.</summary>
    public ResampleInfo Destination { get; }

    /// <summary>
    /// Resamples a block of audio given raw input plane pointers. The output planes point into
    /// the resampler's internal buffer and stay valid until the next call or dispose.
    /// </summary>
    /// <param name="inputPlanes">Input plane pointers (one per channel for planar formats, one for interleaved).</param>
    /// <param name="inputFrames">Number of input frames (samples per channel).</param>
    /// <param name="outputPlanes">Receives the output plane pointers (up to 8).</param>
    /// <param name="outputFrames">Number of output frames produced.</param>
    /// <param name="timestampOffsetNs">Latency introduced by the resampler, to subtract from the input timestamp.</param>
    public unsafe bool Resample(ReadOnlySpan<nint> inputPlanes, uint inputFrames, Span<nint> outputPlanes, out uint outputFrames, out ulong timestampOffsetNs)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (inputPlanes.Length > MaxPlanes || outputPlanes.Length > MaxPlanes)
            throw new ArgumentException("At most 8 planes are supported.");

        var outData = (nint*)_scratch;
        var inData = outData + MaxPlanes;
        new Span<nint>(outData, MaxPlanes * 2).Clear();
        inputPlanes.CopyTo(new Span<nint>(inData, MaxPlanes));

        var ok = MediaIo.audio_resampler_resample(_resampler, (nint)outData, out outputFrames, out timestampOffsetNs, (nint)inData, inputFrames);
        new ReadOnlySpan<nint>(outData, outputPlanes.Length).CopyTo(outputPlanes);
        return ok;
    }

    /// <summary>
    /// Resamples a raw callback frame. The returned frame points into the resampler's internal
    /// buffer and is only valid until the next call or dispose; copy it out if needed.
    /// </summary>
    /// <param name="frame">The input frame (must match <see cref="Source"/>).</param>
    /// <param name="output">The converted audio, with the timestamp adjusted for resampler latency.</param>
    public unsafe bool Resample(in RawAudioFrame frame, out RawAudioFrame output)
    {
        Span<nint> inPlanes = stackalloc nint[MaxPlanes];
        for (var i = 0; i < frame.PlaneCount; i++)
            inPlanes[i] = frame.GetPlanePointer(i);

        Span<nint> outPlanes = stackalloc nint[MaxPlanes];
        if (!Resample(inPlanes, frame.Frames, outPlanes, out var outFrames, out var offset))
        {
            output = default;
            return false;
        }

        var channels = ChannelCount(Destination.Speakers);
        var ts = frame.Timestamp >= offset ? frame.Timestamp - offset : 0;
        output = new RawAudioFrame((nint*)_scratch, Destination.Format, Destination.SampleRate, Destination.Speakers, channels, outFrames, ts);
        return true;
    }

    private static int ChannelCount(SpeakerLayout layout) => layout switch
    {
        SpeakerLayout.Mono => 1,
        SpeakerLayout.Stereo => 2,
        SpeakerLayout.TwoPointOne => 3,
        SpeakerLayout.FourPointZero => 4,
        SpeakerLayout.FourPointOne => 5,
        SpeakerLayout.FivePointOne => 6,
        SpeakerLayout.SevenPointOne => 8,
        _ => 0,
    };

    /// <summary>Releases the resampler.</summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~AudioResampler()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        if (_resampler != nint.Zero)
            MediaIo.audio_resampler_destroy(_resampler);
        _resampler = nint.Zero;
        if (_scratch != nint.Zero)
            Marshal.FreeHGlobal(_scratch);
    }
}
