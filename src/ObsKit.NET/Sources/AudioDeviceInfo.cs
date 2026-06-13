namespace ObsKit.NET.Sources;

/// <summary>
/// Describes an audio capture device.
/// </summary>
/// <param name="Name">The device's friendly name (for display in a picker).</param>
/// <param name="DeviceId">The device id to pass to <c>SetDevice</c>/<c>FromDevice</c> ("default" for the system default).</param>
public sealed record AudioDeviceInfo(string Name, string DeviceId)
{
    /// <summary>Gets whether this entry refers to the system default device.</summary>
    public bool IsDefault => DeviceId == "default";
}
