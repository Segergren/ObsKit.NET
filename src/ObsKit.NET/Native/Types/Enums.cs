namespace ObsKit.NET.Native.Types;

/// <summary>
/// Video pixel formats supported by OBS.
/// </summary>
public enum VideoFormat
{
    None = 0,

    // Planar 4:2:0 formats
    I420,   // Three-plane
    NV12,   // Two-plane, luma and packed chroma

    // Packed 4:2:2 formats
    YVYU,
    YUY2,   // YUYV
    UYVY,

    // Packed uncompressed formats
    RGBA,
    BGRA,
    BGRX,
    Y800,   // Grayscale

    // Planar 4:4:4
    I444,

    // More packed uncompressed formats
    BGR3,

    // Planar 4:2:2
    I422,

    // Planar 4:2:0 with alpha
    I40A,

    // Planar 4:2:2 with alpha
    I42A,

    // Planar 4:4:4 with alpha
    YUVA,

    // Packed 4:4:4 with alpha
    AYUV,

    // Planar 4:2:0 format, 10 bpp
    I010,   // Three-plane
    P010,   // Two-plane, luma and packed chroma

    // Planar 4:2:2 format, 10 bpp
    I210,

    // Planar 4:4:4 format, 12 bpp
    I412,

    // Planar 4:4:4:4 format, 12 bpp
    YA2L,

    // Planar 4:2:2 format, 16 bpp
    P216,   // Two-plane, luma and packed chroma

    // Planar 4:4:4 format, 16 bpp
    P416,   // Two-plane, luma and packed chroma

    // Packed 4:2:2 format, 10 bpp
    V210,

    // Packed uncompressed 10-bit format
    R10L,
}

/// <summary>
/// Video transfer characteristics.
/// </summary>
public enum VideoTrc
{
    Default = 0,
    Srgb,
    PQ,
    HLG,
}

/// <summary>
/// Video colorspace types.
/// </summary>
public enum VideoColorspace
{
    Default = 0,
    CS601,
    CS709,
    Srgb,
    CS2100PQ,
    CS2100HLG,
}

/// <summary>
/// Video range types (full vs partial/limited).
/// </summary>
public enum VideoRangeType
{
    Default = 0,
    Partial,
    Full,
}

/// <summary>
/// Audio sample formats.
/// </summary>
public enum AudioFormat
{
    Unknown = 0,

    U8Bit,
    Bit16,
    Bit32,
    Float,

    U8BitPlanar,
    Bit16Planar,
    Bit32Planar,
    FloatPlanar,
}

/// <summary>
/// Speaker layout configurations.
/// </summary>
public enum SpeakerLayout
{
    Unknown = 0,
    Mono,
    Stereo,
    TwoPointOne,
    FourPointZero,
    FourPointOne,
    FivePointOne,
    SevenPointOne = 8,
}

/// <summary>
/// Scale/interpolation types for video scaling.
/// </summary>
public enum ObsScaleType
{
    Disable = 0,
    Point,
    Bicubic,
    Bilinear,
    Lanczos,
    Area,
}

/// <summary>
/// Bounds types for scene items.
/// </summary>
public enum ObsBoundsType
{
    None = 0,
    Stretch,
    ScaleInner,
    ScaleOuter,
    ScaleToWidth,
    ScaleToHeight,
    MaxOnly,
}

/// <summary>
/// Order movement directions for items.
/// </summary>
public enum ObsOrderMovement
{
    MoveUp = 0,
    MoveDown,
    MoveTop,
    MoveBottom,
}

/// <summary>
/// Blending methods.
/// </summary>
public enum ObsBlendingMethod
{
    Default = 0,
    SrgbOff,
}

/// <summary>
/// Blending types.
/// </summary>
public enum ObsBlendingType
{
    Normal = 0,
    Additive,
    Subtract,
    Screen,
    Multiply,
    Lighten,
    Darken,
}

/// <summary>
/// Source types.
/// </summary>
public enum ObsSourceType
{
    Input = 0,
    Filter,
    Transition,
    Scene,
}

/// <summary>
/// Encoder types.
/// </summary>
public enum ObsEncoderType
{
    Audio = 0,
    Video,
}

/// <summary>
/// Log levels for OBS logging.
/// </summary>
public enum ObsLogLevel
{
    Error = 100,
    Warning = 200,
    Info = 300,
    Debug = 400,
}

/// <summary>
/// Video reset return codes.
/// </summary>
public enum ObsVideoResetResult
{
    Success = 0,
    Fail = -1,
    NotSupported = -2,
    InvalidParam = -3,
    CurrentlyActive = -4,
    ModuleNotFound = -5,
}

/// <summary>
/// Source output flags.
/// </summary>
[Flags]
public enum ObsSourceFlags : uint
{
    None = 0,
    Video = 1 << 0,
    Audio = 1 << 1,
    AsyncVideo = 1 << 2,
    CustomDraw = 1 << 3,
    Interaction = 1 << 5,
    Composite = 1 << 6,
    DoNotDuplicate = 1 << 7,
    Deprecated = 1 << 8,
    DoNotSelfMonitor = 1 << 9,
    Submix = 1 << 10,
    Controllable = 1 << 11,
    CapObsolete = 1 << 12,
    SrgbTransform = 1 << 13,
}

/// <summary>
/// Output flags.
/// </summary>
[Flags]
public enum ObsOutputFlags : uint
{
    None = 0,
    Video = 1 << 0,
    Audio = 1 << 1,
    AV = Video | Audio,
    Encoded = 1 << 2,
    Service = 1 << 3,
    MultiTrack = 1 << 4,
    CanPause = 1 << 5,
    // New in OBS 30+
    MultiTrackVideo = 1 << 6,
    MultiTrackAV = MultiTrack | MultiTrackVideo,
}
