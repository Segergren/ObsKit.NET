namespace ObsKit.NET.Exceptions;

/// <summary>
/// Base exception for all OBS-related errors.
/// </summary>
public class ObsException : Exception
{
    public ObsException() { }

    public ObsException(string message) : base(message) { }

    public ObsException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when OBS initialization fails.
/// </summary>
public class ObsInitializationException : ObsException
{
    public ObsInitializationException() : base("Failed to initialize OBS") { }

    public ObsInitializationException(string message) : base(message) { }

    public ObsInitializationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when video reset fails.
/// </summary>
public class ObsVideoResetException : ObsException
{
    /// <summary>
    /// The error code returned by obs_reset_video.
    /// </summary>
    public int ErrorCode { get; }

    public ObsVideoResetException(int errorCode)
        : base(GetMessage(errorCode))
    {
        ErrorCode = errorCode;
    }

    private static string GetMessage(int errorCode) => errorCode switch
    {
        -1 => "Video reset failed: General failure",
        -2 => "Video reset failed: Graphics module not supported",
        -3 => "Video reset failed: Invalid parameters",
        -4 => "Video reset failed: Video currently active",
        -5 => "Video reset failed: Graphics module not found",
        _ => $"Video reset failed with error code: {errorCode}"
    };
}

/// <summary>
/// Exception thrown when audio reset fails.
/// </summary>
public class ObsAudioResetException : ObsException
{
    public ObsAudioResetException() : base("Failed to reset audio") { }

    public ObsAudioResetException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when a source cannot be created.
/// </summary>
public class ObsSourceException : ObsException
{
    /// <summary>
    /// The source type ID that failed to create.
    /// </summary>
    public string? SourceTypeId { get; }

    public ObsSourceException(string message) : base(message) { }

    public ObsSourceException(string message, string sourceTypeId) : base(message)
    {
        SourceTypeId = sourceTypeId;
    }
}

/// <summary>
/// Exception thrown when an output operation fails.
/// </summary>
public class ObsOutputException : ObsException
{
    /// <summary>
    /// The last error message from the output, if available.
    /// </summary>
    public string? LastError { get; }

    public ObsOutputException(string message) : base(message) { }

    public ObsOutputException(string message, string? lastError) : base(message)
    {
        LastError = lastError;
    }
}

/// <summary>
/// Exception thrown when an encoder cannot be created.
/// </summary>
public class ObsEncoderException : ObsException
{
    /// <summary>
    /// The encoder type ID that failed.
    /// </summary>
    public string? EncoderTypeId { get; }

    public ObsEncoderException(string message) : base(message) { }

    public ObsEncoderException(string message, string encoderTypeId) : base(message)
    {
        EncoderTypeId = encoderTypeId;
    }
}

/// <summary>
/// Exception thrown when OBS is not initialized but an operation requires it.
/// </summary>
public class ObsNotInitializedException : ObsException
{
    public ObsNotInitializedException()
        : base("OBS is not initialized. Call Obs.Initialize() first.") { }
}
