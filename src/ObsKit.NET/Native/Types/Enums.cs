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
/// Canvas behavior flags (obs_canvas_flags, OBS 31+).
/// </summary>
[Flags]
public enum ObsCanvasFlags : uint
{
    /// <summary>The main canvas created by libobs (cannot be created, renamed, or reset by users).</summary>
    Main = 1 << 0,
    /// <summary>Sources on this canvas become active when visible.</summary>
    Activate = 1 << 1,
    /// <summary>Audio from this canvas's channels is mixed into the audio output.</summary>
    MixAudio = 1 << 2,
    /// <summary>The canvas holds references for its scene sources.</summary>
    SceneRef = 1 << 3,
    /// <summary>The canvas is not saved.</summary>
    Ephemeral = 1 << 4,

    /// <summary>Preset for a recordable program canvas (Activate | MixAudio | SceneRef).</summary>
    Program = Activate | MixAudio | SceneRef,
    /// <summary>Preset for a preview-only canvas (Ephemeral).</summary>
    Preview = Ephemeral,
    /// <summary>Preset for a device canvas (Activate | Ephemeral).</summary>
    Device = Activate | Ephemeral,
}

/// <summary>
/// Modifier flags for interaction events (obs_interaction_flags).
/// </summary>
[Flags]
public enum ObsInteractionFlags : uint
{
    None = 0,
    CapsLock = 1 << 0,
    Shift = 1 << 1,
    Control = 1 << 2,
    Alt = 1 << 3,
    MouseLeft = 1 << 4,
    MouseMiddle = 1 << 5,
    MouseRight = 1 << 6,
    Command = 1 << 7,
    NumLock = 1 << 8,
    IsKeyPad = 1 << 9,
    IsLeft = 1 << 10,
    IsRight = 1 << 11,
}

/// <summary>
/// Mouse buttons for interaction events (obs_mouse_button_type).
/// </summary>
public enum ObsMouseButton
{
    Left = 0,
    Middle,
    Right,
}

/// <summary>
/// How a scene is duplicated (obs_scene_duplicate_type).
/// </summary>
public enum ObsSceneDuplicateType
{
    /// <summary>The new scene references the same sources.</summary>
    Refs = 0,
    /// <summary>Sources are fully duplicated.</summary>
    Copy,
    /// <summary>The new scene references the same sources, created as private.</summary>
    PrivateRefs,
    /// <summary>Sources are fully duplicated, created as private.</summary>
    PrivateCopy,
}

/// <summary>
/// Deinterlacing modes (obs_deinterlace_mode).
/// </summary>
public enum ObsDeinterlaceMode
{
    /// <summary>No deinterlacing (default).</summary>
    Disable = 0,
    /// <summary>Discard one field.</summary>
    Discard,
    /// <summary>Retro (bob with field doubling).</summary>
    Retro,
    /// <summary>Blend fields.</summary>
    Blend,
    /// <summary>Blend fields, doubling frame rate.</summary>
    Blend2X,
    /// <summary>Linear interpolation.</summary>
    Linear,
    /// <summary>Linear interpolation, doubling frame rate.</summary>
    Linear2X,
    /// <summary>YADIF.</summary>
    Yadif,
    /// <summary>YADIF, doubling frame rate.</summary>
    Yadif2X,
}

/// <summary>
/// Deinterlacing field order (obs_deinterlace_field_order).
/// </summary>
public enum ObsDeinterlaceFieldOrder
{
    /// <summary>Top field first.</summary>
    Top = 0,
    /// <summary>Bottom field first.</summary>
    Bottom,
}

/// <summary>
/// Audio monitoring types (obs_monitoring_type).
/// Determines whether a source's audio is played back through the
/// monitoring device, included in the output mix, or both.
/// </summary>
public enum ObsMonitoringType
{
    /// <summary>Audio is only sent to the output mix (default).</summary>
    None = 0,
    /// <summary>Audio is only played through the monitoring device and excluded from the output mix.</summary>
    MonitorOnly,
    /// <summary>Audio is played through the monitoring device and included in the output mix.</summary>
    MonitorAndOutput,
}

