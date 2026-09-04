using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ObsKit.NET.Platform.Windows.Interop;

/// <summary>
/// Details for a single display, resolved through the Windows CCD (Connecting and
/// Configuring Displays) API.
/// </summary>
internal sealed class DisplayConfigDetails
{
    public string FriendlyName { get; init; } = string.Empty;
    public string DevicePath { get; init; } = string.Empty;
    public double RefreshRate { get; init; }
    public bool IsInternal { get; init; }
    public bool IsHdrCapable { get; init; }
    public MonitorColorMode ColorMode { get; init; }
    public int BitsPerColorChannel { get; init; }
    public int SdrWhiteLevelNits { get; init; }
}

/// <summary>
/// P/Invoke bindings for the Windows CCD API, used to enrich monitor enumeration with
/// friendly names, exact refresh rates and color information that GDI does not expose.
/// </summary>
[SupportedOSPlatform("windows")]
internal static unsafe partial class DisplayConfig
{
    private const string Lib = "user32.dll";

    internal const int ERROR_SUCCESS = 0;
    internal const int ERROR_INSUFFICIENT_BUFFER = 122;

    internal const uint QDC_ALL_PATHS = 0x00000001;
    internal const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;

    internal const uint DISPLAYCONFIG_PATH_MODE_IDX_INVALID = 0xFFFFFFFF;

    internal const uint DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1;
    internal const uint DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2;

    internal const uint DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1;
    internal const uint DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2;
    internal const uint DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO = 9;
    internal const uint DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL = 11;
    internal const uint DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO_2 = 15;

    // DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO (type 9) flag bits
    internal const uint ADVANCED_COLOR_SUPPORTED = 1 << 0;
    internal const uint ADVANCED_COLOR_ENABLED = 1 << 1;
    internal const uint WIDE_COLOR_ENFORCED = 1 << 2;

    // DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2 (type 15) flag bits, which differ from type 9
    internal const uint ADVANCED_COLOR_SUPPORTED_2 = 1 << 0;
    internal const uint ADVANCED_COLOR_ACTIVE_2 = 1 << 1;
    internal const uint ADVANCED_COLOR_LIMITED_BY_POLICY_2 = 1 << 3;
    internal const uint HIGH_DYNAMIC_RANGE_SUPPORTED_2 = 1 << 4;
    internal const uint HIGH_DYNAMIC_RANGE_USER_ENABLED_2 = 1 << 5;
    internal const uint WIDE_COLOR_SUPPORTED_2 = 1 << 6;
    internal const uint WIDE_COLOR_USER_ENABLED_2 = 1 << 7;

    internal const uint DISPLAYCONFIG_ADVANCED_COLOR_MODE_SDR = 0;
    internal const uint DISPLAYCONFIG_ADVANCED_COLOR_MODE_WCG = 1;
    internal const uint DISPLAYCONFIG_ADVANCED_COLOR_MODE_HDR = 2;

    internal const int DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11;
    internal const int DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13;
    internal const int DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL = unchecked((int)0x80000000);

    /// <summary>
    /// Default SDR white level in nits when the display does not report one.
    /// </summary>
    internal const int DefaultSdrWhiteLevelNits = 80;

    #region Imports

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int GetDisplayConfigBufferSizes(
        uint flags,
        uint* numPathArrayElements,
        uint* numModeInfoArrayElements);

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int QueryDisplayConfig(
        uint flags,
        uint* numPathArrayElements,
        DISPLAYCONFIG_PATH_INFO* pathArray,
        uint* numModeInfoArrayElements,
        DISPLAYCONFIG_MODE_INFO* modeInfoArray,
        nint currentTopologyId);

