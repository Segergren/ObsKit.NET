using System.Runtime.InteropServices;

namespace ObsKit.NET.Native.Types;

/// <summary>
/// Video initialization structure for OBS.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsVideoInfo
{
    /// <summary>
    /// Graphics module to use (usually "libobs-opengl" or "libobs-d3d11").
    /// </summary>
    public nint GraphicsModule;

    /// <summary>
    /// Output FPS numerator.
    /// </summary>
    public uint FpsNum;

    /// <summary>
    /// Output FPS denominator.
    /// </summary>
    public uint FpsDen;

    /// <summary>
    /// Base compositing width.
    /// </summary>
    public uint BaseWidth;

    /// <summary>
    /// Base compositing height.
    /// </summary>
    public uint BaseHeight;

    /// <summary>
    /// Output width.
    /// </summary>
    public uint OutputWidth;

    /// <summary>
    /// Output height.
    /// </summary>
    public uint OutputHeight;

    /// <summary>
    /// Output format.
    /// </summary>
    public VideoFormat OutputFormat;

    /// <summary>
    /// Video adapter index to use.
    /// </summary>
    public uint Adapter;

    /// <summary>
    /// Use shaders to convert to different color formats.
    /// </summary>
    private byte _gpuConversion;

    /// <summary>
    /// Use shaders to convert to different color formats.
    /// </summary>
    public bool GpuConversion
    {
        get => _gpuConversion != 0;
        set => _gpuConversion = value ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// YUV color space type.
    /// </summary>
    public VideoColorspace Colorspace;

    /// <summary>
    /// YUV range type.
    /// </summary>
    public VideoRangeType Range;

    /// <summary>
    /// How to scale if scaling is needed.
    /// </summary>
    public ObsScaleType ScaleType;
}

/// <summary>
/// Audio initialization structure for OBS.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsAudioInfo
{
    /// <summary>
    /// Samples per second (e.g., 44100, 48000).
    /// </summary>
    public uint SamplesPerSec;

    /// <summary>
    /// Speaker layout configuration.
    /// </summary>
    public SpeakerLayout Speakers;
}

/// <summary>
/// Extended audio initialization structure for OBS.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsAudioInfo2
{
    /// <summary>
    /// Samples per second (e.g., 44100, 48000).
    /// </summary>
    public uint SamplesPerSec;

    /// <summary>
    /// Speaker layout configuration.
    /// </summary>
    public SpeakerLayout Speakers;

    /// <summary>
    /// Maximum buffering in milliseconds.
    /// </summary>
    public uint MaxBufferingMs;

    /// <summary>
    /// Use fixed buffering.
    /// </summary>
    private byte _fixedBuffering;

    /// <summary>
    /// Use fixed buffering.
    /// </summary>
    public bool FixedBuffering
    {
        get => _fixedBuffering != 0;
        set => _fixedBuffering = value ? (byte)1 : (byte)0;
    }
}

/// <summary>
/// 2D vector structure.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vec2
{
    public float X;
    public float Y;

    public Vec2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Vec2 Zero => new(0, 0);
    public static Vec2 One => new(1, 1);

    public override string ToString() => $"({X}, {Y})";
}

/// <summary>
/// 3D vector structure.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vec3
{
    public float X;
    public float Y;
    public float Z;

    public Vec3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vec3 Zero => new(0, 0, 0);
    public static Vec3 One => new(1, 1, 1);

    public override string ToString() => $"({X}, {Y}, {Z})";
}

/// <summary>
/// Transform information for scene items.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsTransformInfo
{
    /// <summary>
    /// Position of the item.
    /// </summary>
    public Vec2 Pos;

    /// <summary>
    /// Rotation in degrees.
    /// </summary>
    public float Rot;

    /// <summary>
    /// Scale factor.
    /// </summary>
    public Vec2 Scale;

    /// <summary>
    /// Alignment flags.
    /// </summary>
    public uint Alignment;

    /// <summary>
    /// Bounds type.
    /// </summary>
    public ObsBoundsType BoundsType;

    /// <summary>
    /// Bounds alignment flags.
    /// </summary>
    public uint BoundsAlignment;

    /// <summary>
    /// Bounds dimensions.
    /// </summary>
    public Vec2 Bounds;

    /// <summary>
    /// Whether to crop to bounds.
    /// </summary>
    private byte _cropToBounds;

    /// <summary>
    /// Whether to crop to bounds.
    /// </summary>
    public bool CropToBounds
    {
        get => _cropToBounds != 0;
        set => _cropToBounds = value ? (byte)1 : (byte)0;
    }
}

/// <summary>
/// Crop settings for scene items.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsSceneItemCrop
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}

/// <summary>
/// Module failure information for obs_load_all_modules2.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ObsModuleFailureInfo
{
    public nint FailedModules; // char** - array of module names that failed
    public nuint Count;
}
