using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Sources;

/// <summary>
/// Information about an available webcam / video capture device.
/// </summary>
/// <param name="Name">Friendly device name (e.g. "Logitech BRIO").</param>
/// <param name="DeviceId">The platform-specific device id string used to select this device.</param>
public sealed record WebcamDeviceInfo(string Name, string DeviceId);

/// <summary>
/// Resolution selection mode for a webcam capture.
/// </summary>
public enum WebcamResolutionMode
{
    /// <summary>Let the device pick its preferred default resolution and frame rate.</summary>
    Preferred = 0,
    /// <summary>Use the explicit resolution / frame rate / video format set on the source.</summary>
    Custom = 1,
}

/// <summary>
/// Buffering mode for the dshow plugin.
/// </summary>
public enum WebcamBufferingMode
{
    /// <summary>Let OBS pick (default).</summary>
    Auto = 0,
    /// <summary>Always buffer (smoother but adds latency).</summary>
    On = 1,
    /// <summary>Never buffer (lowest latency, may drop frames).</summary>
    Off = 2,
}

/// <summary>
/// Represents a video capture device (webcam) source. On Windows this wraps the
/// DirectShow plugin (<c>dshow_input</c>); on Linux it uses V4L2; on macOS AVFoundation.
/// </summary>
public sealed class WebcamCapture : Source
{
    /// <summary>Windows source type id (DirectShow).</summary>
    public const string WindowsTypeId = "dshow_input";

    /// <summary>Linux source type id (V4L2).</summary>
    public const string LinuxTypeId = "v4l2_input";

    /// <summary>macOS source type id (AVFoundation).</summary>
    public const string MacOSTypeId = "av_capture_input";

    /// <summary>The platform-appropriate source type id.</summary>
    public static string TypeIdForPlatform => OperatingSystem.IsWindows() ? WindowsTypeId :
                                              OperatingSystem.IsLinux() ? LinuxTypeId :
                                              OperatingSystem.IsMacOS() ? MacOSTypeId :
                                              WindowsTypeId;

    /// <summary>The dshow property key used to populate the device list.</summary>
    private const string VideoDeviceIdKey = "video_device_id";

    /// <summary>
    /// Creates a webcam capture source. If <paramref name="deviceId"/> is null, the source is
    /// created with no device selected; use <see cref="SetDevice(string)"/> after picking one.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="deviceId">The device id (from <see cref="ListDevices"/>) or null.</param>
    public WebcamCapture(string name, string? deviceId = null)
        : base(Create(name, deviceId), TypeIdForPlatform, ownsHandle: true)
    {
    }

    private static ObsSourceHandle Create(string name, string? deviceId)
    {
        ThrowIfNotInitialized();
        // obs_source_create takes its own reference to the settings (obs_data_addref), so we must
        // dispose our create-time reference; otherwise it leaks until finalization.
        using var settings = BuildInitialSettings(deviceId);
        var handle = ObsSource.obs_source_create(TypeIdForPlatform, name, settings.Handle, default);
        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create source of type '{TypeIdForPlatform}'");
        return handle;
    }

    /// <summary>
    /// Creates a webcam capture source for the first available device.
    /// Returns null if no capture devices are present on the system.
    /// </summary>
    /// <param name="name">Optional source name.</param>
    /// <param name="includeVirtualDevices">
    /// If true, also consider virtual / proxy entries that have no DirectShow device path
    /// (Meta Quest companion app, NVIDIA Broadcast, OBS Virtual Camera, etc.). Default false.
    /// </param>
    public static WebcamCapture? FromDefault(string? name = null, bool includeVirtualDevices = false)
    {
        var device = ListDevices(includeVirtualDevices).FirstOrDefault();
        if (device == null)
            return null;
        return new WebcamCapture(name ?? device.Name, device.DeviceId);
    }

    /// <summary>
    /// Creates a webcam capture source for the device whose friendly name contains the given
    /// substring (case-insensitive). Useful for selecting "Logitech 4K", "BRIO", "C920", etc.
    /// </summary>
    /// <param name="nameSubstring">Substring to match against device names.</param>
    /// <param name="name">Optional source name; defaults to the matched device's name.</param>
    /// <param name="includeVirtualDevices">
    /// If true, virtual / proxy entries (Meta Quest, NVIDIA Broadcast, OBS Virtual Camera, ...)
    /// are also eligible for matching. Default false.
    /// </param>
    /// <returns>A configured WebcamCapture, or null if no device matches.</returns>
    public static WebcamCapture? FromDeviceName(string nameSubstring, string? name = null, bool includeVirtualDevices = false)
    {
        var device = ListDevices(includeVirtualDevices).FirstOrDefault(d =>
            d.Name.Contains(nameSubstring, StringComparison.OrdinalIgnoreCase));
        if (device == null)
            return null;
        return new WebcamCapture(name ?? device.Name, device.DeviceId);
    }

