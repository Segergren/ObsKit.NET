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

    /// <summary>Padding to match libobs's 16-byte <c>struct vec3</c>. Always 0.</summary>
    public float W;

    public Vec3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
        W = 0;
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

/// <summary>
/// Conversion / scaling info for raw video output.
/// Maps to OBS's <c>struct video_scale_info</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct VideoScaleInfo
{
    /// <summary>Pixel format the frames will be delivered in.</summary>
    public VideoFormat Format;

    /// <summary>Output width (after scaling).</summary>
    public uint Width;

    /// <summary>Output height (after scaling).</summary>
    public uint Height;

    /// <summary>Color range. Use <see cref="VideoRangeType.Default"/> to inherit canvas range.</summary>
    public VideoRangeType Range;

    /// <summary>Color space. Use <see cref="VideoColorspace.Default"/> to inherit canvas colorspace.</summary>
    public VideoColorspace Colorspace;
}

/// <summary>
/// Native layout of OBS's <c>struct obs_mouse_event</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ObsMouseEventNative
{
    public uint Modifiers;
    public int X;
    public int Y;
}

/// <summary>
/// Native layout of OBS's <c>struct obs_key_event</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ObsKeyEventNative
{
    public uint Modifiers;
    public nint Text;
    public uint NativeModifiers;
    public uint NativeScancode;
    public uint NativeVkey;
}

/// <summary>
/// Native layout of OBS's <c>struct audio_convert_info</c>.
/// Describes the format raw audio callbacks should be converted to.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct AudioConvertInfo
{
    /// <summary>Sample rate in Hz.</summary>
    public uint SamplesPerSec;

    /// <summary>Sample format.</summary>
    public AudioFormat Format;

    /// <summary>Speaker layout.</summary>
    public SpeakerLayout Speakers;

    private byte _allowClipping;

    /// <summary>Whether converted samples may clip instead of being limited.</summary>
    public bool AllowClipping
    {
        get => _allowClipping != 0;
        set => _allowClipping = value ? (byte)1 : (byte)0;
    }
}

/// <summary>
/// Native layout of OBS's <c>struct audio_data</c>.
/// Holds up to <see cref="MaxAvPlanes"/> plane pointers, the frame count, and a timestamp.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct AudioDataNative
{
    /// <summary>Maximum number of planes OBS exposes per audio packet.</summary>
    public const int MaxAvPlanes = 8;

    public nint Data0;
    public nint Data1;
    public nint Data2;
    public nint Data3;
    public nint Data4;
    public nint Data5;
    public nint Data6;
    public nint Data7;

    public uint Frames;

    public ulong Timestamp;
}

/// <summary>
/// Native layout of OBS's <c>struct video_data</c>.
/// Holds up to <see cref="MaxAvPlanes"/> data plane pointers, their linesizes, and a timestamp.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct VideoDataNative
{
    /// <summary>Maximum number of planes OBS exposes per frame.</summary>
    public const int MaxAvPlanes = 8;

    public nint Data0;
    public nint Data1;
    public nint Data2;
    public nint Data3;
    public nint Data4;
    public nint Data5;
    public nint Data6;
    public nint Data7;

    public uint Linesize0;
    public uint Linesize1;
    public uint Linesize2;
    public uint Linesize3;
    public uint Linesize4;
    public uint Linesize5;
    public uint Linesize6;
    public uint Linesize7;

    public ulong Timestamp;
}

/// <summary>
/// A physical key plus modifier flags, as bound to an OBS hotkey (obs_key_combination).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsKeyCombination
{
    /// <summary>Modifier and state flags that must be held for the combination to match.</summary>
    public ObsKeyModifiers Modifiers;

    /// <summary>The key. May be <see cref="ObsKey.None"/> for modifier-only combinations.</summary>
    public ObsKey Key;

    /// <summary>Creates a key combination.</summary>
    /// <param name="key">The key.</param>
    /// <param name="modifiers">Modifiers that must be held.</param>
    public ObsKeyCombination(ObsKey key, ObsKeyModifiers modifiers = ObsKeyModifiers.None)
    {
        Key = key;
        Modifiers = modifiers;
    }

    /// <summary>Gets whether this combination is empty (no key and no modifiers).</summary>
    public readonly bool IsEmpty => Modifiers == ObsKeyModifiers.None && Key == ObsKey.None;
}

/// <summary>
/// A region of interest for a video encoder (obs_encoder_roi) — prioritizes
/// quality inside the region. Edges are in pixels from the frame's top-left.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ObsEncoderRoi
{
    /// <summary>Top edge, in pixels from row 0.</summary>
    public uint Top;
    /// <summary>Bottom edge, in pixels from row 0 (exclusive).</summary>
    public uint Bottom;
    /// <summary>Left edge, in pixels from column 0.</summary>
    public uint Left;
    /// <summary>Right edge, in pixels from column 0 (exclusive).</summary>
    public uint Right;
    /// <summary>
    /// Priority from -1 to 1: above 0 increases quality in the region, below 0
    /// reduces it (negative values are ignored by some encoders).
    /// </summary>
    public float Priority;
}

