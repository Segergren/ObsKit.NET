namespace ObsKit.NET.Outputs;

/// <summary>
/// Virtual camera output (virtualcam_output).
/// Exposes the OBS canvas as a system camera device that other applications
/// (browsers, conferencing apps) can use.
/// Requires the OBS virtual camera driver to be installed
/// (bundled with OBS Studio on Windows/macOS; v4l2loopback on Linux).
/// </summary>
public sealed class VirtualCameraOutput : Output
{
    /// <summary>
    /// The OBS output type ID for the virtual camera.
    /// </summary>
    public const string SourceTypeId = "virtualcam_output";

    /// <summary>
    /// Creates a virtual camera output. The virtual camera is a raw video output —
    /// no encoders are required; call <see cref="Output.Start"/> to go live.
    /// </summary>
    /// <param name="name">The output name.</param>
    public VirtualCameraOutput(string name = "Virtual Camera")
        : base(SourceTypeId, name)
    {
    }

    /// <summary>
    /// Gets whether the virtual camera output type is available
    /// (the driver/module is installed and loaded).
    /// </summary>
    public static bool IsAvailable()
    {
        return Obs.EnumerateOutputTypes().Contains(SourceTypeId);
    }
}
