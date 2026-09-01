using ObsKit.NET.Signals;
using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Scenes;

/// <summary>
/// An independent video mix with its own resolution and scenes (OBS 31+).
/// Use a second canvas to compose and record a different view than the main canvas —
/// e.g. a vertical 1080x1920 mix recorded simultaneously with the horizontal one.
/// </summary>
/// <example>
/// <code>
/// using var vertical = Canvas.Create("Vertical", 1080, 1920);
/// using var verticalScene = vertical.CreateScene("Vertical Scene");
/// verticalScene.AddSource(gameCapture);
/// vertical.SetScene(verticalScene);
///
/// using var verticalRecording = new RecordingOutput("Vertical Recording")
///     .SetPath("vertical.mp4")
///     .WithVideoEncoder(VideoEncoder.CreateBest("Vertical Video"), vertical, takeOwnership: true)
///     .WithAudioEncoder(AudioEncoder.CreateAac("Vertical Audio"), takeOwnership: true);
/// </code>
/// </example>
public sealed class Canvas : ObsObject
{
    internal Canvas(ObsCanvasHandle handle)
        : base(handle, ownsHandle: true)
    {
    }

    internal new ObsCanvasHandle Handle => (ObsCanvasHandle)base.Handle;

    /// <summary>
    /// Creates a new canvas with its own resolution. Other video settings
    /// (FPS, color space, etc.) are inherited from the main canvas.
    /// </summary>
    /// <param name="name">The canvas name.</param>
    /// <param name="width">The canvas width in pixels.</param>
    /// <param name="height">The canvas height in pixels.</param>
    /// <param name="flags">Canvas behavior flags (default <see cref="ObsCanvasFlags.Program"/>, suitable for recording).</param>
    /// <exception cref="NotSupportedException">The OBS runtime does not support the canvas API (requires OBS 31+).</exception>
    public static Canvas Create(string name, uint width, uint height, ObsCanvasFlags flags = ObsCanvasFlags.Program)
    {
        var ovi = default(ObsVideoInfo);
        if (!ObsCore.obs_get_video_info(ref ovi))
            throw new InvalidOperationException("OBS video is not initialized.");

        ovi.BaseWidth = width;
        ovi.BaseHeight = height;
        ovi.OutputWidth = width;
        ovi.OutputHeight = height;

        ObsCanvasHandle handle;
        try
        {
            handle = ObsCanvas.obs_canvas_create(name, ref ovi, (uint)flags);
        }
        catch (EntryPointNotFoundException e)
        {
            throw new NotSupportedException("The canvas API requires OBS Studio 31 or later.", e);
        }

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create canvas '{name}'.");

        return new Canvas(handle);
    }

    /// <summary>
    /// Gets a reference to the main canvas. Dispose it when done (the canvas itself is not destroyed).
    /// </summary>
    public static Canvas GetMain()
    {
        ObsCanvasHandle handle;
        try
        {
            handle = ObsCanvas.obs_get_main_canvas();
        }
        catch (EntryPointNotFoundException e)
        {
            throw new NotSupportedException("The canvas API requires OBS Studio 31 or later.", e);
        }

        if (handle.IsNull)
            throw new InvalidOperationException("OBS is not initialized.");

        return new Canvas(handle);
    }