/// <summary>
/// 4D vector structure (libobs <c>struct vec4</c>).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vec4
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vec4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static Vec4 Zero => new(0, 0, 0, 0);

    public override readonly string ToString() => $"({X}, {Y}, {Z}, {W})";
}

/// <summary>
/// Quaternion structure (libobs <c>struct quat</c>).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Quat
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Quat(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static Quat Identity => new(0, 0, 0, 1);

    public override readonly string ToString() => $"({X}, {Y}, {Z}, {W})";
}

/// <summary>
/// 4x4 matrix (libobs <c>struct matrix4</c>): three axis rows plus a translation row.
/// Used for scene item draw/box transforms; a point transforms as
/// <c>p.x * X + p.y * Y + p.z * Z + T</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Matrix4
{
    public Vec4 X;
    public Vec4 Y;
    public Vec4 Z;
    public Vec4 T;

    public static Matrix4 Identity => new()
    {
        X = new Vec4(1, 0, 0, 0),
        Y = new Vec4(0, 1, 0, 0),
        Z = new Vec4(0, 0, 1, 0),
        T = new Vec4(0, 0, 0, 1),
    };

    /// <summary>
    /// Transforms a 2D point (z = 0, w = 1) by this matrix, returning the resulting x/y.
    /// For scene item transforms this maps item-local coordinates to canvas pixels.
    /// </summary>
    public readonly Vec2 Transform(Vec2 point) => new(
        point.X * X.X + point.Y * Y.X + T.X,
        point.X * X.Y + point.Y * Y.Y + T.Y);

    public override readonly string ToString() => $"[{X}; {Y}; {Z}; {T}]";
}

/// <summary>
/// A frame rate expressed as a rational number (libobs <c>struct media_frames_per_second</c>).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MediaFramesPerSecond
{
    public uint Numerator;
    public uint Denominator;

    public MediaFramesPerSecond(uint numerator, uint denominator)
    {
        Numerator = numerator;
        Denominator = denominator;
    }

    /// <summary>Gets whether both terms are non-zero.</summary>
    public readonly bool IsValid => Numerator != 0 && Denominator != 0;

    /// <summary>Gets the frame rate as a floating-point value (0 if invalid).</summary>
    public readonly double Value => IsValid ? (double)Numerator / Denominator : 0;

    /// <summary>Gets the frame interval, or <see cref="TimeSpan.Zero"/> if invalid.</summary>
    public readonly TimeSpan Interval => IsValid ? TimeSpan.FromSeconds((double)Denominator / Numerator) : TimeSpan.Zero;

    public override readonly string ToString() => Denominator == 1 ? $"{Numerator} fps" : $"{Numerator}/{Denominator} fps";
}

/// <summary>
/// Native layout of OBS's <c>struct obs_module_info2</c> (strings are OBS-owned).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ObsModuleInfo2Native
{
    public nint BinPath;
    public nint DataPath;
    public nint Name;
}

/// <summary>
/// Native layout of OBS's <c>struct encoder_packet</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct EncoderPacketNative
{
    public nint Data;
    public nuint Size;
    public long Pts;
    public long Dts;
    public int TimebaseNum;
    public int TimebaseDen;
    public ObsEncoderType Type;
    public byte Keyframe;
    public long DtsUsec;
    public long SysDtsUsec;
    public int Priority;
    public int DropPriority;
    public nuint TrackIdx;
    public nint Encoder;
}

/// <summary>
/// Native layout of OBS's <c>struct encoder_packet_time</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct EncoderPacketTimeNative
{
    public long Pts;
    public ulong Cts;
    public ulong Fer;
    public ulong Ferc;
    public ulong Pir;
}

/// <summary>
/// Native layout of the readable head of OBS's <c>struct obs_source_frame</c> (the trailing
/// internal refcount fields are omitted; this struct is only ever read through a pointer).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct ObsSourceFrameNative
{
    public nint Data0;
    public nint Data1;
    public nint Data2;
    public nint Data3;
    public nint Data4;
    public nint Data5;
    public nint Data6;
    public nint Data7;

    public uint Linesize0;
    public uint Linesize1;
    public uint Linesize2;
    public uint Linesize3;
    public uint Linesize4;
    public uint Linesize5;
    public uint Linesize6;
    public uint Linesize7;

    public uint Width;
    public uint Height;
    public ulong Timestamp;

    public VideoFormat Format;
    public fixed float ColorMatrix[16];
    public byte FullRange;
    public ushort MaxLuminance;
    public fixed float ColorRangeMin[3];
    public fixed float ColorRangeMax[3];
    public byte Flip;
    public byte Flags;
    public byte Trc;
}

/// <summary>
/// Native layout of OBS's <c>struct obs_sceneitem_order_info</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct ObsSceneItemOrderInfoNative
{
    public nint Group;
    public nint Item;
}
