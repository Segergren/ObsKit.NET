using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Services;

/// <summary>
/// A weak reference to a service (obs_weak_service_t) that does not keep it alive.
/// Obtain via <see cref="Service.GetWeakReference"/> and upgrade with <see cref="TryGetService"/>.
/// </summary>
public sealed class WeakService : IDisposable
{
    private nint _weak;
    private bool _disposed;

    internal WeakService(nint weak)
    {
        _weak = weak;
    }

    /// <summary>Gets whether the service has been destroyed.</summary>
    public bool IsExpired
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return ObsCore.obs_weak_object_expired(_weak);
        }
    }

    /// <summary>Gets whether this weak reference points at <paramref name="service"/>.</summary>
    public bool References(Service service)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(service);
        return ObsService.obs_weak_service_references_service(_weak, service.Handle);
    }

    /// <summary>
    /// Attempts to get a strong reference to the service.
    /// </summary>
    /// <returns>The service (dispose it when done), or null if it has been destroyed.</returns>
    public Service? TryGetService()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var handle = ObsService.obs_weak_service_get_service(_weak);
        return handle.IsNull ? null : new Service(handle, ownsHandle: true);
    }

    /// <summary>Releases the weak reference.</summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~WeakService()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        if (_weak != nint.Zero)
            ObsService.obs_weak_service_release(_weak);
        _weak = nint.Zero;
    }
}
