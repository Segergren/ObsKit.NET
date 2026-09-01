using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Outputs;

/// <summary>
/// An active packet tap on an output (see <see cref="Output.SubscribePackets"/>).
/// Dispose to stop receiving packets.
/// </summary>
public sealed class OutputPacketSubscription : IDisposable
{
    private readonly OutputPacketCallback _userCallback;
    private readonly ObsOutput.PacketCallbackNative _nativeCallback;
    private ObsOutputHandle _output;
    private bool _disposed;

    internal OutputPacketSubscription(ObsOutputHandle output, OutputPacketCallback callback)
    {
        _userCallback = callback;
        _nativeCallback = NativeCallback;

        // Hold our own reference so removing the callback later never touches a freed output.
        _output = ObsOutput.obs_output_get_ref(output);
        if (_output.IsNull)
            throw new ObjectDisposedException(nameof(Output), "The output is being destroyed.");

        ObsOutput.obs_output_add_packet_callback(_output, _nativeCallback, nint.Zero);
    }

    private unsafe void NativeCallback(ObsOutputHandle output, nint pkt, nint pktTime, nint param)
    {
        if (_disposed || pkt == nint.Zero)
            return;

        try
        {
            var packet = new EncoderPacket((EncoderPacketNative*)pkt);
            EncoderPacketTiming? timing = pktTime != nint.Zero
                ? new EncoderPacketTiming(in *(EncoderPacketTimeNative*)pktTime)
                : null;
            _userCallback(in packet, timing);
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
        Release();
        GC.SuppressFinalize(this);
    }

    ~OutputPacketSubscription()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        // After obs_shutdown the output (and our reference) is already gone.
        if (Obs.IsInitialized && !_output.IsNull)
        {
            ObsOutput.obs_output_remove_packet_callback(_output, _nativeCallback, nint.Zero);
            ObsOutput.obs_output_release(_output);
        }
        _output = default;
        GC.KeepAlive(_nativeCallback);
    }
}