    [LibraryImport(Lib)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    internal static partial int DisplayConfigGetDeviceInfo(DISPLAYCONFIG_DEVICE_INFO_HEADER* requestPacket);

    #endregion

    #region Query

    /// <summary>
    /// Queries all active display paths and returns per-display details keyed by GDI device
    /// name, which is the value reported by MONITORINFOEX.szDevice. Returns an empty map if
    /// the CCD API is unavailable or the query fails.
    /// </summary>
    internal static Dictionary<string, DisplayConfigDetails> QueryActiveDisplays()
    {
        var result = new Dictionary<string, DisplayConfigDetails>(StringComparer.OrdinalIgnoreCase);

        try
        {
            // The topology can change between sizing and querying, so retry a few times.
            for (int attempt = 0; attempt < 3; attempt++)
            {
                uint pathCount = 0;
                uint modeCount = 0;

                if (GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, &pathCount, &modeCount) != ERROR_SUCCESS)
                    return result;

                if (pathCount == 0 || modeCount == 0)
                    return result;

                var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

                int status;
                fixed (DISPLAYCONFIG_PATH_INFO* pathPtr = paths)
                fixed (DISPLAYCONFIG_MODE_INFO* modePtr = modes)
                {
                    status = QueryDisplayConfig(
                        QDC_ONLY_ACTIVE_PATHS, &pathCount, pathPtr, &modeCount, modePtr, 0);
                }

                if (status == ERROR_INSUFFICIENT_BUFFER)
                    continue;

                if (status != ERROR_SUCCESS)
                    return result;

                for (uint i = 0; i < pathCount; i++)
                {
                    var deviceName = GetSourceName(paths[i].sourceInfo.adapterId, paths[i].sourceInfo.id);
                    if (string.IsNullOrEmpty(deviceName) || result.ContainsKey(deviceName))
                        continue;

                    result[deviceName] = BuildDetails(in paths[i], modes, modeCount);
                }

                return result;
            }
        }
        catch (DllNotFoundException)
        {
            // CCD unavailable, caller falls back to GDI-only information.
        }
        catch (EntryPointNotFoundException)
        {
        }

        return result;
    }

    private static string GetSourceName(LUID adapterId, uint sourceId)
    {
        var source = default(DISPLAYCONFIG_SOURCE_DEVICE_NAME);
        source.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
        source.header.size = (uint)sizeof(DISPLAYCONFIG_SOURCE_DEVICE_NAME);
        source.header.adapterId = adapterId;
        source.header.id = sourceId;

        if (DisplayConfigGetDeviceInfo((DISPLAYCONFIG_DEVICE_INFO_HEADER*)&source) != ERROR_SUCCESS)
            return string.Empty;

        return ReadFixedString(source.viewGdiDeviceName, 32);
    }

