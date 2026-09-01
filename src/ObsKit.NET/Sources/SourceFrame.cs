using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Sources;

/// <summary>
/// A managed copy of an async video frame pulled from a source with
/// <see cref="Source.TryGetAsyncFrame"/> (webcams, media, capture cards, and other sources that
/// push frames through the async pipeline). Plane data is copied out of libobs, so the object
/// can be kept and used from any thread.
/// </summary>
public sealed class SourceFrame
{
    private readonly byte[]?[] _planes;
    private readonly uint[] _linesizes;

    internal unsafe SourceFrame(ObsSourceFrameNative* native)
    {
        Width = native->Width;
        Height = native->Height;
        Timestamp = native->Timestamp;
        Format = native->Format;
        FullRange = native->FullRange != 0;
        MaxLuminance = native->MaxLuminance;
        Flip = native->Flip != 0;
        Flags = native->Flags;
        Trc = (VideoTrc)native->Trc;

        ColorMatrix = new float[16];
        for (var i = 0; i < 16; i++)
            ColorMatrix[i] = native->ColorMatrix[i];

        var rows = VideoFormatInfo.GetPlaneRows(Format, Height);
        var data = &native->Data0;
        var linesize = &native->Linesize0;
        _planes = new byte[VideoDataNative.MaxAvPlanes][];
        _linesizes = new uint[VideoDataNative.MaxAvPlanes];
        for (var i = 0; i < VideoDataNative.MaxAvPlanes; i++)
        {
            _linesizes[i] = linesize[i];
            if (data[i] == nint.Zero || rows[i] == 0 || linesize[i] == 0)
                continue;
            var bytes = new byte[checked((int)(rows[i] * linesize[i]))];
            new ReadOnlySpan<byte>((void*)data[i], bytes.Length).CopyTo(bytes);
            _planes[i] = bytes;
        }
    }

    /// <summary>Frame width in pixels.</summary>
    public uint Width { get; }

    /// <summary>Frame height in pixels.</summary>
    public uint Height { get; }

    /// <summary>Timestamp in nanoseconds (source clock).</summary>
    public ulong Timestamp { get; }

    /// <summary>Pixel format.</summary>
    public VideoFormat Format { get; }

    /// <summary>Whether YUV data uses full range (true) or limited/video range (false).</summary>
    public bool FullRange { get; }

    /// <summary>Peak luminance in nits for HDR content (0 if unknown/SDR).</summary>
    public ushort MaxLuminance { get; }

    /// <summary>Whether the frame is stored bottom-up.</summary>
    public bool Flip { get; }

    /// <summary>Raw frame flags (OBS_SOURCE_FRAME_*).</summary>
    public byte Flags { get; }

    /// <summary>Transfer characteristics.</summary>
    public VideoTrc Trc { get; }

    /// <summary>The 4x4 YUV-to-RGB matrix libobs associated with the frame (row-major).</summary>
    public float[] ColorMatrix { get; }

    /// <summary>Number of planes that carry data.</summary>
    public int PlaneCount => _planes.Count(p => p != null);

    /// <summary>Stride in bytes of the given plane (0 if unused).</summary>
    public uint GetLinesize(int planeIndex)
    {
        if ((uint)planeIndex >= VideoDataNative.MaxAvPlanes)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        return _linesizes[planeIndex];
    }

    /// <summary>Number of rows in the given plane for this frame's format (0 if unused).</summary>
    public uint GetPlaneRows(int planeIndex)
    {
        if ((uint)planeIndex >= VideoDataNative.MaxAvPlanes)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        return VideoFormatInfo.GetPlaneRows(Format, Height)[planeIndex];
    }

    /// <summary>The bytes of the given plane (rows x linesize), or an empty span if unused.</summary>
    public ReadOnlySpan<byte> GetPlane(int planeIndex)
    {
        if ((uint)planeIndex >= VideoDataNative.MaxAvPlanes)
            throw new ArgumentOutOfRangeException(nameof(planeIndex));
        return _planes[planeIndex];
    }

    /// <summary>Convenience for packed single-plane formats: the bytes of plane 0.</summary>
    public ReadOnlySpan<byte> GetPackedPlane() => GetPlane(0);
}

/// <summary>
/// Plane layout facts for libobs video formats (mirrors <c>video_frame_get_plane_heights</c>).
/// </summary>
internal static class VideoFormatInfo
{
    private static uint Half(uint x) => (x + 1) / 2;

    /// <summary>Row count of each of the 8 possible planes for a format at a given height.</summary>
    public static uint[] GetPlaneRows(VideoFormat format, uint height)
    {
        var rows = new uint[VideoDataNative.MaxAvPlanes];
        switch (format)
        {
            case VideoFormat.I420:
            case VideoFormat.I010:
                rows[0] = height; rows[1] = Half(height); rows[2] = Half(height);
                break;
            case VideoFormat.NV12:
            case VideoFormat.P010:
                rows[0] = height; rows[1] = Half(height);
                break;
            case VideoFormat.Y800:
            case VideoFormat.YVYU:
            case VideoFormat.YUY2:
            case VideoFormat.UYVY:
            case VideoFormat.RGBA:
            case VideoFormat.BGRA:
            case VideoFormat.BGRX:
            case VideoFormat.BGR3:
            case VideoFormat.AYUV:
            case VideoFormat.V210:
            case VideoFormat.R10L:
                rows[0] = height;
                break;
            case VideoFormat.I444:
            case VideoFormat.I422:
            case VideoFormat.I210:
            case VideoFormat.I412:
                rows[0] = height; rows[1] = height; rows[2] = height;
                break;
            case VideoFormat.I40A:
                rows[0] = height; rows[1] = Half(height); rows[2] = Half(height); rows[3] = height;
                break;
            case VideoFormat.I42A:
            case VideoFormat.YUVA:
            case VideoFormat.YA2L:
                rows[0] = height; rows[1] = height; rows[2] = height; rows[3] = height;
                break;
            case VideoFormat.P216:
            case VideoFormat.P416:
                rows[0] = height; rows[1] = height;
                break;
        }
        return rows;
    }
}
