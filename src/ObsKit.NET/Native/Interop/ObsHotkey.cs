using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using ObsKit.NET.Native.Marshalling;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS hotkey enumeration and triggering.
/// </summary>
internal static partial class ObsHotkey
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    /// <summary>
    /// Callback for enumerating hotkeys. Return 0 to stop enumerating.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte EnumHotkeyCallback(nint data, nuint id, nint key);

    /// <summary>
    /// Enumerates all registered hotkeys.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_enum_hotkeys")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_enum_hotkeys(EnumHotkeyCallback callback, nint data);

    /// <summary>
    /// Gets the internal name of a hotkey (e.g. "hotkey_start").
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_hotkey_get_name(nint key);

    /// <summary>
    /// Gets the localized description of a hotkey.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_description")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalUsing(typeof(Utf8StringMarshalerNoFree))]
    internal static partial string? obs_hotkey_get_description(nint key);

    /// <summary>
    /// Gets what kind of object registered the hotkey (frontend/source/output/encoder/service).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_registerer_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_hotkey_get_registerer_type(nint key);

    /// <summary>
    /// Gets a weak reference pointer to the object that registered the hotkey.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_get_registerer")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_hotkey_get_registerer(nint key);

    /// <summary>
    /// Invokes a hotkey's registered callback by id. Only effective while
    /// callback rerouting is enabled.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_trigger_routed_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_trigger_routed_callback(nuint id, byte pressed);

    /// <summary>
    /// Enables routing of hotkey callbacks through obs_hotkey_trigger_routed_callback.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_hotkey_enable_callback_rerouting")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_hotkey_enable_callback_rerouting(byte enable);

    /// <summary>
    /// Gets a strong source reference from a weak source reference (for hotkey registerers).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_weak_source_get_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial Types.ObsSourceHandle obs_weak_source_get_source(nint weak);
}