    private static DisplayConfigDetails BuildDetails(
        in DISPLAYCONFIG_PATH_INFO path,
        DISPLAYCONFIG_MODE_INFO[] modes,
        uint modeCount)
    {
        var adapterId = path.targetInfo.adapterId;
        var targetId = path.targetInfo.id;

        string friendlyName = string.Empty;
        string devicePath = string.Empty;
        int outputTechnology = path.targetInfo.outputTechnology;

        var target = default(DISPLAYCONFIG_TARGET_DEVICE_NAME);
        target.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
        target.header.size = (uint)sizeof(DISPLAYCONFIG_TARGET_DEVICE_NAME);
        target.header.adapterId = adapterId;
        target.header.id = targetId;

        if (DisplayConfigGetDeviceInfo((DISPLAYCONFIG_DEVICE_INFO_HEADER*)&target) == ERROR_SUCCESS)
        {
            friendlyName = ReadFixedString(target.monitorFriendlyDeviceName, 64);
            devicePath = ReadFixedString(target.monitorDevicePath, 128);
            outputTechnology = target.outputTechnology;
        }

        // Prefer the target mode's signal timing, which stays an exact rational (59.94 Hz
        // rather than the 59 that EnumDisplaySettings reports).
        double refreshRate = path.targetInfo.refreshRate.ToDouble();
        uint modeIndex = path.targetInfo.modeInfoIdx;
        if (modeIndex != DISPLAYCONFIG_PATH_MODE_IDX_INVALID && modeIndex < modeCount &&
            modes[modeIndex].infoType == DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
        {
            double signalRate = modes[modeIndex].targetVideoSignalInfo.vSyncFreq.ToDouble();
            if (signalRate > 0)
                refreshRate = signalRate;
        }

        bool hdrCapable = false;
        var colorMode = MonitorColorMode.Sdr;
        int bitsPerColor = 0;

        // Windows 11 24H2+ separates HDR from wide color gamut. Probe for it rather than
        // checking the OS version; older builds fail the request and fall back to type 9,
        // which cannot tell the two apart beyond the wideColorEnforced hint.
        var color2 = default(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2);
        color2.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO_2;
        color2.header.size = (uint)sizeof(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2);
        color2.header.adapterId = adapterId;
        color2.header.id = targetId;

        if (DisplayConfigGetDeviceInfo((DISPLAYCONFIG_DEVICE_INFO_HEADER*)&color2) == ERROR_SUCCESS)
        {
            hdrCapable = (color2.value & HIGH_DYNAMIC_RANGE_SUPPORTED_2) != 0;
            bitsPerColor = (int)color2.bitsPerColorChannel;
            colorMode = color2.activeColorMode switch
            {
                DISPLAYCONFIG_ADVANCED_COLOR_MODE_WCG => MonitorColorMode.WideColorGamut,
                DISPLAYCONFIG_ADVANCED_COLOR_MODE_HDR => MonitorColorMode.Hdr,
                _ => MonitorColorMode.Sdr
            };
        }
        else
        {
            var color = default(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO);
            color.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
            color.header.size = (uint)sizeof(DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO);
            color.header.adapterId = adapterId;
            color.header.id = targetId;

            if (DisplayConfigGetDeviceInfo((DISPLAYCONFIG_DEVICE_INFO_HEADER*)&color) == ERROR_SUCCESS)
            {
                hdrCapable = (color.value & ADVANCED_COLOR_SUPPORTED) != 0;
                bitsPerColor = (int)color.bitsPerColorChannel;

                if ((color.value & ADVANCED_COLOR_ENABLED) != 0)
                {
                    colorMode = (color.value & WIDE_COLOR_ENFORCED) != 0
                        ? MonitorColorMode.WideColorGamut
                        : MonitorColorMode.Hdr;
                }
            }
        }

        int sdrWhiteLevel = DefaultSdrWhiteLevelNits;

        var white = default(DISPLAYCONFIG_SDR_WHITE_LEVEL);
        white.header.type = DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL;
        white.header.size = (uint)sizeof(DISPLAYCONFIG_SDR_WHITE_LEVEL);
        white.header.adapterId = adapterId;
        white.header.id = targetId;

        if (DisplayConfigGetDeviceInfo((DISPLAYCONFIG_DEVICE_INFO_HEADER*)&white) == ERROR_SUCCESS)
            sdrWhiteLevel = (int)(white.SDRWhiteLevel * 80 / 1000);

        return new DisplayConfigDetails
        {
            FriendlyName = friendlyName,
            DevicePath = devicePath,
            RefreshRate = refreshRate,
            IsInternal = IsInternalOutput(outputTechnology),
            IsHdrCapable = hdrCapable,
            ColorMode = colorMode,
            BitsPerColorChannel = bitsPerColor,
            SdrWhiteLevelNits = sdrWhiteLevel
        };
    }

    private static bool IsInternalOutput(int outputTechnology) => outputTechnology switch
    {
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL => true,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED => true,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED => true,
        _ => false
    };

    private static string ReadFixedString(char* buffer, int maxLength)
    {
        var span = new ReadOnlySpan<char>(buffer, maxLength);
        int end = span.IndexOf('\0');
        return end >= 0 ? new string(span[..end]) : new string(span);
    }

    #endregion

    #region Structures

    [StructLayout(LayoutKind.Sequential)]
    internal struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_RATIONAL
    {
        public uint Numerator;
        public uint Denominator;

        public readonly double ToDouble() => Denominator == 0 ? 0 : (double)Numerator / Denominator;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_PATH_SOURCE_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_PATH_TARGET_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public int outputTechnology;
        public uint rotation;
        public uint scaling;
        public DISPLAYCONFIG_RATIONAL refreshRate;
        public uint scanLineOrdering;
        public int targetAvailable;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_PATH_INFO
    {
        public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
        public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
        public uint flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_2DREGION
    {
        public uint cx;
        public uint cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
    {
        public ulong pixelRate;
        public DISPLAYCONFIG_RATIONAL hSyncFreq;
        public DISPLAYCONFIG_RATIONAL vSyncFreq;
        public DISPLAYCONFIG_2DREGION activeSize;
        public DISPLAYCONFIG_2DREGION totalSize;
        public uint videoStandard;
        public uint scanLineOrdering;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_SOURCE_MODE
    {
        public uint width;
        public uint height;
        public uint pixelFormat;
        public int positionX;
        public int positionY;
    }

    /// <summary>
    /// Explicit layout mirrors the native union of target, source and desktop-image modes.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    internal struct DISPLAYCONFIG_MODE_INFO
    {
        [FieldOffset(0)] public uint infoType;
        [FieldOffset(4)] public uint id;
        [FieldOffset(8)] public LUID adapterId;
        [FieldOffset(16)] public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
        [FieldOffset(16)] public DISPLAYCONFIG_SOURCE_MODE sourceMode;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        public uint type;
        public uint size;
        public LUID adapterId;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public fixed char viewGdiDeviceName[32];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_TARGET_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint flags;
        public int outputTechnology;
        public ushort edidManufactureId;
        public ushort edidProductCodeId;
        public uint connectorInstance;
        public fixed char monitorFriendlyDeviceName[64];
        public fixed char monitorDevicePath[128];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_SDR_WHITE_LEVEL
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint SDRWhiteLevel;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;
        public uint colorEncoding;
        public uint bitsPerColorChannel;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO_2
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public uint value;
        public uint colorEncoding;
        public uint bitsPerColorChannel;
        public uint activeColorMode;
    }

    #endregion
}
