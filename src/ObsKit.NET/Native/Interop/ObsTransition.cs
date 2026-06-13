using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Native.Interop;

/// <summary>
/// P/Invoke bindings for OBS transition functions. Transitions are sources
/// (obs_source_t) created with a transition type id, so they share the
/// <see cref="ObsSourceHandle"/> type.
/// </summary>
internal static partial class ObsTransition
{
    private const string Lib = LibraryLoader.ObsLibraryName;

    /// <summary>
    /// Begins a transition to <paramref name="dest"/>. Returns false if a transition
    /// is already in progress.
    /// </summary>
    public static bool obs_transition_start(ObsSourceHandle transition, ObsTransitionMode mode, uint durationMs, ObsSourceHandle dest)
        => obs_transition_start_native(transition, mode, durationMs, dest) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_transition_start")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_transition_start_native(ObsSourceHandle transition, ObsTransitionMode mode, uint durationMs, ObsSourceHandle dest);

    /// <summary>
    /// Sets the transition's current source immediately, without animating.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_set")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_set(ObsSourceHandle transition, ObsSourceHandle source);

    /// <summary>
    /// Clears both sub-sources of the transition.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_clear")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_clear(ObsSourceHandle transition);

    /// <summary>
    /// Gets one of the transition's sub-sources (reference incremented).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_get_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_transition_get_source(ObsSourceHandle transition, ObsTransitionTarget target);

    /// <summary>
    /// Gets the currently active sub-source (reference incremented).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_get_active_source")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsSourceHandle obs_transition_get_active_source(ObsSourceHandle transition);

    /// <summary>
    /// Sets the manual transition position (0.0 to 1.0) when started in manual mode.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_set_manual_time")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_set_manual_time(ObsSourceHandle transition, float t);

    /// <summary>
    /// Sets the torque and clamp used to smooth manual transition movement.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_set_manual_torque")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_set_manual_torque(ObsSourceHandle transition, float torque, float clamp);

    [LibraryImport(Lib, EntryPoint = "obs_transition_set_scale_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_set_scale_type(ObsSourceHandle transition, ObsTransitionScaleType type);

    [LibraryImport(Lib, EntryPoint = "obs_transition_get_scale_type")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial ObsTransitionScaleType obs_transition_get_scale_type(ObsSourceHandle transition);

    [LibraryImport(Lib, EntryPoint = "obs_transition_set_alignment")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_set_alignment(ObsSourceHandle transition, uint alignment);

    [LibraryImport(Lib, EntryPoint = "obs_transition_get_alignment")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial uint obs_transition_get_alignment(ObsSourceHandle transition);

    [LibraryImport(Lib, EntryPoint = "obs_transition_set_size")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_set_size(ObsSourceHandle transition, uint cx, uint cy);

    [LibraryImport(Lib, EntryPoint = "obs_transition_get_size")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_get_size(ObsSourceHandle transition, out uint cx, out uint cy);

    /// <summary>
    /// Returns true while a transition is animating.
    /// </summary>
    public static bool obs_transition_is_active(ObsSourceHandle transition)
        => obs_transition_is_active_native(transition) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_transition_is_active")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_transition_is_active_native(ObsSourceHandle transition);

    /// <summary>
    /// Enables or disables fixed-duration transitions (e.g. stingers/videos).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_enable_fixed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_enable_fixed(ObsSourceHandle transition, [MarshalAs(UnmanagedType.U1)] bool enable, uint durationMs);

    /// <summary>
    /// Returns true if the transition has a fixed duration.
    /// </summary>
    public static bool obs_transition_fixed(ObsSourceHandle transition)
        => obs_transition_fixed_native(transition) != 0;

    [LibraryImport(Lib, EntryPoint = "obs_transition_fixed")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial byte obs_transition_fixed_native(ObsSourceHandle transition);

    /// <summary>
    /// Gets the current transition position (0.0 to 1.0).
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_get_time")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial float obs_transition_get_time(ObsSourceHandle transition);

    /// <summary>
    /// Immediately stops an in-progress transition, snapping to the destination.
    /// </summary>
    [LibraryImport(Lib, EntryPoint = "obs_transition_force_stop")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void obs_transition_force_stop(ObsSourceHandle transition);
}
