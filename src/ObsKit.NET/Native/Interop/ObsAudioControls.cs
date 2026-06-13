using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS audio control functions (obs-audio-controls.h).
/// </summary>
internal static partial class ObsAudioControls
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    /// <summary>
    /// The maximum number of audio channels (MAX_AUDIO_CHANNELS).
    /// </summary>
    internal const int MaxAudioChannels = 8;

    /// <summary>
    /// Callback for volume meter updates. The float pointers reference
    /// MAX_AUDIO_CHANNELS-sized arrays of dB values.
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void VolmeterUpdatedCallback(nint param, nint magnitude, nint peak, nint inputPeak);

    [LibraryImport(Lib, EntryPoint = "obs_volmeter_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_volmeter_create(int faderType);

    [LibraryImport(Lib, EntryPoint = "obs_volmeter_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_volmeter_destroy(nint volmeter);

    internal static bool obs_volmeter_attach_source(nint volmeter, ObsSourceHandle source)
        => obs_volmeter_attach_source_native(volmeter, source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_volmeter_attach_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_volmeter_attach_source_native(nint volmeter, ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_volmeter_detach_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_volmeter_detach_source(nint volmeter);

    [LibraryImport(Lib, EntryPoint = "obs_volmeter_set_peak_meter_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_volmeter_set_peak_meter_type(nint volmeter, int peakMeterType);

    [LibraryImport(Lib, EntryPoint = "obs_volmeter_get_nr_channels")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int obs_volmeter_get_nr_channels(nint volmeter);

    [LibraryImport(Lib, EntryPoint = "obs_volmeter_add_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_volmeter_add_callback(nint volmeter, VolmeterUpdatedCallback callback, nint param);

    [LibraryImport(Lib, EntryPoint = "obs_volmeter_remove_callback")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_volmeter_remove_callback(nint volmeter, VolmeterUpdatedCallback callback, nint param);

    [LibraryImport(Lib, EntryPoint = "obs_mul_to_db")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_mul_to_db(float mul);

    [LibraryImport(Lib, EntryPoint = "obs_db_to_mul")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_db_to_mul(float db);

    // ---- Fader (obs_fader_t) ----

    [LibraryImport(Lib, EntryPoint = "obs_fader_create")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial nint obs_fader_create(int faderType);

    [LibraryImport(Lib, EntryPoint = "obs_fader_destroy")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_fader_destroy(nint fader);

    internal static bool obs_fader_set_db(nint fader, float db) => obs_fader_set_db_native(fader, db) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_fader_set_db")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_fader_set_db_native(nint fader, float db);

    [LibraryImport(Lib, EntryPoint = "obs_fader_get_db")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_fader_get_db(nint fader);

    internal static bool obs_fader_set_deflection(nint fader, float def) => obs_fader_set_deflection_native(fader, def) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_fader_set_deflection")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_fader_set_deflection_native(nint fader, float def);

    [LibraryImport(Lib, EntryPoint = "obs_fader_get_deflection")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_fader_get_deflection(nint fader);

    internal static bool obs_fader_set_mul(nint fader, float mul) => obs_fader_set_mul_native(fader, mul) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_fader_set_mul")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_fader_set_mul_native(nint fader, float mul);

    [LibraryImport(Lib, EntryPoint = "obs_fader_get_mul")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_fader_get_mul(nint fader);

    internal static bool obs_fader_attach_source(nint fader, ObsSourceHandle source)
        => obs_fader_attach_source_native(fader, source) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_fader_attach_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_fader_attach_source_native(nint fader, ObsSourceHandle source);

    [LibraryImport(Lib, EntryPoint = "obs_fader_detach_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_fader_detach_source(nint fader);
}