    /// <summary>
    /// Finds a canvas by its name (private canvases are not searched). The main
    /// canvas is included and is named "Main" — prefer <see cref="GetMain"/> to
    /// fetch it directly rather than relying on that name.
    /// </summary>
    /// <param name="name">The canvas name.</param>
    /// <returns>The canvas, or null if no canvas with that name exists. Dispose it when done.</returns>
    public static Canvas? GetByName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var handle = ObsCanvas.obs_get_canvas_by_name(name);
        return handle.IsNull ? null : new Canvas(handle);
    }

    /// <summary>
    /// Finds a canvas by its UUID.
    /// </summary>
    /// <param name="uuid">The canvas UUID (see <see cref="Uuid"/>).</param>
    /// <returns>The canvas, or null if no canvas with that UUID exists. Dispose it when done.</returns>
    public static Canvas? GetByUuid(string uuid)
    {
        ArgumentNullException.ThrowIfNull(uuid);
        var handle = ObsCanvas.obs_get_canvas_by_uuid(uuid);
        return handle.IsNull ? null : new Canvas(handle);
    }

    /// <summary>
    /// Gets all canvases that currently exist (including the main canvas).
    /// Note: Each canvas in the returned list should be disposed when no longer needed.
    /// </summary>
    public static List<Canvas> GetAll()
    {
        var canvases = new List<Canvas>();
        ObsCanvas.EnumCanvasCallback callback = (data, handle) =>
        {
            if (!handle.IsNull)
            {
                // The enum hands us a borrowed pointer; take our own owning ref.
                var refHandle = ObsCanvas.obs_canvas_get_ref(handle);
                if (!refHandle.IsNull)
                    canvases.Add(new Canvas(refHandle));
            }
            return 1;
        };
        ObsCanvas.obs_enum_canvases(callback, 0);
        GC.KeepAlive(callback);
        return canvases;
    }

    /// <summary>Gets or sets the canvas name (the main canvas cannot be renamed).</summary>
    public string? Name
    {
        get => ObsCanvas.obs_canvas_get_name(Handle);
        set
        {
            if (value != null)
                ObsCanvas.obs_canvas_set_name(Handle, value);
        }
    }

    /// <summary>Gets the canvas UUID.</summary>
    public string? Uuid => ObsCanvas.obs_canvas_get_uuid(Handle);

    /// <summary>Gets the canvas behavior flags.</summary>
    public ObsCanvasFlags Flags => (ObsCanvasFlags)ObsCanvas.obs_canvas_get_flags(Handle);

    /// <summary>Gets whether this is the main canvas.</summary>
    public bool IsMain => (Flags & ObsCanvasFlags.Main) != 0;

    /// <summary>Gets whether the canvas has been removed.</summary>
    public bool IsRemoved => ObsCanvas.obs_canvas_removed(Handle);

    /// <summary>Gets whether the canvas has video configured.</summary>
    public bool HasVideo => ObsCanvas.obs_canvas_has_video(Handle);

    /// <summary>Gets the canvas resolution, or null if video is not configured.</summary>
    public (uint Width, uint Height)? Size
    {
        get
        {
            var ovi = default(ObsVideoInfo);
            if (!ObsCanvas.obs_canvas_get_video_info(Handle, ref ovi))
                return null;
            return (ovi.BaseWidth, ovi.BaseHeight);
        }
    }

    /// <summary>Gets the canvas's video output handle (for attaching encoders).</summary>
    internal VideoHandle Video => ObsCanvas.obs_canvas_get_video(Handle);

    /// <summary>
    /// Creates a scene attached to this canvas. Sources added to it render at
    /// this canvas's resolution, independent of the main canvas.
    /// </summary>
    /// <param name="name">The scene name.</param>
    public Scene CreateScene(string name)
    {
        var sceneHandle = ObsCanvas.obs_canvas_scene_create(Handle, name);
        if (sceneHandle.IsNull)
            throw new InvalidOperationException($"Failed to create scene '{name}' on canvas '{Name}'.");

        return new Scene(sceneHandle, ownsHandle: true);
    }

    /// <summary>
    /// Finds a scene attached to this canvas by name.
    /// </summary>
    /// <param name="name">The scene name.</param>
    /// <returns>The scene, or null if this canvas has no scene with that name. Dispose it when done.</returns>
    public Scene? FindScene(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var handle = ObsCanvas.obs_canvas_get_scene_by_name(Handle, name);
        return handle.IsNull ? null : new Scene(handle, ownsHandle: true);
    }

    /// <summary>
    /// Finds a source attached to this canvas by name.
    /// </summary>
    /// <param name="name">The source name.</param>
    /// <returns>The source, or null if this canvas has no source with that name. Dispose it when done.</returns>
    public Source? FindSource(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var handle = ObsCanvas.obs_canvas_get_source_by_name(Handle, name);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>
    /// Gets all scenes attached to this canvas.
    /// Note: Each scene in the returned list should be disposed when no longer needed.
    /// </summary>
    public List<Scene> GetScenes()
    {
        var scenes = new List<Scene>();
        ObsSource.EnumSourceCallback callback = (data, handle) =>
        {
            if (!handle.IsNull)
            {
                var sceneHandle = ObsScene.obs_scene_from_source(handle);
                if (!sceneHandle.IsNull)
                {
                    // obs_scene_from_source returns a borrowed pointer; take an owning ref
                    // via the exported obs_scene_get_ref (null if being destroyed).
                    var refd = ObsScene.obs_scene_get_ref(sceneHandle);
                    if (!refd.IsNull)
                        scenes.Add(new Scene(refd, ownsHandle: true));
                }
            }
            return 1;
        };
        ObsCanvas.obs_canvas_enum_scenes(Handle, callback, 0);
        GC.KeepAlive(callback);
        return scenes;
    }

    /// <summary>
    /// Moves a scene to this canvas, detaching it from its previous canvas.
    /// Useful with <see cref="Scene.Duplicate"/> to reframe an existing scene
    /// at this canvas's resolution.
    /// </summary>
    /// <param name="scene">The scene to move.</param>
    public void MoveScene(Scene scene)
    {
        ObsCanvas.obs_canvas_move_scene(scene.Handle, Handle);
    }

    /// <summary>
    /// Sets the scene rendered on this canvas (channel 0), like
    /// <c>Obs.SetOutputSource</c> does for the main canvas.
    /// </summary>
    /// <param name="scene">The scene to render.</param>
    /// <param name="channel">The canvas channel (0-63).</param>
    public void SetScene(Scene scene, uint channel = 0)
    {
        var sourceHandle = ObsScene.obs_scene_get_source(scene.Handle);
        ObsCanvas.obs_canvas_set_channel(Handle, channel, sourceHandle);
    }

    /// <summary>
    /// Clears a channel of this canvas.
    /// </summary>
    /// <param name="channel">The canvas channel (0-63).</param>
    public void ClearChannel(uint channel = 0)
    {
        ObsCanvas.obs_canvas_set_channel(Handle, channel, ObsSourceHandle.Null);
    }

    /// <summary>
    /// Changes the canvas resolution. Fails while an output is actively using the canvas
    /// and on the main canvas (use <c>ObsContext.SetVideo</c> for that).
    /// </summary>
    /// <param name="width">The new width in pixels.</param>
    /// <param name="height">The new height in pixels.</param>
    /// <returns>True if the video mix was reset.</returns>
    public bool ResetVideo(uint width, uint height)
    {
        var ovi = default(ObsVideoInfo);
        if (!ObsCanvas.obs_canvas_get_video_info(Handle, ref ovi) && !ObsCore.obs_get_video_info(ref ovi))
            return false;

        ovi.BaseWidth = width;
        ovi.BaseHeight = height;
        ovi.OutputWidth = width;
        ovi.OutputHeight = height;
        return ObsCanvas.obs_canvas_reset_video(Handle, ref ovi);
    }

    /// <summary>
    /// Marks the canvas as removed, signaling holders of references to release them.
    /// </summary>
    public void Remove()
    {
        ObsCanvas.obs_canvas_remove(Handle);
    }

    /// <inheritdoc/>
    protected override void ReleaseHandle(nint handle)
    {
        ObsCanvas.obs_canvas_release((ObsCanvasHandle)handle);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Canvas: {Name}";

    #region Private Canvases, Persistence, Signals and Weak References

    /// <summary>
    /// Creates a private canvas: not enumerated by <see cref="GetAll"/>, not found by name,
    /// and not saved. Otherwise identical to <see cref="Create"/>.
    /// </summary>
    public static Canvas CreatePrivate(string name, uint width, uint height, ObsCanvasFlags flags = ObsCanvasFlags.Program)
    {
        var ovi = default(ObsVideoInfo);
        if (!ObsCore.obs_get_video_info(ref ovi))
            throw new InvalidOperationException("OBS video is not initialized.");

        ovi.BaseWidth = width;
        ovi.BaseHeight = height;
        ovi.OutputWidth = width;
        ovi.OutputHeight = height;

        ObsCanvasHandle handle;
        try
        {
            handle = ObsCanvas.obs_canvas_create_private(name, ref ovi, (uint)flags);
        }
        catch (EntryPointNotFoundException e)
        {
            throw new NotSupportedException("The canvas API requires OBS Studio 31 or later.", e);
        }

        if (handle.IsNull)
            throw new InvalidOperationException($"Failed to create private canvas '{name}'.");

        return new Canvas(handle);
    }

    /// <summary>
    /// Serializes the canvas identity (name, UUID, flags) so <see cref="Load"/> can recreate it
    /// with the same UUID. Returns null for ephemeral or removed canvases. Scenes are saved
    /// separately (see <see cref="Obs.SaveSources"/>). Dispose when done.
    /// </summary>
    public Settings? Save()
    {
        var handle = ObsCanvas.obs_save_canvas(Handle);
        return handle.IsNull ? null : new Settings(handle);
    }

    /// <summary>
    /// Recreates a canvas from <see cref="Save"/> data, preserving its UUID. Video settings are
    /// inherited from the main canvas; call <see cref="ResetVideo"/> to set its resolution.
    /// </summary>
    public static Canvas Load(Settings data)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(data);
        var handle = ObsCanvas.obs_load_canvas(data.Handle);
        if (handle.IsNull)
            throw new InvalidOperationException("Failed to load canvas.");
        return new Canvas(handle);
    }

    /// <summary>
    /// Detaches a scene from this canvas (the scene keeps existing).
    /// </summary>
    public void RemoveScene(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ObsCanvas.obs_canvas_scene_remove(scene.Handle);
    }

    /// <summary>
    /// Connects to one of the canvas's signals. Keep the returned connection alive for as
    /// long as the callback should fire; dispose it to disconnect.
    /// </summary>
    public Signals.SignalConnection ConnectSignal(Signals.CanvasSignal signal, Signals.SignalCallback callback)
        => ConnectSignal(signal.ToSignalName(), callback);

    /// <summary>
    /// Connects to a canvas signal by name (e.g. "source_add", "channel_change").
    /// </summary>
    public Signals.SignalConnection ConnectSignal(string signal, Signals.SignalCallback callback)
    {
        ArgumentException.ThrowIfNullOrEmpty(signal);
        ArgumentNullException.ThrowIfNull(callback);
        var handler = ObsCanvas.obs_canvas_get_signal_handler(Handle);
        return new Signals.SignalConnection(handler, signal, callback);
    }

    /// <summary>
    /// Creates a weak reference that does not keep the canvas alive. Upgrade with
    /// <see cref="WeakCanvas.TryGetCanvas"/>.
    /// </summary>
    public WeakCanvas GetWeakReference()
    {
        var weak = ObsCanvas.obs_canvas_get_weak_canvas(Handle);
        if (weak == nint.Zero)
            throw new InvalidOperationException("Failed to create a weak reference.");
        return new WeakCanvas(weak);
    }

    #endregion
}
