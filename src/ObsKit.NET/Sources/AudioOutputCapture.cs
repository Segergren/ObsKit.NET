using ObsKit.NET.Core;

namespace ObsKit.NET.Sources;

/// <summary>
/// Represents an audio output (desktop audio) capture source.
/// </summary>
public sealed class AudioOutputCapture : Source
{
    /// <summary>
    /// The source type ID for Windows audio output capture.
    /// </summary>
    public const string WindowsTypeId = "wasapi_output_capture";

    /// <summary>
    /// The source type ID for Linux audio output capture (PulseAudio).
    /// </summary>
    public const string LinuxPulseTypeId = "pulse_output_capture";

    /// <summary>
    /// The source type ID for Linux audio output capture (PipeWire).
    /// </summary>
    public const string LinuxPipeWireTypeId = "pipewire_output_capture";

    /// <summary>
    /// The source type ID for macOS audio output capture.
    /// Note: macOS requires additional plugins for desktop audio capture.
    /// </summary>
    public const string MacOSTypeId = "coreaudio_output_capture";

    /// <summary>
    /// Gets the platform-appropriate source type ID.
    /// </summary>
    public static string TypeIdForPlatform => OperatingSystem.IsWindows() ? WindowsTypeId :
                                               OperatingSystem.IsLinux() ? LinuxPulseTypeId :
                                               OperatingSystem.IsMacOS() ? MacOSTypeId :
                                               WindowsTypeId;

    /// <summary>
    /// Creates an audio output capture source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="deviceId">Optional device ID. Use "default" for default device.</param>
    public AudioOutputCapture(string name, string? deviceId = null)
        : base(TypeIdForPlatform, name)
    {
        if (!string.IsNullOrEmpty(deviceId))
        {
            SetDevice(deviceId);
        }
    }

    /// <summary>
    /// Creates an audio output capture source for the default desktop audio.
    /// </summary>
    /// <param name="name">Optional source name.</param>
    /// <returns>An audio output capture source.</returns>
    public static AudioOutputCapture FromDefault(string? name = null)
    {
        return new AudioOutputCapture(name ?? "Desktop Audio", "default");
    }

    /// <summary>
    /// Creates an audio output capture source for a specific device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="name">Optional source name.</param>
    /// <returns>An audio output capture source.</returns>
    public static AudioOutputCapture FromDevice(string deviceId, string? name = null)
    {
        return new AudioOutputCapture(name ?? "Audio Output", deviceId);
    }

    /// <summary>
    /// Sets the audio device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    public AudioOutputCapture SetDevice(string deviceId)
    {
        Update(s => s.Set("device_id", deviceId));
        return this;
    }

    /// <summary>
    /// Sets whether to use device timing (Windows).
    /// </summary>
    /// <param name="useDeviceTiming">Whether to use device timing.</param>
    public AudioOutputCapture SetUseDeviceTiming(bool useDeviceTiming)
    {
        if (OperatingSystem.IsWindows())
        {
            Update(s => s.Set("use_device_timing", useDeviceTiming));
        }
        return this;
    }
}
