using ObsKit.NET.Core;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;

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
    private Canvas(ObsCanvasHandle handle)
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
    /// <c>Scene.SetAsProgram</c> does for the main canvas.
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
}
