using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Audio;

/// <summary>
/// A block of raw audio delivered to a <see cref="RawAudioSubscription"/> callback.
/// The pointers are only valid for the duration of the callback invocation; do not store them.
/// Copy the data out (e.g. via <see cref="GetPlane"/>) if you need to use it later.
/// </summary>
public unsafe readonly ref struct RawAudioFrame
{
    private readonly nint* _data;

    /// <summary>Sample format (matches what was requested in the subscription).</summary>
    public AudioFormat Format { get; }

    /// <summary>Sample rate in Hz.</summary>
    public uint SampleRate { get; }

    /// <summary>Speaker layout.</summary>
    public SpeakerLayout Speakers { get; }

    /// <summary>Number of channels.</summary>
    public int Channels { get; }

    /// <summary>Number of audio frames (samples per channel) in this block.</summary>
    public uint Frames { get; }

    /// <summary>Timestamp in nanoseconds (OBS monotonic clock).</summary>
    public ulong Timestamp { get; }

    internal RawAudioFrame(nint* data, AudioFormat format, uint sampleRate, SpeakerLayout speakers,
        int channels, uint frames, ulong timestamp)
    {
        _data = data;
        Format = format;
        SampleRate = sampleRate;
        Speakers = speakers;
        Channels = channels;
        Frames = frames;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets whether the format is planar (one plane per channel).
    /// Interleaved formats put all channels in plane 0.
    /// </summary>
    public bool IsPlanar => Format >= AudioFormat.U8BitPlanar;

    /// <summary>Number of valid planes: <see cref="Channels"/> for planar formats, 1 for interleaved.</summary>
    public int PlaneCount => IsPlanar ? Channels : 1;

    /// <summary>Bytes per sample for the format.</summary>
    public int BytesPerSample => Format switch
    {
        AudioFormat.U8Bit or AudioFormat.U8BitPlanar => 1,
        AudioFormat.Bit16 or AudioFormat.Bit16Planar => 2,
        AudioFormat.Bit32 or AudioFormat.Bit32Planar => 4,
        AudioFormat.Float or AudioFormat.FloatPlanar => 4,
        _ => 0
    };

    /// <summary>
    /// Raw pointer to the start of the given plane (valid indices are 0 to <see cref="PlaneCount"/> - 1).
    /// </summary>
    /// <remarks>
    /// Only the first <see cref="PlaneCount"/> planes are meaningful. libobs does not zero the
    /// remaining slots of its native plane array, so indices &gt;= <see cref="PlaneCount"/> would
    /// alias uninitialized (garbage) pointers; they are rejected rather than returned.
    /// </remarks>
    /// <param name="planeIndex">The plane index (0 to <see cref="PlaneCount"/> - 1).</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="planeIndex"/> is outside [0, <see cref="PlaneCount"/>).</exception>
    public nint GetPlanePointer(int planeIndex)
    {
        if ((uint)planeIndex >= (uint)PlaneCount)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        return _data[planeIndex];
    }

    /// <summary>
    /// Returns the raw bytes of a plane. For planar formats each plane holds one channel
    /// (<see cref="Frames"/> samples); for interleaved formats plane 0 holds all channels.
    /// </summary>
    /// <param name="planeIndex">The plane index (0 to <see cref="PlaneCount"/> - 1).</param>
    public ReadOnlySpan<byte> GetPlane(int planeIndex)
    {
        if ((uint)planeIndex >= (uint)PlaneCount)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        var ptr = _data[planeIndex];
        if (ptr == nint.Zero)
            return default;

        var samples = IsPlanar ? Frames : Frames * (uint)Channels;
        return new ReadOnlySpan<byte>((void*)ptr, checked((int)(samples * (uint)BytesPerSample)));
    }

    /// <summary>
    /// Returns a plane as 32-bit float samples. Only valid for
    /// <see cref="AudioFormat.Float"/> and <see cref="AudioFormat.FloatPlanar"/>.
    /// </summary>
    /// <param name="planeIndex">The plane index (the channel for planar float).</param>
    public ReadOnlySpan<float> GetFloatPlane(int planeIndex)
    {
        if (Format != AudioFormat.Float && Format != AudioFormat.FloatPlanar)
            throw new InvalidOperationException($"Samples are {Format}, not float.");

        if ((uint)planeIndex >= (uint)PlaneCount)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        var ptr = _data[planeIndex];
        if (ptr == nint.Zero)
            return default;

        var samples = IsPlanar ? Frames : Frames * (uint)Channels;
        return new ReadOnlySpan<float>((void*)ptr, checked((int)samples));
    }
}
