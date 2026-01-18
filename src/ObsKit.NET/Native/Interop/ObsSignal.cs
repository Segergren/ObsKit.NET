using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS signal handler functions.
/// </summary>
internal static partial class ObsSignal
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    #region Signal Handler

    /// <summary>
    /// Callback for signal handlers.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SignalCallback(nint data, nint calldata);

    /// <summary>
    /// Connects a callback to a signal.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "signal_handler_connect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void signal_handler_connect(
        SignalHandlerHandle handler,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string signal,
        SignalCallback callback,
        nint data);

    /// <summary>
    /// Connects a callback to a signal (reference version).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "signal_handler_connect_ref")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void signal_handler_connect_ref(
        SignalHandlerHandle handler,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string signal,
        SignalCallback callback,
        nint data);

    /// <summary>
    /// Disconnects a callback from a signal.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "signal_handler_disconnect")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void signal_handler_disconnect(
        SignalHandlerHandle handler,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string signal,
        SignalCallback callback,
        nint data);

    #endregion

    #region Calldata

    /// <summary>
    /// Gets raw data from calldata. This is the core function that other helpers use.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "calldata_get_data")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte calldata_get_data_native(
        nint calldata,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        nint outPtr,
        nuint size);

    /// <summary>
    /// Gets an integer from calldata.
    /// </summary>
    internal static bool calldata_get_int(nint calldata, string name, out long value)
    {
        value = 0;
        unsafe
        {
            fixed (long* ptr = &value)
            {
                return calldata_get_data_native(calldata, name, (nint)ptr, (nuint)sizeof(long)) != 0;
            }
        }
    }

    /// <summary>
    /// Gets a float from calldata.
    /// </summary>
    internal static bool calldata_get_float(nint calldata, string name, out double value)
    {
        value = 0;
        unsafe
        {
            fixed (double* ptr = &value)
            {
                return calldata_get_data_native(calldata, name, (nint)ptr, (nuint)sizeof(double)) != 0;
            }
        }
    }

    /// <summary>
    /// Gets a bool from calldata.
    /// </summary>
    internal static bool calldata_get_bool(nint calldata, string name, out bool value)
    {
        value = false;
        unsafe
        {
            byte byteVal = 0;
            var result = calldata_get_data_native(calldata, name, (nint)(&byteVal), (nuint)sizeof(byte)) != 0;
            value = byteVal != 0;
            return result;
        }
    }

    /// <summary>
    /// Gets a pointer from calldata.
    /// </summary>
    internal static bool calldata_get_ptr(nint calldata, string name, out nint value)
    {
        value = nint.Zero;
        unsafe
        {
            fixed (nint* ptr = &value)
            {
                return calldata_get_data_native(calldata, name, (nint)ptr, (nuint)sizeof(nint)) != 0;
            }
        }
    }

    /// <summary>
    /// Gets a string from calldata.
    /// </summary>
    internal static bool calldata_get_string(nint calldata, string name, out nint strPtr)
    {
        strPtr = nint.Zero;
        unsafe
        {
            fixed (nint* ptr = &strPtr)
            {
                return calldata_get_string_native(calldata, name, (nint)ptr) != 0;
            }
        }
    }

    [LibraryImport(Lib, EntryPoint = "calldata_get_string")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte calldata_get_string_native(
        nint calldata,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        nint strPtrPtr);

    #endregion

    #region Global Signal Handler

    /// <summary>
    /// Gets the global OBS signal handler.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_get_signal_handler")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SignalHandlerHandle obs_get_signal_handler();

    #endregion

    #region Proc Handler

    /// <summary>
    /// Calls a procedure on a proc handler.
    /// </summary>
    public static bool proc_handler_call(nint handler, string name, nint calldata = 0)
        => proc_handler_call_native(handler, name, calldata) != 0;

    [LibraryImport(Lib, EntryPoint = "proc_handler_call")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte proc_handler_call_native(
        nint handler,
        [MarshalUsing(typeof(Utf8StringMarshaler))] string name,
        nint calldata);

    #endregion
}
