using ObsKit.NET.Exceptions;

namespace ObsKit.NET.Core;

/// <summary>
/// Base class for all OBS objects that follow the reference counting pattern.
/// Provides automatic cleanup through IDisposable.
/// </summary>
public abstract class ObsObject : IDisposable
{
    private nint _handle;
    private bool _disposed;
    private readonly bool _ownsHandle;

    /// <summary>
    /// Gets the native handle. Throws if disposed.
    /// </summary>
    protected internal nint Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    /// <summary>
    /// Gets the native OBS handle for advanced interop scenarios.
    /// Use with caution - incorrect usage can cause crashes or memory corruption.
    /// </summary>
    public nint NativeHandle => Handle;

    /// <summary>
    /// Gets whether this object has been disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Gets whether the native handle is valid (non-null).
    /// </summary>
    public bool IsValid => _handle != 0 && !_disposed;

    /// <summary>
    /// Creates a new OBS object wrapper.
    /// </summary>
    /// <param name="handle">The native handle.</param>
    /// <param name="ownsHandle">Whether this object owns the handle and should release it on dispose.</param>
    protected ObsObject(nint handle, bool ownsHandle = true)
    {
        if (handle == 0)
            throw new ArgumentException("Handle cannot be null", nameof(handle));

        _handle = handle;
        _ownsHandle = ownsHandle;
    }

    /// <summary>
    /// When overridden in a derived class, releases the native handle.
    /// </summary>
    /// <param name="handle">The handle to release.</param>
    protected abstract void ReleaseHandle(nint handle);

    /// <summary>
    /// Whether <see cref="ReleaseHandle"/> must only run while the OBS core is initialized.
    /// </summary>
    /// <remarks>
    /// True for objects owned by the OBS core (sources, scenes, scene items, canvases, outputs,
    /// encoders, services): <c>obs_shutdown</c> already destroys these native objects and nulls the
    /// global <c>obs</c> pointer, and several of their release functions (e.g. obs_output_release,
    /// obs_encoder_release, obs_service_release, obs_sceneitem_release) do not guard against that, so
    /// releasing after shutdown reads/writes freed memory — a crash on the finalizer thread at exit.
    /// Independent objects whose lifetime is not tied to the core (e.g. obs_data settings) override
    /// this to false so they are always released.
    /// </remarks>
    protected virtual bool ReleaseRequiresObs => true;

    /// <summary>
    /// Replaces the native handle without releasing the previous one.
    /// The caller is responsible for releasing the returned previous handle.
    /// </summary>
    /// <param name="newHandle">The new native handle.</param>
    /// <returns>The previous native handle.</returns>
    private protected nint ReplaceHandle(nint newHandle)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (newHandle == 0)
            throw new ArgumentException("Handle cannot be null", nameof(newHandle));

        var previous = _handle;
        _handle = newHandle;
        return previous;
    }

    /// <summary>
    /// Releases the unmanaged resources used by this object.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (_ownsHandle && _handle != 0)
        {
            // After obs_shutdown, every OBS-core-owned native object has already been freed and the
            // global obs pointer is NULL. Releasing again would be a use-after-free (most release
            // functions do not self-guard). Skip it once the core is down — the native object is
            // already gone, so nothing leaks. Independent objects (obs_data) keep releasing.
            if (!ReleaseRequiresObs || Obs.IsInitialized)
                ReleaseHandle(_handle);
        }

        _handle = 0;
        _disposed = true;
    }

    /// <summary>
    /// Releases all resources used by this object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources if Dispose was not called.
    /// </summary>
    ~ObsObject()
    {
        Dispose(false);
    }

    /// <summary>
    /// Throws if OBS is not initialized.
    /// </summary>
    protected static void ThrowIfNotInitialized()
    {
        if (!Obs.IsInitialized)
            throw new ObsNotInitializedException();
    }
}
