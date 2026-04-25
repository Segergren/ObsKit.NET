using System.Runtime.InteropServices;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Video;

/// <summary>
/// Callback signature for raw video frames.
/// </summary>
/// <param name="frame">The frame data. Pointers inside are only valid for this call.</param>
public delegate void RawVideoFrameCallback(in RawVideoFrame frame);

/// <summary>
/// An active subscription to OBS's raw video output.
/// Dispose to stop receiving frames.
/// </summary>
public sealed class RawVideoSubscription : IDisposable
{
    private readonly RawVideoFrameCallback _userCallback;
    private readonly ObsCore.RawVideoCallbackNative _nativeCallback;
    private readonly VideoFormat _format;
    private readonly uint _width;
    private readonly uint _height;
    private nint _conversionPtr;
    private bool _disposed;

    internal RawVideoSubscription(
        VideoScaleInfo conversion,
        uint frameRateDivisor,
        RawVideoFrameCallback callback)
    {
        _userCallback = callback;
        _format = conversion.Format;
        _width = conversion.Width;
        _height = conversion.Height;
        _nativeCallback = NativeCallback;

        // OBS reads conversion synchronously inside obs_add_raw_video_callback (it copies into
        // its own connection struct), so we can free the unmanaged buffer immediately after.
        _conversionPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VideoScaleInfo>());
        try
        {
            Marshal.StructureToPtr(conversion, _conversionPtr, fDeleteOld: false);
            ObsCore.obs_add_raw_video_callback2(_conversionPtr, frameRateDivisor, _nativeCallback, nint.Zero);
        }
        finally
        {
            Marshal.FreeHGlobal(_conversionPtr);
            _conversionPtr = nint.Zero;
        }
    }

    private unsafe void NativeCallback(nint param, nint framePtr)
    {
        if (_disposed || framePtr == nint.Zero)
            return;

        try
        {
            var native = (VideoDataNative*)framePtr;
            var frame = new RawVideoFrame(
                data: &native->Data0,
                linesize: &native->Linesize0,
                format: _format,
                width: _width,
                height: _height,
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
        if (_disposed) return;
        _disposed = true;
        ObsCore.obs_remove_raw_video_callback(_nativeCallback, nint.Zero);
        GC.KeepAlive(_nativeCallback);
    }
}