    /// <summary>
    /// Enumerates the available video capture devices on the system. On Windows this reads
    /// the DirectShow device monikers via <c>ICreateDevEnum</c> / <c>IPropertyBag</c>, which
    /// only touches the registry-backed moniker — it never calls <c>CoCreateInstance</c> on
    /// the underlying filter. That avoids loading third-party DShow filter DLLs (e.g. Meta
    /// Quest's <c>magicdsfilterQuest3.dll</c>) that crash when instantiated headlessly.
    /// On Linux/macOS it falls back to the OBS property-list probe, which is safe there.
    /// </summary>
    /// <param name="includeVirtualDevices">
    /// If true, also return virtual / proxy entries that have no DirectShow device path —
    /// e.g. Meta Quest companion app, NVIDIA Broadcast, OBS Virtual Camera. These are usually
    /// not openable in a headless app, so they are excluded by default.
    /// </param>
    public static IReadOnlyList<WebcamDeviceInfo> ListDevices(bool includeVirtualDevices = false)
    {
        if (OperatingSystem.IsWindows())
            return ListDevicesWindows(includeVirtualDevices);

        return ListDevicesViaObsProbe(includeVirtualDevices);
    }

    // Fallback path used on Linux/macOS. On Windows this code is unsafe in the presence of
    // buggy third-party DShow filters because the dshow plugin CoCreates each filter when
    // populating its property list.
    private static IReadOnlyList<WebcamDeviceInfo> ListDevicesViaObsProbe(bool includeVirtualDevices)
    {
        using var probe = Source.CreatePrivate(TypeIdForPlatform, "__obskit_webcam_probe__");
        var items = probe.GetListPropertyItems(VideoDeviceIdKey);

        var result = new List<WebcamDeviceInfo>(items.Count);
        foreach (var (itemName, itemValue) in items)
        {
            if (string.IsNullOrEmpty(itemValue))
                continue;

            if (!includeVirtualDevices && OperatingSystem.IsWindows())
            {
                var sep = itemValue.IndexOf(':');
                if (sep < 0 || sep == itemValue.Length - 1)
                    continue;
            }

            result.Add(new WebcamDeviceInfo(itemName, itemValue));
        }
        return result;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static IReadOnlyList<WebcamDeviceInfo> ListDevicesWindows(bool includeVirtualDevices)
    {
        var result = new List<WebcamDeviceInfo>();
        object? devEnumObj = null;
        IEnumMoniker? enumMoniker = null;

        try
        {
            var sysDeviceEnumType = Type.GetTypeFromCLSID(CLSID_SystemDeviceEnum, throwOnError: false);
            if (sysDeviceEnumType == null)
                return result;

            devEnumObj = Activator.CreateInstance(sysDeviceEnumType);
            if (devEnumObj is not ICreateDevEnum devEnum)
                return result;

            int hr = devEnum.CreateClassEnumerator(CLSID_VideoInputDeviceCategory, out enumMoniker, 0);
            // S_FALSE (1) means the category is empty.
            if (hr != 0 || enumMoniker == null)
                return result;

            var monikers = new IMoniker[1];
            while (enumMoniker.Next(1, monikers, IntPtr.Zero) == 0)
            {
                var moniker = monikers[0];
                if (moniker == null)
                    continue;

                try
                {
                    string? name = ReadStringProperty(moniker, "FriendlyName");
                    if (string.IsNullOrEmpty(name))
                        name = ReadStringProperty(moniker, "Description");
                    if (string.IsNullOrEmpty(name))
                        continue;

                    string? path = ReadStringProperty(moniker, "DevicePath") ?? string.Empty;

                    if (!includeVirtualDevices && string.IsNullOrEmpty(path))
                        continue;

                    string deviceId = EncodeDeviceId(name!, path);
                    result.Add(new WebcamDeviceInfo(name!, deviceId));
                }
                finally
                {
                    Marshal.ReleaseComObject(moniker);
                }
            }
        }
        catch
        {
            // Swallow — enumeration must never throw past this boundary. Callers can deal
            // with an empty list (e.g. show a "no devices found" UI) but a thrown exception
            // would propagate up through the OBS thread and look like a crash.
        }
        finally
        {
            if (enumMoniker != null)
                Marshal.ReleaseComObject(enumMoniker);
            if (devEnumObj != null)
                Marshal.ReleaseComObject(devEnumObj);
        }

        return result;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static string? ReadStringProperty(IMoniker moniker, string propertyName)
    {
        object? bagObj = null;
        try
        {
            var bagGuid = IID_IPropertyBag;
            moniker.BindToStorage(null!, null!, ref bagGuid, out bagObj);
            if (bagObj is not IPropertyBag bag)
                return null;

            object? value = null;
            int hr = bag.Read(propertyName, ref value, IntPtr.Zero);
            if (hr != 0 || value == null)
                return null;
            return value.ToString();
        }
        catch
        {
            return null;
        }
        finally
        {
            if (bagObj != null)
                Marshal.ReleaseComObject(bagObj);
        }
    }

    // Mirror of obs-studio's win-dshow/encode-dstr.hpp::encode_dstr. The OBS dshow plugin
    // produces device ids of the form "{name}:{path}" with '#' replaced by "#22" and ':'
    // by "#3A" inside each component, so we must produce the exact same string for the
    // dshow plugin to recognise the device when we later assign it via SetDevice().
    private static string EncodeDeviceId(string name, string path)
    {
        string encName = EncodeDshowComponent(name);
        string encPath = EncodeDshowComponent(path);
        return $"{encName}:{encPath}";
    }

    private static string EncodeDshowComponent(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        return s.Replace("#", "#22").Replace(":", "#3A");
    }

    private static readonly Guid CLSID_SystemDeviceEnum = new("62BE5D10-60EB-11d0-BD3B-00A0C911CE86");
    private static readonly Guid CLSID_VideoInputDeviceCategory = new("860BB310-5D01-11d0-BD3B-00A0C911CE86");
    private static readonly Guid IID_IPropertyBag = new("55272A00-42CB-11CE-8135-00AA004BB851");

    [ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ICreateDevEnum
    {
        [PreserveSig]
        int CreateClassEnumerator([In] in Guid clsidDeviceClass, out IEnumMoniker? ppEnumMoniker, [In] int dwFlags);
    }

    [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPropertyBag
    {
        [PreserveSig]
        int Read([MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In, Out, MarshalAs(UnmanagedType.Struct)] ref object? pVar, IntPtr pErrorLog);

        [PreserveSig]
        int Write([MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
    }

    private static Settings BuildInitialSettings(string? deviceId)
    {
        var settings = new Settings();

        if (OperatingSystem.IsWindows())
        {
            if (!string.IsNullOrEmpty(deviceId))
                settings.Set(VideoDeviceIdKey, deviceId);

            // Preferred resolution/format/fps — let the device pick what it advertises.
            // (User can switch to Custom later via SetCustomResolution.)
            settings.Set("res_type", (long)WebcamResolutionMode.Preferred);

            // Default to capturing audio (some webcams have built-in mics).
            settings.Set("audio_output_mode", 0L);

            // We're running headless — the source should keep capturing even when no preview
            // is rendering. The dshow plugin's default is false, but we force it explicitly.
            settings.Set("deactivate_when_not_showing", false);

            // Use hardware decode for compressed formats (MJPG / H264) when the device offers them.
            settings.Set("hw_decode", true);

            // active=true ensures the capture thread actually starts. dshow defaults this to
            // true already but make it explicit for clarity.
            settings.Set("active", true);
        }
        else if (OperatingSystem.IsLinux())
        {
            if (!string.IsNullOrEmpty(deviceId))
                settings.Set("device_id", deviceId);
        }
        else if (OperatingSystem.IsMacOS())
        {
            if (!string.IsNullOrEmpty(deviceId))
                settings.Set("device", deviceId);
        }

        return settings;
    }

    /// <summary>Selects the video capture device by its platform-specific id.</summary>
    public WebcamCapture SetDevice(string deviceId)
    {
        Update(s =>
        {
            if (OperatingSystem.IsWindows())
                s.Set(VideoDeviceIdKey, deviceId);
            else if (OperatingSystem.IsLinux())
                s.Set("device_id", deviceId);
            else if (OperatingSystem.IsMacOS())
                s.Set("device", deviceId);
        });
        return this;
    }

    /// <summary>Selects the video capture device by friendly-name substring match.</summary>
    /// <returns>True if a matching device was found.</returns>
    public bool TrySetDeviceByName(string nameSubstring)
    {
        var device = ListDevices().FirstOrDefault(d =>
            d.Name.Contains(nameSubstring, StringComparison.OrdinalIgnoreCase));
        if (device == null)
            return false;
        SetDevice(device.DeviceId);
        return true;
    }

    /// <summary>
    /// Forces a custom resolution / frame rate. Pass an FPS like 30 or 60; <paramref name="videoFormat"/>
    /// is the dshow format name ("Any", "YUY2", "NV12", "MJPEG", "H264", ...).
    /// </summary>
    public WebcamCapture SetCustomResolution(int width, int height, double fps, string videoFormat = "Any")
    {
        if (!OperatingSystem.IsWindows())
            return this;

        // frame_interval is in 100-nanosecond units (DirectShow REFERENCE_TIME).
        // win-dshow sentinels: -1 (FPS_MATCHING) = match the OBS canvas/output FPS;
        // 0 (FPS_HIGHEST) = the device's highest available rate. We default fps<=0 to
        // FPS_MATCHING (the plugin's own default). For a positive FPS, interval = 10_000_000 / fps.
        var frameInterval = fps > 0 ? (long)Math.Round(10_000_000.0 / fps) : -1L;

        Update(s =>
        {
            s.Set("res_type", (long)WebcamResolutionMode.Custom);
            s.Set("resolution", $"{width}x{height}");
            s.Set("frame_interval", frameInterval);
            // video_format is an int enum; map the format name to its numeric value.
            s.Set("video_format", MapDshowVideoFormat(videoFormat));
        });
        return this;
    }

    // Maps a dshow video format name to its libdshowcapture VideoFormat enum value.
    private static long MapDshowVideoFormat(string? name) => (name ?? "Any").Trim().ToUpperInvariant() switch
    {
        "ANY" => 0,
        "ARGB" => 100, "XRGB" => 101, "RGB24" => 102,
        "I420" => 200, "NV12" => 201, "YV12" => 202, "Y800" => 203, "P010" => 204,
        "YVYU" => 300, "YUY2" => 301, "UYVY" => 302, "HDYC" => 303,
        "MJPEG" => 400, "H264" => 401, "HEVC" => 402,
        _ => 0, // unknown -> Any (matches any device format)
    };

    /// <summary>Reverts to the device's preferred (default) resolution and frame rate.</summary>
    public WebcamCapture UsePreferredResolution()
    {
        if (OperatingSystem.IsWindows())
            Update(s => s.Set("res_type", (long)WebcamResolutionMode.Preferred));
        return this;
    }

    /// <summary>Sets the buffering mode (Windows only).</summary>
    public WebcamCapture SetBuffering(WebcamBufferingMode mode)
    {
        if (OperatingSystem.IsWindows())
            Update(s => s.Set("buffering", (long)mode));
        return this;
    }

    /// <summary>Enables or disables hardware decoding for compressed feeds (Windows only).</summary>
    public WebcamCapture SetHardwareDecode(bool enabled)
    {
        if (OperatingSystem.IsWindows())
            Update(s => s.Set("hw_decode", enabled));
        return this;
    }

    /// <summary>Flips the image vertically (Windows only).</summary>
    public WebcamCapture SetFlipVertically(bool flip)
    {
        if (OperatingSystem.IsWindows())
            Update(s => s.Set("flip_vertically", flip));
        return this;
    }

    /// <summary>Enables/disables auto-rotation based on device orientation (Windows only).</summary>
    public WebcamCapture SetAutoRotation(bool enabled)
    {
        if (OperatingSystem.IsWindows())
            Update(s => s.Set("autorotation", enabled));
        return this;
    }

    /// <summary>
    /// Sets the color space ("default", "709", "601", "2100PQ", "2100HLG"). Windows only.
    /// </summary>
    public WebcamCapture SetColorSpace(string colorSpace)
    {
        if (OperatingSystem.IsWindows())
            Update(s => s.Set("color_space", colorSpace));
        return this;
    }

    /// <summary>
    /// Sets the color range ("default", "partial", "full"). Windows only.
    /// </summary>
    public WebcamCapture SetColorRange(string colorRange)
    {
        if (OperatingSystem.IsWindows())
            Update(s => s.Set("color_range", colorRange));
        return this;
    }

    /// <summary>
    /// Activates or deactivates the capture. Equivalent to the on/off button in the OBS UI.
    /// Defaults to true on creation.
    /// </summary>
    public WebcamCapture SetActive(bool active)
    {
        if (OperatingSystem.IsWindows())
            Update(s => s.Set("active", active));
        return this;
    }
}
