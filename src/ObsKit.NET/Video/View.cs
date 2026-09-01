using ObsKit.NET.Exceptions;
using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Video;

/// <summary>
/// An auxiliary set of output channels (obs_view_t), independent of the main canvas. A view
/// can be rendered directly inside a graphics callback, or turned into its own video mix
/// with <see cref="AddVideoMix"/> so encoders and outputs can consume it (this is how OBS
/// feeds the virtual camera from a single scene or source). On OBS 31+ a
/// <see cref="Scenes.Canvas"/> is usually the better fit; views remain useful for a
/// lightweight extra mix of a single source, or for preview rendering.
/// </summary>
public sealed class View : IDisposable
{
    private nint _view;
    private bool _disposed;
    private VideoHandle _video;

    /// <summary>
    /// Creates an empty view.
    /// </summary>
    public View()
    {
        if (!Obs.IsInitialized)
            throw new ObsNotInitializedException();

        _view = ObsView.obs_view_create();
        if (_view == nint.Zero)
            throw new InvalidOperationException("Failed to create view.");
    }

    /// <summary>Gets whether <see cref="AddVideoMix"/> has been called.</summary>
    public bool HasVideoMix => !_video.IsNull;

    /// <summary>The video output of the mix created by <see cref="AddVideoMix"/>.</summary>
    internal VideoHandle Video
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_video.IsNull)
                throw new InvalidOperationException("Call AddVideoMix() before attaching encoders to a view.");
            return _video;
        }
    }

    /// <summary>
    /// Assigns a source to one of the view's channels (0-63). Pass null to clear the channel.
    /// The view holds its own reference to the source.
    /// </summary>
    public void SetSource(uint channel, Source? source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(channel, 64u);
        ObsView.obs_view_set_source(_view, channel, source?.Handle ?? default);
    }

    /// <summary>
    /// Assigns a scene to one of the view's channels. Pass null to clear the channel.
    /// </summary>
    public void SetScene(uint channel, Scenes.Scene? scene)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(channel, 64u);
        ObsView.obs_view_set_source(_view, channel, scene != null ? (ObsSourceHandle)scene.AsSource.NativeHandle : default);
    }

    /// <summary>
    /// Gets the source assigned to a channel (dispose when done), or null.
    /// </summary>
    public Source? GetSource(uint channel)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(channel, 64u);
        var handle = ObsView.obs_view_get_source(_view, channel);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>
    /// Renders the view's channels into the current render target. Only valid on the graphics
    /// thread inside a render callback (e.g. <see cref="Obs.SubscribeMainRender"/> or a
    /// display draw callback).
    /// </summary>
    public void Render()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObsView.obs_view_render(_view);
    }

    /// <summary>
    /// Creates a dedicated video mix that renders this view every frame at the given
    /// resolution (other video settings are inherited from the main canvas). Afterwards the
    /// view can feed encoders via <c>RecordingOutput.WithVideoEncoder(encoder, view)</c>.
    /// Pass 0 for width/height to use the main canvas size. Can only be called once.
    /// </summary>
    public void AddVideoMix(uint width = 0, uint height = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_video.IsNull)
            throw new InvalidOperationException("The view already has a video mix.");

        var ovi = default(ObsVideoInfo);
        if (!ObsCore.obs_get_video_info(ref ovi))
            throw new InvalidOperationException("OBS video is not initialized.");

        if (width != 0 && height != 0)
        {
            ovi.BaseWidth = width;
            ovi.BaseHeight = height;
            ovi.OutputWidth = width;
            ovi.OutputHeight = height;
        }

        _video = ObsView.obs_view_add2(_view, ref ovi);
        if (_video.IsNull)
            throw new InvalidOperationException("Failed to create a video mix for the view.");
    }

    /// <summary>
    /// Gets the video settings of every mix rendering this view (usually zero or one).
    /// </summary>
    public IReadOnlyList<ObsVideoInfo> GetVideoMixes()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var result = new List<ObsVideoInfo>();
        ObsView.EnumVideoInfoCallback callback = (_, ovi) =>
        {
            if (ovi != nint.Zero)
                result.Add(System.Runtime.InteropServices.Marshal.PtrToStructure<ObsVideoInfo>(ovi));
            return 1;
        };
        ObsView.obs_view_enum_video_info(_view, callback, nint.Zero);
        GC.KeepAlive(callback);
        return result;
    }

    /// <summary>
    /// Removes the video mix (if any), clears all channels, and destroys the view. Stop any
    /// output that encodes from this view first.
    /// </summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~View()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        // Both the mix table and the channel sources are gone after obs_shutdown.
        if (_view != nint.Zero && Obs.IsInitialized)
        {
            ObsView.obs_view_remove(_view);
            for (uint i = 0; i < 64; i++)
                ObsView.obs_view_set_source(_view, i, default);
            ObsView.obs_view_destroy(_view);
        }
        _view = nint.Zero;
        _video = default;
    }
}
