using System.Runtime.InteropServices;
using ObsKit.NET.Native;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Video;

/// <summary>
/// A software scaler/pixel-format converter (libobs <c>video_scaler_t</c>, backed by swscale).
/// Converts raw frames between formats and sizes on the CPU, e.g. NV12 frames from
/// <see cref="Obs.SubscribeRawVideo"/> or <see cref="Source.TryGetAsyncFrame"/> into BGRA for
/// a thumbnail. Create one per input/output configuration and reuse it; instances are not
/// thread-safe.
/// </summary>
public sealed class VideoScaler : IDisposable
{
    private const int MaxPlanes = VideoDataNative.MaxAvPlanes;

    private nint _scaler;
    private readonly nint _scratch;
    private bool _disposed;

    /// <summary>
    /// Creates a scaler.
    /// </summary>
    /// <param name="source">Format, size and color info of the input frames.</param>
    /// <param name="destination">Format, size and color info to produce.</param>
    /// <param name="type">Scaling algorithm.</param>
    /// <exception cref="NotSupportedException">The conversion is not supported.</exception>
    public VideoScaler(VideoScaleInfo source, VideoScaleInfo destination, VideoScaleType type = VideoScaleType.Default)
    {
        if (source.Width == 0 || source.Height == 0 || destination.Width == 0 || destination.Height == 0)
            throw new ArgumentException("Source and destination sizes must be non-zero.");

        LibraryLoader.Initialize();
        Source = source;
        Destination = destination;

        var result = MediaIo.video_scaler_create(out _scaler, in destination, in source, type);
        if (result != 0 || _scaler == nint.Zero)
            throw new NotSupportedException($"Cannot convert {source.Format} {source.Width}x{source.Height} to {destination.Format} {destination.Width}x{destination.Height} (code {result}).");

        // 4 arrays of MaxPlanes entries: out data, out linesize, in data, in linesize.
        _scratch = Marshal.AllocHGlobal(MaxPlanes * (nint.Size * 2 + sizeof(uint) * 2));
    }

    /// <summary>The input configuration.</summary>
    public VideoScaleInfo Source { get; }

    /// <summary>The output configuration.</summary>
    public VideoScaleInfo Destination { get; }

    /// <summary>
    /// Scales/converts one frame given raw plane pointers and strides (up to 8 planes each;
    /// unused entries may be omitted or zero).
    /// </summary>
    /// <returns>False if the scaler rejected the frame.</returns>
    public unsafe bool Scale(ReadOnlySpan<nint> inputPlanes, ReadOnlySpan<uint> inputLinesizes, ReadOnlySpan<nint> outputPlanes, ReadOnlySpan<uint> outputLinesizes)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (inputPlanes.Length > MaxPlanes || inputLinesizes.Length > MaxPlanes || outputPlanes.Length > MaxPlanes || outputLinesizes.Length > MaxPlanes)
            throw new ArgumentException("At most 8 planes are supported.");

        var outData = (nint*)_scratch;
        var inData = outData + MaxPlanes;
        var outLine = (uint*)(inData + MaxPlanes);
        var inLine = outLine + MaxPlanes;
        new Span<byte>((void*)_scratch, MaxPlanes * (nint.Size * 2 + sizeof(uint) * 2)).Clear();

        outputPlanes.CopyTo(new Span<nint>(outData, MaxPlanes));
        inputPlanes.CopyTo(new Span<nint>(inData, MaxPlanes));
        outputLinesizes.CopyTo(new Span<uint>(outLine, MaxPlanes));
        inputLinesizes.CopyTo(new Span<uint>(inLine, MaxPlanes));

        return MediaIo.video_scaler_scale(_scaler, (nint)outData, (nint)outLine, (nint)inData, (nint)inLine);
    }

    /// <summary>
    /// Converts a raw callback frame into a packed single-plane buffer (BGRA, RGBA, BGRX, ...).
    /// The destination format must be a packed format.
    /// </summary>
    /// <param name="frame">The input frame (must match <see cref="Source"/>).</param>
    /// <param name="output">Buffer of at least <paramref name="outputLinesize"/> x destination height bytes.</param>
    /// <param name="outputLinesize">Stride of the output in bytes (0 = tightly packed, 4 bytes per pixel).</param>
    public unsafe bool Scale(in RawVideoFrame frame, Span<byte> output, uint outputLinesize = 0)
    {
        if (outputLinesize == 0)
            outputLinesize = Destination.Width * 4;
        if (output.Length < checked((int)(outputLinesize * Destination.Height)))
            throw new ArgumentException("Output buffer is too small.", nameof(output));

        Span<nint> inPlanes = stackalloc nint[MaxPlanes];
        Span<uint> inLines = stackalloc uint[MaxPlanes];
        for (var i = 0; i < MaxPlanes; i++)
        {
            inPlanes[i] = frame.GetPlanePointer(i);
            inLines[i] = frame.GetLinesize(i);
        }

        fixed (byte* dst = output)
        {
            Span<nint> outPlanes = stackalloc nint[1];
            outPlanes[0] = (nint)dst;
            Span<uint> outLines = stackalloc uint[1];
            outLines[0] = outputLinesize;
            return Scale(inPlanes, inLines, outPlanes, outLines);
        }
    }

    /// <summary>
    /// Converts a copied async frame into a packed single-plane buffer (see
    /// <see cref="Scale(in RawVideoFrame, Span{byte}, uint)"/>).
    /// </summary>
    public unsafe bool Scale(SourceFrame frame, Span<byte> output, uint outputLinesize = 0)
    {
        ArgumentNullException.ThrowIfNull(frame);
        if (outputLinesize == 0)
            outputLinesize = Destination.Width * 4;
        if (output.Length < checked((int)(outputLinesize * Destination.Height)))
            throw new ArgumentException("Output buffer is too small.", nameof(output));

        var handles = new GCHandle[MaxPlanes];
        Span<nint> inPlanes = stackalloc nint[MaxPlanes];
        Span<uint> inLines = stackalloc uint[MaxPlanes];
        try
        {
            for (var i = 0; i < MaxPlanes; i++)
            {
                var plane = frame.GetPlane(i);
                if (plane.IsEmpty)
                    continue;
                handles[i] = GCHandle.Alloc(plane.ToArray(), GCHandleType.Pinned);
                inPlanes[i] = handles[i].AddrOfPinnedObject();
                inLines[i] = frame.GetLinesize(i);
            }

            fixed (byte* dst = output)
            {
                Span<nint> outPlanes = stackalloc nint[1];
                outPlanes[0] = (nint)dst;
                Span<uint> outLines = stackalloc uint[1];
                outLines[0] = outputLinesize;
                return Scale(inPlanes, inLines, outPlanes, outLines);
            }
        }
        finally
        {
            foreach (var h in handles)
                if (h.IsAllocated)
                    h.Free();
        }
    }

    /// <summary>Releases the scaler.</summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~VideoScaler()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        if (_scaler != nint.Zero)
            MediaIo.video_scaler_destroy(_scaler);
        _scaler = nint.Zero;
        if (_scratch != nint.Zero)
            Marshal.FreeHGlobal(_scratch);
    }
}
