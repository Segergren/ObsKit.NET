using ObsKit.NET.Core;

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
        : base(TypeIdForPlatform, name, BuildInitialSettings(deviceId))
    {
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
    /// Enumerates the available video capture devices on the system. This works by creating
    /// a temporary dshow_input source and asking OBS to populate its property list, which is
    /// the same code path the OBS UI uses for the device dropdown.
    /// </summary>
    /// <param name="includeVirtualDevices">
    /// If true, also return virtual / proxy entries that have no DirectShow device path —
    /// e.g. Meta Quest companion app, NVIDIA Broadcast, OBS Virtual Camera. These are usually
    /// not openable in a headless app, so they are excluded by default.
    /// </param>
    public static IReadOnlyList<WebcamDeviceInfo> ListDevices(bool includeVirtualDevices = false)
    {
        // The dshow plugin only populates the device list when an instance exists, so we
        // create a private (un-saved) source just for the property query and dispose it.
        using var probe = Source.CreatePrivate(TypeIdForPlatform, "__obskit_webcam_probe__");
        var items = probe.GetListPropertyItems(VideoDeviceIdKey);

        var result = new List<WebcamDeviceInfo>(items.Count);
        foreach (var (itemName, itemValue) in items)
        {
            if (string.IsNullOrEmpty(itemValue))
                continue;

            // OBS encodes device ids as "Name:Path" (the dshow plugin escapes any literal
            // ':' and '#' inside the components, so the first ':' is always the separator).
            // Entries with an empty path component are virtual / proxy devices (Meta Quest
            // companion app, NVIDIA Broadcast, OBS Virtual Camera, ...). Skip them unless
            // the caller opts in.
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
        // -1 means "any". For a positive FPS, interval = 10_000_000 / fps.
        var frameInterval = fps > 0 ? (long)Math.Round(10_000_000.0 / fps) : -1L;

        Update(s =>
        {
            s.Set("res_type", (long)WebcamResolutionMode.Custom);
            s.Set("resolution", $"{width}x{height}");
            s.Set("frame_interval", frameInterval);
            s.Set("video_format", videoFormat);
        });
        return this;
    }

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
