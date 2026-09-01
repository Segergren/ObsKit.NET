using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Sources;

/// <summary>
/// A weak reference to a source (obs_weak_source_t) — does not keep the source
/// alive. Use it to remember a source across threads or callbacks without
/// affecting its lifetime, then upgrade with <see cref="TryGetSource"/> when
/// access is needed. Obtain via <see cref="Source.GetWeakReference"/>.
/// </summary>
public sealed class WeakSource : IDisposable
{
    private nint _weak;
    private bool _disposed;

    internal WeakSource(nint weak)
    {
        _weak = weak;
    }

    /// <summary>
    /// Gets whether the source has been destroyed (a subsequent
    /// <see cref="TryGetSource"/> would return null).
    /// </summary>
    public bool IsExpired
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return ObsSource.obs_weak_source_expired(_weak);
        }
    }

    /// <summary>
    /// Attempts to get a strong reference to the source.
    /// </summary>
    /// <returns>The source (dispose it when done), or null if it has been destroyed.</returns>
    public Source? TryGetSource()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var handle = ObsSource.obs_weak_source_get_source(_weak);
        return handle.IsNull ? null : new Source(handle, ownsHandle: true);
    }

    /// <summary>
    /// Releases the weak reference.
    /// </summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~WeakSource()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        // The weak-ref control block stays valid while we hold a weak ref and is
        // freed independently of the OBS core, so this is safe even after shutdown.
        if (_weak != nint.Zero)
            ObsSource.obs_weak_source_release(_weak);
        _weak = nint.Zero;
    }

    /// <summary>Gets whether this weak reference points at <paramref name="source"/>.</summary>
    public bool References(Source source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(source);
        return ObsSource.obs_weak_source_references_source(_weak, source.Handle);
    }
}
