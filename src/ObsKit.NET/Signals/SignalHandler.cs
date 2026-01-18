using System.Runtime.InteropServices;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Signals;

/// <summary>
/// Delegate for OBS signal callbacks.
/// </summary>
/// <param name="calldata">The calldata pointer containing signal parameters.</param>
public delegate void SignalCallback(nint calldata);

/// <summary>
/// Provides methods to extract data from OBS signal calldata.
/// </summary>
public static class Calldata
{
    /// <summary>
    /// Gets a string value from calldata.
    /// </summary>
    /// <param name="calldata">The calldata pointer.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>The string value, or null if not found.</returns>
    public static string? GetString(nint calldata, string name)
    {
        var ptr = ObsSignal.calldata_get_string(calldata, name);
        return ptr == nint.Zero ? null : Marshal.PtrToStringUTF8(ptr);
    }

    /// <summary>
    /// Gets an integer value from calldata.
    /// </summary>
    /// <param name="calldata">The calldata pointer.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>The integer value.</returns>
    public static long GetInt(nint calldata, string name)
        => ObsSignal.calldata_get_int(calldata, name);

    /// <summary>
    /// Gets a float value from calldata.
    /// </summary>
    /// <param name="calldata">The calldata pointer.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>The float value.</returns>
    public static double GetFloat(nint calldata, string name)
        => ObsSignal.calldata_get_float(calldata, name);

    /// <summary>
    /// Gets a boolean value from calldata.
    /// </summary>
    /// <param name="calldata">The calldata pointer.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>The boolean value.</returns>
    public static bool GetBool(nint calldata, string name)
        => ObsSignal.calldata_get_bool(calldata, name);

    /// <summary>
    /// Gets a pointer value from calldata.
    /// </summary>
    /// <param name="calldata">The calldata pointer.</param>
    /// <param name="name">The parameter name.</param>
    /// <returns>The pointer value.</returns>
    public static nint GetPointer(nint calldata, string name)
        => ObsSignal.calldata_get_ptr(calldata, name);
}

/// <summary>
/// Manages signal connections for an OBS object.
/// </summary>
public sealed class SignalConnection : IDisposable
{
    private readonly SignalHandlerHandle _signalHandler;
    private readonly string _signal;
    private readonly ObsSignal.SignalCallback _nativeCallback;
    private readonly SignalCallback _userCallback;
    private bool _disposed;

    internal SignalConnection(SignalHandlerHandle signalHandler, string signal, SignalCallback callback)
    {
        _signalHandler = signalHandler;
        _signal = signal;
        _userCallback = callback;

        // Create a native callback that wraps the user's callback
        _nativeCallback = (data, calldata) =>
        {
            try
            {
                _userCallback(calldata);
            }
            catch
            {
                // Don't let exceptions escape from callbacks
            }
        };

        ObsSignal.signal_handler_connect(_signalHandler, _signal, _nativeCallback, nint.Zero);
    }

    /// <summary>
    /// Disconnects the signal handler.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        ObsSignal.signal_handler_disconnect(_signalHandler, _signal, _nativeCallback, nint.Zero);
        _disposed = true;
    }
}