/// <summary>
/// Media playback states (obs_media_state).
/// </summary>
public enum ObsMediaState
{
    /// <summary>No media loaded or the source does not support media controls.</summary>
    None = 0,
    /// <summary>Media is playing.</summary>
    Playing,
    /// <summary>Media is being opened.</summary>
    Opening,
    /// <summary>Media is buffering.</summary>
    Buffering,
    /// <summary>Media is paused.</summary>
    Paused,
    /// <summary>Media is stopped.</summary>
    Stopped,
    /// <summary>Media reached its end.</summary>
    Ended,
    /// <summary>An error occurred during playback.</summary>
    Error,
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
    CapObsolete = 1 << 10,        // OBS_SOURCE_CAP_DISABLED / CAP_OBSOLETE
    MonitorByDefault = 1 << 11,   // OBS_SOURCE_MONITOR_BY_DEFAULT
    Submix = 1 << 12,             // OBS_SOURCE_SUBMIX
    ControllableMedia = 1 << 13,  // OBS_SOURCE_CONTROLLABLE_MEDIA
    Cea708 = 1 << 14,             // OBS_SOURCE_CEA_708
    SrgbTransform = 1 << 15,      // OBS_SOURCE_SRGB
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

/// <summary>
/// Mode used when starting a transition (obs_transition_mode).
/// </summary>
public enum ObsTransitionMode
{
    /// <summary>Automatically animate from the current source to the destination over the given duration.</summary>
    Auto = 0,
    /// <summary>Drive the transition manually via <see cref="ObsKit.NET.Sources.Transition.SetManualTime"/>.</summary>
    Manual = 1,
}

/// <summary>
/// Identifies one of the two sub-sources a transition blends between (obs_transition_target).
/// </summary>
public enum ObsTransitionTarget
{
    /// <summary>The source being transitioned away from.</summary>
    SourceA = 0,
    /// <summary>The source being transitioned to.</summary>
    SourceB = 1,
}

/// <summary>
/// Controls how sub-sources of differing sizes are scaled within a transition (obs_transition_scale_type).
/// </summary>
public enum ObsTransitionScaleType
{
    /// <summary>Scale up to the maximum size only.</summary>
    MaxOnly = 0,
    /// <summary>Scale preserving aspect ratio.</summary>
    Aspect = 1,
    /// <summary>Stretch to fill.</summary>
    Stretch = 2,
}

/// <summary>
/// The kind of a source/encoder property (obs_property_type), used when introspecting
/// configurable properties for building dynamic UIs.
/// </summary>
public enum ObsPropertyType
{
    /// <summary>Not a valid property.</summary>
    Invalid = 0,
    /// <summary>Boolean checkbox.</summary>
    Bool,
    /// <summary>Integer value (see int min/max/step).</summary>
    Int,
    /// <summary>Floating-point value (see float min/max/step).</summary>
    Float,
    /// <summary>Free text.</summary>
    Text,
    /// <summary>File or directory path.</summary>
    Path,
    /// <summary>A list/combo of selectable items.</summary>
    List,
    /// <summary>Color (RGB).</summary>
    Color,
    /// <summary>A clickable button.</summary>
    Button,
    /// <summary>Font selection.</summary>
    Font,
    /// <summary>An editable list of strings.</summary>
    EditableList,
    /// <summary>A frame-rate picker.</summary>
    FrameRate,
    /// <summary>A group of nested properties.</summary>
    Group,
    /// <summary>Color with alpha (RGBA).</summary>
    ColorAlpha,
}

/// <summary>
/// Alignment flags (OBS_ALIGN_*), used for scene-item alignment and bounds alignment.
/// Combine a horizontal flag (Left/Right) with a vertical flag (Top/Bottom); omit one for
/// centering on that axis. <see cref="Center"/> (0) centers on both axes.
/// </summary>
[Flags]
public enum ObsAlignment : uint
{
    /// <summary>Centered on both axes.</summary>
    Center = 0,
    /// <summary>Align to the left edge.</summary>
    Left = 1 << 0,
    /// <summary>Align to the right edge.</summary>
    Right = 1 << 1,
    /// <summary>Align to the top edge.</summary>
    Top = 1 << 2,
    /// <summary>Align to the bottom edge.</summary>
    Bottom = 1 << 3,
    /// <summary>Top-left corner.</summary>
    TopLeft = Top | Left,
    /// <summary>Top-right corner.</summary>
    TopRight = Top | Right,
    /// <summary>Bottom-left corner.</summary>
    BottomLeft = Bottom | Left,
    /// <summary>Bottom-right corner.</summary>
    BottomRight = Bottom | Right,
}

/// <summary>
/// The value format of a list/combo property's items (obs_combo_format).
/// </summary>
public enum ObsPropertyListFormat
{
    /// <summary>Not a valid format.</summary>
    Invalid = 0,
    /// <summary>Items carry integer values.</summary>
    Int = 1,
    /// <summary>Items carry floating-point values.</summary>
    Float = 2,
    /// <summary>Items carry string values.</summary>
    String = 3,
    /// <summary>Items carry boolean values.</summary>
    Bool = 4,
}
