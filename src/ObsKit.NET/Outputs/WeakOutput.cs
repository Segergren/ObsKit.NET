using ObsKit.NET.Native.Interop;

namespace ObsKit.NET.Outputs;

/// <summary>
/// A weak reference to an output (obs_weak_output_t) that does not keep it alive.
/// Obtain via <see cref="Output.GetWeakReference"/> and upgrade with <see cref="TryGetOutput"/>.
/// </summary>
public sealed class WeakOutput : IDisposable
{
    private nint _weak;
    private bool _disposed;

    internal WeakOutput(nint weak)
    {
        _weak = weak;
    }

    /// <summary>Gets whether the output has been destroyed.</summary>
    public bool IsExpired
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return ObsCore.obs_weak_object_expired(_weak);
        }
    }

    /// <summary>Gets whether this weak reference points at <paramref name="output"/>.</summary>
    public bool References(Output output)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(output);
        return ObsOutput.obs_weak_output_references_output(_weak, output.Handle);
    }

    /// <summary>
    /// Attempts to get a strong reference to the output.
    /// </summary>
    /// <returns>The output (dispose it when done), or null if it has been destroyed.</returns>
    public Output? TryGetOutput()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var handle = ObsOutput.obs_weak_output_get_output(_weak);
        return handle.IsNull ? null : new Output(handle, ownsHandle: true);
    }

    /// <summary>Releases the weak reference.</summary>
    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }

    ~WeakOutput()
    {
        Release();
    }

    private void Release()
    {
        if (_disposed)
            return;
        _disposed = true;

        // The weak control block is freed independently of the OBS core, so this is safe
        // even after shutdown.
        if (_weak != nint.Zero)
            ObsOutput.obs_weak_output_release(_weak);
        _weak = nint.Zero;
    }
}
