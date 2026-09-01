using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Scenes;

/// <summary>
/// A weak reference to a canvas (obs_weak_canvas_t) that does not keep it alive.
/// Obtain via <see cref="Canvas.GetWeakReference"/> and upgrade with <see cref="TryGetCanvas"/>.
/// </summary>
public sealed class WeakCanvas : IDisposable
{
    private nint _weak;
    private bool _disposed;

    internal WeakCanvas(nint weak)
    {
        _weak = weak;
    }

    /// <summary>Gets whether the canvas has been destroyed.</summary>
    public bool IsExpired
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return ObsCore.obs_weak_object_expired(_weak);
        }
    }

    /// <summary>
    /// Attempts to get a strong reference to the canvas.
    /// </summary>
    /// <returns>The canvas (dispose it when done), or null if it has been destroyed.</returns>
    public Canvas? TryGetCanvas()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var handle = ObsCanvas.obs_weak_canvas_get_canvas(_weak);
        return handle.IsNull ? null : new Canvas(handle);
    }

    /// <summary>Releases the weak reference.</summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~WeakCanvas()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        if (_weak != nint.Zero)
            ObsCanvas.obs_weak_canvas_release(_weak);
        _weak = nint.Zero;
    }
}
