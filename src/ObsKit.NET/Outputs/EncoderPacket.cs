using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Outputs;

/// <summary>
/// An encoded packet passing through an output (see <see cref="Output.SubscribePackets"/>).
/// The data is only valid for the duration of the callback; copy it out if you need it later.
/// </summary>
public unsafe readonly ref struct EncoderPacket
{
    private readonly EncoderPacketNative* _native;

    internal EncoderPacket(EncoderPacketNative* native)
    {
        _native = native;
    }

    /// <summary>The encoded bytes (for video: Annex B / AVCC as produced by the encoder).</summary>
    public ReadOnlySpan<byte> Data => _native->Data == nint.Zero
        ? default
        : new ReadOnlySpan<byte>((void*)_native->Data, checked((int)_native->Size));

    /// <summary>Packet size in bytes.</summary>
    public long Size => (long)_native->Size;

    /// <summary>Presentation timestamp in <see cref="TimebaseNum"/>/<see cref="TimebaseDen"/> units.</summary>
    public long Pts => _native->Pts;

    /// <summary>Decode timestamp in <see cref="TimebaseNum"/>/<see cref="TimebaseDen"/> units.</summary>
    public long Dts => _native->Dts;

    /// <summary>Timebase numerator (e.g. 1 for audio, fps denominator for video).</summary>
    public int TimebaseNum => _native->TimebaseNum;

    /// <summary>Timebase denominator (e.g. the sample rate for audio, fps numerator for video).</summary>
    public int TimebaseDen => _native->TimebaseDen;

    /// <summary>Whether this is an audio or video packet.</summary>
    public ObsEncoderType Type => _native->Type;

    /// <summary>Whether the packet is a keyframe (video only).</summary>
    public bool IsKeyframe => _native->Keyframe != 0;

    /// <summary>Decode timestamp in microseconds.</summary>
    public long DtsUsec => _native->DtsUsec;

    /// <summary>Decode timestamp in microseconds on the system clock.</summary>
    public long SysDtsUsec => _native->SysDtsUsec;

    /// <summary>Encoder-assigned packet priority (video only).</summary>
    public int Priority => _native->Priority;

    /// <summary>Minimum priority the next packet must have to resume after this one is dropped.</summary>
    public int DropPriority => _native->DropPriority;

    /// <summary>The output track index the packet belongs to (audio track or video track).</summary>
    public int TrackIndex => (int)_native->TrackIdx;

    /// <summary>Native handle of the encoder that produced the packet (borrowed).</summary>
    public nint EncoderHandle => _native->Encoder;
}

/// <summary>
/// Latency timestamps for a video packet (all in nanoseconds on OBS's monotonic clock),
/// captured as the frame moved from render to interleaving.
/// </summary>
public readonly struct EncoderPacketTiming
{
    internal EncoderPacketTiming(in EncoderPacketTimeNative native)
    {
        Pts = native.Pts;
        CompositionTime = native.Cts;
        EncodeRequestTime = native.Fer;
        EncodeCompleteTime = native.Ferc;
        InterleaveRequestTime = native.Pir;
    }

    /// <summary>PTS used to match the raw frame with its encoded packet.</summary>
    public long Pts { get; }

    /// <summary>When the frame was rendered/captured.</summary>
    public ulong CompositionTime { get; }

    /// <summary>When the frame was handed to the encoder.</summary>
    public ulong EncodeRequestTime { get; }

    /// <summary>When the encoder returned the packet.</summary>
    public ulong EncodeCompleteTime { get; }

    /// <summary>When the packet was interleaved into the output stream.</summary>
    public ulong InterleaveRequestTime { get; }

    /// <summary>Time spent inside the encoder (request to completion).</summary>
    public TimeSpan EncodeLatency => FromNs(EncodeCompleteTime - EncodeRequestTime);

    /// <summary>Total time from render to interleaving.</summary>
    public TimeSpan TotalLatency => FromNs(InterleaveRequestTime - CompositionTime);

    private static TimeSpan FromNs(ulong ns) => TimeSpan.FromTicks((long)(ns / 100));
}

/// <summary>
/// Callback for <see cref="Output.SubscribePackets"/>.
/// </summary>
/// <param name="packet">The packet. Only valid during the callback.</param>
/// <param name="timing">Render-to-interleave timestamps, available for video packets when the
/// encoder reports them; null otherwise.</param>
public delegate void OutputPacketCallback(in EncoderPacket packet, EncoderPacketTiming? timing);
