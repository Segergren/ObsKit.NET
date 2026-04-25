using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Video;

/// <summary>
/// A single raw video frame delivered to a <see cref="RawVideoSubscription"/> callback.
/// The pointers are only valid for the duration of the callback invocation; do not store them.
/// Copy the data out (e.g. via <see cref="GetPlane"/>) if you need to use it later.
/// </summary>
public unsafe readonly ref struct RawVideoFrame
{
    private readonly nint* _data;
    private readonly uint* _linesize;

    /// <summary>Pixel format of the frame (matches what was requested in <see cref="RawVideoSubscription"/>).</summary>
    public VideoFormat Format { get; }

    /// <summary>Frame width in pixels.</summary>
    public uint Width { get; }

    /// <summary>Frame height in pixels.</summary>
    public uint Height { get; }

    /// <summary>Frame timestamp in nanoseconds (OBS monotonic clock).</summary>
    public ulong Timestamp { get; }

    internal RawVideoFrame(nint* data, uint* linesize, VideoFormat format, uint width, uint height, ulong timestamp)
    {
        _data = data;
        _linesize = linesize;
        Format = format;
        Width = width;
        Height = height;
        Timestamp = timestamp;
    }

    /// <summary>Linesize (stride in bytes) for the given plane.</summary>
    public uint GetLinesize(int planeIndex)
    {
        if ((uint)planeIndex >= VideoDataNative.MaxAvPlanes)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        return _linesize[planeIndex];
    }

    /// <summary>Raw pointer to the start of the given plane, or <see cref="nint.Zero"/> if unused.</summary>
    public nint GetPlanePointer(int planeIndex)
    {
        if ((uint)planeIndex >= VideoDataNative.MaxAvPlanes)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        return _data[planeIndex];
    }

    /// <summary>
    /// Returns a span over the given plane.
    /// </summary>
    /// <param name="planeIndex">Plane index (0 for packed formats; 0..2 for planar YUV).</param>
    /// <param name="rows">Number of rows the plane covers. For packed formats this is <see cref="Height"/>;
    /// for planar 4:2:0 chroma planes this is <see cref="Height"/> / 2.</param>
    public ReadOnlySpan<byte> GetPlane(int planeIndex, uint rows)
    {
        if ((uint)planeIndex >= VideoDataNative.MaxAvPlanes)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        var ptr = _data[planeIndex];
        if (ptr == nint.Zero)
            return default;
        return new ReadOnlySpan<byte>((void*)ptr, checked((int)(rows * _linesize[planeIndex])));
    }

    /// <summary>
    /// Convenience for single-plane packed formats (BGRA, RGBA, BGRX): returns plane 0 sized by <see cref="Height"/>.
    /// </summary>
    public ReadOnlySpan<byte> GetPackedPlane() => GetPlane(0, Height);
}
