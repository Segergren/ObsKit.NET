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
    /// Releases the unmanaged resources used by this object.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (_ownsHandle && _handle != 0)
        {
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
