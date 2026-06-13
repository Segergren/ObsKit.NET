namespace ObsKit.NET.Outputs;

/// <summary>
/// Output stop/error codes (OBS_OUTPUT_* from libobs/obs-defs.h).
/// Delivered by the output "stop" signal and <see cref="Output.Stopped"/>.
/// </summary>
public enum ObsOutputStopCode
{
    /// <summary>The output stopped successfully (OBS_OUTPUT_SUCCESS).</summary>
    Success = 0,

    /// <summary>The specified path or connection URL was invalid (OBS_OUTPUT_BAD_PATH).</summary>
    BadPath = -1,

    /// <summary>Failed to connect to a server (OBS_OUTPUT_CONNECT_FAILED).</summary>
    ConnectFailed = -2,

    /// <summary>Invalid stream path/key (OBS_OUTPUT_INVALID_STREAM).</summary>
    InvalidStream = -3,

    /// <summary>Generic error (OBS_OUTPUT_ERROR).</summary>
    Error = -4,

    /// <summary>Unexpectedly disconnected from the server (OBS_OUTPUT_DISCONNECTED).</summary>
    Disconnected = -5,

    /// <summary>The settings, encoder, or format are not supported by this output (OBS_OUTPUT_UNSUPPORTED).</summary>
    Unsupported = -6,

    /// <summary>The device or disk ran out of space (OBS_OUTPUT_NO_SPACE).</summary>
    NoSpace = -7,

    /// <summary>An encoder error occurred while streaming or recording (OBS_OUTPUT_ENCODE_ERROR).</summary>
    EncodeError = -8,

    /// <summary>The service requires HDR to be disabled (OBS_OUTPUT_HDR_DISABLED).</summary>
    HdrDisabled = -9
}

/// <summary>
/// Event data for <see cref="Output.Stopped"/>.
/// </summary>
public sealed class OutputStoppedEventArgs : EventArgs
{
    internal OutputStoppedEventArgs(ObsOutputStopCode code, string? lastError)
    {
        Code = code;
        LastError = lastError;
    }

    /// <summary>
    /// Gets the stop code reported by the output.
    /// </summary>
    public ObsOutputStopCode Code { get; }

    /// <summary>
    /// Gets the last error message from the output or its encoders, if any.
    /// </summary>
    public string? LastError { get; }

    /// <summary>
    /// Gets whether the output stopped normally.
    /// </summary>
    public bool IsSuccess => Code == ObsOutputStopCode.Success;
}
