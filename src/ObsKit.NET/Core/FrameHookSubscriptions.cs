using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Core;

/// <summary>
/// Base for callbacks registered with the OBS core that must stay rooted until removed.
/// Disposing removes the native callback; the finalizer does the same if the object is dropped.
/// </summary>
public abstract class NativeHookSubscription : IDisposable
{
    private bool _disposed;

    /// <summary>Gets whether the hook has been removed.</summary>
    public bool IsDisposed => _disposed;

    /// <summary>Removes the hook. Safe to call multiple times.</summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~NativeHookSubscription()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        // The core frees its callback arrays in obs_shutdown; removing afterwards would be a
        // use-after-free on the finalizer thread.
        if (Obs.IsInitialized)
            Remove();
    }

    /// <summary>Removes the native callback (only called while the core is initialized).</summary>
    protected abstract void Remove();

    private protected static void Invoke(Action action)
    {
        try
        {
            action();
        }
        catch
        {
            // Never let exceptions cross the native boundary.
        }
    }
}

/// <summary>
/// A per-frame tick hook (see <see cref="Obs.SubscribeTick"/>). Fires on the graphics thread
/// once per video frame, before sources are rendered.
/// </summary>
public sealed class TickSubscription : NativeHookSubscription
{
    private readonly ObsCore.TickCallbackNative _native;

    internal TickSubscription(Action<float> callback)
    {
        _native = (_, seconds) =>
        {
            if (!IsDisposed)
                Invoke(() => callback(seconds));
        };
        ObsCore.obs_add_tick_callback(_native, nint.Zero);
    }

    /// <inheritdoc/>
    protected override void Remove()
    {
        ObsCore.obs_remove_tick_callback(_native, nint.Zero);
        GC.KeepAlive(_native);
    }
}

/// <summary>
/// A main-canvas overlay hook (see <see cref="Obs.SubscribeMainRender"/>). Fires on the
/// graphics thread after the main canvas is composited, with the graphics context active and
/// the main texture as the render target, so anything drawn here appears in every output.
/// </summary>
public sealed class MainRenderSubscription : NativeHookSubscription
{
    private readonly ObsCore.MainRenderCallbackNative _native;

    internal MainRenderSubscription(Action<uint, uint> draw)
    {
        _native = (_, cx, cy) =>
        {
            if (!IsDisposed)
                Invoke(() => draw(cx, cy));
        };
        ObsCore.obs_add_main_render_callback(_native, nint.Zero);
    }

    /// <inheritdoc/>
    protected override void Remove()
    {
        ObsCore.obs_remove_main_render_callback(_native, nint.Zero);
        GC.KeepAlive(_native);
    }
}

/// <summary>
/// A frame-completed hook (see <see cref="Obs.SubscribeMainRendered"/>). Fires on the graphics
/// thread once every canvas has finished rendering a frame.
/// </summary>
public sealed class MainRenderedSubscription : NativeHookSubscription
{
    private readonly ObsCore.MainRenderedCallbackNative _native;

    internal MainRenderedSubscription(Action rendered)
    {
        _native = _ =>
        {
            if (!IsDisposed)
                Invoke(rendered);
        };
        ObsCore.obs_add_main_rendered_callback(_native, nint.Zero);
    }

    /// <inheritdoc/>
    protected override void Remove()
    {
        ObsCore.obs_remove_main_rendered_callback(_native, nint.Zero);
        GC.KeepAlive(_native);
    }
}
