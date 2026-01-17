using ObsKit.NET.Core;

namespace ObsKit.NET.Sources;

/// <summary>
/// Represents an audio input (microphone) capture source.
/// </summary>
public sealed class AudioInputCapture : Source
{
    /// <summary>
    /// The source type ID for Windows audio input capture.
    /// </summary>
    public const string WindowsTypeId = "wasapi_input_capture";

    /// <summary>
    /// The source type ID for Linux audio input capture (PulseAudio).
    /// </summary>
    public const string LinuxPulseTypeId = "pulse_input_capture";

    /// <summary>
    /// The source type ID for Linux audio input capture (PipeWire).
    /// </summary>
    public const string LinuxPipeWireTypeId = "pipewire_input_capture";

    /// <summary>
    /// The source type ID for macOS audio input capture.
    /// </summary>
    public const string MacOSTypeId = "coreaudio_input_capture";

    /// <summary>
    /// Gets the platform-appropriate source type ID.
    /// </summary>
    public static string TypeIdForPlatform => OperatingSystem.IsWindows() ? WindowsTypeId :
                                               OperatingSystem.IsLinux() ? LinuxPulseTypeId :
                                               OperatingSystem.IsMacOS() ? MacOSTypeId :
                                               WindowsTypeId;

    /// <summary>
    /// Creates an audio input capture source.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <param name="deviceId">Optional device ID. Use "default" for default device.</param>
    public AudioInputCapture(string name, string? deviceId = null)
        : base(TypeIdForPlatform, name)
    {
        if (!string.IsNullOrEmpty(deviceId))
        {
            SetDevice(deviceId);
        }
    }

    /// <summary>
    /// Creates an audio input capture source for the default microphone.
    /// </summary>
    /// <param name="name">Optional source name.</param>
    /// <returns>An audio input capture source.</returns>
    public static AudioInputCapture FromDefault(string? name = null)
    {
        return new AudioInputCapture(name ?? "Microphone", "default");
    }

    /// <summary>
    /// Creates an audio input capture source for a specific device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="name">Optional source name.</param>
    /// <returns>An audio input capture source.</returns>
    public static AudioInputCapture FromDevice(string deviceId, string? name = null)
    {
        return new AudioInputCapture(name ?? "Audio Input", deviceId);
    }

    /// <summary>
    /// Sets the audio device.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    public AudioInputCapture SetDevice(string deviceId)
    {
        Update(s => s.Set("device_id", deviceId));
        return this;
    }

    /// <summary>
    /// Sets whether to use device timing (Windows).
    /// </summary>
    /// <param name="useDeviceTiming">Whether to use device timing.</param>
    public AudioInputCapture SetUseDeviceTiming(bool useDeviceTiming)
    {
        if (OperatingSystem.IsWindows())
        {
            Update(s => s.Set("use_device_timing", useDeviceTiming));
        }
        return this;
    }
}
