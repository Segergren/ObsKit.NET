using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

namespace ObsKit.NET.Sources;

/// <summary>
/// Represents an OBS transition (a source created with a transition type id, e.g.
/// <see cref="ObsKit.NET.Scenes.TransitionTypes.Fade"/>).
///
/// <para>A transition blends between two sub-sources. To use one as the program
/// transition, assign it to an output channel with
/// <see cref="ObsKit.NET.Obs.SetOutputSource"/>, seed the starting scene with
/// <see cref="Set(Scenes.Scene?)"/>, then animate to a new scene with
/// <see cref="Start(Scenes.Scene, System.TimeSpan, Native.Types.ObsTransitionMode)"/>.</para>
/// </summary>
public sealed class Transition : Source
{
    /// <summary>
    /// Creates a transition source.
    /// </summary>
    /// <param name="typeId">The transition type id (see <see cref="ObsKit.NET.Scenes.TransitionTypes"/>).</param>
    /// <param name="name">The transition name.</param>
    /// <param name="settings">Optional settings for the transition.</param>
    public Transition(string typeId, string name, Settings? settings = null)
        : base(Create(typeId, name, settings), typeId, ownsHandle: true)
    {
    }

    private static ObsSourceHandle Create(string typeId, string name, Settings? settings)
    {
        ThrowIfNotInitialized();
        var handle = ObsSource.obs_source_create_private(typeId, name, settings?.Handle ?? default);
        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create transition of type '{typeId}'");
        return handle;
    }

    /// <summary>Creates a cross-fade transition.</summary>
    public static Transition Fade(string name = "Fade") => new(Scenes.TransitionTypes.Fade, name);

    /// <summary>Creates an instant-cut transition.</summary>
    public static Transition Cut(string name = "Cut") => new(Scenes.TransitionTypes.Cut, name);

    /// <summary>Creates a slide transition.</summary>
    public static Transition Slide(string name = "Slide") => new(Scenes.TransitionTypes.Slide, name);

    /// <summary>
    /// Begins animating from the current source to <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">The source to transition to.</param>
    /// <param name="duration">Animation duration (ignored for fixed-duration transitions and instant cuts).</param>
    /// <param name="mode">Auto-animate, or drive manually via <see cref="SetManualTime"/>.</param>
    /// <returns>False if a transition is already in progress.</returns>
    public bool Start(Source destination, TimeSpan duration, ObsTransitionMode mode = ObsTransitionMode.Auto)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return ObsTransition.obs_transition_start(Handle, mode, (uint)duration.TotalMilliseconds, destination.Handle);
    }

    /// <summary>
    /// Sets the transition's current source immediately, without animating. Use this to
    /// seed the starting scene before the first <see cref="Start(Source, TimeSpan, ObsTransitionMode)"/> call.
    /// </summary>
    public void Set(Source? source)
        => ObsTransition.obs_transition_set(Handle, source?.Handle ?? ObsSourceHandle.Null);

    /// <summary>
    /// Begins animating to a scene. The transition takes its own reference to the scene, so
    /// you can pass it directly (no <see cref="Scenes.Scene.AsSource"/> needed).
    /// </summary>
    /// <param name="destination">The scene to transition to.</param>
    /// <param name="duration">Animation duration (ignored for fixed-duration transitions and instant cuts).</param>
    /// <param name="mode">Auto-animate, or drive manually via <see cref="SetManualTime"/>.</param>
    /// <returns>False if a transition is already in progress.</returns>
    public bool Start(Scenes.Scene destination, TimeSpan duration, ObsTransitionMode mode = ObsTransitionMode.Auto)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return ObsTransition.obs_transition_start(Handle, mode, (uint)duration.TotalMilliseconds, ObsScene.obs_scene_get_source(destination.Handle));
    }

    /// <summary>
    /// Sets the transition's current scene immediately, without animating. Use this to seed
    /// the starting scene before the first <see cref="Start(Scenes.Scene, TimeSpan, ObsTransitionMode)"/> call.
    /// </summary>
    public void Set(Scenes.Scene? scene)
        => ObsTransition.obs_transition_set(Handle, scene != null ? ObsScene.obs_scene_get_source(scene.Handle) : ObsSourceHandle.Null);

    /// <summary>Clears both sub-sources of the transition.</summary>
    public void Clear() => ObsTransition.obs_transition_clear(Handle);

    /// <summary>Gets the currently active sub-source, or null if none.</summary>
    public Source? GetActiveSource()
    {
        var handle = ObsTransition.obs_transition_get_active_source(Handle);
        return handle.IsNull ? null : new Source(handle, null, ownsHandle: true);
    }

    /// <summary>Gets one of the transition's sub-sources, or null if not set.</summary>
    public Source? GetSource(ObsTransitionTarget target)
    {
        var handle = ObsTransition.obs_transition_get_source(Handle, target);
        return handle.IsNull ? null : new Source(handle, null, ownsHandle: true);
    }

    /// <summary>True while a transition is animating (distinct from <see cref="Source.IsActive"/>,
    /// which reports whether the underlying source is active in the output).</summary>
    public bool IsTransitioning => ObsTransition.obs_transition_is_active(Handle);

    /// <summary>Immediately stops an in-progress transition, snapping to the destination.</summary>
    public void ForceStop() => ObsTransition.obs_transition_force_stop(Handle);

    /// <summary>
    /// Sets the manual transition position (0.0 to 1.0). Only meaningful after starting
    /// with <see cref="ObsTransitionMode.Manual"/>.
    /// </summary>
    public void SetManualTime(float t) => ObsTransition.obs_transition_set_manual_time(Handle, t);

    /// <summary>Sets the torque and clamp used to smooth manual transition movement.</summary>
    public void SetManualTorque(float torque, float clamp)
        => ObsTransition.obs_transition_set_manual_torque(Handle, torque, clamp);

    /// <summary>The current transition position (0.0 to 1.0).</summary>
    public float Time => ObsTransition.obs_transition_get_time(Handle);

    /// <summary>Controls how sub-sources of differing sizes are scaled within the transition.</summary>
    public ObsTransitionScaleType ScaleType
    {
        get => ObsTransition.obs_transition_get_scale_type(Handle);
        set => ObsTransition.obs_transition_set_scale_type(Handle, value);
    }

    /// <summary>The alignment flags used when a sub-source is smaller than the transition.</summary>
    public uint Alignment
    {
        get => ObsTransition.obs_transition_get_alignment(Handle);
        set => ObsTransition.obs_transition_set_alignment(Handle, value);
    }

    /// <summary>Gets the transition's render size.</summary>
    public (uint Width, uint Height) GetSize()
    {
        ObsTransition.obs_transition_get_size(Handle, out var cx, out var cy);
        return (cx, cy);
    }

    /// <summary>Sets the transition's render size.</summary>
    public void SetSize(uint width, uint height) => ObsTransition.obs_transition_set_size(Handle, width, height);

    /// <summary>
    /// Enables or disables fixed-duration transitions (e.g. stingers/videos that are of a
    /// fixed length and linearly interpolated).
    /// </summary>
    public void EnableFixed(bool enable, TimeSpan duration)
        => ObsTransition.obs_transition_enable_fixed(Handle, enable, (uint)duration.TotalMilliseconds);

    /// <summary>True if the transition has a fixed duration.</summary>
    public bool IsFixed => ObsTransition.obs_transition_fixed(Handle);
}
